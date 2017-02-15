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
using AvanteSales.Pro.Formatters;
using AvanteSales.SystemFramework;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;

namespace AvanteSales.Pro.Fragments
{
    public class ListaProdutosRamoNaoVendidos : Android.Support.V4.App.Fragment
    {
        private static bool ListarApelido;
        public ListView lvwMotivos { get; set; }
        private Spinner cmbGrupoComercializacao;
        private Spinner cmbGrupoProduto;
        private Spinner cmbFamiliaProduto;
        private ListView lvwProdutos;
        private const int frmProdutoPedido = 1;
        private const int frmMotivoNaoPositivado = 2;
        private bool m_executarMontaProdutos = false;
        //private static Activity CurrentActivity;
        //private static ProgressDialog progressDialog;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.lista_produtos_ramo_nao_vendidos, container, false);

            FindViewsByIds(view);
            Eventos();

            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            Load();
        }

        private void Eventos()
        {
            cmbGrupoProduto.ItemSelected += CmbGrupoProduto_ItemSelected;
            cmbGrupoComercializacao.ItemSelected += CmbGrupoComercializacao_ItemSelected;
            cmbFamiliaProduto.ItemSelected += CmbFamiliaProduto_ItemSelected;
            lvwProdutos.ItemClick += LvwProdutos_ItemClick;

            ListarApelido = false;
        }

        private void LvwProdutos_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (!CSGlobal.PedidoComCombo)
            {
                CSProdutos.CSProduto prod = CSProdutos.GetProduto(Convert.ToInt32(((CSListViewItem)lvwProdutos.Adapter.GetItem(e.Position)).Valor));
                if (prod.COD_PRODUTO != 0)
                {
                    CSProdutos.Current = prod;

                    decimal prcadf = 0;
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current = new CSItemsPedido.CSItemPedido();
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_ADICIONAL_FINANCEIRO = prcadf;

                    if (CSProdutos.Current.PRECOS_PRODUTO == null || CSProdutos.Current.PRECOS_PRODUTO.Count == 0)
                    {
                        if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 1)
                            MessageBox.ShowShortMessageCenter(Activity, "Preço do produto não cadastrado.\r\nNão é possivel realizar esta venda.");
                        else
                            MessageBox.ShowShortMessageCenter(Activity, "Cliente ou Produto com informações incompletas no cadastro Nestlê!\nNão é possivel realizar esta venda.");

                        return;
                    }

                    //CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRODUTO = CSProdutos.Current;

                    //bool novoItem = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.NOVO;

                    //Intent i = new Intent();
                    //i.SetClass(this, typeof(ProdutoPedido));
                    //this.StartActivityForResult(i, frmProdutoPedido);
                }
            }
        }

        private void CmbFamiliaProduto_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (m_executarMontaProdutos)
            {
                CarregaListViewItemPedido();
            }
        }

        private void CmbGrupoComercializacao_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (m_executarMontaProdutos)
            {
                CarregaListViewItemPedido();
            }
        }

        private void CmbGrupoProduto_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            CarregarComboBoxFamiliaProduto();

            if (m_executarMontaProdutos)
            {
                CarregaListViewItemPedido();
            }
        }

        private void FindViewsByIds(View view)
        {
            cmbGrupoComercializacao = view.FindViewById<Spinner>(Resource.Id.cmbGrupoComercializacao);
            cmbGrupoProduto = view.FindViewById<Spinner>(Resource.Id.cmbGrupoProduto);
            cmbFamiliaProduto = view.FindViewById<Spinner>(Resource.Id.cmbFamiliaProduto);
            lvwProdutos = view.FindViewById<ListView>(Resource.Id.lvwProdutos);
        }

        private void Load()
        {
            if (CSPDVs.Current.PEDIDOS_PDV.Current != null)
            {
                m_executarMontaProdutos = false;

                CarregaComboBoxGrupoComercializacao();
                CarregaComboBoxGrupoProduto();
                CarregarComboBoxFamiliaProduto();

                m_executarMontaProdutos = true;
                // Mostra os items do pedido
                CarregaListViewItemPedido();
            }
        }

        private void CarregarComboBoxFamiliaProduto()
        {
            // Limpa o combo de familias
            cmbFamiliaProduto.Adapter = null;

            var adapter = cmbFamiliaProduto.SetDefaultAdapter();

            // Busca o grupo selecionado
            CSGruposProduto.CSGrupoProduto grupo = (CSGruposProduto.CSGrupoProduto)((CSItemCombo)cmbGrupoProduto.SelectedItem).Valor;

            // Preenche o combo de familias a partir do grupo selecionado
            foreach (CSFamiliasProduto.CSFamiliaProduto fam in CSFamiliasProduto.Items)
            {
                if (fam.GRUPO.COD_GRUPO == grupo.COD_GRUPO)
                {
                    CSItemCombo ic = new CSItemCombo();
                    ic.Texto = fam.DSC_FAMILIA_PRODUTO;
                    ic.Valor = fam;
                    adapter.Add(ic);
                }
            }

            // Adiciona um opção para selecionar todos as produtos da familia
            CSFamiliasProduto.CSFamiliaProduto famtodos = new CSFamiliasProduto.CSFamiliaProduto();
            famtodos.GRUPO = grupo;
            famtodos.COD_FAMILIA_PRODUTO = -1;
            famtodos.DSC_FAMILIA_PRODUTO = "==== TODOS ====";

            CSItemCombo ictodos = new CSItemCombo();
            ictodos.Texto = famtodos.DSC_FAMILIA_PRODUTO;
            ictodos.Valor = famtodos;
            adapter.Add(ictodos);

            // Coloca como default a opção todos os produtos da familia.
            if (cmbFamiliaProduto.Adapter != null)
                cmbFamiliaProduto.SetSelection(cmbFamiliaProduto.Adapter.Count - 1);
        }

        private void CarregaListViewItemPedido()
        {
            StringBuilder sqlQuery = null;
            SQLiteParameter pCOD_PDV = null;
            SQLiteParameter pCOD_GRUPO1 = null;
            SQLiteParameter pCOD_GRUPO2 = null;
            SQLiteParameter pCOD_GRUPO3 = null;
            SQLiteParameter pCOD_FAMILIA_PRODUTO1 = null;
            SQLiteParameter pCOD_FAMILIA_PRODUTO2 = null;
            SQLiteParameter pCOD_FAMILIA_PRODUTO3 = null;
            SQLiteParameter pCOD_GRUPO_COMERCIALIZACAO1 = null;
            SQLiteParameter pCOD_GRUPO_COMERCIALIZACAO2 = null;
            SQLiteParameter pCOD_GRUPO_COMERCIALIZACAO3 = null;

            string sqlProduto;
            string _ProdutosPedido = "";
            int _familia = -1;
            int _grupoProduto = -1;
            int _grupoComercializacao = -1;

            /* Seleciona os produtos já */
            _ProdutosPedido = "";

            if (CSPDVs.Current.PEDIDOS_PDV.Current == null)
                return;

            foreach (CSItemsPedido.CSItemPedido produto in CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Items.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.STATE != ObjectState.DELETADO))
            {
                if (!CSEmpresa.Current.IND_CLUSTER_PRODUTOS_PELO_TIPO_FREEZER)
                    _ProdutosPedido += produto.PRODUTO.COD_PRODUTO.ToString() + ",";
                else
                {
                    if (produto.STATE != ObjectState.DELETADO)
                        _ProdutosPedido += produto.PRODUTO.COD_PRODUTO.ToString() + ",";
                }
            }

            // Limpa o listview
            lvwProdutos.Adapter = null;

            sqlQuery = new StringBuilder();

            sqlQuery.Length = 0;
            sqlQuery.Append("   SELECT T1.DESCRICAO_APELIDO_PRODUTO, " + (ListarApelido ? "T1.DSC_APELIDO_PRODUTO," : "T1.DSC_PRODUTO,"));
            sqlQuery.Append("          T4.COD_GRUPO, T4.DSC_GRUPO,");
            sqlQuery.Append("          T5.COD_FAMILIA_PRODUTO, T5.DSC_FAMILIA_PRODUTO,");
            sqlQuery.Append("          T1.COD_PRODUTO");
            sqlQuery.Append("     FROM PRODUTO T1 ");
            sqlQuery.Append("          INNER JOIN PRODUTO_CATEGORIA T2");
            sqlQuery.Append("             ON T1.COD_PRODUTO = T2.COD_PRODUTO");
            sqlQuery.Append("          INNER JOIN PDV T3");

            if (!CSEmpresa.Current.IND_CLUSTER_PRODUTOS_PELO_TIPO_FREEZER)
                sqlQuery.Append("             ON T3.COD_CATEGORIA = T2.COD_CATEGORIA");
            else
                sqlQuery.Append("             ON T3.COD_DENVER = T2.COD_CATEGORIA");

            sqlQuery.Append("          INNER JOIN GRUPO_PRODUTO T4");
            sqlQuery.Append("             ON T4.COD_GRUPO = T1.COD_GRUPO");
            sqlQuery.Append("          INNER JOIN FAMILIA_PRODUTO T5");
            sqlQuery.Append("             ON T5.COD_GRUPO = T1.COD_GRUPO AND ");
            sqlQuery.Append("                T5.COD_FAMILIA_PRODUTO = T1.COD_FAMILIA_PRODUTO");
            sqlQuery.Append("    WHERE T1.IND_ATIVO = 'A' ");
            sqlQuery.Append("      AND T3.COD_PDV = ? ");
            sqlQuery.Append("      AND ((T4.COD_GRUPO = ? AND ? <> -1) OR ? = -1) ");
            sqlQuery.Append("      AND ((T5.COD_FAMILIA_PRODUTO = ? AND ? <> -1) OR ? = -1) ");
            sqlQuery.Append("      AND ((T1.COD_GRUPO_COMERCIALIZACAO = ? AND ? <> -1) OR ? = -1)");
            sqlQuery.Append(" ORDER BY T4.DSC_GRUPO, T5.DSC_FAMILIA_PRODUTO, T1.DSC_PRODUTO");

            sqlProduto = sqlQuery.ToString();

            if (cmbGrupoComercializacao.Adapter != null)
            {
                // Busca o grupo de comercializacao selecionado
                CSGruposComercializacao.CSGrupoComercializacao grupoComercializacao =
                    (CSGruposComercializacao.CSGrupoComercializacao)((CSItemCombo)cmbGrupoComercializacao.SelectedItem).Valor;
                _grupoComercializacao = grupoComercializacao.COD_GRUPO_COMERCIALIZACAO;
            }

            // Busca a familia selecionada
            if (cmbFamiliaProduto.Adapter != null)
            {
                CSFamiliasProduto.CSFamiliaProduto familia = (CSFamiliasProduto.CSFamiliaProduto)((CSItemCombo)cmbFamiliaProduto.SelectedItem).Valor;
                _familia = familia.COD_FAMILIA_PRODUTO;
            }

            // Busca o Grupo selecionado
            if (cmbGrupoProduto.Adapter != null)
            {
                CSGruposProduto.CSGrupoProduto grupoProduto = (CSGruposProduto.CSGrupoProduto)((CSItemCombo)cmbGrupoProduto.SelectedItem).Valor;
                _grupoProduto = grupoProduto.COD_GRUPO;
            }

            pCOD_PDV = new SQLiteParameter("@COD_PDV", CSPDVs.Current.COD_PDV);
            pCOD_GRUPO1 = new SQLiteParameter("@COD_GRUPO1", _grupoProduto);
            pCOD_GRUPO2 = new SQLiteParameter("@COD_GRUPO2", _grupoProduto);
            pCOD_GRUPO3 = new SQLiteParameter("@COD_GRUPO3", _grupoProduto);
            pCOD_FAMILIA_PRODUTO1 = new SQLiteParameter("@COD_FAMILIA_PRODUTO1", _familia);
            pCOD_FAMILIA_PRODUTO2 = new SQLiteParameter("@COD_FAMILIA_PRODUTO2", _familia);
            pCOD_FAMILIA_PRODUTO3 = new SQLiteParameter("@COD_FAMILIA_PRODUTO3", _familia);
            pCOD_GRUPO_COMERCIALIZACAO1 = new SQLiteParameter("@COD_GRUPO_COMERCIALIZACAO1", _grupoComercializacao);
            pCOD_GRUPO_COMERCIALIZACAO2 = new SQLiteParameter("@COD_GRUPO_COMERCIALIZACAO2", _grupoComercializacao);
            pCOD_GRUPO_COMERCIALIZACAO3 = new SQLiteParameter("@COD_GRUPO_COMERCIALIZACAO3", _grupoComercializacao);

            List<CSListViewItem> listLvi = new List<CSListViewItem>();

            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlProduto.ToString(), pCOD_PDV, pCOD_GRUPO1, pCOD_GRUPO2, pCOD_GRUPO3, pCOD_FAMILIA_PRODUTO1, pCOD_FAMILIA_PRODUTO2, pCOD_FAMILIA_PRODUTO3, pCOD_GRUPO_COMERCIALIZACAO1, pCOD_GRUPO_COMERCIALIZACAO2, pCOD_GRUPO_COMERCIALIZACAO3))
            {

                while (sqlReader.Read())
                {

                    if (_ProdutosPedido.IndexOf(sqlReader.GetInt32(6).ToString() + ",", 0) == -1)
                    {
                        CSListViewItem lvi = new CSListViewItem();
                        lvi.Text = sqlReader.GetString(3).Trim();
                        lvi.SubItems = new List<object>();
                        lvi.SubItems.Add(sqlReader.GetString(5).Trim());
                        lvi.SubItems.Add(sqlReader.GetString(1).Trim());
                        lvi.SubItems.Add(sqlReader.GetString(0).Trim());
                        lvi.Valor = sqlReader.GetInt32(6);
                        listLvi.Add(lvi);
                    }
                }

                lvwProdutos.Adapter = new ListProdutoRamoAtividade(Activity, Resource.Layout.lista_produtos_ramo_nao_vendidos_row, listLvi);
                sqlReader.Close();
                sqlReader.Dispose();
            }
        }

        private void CarregaComboBoxGrupoProduto()
        {
            // Limpa o combo de grupos
            cmbGrupoProduto.Adapter = null;

            var adapter = cmbGrupoProduto.SetDefaultAdapter();

            // Preenche o combo de grupos
            foreach (CSGruposProduto.CSGrupoProduto grp in CSGruposProduto.Items)
            {
                CSItemCombo ic = new CSItemCombo();
                ic.Texto = grp.DSC_GRUPO;
                ic.Valor = grp;
                adapter.Add(ic);
            }

            // Adiciona um opção para selecionar todos os grupos
            CSGruposProduto.CSGrupoProduto grptodos = new CSGruposProduto.CSGrupoProduto();
            grptodos.COD_GRUPO = -1;
            grptodos.DSC_GRUPO = "==== TODOS ====";

            CSItemCombo ictodos = new CSItemCombo();
            ictodos.Texto = grptodos.DSC_GRUPO;
            ictodos.Valor = grptodos;
            adapter.Add(ictodos);

            if (cmbGrupoProduto.Adapter != null)
                cmbGrupoProduto.SetSelection(cmbGrupoProduto.Adapter.Count - 1);

        }

        private void CarregaComboBoxGrupoComercializacao()
        {
            cmbGrupoComercializacao.Adapter = null;

            var adapter = cmbGrupoComercializacao.SetDefaultAdapter();

            // Preenche o combo
            foreach (CSGruposComercializacao.CSGrupoComercializacao grp in CSGruposComercializacao.Items)
            {
                CSItemCombo ic = new CSItemCombo();
                ic.Texto = grp.DES_GRUPO_COMERCIALIZACAO;
                ic.Valor = grp;
                adapter.Add(ic);
            }

            CSGruposComercializacao.CSGrupoComercializacao grptodos = new CSGruposComercializacao.CSGrupoComercializacao();
            grptodos.COD_GRUPO_COMERCIALIZACAO = -1;
            grptodos.DES_GRUPO_COMERCIALIZACAO = "==== TODOS ====";
            grptodos.COD_SETOR_BROKER = "";

            CSItemCombo ictodos = new CSItemCombo();
            ictodos.Texto = grptodos.DES_GRUPO_COMERCIALIZACAO;
            ictodos.Valor = grptodos;
            adapter.Add(ictodos);

            if (cmbGrupoComercializacao.Adapter != null)
                cmbGrupoComercializacao.SetSelection(cmbGrupoComercializacao.Adapter.Count - 1);
        }

        class ListProdutoRamoAtividade : ArrayAdapter<CSListViewItem>
        {
            Activity act;
            IList<CSListViewItem> produto;
            int resourceId;

            public ListProdutoRamoAtividade(Activity c, int textViewResourceId, IList<CSListViewItem> objects)
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
                    TextView tvCodigo = linha.FindViewById<TextView>(Resource.Id.tvCodigo);
                    TextView tvGrupo = linha.FindViewById<TextView>(Resource.Id.tvGrupo);
                    TextView tvFamilia = linha.FindViewById<TextView>(Resource.Id.tvFamilia);

                    tvGrupo.Text = item.Text.ToString();
                    tvFamilia.Text = item.SubItems.FirstOrDefault().ToString();
                    tvProduto.Text = item.SubItems[1].ToString();
                    tvCodigo.Text = item.SubItems[2].ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.AlertErro(act, ex.Message);
                }

                return linha;
            }
        }
    }
}