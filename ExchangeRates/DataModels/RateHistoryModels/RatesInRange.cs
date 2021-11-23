using System.Collections.Generic;

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
