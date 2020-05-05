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
using Windows.UI.Xaml.Media.Animation;
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
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            ViewModel = DataViewModel.getInstance();
            webHandler = new WebHandler();
            if (ViewModel.Dates == null)
            {
                DownloadDates();
            } else
            {
                if (DatesListView != null)
                    DatesListView.SelectedIndex = ViewModel.CurrentDateSelection;
            }
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
                // Reset current currency code
                ViewModel.CurrentCurrencyCode = null;
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
                ViewModel.CurrentDateSelection = DatesListView.SelectedIndex;
            }
        }

        private void RatesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine("Clicked");
            Rate chosenRate = (Rate)e.ClickedItem;
            ViewModel.CurrentCurrencyCode = chosenRate.code;
            this.Frame.Navigate(typeof(RateHistory), chosenRate.code, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
        
        // Add blank placeholder if no flag image is available for currency
        private void FlagImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ((BitmapImage)sender).UriSource = new Uri("ms-appx:///Assets/DummyFlag.png");
        }
    }
}
