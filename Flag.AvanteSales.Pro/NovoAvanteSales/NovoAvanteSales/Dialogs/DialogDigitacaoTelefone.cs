using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

namespace AvanteSales.Pro.Dialogs
{
    [Activity(Theme = "@style/AvanteSales.Theme.Dialogs", ScreenOrientation = ScreenOrientation.Portrait)]
    public class DialogDigitacaoTelefone : Activity
    {
        EditText txtTelefone;
        Spinner cboTipoTelefone;
        Button btnOK;
        Button btnCancelar;
        bool isChanging;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.dialog_digitacao_telefone);

            FindViewsById();

            Eventos();

            CarregarTiposTelefone();

            if (CSTelefonesPDV.Current != null)
            {
                btnOK.Text = "Editar";

                CarregarDados();
            }

            txtTelefone.FindFocus();
        }

        private void CarregarDados()
        {
            txtTelefone.Text = CSTelefonesPDV.Current.NUM_DDD_TELEFONE.ToString() + CSTelefonesPDV.Current.NUM_TELEFONE;

            if (cboTipoTelefone.Adapter != null &&
                cboTipoTelefone.Adapter.Count > 0)
            {
                for (int i = 0; i < cboTipoTelefone.Adapter.Count; i++)
                {
                    var itemTipo = Convert.ToInt32(((CSItemCombo)cboTipoTelefone.GetItemAtPosition(i)).Valor);

                    if (itemTipo == CSTelefonesPDV.Current.COD_TIPO_TELEFONE)
                    {
                        cboTipoTelefone.SetSelection(i);
                        break;
                    }
                }

                cboTipoTelefone.Enabled = false;
            }
        }

        private void CarregarTiposTelefone()
        {
            cboTipoTelefone.Adapter = null;
            var adapter = cboTipoTelefone.SetDefaultAdapter();

            var tipos = CSTelefonesPDV.CSTipoTelefone.Items;

            foreach (var tipoAtual in tipos)
            {
                CSItemCombo ic = new CSItemCombo();
                ic.Texto = tipoAtual.DSC_TIPO_TELEFONE;
                ic.Valor = tipoAtual.COD_TIPO_TELEFONE;

                adapter.Add(ic);
            }
        }

        private void Eventos()
        {
            btnOK.Click += new EventHandler(btnOK_Click);
            btnCancelar.Click += new EventHandler(btnCancelar_Click);
            txtTelefone.TextChanged += new EventHandler<Android.Text.TextChangedEventArgs>(txtTelefone_TextChanged);
        }

        void txtTelefone_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (isChanging)
                return;

            if (e.AfterCount < 1)
                return;

            isChanging = true;

            string telefone = txtTelefone.Text.Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty);

            switch (telefone.Length)
            {
                case 2:
                    {
                        txtTelefone.Text = string.Format("({0})", telefone);
                    }
                    break;
                case 7:
                    {
                        txtTelefone.Text = string.Format("({0}){1}-{2}", telefone.Substring(0, 2), telefone.Substring(2, 4), telefone.Substring(6, telefone.Length - 6));
                    }
                    break;
                case 11:
                    {
                        txtTelefone.Text = string.Format("({0}){1}-{2}", telefone.Substring(0, 2), telefone.Substring(2, 5), telefone.Substring(7, 4));
                    }
                    break;
            }

            txtTelefone.SetSelection(txtTelefone.Text.Length);
            isChanging = false;
        }

        public override void OnBackPressed()
        {
            SalvarTelefone();
        }

        void btnCancelar_Click(object sender, EventArgs e)
        {
            Finish();
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            SalvarTelefone();
        }

        private void SalvarTelefone()
        {
            try
            {
                if (cboTipoTelefone.SelectedItem == null)
                {
                    MessageBox.AlertErro(this, "Inclusão de telefone interrompida por falta de Tipo cadastrado.");
                    return;
                }

                if (TelefoneValido())
                {
                    string telefone = txtTelefone.Text.Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty);
                    int DDDtelefone = Convert.ToInt32(telefone.Substring(0, 2));
                    string telefoneSemDDD = string.Empty;

                    if (telefone.Length <= 10)
                        telefoneSemDDD = telefone.Substring(2, telefone.Length - 2);
                    else
                        telefoneSemDDD = telefone.Substring(2, 9);

                    if (CSTelefonesPDV.Current == null)
                    {
                        CSTelefonesPDV.CSTelefonePDV novoTelefone = new CSTelefonesPDV.CSTelefonePDV();
                        novoTelefone.NUM_DDD_TELEFONE = DDDtelefone;
                        novoTelefone.NUM_TELEFONE = telefoneSemDDD;
                        novoTelefone.COD_TIPO_TELEFONE = Convert.ToInt32(((CSItemCombo)cboTipoTelefone.SelectedItem).Valor);
                        novoTelefone.AdicionarTelefone(novoTelefone);

                        CSPDVs.Current.TELEFONES_PDV = null;

                        ListaTelefone.CarregarTelefone = true;
                        MessageBox.ShowShortMessageBottom(this,"Telefone adicionado.");
                    }
                    else
                    {
                        CSTelefonesPDV.Current.NUM_DDD_TELEFONE = DDDtelefone;
                        CSTelefonesPDV.Current.NUM_TELEFONE = telefoneSemDDD;
                        CSTelefonesPDV.Current.COD_TIPO_TELEFONE = Convert.ToInt32(((CSItemCombo)cboTipoTelefone.SelectedItem).Valor);
                        CSTelefonesPDV.Current.EditarTelefone();

                        //Limpa a coleção obrigando um Refresh dos dados
                        CSTelefonesPDV.Current = null;
                        CSPDVs.Current.TELEFONES_PDV = null;

                        ListaTelefone.CarregarTelefone = true;
                        MessageBox.ShowShortMessageBottom(this,"Telefone editado.");
                    }

                    SetResult(Result.Ok);
                    Finish();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Alert(this, ex.Message);
            }
        }

        private bool TelefoneValido()
        {
            string telefone = txtTelefone.Text.Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty);

            if (telefone.Length < 10)
            {
                MessageBox.Alert(this, "Número inválido de caracteres. Telefone deve conter DDD.");
                return false;
            }

            return true;
        }

        private void FindViewsById()
        {
            txtTelefone = FindViewById<EditText>(Resource.Id.txtTelefone);
            cboTipoTelefone = FindViewById<Spinner>(Resource.Id.cboTipoTelefone);
            btnOK = FindViewById<Button>(Resource.Id.btnOK);
            btnCancelar = FindViewById<Button>(Resource.Id.btnCancelar);
        }
    }
}