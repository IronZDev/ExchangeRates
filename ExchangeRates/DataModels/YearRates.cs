using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRates.DataModels
{
    public class YearRates
    {
        public YearRates(string filename, string date)
        {
            this.filename = filename;
            this.date = date;
        }

        public string filename { get; set; }
        public string date { get; set; }
    }
}
