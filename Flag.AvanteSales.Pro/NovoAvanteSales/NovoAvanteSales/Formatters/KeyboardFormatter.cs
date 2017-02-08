using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Views.InputMethods;

namespace AvanteSales.Pro.Formatters
{
    public static class KeyboardFormatter
    {
        public static void HideKeyboard(this Context context ,View view)
        {
            InputMethodManager imm = (InputMethodManager)context.GetSystemService(Context.InputMethodService);
            imm. HideSoftInputFromWindow(view.WindowToken, 0);
        }

        public static void ShowKeyboard(this Context context)
        {
            InputMethodManager imm = (InputMethodManager)context.GetSystemService(Context.InputMethodService);
            imm.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);            
        }
    }
}