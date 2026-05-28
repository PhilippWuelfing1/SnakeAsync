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
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        Polyline _polyline = new Polyline();

        public MainWindow()
        {
            InitializeComponent();
            //canv.Focus();                        
            BuildLine();
        }

        private void BuildLine()
        {
            // Create the Polyline element
            _polyline.Stroke = Brushes.Black;
            _polyline.StrokeThickness = 10;

            // Define the coordinates
            PointCollection points = new PointCollection();
            points.Add(new Point(100, 100));
            points.Add(new Point(100, 200));

            _polyline.Points = points;
            canv.Children.Add(_polyline);
        }

        private async void canv_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D)
            {
                ResetToken();
                _ = move(new Point(10, 0), canv, _polyline, _cts.Token);
                //_ = GrowingRightAsync(canv, _polyline, _cts.Token);
                e.Handled = true;
            }
            else if (e.Key == Key.S)
            {
                ResetToken();
                _ = move(new Point(0, 10), canv, _polyline, _cts.Token);
                e.Handled = true;
            }

            else if (e.Key == Key.A)
            {
                ResetToken();
                _ = move(new Point(-10, 0), canv, _polyline, _cts.Token);
                e.Handled = true;
            }
            else if (e.Key == Key.W)
            {
                ResetToken();
                _ = move(new Point(0, -10), canv, _polyline, _cts.Token);
                e.Handled = true;
            }
        }

        private async Task move(Point direction, Canvas canv, Polyline _polyline, CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                bool shouldContinue = false;

                await Dispatcher.InvokeAsync(() =>
                {
                    var lastPoint = _polyline.Points.LastOrDefault();
                    shouldContinue = lastPoint.Y < canv.ActualHeight - 400;

                    if (shouldContinue)
                    {
                        _polyline.Points.Add(new Point(lastPoint.X + direction.X, lastPoint.Y + direction.Y));
                    }
                }, System.Windows.Threading.DispatcherPriority.Render);

                if (!shouldContinue) break;

                await Task.Delay(30, token);
            }
        }

        private void ResetToken()
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }
    }
}