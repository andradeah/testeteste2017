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
using AvanteSales.Pro.Activities;
using AvanteSales.Pro.Dialogs;
using AvanteSales.Pro.Formatters;
using AvanteSales.SystemFramework;

namespace AvanteSales.Pro.Fragments
{
    public class ListaProdutos : Android.Support.V4.App.Fragment
    {
        private int currentPostion;
        int PositionItemVendido;
        static ProgressDialog progress;
        static ProgressDialog progressFamilia;
        LayoutInflater thisLayoutInflater;
        static Android.Support.V4.App.FragmentActivity ActivityContext;
        static ListView listProdutos;
        static ProdutosListViewBaseAdapter ProdutosListViewBaseAdapterProp;
        Button btnTrocarGrupo;
        EditText txtPesquisa;
        static List<CSProdutos.CSProduto> AdapterProdutos;
        static Cliente cliente;
        static Spinner spnFamilia;
        static ArrayAdapter FamiliaAdapter;
        static int FamiliaSelecionada;
        LinearLayout llOpcoes;
        CheckBox chkExibirImagens;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                var view = inflater.Inflate(Resource.Layout.lista_produtos, container, false);
                thisLayoutInflater = inflater;
                FindViewsById(view);
                Eventos();
                ActivityContext = ((Cliente)Activity);
                ProdutosListViewBaseAdapterProp = new ProdutosListViewBaseAdapter(ActivityContext);
                listProdutos.Adapter = ProdutosListViewBaseAdapterProp;
                cliente = (Cliente)Activity;
                return view;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void Eventos()
        {
            btnTrocarGrupo.Click += BtnTrocarGrupo_Click;
            txtPesquisa.TextChanged += TxtPesquisa_TextChanged;
            listProdutos.ItemClick += ListProdutos_ItemClick;
            spnFamilia.ItemSelected += SpnFamilia_ItemSelected;
            chkExibirImagens.CheckedChange += ChkExibirImagens_CheckedChange;
        }

        private void ChkExibirImagens_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                CSConfiguracao.SetConfig("ExibirImagem", chkExibirImagens.Checked.ToString());

                SaveScrollPosition();

                ProdutosListViewBaseAdapterProp = new ProdutosListViewBaseAdapter(ActivityContext);
                listProdutos.Adapter = ProdutosListViewBaseAdapterProp;

                if (((Cliente)Activity).LinhaSelecionada != null)
                    CarregarListaProdutos();

                SetScrollPosition();
            }
            catch (Exception)
            {

            }
        }

        private void SaveScrollPosition()
        {
            try
            {
                currentPostion = listProdutos.FirstVisiblePosition;
            }
            catch (Exception)
            {

            }
        }

        private void SetScrollPosition()
        {
            try
            {
                listProdutos.SetSelection(currentPostion);
            }
            catch (Exception)
            {

            }
        }

        private void SpnFamilia_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (cliente.LinhaSelecionada != null)
            {
                FamiliaSelecionada = ((CSFamiliasProduto.CSFamiliaProduto)((CSItemCombo)spnFamilia.SelectedItem).Valor).COD_FAMILIA_PRODUTO_FILTRADO;
                cliente.FamiliaSelecionada = e.Position;
                CarregarListaProdutos();
            }
        }

        private void ListProdutos_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (!CSGlobal.PedidoComCombo)
            {
                CSProdutos.Current = (CSProdutos.CSProduto)listProdutos.Adapter.GetItem(e.Position);
                PositionItemVendido = e.Position;
                cliente.AbrirDialogProduto(ProdutoFoiVendidoNasUltimasVisitas(), !CSGlobal.PedidoComCombo, false, false);
            }
            else
                MessageBox.Alert(ActivityContext, "Não é possível adicionar produtos em um pedido combo.");
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

        private void TxtPesquisa_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                List<CSProdutos.CSProduto> produtosFiltrados = new List<CSProdutos.CSProduto>();

                if (!string.IsNullOrEmpty(txtPesquisa.Text))
                {
                    int codigo = 0;

                    if (int.TryParse(txtPesquisa.Text, out codigo))
                    {
                        produtosFiltrados = AdapterProdutos.Where(p => p.DESCRICAO_APELIDO_PRODUTO.StartsWith(codigo.ToString()))
                                                       .ToList();
                    }
                    else
                    {
                        produtosFiltrados = AdapterProdutos.Where(p => p.DSC_APELIDO_PRODUTO.ToUpper().Contains(txtPesquisa.Text.ToUpper()) || p.DSC_PRODUTO.ToUpper().Contains(txtPesquisa.Text.ToUpper()))
                                                       .ToList();
                    }

                    ProdutosListViewBaseAdapterProp.UpdateAdapter(produtosFiltrados);
                }
                else
                {
                    ProdutosListViewBaseAdapterProp.UpdateAdapter(AdapterProdutos);
                }
            }
            catch (Exception ex)
            {
                MessageBox.AlertErro(ActivityContext, ex.Message);
            }
        }

        private static bool IsBroker()
        {
            return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
        }

        private void BtnTrocarGrupo_Click(object sender, EventArgs e)
        {
            if (IsBroker())
                cliente.NavegarParaPasso(2);
            else
                cliente.NavegarParaPasso(3);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            Inicializacao();
        }

        private void FindViewsById(View view)
        {
            listProdutos = view.FindViewById<ListView>(Resource.Id.listProdutos);
            btnTrocarGrupo = view.FindViewById<Button>(Resource.Id.btnTrocarGrupo);
            txtPesquisa = view.FindViewById<EditText>(Resource.Id.txtPesquisa);
            spnFamilia = view.FindViewById<Spinner>(Resource.Id.spnFamilia);
            llOpcoes = view.FindViewById<LinearLayout>(Resource.Id.llOpcoes);
            chkExibirImagens = view.FindViewById<CheckBox>(Resource.Id.chkExibirImagens);
        }

        private void Inicializacao()
        {
            if (!CSEmpregados.Current.IND_PERMITIR_ROTINA_IMAGEM)
            {
                CSConfiguracao.SetConfig("ExibirImagem", "false");
                llOpcoes.Visibility = ViewStates.Gone;
            }
            else
            {
                string exibeImagens = CSConfiguracao.GetConfig("ExibirImagem");

                if (exibeImagens.Length > 0)
                    chkExibirImagens.Checked = bool.Parse(exibeImagens);
                else
                    chkExibirImagens.Checked = true;
            }

            if (((Cliente)Activity).LinhaSelecionada != null)
            {
                progressFamilia = new ProgressDialogCustomizado(ActivityContext, thisLayoutInflater).Customizar();
                progressFamilia.Show();

                new ThreadCarregarFamiliaProduto().Execute();
            }
        }

        private class ThreadCarregarFamiliaProduto : AsyncTask
        {
            int position = 0;
            //static int codFamilia;

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                int i = 0;

                if ((IsBroker() && cliente.GrupoSelecionado > 0) ||
                    (!IsBroker() && cliente.GrupoSelecionado != 0))
                {
                    // Busca o grupo selecionado
                    int grupo = cliente.GrupoSelecionado;
                    int grupoComercializacao = cliente.LinhaSelecionada.COD_GRUPO_COMERCIALIZACAO_FILTRADO;

                    FamiliaAdapter = new ArrayAdapter(ActivityContext, Android.Resource.Layout.SimpleSpinnerItem);
                    FamiliaAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

                    // Adiciona um opção para selecionar todos as produtos da familia
                    CSFamiliasProduto.CSFamiliaProduto famtodos = new CSFamiliasProduto.CSFamiliaProduto();
                    famtodos.GRUPO = CSGruposProduto.GetGrupoProduto(grupo);
                    famtodos.COD_FAMILIA_PRODUTO = -1;
                    famtodos.COD_FAMILIA_PRODUTO_FILTRADO = -1;
                    famtodos.DSC_FAMILIA_PRODUTO = "TODOS";
                    famtodos.DSC_FAMILIA_PRODUTO_FILTRADO = "TODOS";

                    CSItemCombo ictodos = new CSItemCombo();
                    ictodos.Texto = famtodos.DSC_FAMILIA_PRODUTO;
                    ictodos.Valor = famtodos;
                    FamiliaAdapter.Add(ictodos);

                    CSFamiliasProduto classeFamilia = new CSFamiliasProduto();
                    var familia = classeFamilia.FamiliaFiltrada(grupo, grupoComercializacao, CSPDVs.Current.COD_PDV, CSPDVs.Current.COD_CATEGORIA, CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA, CSEmpresa.Current.IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE);

                    foreach (CSFamiliasProduto.CSFamiliaProduto fam in familia)
                    {
                        if (fam.GRUPO.COD_GRUPO == grupo)
                        {
                            CSItemCombo ic = new CSItemCombo();
                            fam.DSC_FAMILIA_PRODUTO = fam.DSC_FAMILIA_PRODUTO_FILTRADO;
                            fam.COD_FAMILIA_PRODUTO = fam.COD_FAMILIA_PRODUTO_FILTRADO;
                            ic.Texto = fam.DSC_FAMILIA_PRODUTO;
                            ic.Valor = fam;
                            FamiliaAdapter.Add(ic);

                            //if (codFamilia != -1)
                            //{
                            //    if (fam.COD_FAMILIA_PRODUTO == codFamilia)
                                    position = i;
                            //}


                            i++;
                        }
                    }
                }
                else
                {

                }

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                try
                {
                    base.OnPostExecute(result);

                    if (FamiliaAdapter != null)
                        spnFamilia.Adapter = FamiliaAdapter;

                    // Coloca como default a opção todos os produtos da familia.
                    if (FamiliaAdapter != null)
                    {
                        if (spnFamilia.Adapter != null &&
                            spnFamilia.Adapter.Count > 0)
                        {
                            if (cliente.FamiliaSelecionada == -1)
                                spnFamilia.SetSelection(0);
                            else
                                spnFamilia.SetSelection(cliente.FamiliaSelecionada);
                        }
                    }
                    else
                    {
                        if (spnFamilia.Adapter != null &&
                            spnFamilia.Adapter.Count > 0)
                        {
                            int novaPosition = spnFamilia.SelectedItemPosition + 1;

                            if (novaPosition > (spnFamilia.Adapter.Count - 1))
                                spnFamilia.SetSelection(0);
                            else
                                spnFamilia.SetSelection(novaPosition);
                        }
                    }

                    if (progressFamilia != null)
                        progressFamilia.Dismiss();
                }
                catch (Exception)
                {

                }
            }
        }

        private void CarregarListaProdutos()
        {
            progress = new ProgressDialogCustomizado(ActivityContext, thisLayoutInflater).Customizar();
            progress.Show();

            new ThreadCarregarListaProdutos().Execute();
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
                TextView lblDescProduto;
                TextView lblCodigo;
                ImageView imgVendaMes;
                ImageView imgVendaUltimaVisita;
                ImageView imgEstoque;
                ImageView imgProdEspecifico;
                TextView lblUM;
                TextView lblGC;
                ImageView imgIconeProduto = null;
                CSProdutos.CSProduto produtoAtual = this.Produtos[position];

                if (convertView == null)
                {
                    string exibeImagens = CSConfiguracao.GetConfig("ExibirImagem");
                    bool exibirImagens = false;

                    if (exibeImagens.Length > 0)
                        exibirImagens = bool.Parse(exibeImagens);
                    else
                        exibirImagens = true;

                    if (!CSEmpregados.Current.IND_PERMITIR_ROTINA_IMAGEM ||
                        !exibirImagens)
                    {
                        convertView = LayoutInflater.From(context)
                          .Inflate(Resource.Layout.lista_produtos_row, parent, false);
                    }
                    else
                    {
                        convertView = LayoutInflater.From(context)
                          .Inflate(Resource.Layout.lista_produtos_row_imagem, parent, false);

                        imgIconeProduto = convertView.FindViewById<ImageView>(Resource.Id.imgIconeProduto);
                        convertView.SetTag(imgIconeProduto.Id, imgIconeProduto);
                    }

                    lblDescProduto = convertView.FindViewById<TextView>(Resource.Id.lblDescProduto);
                    lblCodigo = convertView.FindViewById<TextView>(Resource.Id.lblCodigo);
                    imgVendaMes = convertView.FindViewById<ImageView>(Resource.Id.imgVendaMes);
                    imgVendaUltimaVisita = convertView.FindViewById<ImageView>(Resource.Id.imgVendaUltimaVisita);
                    imgEstoque = convertView.FindViewById<ImageView>(Resource.Id.imgEstoque);
                    imgProdEspecifico = convertView.FindViewById<ImageView>(Resource.Id.imgProdEspecifico);
                    lblUM = convertView.FindViewById<TextView>(Resource.Id.lblUM);
                    lblGC = convertView.FindViewById<TextView>(Resource.Id.lblGC);

                    convertView.SetTag(lblDescProduto.Id, lblDescProduto);
                    convertView.SetTag(lblCodigo.Id, lblCodigo);
                    convertView.SetTag(imgVendaMes.Id, imgVendaMes);
                    convertView.SetTag(imgVendaUltimaVisita.Id, imgVendaUltimaVisita);
                    convertView.SetTag(imgEstoque.Id, imgEstoque);
                    convertView.SetTag(imgProdEspecifico.Id, imgProdEspecifico);
                    convertView.SetTag(lblUM.Id, lblUM);
                    convertView.SetTag(lblGC.Id, lblGC);
                }
                else
                {
                    lblDescProduto = (TextView)convertView.GetTag(Resource.Id.lblDescProduto);
                    lblCodigo = (TextView)convertView.GetTag(Resource.Id.lblCodigo);
                    imgVendaMes = (ImageView)convertView.GetTag(Resource.Id.imgVendaMes);
                    imgVendaUltimaVisita = (ImageView)convertView.GetTag(Resource.Id.imgVendaUltimaVisita);
                    imgEstoque = (ImageView)convertView.GetTag(Resource.Id.imgEstoque);
                    imgProdEspecifico = (ImageView)convertView.GetTag(Resource.Id.imgProdEspecifico);
                    lblUM = (TextView)convertView.GetTag(Resource.Id.lblUM);
                    lblGC = (TextView)convertView.GetTag(Resource.Id.lblGC);

                    if (CSEmpregados.Current.IND_PERMITIR_ROTINA_IMAGEM)
                        imgIconeProduto = (ImageView)convertView.GetTag(Resource.Id.imgIconeProduto);
                }

                lblDescProduto.Text = produtoAtual.DSC_PRODUTO;
                lblCodigo.Text = produtoAtual.DESCRICAO_APELIDO_PRODUTO;
                imgVendaMes.SetImageResource(produtoAtual.IND_VENDA_MES ? Resource.Drawable.ic_positivo : Resource.Drawable.ic_negativo);
                imgVendaUltimaVisita.SetImageResource(produtoAtual.IND_VENDA_ULTIMA_VISITA ? Resource.Drawable.ic_positivo : Resource.Drawable.ic_negativo);
                imgEstoque.SetImageResource(produtoAtual.QTD_ESTOQUE > 0 ? Resource.Drawable.ic_positivo : Resource.Drawable.ic_negativo);
                lblUM.Text = produtoAtual.DSC_UNIDADE_MEDIDA;
                lblGC.Text = produtoAtual.GRUPO_COMERCIALIZACAO.DES_GRUPO_COMERCIALIZACAO;

                if (produtoAtual.IND_PROD_TOP_CATEGORIA)
                    imgProdEspecifico.SetImageResource(Resource.Drawable.circulo_verde_top);
                else if (produtoAtual.IND_PROD_ESPECIFICO_CATEGORIA)
                    imgProdEspecifico.SetImageResource(Resource.Drawable.circulo_azul);
                else
                    imgProdEspecifico.SetImageResource(Resource.Drawable.circulo_cinza);

                if (imgIconeProduto != null)
                {
                    var imagemProduto = CSGlobal.ArquivoImagem(produtoAtual);

                    if (imagemProduto != null)
                    {
                        Android.Graphics.Bitmap btmp = imagemProduto.LoadAndResizeBitmap(100, 100);
                        imgIconeProduto.SetImageBitmap(btmp);
                    }
                    else
                        imgIconeProduto.SetImageResource(Resource.Drawable.imagem_indisponivel);
                }

                return convertView;
            }
        }

        private void ReorganizarProdutos(int positionItem)
        {
            AdapterProdutos.RemoveAt(positionItem);
            ProdutosListViewBaseAdapterProp.NotifyDataSetChanged();
        }

        private class ThreadCarregarListaProdutos : AsyncTask
        {
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
                return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 3;
            }

            public static List<CSProdutos.CSProduto> RemoveProdutosJaAdicionadosAoPedido(List<CSProdutos.CSProduto> prodFiltrados)
            {
                try
                {
                    IEnumerable<int> codigoDosProdutosJaAdicionados = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.STATE != ObjectState.DELETADO).Select(p => p.PRODUTO.COD_PRODUTO);
                    prodFiltrados = prodFiltrados.Where(p => !codigoDosProdutosJaAdicionados.Contains(p.COD_PRODUTO)).ToList();
                    return prodFiltrados;
                }
                catch (Exception)
                {
                    return null;
                }
            }

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                int i = 0;

                var produtos = CSProdutos.BuscaProdutos(cliente.LinhaSelecionada.COD_GRUPO_COMERCIALIZACAO_FILTRADO, cliente.GrupoSelecionado, FamiliaSelecionada, CSPDVs.Current.COD_CATEGORIA, CSPDVs.Current.COD_PDV, CSEmpresa.Current.IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE, CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA, false, CSPDVs.Current.COD_DENVER, !IsBroker() && !IsBunge()).Cast<CSProdutos.CSProduto>().ToList();

                AdapterProdutos = RemoveProdutosJaAdicionadosAoPedido(produtos);

                if (AdapterProdutos != null)
                {
                    AdapterProdutos = AdapterProdutos.Where(prd => int.TryParse(prd.DESCRICAO_APELIDO_PRODUTO, out i)).OrderBy(pd => !pd.IND_PROD_TOP_CATEGORIA).ThenBy(p => !p.IND_PROD_ESPECIFICO_CATEGORIA).ThenBy(pr => int.Parse(pr.DESCRICAO_APELIDO_PRODUTO)).ToList();
                    AdapterProdutos.AddRange(produtos.Cast<CSProdutos.CSProduto>().Where(p => !int.TryParse(p.DESCRICAO_APELIDO_PRODUTO, out i)).OrderBy(p => !p.IND_PROD_ESPECIFICO_CATEGORIA).ThenBy(b => b.DESCRICAO_APELIDO_PRODUTO).ToList());
                }

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                ProdutosListViewBaseAdapterProp.UpdateAdapter(AdapterProdutos);
                progress.Dismiss();

                base.OnPostExecute(result);
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