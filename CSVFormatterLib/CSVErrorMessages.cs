using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSVFormatterLib
{
    internal class CSVErrorMessages
    {
        public const string ErrorMsgParsingError = "***ERROR: UNABLE TO PARSE \'{0}\' AS A {1:g}***";
        public const string ErrorMsgOverflowError = "***ERROR: OVERFLOW PARSING \'{0}\' AS A {1:g}***";
        public const string ErrorMsgElementNotFound = "***ERROR: ELEMENT {0} NOT FOUND***";
        public const string ErrorMsgFormatError = "***ERROR: UNABLE TO FORMAT \'{0}\' VIA FORMAT STRING \'{1}\'***";
        public const string ErrorMsgMapEntryNotFound = "***ERROR: ENTRY \'{0}\' NOT FOUND IN TRANSLATE MAP {1}***";

        private CSVErrorMessages() {}
    }
}
