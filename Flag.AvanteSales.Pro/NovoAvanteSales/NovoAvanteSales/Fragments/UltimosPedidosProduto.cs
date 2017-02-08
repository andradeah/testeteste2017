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
using AvanteSales.SystemFramework;

namespace AvanteSales.Pro.Fragments
{
    public class UltimosPedidosProduto : Android.Support.V4.App.DialogFragment
    {
        TextView tvPedido;
        ListView lvwProdutoPedido;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.ultimos_pedidos_produto, container, false);
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);
            FindViewsById(view);

            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            MostraPedidos();
        }

        private void MostraPedidos()
        {
            var produtoAtual = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRODUTO;

            // Mostra informação do produto.
            tvPedido.Text = produtoAtual.DESCRICAO_APELIDO_PRODUTO + "-" + produtoAtual.DSC_PRODUTO + " " + produtoAtual.DSC_UNIDADE_MEDIDA + " (" + produtoAtual.GRUPO_COMERCIALIZACAO.DES_GRUPO_COMERCIALIZACAO + ")";

            List<CSListViewItem> listPedidoProduto = new List<CSListViewItem>();

            // Lista os pedidos existentes do PDV                                                  
            foreach (CSUltimasVisitasPDV.CSUltimaVisitaPDV pedido in CSPDVs.Current.ULTIMAS_VISITAS.Items)
            {
                // Seta qual é o pedido atual
                CSPDVs.Current.ULTIMAS_VISITAS.Current = pedido;

                CSItemsPedido.CSItemPedido itempedido = CSPDVs.Current.ULTIMAS_VISITAS.Current.ITEMS_PEDIDOS.Items.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.PRODUTO.COD_PRODUTO == CSProdutos.Current.COD_PRODUTO).FirstOrDefault();
                if (itempedido == null)
                    continue;

                CSListViewItem lviitempedido = new CSListViewItem();

                lviitempedido.Text = pedido.DAT_PEDIDO.ToString("dd/MM/yyyy");
                lviitempedido.SubItems = new List<object>();
                lviitempedido.SubItems.Add(pedido.COD_PEDIDO.ToString());
                lviitempedido.SubItems.Add(CSProdutos.CSProduto.ConverteUnidadesParaMedida(itempedido.QTD_PEDIDA_TOTAL, itempedido.PRODUTO.COD_UNIDADE_MEDIDA, itempedido.PRODUTO.QTD_UNIDADE_EMBALAGEM));
                lviitempedido.SubItems.Add(itempedido.VLR_ITEM_UNIDADE.ToString(CSGlobal.DecimalStringFormat));
                lviitempedido.SubItems.Add(itempedido.VLR_TOTAL_ITEM.ToString(CSGlobal.DecimalStringFormat));

                // Guarda a instancia do pedido no listview
                lviitempedido.Valor = itempedido;
                listPedidoProduto.Add(lviitempedido);



                lvwProdutoPedido.Adapter = new ListarProdudoPedido(Activity, Resource.Layout.ultimos_pedidos_produto_row, listPedidoProduto);

                // Coloca o pedido atual como nenhum
                CSPDVs.Current.ULTIMAS_VISITAS.Current = null;

            }
        }

        private void FindViewsById(View view)
        {
            tvPedido = view.FindViewById<TextView>(Resource.Id.tvPedido);
            lvwProdutoPedido = view.FindViewById<ListView>(Resource.Id.lvwProdutoPedido);
        }

        class ListarProdudoPedido : ArrayAdapter<CSListViewItem>
        {
            Activity act;
            IList<CSListViewItem> ProdutoPedido;
            int resourceId;

            public ListarProdudoPedido(Activity c, int textViewResourceId, IList<CSListViewItem> objects)
                : base(c, textViewResourceId, objects)
            {
                act = c;
                ProdutoPedido = objects;
                resourceId = textViewResourceId;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSListViewItem item = ProdutoPedido[position];

                LayoutInflater layout = (LayoutInflater)act.GetSystemService(Context.LayoutInflaterService);
                //View linha = layout.Inflate(resourceId, null);

                try
                {
                    if (convertView == null)
                        convertView = layout.Inflate(resourceId, null);

                    TextView tvDataCodPedido = convertView.FindViewById<TextView>(Resource.Id.tvDataCodPedido);
                    TextView tvQuantidade = convertView.FindViewById<TextView>(Resource.Id.tvQuantidade);
                    TextView tvValorUnitario = convertView.FindViewById<TextView>(Resource.Id.tvValorUnitario);
                    TextView tvValorTotal = convertView.FindViewById<TextView>(Resource.Id.tvValorTotal);

                    tvDataCodPedido.Text = item.Text + " - " + item.SubItems[0];
                    tvQuantidade.Text = item.SubItems[1].ToString();
                    tvValorUnitario.Text = item.SubItems[2].ToString();
                    tvValorTotal.Text = item.SubItems[3].ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.AlertErro(act, ex.Message);
                }

                return convertView;
            }
        }
    }
}