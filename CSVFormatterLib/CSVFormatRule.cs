using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSVFormatterLib
{
    public class CSVFormatRule :  ICSVFormatRule   
    {
        #region ICSVFormatRule Members

        public string ColumnName { get; set; }
        public string MappedName { get; set; }
        public string FormatString { get; set; }
        public CSVFormatType FormatType { get; set; }
        public bool Required { get; set; }

        #endregion
    }
}
