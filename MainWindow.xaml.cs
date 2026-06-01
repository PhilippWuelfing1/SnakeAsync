using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
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

namespace SnakeAsync
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        Rectangle? _food;
        private string? _snakeHead_x;
        private string? _snakeHead_y;
        private double? _food_x;
        private double? _food_y;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            var head = snake.Points.Last(); 
            SnakeHead_x = head.X.ToString(); 
            SnakeHead_y = head.Y.ToString();
            canv.Focus();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string? Food_x
        {
            get
            {
                if(_food == null)
                {
                    return "0";
                }
                else if (_food.Name != "food")
                {
                    return "0";
                }
                return Canvas.GetLeft(_food).ToString();
            }
            set
            {
                _food_x = Canvas.GetLeft(_food);
                OnPropertyChanged(nameof(Food_x));
            }
        }

        public string? Food_y
        {
            get
            {
                if(_food == null)
                {
                    return "0";
                }
                else if (_food.Name != "food")
                {
                    return "0";
                }
                return Canvas.GetTop(_food).ToString();
            }
            set
            {
                _food_y = Canvas.GetTop(_food);
                OnPropertyChanged(nameof(Food_y));
            }
        }



        public string? SnakeHead_x
        {
            get
            {
                return snake == null ? "0" : snake.Points.LastOrDefault().X.ToString(); 
            }
            set
            {
                _snakeHead_x = snake.Points.LastOrDefault().X.ToString(); 
                OnPropertyChanged(nameof(SnakeHead_x));
            }
        }

        public string? SnakeHead_y
        {
            get
            {
                return snake == null ? "0" : snake.Points.LastOrDefault().Y.ToString();
            }
            set
            {
                _snakeHead_y = snake.Points.LastOrDefault().Y.ToString();
                OnPropertyChanged(nameof(SnakeHead_y));
            }
        }
      
        private async void canv_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D)
            {
                ResetToken();
                _ = move(new Point(10, 0), canv, snake, _cts.Token);
                e.Handled = true;
            }
            else if (e.Key == Key.S)
            {
                ResetToken();
                _ = move(new Point(0, 10), canv, snake, _cts.Token);
                e.Handled = true;
            }

            else if (e.Key == Key.A)
            {
                ResetToken();
                _ = move(new Point(-10, 0), canv, snake, _cts.Token);
                e.Handled = true;
            }
            else if (e.Key == Key.W)
            {
                ResetToken();
                _ = move(new Point(0, -10), canv, snake, _cts.Token);
                e.Handled = true;
            }
            else if (e.Key == Key.Space)
            {
                ResetToken();
                e.Handled = true;
            }
        }

        private void pause()
        {
            ResetToken();
        }

        private async Task move(Point direction, Canvas canv, Polyline snake, CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                bool shouldContinueV = false;
                bool shouldContinueH = false;

                await Dispatcher.InvokeAsync(() =>
                {
                    var lastHead = snake.Points.LastOrDefault();
                    Point newHead = new Point(lastHead.X + direction.X, lastHead.Y + direction.Y);

                    shouldContinueV = lastHead.Y < canv.ActualHeight - 100 && lastHead.Y >= 100;
                    shouldContinueH = lastHead.X < canv.ActualWidth && lastHead.X >= 100;

                    if (shouldContinueH && shouldContinueV)
                    {
                        snake.Points.Add(new Point(lastHead.X + direction.X, lastHead.Y + direction.Y));

                        //Letztes Element entfernen, damit die Länge konstant bleibt
                        snake.Points.RemoveAt(0);

                        OnPropertyChanged(nameof(SnakeHead_x));
                        OnPropertyChanged(nameof(SnakeHead_y));

/*                        if(_food_x < newHead.X + 5 && _food_y > newHead.X - 5)
                        {
                            if (_food_y < newHead.Y + 30 && _food_y > newHead.Y - 30)
                            {
                                canv.Children.Remove(_food);
                                spawnFood();
                            }
                        }*/

                        if(_food_x < newHead.X + 5 && _food_y > newHead.X - 5 && _food_y < newHead.Y + 30 && _food_y > newHead.Y - 30)
                        {
                            canv.Children.Remove(_food);
                            spawnFood();
                        }
                    }
                    else
                    {
                        if (!shouldContinueH)
                        {
                            //Horizontal am anderen Rand wieder auftauchen
                            spawnOnOtherSideHor(direction.X);
                        }
                        if (!shouldContinueV)
                        {
                            //Vertikal am anderen Rand wieder auftauchen
                            spawnOnOtherSideVer(direction.Y);
                        }
                    }

                }, System.Windows.Threading.DispatcherPriority.Render);

                await Task.Delay(100, token);
            }
        }

        private void spawnOnOtherSideHor(double direction)
        {
            var y = snake.Points.LastOrDefault().Y;
            var length = snake.Points.Count;
            snake.Points.Clear();
            double newHead_x = 0;

            if (direction > 0)
            {
                for (int i = 0; i < length - 1; i++)
                {
                    snake.Points.Add(new Point(newHead_x, y));
                    newHead_x += 10;
                }
            }

            else
            {
                newHead_x = canv.ActualWidth;

                for (int i = 0; i < length - 1; i++)
                {
                    snake.Points.Add(new Point(newHead_x, y));
                    newHead_x -= 10;
                    
                }
            }

        }

        private void spawnOnOtherSideVer(double direction)
        {
            var x = snake.Points.LastOrDefault().X;
            var length = snake.Points.Count;
            snake.Points.Clear();
            double newHead_y = 0;

            if (direction > 0)
            {
                for (int i = 0; i < length - 1; i++)
                {
                    snake.Points.Add(new Point(x, newHead_y));
                    newHead_y += 10;
                }
            }

            else
            {
                newHead_y = canv.ActualHeight;

                for (int i = 0; i < length - 1; i++)
                {
                    snake.Points.Add(new Point(x, newHead_y));
                    newHead_y -= 10;
                }
            }

        }

        private void spawnFood()
        {
            _food = new Rectangle();
            _food.Name = "food";
            _food.Height = 30;
            _food.Width = 30;
            _food.Fill = Brushes.Red;
            _food.StrokeThickness = 10;

            Random rnd = new Random();
            var leftAndTop = rnd.Next(1, Convert.ToInt32(SnakeWindow.ActualWidth - 200));
            var topAndLeft = rnd.Next(1, Convert.ToInt32(SnakeWindow.ActualHeight - 200));
            //var topAndLeft = 600;
            //var leftAndTop = 600;

            Canvas.SetTop(_food, topAndLeft);
            Canvas.SetLeft(_food, leftAndTop);

            Food_x = leftAndTop.ToString();
            Food_y = topAndLeft.ToString();
            OnPropertyChanged(nameof(Food_x));
            OnPropertyChanged(nameof(Food_y));

            canv.Children.Add(_food);
        }

        private void ResetToken()
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                canv.Focus();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            canv.Focus();
            spawnFood();
        }
    }

}