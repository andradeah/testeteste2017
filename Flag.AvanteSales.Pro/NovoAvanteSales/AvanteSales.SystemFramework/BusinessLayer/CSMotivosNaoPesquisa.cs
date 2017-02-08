#region Using directives

using System;
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
using System.Collections;

#endregion

namespace AvanteSales.BusinessRules
{
    /// <summary>
    /// Summary description for CSMotivosNaoPesquisa.
    /// </summary>
    public class CSMotivosNaoPesquisa : CollectionBase
    {
        #region [ Variáveis ]

        private CSMotivosNaoPesquisa.CSMotivoNaoPesquisa current;

        #endregion

        #region [ Propriedades ]

        public CSMotivosNaoPesquisa.CSMotivoNaoPesquisa Current
        {
            get
            {
                return current;
            }
            set
            {
                current = value;
            }
        }

        #endregion

        #region [ Métodos Públicos ]

        public CSMotivosNaoPesquisa(int COD_PDV, bool mercado)
        {
            string sqlQuery = null;
            SQLiteParameter pCOD_PDV = null;

            try
            {
                sqlQuery =
                    "SELECT COD_PESQUISA_MERC, COD_PDV, COD_MOTIVO " +
                    "      ,COD_TIPO_MOTIVO, DAT_COLETA " +
                    "  FROM MOTIVO_NAO_PESQUISA " +
                    " WHERE COD_PDV = ? ";

                pCOD_PDV = new SQLiteParameter("@COD_PDV", COD_PDV);

                // Busca todos os PDVs
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_PDV))
                {
                    while (sqlReader.Read())
                    {
                        CSMotivosNaoPesquisa.CSMotivoNaoPesquisa motivo = new CSMotivosNaoPesquisa.CSMotivoNaoPesquisa();

                        motivo.COD_PESQUISA = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        motivo.COD_PDV = sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1);
                        motivo.COD_MOTIVO = sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2);
                        motivo.COD_TIPO_MOTIVO = sqlReader.GetValue(3) == System.DBNull.Value ? -1 : sqlReader.GetInt32(3);
                        motivo.DAT_COLETA = sqlReader.GetValue(4) == System.DBNull.Value ? new DateTime(1900, 1, 1) : sqlReader.GetDateTime(4);
                        motivo.MOTIVO = CSMotivos.GetMotivo(motivo.COD_MOTIVO);
                        motivo.STATE = ObjectState.INALTERADO;

                        // Adiciona o motivo na coleção de motivos
                        base.InnerList.Add(motivo);
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos motivos de não pesquisa do PDV", ex);
            }
        }

        public CSMotivosNaoPesquisa(int COD_PDV)
        {
            string sqlQuery = null;
            SQLiteParameter pCOD_PDV = null;

            try
            {
                sqlQuery =
                    "SELECT COD_PESQUISA_PIV, COD_PDV, COD_MOTIVO " +
                    "      ,COD_TIPO_MOTIVO, COD_EMPREGADO, DAT_COLETA " +
                    "  FROM MOTIVO_NAORESP_PESQ " +
                    " WHERE COD_PDV = ? ";

                pCOD_PDV = new SQLiteParameter("@COD_PDV", COD_PDV);

                // Busca todos os PDVs
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_PDV))
                {
                    while (sqlReader.Read())
                    {
                        CSMotivosNaoPesquisa.CSMotivoNaoPesquisa motivo = new CSMotivosNaoPesquisa.CSMotivoNaoPesquisa();

                        motivo.COD_PESQUISA = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        motivo.COD_PDV = sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1);
                        motivo.COD_MOTIVO = sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2);
                        motivo.COD_TIPO_MOTIVO = sqlReader.GetValue(3) == System.DBNull.Value ? -1 : sqlReader.GetInt32(3);
                        motivo.COD_EMPREGADO = sqlReader.GetValue(4) == System.DBNull.Value ? -1 : sqlReader.GetInt32(4);
                        motivo.DAT_COLETA = sqlReader.GetValue(5) == System.DBNull.Value ? new DateTime(1900, 1, 1) : sqlReader.GetDateTime(5);
                        motivo.MOTIVO = CSMotivos.GetMotivo(motivo.COD_MOTIVO);
                        motivo.STATE = ObjectState.INALTERADO;

                        // Adiciona o motivo na coleção de motivos
                        base.InnerList.Add(motivo);
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos motivos de não pesquisa do PDV", ex);
            }
        }

        /// <summary>
        /// Salva os dados na coleção no banco
        /// </summary>
        public bool Flush()
        {
            // Criar os parametros de salvamento
            SQLiteParameter pCOD_PESQUISA = null;
            SQLiteParameter pCOD_PDV = null;
            SQLiteParameter pCOD_MOTIVO = null;
            SQLiteParameter pCOD_TIPO_MOTIVO = null;
            SQLiteParameter pCOD_EMPREGADO = null;
            SQLiteParameter pDAT_COLETA = null;

            string sqlQueryInsert =
                "INSERT INTO MOTIVO_NAORESP_PESQ " +
                "  (COD_PESQUISA_PIV, COD_PDV, COD_MOTIVO, COD_TIPO_MOTIVO, COD_EMPREGADO, DAT_COLETA) " +
                "  VALUES (?,?,?,?,?,?) ";

            string sqlQueryUpdate =
                "UPDATE MOTIVO_NAORESP_PESQ " +
                "   SET COD_MOTIVO = ? " +
                "      ,DAT_COLETA = ? " +
                " WHERE COD_PESQUISA_PIV = ? " +
                "   AND COD_PDV = ? " +
                "   AND COD_TIPO_MOTIVO = ? ";

            string sqlQueryDelete =
                "DELETE FROM MOTIVO_NAORESP_PESQ " +
                " WHERE COD_PESQUISA_PIV = ? " +
                "   AND COD_PDV = ? " +
                "   AND COD_TIPO_MOTIVO = ? ";

            // Varre a coleção procurando os objetos a serem persistidos
            foreach (CSMotivoNaoPesquisa motivo in base.InnerList)
            {
                switch (motivo.STATE)
                {
                    case ObjectState.NOVO:
                        pCOD_PESQUISA = new SQLiteParameter("@COD_PESQUISA_PIV", motivo.COD_PESQUISA);
                        pCOD_PDV = new SQLiteParameter("@COD_PDV", motivo.COD_PDV);
                        pCOD_MOTIVO = new SQLiteParameter("@COD_MOTIVO", motivo.COD_MOTIVO);
                        pCOD_TIPO_MOTIVO = new SQLiteParameter("@COD_TIPO_MOTIVO", motivo.COD_TIPO_MOTIVO);
                        pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                        pDAT_COLETA = new SQLiteParameter("@DAT_COLETA", motivo.DAT_COLETA);

                        // Executa a query salvando os dados
                        CSDataAccess.Instance.ExecuteScalar(sqlQueryInsert, pCOD_PESQUISA, pCOD_PDV, pCOD_MOTIVO, pCOD_TIPO_MOTIVO, pCOD_EMPREGADO, pDAT_COLETA);

                        // Muda o state dele para ObjectState.SALVO
                        motivo.STATE = ObjectState.SALVO;
                        break;

                    case ObjectState.ALTERADO:
                        pCOD_PESQUISA = new SQLiteParameter("@COD_PESQUISA_PIV", motivo.COD_PESQUISA);
                        pCOD_PDV = new SQLiteParameter("@COD_PDV", motivo.COD_PDV);
                        pCOD_MOTIVO = new SQLiteParameter("@COD_MOTIVO", motivo.COD_MOTIVO);
                        pDAT_COLETA = new SQLiteParameter("@DAT_COLETA", motivo.DAT_COLETA);
                        pCOD_TIPO_MOTIVO = new SQLiteParameter("@COD_TIPO_MOTIVO", motivo.COD_TIPO_MOTIVO);

                        // Executa a query salvando os dados
                        CSDataAccess.Instance.ExecuteScalar(sqlQueryUpdate, pCOD_MOTIVO, pDAT_COLETA, pCOD_PESQUISA, pCOD_PDV, pCOD_TIPO_MOTIVO);

                        // Muda o state dele para ObjectState.SALVO
                        motivo.STATE = ObjectState.SALVO;
                        break;

                    case ObjectState.DELETADO:
                        pCOD_PESQUISA = new SQLiteParameter("@COD_PESQUISA_PIV", motivo.COD_PESQUISA);
                        pCOD_PDV = new SQLiteParameter("@COD_PDV", motivo.COD_PDV);
                        pCOD_TIPO_MOTIVO = new SQLiteParameter("@COD_TIPO_MOTIVO", motivo.COD_TIPO_MOTIVO);

                        // Executa a query apagando os dados
                        CSDataAccess.Instance.ExecuteScalar(sqlQueryDelete, pCOD_PESQUISA, pCOD_PDV, pCOD_TIPO_MOTIVO);

                        // Remove o historico da coleção
                        this.InnerList.Remove(motivo);
                        break;
                }
            }

            return true;
        }

        public int Add(CSMotivosNaoPesquisa.CSMotivoNaoPesquisa motivo)
        {
            return this.InnerList.Add(motivo);
        }

        // Retorna a coleção dos historicos de motivo do PDV
        public CSMotivosNaoPesquisa Items
        {
            get
            {
                return this;
            }
        }

        #endregion

        #region [ SubClasses ]

        /// <summary>
        /// Summary description for CSMotivoNaoPesquisa.
        /// </summary>
        public class CSMotivoNaoPesquisa
        {
            #region [ Variáveis ]

            private int m_COD_PESQUISA;
            private int m_COD_PDV;
            private int m_COD_MOTIVO;
            private int m_COD_TIPO_MOTIVO;
            private int m_COD_EMPREGADO;
            private DateTime m_DAT_COLETA;
            private CSMotivos.CSMotivo m_MOTIVO;
            private ObjectState m_STATE;

            #endregion

            #region [ Propriedades ]

            public int COD_PESQUISA
            {
                get
                {
                    return m_COD_PESQUISA;
                }
                set
                {
                    m_COD_PESQUISA = value;
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

            public int COD_MOTIVO
            {
                get
                {
                    return m_COD_MOTIVO;
                }
                set
                {
                    m_COD_MOTIVO = value;
                }
            }

            public int COD_TIPO_MOTIVO
            {
                get
                {
                    return m_COD_TIPO_MOTIVO;
                }
                set
                {
                    m_COD_TIPO_MOTIVO = value;
                }
            }

            public int COD_EMPREGADO
            {
                get
                {
                    return m_COD_EMPREGADO;
                }
                set
                {
                    m_COD_EMPREGADO = value;
                }
            }

            public DateTime DAT_COLETA
            {
                get
                {
                    return m_DAT_COLETA;
                }
                set
                {
                    m_DAT_COLETA = value;
                }
            }

            public CSMotivos.CSMotivo MOTIVO
            {
                get
                {
                    return m_MOTIVO;
                }
                set
                {
                    m_MOTIVO = value;
                }
            }

            public ObjectState STATE
            {
                get
                {
                    return m_STATE;
                }
                set
                {
                    m_STATE = value;
                }
            }

            #endregion

            #region [ Métodos Públicos ]

            public CSMotivoNaoPesquisa()
            {
                this.STATE = ObjectState.NOVO;
            }

            #endregion
        }

        #endregion

        public bool FlushMercado()
        {
            // Criar os parametros de salvamento
            SQLiteParameter pCOD_PESQUISA = null;
            SQLiteParameter pCOD_PDV = null;
            SQLiteParameter pCOD_MOTIVO = null;
            SQLiteParameter pCOD_TIPO_MOTIVO = null;
            SQLiteParameter pDAT_COLETA = null;
            SqliteParameter pCOD_EMPREGADO = null;

            string sqlQueryInsert =
                "INSERT INTO MOTIVO_NAO_PESQUISA " +
                "  (COD_PESQUISA_MERC, COD_PDV, COD_MOTIVO, COD_TIPO_MOTIVO, DAT_COLETA, COD_EMPREGADO) " +
                "  VALUES (?,?,?,?,?,?) ";

            string sqlQueryUpdate =
                "UPDATE MOTIVO_NAO_PESQUISA " +
                "   SET COD_MOTIVO = ? " +
                "      ,DAT_COLETA = ? " +
                " WHERE COD_PESQUISA_MERC = ? " +
                "   AND COD_PDV = ? " +
                "   AND COD_TIPO_MOTIVO = ? ";

            string sqlQueryDelete =
                "DELETE FROM MOTIVO_NAO_PESQUISA " +
                " WHERE COD_PESQUISA_MERC = ? " +
                "   AND COD_PDV = ? " +
                "   AND COD_TIPO_MOTIVO = ? ";

            // Varre a coleção procurando os objetos a serem persistidos
            foreach (CSMotivoNaoPesquisa motivo in base.InnerList)
            {
                switch (motivo.STATE)
                {
                    case ObjectState.NOVO:
                        pCOD_PESQUISA = new SQLiteParameter("@COD_PESQUISA_MERC", motivo.COD_PESQUISA);
                        pCOD_PDV = new SQLiteParameter("@COD_PDV", motivo.COD_PDV);
                        pCOD_MOTIVO = new SQLiteParameter("@COD_MOTIVO", motivo.COD_MOTIVO);
                        pCOD_TIPO_MOTIVO = new SQLiteParameter("@COD_TIPO_MOTIVO", motivo.COD_TIPO_MOTIVO);
                        pDAT_COLETA = new SQLiteParameter("@DAT_COLETA", motivo.DAT_COLETA);
                        pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);

                        // Executa a query salvando os dados
                        CSDataAccess.Instance.ExecuteScalar(sqlQueryInsert, pCOD_PESQUISA, pCOD_PDV, pCOD_MOTIVO, pCOD_TIPO_MOTIVO, pDAT_COLETA, pCOD_EMPREGADO);

                        // Muda o state dele para ObjectState.SALVO
                        motivo.STATE = ObjectState.SALVO;
                        break;

                    case ObjectState.ALTERADO:
                        pCOD_PESQUISA = new SQLiteParameter("@COD_PESQUISA_MERC", motivo.COD_PESQUISA);
                        pCOD_PDV = new SQLiteParameter("@COD_PDV", motivo.COD_PDV);
                        pCOD_MOTIVO = new SQLiteParameter("@COD_MOTIVO", motivo.COD_MOTIVO);
                        pDAT_COLETA = new SQLiteParameter("@DAT_COLETA", motivo.DAT_COLETA);
                        pCOD_TIPO_MOTIVO = new SQLiteParameter("@COD_TIPO_MOTIVO", motivo.COD_TIPO_MOTIVO);

                        // Executa a query salvando os dados
                        CSDataAccess.Instance.ExecuteScalar(sqlQueryUpdate, pCOD_MOTIVO, pDAT_COLETA, pCOD_PESQUISA, pCOD_PDV, pCOD_TIPO_MOTIVO);

                        // Muda o state dele para ObjectState.SALVO
                        motivo.STATE = ObjectState.SALVO;
                        break;

                    case ObjectState.DELETADO:
                        pCOD_PESQUISA = new SQLiteParameter("@COD_PESQUISA_MERC", motivo.COD_PESQUISA);
                        pCOD_PDV = new SQLiteParameter("@COD_PDV", motivo.COD_PDV);
                        pCOD_TIPO_MOTIVO = new SQLiteParameter("@COD_TIPO_MOTIVO", motivo.COD_TIPO_MOTIVO);

                        // Executa a query apagando os dados
                        CSDataAccess.Instance.ExecuteScalar(sqlQueryDelete, pCOD_PESQUISA, pCOD_PDV, pCOD_TIPO_MOTIVO);

                        // Remove o historico da coleção
                        this.InnerList.Remove(motivo);
                        break;
                }
            }

            return true;
        }
    }
}