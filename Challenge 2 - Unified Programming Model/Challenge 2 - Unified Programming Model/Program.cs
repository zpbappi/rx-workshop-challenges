using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;
using System.Reactive;
using UnifiedProgrammingModel.DictionaryService;

namespace UnifiedProgrammingModel
{
    class Program
    {
        static void Main(string[] args)
        {
            var txt = new TextBox();
            var lst = new ListBox { Top = txt.Height + 10 };
            var frm = new Form { Controls = { txt, lst } };

            // TODO: Convert txt.TextChanged to IObservable<EventPattern<EventArgs>> and assign it to textChanged.
            // HINT: Try using FromEventPattern.
            var textChanged = Observable.FromEventPattern(txt, "TextChanged");

            // TODO: Convert BeginMatch/EndMatch to Func<string, IObservable<DictionaryWord[]>> and assign it to getSuggestions.
            // HINT: Try using FromAsyncPattern

            var getSuggestions =
                new Func<string, IObservable<DictionaryWord[]>>(
                    s =>
                        Observable.FromAsyncPattern<DictionaryWord[]>((cb, state) => BeginMatch(s, cb, state), EndMatch)()
                    );

            var results = from _ in textChanged
                          let text = txt.Text
                          where text.Length >= 3
                          from suggestions in getSuggestions(text)
                          select suggestions;

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
