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
using AvanteSales.Pro.Activities;
using AvanteSales.Pro.Controles;
using AvanteSales.Pro.Dialogs;
using AvanteSales.Pro.Formatters;
using Java.Lang;

namespace AvanteSales.Pro.Fragments
{
    public class ProdutoVencimento : Android.Support.V4.App.Fragment
    {
        TextView lblCodigoProduto;
        TextView lblProduto;
        EditText txtQtdVencimento;
        CustomDatePicker txtDataVencimento;
        Button btnOK;
        Button btnCancelar;
        static ProdutosVencimentoListViewBaseAdapter ProdutosVencimentoAdapter;
        ListView listProdutos;
        static List<CSProdutos.CSProdutoVencimento> AdapterProdutos;
        static ProgressDialog progress;
        LayoutInflater thisLayoutInflater;
        static Android.Support.V4.App.FragmentActivity ActivityContext;
        Status m_StatusAtual;
        Status StatusAtual
        {
            get
            {
                return m_StatusAtual;
            }
            set
            {
                if (value == Status.Novo)
                    btnOK.Text = "Inserir";
                else
                    btnOK.Text = "Editar";

                m_StatusAtual = value;
            }
        }
        int indexSelecionado;

        enum Status
        {
            Novo,
            Editar
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.produto_vencimento, container, false);
            indexSelecionado = -1;
            thisLayoutInflater = inflater;
            FindViewsById(view);
            Eventos();
            ProdutosVencimentoAdapter = new ProdutosVencimentoListViewBaseAdapter(Context);
            listProdutos.Adapter = ProdutosVencimentoAdapter;
            ActivityContext = ((Cliente)Activity);
            StatusAtual = Status.Novo;
            CarregarProdutosVencimento();
            return view;
        }

        private void CarregarProdutosVencimento()
        {
            progress = new ProgressDialogCustomizado(ActivityContext, thisLayoutInflater).Customizar();
            progress.Show();

            new ThreadCarregarProdutosVencimento().Execute();
        }

        private class ThreadCarregarProdutosVencimento : AsyncTask
        {
            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                AdapterProdutos = CSProdutos.GetProdutoVencimentoDia(CSProdutos.Current.COD_PRODUTO);

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                ProdutosVencimentoAdapter.UpdateAdapter(AdapterProdutos);

                progress.Dismiss();
            }
        }

        private void Eventos()
        {
            btnOK.Click += BtnOK_Click;
            btnCancelar.Click += BtnCancelar_Click;
            txtDataVencimento.AddTextChangedListener(new Mask(txtDataVencimento, "##/##/####"));
            listProdutos.ItemClick += ListProdutos_ItemClick;
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            LimparCampos();
        }

        private void ListProdutos_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            MessageBox.Alert(ActivityContext, "Deseja editar ou excluir o registro selecionado?", "Editar",
            (_sender, _e) => { EditarRegistro(e.Position); }, "Excluir", (_sender, e_e) => { ExcluirRegistro(e.Position); }, true);
        }

        private void ExcluirRegistro(int position)
        {
            StatusAtual = Status.Novo;

            CSProdutos.RemoverRegistroVencimento(AdapterProdutos[position]);
            AdapterProdutos.RemoveAt(position);
            ProdutosVencimentoAdapter.UpdateAdapter(AdapterProdutos);

            MessageBox.ShowShortMessageBottom(ActivityContext, "Validade de produto deletada.");
        }

        private void EditarRegistro(int position)
        {
            StatusAtual = Status.Editar;

            indexSelecionado = position;
            txtQtdVencimento.Text = AdapterProdutos[indexSelecionado].QTD_AVENCER.ToString();
            txtDataVencimento.Text = AdapterProdutos[indexSelecionado].DAT_VENCIMENTO.ToString("dd/MM/yyyy");
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime data = new DateTime();
                CSProdutos.CSProdutoVencimento produtoVencimento = new CSProdutos.CSProdutoVencimento();
                produtoVencimento.COD_PRODUTO = CSProdutos.Current.COD_PRODUTO;

                if (StatusAtual == Status.Novo &&
                    produtoVencimento.ItemVencimentoColetado(Convert.ToDateTime(txtDataVencimento.Text)))
                {
                    MessageBox.Alert(Activity, "Este produto já foi coletado com o vencimento informado.");
                }
                else if (string.IsNullOrEmpty(txtQtdVencimento.Text) ||
                         string.IsNullOrEmpty(txtDataVencimento.Text))
                {
                    MessageBox.Alert(Activity, "O preenchimento dos campos é obrigatório.");
                }
                else if (Convert.ToDateTime(txtDataVencimento.Text).Date <= DateTime.Now.Date)
                {
                    MessageBox.Alert(Activity, "Data de vencimento inválida.");
                }
                else if (!DateTime.TryParse(txtDataVencimento.Text, out data))
                {
                    MessageBox.Alert(Activity, "Formato de ata de inválido.");
                }
                else
                {
                    if (StatusAtual == Status.Novo)
                    {
                        produtoVencimento.COD_PDV = CSPDVs.Current.COD_PDV;
                        produtoVencimento.COD_EMPREGADO = CSEmpregados.Current.COD_EMPREGADO;
                        produtoVencimento.DAT_COLETA = DateTime.Now;
                        produtoVencimento.DAT_VENCIMENTO = Convert.ToDateTime(txtDataVencimento.Text);
                        produtoVencimento.QTD_AVENCER = Convert.ToInt32(txtQtdVencimento.Text);
                        produtoVencimento.AdicionarVencimento();

                        AdapterProdutos.Add(produtoVencimento);
                        ProdutosVencimentoAdapter.UpdateAdapter(AdapterProdutos);
                    }
                    else
                    {
                        CSProdutos.AlterarRegistroVencimento(AdapterProdutos[indexSelecionado], Convert.ToInt32(txtQtdVencimento.Text), Convert.ToDateTime(txtDataVencimento.Text).ToString("yyyy-MM-dd"));
                        AdapterProdutos[indexSelecionado].QTD_AVENCER = Convert.ToInt32(txtQtdVencimento.Text);
                        AdapterProdutos[indexSelecionado].DAT_VENCIMENTO = Convert.ToDateTime(txtDataVencimento.Text);
                        ProdutosVencimentoAdapter.UpdateAdapter(AdapterProdutos);
                    }

                    MessageBox.ShowShortMessageBottom(Activity, string.Format("Validade de produto {0}.", StatusAtual == Status.Novo ? "adicionada" : "editada"));
                    LimparCampos();
                }
            }
            catch (System.Exception)
            {
                MessageBox.Alert(Activity, "Verifique o formato da data.");
            }
        }

        private void LimparCampos()
        {
            indexSelecionado = -1;
            StatusAtual = Status.Novo;
            txtQtdVencimento.Text = string.Empty;
            txtDataVencimento.Text = string.Empty;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            lblCodigoProduto.Text = CSProdutos.Current.DESCRICAO_APELIDO_PRODUTO;
            lblProduto.Text = CSProdutos.Current.DSC_PRODUTO;

            if (!CSEmpresa.ColunaExiste("PDV_PRODUTO_VALIDADE", "COD_PDV"))
            {
                btnOK.Enabled = false;
                MessageBox.Alert(Activity, "Tabela de coleta de validade não existente.");
            }
        }

        private void FindViewsById(View view)
        {
            lblCodigoProduto = view.FindViewById<TextView>(Resource.Id.lblCodigoProduto);
            lblProduto = view.FindViewById<TextView>(Resource.Id.lblProduto);
            txtQtdVencimento = view.FindViewById<EditText>(Resource.Id.txtQtdVencimento);
            txtDataVencimento = view.FindViewById<CustomDatePicker>(Resource.Id.txtDataVencimento);
            btnOK = view.FindViewById<Button>(Resource.Id.btnOK);
            btnCancelar = view.FindViewById<Button>(Resource.Id.btnCancelar);
            listProdutos = view.FindViewById<ListView>(Resource.Id.listProdutos);
        }

        private class ProdutosVencimentoListViewBaseAdapter : BaseAdapter
        {
            private Context context;
            private List<CSProdutos.CSProdutoVencimento> Produtos = null;

            public ProdutosVencimentoListViewBaseAdapter(Context context)
            {
                this.context = context;
            }

            public void UpdateAdapter(List<CSProdutos.CSProdutoVencimento> produtos)
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
                TextView lblQuantidade;
                TextView lblDataVencimento;
                CSProdutos.CSProdutoVencimento produtoAtual = this.Produtos[position];

                if (convertView == null)
                {
                    convertView = LayoutInflater.From(context)
                      .Inflate(Resource.Layout.produto_vencimento_row, parent, false);

                    lblQuantidade = convertView.FindViewById<TextView>(Resource.Id.lblQuantidade);
                    lblDataVencimento = convertView.FindViewById<TextView>(Resource.Id.lblDataVencimento);

                    convertView.SetTag(lblQuantidade.Id, lblQuantidade);
                    convertView.SetTag(lblDataVencimento.Id, lblDataVencimento);
                }
                else
                {
                    lblQuantidade = (TextView)convertView.GetTag(Resource.Id.lblQuantidade);
                    lblDataVencimento = (TextView)convertView.GetTag(Resource.Id.lblDataVencimento);
                }

                lblQuantidade.Text = produtoAtual.QTD_AVENCER.ToString();
                lblDataVencimento.Text = produtoAtual.DAT_VENCIMENTO.ToString("dd/MM/yyyy");

                return convertView;
            }
        }
    }
}