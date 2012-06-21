using MySoft.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace MySoft.Data.UnitTest
{


    /// <summary>
    ///这是 DbFieldTest 的测试类，旨在
    ///包含所有 DbFieldTest 单元测试
    ///</summary>
    [TestClass()]
    public class DbFieldTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 附加测试特性
        // 
        //编写测试时，还可使用以下特性:
        //
        //使用 ClassInitialize 在运行类中的第一个测试前先运行代码
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //使用 ClassCleanup 在运行完类中的所有测试后再运行代码
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //使用 TestInitialize 在运行每个测试前先运行代码
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //使用 TestCleanup 在运行完每个测试后运行代码
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///Name 的测试
        ///</summary>
        [TestMethod()]
        public void NameTest()
        {
            //Field field = new Field("userid").Func(1, "datediff", "day", "getdate()");

            Field field = Field.Func("upper({0})", new Field("userid"));
            Field.Func("isnull({0},1)", new Field("aaa"));

            WhereClip where = new Field("userid").Between(1, 10);

            string fieldName = "userid"; // TODO: 初始化为适当的值
            DbField target = new DbField(fieldName); // TODO: 初始化为适当的值
            string actual;
            actual = target.Name;
            Assert.AreEqual(actual, fieldName);
            //Assert.Inconclusive("验证此测试方法的正确性。");
        }

        [TestMethod]
        public void TaskTest()
        {
            var dict = new System.Collections.Generic.Dictionary<int, object>();
            for (int i = 0; i < 100; i++)
            {
                dict[i] = "test" + i;
            }

            var keys = dict.Keys.Take(10).ToList();

            //DbSession.Default.InsertOrUpdate()
        }

        [TestMethod]
        public void QueryCreatorTest()
        {
            var qc = QueryCreator.NewCreator().From("document")
                .Join("document_attr", "document.did = document_attr.did")
                .Join("document_content", "document.did = document_content.did")
                .AddField("document", "*")
                .AddField("document_attr", "*")
                .AddField("document_attr", "*")
                .AddWhere("document.did = 1");

            var value = DbSession.Default.From(qc).ToTable();
        }
    }
}
