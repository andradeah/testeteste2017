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
using Android.Content.PM;
using System.IO;

namespace AvanteSales.Pro.Dialogs
{
    [Activity(Theme = "@style/AvanteSales.Theme.Dialogs", ScreenOrientation = ScreenOrientation.Portrait)]
    public class DialogAutenticar : Activity
    {
        private EditText edtUsuario;
        private EditText edtSenha;
        private Button btnOk;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.Title = "Autenticar usuário";
            // Create your application here
            SetContentView(Resource.Layout.dialog_autenticar);

            FindViewsById();

            btnOk.Click += new EventHandler(btnOk_Click);
        }

        void btnOk_Click(object sender, EventArgs e)
        {
            if (edtUsuario.Text == string.Empty)
            {
                MessageBox.AlertErro(this, "Preencha o usuário");
            }
            else
            {
                if (!CSUsuario.ValidaUsuario(edtUsuario.Text, edtSenha.Text, Convert.ToInt32(Intent.GetStringExtra("codigoVendedorAtual"))))
                {
                    MessageBox.AlertErro(this, "Usuário ou senha inválido.");
                }
                else
                {
                    DeletarBanco();
                    MessageBox.Alert(this, "Banco apagado com sucesso","Ok", (_sender, _e) => { OnBackPressed(); },false);
                }
            }
        }

        private void DeletarBanco()
        {
            string file = Path.Combine(CSGlobal.GetCurrentDirectoryDB(), "AvanteSales" + CSGlobal.COD_REVENDA + ".sdf");

            if (File.Exists(file))
            {
                try
                {
                    CSDataAccess.Instance.DisposeConexao();
                    File.Delete(file);
                }
				catch (Exception)
                {
                }
            }
        }

        private void FindViewsById()
        {
            edtUsuario = FindViewById<EditText>(Resource.Id.edtUsuario);
            edtSenha = FindViewById<EditText>(Resource.Id.edtSenha);
            btnOk = FindViewById<Button>(Resource.Id.btnOK);
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
        }
    }
}