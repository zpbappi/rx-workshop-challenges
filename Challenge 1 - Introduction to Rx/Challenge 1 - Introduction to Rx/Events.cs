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

        private event Action<int> LengthChangeListeners;
        private int previousLength = -1;

        public event Action<int> LengthChanged
        {
            add
            {
                LengthChangeListeners += value;
                TextChanged += s =>
                {
                    var text = s ?? string.Empty;
                    if (text.Length == previousLength || LengthChangeListeners == null)
                    {
                        return;
                    }

                    previousLength = text.Length;
                    LengthChangeListeners(text.Length);
                };
            }
            remove
            {
                LengthChangeListeners -= value;
            }
        }
    }
}
