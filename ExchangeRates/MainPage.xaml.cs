using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//Szablon elementu Pusta strona jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x415

namespace ExchangeRates
{
    /// <summary>
    /// Pusta strona, która może być używana samodzielnie lub do której można nawigować wewnątrz ramki.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        WebHandler webHandler;
        public DataViewModel ViewModel { get; set; }
        public MainPage()
        {
            this.InitializeComponent();
            ViewModel = DataViewModel.getInstance();
            webHandler = new WebHandler();
            LoadInitialViews();
        }

        private void LoadInitialViews() {
            DownloadDates();
            String currentDate = DateTime.Today.ToString("yyyy-MM-dd");
            DownloadRateForDay(currentDate);
        }

        private void DownloadDates()
        {
            DownloadDatesButton.IsEnabled = false;
            DatesListViewLoading.IsActive = true;
            Task.Run(() => webHandler.GetDates()).ContinueWith(antecedent => {
                DownloadDatesButton.IsEnabled = true;
                DatesListViewLoading.IsActive = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void DownloadRateForDay(string date)
        {
            RatesListViewLoading.IsActive = true;
            ViewModel.CurrentDate = date;
            Task.Run(() => webHandler.GetRates(date)).ContinueWith(antecedent => {
                RatesListViewLoading.IsActive = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void DownloadDatesButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadDates();
        }

        private void DatesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DownloadRateForDay(e.AddedItems.Last().ToString());
        }

        private void RatesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
