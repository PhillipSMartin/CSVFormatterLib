using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSVFormatterLib
{
    public interface ICSVFormatRule
    {
        string ColumnName { get; }
        string MappedName { get;  }
        string FormatString { get; }
        CSVFormatType FormatType { get; }
        bool Required { get; }
    }
}
