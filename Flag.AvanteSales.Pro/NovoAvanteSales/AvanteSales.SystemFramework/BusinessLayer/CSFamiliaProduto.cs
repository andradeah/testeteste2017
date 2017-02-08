using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
#if ANDROID
using Mono.Data.Sqlite;
#else
using System.Data.SQLite;
using System.Windows.Forms;
using System.Drawing;
#endif

#if ANDROID
using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
using SQLiteCommand = Mono.Data.Sqlite.SqliteCommand;
using SQLiteDataAdapter = Mono.Data.Sqlite.SqliteDataAdapter;
using SQLiteException = Mono.Data.Sqlite.SqliteException;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteTransaction = Mono.Data.Sqlite.SqliteTransaction;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;
using System.Text;
#endif
namespace AvanteSales
{
    public class CSFamiliasProduto : CollectionBase
    {
        #region [ Variaveis ]

        private static CSFamiliasProduto m_Items;

        #endregion

        #region [ Propriedades ]

        /// <summary>
        /// Retorna coleção das familias
        /// </summary>
        public static CSFamiliasProduto Items
        {
            get
            {
                if (m_Items == null)
                    m_Items = new CSFamiliasProduto();
                return m_Items;
            }
        }

        public CSFamiliasProduto.CSFamiliaProduto this[int Index]
        {
            get
            {
                return (CSFamiliasProduto.CSFamiliaProduto)this.InnerList[Index];
            }
        }

        /// <summary>
        /// Busca a familia pelo codigo
        /// </summary>
        /// <param name="COD_OPERACAO">Codigo da familia a ser procurada</param>
        /// <returns>Retorna o objeto da familia</returns>
        public static CSFamiliaProduto GetFamiliaProduto(int COD_FAMILIA_PRODUTO)
        {
            CSFamiliaProduto ret = new CSFamiliaProduto();
            // Procura pela operação
            foreach (CSFamiliaProduto familia in Items.InnerList)
            {
                if (familia.COD_FAMILIA_PRODUTO == COD_FAMILIA_PRODUTO)
                {
                    ret = familia;
                    break;
                }
            }
            // retorna o objeto da familia
            return ret;
        }

        #endregion

        #region [ Metodos ]

        /// <summary>
        /// Contrutor da classe. Busca as familias
        /// </summary>
        public CSFamiliasProduto()
        {
            try
            {
                string sqlQuery;
                sqlQuery = "SELECT DISTINCT FAMILIA_PRODUTO.COD_FAMILIA_PRODUTO " +
                           "      ,FAMILIA_PRODUTO.COD_GRUPO,FAMILIA_PRODUTO.DSC_FAMILIA_PRODUTO " +
                           "  FROM FAMILIA_PRODUTO " +
                           " INNER JOIN PRODUTO " +
                           "    ON FAMILIA_PRODUTO.COD_GRUPO = PRODUTO.COD_GRUPO " +
                           "   AND FAMILIA_PRODUTO.COD_FAMILIA_PRODUTO = PRODUTO.COD_FAMILIA_PRODUTO " +
                           " ORDER BY FAMILIA_PRODUTO.DSC_FAMILIA_PRODUTO ";

                // Busca todas as familias
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery))
                {
                    while (sqlReader.Read())
                    {
                        CSFamiliaProduto familia = new CSFamiliaProduto();

                        // Preenche a instancia da classe de familias dos produtos
                        familia.COD_FAMILIA_PRODUTO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        familia.GRUPO = CSGruposProduto.GetGrupoProduto(sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1));
                        familia.DSC_FAMILIA_PRODUTO = sqlReader.GetValue(2) == System.DBNull.Value ? "" : sqlReader.GetString(2);

                        // Adciona a familia na coleção de familias
                        base.InnerList.Add(familia);
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca das familias dos produtos.", ex);
            }
        }

        /// <summary>
        /// Retorna as famílias filtradas de todos os grupos de comercialização e produtos para a regra de grupos/famílias não vendidos
        /// </summary>
        public List<CSFamiliasProduto.CSFamiliaProduto> FamiliaRegraNaoVendidos(int COD_PDV, int COD_CATEGORIA, int COD_TIPO_DISTRIBUICAO_POLITICA, string IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE)
        {
            try
            {
                StringBuilder sqlQuery = new StringBuilder();

                sqlQuery.Append("SELECT DISTINCT T6.COD_FAMILIA_PRODUTO,T6.DSC_FAMILIA_PRODUTO,T6.COD_GRUPO ");
                sqlQuery.Append(" FROM PRODUTO T1 ");
                sqlQuery.Append(" LEFT JOIN TMPITENS T3 ");
                sqlQuery.Append(" ON T3.COD_PRODUTO = T1.COD_PRODUTO ");
                sqlQuery.Append(" LEFT JOIN PDV_PRODUTO_MUR T4 ");
                sqlQuery.Append(" ON T1.COD_PRODUTO = T4.COD_PRODUTO AND T4.COD_PDV  = " + COD_PDV + " ");
                sqlQuery.Append(" LEFT JOIN PRODUTO_CATEGORIA T5 ");
                sqlQuery.Append(" ON T5.COD_PRODUTO = T1.COD_PRODUTO AND T5.COD_CATEGORIA = " + COD_CATEGORIA + " ");
                sqlQuery.Append(" JOIN FAMILIA_PRODUTO T6 ON T6.COD_FAMILIA_PRODUTO = T1.COD_FAMILIA_PRODUTO AND T6.COD_GRUPO = T1.COD_GRUPO ");

                sqlQuery.Append("LEFT JOIN ");
                sqlQuery.Append("(SELECT DISTINCT FAMILIA.COD_FAMILIA_PRODUTO ");
                sqlQuery.Append("      ,FAMILIA.DSC_FAMILIA_PRODUTO ");
                sqlQuery.Append("      ,FAMILIA.COD_GRUPO ");
                sqlQuery.Append("FROM TMPITEMPEDIDO ITEM ");
                sqlQuery.Append("JOIN PRODUTO ");
                sqlQuery.Append("     ON ITEM.CODPRODUTO = PRODUTO.COD_PRODUTO ");
                sqlQuery.Append("JOIN FAMILIA_PRODUTO FAMILIA ");
                sqlQuery.Append("     ON PRODUTO.COD_FAMILIA_PRODUTO = FAMILIA.COD_FAMILIA_PRODUTO ");
                sqlQuery.Append("ORDER BY FAMILIA.COD_FAMILIA_PRODUTO) VENDIDOS ");
                sqlQuery.Append("   ON T6.COD_FAMILIA_PRODUTO = VENDIDOS.COD_FAMILIA_PRODUTO ");
                sqlQuery.Append(" WHERE T1.IND_ATIVO = 'A' ");
                sqlQuery.Append(" AND " + -1 + " IN (-1, T1.COD_GRUPO) ");
                sqlQuery.Append(" AND " + -1 + " IN (-1, T1.COD_GRUPO_COMERCIALIZACAO) ");
                sqlQuery.Append(" AND T1.IND_PRODUTO_COM_PRECO = 1 ");
                sqlQuery.Append(" AND T1.COD_TIPO_DISTRIBUICAO_POLITICA = " + COD_TIPO_DISTRIBUICAO_POLITICA + "");
                sqlQuery.Append(" AND T3.COD_PRODUTO IS NULL ");
                sqlQuery.Append(IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE == "N" ? " AND T1.QTD_ESTOQUE > 0 " : " ");
                sqlQuery.Append(" AND VENDIDOS.COD_FAMILIA_PRODUTO IS NULL ");
                sqlQuery.Append("ORDER BY T6.COD_FAMILIA_PRODUTO ");

                var familia = new List<CSFamiliasProduto.CSFamiliaProduto>();

                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString()))
                {
                    while (sqlReader.Read())
                    {
                        CSFamiliaProduto fam = new CSFamiliaProduto();

                        // Preenche a instancia da classe dos grupos dos produtos
                        fam.COD_FAMILIA_PRODUTO_FILTRADO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        fam.DSC_FAMILIA_PRODUTO_FILTRADO = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1);
                        fam.GRUPO = CSGruposProduto.GetGrupoProduto(sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2));

                        // Adcionao grupo na coleção dos grupos do produtos
                        familia.Add(fam);
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
                return familia;
            }

            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca das famílias dos produtos.", ex);
            }
        }

        /// <summary>
        /// Retorna as famílias filtradas de todos os grupos de comercialização e produtos para a regra de grupos/famílias não vendidos
        /// </summary>
        public List<CSFamiliasProduto.CSFamiliaProduto> FamiliaFiltradaRegraVendidos()
        {
            try
            {
                StringBuilder sqlQuery = new StringBuilder();

                sqlQuery.AppendLine("SELECT DISTINCT FAMILIA.COD_FAMILIA_PRODUTO ");
                sqlQuery.AppendLine("      ,FAMILIA.DSC_FAMILIA_PRODUTO ");
                sqlQuery.AppendLine("      ,FAMILIA.COD_GRUPO ");
                sqlQuery.AppendLine("FROM TMPITEMPEDIDO ITEM ");
                sqlQuery.AppendLine("JOIN PRODUTO ");
                sqlQuery.AppendLine("     ON ITEM.CODPRODUTO = PRODUTO.COD_PRODUTO ");
                sqlQuery.AppendLine("JOIN FAMILIA_PRODUTO FAMILIA ");
                sqlQuery.AppendLine("     ON PRODUTO.COD_FAMILIA_PRODUTO = FAMILIA.COD_FAMILIA_PRODUTO ");

                var familia = new List<CSFamiliasProduto.CSFamiliaProduto>();

                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString()))
                {
                    while (sqlReader.Read())
                    {
                        CSFamiliaProduto fam = new CSFamiliaProduto();

                        // Preenche a instancia da classe dos grupos dos produtos
                        fam.COD_FAMILIA_PRODUTO_FILTRADO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        fam.DSC_FAMILIA_PRODUTO_FILTRADO = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1);
                        fam.GRUPO = CSGruposProduto.GetGrupoProduto(sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2));

                        // Adcionao grupo na coleção dos grupos do produtos
                        familia.Add(fam);
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
                return familia;
            }

            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca das famílias dos produtos.", ex);
            }
        }

        /// <summary>
        /// Retorna as famílias filtradas de todos os grupos de comercialização e produtos para a regra de grupos/famílias não vendidos
        /// </summary>
        public List<CSFamiliasProduto.CSFamiliaProduto> FamiliaFiltradaRegraNaoVendidos(int COD_PDV, int COD_CATEGORIA, int COD_TIPO_DISTRIBUICAO_POLITICA, string IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE)
        {
            try
            {
                string sqlQuery = string.Empty;

                sqlQuery = "SELECT DISTINCT T6.COD_FAMILIA_PRODUTO,T6.DSC_FAMILIA_PRODUTO,T6.COD_GRUPO " +
                            " FROM PRODUTO T1 " +
                            " LEFT JOIN TMPITENS T3 " +
                            " ON T3.COD_PRODUTO = T1.COD_PRODUTO " +
                            " LEFT JOIN PDV_PRODUTO_MUR T4 " +
                            " ON T1.COD_PRODUTO = T4.COD_PRODUTO AND T4.COD_PDV  = " + COD_PDV + " " +
                            " LEFT JOIN PRODUTO_CATEGORIA T5 " +
                            " ON T5.COD_PRODUTO = T1.COD_PRODUTO AND T5.COD_CATEGORIA = " + COD_CATEGORIA + " " +
                            " JOIN FAMILIA_PRODUTO T6 ON T6.COD_FAMILIA_PRODUTO = T1.COD_FAMILIA_PRODUTO AND T6.COD_GRUPO = T1.COD_GRUPO " +
                            " WHERE T1.IND_ATIVO = 'A' " +
                            " AND " + -1 + " IN (-1, T1.COD_GRUPO) " +
                            " AND " + -1 + " IN (-1, T1.COD_GRUPO_COMERCIALIZACAO) " +
                            " AND T1.IND_PRODUTO_COM_PRECO = 1 " +
                            " AND T1.COD_TIPO_DISTRIBUICAO_POLITICA = " + COD_TIPO_DISTRIBUICAO_POLITICA + " " +
                            " AND T3.COD_PRODUTO IS NULL " +
                            "  " + (IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE == "N" ? " AND T1.QTD_ESTOQUE > 0 " : "") + " " +
                            " ORDER BY T6.DSC_FAMILIA_PRODUTO";

                var familia = new List<CSFamiliasProduto.CSFamiliaProduto>();

                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery))
                {
                    while (sqlReader.Read())
                    {
                        CSFamiliaProduto fam = new CSFamiliaProduto();

                        // Preenche a instancia da classe dos grupos dos produtos
                        fam.COD_FAMILIA_PRODUTO_FILTRADO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        fam.DSC_FAMILIA_PRODUTO_FILTRADO = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1);
                        fam.GRUPO = CSGruposProduto.GetGrupoProduto(sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2));

                        // Adcionao grupo na coleção dos grupos do produtos
                        familia.Add(fam);
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
                return familia;
            }

            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca das famílias dos produtos.", ex);
            }
        }

        /// <summary>
        /// Retorna as famílias filtradas com o grupo de comercialização e grupo de produto
        /// </summary>
        public List<CSFamiliasProduto.CSFamiliaProduto> FamiliaFiltrada(int CodigoGrupoProduto, int CodigoComercializacao, int COD_PDV, int COD_CATEGORIA, int COD_TIPO_DISTRIBUICAO_POLITICA, string IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE)
        {
            try
            {
                string sqlQuery = string.Empty;

                sqlQuery = "SELECT DISTINCT T6.COD_FAMILIA_PRODUTO,T6.DSC_FAMILIA_PRODUTO,T6.COD_GRUPO " +
                            " FROM PRODUTO T1 " +
                            //" LEFT JOIN TMPITEMPEDIDO T2 " +
                            //" ON T2.CODPRODUTO = T1.COD_PRODUTO " +
                            //" LEFT JOIN TMPITENS T3 " +
                            //" ON T3.COD_PRODUTO = T1.COD_PRODUTO " +
                            " LEFT JOIN PDV_PRODUTO_MUR T4 " +
                            " ON T1.COD_PRODUTO = T4.COD_PRODUTO AND T4.COD_PDV  = " + COD_PDV + " " +
                            " LEFT JOIN PRODUTO_CATEGORIA T5 " +
                            " ON T5.COD_PRODUTO = T1.COD_PRODUTO AND T5.COD_CATEGORIA = " + COD_CATEGORIA + " " +
                            " JOIN FAMILIA_PRODUTO T6 ON T6.COD_FAMILIA_PRODUTO = T1.COD_FAMILIA_PRODUTO AND T6.COD_GRUPO = T1.COD_GRUPO " +
                            " WHERE T1.IND_ATIVO = 'A' " +
                            " AND " + CodigoGrupoProduto + " IN (-1, T1.COD_GRUPO) " +
                            " AND " + CodigoComercializacao + " IN (-1, T1.COD_GRUPO_COMERCIALIZACAO) " +
                            " AND T1.IND_PRODUTO_COM_PRECO = 1 " +
                            " AND T1.COD_TIPO_DISTRIBUICAO_POLITICA = " + COD_TIPO_DISTRIBUICAO_POLITICA + " " +
                            //" AND T2.CODPRODUTO IS NULL " +
                            //" AND T3.COD_PRODUTO IS NULL " +
                            "  " + (IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE == "N" ? " AND T1.QTD_ESTOQUE > 0 " : "") + " " +
                            " ORDER BY T6.DSC_FAMILIA_PRODUTO";

                var familia = new List<CSFamiliasProduto.CSFamiliaProduto>();

                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery))
                {
                    while (sqlReader.Read())
                    {
                        CSFamiliaProduto fam = new CSFamiliaProduto();

                        // Preenche a instancia da classe dos grupos dos produtos
                        fam.COD_FAMILIA_PRODUTO_FILTRADO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        fam.DSC_FAMILIA_PRODUTO_FILTRADO = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1);
                        fam.GRUPO = CSGruposProduto.GetGrupoProduto(sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2));

                        // Adcionao grupo na coleção dos grupos do produtos
                        familia.Add(fam);
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
                return familia;
            }

            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca das famílias dos produtos.", ex);
            }
        }
        #endregion

        #region [ SubClasses ]

        /// <summary>
        /// Guarda a informação sobre a familia dos proddutos
        /// </summary>
        public class CSFamiliaProduto : Java.Lang.Object
        {
            #region [ Variaveis ]

            private int m_COD_FAMILIA_PRODUTO;
            private int m_COD_FAMILIA_PRODUTO_FILTRADO;
            private CSGruposProduto.CSGrupoProduto m_GRUPO;
            private string m_DSC_FAMILIA_PRODUTO;
            private string m_DSC_FAMILIA_PRODUTO_FILTRADO;
            private static CSFamiliasProduto.CSFamiliaProduto m_Current;

            #endregion

            #region [ Propriedades ]

            /// <summary>
            /// Guarda o codigo da familia
            /// </summary>
            public int COD_FAMILIA_PRODUTO
            {
                get
                {
                    return m_COD_FAMILIA_PRODUTO;
                }
                set
                {
                    m_COD_FAMILIA_PRODUTO = value;
                }
            }

            /// <summary>
            /// Guarda o codigo da familia filtrado
            /// </summary>
            public int COD_FAMILIA_PRODUTO_FILTRADO
            {
                get
                {
                    return m_COD_FAMILIA_PRODUTO_FILTRADO;
                }
                set
                {
                    m_COD_FAMILIA_PRODUTO_FILTRADO = value;
                }
            }

            /// <summary>
            /// Guarda o objeto do grupo a que a familia pertence
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

            /// <summary>
            /// Guarda a descrição da familia
            /// </summary>
            public string DSC_FAMILIA_PRODUTO
            {
                get
                {
                    return m_DSC_FAMILIA_PRODUTO;
                }
                set
                {
                    m_DSC_FAMILIA_PRODUTO = value.Trim();
                }
            }

            /// <summary>
            /// Guarda a descrição da familia filtrado
            /// </summary>
            public string DSC_FAMILIA_PRODUTO_FILTRADO
            {
                get
                {
                    return m_DSC_FAMILIA_PRODUTO_FILTRADO;
                }
                set
                {
                    m_DSC_FAMILIA_PRODUTO_FILTRADO = value.Trim();
                }
            }

            /// <summary>
            /// Guarda a familia atual.
            /// </summary>
            public static CSFamiliasProduto.CSFamiliaProduto Current
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

            public CSFamiliaProduto()
            {

            }

            #endregion
        }

        #endregion
    }
}