using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Threading;
using System.Windows.Navigation;

namespace StopWatchPApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        // A timer, so we can update the display every 100 milliseconds
        DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(.1) };

        PersistSettings<DateTime> previousTick = new PersistSettings<DateTime> ("PreviousTick", DateTime.MinValue);
        
        PersistSettings <TimeSpan> totalTime = new PersistSettings<TimeSpan> ("TotalTime", TimeSpan.Zero);
        PersistSettings<int> currentLap = new PersistSettings<int>("CurrentLap", 0);

        PersistSettings<bool> isTimerRunning = new PersistSettings<bool> ("IsTimerRunning", false);
        PersistSettings<bool> resetWarning = new PersistSettings<bool>("ResetWarning", true);
        PersistSettings<bool> firstGone = new PersistSettings<bool>("firstGone", false);
        PersistSettings<TimeSpan> currentLapTime = new PersistSettings<TimeSpan>("CurrentLapTime", TimeSpan.Zero);
        PersistSettings<List<TimeSpan>> currentLapList = new PersistSettings<List<TimeSpan>>("CurrentLapList", new List<TimeSpan>()); 

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            this.timer.Tick += Timer_Tick;
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Update the time displays with the data from last time
            ShowCurrentTime();

            // Load current laps completed
            if (this.currentLapList.Value != null)
                foreach (TimeSpan ts in this.currentLapList.Value)
                    InsertLapTime(ts);

             // If we previously left the page with a non-zero total time, then the reset
            // button was enabled. Enable it again:
            if (this.totalTime.Value > TimeSpan.Zero)
            {
                this.StopButton.Visibility = System.Windows.Visibility.Visible;
                this.RunButton.Visibility = System.Windows.Visibility.Collapsed;
            }

            // If the page was left while running, automatically start running again
            if (this.isTimerRunning.Value)
                StartTimer();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset previousTick so the calculations start from the current time
            this.previousTick.Value = DateTime.UtcNow;
            this.StopButton.Visibility = System.Windows.Visibility.Visible;
            this.RunButton.Visibility = System.Windows.Visibility.Collapsed;
            StartTimer();
        }

        void StartTimer()
        {
            // Start the timer
            this.timer.Start();

            // Remember that the timer was running if the page is left before stopping
            this.isTimerRunning.Value = true;
            if ((TimeSpan)this.StartOffset.Value == TimeSpan.Zero)
            {
                this.firstGone.Value = false;
            }
            else
            {
                this.firstGone.Value = true;
                this.currentLapTime = StartOffset.Value;
            }
        }

        void ResetContents()
        {
            // Reset all data
            this.totalTime.Value = TimeSpan.Zero;
            this.LapCount.Text = "0";
            this.currentLapTime.Value = TimeSpan.Zero;
            this.currentLapList.Value.Clear();


            ShowCurrentTime();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBoxResult.Cancel;
            if (this.resetWarning.Value == true)
            {
                result = MessageBox.Show("Resetting the timer would clear all laps and elapsed time. Are you sure you want to reset the time?",
                "Warning", MessageBoxButton.OKCancel);                
            }

            if (result == MessageBoxResult.OK)
                ResetContents();
        }

        private void AddLapButton_Click(object sender, RoutedEventArgs e)
        {
            this.LapCount.Text = (Convert.ToInt32(this.LapCount.Text) + 1).ToString();
            InsertLapTime(this.totalTime.Value);
            currentLapList.Value.Add(this.totalTime.Value);
            this.currentLapTime.Value = TimeSpan.Zero;
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            // Determine how much time has passed since the last tick.
            TimeSpan delta = DateTime.UtcNow - this.previousTick.Value;

            // Remember the current time 
            this.previousTick.Value += delta;

            // Update the total time
            this.totalTime.Value += delta;
            // save the current lap time
            currentLapTime.Value += delta;

            // Refresh the UI
            ShowCurrentTime();
        }

        void ShowCurrentTime()
        {
            // Update the two numeric displays
            this.TotalTimeDisplay.Time = this.totalTime.Value;
            CurrentLapTimeText.Time = currentLapTime.Value;
            this.LapCount.Text = this.currentLap.Value.ToString ();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            this.timer.Stop();
            this.isTimerRunning.Value = false;
            this.StopButton.Visibility = System.Windows.Visibility.Collapsed;
            this.RunButton.Visibility = System.Windows.Visibility.Visible;
        }

        private void UserIdleModeOnOff_Checked(object sender, RoutedEventArgs e)
        {
            this.UserIdleModeOnOff.Content = "Yes";
            try
            {
                Microsoft.Phone.Shell.PhoneApplicationService.Current.ApplicationIdleDetectionMode =
                    Microsoft.Phone.Shell.IdleDetectionMode.Disabled;
            }
            catch (InvalidOperationException ex)
            {
                // This exception is expected in the current release.
            }
        }

        private void UserIdleModeOnOff_Unchecked(object sender, RoutedEventArgs e)
        {
            this.UserIdleModeOnOff.Content = "No";
            try
            {
                Microsoft.Phone.Shell.PhoneApplicationService.Current.ApplicationIdleDetectionMode =
                    Microsoft.Phone.Shell.IdleDetectionMode.Enabled;
            }
            catch (InvalidOperationException ex)
            {
                // This exception is expected in the current release.
            }
        }

        private void ResetWarning_Checked(object sender, RoutedEventArgs e)
        {
            this.ResetWarning.Content = "Yes";
            resetWarning.Value = true;
        }

        private void ResetWarning_Unchecked(object sender, RoutedEventArgs e)
        {
            this.ResetWarning.Content = "No";
            resetWarning.Value = false;
        }

 //       private void About_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
 //       {
 //           Microsoft.Phone.Tasks.EmailComposeTask t = new Microsoft.Phone.Tasks.EmailComposeTask();
 //           t.Subject = "Feedback on Stop Watch Windows Phone application " + this.VersionTextBlock.Text.Substring("version ".Length);
 //           t.To = "vpstopwatch@hotmail.com";
 //           t.Show();
 //       }

        void InsertLapTime(TimeSpan actualTime)
        {
            int lapNumber = CurrentLapsList.Children.Count + 1;
            TimeSpan ri;
            TimeSpan st;

            TimeSpan releaseTime;
            TimeSpan elapsedTime;

            ri = (TimeSpan)ReleaseInterval.Value;
            st = (TimeSpan)StartOffset.Value;

            releaseTime = st.Add(TimeSpan.FromSeconds((lapNumber - 1) * ri.TotalSeconds));
            elapsedTime = actualTime - releaseTime;
                          
            // Dynamically create a new grid to represent the new lap entry in the list
            Grid grid = new Grid();

            // The grid has "lap N" docked on the left, where N is 1, 2, 3, ...
            grid.Children.Add(new TextBlock { Text = lapNumber.ToString(), Margin = new Thickness(24, 0, 0, 0) });

            // The grid has a TimeSpanDisplay instance docked on the right that
            // shows the length of the lap
            TimeSpanControl display = new TimeSpanControl { Time = elapsedTime,
                                        DigitWidth = 18, HorizontalAlignment = HorizontalAlignment.Right,
                                        Margin = new Thickness(0, 0, 24, 0) };
            grid.Children.Add(display);

            // Insert the new grid at the beginning of the StackPanel
            CurrentLapsList.Children.Insert(0, grid);           
        }
    }
}