using System;
using System.Collections;
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
#endif

namespace AvanteSales
{
    /// <summary>
    /// Guarda a informação sobre o produtoMUR
    /// </summary>
    public class CSPDVProdutoMUR
    {
        #region [ Variaveis ]

        private int m_COD_PRODUTO;
        private int m_COD_PDV;
        private bool m_IND_VENDA_MES;
        private bool m_IND_VENDA_ULTIMA_VISITA;

        #endregion

        #region [ Propriedades ]

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

        public int COD_PDV
        {
            get
            {
                return m_COD_PDV;
            }
            set
            {
                m_COD_PDV = value;
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

        #endregion

        #region [ Metodos ]

        public CSPDVProdutoMUR(int COD_PDV, int COD_PRODUTO)
        {
            try
            {
                string sqlQuery =
                    "SELECT COD_PRODUTO, COD_PDV, IND_VENDA_MES, IND_VENDA_ULTIMA_VISITA " +
                    "  FROM PDV_PRODUTO_MUR " +
                    " WHERE COD_PDV = ? " +
                    "   AND COD_PRODUTO = ? ";

                SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", COD_PDV);
                SQLiteParameter pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", COD_PRODUTO);

                // Busca todos os produtosMUR
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_PDV, pCOD_PRODUTO))
                {
                    while (sqlReader.Read())
                    {
                        // Preenche a instancia da classe dos preços dos produtos
                        this.COD_PRODUTO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        this.COD_PDV = sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1);
                        this.IND_VENDA_MES = sqlReader.GetValue(2) == System.DBNull.Value ? false : bool.Parse(sqlReader.GetValue(2).ToString());
                        this.IND_VENDA_ULTIMA_VISITA = sqlReader.GetValue(3) == System.DBNull.Value ? false : bool.Parse(sqlReader.GetValue(3).ToString());
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos PDVProdutosMUR.", ex);
            }
        }
        #endregion
    }
}