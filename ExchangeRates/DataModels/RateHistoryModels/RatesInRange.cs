using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRates.DataModels.RateHistoryModels
{
    class RatesInRange
    {
        public string table { get; set; }

        public string currency { get; set; }

        public string code { get; set; }

        public List<DayRate> rates { get; set; }
    }
}
