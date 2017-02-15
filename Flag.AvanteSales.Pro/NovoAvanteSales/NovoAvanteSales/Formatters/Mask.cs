using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace AvanteSales.Pro.Formatters
{
    public class Mask : Java.Lang.Object, ITextWatcher
    {
        private readonly EditText _editText;
        private readonly string _mask;
        bool isUpdating;
        string old = "";

        public Mask(EditText editText, string mask)
        {
            _editText = editText;
            _mask = mask;
        }

        public static string Unmask(string s)
        {
            return s.Replace(".", "").Replace("-", "")
                .Replace("/", "").Replace("(", "")
                .Replace(")", "");
        }

        public void AfterTextChanged(IEditable s)
        {
        }

        public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
        }

        public void OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            string str = Unmask(s.ToString());
            string mascara = "";

            if (isUpdating)
            {
                old = str;
                isUpdating = false;
                return;
            }

            int i = 0;

            foreach (var m in _mask.ToCharArray())
            {
                if (m != '#' && str.Length > old.Length)
                {
                    mascara += m;
                    continue;
                }
                try
                {
                    if (!string.IsNullOrEmpty(str))
                        mascara += str[i];
                }
                catch (System.Exception)
                {
                    break;
                }
                i++;
            }

            isUpdating = true;

            if (mascara.Length == 8 &&
                !mascara.Contains('/'))
            {
                string dia = mascara.Substring(0, 2);
                string mes = mascara.Substring(2, 2);
                string ano = mascara.Substring(4, 4);

                _editText.Text = string.Format("{0}/{1}/{2}", dia, mes, ano);
            }
            else
                _editText.Text = mascara;

            _editText.SetSelection(mascara.Length);
        }
    }
}