using System;
#if ANDROID
using Android.Graphics;
using Mono.Data.Sqlite;
#else
using System.Data.SQLite;
#endif

#if ANDROID
using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
using SQLiteCommand = Mono.Data.Sqlite.SqliteCommand;
using SQLiteDataAdapter = Mono.Data.Sqlite.SqliteDataAdapter;
using SQLiteException = Mono.Data.Sqlite.SqliteException;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteTransaction = Mono.Data.Sqlite.SqliteTransaction;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;
using System.Linq;
#endif
using System.Text;
using System.Collections;
using System.Data;
using System.Data.SqlTypes;
using AvanteSales.BusinessRules;
using AvanteSales.SystemFramework;
using System.Collections.Generic;
using AvanteSales.SystemFramework.CSPDV;

namespace AvanteSales
{
    public class CSProdutos : CollectionBase
    {
        #region [ Variaveis ]

        private static CSProdutos m_Items;
        private static CSProdutos.CSProduto m_Current;
        private static bool m_Exclusivista;
        private static SQLiteCommand sqlCommandCodigo;
        private static ComparaProdutos compara = new ComparaProdutos();

        #endregion

        #region [ Eventos ]

        public delegate void BeginGetProdutos(int TotalProdutos);
        public delegate void TickNewProduto();
        public delegate void EndGetProdutos();

        public static event BeginGetProdutos OnBeginProdutos;
        public static event TickNewProduto OnTickNewProduto;
        public static event EndGetProdutos OnEndProdutos;

        #endregion

        #region [ Propriedades ]

        /// <summary>
        /// Retorna coleção dos produtos
        /// </summary>
        public static CSProdutos Items
        {
            get
            {
                if (m_Items == null)
                {
                    m_Items = new CSProdutos();
                }
                return m_Items;
            }
        }

        public static List<CSProdutos.CSProdutoVencimento> ItemsVencimento(int linha, int grupo)
        {
            return GetProdutosVencimento(linha, grupo, null);
        }

        public static List<CSProdutos.CSProdutoVencimento> ItemsVencimentoColetaAtual(int linha, int grupo)
        {
            return GetProdutosVencimento(linha, grupo, DateTime.Now.ToString("yyyy-MM-dd"));
        }

        public static void RemoverRegistroVencimento(CSProdutos.CSProdutoVencimento produto)
        {
            if (CSEmpresa.ColunaExiste("PDV_PRODUTO_VALIDADE", "COD_PDV"))
            {
                StringBuilder sql = new StringBuilder();

                sql.AppendLine("DELETE FROM PDV_PRODUTO_VALIDADE WHERE ");
                sql.AppendFormat("     COD_PRODUTO = {0} ", produto.COD_PRODUTO);
                sql.AppendFormat(" AND DATE(DAT_COLETA) = DATE('{0}') ", DateTime.Now.ToString("yyyy-MM-dd"));
                sql.AppendFormat(" AND COD_PDV = {0} ", produto.COD_PDV);
                sql.AppendFormat(" AND QTD_AVENCER ", produto.QTD_AVENCER);
                sql.AppendFormat(" AND DATE(DAT_VENCIMENTO) = DATE('{0}') ", produto.DAT_VENCIMENTO.ToString("yyyy-MM-dd"));

                CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());
            }
        }

        public static void AlterarRegistroVencimento(CSProdutos.CSProdutoVencimento produto, int novaQtd, string novoVencimento)
        {
            if (CSEmpresa.ColunaExiste("PDV_PRODUTO_VALIDADE", "COD_PDV"))
            {
                StringBuilder sql = new StringBuilder();

                sql.AppendLine("UPDATE PDV_PRODUTO_VALIDADE ");
                sql.AppendFormat(" SET QTD_AVENCER = {0} ", novaQtd);
                sql.AppendFormat("    ,DAT_VENCIMENTO = DATE('{0}') ", novoVencimento);
                sql.AppendFormat("    ,DAT_COLETA = DATETIME('{0}') ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sql.AppendFormat(" WHERE COD_PRODUTO = {0} ", produto.COD_PRODUTO);
                sql.AppendFormat(" AND DATE(DAT_COLETA) = DATE('{0}') ", DateTime.Now.ToString("yyyy-MM-dd"));
                sql.AppendFormat(" AND DATE(DAT_VENCIMENTO) = DATE('{0}') ", produto.DAT_VENCIMENTO.ToString("yyyy-MM-dd"));
                sql.AppendFormat(" AND COD_PDV = {0} ", produto.COD_PDV);
                sql.AppendFormat(" AND QTD_AVENCER ", produto.QTD_AVENCER);

                CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());
            }
        }

        public static List<CSProdutos.CSProdutoVencimento> GetProdutoVencimentoDia(int codProduto)
        {
            List<CSProdutos.CSProdutoVencimento> produtos = new List<CSProdutos.CSProdutoVencimento>();

            if (CSEmpresa.ColunaExiste("PDV_PRODUTO_VALIDADE", "COD_PDV"))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT ");
                sql.AppendLine("        COD_PDV ");
                sql.AppendLine("       ,COD_EMPREGADO ");
                sql.AppendLine("       ,PRODUTO.COD_PRODUTO ");
                sql.AppendLine("	   ,PRODUTO.DSC_PRODUTO ");
                sql.AppendLine("       ,PRODUTO.DESCRICAO_APELIDO_PRODUTO");
                sql.AppendLine("	   ,DAT_COLETA ");
                sql.AppendLine("	   ,QTD_AVENCER ");
                sql.AppendLine("	   ,DAT_VENCIMENTO ");
                sql.AppendLine("FROM PDV_PRODUTO_VALIDADE ");
                sql.AppendLine("JOIN PRODUTO ");
                sql.AppendLine("        ON PDV_PRODUTO_VALIDADE.COD_PRODUTO = PRODUTO.COD_PRODUTO ");
                sql.AppendFormat("         AND PDV_PRODUTO_VALIDADE.COD_PDV = {0} ", CSPDVs.Current.COD_PDV);
                sql.AppendFormat("     WHERE DATE(PDV_PRODUTO_VALIDADE.DAT_COLETA) = DATE('{0}') ", DateTime.Now.ToString("yyyy-MM-dd"));
                sql.AppendFormat("      AND PRODUTO.COD_PRODUTO = {0} ", codProduto);

                CSProdutoVencimento pdd;

                using (SqliteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
                {
                    while (sqlReader.Read())
                    {
                        pdd = new CSProdutoVencimento();

                        pdd.COD_PDV = sqlReader.GetValue(0) == System.DBNull.Value ? 0 : sqlReader.GetInt32(0);
                        pdd.COD_EMPREGADO = sqlReader.GetValue(1) == System.DBNull.Value ? 0 : sqlReader.GetInt32(1);
                        pdd.COD_PRODUTO = sqlReader.GetValue(2) == System.DBNull.Value ? 0 : sqlReader.GetInt32(2);
                        pdd.DSC_PRODUTO = sqlReader.GetValue(3) == System.DBNull.Value ? string.Empty : sqlReader.GetString(3);
                        pdd.DESCRICAO_APELIDO_PRODUTO = sqlReader.GetValue(4) == System.DBNull.Value ? string.Empty : sqlReader.GetString(4);
                        pdd.DAT_COLETA = sqlReader.GetValue(5) == System.DBNull.Value ? new DateTime() : sqlReader.GetDateTime(5);
                        pdd.QTD_AVENCER = sqlReader.GetValue(6) == System.DBNull.Value ? 0 : sqlReader.GetDecimal(6);
                        pdd.DAT_VENCIMENTO = sqlReader.GetValue(7) == System.DBNull.Value ? new DateTime() : sqlReader.GetDateTime(7);

                        produtos.Add(pdd);
                    }
                }
            }

            return produtos;
        }

        private static List<CSProdutos.CSProdutoVencimento> GetProdutosVencimento(int linha, int grupo, string data)
        {
            List<CSProdutos.CSProdutoVencimento> produtos = new List<CSProdutos.CSProdutoVencimento>();

            if (CSEmpresa.ColunaExiste("PDV_PRODUTO_VALIDADE", "COD_PDV"))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT ");
                sql.AppendLine("        COD_PDV ");
                sql.AppendLine("       ,COD_EMPREGADO ");
                sql.AppendLine("       ,PRODUTO.COD_PRODUTO ");
                sql.AppendLine("	   ,PRODUTO.DSC_PRODUTO ");
                sql.AppendLine("       ,PRODUTO.DESCRICAO_APELIDO_PRODUTO");
                sql.AppendLine("	   ,DAT_COLETA ");
                sql.AppendLine("	   ,QTD_AVENCER ");
                sql.AppendLine("	   ,DAT_VENCIMENTO ");
                sql.AppendLine("FROM PDV_PRODUTO_VALIDADE ");
                sql.AppendLine("JOIN PRODUTO ");
                sql.AppendLine("        ON PDV_PRODUTO_VALIDADE.COD_PRODUTO = PRODUTO.COD_PRODUTO ");
                sql.AppendFormat("         AND PRODUTO.COD_GRUPO_COMERCIALIZACAO = {0} ", linha);
                sql.AppendFormat("         AND PRODUTO.COD_GRUPO = {0} ", grupo);
                sql.AppendFormat("         AND PDV_PRODUTO_VALIDADE.COD_PDV = {0} ", CSPDVs.Current.COD_PDV);

                if (!string.IsNullOrEmpty(data))
                {
                    sql.AppendFormat("     WHERE DATE(PDV_PRODUTO_VALIDADE.DAT_COLETA) = DATE('{0}') ", data);
                }

                CSProdutoVencimento pdd;

                using (SqliteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
                {
                    while (sqlReader.Read())
                    {
                        pdd = new CSProdutoVencimento();

                        pdd.COD_PDV = sqlReader.GetValue(0) == System.DBNull.Value ? 0 : sqlReader.GetInt32(0);
                        pdd.COD_EMPREGADO = sqlReader.GetValue(1) == System.DBNull.Value ? 0 : sqlReader.GetInt32(1);
                        pdd.COD_PRODUTO = sqlReader.GetValue(2) == System.DBNull.Value ? 0 : sqlReader.GetInt32(2);
                        pdd.DSC_PRODUTO = sqlReader.GetValue(3) == System.DBNull.Value ? string.Empty : sqlReader.GetString(3);
                        pdd.DESCRICAO_APELIDO_PRODUTO = sqlReader.GetValue(4) == System.DBNull.Value ? string.Empty : sqlReader.GetString(4);
                        pdd.DAT_COLETA = sqlReader.GetValue(5) == System.DBNull.Value ? new DateTime() : sqlReader.GetDateTime(5);
                        pdd.QTD_AVENCER = sqlReader.GetValue(6) == System.DBNull.Value ? 0 : sqlReader.GetDecimal(6);
                        pdd.DAT_VENCIMENTO = sqlReader.GetValue(7) == System.DBNull.Value ? new DateTime() : sqlReader.GetDateTime(7);

                        produtos.Add(pdd);
                    }
                }
            }


            return produtos;
        }

        public CSProdutos.CSProduto this[int Index]
        {
            get
            {
                return (CSProdutos.CSProduto)this.InnerList[Index];
            }
        }

#if ANDROID

        public static IList<CSProdutos.CSProduto> OrdenarListaProdutos(IList<CSProdutos.CSProduto> itens)
        {
            int i;

            var ListaItens = itens.Cast<CSProdutos.CSProduto>().Where(p => int.TryParse(p.DESCRICAO_APELIDO_PRODUTO, out i)).OrderBy(b => Convert.ToInt32(b.DESCRICAO_APELIDO_PRODUTO)).ToList();
            ListaItens.AddRange(itens.Cast<CSProdutos.CSProduto>().Where(p => !int.TryParse(p.DESCRICAO_APELIDO_PRODUTO, out i)).OrderBy(b => b.DESCRICAO_APELIDO_PRODUTO).ToList());

            return ListaItens;
        }

        public static IList<CSItemsPedido.CSItemPedido> OrdenarListaProdutosPedido(IList<CSItemsPedido.CSItemPedido> itens)
        {
            int i;

            var ListaItens = itens.Cast<CSItemsPedido.CSItemPedido>().Where(p => int.TryParse(p.PRODUTO.DESCRICAO_APELIDO_PRODUTO, out i)).OrderBy(b => Convert.ToInt32(b.PRODUTO.DESCRICAO_APELIDO_PRODUTO)).ToList();
            ListaItens.AddRange(itens.Cast<CSItemsPedido.CSItemPedido>().Where(p => !int.TryParse(p.PRODUTO.DESCRICAO_APELIDO_PRODUTO, out i)).OrderBy(b => b.PRODUTO.DESCRICAO_APELIDO_PRODUTO).ToList());

            return ListaItens;
        }

        public static IList<CSItemsIndenizacao.CSItemIndenizacao> OrdenarListaProdutosIndenizacao(IList<CSItemsIndenizacao.CSItemIndenizacao> itens)
        {
            int i;

            var ListaItens = itens.Cast<CSItemsIndenizacao.CSItemIndenizacao>().Where(p => int.TryParse(p.PRODUTO.DESCRICAO_APELIDO_PRODUTO, out i)).OrderBy(b => Convert.ToInt32(b.PRODUTO.DESCRICAO_APELIDO_PRODUTO)).ToList();
            ListaItens.AddRange(itens.Cast<CSItemsIndenizacao.CSItemIndenizacao>().Where(p => !int.TryParse(p.PRODUTO.DESCRICAO_APELIDO_PRODUTO, out i)).OrderBy(b => b.PRODUTO.DESCRICAO_APELIDO_PRODUTO).ToList());

            return ListaItens;
        }

        public static IList<CSItemsHistoricoIndenizacao.CSItemHistoricoIndenizacao> OrdenarListaProdutosHistoricoIndenizacao(IList<CSItemsHistoricoIndenizacao.CSItemHistoricoIndenizacao> itens)
        {
            int i;

            var ListaItens = itens.Cast<CSItemsHistoricoIndenizacao.CSItemHistoricoIndenizacao>().Where(p => int.TryParse(p.PRODUTO.DESCRICAO_APELIDO_PRODUTO, out i)).OrderBy(b => Convert.ToInt32(b.PRODUTO.DESCRICAO_APELIDO_PRODUTO)).ToList();
            ListaItens.AddRange(itens.Cast<CSItemsHistoricoIndenizacao.CSItemHistoricoIndenizacao>().Where(p => !int.TryParse(p.PRODUTO.DESCRICAO_APELIDO_PRODUTO, out i)).OrderBy(b => b.PRODUTO.DESCRICAO_APELIDO_PRODUTO).ToList());

            return ListaItens;
        }
#endif
        public static bool Existe_Produto_Categoria_Exclusiva
        {
            get
            {
                return m_Exclusivista;
            }
        }

        public static CSProdutos.CSProduto Current
        {
            get
            {
                // Se nao tiver item corrente configurado
                // entao pega o primeiro este codigo colocado
                // somente para chamar o metodo ConverteUnidadesParaMedida da 
                // classse produto pois este nao e estatico
                if (m_Current == null)
                    m_Current = Items[0];

                return m_Current;
            }
            set
            {
                m_Current = value;
            }
        }

        /// <summary>
        /// Busca o produto pelo codigo
        /// </summary>
        /// <param name="COD_OPERACAO">Codigo do produto a ser procurado</param>
        /// <returns>Retorna o objeto do produto</returns>
        public static CSProduto GetProduto(int COD_PRODUTO)
        {
            try
            {
                CSProduto ret = new CSProduto();

                // Procura pelo produto
                foreach (CSProduto prod in Items.InnerList)
                {
                    if (prod.COD_PRODUTO == COD_PRODUTO)
                    {
                        ret = prod;
                        break;
                    }
                }

                // retorna o objeto do produto
                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static CSProduto GetProdutoInativo(int COD_PRODUTO)
        {
            SQLiteParameter pCOD_PRODUTO = null;
            SQLiteParameter pCOD_EMPREGADO = null;
            CSProduto prod = null;

            try
            {
                string sqlQuery =
                    "SELECT P.COD_PRODUTO, P.COD_GRUPO, P.COD_TIPO_DISTRIBUICAO_POLITICA " +
                    "      ,P.COD_FAMILIA_PRODUTO, P.COD_UNIDADE_MEDIDA, P.DSC_PRODUTO, P.DSC_APELIDO_PRODUTO " +
                    "      ,P.QTD_UNIDADE_EMBALAGEM, P.VLR_MINIMO_PEDIDO, P.PRC_MAXIMO_DESCONTO " +
                    "      ,P.PRC_ACRESCIMO_QTDE_UNITARIA, P.COD_GRUPO_COMERCIALIZACAO, P.IND_ATIVO " +
                    "      ,P.VLR_PESO_PRODUTO, P.QTD_ESTOQUE, P.DESCRICAO_APELIDO_PRODUTO,P.COD_SUBFAMILIA_PRODUTO " +
                    "      ,P.COD_LINHA_SUBFAMILIA_PRODUTO, P.PCT_MINIMO_LUCRATIVIDADE, P.VLR_CUSTO_GERENCIAL " +
                    "      ,P.IND_PRODUTO_COM_PRECO, P.IND_ITEM_CONJUNTO, P.IND_ITEM_COMBO, C.COD_PRODUTO_CONJUNTO " +
                    "      ,C.QTD_PRODUTO_COMPOSICAO, C.PCT_DESCONTO_PRODUTO_COMPOSICAO, P.QTD_UNIDADE_MEDIDA " +
                    "      ,P.DAT_VALIDADE_INICIO_COMBO, P.DAT_VALIDADE_TERMINO_COMBO, P.IND_STATUS_COMBO " +
                    "      ,P.QTD_MAXIMA_COMBO_PEDIDO, C.COD_TABELA_PRECO ,D.QTD_DISPONIVEL " +
                    "  FROM PRODUTO P LEFT JOIN PRODUTO_CONJUNTO C " +
                    "                   ON P.COD_PRODUTO = C.COD_PRODUTO_COMPOSICAO" +
                    "                 LEFT JOIN SALDO_PRONTA_ENTREGA_ITEM D  " +
                    "                   ON P.COD_PRODUTO = D.COD_PRODUTO " +
                    "                  AND D.COD_EMPREGADO = ? " +
                    " WHERE P.COD_PRODUTO = ? ";

                pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", COD_PRODUTO);
                pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                pCOD_EMPREGADO.DbType = DbType.Int32;

                prod = new CSProduto();

                // Busca todos os grupos
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_EMPREGADO, pCOD_PRODUTO))
                {
                    if (sqlReader.Read())
                    {
                        // Preenche a instancia da classe dos produtos
                        prod.COD_PRODUTO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        prod.GRUPO = CSGruposProduto.GetGrupoProduto(sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1));
                        prod.COD_TIPO_DISTRIBUICAO_POLITICA = sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2);
                        prod.FAMILIA_PRODUTO = CSFamiliasProduto.GetFamiliaProduto(sqlReader.GetValue(3) == System.DBNull.Value ? -1 : sqlReader.GetInt32(3));
                        prod.COD_UNIDADE_MEDIDA = sqlReader.GetValue(4) == System.DBNull.Value ? "" : sqlReader.GetString(4).ToUpper();
                        prod.DSC_PRODUTO = sqlReader.GetValue(5) == System.DBNull.Value ? "" : sqlReader.GetString(5);
                        prod.DSC_APELIDO_PRODUTO = sqlReader.GetValue(6) == System.DBNull.Value ? "" : sqlReader.GetString(6);
                        prod.QTD_UNIDADE_EMBALAGEM = sqlReader.GetValue(7) == System.DBNull.Value ? -1 : sqlReader.GetInt32(7);
                        prod.VLR_MINIMO_PEDIDO = sqlReader.GetValue(8) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(8));
                        prod.PRC_MAXIMO_DESCONTO = sqlReader.GetValue(9) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(9));
                        prod.PRC_ACRESCIMO_QTDE_UNITARIA = sqlReader.GetValue(10) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(10));
                        prod.GRUPO_COMERCIALIZACAO = CSGruposComercializacao.GetGrupoComercializacao(sqlReader.GetValue(11) == System.DBNull.Value ? -1 : sqlReader.GetInt32(11));
                        prod.VLR_PESO_PRODUTO = sqlReader.GetValue(13) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(13));
                        prod.QTD_ESTOQUE = sqlReader.GetValue(14) == System.DBNull.Value ? 0 : decimal.Parse(sqlReader.GetValue(14).ToString());
                        prod.DESCRICAO_APELIDO_PRODUTO = sqlReader.GetValue(15) == System.DBNull.Value ? "" : sqlReader.GetString(15);
                        prod.COD_SUBFAMILIA_PRODUTO = sqlReader.GetValue(16) == System.DBNull.Value ? -1 : sqlReader.GetInt32(16);
                        prod.COD_LINHA_SUBFAMILIA_PRODUTO = sqlReader.GetValue(17) == System.DBNull.Value ? -1 : sqlReader.GetInt32(17);
                        prod.PCT_MINIMO_LUCRATIVIDADE = sqlReader.GetValue(18) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(18));
                        prod.VLR_CUSTO_GERENCIAL = sqlReader.GetValue(19) == System.DBNull.Value ? 0 : decimal.Parse(sqlReader.GetValue(19).ToString());
                        prod.IND_PRODUTO_COM_PRECO = sqlReader.GetValue(20) == System.DBNull.Value ? false : sqlReader.GetBoolean(20);
                        prod.IND_ITEM_CONJUNTO = sqlReader.GetBoolean(21);
                        prod.IND_ITEM_COMBO = sqlReader.GetBoolean(22);
                        prod.COD_PRODUTO_CONJUNTO = sqlReader.GetValue(23) == System.DBNull.Value ? -1 : sqlReader.GetInt32(23);
                        prod.QTD_PRODUTO_COMPOSICAO = sqlReader.GetValue(24) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(24));
                        prod.PCT_DESCONTO_PRODUTO_COMPOSICAO = sqlReader.GetValue(25) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(25));
                        prod.QTD_UNIDADE_MEDIDA = sqlReader.GetValue(26) == System.DBNull.Value ? -1 : sqlReader.GetInt32(26);
                        prod.DAT_VALIDADE_INICIO_COMBO = sqlReader.GetValue(27) == System.DBNull.Value ? new DateTime(1900, 1, 1) : sqlReader.GetDateTime(27);
                        prod.DAT_VALIDADE_TERMINO_COMBO = sqlReader.GetValue(28) == System.DBNull.Value ? new DateTime(1900, 1, 1) : sqlReader.GetDateTime(28);
                        prod.IND_STATUS_COMBO = sqlReader.GetValue(29) == System.DBNull.Value ? "" : sqlReader.GetString(29);
                        prod.QTD_MAXIMA_COMBO_PEDIDO = sqlReader.GetValue(30) == System.DBNull.Value ? -1 : sqlReader.GetInt32(30);
                        prod.COD_TABELA_PRECO_COMBO = sqlReader.GetValue(31) == System.DBNull.Value ? -1 : sqlReader.GetInt32(31);
                        prod.QTD_ESTOQUE_PRONTA_ENTREGA = sqlReader.GetValue(32) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(32));

                    }

                    else
                    {
                        prod.DESCRICAO_APELIDO_PRODUTO = COD_PRODUTO.ToString();
                        prod.DSC_APELIDO_PRODUTO = "<produto não encontrado>";
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }

                return prod;

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos produtos.", ex);
            }
        }

        #region [ Classe de Comparacao implementar IComparer ]
        private class ComparaProdutos : System.Collections.IComparer
        {
            int System.Collections.IComparer.Compare(object x, object y)
            {
                return ((CSProduto)x).COD_PRODUTO.CompareTo((int)y);
            }
        }

        #endregion

        #endregion

        #region [ Metodos ]

        /// <summary>
        /// Contrutor da classe. Busca os produtos
        /// </summary>
        public CSProdutos()
        {
            SQLiteParameter pCOD_PRODUTO = null;
            SQLiteParameter pCOD_EMPREGADO = null;
            StringBuilder sqlQuery = new StringBuilder();

            string sqlDelete = "DELETE FROM TMPITENS ";
            string sqlInsert = "INSERT INTO TMPITENS(COD_PRODUTO) VALUES(?) ";
            object result = null;

            try
            {
                CSDataAccess.Instance.ExecuteNonQuery(sqlDelete);

                sqlQuery.Append("SELECT COUNT(*) FROM PRODUTO WHERE IND_ATIVO='A' OR IND_ITEM_COMBO = 1 ");

                result = CSDataAccess.Instance.ExecuteScalar(sqlQuery.ToString());

                if (result != null)
                {
                    // Dispara o evento passando o numero de produtos a serem processados
                    OnBeginProdutos(int.Parse(result.ToString()));
                }

                pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                pCOD_EMPREGADO.DbType = DbType.Int32;

                bool EXISTE_NUM_ORDEM_ESTOQUE = CSEmpresa.ColunaExiste("PRODUTO", "NUM_ORDEM_ESTOQUE");

                sqlQuery.Length = 0;
                sqlQuery.Append("  SELECT P.COD_PRODUTO, P.COD_GRUPO, P.COD_TIPO_DISTRIBUICAO_POLITICA ");
                sqlQuery.Append("        ,P.COD_FAMILIA_PRODUTO, P.COD_UNIDADE_MEDIDA, P.DSC_PRODUTO, P.DSC_APELIDO_PRODUTO ");
                sqlQuery.Append("        ,P.QTD_UNIDADE_EMBALAGEM, P.VLR_MINIMO_PEDIDO, P.PRC_MAXIMO_DESCONTO ");
                sqlQuery.Append("        ,P.PRC_ACRESCIMO_QTDE_UNITARIA, P.COD_GRUPO_COMERCIALIZACAO, P.IND_ATIVO ");
                sqlQuery.Append("        ,P.VLR_PESO_PRODUTO, P.QTD_ESTOQUE, P.DESCRICAO_APELIDO_PRODUTO,P.COD_SUBFAMILIA_PRODUTO ");
                sqlQuery.Append("        ,P.COD_LINHA_SUBFAMILIA_PRODUTO, P.PCT_MINIMO_LUCRATIVIDADE, P.VLR_CUSTO_GERENCIAL ");
                sqlQuery.Append("        ,P.IND_PRODUTO_COM_PRECO, P.IND_ITEM_CONJUNTO, P.IND_ITEM_COMBO, C.COD_PRODUTO_CONJUNTO ");
                sqlQuery.Append("        ,C.QTD_PRODUTO_COMPOSICAO, C.PCT_DESCONTO_PRODUTO_COMPOSICAO, P.QTD_UNIDADE_MEDIDA ");
                sqlQuery.Append("        ,P.DAT_VALIDADE_INICIO_COMBO, P.DAT_VALIDADE_TERMINO_COMBO, P.IND_STATUS_COMBO ");
                sqlQuery.Append("        ,P.QTD_MAXIMA_COMBO_PEDIDO, C.COD_TABELA_PRECO ,D.QTD_DISPONIVEL, P.IND_PRODUTO_FOCO, BLQ.[COD_BLOQUEIO] ");

                if (EXISTE_NUM_ORDEM_ESTOQUE)
                    sqlQuery.Append(", P.NUM_ORDEM_ESTOQUE ");

                if (CSEmpresa.ColunaExiste("PRODUTO", "PCT_TAXA_MAX_INDENIZACAO"))
                    sqlQuery.Append(", P.PCT_TAXA_MAX_INDENIZACAO ");

                if (CSEmpresa.ColunaExiste("PRODUTO", "IND_LIBERAR_CONDICAO_PAGAMENTO"))
                    sqlQuery.Append(" , P.IND_LIBERAR_CONDICAO_PAGAMENTO ");

                if (CSEmpresa.ColunaExiste("PRODUTO", "DSC_INFO_CALORICA"))
                    sqlQuery.Append(" , P.DSC_INFO_CALORICA");
                else
                    sqlQuery.Append(" , ' ' AS CALORICA");

                if (CSEmpresa.ColunaExiste("PRODUTO", "DSC_INFO_NUTRICIONAL"))
                    sqlQuery.Append(" , P.DSC_INFO_NUTRICIONAL");
                else
                    sqlQuery.Append(" , ' ' AS NUTRICIONAL");

                if (CSEmpresa.ColunaExiste("PRODUTO", "DSC_INFO_OUTRAS"))
                    sqlQuery.Append(" , P.DSC_INFO_OUTRAS");
                else
                    sqlQuery.Append(" , ' ' AS OUTROS");

                if (CSEmpresa.ColunaExiste("PRODUTO", "COD_FABRICA_PRODUTO"))
                    sqlQuery.Append(" ,P.COD_FABRICA_PRODUTO");
                else
                    sqlQuery.Append(" ,P.COD_PRODUTO");

                sqlQuery.Append("    FROM PRODUTO P LEFT JOIN PRODUTO_CONJUNTO C ");
                sqlQuery.Append("                     ON P.COD_PRODUTO = C.COD_PRODUTO_COMPOSICAO ");
                sqlQuery.Append("                   LEFT JOIN SALDO_PRONTA_ENTREGA_ITEM D  ");
                sqlQuery.Append("                     ON P.COD_PRODUTO = D.COD_PRODUTO ");
                sqlQuery.Append("                    AND D.COD_EMPREGADO = ? ");
                //"                   LEFT JOIN PRODUTO_CATEGORIA PC " +
                //"                     ON PC.COD_PRODUTO = P.COD_PRODUTO AND PC.COD_CATEGORIA = 2" +
                sqlQuery.Append(" LEFT JOIN BLOQUEIO_PRODUTO_TABELA_PRECO BLQ ");
                sqlQuery.Append(" ON BLQ.COD_PRODUTO = P.COD_PRODUTO ");
                sqlQuery.Append(" AND BLQ.TIPO_BLOQUEIO = 'B' ");
                sqlQuery.AppendFormat(" AND BLQ.COD_BLOQUEIO = {0} ", CSEmpregados.Current.COD_EMPREGADO);
                sqlQuery.Append("    WHERE (P.IND_ATIVO = 'A' OR P.IND_ITEM_COMBO = 1) AND P.COD_PRODUTO <> 99999 ");
                //" ORDER BY P.COD_PRODUTO ";
                sqlQuery.Append(" GROUP BY P.COD_PRODUTO,C.COD_PRODUTO_CONJUNTO ");
                sqlQuery.Append(" ORDER BY P.DESCRICAO_APELIDO_PRODUTO");

                // Busca todos os grupos
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString(), pCOD_EMPREGADO))
                {
                    while (sqlReader.Read())
                    {
                        CSProduto prod = new CSProduto();

                        // Preenche a instancia da classe dos produtos
                        prod.COD_PRODUTO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        prod.GRUPO = CSGruposProduto.GetGrupoProduto(sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1));
                        prod.COD_TIPO_DISTRIBUICAO_POLITICA = sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2);
                        prod.FAMILIA_PRODUTO = CSFamiliasProduto.GetFamiliaProduto(sqlReader.GetValue(3) == System.DBNull.Value ? -1 : sqlReader.GetInt32(3));
                        prod.COD_UNIDADE_MEDIDA = sqlReader.GetValue(4) == System.DBNull.Value ? "" : sqlReader.GetString(4).ToUpper();
                        prod.DSC_PRODUTO = sqlReader.GetValue(5) == System.DBNull.Value ? "" : sqlReader.GetString(5);
                        prod.DSC_APELIDO_PRODUTO = sqlReader.GetValue(6) == System.DBNull.Value ? "" : sqlReader.GetString(6);
                        prod.QTD_UNIDADE_EMBALAGEM = sqlReader.GetValue(7) == System.DBNull.Value ? -1 : sqlReader.GetInt32(7);
                        prod.VLR_MINIMO_PEDIDO = sqlReader.GetValue(8) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(8));
                        prod.PRC_MAXIMO_DESCONTO = sqlReader.GetValue(9) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(9));
                        prod.PRC_ACRESCIMO_QTDE_UNITARIA = sqlReader.GetValue(10) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(10));
                        prod.GRUPO_COMERCIALIZACAO = CSGruposComercializacao.GetGrupoComercializacao(sqlReader.GetValue(11) == System.DBNull.Value ? -1 : sqlReader.GetInt32(11));
                        prod.VLR_PESO_PRODUTO = sqlReader.GetValue(13) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(13));
                        prod.QTD_ESTOQUE = sqlReader.GetValue(14) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(14));
                        prod.DESCRICAO_APELIDO_PRODUTO = sqlReader.GetValue(15) == System.DBNull.Value ? "" : sqlReader.GetString(15);
                        prod.COD_SUBFAMILIA_PRODUTO = sqlReader.GetValue(16) == System.DBNull.Value ? -1 : sqlReader.GetInt32(16);
                        prod.COD_LINHA_SUBFAMILIA_PRODUTO = sqlReader.GetValue(17) == System.DBNull.Value ? -1 : sqlReader.GetInt32(17);
                        prod.PCT_MINIMO_LUCRATIVIDADE = sqlReader.GetValue(18) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(18));
                        prod.VLR_CUSTO_GERENCIAL = sqlReader.GetValue(19) == System.DBNull.Value ? 0 : decimal.Parse(sqlReader.GetValue(19).ToString());
                        prod.IND_PRODUTO_COM_PRECO = sqlReader.GetValue(20) == System.DBNull.Value ? false : sqlReader.GetBoolean(20);
                        prod.IND_ITEM_CONJUNTO = sqlReader.GetBoolean(21);
                        prod.IND_ITEM_COMBO = sqlReader.GetBoolean(22);
                        prod.COD_PRODUTO_CONJUNTO = sqlReader.GetValue(23) == System.DBNull.Value ? -1 : sqlReader.GetInt32(23);
                        prod.QTD_PRODUTO_COMPOSICAO = sqlReader.GetValue(24) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(24));
                        prod.PCT_DESCONTO_PRODUTO_COMPOSICAO = sqlReader.GetValue(25) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(25));
                        prod.QTD_UNIDADE_MEDIDA = sqlReader.GetValue(26) == System.DBNull.Value ? -1 : sqlReader.GetInt32(26);
                        prod.DAT_VALIDADE_INICIO_COMBO = sqlReader.GetValue(27) == System.DBNull.Value ? new DateTime(1900, 1, 1) : sqlReader.GetDateTime(27);
                        prod.DAT_VALIDADE_TERMINO_COMBO = sqlReader.GetValue(28) == System.DBNull.Value ? new DateTime(1900, 1, 1) : sqlReader.GetDateTime(28);
                        prod.IND_STATUS_COMBO = sqlReader.GetValue(29) == System.DBNull.Value ? "" : sqlReader.GetString(29);
                        prod.QTD_MAXIMA_COMBO_PEDIDO = sqlReader.GetValue(30) == System.DBNull.Value ? -1 : sqlReader.GetInt32(30);
                        prod.COD_TABELA_PRECO_COMBO = sqlReader.GetValue(31) == System.DBNull.Value ? -1 : sqlReader.GetInt32(31);
                        prod.QTD_ESTOQUE_PRONTA_ENTREGA = sqlReader.GetValue(32) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(32));
                        prod.IND_PRODUTO_FOCO = sqlReader.GetBoolean(33);
                        prod.IND_PRODUTO_BLOQUEADO = sqlReader.GetValue(34) == System.DBNull.Value ? false : true;

                        if (EXISTE_NUM_ORDEM_ESTOQUE)
                        {
                            prod.NUM_ORDEM_ESTOQUE = sqlReader.GetValue(35) == System.DBNull.Value ? 0 : sqlReader.GetInt32(35);

                            if (CSEmpresa.ColunaExiste("PRODUTO", "PCT_TAXA_MAX_INDENIZACAO"))
                                prod.PCT_TAXA_MAX_INDENIZACAO = sqlReader.GetValue(36) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(36));
                        }
                        else
                        {
                            if (CSEmpresa.ColunaExiste("PRODUTO", "PCT_TAXA_MAX_INDENIZACAO"))
                                prod.PCT_TAXA_MAX_INDENIZACAO = sqlReader.GetValue(35) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(35));
                        }

                        if (CSEmpresa.ColunaExiste("PRODUTO", "IND_LIBERAR_CONDICAO_PAGAMENTO"))
                            prod.IND_LIBERAR_CONDICAO_PAGAMENTO = sqlReader.GetValue(37) == System.DBNull.Value ? false : Convert.ToBoolean(sqlReader.GetValue(37));
                        else
                            prod.IND_LIBERAR_CONDICAO_PAGAMENTO = false;

                        prod.DSC_INFO_CALORICA = sqlReader.GetValue(38) == System.DBNull.Value ? "-" : string.IsNullOrEmpty(sqlReader.GetString(38)) ? "-" : sqlReader.GetString(38);
                        prod.DSC_INFO_NUTRICIONAL = sqlReader.GetValue(39) == System.DBNull.Value ? "-" : string.IsNullOrEmpty(sqlReader.GetString(39)) ? "-" : sqlReader.GetString(39);
                        prod.DSC_INFO_OUTRAS = sqlReader.GetValue(40) == System.DBNull.Value ? "-" : string.IsNullOrEmpty(sqlReader.GetString(40)) ? "-" : sqlReader.GetString(40);
                        prod.COD_FABRICA_PRODUTO = sqlReader.GetValue(41) == System.DBNull.Value ? prod.COD_PRODUTO : sqlReader.GetInt32(41);

                        // Busca o preço do produto
                        prod.PRECOS_PRODUTO = null;

                        if (!CSGlobal.IsNumeric(prod.DESCRICAO_APELIDO_PRODUTO))
                        {
                            pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", 0);
                            pCOD_PRODUTO.DbType = DbType.Int32;
                            pCOD_PRODUTO.Value = prod.COD_PRODUTO;

                            //Insere produto cujo apelido seja string, este deve aparecer no final da lista de produtos
                            CSDataAccess.Instance.ExecuteNonQuery(sqlInsert, pCOD_PRODUTO);
                        }

                        if (CSEmpresa.Current.UtilizaTabelaNova &&
                            CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                        {
                            StringBuilder sbQuantidadeMinima = new StringBuilder();
                            sbQuantidadeMinima.AppendLine(" SELECT COD_CATEGORIA, QTD_MINIMA ");
                            sbQuantidadeMinima.AppendLine(" FROM PRODUTO_CATEGORIA ");
                            sbQuantidadeMinima.AppendLine(" WHERE COD_PRODUTO = ? ");
                            sbQuantidadeMinima.AppendLine(" ORDER BY COD_CATEGORIA ");

                            var codProduto = new SQLiteParameter("@codProduto", prod.COD_PRODUTO);
                            codProduto.DbType = DbType.Int32;

                            using (SQLiteDataReader sqlReader2 = CSDataAccess.Instance.ExecuteReader(sbQuantidadeMinima.ToString(), codProduto))
                            {
                                Hashtable ht = new Hashtable();
                                while (sqlReader2.Read())
                                {
                                    if (sqlReader2.GetValue(0) != DBNull.Value)
                                    {
                                        ht.Add(sqlReader2.GetValue(0), sqlReader2.GetValue(1));
                                    }
                                }
                                prod.HT_QTD_MINIMA = ht;
                            }
                        }

                        // Adcionao o produto na coleção dos produtos
                        base.InnerList.Add(prod);

                        // Dispara o evento de mais um produto processado
                        OnTickNewProduto();
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }

                // Dispara o evento de fim do processamento dos produtos
                OnEndProdutos();

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos produtos.", ex);
            }
        }

        private static decimal PrecoTabelaPrecoPadrao(int COD_PRODUTO, int COD_TABELA_PRECO_PADRAO)
        {
            try
            {
                decimal precoProduto = 0;

                SQLiteDataReader reader = null;
                SQLiteParameter pCOD_PRODUTO = null;
                SQLiteParameter pCOD_EMPREGADO = null;
                SqliteParameter pCOD_TABELA_PRECO_PADRAO = null;

                // Query de busca dos preços dos produtos
                string sqlQuery =
                    "SELECT T1.VLR_PRODUTO " +
                    "  FROM TABELA_PRECO_PRODUTO T1 " +
                    "  JOIN TABELA_PRECO T2 ON T1.COD_TABELA_PRECO = T2.COD_TABELA_PRECO " +
                    "  JOIN PRODUTO T3 ON T1.COD_PRODUTO = T3.COD_PRODUTO " +
                    "  LEFT JOIN BLOQUEIO_TABELA_PRECO T4 ON T2.COD_TABELA_PRECO = T4.COD_TABELA_PRECO " +
                    "   AND T4.COD_TABELA_BLOQUEIO = 7 AND T4.TIPO_BLOQUEIO = 'B' AND T4.COD_BLOQUEIO = ? " +
                    " WHERE T1.COD_PRODUTO = ? " +
                    "   AND T1.VLR_PRODUTO > 0 " +
                    "   AND T4.COD_TABELA_PRECO IS NULL " +
                    "   AND T2.COD_TABELA_PRECO = ? " +
                    " ORDER BY T2.COD_TABELA_PRECO ";

                // Cria os parametros 
                pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", COD_PRODUTO);
                pCOD_PRODUTO.DbType = DbType.Int32;
                pCOD_TABELA_PRECO_PADRAO = new SQLiteParameter("@COD_TABELA_PRECO_PADRAO", COD_TABELA_PRECO_PADRAO);
                pCOD_TABELA_PRECO_PADRAO.DbType = DbType.Int32;

                // Controle do Boleano para realizar o Prepare do SQL.
                using (reader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_EMPREGADO, pCOD_PRODUTO, pCOD_TABELA_PRECO_PADRAO))
                {
                    if (reader.Read())
                    {
                        precoProduto = reader.GetValue(0) == System.DBNull.Value ? -1 : reader.GetDecimal(0);
                    }
                    // Fecha o reader
                    reader.Close();
                    reader.Dispose();
                }

                return precoProduto;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Retorna produtos conforme filtros passados
        /// </summary>
        public static ArrayList BuscaProdutos(int COD_GRUPO_COMERCIALIZACAO, int COD_GRUPO, int COD_GRUPO_FAMILIA, int COD_CATEGORIA, int COD_PDV,
            string IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE, int COD_TIPO_DISTRIBUICAO_POLITICA, bool ordenaPorCodigo, int COD_DENVER, bool PoliticaFlexx)
        {
            StringBuilder sqlQuery = null;
            ArrayList produtos = null;

            SQLiteParameter pCOD_GRUPO_COMERCIALIZACAO = null;
            SQLiteParameter pCOD_GRUPO = null;
            SQLiteParameter pCOD_GRUPO_FAMILIA = null;
            SQLiteParameter pCOD_CATEGORIA = null;
            SQLiteParameter pCOD_PDV = null;
            SQLiteParameter pCOD_TIPO_DISTRIBUICAO_POLITICA = null;

            int codProd;

            sqlQuery = new StringBuilder();

            try
            {
                produtos = new ArrayList();

                sqlQuery.Length = 0;
                sqlQuery.Append(" SELECT T1.COD_PRODUTO,T4.IND_VENDA_MES ");
                sqlQuery.Append("       ,T4.IND_VENDA_ULTIMA_VISITA,T5.IND_PROD_ESPECIFICO_CATEGORIA ");
                sqlQuery.Append("       ,T1.DESCRICAO_APELIDO_PRODUTO,T6.DSC_FAMILIA_PRODUTO");

                if (CSEmpresa.Current.UtilizaTabelaNova &&
                    CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                {
                    sqlQuery.Append("      , T5.QTD_MINIMA ");
                }

                sqlQuery.Append(CSGlobal.ValidarTopCategoria ? ", IND_PROD_TOP_CATEGORIA" : ", 0 AS IND_PROD_CATEGORIA ");

                sqlQuery.Append("   FROM PRODUTO T1 ");
                sqlQuery.Append("   LEFT JOIN TMPITEMPEDIDO T2 ");
                sqlQuery.Append("     ON T2.CODPRODUTO = T1.COD_PRODUTO ");
                sqlQuery.Append("   LEFT JOIN TMPITENS T3 ");
                sqlQuery.Append("     ON T3.COD_PRODUTO = T1.COD_PRODUTO ");
                sqlQuery.Append("   LEFT JOIN PDV_PRODUTO_MUR T4 ");
                sqlQuery.Append("     ON T1.COD_PRODUTO = T4.COD_PRODUTO AND T4.COD_PDV = ? ");
                sqlQuery.Append("   LEFT JOIN PRODUTO_CATEGORIA T5 ");
                sqlQuery.Append("     ON T5.COD_PRODUTO = T1.COD_PRODUTO AND T5.COD_CATEGORIA = ? ");
                sqlQuery.Append("   JOIN FAMILIA_PRODUTO T6 ON T6.COD_FAMILIA_PRODUTO = T1.COD_FAMILIA_PRODUTO AND T6.COD_GRUPO = T1.COD_GRUPO");
                sqlQuery.Append("  WHERE T1.IND_ATIVO = 'A' ");
                sqlQuery.Append("    AND T1.COD_GRUPO = ? ");
                sqlQuery.Append("    AND ? IN (-1, T1.COD_FAMILIA_PRODUTO) ");
                sqlQuery.Append("    AND ? IN (-1, T1.COD_GRUPO_COMERCIALIZACAO) ");
                sqlQuery.Append("    AND T1.IND_PRODUTO_COM_PRECO = 1 ");
                sqlQuery.Append("    AND T1.COD_TIPO_DISTRIBUICAO_POLITICA = ? ");
                sqlQuery.Append("    AND T2.CODPRODUTO IS NULL ");
                sqlQuery.Append("    AND T3.COD_PRODUTO IS NULL ");
                sqlQuery.Append("  " + (IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE == "N" ? " AND T1.QTD_ESTOQUE > 0 " : "") + " ");

                if (ordenaPorCodigo)
                {
                    if (CSGlobal.ValidarTopCategoria)
                        sqlQuery.Append(" ORDER BY IND_PROD_TOP_CATEGORIA DESC, T5.IND_PROD_ESPECIFICO_CATEGORIA DESC, CAST(T1.DESCRICAO_APELIDO_PRODUTO as integer) ");
                    else
                        sqlQuery.Append(" ORDER BY T5.IND_PROD_ESPECIFICO_CATEGORIA DESC, CAST(T1.DESCRICAO_APELIDO_PRODUTO as integer) ");
                }
                else
                {
                    if (CSGlobal.ValidarTopCategoria)
                        sqlQuery.Append(" ORDER BY IND_PROD_TOP_CATEGORIA DESC, T5.IND_PROD_ESPECIFICO_CATEGORIA DESC, T1.DSC_PRODUTO, CAST(T1.DESCRICAO_APELIDO_PRODUTO as integer) ");
                    else
                        sqlQuery.Append(" ORDER BY T5.IND_PROD_ESPECIFICO_CATEGORIA DESC, T1.DSC_PRODUTO, CAST(T1.DESCRICAO_APELIDO_PRODUTO as integer) ");
                }

                m_Exclusivista = false;

                // Cria os parametros
                pCOD_PDV = new SQLiteParameter("@COD_PDV", COD_PDV);

                if (CSEmpresa.Current.IND_CLUSTER_PRODUTOS_PELO_TIPO_FREEZER)
                    pCOD_CATEGORIA = new SQLiteParameter("@COD_CATEGORIA", COD_DENVER);
                else
                    pCOD_CATEGORIA = new SQLiteParameter("@COD_CATEGORIA", COD_CATEGORIA);

                pCOD_GRUPO = new SQLiteParameter("@COD_GRUPO", COD_GRUPO);
                pCOD_GRUPO_FAMILIA = new SQLiteParameter("@COD_GRUPO_FAMILIA", COD_GRUPO_FAMILIA);
                pCOD_TIPO_DISTRIBUICAO_POLITICA = new SQLiteParameter("@COD_TIPO_DISTRIBUICAO_POLITICA", COD_TIPO_DISTRIBUICAO_POLITICA);
                pCOD_GRUPO_COMERCIALIZACAO = new SQLiteParameter("@COD_GRUPO_COMERCIALIZACAO", COD_GRUPO_COMERCIALIZACAO);

                // Seta os tipos do parametros
                pCOD_PDV.DbType = DbType.Int32;
                pCOD_CATEGORIA.DbType = DbType.Int32;
                pCOD_GRUPO.DbType = DbType.Int32;
                pCOD_GRUPO_FAMILIA.DbType = DbType.Int32;
                pCOD_TIPO_DISTRIBUICAO_POLITICA.DbType = DbType.Int32;
                pCOD_GRUPO_COMERCIALIZACAO.DbType = DbType.Int32;

                // Busca todos os grupos
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(ref sqlCommandCodigo, sqlQuery.ToString(), pCOD_PDV, pCOD_CATEGORIA, pCOD_GRUPO, pCOD_GRUPO_FAMILIA, pCOD_GRUPO_COMERCIALIZACAO, pCOD_TIPO_DISTRIBUICAO_POLITICA))
                {
                    while (sqlReader.Read())
                    {
                        codProd = sqlReader.GetInt32(0);

                        CSProduto produtoVenda = GetProduto(codProd);

                        if (produtoVenda != null)
                        {

                            produtoVenda.IND_VENDA_MES = sqlReader.GetValue(1) == System.DBNull.Value ? false : sqlReader.GetBoolean(1);
                            produtoVenda.IND_VENDA_ULTIMA_VISITA = sqlReader.GetValue(2) == System.DBNull.Value ? false : sqlReader.GetBoolean(2);
                            //produtoVenda.IND_PROD_ESPECIFICO_CATEGORIA = sqlReader.GetValue(3) == System.DBNull.Value ? false : sqlReader.GetBoolean(3);
                            produtoVenda.FAMILIA_PRODUTO.DSC_FAMILIA_PRODUTO = sqlReader.GetString(5);
                            //produtoVenda.HT_QTD_MINIMA = sqlReader.GetValue(6) == System.DBNull.Value ? 0 : sqlReader.GetInt32(6);
                            //System.Windows.Forms.MessageBox.Show(produtoVenda.DESCRICAO_APELIDO_PRODUTO + " - qtdMin:" + produtoVenda.QTD_MINIMA);
                            if (produtoVenda.IND_PROD_ESPECIFICO_CATEGORIA)
                                m_Exclusivista = true;

                            if (PoliticaFlexx)
                            {
                                produtoVenda.PRECOS_PRODUTO_TABELA_PRECO_PADRAO = PrecoTabelaPrecoPadrao(produtoVenda.COD_PRODUTO, CSPDVs.Current.COD_TABPRECO_PADRAO);
                            }

                            produtos.Add(produtoVenda);
                        }

                    }
                    sqlReader.Close();
                    sqlReader.Dispose();
                }

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.Message);
                throw new Exception("Erro na busca de produtos.", ex);
            }

            return produtos;
        }

        public static void DescartaPoliticaBroker()
        {
            try
            {
                //[ Se política broker... ]
                if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2)
                {
                    // [ Descarta cálculos realizados ]
                    foreach (CSProduto produto in CSProdutos.Items)
                    {
                        produto.PRECOS_PRODUTO = null;
                    }

                    if (CSEmpregados.Current.USAR_CACHE_PRICE)
                    {
                        // [ libera tabela hash de comandos preparados ]
                        CSDataAccess.Instance.DisposePreparedCommands(TypedHashtable.HashtableEntryType.All);

                        // [ libera tabela hash de cache de resultados ]
                        CSDataAccess.Instance.DisposeCachedResults(TypedHashtable.HashtableEntryType.All);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        public static bool GetProdutoPoliticaBroker(int COD_PRODUTO, int clienteSoldto, string grupoComercializacao)
        {
            try
            {
                StringBuilder sqlQuery = null;

                SQLiteParameter[] parametros = new SQLiteParameter[03];
                bool Achou = false;
                string grupoComercializacaoRetorno = "";
                string produtoRetorno = "";

                sqlQuery = new StringBuilder();

                // [ Tratamento temporário para atender à mudança de códigos de produtos da nestle ]
                if (COD_PRODUTO < 41000)
                    COD_PRODUTO += 410000;

                parametros[00] = new SQLiteParameter("@04PRODUTO", COD_PRODUTO.ToString().PadLeft(18, '0'));
                parametros[01] = new SQLiteParameter("@05COD_GRUPO_COMERCIALIZACAO", grupoComercializacao);
                parametros[02] = new SQLiteParameter("@06CLIENTE_SOLDTO", clienteSoldto.ToString().PadLeft(10, '0'));

                sqlQuery.Length = 0;
                sqlQuery.Append("  SELECT BRK_EPRODORG.CDPRD AS PRODUTO ");
                sqlQuery.Append("        ,BRK_ECLIENTE.CDGER0 AS CDGER0 ");
                sqlQuery.Append("    FROM BRK_ECLIENTE ");
                sqlQuery.Append("    JOIN BRK_ECLIENTBAS    ON BRK_ECLIENTBAS.CDCLI = BRK_ECLIENTE.CDCLI ");
                sqlQuery.Append("    LEFT JOIN BRK_TFILFAT  ON BRK_TFILFAT.CDFILFAT = BRK_ECLIENTE.CDFILFAT ");
                sqlQuery.Append("    LEFT JOIN BRK_EPRODFIL ON BRK_EPRODFIL.CDPRD = ? AND BRK_EPRODFIL.CDFILFAT = BRK_ECLIENTE.CDFILFAT ");
                sqlQuery.Append("    LEFT JOIN BRK_ECLIIMP  ON BRK_ECLIIMP.CDCLI = BRK_ECLIENTE.CDCLI AND BRK_ECLIIMP.CDCATIMP = 'IBRX' ");
                sqlQuery.Append("    LEFT JOIN BRK_EPRODTPIMP ON BRK_EPRODTPIMP.CDPRD = BRK_EPRODFIL.CDPRD AND BRK_EPRODTPIMP.CDPAIS = 'BR' ");
                sqlQuery.Append("    LEFT JOIN BRK_EPRODORG ");
                sqlQuery.Append("      ON BRK_EPRODORG.CDPRD = BRK_EPRODFIL.CDPRD ");
                sqlQuery.Append("     AND BRK_EPRODORG.CDGER0 = BRK_ECLIENTE.CDGER0 ");
                sqlQuery.Append("     AND BRK_EPRODORG.CDCANDISTR = BRK_ECLIENTE.CDCANDISTR ");
                sqlQuery.Append("     AND BRK_EPRODORG.CDDIVISAO = '00' ");
                sqlQuery.Append("     AND BRK_EPRODORG.CDGER0 = ? ");
                sqlQuery.Append("    LEFT JOIN BRK_EPRODUTO ON BRK_EPRODUTO.CDPRD = BRK_EPRODFIL.CDPRD ");
                sqlQuery.Append("    LEFT JOIN BRK_EUNIDMED ON BRK_EUNIDMED.CDPRD = BRK_EPRODFIL.CDPRD AND BRK_EUNIDMED.CDUNIDMED = 'KGM' ");
                sqlQuery.Append("   WHERE BRK_ECLIENTE.CDCLI = ? ");

                using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(true, sqlQuery.ToString(), parametros))
                {
                    while (reader.Read())
                    {
                        produtoRetorno = (reader.GetValue(0) == System.DBNull.Value) ? "" : reader.GetString(0);
                        grupoComercializacaoRetorno = (reader.GetValue(1) == System.DBNull.Value) ? "" : reader.GetString(1);

                        // [ Se o grupo de comercialização for o procurado. ]
                        if (grupoComercializacaoRetorno == grupoComercializacao && produtoRetorno != "")
                        {
                            Achou = true;
                            break;
                        }
                    }

                    reader.Close();
                    reader.Dispose();

                }
                return Achou;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region [ SubClasses ]

        /// <summary>
        /// Guarda a informação sobre o grupo dos proddutos
        /// </summary>
        public class CSProduto
#if ANDROID
 : Java.Lang.Object
#endif
        {
            #region [ Variaveis ]

            private int m_COD_PRODUTO;
            private CSGruposProduto.CSGrupoProduto m_GRUPO;
            private int m_COD_TIPO_DISTRIBUICAO_POLITICA;
            private CSGruposComercializacao.CSGrupoComercializacao m_GRUPO_COMERCIALIZACAO;
            private int m_COD_SUBFAMILIA_PRODUTO;
            private int m_COD_LINHA_SUBFAMILIA_PRODUTO;
            private CSFamiliasProduto.CSFamiliaProduto m_FAMILIA_PRODUTO;
            private string m_COD_UNIDADE_MEDIDA;
            private string m_DSC_PRODUTO;
            private string m_DSC_APELIDO_PRODUTO;
            private int m_QTD_UNIDADE_EMBALAGEM;
            private decimal m_VLR_MINIMO_PEDIDO;
            private decimal m_PRC_MAXIMO_DESCONTO;
            private decimal m_PRC_ACRESCIMO_QTDE_UNITARIA;
            private bool m_IND_PROD_ESPECIFICO_CATEGORIA;
            private bool m_IND_PROD_TOP_CATEGORIA;
            private Hashtable m_HT_QTD_MINIMA;
            private decimal m_VLR_PESO_PRODUTO;
            private decimal m_QTD_ESTOQUE;
            private decimal m_QTD_ESTOQUE_PRONTA_ENTREGA;
            private string m_DESCRICAO_APELIDO_PRODUTO;
            private int m_NUM_ORDEM_ESTOQUE = 0;
            private bool m_EDITOU_DADOS;
            //Somente utilizadas na tela de produtos do pedido
            private bool m_IND_VENDA_MES;
            private bool m_IND_VENDA_ULTIMA_VISITA;
            private decimal m_PCT_MINIMO_LUCRATIVIDADE;
            private decimal m_VLR_CUSTO_GERENCIAL;
            private bool m_IND_PRODUTO_COM_PRECO;
            private CSProdutos.CSProduto.CSPrecosProdutos m_PRECOS_PRODUTO;
            private decimal m_PRECOS_PRODUTO_TABELA_PRECO_PADRAO;
            private string m_POPUP_TEXT = null;
            private bool m_IND_ITEM_CONJUNTO;
            private bool m_IND_LIBERAR_CONDICAO_PAGAMENTO;
            private bool m_IND_PRODUTO_BLOQUEADO;
            private bool m_IND_ITEM_COMBO;
            private int m_COD_PRODUTO_CONJUNTO;
            private decimal m_QTD_PRODUTO_COMPOSICAO;
            private decimal m_PCT_DESCONTO_PRODUTO_COMPOSICAO;
            private int m_QTD_UNIDADE_MEDIDA;
            private DateTime m_DAT_VALIDADE_INICIO_COMBO;
            private DateTime m_DAT_VALIDADE_TERMINO_COMBO;
            private string m_IND_STATUS_COMBO;
            private int m_QTD_MAXIMA_COMBO_PEDIDO;
            private int m_COD_TABELA_PRECO_COMBO;
            private bool m_IND_PRODUTO_FOCO;
            private int m_QTD_PRODUTO_SUGERIDO;
            private bool m_IND_UTILIZA_QTD_MINIMA;
            private bool m_PRODUTO_SUGERIDO_REGRA_FREEZER = false;
            private decimal m_QTD_GIRO_SELLOUT;
            private decimal m_QTD_GIRO_MEDIO;
            private decimal m_PCT_TAXA_MAX_INDENIZACAO;
            private string m_DSC_INFO_OUTRAS;
            private string m_DSC_INFO_NUTRICIONAL;
            private string m_DSC_INFO_CALORICA;
            private int m_COD_FABRICA_PRODUTO;

            #endregion

            #region [ Propriedades ]

            /// <summary>
            /// Guarda o codigo do produto
            /// </summary>
            public int COD_PRODUTO
            {
                get
                {
                    return m_COD_PRODUTO;
                }
                set
                {
                    m_COD_PRODUTO = value;
                }
            }

            public int COD_FABRICA_PRODUTO
            {
                get
                {
                    return m_COD_FABRICA_PRODUTO;
                }
                set
                {
                    m_COD_FABRICA_PRODUTO = value;
                }
            }

            /// <summary>
            /// Guarda a grupo do produto
            /// </summary>
            public CSGruposProduto.CSGrupoProduto GRUPO
            {
                get
                {
                    return m_GRUPO;
                }
                set
                {
                    m_GRUPO = value;
                }
            }

            public int COD_TIPO_DISTRIBUICAO_POLITICA
            {
                get
                {
                    return m_COD_TIPO_DISTRIBUICAO_POLITICA;
                }
                set
                {
                    m_COD_TIPO_DISTRIBUICAO_POLITICA = value;
                }
            }

            public CSGruposComercializacao.CSGrupoComercializacao GRUPO_COMERCIALIZACAO
            {
                get
                {
                    return m_GRUPO_COMERCIALIZACAO;
                }
                set
                {
                    m_GRUPO_COMERCIALIZACAO = value;
                }
            }

            public int COD_SUBFAMILIA_PRODUTO
            {
                get
                {
                    return m_COD_SUBFAMILIA_PRODUTO;
                }
                set
                {
                    m_COD_SUBFAMILIA_PRODUTO = value;
                }
            }

            public int COD_LINHA_SUBFAMILIA_PRODUTO
            {
                get
                {
                    return m_COD_LINHA_SUBFAMILIA_PRODUTO;
                }
                set
                {
                    m_COD_LINHA_SUBFAMILIA_PRODUTO = value;
                }
            }

            public decimal VLR_PESO_PRODUTO
            {
                get
                {
                    return m_VLR_PESO_PRODUTO;
                }
                set
                {
                    m_VLR_PESO_PRODUTO = value;
                }
            }

            public CSFamiliasProduto.CSFamiliaProduto FAMILIA_PRODUTO
            {
                get
                {
                    return m_FAMILIA_PRODUTO;
                }
                set
                {
                    m_FAMILIA_PRODUTO = value;
                }
            }

            public string COD_UNIDADE_MEDIDA
            {
                get
                {
                    return m_COD_UNIDADE_MEDIDA;
                }
                set
                {
                    m_COD_UNIDADE_MEDIDA = value.Trim();
                }
            }

            public string DSC_UNIDADE_MEDIDA
            {
                get
                {
                    string unidadeMedida = "";

                    try
                    {
                        if (!string.IsNullOrEmpty(this.COD_UNIDADE_MEDIDA))
                            switch (this.COD_UNIDADE_MEDIDA)
                            {
                                case "DZ":
                                case "CX":
                                    unidadeMedida = this.COD_UNIDADE_MEDIDA.ToString() + "/" + this.QTD_UNIDADE_EMBALAGEM.ToString();
                                    break;
                                default:
                                    unidadeMedida = this.COD_UNIDADE_MEDIDA.ToString();
                                    break;
                            }
                    }
                    catch
                    {
                    }

                    return unidadeMedida;
                }
            }

            public string DSC_PRODUTO
            {
                get
                {
                    return m_DSC_PRODUTO;
                }
                set
                {
                    m_DSC_PRODUTO = value.Trim();
                }
            }

            public string DSC_INFO_CALORICA
            {
                get
                {
                    return m_DSC_INFO_CALORICA;
                }
                set
                {
                    m_DSC_INFO_CALORICA = value;
                }
            }

            public string DSC_INFO_NUTRICIONAL
            {
                get
                {
                    return m_DSC_INFO_NUTRICIONAL;
                }
                set
                {
                    m_DSC_INFO_NUTRICIONAL = value;
                }
            }

            public string DSC_INFO_OUTRAS
            {
                get
                {
                    return m_DSC_INFO_OUTRAS;
                }
                set
                {
                    m_DSC_INFO_OUTRAS = value;
                }
            }

            public string DSC_APELIDO_PRODUTO
            {
                get
                {
                    return m_DSC_APELIDO_PRODUTO;
                }
                set
                {
                    m_DSC_APELIDO_PRODUTO = value.Trim();
                }
            }

            public int QTD_UNIDADE_EMBALAGEM
            {
                get
                {
                    return m_QTD_UNIDADE_EMBALAGEM;
                }
                set
                {
                    m_QTD_UNIDADE_EMBALAGEM = value;
                }
            }

            public decimal VLR_MINIMO_PEDIDO
            {
                get
                {
                    return m_VLR_MINIMO_PEDIDO;
                }
                set
                {
                    m_VLR_MINIMO_PEDIDO = value;
                }
            }

            public decimal PRC_MAXIMO_DESCONTO
            {
                get
                {
                    return m_PRC_MAXIMO_DESCONTO;
                }
                set
                {
                    m_PRC_MAXIMO_DESCONTO = value;
                }
            }

            public decimal PRC_ACRESCIMO_QTDE_UNITARIA
            {
                get
                {
                    return m_PRC_ACRESCIMO_QTDE_UNITARIA;
                }
                set
                {
                    m_PRC_ACRESCIMO_QTDE_UNITARIA = value;
                }
            }
            public bool IND_PROD_ESPECIFICO_CATEGORIA
            {
                get
                {
                    if (CSEmpresa.Current.IND_CLUSTER_PRODUTOS_PELO_TIPO_FREEZER)
                        m_IND_PROD_ESPECIFICO_CATEGORIA = CSProdutos.CSProduto.CSCategoria.GetEspecificoCategoria(CSPDVs.Current.COD_DENVER, this.COD_PRODUTO);
                    else
                        m_IND_PROD_ESPECIFICO_CATEGORIA = CSProdutos.CSProduto.CSCategoria.GetEspecificoCategoria(CSPDVs.Current.COD_CATEGORIA, this.COD_PRODUTO);

                    return m_IND_PROD_ESPECIFICO_CATEGORIA;
                }
            }

            public bool IND_PROD_TOP_CATEGORIA
            {
                get
                {
                    if (!CSGlobal.ValidarTopCategoria)
                        return false;

                    if (CSEmpresa.Current.IND_CLUSTER_PRODUTOS_PELO_TIPO_FREEZER)
                        m_IND_PROD_TOP_CATEGORIA = CSProdutos.CSProduto.CSCategoria.GetEspecificoTopCategoria(CSPDVs.Current.COD_DENVER, this.COD_PRODUTO);
                    else
                        m_IND_PROD_TOP_CATEGORIA = CSProdutos.CSProduto.CSCategoria.GetEspecificoTopCategoria(CSPDVs.Current.COD_CATEGORIA, this.COD_PRODUTO);

                    return m_IND_PROD_TOP_CATEGORIA;
                }
            }

            public Hashtable HT_QTD_MINIMA
            {
                get
                {
                    return m_HT_QTD_MINIMA;
                }
                set
                {
                    m_HT_QTD_MINIMA = value;
                }
            }

            public decimal PCT_MINIMO_LUCRATIVIDADE
            {
                get
                {
                    return m_PCT_MINIMO_LUCRATIVIDADE;
                }
                set
                {
                    m_PCT_MINIMO_LUCRATIVIDADE = value;
                }
            }

            public decimal VLR_CUSTO_GERENCIAL
            {
                get
                {
                    return m_VLR_CUSTO_GERENCIAL;
                }
                set
                {
                    m_VLR_CUSTO_GERENCIAL = value;
                }
            }

            public decimal PRECOS_PRODUTO_TABELA_PRECO_PADRAO
            {
                get
                {
                    return m_PRECOS_PRODUTO_TABELA_PRECO_PADRAO;
                }
                set
                {
                    m_PRECOS_PRODUTO_TABELA_PRECO_PADRAO = value;
                }
            }

            public CSProdutos.CSProduto.CSPrecosProdutos PRECOS_PRODUTO
            {
                get
                {
                    if (m_PRECOS_PRODUTO == null)
                    {
                        try
                        {
                            m_PRECOS_PRODUTO = new CSPrecosProdutos(this.COD_PRODUTO);

                        }
                        catch
                        {
                            m_PRECOS_PRODUTO = null;
                        }
                    }

                    return m_PRECOS_PRODUTO;
                }
                set
                {
                    m_PRECOS_PRODUTO = value;
                }
            }

            public decimal QTD_ESTOQUE
            {
                get
                {
                    return m_QTD_ESTOQUE;
                }
                set
                {
                    m_QTD_ESTOQUE = value;
                }
            }

            public decimal QTD_ESTOQUE_PRONTA_ENTREGA
            {
                get
                {
                    return m_QTD_ESTOQUE_PRONTA_ENTREGA;
                }
                set
                {
                    m_QTD_ESTOQUE_PRONTA_ENTREGA = value;
                }
            }

            public string DESCRICAO_APELIDO_PRODUTO
            {
                get
                {
                    return m_DESCRICAO_APELIDO_PRODUTO;
                }
                set
                {
                    m_DESCRICAO_APELIDO_PRODUTO = value.Trim();
                }
            }

            public int NUM_ORDEM_ESTOQUE
            {
                get
                {
                    return m_NUM_ORDEM_ESTOQUE;
                }
                set
                {
                    m_NUM_ORDEM_ESTOQUE = value;
                }
            }

            public bool EDITOU_DADOS
            {
                get
                {
                    return m_EDITOU_DADOS;
                }
                set
                {
                    m_EDITOU_DADOS = value;
                }
            }

            public bool IND_VENDA_MES
            {
                get
                {
                    return m_IND_VENDA_MES;
                }
                set
                {
                    m_IND_VENDA_MES = value;
                }
            }

            public bool IND_VENDA_ULTIMA_VISITA
            {
                get
                {
                    return m_IND_VENDA_ULTIMA_VISITA;
                }
                set
                {
                    m_IND_VENDA_ULTIMA_VISITA = value;
                }
            }

            public int UNIDADES_POR_CAIXA
            {
                get
                {
                    return GetUnidadesPorCaixa(this.COD_UNIDADE_MEDIDA, this.QTD_UNIDADE_EMBALAGEM);
                }
            }

            public bool IND_PRODUTO_COM_PRECO
            {
                get
                {
                    return m_IND_PRODUTO_COM_PRECO;
                }
                set
                {
                    m_IND_PRODUTO_COM_PRECO = value;
                }
            }

            public int QTD_UNIDADE_MEDIDA
            {
                get
                {
                    return m_QTD_UNIDADE_MEDIDA;
                }
                set
                {
                    m_QTD_UNIDADE_MEDIDA = value;
                }
            }
            /// <summary>
            /// Data inicial da validade do combo
            /// </summary>
            public DateTime DAT_VALIDADE_INICIO_COMBO
            {
                get
                {
                    return m_DAT_VALIDADE_INICIO_COMBO;
                }
                set
                {
                    m_DAT_VALIDADE_INICIO_COMBO = value;
                }
            }
            /// <summary>
            /// Data final da validade do combo
            /// </summary>
            public DateTime DAT_VALIDADE_TERMINO_COMBO
            {
                get
                {
                    return m_DAT_VALIDADE_TERMINO_COMBO;
                }
                set
                {
                    m_DAT_VALIDADE_TERMINO_COMBO = value;
                }
            }
            /// <summary>
            /// Status do combo
            /// </summary>
            public string IND_STATUS_COMBO
            {
                get
                {
                    return m_IND_STATUS_COMBO;
                }
                set
                {
                    m_IND_STATUS_COMBO = value;
                }
            }
            /// <summary>
            /// Quantidade maxima de combo por pedido
            /// </summary>
            public int QTD_MAXIMA_COMBO_PEDIDO
            {
                get
                {
                    return m_QTD_MAXIMA_COMBO_PEDIDO;
                }
                set
                {
                    m_QTD_MAXIMA_COMBO_PEDIDO = value;
                }
            }
            /// <summary>
            /// Tabela de preço do combo
            /// </summary>
            public int COD_TABELA_PRECO_COMBO
            {
                get
                {
                    return m_COD_TABELA_PRECO_COMBO;
                }
                set
                {
                    m_COD_TABELA_PRECO_COMBO = value;
                }
            }

            public string POPUP_TEXT
            {
                get
                {
                    try
                    {
                        if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2 &&
                            this.m_POPUP_TEXT == null)
                        {
                            string texto = "";
                            texto += "Produto: " + this.DESCRICAO_APELIDO_PRODUTO;
                            texto += "\nGrupo: " + this.GRUPO.DSC_GRUPO;
                            texto += "\nFamília: " + this.FAMILIA_PRODUTO.DSC_FAMILIA_PRODUTO;

                            string sqlQuery =
                                "SELECT DSTEXTOWEB " +
                                "  FROM BRK_EPRODUTO " +
                                " WHERE CDPRD = ? ";

                            int produto = this.COD_PRODUTO;
                            if (produto < 41000)
                                produto += 410000;

                            SQLiteParameter pCOD_PRODUTO = new SQLiteParameter("@CDPRD", produto.ToString().PadLeft(18, '0'));

                            object result = CSDataAccess.Instance.ExecuteScalar(sqlQuery, pCOD_PRODUTO);

                            //if (result != null && result != DBNull.Value)
                            string detalheProduto = DetalheDoProduto();
                            if (!string.IsNullOrEmpty(detalheProduto))
                            {
                                texto += "\nDetalhe do Produto: " + result.ToString().Trim();
                            }

                            this.m_POPUP_TEXT = texto;
                        }

                        return this.m_POPUP_TEXT;
                    }
                    catch (Exception ex)
                    {
                        return "";
                    }
                }
            }
            public bool IND_ITEM_CONJUNTO
            {
                get
                {
                    return m_IND_ITEM_CONJUNTO;
                }
                set
                {
                    m_IND_ITEM_CONJUNTO = value;
                }
            }
            public bool IND_LIBERAR_CONDICAO_PAGAMENTO
            {
                get
                {
                    return m_IND_LIBERAR_CONDICAO_PAGAMENTO;
                }
                set
                {
                    m_IND_LIBERAR_CONDICAO_PAGAMENTO = value;
                }
            }
            public bool IND_PRODUTO_BLOQUEADO
            {
                get
                {
                    return m_IND_PRODUTO_BLOQUEADO;
                }
                set
                {
                    m_IND_PRODUTO_BLOQUEADO = value;
                }
            }
            public bool IND_ITEM_COMBO
            {
                get
                {
                    return m_IND_ITEM_COMBO;
                }
                set
                {
                    m_IND_ITEM_COMBO = value;
                }
            }
            public int COD_PRODUTO_CONJUNTO
            {
                get
                {
                    return m_COD_PRODUTO_CONJUNTO;
                }
                set
                {
                    m_COD_PRODUTO_CONJUNTO = value;
                }
            }
            public decimal QTD_PRODUTO_COMPOSICAO
            {
                get
                {
                    return m_QTD_PRODUTO_COMPOSICAO;
                }
                set
                {
                    m_QTD_PRODUTO_COMPOSICAO = value;
                }
            }
            public decimal PCT_DESCONTO_PRODUTO_COMPOSICAO
            {
                get
                {
                    return m_PCT_DESCONTO_PRODUTO_COMPOSICAO;
                }
                set
                {
                    m_PCT_DESCONTO_PRODUTO_COMPOSICAO = value;
                }
            }
            public bool IND_PRODUTO_FOCO
            {
                get
                {
                    return m_IND_PRODUTO_FOCO;
                }
                set
                {
                    m_IND_PRODUTO_FOCO = value;
                }
            }

            public bool IND_UTILIZA_QTD_MINIMA
            {
                get
                {
                    return m_IND_UTILIZA_QTD_MINIMA;
                }

                set
                {
                    m_IND_UTILIZA_QTD_MINIMA = value;
                }
            }

            public bool PRODUTO_SUGERIDO_REGRA_FREEZER
            {
                get
                {
                    return m_PRODUTO_SUGERIDO_REGRA_FREEZER;
                }

                set
                {
                    m_PRODUTO_SUGERIDO_REGRA_FREEZER = value;
                }
            }

            public decimal PCT_TAXA_MAX_INDENIZACAO
            {
                get
                {
                    return m_PCT_TAXA_MAX_INDENIZACAO;
                }
                set
                {
                    m_PCT_TAXA_MAX_INDENIZACAO = value;
                }
            }

            public int QTD_PRODUTO_SUGERIDO
            {
                get
                {
                    return m_QTD_PRODUTO_SUGERIDO;
                }
                set
                {
                    m_QTD_PRODUTO_SUGERIDO = value;
                }
            }

            public decimal QTD_GIRO_SELLOUT
            {
                get
                {
                    return m_QTD_GIRO_SELLOUT;
                }
                set
                {
                    m_QTD_GIRO_SELLOUT = value;
                }
            }

            public decimal QTD_GIRO_MEDIO
            {
                get
                {
                    return m_QTD_GIRO_MEDIO;
                }
                set
                {
                    m_QTD_GIRO_MEDIO = value;
                }
            }

            #endregion

            #region [ Metodos ]

            public CSProduto()
            {
                try
                {
                    // Adiciona o Evento na classe de mudanca do PDV
                    CSPDVs.OnChangePDV += new AvanteSales.CSPDVs.ChangePDV(CSPDVs_OnChangePDV);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            public string DetalheDoProduto()
            {
                try
                {
                    string sqlQuery =
                                        "SELECT DSTEXTOWEB " +
                                        "  FROM BRK_EPRODUTO " +
                                        " WHERE CDPRD = ? ";

                    int produto = this.COD_PRODUTO;
                    if (produto < 41000)
                        produto += 410000;

                    SQLiteParameter pCOD_PRODUTO = new SQLiteParameter("@CDPRD", produto.ToString().PadLeft(18, '0'));

                    object result = CSDataAccess.Instance.ExecuteScalar(sqlQuery, pCOD_PRODUTO);

                    if (result != null && result != DBNull.Value)
                    {
                        return result.ToString().Trim();
                    }
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    throw new Exception("Falha ao buscar detalhe do produto", ex);
                }
            }

            /// <summary>
            /// Converte a quantidade de venda em caixas
            /// </summary>
            /// <param name="qtdUni">Quantidade vendida unitaria</param>
            /// <param name="uniMdd">Unidade de medida</param>
            /// <param name="qtdUniCaixa">Quantidade de itens dentro da caixa</param>
            /// <returns></returns>
            public static int ConverterParaQuantidadeCaixaVendida(decimal qtdUni, string uniMdd, int qtdUniCaixa)
            {
                int qtdCaixa = 0;

                decimal QtdInteira = 0;
                int QtdUnidades = 0;

                QtdUnidades = Convert.ToInt32(qtdUni % qtdUniCaixa);

                if (uniMdd != "KG" && uniMdd != "LT" && uniMdd != "MT" && uniMdd != null)
                    QtdInteira = Convert.ToInt32((qtdUni - QtdUnidades) / qtdUniCaixa);
                else
                    QtdInteira = qtdUni;

                qtdCaixa = Convert.ToInt32(QtdInteira);


                return qtdCaixa;
            }
            /// <summary>
            /// Converte quantidade em unidades para Unidade de Medida do Produto
            /// </summary>
            /// <returns></returns>
            public static string ConverteUnidadesParaMedida(decimal QtdUni, string UniMdd, int QtdUniCxa)
            {
                string Mascara = "";
                try
                {
                    string Sinal = "";
                    decimal QtdInteira = 0;
                    int QtdUnidades = 0;
                    if (QtdUni < 0)
                        Sinal = "-";

                    //QtdUnidades = Convert.ToInt32((Math.Round(QtdUni) - (QtdInteira * QtdUniCxa)));
                    QtdUnidades = Convert.ToInt32(QtdUni % QtdUniCxa);

                    if (UniMdd != "KG" && UniMdd != "LT" && UniMdd != "MT" && UniMdd != null)
                        QtdInteira = Convert.ToInt32((QtdUni - QtdUnidades) / QtdUniCxa);
                    else
                        QtdInteira = QtdUni;

                    switch (UniMdd)
                    {
                        case "DZ":
                        case "CX":
                            Mascara = Math.Abs(QtdInteira) + "/" + (Math.Abs(QtdUnidades)).ToString("000");
                            break;
                        case "UN":
                        case "BJ":
                        case "CT":
                        case "DY":
                        case "EM":
                        case "FD":
                        case "PC":
                        case "TR":
                            Mascara = (Convert.ToInt32(Math.Abs(QtdInteira))).ToString("########0");
                            break;
                        case "KG":
                        case "LT":
                        case "MT":
                            Mascara = Math.Abs(QtdInteira).ToString(CSGlobal.DecimalStringFormat);
                            break;
                        default:
                            Mascara = Math.Abs(QtdUni).ToString();
                            break;
                    }
                    //Adiciona o SINAL antes o codigo acima era acim  Mascara + Sinal dava erro no estoque 
                    //ficava assim a quantidade 6-/00 dava erro formatexception ao salvar o pedido
                    //o correto e -6/00
                    Mascara = Sinal + Mascara;

                }
                catch (Exception ex)
                {
                    CSGlobal.ShowMessage(ex.ToString());
                }

                // retorno do valor
                return Mascara;
            }

            public static int GetUnidadesPorCaixa(string unidadeMedida, int unidadeEmbalagem)
            {
                switch (unidadeMedida)
                {
                    case "DZ":
                    case "CX":
                        return unidadeEmbalagem;
                    default:
                        return 1;
                }
            }

            public int QuantidadeVendida(CSProdutos.CSProduto produto)
            {
                try
                {
                    int qtd = 0;

                    string sql = string.Format(@"SELECT ITEM_PEDIDO.QTD_PEDIDA / PRODUTO_CONJUNTO.QTD_PRODUTO_COMPOSICAO FROM ITEM_PEDIDO
                                                       JOIN PEDIDO       
                                                            ON PEDIDO.COD_PEDIDO = ITEM_PEDIDO.COD_PEDIDO            
                                                       JOIN PRODUTO_CONJUNTO       
                                                            ON PRODUTO_CONJUNTO.COD_PRODUTO_CONJUNTO = ITEM_PEDIDO.COD_ITEM_COMBO
                                                WHERE ITEM_PEDIDO.COD_ITEM_COMBO = {1}
                                                      AND PEDIDO.IND_HISTORICO = 0      
                                                      AND PEDIDO.COD_PDV = {0}
                                                GROUP BY ITEM_PEDIDO.COD_PEDIDO", CSPDVs.Current.COD_PDV, produto.COD_PRODUTO);

                    using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql))
                    {
                        while (sqlReader.Read())
                        {
                            qtd += sqlReader.GetInt32(0);
                        }
                    }
                    return qtd;
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }

            /// <summary>
            /// Retorna ultimas quantidade de venda do antepenúltimo, penúltimo, último e média do produto
            /// </summary>
            /// <param name="produto"></param>
            /// <returns></returns>
            public decimal[] GetRetornaUltimasQuantidadeVendidaProdutoQ(int produto)
            {
                StringBuilder sqlQuery = null;
                int contador = 1;
                int ocorrencia = 0;
                decimal[] aUltimasVendas = new decimal[4];

                aUltimasVendas[0] = -1;

                try
                {
                    sqlQuery = new StringBuilder();

                    sqlQuery.Length = 0;
                    sqlQuery.Append("  SELECT T2.QTD_PEDIDA ");
                    sqlQuery.Append("    FROM PEDIDO T1 INNER JOIN ITEM_PEDIDO T2 ");
                    sqlQuery.Append("                      ON T1.COD_PEDIDO = T2.COD_PEDIDO ");
                    sqlQuery.Append("                   INNER JOIN OPERACAO T3 ");
                    sqlQuery.Append("                      ON T3.COD_OPERACAO = T1.COD_OPERACAO ");
                    sqlQuery.Append("    WHERE T1.IND_HISTORICO = 1 ");
                    sqlQuery.Append("      AND T3.COD_OPERACAO_CFO IN (1, 21) ");
                    sqlQuery.Append("      AND T1.COD_EMPREGADO = ? ");
                    sqlQuery.Append("      AND T1.COD_PDV = ? ");
                    sqlQuery.Append("      AND T2.COD_PRODUTO = ? ");
                    sqlQuery.Append(" ORDER BY T1.DAT_PEDIDO DESC ");

                    SQLiteParameter[] parametros = new SQLiteParameter[03];

                    parametros[00] = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                    parametros[01] = new SQLiteParameter("@COD_PDV", CSPDVs.Current.COD_PDV);
                    parametros[02] = new SQLiteParameter("@COD_PRODUTO", produto);

                    using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(false, sqlQuery.ToString(), parametros))
                    {
                        // Retorna ultimas vendas
                        while (reader.Read())
                        {
                            aUltimasVendas[contador] = (reader.GetValue(0) == System.DBNull.Value) ? 0 : Convert.ToDecimal(reader.GetValue(0));
                            ocorrencia++;

                            if (contador == 3)
                                break;

                            contador++;

                        }

                        // Fecha o reader
                        reader.Close();
                        reader.Dispose();

                        // Média dos últimos pedidos
                        if (ocorrencia > 0)
                            aUltimasVendas[0] = (aUltimasVendas[1] + aUltimasVendas[2] + aUltimasVendas[3]) / ocorrencia;
                        else
                            aUltimasVendas[0] = 0;

                    }

                    return aUltimasVendas;

                }
                catch (Exception ex)
                {
                    throw new Exception("Erro na busca ultimas quantidade de venda do antepenúltimo, penúltimo, último e média do produto", ex);
                }
            }

            /// <summary>
            /// Retorna se a tabela de preço relacionada ao Combo está cadastrada na tabela "Tabela_preço"
            /// </summary>
            /// <param name="produto"></param>
            /// <returns></returns>
            public List<string> ValidaTabelaPrecoCombo(int CodigoProduto)
            {
                try
                {
                    List<string> list = new List<string>();
                    string linha;
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine("SELECT  PRODUTO_CONJUNTO.COD_PRODUTO_COMPOSICAO ");
                    sql.AppendLine("       ,PRODUTO_CONJUNTO.COD_TABELA_PRECO ");
                    sql.AppendLine("FROM PRODUTO_CONJUNTO ");
                    sql.AppendLine("LEFT JOIN TABELA_PRECO_PRODUTO ");
                    sql.AppendLine("     ON PRODUTO_CONJUNTO.COD_PRODUTO_COMPOSICAO = TABELA_PRECO_PRODUTO.COD_PRODUTO ");
                    sql.AppendLine("     AND PRODUTO_CONJUNTO.COD_TABELA_PRECO = TABELA_PRECO_PRODUTO.[COD_TABELA_PRECO] ");
                    sql.AppendFormat("WHERE PRODUTO_CONJUNTO.COD_PRODUTO_CONJUNTO = {0} ", CodigoProduto);
                    sql.AppendLine("      AND TABELA_PRECO_PRODUTO.COD_PRODUTO IS NULL ");

                    using (SqliteDataReader reader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
                    {
                        while (reader.Read())
                        {
                            string codProduto = reader.GetValue(0) == System.DBNull.Value ? "0" : reader.GetValue(0).ToString();
                            string codTabPreco = reader.GetValue(1) == System.DBNull.Value ? "0" : reader.GetValue(1).ToString();

                            linha = string.Empty;
                            linha = string.Format("{0}({1})", codProduto, codTabPreco);

                            list.Add(linha);
                        }
                    }

                    return list;
                }
                catch (Exception ex)
                {
                    throw new Exception("Erro ao validar tabela de preço.");
                }
            }

            /// <summary>
            /// Retorna Giro do produto
            /// </summary>
            /// <param name="produto"></param>
            /// <returns></returns>
            public decimal[] GetRetornaGiroProduto(int produto)
            {
                StringBuilder sqlQuery = null;
                decimal[] aGiroProduto = new decimal[5];

                aGiroProduto[0] = -1;

                try
                {
                    sqlQuery = new StringBuilder();

                    sqlQuery.Length = 0;
                    sqlQuery.Append("  SELECT T1.QTD_COLETADA  ");
                    sqlQuery.Append("    FROM PRODUTO_COLETA_ESTOQUE T1 ");
                    sqlQuery.Append("   WHERE T1.IND_HISTORICO = 1 ");
                    sqlQuery.Append("     AND T1.COD_EMPREGADO = ? ");
                    sqlQuery.Append("     AND T1.COD_PDV = ? ");
                    sqlQuery.Append("     AND T1.COD_PRODUTO = ? ");

                    SQLiteParameter[] parametros1 = new SQLiteParameter[03];

                    parametros1[00] = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                    parametros1[01] = new SQLiteParameter("@COD_PDV", CSPDVs.Current.COD_PDV);
                    parametros1[02] = new SQLiteParameter("@COD_PRODUTO", produto);

                    using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(false, sqlQuery.ToString(), parametros1))
                    {
                        // Retorna ultimas coleta
                        while (reader.Read())
                        {
                            aGiroProduto[1] = (reader.GetValue(0) == System.DBNull.Value) ? 0 : Convert.ToDecimal(reader.GetValue(0));
                            break;
                        }

                        // Fecha o reader
                        reader.Close();
                        reader.Dispose();
                    }

                    sqlQuery.Length = 0;
                    sqlQuery.Append("  SELECT T2.QTD_PEDIDA ");

                    if (CSGlobal.PedidoSugerido ||
                        CSGlobal.PesquisarComoSugerido ||
                        CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                        sqlQuery.Append("  ,DATE(T1.DAT_PEDIDO) ");

                    sqlQuery.Append("    FROM PEDIDO T1 INNER JOIN ITEM_PEDIDO T2 ");
                    sqlQuery.Append("                      ON T1.COD_PEDIDO = T2.COD_PEDIDO ");
                    sqlQuery.Append("                   INNER JOIN OPERACAO T3 ");
                    sqlQuery.Append("                      ON T3.COD_OPERACAO = T1.COD_OPERACAO ");
                    sqlQuery.Append("    WHERE T1.IND_HISTORICO = 1 ");
                    sqlQuery.Append("      AND T3.COD_OPERACAO_CFO IN (1, 21) ");
                    sqlQuery.Append("      AND T1.COD_EMPREGADO = ? ");
                    sqlQuery.Append("      AND T1.COD_PDV = ? ");
                    sqlQuery.Append("      AND T2.COD_PRODUTO = ? ");

                    if (CSGlobal.PedidoSugerido ||
                        CSGlobal.PesquisarComoSugerido ||
                        CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                        sqlQuery.Append("    AND T2.QTD_PEDIDA > 0 ");

                    sqlQuery.Append(" ORDER BY T1.DAT_PEDIDO DESC ");

                    SQLiteParameter[] parametros2 = new SQLiteParameter[03];

                    parametros2[00] = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                    parametros2[01] = new SQLiteParameter("@COD_PDV", CSPDVs.Current.COD_PDV);
                    parametros2[02] = new SQLiteParameter("@COD_PRODUTO", produto);

                    using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(false, sqlQuery.ToString(), parametros2))
                    {
                        // Retorna ultima venda
                        while (reader.Read())
                        {
                            aGiroProduto[2] = (reader.GetValue(0) == System.DBNull.Value) ? 0 : Convert.ToDecimal(reader.GetValue(0));

                            if (CSGlobal.PedidoSugerido ||
                                CSGlobal.PesquisarComoSugerido ||
                                CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                            {
                                TimeSpan Diferenca = DateTime.Now - reader.GetDateTime(1);

                                aGiroProduto[4] = Convert.ToDecimal(Diferenca.Days);
                            }

                            break;
                        }

                        // Fecha o reader
                        reader.Close();
                        reader.Dispose();
                    }

                    sqlQuery.Length = 0;
                    sqlQuery.Append("  SELECT T1.QTD_COLETADA  ");
                    sqlQuery.Append("    FROM PRODUTO_COLETA_ESTOQUE T1 ");
                    sqlQuery.Append("   WHERE T1.IND_HISTORICO = 0 ");
                    sqlQuery.Append("     AND T1.COD_EMPREGADO = ? ");
                    sqlQuery.Append("     AND T1.COD_PDV = ? ");
                    sqlQuery.Append("     AND T1.COD_PRODUTO = ? ");

                    SQLiteParameter[] parametros3 = new SQLiteParameter[03];

                    parametros3[00] = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                    parametros3[01] = new SQLiteParameter("@COD_PDV", CSPDVs.Current.COD_PDV);
                    parametros3[02] = new SQLiteParameter("@COD_PRODUTO", produto);

                    using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(false, sqlQuery.ToString(), parametros3))
                    {
                        // Retorna coleta atual
                        while (reader.Read())
                        {
                            aGiroProduto[3] = (reader.GetValue(0) == System.DBNull.Value) ? 0 : Convert.ToDecimal(reader.GetValue(0));
                            break;
                        }

                        // Fecha o reader
                        reader.Close();
                        reader.Dispose();
                    }

                    // Cálculo do giro do produto
                    if (aGiroProduto[3] <= (aGiroProduto[1] + aGiroProduto[2]))
                        aGiroProduto[0] = (aGiroProduto[1] + aGiroProduto[2]) - aGiroProduto[3];
                    else
                        aGiroProduto[0] = 0;

                    // Estoque anterior ou atual for zero, o giro será zero;
                    if (aGiroProduto[1] == 0 || aGiroProduto[3] == 0)
                        aGiroProduto[0] = 0;


                    return aGiroProduto;

                }
                catch (Exception ex)
                {
                    throw new Exception("Erro no cálculo do Giro do produto", ex);
                }

            }
            /// <summary>
            /// Retorna Produto Sugerido
            /// </summary>
            /// <param name="produto"></param>
            /// <returns></returns>
            public decimal[] GetRetornaProdutoSugerido(int produto)
            {
                StringBuilder sqlQuery = null;
                decimal[] aProdutoSugerido = new decimal[3];

                int valorAuxiliar = 0;
                aProdutoSugerido[0] = 0;
                aProdutoSugerido[1] = 0;
                aProdutoSugerido[2] = 0;

                try
                {
                    sqlQuery = new StringBuilder();

                    sqlQuery.Length = 0;
                    sqlQuery.Append("  SELECT T1.QTD_MAIOR_VENDA  ");
                    sqlQuery.Append("    FROM PDV_PRODUTO_MAIOR_VENDA T1 ");
                    sqlQuery.Append("   WHERE T1.COD_EMPREGADO = ? ");
                    sqlQuery.Append("     AND T1.COD_PDV = ? ");
                    sqlQuery.Append("     AND T1.COD_PRODUTO = ? ");

                    SQLiteParameter[] parametros1 = new SQLiteParameter[03];

                    parametros1[00] = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                    parametros1[01] = new SQLiteParameter("@COD_PDV", CSPDVs.Current.COD_PDV);
                    parametros1[02] = new SQLiteParameter("@COD_PRODUTO", produto);

                    using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(false, sqlQuery.ToString(), parametros1))
                    {
                        // Retorna Maior Venda
                        while (reader.Read())
                        {
                            aProdutoSugerido[0] = (reader.GetValue(0) == System.DBNull.Value) ? 0 : Convert.ToDecimal(reader.GetValue(0));
                            break;
                        }

                        // Fecha o reader
                        reader.Close();
                        reader.Dispose();
                    }

                    sqlQuery.Length = 0;
                    sqlQuery.Append("  SELECT T1.QTD_COLETADA  ");
                    sqlQuery.Append("    FROM PRODUTO_COLETA_ESTOQUE T1 ");
                    sqlQuery.Append("   WHERE T1.IND_HISTORICO = 0 ");
                    sqlQuery.Append("     AND T1.COD_EMPREGADO = ? ");
                    sqlQuery.Append("     AND T1.COD_PDV = ? ");
                    sqlQuery.Append("     AND T1.COD_PRODUTO = ? ");

                    SQLiteParameter[] parametros2 = new SQLiteParameter[03];

                    parametros2[00] = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                    parametros2[01] = new SQLiteParameter("@COD_PDV", CSPDVs.Current.COD_PDV);
                    parametros2[02] = new SQLiteParameter("@COD_PRODUTO", produto);

                    using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(false, sqlQuery.ToString(), parametros2))
                    {
                        // Retorna coleta atual
                        while (reader.Read())
                        {
                            aProdutoSugerido[1] = (reader.GetValue(0) == System.DBNull.Value) ? 0 : Convert.ToDecimal(reader.GetValue(0));
                            break;
                        }

                        // Fecha o reader
                        reader.Close();
                        reader.Dispose();
                    }

                    // Cálculo do pedido sugerido
                    // Estoque coletado igual a zero - Margem segurança 20% sobre a maior venda
                    if (aProdutoSugerido[1] == 0)
                    {
                        valorAuxiliar = (int)aProdutoSugerido[0];
                        aProdutoSugerido[2] = (int)(valorAuxiliar * 1.2);
                    }
                    else
                        if (aProdutoSugerido[0] >= aProdutoSugerido[1])
                        aProdutoSugerido[2] = aProdutoSugerido[0] - aProdutoSugerido[1];
                    else
                        aProdutoSugerido[2] = 0;

                    return aProdutoSugerido;

                }
                catch (Exception ex)
                {
                    throw new Exception("Erro no cálculo do produto sugerido", ex);
                }
            }
            /// <summary>
            /// Salva os dados na coleção no banco
            /// </summary>
            public void Flush()
            {
                try
                {
                    string sqlQueryUpdate = "";

                    /* Atualiza Saldo do Estoque */
                    sqlQueryUpdate = "UPDATE PRODUTO " +
                                     "   SET QTD_ESTOQUE = ? " +
                                     " WHERE COD_PRODUTO = ? ";

                    // Criar os parametros de salvamento
                    SQLiteParameter pQTD_ESTOQUE = new SQLiteParameter("@QTD_ESTOQUE", this.QTD_ESTOQUE);
                    pQTD_ESTOQUE.DbType = DbType.Decimal;

                    SQLiteParameter pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", this.COD_PRODUTO);
                    pCOD_PRODUTO.DbType = DbType.Int32;

                    // Executa a query salvando os dados
                    CSDataAccess.Instance.ExecuteScalar(sqlQueryUpdate, pQTD_ESTOQUE, pCOD_PRODUTO);

                    // Criar os parametros de salvamento do estoque de pronta entrega
                    SQLiteParameter pQTD_ESTOQUE_PRONTA_ENTREGA = new SQLiteParameter("@QTD_ESTOQUE_PRONTA_ENTREGA", this.QTD_ESTOQUE_PRONTA_ENTREGA);
                    pQTD_ESTOQUE_PRONTA_ENTREGA.DbType = DbType.Decimal;

                    SQLiteParameter pCOD_PRODUTO2 = new SQLiteParameter("@COD_PRODUTO2", this.COD_PRODUTO);
                    pCOD_PRODUTO2.DbType = DbType.Int32;

                    SQLiteParameter pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                    pCOD_EMPREGADO.DbType = DbType.Int32;

                    sqlQueryUpdate = " UPDATE SALDO_PRONTA_ENTREGA_ITEM  " +
                                     "    SET QTD_DISPONIVEL = ? " +
                                     "  WHERE COD_PRODUTO    = ? " +
                                     "    AND COD_EMPREGADO  = ? ";

                    // Executa a query salvando os dados do estoque de pronta entrega
                    CSDataAccess.Instance.ExecuteScalar(sqlQueryUpdate, pQTD_ESTOQUE_PRONTA_ENTREGA, pCOD_PRODUTO2, pCOD_EMPREGADO);


                }
                catch (Exception ex)
                {
                    CSGlobal.ShowMessage("Erro no flush de Itens (Estoque)");
                    throw new Exception("Erro no flush de Itens (Estoque)", ex);
                }
            }

            #endregion

            #region [ SubClasses ]

            public class CSPrecosProdutos : CollectionBase
            {
                #region [ Variaveis ]

                private CSPrecosProdutos.CSPrecoProduto m_Current;
                public static SQLiteCommand sqlCommand = null;

                #endregion

                #region [ Propriedades ]

                /// <summary>
                /// Retorna coleção dos preços produtos
                /// </summary>
                public CSPrecosProdutos Items
                {
                    get
                    {
                        return this;
                    }
                }

                public CSPrecosProdutos.CSPrecoProduto this[int Index]
                {
                    get
                    {
                        return (CSPrecosProdutos.CSPrecoProduto)this.InnerList[Index];
                    }
                }

                /// <summary>
                /// Guarda qual o preço atual selecionado
                /// </summary>
                public CSPrecosProdutos.CSPrecoProduto Current
                {
                    get
                    {
                        return m_Current;
                    }
                    set
                    {
                        m_Current = value;
                    }
                }

                #endregion

                #region [ Metodos ]

                /// <summary>
                /// Contrutor da classe. Busca os produtos
                /// </summary>
                public CSPrecosProdutos(int COD_PRODUTO)
                {
                    SQLiteDataReader reader = null;
                    SQLiteParameter pCOD_PRODUTO = null;
                    SQLiteParameter pCOD_EMPREGADO = null;
                    CSPoliticaBroker.TmpPricingCons[] valorBroker = null;
                    CSPoliticaBroker2014.TmpPricingCons[] valorBroker2014 = null;

                    try
                    {
                        // Query de busca dos preços dos produtos
                        string sqlQuery =
                            "SELECT T2.COD_TABELA_PRECO, T2.DSC_TABELA_PRECO, T1.QTD_MINIMA_PEDIDA " +
                            "      ,T1.QTD_MAXIMA_PEDIDA, T3.PRC_ACRESCIMO_QTDE_UNITARIA, T1.BOL_QTDUNITARIA_MULTIPLA_MINIMO " +
                            "      ,T1.COD_TRIBUTACAO, T1.VLR_PRODUTO, T1.VLR_VERBA_EXTRA " +
                            "  FROM TABELA_PRECO_PRODUTO T1 " +
                            "  JOIN TABELA_PRECO T2 ON T1.COD_TABELA_PRECO = T2.COD_TABELA_PRECO " +
                            "  JOIN PRODUTO T3 ON T1.COD_PRODUTO = T3.COD_PRODUTO " +
                            "  LEFT JOIN BLOQUEIO_TABELA_PRECO T4 ON T2.COD_TABELA_PRECO = T4.COD_TABELA_PRECO " +
                            "   AND T4.COD_TABELA_BLOQUEIO = 7 AND T4.TIPO_BLOQUEIO = 'B' AND T4.COD_BLOQUEIO = ? " +
                            " WHERE T1.COD_PRODUTO = ? " +
                            "   AND T1.VLR_PRODUTO > 0 " +
                            "   AND T4.COD_TABELA_PRECO IS NULL " +
                            " ORDER BY T2.COD_TABELA_PRECO ";

                        // Cria os parametros 
                        pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                        pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", COD_PRODUTO);
                        pCOD_PRODUTO.DbType = DbType.Int32;

                        //[ Se política broker... ]
                        if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2)
                        {
                            // [ Recupera produto solicitado ]
                            CSProdutos.CSProduto produto = CSProdutos.GetProduto(COD_PRODUTO);

                            // [ Realiza o cálculo do preço padrão ]
                            if (CSEmpresa.Current.IND_UTILIZA_PRICE_2014)
                                valorBroker2014 = CSPDVs.Current.POLITICA_BROKER_2014.CalculaPreco(produto.COD_PRODUTO, produto.GRUPO_COMERCIALIZACAO.COD_SETOR_BROKER, 1, 0, 0, produto.QTD_UNIDADE_EMBALAGEM);
                            else
                                valorBroker = CSPDVs.Current.POLITICA_BROKER.CalculaPreco(produto.COD_PRODUTO, produto.GRUPO_COMERCIALIZACAO.COD_SETOR_BROKER, 1, 0, 0, produto.QTD_UNIDADE_EMBALAGEM);

                            // [ Preço não encontrado ]

                            if (CSEmpresa.Current.IND_UTILIZA_PRICE_2014)
                            {
                                if (valorBroker2014 == null)
                                    return;
                            }
                            else
                            {
                                if (valorBroker == null)
                                    return;
                            }
                        }

                        // Controle do Boleano para realizar o Prepare do SQL.
                        using (reader = CSDataAccess.Instance.ExecuteReader(ref sqlCommand, sqlQuery, pCOD_EMPREGADO, pCOD_PRODUTO))
                        {
                            while (reader.Read())
                            {
                                CSPrecoProduto preco = new CSPrecoProduto();

                                // Preenche a instancia da classe dos preços dos produtos
                                preco.COD_TABELA_PRECO = reader.GetValue(0) == System.DBNull.Value ? -1 : Convert.ToInt32(reader.GetValue(0));
                                preco.DSC_TABELA_PRECO = reader.GetValue(1) == System.DBNull.Value ? "" : reader.GetString(1);
                                preco.QTD_MINIMA_PEDIDA = reader.GetValue(2) == System.DBNull.Value ? -1 : Convert.ToDecimal(reader.GetValue(2));
                                preco.QTD_MAXIMA_PEDIDA = reader.GetValue(3) == System.DBNull.Value ? -1 : Convert.ToDecimal(reader.GetValue(3));
                                preco.PRC_ACRESCIMO_QTDE_UNITARIA = reader.GetValue(4) == System.DBNull.Value ? -1 : Convert.ToDecimal(reader.GetValue(4));
                                preco.BOL_QTDUNITARIA_MULTIPLA_MINIMO = reader.GetValue(5) == System.DBNull.Value ? false : reader.GetBoolean(5);

                                //[ Se política broker... ]
                                if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2)
                                {
                                    preco.COD_TRIBUTACAO = 0;

                                    if (CSEmpresa.Current.IND_UTILIZA_PRICE_2014)
                                        preco.VLR_PRODUTO = valorBroker2014[valorBroker2014.Length - 1].VALOR;
                                    else
                                        preco.VLR_PRODUTO = valorBroker[valorBroker.Length - 1].VALOR;

                                }
                                else
                                {
                                    preco.COD_TRIBUTACAO = reader.GetValue(6) == System.DBNull.Value ? -1 : reader.GetInt32(6);
                                    preco.VLR_PRODUTO = reader.GetValue(7) == System.DBNull.Value ? -1 : reader.GetDecimal(7);
                                }

                                preco.VLR_VERBA_EXTRA = reader.GetValue(8) == System.DBNull.Value ? 0 : reader.GetDecimal(8);

                                // Adcionao o preço produto na coleção dos preços do produto
                                base.InnerList.Add(preco);
                            }
                            // Fecha o reader
                            reader.Close();
                            reader.Dispose();
                        }

                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Erro na busca dos preços dos produtos.", ex);
                    }
                }

                public CSPrecosProdutos.CSPrecoProduto GetPrecoProduto(int COD_TABELA_PRECO)
                {
                    // Procura pelo precp tabela
                    foreach (CSPrecosProdutos.CSPrecoProduto prc in this.InnerList)
                    {
                        if (prc.COD_TABELA_PRECO == COD_TABELA_PRECO)
                        {
                            return prc;
                        }
                    }
                    // retorna o objeto
                    return null;
                }

                #endregion

                #region [ SubClasses ]

                /// <summary>
                /// Guarda a informação sobre o preço do produto
                /// </summary>
                public class CSPrecoProduto
                {
                    #region [ Variaveis ]

                    private int m_COD_TABELA_PRECO;
                    private string m_DSC_TABELA_PRECO;
                    private decimal m_VLR_PRODUTO;
                    private decimal m_QTD_MINIMA_PEDIDA;
                    private decimal m_QTD_MAXIMA_PEDIDA;
                    private int m_COD_TRIBUTACAO;
                    private decimal m_PRC_ACRESCIMO_QTDE_UNITARIA;
                    private bool m_BOL_QTDUNITARIA_MULTIPLA_MINIMO;
                    private decimal m_VLR_VERBA_EXTRA;

                    #endregion

                    #region [ Propriedades ]

                    public int COD_TABELA_PRECO
                    {
                        get
                        {
                            return m_COD_TABELA_PRECO;
                        }
                        set
                        {
                            m_COD_TABELA_PRECO = value;
                        }
                    }

                    public string DSC_TABELA_PRECO
                    {
                        get
                        {
                            return m_DSC_TABELA_PRECO;
                        }
                        set
                        {
                            m_DSC_TABELA_PRECO = value;
                        }
                    }

                    public decimal VLR_PRODUTO
                    {
                        get
                        {
                            return m_VLR_PRODUTO;
                        }
                        set
                        {
                            m_VLR_PRODUTO = value;
                        }
                    }

                    public decimal QTD_MINIMA_PEDIDA
                    {
                        get
                        {
                            return m_QTD_MINIMA_PEDIDA;
                        }
                        set
                        {
                            m_QTD_MINIMA_PEDIDA = value;
                        }
                    }

                    public decimal QTD_MAXIMA_PEDIDA
                    {
                        get
                        {
                            return m_QTD_MAXIMA_PEDIDA;
                        }
                        set
                        {
                            m_QTD_MAXIMA_PEDIDA = value;
                        }
                    }

                    public int COD_TRIBUTACAO
                    {
                        get
                        {
                            return m_COD_TRIBUTACAO;
                        }
                        set
                        {
                            m_COD_TRIBUTACAO = value;
                        }
                    }

                    public decimal PRC_ACRESCIMO_QTDE_UNITARIA
                    {
                        get
                        {
                            return m_PRC_ACRESCIMO_QTDE_UNITARIA;
                        }
                        set
                        {
                            m_PRC_ACRESCIMO_QTDE_UNITARIA = value;
                        }
                    }

                    public bool BOL_QTDUNITARIA_MULTIPLA_MINIMO
                    {
                        get
                        {
                            return m_BOL_QTDUNITARIA_MULTIPLA_MINIMO;
                        }
                        set
                        {
                            m_BOL_QTDUNITARIA_MULTIPLA_MINIMO = value;
                        }
                    }

                    public decimal VLR_VERBA_EXTRA
                    {
                        get
                        {
                            return m_VLR_VERBA_EXTRA;
                        }
                        set
                        {
                            m_VLR_VERBA_EXTRA = value;
                        }
                    }

                    #endregion

                    #region [ Metodos ]

                    public CSPrecoProduto()
                    {

                    }

                    public string[] GetRetornaBloqueios(CSPDVs.CSPDV pdv, CSPedidosPDV.CSPedidoPDV pdd)
                    {
                        try
                        {
                            StringBuilder sqlQuery = new StringBuilder();
                            sqlQuery.AppendLine("SELECT COD_TABELA_BLOQUEIO,COD_BLOQUEIO,COD_SUB_GRUPO_TABELA_BLOQUEIO,TIPO_BLOQUEIO ");

                            if (CSEmpresa.ColunaExiste("BLOQUEIO_TABELA_PRECO", "COD_UF"))
                                sqlQuery.AppendLine(",COD_UF");

                            sqlQuery.AppendLine("  FROM BLOQUEIO_TABELA_PRECO ");
                            sqlQuery.AppendFormat(" WHERE COD_TABELA_PRECO = {0}", CSProdutos.Current.PRECOS_PRODUTO.Current.COD_TABELA_PRECO.ToString());
                            sqlQuery.AppendLine(" ORDER BY COD_TABELA_BLOQUEIO,COD_BLOQUEIO");

                            string[] aBlqTabPre = new string[9];
                            aBlqTabPre[0] = "";

                            // Busca todos os bloqueios de tabela de preço configurados
                            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString()))
                            {
                                while (sqlReader.Read())
                                {
                                    switch (sqlReader.GetInt32(0))
                                    {
                                        case 1:
                                            {
                                                if (sqlReader.GetInt32(1) == pdd.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO)
                                                {
                                                    if (sqlReader.GetString(3) == "B")
                                                    {
                                                        aBlqTabPre[0] = "B";
                                                        aBlqTabPre[1] = "Condição de pagamento bloqueada nesta tabela";

                                                    }
                                                    else
                                                    {
                                                        aBlqTabPre[1] = "Condição pagamento com advertência nesta tabela";

                                                        if (aBlqTabPre[0] == "")
                                                            aBlqTabPre[0] = "A";
                                                    }
                                                }
                                                break;
                                            }

                                        case 2:
                                            {
                                                if (sqlReader.GetInt32(1) == pdv.COD_CATEGORIA)
                                                {
                                                    if (sqlReader.GetString(3) == "B")
                                                    {
                                                        aBlqTabPre[0] = "B";
                                                        aBlqTabPre[2] = "Categoria do cliente bloqueada nesta tabela";

                                                    }
                                                    else
                                                    {
                                                        aBlqTabPre[2] = "Categoria do cliente com advertência nesta tabela";

                                                        if (aBlqTabPre[0] == "")
                                                            aBlqTabPre[0] = "A";
                                                    }
                                                }
                                                break;
                                            }

                                        case 3:
                                            {
                                                if (sqlReader.GetInt32(1) == pdv.COD_GRUPO)
                                                {
                                                    if (sqlReader.GetString(3) == "B")
                                                    {
                                                        aBlqTabPre[0] = "B";
                                                        aBlqTabPre[3] = "Grupo do cliente com bloqueio nesta tabela";

                                                    }
                                                    else
                                                    {
                                                        aBlqTabPre[3] = "Grupo do cliente com advertência nesta tabela";

                                                        if (aBlqTabPre[0] == "")
                                                            aBlqTabPre[0] = "A";
                                                    }
                                                }
                                                break;
                                            }

                                        case 4:
                                            {
                                                if (sqlReader.GetInt32(2) == pdv.COD_GRUPO &&
                                                    sqlReader.GetInt32(1) == pdv.COD_CLASSIFICACAO)
                                                {
                                                    if (sqlReader.GetString(3) == "B")
                                                    {
                                                        aBlqTabPre[0] = "B";
                                                        aBlqTabPre[4] = "Classificação cliente com bloqueio nesta tabela";

                                                    }
                                                    else
                                                    {
                                                        aBlqTabPre[4] = "Classificação cliente com advertência nesta tabela";

                                                        if (aBlqTabPre[0] == "")
                                                            aBlqTabPre[0] = "A";
                                                    }
                                                }
                                                break;
                                            }

                                        case 5:
                                            {
                                                if (sqlReader.GetInt32(1) == pdv.COD_SEGMENTACAO)
                                                {
                                                    if (sqlReader.GetString(3) == "B")
                                                    {
                                                        aBlqTabPre[0] = "B";
                                                        aBlqTabPre[5] = "Segmento do cliente com bloqueio nesta tabela";

                                                    }
                                                    else
                                                    {
                                                        aBlqTabPre[5] = "Segmento do cliente com advertência nesta tabela";

                                                        if (aBlqTabPre[0] == "")
                                                            aBlqTabPre[0] = "A";
                                                    }
                                                }
                                                break;
                                            }

                                        case 6:
                                            {
                                                if (sqlReader.GetInt32(1) == pdv.COD_UNIDADE_NEGOCIO)
                                                {
                                                    if (sqlReader.GetString(3) == "B")
                                                    {
                                                        aBlqTabPre[0] = "B";
                                                        aBlqTabPre[6] = "Negócio do cliente com bloqueio nesta tabela";

                                                    }
                                                    else
                                                    {
                                                        aBlqTabPre[6] = "Negócio do cliente com advertência nesta tabela";

                                                        if (aBlqTabPre[0] == "")
                                                            aBlqTabPre[0] = "A";
                                                    }
                                                }
                                                break;
                                            }

                                        case 7:
                                            {
                                                if (sqlReader.GetInt32(1) == pdd.EMPREGADO.COD_EMPREGADO)
                                                {
                                                    if (sqlReader.GetString(3) == "B")
                                                    {
                                                        aBlqTabPre[0] = "B";
                                                        aBlqTabPre[7] = "Vendedor bloqueado nesta tabela";

                                                    }
                                                    else
                                                    {
                                                        aBlqTabPre[7] = "Vendedor com advertência nesta tabela";

                                                        if (aBlqTabPre[0] == "")
                                                            aBlqTabPre[0] = "A";
                                                    }
                                                }
                                                break;
                                            }
                                        case 8:
                                            {
                                                if (CSEmpresa.ColunaExiste("BLOQUEIO_TABELA_PRECO", "COD_UF"))
                                                    sqlQuery.AppendLine(",COD_UF");
                                                {
                                                    if (pdv.ENDERECOS_PDV.Count > 0 &&
                                                        sqlReader.GetInt32(1) == pdv.ENDERECOS_PDV[0].COD_CIDADE &&
                                                        sqlReader.GetString(4) == pdv.ENDERECOS_PDV[0].DSC_UF)
                                                    {
                                                        if (sqlReader.GetString(3) == "B")
                                                        {
                                                            aBlqTabPre[0] = "B";
                                                            aBlqTabPre[8] = "Cidade do cliente bloqueada nesta tabela";

                                                        }
                                                        else
                                                        {
                                                            aBlqTabPre[8] = "Cidade do cliente com advertência nesta tabela";

                                                            if (aBlqTabPre[0] == "")
                                                                aBlqTabPre[0] = "A";
                                                        }
                                                    }
                                                }
                                                break;
                                            }
                                    }
                                }

                                // Fecha o reader
                                sqlReader.Close();
                                sqlReader.Dispose();
                            }

                            return aBlqTabPre;

                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Erro na busca de bloqueios de tabela de preço", ex);
                        }
                    }

                    public string[] GetRetornaBloqueiosProduto(CSPDVs.CSPDV pdv, CSPedidosPDV.CSPedidoPDV pdd, int produto)
                    {
                        try
                        {
                            StringBuilder sqlQuery = new StringBuilder();
                            sqlQuery.AppendLine(" SELECT COD_TABELA_BLOQUEIO ");
                            sqlQuery.AppendLine("       ,COD_BLOQUEIO ");
                            sqlQuery.AppendLine("       ,COD_SUB_GRUPO_TABELA_BLOQUEIO ");
                            sqlQuery.AppendLine("       ,TIPO_BLOQUEIO ");

                            if (CSEmpresa.ColunaExiste("BLOQUEIO_PRODUTO_TABELA_PRECO", "COD_UF"))
                                sqlQuery.AppendLine(",COD_UF");

                            sqlQuery.AppendLine("   FROM BLOQUEIO_PRODUTO_TABELA_PRECO ");
                            sqlQuery.AppendFormat("  WHERE COD_TABELA_PRECO = {0} ", CSProdutos.Current.PRECOS_PRODUTO.Current.COD_TABELA_PRECO.ToString());
                            sqlQuery.AppendFormat("    AND COD_PRODUTO = {0} ", produto.ToString());
                            sqlQuery.AppendLine("  ORDER BY COD_TABELA_BLOQUEIO ");
                            sqlQuery.AppendLine("          ,COD_BLOQUEIO");

                            string[] aBlqTabPre = new string[9];
                            aBlqTabPre[0] = "";

                            // Busca todos os bloqueios do produto configurados
                            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString()))
                            {
                                while (sqlReader.Read())
                                {
                                    switch (sqlReader.GetInt32(0))
                                    {
                                        case 1:
                                            {
                                                if (sqlReader.GetInt32(1) == pdd.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO)
                                                {
                                                    if (sqlReader.GetString(3) == "B")
                                                    {
                                                        aBlqTabPre[0] = "B";
                                                        aBlqTabPre[1] = "Condição de pagamento bloqueada neste produto";

                                                    }
                                                    else
                                                    {
                                                        aBlqTabPre[1] = "Condição pagamento com advertência neste produto";

                                                        if (aBlqTabPre[0] == "")
                                                            aBlqTabPre[0] = "A";
                                                    }
                                                }
                                                break;
                                            }

                                        case 2:
                                            {
                                                if (sqlReader.GetInt32(1) == pdv.COD_CATEGORIA)
                                                {
                                                    if (sqlReader.GetString(3) == "B")
                                                    {
                                                        aBlqTabPre[0] = "B";
                                                        aBlqTabPre[2] = "Categoria do cliente bloqueada neste produto";

                                                    }
                                                    else
                                                    {
                                                        aBlqTabPre[2] = "Categoria do cliente com advertência neste produto";

                                                        if (aBlqTabPre[0] == "")
                                                            aBlqTabPre[0] = "A";
                                                    }
                                                }
                                                break;
                                            }

                                        case 3:
                                            {
                                                if (sqlReader.GetInt32(1) == pdv.COD_GRUPO)
                                                {
                                                    if (sqlReader.GetString(3) == "B")
                                                    {
                                                        aBlqTabPre[0] = "B";
                                                        aBlqTabPre[3] = "Grupo do cliente com bloqueio neste produto";

                                                    }
                                                    else
                                                    {
                                                        aBlqTabPre[3] = "Grupo do cliente com advertência neste produto";

                                                        if (aBlqTabPre[0] == "")
                                                            aBlqTabPre[0] = "A";
                                                    }
                                                }
                                                break;
                                            }

                                        case 4:
                                            {
                                                if (sqlReader.GetInt32(2) == pdv.COD_GRUPO &&
                                                    sqlReader.GetInt32(1) == pdv.COD_CLASSIFICACAO)
                                                {
                                                    if (sqlReader.GetString(3) == "B")
                                                    {
                                                        aBlqTabPre[0] = "B";
                                                        aBlqTabPre[4] = "Classificação cliente com bloqueio neste produto";

                                                    }
                                                    else
                                                    {
                                                        aBlqTabPre[4] = "Classificação cliente com advertência neste produto";

                                                        if (aBlqTabPre[0] == "")
                                                            aBlqTabPre[0] = "A";
                                                    }
                                                }
                                                break;
                                            }

                                        case 5:
                                            {
                                                if (sqlReader.GetInt32(1) == pdv.COD_SEGMENTACAO)
                                                {
                                                    if (sqlReader.GetString(3) == "B")
                                                    {
                                                        aBlqTabPre[0] = "B";
                                                        aBlqTabPre[5] = "Segmento do cliente com bloqueio neste produto";

                                                    }
                                                    else
                                                    {
                                                        aBlqTabPre[5] = "Segmento do cliente com advertência neste produto";

                                                        if (aBlqTabPre[0] == "")
                                                            aBlqTabPre[0] = "A";
                                                    }
                                                }
                                                break;
                                            }

                                        case 6:
                                            {
                                                if (sqlReader.GetInt32(1) == pdv.COD_UNIDADE_NEGOCIO)
                                                {
                                                    if (sqlReader.GetString(3) == "B")
                                                    {
                                                        aBlqTabPre[0] = "B";
                                                        aBlqTabPre[6] = "Negócio do cliente com bloqueio neste produto";

                                                    }
                                                    else
                                                    {
                                                        aBlqTabPre[6] = "Negócio do cliente com advertência neste produto";

                                                        if (aBlqTabPre[0] == "")
                                                            aBlqTabPre[0] = "A";
                                                    }
                                                }
                                                break;
                                            }

                                        case 7:
                                            {
                                                if (sqlReader.GetInt32(1) == pdd.EMPREGADO.COD_EMPREGADO)
                                                {
                                                    if (sqlReader.GetString(3) == "B")
                                                    {
                                                        aBlqTabPre[0] = "B";
                                                        aBlqTabPre[7] = "Vendedor bloqueado neste produto";

                                                    }
                                                    else
                                                    {
                                                        aBlqTabPre[7] = "Vendedor com advertência neste produto";

                                                        if (aBlqTabPre[0] == "")
                                                            aBlqTabPre[0] = "A";
                                                    }
                                                }
                                                break;
                                            }
                                        case 8:
                                            {
                                                if (CSEmpresa.ColunaExiste("BLOQUEIO_PRODUTO_TABELA_PRECO", "COD_UF"))
                                                    sqlQuery.AppendLine(",COD_UF");
                                                {
                                                    if (pdv.ENDERECOS_PDV.Count > 0 &&
                                                        sqlReader.GetInt32(1) == pdv.ENDERECOS_PDV[0].COD_CIDADE &&
                                                        sqlReader.GetString(4) == pdv.ENDERECOS_PDV[0].DSC_UF)
                                                    {
                                                        if (sqlReader.GetString(3) == "B")
                                                        {
                                                            aBlqTabPre[0] = "B";
                                                            aBlqTabPre[8] = "Cidade do cliente bloqueada nesta tabela";

                                                        }
                                                        else
                                                        {
                                                            aBlqTabPre[8] = "Cidade do cliente com advertência nesta tabela";

                                                            if (aBlqTabPre[0] == "")
                                                                aBlqTabPre[0] = "A";
                                                        }
                                                    }
                                                }
                                                break;
                                            }
                                    }
                                }

                                // Fecha o reader
                                sqlReader.Close();
                                sqlReader.Dispose();
                            }

                            return aBlqTabPre;

                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Erro na busca de bloqueios de produto", ex);
                        }
                    }
                    #endregion
                }

                #endregion
            }

            public class CSCategoria
            {
                #region [ Metodos ]

                public CSCategoria()
                {
                }

                public static bool GetEspecificoCategoria(int COD_CATEGORIA, int COD_PRODUTO)
                {
                    try
                    {
                        string sqlQuery =
                            "SELECT IND_PROD_ESPECIFICO_CATEGORIA " +
                            "  FROM PRODUTO_CATEGORIA " +
                            " WHERE COD_PRODUTO = ? " +
                            "   AND COD_CATEGORIA = ? ";

                        SQLiteParameter pCOD_CATEGORIA = new SQLiteParameter("@COD_CATEGORIA", COD_CATEGORIA);
                        SQLiteParameter pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", COD_PRODUTO);

                        bool result = false;

                        // Busca todos os PDVs
                        using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, CommandBehavior.SingleResult, pCOD_PRODUTO, pCOD_CATEGORIA))
                        {
                            while (sqlReader.Read())
                            {
                                result = sqlReader.GetValue(0) == System.DBNull.Value ? false : sqlReader.GetBoolean(0);
                            }

                            // Fecha o reader
                            sqlReader.Close();
                            sqlReader.Dispose();
                        }
                        return result;

                    }
                    catch (Exception ex)
                    {
                        CSGlobal.ShowMessage(ex.ToString());
                        throw new Exception("Erro na busca do flag de produto especifico da categoria.", ex);
                    }
                }

                #endregion

                public static bool GetEspecificoTopCategoria(int COD_CATEGORIA, int COD_PRODUTO)
                {
                    try
                    {
                        string sqlQuery =
                            "SELECT IND_PROD_TOP_CATEGORIA " +
                            "  FROM PRODUTO_CATEGORIA " +
                            " WHERE COD_PRODUTO = ? " +
                            "   AND COD_CATEGORIA = ? " +
                            "   AND IND_PROD_ESPECIFICO_CATEGORIA = 1";

                        SQLiteParameter pCOD_CATEGORIA = new SQLiteParameter("@COD_CATEGORIA", COD_CATEGORIA);
                        SQLiteParameter pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", COD_PRODUTO);

                        bool result = false;

                        // Busca todos os PDVs
                        using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, CommandBehavior.SingleResult, pCOD_PRODUTO, pCOD_CATEGORIA))
                        {
                            while (sqlReader.Read())
                            {
                                result = sqlReader.GetValue(0) == System.DBNull.Value ? false : sqlReader.GetBoolean(0);
                            }

                            // Fecha o reader
                            sqlReader.Close();
                            sqlReader.Dispose();
                        }
                        return result;

                    }
                    catch (Exception ex)
                    {
                        CSGlobal.ShowMessage(ex.ToString());
                        throw new Exception("Erro na busca do flag de produto top da categoria.", ex);
                    }
                }
            }

            #endregion

            // Evento de mudança do PDV
            private void CSPDVs_OnChangePDV()
            {
            }

        }
        #endregion

        public class CSProdutoVencimento : Java.Lang.Object
        {
            public int COD_PDV { get; set; }
            public int COD_EMPREGADO { get; set; }
            public int COD_PRODUTO { get; set; }

            public string DESCRICAO_APELIDO_PRODUTO { get; set; }
            public string DSC_PRODUTO { get; set; }
            public DateTime DAT_COLETA { get; set; }
            public decimal QTD_AVENCER { get; set; }
            public DateTime DAT_VENCIMENTO { get; set; }

            public void AdicionarVencimento()
            {
                StringBuilder sql = new StringBuilder();

                sql.AppendLine("INSERT INTO PDV_PRODUTO_VALIDADE ");
                sql.AppendLine("          (COD_PDV ");
                sql.AppendLine("          ,COD_EMPREGADO ");
                sql.AppendLine("          ,COD_PRODUTO ");
                sql.AppendLine("          ,DAT_COLETA ");
                sql.AppendLine("          ,QTD_AVENCER ");
                sql.AppendLine("          ,DAT_VENCIMENTO) ");
                sql.AppendLine("    VALUES ");
                sql.AppendFormat("          ({0}", this.COD_PDV);
                sql.AppendFormat("          ,{0} ", this.COD_EMPREGADO);
                sql.AppendFormat("          ,{0} ", this.COD_PRODUTO);
                sql.AppendFormat("          ,DATETIME('{0}') ", this.DAT_COLETA.ToString("yyyy-MM-dd HH:mm:ss"));
                sql.AppendFormat("          ,{0} ", this.QTD_AVENCER);
                sql.AppendFormat("          ,DATE('{0}')) ", this.DAT_VENCIMENTO.ToString("yyyy-MM-dd"));

                CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());
            }

            public bool ItemVencimentoColetado(DateTime dataVencimento)
            {
                bool retorno = false;

                if (CSEmpresa.ColunaExiste("PDV_PRODUTO_VALIDADE", "COD_PDV"))
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine("SELECT COUNT(*) ");
                    sql.AppendLine("   FROM PDV_PRODUTO_VALIDADE ");
                    sql.AppendFormat(" WHERE DATE(DAT_VENCIMENTO) = DATE('{0}') ", dataVencimento.ToString("yyyy-MM-dd"));
                    sql.AppendFormat(" AND COD_PRODUTO = {0}", this.COD_PRODUTO);

                    var result = CSDataAccess.Instance.ExecuteScalar(sql.ToString());

                    retorno = Convert.ToInt32(result) > 0;
                }

                return retorno;
            }
        }
    }
}