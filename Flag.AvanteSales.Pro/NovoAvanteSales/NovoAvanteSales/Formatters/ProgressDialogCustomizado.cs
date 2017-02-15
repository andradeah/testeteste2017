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
using Android.Graphics.Drawables;

namespace AvanteSales.Pro.Formatters
{
    public class ProgressDialogCustomizado
    {
        Context Ctx;
        LayoutInflater Inflater;

        public ProgressDialogCustomizado(Activity contexto,LayoutInflater inflater)
        {
            Ctx = contexto;
            Inflater = inflater;
        }

        public ProgressDialog Customizar()
        {
            try
            {
                ProgressDialog progress = ProgressDialog.Show(Ctx, "", null, true);

                View view = Inflater.Inflate(Resource.Layout.progress_dialog, null);

                TextView txt = view.FindViewById<TextView>(Resource.Id.lblStatus);
                txt.SetTextColor(Android.Graphics.Color.White);

                progress.SetContentView(view);
                progress.Window.SetBackgroundDrawable(new ColorDrawable(Android.Graphics.Color.Transparent));

                return progress;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}