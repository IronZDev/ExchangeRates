using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ExchangeRates
{
    /// <summary>
    /// Zapewnia zachowanie specyficzne dla aplikacji, aby uzupełnić domyślną klasę aplikacji.
    /// </summary>
    sealed partial class App : Application
    {
        public DataViewModel ViewModel { get; set; }
        Windows.Storage.ApplicationDataContainer localSettings;
        Windows.Storage.StorageFolder localFolder;
        Windows.Storage.ApplicationDataCompositeValue composite;
        private Frame rootFrame;
        /// <summary>
        /// Inicjuje pojedynczy obiekt aplikacji. Jest to pierwszy wiersz napisanego kodu
        /// wykonywanego i jest logicznym odpowiednikiem metod main() lub WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.ViewModel = new DataViewModel();
            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            composite = (Windows.Storage.ApplicationDataCompositeValue)localSettings.Values["DataStoreViewModel"];
        }

        /// <summary>
        /// Wywoływane, gdy aplikacja jest uruchamiana normalnie przez użytkownika końcowego. Inne punkty wejścia
        /// będą używane, kiedy aplikacja zostanie uruchomiona w celu otworzenia określonego pliku.
        /// </summary>
        /// <param name="e">Szczegóły dotyczące żądania uruchomienia i procesu.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            rootFrame = Window.Current.Content as Frame;

            // Nie powtarzaj inicjowania aplikacji, gdy w oknie znajduje się już zawartość,
            // upewnij się tylko, że okno jest aktywne
            if (rootFrame == null)
            {
                // Utwórz ramkę, która będzie pełnić funkcję kontekstu nawigacji, i przejdź do pierwszej strony
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Załaduj stan z wstrzymanej wcześniej aplikacji
                }

                // Umieść ramkę w bieżącym oknie
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // Kiedy stos nawigacji nie jest przywrócony, przejdź do pierwszej strony,
                    // konfigurując nową stronę przez przekazanie wymaganych informacji jako
                    // parametr
                    composite = (Windows.Storage.ApplicationDataCompositeValue)localSettings.Values["DataStoreViewModel"];
                    if (composite["currentPage"] != null)
                        rootFrame.SetNavigationState((string)composite["currentPage"]);
                    else
                        rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Upewnij się, ze bieżące okno jest aktywne
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Wywoływane, gdy nawigacja do konkretnej strony nie powiedzie się
        /// </summary>
        /// <param name="sender">Ramka, do której nawigacja nie powiodła się</param>
        /// <param name="e">Szczegóły dotyczące niepowodzenia nawigacji</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Wywoływane, gdy wykonanie aplikacji jest wstrzymywane. Stan aplikacji jest zapisywany
        /// bez wiedzy o tym, czy aplikacja zostanie zakończona, czy wznowiona z niezmienioną zawartością
        /// pamięci.
        /// </summary>
        /// <param name="sender">Źródło żądania wstrzymania.</param>
        /// <param name="e">Szczegóły żądania wstrzymania.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // To save all data,  extra time is needed, that's why ExtendedExecutionSession is used. On deny almost no data is saved.
            using (var session = new ExtendedExecutionSession())
            {
                session.Reason = ExtendedExecutionReason.SavingData;
                session.Description = "Pretending to save data to slow storage.";
                ExtendedExecutionResult result = await session.RequestExtensionAsync();
                switch (result)
                {
                    case ExtendedExecutionResult.Allowed:
                        Debug.WriteLine("Saving state");
                        StorageFile datesFile = await localFolder.CreateFileAsync("dates.txt",
                            CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteTextAsync(datesFile, JsonConvert.SerializeObject(ViewModel.Dates));
                        StorageFile ratesFile = await localFolder.CreateFileAsync("rates.txt",
                            CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteTextAsync(ratesFile, JsonConvert.SerializeObject(ViewModel.Rates));
                        
                        composite["currentDate"] = ViewModel.CurrentDate;
                        composite["currentDateSelection"] = ViewModel.CurrentDateSelection;
                        if (rootFrame.SourcePageType.Name == "RateHistory")
                        {
                            composite["currentCurrencyCode"] = ViewModel.CurrentCurrencyCode;
                            composite["toRateHistoryDate"] = ViewModel.ToRateHistoryDate;
                            composite["fromRateHistoryDate"] = ViewModel.FromRateHistoryDate;
                            StorageFile historyFile = await localFolder.CreateFileAsync("history.txt",
                                CreationCollisionOption.ReplaceExisting);
                            await FileIO.WriteTextAsync(historyFile, JsonConvert.SerializeObject(ViewModel.HistoryOfCurrency));
                        } else {
                            composite["currentCurrencyCode"] = null;
                            composite["toRateHistoryDate"] = null;
                            composite["fromRateHistoryDate"] = null;
                        }
                        composite["currentPage"] = rootFrame.GetNavigationState();
                        localSettings.Values["DataStoreViewModel"] = composite;
                        break;
                    default:
                    case ExtendedExecutionResult.Denied:
                        Debug.WriteLine("Can't save the whole state");
                        if (rootFrame.SourcePageType.Name == "RateHistory")
                        {
                            composite["toRateHistoryDate"] = ViewModel.ToRateHistoryDate;
                            composite["fromRateHistoryDate"] = ViewModel.FromRateHistoryDate;
                            composite["currentCurrencyCode"] = ViewModel.CurrentCurrencyCode;
                        }
                        else
                        {
                            composite["currentCurrencyCode"] = null;
                            composite["toRateHistoryDate"] = null;
                            composite["fromRateHistoryDate"] = null;
                        }
                        composite["currentDate"] = null; ;
                        composite["currentDateSelection"] = null; ;
                        composite["currentPage"] = rootFrame.GetNavigationState();
                        localSettings.Values["DataStoreViewModel"] = composite;
                        break;
                }
            }
            deferral.Complete();
        }
    }
}
