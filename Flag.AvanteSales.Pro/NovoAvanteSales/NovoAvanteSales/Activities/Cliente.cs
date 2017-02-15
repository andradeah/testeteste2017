using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Android.Content.PM;
using AvanteSales.Pro.Fragments;
using AvanteSales.Pro.Dialogs;
using Android.Views.InputMethods;
using AvanteSales.Pro.Controles;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "Cliente", ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AvanteSalesTheme", WindowSoftInputMode = SoftInput.AdjustNothing)]
    public class Cliente : AppCompatActivity
    {
        static AppCompatActivity CurrentActivity;
        public bool? MotivoInformado;
        Android.Support.V7.Widget.Toolbar tbToolbar;
        //Chronometer cronometro;
        TextView lblPasso;
        FrameLayout frmLayout;
        TextView lblCodPdv;
        TextView lblNomePdv;
        Android.Support.V4.App.FragmentTransaction ft;
        Button btnPassoAnterior;
        Button btnProximoPasso;
        public int PassoAtual;
        TextView lblValorPedido;
        TextView lblSkus;
        DrawerLayout drawerLayout;
        public static CSMonitoramentosVendedoresRotas.CSMonitoramentoVendedorRota monitoramento;
        static string localizacaoFlexxGpsInicial = "";
        static string localizacaoFlexxGpsFinal = "";
        private CSGruposComercializacao.CSGrupoComercializacao m_LinhaSelecionada;
        private int m_GrupoSelecionado;
        private int m_FamiliaSelecionada;
        public const int DialogProduto = 1;
        private int IDListaProduto;
        public bool MenuClicado;
        ImageView imgLogoLinha;
        ListView left_drawer;
        Android.Support.V7.App.ActionBarDrawerToggle DrawerToggle;
        static bool m_ListaPedidosAberto;
        public bool EdicaoProduto;
        public bool RotinaProdutosIndicados;
        private bool m_ValidarPoliticaPreco;
        private bool m_MotivoNaoCompraProdutoIndicado;
        public bool ValidarPoliticaPreco
        {
            get
            {
                return m_ValidarPoliticaPreco;
            }
            set
            {
                m_ValidarPoliticaPreco = value;
            }
        }
        public bool ListaPedidosAberto
        {
            get
            {
                return m_ListaPedidosAberto;
            }
            set
            {
                m_ListaPedidosAberto = value;
            }
        }
        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            DrawerToggle.SyncState();
        }

        public CSGruposComercializacao.CSGrupoComercializacao LinhaSelecionada
        {
            get
            {
                return m_LinhaSelecionada;
            }
            set
            {
                m_LinhaSelecionada = value;

                if (m_LinhaSelecionada != null &&
                    m_LinhaSelecionada.COD_SETOR_BROKER.Contains("BRL1"))
                {
                    imgLogoLinha.Visibility = ViewStates.Visible;
                    imgLogoLinha.SetImageResource(Resource.Drawable.logo_starb);
                }
                else
                {
                    imgLogoLinha.Visibility = ViewStates.Gone;
                }
            }
        }

        internal void AbrirDigitacaoCombo(int QTD_MAX, int QTD_VENDIDA, int PRODUTO)
        {
            DialogFragmentDigitacaoCombo digitacaoCombo = new DialogFragmentDigitacaoCombo();

            Bundle bundle = new Bundle();
            bundle.PutInt("QTD_MAX", QTD_MAX);
            bundle.PutInt("QTD_VENDIDA", QTD_VENDIDA);
            bundle.PutInt("PRODUTO", PRODUTO);

            digitacaoCombo.Arguments = bundle;

            digitacaoCombo.Show(SupportFragmentManager, "DialogFragmentDigitacaoCombo");
        }

        public int GrupoSelecionado
        {
            get
            {
                return m_GrupoSelecionado;
            }
            set
            {
                m_GrupoSelecionado = value;
            }
        }

        public int FamiliaSelecionada
        {
            get
            {
                return m_FamiliaSelecionada;
            }
            set
            {
                m_FamiliaSelecionada = value;
            }
        }

        public bool MotivoNaoCompraProdutoIndicado
        {
            get
            {
                return m_MotivoNaoCompraProdutoIndicado;
            }
            set
            {
                m_MotivoNaoCompraProdutoIndicado = value;
            }
        }

        public void IniciarNovoPedido()
        {
            ValidarPoliticaPreco = true;
            ft = SupportFragmentManager.BeginTransaction();
            ft.Replace(frmLayout.Id, new ContatoCliente(), "ContatoCliente");
            ft.AddToBackStack("ContatoCliente");
            ft.Commit();

            AtualizarValorParcial();
        }

        internal void AbrirListaProdutos(int ultimoFragment, string txtDescontoIndenizacao, string txtAdf)
        {
            MenuClicado = true;
            ListaProdutosPedido listaProdutos = new ListaProdutosPedido();

            Bundle bundle = new Bundle();
            bundle.PutInt("ultimaActivity", ultimoFragment);
            bundle.PutString("txtDescontoIndenizacao", txtDescontoIndenizacao);
            bundle.PutString("txtAdf", txtAdf);
            listaProdutos.Arguments = bundle;

            ft = SupportFragmentManager.BeginTransaction();
            ft.Replace(frmLayout.Id, listaProdutos, "ListaProdutosPedido");
            ft.AddToBackStack("ListaProdutosPedido");
            ft.Commit();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ValidarPoliticaPreco = true;
            SetContentView(Resource.Layout.cliente);
            EdicaoProduto = false;
            FindViewsById();

            DrawerToggle = new MyActionBarDawerToggle(
           this,
           drawerLayout);

            //MenuLateral();

            Eventos();

            SetSupportActionBar(tbToolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            drawerLayout.AddDrawerListener(DrawerToggle);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            DrawerToggle.SyncState();

            Inicializacao();
        }

        private void Eventos()
        {
            btnPassoAnterior.Click += BtnPassoAnterior_Click;
            btnProximoPasso.Click += BtnProximoPasso_Click;
            left_drawer.ItemClick += Left_drawer_ItemClick;
        }

        private void Left_drawer_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            switch (e.Position)
            {
                case 0:
                    {
                        NavegarParaPasso(1);
                    }
                    break;
                case 1:
                    {
                        NavegarParaPasso(2);
                    }
                    break;
                case 2:
                    {
                        NavegarParaPasso(3);
                    }
                    break;
                case 3:
                    {
                        NavegarParaPasso(4);
                    }
                    break;
                case 4:
                    {
                        NavegarParaPasso(6);
                    }
                    break;
                case 5:
                    {
                        NavegarParaPasso(7);
                    }
                    break;
                case 6:
                    {
                        NavegarParaPasso(8);
                    }
                    break;
                case 7:
                    {
                        NavegarParaPasso(9);
                    }
                    break;
                case 8:
                    {
                        NavegarParaPasso(10);
                    }
                    break;
            }

            drawerLayout.CloseDrawers();
        }

        public void NavegarVendaProdutoIndicado()
        {
            RotinaProdutosIndicados = true;
            Produto Produto = new Produto();
            ft = SupportFragmentManager.BeginTransaction();
            ft.Replace(frmLayout.Id, Produto, "Produto");
            ft.AddToBackStack("Produto");
            ft.Commit();
            PassoAtual = 6;
            AlterarLabelPasso();
            TratamentosPasso(false);
        }

        private void BtnProximoPasso_Click(object sender, EventArgs e)
        {
            drawerLayout.CloseDrawers();

            if (MotivoNaoCompraProdutoIndicado)
            {
                SalvarMotivoNaoCompraProdutoIndicado();
            }
            else if (!ListaPedidosAberto)
            {
                var fragment = SupportFragmentManager.FindFragmentByTag("MotivoNaoPositivado");

                if (fragment == null)
                {
                    if (PassoAtual == 10)
                    {
                        Pedido.ValidacoesSalvarPedido();
                    }
                    else
                        ProximoPasso(false);
                }
                else
                {
                    if (MotivoInformado.HasValue)
                    {
                        SalvarMotivo();
                    }
                }
            }
            else
                MessageBox.ShowShortMessageCenter(this, "Não é possível navegar através da Lista de Pedidos.");
        }

        private void BtnPassoAnterior_Click(object sender, EventArgs e)
        {
            drawerLayout.CloseDrawers();

            OnBackPressed();
        }

        private void FindViewsById()
        {
            left_drawer = FindViewById<ListView>(Resource.Id.left_drawer);
            tbToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tbToolbar);
            //cronometro = FindViewById<Chronometer>(Resource.Id.cronometro);
            lblPasso = FindViewById<TextView>(Resource.Id.lblPasso);
            frmLayout = FindViewById<FrameLayout>(Resource.Id.frmLayout);
            lblCodPdv = FindViewById<TextView>(Resource.Id.lblCodPdv);
            lblNomePdv = FindViewById<TextView>(Resource.Id.lblNomePdv);
            btnPassoAnterior = FindViewById<Button>(Resource.Id.btnPassoAnterior);
            btnProximoPasso = FindViewById<Button>(Resource.Id.btnProximoPasso);
            lblValorPedido = FindViewById<TextView>(Resource.Id.lblValorPedido);
            lblSkus = FindViewById<TextView>(Resource.Id.lblSkus);
            imgLogoLinha = FindViewById<ImageView>(Resource.Id.imgLogoLinha);
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawerLayout);
        }

        internal void AbrirListaCombos()
        {
            MenuClicado = true;

            ListaCombo ListaCombo = new ListaCombo();
            ft = SupportFragmentManager.BeginTransaction();
            ft.Replace(frmLayout.Id, ListaCombo, "ListaCombo");
            ft.AddToBackStack("ListaCombo");
            ft.Commit();
        }

        public void AtualizarValorParcial()
        {
            if (CSPDVs.Current.PEDIDOS_PDV.Current != null)
            {
                lblValorPedido.Text = CSPDVs.Current.PEDIDOS_PDV.Current.VLR_TOTAL_PEDIDO.ToString("0.00");
                lblSkus.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS != null ? CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.STATE != ObjectState.DELETADO).ToList().Count.ToString() : "0";
            }
            else
            {
                lblValorPedido.Text = "0,00";
                lblSkus.Text = "0";
            }
        }

        private void Inicializacao()
        {
            PassoAtual = 1;
            CurrentActivity = this;

            lblPasso.Text = string.Format("{0}/10", "1");
            lblCodPdv.Text = CSPDVs.Current.COD_PDV.ToString();
            lblNomePdv.Text = CSPDVs.Current.DSC_RAZAO_SOCIAL;
            btnPassoAnterior.Visibility = ViewStates.Invisible;
            lblValorPedido.Text = "0,00";
            lblSkus.Text = "0";
            ListaPedidosAberto = false;

            UpdateData();

            var motivoNaoCompra = Intent.GetBooleanExtra("motivoNaoCompra", false);

            if (motivoNaoCompra)
                ValidarPoliticaPreco = false;

            ft = SupportFragmentManager.BeginTransaction();
            ft.Replace(frmLayout.Id, new ContatoCliente(), "ContatoCliente");
            ft.AddToBackStack("ContatoCliente");
            ft.Commit();

            if (motivoNaoCompra)
                OnBackPressed();
        }

        public void MenuLateral()
        {
            List<string> menus = new List<string>();

            if (IsBroker())
            {
                menus.Add("Passo 1: Linha");
                menus.Add("Passo 2: Grupo");
                menus.Add("Passo 3: Validade de produtos");
                menus.Add("Passo 4: Lista de produtos");
                menus.Add("Passo 6: Validades coletadas");
                menus.Add("Passo 7: Mensagem pedido");
                menus.Add("Passo 8: Últimos pedidos");
                menus.Add("Passo 9: Documentos a receber");
                menus.Add("Passo 10: Resumo pedido");
            }
            else
            {
                menus.Add("Passo 1: Linha");
                menus.Add("Passo 2: Produtos indicados");
                menus.Add("Passo 3: Grupo");
                menus.Add("Passo 4: Validade de produtos");
                menus.Add("Passo 5: Lista de produtos");
                menus.Add("Passo 7: Validades coletadas");
                menus.Add("Passo 8: Mensagem pedido");
                menus.Add("Passo 9: Produtos não vendidos");
                menus.Add("Passo 10: Resumo pedido");
            }

            left_drawer.Adapter = new DrawerMenuAdapter(this, menus, Resource.Layout.drawer_menu_row);
        }

        internal void UltimosPedidos()
        {
            ft = SupportFragmentManager.BeginTransaction();
            ft.Replace(frmLayout.Id, new ConsultaProdutoPedidos(), "ConsultaProdutoPedidos");
            ft.AddToBackStack("ConsultaProdutoPedidos");
            ft.Commit();
        }


        private class DrawerMenuAdapter : ArrayAdapter<string>
        {
            List<string> MenuItems;
            int LayoutResourceId;

            public DrawerMenuAdapter(Context context, List<string> menuItems, int layoutResourceId) : base(context, layoutResourceId, menuItems)
            {
                MenuItems = menuItems;
                LayoutResourceId = layoutResourceId;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                string menuItemAtual = MenuItems[position];

                if (convertView == null)
                    convertView = LayoutInflater.From(Context)
                      .Inflate(LayoutResourceId, parent, false);

                TextView lblMenu = convertView.FindViewById<TextView>(Resource.Id.lblMenu);
                lblMenu.Text = menuItemAtual;

                return convertView;
            }
        }

        private void UpdateData()
        {
            CSTiposDistribPolicitcaPrecos.Current = CSTiposDistribPolicitcaPrecos.GetTipoDistribPolicitcaPreco(CSTiposDistribPolicitcaPrecos.GetPoliticaPreco());

            // Entrada no PDV
            // Variavel para monitoramento de controle do vendedor
            monitoramento = new CSMonitoramentosVendedoresRotas.CSMonitoramentoVendedorRota();
            monitoramento.COD_PDV = CSPDVs.Current.COD_PDV;
            monitoramento.DAT_ENTRADA = DateTime.Now;
            monitoramento.IND_TIPO_ACESSO = "P";
            monitoramento.COD_EMPREGADO = CSEmpregados.Current.COD_EMPREGADO;

            CSPDVs.Current.MONITORAMENTOS.Current = monitoramento;

            if (CSEmpresa.Current.IND_UTILIZA_FLEXX_GPS == "S")
                localizacaoFlexxGpsInicial = CSGlobal.GetLocalizacaoFlexXGPS();
        }

        public void AbrirDialogProduto(bool ultimasVisitasHabilitada, bool vendaHabilitada, bool vendaIndicados, bool produtoVendido)
        {
            Android.Support.V4.App.DialogFragment df;
            if (!CSEmpregados.Current.IND_PERMITIR_ROTINA_IMAGEM)
                df = new DialogFragmentProdutoComum();
            else
                df = new DialogFragmentProdutoImagem();

            Bundle bundle = new Bundle();
            bundle.PutBoolean("ultimasVisitasHabilitada", ultimasVisitasHabilitada);
            bundle.PutBoolean("vendaHabilidada", vendaHabilitada);
            bundle.PutBoolean("vendaIndicados", vendaIndicados);
            bundle.PutBoolean("ProdutoVendido", produtoVendido);

            df.Arguments = bundle;
            df.Show(SupportFragmentManager, "DialogFragmentProdutoComum");
        }
        public void ProximoPasso(bool proximoPassoComVenda)
        {
            if (PassoAtual == PassoDigitacaoProduto())
            {
                ProdutoVenda.ValidacoesVenda(true, false);
                AtualizarValorParcial();
            }
            else
            {
                if (((IsBroker() && PassoAtual == 4) ||
                    (!IsBroker() && PassoAtual == 5)) &&
                    !proximoPassoComVenda)
                    PassoAtual = PassoAtual + 2;
                else
                    PassoAtual++;

                AlterarFragment(true,false);
            }
        }

        public void PassoAnterior()
        {
            PassoAtual--;

            AlterarFragment(false,false);
        }

        public void AlterarLabelPasso()
        {
            lblPasso.Text = string.Format("{0}/10", PassoAtual);

            if (PassoAtual > 1)
            {
                btnPassoAnterior.Visibility = ViewStates.Visible;
                btnProximoPasso.Visibility = ViewStates.Visible;
            }
            else
            {
                btnPassoAnterior.Visibility = ViewStates.Invisible;
            }
        }

        public void NavegarParaPasso(int passo, bool edicao)
        {
            FuncaoNavegacao(passo, edicao);
        }

        public void NavegarParaPasso(int passo)
        {
            if (!ListaPedidosAberto)
            {
                if (passo <= 6 && CSGlobal.PedidoComCombo)
                    MessageBox.ShowShortMessageCenter(this, "Não é possível navegar para o passo desejado com pedido combo.");
                else
                    FuncaoNavegacao(passo,false);
            }
            else
                MessageBox.ShowShortMessageCenter(this, "Não é possível navegar através da Lista de Pedidos.");
        }

        public void NavegarEdicaoProduto()
        {
            if (!ListaPedidosAberto)
            {
                if (CSGlobal.PedidoComCombo)
                    MessageBox.ShowShortMessageCenter(this, "Não é possível editar o produto desejado com pedido combo.");
                else
                {
                    EdicaoProduto = true;

                    ft = SupportFragmentManager.BeginTransaction();
                    ft.Replace(frmLayout.Id, new Produto(), "Produto");
                    ft.AddToBackStack("Produto");
                    ft.SetCustomAnimations(Resource.Animation.slide_right_to_left, Resource.Animation.slide_left_to_right);
                    ft.Commit();

                    PassoAtual = PassoDigitacaoProduto();
                    AlterarLabelPasso();
                    TratamentosPasso(true);
                }
            }
            else
                MessageBox.ShowShortMessageCenter(this, "Não é possível navegar através da Lista de Pedidos.");
        }

        private void FuncaoNavegacao(int passo, bool edicao)
        {
            if (PassoAtual < passo)
            {
                while (PassoAtual < passo)
                {
                    PassoAtual++;

                    if (PassoAtual != PassoDigitacaoProduto())
                        AlterarFragment(true, edicao);
                }
            }
            else if (PassoAtual > passo)
            {
                while (PassoAtual > passo)
                {
                    PassoAtual--;

                    if (PassoAtual != PassoDigitacaoProduto())
                        AlterarFragment(false,edicao);
                }
            }
        }

        private int PassoDigitacaoProduto()
        {
            if (IsBroker())
                return 5;
            else
                return 6;
        }

        public void TratamentosPasso(bool edicaoProduto)
        {
            if (PassoAtual == 1)
            {
                btnPassoAnterior.Visibility = ViewStates.Invisible;
            }
            else if (PassoAtual == 10)
            {
                btnProximoPasso.Text = "Finalizar pedido";
                btnPassoAnterior.Visibility = ViewStates.Visible;
            }
            else if (PassoAtual == PassoDigitacaoProduto())
            {
                if (!edicaoProduto)
                    btnPassoAnterior.Text = "Novo produto";
                else
                    btnPassoAnterior.Visibility = ViewStates.Invisible;

                btnProximoPasso.Text = "Finalizar produtos";
            }
            else
            {
                btnPassoAnterior.Text = "Passo anterior";
                btnProximoPasso.Text = "Próximo passo";
                btnPassoAnterior.Visibility = ViewStates.Visible;
            }
        }

        public void AlterarFragment(bool proximoPasso, bool edicao)
        {
            try
            {
                AlterarLabelPasso();

                ft = SupportFragmentManager.BeginTransaction();

                if (IsBroker())
                    NavegacaoBroker(proximoPasso);
                else
                    NavegacaoBungeEDistribuidor(proximoPasso, edicao);

                TratamentosPasso(false);
                ft.SetCustomAnimations(Resource.Animation.slide_right_to_left, Resource.Animation.slide_left_to_right);
                ft.Commit();
            }
            catch (Exception)
            {

            }
        }

        private void NavegacaoBungeEDistribuidor(bool proximoPasso, bool edicao)
        {
            switch (PassoAtual)
            {
                case 1:
                    {
                        if (proximoPasso)
                        {
                            ft.Replace(frmLayout.Id, new ContatoCliente(), "ContatoCliente");
                            ft.AddToBackStack("ContatoCliente");
                        }
                        else
                            SupportFragmentManager.PopBackStack("ContatoCliente", 0);
                    }
                    break;
                case 2:
                    {
                        if (proximoPasso)
                        {
                            ft.Replace(frmLayout.Id, new ListaProdutosIndicados(), "ListaProdutosIndicados");
                            ft.AddToBackStack("ListaProdutosIndicados");
                        }
                        else
                            SupportFragmentManager.PopBackStack("ListaProdutosIndicados", 0);
                    }
                    break;
                case 3:
                    {
                        if (proximoPasso)
                        {
                            ft.Replace(frmLayout.Id, new GrupoProduto(), "GrupoProduto");
                            ft.AddToBackStack("GrupoProduto");
                        }
                        else
                            SupportFragmentManager.PopBackStack("GrupoProduto", 0);
                    }
                    break;
                case 4:
                    {
                        if (proximoPasso)
                        {
                            var frag = new ProdutosVencer();
                            IDListaProduto = frag.Id;

                            ft.Replace(frmLayout.Id, frag, "ProdutosVencer");
                            ft.AddToBackStack("ProdutosVencer");
                        }
                        else
                            SupportFragmentManager.PopBackStack("ProdutosVencer", 0);
                    }
                    break;
                case 5:
                    {
                        if (proximoPasso)
                        {
                            ft.Replace(frmLayout.Id, new ListaProdutos(), "ListaProdutos");
                            ft.AddToBackStack("ListaProdutos");
                        }
                        else
                            SupportFragmentManager.PopBackStack("ListaProdutos", 0);
                    }
                    break;
                case 6:
                    {
                        if (proximoPasso)
                        {
                            ft.Replace(frmLayout.Id, new Produto(), "Produto");
                            ft.AddToBackStack("Produto");
                        }
                        else
                            SupportFragmentManager.PopBackStack("Produto", 0);
                    }
                    break;
                case 7:
                    {
                        if (proximoPasso)
                        {
                            ft.Replace(frmLayout.Id, new ListaProdutoVencimento(), "ListaProdutoVencimento");
                            ft.AddToBackStack("ListaProdutoVencimento");
                        }
                        else
                            SupportFragmentManager.PopBackStack("ListaProdutoVencimento", 0);
                    }
                    break;
                case 8:
                    {
                        if (proximoPasso)
                        {
                            ft.Replace(frmLayout.Id, new MensagemPedido(), "MensagemPedido");
                            ft.AddToBackStack("MensagemPedido");
                        }
                        else
                            SupportFragmentManager.PopBackStack("MensagemPedido", 0);
                    }
                    break;
                case 9:
                    {
                        if (proximoPasso)
                        {
                            ft.Replace(frmLayout.Id, new ListaProdutosRamoNaoVendidos(), "ListaProdutosRamoNaoVendidos");
                            ft.AddToBackStack("ListaProdutosRamoNaoVendidos");
                        }
                        else
                            SupportFragmentManager.PopBackStack("ListaProdutosRamoNaoVendidos", 0);
                    }
                    break;
                case 10:
                    {
                        if (proximoPasso)
                        {
                            Android.Support.V4.App.Fragment fragmentPedido = new Pedido();

                            Bundle arguments = new Bundle();
                            arguments.PutBoolean("edicaoPedido", edicao);

                            fragmentPedido.Arguments = arguments;

                            ft.Replace(frmLayout.Id, fragmentPedido, "Pedido");
                            ft.AddToBackStack("Pedido");
                        }
                        else
                            SupportFragmentManager.PopBackStack("Pedido", 0);
                    }
                    break;
            }
        }

        private void NavegacaoBroker(bool proximoPasso)
        {
            switch (PassoAtual)
            {
                case 1:
                    {
                        if (proximoPasso)
                        {
                            ft.Replace(frmLayout.Id, new ContatoCliente(), "ContatoCliente");
                            ft.AddToBackStack("ContatoCliente");
                        }
                        else
                            SupportFragmentManager.PopBackStack("ContatoCliente", 0);
                    }
                    break;
                case 2:
                    {
                        if (proximoPasso)
                        {
                            ft.Replace(frmLayout.Id, new GrupoProduto(), "GrupoProduto");
                            ft.AddToBackStack("GrupoProduto");
                        }
                        else
                            SupportFragmentManager.PopBackStack("GrupoProduto", 0);
                    }
                    break;
                case 3:
                    {
                        if (proximoPasso)
                        {
                            ft.Replace(frmLayout.Id, new ProdutosVencer(), "ProdutosVencer");
                            ft.AddToBackStack("ProdutosVencer");
                        }
                        else
                            SupportFragmentManager.PopBackStack("ProdutosVencer", 0);
                    }
                    break;
                case 4:
                    {
                        if (proximoPasso)
                        {
                            var frag = new ListaProdutos();
                            IDListaProduto = frag.Id;

                            ft.Replace(frmLayout.Id, frag, "ListaProdutos");
                            ft.AddToBackStack("ListaProdutos");
                        }
                        else
                            SupportFragmentManager.PopBackStack("ListaProdutos", 0);
                    }
                    break;
                case 5:
                    {
                        if (proximoPasso)
                        {
                            ft.Replace(frmLayout.Id, new Produto(), "Produto");
                            ft.AddToBackStack("Produto");
                        }
                        else
                            SupportFragmentManager.PopBackStack("Produto", 0);
                    }
                    break;
                case 6:
                    {
                        if (proximoPasso)
                        {
                            ft.Replace(frmLayout.Id, new ListaProdutoVencimento(), "ListaProdutoVencimento");
                            ft.AddToBackStack("ListaProdutoVencimento");
                        }
                        else
                            SupportFragmentManager.PopBackStack("ListaProdutoVencimento", 0);
                    }
                    break;
                case 7:
                    {
                        if (proximoPasso)
                        {
                            ft.Replace(frmLayout.Id, new MensagemPedido(), "MensagemPedido");
                            ft.AddToBackStack("MensagemPedido");
                        }
                        else
                            SupportFragmentManager.PopBackStack("MensagemPedido", 0);
                    }
                    break;
                case 8:
                    {
                        if (proximoPasso)
                        {
                            ft.Replace(frmLayout.Id, new UltimasVisitas(), "UltimasVisitas");
                            ft.AddToBackStack("UltimasVisitas");
                        }
                        else
                            SupportFragmentManager.PopBackStack("UltimasVisitas", 0);
                    }
                    break;
                case 9:
                    {
                        if (proximoPasso)
                        {
                            ft.Replace(frmLayout.Id, new DocumentoReceberFragment(), "DocumentoReceberFragment");
                            ft.AddToBackStack("DocumentoReceberFragment");
                        }
                        else
                            SupportFragmentManager.PopBackStack("DocumentoReceberFragment", 0);
                    }
                    break;
                case 10:
                    {
                        if (proximoPasso)
                        {
                            ft.Replace(frmLayout.Id, new Pedido(), "Pedido");
                            ft.AddToBackStack("Pedido");
                        }
                        else
                            SupportFragmentManager.PopBackStack("Pedido", 0);
                    }
                    break;
            }
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            //if (CSPDVs.Current.PESQUISA_MERCADO.Count > 0 &&
            //  CSPDVs.Current.IND_PERMITIR_PESQUISA)
            //    menu.FindItem(Resource.Id.itmPesquisa).SetVisible(true);
            //else
            menu.FindItem(Resource.Id.itmPesquisa).SetVisible(false);

            if (!IsBroker() &&
                (CSPDVs.Current.COD_CATEGORIA != 10301 &&
                CSPDVs.Current.COD_CATEGORIA != 10302 &&
                CSPDVs.Current.COD_CATEGORIA != 10303 &&
                CSPDVs.Current.COD_CATEGORIA != 10304 &&
                CSPDVs.Current.COD_CATEGORIA != 10305 &&
                CSPDVs.Current.COD_CATEGORIA != 30101 &&
                CSPDVs.Current.COD_CATEGORIA != 30102 &&
                CSPDVs.Current.COD_CATEGORIA != 20101 &&
                CSPDVs.Current.COD_CATEGORIA != 60402))
                menu.FindItem(Resource.Id.itmLayout).SetVisible(false);
            else
                menu.FindItem(Resource.Id.itmLayout).SetVisible(true);

            if (PassoAtual != PassoDigitacaoProduto())
            {
                menu.FindItem(Resource.Id.itmDetalhePreco).SetVisible(false);
                menu.FindItem(Resource.Id.itmUltimosPedidosProduto).SetVisible(false);
            }
            else
            {
                if (IsBroker())
                    menu.FindItem(Resource.Id.itmDetalhePreco).SetVisible(true);

                menu.FindItem(Resource.Id.itmUltimosPedidosProduto).SetVisible(true);
            }

            if (IsBroker())
            {
                menu.FindItem(Resource.Id.itmIndenizacao).SetVisible(true);
                menu.FindItem(Resource.Id.itmHistoricoIndenizacao).SetVisible(true);
            }
            else
            {
                menu.FindItem(Resource.Id.itmIndenizacao).SetVisible(false);
                menu.FindItem(Resource.Id.itmHistoricoIndenizacao).SetVisible(false);
            }

            if (IsBroker())
                menu.FindItem(Resource.Id.itmUltimasVisitas).SetVisible(false);
            else
                menu.FindItem(Resource.Id.itmUltimasVisitas).SetVisible(true);

            return true;
        }

        private static void GravaInformacaoFlexxGPS()
        {
            string dadosPedidos = "";
            string pathArquivo = "";
            string[] pedidosVendedor;
            int indicePedido = -1;
            bool gerarArquivo = false;

            pedidosVendedor = new string[20];

            pathArquivo = System.IO.Path.Combine("/sdcard/FLAGPS_BD/ENVIAR/", "AV" + monitoramento.COD_PDV.ToString());

            // Codigo da Revenda
            dadosPedidos = CSEmpresa.Current.CODIGO_REVENDA.ToString().Trim();

            // Codigo do vendedor
            dadosPedidos += "|" + monitoramento.COD_EMPREGADO.ToString().Trim();

            // Codigo do cliente
            dadosPedidos += "|" + monitoramento.COD_PDV.ToString().Trim();

            // (Inicial) Latitude & Longitude
            dadosPedidos += "|LOC_INICIAL";

            // (Final) Latitude & Longitude
            dadosPedidos += "|" + localizacaoFlexxGpsFinal;

            // Data
            dadosPedidos += "|" + DateTime.Now.ToString("dd/MM/yyyy");

            // Horas inicial 
            dadosPedidos += "|" + monitoramento.DAT_ENTRADA.ToString("HH:mm:ss");

            // Horas final 
            dadosPedidos += "|" + monitoramento.DAT_SAIDA.ToString("HH:mm:ss");

            // Motivo de não compra            
            if (CSPDVs.Current.PEDIDOS_PDV.Items.Count == 0)
            {
                CSHistoricosMotivo.CSHistoricoMotivo naoPositivado = new CSHistoricosMotivo.CSHistoricoMotivo();
                int codigoMotivo = -1;
                bool achouMotivo = false;
                string latitude = string.Empty;
                string longitude = string.Empty;

                // [ Buscar a partir do PDV se existe motivo de não positivacao ]
                foreach (CSHistoricosMotivo.CSHistoricoMotivo motivo in CSPDVs.Current.HISTORICOS_MOTIVO)
                {
                    naoPositivado = motivo;
                    codigoMotivo = motivo.COD_MOTIVO;
                    latitude = motivo.NUM_LATITUDE_LOCALIZACAO;
                    longitude = motivo.NUM_LONGITUDE_LOCALIZACAO;
                    break;
                }

                // [ Busca o motivos do tipo escolhido ]
                foreach (CSMotivos.CSMotivo motivo in CSMotivos.Items)
                {
                    if (motivo.COD_TIPO_MOTIVO == CSMotivos.CSTipoMotivo.NAO_POSITIVACAO_CLIENTE)
                    {
                        if (codigoMotivo == motivo.COD_MOTIVO)
                        {
                            dadosPedidos = dadosPedidos.Replace("LOC_INICIAL", string.Format("{0}|{1}", latitude, longitude));
                            dadosPedidos += "|" + motivo.COD_MOTIVO.ToString().Trim() + " - " + motivo.DSC_MOTIVO.Trim();
                            achouMotivo = true;
                            gerarArquivo = true;
                            break;
                        }
                    }
                }

                if (!achouMotivo)
                    dadosPedidos += "|";

                //dadosPedidos += "| ";

                //Inclusão posição referente ao nome de imagem
                dadosPedidos += string.Format("|||||{0}", CSPDVs.Current.DSC_NOME_FOTO);
                dadosPedidos += "|" + Android.OS.Build.VERSION.Release;
                dadosPedidos += "|" + string.Format("{0} {1}", Android.OS.Build.Manufacturer, Android.OS.Build.Model);
                dadosPedidos += "|" + (string.IsNullOrEmpty(CSPDVs.Current.IND_FOTO_DUVIDOSA) ? "N" : CSPDVs.Current.IND_FOTO_DUVIDOSA);
                dadosPedidos += "|" + CSPDVs.Current.NUM_LATITUDE_FOTO;
                dadosPedidos += "|" + CSPDVs.Current.NUM_LONGITUDE_FOTO;
            }
            else
            {
                dadosPedidos += "| ";  // Motivo de não compra     
                indicePedido = 0;

                foreach (CSPedidosPDV.CSPedidoPDV pedido in CSPDVs.Current.PEDIDOS_PDV.Items)
                {
                    if (indicePedido <= 20)
                    {
                        dadosPedidos = dadosPedidos.Replace("LOC_INICIAL", string.Format("{0}|{1}", pedido.NUM_LATITUDE_LOCALIZACAO, pedido.NUM_LONGITUDE_LOCALIZACAO));
                        pedidosVendedor[indicePedido] = dadosPedidos.Trim();
                        pedidosVendedor[indicePedido] += "|" + pedido.COD_PEDIDO.ToString();
                        pedidosVendedor[indicePedido] += "|" + pedido.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO.ToString().Trim() + "-" + pedido.CONDICAO_PAGAMENTO.DSC_CONDICAO_PAGAMENTO.Trim();
                        pedidosVendedor[indicePedido] += "|" + pedido.VLR_TOTAL_PEDIDO.ToString(CSGlobal.DecimalStringFormat);
                        pedidosVendedor[indicePedido] += "|" + pedido.ITEMS_PEDIDOS.Count.ToString().Trim();

                        if (indicePedido == 0)
                            pedidosVendedor[indicePedido] += "|" + CSPDVs.Current.DSC_NOME_FOTO;
                        else
                            pedidosVendedor[indicePedido] += "|";

                        pedidosVendedor[indicePedido] += "|" + Android.OS.Build.VERSION.Release;
                        pedidosVendedor[indicePedido] += "|" + string.Format("{0} {1}", Android.OS.Build.Manufacturer, Android.OS.Build.Model);

                        if (indicePedido == 0)
                        {
                            pedidosVendedor[indicePedido] += "|" + (string.IsNullOrEmpty(CSPDVs.Current.IND_FOTO_DUVIDOSA) ? "N" : CSPDVs.Current.IND_FOTO_DUVIDOSA);
                            pedidosVendedor[indicePedido] += "|" + CSPDVs.Current.NUM_LATITUDE_FOTO;
                            pedidosVendedor[indicePedido] += "|" + CSPDVs.Current.NUM_LONGITUDE_FOTO;
                        }
                        else
                        {
                            pedidosVendedor[indicePedido] += "|||";
                        }

                        gerarArquivo = true;
                    }
                    else
                        break;

                    indicePedido += 1;
                }
            }

            // Cria os diretorios do FlexX GPS caso não tenha
            CSGlobal.criaDiretoriosFlexxGPS();

            // Gera arquivo quando tem pedido ou motivo de não compra.
            if (gerarArquivo)
            {
                if (System.IO.File.Exists(pathArquivo))
                    System.IO.File.Delete(pathArquivo);

                if (System.IO.File.Exists(pathArquivo + ".txt"))
                    System.IO.File.Delete(pathArquivo + ".txt");

                System.IO.TextWriter fileOut = System.IO.File.CreateText(pathArquivo);

                if (indicePedido == -1)
                    fileOut.WriteLine(dadosPedidos.ToString());
                else
                {
                    for (int i = 0; i < pedidosVendedor.Length; i++)
                        if (pedidosVendedor[i] != null)
                            fileOut.WriteLine(pedidosVendedor[i].Trim());
                }

                fileOut.Close();

                System.IO.File.Move(pathArquivo, pathArquivo + ".txt");
            }
        }

        internal void AbrirPedidos()
        {
            NavegarParaPasso(1, true);

            ft = SupportFragmentManager.BeginTransaction();

            ft.Replace(frmLayout.Id, new ListaPedido(), "ListaPedido");
            ft.AddToBackStack("ListaPedido");
            ft.SetCustomAnimations(Resource.Animation.slide_right_to_left, Resource.Animation.slide_left_to_right);
            ft.Commit();
        }

        public static void Fechar(bool finishActivity)
        {
            try
            {
                if (CSPDVs.Current == null)
                    return;

                try
                {
                    if (CSEmpresa.Current.IND_UTILIZA_FLEXX_GPS == "S")
                        localizacaoFlexxGpsFinal = CSGlobal.GetLocalizacaoFlexXGPS();
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.AlertErro(CurrentActivity, "Acesso negado à pasta de gravação de dados do FlexX GPS");
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.AlertErro(CurrentActivity, ex.Message);
                    return;
                }
                monitoramento.DAT_SAIDA = DateTime.Now;

                monitoramento.LOG_GPS_FINAL = localizacaoFlexxGpsFinal;

                // Adiciona o historico do motivo na coleção de monitoramento
                CSPDVs.Current.MONITORAMENTOS.Add(monitoramento);
                // Dispara o metodo de salvamento dos dados no banco			
                CSPDVs.Current.MONITORAMENTOS.Flush();

                //Grava informação para o FlexX GPS
                try
                {
                    if (CSEmpresa.Current.IND_UTILIZA_FLEXX_GPS == "S")
                    {
                        GravaInformacaoFlexxGPS();
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.AlertErro(CurrentActivity, "Acesso negado à pasta de gravação de dados do FlexX GPS");
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.AlertErro(CurrentActivity, ex.Message);
                    return;
                }

                localizacaoFlexxGpsInicial = "";
                localizacaoFlexxGpsFinal = "";

                CSPDVs.Current.PEDIDOS_PDV.Dispose();
                CSPDVs.Current.POLITICA_BROKER = null;
                CSPDVs.Current.POLITICA_BROKER_2014 = null;
                CSPDVs.Current.CONTATOS_PDV.Current = null;

                CSPDVs.Items.Dispose();

                if (finishActivity)
                {
                    decimal vlr_venda = 0;

                    foreach (var pedido in CSPDVs.Current.PEDIDOS_PDV.Items.Cast<CSPedidosPDV.CSPedidoPDV>().Where(p => p.STATE != ObjectState.DELETADO))
                    {
                        vlr_venda += pedido.VLR_TOTAL_PEDIDO;
                    }

                    var data = new Intent();
                    data.PutExtra("cod_pdv", CSPDVs.Current.COD_PDV);
                    data.PutExtra("vlr_venda", Convert.ToDouble(vlr_venda));
                    CurrentActivity.SetResult(Result.Ok, data);

                    CSProdutos.DescartaPoliticaBroker();

                    CurrentActivity.Finish();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public override void OnBackPressed()
        {
            if (MenuClicado)
            {
                if (PassoAtual == PassoDigitacaoProduto())
                {
                    ProdutoVenda.ValidacoesVenda(false, true);
                    AtualizarValorParcial();
                }
                else
                {
                    MenuClicado = false;
                    base.OnBackPressed();
                }
            }
            else if (IsBroker() && PassoAtual == 4)
            {
                PassoAtual--;
                TratamentosPasso(false);
                SupportFragmentManager.PopBackStack("ProdutosVencer", 0);
                AlterarLabelPasso();
            }
            else if (PassoAtual == PassoDigitacaoProduto())
            {
                ProdutoVenda.ValidacoesVenda(false, false);
                AtualizarValorParcial();
            }
            else if (PassoAtual == 7 && CSGlobal.PedidoComCombo)
            {
                MessageBox.Alert(this, "Não é permitido voltar para os passos anteriores com pedido combo.");
            }
            else
            {
                if (CSPDVs.Current != null &&
                    CSPDVs.Current.PESQUISA_MERCADO != null &&
                    CSPDVs.Current.PESQUISA_MERCADO.Current != null)
                {
                    if (!CSPDVs.Current.PESQUISA_MERCADO.Current.PESQUISA_RESPONDIDA)
                    {
                        MessageBox.Alert(this, "Nenhuma pergunta foi respondida.\nDeseja informar o motivo da NÃO RESPOSTA?", "Informar", (sender, e) =>
                        {
                            Android.Support.V4.App.Fragment motivoNaoPesquisa = new MotivoNaoPositivado();

                            Bundle bundle = new Bundle();
                            bundle.PutInt("TipoMotivo", CSMotivos.CSTipoMotivo.NAO_PESQUISA_MERCADO);
                            motivoNaoPesquisa.Arguments = bundle;

                            ft.Replace(frmLayout.Id, motivoNaoPesquisa);
                            ft.Commit();

                        }, "Cancelar",
                            (sender, e) => { base.OnBackPressed(); }, false);
                    }
                    else
                    {
                        DeletarMotivoDeNaoPesquisa(CSPDVs.Current.PESQUISA_MERCADO.Current.COD_PESQUISA_MERC, CSPDVs.Current.COD_PDV);
                        CSPDVs.Current.PESQUISA_MERCADO.Current = null;
                        VoltarPressionado();
                    }
                }
                else
                {
                    VoltarPressionado();
                }
            }
        }

        public void FinalizarEdicao(bool edicaoProduto)
        {
            EdicaoProduto = false;
            PassoAtual = 10;
            AlterarLabelPasso();
            TratamentosPasso(false);

            if (edicaoProduto)
                base.OnBackPressed();
            else
                SupportFragmentManager.PopBackStack("Pedido", 0);
        }

        private void DeletarMotivoDeNaoPesquisa(int codPesquisa, int codPdv)
        {
            var sql = new StringBuilder();
            sql.AppendFormat("DELETE FROM MOTIVO_NAO_PESQUISA WHERE COD_PESQUISA_MERC = {0} AND COD_PDV = {1}", codPesquisa, codPdv);

            CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());
        }

        public void VoltarProdutosIndicados()
        {
            SupportFragmentManager.PopBackStack("ListaProdutosIndicados", 0);
            PassoAtual = 2;
            AlterarLabelPasso();
            TratamentosPasso(false);
        }

        public void VoltarPressionado()
        {
            if (MotivoNaoCompraProdutoIndicado)
            {
                SalvarMotivoNaoCompraProdutoIndicado();
            }
            else if (SupportFragmentManager.BackStackEntryCount > 0 &&
                                PassoAtual > 1)
            {
                if ((IsBroker() && PassoAtual == 6) ||
                    (!IsBroker() && PassoAtual == 7))
                {
                    PassoAtual = PassoAtual - 2;

                    TratamentosPasso(false);
                    SupportFragmentManager.PopBackStack("ListaProdutos", 0);
                    AlterarLabelPasso();
                }
                else
                {
                    PassoAtual--;
                    AlterarFragment(false,false);
                    //TratamentosPasso();
                    //SupportFragmentManager.PopBackStack();
                    //AlterarLabelPasso();
                }
            }
            else
            {
                string alerta = string.Empty;

                if (CSPDVs.Current != null &&
                    CSPDVs.Current.PEDIDOS_PDV.Items.Count == 0)
                {
                    MotivoNaoPositivado();
                }
                else
                {
                    if (CSGlobal.BloquearSaidaCliente)
                    {
                        MessageBox.Alert(this, "Para sair, descarte (menu) ou salve (passo 10) o pedido atual.");
                    }
                    else
                    {
                        //if (CSEmpregados.Current.VendedorDentroExpediente(ref alerta))
                        //{
                        Fechar(true);
                        //}
                        //else
                        //    this.Finish();
                    }
                }
            }
        }

        private void SalvarMotivoNaoCompraProdutoIndicado()
        {
            Fragments.MotivoNaoPositivado.GravarMotivo();

            if (MotivoInformado.Value)
            {
                btnPassoAnterior.Visibility = ViewStates.Visible;
                btnProximoPasso.Text = "Finalizar pedido";

                ft = SupportFragmentManager.BeginTransaction();
                ft.Replace(frmLayout.Id, new Pedido(), "Pedido");
                ft.AddToBackStack("Pedido");
                ft.SetCustomAnimations(Resource.Animation.slide_right_to_left, Resource.Animation.slide_left_to_right);
                ft.Commit();
            }
        }

        public void MotivoNaoPositivado()
        {
            if (MotivoInformado.HasValue)
            {
                SalvarMotivo();
            }
            else
            {
                MotivoInformado = false;

                Android.Support.V4.App.Fragment naoPositivado = new MotivoNaoPositivado();

                Bundle bundle = new Bundle();
                bundle.PutInt("TipoMotivo", CSMotivos.CSTipoMotivo.NAO_POSITIVACAO_CLIENTE);
                naoPositivado.Arguments = bundle;

                ft = SupportFragmentManager.BeginTransaction();
                ft.Replace(frmLayout.Id, naoPositivado, "MotivoNaoPositivado");
                ft.Commit();

                btnProximoPasso.Text = "Lista de cliente";
            }
        }

        private void SalvarMotivo()
        {
            Fragments.MotivoNaoPositivado.GravarMotivo();

            if (MotivoInformado.Value)
                Fechar(true);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.menu_cliente, menu);
            return true;
        }

        private bool IsBroker()
        {
            return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            MenuClicado = true;
            ft = SupportFragmentManager.BeginTransaction();

            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    {
                        DrawerToggle.OnOptionsItemSelected(item);
                        MenuClicado = false;
                    }
                    break;
                case Resource.Id.itmLayout:
                    {
                        Intent i = new Intent();
                        i.SetClass(this, typeof(Layout));
                        StartActivity(i);
                    }
                    break;
                case Resource.Id.itmClipp:
                    {
                        ft.Replace(frmLayout.Id, new Clipp());
                        ft.AddToBackStack("Clipp");
                    }
                    break;
                case Resource.Id.itmListaEmail:
                    {
                        ft.Replace(frmLayout.Id, new ListaEmail());
                        ft.AddToBackStack("ListaEmail");
                    }
                    break;
                case Resource.Id.itmListaTelefone:
                    {
                        ft.Replace(frmLayout.Id, new ListaTelefone());
                        ft.AddToBackStack("ListaTelefone");
                    }
                    break;
                case Resource.Id.itmInformacoes:
                    {
                        ft.Replace(frmLayout.Id, new Informacoes());
                        ft.AddToBackStack("Informacoes");
                    }
                    break;
                case Resource.Id.itmDocumentoReceber:
                    {
                        ft.Replace(frmLayout.Id, new DocumentoReceberFragment(), "DocumentoReceberFragment");
                        ft.AddToBackStack("DocumentoReceberFragment");
                    }
                    break;
                case Resource.Id.itmHistoricoIndenizacao:
                    {
                        if (CSPDVs.Current.HISTORICO_INDENIZACOES.Count > 0)
                        {
                            Intent i = new Intent();
                            i.SetClass(this, typeof(HistoricoIndenizacao));
                            StartActivity(i);
                        }
                        else
                        {
                            MessageBox.Alert(this, "Nenhum histórico de indenização disponível.");
                        }
                    }
                    break;
                case Resource.Id.itmColetaEstoque:
                    {
                        Intent i = new Intent();
                        i.SetClass(this, typeof(ColetarEstoque));
                        this.StartActivity(i);
                    }
                    break;
                case Resource.Id.itmDetalhePreco:
                    {
                        DetalhePreco detalhePreco = new DetalhePreco();

                        Bundle arguments = new Bundle();

                        ProdutoVenda.BtnCalcular_Click(null, null);

                        arguments.PutString("txtQtdeInteiro", ProdutoVenda.txtQtdeInteiro.Text);
                        arguments.PutString("txtQtdeUnidade", ProdutoVenda.txtQtdeUnidade.Text);
                        arguments.PutString("lblValorTabela", ProdutoVenda.lblValorTabela.Text);
                        if (IsBroker())
                        {
                            arguments.PutString("txtDescIncond", ProdutoVenda.txtDescIncond.Text);
                        }
                        else
                        {
                            arguments.PutString("txtDescIncond", ProdutoVenda.txtDescIncond.Text);
                            arguments.PutString("lblValorDesconto", ProdutoVenda.lblValorDescontoUnitario.Text);
                            arguments.PutString("lblValorAdicionalFinanceiro", ProdutoVenda.lblValorAdicionalFinanceiro.Text);
                            CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.AplicaDescontoMaximoProdutoTabPreco();
                        }

                        arguments.PutString("descMaximo", CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_DESCONTO_MAXIMO.ToString());

                        detalhePreco.Arguments = arguments;
                        detalhePreco.Show(SupportFragmentManager, "DetalhePreco");
                    }
                    break;
                case Resource.Id.itmUltimosPedidosProduto:
                    {
                        //ft.Replace(frmLayout.Id, new UltimosPedidosProduto());
                        //ft.AddToBackStack("UltimosPedidosProduto");

                        UltimosPedidosProduto ultimosPedidosProduto = new UltimosPedidosProduto();
                        ultimosPedidosProduto.Show(SupportFragmentManager, "UltimosPedidosProduto");
                    }
                    break;
                case Resource.Id.itmUltimasVisitas:
                    {
                        ft.Replace(frmLayout.Id, new UltimasVisitas(), "UltimasVisitas");
                        ft.AddToBackStack("UltimasVisitas");
                    }
                    break;
                case Resource.Id.itmPesquisa:
                    {
                        if (!CSEmpresa.ColunaExiste("RESPOSTA_PESQUISA_MERCADO", "COD_EMPREGADO") ||
                            !CSEmpresa.ColunaExiste("MOTIVO_NAO_PESQUISA", "COD_EMPREGADO"))
                        {
                            MessageBox.Alert(this, "Banco de dados desatualizado. Faça a atualização e uma carga antes de iniciar a pesquisa de mercado.");
                        }
                        else
                        {
                            if (CSPDVs.Current.PESQUISA_MERCADO.Count > 0)
                            {
                                if (CSPDVs.Current.PESQUISA_MERCADO.Count == 1)
                                {
                                    CSPDVs.Current.PESQUISA_MERCADO.Current = CSPDVs.Current.PESQUISA_MERCADO[0];
                                    AbrirPesquisaMercado();
                                }
                                else
                                {
                                    ft.Replace(frmLayout.Id, new ListaPesquisaMercado());
                                    ft.AddToBackStack("ListaPesquisaMercado");
                                }
                            }
                            else
                                MessageBox.Alert(this, "Não existe pesquisa cadastrada.");
                        }
                    }
                    break;
                case Resource.Id.itmListaPedido:
                    {
                        ft.Replace(frmLayout.Id, new ListaPedido(), "ListaPedido");
                        ft.AddToBackStack("ListaPedido");
                    }
                    break;
                case Resource.Id.itmIndenizacao:
                    {
                        Intent i = new Intent();
                        i.SetClass(this, typeof(ListaIndenizacao));
                        this.StartActivity(i);
                    }
                    break;
                case Resource.Id.itmDescartarPedido:
                    {
                        MessageBox.Alert(this, "Deseja descartar o pedido atual?", "Descartar",
                        (_sender, _e) =>
                        {
                            if (Pedido.PdvSemPedido())
                            {
                                NavegarParaPasso(1);
                                OnBackPressed();
                            }
                            else
                            {
                                if (CSPDVs.Current.PEDIDOS_PDV.Current != null)
                                {
                                    CSPDVs.Current.PEDIDOS_PDV.Current.STATE = ObjectState.DELETADO;

                                    CSPDVs.Current.PEDIDOS_PDV.Flush();
                                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS = null;

                                    if (CSEmpresa.Current.IND_LIMITE_DESCONTO)
                                        CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO.Flush();

                                    AbrirPedidos();

                                    CSGlobal.BloquearSaidaCliente = false;
                                    CSPDVs.Current.PEDIDOS_PDV.Current = null;
                                }
                            }

                            AtualizarValorParcial();

                        }, "Cancelar", null, true);
                    }
                    break;
                default:
                    return base.OnOptionsItemSelected(item);
            }

            ft.SetCustomAnimations(Resource.Animation.slide_right_to_left, Resource.Animation.slide_left_to_right);
            ft.Commit();

            return true;
        }

        public void AbrirPesquisaMercado()
        {
            ft.Replace(frmLayout.Id, new PesquisaMercado());
            ft.Commit();
        }

        public void BloqueioProduto(string[] aBloqueios, bool bloqueioProduto)
        {
            MessageBox.Alert(this, "Produto bloqueado.");

            //Android.Support.V4.App.Fragment bloqueio = new BlqTabela();

            //Bundle bundle = new Bundle();
            //bundle.PutStringArray("bloqueios", aBloqueios);
            //bundle.PutBoolean("bloqueioProduto", bloqueioProduto);
            //bloqueio.Arguments = bundle;

            //ft.Replace(frmLayout.Id, bloqueio);
            //ft.Commit();
        }

        public void SalvarPedido(bool proximoPasso, bool edicaoProduto)
        {
            try
            {
                //if (blIndenizacaoItem == false)
                //    CSPDVs.Current.PEDIDOS_PDV.Current.CalculaRateioIndenizacao(CSGlobal.StrToDecimal(txtDescontoIndenizacao.Text));

                if (CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO == -1)
                    CSPDVs.Current.PEDIDOS_PDV.Add(CSPDVs.Current.PEDIDOS_PDV.Current);

                if (CSGlobal.PedidoSugerido)
                {
                    CSGlobal.ExisteProdutoColetadoPerda = false;
                }

                //if ((CSItemCombo)cboCondicao.SelectedItem != null)
                //{
                //    foreach (CSItemsPedido.CSItemPedido itemPedido in CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS)
                //    {
                //        itemPedido.PRC_ADICIONAL_FINANCEIRO = ((CSCondicoesPagamento.CSCondicaoPagamento)((CSItemCombo)cboCondicao.SelectedItem).Valor).PRC_ADICIONAL_FINANCEIRO;

                //        if (CSGlobal.PedidoSugerido)
                //        {
                //            if (itemPedido.QTD_INDENIZACAO_EXIBICAO > 0)
                //                CSGlobal.ExisteProdutoColetadoPerda = true;
                //        }
                //    }

                //CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO = ((CSCondicoesPagamento.CSCondicaoPagamento)((CSItemCombo)cboCondicao.SelectedItem).Valor).COD_CONDICAO_PAGAMENTO;
                //}

                //if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2)
                //    Emissor();
                //else
                //{
                //    //// Busca o emissor
                //    //CSEmissoresPDV.CSEmissorPDV emissor = (CSEmissoresPDV.CSEmissorPDV)((CSItemCombo)cboEmissor.SelectedItem).Valor;

                //    //CSPDVs.Current.PEDIDOS_PDV.Current.COD_PDV_SOLDTO = emissor.COD_PDV_SOLDTO;

                //    CSPDVs.Current.PEDIDOS_PDV.Current.COD_PDV_SOLDTO = CSPDVs.Current.COD_PDV;
                //}

                // Acerta Politica de calculo do preço
                CSPDVs.Current.PEDIDOS_PDV.Current.COD_POLITICA_CALCULO_PRECO = CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA;

                CSPDVs.Current.PEDIDOS_PDV.Flush();

                if (CSEmpresa.Current.IND_LIMITE_DESCONTO)
                    CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO.Flush();

                // Atualiza Pedido de Indenização
                if ((CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2) &&
                    CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO == 20 &&
                    CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDOS_INDENIZACAO.Count > 0)
                {
                    // Atualiza Pedido de Indenização                    
                    foreach (CSPedidosIndenizacao.CSPedidoIndenizacao pedidoIndenizacao in CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDOS_INDENIZACAO.Items)
                    {
                        if (pedidoIndenizacao.COD_PEDIDO == CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO)
                            CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDOS_INDENIZACAO.Current = pedidoIndenizacao;
                    }

                    CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDOS_INDENIZACAO.Flush();
                }

                if (EdicaoProduto)
                {
                    FinalizarEdicao(edicaoProduto);
                }
                else
                {
                    if (RotinaProdutosIndicados)
                    {
                        VoltarProdutosIndicados();
                    }
                    else
                    {
                        if (proximoPasso)
                        {
                            PassoAtual++;

                            AlterarFragment(true,false);
                        }
                        else
                            VoltarPressionado();
                    }
                }
            }
            catch (Exception)
            {

            }
        }
        public void MotivoNaoCompraIndicado()
        {
            MotivoNaoCompraProdutoIndicado = true;

            Android.Support.V4.App.Fragment naoPositivado = new MotivoNaoPositivado();

            Bundle bundle = new Bundle();
            bundle.PutInt("TipoMotivo", CSMotivos.CSTipoMotivo.NAO_COMPRA_PRODUTOS_INDICADOS);
            naoPositivado.Arguments = bundle;

            ft = SupportFragmentManager.BeginTransaction();
            ft.Replace(frmLayout.Id, naoPositivado, "MotivoNaoCompraProdutoIndicado");
            ft.AddToBackStack("MotivoNaoCompraProdutoIndicado");
            ft.Commit();

            btnPassoAnterior.Visibility = ViewStates.Invisible;
            btnProximoPasso.Text = "Salvar";
        }
    }
}