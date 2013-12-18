using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CSVFormatterLib
{
    public class CSVFormatter
    {
        // This class will convert an xml string to a csv file
        // The Convert and ConvertWithHeaders methods take as arguments:
        //      An XDocument object
        //      A parentElementName - this xml element represents the row to be converted, child elements of this parent
        //          represent the comma-separated fields
        //      An optional list of ICSVFormatRules (see ICSVFormatRules.cs for an explanation)
        //
        //      For example, the following xml document
        //
        //      <ClientReportDataset xmlns="http://tempuri.org/ClientReportDataset.xsd">
        //          <EndOfDayTradeReportForClient>
        //              <OpenClose>C</OpenClose>
        //              <OptionClassSymbol>RUT</OptionClassSymbol>
        //              <BuySell>Buy</BuySell>
        //              <Quantity>20</Quantity> 
        //          </EndOfDayTradeReportForClient>
        //          <EndOfDayTradeReportForClient>
        //              <OpenClose>O</OpenClose>
        //              <OptionClassSymbol>RUT</OptionClassSymbol>
        //              <BuySell>Sell</BuySell><Quantity>12</Quantity>
        //          </EndOfDayTradeReportForClient>
        //         </ClientReportDataset>
        //
        //      will be formatted as follows (assuming no CSVFormatRules)
        //
        //      OpenClose,OptionClassSymbol,BuySell,Quantity
        //      C,RUT,Buy,20
        //      O,RUT,Sell,12
        //
        //      The first line will be included only if you call ConvertWithHeaders
        //      The output is returned as a List<string>
        //
        //      You may change the delimiter character to something other than a comma via the Delimiter property

        private static char _delimiter = ',';
 
        private CSVFormatter() { }

        #region Public Properties
        public static char Delimiter { get { return _delimiter; } set { _delimiter = value; } }
        #endregion


        #region Public Methods
        public static IList<string> Convert(XDocument doc, string parentElementName)
        {
           return DoConvert(doc, parentElementName, null, false);
        }
        public static IList<string> Convert(XDocument doc, string parentElementName, IList<ICSVFormatRule> rules)
        {
            return DoConvert(doc, parentElementName, rules, false);
        }
        public static IList<string> ConvertWithHeaders(XDocument doc, string parentElementName)
        {
            return DoConvert(doc, parentElementName, null, true);
        }
        public static IList<string> ConvertWithHeaders(XDocument doc, string parentElementName, IList<ICSVFormatRule> rules)
        {
            return DoConvert(doc, parentElementName, rules, true);
        }
        #endregion

        #region Private Methods
        private static IList<string> DoConvert(XDocument doc, string parentElementName, IList<ICSVFormatRule> rules, bool writeHeaders)
        {
            if (doc == null)
                throw new ArgumentNullException("doc");
            if (string.IsNullOrEmpty(parentElementName))
                throw new ArgumentNullException("parentElementName");

            // if the rules list is empty, change rules to null so we don't have to keep checking
            if (rules != null)
            {
                if (rules.Count <= 0)
                    rules = null;
            }

            List<string> output = new List<string>();

            // if the document includes a namespace, it must be used as a prefix in all element names
            string namePrefix = BuildNamePrefix(doc);

            // get a list of elements to convert into rows
            List<XElement> parentNodes = doc.Descendants(namePrefix + parentElementName).ToList();
            if (parentNodes.Count > 0)
            {
                // write row of headers if necessary
                if (writeHeaders)
                {
                    output.Add(WriteHeaders(parentNodes[0], rules));
                }

                // write each row of data
                foreach (XElement parentNode in parentNodes)
                {
                    // use rules if specifed
                    if (rules != null)
                        output.Add(BuildLine(parentNode, namePrefix, rules));

                    // otherwise write data as is
                    else
                        output.Add(BuildLine(parentNode));
                }
            }

            return output;

        }

        private static string BuildLine(XElement parentNode)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (XElement innerNode in parentNode.Elements())
            {
                AddField(stringBuilder, innerNode.Value);
            }
            return stringBuilder.ToString();
        }

        private static string BuildLine(XElement parentNode, string namePrefix, IList<ICSVFormatRule> rules)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (ICSVFormatRule rule in rules)
            {
                string fieldValue = GetFieldValue(namePrefix, rule.MappedName, parentNode);
                if (string.IsNullOrEmpty(fieldValue) && rule.Required)
                {
                    AddField(stringBuilder, string.Format(CSVErrorMessages.ErrorMsgElementNotFound, namePrefix + rule.MappedName));
                }
                else
                {
                    AddField(stringBuilder, FormatField(fieldValue, rule));
                }
            }

            return stringBuilder.ToString();
        }

        private static string FormatField(string fieldValue, ICSVFormatRule rule)
        {
             CSVField field = new CSVField(fieldValue, rule);
             return field.ToString();
        }

        private static string GetFieldValue(string namePrefix, string elementName, XElement parentNode)
        {
            if (!string.IsNullOrEmpty(elementName))
            {
                XElement innerNode = parentNode.Element(namePrefix + elementName);
                if (innerNode != null)
                {
                    return innerNode.Value;
                }
            }

            return null;
         }

        // return a string of delimeter-separated column headers
        private static string WriteHeaders(XElement parentNode, IList<ICSVFormatRule> rules)
        {
            StringBuilder stringBuilder = new StringBuilder();
  
            // if we have rules, get column headers from the rules
            if (rules != null)
            {
                foreach (ICSVFormatRule rule in rules)
                {
                    AddField(stringBuilder, rule.ColumnName);
                }
            }

            // if not, build it from the element list
            else
            {
                foreach (XElement innerNode in parentNode.Elements())
                {
                    AddField(stringBuilder, innerNode.Name.LocalName);
                }
             }

            return stringBuilder.ToString();
        }

        // enclose field in quotes if it contains a space or comma
        // add it to string builder preceded by a comma if necessary
        private static void AddField(StringBuilder stringBuilder, string fieldValue)
        {
            // use an empty string if fieldValue is null
            fieldValue = fieldValue ?? String.Empty;

            if (fieldValue.Contains(' ') || fieldValue.Contains(','))
            {
                fieldValue = "\"" + fieldValue + "\"";
            }
            stringBuilder.AppendFormat((stringBuilder.Length > 0 ? _delimiter.ToString() : "") + "{0}", fieldValue);
        }

        // extract namespace and, if it exists, surround it with brackets to be used as a prefix for the element name
        private static string BuildNamePrefix(XDocument doc)
         {
             string nameSpace = doc.Root.Name.NamespaceName;
             if (!String.IsNullOrEmpty(nameSpace))
             {
                 return "{" + nameSpace + "}";
             }
             else
             {
                 return String.Empty;
             }
         }
       #endregion
    }
}
