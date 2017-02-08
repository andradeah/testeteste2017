#region Using directives

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

using AvanteSales.BusinessRules;

#endregion

namespace AvanteSales
{
    /// <summary>
    /// Retorna coleção dos Emissores do PDV 
    /// </summary>
    public class CSEmissoresPDV : CollectionBase, IDisposable
    {
        #region [ Variaveis ]

        #endregion

        #region [ Propriedades ]

        /// <summary>
        /// Retorna coleção dos emissores do PDV 
        /// </summary>
        public CSEmissoresPDV Items
        {
            get
            {
                return this;
            }
        }

        public CSEmissoresPDV.CSEmissorPDV this[int Index]
        {
            get
            {
                return (CSEmissoresPDV.CSEmissorPDV)this.InnerList[Index];
            }
        }

        #endregion

        #region [ Metodos ]

        /// <summary>
        /// Contrutor da classe. Busca os Emissor do PDV
        /// </summary>
        public CSEmissoresPDV(int COD_PDV)
        {
            try
            {
                string sqlQuery;

                // Se for Broker...
                if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2)
                {
                    sqlQuery = " SELECT DISTINCT " +
                               "        T2.CDCLI AS COD_PDV, " +
                               "        T2.DSENDER " +
                               "   FROM BRK_ECLIENTENT T1 " +
                               "   JOIN BRK_ECLIENTBAS T2 " +
                               "     ON T2.CDCLI = T1.CDCLIENTR " +
                               "  WHERE T1.CDCLI = ? " +
                               "    AND T1.CDFUNCPARC = 'WE' ";
                }
                else
                {
                    sqlQuery = " SELECT P.COD_PDV, " +
                               "        P.DSC_RAZAO_SOCIAL " +
                               "   FROM PDV P " +
                               "  WHERE P.COD_PDV = ? ";
                }

                SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", COD_PDV.ToString().PadLeft(10, '0'));

                // Busca todos os emissores do PDV                                                                                       
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_PDV))
                {
                    while (sqlReader.Read())
                    {
                        CSEmissorPDV emissorPDV = new CSEmissorPDV();

                        // Preenche a instancia da classe de emissor do pdv                   
                        emissorPDV.COD_PDV_SOLDTO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : Convert.ToInt32(sqlReader.GetValue(0));
                        emissorPDV.DSC_RAZAO_SOCIAL = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1);

                        // Adciona o emissor do PDV na coleção de emissores deste PDV
                        base.InnerList.Add(emissorPDV);

                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos emissores do PDV", ex);
            }
        }

        public void Dispose()
        {
            this.InnerList.Clear();
            this.InnerList.TrimToSize();
        }

        #endregion

        #region [ SubClasses ]

        /// <summary>
        /// Guarda a informaçõa sobre emissor do PDV
        /// </summary>
        public class CSEmissorPDV
        {
            #region [ Variaveis ]

            private int m_COD_PDV_SOLDTO;
            private string m_DSC_RAZAO_SOCIAL;

            #endregion

            #region [ Propriedades ]

            /// <summary>
            /// Guarda o codigo do cliente emisssor
            /// </summary>
            public int COD_PDV_SOLDTO
            {
                get
                {
                    return m_COD_PDV_SOLDTO;
                }
                set
                {
                    m_COD_PDV_SOLDTO = value;
                }
            }
            /// <summary>
            /// Guarda a Razão Social 
            /// </summary>
            public string DSC_RAZAO_SOCIAL
            {
                get
                {
                    return m_DSC_RAZAO_SOCIAL;
                }
                set
                {
                    m_DSC_RAZAO_SOCIAL = value;
                }
            }

            #endregion

            #region [ Metodos ]

            public CSEmissorPDV()
            {

            }

            #endregion
        }

        #endregion
    }
}
