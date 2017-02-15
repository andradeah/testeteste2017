using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using AvanteSales.SystemFramework;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "Produtos", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Produtos : AppCompatActivity
    {
        public static bool ExibirDescricaoProduto
        {
            get
            {
                string exibeProduto = CSConfiguracao.GetConfig("ExibirDescricaoProduto");

                if (exibeProduto.Length > 0)
                    return bool.Parse(exibeProduto);

                return false;
            }
        }
        TextView lblCodPdv;
        TextView lblNomePdv;
        Android.Support.V7.Widget.Toolbar tbToolbar;
        public static bool ItemClicado;
        private static CSFamiliasProduto.CSFamiliaProduto familia;
        private const int frmProdutoRamoAtividade = 0;
        private const int frmProdutoPedido = 1;
        private const int frmProcuraProduto = 2;
        private const int frmConsultaProdutoPedidos = 3;
        private const int dialogProduto = 4;
        private const int dialogDigitacaoCombo = 5;
        private const int frmProdutoIndenizao = 6;
        private const int frmOpcaoOrdenacaoProduto = 7;
        private static ProgressDialog progressDialog;
        //private static ProgressDialog progressDialogStart;
        private static AppCompatActivity CurrentActivity;
        //private CSItemsPedido.CSItemPedido backupItem;
        public static bool RealizouVendaCombo = false;
        //static CSProdutos.CSProduto produtoCombo = null;
        private static int COD_GRUPO;
        private static List<CSProdutos.CSProduto> produtosCarregadosBunge;
        private List<CSProdutos.CSProduto> ProdutosAdapter;
        #region [ Controles ]
        private static int COD_COMERCIALIZACAO;
        private TextView lblGrupoComercial;
        private static Spinner cboGrupoComercializacao;
        private static Spinner cboGrupoProduto;
        private static Spinner cboFamiliaProduto;
        private static CheckBox chkProdutosExclusivos;
        private LinearLayout headerListView;
        private TableRow trSearch;
        private TableRow trCheckBoxes;
        private static TextView tvHeaderValor;
        private LinearLayout HeaderListView;
        private ExpandableListView elvResultado;
        //private static bool thread_executando;
        //private int Repeticao = 0;
        ProdutosListViewBaseAdapter ProdutosListViewBaseAdapterProp;
        static ProgressDialog progressDialogFamilia;

        #endregion

        #region [ Variáveis ]

        static int codFamilia;
        static int codGrupo;
        static ListView lvwProdutos;
        private static bool m_ExecutandoMostraProdutos = false;
        private int m_NumeroProdutosPedido;
        //private static bool m_IndClicouFiltroEspCat = false;
        private static bool m_executarMontaProdutos = true;
        private bool m_IsDirty = false;
        private static bool CarregandoCombo;
        private static string txtDescontoIndenizacao;
        public static decimal txtAdf;
        //private bool itmCalculaPrecoChecked;
        private static int PositionItemVendido = 0;
        private int currentPostion;
        private int _SelectedItemIndex = -1;
        public int SelectedItemIndex
        {
            set
            {
                _SelectedItemIndex = value;
            }
            get
            {
                return _SelectedItemIndex;
            }
        }

        #endregion

        #region [ Propriedades ]

        private bool IsDirty
        {
            get
            {
                return m_IsDirty;
            }
            set
            {
                m_IsDirty = value;
            }
        }

        private int NumeroProdutosPedido
        {
            get
            {
                return m_NumeroProdutosPedido;
            }
            set
            {
                m_NumeroProdutosPedido = value;
            }
        }

        private CSGruposProduto.CSGrupoProduto CurrentItem { get; set; }

        #endregion

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                Produtos.ItemClicado = false;

                if (resultCode == Result.Ok)
                {
                    IsDirty = true;
                }

                switch (requestCode)
                {
                    case frmProdutoIndenizao:
                        RetornoDeProdutoIndenizacao(resultCode);
                        break;
                    case frmProcuraProduto:
                        if (resultCode == Result.Ok)
                        {
                            if (data != null)
                            {
                                RetornoDeProcuraProdutoIndenizao(data);
                            }
                            else
                            {
                                RetornoDeProdutoIndenizacao(resultCode);
                            }
                        }
                        break;
                    case frmProdutoRamoAtividade:
                        RetornoDeProdutoRamoAtividade();
                        break;
                    case dialogProduto:
                        {
                            RetornoDeDialogProduto(resultCode, data);
                        }
                        break;
                    case frmOpcaoOrdenacaoProduto:
                        {
                            if (resultCode == Result.Ok)
                            {
                                MostraProdutosComFiltrosSelecionados();
                            }
                        }
                        break;
                    default:
                        break;
                }
                base.OnActivityResult(requestCode, resultCode, data);
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-OnActivityResult", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void FiltrosProdutos()
        {
            try
            {
                COD_GRUPO = 0;
                COD_COMERCIALIZACAO = 0;

                if (cboGrupoComercializacao.Adapter.Count > 0)
                {
                    // Busca o grupo de comercializacao selecionado
                    var grupoComercializacao = (CSGruposComercializacao.CSGrupoComercializacao)((CSItemCombo)cboGrupoComercializacao.SelectedItem).Valor;
                    COD_COMERCIALIZACAO = grupoComercializacao.COD_GRUPO_COMERCIALIZACAO;
                }
                else
                    COD_COMERCIALIZACAO = 0;

                if (cboGrupoProduto.Adapter.Count > 0)
                {
                    //Busca o grupo selecionado
                    var grupo = (CSGruposProduto.CSGrupoProduto)((CSItemCombo)cboGrupoProduto.SelectedItem).Valor;
                    COD_GRUPO = grupo.COD_GRUPO;
                }
                else
                    COD_GRUPO = 0;

                // Busca a familia selecionada
                if (cboFamiliaProduto.Adapter == null)
                {
                    cboGrupoProduto_ItemSelected(null, null);
                }
                familia = (CSFamiliasProduto.CSFamiliaProduto)((CSItemCombo)cboFamiliaProduto.SelectedItem).Valor;
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-FiltrosProdutos", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void RetornoDeProcuraProdutoIndenizao(Android.Content.Intent data)
        {
            try
            {
                int codProdutoEncontrado = data.GetIntExtra("codProduto", -1);
                var produtoJaIndenizado = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Items.Cast<CSItemsIndenizacao.CSItemIndenizacao>().Where(p => p.STATE != ObjectState.DELETADO && p.PRODUTO.COD_PRODUTO == codProdutoEncontrado).FirstOrDefault();

                if (produtoJaIndenizado != null)
                {
                    CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current = produtoJaIndenizado;
                    CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.STATE = ObjectState.ALTERADO;

                    if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.PRODUTO != null)
                        CSProdutos.Current = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.PRODUTO;

                    Intent i = new Intent();
                    i.SetClass(this, typeof(ProdutoIndenizacao));
                    this.StartActivityForResult(i, frmProdutoPedido);
                }
                else
                {
                    var produto = CSProdutos.Items.Cast<CSProdutos.CSProduto>().Where(p => p.COD_PRODUTO == codProdutoEncontrado).FirstOrDefault();
                    frmProdutos_OnProdutoEncontrado(produto);
                }
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-RetornoDeProcuraProdutoIndenizacao", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void RetornoDeProdutoIndenizacao(Result resultCode)
        {
            try
            {
                if (resultCode == Result.Ok)
                {
                    MostraProdutosComFiltrosSelecionados();
                    lvwProdutos.SetSelection(PositionItemVendido);
                }

                // marca que nao existe nenhum produto mais selecionado
                CSProdutos.Current = null;
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-RetornoDeProdutoIndenizacao", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                base.OnCreate(bundle);

                ItemClicado = false;
                RealizouVendaCombo = false;
                SetContentView(Resource.Layout.produtos);

                CurrentActivity = this;

                FindViewsByIds();

                HeaderListView.Click += new EventHandler(HeaderListView_Click);
                cboGrupoComercializacao.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(cboGrupoComercializacao_ItemSelected);
                cboGrupoProduto.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(cboGrupoProduto_ItemSelected);
                cboFamiliaProduto.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(cboFamiliaProduto_ItemSelected);
                chkProdutosExclusivos.CheckedChange += new EventHandler<CompoundButton.CheckedChangeEventArgs>(chkProdutosExclusivos_CheckedChange);
                lvwProdutos.ItemClick += LvwProdutos_ItemClick;

                var grp = Intent.GetStringExtra("grupo");
                var fml = Intent.GetStringExtra("familia");

                if (!int.TryParse(grp, out codGrupo))
                    codGrupo = -1;

                if (!int.TryParse(fml, out codFamilia))
                    codFamilia = -1;

                ProdutosListViewBaseAdapterProp = new ProdutosListViewBaseAdapter(this);

                frmProdutos_Load();

                SetSupportActionBar(tbToolbar);
                SupportActionBar.SetHomeButtonEnabled(true);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetDisplayShowTitleEnabled(false);

                lblCodPdv.Text = CSPDVs.Current.COD_PDV.ToString();
                lblNomePdv.Text = CSPDVs.Current.DSC_RAZAO_SOCIAL;
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-OnCreate", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void LvwProdutos_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                if (ItemClicado)
                    return;

                ItemClicado = true;
                string alerta = string.Empty;

                //if (!CSEmpregados.Current.VendedorDentroExpediente(ref alerta))
                //{
                //    MessageBox.Alert(this, alerta, "OK",
                //        (_sender, _e) =>
                //        {
                //            SetResult(Result.FirstUser);
                //            base.Finish();
                //        }, false);
                //}
                //else
                //{
                this.SelectedItemIndex = e.Position;

                PositionItemVendido = e.Position;

                CSProdutos.Current = (CSProdutos.CSProduto)lvwProdutos.Adapter.GetItem(SelectedItemIndex);

                if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current == null)
                {
                    if (!CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDO_SUGERIDO.HasValue)
                        CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDO_SUGERIDO = PedidoSugerido();

                    if (CSGlobal.PedidoSugerido ||
                        CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDO_SUGERIDO.Value)
                    {
                        if (CSEmpresa.Current.IND_PERMITIR_VENDA_SOMENTE_LAYOUT == "S")
                        {
                            if (CSProdutos.Current.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO == 2)
                            {
                                MessageBox.Alert(this, "Permitido apenas produtos de Layout/CR.");
                            }
                            else
                                AbrirProduto();
                        }
                        else
                        {
                            if (CSProdutos.Current.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO == 2)
                            {
                                CSPDVs.Current.PEDIDOS_PDV.Current.TODOS_SUGERIDOS_VENDIDOS = TodosProdutosSugeridosVendidos();

                                if (CSPDVs.Current.PEDIDOS_PDV.Current.TODOS_SUGERIDOS_VENDIDOS.Value)
                                {
                                    int quantidadeProdutosTerceiros = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.PRODUTO.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO == 2 && !p.PRODUTO.IND_PROD_ESPECIFICO_CATEGORIA && p.STATE != ObjectState.DELETADO).Count();

                                    quantidadeProdutosTerceiros = CSProdutos.Current.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO == 2 ? quantidadeProdutosTerceiros + 1 : quantidadeProdutosTerceiros;

                                    if (quantidadeProdutosTerceiros > CSEmpresa.Current.QTD_MAX_VENDA_OUTROS_DANONE)
                                        MessageBox.Alert(this, string.Format("Quantidade máxima ({0}) de produtos DANONE atingida.", CSEmpresa.Current.QTD_MAX_VENDA_OUTROS_DANONE.ToString()));
                                    else
                                        AbrirProduto();
                                }
                                else
                                    MessageBox.Alert(this, "Venda de outros DANONE impedida. Todos produtos LAYOUT devem ser vendidos.");
                            }
                            else
                                AbrirProduto();
                        }
                    }
                    else
                        AbrirProduto();
                }
                else
                    AbrirProduto();
                //}
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-OnListItemClick", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        void chkExibirImagens_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                if (m_ExecutandoMostraProdutos)
                    return;

                SaveScrollPosition();

                MostraProdutosComFiltrosSelecionados();

                SetScrollPosition();
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-ChkExibirImagens", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        void HeaderListView_Click(object sender, EventArgs e)
        {
            try
            {
                string exibeProduto = CSConfiguracao.GetConfig("ExibirDescricaoProduto");

                if (exibeProduto.Length > 0)
                {
                    if (Convert.ToBoolean(exibeProduto) == true)
                        CSConfiguracao.SetConfig("ExibirDescricaoProduto", "false");
                    else
                        CSConfiguracao.SetConfig("ExibirDescricaoProduto", "true");
                }
                else
                    CSConfiguracao.SetConfig("ExibirDescricaoProduto", "false");

                if (m_ExecutandoMostraProdutos)
                    return;

                SaveScrollPosition();

                MostraProdutosComFiltrosSelecionados();

                SetScrollPosition();
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-HeaderListView", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void AlterarVisibilidadeCabecalho(ViewStates visibilityState)
        {
            try
            {
                lblGrupoComercial.Visibility = visibilityState;
                cboGrupoComercializacao.Visibility = visibilityState;
                cboGrupoProduto.Visibility = visibilityState;
                cboFamiliaProduto.Visibility = visibilityState;
                lvwProdutos.Visibility = visibilityState;

                if (CSProdutos.Existe_Produto_Categoria_Exclusiva)
                    chkProdutosExclusivos.Visibility = visibilityState;
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-AlterarVisibilidadeCabecalho", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        void cboFamiliaProduto_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                if (cboFamiliaProduto.Adapter == null)
                    return;

                if (cboFamiliaProduto.Adapter.Count > 0 && cboGrupoComercializacao.Adapter.Count > 0)
                {
                    MostraProdutosComFiltrosSelecionados();
                }
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-CboFamiliaProdutoItemSelected", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        void cboGrupoComercializacao_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                if (cboFamiliaProduto.Adapter == null ||
                    CarregandoCombo)
                    return;

                if (cboFamiliaProduto.Adapter.Count > 0 && cboGrupoComercializacao.Adapter.Count > 0)
                {
                    CarregandoCombo = true;
                    progressDialog = new ProgressDialog(this);
                    progressDialog.SetTitle("Processando...");
                    progressDialog.SetCancelable(false);
                    progressDialog.SetMessage("Carregando grupos...");
                    progressDialog.Show();

                    new ThreadGrupoProduto().Execute();
                }
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-CboGrupoComercializacao", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void FindViewsByIds()
        {
            lblCodPdv = FindViewById<TextView>(Resource.Id.lblCodPdv);
            lblNomePdv = FindViewById<TextView>(Resource.Id.lblNomePdv);
            tbToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tbToolbar);
            lvwProdutos = FindViewById<ListView>(Resource.Id.lvwProdutos);
            lblGrupoComercial = FindViewById<TextView>(Resource.Id.lblGrupoComercial);
            cboGrupoComercializacao = FindViewById<Spinner>(Resource.Id.cboGrupoComercializacao);
            cboGrupoProduto = FindViewById<Spinner>(Resource.Id.cboGrupoProduto);
            cboFamiliaProduto = FindViewById<Spinner>(Resource.Id.cboFamiliaProduto);
            chkProdutosExclusivos = FindViewById<CheckBox>(Resource.Id.chkProdutosExclusivos);
            headerListView = FindViewById<LinearLayout>(Resource.Id.HeaderListView);
            trSearch = FindViewById<TableRow>(Resource.Id.trSearch);
            trCheckBoxes = FindViewById<TableRow>(Resource.Id.trCheckBoxes);
            HeaderListView = FindViewById<LinearLayout>(Resource.Id.HeaderListView);
            var view = LayoutInflater.Inflate(Resource.Layout.produtos_header, null);
            HeaderListView.AddView(view);
            tvHeaderValor = HeaderListView.FindViewById<TextView>(Resource.Id.tvHeaderValor);
            elvResultado = FindViewById<ExpandableListView>(Resource.Id.elvResultado);
        }

        protected override void OnStart()
        {
            try
            {
                base.OnStart();

                if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current != null)
                    SelecionarGrupoComercializacaoIndenizacao();
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-OnStart", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void SelecionarGrupoComercializacaoIndenizacao()
        {
            try
            {
                CSGruposComercializacao classeGrupoComercializacao = new CSGruposComercializacao();
                var grupos = classeGrupoComercializacao.GrupoComercializacaoFiltrado();
                int i = 0;

                foreach (CSGruposComercializacao.CSGrupoComercializacao grp in grupos)
                {
                    if (grp.COD_GRUPO_COMERCIALIZACAO_FILTRADO == CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.COD_GRUPO_COMERCIALIZACAO)
                        cboGrupoComercializacao.SetSelection(i);

                    i++;
                }

                cboGrupoComercializacao.Enabled = false;
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-SelecionarGrupoComercializacaoIndenizacao", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            base.OnCreateOptionsMenu(menu);
            MenuInflater inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.menu_produtos, menu);
            return true;
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            try
            {
                switch (item.ItemId)
                {
                    case Android.Resource.Id.Home:
                        this.Finish();
                        return true;
                    case Resource.Id.itmPesquisar:
                        mnuPesquisar_Click();
                        return true;
                    default:
                        return base.OnOptionsItemSelected(item);
                }
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-OnOptionsItemSelected", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                return false;
            }
        }
        public override void OnBackPressed()
        {
            try
            {
                if (DismissPopupSearch())
                    return;

                base.OnBackPressed();

            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-OnBackPressed", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private bool DismissPopupSearch()
        {
            try
            {
                bool retorno = false;
                if (trSearch.Visibility == ViewStates.Visible)
                    retorno = true;

                trSearch.Visibility = ViewStates.Gone;
                trCheckBoxes.Visibility = ViewStates.Visible;
                return retorno;
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-DismissPopupSearch", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                return false;
            }
        }

        private void RetornoDeProdutoRamoAtividade()
        {
            try
            {
                Fechar();
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-Fechar", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        public override bool OnSearchRequested()
        {
            try
            {
                mnuPesquisar_Click();
                return true;
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-OnSearchedRequested", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                return false;
            }
        }

        private void mnuPesquisar_Click()
        {
            try
            {
                Intent i = new Intent();
                i.SetClass(this, typeof(ProcuraProduto));
                i.PutExtra("ultimaActivity", (int)ActivitiesNames.Produtos);
                this.StartActivityForResult(i, frmProcuraProduto);
            }
            catch (Exception ex)
            {
                MessageBox.AlertErro(this, ex.Message);
            }
        }

        private bool? PedidoSugerido()
        {
            try
            {
                foreach (CSItemsPedido.CSItemPedido itemAtual in CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS)
                {
                    if (itemAtual.IND_UTILIZA_QTD_SUGERIDA)
                        return true;
                }

                return false;
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-PedidoSugerido", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                return false;
            }
        }

        private bool? TodosProdutosSugeridosVendidos()
        {
            try
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
            catch (Exception ex)
            {
                //CSGlobal.GravarLog("Produto-TodosProdutosSugeridosVendidos", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                return false;
            }
        }

        private void AbrirProduto()
        {
            try
            {
                if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current == null &&
                    CSEmpresa.ColunaExiste("GRUPO_COMERCIALIZACAO", "IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO"))
                {
                    if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Count > 0)
                    {
                        if (CSProdutos.Current.GRUPO_COMERCIALIZACAO.IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO.ToUpper() == "S")
                        {
                            if (CSProdutos.Current.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO != CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS[0].PRODUTO.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO)
                                MessageBox.Alert(this, string.Format("Venda impedida. O grupo de comercialização \"{0}\" só pode ser vendido com produtos do mesmo grupo.", CSProdutos.Current.GRUPO_COMERCIALIZACAO.DES_GRUPO_COMERCIALIZACAO));
                            else
                                AbrirDialogProduto();
                        }
                        else
                        {
                            if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS[0].PRODUTO.GRUPO_COMERCIALIZACAO.IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO.ToUpper() == "S")
                            {
                                if (CSProdutos.Current.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO != CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS[0].PRODUTO.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO)
                                    MessageBox.Alert(this, string.Format("Venda impedida. Somente são permitidos produtos com grupo de comercialiação {0}.", CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS[0].PRODUTO.GRUPO_COMERCIALIZACAO.DES_GRUPO_COMERCIALIZACAO));
                                else
                                    AbrirDialogProduto();
                            }
                            else
                                AbrirDialogProduto();
                        }
                    }
                    else
                        AbrirDialogProduto();
                }
                else
                    AbrirDialogProduto();
            }
            catch (Exception ex)
            {
                //CSGlobal.GravarLog("Produto-AbrirProduto", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void AbrirDialogProduto()
        {
            try
            {
                Intent i = new Intent();
                i.SetClass(this, typeof(DialogProdutoComum));

                if (!IsBunge())
                {
                    if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current != null)
                    {
                        if (CSProdutos.Current.PCT_TAXA_MAX_INDENIZACAO == 0)
                        {
                            MessageBox.Alert(this, "Taxa de indenização máxima deve ser maior que 0. Não é possível realizar a indenização.");
                            ItemClicado = false;
                            return;
                        }
                    }
                }
                else
                {
                    if (!ValidarDadosBunge(CSProdutos.Current))
                    {
                        ItemClicado = false;
                        return;
                    }
                }

                i.PutExtra("ultimasVisitasHabilitada", ProdutoFoiVendidoNasUltimasVisitas());
                i.PutExtra("vendaHabilidada", !CSGlobal.PedidoComCombo);
                this.StartActivityForResult(i, dialogProduto);
                //StartActivityForResult(i, dialogProduto);
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-AbrirDialogProduto", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
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
                MessageBox.AlertErro(this, ex.Message);

                //CSGlobal.GravarLog("Produto-ValidarDadosBunge", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }

            return retorno;
        }

        private bool IsBunge()
        {
            return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 3;
        }

        private bool ProdutoFoiVendidoNasUltimasVisitas()
        {
            try
            {
                foreach (CSUltimasVisitasPDV.CSUltimaVisitaPDV pedido in CSPDVs.Current.ULTIMAS_VISITAS.Items)
                {
                    // Seta qual é o pedido atual
                    CSPDVs.Current.ULTIMAS_VISITAS.Current = pedido;
                    var itempedido = CSPDVs.Current.ULTIMAS_VISITAS.Current.ITEMS_PEDIDOS.Items.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.PRODUTO.COD_PRODUTO == CSProdutos.Current.COD_PRODUTO).FirstOrDefault();
                    if (itempedido != null)
                        return true;
                }
                return false;
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-ProdutoFoiVendidoNasUltimasVisitas", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                return false;
            }
        }

        private void RetornoDeDialogProduto(Result resultCode, Android.Content.Intent data)
        {
            try
            {
                switch (resultCode)
                {
                    case Result.Canceled:
                        break;
                    case Result.Ok:
                        {
                            if (data != null &&
                                data.GetIntExtra("qtdVendaRapida", 0) == 0)
                            {
                                if (!data.GetBooleanExtra("irParaVenda", false))
                                {
                                    ReorganizarProdutos(PositionItemVendido);
                                    //MostraProdutosComFiltrosSelecionados();
                                    lvwProdutos.SetSelection(PositionItemVendido);
                                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current = null;
                                }
                            }
                            else
                                SelecionarProduto(data == null ? 0 : data.GetIntExtra("qtdVendaRapida", 0));
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-RetornoDeDialogProduto", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void ReorganizarProdutos(int positionItem)
        {
            ProdutosAdapter.RemoveAt(positionItem);
            ProdutosListViewBaseAdapterProp.NotifyDataSetChanged();
        }

        private bool IsBroker()
        {
            try
            {
                return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-IsBroker", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                return false;
            }
        }

        private void frmProdutos_OnProdutoEncontrado(CSProdutos.CSProduto produto)
        {
            try
            {
                if (produto == null)
                {
                    MessageBox.AlertErro(this, "Falha na busca do produto");
                    return;
                }

                //if (IsBunge())
                //{
                //    if (!ValidarDadosBunge(produto))
                //        return;
                //}
                CSGruposComercializacao.CSGrupoComercializacao grupoComercializacao = null;
                m_executarMontaProdutos = false;

                if (!produto.IND_ITEM_COMBO)
                {
                    // [ Seleciona combo de grupo de comercializacao ]                
                    int i;
                    for (i = 0; i < cboGrupoComercializacao.Adapter.Count; i++)
                    {
                        grupoComercializacao = (CSGruposComercializacao.CSGrupoComercializacao)((CSItemCombo)cboGrupoComercializacao.Adapter.GetItem(i)).Valor;
                        if (grupoComercializacao.COD_GRUPO_COMERCIALIZACAO == produto.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO)
                            break;
                    }

                    // [ Seleciona combos de grupo de família ]
                    cboGrupoComercializacao.SetSelection(i);

                    cboFamiliaProduto.SetSelection(cboFamiliaProduto.Adapter.Count - 1);

                    //Configura check chkProdutosExclusivos conforme propriedades do produto
                    //chkProdutosExclusivos.Checked = produto.IND_PROD_ESPECIFICO_CATEGORIA;

                    m_executarMontaProdutos = true;

                    // Mostra os filtros conforme o produto encontrado na pesquisa
                    MostraProdutos(produto.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO, produto.GRUPO.COD_GRUPO, -1, produto.COD_PRODUTO);
                    //Abre a tela de vendas apos retornar da pesquisa de produto
                    SelecionarProduto(produto);
                }
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-OnProdutoEncontrado", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void frmProdutos_Load()
        {
            try
            {
                if (codFamilia == -1 ||
                    codGrupo == -1)
                {
                    txtDescontoIndenizacao = Intent.GetStringExtra("txtDescontoIndenizacao");
                    txtAdf = CSGlobal.StrToDecimal(Intent.GetStringExtra("txtAdf"));
                }

                trSearch.Visibility = ViewStates.Gone;
                m_ExecutandoMostraProdutos = true;

                ExibeCheckProdutosExclusivos(CSProdutos.Existe_Produto_Categoria_Exclusiva);

                if (CSEmpregados.Current.IND_PERMITIR_ROTINA_IMAGEM)
                {
                    // [ Verifica se é para exibir a coluna de descrição do produto ]
                    string exibeImagens = CSConfiguracao.GetConfig("ExibirImagem");
                }

                cboGrupoComercializacao.Adapter = null;
                cboGrupoProduto.Adapter = null;
                cboFamiliaProduto.Adapter = null;

                progressDialog = new ProgressDialog(this);
                progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
                progressDialog.SetTitle("Processando...");
                progressDialog.SetCancelable(false);
                progressDialog.SetMessage("Carregando filtros...");
                progressDialog.Show();

                new ThreadGrupoComercializacao().Execute();
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-FrmProdutosLoad", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private class ThreadGrupoProduto : AsyncTask
        {
            ArrayAdapter adapter;
            int position = 0;

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                CarregaComboBoxGrupoProduto();

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                base.OnPostExecute(result);

                cboGrupoProduto.Adapter = adapter;

                if (cboGrupoProduto.Adapter.Count > 0)
                    cboGrupoProduto.SetSelection(position);

                new ThreadCarregarFamiliaProduto().Execute();
            }

            private void CarregaComboBoxGrupoProduto()
            {
                try
                {
                    adapter = new ArrayAdapter(CurrentActivity, Android.Resource.Layout.SimpleSpinnerItem);
                    adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

                    int grupoComercializacao = ((CSGruposComercializacao.CSGrupoComercializacao)((CSItemCombo)cboGrupoComercializacao.SelectedItem).Valor).COD_GRUPO_COMERCIALIZACAO_FILTRADO;
                    int i = 0;
                    CSGruposProduto classeGrupoProduto = new CSGruposProduto();

                    if (grupoComercializacao == 0 && cboGrupoComercializacao.Adapter.Count > 0)
                        grupoComercializacao = -1;

                    var grupos = classeGrupoProduto.GrupoProdutoFiltrado(grupoComercializacao, CSPDVs.Current.COD_PDV, CSPDVs.Current.COD_CATEGORIA, CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA, CSEmpresa.Current.IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE);

                    // Preenche o combo de grupos
                    foreach (CSGruposProduto.CSGrupoProduto grp in grupos)
                    {
                        CSItemCombo ic = new CSItemCombo();
                        //ic.Texto = grp.DSC_GRUPO_FILTRADO;
                        grp.DSC_GRUPO = grp.DSC_GRUPO_FILTRADO;
                        grp.COD_GRUPO = grp.COD_GRUPO_FILTRADO;
                        ic.Texto = grp.DSC_GRUPO;
                        ic.Valor = grp;
                        adapter.Add(ic);

                        if (codGrupo != -1)
                        {
                            if (grp.COD_GRUPO == codGrupo)
                                position = i;
                        }

                        i++;
                    }
                }
                catch (Exception)
                {
                    //CSGlobal.GravarLog("Produto-CarregarComboBoxGrupoProduto", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                }
            }
        }

        private class ThreadGrupoComercializacao : AsyncTask
        {
            ArrayAdapter adapter;
            int positionSelection = -1;

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                CarregandoCombo = true;
                CarregaComboBoxGrupoComercializacao();

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                base.OnPostExecute(result);

                cboGrupoComercializacao.Adapter = adapter;

                if (cboGrupoComercializacao.Adapter.Count > 0)
                {
                    if (positionSelection == -1)
                        cboGrupoComercializacao.SetSelection(cboGrupoComercializacao.Adapter.Count - 1);
                    else
                        cboGrupoComercializacao.SetSelection(positionSelection);
                }

                new ThreadGrupoProduto().Execute();
            }

            private void CarregaComboBoxGrupoComercializacao()
            {
                try
                {
                    adapter = new ArrayAdapter(CurrentActivity, Android.Resource.Layout.SimpleSpinnerItem);
                    adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

                    CSGruposComercializacao classeGrupoComercializacao = new CSGruposComercializacao();
                    var grupos = classeGrupoComercializacao.GrupoComercializacaoFiltrado();

                    // Preenche o combo
                    int i = 0;
                    foreach (CSGruposComercializacao.CSGrupoComercializacao grp in grupos)
                    {
                        CSItemCombo ic = new CSItemCombo();
                        //ic.Texto = grp.DES_GRUPO_COMERCIALIZACAO_FILTRADO;
                        grp.DES_GRUPO_COMERCIALIZACAO = grp.DES_GRUPO_COMERCIALIZACAO_FILTRADO;
                        grp.COD_GRUPO_COMERCIALIZACAO = grp.COD_GRUPO_COMERCIALIZACAO_FILTRADO;
                        ic.Texto = grp.DES_GRUPO_COMERCIALIZACAO;
                        ic.Valor = grp;
                        adapter.Add(ic);

                        if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current != null)
                        {
                            if (grp.COD_GRUPO_COMERCIALIZACAO == CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.COD_GRUPO_COMERCIALIZACAO)
                                positionSelection = i;
                        }

                        i++;
                    }

                    // Adiciona um opção para selecionar todos os grupos
                    CSGruposComercializacao.CSGrupoComercializacao grptodos = new CSGruposComercializacao.CSGrupoComercializacao();
                    grptodos.COD_GRUPO_COMERCIALIZACAO_FILTRADO = -1;
                    grptodos.COD_GRUPO_COMERCIALIZACAO = -1;
                    grptodos.DES_GRUPO_COMERCIALIZACAO_FILTRADO = "==== TODOS ====";
                    grptodos.DES_GRUPO_COMERCIALIZACAO = "==== TODOS ====";
                    grptodos.COD_SETOR_BROKER = "";

                    CSItemCombo ictodos = new CSItemCombo();
                    ictodos.Texto = grptodos.DES_GRUPO_COMERCIALIZACAO_FILTRADO;
                    ictodos.Valor = grptodos;
                    adapter.Add(ictodos);
                }
                catch (Exception)
                {
                    //CSGlobal.GravarLog("Produto-CarregaComboBoxGrupoComercializacao", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                }
            }
        }

        private static void ExibeCheckProdutosExclusivos(bool exibir)
        {
            try
            {
                if (exibir)
                    chkProdutosExclusivos.Visibility = ViewStates.Visible;
                else
                    chkProdutosExclusivos.Visibility = ViewStates.Gone;
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-ExibeCheckProdutosExclusivos", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void cboGrupoProduto_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                if (CarregandoCombo)
                    return;

                progressDialogFamilia = new ProgressDialog(this);
                progressDialogFamilia.SetTitle("Processando...");
                progressDialogFamilia.SetCancelable(false);
                progressDialogFamilia.SetMessage("Carregando famílias...");
                progressDialogFamilia.Show();

                new ThreadCarregarFamiliaProduto().Execute();
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-CboGrupoProdutoItemSelected", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private class ThreadCarregarFamiliaProduto : AsyncTask
        {
            ArrayAdapter adapter;
            int position = 0;

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                //m_IndClicouFiltroEspCat = false;

                int i = 0;

                if (cboGrupoProduto.Adapter.Count > 0)
                {
                    // Busca o grupo selecionado
                    CSGruposProduto.CSGrupoProduto grupo = (CSGruposProduto.CSGrupoProduto)((CSItemCombo)cboGrupoProduto.SelectedItem).Valor;
                    CSGruposComercializacao.CSGrupoComercializacao grupoComercializacao = (CSGruposComercializacao.CSGrupoComercializacao)((CSItemCombo)cboGrupoComercializacao.SelectedItem).Valor;

                    adapter = new ArrayAdapter(CurrentActivity, Android.Resource.Layout.SimpleSpinnerItem);
                    adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

                    CSFamiliasProduto classeFamilia = new CSFamiliasProduto();
                    var familia = classeFamilia.FamiliaFiltrada(grupo.COD_GRUPO, grupoComercializacao.COD_GRUPO_COMERCIALIZACAO, CSPDVs.Current.COD_PDV, CSPDVs.Current.COD_CATEGORIA, CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA, CSEmpresa.Current.IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE);

                    foreach (CSFamiliasProduto.CSFamiliaProduto fam in familia)
                    {
                        if (fam.GRUPO.COD_GRUPO == grupo.COD_GRUPO)
                        {
                            CSItemCombo ic = new CSItemCombo();
                            fam.DSC_FAMILIA_PRODUTO = fam.DSC_FAMILIA_PRODUTO_FILTRADO;
                            fam.COD_FAMILIA_PRODUTO = fam.COD_FAMILIA_PRODUTO_FILTRADO;
                            ic.Texto = fam.DSC_FAMILIA_PRODUTO;
                            ic.Valor = fam;
                            adapter.Add(ic);

                            if (codFamilia != -1)
                            {
                                if (fam.COD_FAMILIA_PRODUTO == codFamilia)
                                    position = i;
                            }


                            i++;
                        }
                    }

                    // Adiciona um opção para selecionar todos as produtos da familia
                    CSFamiliasProduto.CSFamiliaProduto famtodos = new CSFamiliasProduto.CSFamiliaProduto();
                    famtodos.GRUPO = grupo;
                    famtodos.COD_FAMILIA_PRODUTO = -1;
                    famtodos.COD_FAMILIA_PRODUTO_FILTRADO = -1;
                    famtodos.DSC_FAMILIA_PRODUTO = "==== TODOS ====";
                    famtodos.DSC_FAMILIA_PRODUTO_FILTRADO = "==== TODOS ====";

                    CSItemCombo ictodos = new CSItemCombo();
                    ictodos.Texto = famtodos.DSC_FAMILIA_PRODUTO;
                    ictodos.Valor = famtodos;
                    adapter.Add(ictodos);
                }
                else
                {

                }

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                base.OnPostExecute(result);

                if (adapter != null)
                    cboFamiliaProduto.Adapter = adapter;

                CarregandoCombo = false;

                // Coloca como default a opção todos os produtos da familia.
                if (adapter != null)
                {
                    if (cboFamiliaProduto.Adapter != null &&
                        cboFamiliaProduto.Adapter.Count > 0)
                        cboFamiliaProduto.SetSelection(codFamilia != -1 ? position : cboFamiliaProduto.Adapter.Count - 1);
                }
                else
                {
                    if (cboFamiliaProduto.Adapter != null &&
                        cboFamiliaProduto.Adapter.Count > 0)
                    {
                        int novaPosition = cboFamiliaProduto.SelectedItemPosition + 1;

                        if (novaPosition > (cboFamiliaProduto.Adapter.Count - 1))
                            cboFamiliaProduto.SetSelection(0);
                        else
                            cboFamiliaProduto.SetSelection(novaPosition);
                    }
                }


                if (progressDialog != null)
                    progressDialog.Dismiss();

                if (progressDialogFamilia != null)
                    progressDialogFamilia.Dismiss();
            }
        }

        void chkProdutosExclusivos_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                //m_IndClicouFiltroEspCat = true;

                //if (!m_ExecutandoMostraProdutos)
                //    m_IndClicouFiltroEspCat = true;

                if (m_ExecutandoMostraProdutos)
                    return;

                MostraProdutosComFiltrosSelecionados();
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-ChkProdutosExclusivosCheckedChange", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void MostraProdutosComFiltrosSelecionados()
        {
            try
            {
                progressDialog = new ProgressDialog(this);
                progressDialog.SetTitle("Processando...");
                progressDialog.SetCancelable(false);
                progressDialog.SetMessage("Carregando produtos.");
                progressDialog.Show();

                ThreadPool.QueueUserWorkItem(o => CarregarProdutos());
            }
            catch (Exception ex)
            {
                //CSGlobal.GravarLog("Produto-MostraProdutosComFiltrosSelecionados", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                MessageBox.AlertErro(this, ex.Message);
            }
        }

        private void CarregarProdutos()
        {
            try
            {
                COD_GRUPO = 0;
                COD_COMERCIALIZACAO = 0;

                if (cboGrupoComercializacao.Adapter.Count > 0)
                {
                    // Busca o grupo de comercializacao selecionado
                    var grupoComercializacao = (CSGruposComercializacao.CSGrupoComercializacao)((CSItemCombo)cboGrupoComercializacao.SelectedItem).Valor;
                    COD_COMERCIALIZACAO = grupoComercializacao.COD_GRUPO_COMERCIALIZACAO;
                }
                else
                    COD_COMERCIALIZACAO = 0;

                if (cboGrupoProduto.Adapter.Count > 0)
                {
                    //Busca o grupo selecionado
                    var grupo = (CSGruposProduto.CSGrupoProduto)((CSItemCombo)cboGrupoProduto.SelectedItem).Valor;
                    COD_GRUPO = grupo.COD_GRUPO;
                }
                else
                    COD_GRUPO = 0;

                // Busca a familia selecionada
                if (cboFamiliaProduto.Adapter == null)
                {
                    cboGrupoProduto_ItemSelected(null, null);
                }
                familia = (CSFamiliasProduto.CSFamiliaProduto)((CSItemCombo)cboFamiliaProduto.SelectedItem).Valor;

                MostraProdutos(COD_COMERCIALIZACAO, COD_GRUPO, familia.COD_FAMILIA_PRODUTO, 0);

                if (progressDialog != null)
                    progressDialog.Dismiss();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private class ThreadPreparaProdutos : AsyncTask<int, int, decimal>
        {
            protected override decimal RunInBackground(params int[] @params)
            {
                //thread_executando = true;
                MostraProdutos(COD_COMERCIALIZACAO, COD_GRUPO, familia.COD_FAMILIA_PRODUTO, 0);

                return 0;
            }

            protected override void OnPostExecute(decimal result)
            {
                try
                {
                    if (Produtos.progressDialog != null)
                    {
                        Produtos.progressDialog.Dismiss();
                        Produtos.progressDialog.Dispose();
                    }
                }
                catch (Exception)
                {
                    //CSGlobal.GravarLog("Produto-OnPostExecute", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                }
                finally
                {
                    //thread_executando = false;
                    base.OnPostExecute(result);
                }
            }

            private void MostraProdutos(int COD_GRUPO_COMERCIALIZACAO, int COD_GRUPO, int COD_FAMILIA_PRODUTO, int COD_PRODUTO)
            {
                try
                {
                    if (!m_executarMontaProdutos)
                        return;

                    m_ExecutandoMostraProdutos = true;

                    CurrentActivity.RunOnUiThread(() =>
                    {
                        // Limpa o list dos produtos
                        lvwProdutos.Adapter = null;
                    });

                    if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current == null)
                    {
                        //Ordena os pedidos ja realizados
                        CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Sort(!ExibirDescricaoProduto);
                    }

                    List<CSProdutos.CSProduto> prodFiltrados;
                    List<CSProdutos.CSProduto> produtosASeremListados = new List<CSProdutos.CSProduto>();

                    prodFiltrados = CSProdutos.BuscaProdutos(COD_GRUPO_COMERCIALIZACAO, COD_GRUPO, COD_FAMILIA_PRODUTO, CSPDVs.Current.COD_CATEGORIA, CSPDVs.Current.COD_PDV, CSEmpresa.Current.IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE, CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA, !ExibirDescricaoProduto, CSPDVs.Current.COD_DENVER, !IsBroker() && !IsBunge()).Cast<CSProdutos.CSProduto>().ToList();

                    foreach (CSProdutos.CSProduto produtoAtual in prodFiltrados)
                    {
                        if (ValidarDadosBunge(produtoAtual))
                            produtosASeremListados.Add(produtoAtual);
                    }

                    if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current == null)
                    {
                        //Remove os produtos que já foram adicionados no pedido
                        produtosASeremListados = RemoveProdutosJaAdicionadosAoPedido(produtosASeremListados);
                    }
                    else
                    {
                        //Remove os produtos que já foram adicionados na indenização
                        produtosASeremListados = RemoveProdutosJaAdicionadosAIndenizacao(produtosASeremListados);
                    }

                    ExibeCheckProdutosExclusivos(CSProdutos.Existe_Produto_Categoria_Exclusiva);

                    // Preenche os produtos de acordo com os filtros selecionados            
                    var prodsFiltrados = produtosASeremListados.Where(p => (p.IND_PROD_ESPECIFICO_CATEGORIA && CSProdutos.Existe_Produto_Categoria_Exclusiva)
                                                                || !chkProdutosExclusivos.Checked).ToList();


                    if (IsBroker() ||
                        IsBunge())
                    {
                        CurrentActivity.RunOnUiThread(() =>
                        {
                            tvHeaderValor.Visibility = ViewStates.Gone;
                        });
                    }

                    produtosCarregadosBunge = prodsFiltrados;

                    m_ExecutandoMostraProdutos = false;
                    //m_IndClicouFiltroEspCat = false;
                }
                catch (Exception)
                {
                    //CSGlobal.GravarLog("Produto-MostraProdutosThread", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                }
            }

            private static bool ValidarDadosBunge(CSProdutos.CSProduto produto)
            {
                bool retorno;

                try
                {
                    CSPoliticaBunge pricingBunge = new CSPoliticaBunge(produto.COD_PRODUTO, CSEmpresa.Current.COD_NOTEBOOK1);
                    retorno = pricingBunge.ValidacaoPrecoProduto();
                }
                catch (Exception)
                {
                    //CSGlobal.GravarLog("Produto-ValidarDadosBungeThread", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);

                    retorno = false;
                }

                return retorno;
            }

            private bool IsBroker()
            {
                return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
            }

            private bool IsBunge()
            {
                return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 3;
            }
        }

        private void MostraProdutos(int COD_GRUPO_COMERCIALIZACAO, int COD_GRUPO, int COD_FAMILIA_PRODUTO, int COD_PRODUTO)
        {
            try
            {
                this.RunOnUiThread(() =>
                {
                    if (!m_executarMontaProdutos)
                        return;

                    m_ExecutandoMostraProdutos = true;

                    // Limpa o list dos produtos
                    lvwProdutos.Adapter = null;

                    if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current == null)
                    {
                        //Ordena os pedidos ja realizados
                        CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Sort(!ExibirDescricaoProduto);
                    }
                    //m_NumeroProdutosPedido = ListView.Adapter.Count;

                    var prodFiltrados = CSProdutos.BuscaProdutos(COD_GRUPO_COMERCIALIZACAO, COD_GRUPO, COD_FAMILIA_PRODUTO, CSPDVs.Current.COD_CATEGORIA, CSPDVs.Current.COD_PDV, CSEmpresa.Current.IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE, CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA, !ExibirDescricaoProduto, CSPDVs.Current.COD_DENVER, !IsBroker() && !IsBunge()).Cast<CSProdutos.CSProduto>().ToList();

                    if (CSEmpresa.ColunaExiste("EMPRESA", "IND_MOSTRAR_PRODUTO_BLOQUEADO"))
                    {
                        if (CSEmpresa.Current.IND_MOSTRAR_PRODUTO_BLOQUEADO == "N")
                            prodFiltrados = prodFiltrados.Where(p => p.IND_PRODUTO_BLOQUEADO == false).ToList();
                    }

                    if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current == null)
                    {
                        //Remove os produtos que já foram adicionados no pedido
                        RemoveProdutosJaAdicionadosAoPedido(prodFiltrados);
                    }
                    else
                    {
                        //Remove os produtos que já foram adicionados na indenização
                        prodFiltrados = RemoveProdutosJaAdicionadosAIndenizacao(prodFiltrados);
                    }

                    //if (COD_PRODUTO == 0)
                    //{
                    //    if (!m_IndClicouFiltroEspCat)
                    //        //Configura chkProdutosExclusivos se existe algum produto que é exclusivo da categoria ou nao
                    //        chkProdutosExclusivos.Checked = CSProdutos.Existe_Produto_Categoria_Exclusiva;
                    //    else
                    //    {
                    //        if (!CSProdutos.Existe_Produto_Categoria_Exclusiva)
                    //            chkProdutosExclusivos.Checked = false;
                    //    }
                    //}

                    ExibeCheckProdutosExclusivos(CSProdutos.Existe_Produto_Categoria_Exclusiva);

                    string ordenacao = CSConfiguracao.GetConfig("ORDENACAO_PRODUTOS");

                    // Preenche os produtos de acordo com os filtros selecionados            
                    var prodsFiltrados = prodFiltrados.Where(p => ((p.IND_PROD_ESPECIFICO_CATEGORIA || p.IND_PROD_TOP_CATEGORIA) && CSProdutos.Existe_Produto_Categoria_Exclusiva)
                                                                || !chkProdutosExclusivos.Checked).ToList();
                    int i;

                    switch (ordenacao)
                    {
                        case "Apelido":
                            {
                                prodsFiltrados = prodsFiltrados.OrderBy(p => !p.IND_PROD_ESPECIFICO_CATEGORIA).ThenBy(pr => pr.DSC_APELIDO_PRODUTO).ToList();
                            }
                            break;
                        case "Nome":
                            {
                                prodsFiltrados = prodsFiltrados.OrderBy(p => !p.IND_PROD_ESPECIFICO_CATEGORIA).ThenBy(pr => pr.DSC_PRODUTO).ToList();
                            }
                            break;
                        case "Codigo":
                            {
                                prodsFiltrados = prodsFiltrados.Where(prd => int.TryParse(prd.DESCRICAO_APELIDO_PRODUTO, out i)).OrderBy(p => !p.IND_PROD_ESPECIFICO_CATEGORIA).ThenBy(pr => int.Parse(pr.DESCRICAO_APELIDO_PRODUTO)).ToList();
                                prodsFiltrados.AddRange(prodsFiltrados.Cast<CSProdutos.CSProduto>().Where(p => !int.TryParse(p.DESCRICAO_APELIDO_PRODUTO, out i)).OrderBy(p => !p.IND_PROD_ESPECIFICO_CATEGORIA).ThenBy(b => b.DESCRICAO_APELIDO_PRODUTO).ToList());
                            }
                            break;
                        default:
                            break;
                    }

                    //ListViewAdapterProdutos.dicPrecoProdutos = new Dictionary<int, decimal>();
                    //ListViewAdapterProdutos.dicProdTextViews = new Dictionary<int, TextView>();

                    if (IsBroker() ||
                        IsBunge())
                    {
                        tvHeaderValor.Visibility = ViewStates.Gone;
                    }

                    //if (chkExibirImagens.Checked == false ||
                    //    !CSEmpregados.Current.IND_PERMITIR_ROTINA_IMAGEM)
                    //{
                    lvwProdutos.Adapter = ProdutosListViewBaseAdapterProp;
                    ProdutosListViewBaseAdapterProp.UpdateAdapter(prodsFiltrados);
                    //ListAdapter = new ProdutosListViewAdapter(this, Resource.Layout.produtos_row_descricao, prodsFiltrados, chkProdutosExclusivos.Checked);
                    //}
                    //else
                    //{
                    //ListAdapter = ProdutosListViewBaseAdapterProp;
                    //ProdutosListViewBaseAdapterProp.UpdateAdapter(prodsFiltrados);
                    //ListAdapter = new ProdutosListViewAdapter(this, Resource.Layout.produtos_row_imagem, prodsFiltrados, chkProdutosExclusivos.Checked);
                    //}

                    ProdutosAdapter = prodsFiltrados;
                    //if (ThreadBuscaPrecos != null)
                    //    if (ThreadBuscaPrecos.GetStatus().Name() == AsyncTask.Status.Running.Name())
                    //    {
                    //        ThreadBuscaPrecos.Cancel(true);
                    //    }

                    //ThreadBuscaPrecos = new BuscandoPrecoProduto();
                    //ThreadBuscaPrecos.Execute(prodsFiltrados.ToArray());

                    m_ExecutandoMostraProdutos = false;
                    //m_IndClicouFiltroEspCat = false;
                });
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-MostraProdutos", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        public static List<CSProdutos.CSProduto> RemoveProdutosJaAdicionadosAoPedido(List<CSProdutos.CSProduto> prodFiltrados)
        {
            try
            {
                IEnumerable<int> codigoDosProdutosJaAdicionados = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Items.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.STATE != ObjectState.DELETADO).Select(p => p.PRODUTO.COD_PRODUTO);
                prodFiltrados = prodFiltrados.Where(p => !codigoDosProdutosJaAdicionados.Contains(p.COD_PRODUTO)).ToList();
                return prodFiltrados;
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-RemoveProdutosJaAdicionadosAoPedido", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                return null;
            }
        }

        public static List<CSProdutos.CSProduto> RemoveProdutosJaAdicionadosAIndenizacao(List<CSProdutos.CSProduto> prodFiltrados)
        {
            try
            {
                IEnumerable<int> codigoDosProdutosJaAdicionados = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Items.Cast<CSItemsIndenizacao.CSItemIndenizacao>().Where(p => p.STATE != ObjectState.DELETADO).Select(p => p.PRODUTO.COD_PRODUTO);
                prodFiltrados = prodFiltrados.Where(p => !codigoDosProdutosJaAdicionados.Contains(p.COD_PRODUTO)).ToList();
                return prodFiltrados;
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-RemoveProdutosJaAdicionadosAIndenizacao", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                return null;
            }
        }

        //static ListActivity CurrentActivity;
        //private BuscandoPrecoProduto ThreadBuscaPrecos;
        //private static bool EnableThreadBuscaPrecosOnScroll = true;
        //private CSProdutos.CSProduto[] ProdutosVisiveisNaLista;
        //private static ProgressDialog pd;
        //#region IOnScrollListener Members

        //public void OnScroll(AbsListView view, int firstVisibleItem, int visibleItemCount, int totalItemCount)
        //{
        //    if (!IsBroker())
        //        return;
        //    try
        //    {
        //        if ((ProdutosListViewAdapter)ListAdapter == null)
        //            return;

        //        ProdutosVisiveisNaLista = ((ProdutosListViewAdapter)ListAdapter).produtos.Skip(firstVisibleItem).Take(visibleItemCount).ToArray();

        //        if (EnableThreadBuscaPrecosOnScroll)
        //        {
        //            //ThreadBuscaPrecos = new BuscandoPrecoProduto();
        //            //ThreadBuscaPrecos.Execute(ProdutosVisiveisNaLista);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(this, ex);
        //    }
        //}

        //public void OnScrollStateChanged(AbsListView view, ScrollState scrollState)
        //{
        //    if (!IsBroker())
        //        return;
        //    try
        //    {
        //        switch (scrollState)
        //        {
        //            case ScrollState.Fling:
        //                Produtos.EnableThreadBuscaPrecosOnScroll = false;
        //                //ThreadBuscaPrecos.Cancel(true);
        //                break;
        //            case ScrollState.Idle:
        //                Produtos.EnableThreadBuscaPrecosOnScroll = true;
        //                //ThreadBuscaPrecos.Cancel(true);
        //                //ThreadBuscaPrecos = new BuscandoPrecoProduto();
        //                //ThreadBuscaPrecos.Execute(ProdutosVisiveisNaLista);
        //                break;
        //            case ScrollState.TouchScroll:
        //                Produtos.EnableThreadBuscaPrecosOnScroll = false;
        //                //ThreadBuscaPrecos.Cancel(true);
        //                break;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(this, ex);
        //    }
        //}

        //#endregion

        //private class BuscandoPrecoProduto : AsyncTask<CSProdutos.CSProduto, object, decimal>
        //{
        //    private CSProdutos.CSProduto prod;
        //    protected override decimal RunInBackground(params CSProdutos.CSProduto[] @params)
        //    {
        //        try
        //        {
        //            CurrentActivity.RunOnUiThread(() =>
        //                    {
        //                        Produtos.pd = new ProgressDialog(CurrentActivity);
        //                        Produtos.pd.SetProgressStyle(ProgressDialogStyle.Horizontal);
        //                        Produtos.pd.SetTitle("Buscando Preços...");
        //                        Produtos.pd.SetMessage("Realizando a busca de preços...");
        //                        Produtos.pd.SetCancelable(true);
        //                        Produtos.pd.Max = @params.Count();
        //                        Produtos.pd.Show();
        //                    });
        //            for (int i = 0; i < @params.Count(); i++)
        //            {
        //                prod = @params[i];
        //                decimal valor = CalculaPrecoDoProdutoBroker();
        //                Produtos.CurrentActivity.RunOnUiThread(() =>
        //                    {
        //                        Produtos.pd.IncrementProgressBy(1);
        //                        string valorTextView = "erro";
        //                        if (valor != -1)
        //                        {
        //                            valorTextView = valor.ToString(CSGlobal.DecimalStringFormat);
        //                        }
        //                        if (Produtos.EnableThreadBuscaPrecosOnScroll)
        //                        {
        //                            TextView tv = ((ProdutosListViewAdapter)Produtos.CurrentActivity.ListAdapter).GetTextViewValor(prod.COD_PRODUTO);
        //                            if (tv != null)
        //                            {
        //                                tv.Text = valorTextView;
        //                            }
        //                        }
        //                        else
        //                        {

        //                        }
        //                        if (ProdutosListViewAdapter.dicPrecoProdutos.ContainsKey(prod.COD_PRODUTO))
        //                        {
        //                            ProdutosListViewAdapter.dicPrecoProdutos[prod.COD_PRODUTO] = valor;
        //                        }
        //                        else
        //                        {
        //                            ProdutosListViewAdapter.dicPrecoProdutos.Add(prod.COD_PRODUTO, valor);
        //                        }
        //                    }
        //                );
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show(CurrentActivity, ex);
        //        }

        //        return 0;
        //    }

        //    protected override void OnPostExecute(decimal result)
        //    {
        //        try
        //        {
        //            if (Produtos.pd != null)
        //            {
        //                Produtos.pd.Dismiss();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show(CurrentActivity, ex);
        //        }
        //    }

        //    protected override void OnCancelled()
        //    {
        //        if (Produtos.pd != null)
        //        {
        //            Produtos.pd.Dismiss();
        //        }
        //    }

        //    private decimal CalculaPrecoDoProdutoBroker()
        //    {
        //        if (ProdutosListViewAdapter.dicPrecoProdutos.ContainsKey(prod.COD_PRODUTO))
        //        {
        //            return ProdutosListViewAdapter.dicPrecoProdutos[prod.COD_PRODUTO];
        //        }
        //        try
        //        {
        //            var valorBroker = CSPDVs.Current.POLITICA_BROKER.CalculaPreco(prod.COD_PRODUTO, prod.GRUPO_COMERCIALIZACAO.COD_SETOR_BROKER, 1, 0, 0, prod.QTD_UNIDADE_EMBALAGEM);
        //            decimal precoBroker = valorBroker[valorBroker.Length - 1].VALOR;
        //            return precoBroker;
        //        }
        //        catch (Exception)
        //        {
        //            //MessageBox.Show(this, ex);
        //            return -1;
        //        }
        //    }

        //}

        public override void Finish()
        {
            try
            {
                Fechar();
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-Finish", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        /// <summary>
        /// Fecha a tela de pedidos
        /// </summary>
        private void Fechar()
        {
            try
            {
                if (IsDirty)
                {
                    SetResult(Result.Ok);
                }
                else
                {
                    SetResult(Result.Canceled);
                }
                base.Finish();
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-Fechar", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void SelecionarProduto(int qtdVendaRapida)
        {
            try
            {
                CSProdutos.Current = (CSProdutos.CSProduto)lvwProdutos.Adapter.GetItem(SelectedItemIndex);

                lvwProdutosIndenizacao_ItemActivate();
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-SelecionarProduto", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void SelecionarProduto(CSProdutos.CSProduto current)
        {
            try
            {
                CSProdutos.Current = current;

                lvwProdutosIndenizacao_ItemActivate();
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-SelecionarProduto", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void lvwProdutosIndenizacao_ItemActivate()
        {
            try
            {
                CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current = new CSItemsIndenizacao.CSItemIndenizacao();

                if (CSProdutos.Current.PRECOS_PRODUTO == null || CSProdutos.Current.PRECOS_PRODUTO.Count == 0)
                {

                    if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 1 /*&& CSGlobal.CalculaPrecoNestle*/)
                        MessageBox.AlertErro(this, "Preço do produto não cadastrado.\r\nNão é possivel realizar esta venda.");
                    else
                        MessageBox.AlertErro(this, "Cliente ou Produto com informações incompletas no cadastro Nestlê!\nNão é possivel realizar esta venda.");

                    return;
                }

                if (CSProdutos.Current.PCT_TAXA_MAX_INDENIZACAO == 0)
                {
                    MessageBox.AlertErro(this, "Taxa de indenização máxima deve ser maior que 0. Não é possível realizar a indenização.");
                    ItemClicado = false;
                    return;
                }

                // [ Seta produto do item ]
                CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.PRODUTO = CSProdutos.Current;

                bool novoItem = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.STATE == ObjectState.NOVO;

                // Mostra a tela de pedido
                Intent i = new Intent();
                i.SetClass(this, typeof(ProdutoIndenizacao));
                //i.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                this.StartActivityForResult(i, frmProdutoIndenizao);
                //StartActivityForResult(new Intent(this, new ProdutoIndenizacao().Class), frmProdutoIndenizao);
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-LvwProdutosIndenizacao", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }
        private void SaveScrollPosition()
        {
            try
            {
                currentPostion = lvwProdutos.FirstVisiblePosition;
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-SaveScrollPosition", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void SetScrollPosition()
        {
            try
            {
                lvwProdutos.SetSelection(currentPostion);
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("Produto-SetScrollPosition", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private class ProdutosListViewBaseAdapter : BaseAdapter
        {
            private Context context;
            private List<CSProdutos.CSProduto> Produtos = null;

            public ProdutosListViewBaseAdapter(Context context)
            {
                this.context = context;
            }

            public void UpdateAdapter(List<CSProdutos.CSProduto> produtos)
            {
                this.Produtos = produtos;
                NotifyDataSetChanged();
            }

            public override int Count
            {
                get { return this.Produtos == null ? 0 : this.Produtos.Count; }
            }

            public override Java.Lang.Object GetItem(int position)
            {
                return this.Produtos[position];
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                TextView tvCodigo;
                TextView tvVendaMes;
                TextView tvVendaUltimaVisita;
                TextView tvEstoque;
                TextView tvUnidadeMedida;
                TextView tvGrupoComercializacao;
                TextView tvValorUnitario;
                ImageView imgProdEspecifico;
                TextView tvDescProduto;
                CSProdutos.CSProduto ProdutoAtual = this.Produtos[position];

                if (convertView == null)
                {
                    convertView = LayoutInflater.From(context)
                      .Inflate(Resource.Layout.produtos_indenizacao_row_descricao, parent, false);

                    tvCodigo = convertView.FindViewById<TextView>(Resource.Id.tvCodigo);
                    tvVendaMes = convertView.FindViewById<TextView>(Resource.Id.tvVendaMes);
                    tvVendaUltimaVisita = convertView.FindViewById<TextView>(Resource.Id.tvVendaUltimaVisita);
                    tvEstoque = convertView.FindViewById<TextView>(Resource.Id.tvEstoque);
                    tvUnidadeMedida = convertView.FindViewById<TextView>(Resource.Id.tvUnidadeMedida);
                    tvGrupoComercializacao = convertView.FindViewById<TextView>(Resource.Id.tvGrupoComercializacao);
                    tvValorUnitario = convertView.FindViewById<TextView>(Resource.Id.tvValorUnitario);
                    imgProdEspecifico = convertView.FindViewById<ImageView>(Resource.Id.imgProdEspecifico);
                    tvDescProduto = convertView.FindViewById<TextView>(Resource.Id.tvDescProduto);

                    convertView.SetTag(tvCodigo.Id, tvCodigo);
                    convertView.SetTag(tvVendaMes.Id, tvVendaMes);
                    convertView.SetTag(tvVendaUltimaVisita.Id, tvVendaUltimaVisita);
                    convertView.SetTag(tvEstoque.Id, tvEstoque);
                    convertView.SetTag(tvUnidadeMedida.Id, tvUnidadeMedida);
                    convertView.SetTag(tvGrupoComercializacao.Id, tvGrupoComercializacao);
                    convertView.SetTag(tvValorUnitario.Id, tvValorUnitario);
                    convertView.SetTag(imgProdEspecifico.Id, imgProdEspecifico);
                    convertView.SetTag(tvDescProduto.Id, tvDescProduto);
                }
                else
                {
                    tvCodigo = (TextView)convertView.GetTag(Resource.Id.tvCodigo);
                    tvVendaMes = (TextView)convertView.GetTag(Resource.Id.tvVendaMes);
                    tvVendaUltimaVisita = (TextView)convertView.GetTag(Resource.Id.tvVendaUltimaVisita);
                    tvEstoque = (TextView)convertView.GetTag(Resource.Id.tvEstoque);
                    tvUnidadeMedida = (TextView)convertView.GetTag(Resource.Id.tvUnidadeMedida);
                    tvGrupoComercializacao = (TextView)convertView.GetTag(Resource.Id.tvGrupoComercializacao);
                    tvValorUnitario = (TextView)convertView.GetTag(Resource.Id.tvValorUnitario);
                    imgProdEspecifico = (ImageView)convertView.GetTag(Resource.Id.imgProdEspecifico);
                    tvDescProduto = (TextView)convertView.GetTag(Resource.Id.tvDescProduto);
                }

                if (ExibirDescricaoProduto)
                    tvDescProduto.Text = ProdutoAtual.DSC_PRODUTO.Trim();
                else
                    tvDescProduto.Text = ProdutoAtual.DSC_APELIDO_PRODUTO.Trim();

                if (IsBroker() ||
                    IsBunge())
                    tvValorUnitario.Visibility = ViewStates.Gone;
                else
                {
                    tvValorUnitario.Visibility = ViewStates.Visible;
                    tvValorUnitario.Text = ProdutoAtual.PRECOS_PRODUTO_TABELA_PRECO_PADRAO != -1 ? ProdutoAtual.PRECOS_PRODUTO_TABELA_PRECO_PADRAO.ToString(CSGlobal.DecimalStringFormat) : "-";

                    if (ProdutoAtual.PRECOS_PRODUTO_TABELA_PRECO_PADRAO == 0)
                        tvValorUnitario.Text = CalculaPrecoDoProduto(ProdutoAtual).ToString(CSGlobal.DecimalStringFormat);
                }

                tvCodigo.Text = ProdutoAtual.DESCRICAO_APELIDO_PRODUTO;
                tvVendaMes.Text = ProdutoAtual.IND_VENDA_MES ? "S" : "";
                tvVendaUltimaVisita.Text = ProdutoAtual.IND_VENDA_ULTIMA_VISITA ? "S" : "";
                tvEstoque.Text = ProdutoAtual.QTD_ESTOQUE > 0 ? "S" : "N";
                tvUnidadeMedida.Text = ProdutoAtual.DSC_UNIDADE_MEDIDA;
                tvGrupoComercializacao.Text = ProdutoAtual.GRUPO_COMERCIALIZACAO.DES_GRUPO_COMERCIALIZACAO;

                if (ProdutoAtual.IND_PROD_TOP_CATEGORIA)
                    imgProdEspecifico.SetImageResource(Resource.Drawable.circulo_verde_top);
                else if (ProdutoAtual.IND_PROD_ESPECIFICO_CATEGORIA)
                    imgProdEspecifico.SetImageResource(Resource.Drawable.circulo_azul);
                else
                    imgProdEspecifico.SetImageResource(Resource.Drawable.circulo_cinza);

                return convertView;
            }

            private bool IsBroker()
            {
                try
                {
                    return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
                }
                catch (Exception)
                {
                    //CSGlobal.GravarLog("Produto-IsBroker", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                    return false;
                }
            }

            private bool IsBunge()
            {
                try
                {
                    return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 3;
                }
                catch (Exception)
                {
                    //CSGlobal.GravarLog("Produto-IsBroker", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                    return false;
                }
            }

            private decimal CalculaPrecoDoProduto(CSProdutos.CSProduto produtoParaCalcular)
            {
                CSProdutos.CSProduto prod = produtoParaCalcular;

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
        }

        private class ProdutosListViewAdapter : ArrayAdapter<CSProdutos.CSProduto>
        {
            ListActivity lAct;
            public IList<CSProdutos.CSProduto> produtos;
            public static Dictionary<int, decimal> dicPrecoProdutos;
            int resourceId;
            //bool produtosExclusivos;

            public ProdutosListViewAdapter(ListActivity c, int textViewResourceId, IList<CSProdutos.CSProduto> objects, bool chkProdutosExclusivos)
                : base(c, textViewResourceId, objects)
            {
                lAct = c;
                produtos = objects;
                resourceId = textViewResourceId;
                if (dicPrecoProdutos == null)
                {
                    dicPrecoProdutos = new Dictionary<int, decimal>();
                }
            }

            public List<CSProdutos.CSProduto> FilterData(string search)
            {
                try
                {
                    var produtosFiltrados = new List<CSProdutos.CSProduto>();
                    var iniciarPesquisaEm = new List<CSProdutos.CSProduto>(produtos);
                    var searches = search.Split(' ');
                    for (int i = 0; i < searches.Count(); i++)
                    {
                        if (i > 0)
                            iniciarPesquisaEm = new List<CSProdutos.CSProduto>(produtosFiltrados);

                        if (string.IsNullOrEmpty(searches[i].Trim()))
                            break;

                        search = searches[i].ToUpper();

                        //Se a pesquisa for por código do produto
                        int codProduto;
                        if (int.TryParse(search, out codProduto))
                        {
                            produtosFiltrados.AddRange(iniciarPesquisaEm.Where(p => p.DESCRICAO_APELIDO_PRODUTO.Contains(codProduto.ToString())));
                        }

                        //Se a pesquisa for por descrição do produto
                        produtosFiltrados.AddRange(iniciarPesquisaEm.Where(p => p.DSC_PRODUTO.ToUpper().Contains(search)));

                        //Se a pesquisa for por apelido do produto
                        produtosFiltrados.AddRange(iniciarPesquisaEm.Where(p => p.DSC_APELIDO_PRODUTO.ToUpper().Contains(search)));

                    }
                    return produtosFiltrados.Distinct().ToList();
                }
                catch (Exception)
                {
                    //CSGlobal.GravarLog("Produto-FilterDada", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                    return null;
                }
            }

            private bool IsBroker()
            {
                return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
            }

            private bool IsBunge()
            {
                return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 3;
            }

            public override long GetItemId(int position)
            {
                return produtos[position].COD_PRODUTO;
            }

            public TextView GetTextViewValor(int codProduto)
            {
                return lAct.ListView.FindViewById<TextView>(codProduto);
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                try
                {
                    CSProdutos.CSProduto prod = produtos[position];

                    LayoutInflater layout = (LayoutInflater)lAct.GetSystemService(Context.LayoutInflaterService);

                    if (convertView == null)
                        convertView = layout.Inflate(resourceId, null);

                    if (prod != null)
                    {
                        TextView tvCodigo = convertView.FindViewById<TextView>(Resource.Id.tvCodigo);
                        TextView tvVendaMes = convertView.FindViewById<TextView>(Resource.Id.tvVendaMes);
                        TextView tvVendaUltimaVisita = convertView.FindViewById<TextView>(Resource.Id.tvVendaUltimaVisita);
                        TextView tvEstoque = convertView.FindViewById<TextView>(Resource.Id.tvEstoque);
                        TextView tvUnidadeMedida = convertView.FindViewById<TextView>(Resource.Id.tvUnidadeMedida);
                        TextView tvGrupoComercializacao = convertView.FindViewById<TextView>(Resource.Id.tvGrupoComercializacao);
                        TextView tvValorUnitario = convertView.FindViewById<TextView>(Resource.Id.tvValorUnitario);
                        ImageView imgProdEspecifico = convertView.FindViewById<ImageView>(Resource.Id.imgProdEspecifico);
                        TextView tvDescProduto = convertView.FindViewById<TextView>(Resource.Id.tvDescProduto);

                        if (tvDescProduto != null)
                        {
                            if (ExibirDescricaoProduto)
                                tvDescProduto.Text = prod.DSC_PRODUTO.Trim();
                            else
                                tvDescProduto.Text = prod.DSC_APELIDO_PRODUTO.Trim();
                        }

                        if (IsBroker())
                        {
                            tvValorUnitario.Visibility = ViewStates.Gone;
                            tvValorUnitario.Text = "...";

                            if (dicPrecoProdutos.ContainsKey(prod.COD_PRODUTO))
                            {
                                var valor = dicPrecoProdutos[prod.COD_PRODUTO];
                                if (valor != -1)
                                {
                                    tvValorUnitario.Text = dicPrecoProdutos[prod.COD_PRODUTO].ToString(CSGlobal.DecimalStringFormat);
                                }
                                else
                                {
                                    tvValorUnitario.Text = "erro";
                                }
                            }
                        }
                        else if (IsBunge())
                        {
                            tvValorUnitario.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            // [ Busca tabela de preço padrão do pdv ]
                            prod.PRECOS_PRODUTO.Current = prod.PRECOS_PRODUTO.Cast<CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto>()
                                .Where(p => p.COD_TABELA_PRECO == CSPDVs.Current.COD_TABPRECO_PADRAO).FirstOrDefault();

                            // Seta a primeira tabela de preço como default para mostrar o preço final
                            if (prod.PRECOS_PRODUTO.Current == null)
                            {
                                if (prod.PRECOS_PRODUTO.Items.Count > 0)
                                {
                                    prod.PRECOS_PRODUTO.Current = prod.PRECOS_PRODUTO.Items[0];
                                    tvValorUnitario.Text = prod.PRECOS_PRODUTO.Current.VLR_PRODUTO.ToString(CSGlobal.DecimalStringFormat);
                                }
                                else
                                {
                                    tvValorUnitario.Text = "0,00";
                                }
                            }
                            else
                            {
                                tvValorUnitario.Text = prod.PRECOS_PRODUTO.Current.VLR_PRODUTO.ToString(CSGlobal.DecimalStringFormat);
                            }
                        }


                        tvCodigo.Text = prod.DESCRICAO_APELIDO_PRODUTO;
                        tvVendaMes.Text = prod.IND_VENDA_MES ? "S" : "";
                        tvVendaUltimaVisita.Text = prod.IND_VENDA_ULTIMA_VISITA ? "S" : "";
                        tvEstoque.Text = prod.QTD_ESTOQUE > 0 ? "S" : "N";
                        tvUnidadeMedida.Text = prod.DSC_UNIDADE_MEDIDA;
                        tvGrupoComercializacao.Text = prod.GRUPO_COMERCIALIZACAO.DES_GRUPO_COMERCIALIZACAO;

                        if (prod.IND_PROD_TOP_CATEGORIA)
                            imgProdEspecifico.SetImageResource(Resource.Drawable.circulo_verde_top);
                        else if (prod.IND_PROD_ESPECIFICO_CATEGORIA)
                            imgProdEspecifico.SetImageResource(Resource.Drawable.circulo_azul);
                        else
                            imgProdEspecifico.SetImageResource(Resource.Drawable.circulo_cinza);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Alert(lAct, ex.Message);
                    //CSGlobal.GravarLog("Produto-GetViewProdutosListView", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                }
                return convertView;
            }
        }
    }
}
