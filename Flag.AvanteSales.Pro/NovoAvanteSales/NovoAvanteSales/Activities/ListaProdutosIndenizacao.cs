using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AvanteSales.Pro.Controles;
using AvanteSales.Pro.Dialogs;
using AvanteSales.SystemFramework.CSPDV;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "ListaProdutosIndenizacao", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ListaProdutosIndenizacao : AppCompatActivity
    {
        TextView lblCodPdv;
        TextView lblNomePdv;
        Android.Support.V7.Widget.Toolbar tbToolbar;
        private static AppCompatActivity CurrentActivity;
        private int indexProdutoSelecionado;
        ListView lvwProdutos;
        ActivitiesNames UltimaActivity;
        public static List<CSItemsPedido.CSItemPedido> ItemsAdapter;
        LinearLayout HeaderListView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.lista_produtos_indenizacao);

            FindViewsById();

            ListaProdutosIndenizacao.CurrentActivity = this;

            SetSupportActionBar(tbToolbar);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(false);

            UltimaActivity = (ActivitiesNames)Intent.GetIntExtra("ultimaActivity", -1);

            if (UltimaActivity == ActivitiesNames.SimulacaoPreco)
            {
                var view = LayoutInflater.Inflate(Resource.Layout.lista_produto_simulacao_header, null);
                HeaderListView.AddView(view);
            }
            else
            {
                var view = LayoutInflater.Inflate(Resource.Layout.lista_produtos_indenizacao_header, null);
                HeaderListView.AddView(view);
            }
        }

        private void FindViewsById()
        {
            lblCodPdv = FindViewById<TextView>(Resource.Id.lblCodPdv);
            lblNomePdv = FindViewById<TextView>(Resource.Id.lblNomePdv);
            tbToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tbToolbar);
            lvwProdutos = FindViewById<ListView>(Resource.Id.lvwProdutos);
            lvwProdutos.ItemClick += LvwProdutos_ItemClick;
            HeaderListView = FindViewById<LinearLayout>(Resource.Id.HeaderListView);
        }

        private void LvwProdutos_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (UltimaActivity == ActivitiesNames.SimulacaoPreco)
            {
                MessageBox.Alert(this, "Deseja excluir o produto?", "Excluir", (_sender, _e) => { ListView_ItemClick_Excluir_Yes(e.Position); }, "Cancelar", null, true);
            }
            else
            {
                indexProdutoSelecionado = e.Position;

                MessageBox.Alert(this, "Selecione a opção desejada", "Editar", ListView_ItemClick_Editar, "Excluir", ListView_ItemClick_Excluir, true);
            }
        }

        private void ListView_ItemClick_Excluir_Yes(int position)
        {
            CSItemsPedido.CSItemPedido produto = ItemsAdapter[position];
            produto.ATUALIZAR_SALDO_DESCONTO = false;
            produto.STATE = ObjectState.DELETADO;

            ListarProdutos();
        }

        protected override void OnStart()
        {
            base.OnStart();

            ListarProdutos();
        }

        private ActivitiesNames LastActivity()
        {
            try
            {
                return (ActivitiesNames)Intent.GetIntExtra("ultimaActivity", -1);
            }
            catch (NullReferenceException)
            {
                return ActivitiesNames.Nenhum;
            }
        }

        private void CarregaDadosTelaPedido()
        {
            try
            {
                if (!CSGlobal.PedidoSugerido)
                {
                    //LinearLayout llFooter = FindViewById<LinearLayout>(Resource.Id.llFooter);
                    //llFooter.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception ex)
            {
                MessageBox.ShowShortMessageCenter(CurrentActivity, ex.Message);
            }

            ListarProdutos();
        }

        private void ListarProdutos()
        {
            if (LastActivity() != ActivitiesNames.HistoricoIndenizacao &&
                UltimaActivity != ActivitiesNames.SimulacaoPreco)
            {
                var itensIndenizacao = CSProdutos.OrdenarListaProdutosIndenizacao(CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Items.Cast<CSItemsIndenizacao.CSItemIndenizacao>().Where(p => p.STATE != ObjectState.DELETADO).ToList());

                CurrentActivity.RunOnUiThread(() =>
                {
                    lvwProdutos.Adapter = new ProdutosIndenizacaoLitemItemAdapter(CurrentActivity, Resource.Layout.lista_produtos_indenizacao_row, itensIndenizacao);
                });
            }
            else if (UltimaActivity == ActivitiesNames.SimulacaoPreco)
            {
                //LinearLayout llFooter = FindViewById<LinearLayout>(Resource.Id.llFooter);
                //llFooter.Visibility = ViewStates.Gone;

                var itensPedidos = CSProdutos.OrdenarListaProdutosPedido(CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Items.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.STATE != ObjectState.DELETADO).ToList());

                CurrentActivity.RunOnUiThread(() =>
                {
                    ItemsAdapter = itensPedidos.ToList();
                    lvwProdutos.Adapter = new ProdutosPedidoLitemItemAdapter(CurrentActivity, Resource.Layout.lista_produtos_pedido_row, ItemsAdapter);
                });
            }
            else
            {
                var itensHistoricoIndenizacao = CSProdutos.OrdenarListaProdutosHistoricoIndenizacao(CSPDVs.Current.HISTORICO_INDENIZACOES.Current.ITEMS_INDENIZACAO.Items.Cast<CSItemsHistoricoIndenizacao.CSItemHistoricoIndenizacao>().ToList());

                CurrentActivity.RunOnUiThread(() =>
                {
                    lvwProdutos.Adapter = new ProdutosHistoricoIndenizacaoLitemItemAdapter(CurrentActivity, Resource.Layout.lista_produtos_indenizacao_row, itensHistoricoIndenizacao);
                });
            }
        }

        class ProdutosPedidoLitemItemAdapter : ArrayAdapter<CSItemsPedido.CSItemPedido>
        {
            Context context;
            IList<CSItemsPedido.CSItemPedido> pedidos;
            int resourceId;

            public ProdutosPedidoLitemItemAdapter(Context c, int resource, IList<CSItemsPedido.CSItemPedido> objects)
                : base(c, resource, objects)
            {
                context = c;
                pedidos = objects;
                resourceId = resource;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSItemsPedido.CSItemPedido pedido = pedidos[position];

                LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                //View linha = layout.Inflate(resourceId, null);
                if (convertView == null)
                    convertView = layout.Inflate(resourceId, null);

                if (pedido != null)
                {
                    TextView tvDescProduto = convertView.FindViewById<TextView>(Resource.Id.tvDescProduto);
                    TextView tvQtd = convertView.FindViewById<TextView>(Resource.Id.tvQtd);
                    TextView tvPrecoUnitario = convertView.FindViewById<TextView>(Resource.Id.tvPrecoUnitario);
                    TextView tvPctDesc = convertView.FindViewById<TextView>(Resource.Id.tvPctDesc);
                    TextView tvValorTotal = convertView.FindViewById<TextView>(Resource.Id.tvValorTotal);
                    TextView tvUnidadeMedida = convertView.FindViewById<TextView>(Resource.Id.tvUnidadeMedida);
                    ImageView imgProdEspecifico = convertView.FindViewById<ImageView>(Resource.Id.imgProdEspecifico);

                    tvDescProduto.Text = pedido.PRODUTO.DESCRICAO_APELIDO_PRODUTO + " - " + (pedido.PRODUTO.DSC_PRODUTO);

                    if (pedido.PRODUTO.COD_UNIDADE_MEDIDA != "KG" && pedido.PRODUTO.COD_UNIDADE_MEDIDA != "LT")
                    {
                        tvQtd.Text = string.Format("{0}/{1}", pedido.QTD_PEDIDA_INTEIRA.ToString(), pedido.QTD_PEDIDA_UNIDADE.ToString("###000"));
                    }
                    else
                    {
                        tvQtd.Text = pedido.QTD_PEDIDA_INTEIRA.ToString();
                    }

                    tvPctDesc.Text = pedido.PRC_DESCONTO.ToString(CSGlobal.DecimalStringFormat) + "%";

                    tvPrecoUnitario.Text = pedido.VLR_ITEM_UNIDADE.ToString(CSGlobal.DecimalStringFormat);

                    tvValorTotal.Text = pedido.VLR_TOTAL_ITEM.ToString(CSGlobal.DecimalStringFormat);

                    tvUnidadeMedida.Text = pedido.PRODUTO.DSC_UNIDADE_MEDIDA;

                    //bool especificoCategoria = false;

                    imgProdEspecifico.Visibility = ViewStates.Gone;
                }
                return convertView;
            }
        }

        protected void ListView_ItemClick_Editar(object sender, DialogClickEventArgs e)
        {
            CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current = (CSItemsIndenizacao.CSItemIndenizacao)lvwProdutos.Adapter.GetItem(indexProdutoSelecionado);
            CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.PRODUTO = CSProdutos.GetProduto(CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.COD_PRODUTO);
            CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.STATE = ObjectState.ALTERADO;

            Intent i = new Intent();
            i.SetClass(this, typeof(ProdutoIndenizacao));
            i.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
            this.StartActivity(i);
            //StartActivity(new Intent(this, new ProdutoIndenizacao().Class));
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        protected void ListView_ItemClick_Excluir(object sender, DialogClickEventArgs e)
        {
            CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current = (CSItemsIndenizacao.CSItemIndenizacao)lvwProdutos.Adapter.GetItem(indexProdutoSelecionado);
            CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.STATE = ObjectState.DELETADO;
            CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.DeletarImagem();
            CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.DeletarFlush();
            ListarProdutos();
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

                    //tvQtd.Text = (indenizacao.QTD_INDENIZACAO / indenizacao.PRODUTO.QTD_UNIDADE_EMBALAGEM).ToString();

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

        class ProdutosHistoricoIndenizacaoLitemItemAdapter : ArrayAdapter<CSItemsHistoricoIndenizacao.CSItemHistoricoIndenizacao>
        {
            Context context;
            IList<CSItemsHistoricoIndenizacao.CSItemHistoricoIndenizacao> indenizacoes;
            int resourceId;

            public ProdutosHistoricoIndenizacaoLitemItemAdapter(Context c, int resource, IList<CSItemsHistoricoIndenizacao.CSItemHistoricoIndenizacao> objects)
                : base(c, resource, objects)
            {
                context = c;
                indenizacoes = objects;
                resourceId = resource;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSItemsHistoricoIndenizacao.CSItemHistoricoIndenizacao indenizacao = indenizacoes[position];

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