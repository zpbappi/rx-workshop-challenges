using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reactive.Concurrency;
using System.Windows.Forms.DataVisualization.Charting;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Schedulers
{
    public partial class MainForm : Form
    {
        IDisposable chartUpdater;

        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var scheduler = new MyHistoricalScheduler();
            var quotes = GetQuotes(scheduler, StockQuote.LoadQuotes());
            var query = Query(quotes);

            chartUpdater = query.BindToChart(chart, 30);
            scheduler.Run(TimeSpan.FromSeconds(.1));
        }

        IObservable<StockQuote> GetQuotes(IScheduler scheduler, IEnumerable<StockQuote> quotes)
        {
            // TODO: Create an observable source of stock quotes
            // HINT: Use both the scheduler and the quotes and think about how to create sources which are like events
            var subject = new Subject<StockQuote>();

            quotes.ToObservable()
                .Subscribe(q => scheduler.Schedule(new DateTimeOffset(q.Date), () => subject.OnNext(q)));

            return subject;
        }

        IObservable<object> Query(IObservable<StockQuote> quotes)
        {
            // TODO: Write a query to grab the Microsoft "MSFT" stock quotes and output the closing price
            // HINT: Make sure you include a property in the result which has a type of DateTime

            return quotes
                .Where(s => s.Symbol == "MSFT")
                .Select(q => new {q.Close, q.Date});
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            chartUpdater.Dispose();

            base.OnClosing(e);
        }
    }

    static class Ext
    {
        public static IDisposable BindToChart<T>(this IObservable<T> source, Chart chart, int n)
        {
            var all = from p in source.GetType().GetGenericArguments()[0].GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                      where p.GetGetMethod() != null
                      select p;

            var dt = all.Where(p => p.PropertyType.Equals(typeof(DateTime))).Single();

            var ps = all.Where(p => p != dt).ToList();

            chart.Series.Clear();
            foreach (var p in ps)
            {
                var series = new Series();
                series.ChartArea = chart.ChartAreas[0].Name;
                series.Legend = chart.Legends[0].Name;
                series.Name = p.Name;
                series.ChartType = SeriesChartType.Line;
                series.XValueType = ChartValueType.DateTime;
                chart.Series.Add(series);
            }
            chart.Update();

            return source.ObserveOn(chart).Subscribe(data =>
            {
                var x = dt.GetGetMethod().Invoke(data, new object[0]);
                foreach (var p in ps)
                {
                    var series = chart.Series[p.Name];
                    var y = p.GetGetMethod().Invoke(data, new object[0]);
                    series.Points.AddXY(x, y);
                }
                chart.Update();

                foreach (var series in chart.Series)
                    while (series.Points.Count >= n)
                        series.Points.RemoveAt(0);
            });
        }

    }
}
