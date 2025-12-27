using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace TnieYuPackage.FileData
{
    [Serializable]
    public class CsvSerializer
    {
        private static CsvConfiguration csvConfiguration;
        
        public static CsvConfiguration GetCsvConfiguration()
        {
            if (csvConfiguration == null)
            {
                csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    IgnoreBlankLines = true,
                    TrimOptions = TrimOptions.Trim,
                    MissingFieldFound = null,   // Không lỗi khi thiếu cột
                    BadDataFound = null,        // Không lỗi khi dữ liệu hỏng
                    DetectColumnCountChanges = false,
                };
            }

            return csvConfiguration;
        } 
        
        public void WriteListData<T>(string filepath, IEnumerable<T> datas)
        {
            using var writer = new StreamWriter(filepath, false, Encoding.UTF8);
            using var csvWriter = new CsvWriter(writer, GetCsvConfiguration());

            csvWriter.WriteHeader<T>();
            csvWriter.NextRecord();
            
            csvWriter.WriteRecords(datas);
            csvWriter.Flush();
        }
        
        public IEnumerable<T> ReadListData<T>(string filepath)
        {
            using var reader = new StreamReader(filepath, Encoding.UTF8);
            using var csvReader = new CsvReader(reader, GetCsvConfiguration());

            return csvReader.GetRecords<T>().ToList();
        }
    }
}