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
    public class ConsultaProdutoPedidos : Android.Support.V4.App.Fragment
    {
        LayoutInflater thisLayoutInflater;
        TextView tvPedido;
        ListView lvwProdutoPedido;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.consulta_produto_pedido, container, false);
            thisLayoutInflater = inflater;
            FindViewsById(view);
            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            MostraPedidos();

            base.OnViewCreated(view, savedInstanceState);
        }

        private void FindViewsById(View view)
        {
            tvPedido = view.FindViewById<TextView>(Resource.Id.tvPedido);
            lvwProdutoPedido = view.FindViewById<ListView>(Resource.Id.lvwProdutoPedido);
        }

        private void MostraPedidos()
        {
            // Mostra informação do produto.
            tvPedido.Text = CSProdutos.Current.DESCRICAO_APELIDO_PRODUTO + "-" + CSProdutos.Current.DSC_PRODUTO + " " + CSProdutos.Current.DSC_UNIDADE_MEDIDA + " (" + CSProdutos.Current.GRUPO_COMERCIALIZACAO.DES_GRUPO_COMERCIALIZACAO + ")";

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

                lviitempedido.Valor = itempedido;
                listPedidoProduto.Add(lviitempedido);

                lvwProdutoPedido.Adapter = new ListarProdudoPedido(Activity, Resource.Layout.consulta_produto_pedido_row, listPedidoProduto);

                CSPDVs.Current.ULTIMAS_VISITAS.Current = null;
            }
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