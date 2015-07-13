using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntroductionToRx
{
    class Events
    {
        public event Action<string> TextChanged;

        public virtual void OnTextChanged(string text)
        {
            var t = TextChanged;
            if (t != null)
                t(text);
        }

        private event Action<int> LengthChangedEvents;
        private int previousLength = -1;

        public event Action<int> LengthChanged
        {
            add
            {
                LengthChangedEvents += value;
                TextChanged += text =>
                {
                    if (text.Length == previousLength || LengthChangedEvents == null)
                    {
                        return;
                    }

                    previousLength = text.Length;
                    LengthChangedEvents(text.Length);
                };
            }
            remove
            {
                LengthChangedEvents -= value;
            }
        }
    }
}
