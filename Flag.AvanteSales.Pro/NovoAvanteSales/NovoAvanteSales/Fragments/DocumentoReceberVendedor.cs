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

namespace AvanteSales.Pro.Fragments
{
    public class DocumentoReceberVendedor : Android.Support.V4.App.Fragment
    {
        static Android.Support.V4.App.FragmentActivity ActivityContext;
        LinearLayout HeaderListView;
        LayoutInflater thisLayoutInflater;
        ListView listDocumentos;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.documento_receber_vendedor, container, false);
            FindViewsById(view);
            thisLayoutInflater = inflater;
            ActivityContext = Activity;
            return view;
        }

        private void FindViewsById(View view)
        {
            HeaderListView = view.FindViewById<LinearLayout>(Resource.Id.HeaderListView);
            listDocumentos = view.FindViewById<ListView>(Resource.Id.listDocumentos);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            Inicializacao();
        }

        private void Inicializacao()
        {
            View viewHeader = thisLayoutInflater.Inflate(Resource.Layout.documento_receber_vendedor_resumo_header, null);
            HeaderListView.AddView(viewHeader);

            var documentosReceber = CSEmpregados.Current.DOCUMENTOS_RECEBER_EMPREGADO.Items.Cast<CSEmpregados.CSEmpregado.CSDocumentosReceberEmpregados.CSDocumentoReceberEmpregado>().ToList();
            listDocumentos.Adapter = new DocReceberVendedorItemAdapter(ActivityContext, Resource.Layout.documento_receber_vendedor_resumo_row, documentosReceber);
        }

        class DocReceberVendedorItemAdapter : ArrayAdapter<CSEmpregados.CSEmpregado.CSDocumentosReceberEmpregados.CSDocumentoReceberEmpregado>
        {
            Context context;
            IList<CSEmpregados.CSEmpregado.CSDocumentosReceberEmpregados.CSDocumentoReceberEmpregado> docsReceber;
            int resourceId;

            public DocReceberVendedorItemAdapter(Context c, int textViewResourceId, IList<CSEmpregados.CSEmpregado.CSDocumentosReceberEmpregados.CSDocumentoReceberEmpregado> objects)
                : base(c, textViewResourceId, objects)
            {
                context = c;
                docsReceber = objects;
                resourceId = textViewResourceId;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var docs = docsReceber[position];

                LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                View linha = layout.Inflate(resourceId, null);

                if (docs != null)
                {
                    TextView tvTipoDoc = linha.FindViewById<TextView>(Resource.Id.tvTipoDoc);
                    TextView tvQtd = linha.FindViewById<TextView>(Resource.Id.tvQtd);
                    TextView tvValorTotal = linha.FindViewById<TextView>(Resource.Id.tvValorTotal);

                    switch (docs.TIPO_DOCUMENTO)
                    {
                        case CSEmpregados.CSEmpregado.CSDocumentosReceberEmpregados.CSDocumentoReceberEmpregado.TipoDocumento.VENCIDO:
                            tvTipoDoc.Text = "Vencidos";
                            tvQtd.Text = docs.QUANTIDADE.ToString();
                            tvValorTotal.Text = docs.VALOR.ToString(CSGlobal.DecimalStringFormat);
                            break;
                        case CSEmpregados.CSEmpregado.CSDocumentosReceberEmpregados.CSDocumentoReceberEmpregado.TipoDocumento.A_VENCER:
                            tvTipoDoc.Text = "A Vencer";
                            tvQtd.Text = docs.QUANTIDADE.ToString();
                            tvValorTotal.Text = docs.VALOR.ToString(CSGlobal.DecimalStringFormat);
                            break;
                        case CSEmpregados.CSEmpregado.CSDocumentosReceberEmpregados.CSDocumentoReceberEmpregado.TipoDocumento.TOTAL:
                            tvTipoDoc.Text = "Total";
                            tvQtd.Text = docs.TOTAL_QUANTIDADE.ToString();
                            tvValorTotal.Text = docs.TOTAL_VALOR.ToString(CSGlobal.DecimalStringFormat);
                            break;
                    }
                }
                return linha;
            }
        }
    }
}