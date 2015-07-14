using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
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
using System.Reactive.Linq;

namespace ReactiveCoincidence
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var mouseDown = from evt in Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>(h => image.MouseDown += h, h => image.MouseDown -= h)
                            select evt.EventArgs.GetPosition(this);
            var mouseUp = from evt in Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>(h => MouseUp += h, h => MouseUp -= h)
                          select evt.EventArgs.GetPosition(this);
            var mouseMove = from evt in Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(h => MouseMove += h, h => MouseMove -= h)
                            select evt.EventArgs.GetPosition(this);

            // TODO: Write a query to compute a stream of deltas to move the picture box when the mouse
            //       button is down.
            // HINT: There are many ways to do this. Try to use the concepts of events with duration and aggregate
            //       to see one way to solve this problem.  Also don't forget the Subtract method given below.



            var query = Observable.Join(
                mouseDown,
                mouseMove,
                _ => mouseUp,
                _ => Observable.Empty<Unit>(),
                (start, end) => end
            )
            .Buffer(2, 1)
            .Select(b => Subtract(b.Last(), b.First()));

            query.Subscribe(delta =>
            {
                Canvas.SetLeft(image, Canvas.GetLeft(image) + delta.X);
                Canvas.SetTop(image, Canvas.GetTop(image) + delta.Y);
            });

        }

        static Point Subtract(Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }
    }
}
