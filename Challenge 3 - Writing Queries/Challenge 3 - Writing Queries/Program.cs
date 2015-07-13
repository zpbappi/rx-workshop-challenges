using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;
using System.Reactive;
using WritingQueries.DictionaryService;

namespace WritingQueries
{
    static class Program
    {
        static void Main(string[] args)
        {
            var txt = new TextBox();
            var lst = new ListBox { Top = txt.Height + 10 };
            var frm = new Form { Controls = { txt, lst } };

            var textChanged = Observable.FromEventPattern<EventHandler, EventArgs>(h => txt.TextChanged += h, h => txt.TextChanged -= h);
                    
            var getSuggestions = Observable.FromAsyncPattern<string, DictionaryWord[]>(BeginMatch, EndMatch);

            // TODO: Call additional query operators to avoid looking up suggestions for the same text twice in a row
            //       and waiting to look up suggestions until the user pauses at least 200 ms.
            // HINT: Try using DistinctUntilChanged and Throttle.

            var lookup = textChanged
                            .Select(_ => txt.Text)
                            .Do(text => Console.WriteLine("TextChanged: {0}", text))
                            .Where(text => text.Length >= 3)
                            .Do(text => Console.WriteLine("Lookup: {0}", text));


            // TODO: Eliminate the race condition caused by out-of-order arrivals.
            // HINT: Try using Switch.
            var results = lookup
                          .Select(text => getSuggestions(text))
                          .Merge();

            using (results
                .ObserveOn(lst)
                .Subscribe(words =>
                {
                    lst.Items.Clear();
                    lst.Items.AddRange(words.Select(word => word.Word).Take(10).ToArray());
                }))
            {
                Application.Run(frm);
            }
        }

        static DictServiceSoapClient service = new DictServiceSoapClient("DictServiceSoap");

        static IAsyncResult BeginMatch(string prefix, AsyncCallback callback, object state)
        {
            return service.BeginMatchInDict("wn", prefix, "prefix", callback, state);
        }

        static DictionaryWord[] EndMatch(IAsyncResult result)
        {
            return service.EndMatchInDict(result);
        }
    }
}
