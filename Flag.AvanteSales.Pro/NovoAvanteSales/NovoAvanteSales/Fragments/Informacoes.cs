using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AvanteSales.Pro.Dialogs;
using AvanteSales.Pro.Formatters;
using AvanteSales.SystemFramework;

namespace AvanteSales.Pro.Fragments
{
    public class Informacoes : Android.Support.V4.App.Fragment
    {
        private TextView lblRazaoSocial;
        private TextView lblInscrEstadual;
        private TextView lblCNPJ;
        private TextView lblCondPag;
        private TextView lblRamoAtividade;
        private TextView lblLimiteCredito;
        private TextView lblSegmento;
        private TextView lblGrupoCliente;
        private TextView lblClassificacaoGrupoCliente;
        private TextView lblUnidadeNegocio;
        private Spinner cboTipoEndereco;
        private TextView tvEndereco;
        private TextView lblBairro;
        private TextView lblCEP;
        private Spinner cboTipoTelefone;
        private TextView lblTelefone;
        private TextView lblDescDenver;
        private TextView tvDescDenver;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.informacoes, container, false);
            FindViewsById(view);
            Eventos();
            return view;
        }

        private void Eventos()
        {
            cboTipoEndereco.ItemSelected += CboTipoEndereco_ItemSelected;
            cboTipoTelefone.ItemSelected += CboTipoTelefone_ItemSelected;
            SetUpCombo<CSEnderecosPDV.CSEnderecoPDV>(cboTipoEndereco, CSPDVs.Current.ENDERECOS_PDV.Items, c => c.DSC_TIPO_ENDERECO);
            SetUpCombo<CSTelefonesPDV.CSTelefonePDV>(cboTipoTelefone, CSPDVs.Current.TELEFONES_PDV.Items, c => c.DSC_TIPO_TELEFONE);
        }

        private void SetUpCombo<T>(Spinner spinner, CollectionBase colection, Func<T, string> campo)
        {
            var adapter = new ArrayAdapter(Activity, Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            foreach (T endPDV in colection)
            {
                CSItemCombo ic = new CSItemCombo();
                ic.Texto = campo(endPDV);
                ic.Valor = endPDV;
                adapter.Add(ic);
            }
            spinner.Adapter = adapter;
            if (adapter.Count > 0) { spinner.SetSelection(0); }
        }

        private void CboTipoTelefone_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            CSTelefonesPDV.CSTelefonePDV telPDV = CSPDVs.Current.TELEFONES_PDV.Items[cboTipoTelefone.SelectedItemPosition];
            lblTelefone.Text = "(" + telPDV.NUM_DDD_TELEFONE + ")" + telPDV.NUM_TELEFONE;
        }

        private void CboTipoEndereco_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                CSEnderecosPDV.CSEnderecoPDV endPDV = CSPDVs.Current.ENDERECOS_PDV.Items[cboTipoEndereco.SelectedItemPosition];
                tvEndereco.Text = endPDV.DSC_LOGRADOURO_COMPLEMENTO;
                lblBairro.Text = endPDV.DSC_BAIRRO;

                // Mascarando CEP
                string CEP = endPDV.NUM_CEP.PadRight(8, '0');
                CEP = CEP.Insert(2, ".");
                CEP = CEP.Insert(6, "-");
                lblCEP.Text = CEP;

                lblInscrEstadual.Text = lblInscrEstadual.Text.ToInscSocial(endPDV.COD_UF);
            }
            catch (Exception ex)
            {
                MessageBox.AlertErro(Activity, ex.Message);
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            Inicializacao();
        }

        private void Inicializacao()
        {
            if (!CSEmpresa.Current.UtilizaDescricaoDenver)
            {
                lblDescDenver.Visibility = ViewStates.Gone;
                tvDescDenver.Visibility = ViewStates.Gone;
            }
            else
            {
                lblDescDenver.Visibility = ViewStates.Visible;
                tvDescDenver.Visibility = ViewStates.Visible;
                tvDescDenver.Text = CSPDVs.Current.DSC_DENVER;
            }

            lblRazaoSocial.Text = CSPDVs.Current.DSC_RAZAO_SOCIAL.ToTitleCase();
            lblInscrEstadual.Text = CSPDVs.Current.NUM_INSCRICAO_ESTADUAL;
            lblCNPJ.Text = CSPDVs.Current.NUM_CGC.ToCNPJ();
            lblCondPag.Text = CSPDVs.Current.DSC_CONDICAO_PAGAMENTO.ToString();
            lblRamoAtividade.Text = CSPDVs.Current.DSC_CATEGORIA;

            lblLimiteCredito.Text = CSPDVs.Current.VLR_LIMITE_CREDITO.ToString("C");
            lblSegmento.Text = CSPDVs.Current.COD_SEGMENTACAO.ToString() + " - " + CSPDVs.Current.DSC_SEGMENTACAO;
            lblGrupoCliente.Text = CSPDVs.Current.COD_GRUPO.ToString() + " - " + CSPDVs.Current.DESCRICAO_GRUPO;
            lblClassificacaoGrupoCliente.Text = CSPDVs.Current.COD_CLASSIFICACAO.ToString() + " - " + CSPDVs.Current.DESCRICAO_CLASSIFICACAO;
            lblUnidadeNegocio.Text = CSPDVs.Current.COD_UNIDADE_NEGOCIO.ToString() + " - " + CSPDVs.Current.DESCRICAO_UNIDADE_NEGOCIO;
        }

        private void FindViewsById(View view)
        {
            lblRazaoSocial = view.FindViewById<TextView>(Resource.Id.tvRazao);
            lblInscrEstadual = view.FindViewById<TextView>(Resource.Id.tvInscr);
            lblCNPJ = view.FindViewById<TextView>(Resource.Id.tvCNPJ);
            lblCondPag = view.FindViewById<TextView>(Resource.Id.tvCond);
            lblRamoAtividade = view.FindViewById<TextView>(Resource.Id.tvRamo);
            lblLimiteCredito = view.FindViewById<TextView>(Resource.Id.tvLimit);
            lblSegmento = view.FindViewById<TextView>(Resource.Id.tvSegmento);
            lblGrupoCliente = view.FindViewById<TextView>(Resource.Id.tvGrupoCliente);
            lblClassificacaoGrupoCliente = view.FindViewById<TextView>(Resource.Id.tvClassGrupoCliente);
            lblUnidadeNegocio = view.FindViewById<TextView>(Resource.Id.tvUnidadeNegocio);
            tvEndereco = view.FindViewById<TextView>(Resource.Id.tvEndereco);
            lblBairro = view.FindViewById<TextView>(Resource.Id.tvBairro);
            lblCEP = view.FindViewById<TextView>(Resource.Id.tvCep);
            cboTipoTelefone = view.FindViewById<Spinner>(Resource.Id.cboTipoTelefone);
            lblTelefone = view.FindViewById<TextView>(Resource.Id.tvTelefone);
            lblDescDenver = view.FindViewById<TextView>(Resource.Id.lblDescDenver);
            tvDescDenver = view.FindViewById<TextView>(Resource.Id.tvDescDenver);
            cboTipoEndereco = view.FindViewById<Spinner>(Resource.Id.cboTipoEndereco);
        }
    }
}