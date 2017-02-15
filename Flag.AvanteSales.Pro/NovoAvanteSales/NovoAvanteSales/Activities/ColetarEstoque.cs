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
using AvanteSales.SystemFramework;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "ColetarEstoque", ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AvanteSalesTheme")]
    public class ColetarEstoque : AppCompatActivity
    {
        TextView lblCodPdv;
        TextView lblNomePdv;
        Android.Support.V7.Widget.Toolbar tbToolbar;
        private TextView lblProduto;
        private EditText txtProduto;
        private Button btnProduto;
        private EditText txtQtdeInteiro;
        private EditText txtQtdeUnidade;
        private Button btnColetar;
        private Button btnPesquisar;
        private TextView lblQtdeInteiro;
        private TextView lblQtdeUnidade;
        //private TextView lblPerdaInteira;
        //private EditText txtPerdaInteira;
        //private TextView lblPerdaUnidade;
        //private EditText txtPerdaUnidade;
        //private CheckBox chkPerda;
        //private CheckBox chkOcultar;
        private ListView lvwEstoque;
        private CSProdutos.CSProduto produtoAtual = null;
        private static CSColetaEstoquePDV coletaEstoquePDV = null;
        public string codigoProdutoSelecionado = null;
        private const int frmProcuraProduto = 0;
        private int Position = 0;
        private static ProgressDialog progressDialog;
        private static AppCompatActivity CurrentActivity;
        public static string UltimaTela = string.Empty;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CurrentActivity = this;

            SetContentView(Resource.Layout.coletar_estoque);

            FindViewsByIds();

            lblCodPdv.Text = CSPDVs.Current.COD_PDV.ToString();
            lblNomePdv.Text = CSPDVs.Current.DSC_RAZAO_SOCIAL;

            AdicionarEventos();

            lblProduto.Visibility = ViewStates.Gone;

            coletaEstoquePDV = null;

            UltimaTela = "Outras";

            coletaEstoquePDV = new CSColetaEstoquePDV(CSPDVs.Current.COD_PDV);

            if (coletaEstoquePDV.Count == 0)
                CSPDVs.Current.COLETA_OBRIGATORIA = false;

            txtQtdeInteiro.Visibility = ViewStates.Gone;
            txtQtdeUnidade.Visibility = ViewStates.Gone;
            lblQtdeInteiro.Visibility = ViewStates.Gone;
            lblQtdeUnidade.Visibility = ViewStates.Gone;
            btnColetar.Visibility = ViewStates.Gone;

            ListaDados();

            codigoProdutoSelecionado = Intent.GetStringExtra("codigoProdutoSelecionado");

            // [Quando é selecionado algum produto já vem selecionado o mesmo para a coleta]
            if (!string.IsNullOrEmpty(codigoProdutoSelecionado))
            {
                txtProduto.Text = codigoProdutoSelecionado.Trim();
                ProcuraProdutoColeta(true);
                txtQtdeInteiro.RequestFocus();
            }

            SetSupportActionBar(tbToolbar);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
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

        private void FindViewsByIds()
        {
            lblCodPdv = FindViewById<TextView>(Resource.Id.lblCodPdv);
            lblNomePdv = FindViewById<TextView>(Resource.Id.lblNomePdv);
            tbToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tbToolbar);
            lvwEstoque = FindViewById<ListView>(Resource.Id.lvwEstoque);
            lblProduto = FindViewById<TextView>(Resource.Id.lblProduto);
            txtProduto = FindViewById<EditText>(Resource.Id.txtProduto);
            btnProduto = FindViewById<Button>(Resource.Id.btnProduto);
            txtQtdeInteiro = FindViewById<EditText>(Resource.Id.txtQtdeInteiro);
            txtQtdeUnidade = FindViewById<EditText>(Resource.Id.txtQtdeUnidade);
            btnColetar = FindViewById<Button>(Resource.Id.btnColetar);
            btnPesquisar = FindViewById<Button>(Resource.Id.btnPesquisar);
            lblQtdeInteiro = FindViewById<TextView>(Resource.Id.lblQtdeInteiro);
            lblQtdeUnidade = FindViewById<TextView>(Resource.Id.lblQtdeUnidade);
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

        class ListarProdutosColetados : ArrayAdapter<CSListViewItem>
        {
            Activity act;
            IList<CSListViewItem> produto;
            int resourceId;

            public ListarProdutosColetados(Activity c, int textViewResourceId, IList<CSListViewItem> objects)
                : base(c, textViewResourceId, objects)
            {
                act = c;
                produto = objects;
                resourceId = textViewResourceId;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSListViewItem item = produto[position];

                LayoutInflater layout = (LayoutInflater)act.GetSystemService(Context.LayoutInflaterService);
                View linha = layout.Inflate(resourceId, null);

                try
                {
                    TextView tvProduto = linha.FindViewById<TextView>(Resource.Id.tvProduto);
                    TextView tvQuantidadeEstoque = linha.FindViewById<TextView>(Resource.Id.tvQuantidadeEstoque);

                    tvProduto.Text = item.Text;
                    tvQuantidadeEstoque.Text = item.SubItems[0].ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Alert(act, ex.Message);
                }

                return linha;
            }

        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            switch (requestCode)
            {
                case frmProcuraProduto:
                    {
                        if (resultCode == Result.Ok &&
                            data != null)
                        {
                            int codProdutoEncontrado = data.GetIntExtra("codProduto", -1);
                            if (codProdutoEncontrado != -1)
                            {
                                txtProduto.Text = CSProdutos.GetProduto(codProdutoEncontrado).DESCRICAO_APELIDO_PRODUTO;
                                BtnProduto_Click(null, null);
                            }
                            else
                            {
                                MessageBox.ShowShortMessageCenter(this, "Não foi possível buscar o código do produto.");
                            }
                        }
                    }
                    break;
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        private void AdicionarEventos()
        {
            btnProduto.Click += BtnProduto_Click;
            btnColetar.Click += BtnColetar_Click;
            btnPesquisar.Click += BtnPesquisar_Click;
            lvwEstoque.ItemLongClick += LvwEstoque_ItemLongClick;
            lvwEstoque.ItemClick += LvwEstoque_ItemClick;

            //if (CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
            //{
            //    chkOcultar.CheckedChange += ChkOcultar_CheckedChange;
            //    chkPerda.CheckedChange += ChkPerda_CheckedChange;
            //}
        }

        private void ListaDados()
        {
            List<CSListViewItem> listItem = new List<CSListViewItem>();

            if (coletaEstoquePDV.Count > 0)
            {
                foreach (CSColetaEstoquePDV.CSColetaEstoqueProduto produto in coletaEstoquePDV.Cast<CSColetaEstoquePDV.CSColetaEstoqueProduto>().Where(p => p.STATE != ObjectState.DELETADO))
                {
                    CSListViewItem item = new CSListViewItem();

                    item.Text = produto.PRODUTO.DESCRICAO_APELIDO_PRODUTO.Trim() + " - " + produto.PRODUTO.DSC_PRODUTO.Trim();

                    item.SubItems = new List<object>();

                    if (produto.PRODUTO.COD_UNIDADE_MEDIDA != "KG" &&
                        produto.PRODUTO.COD_UNIDADE_MEDIDA != "LT")
                    {
                        item.SubItems.Add(produto.QTD_COLETADA_INTEIRA.ToString() + "/" + produto.QTD_COLETADA_UNIDADE.ToString("###000"));
                    }
                    else
                    {
                        item.SubItems.Add(produto.QTD_COLETADA_INTEIRA.ToString(CSGlobal.DecimalStringFormat));
                    }

                    item.SubItems.Add(produto.PRODUTO.DESCRICAO_APELIDO_PRODUTO);

                    item.Valor = produto;

                    listItem.Add(item);
                }

                lvwEstoque.Adapter = new ListarProdutosColetados(this, Resource.Layout.coletar_estoque_row, listItem);
            }
        }

        private void ProcuraProdutoColeta(bool atualizaTela)
        {
            string produtoProcurado = txtProduto.Text;
            bool achou = false;
            produtoAtual = null;

            if (atualizaTela)
                LimpaEntrada();

            txtProduto.Text = produtoProcurado;

            foreach (CSProdutos.CSProduto produto in CSProdutos.Items)
            {
                if (produto.DESCRICAO_APELIDO_PRODUTO.ToString() == produtoProcurado)
                {
                    //if (CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                    //{
                    //    txtPerdaInteira.Visibility = ViewStates.Visible;
                    //    lblPerdaInteira.Visibility = ViewStates.Visible;
                    //}

                    btnColetar.Visibility = ViewStates.Visible;
                    lblProduto.Visibility = ViewStates.Visible;
                    txtQtdeInteiro.Visibility = ViewStates.Visible;
                    lblQtdeInteiro.Visibility = ViewStates.Visible;
                    txtProduto.Text = produto.DESCRICAO_APELIDO_PRODUTO.ToString();
                    lblProduto.Text = produto.DSC_APELIDO_PRODUTO.ToString();
                    produtoAtual = produto;
                    achou = true;
                    AtualizaDadosTela(produto.COD_PRODUTO, atualizaTela);

                    if (produtoAtual.COD_UNIDADE_MEDIDA == "CX" || produtoAtual.COD_UNIDADE_MEDIDA == "DZ")
                    {
                        txtQtdeUnidade.Visibility = ViewStates.Visible;
                        lblQtdeUnidade.Visibility = ViewStates.Visible;

                        //if (CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                        //{
                            //txtPerdaUnidade.Visibility = ViewStates.Visible;
                            //lblPerdaUnidade.Visibility = ViewStates.Visible;
                        //}
                    }
                    break;
                }

                txtQtdeInteiro.RequestFocus();
            }

            if (!achou)
            {
                lblProduto.Visibility = ViewStates.Gone;
                MessageBox.ShowShortMessageCenter(this, "Nenhum produto encontrado.");
            }
        }

        private void AtualizaDadosTela(int COD_PRODUTO, bool atualizaTela)
        {
            if (COD_PRODUTO > 0 && coletaEstoquePDV.Count > 0)
            {
                foreach (CSColetaEstoquePDV.CSColetaEstoqueProduto produto in coletaEstoquePDV)
                {
                    if (produto.PRODUTO.COD_PRODUTO == COD_PRODUTO)
                    {
                        coletaEstoquePDV.Current = produto;

                        if (atualizaTela)
                        {
                            if (!CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                            {
                                if (produto.PRODUTO.COD_UNIDADE_MEDIDA != "KG" &&
                                    produto.PRODUTO.COD_UNIDADE_MEDIDA != "LT")
                                {
                                    txtQtdeInteiro.Text = produto.QTD_COLETADA_INTEIRA.ToString();
                                    txtQtdeUnidade.Text = produto.QTD_COLETADA_UNIDADE.ToString("###000");
                                }
                                else
                                {
                                    txtQtdeInteiro.Text = produto.QTD_COLETADA_INTEIRA.ToString(CSGlobal.DecimalStringFormat);
                                }
                            }
                            else
                            {
                                if (produto.QTD_COLETADA_INTEIRA == -1)
                                    txtQtdeInteiro.Text = string.Empty;
                                else
                                    txtQtdeInteiro.Text = produto.QTD_COLETADA_INTEIRA.ToString();

                                if (txtQtdeUnidade.Visibility == ViewStates.Visible &&
                                    produto.QTD_COLETADA_UNIDADE == -1)
                                    txtQtdeUnidade.Text = string.Empty;
                                else
                                    txtQtdeUnidade.Text = produto.QTD_COLETADA_UNIDADE.ToString("###000");

                                //if (CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                                //{
                                //    if (produto.QTD_PERDA_INTEIRA == -1)
                                //        txtPerdaInteira.Text = string.Empty;
                                //    else
                                //        txtPerdaInteira.Text = produto.QTD_PERDA_INTEIRA.ToString();

                                //    if (txtPerdaUnidade.Visibility == ViewStates.Visible &&
                                //        produto.QTD_PERDA_UNIDADE == -1)
                                //        txtPerdaUnidade.Text = string.Empty;
                                //    else
                                //        txtPerdaUnidade.Text = produto.QTD_PERDA_UNIDADE.ToString("###000");
                                //}
                            }

                            txtQtdeInteiro.RequestFocus();
                        }
                        break;
                    }
                }
            }
        }

        private void LimpaEntrada()
        {
            txtProduto.Text = string.Empty;
            lblProduto.Text = string.Empty;
            txtQtdeInteiro.Text = string.Empty;
            txtQtdeUnidade.Text = string.Empty;
            produtoAtual = null;

            //if (CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
            //{
            //    txtPerdaInteira.Text = string.Empty;
            //    txtPerdaUnidade.Text = string.Empty;
            //}
        }

        private void LvwEstoque_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            lblProduto.Visibility = ViewStates.Gone;
            txtQtdeInteiro.Visibility = ViewStates.Gone;
            lblQtdeInteiro.Visibility = ViewStates.Gone;
            txtQtdeUnidade.Visibility = ViewStates.Gone;
            lblQtdeUnidade.Visibility = ViewStates.Gone;

            //if (CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
            //{
            //    txtPerdaInteira.Visibility = ViewStates.Gone;
            //    lblPerdaInteira.Visibility = ViewStates.Gone;
            //    txtPerdaUnidade.Visibility = ViewStates.Gone;
            //    lblPerdaUnidade.Visibility = ViewStates.Gone;
            //}

            Position = e.Position; ;
            txtProduto.Text = ((CSColetaEstoquePDV.CSColetaEstoqueProduto)((CSListViewItem)lvwEstoque.Adapter.GetItem(e.Position)).Valor).PRODUTO.DESCRICAO_APELIDO_PRODUTO.Trim();
            ProcuraProdutoColeta(true);
            txtQtdeInteiro.RequestFocus();
        }

        private void ChkPerda_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            //ListaDadosSugerido();
        }

        private void ChkOcultar_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            //ListaDadosSugerido();
        }

        private void BtnProduto_Click(object sender, EventArgs e)
        {
            if (txtProduto.Text.Trim().Length == 0)
                MessageBox.ShowShortMessageCenter(this, "Codigo do produto invalido.");
            else
                ProcuraProdutoColeta(true);
        }

        private void BtnColetar_Click(object sender, EventArgs e)
        {
            // [ Validação sobre o produto ]
            if (txtProduto.Text.Trim().Length == 0)
            {
                MessageBox.ShowShortMessageCenter(this, "Informe o codigo do produto.");
                txtProduto.RequestFocus();
            }
            else
            {
                // [ Quando não foi localizado o produto]
                if (lblProduto.Text.Trim().Length == 0)
                    ProcuraProdutoColeta(false);

                if (produtoAtual != null)
                {
                    // [ Validação sobre a quantidade ]
                    if (!QuantidadesValidadas())
                    {
                        MessageBox.ShowShortMessageCenter(this, "Informe a quantidade.");
                        txtQtdeInteiro.RequestFocus();
                    }
                    else
                    {
                        if (CSGlobal.StrToInt(txtQtdeUnidade.Text) > 0 &&
                           (produtoAtual.COD_UNIDADE_MEDIDA == "CX" || produtoAtual.COD_UNIDADE_MEDIDA == "DZ") &&
                           (CSGlobal.StrToInt(txtQtdeUnidade.Text) >= produtoAtual.QTD_UNIDADE_EMBALAGEM))
                        {
                            MessageBox.Alert(this, "A quantidade fracionaria não pode ser maior ou igual a: " + produtoAtual.QTD_UNIDADE_EMBALAGEM.ToString());
                            txtQtdeUnidade.RequestFocus();
                        }
                        else
                        {
                            if (coletaEstoquePDV.Current != null)
                            {
                                coletaEstoquePDV.Current.QTD_COLETADA_INTEIRA = CSGlobal.StrToDecimal(txtQtdeInteiro.Text);
                                coletaEstoquePDV.Current.QTD_COLETADA_UNIDADE = CSGlobal.StrToInt(txtQtdeUnidade.Text);

                                if (CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                                {
                                    //coletaEstoquePDV.Current.QTD_PERDA_INTEIRA = CSGlobal.StrToInt(txtPerdaInteira.Text);
                                    //coletaEstoquePDV.Current.QTD_PERDA_UNIDADE = CSGlobal.StrToInt(txtPerdaUnidade.Text);
                                    coletaEstoquePDV.CalculaGiroSellout(coletaEstoquePDV.Current);
                                    coletaEstoquePDV.Current.PRODUTO.QTD_PRODUTO_SUGERIDO = CSColetaEstoquePDV.CalculaQuantidadeSugerida(coletaEstoquePDV.Current);

                                    if (LastActivity() == ActivitiesNames.ProdutoPedido)
                                    {
                                        CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_INTEIRA = coletaEstoquePDV.Current.PRODUTO.QTD_PRODUTO_SUGERIDO;
                                        CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.IND_UTILIZA_QTD_SUGERIDA = true;
                                        CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.IND_UILIZA_QTD_MINIMA = CSColetaEstoquePDV.UtilizaQtdMinima(coletaEstoquePDV.GetProdutoColetado(CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRODUTO.COD_PRODUTO));

                                        if (CSEmpresa.Current.TIPO_TROCA == 2)
                                        {
                                            //CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_INDENIZACAO_EXIBICAO = CSGlobal.StrToDecimal(txtPerdaInteira.Text);
                                            CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_INDENIZACAO_EXIBICAO = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRODUTO.PRECOS_PRODUTO.Current.VLR_PRODUTO * CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_INDENIZACAO_INTEIRA;
                                        }
                                        else
                                        {
                                            //CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_INDENIZACAO_INTEIRA = CSGlobal.StrToDecimal(txtPerdaInteira.Text);
                                            CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_INDENIZACAO_UNIDADE = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRODUTO.PRECOS_PRODUTO.Current.VLR_PRODUTO * CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_INDENIZACAO_INTEIRA;
                                        }
                                    }
                                }
                                if (coletaEstoquePDV.Current.STATE != ObjectState.NOVO)
                                    coletaEstoquePDV.Current.STATE = ObjectState.ALTERADO;
                            }
                            else
                            {
                                CSColetaEstoquePDV.CSColetaEstoqueProduto novoProduto = new CSColetaEstoquePDV.CSColetaEstoqueProduto();

                                novoProduto.PRODUTO = produtoAtual;
                                novoProduto.QTD_COLETADA_INTEIRA = CSGlobal.StrToDecimal(txtQtdeInteiro.Text);
                                novoProduto.QTD_COLETADA_UNIDADE = CSGlobal.StrToInt(txtQtdeUnidade.Text);

                                //if (CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                                //{
                                //    novoProduto.QTD_PERDA_INTEIRA = CSGlobal.StrToInt(txtPerdaInteira.Text);
                                //    novoProduto.QTD_PERDA_UNIDADE = CSGlobal.StrToInt(txtPerdaUnidade.Text);
                                //    novoProduto.NUM_COLETA_ESTOQUE = -1;
                                //}

                                coletaEstoquePDV.Add(novoProduto);
                            }
                            lblProduto.Visibility = ViewStates.Gone;
                            btnColetar.Visibility = ViewStates.Gone;
                            txtQtdeInteiro.Visibility = ViewStates.Gone;
                            lblQtdeInteiro.Visibility = ViewStates.Gone;
                            txtQtdeUnidade.Visibility = ViewStates.Gone;
                            lblQtdeUnidade.Visibility = ViewStates.Gone;

                            //if (CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                            //{
                            //    lblPerdaInteira.Visibility = ViewStates.Gone;
                            //    txtPerdaInteira.Visibility = ViewStates.Gone;
                            //    lblPerdaUnidade.Visibility = ViewStates.Gone;
                            //    txtPerdaUnidade.Visibility = ViewStates.Gone;
                            //}

                            coletaEstoquePDV.Current = null;
                            LimpaEntrada();

                            ListaDados();

                            txtQtdeInteiro.RequestFocus();
                            lvwEstoque.SetSelection(Position);
                        }
                        //}
                    }
                }
            }
        }

        private void BtnPesquisar_Click(object sender, EventArgs e)
        {
            OnSearchRequested();
        }

        private void LvwEstoque_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            MessageBox.Alert(this, "Confirma excluir o produto?", "Excluir", (_sender, _e) =>
             {
                 ((CSColetaEstoquePDV.CSColetaEstoqueProduto)((CSListViewItem)lvwEstoque.Adapter.GetItem(e.Position)).Valor).STATE = ObjectState.DELETADO;

                 lblProduto.Text = string.Empty;
                 lblProduto.Visibility = ViewStates.Gone;
                 txtProduto.Text = string.Empty;
                 txtQtdeInteiro.Text = string.Empty;
                 txtQtdeUnidade.Text = string.Empty;

                 //if (CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                 //{
                 //    txtPerdaInteira.Text = string.Empty;
                 //    txtPerdaUnidade.Text = string.Empty;
                 //}
                 //else
                     ListaDados();

                 btnColetar.Visibility = ViewStates.Gone;
                 txtQtdeInteiro.Visibility = ViewStates.Gone;
                 lblQtdeInteiro.Visibility = ViewStates.Gone;
                 txtQtdeUnidade.Visibility = ViewStates.Gone;
                 lblQtdeUnidade.Visibility = ViewStates.Gone;

                 //if (CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                 //{
                 //    lblPerdaInteira.Visibility = ViewStates.Gone;
                 //    txtPerdaInteira.Visibility = ViewStates.Gone;
                 //    lblPerdaUnidade.Visibility = ViewStates.Gone;
                 //    txtPerdaUnidade.Visibility = ViewStates.Gone;
                 //}

             }, "Cancelar", null, true);
        }

        private class ThreadColetaEstoque : AsyncTask<int, int, decimal>
        {
            protected override decimal RunInBackground(params int[] @params)
            {
                CSPDVs.Current.COLETA_OBRIGATORIA = true;
                coletaEstoquePDV.Flush();

                if (UltimaTela != "ProdutoPedido")
                {
                    if (CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                    {
                        CSGlobal.PedidoSugerido = false;
                        CSGlobal.PesquisarComoSugerido = false;

                        foreach (CSColetaEstoquePDV.CSColetaEstoqueProduto ProdutoColetadoAtual in coletaEstoquePDV)
                        {
                            if (ProdutoColetadoAtual.QTD_COLETADA_INTEIRA == -1)
                            {
                                CSGlobal.PedidoSugerido = false;
                                CSGlobal.PesquisarComoSugerido = false;
                                break;
                            }
                            else
                            {
                                CSGlobal.PedidoSugerido = true;
                                CSGlobal.PesquisarComoSugerido = true;
                            }
                        }
                    }
                }

                coletaEstoquePDV.Dispose();

                return 0;
            }

            protected override void OnPostExecute(decimal result)
            {
                if (ColetarEstoque.progressDialog != null)
                {
                    progressDialog.Dismiss();
                    progressDialog.Dispose();
                }
                base.OnPostExecute(result);
                if (coletaEstoquePDV.Count == 0)
                {
                    CurrentActivity.OnBackPressed();
                }
            }
        }

        public override void OnBackPressed()
        {
            if (coletaEstoquePDV.Count > 0)
            {
                progressDialog = new ProgressDialog(CurrentActivity);
                progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
                progressDialog.SetTitle("Coleta estoque...");
                progressDialog.SetCancelable(false);
                progressDialog.SetMessage("Preparando informações...");
                progressDialog.Show();

                new ThreadColetaEstoque().Execute();
            }
            else
            {
                SetResult(Result.Ok);

                base.OnBackPressed();
            }
        }

        public override bool OnSearchRequested()
        {
            if (LastActivity() != ActivitiesNames.ProdutoPedido)
            {
                Intent i = new Intent();
                i.SetClass(this, typeof(ProcuraProduto));
                i.PutExtra("formulario", "Estoque");
                this.StartActivityForResult(i, frmProcuraProduto);
            }

            return base.OnSearchRequested();
        }

        private void FrmColetarEstoque_OnProdutoEncontrado(CSProdutos.CSProduto produto)
        {
            if (produto.COD_PRODUTO > -1)
            {
                txtProduto.Text = produto.DESCRICAO_APELIDO_PRODUTO.Trim();
                ProcuraProdutoColeta(true);
            }
        }

        private bool QuantidadesValidadas()
        {
            if (CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
            {
                if (txtQtdeInteiro.Text == string.Empty)
                    return false;

                //if (!chkPerda.Checked && txtPerdaInteira.Text == string.Empty)
                //    return false;
            }
            else
            {
                if (CSGlobal.StrToDecimal(txtQtdeInteiro.Text) == 0 &&
                    CSGlobal.StrToInt(txtQtdeUnidade.Text) == 0)
                    return false;
            }

            return true;
        }
    }
}