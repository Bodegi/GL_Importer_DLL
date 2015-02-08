using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace Aspensquare.Gl.Util
{
    public class JournalEntries
    {
        public List<JournalEntry> Entries { get; set; }
        public List<string> Errors { get; set; }

        public JournalEntries()
        {
            this.Entries = new List<JournalEntry>();
            this.Errors = new List<string>();
        }

        private JournalEntry GetValueFromRow(Row r, SharedStringTablePart sharedStrings)
        {

            foreach (var item in r.Elements<Cell>())
            {
                if (item.DataType != CellValues.SharedString)
                {
                    Console.WriteLine(item.CellValue.ToString());
                }
                else
                {
                    int sharedStringID = -1;
                    if (Int32.TryParse(item.CellValue.ToString(), out sharedStringID))
                    {
                        var ssi = sharedStrings.SharedStringTable.Elements<SharedStringItem>().ElementAt(sharedStringID);
                    }
                    else
                    {

                    }
                }
            }
            return new JournalEntry();
        }

        private object GetValueAt(Row r, int colIndex, SharedStringTablePart sharedStrings)
        {
            Cell cell = r.Descendants<Cell>().ElementAt(colIndex);
            if(cell.DataType != null && cell.DataType == CellValues.SharedString)
            {
                var ssi = sharedStrings.SharedStringTable.Elements<SharedStringItem>().ElementAt(Int32.Parse(cell.CellValue.InnerText));
                return ssi.InnerText;
            }
            return cell.CellValue.InnerText;
        }

        private DateTime GetDate(double rawdate)
        {
            DateTime Date = DateTime.FromOADate(rawdate);
            return Date;
        }

        private void Validation(List<JournalEntry> Entries)
        {
            var DateQuery = from JE in Entries group JE by JE.Date into d orderby d.Key select d;
            foreach (var date in DateQuery)
            {
                decimal total = 0;
                var query = from test in Entries where test.Date == date.Key select test;
                foreach (var q in query)
                {
                    total = total + q.Amount;
                }
                if (total != 0)
                {
                    Errors.Add("Balance is not 0 for the entries on " + date.Key.ToShortDateString());
                }
            }
        }

        public static void ExportToXml(List<JournalEntry> Entries)
        {
            string testPath = @"xmltest.xml";
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("   ");
            XmlWriter xmlOut = XmlWriter.Create(testPath, settings);
            xmlOut.WriteStartDocument();
            xmlOut.WriteStartElement("GL_Import");
            foreach(JournalEntry e in Entries)
            {
                xmlOut.WriteStartElement("JournalEntry");
                xmlOut.WriteElementString("Date", e.Date.ToShortDateString());
                xmlOut.WriteElementString("Seg1", e.seg1.ToString());
                xmlOut.WriteElementString("Seg2", e.seg2.ToString());
                xmlOut.WriteElementString("Seg3", e.seg3.ToString());
                xmlOut.WriteElementString("Amount", e.Amount.ToString());
                xmlOut.WriteElementString("Description", e.lineItem);
                xmlOut.WriteEndElement();
            }
            xmlOut.WriteEndElement();
            xmlOut.Close();
        }

        public JournalEntries(string path)
        {
            this.Entries = new List<JournalEntry>();
            this.Errors = new List<string>();
            List<DateTime> Dates = new List<DateTime>();

            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(path, false))
            {
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.Last();
                SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
                ConfigHandler Config = new ConfigHandler();
                foreach (Row r in sheetData.Elements<Row>())
                {
                    try
                    {
                        Entries.Add(new JournalEntry
                        {
                            Date = GetDate(Double.Parse(GetValueAt(r, Config.Date, workbookPart.SharedStringTablePart).ToString())),
                            seg1 = Int32.Parse(GetValueAt(r, Config.seg1, workbookPart.SharedStringTablePart).ToString()),
                            seg2 = Int32.Parse(GetValueAt(r, Config.seg2, workbookPart.SharedStringTablePart).ToString()),
                            seg3 = Int32.Parse(GetValueAt(r, Config.seg3, workbookPart.SharedStringTablePart).ToString()),
                            Amount = decimal.Parse(GetValueAt(r, Config.Amount, workbookPart.SharedStringTablePart).ToString()),
                            lineItem = GetValueAt(r, Config.lineItem, workbookPart.SharedStringTablePart).ToString()
                        });
                    }
                    catch (Exception)
                    {
                        string error = "Incorrect value or format at row " + r.RowIndex.ToString();
                        Errors.Add(error);
                    }
                }
                Validation(Entries);
            }
        }
        
        public JournalEntries(System.IO.Stream stream)
        {
            this.Entries = new List<JournalEntry>();
            this.Errors = new List<string>();
            List<DateTime> Dates = new List<DateTime>();

            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(stream, false))
            {
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.Last();
                SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
                ConfigHandler Config = new ConfigHandler();
                foreach (Row r in sheetData.Elements<Row>())
                {
                    try
                    {
                        Entries.Add(new JournalEntry
                        {
                            Date = GetDate(Double.Parse(GetValueAt(r, Config.Date, workbookPart.SharedStringTablePart).ToString())),
                            seg1 = Int32.Parse(GetValueAt(r, Config.seg1, workbookPart.SharedStringTablePart).ToString()),
                            seg2 = Int32.Parse(GetValueAt(r, Config.seg2, workbookPart.SharedStringTablePart).ToString()),
                            seg3 = Int32.Parse(GetValueAt(r, Config.seg3, workbookPart.SharedStringTablePart).ToString()),
                            Amount = decimal.Parse(GetValueAt(r, Config.Amount, workbookPart.SharedStringTablePart).ToString()),
                            lineItem = GetValueAt(r, Config.lineItem, workbookPart.SharedStringTablePart).ToString()
                        });
                    }
                    catch (Exception)
                    {
                        string error = "Incorrect value or format at row " + r.RowIndex.ToString();
                        Errors.Add(error);
                    }
                }
                Validation(Entries);
            }
        }
    }
}
