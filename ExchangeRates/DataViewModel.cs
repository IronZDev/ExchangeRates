using ExchangeRates.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace ExchangeRates
{
    public class DataViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private static DataViewModel myInstance;
        private List<string> dates = new List<string>();
        private List<Rate> rates = new List<Rate>();
        private string currentRate, currentConverter;
        private string currentDate = "";
        public static DataViewModel getInstance()
        {
            return myInstance;
        }
        public DataViewModel()
        {
            myInstance = this;
        }

        public List<string> Dates
        {
            get { return this.dates; }
            set
            {
                this.dates = value;
                this.OnPropertyChanged();
            }
        }

        public string CurrentDate
        {
            get { return $"Publication date: {currentDate}"; }
            set
            {
                currentDate = value;
                OnPropertyChanged();
            }
        }

        public string CurrentRate
        {
            get { return currentRate; }
            set
            {
                currentRate = value;
                OnPropertyChanged();
            }
        }

        public string CurrentConverter
        {
            get { return currentConverter; }
            set
            {
                currentConverter = value;
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

        public async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Run on MainView thread (will not work if more than one view)
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() =>
                {
                    // Raise the PropertyChanged event, passing the name of the property whose value has changed.
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                });
        }
    }
}
