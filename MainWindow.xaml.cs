using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
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
        private bool _pause = false;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            setSnakeHead();
            startMove();
            canv.Focus();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void setSnakeHead()
        {
            var head = snake.Points.Last();
            SnakeHead_x = head.X.ToString();
            SnakeHead_y = head.Y.ToString();
        }

        private void startMove()
        {
            Direction = new Point(PointWidth, 0);
            _ = move(Direction, canv, snake, _cts.Token);
        }

        private double _pointWidth = 20;
        public double PointWidth
        {
            get { return _pointWidth; }
            set
            {
                _pointWidth = value;
                OnPropertyChanged(nameof(PointWidth));
            }
        }


        private int _speed = 30;
        public int Speed
        {
            get { return _speed; }
            set
            {
                _speed = value;
                OnPropertyChanged(nameof(Speed));
            }
        }


        private int _snakeLaenge = 11;
        public int SnakeLaenge
        {
            get => _snakeLaenge;
            set
            {
                _snakeLaenge = value;
                OnPropertyChanged(nameof(SnakeLaenge));
            }
        }

        private List<Rectangle> _foodCollection = new List<Rectangle>();
        public List<Rectangle> FoodCollection
        {
            get => _foodCollection;
            set
            {
                _foodCollection = value;
                OnPropertyChanged(nameof(FoodCollection));
            }
        }

        private double? _food_x;
        public double? Food_x
        {
            get
            {
                if(_food == null)
                {
                    return 0;
                }
                else if (_food.Name != "food")
                {
                    return 0;
                }
                return Canvas.GetLeft(_food);
            }
            set
            {
                _food_x = Canvas.GetLeft(_food);
                OnPropertyChanged(nameof(Food_x));
            }
        }


        private double? _food_y;
        public double? Food_y
        {
            get
            {
                if(_food == null)
                {
                    return 0;
                }
                else if (_food.Name != "food")
                {
                    return 0;
                }
                return Canvas.GetTop(_food);
            }
            set
            {
                _food_y = Canvas.GetTop(_food);
                OnPropertyChanged(nameof(Food_y));
            }
        }


        private string? _snakeHead_x;
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

        private string? _snakeHead_y;
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

        private Point _direction;
        public Point Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                OnPropertyChanged(nameof(Direction));
            }
        }
      
        private async void canv_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D)
            {
                ResetToken();
                Direction = new Point(PointWidth, 0);
                _ = move(Direction, canv, snake, _cts.Token);
                e.Handled = true;
            }
            else if (e.Key == Key.S)
            {
                ResetToken();
                Direction = new Point(0, PointWidth);
                _ = move(Direction, canv, snake, _cts.Token);
                e.Handled = true;
            }

            else if (e.Key == Key.A)
            {
                ResetToken();
                Direction = new Point(-PointWidth, 0);
                _ = move(Direction, canv, snake, _cts.Token);
                e.Handled = true;
            }
            else if (e.Key == Key.W)
            {
                ResetToken();
                Direction = new Point(0, -PointWidth);
                _ = move(Direction, canv, snake, _cts.Token);
                e.Handled = true;
            }
            else if (e.Key == Key.Space)
            {
                if (!_pause)
                {                    
                    ResetToken();
                    _pause = true;
                }
                else
                {
                    _ = move(Direction, canv, snake, _cts.Token); 
                    _pause = false;
                }

                e.Handled = true;
            }
        }

        private void foodEaten(Rectangle food)
        {
            canv.Children.Remove(food);
            FoodCollection.Remove(food);
            addTail();
            SnakeLaenge = snake.Points.Count;
        }

        private void addTail()
        {
            for(int i = 0; i < 3; i++)
            {
                var tail = snake.Points.FirstOrDefault();
                var newTail = new Point(tail.X + Direction.X * -1, tail.Y + Direction.Y * -1);
                snake.Points.Insert(0, newTail);
            }
        }

        private void checkHitFood()
        {
            var f = FoodCollection.Where<Rectangle>(f => Canvas.GetLeft(f) < snake.Points.LastOrDefault().X + 35 && Canvas.GetLeft(f) > snake.Points.LastOrDefault().X - 35 && Canvas.GetTop(f) < snake.Points.LastOrDefault().Y + 35 && Canvas.GetTop(f) > snake.Points.LastOrDefault().Y - 35).FirstOrDefault();
            if (f != null)
            {
                foodEaten(f);
                spawnFood();
            }
        }

        private bool checkCollision(Point p)
        {
            var result = snake.Points.Contains(p);
            for (int i = 0; i < snake.Points.Count; i++)
            {
                if (snake.Points[i].X == p.X && snake.Points[i].Y == p.Y)
                {
                    return true;
                }

            }
            return false;
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

                    shouldContinueV = lastHead.Y < canv.ActualHeight && lastHead.Y >= PointWidth;
                    shouldContinueH = lastHead.X < canv.ActualWidth && lastHead.X >= PointWidth;

                    if (shouldContinueH && shouldContinueV)
                    {
                        bool collisioned = checkCollision(newHead);
                        snake.Points.Add(newHead);

                        //Letztes Element entfernen, damit die Länge konstant bleibt
                        snake.Points.RemoveAt(0);

                        if (collisioned)
                        {
                            ResetToken();
                            MessageBox.Show("Game Over.");
                        }

                        OnPropertyChanged(nameof(SnakeHead_x));
                        OnPropertyChanged(nameof(SnakeHead_y));

                        checkHitFood();

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

                await Task.Delay(Speed, token);
            }
        }

        private void spawnOnOtherSideHor(double direction)
        {
            var y = snake.Points.LastOrDefault().Y;
            int length = snake.Points.Count;
            snake.Points.Clear();            
            double newHead_x = 0;

            if (direction > 0)
            {
                for (int i = 0; i < length; i++)
                {
                    snake.Points.Add(new Point(newHead_x, y));
                    newHead_x += PointWidth;
                }
            }
            else
            {
                newHead_x = canv.ActualWidth;

                for (int i = 0; i < length; i++)
                {
                    snake.Points.Add(new Point(newHead_x, y));
                    newHead_x -= PointWidth;
                    
                }
            }
        }

        private void spawnOnOtherSideVer(double direction)
        {
            var x = snake.Points.LastOrDefault().X;
            int length = snake.Points.Count;
            snake.Points.Clear();
            double newHead_y = 0;

            if (direction > 0)
            {
                for (int i = 0; i < length; i++)
                {
                    snake.Points.Add(new Point(x, newHead_y));
                    newHead_y += PointWidth;
                }
            }
            else
            {
                newHead_y = canv.ActualHeight;

                for (int i = 0; i < length; i++)
                {
                    snake.Points.Add(new Point(x, newHead_y));
                    newHead_y -= PointWidth;
                }
            }
        }

        private void spawnFood()
        {
            int i = FoodCollection.Count;
             while (i < 3)
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

                Canvas.SetTop(_food, topAndLeft);
                Canvas.SetLeft(_food, leftAndTop);

                Food_x = leftAndTop;
                Food_y = topAndLeft;
                OnPropertyChanged(nameof(Food_x));
                OnPropertyChanged(nameof(Food_y));

                canv.Children.Add(_food);
                FoodCollection.Add(_food);
                i++;
            }
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
        {;
            spawnFood();
            canv.Focus();
        }
    }

}