using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using GyroShooterClient;

namespace GyroShooter.WPF
{
    public enum StopMode
    {
        Paused,
        Winner,
        Looser,
        Exit
    }

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int tick = 0;
        private bool gameRunning = false;
        private int destroyedAsteroids = 0;
        private int lifes = 0;
        private int goal;

        private DispatcherTimer timer;
        private Random random;
        private List<Image> asteroidList;
        private List<Image> bulletList;

        private GyroClient gyroClient;

        public MainWindow()
        {
            InitializeComponent();

            lifes = 6;
            goal = 30;

            asteroidList = new List<Image>();
            bulletList = new List<Image>();
            random = new Random();
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(1) };

            this.Loaded += MainWindow_Loaded;

            GyroClient.Listen();
            GyroClient.ClientConnected += GyroClient_ClientConnected;

            timer.Tick += timer_Tick;
            this.MouseMove += OnMouseMove;
            this.MouseDown += OnMouseDown;

            //StartGame();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                Canvas.SetLeft(this.ship, this.ActualWidth / 2 - this.ship.ActualWidth / 2);
                Canvas.SetTop(this.ship, this.ActualHeight / 2 - this.ship.ActualHeight / 2);
            });
        }

        private async void GyroClient_ClientConnected(object sender, GyroClient e)
        {
            gyroClient = e;
            Debug.WriteLine("GyroClient successfully connected...");

            StartGame();

            this.Dispatcher.Invoke(async () =>
            {
                while (true)
                {
                    var res = await gyroClient.GetCommand();
                    Debug.WriteLine(res);

                    switch (res.Command)
                    {
                        case "x":
                            double px = Canvas.GetLeft(this.ship) - res.Value*25;
                            if(px >= 0 && px <= (this.ActualWidth - this.ship.ActualWidth))
                            {
                                Canvas.SetLeft(this.ship, px);
                            }
                            break;
                        case "y":
                            double py = Canvas.GetTop(this.ship) - res.Value*25;
                            if(py >= 0 && py <= (this.ActualHeight - this.ship.ActualHeight - 100))
                            {
                                Canvas.SetTop(this.ship, Canvas.GetTop(this.ship) - res.Value*25);
                            }
                            break;
                        case "shoot_left":
                            Shoot();
                            break;
                        case "shot_right":
                            Shoot();
                            break;
                    }
                }
            });
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            //System.Windows.Point position = e.GetPosition(this);
            //double pX = position.X;
            //double pY = position.Y;

            //Canvas.SetLeft(this.ship, e.GetPosition(this.gameCanvas).X - this.ship.ActualWidth / 2);
            //Canvas.SetTop(this.ship, e.GetPosition(this.gameCanvas).Y - this.ship.ActualHeight / 2);
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (gameRunning)
            {
                Shoot();
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (this.gameRunning)
            {
                tick++;
                if (tick == 100)
                {
                    ThrowAsteroid();
                    tick = 0;
                }

                foreach (var asteroid in asteroidList)
                {
                    Canvas.SetTop(asteroid, Canvas.GetTop(asteroid) + 2);

                    if (Canvas.GetTop(asteroid) >= this.ActualHeight)
                    {
                        lifes--;
                        DrawLifes();

                        this.gameCanvas.Children.Remove(asteroid);
                    }
                }

                asteroidList.RemoveAll(asteroid => Canvas.GetTop(asteroid) >= this.ActualHeight);

                foreach (var bullet in bulletList)
                {
                    Canvas.SetTop(bullet, Canvas.GetTop(bullet) - 6);
                    CheckCollision(bullet);

                    if (Canvas.GetTop(bullet) <= 0)
                    {
                        this.gameCanvas.Children.Remove(bullet);
                    }
                }

                bulletList.RemoveAll(bullet => Canvas.GetTop(bullet) <= 0);


                if (destroyedAsteroids == goal)
                {
                    StopGame(StopMode.Winner);
                }

                if (lifes <= 0)
                {
                    StopGame(StopMode.Looser);
                }
            }
        }

        #region GameEngine

        private void Explosion(double x, double y)
        {
            
        }

        private void CheckCollision(Image bullet)
        {
            Image deleteAsteroid = null;
            //Image deleteBullet = null;

            foreach (var asteroid in asteroidList)
            {
                if (Canvas.GetTop(bullet) >= (Canvas.GetTop(asteroid) - 10)
                    && Canvas.GetTop(bullet) <= (Canvas.GetTop(asteroid) + 10) 
                    && Canvas.GetLeft(bullet) >= Canvas.GetLeft(asteroid) 
                    && Canvas.GetLeft(bullet) <= (Canvas.GetLeft(asteroid) + 50))
                {
                    deleteAsteroid = asteroid;
                    //deleteBullet = bullet;

                    destroyedAsteroids++;
                    this.scoreCounter.Text = destroyedAsteroids.ToString() + " / " + goal.ToString();
                }
            }

            if (deleteAsteroid != null)// && deleteBullet != null)
            {
                Explosion(Canvas.GetLeft(deleteAsteroid), Canvas.GetTop(deleteAsteroid));

                asteroidList.Remove(deleteAsteroid);
                //bulletList.Remove(deleteBullet);
                this.gameCanvas.Children.Remove(deleteAsteroid);
                //this.gameCanvas.Children.Remove(deleteBullet);   
            }
        }

        private void StartGame()
        {
            this.gameRunning = true;
            this.timer.Start();
        }

        private void StopGame(StopMode mode)
        {
            this.gameRunning = false;
            this.timer.Stop();

            switch (mode)
            {
                case StopMode.Paused:
                    break;
                case StopMode.Winner:
                    break;
                case StopMode.Looser:
                    break;
                case StopMode.Exit:
                    break;
            }
        }

        private void ThrowAsteroid()
        {
            Image newAsteroid = new Image();
            newAsteroid.Source = new BitmapImage(new Uri("Assets/asteroid.png", UriKind.Relative));
            newAsteroid.Width = 50;
            newAsteroid.Height = 50;

            Canvas.SetLeft(newAsteroid, random.Next((int)this.ActualWidth - 25));
            Canvas.SetTop(newAsteroid, 0);
            //Canvas.SetZIndex(newAsteroid, 0);

            this.gameCanvas.Children.Add(newAsteroid);
            asteroidList.Add(newAsteroid);

            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = 0;
            doubleAnimation.To = 360;
            doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            doubleAnimation.AutoReverse = false;
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(random.Next(1, 3)));

            RotateTransform rotate = new RotateTransform();
            newAsteroid.RenderTransform = rotate;
            newAsteroid.RenderTransformOrigin = new Point(0.5, 0.5);

            rotate.BeginAnimation(RotateTransform.AngleProperty, doubleAnimation);
        }

        private void Shoot()
        {
            Image bullet = new Image();

            bullet.Source = new BitmapImage(new Uri("Assets/missile.png", UriKind.Relative));
            bullet.Width = 15;
            bullet.Height = 41;

            Canvas.SetLeft(bullet, Canvas.GetLeft(this.ship) + this.ship.ActualWidth / 2 - 15 / 2);
            Canvas.SetTop(bullet, Canvas.GetTop(this.ship) + this.ship.ActualHeight / 2 - 41 / 2);
            //Canvas.SetZIndex(newAsteroid, 0);

            this.gameCanvas.Children.Add(bullet);
            bulletList.Add(bullet);
        }

        private void DrawLifes()
        {
            switch (lifes)
            {
                case 0:
                    this.life1.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    this.life2.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    this.life3.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    this.life4.Visibility = Visibility.Collapsed;
                    break;
                case 4:
                    this.life5.Visibility = Visibility.Collapsed;
                    break;
                case 5:
                    this.life6.Visibility = Visibility.Collapsed;
                    break;
                case 6:
                    break;
            }
        }

        #endregion
    }
}
