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
using AvanteSales.BusinessRules;
using AvanteSales.Pro.Controles;
using AvanteSales.Pro.Dialogs;
using AvanteSales.Pro.Formatters;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "ProcuraProduto", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ProcuraProduto : AppCompatActivity, TextView.IOnEditorActionListener
    {
        TextView lblCodPdv;
        TextView lblNomePdv;
        Android.Support.V7.Widget.Toolbar tbToolbar;
        private static bool ListarApelido;
        private EditText txtPesquisa;
        private EditText txtQtdeInteiro;
        private CheckBox chkOpcoesAvancadas;
        private LinearLayout llOpcoesAvancadas;
        private CheckBox chkCodigo;
        private CheckBox chkApelido;
        private CheckBox chkDescricao;
        private CheckBox chkPesquisaRapida;
        private TextView tvHeaderCodigo;
        private static TextView tvHeaderQuantidade;

        private LinearLayout HeaderListView;
        private ListView ListView;
        private ExpandableListView ExpandableListView;

        private static string CriterioPesquisa;
        private static int CodPdv;
        private static bool ChkCodigo;
        private static bool ChkApelido;
        private static bool ChkDescricao;
        //private static bool ChkConsiderarCombo;
        private static int PositionItem;
        private Button btnPesquisar;

        private static bool ExistePedidoVendido
        {
            set
            {
                if (value)
                {
                    tvHeaderQuantidade.Visibility = ViewStates.Visible;
                }
                else
                {
                    tvHeaderQuantidade.Visibility = ViewStates.Invisible;
                }
            }
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

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.procura_produto);

            FindViewsByIds();

            SetSupportActionBar(tbToolbar);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(false);

            lblCodPdv.Text = CSPDVs.Current.COD_PDV.ToString();
            lblNomePdv.Text = CSPDVs.Current.DSC_RAZAO_SOCIAL;

            chkOpcoesAvancadas.CheckedChange += new EventHandler<CompoundButton.CheckedChangeEventArgs>(chkOpcoesAvancadas_CheckedChange);

            ListView.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(ListView_ItemClick);
            btnPesquisar.Click += new EventHandler(btnPesquisar_Click);
            HeaderListView.Click += new EventHandler(HeaderListView_Click);
            chkPesquisaRapida.CheckedChange += new EventHandler<CompoundButton.CheckedChangeEventArgs>(chkPesquisaRapida_CheckedChange);

            AtualizaCampoQuantidade();

            txtPesquisa.SetOnEditorActionListener(this);
            txtQtdeInteiro.SetOnEditorActionListener(this);

            ListarApelido = !Produtos.ExibirDescricaoProduto;

            if (CSConfiguracao.GetConfig("PESQUISA_RAPIDA") == "S")
                chkPesquisaRapida.Checked = true;
            else
                chkPesquisaRapida.Checked = false;
        }

        void chkPesquisaRapida_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            CSConfiguracao.SetConfig("PESQUISA_RAPIDA", chkPesquisaRapida.Checked ? "S" : "N");
        }

        void HeaderListView_Click(object sender, EventArgs e)
        {
            if (!ListarApelido)
                ListarApelido = true;
            else
                ListarApelido = false;

            btnPesquisar_Click(null, null);
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (CriterioPesquisa != string.Empty &&
                (CodPdv == 0 ||
                CodPdv == CSPDVs.Current.COD_PDV))
            {
                txtPesquisa.Text = CriterioPesquisa;
                chkCodigo.Checked = ChkCodigo;
                chkApelido.Checked = ChkApelido;
                chkDescricao.Checked = ChkDescricao;
                btnPesquisar_Click(null, null);
                ListView.SetSelection(PositionItem);

                if (!string.IsNullOrEmpty(CriterioPesquisa))
                {
                    if (CSConfiguracao.GetConfig("PESQUISA_RAPIDA") == "S")
                        txtPesquisa.Text = string.Empty;

                    txtPesquisa.SetSelection(txtPesquisa.Text.Length);
                }
            }
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

        void chkOpcoesAvancadas_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (chkOpcoesAvancadas.Checked)
                llOpcoesAvancadas.Visibility = ViewStates.Visible;
            else
                llOpcoesAvancadas.Visibility = ViewStates.Gone;
        }

        private void FindViewsByIds()
        {
            lblCodPdv = FindViewById<TextView>(Resource.Id.lblCodPdv);
            lblNomePdv = FindViewById<TextView>(Resource.Id.lblNomePdv);
            tbToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tbToolbar);
            txtPesquisa = FindViewById<EditText>(Resource.Id.txtPesquisa);
            txtQtdeInteiro = FindViewById<EditText>(Resource.Id.txtQtdeInteiro);
            chkOpcoesAvancadas = FindViewById<CheckBox>(Resource.Id.chkOpcoesAvancadas);
            llOpcoesAvancadas = FindViewById<LinearLayout>(Resource.Id.llOpcoesAvancadas);
            chkCodigo = FindViewById<CheckBox>(Resource.Id.chkCodigo);
            chkApelido = FindViewById<CheckBox>(Resource.Id.chkApelido);
            chkDescricao = FindViewById<CheckBox>(Resource.Id.chkDescricao);
            chkPesquisaRapida = FindViewById<CheckBox>(Resource.Id.chkPesquisaRapida);
            tvHeaderCodigo = FindViewById<TextView>(Resource.Id.tvHeaderCodigo);
            tvHeaderQuantidade = FindViewById<TextView>(Resource.Id.tvHeaderQuantidade);
            //chkInicio = FindViewById<CheckBox>(Resource.Id.chkInicio);
            //chkMeio = FindViewById<CheckBox>(Resource.Id.chkMeio);
            //chkFim = FindViewById<CheckBox>(Resource.Id.chkFim);
            ListView = FindViewById<ListView>(Resource.Id.lvResultado);
            ExpandableListView = FindViewById<ExpandableListView>(Resource.Id.elvResultado);
            btnPesquisar = FindViewById<Button>(Resource.Id.btnPesquisar);
            HeaderListView = FindViewById<LinearLayout>(Resource.Id.HeaderListView);
        }

        public override bool OnSearchRequested()
        {
            btnPesquisar_Click(null, null);
            return true;
        }

        private void MostraControlesItemCombo(bool exibirItemCombo)
        {
            ViewGroup.MarginLayoutParams mlp = (ViewGroup.MarginLayoutParams)tvHeaderCodigo.LayoutParameters;
            if (exibirItemCombo)
            {
                tvHeaderQuantidade.Visibility = ViewStates.Visible;
                ListView.Visibility = ViewStates.Gone;
                ExpandableListView.Visibility = ViewStates.Visible;
                mlp.LeftMargin = 50;
            }
            else
            {
                tvHeaderQuantidade.Visibility = ViewStates.Gone;
                ListView.Visibility = ViewStates.Visible;
                ExpandableListView.Visibility = ViewStates.Gone;
                mlp.LeftMargin = 0;
            }
            //tvHeaderCodigo.LayoutParameters = mlp;
        }

        private void btnPesquisar_Click(object sender, EventArgs e)
        {
            this.HideKeyboard(txtPesquisa);

            // Verifica se textbox está vazio
            if (string.IsNullOrWhiteSpace(txtPesquisa.Text))
            {
                MessageBox.ShowShortMessageCenter(this, "Por favor, digite uma palavra ou código a ser pesquisado.");
                return;
            }

            // Limpa os resultados antigos
            ListView.Adapter = null;

            string produtoProcurado = txtPesquisa.Text.ToUpper();

            MostraControlesItemCombo(false);

            if (chkCodigo.Checked || chkDescricao.Checked || chkApelido.Checked)
            {
                BuscaAvancada(produtoProcurado);
            }
            else
            {
                BuscaNormal(produtoProcurado);
            }

            if (ListView.Adapter.Count == 0)
            {
                CriterioPesquisa = string.Empty;
                MessageBox.ShowShortMessageCenter(this, "Nenhum produto encontrado ou produto sem estoque/preço.");
            }
            else
            {
                CriterioPesquisa = txtPesquisa.Text;

                if (CSPDVs.Current != null)
                    CodPdv = CSPDVs.Current.COD_PDV;

                ChkApelido = chkApelido.Checked;
                ChkCodigo = chkCodigo.Checked;
                //ChkConsiderarCombo = false;
                ChkDescricao = chkDescricao.Checked;
                MessageBox.ShowShortMessageCenter(this, string.Format("{0} produtos encontrados.", ListView.Adapter.Count));
            }
        }

        private void BuscaAvancada(string produtoProcurado)
        {
            var produtos = new List<CSProdutos.CSProduto>();
            // Procura os dados do Produto
            foreach (CSProdutos.CSProduto produto in CSProdutos.Items)
            {
                if (!CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA.ToString().Contains(produto.COD_TIPO_DISTRIBUICAO_POLITICA.ToString()))
                    continue;

                if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current != null)
                {
                    if (produto.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO != CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.COD_GRUPO_COMERCIALIZACAO)
                        continue;
                }

                if (produto.IND_ITEM_COMBO == true)
                    continue;

                if ((CSEmpresa.Current.IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE == "N" && produto.QTD_ESTOQUE <= 0) ||
                    (produto.IND_PRODUTO_COM_PRECO == false))
                    continue;

                // Se for encontrado em codigo do produto
                if (chkCodigo.Checked && produto.DESCRICAO_APELIDO_PRODUTO.ToUpper().IndexOf(produtoProcurado) >= 0)
                {
                    if ((!produtos.Select(p => p.COD_PRODUTO).Contains(produto.COD_PRODUTO)))
                    {
                        produtos.Add(produto);
                        continue;
                    }

                    // Se for encontrado em descricao do produto
                }
                else if (chkDescricao.Checked && produto.DSC_PRODUTO.ToUpper().IndexOf(produtoProcurado) >= 0 && !produtos.Contains(produto))
                {
                    if ((!produtos.Select(p => p.COD_PRODUTO).Contains(produto.COD_PRODUTO)))
                    {
                        produtos.Add(produto);
                        continue;
                    }
                    // Se for encontrado em descricao do apelido do produto
                }
                else if (chkApelido.Checked && produto.DSC_APELIDO_PRODUTO.ToUpper().IndexOf(produtoProcurado) >= 0 && !produtos.Contains(produto))
                {
                    if ((!produtos.Select(p => p.COD_PRODUTO).Contains(produto.COD_PRODUTO)))
                    {
                        produtos.Add(produto);
                        continue;
                    }
                }
            }

            if (CSEmpresa.ColunaExiste("EMPRESA", "IND_MOSTRAR_PRODUTO_BLOQUEADO"))
            {
                if (CSEmpresa.Current.IND_MOSTRAR_PRODUTO_BLOQUEADO == "N")
                    produtos = produtos.Where(p => p.IND_PRODUTO_BLOQUEADO == false).ToList();
            }

            if (chkCodigo.Checked || chkCodigo.Checked && chkDescricao.Checked)
                ListView.Adapter = new ProcuraProdutoListAdapter(this, Resource.Layout.procura_produto_row, CSProdutos.OrdenarListaProdutos(produtos));

            else if (chkDescricao.Checked)
                ListView.Adapter = new ProcuraProdutoListAdapter(this, Resource.Layout.procura_produto_row, produtos.OrderBy(p => p.DSC_PRODUTO).ToArray());
            else
                ListView.Adapter = new ProcuraProdutoListAdapter(this, Resource.Layout.procura_produto_row, produtos.OrderBy(p => p.DSC_APELIDO_PRODUTO).ToArray());

        }

        private void BuscaNormal(string produtoProcurado)
        {
            var produtos = new List<CSProdutos.CSProduto>();

            foreach (CSProdutos.CSProduto produto in CSProdutos.Items)
            {
                if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current != null)
                {
                    if (produto.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO != CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.COD_GRUPO_COMERCIALIZACAO)
                        continue;
                }

                if (CSEmpresa.Current.IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE == "N")
                {
                    if (produto.QTD_ESTOQUE <= 0)
                        continue;

                    //produtos = produtos.Where(p => p.QTD_ESTOQUE > 0);
                }

                if (produto.IND_ITEM_COMBO)
                    continue;

                if (!produto.IND_PRODUTO_COM_PRECO)
                    continue;

                //produtos = produtos.Where(p => !p.IND_ITEM_COMBO);
                //produtos = produtos.Where(p => p.IND_PRODUTO_COM_PRECO);

                //produtos = produtos.Where(p =>
                //    p.DSC_PRODUTO.ToUpper().Contains(produtoProcurado)
                //    || p.DSC_APELIDO_PRODUTO.ToUpper().Contains(produtoProcurado)
                //    || p.DESCRICAO_APELIDO_PRODUTO.Contains(produtoProcurado)
                //    && p.COD_TIPO_DISTRIBUICAO_POLITICA == CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA);

                if ((produto.DSC_PRODUTO.ToUpper().IndexOf(produtoProcurado) >= 0 ||
                    produto.DSC_APELIDO_PRODUTO.ToUpper().IndexOf(produtoProcurado) >= 0 ||
                    produto.DESCRICAO_APELIDO_PRODUTO.IndexOf(produtoProcurado) >= 0) &&
                    produto.COD_TIPO_DISTRIBUICAO_POLITICA == CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA &&
                    !produtos.Select(p => p.COD_PRODUTO).Contains(produto.COD_PRODUTO))
                    produtos.Add(produto);
            }

            int codProduto = -1;
            var produtosOrdenados = new List<CSProdutos.CSProduto>();
            if (int.TryParse(produtoProcurado, out codProduto))
            {
                int i;

                var ListaItens = produtos.Cast<CSProdutos.CSProduto>().Where(p => int.TryParse(p.DESCRICAO_APELIDO_PRODUTO, out i)).OrderBy(b => Convert.ToInt32(b.DESCRICAO_APELIDO_PRODUTO)).ToList();
                ListaItens.AddRange(produtos.Cast<CSProdutos.CSProduto>().Where(p => !int.TryParse(p.DESCRICAO_APELIDO_PRODUTO, out i)).OrderBy(b => b.DESCRICAO_APELIDO_PRODUTO).ToList());

                produtosOrdenados = ListaItens.ToList();
            }
            else
            {
                produtosOrdenados = produtos.OrderBy(p => p.DSC_APELIDO_PRODUTO).ToList();
            }

            //var produtosNaoVendidos = Produtos.RemoveProdutosJaAdicionadosAoPedido(produtosOrdenados);

            if (CSEmpresa.ColunaExiste("EMPRESA", "IND_MOSTRAR_PRODUTO_BLOQUEADO"))
            {
                if (CSEmpresa.Current.IND_MOSTRAR_PRODUTO_BLOQUEADO == "N")
                    produtosOrdenados = produtosOrdenados.Where(p => p.IND_PRODUTO_BLOQUEADO == false).ToList();
            }

            ListView.Adapter = new ProcuraProdutoListAdapter(this, Resource.Layout.procura_produto_row, produtosOrdenados);
        }

        void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                PositionItem = ListView.FirstVisiblePosition;
                Intent data = new Intent();
                //int codigoProduto = ((ProcuraProdutoListAdapter)ListView.Adapter).GetCodProduto(e.Position);

                CSProdutos.CSProduto produto = ((ProcuraProdutoListAdapter)ListView.Adapter).GetProduto(e.Position);

                if (LastActivity() == ActivitiesNames.ProdutoPedido &&
                    CSEmpresa.ColunaExiste("GRUPO_COMERCIALIZACAO", "IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO"))
                {
                    if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Count > 0)
                    {
                        if (produto.GRUPO_COMERCIALIZACAO.IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO.ToUpper() == "S")
                        {
                            if (produto.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO != CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS[0].PRODUTO.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO)
                                MessageBox.Alert(this, string.Format("Venda impedida. O grupo de comercialização \"{0}\" só pode ser vendido com produtos do mesmo grupo.", produto.GRUPO_COMERCIALIZACAO.DES_GRUPO_COMERCIALIZACAO));
                            else
                                RetornarProduto(data, produto);
                        }
                        else
                        {
                            if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS[0].PRODUTO.GRUPO_COMERCIALIZACAO.IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO.ToUpper() == "S")
                            {
                                if (produto.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO != CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS[0].PRODUTO.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO)
                                    MessageBox.Alert(this, string.Format("Venda impedida. Somente são permitidos produtos com grupo de comercialiação {0}.", CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS[0].PRODUTO.GRUPO_COMERCIALIZACAO.DES_GRUPO_COMERCIALIZACAO));
                                else
                                    RetornarProduto(data, produto);
                            }
                            else
                                RetornarProduto(data, produto);
                        }
                    }
                    else
                        RetornarProduto(data, produto);
                }
                else
                    RetornarProduto(data, produto);
            }
            catch (Exception ex)
            {
                MessageBox.AlertErro(this, ex.Message);
            }
        }

        private void RetornarProduto(Intent data, CSProdutos.CSProduto produto)
        {
            if (LastActivity() == ActivitiesNames.Produtos &&
                CSPDVs.Current.PEDIDOS_INDENIZACAO.Current == null)
            {
                if (!CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDO_SUGERIDO.HasValue)
                    CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDO_SUGERIDO = PedidoSugerido();

                if (CSGlobal.PedidoSugerido ||
                    CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDO_SUGERIDO.Value)
                {
                    if (CSEmpresa.Current.IND_PERMITIR_VENDA_SOMENTE_LAYOUT == "S")
                    {
                        if (produto.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO == 2 &&
                            !produto.IND_PROD_ESPECIFICO_CATEGORIA)
                        {
                            MessageBox.Alert(this, "Permitido apenas produtos de Layout/CR.");
                            return;
                        }
                    }
                    else
                    {
                        if (produto.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO == 2 &&
                           !produto.IND_PROD_ESPECIFICO_CATEGORIA)
                        {
                            CSPDVs.Current.PEDIDOS_PDV.Current.TODOS_SUGERIDOS_VENDIDOS = TodosProdutosSugeridosVendidos();

                            if (CSPDVs.Current.PEDIDOS_PDV.Current.TODOS_SUGERIDOS_VENDIDOS.Value)
                            {
                                int quantidadeProdutosTerceiros = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.PRODUTO.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO == 2 && !p.PRODUTO.IND_PROD_ESPECIFICO_CATEGORIA && p.STATE != ObjectState.DELETADO).Count();

                                quantidadeProdutosTerceiros = produto.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO == 2 ? quantidadeProdutosTerceiros + 1 : quantidadeProdutosTerceiros;

                                if (quantidadeProdutosTerceiros > CSEmpresa.Current.QTD_MAX_VENDA_OUTROS_DANONE)
                                {
                                    MessageBox.Alert(this, string.Format("Quantidade máxima ({0}) de produtos DANONE atingida.", CSEmpresa.Current.QTD_MAX_VENDA_OUTROS_DANONE.ToString()));
                                    return;
                                }
                            }
                            else
                            {
                                MessageBox.Alert(this, "Venda de outros DANONE impedida. Todos produtos LAYOUT devem ser vendidos.");
                                return;
                            }
                        }
                    }
                }
            }

            if (!chkPesquisaRapida.Checked ||
                LastActivity() == ActivitiesNames.SimulacaoPreco)
            {
                data.PutExtra("codProduto", produto.COD_PRODUTO);
                SetResult(Result.Ok, data);
                Finish();
            }
            else
            {
                RetornoDeProcuraProdutoIndenizao(produto);
            }
        }

        private bool? PedidoSugerido()
        {
            foreach (CSItemsPedido.CSItemPedido itemAtual in CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS)
            {
                if (itemAtual.IND_UTILIZA_QTD_SUGERIDA)
                    return true;
            }

            return false;
        }

        private bool? TodosProdutosSugeridosVendidos()
        {
            var listaProdutos = CSProdutos.Items;
            List<CSProdutos.CSProduto> listaProdutosCategoria = new List<CSProdutos.CSProduto>();
            //bool especificoCategoria = false;

            foreach (CSProdutos.CSProduto produtoAtual in listaProdutos)
            {
                if (listaProdutosCategoria.Cast<CSProdutos.CSProduto>().Where(p => p.COD_PRODUTO == produtoAtual.COD_PRODUTO).ToList().Count() == 0)
                {
                    //if (CSEmpresa.Current.IND_CLUSTER_PRODUTOS_PELO_TIPO_FREEZER)
                    //    especificoCategoria = CSProdutos.CSProduto.CSCategoria.GetEspecificoCategoria(CSPDVs.Current.COD_DENVER, produtoAtual.COD_PRODUTO);
                    //else
                    //    especificoCategoria = CSProdutos.CSProduto.CSCategoria.GetEspecificoCategoria(CSPDVs.Current.COD_CATEGORIA, produtoAtual.COD_PRODUTO);

                    if (produtoAtual.IND_PROD_ESPECIFICO_CATEGORIA &&
                       (produtoAtual.PRECOS_PRODUTO != null && produtoAtual.PRECOS_PRODUTO.Count > 0))
                    {
                        listaProdutosCategoria.Add(produtoAtual);

                        if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.PRODUTO.COD_PRODUTO == produtoAtual.COD_PRODUTO && p.STATE != ObjectState.DELETADO).ToList().Count() == 0)
                            return false;
                    }
                    //else
                    //    produtoAtual.IND_PROD_ESPECIFICO_CATEGORIA = false;
                }
            }

            return true;
        }

        private void RetornoDeProcuraProdutoIndenizao(CSProdutos.CSProduto produto)
        {
            int codProdutoEncontrado = produto.COD_PRODUTO;
            var produtoJaIndenizado = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Items.Cast<CSItemsIndenizacao.CSItemIndenizacao>().Where(p => p.STATE != ObjectState.DELETADO && p.PRODUTO.COD_PRODUTO == codProdutoEncontrado).FirstOrDefault();

            if (produtoJaIndenizado != null)
            {
                CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current = produtoJaIndenizado;
                CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.STATE = ObjectState.ALTERADO;

                if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.PRODUTO != null)
                    CSProdutos.Current = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.PRODUTO;

                Intent i = new Intent();
                i.SetClass(this, typeof(ProdutoIndenizacao));
                i.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                StartActivity(i);
            }
            else
            {
                frmProdutos_OnProdutoEncontrado(produto);
            }
        }

        public override void OnBackPressed()
        {
            SetResult(Result.Ok);
            base.OnBackPressed();
        }

        private bool IsBunge()
        {
            return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 3;
        }

        private bool ValidarDadosBunge(CSProdutos.CSProduto produto)
        {
            bool retorno;

            try
            {
                CSPoliticaBunge pricingBunge = new CSPoliticaBunge(produto.COD_PRODUTO, CSEmpresa.Current.COD_NOTEBOOK1, CSPDVs.Current.COD_PDV, DateTime.Now, produto, 1, 0, 0m, null);
                pricingBunge.DadosIniciais();
                retorno = true;
            }
            catch (Exception ex)
            {
                retorno = false;
                MessageBox.Alert(this, ex.Message);
            }

            return retorno;
        }

        private void frmProdutos_OnProdutoEncontrado(CSProdutos.CSProduto produto)
        {
            if (produto == null)
            {
                MessageBox.ShowShortMessageCenter(this, "Falha na busca do produto");
                return;
            }

            if (!produto.IND_ITEM_COMBO)
            {
                SelecionarProduto(produto);
            }
        }

        private void SelecionarProduto(CSProdutos.CSProduto current)
        {
            CSProdutos.Current = current;

            lvwProdutosIndenizacao_ItemActivate();
        }

        private void lvwProdutosIndenizacao_ItemActivate()
        {
            CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current = new CSItemsIndenizacao.CSItemIndenizacao();

            if (CSProdutos.Current.PRECOS_PRODUTO == null || CSProdutos.Current.PRECOS_PRODUTO.Count == 0)
            {

                if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 1 /*&& CSGlobal.CalculaPrecoNestle*/)
                    MessageBox.ShowShortMessageCenter(this, "Preço do produto não cadastrado.\r\nNão é possivel realizar esta venda.");
                else
                    MessageBox.ShowShortMessageCenter(this, "Cliente ou Produto com informações incompletas no cadastro Nestlê!\nNão é possivel realizar esta venda.");

                return;
            }

            if (CSProdutos.Current.PCT_TAXA_MAX_INDENIZACAO == 0)
            {
                MessageBox.Alert(this, "Taxa de indenização máxima deve ser maior que 0. Não é possível realizar a indenização.");
                return;
            }

            // [ Seta produto do item ]
            CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.PRODUTO = CSProdutos.Current;

            bool novoItem = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.STATE == ObjectState.NOVO;

            // Mostra a tela de indenização
            Intent i = new Intent();
            i.SetClass(this, typeof(ProdutoIndenizacao));
            i.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
            StartActivity(i);
        }

        private void AtualizaCampoQuantidade()
        {
            txtQtdeInteiro.Text = "";
            txtQtdeInteiro.Visibility = ViewStates.Gone;
            CSGlobal.QtdeItemCombo = 0;
            CSGlobal.PedidoComCombo = false;
        }

        private bool IsBroker()
        {
            return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
        }

        private bool ValidaItemCombo(int position)
        {
            // Produto selecionado 
            var produto = (CSProdutos.CSProduto)ExpandableListView.GetItemAtPosition(position);

            CSGlobal.PedidoComCombo = false;

            if (produto.IND_ITEM_COMBO)
            {
                List<string> listaBloqueio = produto.ValidaTabelaPrecoCombo(produto.COD_PRODUTO);

                if (produto.IND_STATUS_COMBO.ToUpper().Trim() == "I")
                {
                    MessageBox.ShowShortMessageCenter(this, "Combo Inativo.");
                    return false;
                }
                else if ((produto.DAT_VALIDADE_INICIO_COMBO > CSEmpresa.Current.DATA_ENTREGA ||
                     produto.DAT_VALIDADE_TERMINO_COMBO < CSEmpresa.Current.DATA_ENTREGA))
                {
                    MessageBox.ShowShortMessageCenter(this, " Pedido fora do período de validade do combo.");
                    return false;
                }
                else if (((CSGlobal.StrToInt(txtQtdeInteiro.Text) + produto.QuantidadeVendida(produto)) > produto.QTD_MAXIMA_COMBO_PEDIDO))
                {
                    int saldo = produto.QTD_MAXIMA_COMBO_PEDIDO - produto.QuantidadeVendida(produto);

                    if (saldo > 0)
                        MessageBox.ShowShortMessageCenter(this, "Quantidade máxima de venda: (" + produto.QTD_MAXIMA_COMBO_PEDIDO + ")\nDisponível: (" + saldo + ")");
                    else
                        MessageBox.ShowShortMessageCenter(this, "Quantidade de venda máxima do combo atingida.");

                    return false;
                }
                else if (!CSPDVs.Current.PermiteVendaCombo(produto.COD_PRODUTO))
                {
                    MessageBox.ShowShortMessageCenter(this, "Venda deste combo não permitido para este cliente.");
                    return false;
                }
                //else if (!(CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2))
                //{
                //    this.ShowShortMessageCenter("Tabela de preço não se encontra cadastrada.");
                //    return false;
                //}
                else if (listaBloqueio.Count > 0 &&
                         !IsBroker())
                {
                    string msg = string.Empty;

                    for (int i = 0; i < listaBloqueio.Count; i++)
                    {
                        msg += listaBloqueio[i];

                        if (i + 1 < listaBloqueio.Count)
                            msg += ", ";
                    }

                    MessageBox.Alert(this, "Os produtos a seguir não possuem suas respectivas tabelas de preço cadastradas: " + msg);
                    return false;
                }
                else
                {
                    if (IsBroker())
                    {
                        foreach (CSProdutos.CSProduto produtoAtual in CSProdutos.Items)
                        {
                            if (produtoAtual.COD_PRODUTO_CONJUNTO == produto.COD_PRODUTO)
                            {
                                if (produtoAtual.PRECOS_PRODUTO == null || produtoAtual.PRECOS_PRODUTO.Count == 0)
                                {
                                    MessageBox.Alert(this, "Cliente ou Produto (" + produtoAtual.COD_PRODUTO + ") com informações incompletas no cadastro Nestlê!\nNão é possivel realizar esta venda.");
                                    return false;
                                }

                                if (!CSProdutos.GetProdutoPoliticaBroker(produtoAtual.COD_PRODUTO, CSPDVs.Current.COD_PDV, produtoAtual.GRUPO_COMERCIALIZACAO.COD_SETOR_BROKER))
                                {
                                    MessageBox.Alert(this, "Cliente ou Produto (" + produtoAtual.COD_PRODUTO + ") com informações incompletas no cadastro Nestlê!\nNão é possivel realizar esta venda.");
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        private class ProcuraProdutoListAdapter : ArrayAdapter<CSProdutos.CSProduto>
        {
            Context context;
            int resourceId;
            IList<CSProdutos.CSProduto> produtos;
            public ProcuraProdutoListAdapter(Context c, int resId, IList<CSProdutos.CSProduto> prds)
                : base(c, resId, prds)
            {
                context = c;
                produtos = prds;
                resourceId = resId;
                ExistePedidoVendido = false;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var produto = produtos[position];
                LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                View linha = layout.Inflate(resourceId, null);

                TextView tvCodigo = linha.FindViewById<TextView>(Resource.Id.tvCodigo);
                TextView tvDescricao = linha.FindViewById<TextView>(Resource.Id.tvDescricao);
                TextView tvQuantidade = linha.FindViewById<TextView>(Resource.Id.tvQuantidade);

                ImageView imgProdEspecifico = linha.FindViewById<ImageView>(Resource.Id.imgProdEspecifico);
                imgProdEspecifico.Visibility = ViewStates.Visible;

                //bool especificoCategoria = false;

                if (CSPDVs.Current != null)
                {
                    //    if (CSEmpresa.Current.IND_CLUSTER_PRODUTOS_PELO_TIPO_FREEZER)
                    //        especificoCategoria = CSProdutos.CSProduto.CSCategoria.GetEspecificoCategoria(CSPDVs.Current.COD_DENVER, produto.COD_PRODUTO);
                    //    else
                    //        especificoCategoria = CSProdutos.CSProduto.CSCategoria.GetEspecificoCategoria(CSPDVs.Current.COD_CATEGORIA, produto.COD_PRODUTO);
                    //}

                    if (produto.IND_PROD_TOP_CATEGORIA)
                        imgProdEspecifico.SetImageResource(Resource.Drawable.circulo_verde_top);
                    else if (produto.IND_PROD_ESPECIFICO_CATEGORIA)
                        imgProdEspecifico.SetImageResource(Resource.Drawable.circulo_azul);
                    else
                        imgProdEspecifico.SetImageResource(Resource.Drawable.circulo_cinza);
                }
                //if (especificoCategoria)
                //    // Seta que foi encontrado produto da categoria
                //    imgProdEspecifico.SetImageResource(Resource.Drawable.circulo_azul);
                //else
                //    imgProdEspecifico.SetImageResource(Resource.Drawable.circulo_cinza);

                tvQuantidade.Visibility = ViewStates.Visible;
                tvCodigo.Text = produto.DESCRICAO_APELIDO_PRODUTO.Trim();
                tvDescricao.Text = ListarApelido ? produto.DSC_APELIDO_PRODUTO : produto.DSC_PRODUTO;

                if (CSPDVs.Current != null && CSPDVs.Current.PEDIDOS_PDV.Current != null && CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS != null)
                {
                    var produtoVendido = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Items.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.STATE != ObjectState.DELETADO && p.PRODUTO.COD_PRODUTO == produto.COD_PRODUTO).FirstOrDefault();
                    if (produtoVendido != null)
                    {
                        if (produtoVendido.PRODUTO.COD_UNIDADE_MEDIDA != "KG" && produtoVendido.PRODUTO.COD_UNIDADE_MEDIDA != "LT")
                        {
                            tvQuantidade.Text = string.Format("{0}/{1}", produtoVendido.QTD_PEDIDA_INTEIRA.ToString(), produtoVendido.QTD_PEDIDA_UNIDADE.ToString("###000"));
                        }
                        else
                        {
                            tvQuantidade.Text = produtoVendido.QTD_PEDIDA_INTEIRA.ToString(CSGlobal.DecimalStringFormat);
                        }

                        ExistePedidoVendido = true;
                        //tvCodigo.SetTextColor(Resource.Color.lightBlue);
                        //tvDescricao.SetTextColor(Resource.Color.lightBlue);
                        //tvQuantidade.SetTextColor(Resource.Color.lightBlue);
                    }
                }
                return linha;
            }

            public int GetCodProduto(int position)
            {
                return produtos[position].COD_PRODUTO;
            }

            public CSProdutos.CSProduto GetProduto(int position)
            {
                return produtos[position];
            }

        }

        private class ProcuraProdutoExpandableListViewAdapter : BaseExpandableListAdapter
        {
            private Context context;
            private int groupResourceId;
            private int childResourceId;
            private IList<CSProdutos.CSProduto> grupoProdutos;
            private Dictionary<int, CSProdutos.CSProduto[]> dicItensProdutos;


            public ProcuraProdutoExpandableListViewAdapter(Context c, int grpResId, int childResId, IList<CSProdutos.CSProduto> grpPrds, Dictionary<int, CSProdutos.CSProduto[]> itensProdutos)
            {
                context = c;
                groupResourceId = grpResId;
                childResourceId = childResId;
                grupoProdutos = grpPrds;
                dicItensProdutos = itensProdutos;
            }

            public override Java.Lang.Object GetChild(int groupPosition, int childPosition)
            {
                return dicItensProdutos[grupoProdutos[groupPosition].COD_PRODUTO];
            }

            public override long GetChildId(int groupPosition, int childPosition)
            {
                return dicItensProdutos[grupoProdutos[groupPosition].COD_PRODUTO][childPosition].COD_PRODUTO;
            }

            public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
            {
                var combo = dicItensProdutos[grupoProdutos[groupPosition].COD_PRODUTO];
                var produto = combo[childPosition];
                LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                View linha = layout.Inflate(childResourceId, null);

                TextView tvProduto = linha.FindViewById<TextView>(Resource.Id.tvProduto);
                TextView tvQuantidade = linha.FindViewById<TextView>(Resource.Id.tvQuantidade);

                tvQuantidade.Visibility = ViewStates.Visible;
                tvProduto.Text = produto.DESCRICAO_APELIDO_PRODUTO.Trim() + " - " + produto.DSC_PRODUTO.Trim();
                tvQuantidade.Text = CSProdutos.CSProduto.ConverteUnidadesParaMedida(produto.QTD_PRODUTO_COMPOSICAO, produto.COD_UNIDADE_MEDIDA, produto.QTD_UNIDADE_EMBALAGEM);

                return linha;
            }

            public override int GetChildrenCount(int groupPosition)
            {
                return dicItensProdutos[grupoProdutos[groupPosition].COD_PRODUTO].Count();
            }

            public override Java.Lang.Object GetGroup(int groupPosition)
            {
                return grupoProdutos[groupPosition];
            }

            public override long GetGroupId(int groupPosition)
            {
                return grupoProdutos[groupPosition].COD_PRODUTO;
            }

            public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
            {
                var produto = grupoProdutos[groupPosition];
                LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                View linha = layout.Inflate(groupResourceId, null);

                TextView tvCodigo = linha.FindViewById<TextView>(Resource.Id.tvCodigo);
                TextView tvDescricao = linha.FindViewById<TextView>(Resource.Id.tvDescricao);
                TextView tvQuantidade = linha.FindViewById<TextView>(Resource.Id.tvQuantidade);

                tvCodigo.Text = produto.DESCRICAO_APELIDO_PRODUTO.Trim();
                tvDescricao.Text = produto.DSC_PRODUTO;

                return linha;
            }

            public override int GroupCount
            {
                get { return grupoProdutos.Count; }
            }

            public override bool HasStableIds
            {
                get { return true; }
            }

            public override bool IsChildSelectable(int groupPosition, int childPosition)
            {
                return false;
            }

            public int GetCodProduto(int position)
            {
                return grupoProdutos[position].COD_PRODUTO;
            }

        }

        public bool OnEditorAction(TextView v, Android.Views.InputMethods.ImeAction actionId, KeyEvent e)
        {
            btnPesquisar_Click(null, null);
            return true;
        }
    }
}