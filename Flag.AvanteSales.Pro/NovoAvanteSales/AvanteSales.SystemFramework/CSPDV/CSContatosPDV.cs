#region Using directives

using System;
using System.Collections;
using System.Data;
using System.Data.SqlTypes;
using System.Reflection;
using System.Xml;
using System.IO;
using System.Text;
using AvanteSales.BusinessRules;

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



#endregion

namespace AvanteSales
{
    public class CSContatosPDV : CollectionBase
    {
        #region [ Variaveis ]

        private CSContatoPDV m_Current;

        #endregion

        #region [ Propriedades ]

        /// <summary>
        /// Retorna coleção dos telefones do PDV 
        /// </summary>
        public CSContatosPDV Items
        {
            get
            {
                return this;
            }
        }

        public CSContatosPDV.CSContatoPDV this[int Index]
        {
            get
            {
                return (CSContatosPDV.CSContatoPDV)this.InnerList[Index];
            }
        }

        public CSContatoPDV Current
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

        #region [ Métodos ]

        /// <summary>
        /// Contrutor da classe. Busca os contatos do PDV
        /// </summary>
        public CSContatosPDV(int COD_PDV)
        {
            try
            {
                string sqlQuery =
                    "SELECT COD_CONTATO_PDV, NOM_CONTATO_PDV, DSC_FUNCAO_CONTATO, DSC_EMAIL " +
                    "      ,NUM_TELEFONE, DIA_NIV_CONTATO_PDV, IND_EMISSAO_CHEQUE " +
                    "      ,MES_NIV_CONTATO_PDV " +
                    "  FROM CONTATO_PDV " +
                    " WHERE COD_PDV = ? ";

                SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", COD_PDV);

                // Busca todos os contatos do PDV
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_PDV))
                {

                    while (sqlReader.Read())
                    {
                        CSContatoPDV contatoPDV = new CSContatoPDV();
                        // Preenche a instancia da classe de contato do pdv
                        contatoPDV.COD_CONTATO_PDV = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        contatoPDV.NOM_CONTATO_PDV = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1);
                        contatoPDV.DSC_FUNCAO_CONTATO = sqlReader.GetValue(2) == System.DBNull.Value ? "" : sqlReader.GetString(2);
                        contatoPDV.DSC_EMAIL = sqlReader.GetValue(3) == System.DBNull.Value ? "" : sqlReader.GetString(3);
                        contatoPDV.NUM_TELEFONE = sqlReader.GetValue(4) == System.DBNull.Value ? "" : sqlReader.GetString(4);
                        contatoPDV.DIA_NIV_CONTATO_PDV = sqlReader.GetValue(5) == System.DBNull.Value ? -1 : sqlReader.GetInt32(5);
                        contatoPDV.IND_EMISSAO_CHEQUE = sqlReader.GetValue(6) == System.DBNull.Value ? "Não" : sqlReader.GetString(6).Trim() == "1" ? "Sim" : "Não";
                        contatoPDV.MES_NIV_CONTATO_PDV = sqlReader.GetValue(7) == System.DBNull.Value ? -1 : sqlReader.GetInt32(7);

                        // Busca os lazeres do contato do PDV
                        contatoPDV.LAZERES_CONTATO = new CSContatoPDV.CSLazeresContato(contatoPDV.COD_CONTATO_PDV);

                        // Adciona o contato do PDV na coleção de contatos deste PDV
                        base.InnerList.Add(contatoPDV);
                    }
                    // Fecha o reader
                    sqlReader.Close();
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos contatos do PDV", ex);
            }
        }

        #endregion

        #region [ SubClasses ]

        public class CSContatoPDV
#if ANDROID
 : Java.Lang.Object
#endif
        {
            #region [ Variaveis ]

            private int m_COD_CONTATO_PDV;
            private string m_NOM_CONTATO_PDV;
            private string m_DSC_FUNCAO_CONTATO;
            private string m_DSC_EMAIL;
            private string m_NUM_TELEFONE;
            private int m_DIA_NIV_CONTATO_PDV;
            private int m_MES_NIV_CONTATO_PDV;
            private string m_IND_EMISSAO_CHEQUE;
            private CSContatoPDV.CSLazeresContato m_LAZERES_CONTATO;

            #endregion

            #region [ Propriedades ]

            public int COD_CONTATO_PDV
            {
                get
                {
                    return m_COD_CONTATO_PDV;
                }
                set
                {
                    m_COD_CONTATO_PDV = value;
                }
            }

            public string NOM_CONTATO_PDV
            {
                get
                {
                    return m_NOM_CONTATO_PDV;
                }
                set
                {
                    m_NOM_CONTATO_PDV = value;
                }
            }

            public string DSC_FUNCAO_CONTATO
            {
                get
                {
                    return m_DSC_FUNCAO_CONTATO;
                }
                set
                {
                    m_DSC_FUNCAO_CONTATO = value;
                }
            }

            public string DSC_EMAIL
            {
                get
                {
                    return m_DSC_EMAIL;
                }
                set
                {
                    m_DSC_EMAIL = value;
                }
            }

            public string NUM_TELEFONE
            {
                get
                {
                    return m_NUM_TELEFONE;
                }
                set
                {
                    m_NUM_TELEFONE = value;
                }
            }

            public int DIA_NIV_CONTATO_PDV
            {
                get
                {
                    return m_DIA_NIV_CONTATO_PDV;
                }
                set
                {
                    m_DIA_NIV_CONTATO_PDV = value;
                }
            }
            public int MES_NIV_CONTATO_PDV
            {
                get
                {
                    return m_MES_NIV_CONTATO_PDV;
                }
                set
                {
                    m_MES_NIV_CONTATO_PDV = value;
                }
            }


            public string IND_EMISSAO_CHEQUE
            {
                get
                {
                    return m_IND_EMISSAO_CHEQUE;
                }
                set
                {
                    m_IND_EMISSAO_CHEQUE = value;
                }
            }

            public CSContatoPDV.CSLazeresContato LAZERES_CONTATO
            {
                get
                {
                    return m_LAZERES_CONTATO;
                }
                set
                {
                    m_LAZERES_CONTATO = value;
                }
            }

            #endregion

            #region [ Metodos ]

            public CSContatoPDV()
            {

            }

            #endregion

            #region [ SubClasses ]

            public class CSLazeresContato : CollectionBase
            {

                #region [ Variaveis ]

                #endregion

                #region [ Propriedades ]

                /// <summary>
                /// Retorna coleção dos telefones do PDV 
                /// </summary>
                public CSLazeresContato Items
                {
                    get
                    {
                        return this;
                    }
                }

                public CSLazeresContato.CSLazerContato this[int Index]
                {
                    get
                    {
                        return (CSLazeresContato.CSLazerContato)this.InnerList[Index];
                    }
                }

                #endregion

                #region [ Metodos ]

                public CSLazeresContato(int COD_CONTATO)
                {
                    try
                    {
                        string sqlQuery =
                            "SELECT DISTINCT L.COD_LAZER, TP.COD_TIPO_LAZER, L.DSC_LAZER, TP.DSC_TIPO_LAZER " +
                            "  FROM LAZER_CONTATO LC " +
                            " INNER JOIN LAZER L ON LC.COD_LAZER = L.COD_LAZER " +
                            " INNER JOIN TIPO_LAZER TP ON L.COD_TIPO_LAZER = TP.COD_TIPO_LAZER " +
                            " WHERE COD_CONTATO_PDV = ? ";

                        SQLiteParameter pCOD_CONTATO = new SQLiteParameter("@pCOD_CONTATO", COD_CONTATO);

                        // Busca todos os contatos do PDV
                        using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_CONTATO))
                        {
                            while (sqlReader.Read())
                            {
                                CSLazerContato lc = new CSLazerContato();
                                // Preenche a instancia da classe de lazer do contato
                                lc.COD_LAZER = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                                lc.COD_TIPO_LAZER = sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1);
                                lc.DSC_LAZER = sqlReader.GetValue(2) == System.DBNull.Value ? "" : sqlReader.GetString(2);
                                lc.DSC_TIPO_LAZER = sqlReader.GetValue(3) == System.DBNull.Value ? "" : sqlReader.GetString(3);
                                // Adciona o lazer do contato na coleção de lazeres deste contato
                                base.InnerList.Add(lc);
                            }
                            // Fecha o reader
                            sqlReader.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        CSGlobal.ShowMessage(ex.ToString());
                        throw new Exception("Erro na busca dos contatos do PDV", ex);
                    }
                }

                #endregion

                #region  [ SubClasses ]

                public class CSLazerContato
                {
                    #region [ Variaveis ]

                    private int m_COD_LAZER;
                    private string m_DSC_LAZER;
                    private int m_COD_TIPO_LAZER;
                    private string m_DSC_TIPO_LAZER;

                    #endregion

                    #region [ Propriedades ]

                    public int COD_LAZER
                    {
                        get
                        {
                            return m_COD_LAZER;
                        }
                        set
                        {
                            m_COD_LAZER = value;
                        }
                    }

                    public string DSC_LAZER
                    {
                        get
                        {
                            return m_DSC_LAZER;
                        }
                        set
                        {
                            m_DSC_LAZER = value;
                        }
                    }

                    public int COD_TIPO_LAZER
                    {
                        get
                        {
                            return m_COD_TIPO_LAZER;
                        }
                        set
                        {
                            m_COD_TIPO_LAZER = value;
                        }
                    }

                    public string DSC_TIPO_LAZER
                    {
                        get
                        {
                            return m_DSC_TIPO_LAZER;
                        }
                        set
                        {
                            m_DSC_TIPO_LAZER = value;
                        }
                    }

                    #endregion

                    #region [ Metodos ]

                    public CSLazerContato()
                    {

                    }

                    #endregion
                }

                #endregion
            }

            #endregion
        }

        #endregion
    }
}