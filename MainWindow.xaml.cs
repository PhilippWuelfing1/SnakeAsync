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
        Rectangle _food = new Rectangle();
        private string? _snakeHead_x;
        private string? _snakeHead_y;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            var head = snake.Points.Last(); 
            SnakeHead_x = head.X.ToString(); 
            SnakeHead_y = head.Y.ToString();
            //spawnFood();
            canv.Focus();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                    var lastPoint = snake.Points.LastOrDefault();

                    shouldContinueV = lastPoint.Y < canv.ActualHeight - 100 && lastPoint.Y > 100;
                    shouldContinueH = lastPoint.X < canv.ActualWidth - 100 && lastPoint.X > 100;

                    if (true)
                    {
                        snake.Points.Add(new Point(lastPoint.X + direction.X, lastPoint.Y + direction.Y));
                        //Letztes Element entfernen, damit die Länge konstant bleibt
                        snake.Points.RemoveAt(0);

                        OnPropertyChanged(nameof(SnakeHead_x));
                        OnPropertyChanged(nameof(SnakeHead_y));

                    }
                    else
                    {

                    }

                    if(snake.Points.LastOrDefault().X == Canvas.GetLeft(_food) && snake.Points.LastOrDefault().Y == Canvas.GetTop(_food))
                    {
                        // Food eaten
                        canv.Children.Remove(_food);
                        spawnFood();
                    }

                }, System.Windows.Threading.DispatcherPriority.Render);

                //if (!shouldContinue) break;

                await Task.Delay(30, token);
            }
        }

        private void spawnFood()
        {
            _food = new Rectangle();
            _food.Height = 20;
            _food.Width = 20;
            _food.Fill = Brushes.Red;
            _food.StrokeThickness = 10;

            Random rnd = new Random();
            int topAndLeft = rnd.Next(1, 1300);  

            Canvas.SetTop(_food, topAndLeft);
            Canvas.SetLeft(_food, topAndLeft);

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
        }
    }

}