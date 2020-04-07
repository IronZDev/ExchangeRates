using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRates.DataModels
{
    public class Rate
    {
        public string currency { get; set; }
        public string code { get; set; }
        public double mid { get; set; }
        public string no { get; set; }
        public string effectiveDate { get; set; }
        public string CodeWithName
        {
            get
            {
                return $"{code} - {currency}";
            }
        }
    }
}
