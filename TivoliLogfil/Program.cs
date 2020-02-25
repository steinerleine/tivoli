using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace TivoliLogfil
{
    class Program
    {
        private const string tab = "\t";
        private const string fileName = @"\Data\errors.csv";

        static void Main(string[] args)
        {
            string path = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            path = Directory.GetParent(Directory.GetParent(path).FullName).FullName + fileName;

            List<LogfilData> results = ReadCSV(path);

            Console.WriteLine("Statuskoder:");
            var statusGrupper = results.GroupBy(x => x.StatusCode);
            foreach (var grp in statusGrupper)
            {
                int statuscode = grp.Key;
                int antal = grp.Count();
                Console.WriteLine(tab + statuscode + " : " + antal);
            }
            Console.WriteLine("Millisekunder:");
            Console.WriteLine(tab + "MAX:" + results.Max(x => x.Milliseconds));
            Console.WriteLine(tab + "MIN:" + results.Min(x => x.Milliseconds));
            Console.WriteLine(tab + "AVG:" + results.Average(x => x.Milliseconds));

            Console.WriteLine("Balance:");
            Console.WriteLine(tab + "MIN:" + results.Min(x => x.Balance));
            Console.WriteLine(tab + "MIN2:" + results.Where(x => x.StatusCode == -3).Min(x => x.Balance));  
            Console.Read();
        }

        private static List<LogfilData> ReadCSV(string filePath)
        {
            List<LogfilData> results = new List<LogfilData>();
            //For at styre tegnsætning
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ",";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            StreamReader reader = new StreamReader(filePath);
            using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
            {
                csv.Configuration.HasHeaderRecord = true;
                csv.Configuration.RegisterClassMap<LogfilMapper>();
                csv.Configuration.Delimiter = ";";
                csv.Configuration.CultureInfo = customCulture;

                while (csv.Read())
                {
                    LogfilData newRow = csv.GetRecord<LogfilData>();
                    newRow.Balance = ExtractFloat(newRow.Details);
                    results.Add(newRow);
                }

            }
            return results;
        }


        private static float? ExtractFloat(string str)
        {
            string searchString = "balance of ";
            float? result = null;
            int index = str.IndexOf(searchString);

            if (index > 0)
            {
                str = str.Substring(index + searchString.Length, str.Length - (index + searchString.Length));
                index = str.IndexOf("\r");
                str = str.Substring(0, index);
                str = str.Replace(".", ",");
                try
                {
                    result = float.Parse(str);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Couldn't parse float: " + e.Message);
                }

            }
            return result;
        }

        public class LogfilData
        {
            public int CustomerContactId { get; set; }
            
            public int StatusCode { get; set; }

            public string StatusName { get; set; }
            
            public string Details { get; set; }
            
            public Single Milliseconds { get; set; }
            
            public float? Balance { get; set; }
        }

        public class LogfilMapper : ClassMap<LogfilData>
        {
            public LogfilMapper()
            {
                Map(m => m.CustomerContactId).Index(0);
                Map(m => m.StatusCode).Index(1);
                Map(m => m.StatusName).Index(2);
                Map(m => m.Details).Index(3);
                Map(m => m.Milliseconds).Index(4);


            }
        }

    }
}