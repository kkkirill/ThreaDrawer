using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ThreaDrawer
{
    public partial class MainWindow : Window
    {
        private SynchronizationContext context;

        private List<Thread> threads;
        private List<Brush> brushes;
        private List<Canvas> canvases;
        private List<Point> points;
        private List<Point> lastPoints;

        private bool isRunning;
        private static object sync = new object();

        public MainWindow()
        {
            InitializeComponent();

            context = SynchronizationContext.Current;
            isRunning = false;
            threads = new List<Thread>();

            brushes = new List<Brush> { Brushes.Red, Brushes.Green, Brushes.Blue };
            canvases = new List<Canvas> { redCanvas, greenCanvas, blueCanvas };
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var target = (sender as Canvas);
            points[canvases.IndexOf(target)] = e.GetPosition(target);
        }

        private void DrawRectangle(SynchronizationContext context, int i)
        {
            while (true)
            {
                lock (sync)
                {
                    if (points[i] != lastPoints[i])
                    {
                        context.Send((v) => DrawRectangle(points[i], canvases[i], brushes[i]), null);
                        lastPoints[i] = points[i];
                    }
                }
                Thread.Sleep(100);
            }
        }

        private void DrawRectangle(Point point, Canvas targetCanvas, Brush brush)
        {
            if (point.X > 0 && point.Y > 0)
            {                
                Rectangle rectangle = new Rectangle()
                {
                    Height = 20,
                    Width = 40,
                    Fill = brush,
                    Margin = new Thickness(point.X - 20, point.Y - 10, 0, 0)
                };
                targetCanvas.Children.Add(rectangle);
            }
        }

        private void runButton_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                MessageBox.Show("Already running!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            points = new List<Point>() { new Point(), new Point(), new Point() };
            lastPoints = new List<Point>() { new Point(), new Point(), new Point() };

            for (int i = 0; i < threadAmount.Value; i++)
            {
                int index = i;
                Thread thread = new Thread((p) => DrawRectangle(context, index));
                thread.Start();
                threads.Add(thread);
            }
            isRunning = true;
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isRunning)
            {
                MessageBox.Show("Already stopped!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            threads.ForEach(v => v.Abort());
            threads.Clear();
            points.Clear();
            lastPoints.Clear();
            isRunning = false;
        }
    }
}
