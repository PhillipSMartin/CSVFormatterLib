using CSVFormatterLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;

namespace CSVFormatterTests
{
    
    
    /// <summary>
    ///This is a test class for CSVFormatterTest and is intended
    ///to contain all CSVFormatterTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CSVFormatterTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
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

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            string xml =
                "<ClientReportDataset xmlns=\"http://tempuri.org/ClientReportDataset.xsd\">" +
                    "<EndOfDayTradeReportForClient>" +
                        "<Account>123456578</Account>" +
                        "<BuySell>Buy</BuySell>" +
                        "<OptionClassSymbol>RUT</OptionClassSymbol>" +
                        "<Quantity>20</Quantity>" +
                        "<StrikePrice>25.5</StrikePrice>" +
                        "<ExpirationDate>2013-10-01</ExpirationDate>" +
                        "<OptionType>Call</OptionType>" +
                    "</EndOfDayTradeReportForClient>" +
                    "<EndOfDayTradeReportForClient>" +
                        "<Account>123456578</Account>" +
                        "<BuySell>Sell</BuySell>" +
                        "<OptionClassSymbol>RUT</OptionClassSymbol>" +
                    "</EndOfDayTradeReportForClient>" +
                "</ClientReportDataset>";
            _doc = XDocument.Parse(xml);
            _parentNodes = _doc.Descendants("{http://tempuri.org/ClientReportDataset.xsd}EndOfDayTradeReportForClient").ToList();

            _rules = new List<ICSVFormatRule>();
            _rules.Add(new CSVFormatRule { ColumnName = "Account", FormatString = "Adar" });
            _rules.Add(new CSVFormatRule { ColumnName = "Buy/Sell", MappedName = "BuySell", FormatString = "{Buy,B}{Sell,S}", FormatType = CSVFormatType.Map });
            _rules.Add(new CSVFormatRule { ColumnName = "Quantity", MappedName = "Quantity" });
            _rules.Add(new CSVFormatRule { ColumnName = "Option Class", MappedName = "OptionClassSymbol" });
            _rules.Add(new CSVFormatRule { ColumnName = "Strike", MappedName = "StrikePrice", FormatString = "{0:f2}", FormatType = CSVFormatType.Numeric, Required = true });
            _rules.Add(new CSVFormatRule { ColumnName = "Expiration", MappedName = "ExpirationDate", FormatString = "{0:d}", FormatType = CSVFormatType.Datetime });
            _rules.Add(new CSVFormatRule { ColumnName = "Call/Put", MappedName = "OptionType", FormatString = "{Call,C}{Put,P}", FormatType = CSVFormatType.Map });
        }

        private static XDocument _doc;
        private static List<XElement> _parentNodes;
        private static List<ICSVFormatRule> _rules;

        private static string expectedXmlHeaders = "Account," +
            "BuySell," +
            "OptionClassSymbol," +
            "Quantity," +
            "StrikePrice," +
            "ExpirationDate," +
            "OptionType";
        private static string expectedFormatRuleHeaders = "Account," +
            "Buy/Sell," +
            "Quantity," +
            "\"Option Class\"," +
            "Strike," +
            "Expiration," +
            "Call/Put";
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for WriteHeaders
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CSVFormatterLib.dll")]
        public void WriteHeadersFromXElementTest()
        {
            XElement parentNode = _parentNodes[0];
            string expected = expectedXmlHeaders;
            string actual = CSVFormatter_Accessor.WriteHeaders(parentNode, null);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [DeploymentItem("CSVFormatterLib.dll")]
        public void WriteHeadersFromRulesTest()
        {
            XElement parentNode = _parentNodes[0];
            string expected = expectedFormatRuleHeaders;
            string actual;
            actual = CSVFormatter_Accessor.WriteHeaders(parentNode, _rules);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Translate
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CSVFormatterLib.dll")]
        public void TranslateTest()
        {
            string inputString = "PUT";
            string translateMap = "{Call,C}{Put,P}";
            string expected = "P";
            string actual;
            actual = CSVField_Accessor.Translate(inputString, translateMap);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for Translate
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CSVFormatterLib.dll")]
        public void TranslateErrorTest()
        {
            string inputString = "Banana";
            string translateMap = "{Call,C}{Put,P}";
            string expected = string.Format(CSVErrorMessages_Accessor.ErrorMsgMapEntryNotFound, inputString, translateMap);
            string actual;
            actual = CSVField_Accessor.Translate(inputString, translateMap);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for FormatField
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CSVFormatterLib.dll")]
        public void FormatErrorTest()
        {
            string fieldValue = "20";
            CSVFormatRule rule = new CSVFormatRule { ColumnName = "Strike", MappedName = "StrikePrice", FormatString = "{1:f2}", FormatType = CSVFormatType.Numeric };
            string expected = string.Format(CSVErrorMessages_Accessor.ErrorMsgFormatError, fieldValue, rule.FormatString);
            string actual;
            actual = CSVFormatter_Accessor.FormatField(fieldValue, rule);
            Assert.AreEqual(expected, actual);
        }


        /// <summary>
        ///A test for FormatField
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CSVFormatterLib.dll")]
        public void FormatConstantTest()
        {
            string fieldValue = string.Empty;
            ICSVFormatRule rule = _rules[0]; // account
            string expected = "Adar";
            string actual;
            actual = CSVFormatter_Accessor.FormatField(fieldValue, rule);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for FormatField
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CSVFormatterLib.dll")]
        public void FormatNumericTest()
        {
            string fieldValue = "20";
            ICSVFormatRule rule = _rules[4]; // strikeprice
            string expected = "20.00";
            string actual;
            actual = CSVFormatter_Accessor.FormatField(fieldValue, rule);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for FormatField
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CSVFormatterLib.dll")]
        public void FormatDateTest()
        {
            string fieldValue = "2013-10-01";
            ICSVFormatRule rule = _rules[5]; // expirationdate
            string expected = "10/1/2013";
            string actual;
            actual = CSVFormatter_Accessor.FormatField(fieldValue, rule);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for FormatField
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CSVFormatterLib.dll")]
        public void FormatMapTest()
        {
            string fieldValue = "Call";
            ICSVFormatRule rule = _rules[6]; // optiontype
            string expected = "C";
            string actual;
            actual = CSVFormatter_Accessor.FormatField(fieldValue, rule);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ConvertWithHeadersWithoutRulesTest
        ///</summary>
        [TestMethod()]
        public void ConvertWithHeadersWithoutRulesTest()
        {
            XDocument doc = _doc;
            string parentElementName = "EndOfDayTradeReportForClient";

            IList<string> expected = new List<string>();
            expected.Add(expectedXmlHeaders);
            expected.Add("123456578,Buy,RUT,20,25.5,2013-10-01,Call");
            expected.Add("123456578,Sell,RUT");
 
            IList<string> actual;
            actual = CSVFormatter.ConvertWithHeaders(doc, parentElementName);
            CompareStringLists(expected, actual);
          }

        /// <summary>
        ///A test for ConvertWithHeadersWithRulesTest
        ///</summary>
        [TestMethod()]
        public void ConvertWithHeadersWithRulesTest()
        {
            XDocument doc = _doc;
            string parentElementName = "EndOfDayTradeReportForClient";

            IList<string> expected = new List<string>();
            expected.Add(expectedFormatRuleHeaders);
            expected.Add("Adar,B,20,RUT,25.50,10/1/2013,C");
            expected.Add(String.Format("Adar,S,,RUT,\"{0}\",,",
                String.Format(CSVErrorMessages_Accessor.ErrorMsgElementNotFound, "{http://tempuri.org/ClientReportDataset.xsd}StrikePrice")));

            IList<string> actual;
            actual = CSVFormatter.ConvertWithHeaders(doc, parentElementName, _rules);
            CompareStringLists(expected, actual);
        }

        private void CompareStringLists(IList<string> expectedList, IList<string> actualList)
        {
            Assert.AreEqual(expectedList.Count, actualList.Count, "lists do not have the same number of elements");
            if (expectedList.Count == actualList.Count)
            {
                for (int n = 0; n < expectedList.Count; n++)
                {
                    Assert.AreEqual(expectedList[n], actualList[n]);
                }
            }
        }

        /// <summary>
        ///A test for Convert
        ///</summary>
        [TestMethod()]
        public void ConvertWithoutRulesTest()
        {
            XDocument doc = _doc;
            string parentElementName = "EndOfDayTradeReportForClient";

            IList<string> expected = new List<string>();
            expected.Add("123456578,Buy,RUT,20,25.5,2013-10-01,Call");
            expected.Add("123456578,Sell,RUT");

            IList<string> actual;
            actual = CSVFormatter.Convert(doc, parentElementName);
            CompareStringLists(expected, actual);
        }

        /// <summary>
        ///A test for Convert
        ///</summary>
        [TestMethod()]
        public void ConvertWithRulesTest()
        {
            XDocument doc = _doc;
            string parentElementName = "EndOfDayTradeReportForClient";

            IList<string> expected = new List<string>();
            expected.Add("Adar,B,20,RUT,25.50,10/1/2013,C");
            expected.Add(String.Format("Adar,S,,RUT,\"{0}\",,",
                String.Format(CSVErrorMessages_Accessor.ErrorMsgElementNotFound, "{http://tempuri.org/ClientReportDataset.xsd}StrikePrice")));

            IList<string> actual;
            actual = CSVFormatter.Convert(doc, parentElementName, _rules);
            CompareStringLists(expected, actual);
        }
    }
}
