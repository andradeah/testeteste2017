using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AvanteSales.BusinessRules;
using AvanteSales.Pro.Activities;

namespace AvanteSales.Pro.Dialogs
{
    public class DialogFragmentProdutoImagem : Android.Support.V4.App.DialogFragment
    {
        Cliente cliente;
        static Android.Support.V4.App.FragmentActivity ActivityContext;
        bool ultimasVisitasHabilitada;
        bool vendaHabilidada;
        bool vendaIndicados;
        private TextView tvDescApelidoProduto;
        private TextView tvDescGrupo;
        private TextView tvFamilia;
        private TextView lblDetalheProduto;
        private TextView tvDetalheProduto;
        private TextView lblNomeProduto;
        private static TextView tvVlrUnitario;
        private static Button btnVender;
        private Button btnConsultaUltimosPedidos;
        private LinearLayout llImagem;
        private ImageView imgIconeProduto;
        private bool ProdutoVendido;
        private static decimal precoProduto
        {
            set
            {
                if (value == -1)
                {
                    btnVender.Enabled = false;
                    btnVender.Visibility = ViewStates.Gone;
                    tvVlrUnitario.Text = "Falha no cálculo do valor.";
                }
                else if (value == -2)
                {
                    tvVlrUnitario.Text = "Buscando valor do produto...";
                }
                else
                {
                    tvVlrUnitario.Text = value.ToString(CSGlobal.DecimalStringFormat);
                    tvVlrUnitario.TextSize = 21;
                    btnVender.Text = "Adicionar";
                }
            }
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.dialog_produto_imagem, container, false);
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);
            FindViewsById(view);
            ActivityContext = ((Cliente)Activity);
            cliente = (Cliente)Activity;
            ultimasVisitasHabilitada = Arguments.GetBoolean("ultimasVisitasHabilitada");
            vendaHabilidada = Arguments.GetBoolean("vendaHabilidada");
            vendaIndicados = Arguments.GetBoolean("vendaIndicados");
            ProdutoVendido = Arguments.GetBoolean("ProdutoVendido");
            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            Inicializacao();
        }

        private bool IsBroker()
        {
            return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
        }

        private bool IsBunge()
        {
            return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 3;
        }

        private void Inicializacao()
        {
            lblNomeProduto.Text = CSProdutos.Current.DSC_PRODUTO;

            if (IsBroker())
            {
                btnVender.Text = "Calculando R$...";
                tvVlrUnitario.Text = "Buscando valor do produto...";
                var detalheDoProduto = CSProdutos.Current.DetalheDoProduto();
                if (!string.IsNullOrEmpty(detalheDoProduto))
                {
                    tvDetalheProduto.Text = detalheDoProduto;
                    tvDetalheProduto.Visibility = ViewStates.Visible;
                    lblDetalheProduto.Visibility = ViewStates.Visible;
                }
                new BuscandoPrecoProduto().Execute("");
            }
            else if (IsBunge())
            {
                btnVender.Text = "Calculando R$...";
                tvVlrUnitario.Text = "Buscando valor do produto...";

                new BuscandoPrecoProdutoBunge().Execute("");
            }
            else
            {
                var preco = CalculaPrecoDoProduto();

                tvVlrUnitario.Text = preco.ToString(CSGlobal.DecimalStringFormat);
                tvVlrUnitario.TextSize = 21;

                if (preco <= 0)
                {
                    btnVender.Visibility = ViewStates.Gone;
                }
            }

            if (ultimasVisitasHabilitada)
                btnConsultaUltimosPedidos.Click += BtnConsultaUltimosPedidos_Click;
            else
                btnConsultaUltimosPedidos.Visibility = ViewStates.Gone;

            if (vendaHabilidada)
                btnVender.Click += BtnVender_Click;
            else
                btnVender.Visibility = ViewStates.Gone;

            tvDescApelidoProduto.Text = CSProdutos.Current.DESCRICAO_APELIDO_PRODUTO;
            tvDescGrupo.Text = CSProdutos.Current.GRUPO.DSC_GRUPO;
            tvFamilia.Text = CSProdutos.Current.FAMILIA_PRODUTO.DSC_FAMILIA_PRODUTO;

            var file = CSGlobal.ArquivoImagem(CSProdutos.Current);

            if (file != null)
            {
                LinearLayout.LayoutParams parametros = (LinearLayout.LayoutParams)llImagem.LayoutParameters;
                parametros.Gravity = GravityFlags.CenterHorizontal;
                llImagem.LayoutParameters = parametros;
                Android.Graphics.Bitmap btmp = file.LoadAndResizeBitmap(270, 270);
                imgIconeProduto.SetImageBitmap(btmp);
            }
            else
            {
                llImagem.SetGravity(GravityFlags.Left);
                imgIconeProduto.SetImageResource(Resource.Drawable.menu_selector_sobre);
                imgIconeProduto.LayoutParameters.Width = 50;
                imgIconeProduto.LayoutParameters.Height = 50;
            }

            imgIconeProduto.Click += ImgIconeProduto_Click;
        }

        private void ImgIconeProduto_Click(object sender, EventArgs e)
        {
            try
            {
                Intent i = new Intent();
                i.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                i.SetClass(Activity, typeof(DialogInformacaoProduto));
                this.StartActivity(i);
            }
            catch (Exception ex)
            {

            }
        }

        private void BtnConsultaUltimosPedidos_Click(object sender, EventArgs e)
        {
            cliente.UltimosPedidos();
            this.Dismiss();
        }

        private void BtnVender_Click(object sender, EventArgs e)
        {
            if (!ProdutoVendido)
            {
                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current = new CSItemsPedido.CSItemPedido();
                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRODUTO = CSProdutos.Current;
                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE = ObjectState.NOVO;
            }

            if (vendaIndicados)
                cliente.NavegarVendaProdutoIndicado();
            else
                cliente.ProximoPasso(true);

            this.Dismiss();
        }

        private void FindViewsById(View view)
        {
            tvDescApelidoProduto = view.FindViewById<TextView>(Resource.Id.tvDescApelidoProduto);
            tvDescGrupo = view.FindViewById<TextView>(Resource.Id.tvDescGrupo);
            tvFamilia = view.FindViewById<TextView>(Resource.Id.tvFamilia);
            lblDetalheProduto = view.FindViewById<TextView>(Resource.Id.lblDetalheProduto);
            tvDetalheProduto = view.FindViewById<TextView>(Resource.Id.tvDetalheProduto);
            tvVlrUnitario = view.FindViewById<TextView>(Resource.Id.tvVlrUnitario);
            lblNomeProduto = view.FindViewById<TextView>(Resource.Id.lblNomeProduto);
            btnVender = view.FindViewById<Button>(Resource.Id.btnVender);
            btnConsultaUltimosPedidos = view.FindViewById<Button>(Resource.Id.btnConsultaUltimosPedidos);
            imgIconeProduto = view.FindViewById<ImageView>(Resource.Id.imgIconeProduto);
            llImagem = view.FindViewById<LinearLayout>(Resource.Id.llImagem);
        }

        private decimal CalculaPrecoDoProduto()
        {
            CSProdutos.CSProduto prod = CSProdutos.Current;
            // [ Busca tabela de preço padrão do pdv ]
            prod.PRECOS_PRODUTO.Current = prod.PRECOS_PRODUTO.Cast<CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto>()
                .Where(p => p.COD_TABELA_PRECO == CSPDVs.Current.COD_TABPRECO_PADRAO).FirstOrDefault();

            // Seta a primeira tabela de preço como default para mostrar o preço final
            if (prod.PRECOS_PRODUTO.Current == null)
            {
                if (prod.PRECOS_PRODUTO.Items.Count > 0)
                {
                    prod.PRECOS_PRODUTO.Current = prod.PRECOS_PRODUTO.Items[0];
                    return prod.PRECOS_PRODUTO.Current.VLR_PRODUTO;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return prod.PRECOS_PRODUTO.Current.VLR_PRODUTO;
            }
        }

        private class BuscandoPrecoProdutoBunge : AsyncTask<string, object, decimal>
        {
            protected override decimal RunInBackground(params string[] @params)
            {
                try
                {
                    return CalculaPrecoDoProdutoBunge();
                }
                catch (Exception)
                {
                    return -1;
                }
            }

            protected override void OnPostExecute(decimal result)
            {
                precoProduto = result;
            }

            private decimal CalculaPrecoDoProdutoBunge()
            {
                CSPoliticaBunge pricingBunge = new CSPoliticaBunge(CSProdutos.Current.COD_PRODUTO, CSEmpresa.Current.COD_NOTEBOOK1, CSPDVs.Current.COD_PDV, DateTime.Now, CSProdutos.Current, 1, 0, 0m, null);
                pricingBunge.ValorFinal();
                var preco = pricingBunge.ValorFinalProduto;

                return preco;
            }
        }

        private class BuscandoPrecoProduto : AsyncTask<string, object, decimal>
        {
            protected override decimal RunInBackground(params string[] @params)
            {
                try
                {
                    return CalculaPrecoDoProdutoBroker();
                }
                catch (Exception)
                {
                    return -1;
                }
            }

            protected override void OnPostExecute(decimal result)
            {
                precoProduto = result;

                if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current != null &&
                    btnVender.Visibility == ViewStates.Visible)
                    btnVender.Text = "Indenizar";
            }

            private decimal CalculaPrecoDoProdutoBroker()
            {
                CSProdutos.CSProduto prod = CSProdutos.Current;

                decimal precoBroker;

                if (CSEmpresa.Current.IND_UTILIZA_PRICE_2014)
                {
                    var valorBroker = CSPDVs.Current.POLITICA_BROKER_2014.CalculaPreco(prod.COD_PRODUTO, prod.GRUPO_COMERCIALIZACAO.COD_SETOR_BROKER, 1, 0, 0, prod.QTD_UNIDADE_EMBALAGEM);
                    precoBroker = valorBroker[valorBroker.Length - 1].VALOR;
                }
                else
                {
                    var valorBroker = CSPDVs.Current.POLITICA_BROKER.CalculaPreco(prod.COD_PRODUTO, prod.GRUPO_COMERCIALIZACAO.COD_SETOR_BROKER, 1, 0, 0, prod.QTD_UNIDADE_EMBALAGEM);
                    precoBroker = valorBroker[valorBroker.Length - 1].VALOR;
                }

                return precoBroker;
            }
        }
    }

    public static class BitmapHelpers
    {
        public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height)
        {
            try
            {
                BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
                BitmapFactory.DecodeFile(fileName, options);

                int outHeight = options.OutHeight;
                int outWidth = options.OutWidth;
                int inSampleSize = 1;

                if (outHeight > height || outWidth > width)
                {
                    inSampleSize = outWidth > outHeight
                                       ? outHeight / height
                                       : outWidth / width;
                }

                options.InJustDecodeBounds = false;
                Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

                return resizedBitmap;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}