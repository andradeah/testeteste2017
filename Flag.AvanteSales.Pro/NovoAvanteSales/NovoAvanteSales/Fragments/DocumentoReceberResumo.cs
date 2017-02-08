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
using AvanteSales.Pro.Activities;
using AvanteSales.Pro.Dialogs;

namespace AvanteSales.Pro.Fragments
{
    public class DocumentoReceberResumo : Android.Support.V4.App.Fragment
    {
        LinearLayout HeaderListView;
        Android.Support.V4.App.FragmentActivity ActivityContext;
        LayoutInflater thisLayoutInflater;
        ListView listResumo;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            Inicializacao();
        }

        private void Inicializacao()
        {
            View view = thisLayoutInflater.Inflate(Resource.Layout.documento_receber_resumo_header, null);
            HeaderListView.AddView(view);
            ActivityContext = Activity;
            CarregarDados();
        }

        private void CarregarDados()
        {
            try
            {
                string codigoRevenda = CSGlobal.COD_REVENDA;

                CSPDVs.CSPDV pdvAtual = CSPDVs.Current;

                // [ Recupera documentos da empresa ]
                CSDocumentosReceberPDV documentosAReceber;
                if (codigoRevenda != null)
                    documentosAReceber = pdvAtual.GetDocumentosAReceber(codigoRevenda);
                else
                    documentosAReceber = pdvAtual.GetDocumentosAReceberConsolidado();

                var documentos = documentosAReceber.Items.Cast<CSDocumentosReceberPDV.CSDocumentoReceber>().ToList();
                listResumo.Adapter = new DocReceberVendedorResumoItemAdapter(ActivityContext, Resource.Layout.documento_receber_resumo_row, documentos);
            }
            catch (Exception ex)
            {
                MessageBox.AlertErro(ActivityContext, ex.Message);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.documento_receber_resumo, container, false);
            FindViewsById(view);
            thisLayoutInflater = inflater;
            return view;
        }

        private void FindViewsById(View view)
        {
            HeaderListView = view.FindViewById<LinearLayout>(Resource.Id.HeaderListView);
            listResumo = view.FindViewById<ListView>(Resource.Id.listResumo);
        }

        class DocReceberVendedorResumoItemAdapter : ArrayAdapter<CSDocumentosReceberPDV.CSDocumentoReceber>
        {
            Context context;
            IList<CSDocumentosReceberPDV.CSDocumentoReceber> documentosReceber;
            int resourceId;

            public DocReceberVendedorResumoItemAdapter(Context c, int textViewResourceId, IList<CSDocumentosReceberPDV.CSDocumentoReceber> objects)
                : base(c, textViewResourceId, objects)
            {
                context = c;
                documentosReceber = objects;
                resourceId = textViewResourceId;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var docs = documentosReceber[position];

                LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                View linha = layout.Inflate(resourceId, null);

                if (docs != null)
                {
                    TextView tvTipoDoc = linha.FindViewById<TextView>(Resource.Id.tvTipoDoc);
                    TextView tvQtd = linha.FindViewById<TextView>(Resource.Id.tvQtd);
                    TextView tvValorTotal = linha.FindViewById<TextView>(Resource.Id.tvValorTotal);

                    switch (docs.TIPO_DOCUMENTO)
                    {
                        case CSDocumentosReceberPDV.CSDocumentoReceber.TipoDocumento.VENCIDO:
                            tvTipoDoc.Text = "Vencidos";
                            tvQtd.Text = docs.QUANTIDADE.ToString();
                            tvValorTotal.Text = docs.VALOR.ToString(CSGlobal.DecimalStringFormat);
                            break;

                        case CSDocumentosReceberPDV.CSDocumentoReceber.TipoDocumento.A_VENCER:
                            tvTipoDoc.Text = "A Vencer";
                            tvQtd.Text = docs.QUANTIDADE.ToString();
                            tvValorTotal.Text = docs.VALOR.ToString(CSGlobal.DecimalStringFormat);
                            break;

                        case CSDocumentosReceberPDV.CSDocumentoReceber.TipoDocumento.TOTAL:
                            tvTipoDoc.Text = "Total";
                            tvQtd.Text = docs.TOTAL_QUANTIDADE.ToString();
                            tvValorTotal.Text = docs.TOTAL_VALOR.ToString(CSGlobal.DecimalStringFormat);
                            break;

                        case CSDocumentosReceberPDV.CSDocumentoReceber.TipoDocumento.LIMITE_CREDITO:
                            tvTipoDoc.Text = "L. Crédito";
                            tvQtd.Text = docs.QUANTIDADE.ToString();
                            tvValorTotal.Text = CSPDVs.Current.VLR_LIMITE_CREDITO.ToString(CSGlobal.DecimalStringFormat);
                            break;
                        case CSDocumentosReceberPDV.CSDocumentoReceber.TipoDocumento.PEDIDOS:
                            tvTipoDoc.Text = "Pedidos do dia";
                            tvQtd.Text = string.Empty;
                            tvValorTotal.Text = docs.VALOR.ToString(CSGlobal.DecimalStringFormat);
                            break;
                        case CSDocumentosReceberPDV.CSDocumentoReceber.TipoDocumento.PEDIDOS_DESCARREGADOS:
                            tvTipoDoc.Text = "Pedidos descarregados";
                            tvQtd.Text = string.Empty;
                            tvValorTotal.Text = docs.VALOR.ToString(CSGlobal.DecimalStringFormat);
                            break;
                        case CSDocumentosReceberPDV.CSDocumentoReceber.TipoDocumento.SALDO:
                            tvTipoDoc.Text = "Saldo Crédito";
                            tvQtd.Text = docs.QUANTIDADE.ToString();
                            tvValorTotal.Text = CSPDVs.Current.VLR_SALDO_CREDITO_ATUALIZADO.ToString(CSGlobal.DecimalStringFormat);
                            break;
                    }
                }
                return linha;
            }
        }
    }
}