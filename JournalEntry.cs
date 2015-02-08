using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;

namespace Aspensquare.Gl.Util
{
    public class JournalEntry
    {
        public int seg1 { get; set; }
        public int seg3 { get; set; }
        public decimal Amount { get; set; }
        public int seg2 { get; set; }
        public string lineItem { get; set; }
        public DateTime Date { get; set; }
    }
}
