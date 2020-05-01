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
        public int converter { get; set; }
        public string CodeWithName
        {
            get
            {
                return $"{code} - {currency}";
            }
        }
        public void NormalizeMid()
        {
            int tempConverter = 1;
            if (mid < 0.1)
            {
                while (mid < 1)
                {
                    tempConverter *= 10;
                    mid *= 10;
                }
            }
            converter = tempConverter;
        }
        public string wholeRateValue
        {
            get
            {
                NormalizeMid();
                return $"{mid} {code} - Converter: {converter}";
            }
        }
        public string flagImage
        {
            get
            {
                return $"https://github.com/transferwise/currency-flags/blob/master/src/flags/{code.ToLower()}.png?raw=true";
            }
        }
    }
}
