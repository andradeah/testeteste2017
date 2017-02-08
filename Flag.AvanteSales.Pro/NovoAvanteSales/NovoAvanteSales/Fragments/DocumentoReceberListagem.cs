using System;
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

namespace AvanteSales.Pro.Fragments
{
    public class DocumentoReceberListagem : Android.Support.V4.App.Fragment
    {
        LayoutInflater thisLayoutInflater;
        LinearLayout llRepeater;
        Android.Support.V4.App.FragmentActivity ActivityContext;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            CarregarDados();
        }

        private void CarregarDados()
        {
            try
            {
                CSPDVs.CSPDV pdvAtual = CSPDVs.Current;

                string codigoRevenda = CSGlobal.COD_REVENDA;

                llRepeater.RemoveAllViews();

                // [ Recupera documentos da empresa ]
                CSDocumentosReceberPDV documentosAReceber;
                if (codigoRevenda != null)
                    documentosAReceber = pdvAtual.GetDocumentosAReceber(codigoRevenda);
                else
                    documentosAReceber = pdvAtual.GetDocumentosAReceberConsolidado();

                var docsReceberTipoOutros = documentosAReceber.Items.Cast<CSDocumentosReceberPDV.CSDocumentoReceber>().Where(d => d.TIPO_DOCUMENTO == CSDocumentosReceberPDV.CSDocumentoReceber.TipoDocumento.OUTROS);

                foreach (CSDocumentosReceberPDV.CSDocumentoReceber docs in docsReceberTipoOutros)
                {
                    View view = thisLayoutInflater.Inflate(Resource.Layout.documento_receber_listagem_row, null);
                    TextView tvEmpresa = view.FindViewById<TextView>(Resource.Id.tvEmpresa);
                    TextView tvData = view.FindViewById<TextView>(Resource.Id.tvData);
                    TextView tvTotalReceber = view.FindViewById<TextView>(Resource.Id.tvTotalReceber);
                    TextView tvTitulo = view.FindViewById<TextView>(Resource.Id.tvTitulo);
                    TextView tvTotalJuros = view.FindViewById<TextView>(Resource.Id.tvTotalJuros);
                    TextView tvNumeroDoc = view.FindViewById<TextView>(Resource.Id.tvNumeroDoc);
                    TextView tvTotalMulta = view.FindViewById<TextView>(Resource.Id.tvTotalMulta);
                    TextView tvClasse = view.FindViewById<TextView>(Resource.Id.tvClasse);
                    TextView tvTotalEncargos = view.FindViewById<TextView>(Resource.Id.tvTotalEncargos);
                    TextView tvVendedor = view.FindViewById<TextView>(Resource.Id.tvVendedor);
                    TextView tvTotalDesconto = view.FindViewById<TextView>(Resource.Id.tvTotalDesconto);

                    tvData.Text = docs.DAT_VENCIMENTO.ToString("dd/MM/yyyy");
                    tvEmpresa.Text = docs.NOME_EMPRESA.ToUpper();
                    tvNumeroDoc.Text = docs.COD_DOCUMENTO_RECEBER.ToString();
                    tvTitulo.Text = docs.COD_DOCUMENTO_ORIGEM.ToString();
                    tvClasse.Text = docs.DSC_CLASSE_DOCUMENTO_RECEBER.ToString();
                    tvTotalReceber.Text = docs.VALOR_ABERTO.ToString(CSGlobal.DecimalStringFormat);
                    tvTotalJuros.Text = docs.VLR_JUROS.ToString(CSGlobal.DecimalStringFormat);
                    tvTotalMulta.Text = docs.VLR_MULTA.ToString(CSGlobal.DecimalStringFormat);
                    tvTotalEncargos.Text = docs.VLR_ENCARGO.ToString(CSGlobal.DecimalStringFormat);
                    tvTotalDesconto.Text = docs.VLR_DESCONTO.ToString(CSGlobal.DecimalStringFormat);
                    tvVendedor.Text = docs.COD_VENDEDOR.ToString();
                    llRepeater.AddView(view);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Alert(ActivityContext, ex.Message);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.documento_receber_listagem, container, false);
            FindViewsById(view);
            thisLayoutInflater = inflater;
            ActivityContext = Activity;
            return view;
        }

        private void FindViewsById(View view)
        {
            llRepeater = view.FindViewById<LinearLayout>(Resource.Id.llRepeater);
        }
    }
}