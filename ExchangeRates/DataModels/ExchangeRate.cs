using System.Collections.Generic;


namespace ExchangeRates.DataModels
{
    public class ExchangeRate
    {
        public string table { get; set; }
        public string no { get; set; }
        public string effectiveDate { get; set; }
        public List<Rate> rates { get; set; }
        public string currency { get; set; }
        public string code { get; set; }
    }
}
