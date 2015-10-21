using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace StopWatchPApp
{
	public partial class TimeSpanControl : UserControl
	{
        int digitWidth;
        TimeSpan time;

        public TimeSpanControl()
		{
			// Required to initialize variables
			InitializeComponent();

            // In design mode, show something other than an empty text box
            if (DesignerProperties.IsInDesignTool)
                this.LayoutRoot.Children.Add(new TextBlock { Text = "00:00:00.0" });
        }

        public int DigitWidth
        {
            get { return this.digitWidth; }
            set
            {
                this.digitWidth = value;
                this.Time = this.time;
            }
        }

        public TimeSpan Time
        {
            get { return this.time; }
            set
            {
                this.LayoutRoot.Children.Clear();

                // Hours
                string hoursString = value.Hours.ToString();
                ConcatenateTime((value.Hours / 10).ToString());
                ConcatenateTime((value.Hours % 10).ToString());

                this.LayoutRoot.Children.Add(new TextBlock { Text = ":" });

                // Support two digits of minutes digits
                string minutesString = value.Minutes.ToString();
                ConcatenateTime((value.Minutes / 10).ToString());
                ConcatenateTime((value.Minutes % 10).ToString());

                this.LayoutRoot.Children.Add(new TextBlock { Text = ":" });

                // Two digits for Seconds
                ConcatenateTime((value.Seconds / 10).ToString());
                ConcatenateTime((value.Seconds % 10).ToString());

                // Add the decimal separator (a period for en-US)
                this.LayoutRoot.Children.Add(new TextBlock
                {
                    Text = System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator
                });

                // msecs display
                ConcatenateTime((value.Milliseconds / 100).ToString());

                this.time = value;
            }
        }

        void ConcatenateTime(string timeValue)
        {
            TextBlock textBlock = new TextBlock
            {
                Text = timeValue,
                Width = this.DigitWidth,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            this.LayoutRoot.Children.Add(textBlock);
        }
	}
}