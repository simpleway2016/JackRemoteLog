using Jack.RemoteLog.WebApi.Dtos;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Documents;
using Jack.RemoteLog.WebApi.Domains;
using Lucene.Net.Search;
using System.Text;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using Quartz.Logging;
using System;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Util;

namespace Jack.RemoteLog.WebApi.Infrastructures
{
    public class LuceneContentReader : ILogContentReader
    {
        string _folderPath;
        ISourceContextCollection _sourceContextReader;
        Analyzer _analyzer;

        DateTime _lastCommitTime = DateTime.Now;
        ILogger<LuceneContentReader> _logger;
        bool _disposed;
        DirectoryInfo INDEX_DIR;
        public LuceneContentReader(string folderPath, ISourceContextCollection sourceContextReader)
        {
            this._folderPath = folderPath;
            this._sourceContextReader = sourceContextReader;
            _logger = Global.ServiceProvider.GetService<ILogger<LuceneContentReader>>();
            INDEX_DIR = new DirectoryInfo(folderPath);
            _analyzer = new PanGuAnalyzer(true);

        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _analyzer.Dispose();
            }
        }

        private BooleanQuery AnalyzerKeyword(string keyword, string field)
        {
            BooleanQuery ret = new BooleanQuery();
            try
            {
                var arr = keyword.Split(' ');
                foreach (var str in arr)
                {
                    if (string.IsNullOrEmpty(str))
                        continue;

                    StringBuilder queryStringBuilder = new StringBuilder();
                    string[] words = LuceneAnalyze.AnalyzerKey(str);

                    BooleanQuery queryMust = new BooleanQuery();

                    foreach (var word in words)
                    {
                        WildcardQuery query = new WildcardQuery(new Term(field, $"*{word}*"));
                        queryMust.Add(query, Occur.MUST);
                    }
                    ret.Add(queryMust, Occur.SHOULD);
                }
            }
            catch
            {
            }

            return ret;
        }

        public LogItem[] Read(ISourceContextCollection sourceContextes, string sourceContext, Microsoft.Extensions.Logging.LogLevel? level, long startTimeStamp, long? endTimeStamp, string keyWord)
        {
            try
            {

                using (var indexer = DirectoryReader.Open(FSDirectory.Open(INDEX_DIR)))
                {
                    IndexSearcher _searcher = new IndexSearcher(indexer);
                    var findSourceId = sourceContextes.GetId(sourceContext);
                    QueryParser qp = new QueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, "Content", _analyzer);

                    var timequery = NumericRangeQuery.NewInt64Range("Timestamp", startTimeStamp, endTimeStamp, true, true);
                    BooleanQuery booleanClauses = new BooleanQuery();
                   
                    booleanClauses.Add(timequery, Occur.MUST);
                    if (findSourceId != 0)
                    {
                        //var sourceidQuery = NumericRangeQuery.NewInt32Range("SourceContextId", findSourceId, findSourceId, true, true);
                        //booleanClauses.Add(sourceidQuery, Occur.MUST);

                        BytesRef bytes = new BytesRef(NumericUtils.BUF_SIZE_INT32);
                        NumericUtils.Int32ToPrefixCoded(findSourceId, 0, bytes);
                        booleanClauses.Add(new TermQuery(new Term("SourceContextId", bytes)), Occur.MUST);
                    }

                    if (level != null)
                    {
                        //var levelQuery = NumericRangeQuery.NewInt32Range("Level", (int)level, (int)level, true, true);
                        //booleanClauses.Add(levelQuery, Occur.MUST);
                        BytesRef bytes = new BytesRef(NumericUtils.BUF_SIZE_INT32);
                        NumericUtils.Int32ToPrefixCoded((int)level, 0, bytes);
                        booleanClauses.Add(new TermQuery(new Term("Level", bytes)), Occur.MUST);
                    }

                    if (string.IsNullOrEmpty(keyWord) == false)
                    {
                        Query query = AnalyzerKeyword(keyWord, "Content");
                        booleanClauses.Add(query, Occur.MUST);
                    }

                    Sort sort = new Sort(new SortField("Timestamp", SortFieldType.INT64, false));
                    TopDocs tds = _searcher.Search(booleanClauses, Global.PageSize, sort);
                    LogItem[] ret = new LogItem[tds.ScoreDocs.Length];
                    for (int i = 0; i < tds.ScoreDocs.Length; i++)
                    {
                        var sd = tds.ScoreDocs[i];
                        Document doc = _searcher.Doc(sd.Doc);
                        int.TryParse(doc.Get("Level"), out int itemlevel);
                        int.TryParse(doc.Get("SourceContextId"), out int sourceContextId);
                        long.TryParse(doc.Get("Timestamp"), out long timestamp);
                        ret[i] = new LogItem
                        {
                            Content = doc.Get("Content"),
                            Level = (Microsoft.Extensions.Logging.LogLevel)itemlevel,
                            SourceContext = sourceContext != null ? sourceContext : _sourceContextReader.GetSourceContext(sourceContextId),
                            Timestamp = timestamp
                        };
                    }
                    return ret;
                }
            }
            catch
            {
                return new LogItem[0];
            }
        }
    }

    class LuceneAnalyze
    {
        #region AnalyzerKey
        /// <summary>
        /// 将搜索的keyword分词
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static string[] AnalyzerKey(string keyword)
        {
            Analyzer analyzer = new PanGuAnalyzer();
            QueryParser parser = new QueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, "title", analyzer);
            Query query = parser.Parse(CleanKeyword(keyword));
            if (query is TermQuery)
            {
                Term term = ((TermQuery)query).Term;
                return new string[] { term.Text() };
            }
            else if (query is PhraseQuery)
            {
                Term[] term = ((PhraseQuery)query).GetTerms();
                return term.Select(t => t.Text()).ToArray();
            }
            else if (query is BooleanQuery)
            {
                BooleanClause[] clauses = ((BooleanQuery)query).GetClauses();
                List<string> analyzerWords = new List<string>();
                foreach (BooleanClause clause in clauses)
                {
                    Query childQuery = clause.Query;
                    if (childQuery is TermQuery)
                    {
                        Term term = ((TermQuery)childQuery).Term;
                        analyzerWords.Add(term.Text());
                    }
                    else if (childQuery is PhraseQuery)
                    {
                        Term[] term = ((PhraseQuery)childQuery).GetTerms();
                        analyzerWords.AddRange(term.Select(t => t.Text()));
                    }
                }
                return analyzerWords.ToArray();
            }
            else
            {
                return new string[] { keyword };
            }
        }

        /// <summary>
        /// 清理头尾and or 关键字
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        private static string CleanKeyword(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            { }
            else
            {
                bool isClean = false;
                while (!isClean)
                {
                    keyword = keyword.Trim();
                    if (keyword.EndsWith(" AND"))
                    {
                        keyword = string.Format("{0}and", keyword.Remove(keyword.Length - 3, 3));
                    }
                    else if (keyword.EndsWith(" OR"))
                    {
                        keyword = string.Format("{0}or", keyword.Remove(keyword.Length - 2, 2));
                    }
                    else if (keyword.StartsWith("AND "))
                    {
                        keyword = string.Format("and{0}", keyword.Substring(3));
                    }
                    else if (keyword.StartsWith("OR "))
                    {
                        keyword = string.Format("or{0}", keyword.Substring(2));
                    }
                    else if (keyword.Contains(" OR "))
                    {
                        keyword = keyword.Replace(" OR ", " or ");
                    }
                    else if (keyword.Contains(" AND "))
                    {
                        keyword = keyword.Replace(" AND ", " and ");
                    }
                    else
                        isClean = true;
                }
            }
            //return keyword;
            return QueryParser.Escape(keyword);
        }
        #endregion AnalyzerKey
    }
}
