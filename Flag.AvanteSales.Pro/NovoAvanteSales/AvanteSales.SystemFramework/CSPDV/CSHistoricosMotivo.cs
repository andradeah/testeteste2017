#region Using directives

using System;
using System.Collections;
using System.Data;
using System.Data.SqlTypes;
using System.Reflection;
using System.Xml;
using System.IO;
using System.Text;
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

using AvanteSales.BusinessRules;

#endregion

namespace AvanteSales
{
    public class CSHistoricosMotivo : CollectionBase
    {
        #region [ Variaveis ]

        private CSHistoricosMotivo.CSHistoricoMotivo m_Current;

        #endregion

        #region [ Propriedades ]

        public CSHistoricosMotivo.CSHistoricoMotivo Current
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

        public CSHistoricosMotivo(int COD_PDV)
        {
            try
            {
                //string sqlQuery =
                //    "SELECT COD_PDV, COD_TIPO_MOTIVO, COD_MOTIVO, DAT_HISTORICO_MOTIVO " +
                //    "  FROM HISTORICO_MOTIVO " +
                //    " WHERE COD_PDV = ? " +
                //    "   AND DAT_HISTORICO_MOTIVO BETWEEN ? AND ? ";

                string sqlQuery =
                    "SELECT COD_PDV, COD_TIPO_MOTIVO, COD_MOTIVO, DAT_HISTORICO_MOTIVO, NUM_LATITUDE_LOCALIZACAO, NUM_LONGITUDE_LOCALIZACAO " +
                    "  FROM HISTORICO_MOTIVO " +
                    " WHERE COD_PDV = ? " +
                    "   AND date(DAT_HISTORICO_MOTIVO) = date('now') ";

                SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", COD_PDV);
                //SQLiteParameter pDATA_HISTORICO_INICIAL = new SQLiteParameter("@DAT_HISTORICO_INICIAL", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0));
                SQLiteParameter pDATA_HISTORICO_INICIAL = new SQLiteParameter("@DAT_HISTORICO_INICIAL", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
                SQLiteParameter pDATA_HISTORICO_FINAL = new SQLiteParameter("@DAT_HISTORICO_FINAL", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59));

                // Busca todos os PDVs
                //using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_PDV, pDATA_HISTORICO_INICIAL, pDATA_HISTORICO_FINAL))
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_PDV, pDATA_HISTORICO_INICIAL))
                {
                    while (sqlReader.Read())
                    {
                        CSHistoricosMotivo.CSHistoricoMotivo histmot = new CSHistoricosMotivo.CSHistoricoMotivo();

                        // Preenche a instancia da classe historico motivo
                        histmot.COD_PDV = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        histmot.COD_TIPO_MOTIVO = sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1);
                        histmot.COD_MOTIVO = sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2);
                        histmot.DAT_HISTORICO_MOTIVO = sqlReader.GetValue(3) == System.DBNull.Value ? new DateTime(1900, 1, 1) : sqlReader.GetDateTime(3);
                        histmot.NUM_LATITUDE_LOCALIZACAO = sqlReader.GetValue(4) == System.DBNull.Value ? string.Empty : sqlReader.GetString(4);
                        histmot.NUM_LONGITUDE_LOCALIZACAO = sqlReader.GetValue(5) == System.DBNull.Value ? string.Empty : sqlReader.GetString(5);
                        //histmot.DSC_NOME_FOTO = sqlReader.GetValue(4) == System.DBNull.Value ? string.Empty : sqlReader.GetString(4);
                        histmot.MOTIVO = CSMotivos.GetMotivo(histmot.COD_MOTIVO);
                        histmot.STATE = ObjectState.INALTERADO;

                        // Adciona o historico motivo do PDV na coleção de historicos motivo do PDV
                        base.InnerList.Add(histmot);
                    }
                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos historicos de motivo do PDV", ex);
            }
        }

        /// <summary>
        /// Adiciona um historico de motico na coleção
        /// </summary>
        /// <param name="c">Instacia do motivo a ser adcionada</param>
        /// <returns>return a posição do motivo adicionado na coleção</returns>
        public int Add(CSHistoricoMotivo motivo)
        {
            // Adiciona na coleção
            int idx = base.InnerList.Add(motivo);
            // Retorna a posição dele na coleção
            return idx;
        }

        /// <summary>
        /// Salva os dados na coleção no banco
        /// </summary>
        public bool Flush()
        {
            StringBuilder sqlQueryInsert = new StringBuilder();
            sqlQueryInsert.Append("INSERT INTO HISTORICO_MOTIVO ");
            sqlQueryInsert.AppendLine("(COD_PDV, COD_TIPO_MOTIVO, COD_MOTIVO, DAT_HISTORICO_MOTIVO, COD_EMPREGADO, NUM_LATITUDE_LOCALIZACAO, NUM_LONGITUDE_LOCALIZACAO ");
            sqlQueryInsert.AppendLine(CSEmpresa.ColunaExiste("HISTORICO_MOTIVO", "NUM_CPF_EMPREGADO") ? ",NUM_CPF_EMPREGADO)" : ")");
            sqlQueryInsert.AppendLine("VALUES(?,?,?,?,?,?,?");
            sqlQueryInsert.AppendLine(CSEmpresa.ColunaExiste("HISTORICO_MOTIVO", "NUM_CPF_EMPREGADO") ? ",?)" : ")");

            string sqlQueryUpdate =
                "UPDATE HISTORICO_MOTIVO SET COD_MOTIVO=?, DAT_HISTORICO_MOTIVO=? WHERE COD_PDV=? AND date(DAT_HISTORICO_MOTIVO)=date(?) AND COD_TIPO_MOTIVO=?";
            //"UPDATE HISTORICO_MOTIVO SET COD_MOTIVO=?, DAT_HISTORICO_MOTIVO=? WHERE COD_PDV=? AND DAT_HISTORICO_MOTIVO=? AND COD_TIPO_MOTIVO=?";

            string sqlQueryDelete =
                "DELETE FROM HISTORICO_MOTIVO WHERE COD_PDV=? AND date(DAT_HISTORICO_MOTIVO)=date(?)";
            //"DELETE FROM HISTORICO_MOTIVO WHERE COD_PDV=? AND DAT_HISTORICO_MOTIVO=? AND COD_TIPO_MOTIVO=?";

            // Varre a coleção procurando os objetos a serem persistidos
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                CSHistoricoMotivo hismot = (CSHistoricoMotivo)base.InnerList[i];
                //}
                //foreach (CSHistoricoMotivo hismot in base.InnerList)
                //{
                // Criar os parametros de salvamento
                SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", hismot.COD_PDV);
                SQLiteParameter pCOD_TIPO_MOTIVO = new SQLiteParameter("@COD_TIPO_MOTIVO", hismot.COD_TIPO_MOTIVO);
                SQLiteParameter pCOD_MOTIVO = new SQLiteParameter("@COD_MOTIVO", hismot.COD_MOTIVO);
                SQLiteParameter pDAT_HISTORICO_MOTIVO = new SQLiteParameter("@DAT_HISTORICO_MOTIVO", hismot.DAT_HISTORICO_MOTIVO.ToString("yyyy-MM-dd HH:mm:ss"));
                SQLiteParameter pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                SQLiteParameter pDAT_ALTERACAO = new SQLiteParameter("@DAT_ALTERACAO", hismot.DAT_ALTERACAO);
                //SqliteParameter pDSC_NOME_FOTO = new SQLiteParameter("@DSC_NOME_FOTO", hismot.DSC_NOME_FOTO);
                SqliteParameter pNUM_CPF_EMPREGADO = new SQLiteParameter("@NUM_CPF_EMPREGADO", CSEmpregados.Current.NUM_CPF_EMPREGADO);
                SqliteParameter pNUM_LATITUDE_LOCALIZACAO = new SQLiteParameter("@NUM_LATITUDE_LOCALIZACAO", hismot.NUM_LATITUDE_LOCALIZACAO);
                SQLiteParameter pNUM_LONGITUDE_LOCALIZACAO = new SQLiteParameter("@NUM_LONGITUDE_LOCALIZACAO", hismot.NUM_LONGITUDE_LOCALIZACAO);

                switch (hismot.STATE)
                {
                    case ObjectState.NOVO:
                        // Executa a query salvando os dados
                        CSDataAccess.Instance.ExecuteScalar(sqlQueryInsert.ToString(), pCOD_PDV, pCOD_TIPO_MOTIVO, pCOD_MOTIVO, pDAT_HISTORICO_MOTIVO, pCOD_EMPREGADO, pNUM_LATITUDE_LOCALIZACAO, pNUM_LONGITUDE_LOCALIZACAO, CSEmpresa.ColunaExiste("HISTORICO_MOTIVO", "NUM_CPF_EMPREGADO") ? pNUM_CPF_EMPREGADO : null);
                        // Muda o state dele para ObjectState.SALVO
                        hismot.STATE = ObjectState.SALVO;
                        break;
                    case ObjectState.ALTERADO:
                        // Executa a query salvando os dados
                        CSDataAccess.Instance.ExecuteScalar(sqlQueryUpdate, pCOD_MOTIVO, pDAT_ALTERACAO, pCOD_PDV, pDAT_HISTORICO_MOTIVO, pCOD_TIPO_MOTIVO);

                        // Muda o state dele para ObjectState.SALVO
                        hismot.STATE = ObjectState.SALVO;
                        hismot.DAT_HISTORICO_MOTIVO = hismot.DAT_ALTERACAO;
                        break;
                    case ObjectState.DELETADO:
                        // Executa a query apagando os dados
                        //CSDataAccess.Instance.ExecuteScalar(sqlQueryDelete, pCOD_PDV, pDAT_HISTORICO_MOTIVO, pCOD_TIPO_MOTIVO);
                        CSDataAccess.Instance.ExecuteScalar(sqlQueryDelete, pCOD_PDV, pDAT_HISTORICO_MOTIVO);
                        // Remove o historico da coleção
                        this.InnerList.Remove(hismot);
                        if (base.Count == 0)
                            return true;

                        break;
                }
            }

            return true;
        }

        // Retorna a coleção dos historicos de motivo do PDV
        public CSHistoricosMotivo Items
        {
            get
            {
                return this;
            }
        }

        #endregion

        #region [ SubClasses ]

        /// <summary>
        /// Summary description for CSEmpregado.
        /// </summary>
        public class CSHistoricoMotivo
        {
            #region [ Variaveis ]

            private int m_COD_TIPO_MOTIVO;
            private DateTime m_DAT_HISTORICO_MOTIVO = new DateTime(1900, 1, 1);
            private int m_COD_MOTIVO;
            private int m_COD_PDV;
            private CSMotivos.CSMotivo m_MOTIVO;
            private DateTime m_DAT_ALTERACAO;
            private string m_NUM_LATITUDE_LOCALIZACAO;
            private string m_NUM_LONGITUDE_LOCALIZACAO;
            // O default deste state deve ser novo para que quando se crie um ele ja venha com este estado
            // o estado dos objetos que estao sendo buscados devem ser alterados no momento do preenchimento
            private ObjectState m_STATE = ObjectState.NOVO;

            #endregion

            #region [ Propriedades ]

            /// <summary>
            /// Guarda o codigo do tipo do motivo
            /// </summary>
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

            /// <summary>
            /// Guarda a data do historico do motivo
            /// </summary>
            public DateTime DAT_HISTORICO_MOTIVO
            {
                get
                {
                    return m_DAT_HISTORICO_MOTIVO;
                }
                set
                {
                    m_DAT_HISTORICO_MOTIVO = value;
                }
            }

            /// <summary>
            /// Guarda o codigo do motivo
            /// </summary>
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

            /// <summary>
            /// Guarda o codigo do PDV
            /// </summary>
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

            /// <summary>
            /// Guarda o codigo do PDV
            /// </summary>
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

            /// <summary>
            /// Guarda o state do objeto
            /// </summary>
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

            public DateTime DAT_ALTERACAO
            {
                get
                {
                    return m_DAT_ALTERACAO;
                }
                set
                {
                    m_DAT_ALTERACAO = value;
                }
            }

            public string NUM_LATITUDE_LOCALIZACAO
            {
                get
                {
                    return m_NUM_LATITUDE_LOCALIZACAO;
                }
                set
                {
                    m_NUM_LATITUDE_LOCALIZACAO = value;
                }
            }

            public string NUM_LONGITUDE_LOCALIZACAO
            {
                get
                {
                    return m_NUM_LONGITUDE_LOCALIZACAO;
                }
                set
                {
                    m_NUM_LONGITUDE_LOCALIZACAO = value;
                }
            }

            #endregion

            #region [ Metodos ]

            public CSHistoricoMotivo()
            {
            }

            public override string ToString()
            {
                string ret = "";
                try
                {
                    // Get the type of MyClass1.
                    Type myType = this.GetType();
                    // Get the members associated with MyClass1.
                    PropertyInfo[] myProps = myType.GetProperties();
                    foreach (PropertyInfo prop in myProps)
                    {
                        object propval;
                        try
                        {
                            propval = myType.GetProperty(prop.Name).GetValue(this, null);
                            ret += prop.Name + ": " + propval.ToString() + "\r\n";
                        }
                        catch (SystemException ex)
                        {
                            ret += prop.Name + ": " + ex.Message + "\r\n";
                        }
                    }

                    return ret;
                }
                catch (Exception e)
                {
                    throw new Exception("An exception occurred...", e);
                }
            }

            #endregion
        }

        #endregion
    }
}