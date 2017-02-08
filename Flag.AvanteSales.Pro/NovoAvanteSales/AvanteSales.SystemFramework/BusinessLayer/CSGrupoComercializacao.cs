using System;
using System.Collections;
using System.Data;
using System.Collections.Generic;
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
    public class CSGruposComercializacao : CollectionBase
    {
        #region [ Variaveis ]

        private static CSGruposComercializacao m_Items;

        #endregion

        #region [ Propriedades ]

        /// <summary>
        /// Retorna coleção dos grupos de comercializacao
        /// </summary>
        public static CSGruposComercializacao Items
        {
            get
            {
                if (m_Items == null)
                    m_Items = new CSGruposComercializacao();
                return m_Items;
            }
        }

        public CSGruposComercializacao.CSGrupoComercializacao this[int Index]
        {
            get
            {
                return (CSGruposComercializacao.CSGrupoComercializacao)this.InnerList[Index];
            }
        }

        #endregion

        #region [ Metodos ]

        /// <summary>
        /// Construtor para grupo de comercialização na indenização
        /// </summary>
        /// <param name="indenizacao"></param>
        public CSGruposComercializacao(bool indenizacao)
        {
            try
            {
                string sqlQuery = @"SELECT G.*,COUNT(*) AS QTD FROM GRUPO_COMERCIALIZACAO G
                                        JOIN PRODUTO P ON G.[COD_GRUPO_COMERCIALIZACAO] = P.[COD_GRUPO_COMERCIALIZACAO]       
                                    WHERE P.[PCT_TAXA_MAX_INDENIZACAO] > 0 AND P.[PCT_TAXA_MAX_INDENIZACAO] IS NOT NULL
                                    GROUP BY G.[COD_GRUPO_COMERCIALIZACAO]
                                    ORDER BY QTD DESC";

                // Busca todos os grupos
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery))
                {
                    while (sqlReader.Read())
                    {
                        CSGrupoComercializacao grupo = new CSGrupoComercializacao();

                        // Preenche a instancia da classe dos grupos
                        grupo.COD_GRUPO_COMERCIALIZACAO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        grupo.DES_GRUPO_COMERCIALIZACAO = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1).Trim();
                        grupo.COD_SETOR_BROKER = ((sqlReader.GetValue(2) == System.DBNull.Value) || (sqlReader.GetValue(2).ToString().Length < 4)) ? "" : sqlReader.GetString(2).Substring(0, 4);

                        if (CSEmpresa.ColunaExiste("GRUPO_COMERCIALIZACAO", "IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO"))
                            grupo.IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO = sqlReader.GetValue(3) == System.DBNull.Value ? "N" : sqlReader.GetString(3);

                        // Adcionao grupo na coleção
                        base.InnerList.Add(grupo);
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos grupos de comercialização na indenização.", ex);
            }
        }

        /// <summary>
        /// Contrutor da classe. Busca os grupos
        /// </summary>
        public CSGruposComercializacao()
        {
            string sqlQuery = null;
            SQLiteParameter[] param = null;

            try
            {
                if (CSEmpregados.Current.COD_GRUPO_COMERCIALIZACAO == 99)
                {
                    sqlQuery =
                        "SELECT T1.COD_GRUPO_COMERCIALIZACAO " +
                        "      ,T1.DES_GRUPO_COMERCIALIZACAO " +
                        "      ,T1.COD_SETOR_BROKER ";

                    if (CSEmpresa.ColunaExiste("GRUPO_COMERCIALIZACAO", "IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO"))
                        sqlQuery += " ,T1.IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO ";

                    sqlQuery += "  FROM GRUPO_COMERCIALIZACAO T1 " +
                    " ORDER BY T1.COD_GRUPO_COMERCIALIZACAO ";

                    param = new SQLiteParameter[0];

                }
                else
                {
                    sqlQuery =
                        "SELECT T1.COD_GRUPO_COMERCIALIZACAO " +
                        "      ,T1.DES_GRUPO_COMERCIALIZACAO " +
                        "      ,T1.COD_SETOR_BROKER ";

                    if (CSEmpresa.ColunaExiste("GRUPO_COMERCIALIZACAO", "IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO"))
                        sqlQuery += " ,T1.IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO ";

                    sqlQuery += "  FROM GRUPO_COMERCIALIZACAO T1 " +
                    " WHERE T1.COD_GRUPO_COMERCIALIZACAO = ? " +
                    " ORDER BY T1.COD_GRUPO_COMERCIALIZACAO ";

                    param = new SQLiteParameter[1];
                    param[0] = new SQLiteParameter("@COD_GRUPO_COMERCIALIZACAO", CSEmpregados.Current.COD_GRUPO_COMERCIALIZACAO);
                }

                // Busca todos os grupos
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, param))
                {
                    while (sqlReader.Read())
                    {
                        CSGrupoComercializacao grupo = new CSGrupoComercializacao();

                        // Preenche a instancia da classe dos grupos
                        grupo.COD_GRUPO_COMERCIALIZACAO_FILTRADO = grupo.COD_GRUPO_COMERCIALIZACAO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        grupo.DES_GRUPO_COMERCIALIZACAO_FILTRADO = grupo.DES_GRUPO_COMERCIALIZACAO = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1).Trim();
                        grupo.COD_SETOR_BROKER = ((sqlReader.GetValue(2) == System.DBNull.Value) || (sqlReader.GetValue(2).ToString().Length < 4)) ? "" : sqlReader.GetString(2).Substring(0, 4);

                        if (CSEmpresa.ColunaExiste("GRUPO_COMERCIALIZACAO", "IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO"))
                            grupo.IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO = sqlReader.GetValue(3) == System.DBNull.Value ? "N" : sqlReader.GetString(3);

                        // Adcionao grupo na coleção
                        base.InnerList.Add(grupo);
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos grupos de comercialização.", ex);
            }
        }

        public List<CSGruposComercializacao.CSGrupoComercializacao> GrupoComercializacaoFiltrado()
        {
            try
            {
                StringBuilder sqlQuery = new StringBuilder();

                sqlQuery.Append("SELECT DISTINCT GC.COD_GRUPO_COMERCIALIZACAO,GC.DES_GRUPO_COMERCIALIZACAO ");

                if (CSEmpresa.ColunaExiste("GRUPO_COMERCIALIZACAO", "IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO"))
                    sqlQuery.Append(" ,GC.IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO ");
                else
                    sqlQuery.Append(", 0 AS 'EXCLUSIVO' ");

                sqlQuery.Append(", GC.COD_SETOR_BROKER ");

                sqlQuery.Append(" FROM GRUPO_COMERCIALIZACAO GC JOIN PRODUTO P ");
                sqlQuery.Append(" ON GC.COD_GRUPO_COMERCIALIZACAO = P.COD_GRUPO_COMERCIALIZACAO ");
                sqlQuery.Append(" JOIN GRUPO_PRODUTO GP ON P.COD_GRUPO = GP.COD_GRUPO ");
                sqlQuery.Append(" WHERE P.IND_ATIVO = 'A' ");
                sqlQuery.AppendFormat(" AND P.COD_TIPO_DISTRIBUICAO_POLITICA = {0} ", CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA);

                var gruposComercializacao = new List<CSGruposComercializacao.CSGrupoComercializacao>();

                // Busca grupos em que estão contidos na tabela de produto
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString()))
                {
                    while (sqlReader.Read())
                    {
                        CSGrupoComercializacao comercializacao = new CSGrupoComercializacao();

                        // Preenche a instancia da classe dos grupos dos produtos
                        comercializacao.COD_GRUPO_COMERCIALIZACAO_FILTRADO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        comercializacao.DES_GRUPO_COMERCIALIZACAO_FILTRADO = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1);
                        comercializacao.IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO = sqlReader.GetValue(2) == System.DBNull.Value ? "N" : sqlReader.GetString(2);
                        comercializacao.COD_SETOR_BROKER = sqlReader.GetValue(3) == System.DBNull.Value ? string.Empty : sqlReader.GetString(3);

                        // Adcionao grupo na coleção dos grupos do produtos
                        gruposComercializacao.Add(comercializacao);
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
                return gruposComercializacao;
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos grupos de comercialização.", ex);
            }
        }

        public static CSGrupoComercializacao GetGrupoComercializacao(int COD_GRUPO_COMERCIALIZACAO)
        {
            // Procura pelo grupo
            foreach (CSGrupoComercializacao grupo in Items.InnerList)
            {
                if (grupo.COD_GRUPO_COMERCIALIZACAO == COD_GRUPO_COMERCIALIZACAO)
                {
                    return grupo;
                }
            }

            return new CSGrupoComercializacao();
        }

        #endregion

        #region [ SubClasses ]

        public class CSGrupoComercializacaoMetaVenda : Java.Lang.Object
        {
            public int COD_GRUPO_COMERCIALIZACAO { get; set; }
            public string DES_GRUPO_COMERCIALIZACAO { get; set; }
            public double VLR_OBJETIVO { get; set; }
            public double VLR_VENDIDO { get; set; }

            public List<CSGruposComercializacao.CSGrupoComercializacaoMetaVenda> RetornarMetaVendaGrupoComercializacao()
            {
                List<CSGruposComercializacao.CSGrupoComercializacaoMetaVenda> resultado = new List<CSGrupoComercializacaoMetaVenda>();

                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT GC.COD_GRUPO_COMERCIALIZACAO ");
                sql.AppendLine("      ,GC.DES_GRUPO_COMERCIALIZACAO ");
                sql.AppendLine(" 	  ,OBJ.VLR_OBJETIVO ");
                sql.AppendLine(" 	  ,VENDA.TOTAL ");
                sql.AppendLine(" FROM PDV_OBJETIVO OBJ ");
                sql.AppendLine(" JOIN GRUPO_COMERCIALIZACAO GC ");
                sql.AppendLine("     ON OBJ.COD_GRUPO_COMERCIALIZACAO = GC.COD_GRUPO_COMERCIALIZACAO ");
                sql.AppendLine(" LEFT JOIN( ");
                sql.AppendLine("             SELECT COD_GRUPO_COMERCIALIZACAO, SUM(ITEM_PEDIDO.VLR_TOTAL) AS 'TOTAL' ");
                sql.AppendLine("         FROM PEDIDO ");
                sql.AppendLine("         JOIN ITEM_PEDIDO ");
                sql.AppendLine("             ON PEDIDO.COD_PEDIDO = ITEM_PEDIDO.COD_PEDIDO ");
                sql.AppendFormat("           AND DATE(PEDIDO.DAT_PEDIDO) = DATE('{0}') ", DateTime.Now.ToString("yyyy-MM-dd"));
                sql.AppendLine("         JOIN PRODUTO ");
                sql.AppendLine("             ON ITEM_PEDIDO.COD_PRODUTO = PRODUTO.COD_PRODUTO ");
                sql.AppendFormat("       WHERE PEDIDO.COD_PDV = {0} ", CSPDVs.Current.COD_PDV);
                sql.AppendLine("         GROUP BY PRODUTO.COD_GRUPO_COMERCIALIZACAO ");
                sql.AppendLine("     ) AS VENDA ");
                sql.AppendLine(" ON GC.COD_GRUPO_COMERCIALIZACAO = VENDA.COD_GRUPO_COMERCIALIZACAO ");
                sql.AppendFormat(" WHERE DATE(OBJ.DAT_OBJETIVO) = DATE('{0}') ", DateTime.Now.ToString("yyyy-MM-dd"));
                sql.AppendFormat(" AND OBJ.COD_EMPREGADO = {0} ", CSEmpregados.Current.COD_EMPREGADO);
                sql.AppendFormat(" AND OBJ.COD_PDV = {0} ", CSPDVs.Current.COD_PDV);

                CSGruposComercializacao.CSGrupoComercializacaoMetaVenda linha;

                using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
                {
                    while (reader.Read())
                    {
                        linha = new CSGrupoComercializacaoMetaVenda();
                        linha.COD_GRUPO_COMERCIALIZACAO = reader.GetValue(0) == System.DBNull.Value ? 0 : reader.GetInt32(0);
                        linha.DES_GRUPO_COMERCIALIZACAO = reader.GetValue(1) == System.DBNull.Value ? string.Empty : reader.GetString(1);
                        linha.VLR_OBJETIVO = reader.GetValue(2) == System.DBNull.Value ? 0 : reader.GetDouble(2);
                        linha.VLR_VENDIDO = reader.GetValue(3) == System.DBNull.Value ? 0 : reader.GetDouble(3);

                        resultado.Add(linha);
                    }
                }

                return resultado;
            }
        }

        /// <summary>
        /// Guarda a informação sobre o grupo
        /// </summary>
        public class CSGrupoComercializacao
        {
            #region [ Variaveis ]

            private int m_COD_GRUPO_COMERCIALIZACAO;
            private int m_COD_GRUPO_COMERCIALIZACAO_FILTRADO;
            private string m_DES_GRUPO_COMERCIALIZACAO;
            private string m_DES_GRUPO_COMERCIALIZACAO_FILTRADO;
            private string m_COD_SETOR_BROKER;
            private string m_IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO;

            private static CSGruposComercializacao.CSGrupoComercializacao m_Current;

            #endregion

            #region [ Propriedades ]

            public int COD_GRUPO_COMERCIALIZACAO
            {
                get
                {
                    return m_COD_GRUPO_COMERCIALIZACAO;
                }
                set
                {
                    m_COD_GRUPO_COMERCIALIZACAO = value;
                }
            }

            public int COD_GRUPO_COMERCIALIZACAO_FILTRADO
            {
                get
                {
                    return m_COD_GRUPO_COMERCIALIZACAO_FILTRADO;
                }
                set
                {
                    m_COD_GRUPO_COMERCIALIZACAO_FILTRADO = value;
                }
            }

            public string DES_GRUPO_COMERCIALIZACAO
            {
                get
                {
                    return m_DES_GRUPO_COMERCIALIZACAO;
                }
                set
                {
                    m_DES_GRUPO_COMERCIALIZACAO = value;
                }
            }

            public string DES_GRUPO_COMERCIALIZACAO_FILTRADO
            {
                get
                {
                    return m_DES_GRUPO_COMERCIALIZACAO_FILTRADO;
                }
                set
                {
                    m_DES_GRUPO_COMERCIALIZACAO_FILTRADO = value;
                }
            }

            public string COD_SETOR_BROKER
            {
                get
                {
                    return m_COD_SETOR_BROKER;
                }
                set
                {
                    m_COD_SETOR_BROKER = value;
                }
            }

            public string IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO
            {
                get
                {
                    if (string.IsNullOrEmpty(m_IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO))
                        return "N";

                    return m_IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO;
                }
                set
                {
                    m_IND_GRUPO_COMERCIALIZACAO_EXCLUSIVO_PEDIDO = value;
                }
            }

            /// <summary>
            /// Guarda o grupo atual
            /// </summary>
            public static CSGruposComercializacao.CSGrupoComercializacao Current
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

            public CSGrupoComercializacao()
            {
                this.COD_GRUPO_COMERCIALIZACAO = -1;
                this.DES_GRUPO_COMERCIALIZACAO = "Grupo 99";
                this.COD_SETOR_BROKER = "";
            }

            public override bool Equals(object obj)
            {
                if (obj is CSGruposComercializacao.CSGrupoComercializacao)
                {
                    return ((CSGruposComercializacao.CSGrupoComercializacao)obj).COD_GRUPO_COMERCIALIZACAO == this.COD_GRUPO_COMERCIALIZACAO;
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