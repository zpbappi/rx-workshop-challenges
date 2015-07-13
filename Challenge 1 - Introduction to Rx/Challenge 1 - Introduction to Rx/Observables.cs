using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace IntroductionToRx
{
    class Observables
    {
        ISubject<string> textChanged = new Subject<string>();

        public virtual void OnTextChanged(string text)
        {
            textChanged.OnNext(text);
        }

        public IObservable<string> TextChanged { get { return textChanged; } }

        public IObservable<int> LengthChanged
        {
            get
            {
                return TextChanged.Select(s => s.Length).DistinctUntilChanged();
            }
        }
    }
}
