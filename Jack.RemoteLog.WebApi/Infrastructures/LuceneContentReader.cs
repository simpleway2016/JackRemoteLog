using Jack.RemoteLog.WebApi.Dtos;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using System.Text;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using Quartz.Logging;
using System;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Util;
using static Lucene.Net.Search.FieldCache;
using Lucene.Net.Support;


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

        private BooleanQuery AnalyzerKeyword(string[] keywords, string field, out List<string> outputWords)
        {
            outputWords = new List<string>();
            BooleanQuery ret = new BooleanQuery();
            try
            {
                foreach (var str in keywords)
                {
                    if (string.IsNullOrWhiteSpace(str))
                        continue;

                    StringBuilder queryStringBuilder = new StringBuilder();
                    string[] words = LuceneAnalyze.AnalyzerKey(str);

                    BooleanQuery queryMust = new BooleanQuery();

                    foreach (var word in words)
                    {
                        if (outputWords.Contains(word) == false)
                            outputWords.Add(word);

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

        public LogItem[] Read(ISourceContextCollection allSourceContexts, SearchRequestBody body)
        {
            try
            {

                using (var indexer = DirectoryReader.Open(FSDirectory.Open(INDEX_DIR)))
                {
                    IndexSearcher searcher = new IndexSearcher(indexer);

                    QueryParser qp = new QueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, "Content", _analyzer);

                    var timequery = NumericRangeQuery.NewInt64Range("Timestamp", body.Start, body.End, true, true);
                    BooleanQuery booleanClauses = new BooleanQuery();

                    booleanClauses.Add(timequery, Occur.MUST);

                    if (body.Sources != null && body.Sources.Length > 0)
                    {
                        BooleanQuery sourceBooleanClauses = new BooleanQuery();


                        foreach (var source in body.Sources)
                        {
                            var findSourceId = allSourceContexts.GetId(source);
                            if (findSourceId != 0)
                            {
                                //var sourceidQuery = NumericRangeQuery.NewInt32Range("SourceContextId", findSourceId, findSourceId, true, true);
                                //booleanClauses.Add(sourceidQuery, Occur.MUST);

                                BytesRef bytes = new BytesRef(NumericUtils.BUF_SIZE_INT32);
                                NumericUtils.Int32ToPrefixCoded(findSourceId, 0, bytes);
                                sourceBooleanClauses.Add(new TermQuery(new Term("SourceContextId", bytes)), Occur.SHOULD);
                            }
                        }

                        if (sourceBooleanClauses.Clauses.Count > 0)
                        {
                            booleanClauses.Add(sourceBooleanClauses, Occur.MUST);
                        }
                    }


                    if (body.Levels != null && body.Levels.Length > 0)
                    {
                        BooleanQuery levelBooleanClauses = new BooleanQuery();


                        //var levelQuery = NumericRangeQuery.NewInt32Range("Level", (int)level, (int)level, true, true);
                        //booleanClauses.Add(levelQuery, Occur.MUST);
                        foreach (var level in body.Levels)
                        {
                            BytesRef bytes = new BytesRef(NumericUtils.BUF_SIZE_INT32);
                            NumericUtils.Int32ToPrefixCoded((int)level, 0, bytes);
                            levelBooleanClauses.Add(new TermQuery(new Term("Level", bytes)), Occur.SHOULD);
                        }

                        if (levelBooleanClauses.Clauses.Count > 0)
                        {
                            booleanClauses.Add(levelBooleanClauses, Occur.MUST);
                        }
                    }

                    if (body.TraceIds != null)
                    {
                        BooleanQuery traceBooleanClauses = new BooleanQuery();


                        foreach (var traceId in body.TraceIds)
                        {
                            if (string.IsNullOrWhiteSpace(traceId))
                                continue;

                            traceBooleanClauses.Add(new TermQuery(new Term("TraceId", traceId)), Occur.SHOULD);
                        }
                        if (traceBooleanClauses.Clauses.Count > 0)
                        {
                            booleanClauses.Add(traceBooleanClauses, Occur.MUST);
                        }
                    }

                    List<string> words = null;
                    if (body.KeyWords != null)
                    {
                        BooleanQuery query = AnalyzerKeyword(body.KeyWords, "Content", out words);

                        if (query.Clauses.Count > 0)
                        {
                            booleanClauses.Add(query, Occur.MUST);
                        }
                    }

                    Sort sort = new Sort(new SortField("Timestamp", SortFieldType.INT64, false));
                    TopDocs tds = searcher.Search(booleanClauses, Global.PageSize, sort);
                    LogItem[] ret = new LogItem[tds.ScoreDocs.Length];

                    if (words != null)
                    {
                        foreach (var key in body.KeyWords)
                        {
                            if (words.Contains(key, StringComparer.OrdinalIgnoreCase) == false)
                            {
                                words.Insert(0, key);
                            }
                        }
                    }

                    for (int i = 0; i < tds.ScoreDocs.Length; i++)
                    {
                        var sd = tds.ScoreDocs[i];
                        Document doc = searcher.Doc(sd.Doc);
                        int.TryParse(doc.Get("Level"), out int itemlevel);
                        int.TryParse(doc.Get("SourceContextId"), out int sourceContextId);
                        long.TryParse(doc.Get("Timestamp"), out long timestamp);

                        ret[i] = new LogItem
                        {
                            Content = doc.Get("Content"),
                            Level = (Microsoft.Extensions.Logging.LogLevel)itemlevel,
                            SourceContext = _sourceContextReader.GetSourceContext(sourceContextId),
                            Timestamp = timestamp,
                            TraceId = doc.Get("TraceId")
                        };

                        if (i == 0)
                        {
                            ret[i].TotalHits = tds.TotalHits;
                            ret[i].SearchWords = words.ToArray();
                        }
                    }
                    words?.Clear();
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
            else if (query is BooleanQuery bq)
            {
                return AnalyzerBooleanQuery(bq);
            }
            else
            {
                return new string[] { keyword };
            }
        }
        static string[] AnalyzerBooleanQuery(BooleanQuery booleanClauses)
        {
            BooleanClause[] clauses = booleanClauses.GetClauses();
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
                else if (childQuery is BooleanQuery bq)
                {
                    analyzerWords.AddRange(AnalyzerBooleanQuery(bq));
                }
            }
            return analyzerWords.ToArray();
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
