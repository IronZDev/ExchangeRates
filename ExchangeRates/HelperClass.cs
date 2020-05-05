using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRates
{
    public static class HelperClass
    {
        public static string formatDateTimeOffset(DateTimeOffset date)
        {
            return date.ToString("yyyy-MM-dd");
        }
    }
}
