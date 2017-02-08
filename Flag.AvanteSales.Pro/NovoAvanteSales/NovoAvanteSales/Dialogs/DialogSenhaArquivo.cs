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
using AvanteSales.Pro.Formatters;
using Android.Views.InputMethods;
using Android.Content.PM;

namespace AvanteSales.Pro.Dialogs
{
    [Activity(Label = "Senha arquivo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class DialogSenhaArquivo : Activity
    {
        EditText txtSenha;
        Button btnOK;
        string Senha;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.dialog_senha_arquivo);

            FindViewsById();

            Eventos();

            Senha = Intent.GetStringExtra("senha");

            this.Title = "Senha do arquivo";
        }

        protected override void OnStart()
        {
            base.OnStart();

            Window.SetSoftInputMode(SoftInput.StateVisible);
        }

        private void Eventos()
        {
            btnOK.Click += new EventHandler(btnOK_Click);
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            ValidarSenha();
        }

        private void ValidarSenha()
        {
            if (txtSenha.Text == Senha)
            {
                MessageBox.Alert(this, "Endereço de servidor importado com sucesso. Para dar continuidade, insira o vendedor.","Inserir",
                    (_sender, _e) =>
                    {
                        SetResult(Result.Ok);
                        base.OnBackPressed();
                    },false);
            }
            else
            {
                MessageBox.AlertErro(this, "Senha de arquivo incorreta.");
            }
        }

        private void FindViewsById()
        {
            txtSenha = FindViewById<EditText>(Resource.Id.txtSenha);
            btnOK = FindViewById<Button>(Resource.Id.btnOK);
        }
    }
}