using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//Szablon elementu Pusta strona jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=234238

namespace ExchangeRates
{
    /// <summary>
    /// Pusta strona, która może być używana samodzielnie lub do której można nawigować wewnątrz ramki.
    /// </summary>
    public sealed partial class RateHistory : Page
    {
        WebHandler webHandler;
        public DataViewModel ViewModel { get; set; }
        private bool _isPageSwiped;
        public RateHistory()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            ViewModel = DataViewModel.getInstance();
            webHandler = new WebHandler();
            FromDatePicker.MinYear = new DateTime(2002, 01, 01);
            FromDatePicker.MaxYear = DateTime.Today;
            ToDatePicker.MaxYear = DateTime.Today;
            if (ViewModel.FromRateHistoryDate != null)
            {
                FromDatePicker.SelectedDate = ViewModel.FromRateHistoryDate;
                ToDatePicker.IsEnabled = true;
                if (ViewModel.ToRateHistoryDate != null)
                {
                    ToDatePicker.SelectedDate = ViewModel.ToRateHistoryDate;
                    DownloadDataButton.IsEnabled = true;
                }
            }
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            On_BackRequested();
        }

        // Handles system-level BackRequested events and page-level back button Click events
        private bool On_BackRequested()
        {
            ViewModel.CurrentCurrencyCode = null;
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
                return true;
            }
            this.Frame.Navigate(typeof(MainPage));
            return false;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string && !string.IsNullOrWhiteSpace((string)e.Parameter))
            {
                TitleTextBlock.Text = $"History of {e.Parameter.ToString().ToUpper()} currency:";
            }
            else
            {
                TitleTextBlock.Text = "History of currency:";
            }
            base.OnNavigatedTo(e);
        }

        private async void FromDatePicker_SelectedDateChanged(DatePicker sender, DatePickerSelectedValueChangedEventArgs args)
        {
            if ((ViewModel.ToRateHistoryDate != null && DateTimeOffset.Compare((DateTimeOffset)ViewModel.ToRateHistoryDate.Value.Date, args.NewDate.Value.Date) > 0)
                || ViewModel.ToRateHistoryDate == null)
            {
                ToDatePicker.IsEnabled = true;
                ToDatePicker.MinYear = args.NewDate.Value.Date;
                ViewModel.FromRateHistoryDate = args.NewDate.Value.Date;
            } else
            {
                var messageDialog = new MessageDialog("The \"from\" date date must be earlier than the \"to\" date!", "Wrong dates selection!");
                DownloadDataButton.IsEnabled = false;
                await messageDialog.ShowAsync();
            }
        }

        private async void ToDatePicker_SelectedDateChanged(DatePicker sender, DatePickerSelectedValueChangedEventArgs args)
        {
            if (ViewModel.FromRateHistoryDate != null && DateTimeOffset.Compare((DateTimeOffset)ViewModel.FromRateHistoryDate.Value.Date, args.NewDate.Value.Date) < 0)
            {
                ViewModel.ToRateHistoryDate = args.NewDate.Value.Date;
                DownloadDataButton.IsEnabled = true;
            } else
            {
                var messageDialog = new MessageDialog("The \"from\" date date must be earlier than the \"to\" date!", "Wrong dates selection!");
                DownloadDataButton.IsEnabled = false;
                await messageDialog.ShowAsync();
            }
        }
        private void PageSwipeHandling_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.IsInertial && !_isPageSwiped)
            {
                var swipedDistance = e.Cumulative.Translation.X;

                if (Math.Abs(swipedDistance) <= 2) return;

                if (swipedDistance > 0)
                {
                    On_BackRequested();
                }
                _isPageSwiped = true;
            }
        }
        private void PageSwipeHandling_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _isPageSwiped = false;
        }

        private void DownloadDataButton_Click(object sender, RoutedEventArgs e)
        {
            // It's necessary to bring the params from the lambda, becuase lambda can't access UI elements
            string currentCurrencyCode = ViewModel.CurrentCurrencyCode;
            DateTimeOffset startDate = FromDatePicker.Date.Date;
            DateTimeOffset endDate = ToDatePicker.Date.Date;
            Task.Run(() => webHandler.GetRatesForCurrency(currentCurrencyCode, startDate, endDate));
        }
    }
}
