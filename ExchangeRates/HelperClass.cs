using System;

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
