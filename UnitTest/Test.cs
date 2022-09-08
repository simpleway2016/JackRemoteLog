using Jack.RemoteLog;
using Jack.RemoteLog.WebApi;
using Jack.RemoteLog.WebApi.Applications;
using Jack.RemoteLog.WebApi.AutoMissions;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Analysis;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Diagnostics;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace UnitTest
{
    [TestClass]
    public class Test
    {
        ServiceCollection _services;
        ServiceProvider _serviceProvider;
        public Test()
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            _services = new ServiceCollection();
            _services.AddSingleton<LogChannelRoute>();
            _services.AddSingleton<LogService>();
            _services.AddSingleton<IConfiguration>(Global.Configuration = builder.Build());

            _services.AddLogging(builder =>
            {
                builder.UseJackRemoteLogger(Global.Configuration, "MyApplicationContext");
            });
            _services.AddScoped<TestObject>();

            Global.ServiceProvider = _serviceProvider = _services.BuildServiceProvider();
           
        }

        [TestMethod]
        public void TestPanGu()
        {
            //将解析内容存储的地方，这里是：bin\Debug下的index文件夹
            DirectoryInfo INDEX_DIR = new DirectoryInfo(AppContext.BaseDirectory + "index");
            Analyzer analyzer = new PanGuAnalyzer(); //MMSegAnalyzer //StandardAnalyzer

            //添加文本内容
            var context = @"中国专业IT社区CSDN (Chinese Software Developer Network) 创立于1999年，致力于为中国软件开发者提供知识传播、在线学习、职业发展等全生命周期服务。
旗下拥有：专业的中文IT技术社区： CSDN.NET；移动端开发者专属APP： CSDN APP、CSDN学院APP；新媒体矩阵微信公众号：CSDN资讯、程序人生、GitChat、CSDN学院、AI科技大本营、区块链大本营、CSDN云计算、GitChat精品课、人工智能头条、CSDN企业招聘；IT技术培训学习平台： CSDN学院；技术知识移动社区： GitChat；人工智能新社区： TinyMind；权威IT技术内容平台：《程序员》+GitChat；IT人力资源服务：科锐福克斯；IT技术管理者平台：CTO俱乐部。";
            //也可指定其他字段信息如：id
            var id = "77889111111111111111";

            IndexWriter iw = new IndexWriter(FSDirectory.Open(INDEX_DIR), analyzer, true, IndexWriter.MaxFieldLength.LIMITED);

            //存储文档添加索引
            Document doc = new Document();
            //指定上方变量：内容存储的key以及解析模式
            doc.Add(new Field("body", context, Field.Store.YES, Field.Index.ANALYZED));
            //指定上方变量：id存储的key以及解析模式
            doc.Add(new Field("id", id, Field.Store.YES, Field.Index.NOT_ANALYZED));

            //将解析完成的内容存储
            iw.AddDocument(doc);

            iw.Commit();
            iw.Optimize();
            iw.Dispose();
        }

        [TestMethod]
        public void TestSearchPanGu()
        {
            var keyword = "致力于为中国软件开发者提供知识传播";
            DirectoryInfo INDEX_DIR = new DirectoryInfo(AppContext.BaseDirectory + "index");
            Analyzer analyzer = new PanGuAnalyzer(); //MMSegAnalyzer //StandardAnalyzer

            IndexSearcher searcher = new IndexSearcher(FSDirectory.Open(INDEX_DIR), true);
            //配置要检索的Key
            QueryParser qp = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "body", analyzer);
            //对检索内容进行分词
            Lucene.Net.Search.PhraseQuery query = (Lucene.Net.Search.PhraseQuery)qp.Parse(keyword);
            Lucene.Net.Search.PhraseQuery realquery = new PhraseQuery();
            var terms = query.GetTerms();
            foreach(Term term in terms)
            {
                realquery.Add(new Term(term.Field,term.Text + "*"));
            }

            Console.WriteLine("分词结果 {0}", query);

            //查询分词内容，以及设置返回检索结果集数量
            TopDocs tds = searcher.Search(realquery, 1000);
            Console.WriteLine("检索结果数: " + tds.TotalHits);
            foreach (ScoreDoc sd in tds.ScoreDocs)
            {
                //查询匹配分数
                Console.WriteLine(sd.Score);

                Document doc = searcher.Doc(sd.Doc);
                Console.WriteLine(doc.Get("body"));
                Console.WriteLine(doc.Get("id"));
            }
            searcher.Dispose();
        }

        [TestMethod]
        public void TestFts5()
        {
            var m_dbConnection = new SqliteConnection(@"Data Source=.\test.db");
            m_dbConnection.Open();

            //string sql = $"select * from message where message match 'body:确认交易*'";
            //SqliteCommand command = new SqliteCommand(sql, m_dbConnection);
            //var reader = command.ExecuteReader();
            //while (reader.Read())
            //{
            //    var num = reader[0];
            //    var name = reader[1];
            //}


            //for (int i = 0; i < 10000; i++)
            //{
            //    string sql = $"insert into message (title,body) values ({i},'确认交易Transaction-ETH,txid0x{i}a3670cf72abc54cfcc093796790f7958ed490db705d8d9eb99e47914be2ce3 r:0x2319db93bf503f850d623b3e67e267685f5f2681 s:0xb5d85cbf7cb3ee0d56b3bb207d5fc4b82f43f511 a:0.01271899')";
            //    SqliteCommand command = new SqliteCommand(sql, m_dbConnection);
            //    command.ExecuteNonQuery();
            //}

            string sql = "CREATE VIRTUAL TABLE message USING fts3(title, body, tokenize=ICU)";
            SqliteCommand command = new SqliteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            m_dbConnection.Close();
        }

        [TestMethod]
        public void TestLogger()
        {
            Global.ServiceProvider.GetService<ILogger<Test>>().LogDebug("LogDebug");
            Global.ServiceProvider.GetService<ILogger<Test>>().LogError(new Exception("abc") , "错误异常");
            Global.ServiceProvider.GetService<ILogger<Test>>().LogInformation("normal");
            Thread.Sleep(10000);
        }



        [TestMethod]
        public void WriteMuilteLog()
        {
            var logService = _serviceProvider.GetService<LogService>();
            for (int i = 0; i < 1000000; i++)
            {
               
                logService.WriteLog(new Jack.RemoteLog.WebApi.Dtos.WriteLogModel
                {
                    ApplicationContext = "UnitTest",
                    SourceContext = "test",
                    Content = "Exchange:Exchange.BlockScan 收到BlockScan信息：{\"BlockNumber\":43325451,\"Txid\":\"04b394121551767dd7237d8fcaf84eab31f102f3bd03122b94bf784581217ec5\",\"Amount\":99999.0,\"Time\":\"1970-01-01T00:00:00+00:00\",\"Confirmations\":3,\"Valid\":true,\"PropertyId\":\"TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t\",\"CoinType\":202,\"TronTransactionType\":\"TriggerSmartContract\",\"ContractRet\":\"SUCCESS\",\"Fee\":0.0,\"SenderAddress\":\"TBA6CypYJizwA9XdC7Ubgc5F1bxrQ7SqPt\",\"ReceivedAddress\":\"TVKCgFfuuzu11idqBjqMoSUDvmVQYnJWwY\",\"Coin\":\"USDT\"}",
                    Level = Microsoft.Extensions.Logging.LogLevel.Debug,
                    Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                });

                if(i % 1000 == 0)
                    Thread.Sleep(1);
            }
        }
    }

    class TestObject:IDisposable
    {
        public TestObject()
        {
            Debug.WriteLine("运行构造函数");
        }

        public void Dispose()
        {
            Debug.WriteLine("释放了");
        }
    }
}