using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

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
                    if (ViewModel.HistoryOfCurrency != null)
                    {
                        loadChartData();
                    }
                }
            }
            lineChart.LegendItems.Clear();
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            On_BackRequested();
        }

        // Handles system-level BackRequested events and page-level back button Click events
        private bool On_BackRequested()
        {
            ViewModel.CurrentCurrencyCode = null;
            ViewModel.HistoryOfCurrency = null;
            (lineChart.Series[0] as LineSeries).ItemsSource = null;
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack(new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                return true;
            }
            this.Frame.Navigate(typeof(MainPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
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
            if (((ViewModel.ToRateHistoryDate != null && DateTimeOffset.Compare((DateTimeOffset)ViewModel.ToRateHistoryDate.Value.Date, args.NewDate.Value.Date) >= 0)
                || ViewModel.ToRateHistoryDate == null) && DateTimeOffset.Compare(new DateTimeOffset(DateTime.Today.Date),args.NewDate.Value.Date) >= 0)
            {
                ToDatePicker.IsEnabled = true;
                ToDatePicker.MinYear = args.NewDate.Value.Date;
                ViewModel.FromRateHistoryDate = args.NewDate.Value.Date;
                if (ViewModel.ToRateHistoryDate.HasValue == true)
                {
                    DownloadDataButton.IsEnabled = true;
                }
            }
            else if (DateTimeOffset.Compare(new DateTimeOffset(DateTime.Today.Date), args.NewDate.Value.Date) < 0)
            {
                var messageDialog = new MessageDialog("The specified dates can't be in the future");
                DownloadDataButton.IsEnabled = false;
                SaveButton.IsEnabled = false;
                ToDatePicker.IsEnabled = false;
                ViewModel.FromRateHistoryDate = null;
                await messageDialog.ShowAsync();
            }
            else
            {
                var messageDialog = new MessageDialog("The \"from\" date date must be earlier than the \"to\" date!", "Wrong dates selection!");
                DownloadDataButton.IsEnabled = false;
                SaveButton.IsEnabled = false;
                ViewModel.FromRateHistoryDate = null;
                await messageDialog.ShowAsync();
            }
            ViewModel.HistoryOfCurrency = null;
        }

        private async void ToDatePicker_SelectedDateChanged(DatePicker sender, DatePickerSelectedValueChangedEventArgs args)
        {
            if (ViewModel.FromRateHistoryDate != null && DateTimeOffset.Compare((DateTimeOffset)ViewModel.FromRateHistoryDate.Value.Date, args.NewDate.Value.Date) <= 0
                && DateTimeOffset.Compare(new DateTimeOffset(DateTime.Today.Date), args.NewDate.Value.Date) >= 0)
            {
                ViewModel.ToRateHistoryDate = args.NewDate.Value.Date;
                DownloadDataButton.IsEnabled = true;
            } else if (DateTimeOffset.Compare(new DateTimeOffset(DateTime.Today.Date), args.NewDate.Value.Date) < 0)
            {
                var messageDialog = new MessageDialog("The specified dates can't be in the future");
                SaveButton.IsEnabled = false;
                DownloadDataButton.IsEnabled = false;
                ViewModel.ToRateHistoryDate = null;
                await messageDialog.ShowAsync();
            } else
            {
                var messageDialog = new MessageDialog("The \"from\" date date must be earlier than the \"to\" date!", "Wrong dates selection!");
                DownloadDataButton.IsEnabled = false;
                ViewModel.ToRateHistoryDate = null;
                await messageDialog.ShowAsync();
            }
            ViewModel.HistoryOfCurrency = null;
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
            Task.Run(() => webHandler.GetRatesForCurrency(currentCurrencyCode, startDate, endDate)).ContinueWith(async antecedent => {
                    var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        if (ViewModel.HistoryOfCurrency == null)
                        {
                            var messageDialog = new MessageDialog("No data to download");
                            await messageDialog.ShowAsync();
                        }
                        else
                        {
                            loadChartData();
                        }
                    });
            });
            
        }

        private void loadChartData()
        {
            (lineChart.Series[0] as LineSeries).ItemsSource = ViewModel.HistoryOfCurrency;
            ((LineSeries)lineChart.Series[0]).IndependentAxis = new DateTimeAxis
            {
                Title = "Dates",
                ShowGridLines = true,
                Orientation = AxisOrientation.X
            };

            ((LineSeries)lineChart.Series[0]).DependentRangeAxis = new LinearAxis
            {
                Title = "Rate values",
                ShowGridLines = true,
                Orientation = AxisOrientation.Y
            };
            SaveButton.IsEnabled = true;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(lineChart);

            var pixelBuffer = await rtb.GetPixelsAsync();
            var pixels = pixelBuffer.ToArray();
            var displayInformation = DisplayInformation.GetForCurrentView();
            MessageDialog messageDialog;
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("PNG Bitmap", new List<string>() { ".png" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = $"Chart-{ViewModel.CurrentCurrencyCode}_{HelperClass.formatDateTimeOffset((DateTimeOffset)ViewModel.FromRateHistoryDate)}_{HelperClass.formatDateTimeOffset((DateTimeOffset)ViewModel.ToRateHistoryDate)}";
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                // write to file
                await Windows.Storage.FileIO.WriteTextAsync(file, file.Name);
                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                        encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                             BitmapAlphaMode.Premultiplied,
                                             (uint)rtb.PixelWidth,
                                             (uint)rtb.PixelHeight,
                                             displayInformation.RawDpiX,
                                             displayInformation.RawDpiY,
                                             pixels);
                        await encoder.FlushAsync();
                    }
                    messageDialog = new MessageDialog("File " + file.Name + " was saved.");
                }
                else
                {
                    messageDialog = new MessageDialog("File " + file.Name + " couldn't be saved.");
                }
            }
            else
            {
                return;
            }
            await messageDialog.ShowAsync();
        }
    }
}
