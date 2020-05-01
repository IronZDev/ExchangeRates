using ExchangeRates.DataModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ExchangeRates
{
    class WebHandler
    {
        HttpClient httpClient;
        public DataViewModel ViewModel { get; set; }

        public WebHandler()
        {
            this.httpClient = new HttpClient(); ;
            ViewModel = DataViewModel.getInstance();
        }

        public async Task GetDates()
        {
            int currentYear = DateTime.Today.Year;
            List<YearRates> rates = new List<YearRates>();
            string responseString = "";
            string line;
            StringReader reader;
            for (int year = DateTime.Today.Year; year >= 2002; year--)
            {

                try
                {
                    if (year == currentYear)
                        responseString = await httpClient.GetStringAsync($"http://www.nbp.pl/kursy/xml/dir.txt");
                    else
                        responseString = await httpClient.GetStringAsync($"http://www.nbp.pl/kursy/xml/dir{year}.txt");
                } catch (Exception e)
                {
                    Debug.WriteLine(e.StackTrace);
                }
                reader = new StringReader(responseString);
                List<YearRates> ratesForYear = new List<YearRates>();
                while ((line = reader.ReadLine()) != null)
                {
                    if (line[0] == 'a')
                    {
                        String result = line.Substring(line.LastIndexOf('z') + 1);
                        string date = $"20{result.Substring(0, 2)}-{result.Substring(2, 2)}-{result.Substring(4, 2)}";
                        ratesForYear.Add(new YearRates(line, date));
                    }
                }
                // Reverse order to have the latest date first
                ratesForYear.Reverse();
                rates.AddRange(ratesForYear);
            }
            ViewModel.Dates = rates;
        }
        
        public async Task GetRates(string date)
        {
            List<Rate> ratesList = null;
            try
            {
                string responseString = await httpClient.GetStringAsync($"http://api.nbp.pl/api/exchangerates/tables/a/{date}/?format=json");
                responseString = responseString.Substring(1, responseString.Length - 2); // Delete enclosing [], in order to deserialize properly
                ExchangeRate responseConverted = JsonConvert.DeserializeObject<ExchangeRate>(responseString);
                ratesList = responseConverted.rates;
            }
            catch (JsonSerializationException ex)
            {
                Debug.WriteLine(ex.Data);
                Debug.WriteLine(ex.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
            ViewModel.Rates = ratesList;
        }
    }
}
