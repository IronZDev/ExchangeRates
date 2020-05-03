using ExchangeRates.DataModels;
using ExchangeRates.DataModels.RateHistoryModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace ExchangeRates
{
    public class DataViewModel : INotifyPropertyChanged
    {
        Windows.Storage.ApplicationDataContainer localSettings;
        Windows.Storage.StorageFolder localFolder;
        Windows.Storage.ApplicationDataCompositeValue composite;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private static DataViewModel myInstance;
        private List<YearRates> dates = new List<YearRates>();
        private List<Rate> rates = new List<Rate>();
        private List<DayRate> historyOfCurrency = new List<DayRate>();
        private string currentCurrencyCode;
        private string currentDate = "";
        private int currentDateSelection = 0;
        private DateTimeOffset? fromRateHistoryDate = null;
        private DateTimeOffset? toRateHistoryDate = null;
        public static DataViewModel getInstance()
        {
            return myInstance;
        }
        public DataViewModel()
        {
            myInstance = this;
            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            composite = (Windows.Storage.ApplicationDataCompositeValue)localSettings.Values["DataStoreViewModel"];
            LoadLocalSettings();
        }

        public List<YearRates> Dates
        {
            get { return this.dates; }
            set
            {
                Debug.WriteLine(value[0].date);
                this.dates = value;
                this.OnPropertyChanged();
            }
        }

        public int CurrentDateSelection
        {
            get { return currentDateSelection; }
            set
            {
                currentDateSelection = value;
            }
        }
        public string CurrentDate
        {
            get { return $"Chosen date: {currentDate}"; }
            set
            {
                currentDate = value;
                OnPropertyChanged();
            }
        }

        public string CurrentCurrencyCode
        {
            get { return currentCurrencyCode; }
            set
            {
                currentCurrencyCode = value;
                OnPropertyChanged();
            }
        }

        public List<Rate> Rates
        {
            get { return this.rates; }
            set
            {
                this.rates = value;
                this.OnPropertyChanged();
            }
        }

        public List<DayRate> HistoryOfCurrency
        {
            get { return this.historyOfCurrency; }
            set
            {
                this.historyOfCurrency = value;
                this.OnPropertyChanged();
            }
        }

        public DateTimeOffset? FromRateHistoryDate
        {
            get
            {
                return fromRateHistoryDate;
            }
            set
            {
                fromRateHistoryDate = value;
            }
        }

        public DateTimeOffset? ToRateHistoryDate
        {
            get
            {
                return toRateHistoryDate;
            }
            set
            {
                toRateHistoryDate = value;
            }
        }

        public async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Run on MainView thread (will not work if more than one view)
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() =>
                {
                    // Raise the PropertyChanged event, passing the name of the property whose value has changed.
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                });
        }



        public async void LoadLocalSettings()
        {
            if (composite == null)
            {
                composite = new Windows.Storage.ApplicationDataCompositeValue();
            }
            else
            {
                try
                {
                    currentCurrencyCode = (string)composite["currentCurrencyCode"];
                    currentDate = (string)composite["currentDate"];
                    currentDateSelection = (int)composite["currentDateSelection"];
                    toRateHistoryDate = (DateTimeOffset?)composite["toRateHistoryDate"];
                    fromRateHistoryDate = (DateTimeOffset?)composite["fromRateHistoryDate"];
                } catch(Exception e)
                {
                    Debug.WriteLine(e.StackTrace);
                }
            }
            try
            {
                StorageFile datesFile = await localFolder.GetFileAsync("dates.txt");
                String datesString = await FileIO.ReadTextAsync(datesFile);
                Debug.WriteLine(datesString);
                dates = JsonConvert.DeserializeObject<List<YearRates>>(datesString);
            }
            catch (Exception)
            {
                Debug.WriteLine("No dates saved");
            }
            try
            {
                StorageFile ratesFile = await localFolder.GetFileAsync("rates.txt");
                String ratesString = await FileIO.ReadTextAsync(ratesFile);
                rates = JsonConvert.DeserializeObject<List<Rate>>(ratesString);
            }
            catch (Exception)
            {
                Debug.WriteLine("No rates saved");
            }
            try
            {
                StorageFile historyFile = await localFolder.GetFileAsync("history.txt");
                String historyOfCurrencyString = await FileIO.ReadTextAsync(historyFile);
                historyOfCurrency = JsonConvert.DeserializeObject<List<DayRate>>(historyOfCurrencyString);
            }
            catch (Exception)
            {
                Debug.WriteLine("No history saved");
            }
        }
    }
}
