using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AvanteSales.Pro.Formatters;
using AvanteSales.Pro.Fragments;
using AvanteSales.SystemFramework;
using AvanteSales.SystemFramework.CSPDV;

namespace AvanteSales.Pro.Dialogs
{
    [Activity(Theme = "@style/AvanteSales.Theme.Dialogs", ScreenOrientation = ScreenOrientation.Portrait)]
    public class DialogDigitacaoEmail : Activity
    {
        Spinner cboTipoEmail;
        EditText txtEmail;
        Button btnOK;
        Button btnCancelar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.dialog_digitacao_email);

            FindViewsById();
            Eventos();

            CarregarTiposEmail();

            if (CSPDVEmails.Current != null)
            {
                btnOK.Text = "Editar";

                CarregarDados();
            }

            txtEmail.FindFocus();
        }

        private void CarregarDados()
        {
            txtEmail.Text = CSPDVEmails.Current.DSC_EMAIL;

            if (cboTipoEmail.Adapter != null &&
                cboTipoEmail.Adapter.Count > 0)
            {
                for (int i = 0; i < cboTipoEmail.Adapter.Count; i++)
                {
                    var itemTipo = Convert.ToInt32(((CSItemCombo)cboTipoEmail.GetItemAtPosition(i)).Valor);

                    if (itemTipo == CSPDVEmails.Current.COD_TIPO_EMAIL)
                    {
                        cboTipoEmail.SetSelection(i);
                        break;
                    }
                }
            }
        }

        private void Eventos()
        {
            btnOK.Click += new EventHandler(btnOK_Click);
            btnCancelar.Click += new EventHandler(btnCancelar_Click);
        }

        void btnCancelar_Click(object sender, EventArgs e)
        {
            CSPDVEmails.Current = null;
            Finish();
        }

        public override void OnBackPressed()
        {
            SalvarEmail();
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            SalvarEmail();
        }

        private void SalvarEmail()
        {
            if (cboTipoEmail.SelectedItem == null)
            {
                MessageBox.ShowShortMessageBottom(this, "Inclusão de e-mail interrompida por falta de Tipo cadastrado.");
                return;
            }

            if (EmailValido())
            {
                if (CSPDVEmails.Current == null)
                {
                    CSPDVEmails.CSPDVEmail novoEmail = new CSPDVEmails.CSPDVEmail();
                    novoEmail.DSC_EMAIL = txtEmail.Text;
                    novoEmail.COD_TIPO_EMAIL = Convert.ToInt32(((CSItemCombo)cboTipoEmail.SelectedItem).Valor);
                    novoEmail.AdicionarEmail(novoEmail);

                    //Limpa a coleção obrigando um Refresh dos dados
                    CSPDVs.Current.EMAILS = null;

                    ListaEmail.CarregarEmail = true;
                    MessageBox.ShowShortMessageBottom(this,"E-mail adicionado.");
                }
                else
                {
                    CSPDVEmails.Current.DSC_EMAIL = txtEmail.Text;
                    CSPDVEmails.Current.COD_TIPO_EMAIL = Convert.ToInt32(((CSItemCombo)cboTipoEmail.SelectedItem).Valor);
                    CSPDVEmails.Current.EditarEmail();

                    //Limpa a coleção obrigando um Refresh dos dados
                    CSPDVEmails.Current = null;
                    CSPDVs.Current.EMAILS = null;

                    ListaEmail.CarregarEmail = true;
                    MessageBox.ShowShortMessageBottom(this, "E-mail editado.");
                }

                SetResult(Result.Ok);
                Finish();
            }
        }

        private bool EmailValido()
        {
            string formato = "^([0-9a-zA-Z]+([_.-]?[0-9a-zA-Z-_.]+)*@[0-9a-zA-Z]+[0-9,a-z,A-Z,.,-]*(.){1}[a-zA-Z]{2,4})+$";

            var math = Regex.Match(txtEmail.Text, formato);

            if (math.Success)
                return true;

            MessageBox.AlertErro(this, "Formato de e-mail inválido.");

            return false;
        }

        private void FindViewsById()
        {
            cboTipoEmail = FindViewById<Spinner>(Resource.Id.cboTipoEmail);
            txtEmail = FindViewById<EditText>(Resource.Id.txtEmail);
            btnOK = FindViewById<Button>(Resource.Id.btnOK);
            btnCancelar = FindViewById<Button>(Resource.Id.btnCancelar);
        }

        private void CarregarTiposEmail()
        {
            cboTipoEmail.Adapter = null;
            var adapter = cboTipoEmail.SetDefaultAdapter();

            int indexPadrao = 0;
            int index = 0;

            var tipos = CSPDVEmails.CSTiposEmail.Items;

            foreach (var tipoAtual in tipos)
            {
                if (tipoAtual.COD_TIPO == 2)
                    indexPadrao = index;

                CSItemCombo ic = new CSItemCombo();
                ic.Texto = tipoAtual.DSC_TIPO;
                ic.Valor = tipoAtual.COD_TIPO;

                adapter.Add(ic);

                index++;
            }

            cboTipoEmail.SetSelection(indexPadrao);
        }
    }
}