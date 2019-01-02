// Read more https://github.com/ExcelDataReader/ExcelDataReader

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using ExcelDataReader;
using Monitor.Interfaces;

namespace Monitor.Providers
{
    public class ExcelFileUrlProvider : IUrlProvider
    {
        private IEnumerator rowsEnum;
        private object rowsLock = new object();
        private int callcounter = 0;

        public ExcelFileUrlProvider(Dictionary<string, object> args) : this((string)args["datafile"])
        { }

        public ExcelFileUrlProvider(string file)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            using (var dataset = reader.AsDataSet())
            {
                var rows = dataset.Tables[0].Rows;
                rowsEnum = rows.GetEnumerator();
                rowsEnum.MoveNext(); //Skipping the title
            }
        }

        public string nextField(int columnNumber)
        {
            return (string)nextRow()[columnNumber];
        }

        public DataRow nextRow()
        {
            lock (rowsLock)
            {
                ++callcounter;
                if (!rowsEnum.MoveNext()) return null;
                if (rowsEnum.Current is System.DBNull) return null;
                return (DataRow)rowsEnum.Current;
            }
        }
    }
}