using EmiMath;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Phone.Devices.Notification;
using Windows.Graphics.Display;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace GyroShooterClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ControlPage : Page
    {
      
        Accelerometer accelerometer;
        MovingAverageVector3 delayFilter;
        const int DelayWindow = 5;
        GyroClient client;
        VibrationDevice vib;

        public ControlPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
            delayFilter = new MovingAverageVector3(DelayWindow);
        }


        void accelerometer_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
        {
            Vector3 reading = new Vector3((float)args.Reading.AccelerationX, (float)args.Reading.AccelerationY, (float)args.Reading.AccelerationZ);

            delayFilter.Push(reading);

            if (!delayFilter.IsValid)
            {
                return;
            }

            var acc = delayFilter.Average;

            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                //Swap for landscape. 
                client.WriteCommand("x", acc.Y);
                client.WriteCommand("y", acc.X);
            });

        }


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.LandscapeFlipped | DisplayOrientations.Landscape;

            vib = VibrationDevice.GetDefault();
            client = e.Parameter as GyroClient;
            accelerometer = Accelerometer.GetDefault();
            accelerometer.ReadingChanged += accelerometer_ReadingChanged;

            while (true)
            {
                GyroCommand command = await client.GetCommand();
                if (command.Command == "hit")
                {
                    vib.Vibrate(TimeSpan.FromSeconds(command.Value));
                }
            }
        }

        private void ButtonLeft_Click(object sender, RoutedEventArgs e)
        {
            client.WriteCommand("shoot_left", 0);
        }

        private void ButtonRight_Click(object sender, RoutedEventArgs e)
        {
            client.WriteCommand("shoot_right", 0);
        }
    }
}
