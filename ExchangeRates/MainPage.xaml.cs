using ExchangeRates.DataModels;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

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
            DownloadDates();
        }

        private void DownloadDates()
        {
            UpdateDatesButton.IsEnabled = false;
            DatesListViewLoading.IsActive = true;
            Task.Run(() => webHandler.GetDates()).ContinueWith(antecedent => {
                UpdateDatesButton.IsEnabled = true;
                DatesListViewLoading.IsActive = false;
                // Select last downloaded day and load info for it
                String currentDate = ViewModel.Dates[0].date;
                DownloadRateForDay(currentDate);
                // Reset rate and converter inputboxes
                ViewModel.CurrentRate = "";
                ViewModel.CurrentConverter = "";
                DatesListView.SelectedIndex = 0;
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

        private void UpdateDatesButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadDates();
        }

        private void DatesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0) // Ignore first selection when reloading list
            {
                DownloadRateForDay(((YearRates)e.AddedItems.Last()).date);
            }
        }

        private void RatesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0) // Ignore first selection when reloading list
            {
                Rate chosenRate = (Rate)e.AddedItems.Last();
                Debug.WriteLine(chosenRate.mid.ToString());
                ViewModel.CurrentRate = chosenRate.mid.ToString();
                ViewModel.CurrentConverter = chosenRate.converter.ToString();
            }
        }
        
        // Add blank placeholder if no flag image is available for currency
        private void FlagImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ((BitmapImage)sender).UriSource = new Uri("ms-appx:///Assets/DummyFlag.png");
        }
    }
}
