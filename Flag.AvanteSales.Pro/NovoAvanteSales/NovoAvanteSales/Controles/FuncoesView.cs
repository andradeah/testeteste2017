using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;

namespace AvanteSales.Pro.Controles
{
    public class FuncoesView
    {
        public static void SetarLabelErroControles(Activity activity, TextInputLayout input, string mensagem)
        {
            try
            {
                input.ErrorEnabled = true;
                input.Error = mensagem;

                CSGlobal.EsconderTeclado(activity);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static void SetarLabelErroControles(Activity activity, TextInputLayout input)
        {
            try
            {
                input.ErrorEnabled = true;
                input.Error = "Preenchimento obrigatório";

                CSGlobal.EsconderTeclado(activity);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}