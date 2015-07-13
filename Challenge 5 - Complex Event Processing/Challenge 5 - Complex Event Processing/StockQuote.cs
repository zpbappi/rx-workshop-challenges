using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace ComplexEventProcessing
{
    class StockQuote
    {
        public string Symbol { get; set; }
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }

        public static IEnumerable<StockQuote> LoadQuotes()
        {
            return LoadQuotes(@"..\..\Data");
        }

        static IEnumerable<StockQuote> LoadQuotes(string path)
        {
            return from filepath in Directory.EnumerateFiles(path, "*.csv")
                   let symbol = Path.GetFileNameWithoutExtension(filepath)
                   from quote in LoadQuotes(symbol, filepath)
                   select quote;
        }

        static IEnumerable<StockQuote> LoadQuotes(string symbol, string path)
        {
            using (var reader = File.OpenText(path))
            {
                reader.ReadLine();
                var prevQuote = default(StockQuote);
                for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    var elements = line.Split(',');

                    var date = DateTime.ParseExact(elements[0], "d-MMM-yy", CultureInfo.InvariantCulture);
                    var open = decimal.Parse(elements[1]);
                    var high = decimal.Parse(elements[2]);
                    var low = decimal.Parse(elements[3]);
                    var close = decimal.Parse(elements[4]);
                    var volume = long.Parse(elements[5]);

                    var quote = new StockQuote
                    {
                        Symbol = symbol,
                        Date = date,
                        Close = close,
                        High = high,
                        Low = low,
                        Open = open,
                        Volume = volume
                    };

                    yield return quote;

                    prevQuote = quote;
                }
            }
        }
    }
}
