using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSVFormatterLib
{
    internal class CSVField
    {
        private string _inputString;
        private object _fieldValue;
        private string _fieldFormatString;

        internal object InputString { get { return _inputString; } }
        internal object FieldValue { get { return _fieldValue; } }
        internal string FieldFormatString { get { return _fieldFormatString; } }

        internal CSVField(string inputString, ICSVFormatRule rule)
        {
            _inputString = inputString ?? String.Empty;
            _fieldValue = String.Empty;

            try
            {
                switch (rule.FormatType)
                {
                    case CSVFormatType.Datetime:
                        if (!string.IsNullOrEmpty(_inputString))
                        {
                            _fieldValue = DateTime.Parse(_inputString);
                            _fieldFormatString = rule.FormatString;
                        }
                        break;

                    case CSVFormatType.DatetimeOffset:
                        if (!string.IsNullOrEmpty(_inputString))
                        {
                            _fieldValue = DateTimeOffset.Parse(_inputString);
                            _fieldFormatString = rule.FormatString;
                        }
                        break;

                    case CSVFormatType.Map:
                        if (!string.IsNullOrEmpty(_inputString))
                        {
                            // formatString contains translate map
                            _fieldValue = Translate(_inputString, rule.FormatString);
                            _fieldFormatString = null;
                        }
                        break;

                    case CSVFormatType.Numeric:
                        if (!string.IsNullOrEmpty(_inputString))
                        {
                            _fieldValue = Double.Parse(_inputString);
                            _fieldFormatString = rule.FormatString;
                        }
                        break;

                    case CSVFormatType.String:
                        _fieldValue = _inputString;
                        _fieldFormatString = rule.FormatString;
                        break;

                    case CSVFormatType.Timespan:
                        if (!string.IsNullOrEmpty(_inputString))
                        {
                            _fieldValue = TimeSpan.Parse(_inputString);
                            _fieldFormatString = rule.FormatString;
                        }
                        break;
                }
            }
            catch (System.FormatException)
            {
                _fieldValue = string.Format(CSVErrorMessages.ErrorMsgParsingError, _inputString, rule.FormatType);
                _fieldFormatString = null;
            }
            catch (System.OverflowException)
            {
                _fieldValue = string.Format(CSVErrorMessages.ErrorMsgOverflowError, _inputString, rule.FormatType);
                _fieldFormatString = null;
            }
        }

        public override string ToString()
        {
            try
            {
                if (_fieldFormatString == null)
                    return _fieldValue.ToString();
                else
                    return string.Format(_fieldFormatString, _fieldValue);
            }
            catch (System.FormatException)
            {
                return string.Format(CSVErrorMessages.ErrorMsgFormatError, _fieldValue, _fieldFormatString);
            }
        }

        // translate field value into a different string based on translateMap
        // translateMap is of the format
        //      {Call,C}{Put,P}
        //  this map will return "C" if the inputString is "Call", "P" if the inputString is "Put", and an error message otherwise
        private static string Translate(string inputString, string translateMap)
        {
            inputString = inputString ?? String.Empty;
            translateMap = translateMap ?? String.Empty;

            int fromStringLocation = translateMap.IndexOf("{" + inputString + ",", StringComparison.CurrentCultureIgnoreCase);
            int toStringLocation = fromStringLocation + inputString.Length + 2;
            int toStringLength = -1;
            if (fromStringLocation >= 0)
            {
                toStringLength = translateMap.IndexOf("}", toStringLocation) - toStringLocation;
            }

            if (toStringLength >= 0)
            {
                return translateMap.Substring(toStringLocation, toStringLength);
            }
            else
            {
                return string.Format(CSVErrorMessages.ErrorMsgMapEntryNotFound, inputString, translateMap);
            }

        }

    }
   
}
