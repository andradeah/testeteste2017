using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
#if ANDROID
using Mono.Data.Sqlite;
using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
using SQLiteCommand = Mono.Data.Sqlite.SqliteCommand;
using SQLiteDataAdapter = Mono.Data.Sqlite.SqliteDataAdapter;
using SQLiteException = Mono.Data.Sqlite.SqliteException;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteTransaction = Mono.Data.Sqlite.SqliteTransaction;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;
#else
using System.Data.SQLite;
using System.Windows.Forms;
using System.Drawing;
#endif

namespace AvanteSales
{
    public class CSGruposProduto : CollectionBase
    {
        #region [ Variaveis ]

        private static CSGruposProduto m_Items;

        #endregion

        #region [ Propriedades ]

        /// <summary>
        /// Retorna coleção dos grupos dos produtos
        /// </summary>
        public static CSGruposProduto Items
        {
            get
            {
                if (m_Items == null)
                    m_Items = new CSGruposProduto();
                return m_Items;
            }
        }

        public CSGruposProduto.CSGrupoProduto this[int Index]
        {
            get
            {
                return (CSGruposProduto.CSGrupoProduto)this.InnerList[Index];
            }
        }

        public static void Markup(int COD_PDV, int COD_GRUPO, string PCT_MARKUP)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT COUNT(*) FROM GRUPO_MARKUP WHERE ");
            sql.AppendFormat("     COD_PDV = {0} ", COD_PDV);
            sql.AppendFormat(" AND COD_GRUPO = {0} ", COD_GRUPO);

            var count = CSDataAccess.Instance.ExecuteScalar(sql.ToString());

            if (Convert.ToInt32(count) == 0)
            {
                sql = new StringBuilder();
                sql.Append("INSERT INTO GRUPO_MARKUP (COD_PDV,COD_GRUPO,PCT_MARKUP,COD_EMPREGADO,DAT_COLETA) VALUES ( ");
                sql.AppendFormat("{0}", COD_PDV);
                sql.AppendFormat(",{0}", COD_GRUPO);
                sql.AppendFormat(",{0}", CSGlobal.StrToDecimal(PCT_MARKUP));
                sql.AppendFormat(",{0}", CSEmpregados.Current.COD_EMPREGADO);
                sql.AppendFormat(",DATETIME('{0}')", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sql.AppendLine(")");

                CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());
            }
            else
            {
                sql = new StringBuilder();
                sql.Append("UPDATE GRUPO_MARKUP SET ");
                sql.AppendFormat(" PCT_MARKUP = {0} ", PCT_MARKUP);
                sql.AppendFormat(",DAT_COLETA = DATETIME('{0}') ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sql.AppendFormat(" WHERE COD_PDV = {0} AND COD_GRUPO = {1} ", COD_PDV, COD_GRUPO);

                CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());
            }
        }

        /// <summary>
        /// Busca grupo pelo codigo
        /// </summary>
        /// <param name="COD_OPERACAO">Codigo do grupo a ser procurado</param>
        /// <returns>Retorna o objeto do grupo</returns>
        public static CSGrupoProduto GetGrupoProduto(int COD_GRUPO)
        {
            CSGrupoProduto ret = new CSGrupoProduto();
            // Procura pelo grupo
            foreach (CSGrupoProduto grupo in Items.InnerList)
            {
                if (grupo.COD_GRUPO == COD_GRUPO)
                {
                    ret = grupo;
                    break;
                }
            }
            // retorna o objeto do grupo
            return ret;
        }


        #endregion

        #region [ Metodos ]

        /// <summary>
        /// Contrutor da classe. Busca os grupos
        /// </summary>
        public CSGruposProduto()
        {
            try
            {
                var grupos = new List<CSGrupoProduto>();
                StringBuilder sqlQuery = new StringBuilder();
                sqlQuery.AppendLine("SELECT DISTINCT GRUPO_PRODUTO.COD_GRUPO, GRUPO_PRODUTO.DSC_GRUPO ");

                if (CSEmpresa.ColunaExiste("GRUPO_MARKUP", "COD_PDV") && CSPDVs.Current != null)
                {
                    sqlQuery.Append(" ,GRUPO_MARKUP.PCT_MARKUP ");
                }

                sqlQuery.AppendLine("  FROM GRUPO_PRODUTO ");
                sqlQuery.AppendLine(" INNER JOIN PRODUTO ");
                sqlQuery.AppendLine("    ON GRUPO_PRODUTO.COD_GRUPO = PRODUTO.COD_GRUPO ");

                if (CSEmpresa.ColunaExiste("GRUPO_MARKUP", "COD_PDV") && CSPDVs.Current != null)
                {
                    sqlQuery.Append("  LEFT JOIN GRUPO_MARKUP ");
                    sqlQuery.Append("       ON  GRUPO_MARKUP.COD_GRUPO = GRUPO_PRODUTO.COD_GRUPO ");
                    sqlQuery.AppendFormat(" AND GRUPO_MARKUP.COD_PDV = {0} ", CSPDVs.Current.COD_PDV);
                }

                sqlQuery.AppendLine(" ORDER BY GRUPO_PRODUTO.DSC_GRUPO");

                // Busca todos os grupos
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString()))
                {
                    while (sqlReader.Read())
                    {
                        CSGrupoProduto grupo = new CSGrupoProduto();

                        // Preenche a instancia da classe dos grupos dos produtos
                        grupo.COD_GRUPO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        grupo.DSC_GRUPO = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1);
                        grupo.COD_GRUPO_FILTRADO = grupo.COD_GRUPO;
                        grupo.DSC_GRUPO_FILTRADO = grupo.DSC_GRUPO;

                        // Adcionao grupo na coleção dos grupos do produtos
                        grupos.Add(grupo);
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
                base.InnerList.AddRange(grupos.OrderBy(c => c.DSC_GRUPO).ToList());
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos grupos dos produtos.", ex);
            }
        }

        /// <summary>
        /// Retorna os grupos que estão contidos na tabela de produto de acordo com o grupo de comercialização
        /// </summary>
        /// <param name="CodigoComercializacao">Pertence ao código do grupo de comercialização informado</param>
        public List<CSGruposProduto.CSGrupoProduto> GrupoProdutoFiltrado(int CodigoComercializacao, int COD_PDV, int COD_CATEGORIA, int COD_TIPO_DISTRIBUICAO_POLITICA, string IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE)
        {
            try
            {
                StringBuilder sqlQuery = new StringBuilder();

                //string CondicaoWhere = string.Empty;

                //if (CodigoComercializacao == -1)
                //{
                //    CondicaoWhere = " WHERE P.COD_GRUPO_COMERCIALIZACAO <> '' ORDER BY GP.DSC_GRUPO";
                //}

                //else
                //{
                //    CondicaoWhere = " WHERE P.COD_GRUPO_COMERCIALIZACAO = '" + CodigoComercializacao + "' ORDER BY GP.DSC_GRUPO";
                //}

                //sqlQuery = "SELECT DISTINCT GP.COD_GRUPO,GP.DSC_GRUPO " +
                //            " FROM GRUPO_PRODUTO GP " +
                //            "  JOIN PRODUTO P ON P.COD_GRUPO = GP.COD_GRUPO " + CondicaoWhere;

                sqlQuery.Append("SELECT DISTINCT GP.COD_GRUPO,GP.DSC_GRUPO, GC.DES_GRUPO_COMERCIALIZACAO, QTD_GRUPO.QTD_VENDA, QTD_GRUPOPRODUTO.QTD_GRUPO_PRODUTO ");

                if (CSEmpresa.ColunaExiste("GRUPO_MARKUP", "COD_PDV"))
                {
                    sqlQuery.Append(" ,GM.PCT_MARKUP ");
                }

                sqlQuery.AppendLine(" FROM GRUPO_PRODUTO GP ");
                sqlQuery.AppendLine(" JOIN PRODUTO T1 ON T1.COD_GRUPO = GP.COD_GRUPO ");
                sqlQuery.AppendLine(" LEFT JOIN TMPITEMPEDIDO T2 ");
                sqlQuery.AppendLine("   ON T2.CODPRODUTO = T1.COD_PRODUTO ");
                sqlQuery.AppendLine(" LEFT JOIN TMPITENS T3 ");
                sqlQuery.AppendLine("   ON T3.COD_PRODUTO = T1.COD_PRODUTO ");
                sqlQuery.AppendLine(" LEFT JOIN PDV_PRODUTO_MUR T4 ");
                sqlQuery.AppendFormat("   ON T1.COD_PRODUTO = T4.COD_PRODUTO AND T4.COD_PDV = {0} ", COD_PDV);
                sqlQuery.AppendLine(" LEFT JOIN PRODUTO_CATEGORIA T5 ");
                sqlQuery.AppendFormat("   ON T5.COD_PRODUTO = T1.COD_PRODUTO AND T5.COD_CATEGORIA = {0} ", COD_CATEGORIA);
                sqlQuery.AppendLine(" JOIN GRUPO_COMERCIALIZACAO GC ");
                sqlQuery.AppendLine("ON GC.COD_GRUPO_COMERCIALIZACAO = T1.COD_GRUPO_COMERCIALIZACAO ");
                sqlQuery.AppendLine(" LEFT JOIN ( ");
                sqlQuery.AppendLine("             SELECT PRODUTO.COD_GRUPO, COUNT(ITEM_PEDIDO.COD_PEDIDO) AS 'QTD_VENDA' ");
                sqlQuery.AppendLine("       FROM PEDIDO ");
                sqlQuery.AppendLine("       JOIN ITEM_PEDIDO ");
                sqlQuery.AppendLine("           ON PEDIDO.COD_PEDIDO = ITEM_PEDIDO.COD_PEDIDO ");
                sqlQuery.AppendFormat("           AND DATE(PEDIDO.DAT_PEDIDO) = DATE('{0}') ", DateTime.Now.ToString("yyyy-MM-dd"));
                sqlQuery.AppendFormat("           AND PEDIDO.COD_PDV = {0} ", CSPDVs.Current.COD_PDV);
                sqlQuery.AppendLine("       JOIN PRODUTO ");
                sqlQuery.AppendLine("           ON ITEM_PEDIDO.COD_PRODUTO = PRODUTO.COD_PRODUTO ");
                sqlQuery.AppendLine("       GROUP BY PRODUTO.COD_GRUPO  ) AS QTD_GRUPO");
                sqlQuery.AppendLine("   ON GP.COD_GRUPO = QTD_GRUPO.COD_GRUPO ");
                sqlQuery.AppendLine(" LEFT JOIN ( ");
                sqlQuery.AppendLine("  SELECT PRODUTO.COD_GRUPO,COUNT(COD_PRODUTO) AS 'QTD_GRUPO_PRODUTO' ");
                sqlQuery.AppendLine("         FROM PRODUTO ");
                sqlQuery.AppendLine("         JOIN GRUPO_PRODUTO ");
                sqlQuery.AppendLine("             ON PRODUTO.COD_GRUPO = GRUPO_PRODUTO.COD_GRUPO ");
                sqlQuery.AppendLine("         WHERE PRODUTO.IND_ATIVO = 'A' ");
                sqlQuery.AppendLine("         AND PRODUTO.IND_PRODUTO_COM_PRECO = 1 ");
                sqlQuery.AppendFormat("       AND PRODUTO.COD_TIPO_DISTRIBUICAO_POLITICA = {0} ", CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA);

                if (IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE == "N")
                    sqlQuery.AppendLine("  AND PRODUTO.QTD_ESTOQUE > 0 ");

                sqlQuery.AppendLine("         GROUP BY PRODUTO.COD_GRUPO ) AS QTD_GRUPOPRODUTO");
                sqlQuery.AppendLine("    ON GP.COD_GRUPO = QTD_GRUPOPRODUTO.COD_GRUPO ");

                if (CSEmpresa.ColunaExiste("GRUPO_MARKUP", "COD_PDV"))
                {
                    sqlQuery.Append("  LEFT JOIN GRUPO_MARKUP GM ");
                    sqlQuery.Append("       ON GM.COD_GRUPO = GP.COD_GRUPO ");
                    sqlQuery.AppendFormat(" AND GM.COD_PDV = {0}", CSPDVs.Current.COD_PDV);
                }

                sqlQuery.AppendLine(" WHERE T1.IND_ATIVO = 'A' ");
                sqlQuery.AppendLine("  AND -1 IN (-1, T1.COD_FAMILIA_PRODUTO) ");
                sqlQuery.AppendFormat("  AND {0} IN (-1, T1.COD_GRUPO_COMERCIALIZACAO) ", CodigoComercializacao);
                sqlQuery.AppendLine("  AND T1.IND_PRODUTO_COM_PRECO = 1 ");
                sqlQuery.AppendFormat("  AND T1.COD_TIPO_DISTRIBUICAO_POLITICA = {0} ", COD_TIPO_DISTRIBUICAO_POLITICA);
                sqlQuery.AppendLine("  AND T2.CODPRODUTO IS NULL ");
                sqlQuery.AppendLine("  AND T3.COD_PRODUTO IS NULL ");

                if (IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE == "N")
                    sqlQuery.AppendLine("  AND T1.QTD_ESTOQUE > 0 ");

                sqlQuery.AppendLine(" ORDER BY  GC.DES_GRUPO_COMERCIALIZACAO, GP.DSC_GRUPO ");

                var grupos = new List<CSGruposProduto.CSGrupoProduto>();

                // Busca grupos em que estão contidos na tabela de produto
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString()))
                {
                    while (sqlReader.Read())
                    {
                        CSGrupoProduto grupo = new CSGrupoProduto();

                        // Preenche a instancia da classe dos grupos dos produtos
                        grupo.COD_GRUPO_FILTRADO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        grupo.DSC_GRUPO_FILTRADO = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1);
                        grupo.DSC_GRUPO_COMERCIALIZACAO = sqlReader.GetValue(2) == System.DBNull.Value ? string.Empty : sqlReader.GetString(2);
                        grupo.QTD_VENDA_DIA = sqlReader.GetValue(3) == System.DBNull.Value ? 0 : sqlReader.GetInt32(3);
                        grupo.QTD_PRODUTO = sqlReader.GetValue(4) == System.DBNull.Value ? 0 : sqlReader.GetInt32(4);
                        grupo.COD_GRUPO = grupo.COD_GRUPO_FILTRADO;
                        grupo.DSC_GRUPO = grupo.DSC_GRUPO_FILTRADO;

                        // Adcionao grupo na coleção dos grupos do produtos
                        grupos.Add(grupo);
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
                return grupos;
            }

            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos grupos dos produtos.", ex);
            }

        }

        #endregion

        #region [ SubClasses ]

        /// <summary>
        /// Guarda a informação sobre o grupo dos proddutos
        /// </summary> 
        public class CSGrupoProduto : Java.Lang.Object
        {
            #region [ Variaveis ]

            private int m_COD_GRUPO;
            private string m_DSC_GRUPO;
            private int m_COD_GRUPO_FILTRADO;
            private string m_DSC_GRUPO_FILTRADO;
            private string m_DSC_GRUPO_COMERCIALIZACAO;
            private int m_QTD_VENDA_DIA;
            private int m_QTD_PRODUTO;
            private decimal m_PCT_MARKUP;
            private static CSGruposProduto.CSGrupoProduto m_Current;

            #endregion

            #region [ Propriedades ]

            /// <summary>
            /// Guarda o codigo do grupo
            /// </summary>
            public int COD_GRUPO
            {
                get
                {
                    return m_COD_GRUPO;
                }
                set
                {
                    m_COD_GRUPO = value;
                }
            }

            /// <summary>
            /// Guarda o código do grupo filtrado com o grupo de comercialização
            /// </summary>
            public int COD_GRUPO_FILTRADO
            {
                get
                {
                    return m_COD_GRUPO_FILTRADO;
                }
                set
                {
                    m_COD_GRUPO_FILTRADO = value;
                }
            }

            /// <summary>
            /// Guarda a descrição do grupo
            /// </summary>
            public string DSC_GRUPO
            {
                get
                {
                    return m_DSC_GRUPO;
                }
                set
                {
                    m_DSC_GRUPO = value;
                }
            }

            public string DSC_GRUPO_COMERCIALIZACAO
            {
                get
                {
                    return m_DSC_GRUPO_COMERCIALIZACAO;
                }
                set
                {
                    m_DSC_GRUPO_COMERCIALIZACAO = value;
                }
            }

            public int QTD_VENDA_DIA
            {
                get
                {
                    return m_QTD_VENDA_DIA;
                }
                set
                {
                    m_QTD_VENDA_DIA = value;
                }
            }

            public int QTD_PRODUTO
            {
                get
                {
                    return m_QTD_PRODUTO;
                }
                set
                {
                    m_QTD_PRODUTO = value;
                }
            }

            public decimal PCT_MARKUP
            {
                get
                {
                    return RetornarMakup(this.COD_GRUPO_FILTRADO == 0 ? this.COD_GRUPO : this.COD_GRUPO_FILTRADO, CSPDVs.Current.COD_PDV);
                }
            }

            public static decimal RetornarMakup(int grupo, int pdv)
            {
                decimal markup = 0;

                if (CSEmpresa.ColunaExiste("GRUPO_MARKUP", "COD_EMPREGADO"))
                {
                    StringBuilder sqlMarkup = new StringBuilder();
                    sqlMarkup.AppendFormat("SELECT PCT_MARKUP FROM GRUPO_MARKUP WHERE COD_GRUPO = {0} AND COD_PDV = {1}", grupo, pdv);
                    var result = CSDataAccess.Instance.ExecuteScalar(sqlMarkup.ToString());

                    markup = Convert.ToDecimal(result);
                }
                
                return markup;
            }

            /// <summary>
            /// Guarda a descrição do grupo filtrado com o grupo de comercialização
            /// </summary>
            public string DSC_GRUPO_FILTRADO
            {
                get
                {
                    return m_DSC_GRUPO_FILTRADO;
                }
                set
                {
                    m_DSC_GRUPO_FILTRADO = value;
                }
            }

            /// <summary>
            /// Guarda o grupo
            /// </summary>
            public static CSGruposProduto.CSGrupoProduto Current
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

            public CSGrupoProduto()
            {

            }

            public override bool Equals(object obj)
            {
                if (obj is CSGruposProduto.CSGrupoProduto)
                {
                    return ((CSGruposProduto.CSGrupoProduto)obj).COD_GRUPO == this.COD_GRUPO;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            #endregion
        }

        #endregion
    }
}