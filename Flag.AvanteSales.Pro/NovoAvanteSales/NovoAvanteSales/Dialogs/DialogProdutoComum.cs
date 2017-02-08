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
using AvanteSales.BusinessRules;

namespace AvanteSales.Pro.Dialogs
{
    [Activity(Theme = "@style/AvanteSales.Theme.Dialogs", ScreenOrientation = ScreenOrientation.Portrait)]
    public class DialogProdutoComum : Activity
    {
        private TextView tvDescApelidoProduto;
        private TextView tvDescGrupo;
        private TextView tvFamilia;
        private TextView lblDetalheProduto;
        private TextView tvDetalheProduto;
        private static TextView tvVlrUnitario;
        private static Button btnAdicionar;
        private Button btnConsultaUltimosPedidos;
        private TextView lblNomeProduto;

        private static Activity CurrentActivity;
        private static bool sairDaTelaQuandoAcabarDeCarregar;

        private static decimal precoProduto
        {
            set
            {
                if (value == -1)
                {
                    btnAdicionar.Enabled = false;
                    btnAdicionar.Visibility = ViewStates.Gone;
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
                    if (btnAdicionar.Text == "Iniciando Venda")
                    {
                        btnAdicionar_Click(null, null);
                    }
                    btnAdicionar.Text = "Adicionar";
                }
                if (sairDaTelaQuandoAcabarDeCarregar)
                {
                    CurrentActivity.OnBackPressed();
                }
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetContentView(Resource.Layout.dialog_produto_comum);

                FindViewsById();

                CurrentActivity = this;
                sairDaTelaQuandoAcabarDeCarregar = false;
                DialogProdutoComum.precoProduto = -2;

                CarregaDadosTela();
            }
            catch (Exception ex)
            {
                if (ex.Message == "Falha ao buscar detalhe do produto")
                {
                    try
                    {
                        CarregaDadosTela();
                    }
                    catch (Exception ex2)
                    {
                        tvVlrUnitario.Text = "Ocorreu um erro inesperado. Tente novamente.";
                        MessageBox.AlertErro(this, ex2.Message);
                    }
                }
                else
                {
                    tvVlrUnitario.Text = "Ocorreu um erro inesperado. Tente novamente.";
                    MessageBox.AlertErro(this, ex.Message);
                }
            }
        }

        private void CarregaDadosTela()
        {
            Title = CSProdutos.Current.DSC_PRODUTO;
            lblNomeProduto.Text = CSProdutos.Current.DSC_PRODUTO;

            if (IsBroker())
            {
                btnAdicionar.Text = "Calculando R$...";
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
                btnAdicionar.Text = "Calculando R$...";
                tvVlrUnitario.Text = "Buscando valor do produto...";

                new BuscandoPrecoProdutoBunge().Execute("");
            }
            else
            {
                tvVlrUnitario.Text = CalculaPrecoDoProduto().ToString(CSGlobal.DecimalStringFormat);
                tvVlrUnitario.TextSize = 21;
            }

            if (Intent.GetBooleanExtra("ultimasVisitasHabilitada", false))
                btnConsultaUltimosPedidos.Click += new EventHandler(btnConsultaUltimosPedidos_Click);
            else
                btnConsultaUltimosPedidos.Visibility = ViewStates.Gone;

            if (Intent.GetBooleanExtra("vendaHabilidada", false))
                btnAdicionar.Click += new EventHandler(btnAdicionar_Click);
            else
                btnAdicionar.Visibility = ViewStates.Gone;

            tvDescApelidoProduto.Text = CSProdutos.Current.DESCRICAO_APELIDO_PRODUTO;
            tvDescGrupo.Text = CSProdutos.Current.GRUPO.DSC_GRUPO;
            tvFamilia.Text = CSProdutos.Current.FAMILIA_PRODUTO.DSC_FAMILIA_PRODUTO;
        }

        private bool IsBunge()
        {
            return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 3;
        }

        private void FindViewsById()
        {
            tvDescApelidoProduto = FindViewById<TextView>(Resource.Id.tvDescApelidoProduto);
            tvDescGrupo = FindViewById<TextView>(Resource.Id.tvDescGrupo);
            tvFamilia = FindViewById<TextView>(Resource.Id.tvFamilia);
            lblDetalheProduto = FindViewById<TextView>(Resource.Id.lblDetalheProduto);
            tvDetalheProduto = FindViewById<TextView>(Resource.Id.tvDetalheProduto);
            tvVlrUnitario = FindViewById<TextView>(Resource.Id.tvVlrUnitario);
            btnAdicionar = FindViewById<Button>(Resource.Id.btnAdicionar);
            btnConsultaUltimosPedidos = FindViewById<Button>(Resource.Id.btnConsultaUltimosPedidos);
            lblNomeProduto = FindViewById<TextView>(Resource.Id.lblNomeProduto);
        }

        private bool IsBroker()
        {
            return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
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

        static void btnAdicionar_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnAdicionar.Text == "Calculando R$..." && tvVlrUnitario.Text == "Buscando valor do produto...")
                {
                    btnAdicionar.Text = "Iniciando Venda";
                }
                else
                {
                    CurrentActivity.SetResult(Result.Ok);
                    CurrentActivity.Finish();
                }
            }
            catch (Exception ex)
            {
                MessageBox.AlertErro(CurrentActivity, ex.Message);
            }
        }

        void btnConsultaUltimosPedidos_Click(object sender, EventArgs e)
        {
            SetResult(Result.FirstUser);
            Finish();
        }

        public override void OnBackPressed()
        {
            try
            {
                if (tvVlrUnitario.Text == "Buscando valor do produto...")
                {
                    sairDaTelaQuandoAcabarDeCarregar = true;
                }
                else
                {
                    base.OnBackPressed();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
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
                DialogProdutoComum.precoProduto = result;
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
                DialogProdutoComum.precoProduto = result;

                if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current != null &&
                    btnAdicionar.Visibility == ViewStates.Visible)
                    btnAdicionar.Text = "Indenizar";
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
}