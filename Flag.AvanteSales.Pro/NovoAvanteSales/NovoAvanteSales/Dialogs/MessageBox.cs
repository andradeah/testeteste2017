using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Android.App;

namespace AvanteSales.Pro.Dialogs
{
    public class MessageBox
    {
        public static void AlertErro(Activity activity, string mensagem)
        {
            try
            {
                activity.RunOnUiThread(() =>
                    {
                        View view = activity.LayoutInflater.Inflate(Resource.Layout.alert_dialog_error, null);
                        ((TextView)view.FindViewById<TextView>(Resource.Id.lblErro)).Text = mensagem;

                        Android.Support.V7.App.AlertDialog.Builder alerta = new Android.Support.V7.App.AlertDialog.Builder(activity);
                        alerta.SetView(view);
                        alerta.SetCancelable(true);
                        alerta.Show();
                    });
            }
            catch
            {

            }
        }

        public static void Alert(Activity activity, string mensagem)
        {
            try
            {
                activity.RunOnUiThread(() =>
                {
                    Android.Support.V7.App.AlertDialog.Builder alerta = new Android.Support.V7.App.AlertDialog.Builder(activity);
                    alerta.SetMessage(mensagem);
                    alerta.SetCancelable(true);
                    alerta.Show();
                });
            }
            catch
            {

            }
        }

        public static void Alert(Activity activity, string mensagem, string nomePositivo, EventHandler<DialogClickEventArgs> acaoPositivo, string nomeNegativo, EventHandler<DialogClickEventArgs> acaoNegativo, string nomeNeutro, EventHandler<DialogClickEventArgs> acaoNeutro, bool cancelable)
        {
            try
            {
                activity.RunOnUiThread(() =>
                {
                    Android.Support.V7.App.AlertDialog.Builder alerta = new Android.Support.V7.App.AlertDialog.Builder(activity);
                    alerta.SetMessage(mensagem);
                    alerta.SetCancelable(cancelable);
                    alerta.SetPositiveButton(nomePositivo, acaoPositivo);
                    alerta.SetNegativeButton(nomeNegativo, acaoNegativo);
                    alerta.SetNeutralButton(nomeNeutro, acaoNeutro);
                    alerta.Show();
                });
            }
            catch
            {

            }
        }

        public static void Alert(Activity activity, string mensagem, string nomePositivo, EventHandler<DialogClickEventArgs> acaoPositivo, string nomeNegativo, EventHandler<DialogClickEventArgs> acaoNegativo, bool cancelable)
        {
            try
            {
                activity.RunOnUiThread(() =>
                    {
                        Android.Support.V7.App.AlertDialog.Builder alerta = new Android.Support.V7.App.AlertDialog.Builder(activity);
                        alerta.SetMessage(mensagem);
                        alerta.SetCancelable(cancelable);
                        alerta.SetPositiveButton(nomePositivo, acaoPositivo);
                        alerta.SetNegativeButton(nomeNegativo, acaoNegativo);
                        alerta.Show();
                    });
            }
            catch
            {

            }
        }

        public static void Alert(Activity activity, string mensagem, string nomePositivo, EventHandler<DialogClickEventArgs> acaoPositivo, bool cancelable)
        {
            try
            {
                activity.RunOnUiThread(() =>
                {
                    Android.Support.V7.App.AlertDialog.Builder alerta = new Android.Support.V7.App.AlertDialog.Builder(activity);
                    alerta.SetMessage(mensagem);
                    alerta.SetCancelable(cancelable);
                    alerta.SetPositiveButton(nomePositivo, acaoPositivo);
                    alerta.Show();
                });
            }
            catch
            {

            }
        }

        public static void ShowShortMessageBottom(Activity activity, string message)
        {
            try
            {
                activity.RunOnUiThread(() =>
                {
                    Toast toast = Toast.MakeText(activity, message, ToastLength.Short);
                    toast.Show();
                });
            }
            catch
            {

            }
        }

        public static void ShowShortMessageCenter(Activity activity, string message)
        {
            try
            {
                activity.RunOnUiThread(() =>
                {
                    Toast toast = Toast.MakeText(activity, message, ToastLength.Short);
                    toast.SetGravity(GravityFlags.CenterVertical, 0, 0);
                    toast.Show();
                });
            }
            catch
            {

            }
        }
    }
}