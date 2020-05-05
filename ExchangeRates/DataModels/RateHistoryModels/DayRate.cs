using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRates.DataModels.RateHistoryModels
{
    public class DayRate
    {
        public string no { get; set; }
        public string effectiveDate { get; set; }
        public double mid { get; set; }
        public DateTime date
        {
            get
            {
                string[] dateParts = effectiveDate.Split('-');
                return new DateTime( Int32.Parse(dateParts[0]), Int32.Parse(dateParts[1]), Int32.Parse(dateParts[2]));
            }
        }
    }
}
