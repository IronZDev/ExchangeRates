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
