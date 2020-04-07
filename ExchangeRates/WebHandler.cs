using ExchangeRates.DataModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
            String currentDate = DateTime.Today.ToString("yyyy-MM-dd");
            String oneMonthAgoDate = DateTime.Today.AddMonths(-1).ToString("yyyy-MM-dd");
            List<string> datesList = null;
            try
            {
                string responseString = await httpClient.GetStringAsync($"http://api.nbp.pl/api/exchangerates/rates/a/eur/{oneMonthAgoDate}/{currentDate}/");
                ExchangeRate responseConverted = JsonConvert.DeserializeObject<ExchangeRate>(responseString);
                datesList = responseConverted.rates.ConvertAll(x => x.effectiveDate);
            } catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
            ViewModel.Dates = datesList;
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
