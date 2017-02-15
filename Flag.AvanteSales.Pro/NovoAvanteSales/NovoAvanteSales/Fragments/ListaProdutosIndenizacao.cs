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
    public class ListaProdutosIndenizacao : Android.Support.V4.App.Fragment
    {
        //private int indexProdutoSelecionado;
        ListView lvwProdutos;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.lista_produtos_indenizacao, container, false);
            FindViewsById(view);
            Eventos();
            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ListarProdutos();
        }

        private void ListarProdutos()
        {
            var itensIndenizacao = CSProdutos.OrdenarListaProdutosIndenizacao(CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Items.Cast<CSItemsIndenizacao.CSItemIndenizacao>().Where(p => p.STATE != ObjectState.DELETADO).ToList());

            lvwProdutos.Adapter = new ProdutosIndenizacaoLitemItemAdapter(Activity, Resource.Layout.lista_produtos_indenizacao_row, itensIndenizacao);
        }

        private void Eventos()
        {

        }

        private void FindViewsById(View view)
        {
            lvwProdutos = view.FindViewById<ListView>(Resource.Id.lvwProdutos);
        }

        class ProdutosIndenizacaoLitemItemAdapter : ArrayAdapter<CSItemsIndenizacao.CSItemIndenizacao>
        {
            Context context;
            IList<CSItemsIndenizacao.CSItemIndenizacao> indenizacoes;
            int resourceId;

            public ProdutosIndenizacaoLitemItemAdapter(Context c, int resource, IList<CSItemsIndenizacao.CSItemIndenizacao> objects)
                : base(c, resource, objects)
            {
                context = c;
                indenizacoes = objects;
                resourceId = resource;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSItemsIndenizacao.CSItemIndenizacao indenizacao = indenizacoes[position];

                LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                //View linha = layout.Inflate(resourceId, null);

                if (convertView == null)
                    convertView = layout.Inflate(resourceId, null);

                if (indenizacao != null)
                {
                    TextView tvDescProduto = convertView.FindViewById<TextView>(Resource.Id.tvDescProduto);
                    TextView tvTotal = convertView.FindViewById<TextView>(Resource.Id.tvTotal);
                    TextView tvTaxa = convertView.FindViewById<TextView>(Resource.Id.tvTaxa);
                    TextView tvQtd = convertView.FindViewById<TextView>(Resource.Id.tvQtd);

                    tvDescProduto.Text = indenizacao.PRODUTO.DESCRICAO_APELIDO_PRODUTO + " - " + indenizacao.PRODUTO.DSC_APELIDO_PRODUTO;

                    tvTotal.Text = indenizacao.VLR_INDENIZACAO.ToString(CSGlobal.DecimalStringFormat);

                    tvTaxa.Text = indenizacao.PCT_TAXA_INDENIZACAO.ToString();

                    if (indenizacao.PRODUTO.COD_UNIDADE_MEDIDA != "KG" && indenizacao.PRODUTO.COD_UNIDADE_MEDIDA != "LT")
                    {
                        tvQtd.Text = string.Format("{0}/{1}", indenizacao.QTD_INDENIZACAO_INTEIRA.ToString(), indenizacao.QTD_INDENIZACAO_UNIDADE.ToString("###000"));
                    }
                    else
                    {
                        tvQtd.Text = indenizacao.QTD_INDENIZACAO_INTEIRA.ToString();
                    }
                }
                return convertView;
            }
        }
    }
}