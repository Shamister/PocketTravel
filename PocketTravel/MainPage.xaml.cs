using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PocketTravel
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DataManager manager;

        public MainPage()
        {
            this.InitializeComponent();

            init();
        }

        /**
         * Initialize the data manager and run a task to construct the airport data
         */
        private async void init()
        {
            manager = new DataManager();
            await Task.Run (() => constructAirportInfo());
        }

        /**
         * Retrieve the airport data from the server and construct the Patricia trees for the data
         * This function will run asynchronously and update the state of the program on the textBlock
         */
        private async void constructAirportInfo()
        {
            manager.currentState = DataManager.State.LOADING_AIRPORT;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                executeTransition();
            });
            Task task = manager.getAirports();
            task.Wait();
            manager.currentState = DataManager.State.CONTSTRUCTING_DATA;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                executeTransition();
            });
            manager.constructAirportTrees();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                executeTransition();
            });
        }

        /**
         * Get the string state of the program
         */
        private void executeTransition()
        {
            if (manager.currentState == DataManager.State.LOADING_AIRPORT)
            {
                loadingState.Text = "Retriving data from server...";
            }
            else if (manager.currentState == DataManager.State.CONTSTRUCTING_DATA)
            {
                loadingState.Text = "Loading data...";
            }
            else if (manager.currentState == DataManager.State.READY)
            {
                loadingState.Text = "Ready.";
                state.Text = "Ready.";
                moveOutLoadingPanel();
            }
            else if (manager.currentState == DataManager.State.LOADING_DEST)
            {
                state.Text = "Loading Destination...";
            }
            else if (manager.currentState == DataManager.State.ERROR)
            {
                state.Text =  "Error.";
            }
        }

        private void moveOutLoadingPanel()
        {
            double width = this.ActualWidth;

            Duration duration = new Duration(TimeSpan.FromSeconds(0.3));

            DoubleAnimation da = new DoubleAnimation();
            da.To = -10*width;

            Storyboard sb = new Storyboard();

            sb.Duration = duration;
            sb.Children.Add(da);

            Storyboard.SetTarget(da, movingOutTrans);

            Storyboard.SetTargetProperty(da, "(movingOutTrans.X)");

            sb.Begin();
        }

        private void OriginTextBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (sender.Text.Length > 0 && manager.currentState == DataManager.State.READY)
            {
                sender.ItemsSource = manager.getAutoCompleteAirport(sender.Text.ToLower());
            }
            else
            {
                sender.ItemsSource = null;
            }
        }

        private void DestinationTextBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (sender.Text.Length > 0 && manager.currentState == DataManager.State.READY)
            {
                sender.ItemsSource = manager.getAutoCompleteAirport(sender.Text.ToLower());
            }
            else
            {
                sender.ItemsSource = null;
            }
        }

        private void button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (OriginTextBox.Text != "" && DestinationTextBox.Text != "")
            {
                checkDestination(OriginTextBox.Text, DestinationTextBox.Text);
            }
        }

        /**
         * Check the information of the destination
         * This function will create a task that will retrieve the destination data asynchronously
         */
        private async void checkDestination(string origin, string dest)
        {
            if (manager.currentState == DataManager.State.READY)
            {
                await Task.Run(() => getDestInfo(origin, dest));
            }
        }

        /**
         * Get the destination data from the server
         * This function will run asynchronously and update the state of the program on the textBlock
         */
        private async void getDestInfo(string origin, string dest)
        {
            manager.currentState = DataManager.State.LOADING_DEST;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                executeTransition();
            });
            Task<TravelInfo> task = manager.getDestInfo(origin, dest);
            task.Wait();
            TravelInfo ti = task.Result;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                manager.currentState = DataManager.State.READY;
                executeTransition();

                city.Text = ti.city;
                country.Text = ti.country;
                coordinate.Text = ti.coordinate;
                timezone.Text = ti.timezone;
                originTime.Text = (ti.originTime.Equals(new DateTime())) ? "not available" : ti.originTime.ToString("MMMM  dd, yyyy hh:mm");
                destTime.Text = (ti.destTime.Equals(new DateTime())) ? "not available" : ti.destTime.ToString("MMMM  dd, yyyy hh:mm");
                weather.Text = ti.weather;
                temp.Text = (ti.temp.Equals(double.NaN)) ? "not available" : ti.temp.ToString()+char.ConvertFromUtf32(176)+"C";
            });
        }
    }
}
