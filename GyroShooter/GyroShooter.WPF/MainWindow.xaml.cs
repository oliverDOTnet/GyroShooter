using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GyroShooter.WPF
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int tick = 0;
        private bool gameRunning = false;
        private int destroyedAsteroids = 0;
        private int lifes;
        private int goal;

        private DispatcherTimer timer;
        private Random random;
        private List<Image> asteroidList;

        public MainWindow()
        {
            InitializeComponent();

            asteroidList = new List<Image>();
            random = new Random();
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(1) };
        }

        private void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {

        }
    }
}
