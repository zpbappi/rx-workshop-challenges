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

        public event Action<int> LengthChanged
        {
            add
            {
                // TODO: Code to add a handler
            }
            remove
            {
                // TODO: Code to remove a handler
            }
        }
    }
}
