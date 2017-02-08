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
namespace AvanteSales.BusinessRules
{
    public class CSPesquisasMarketing : CollectionBase, IDisposable
    {
        #region [ Variáveis ]

        private CSPesquisasMarketing.CSPesquisaMarketing current = null;
        private CSPDVs.CSPDV pdv = null;

        #endregion

        #region [ Propriedades ]

        public CSPesquisasMarketing.CSPesquisaMarketing Current
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

        public CSPDVs.CSPDV Pdv
        {
            get
            {
                return pdv;
            }
            set
            {
                pdv = value;
            }
        }

        #endregion

        #region [ Metodos ]

        /// <summary>
        /// Contrutor da classe. 
        /// </summary>
        public CSPesquisasMarketing(CSPDVs.CSPDV pdv)
        {
            this.Pdv = pdv;

            CSPesquisaMarketing pesquisaPIV = null;
            SQLiteParameter pCOD_CATEGORIA_PDV = null;
            SQLiteParameter pCOD_PDV = null;
            string sqlQuery = null;
            int m_CODPESQUISA;

            try
            {
                pesquisaPIV = new CSPesquisaMarketing();

                // busca dados necessarios para preencher as tabelas PESQUISA_PIV, PERGUNTA_PESQUISA_PIV, PERGUNTA_CONFIG_OPCAO
                sqlQuery =
                    "SELECT T1.COD_PESQUISA_PIV, T1.DSC_PESQUISA_PIV, T1.DATINI_PESQUISA_PIV " +
                    "      ,T1.DATFIM_PESQUISA_PIV, T4.COD_CATEGORIA,T2.COD_PERGUNTA_PIV " +
                    "      ,T2.DSC_PERGUNTA_PIV, T2.TIP_RESP_PERGUNTA_PIV, T2.VAL_FAIXAINI_PIV " +
                    "      ,T2.VAL_FAIXAFIM_PIV, T3.DSC_OPCAORESPOSTA1, T3.DSC_OPCAORESPOSTA2 " +
                    "      ,T3.DSC_OPCAORESPOSTA3, T3.DSC_OPCAORESPOSTA4, T3.DSC_OPCAORESPOSTA5 " +
                    "  FROM PESQUISA_PIV T1 " +
                    "  JOIN PERGUNTA_PESQUISA_PIV T2 ON T1.COD_PESQUISA_PIV = T2.COD_PESQUISA_PIV " +
                    "  LEFT JOIN PERGUNTA_CONFIG_OPCAO T3 ON T2.COD_PESQUISA_PIV = T3.COD_PESQUISA_PIV " +
                    "   AND T2.COD_PERGUNTA_PIV = T3.COD_PERGUNTA_PIV " +
                    "  LEFT JOIN PESQUISA_PIV_CATEGORIA T4 ON T1.COD_PESQUISA_PIV = T4.COD_PESQUISA_PIV " +
                    "   AND T4.COD_CATEGORIA = ? " +
                    " WHERE date('now') BETWEEN T1.DATINI_PESQUISA_PIV AND T1.DATFIM_PESQUISA_PIV " +
                    "   AND NOT EXISTS(SELECT T5.COD_PESQUISA_PIV " +
                    "                    FROM RESPOSTA_PESQUISA T5 " +
                    "                   WHERE T5.COD_PESQUISA_PIV = T1.COD_PESQUISA_PIV " +
                    "                     AND T5.COD_PDV = ?) " +
                    "   AND (T1.COD_PESQUISA_PIV NOT IN (SELECT DISTINCT COD_PESQUISA_PIV FROM PESQUISA_PIV_CATEGORIA) " +
                    "    OR  T4.COD_CATEGORIA IS NOT NULL) " +
                    " ORDER BY T1.COD_PESQUISA_PIV, T4.COD_CATEGORIA, T2.COD_PERGUNTA_PIV ";

                pCOD_CATEGORIA_PDV = new SQLiteParameter("@COD_CATEGORIA", this.Pdv.COD_CATEGORIA);
                pCOD_PDV = new SQLiteParameter("@COD_PDV", this.Pdv.COD_PDV);
                m_CODPESQUISA = 0;

                // Busca todas as pesquisas
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_CATEGORIA_PDV, pCOD_PDV))
                {
                    while (sqlReader.Read())
                    {
                        if (m_CODPESQUISA != sqlReader.GetInt32(0))
                        {
                            m_CODPESQUISA = sqlReader.GetInt32(0);

                            pesquisaPIV = new CSPesquisaMarketing();

                            // Preenche a instancia da classe de pesquisaPIV
                            pesquisaPIV.COD_PESQUISA_PIV = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                            pesquisaPIV.DSC_PESQUISA_PIV = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1);
                            pesquisaPIV.DATINI_PESQUISA_PIV = sqlReader.GetValue(2) == System.DBNull.Value ? new DateTime(1900, 1, 1) : sqlReader.GetDateTime(2);
                            pesquisaPIV.DATFIM_PESQUISA_PIV = sqlReader.GetValue(3) == System.DBNull.Value ? new DateTime(1900, 1, 1) : sqlReader.GetDateTime(3);
                            pesquisaPIV.COD_CATEGORIA = sqlReader.GetValue(4) == System.DBNull.Value ? -1 : sqlReader.GetInt32(4);

                            // Adiciona a pesquisa na colecao
                            base.InnerList.Add(pesquisaPIV);
                        }

                        CSPesquisasMarketing.CSPesquisaMarketing.CSPerguntas.CSPergunta pergunta = new CSPesquisasMarketing.CSPesquisaMarketing.CSPerguntas.CSPergunta();

                        // Preenche a instancia da classe de pergunta PIV
                        pergunta.COD_PESQUISA_PIV = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        pergunta.COD_PERGUNTA_PIV = sqlReader.GetValue(5) == System.DBNull.Value ? -1 : sqlReader.GetInt32(5);
                        pergunta.DSC_PERGUNTA_PIV = sqlReader.GetValue(6) == System.DBNull.Value ? "" : sqlReader.GetString(6);
                        pergunta.TIP_RESP_PERGUNTA_PIV = sqlReader.GetValue(7) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(7));
                        pergunta.VAL_FAIXAINI_PIV = sqlReader.GetValue(8) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(8));
                        pergunta.VAL_FAIXAFIM_PIV = sqlReader.GetValue(9) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(9));

                        pergunta.OPCAO.COD_PESQUISA_PIV = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        pergunta.OPCAO.COD_PERGUNTA_PIV = sqlReader.GetValue(5) == System.DBNull.Value ? -1 : sqlReader.GetInt32(5);
                        pergunta.OPCAO.DSC_OPCAORESPOSTA1 = sqlReader.GetValue(10) == System.DBNull.Value ? "" : sqlReader.GetString(10);
                        pergunta.OPCAO.DSC_OPCAORESPOSTA2 = sqlReader.GetValue(11) == System.DBNull.Value ? "" : sqlReader.GetString(11);
                        pergunta.OPCAO.DSC_OPCAORESPOSTA3 = sqlReader.GetValue(12) == System.DBNull.Value ? "" : sqlReader.GetString(12);
                        pergunta.OPCAO.DSC_OPCAORESPOSTA4 = sqlReader.GetValue(13) == System.DBNull.Value ? "" : sqlReader.GetString(13);
                        pergunta.OPCAO.DSC_OPCAORESPOSTA5 = sqlReader.GetValue(14) == System.DBNull.Value ? "" : sqlReader.GetString(14);

                        // Adiciona as perguntas na colecao
                        pesquisaPIV.PERGUNTAS.Add(pergunta);
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos dados para pesquisa", ex);
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
        /// Summary description for CSPesquisaMarketing.
        /// </summary>
        /// 
        public class CSPesquisaMarketing
        {
            #region [ Variaveis ]

            private CSPesquisasMarketing.CSPesquisaMarketing.CSPerguntas m_perguntas;

            private int m_COD_PESQUISA_PIV;
            private string m_DSC_PESQUISA_PIV;
            private DateTime m_DATINI_PESQUISA_PIV;
            private DateTime m_DATFIM_PESQUISA_PIV;
            private int m_COD_CATEGORIA;

            #endregion

            #region [ Propriedades ]

            public int COD_PESQUISA_PIV
            {
                get
                {
                    return m_COD_PESQUISA_PIV;
                }
                set
                {
                    m_COD_PESQUISA_PIV = value;
                }
            }

            public string DSC_PESQUISA_PIV
            {
                get
                {
                    return m_DSC_PESQUISA_PIV;
                }
                set
                {
                    m_DSC_PESQUISA_PIV = value;
                }
            }

            public DateTime DATINI_PESQUISA_PIV
            {
                get
                {
                    return m_DATINI_PESQUISA_PIV;
                }
                set
                {
                    m_DATINI_PESQUISA_PIV = value;
                }
            }

            public DateTime DATFIM_PESQUISA_PIV
            {
                get
                {
                    return m_DATFIM_PESQUISA_PIV;
                }
                set
                {
                    m_DATFIM_PESQUISA_PIV = value;
                }
            }

            public int COD_CATEGORIA
            {
                get
                {
                    return m_COD_CATEGORIA;
                }
                set
                {
                    m_COD_CATEGORIA = value;
                }
            }

            public CSPesquisaMarketing.CSPerguntas PERGUNTAS
            {
                get
                {
                    return m_perguntas;
                }
                set
                {
                    m_perguntas = value;
                }
            }

            // Teste de gravacao.
            public bool PESQUISA_RESPONDIDA
            {
                get
                {
                    foreach (CSPesquisasMarketing.CSPesquisaMarketing.CSPerguntas.CSPergunta pergunta in PERGUNTAS.Items)
                    {
                        // teste se foi realizado salvamento de uma pesquisa.
                        if (pergunta.RESPOSTA == null)
                            return false;
                    }
                    return true;
                }
            }

            #endregion

            #region [ Metodos ]

            public CSPesquisaMarketing()
            {
                m_perguntas = new CSPesquisasMarketing.CSPesquisaMarketing.CSPerguntas();
            }

            public void Flush()
            {
                foreach (CSPesquisasMarketing.CSPesquisaMarketing.CSPerguntas.CSPergunta pergunta in PERGUNTAS.Items)
                    pergunta.RESPOSTA.Flush();
            }

            #endregion

            #region [ SubClasses ]

            public class CSPerguntas : CollectionBase, IDisposable
            {
                #region [ Variaveis ]

                private CSPerguntas.CSPergunta current;

                #endregion

                #region [ Propriedades ]

                /// <summary>
                /// Retorna coleção dos preços produtos
                /// </summary>
                public CSPerguntas Items
                {
                    get
                    {
                        return this;
                    }
                }

                public CSPerguntas.CSPergunta this[int Index]
                {
                    get
                    {
                        return (CSPerguntas.CSPergunta)this.InnerList[Index];
                    }
                }

                /// <summary>
                /// 
                /// </summary>
                public CSPerguntas.CSPergunta Current
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

                #region [ Metodos ]

                public int Add(CSPerguntas.CSPergunta pergunta)
                {
                    return this.InnerList.Add(pergunta);
                }

                public void Dispose()
                {
                    this.InnerList.Clear();
                    this.InnerList.TrimToSize();
                }

                public CSPerguntas()
                {
                }

                #endregion

                #region [ SubClasses ]

                public class CSPergunta
                {
                    #region [ Variaveis ]

                    private CSPesquisasMarketing.CSPesquisaMarketing.CSPerguntas.CSPergunta.CSOpcao m_Opcao = new CSPesquisasMarketing.CSPesquisaMarketing.CSPerguntas.CSPergunta.CSOpcao();
                    private CSPesquisasMarketing.CSPesquisaMarketing.CSPerguntas.CSPergunta.CSResposta m_Resposta;

                    private int m_COD_PESQUISA_PIV;
                    private int m_COD_PERGUNTA_PIV;
                    private string m_DSC_PERGUNTA_PIV;
                    private decimal m_TIP_RESP_PERGUNTA_PIV;
                    private decimal m_VAL_FAIXAINI_PIV;
                    private decimal m_VAL_FAIXAFIM_PIV;

                    #endregion

                    #region [ Propriedades ]

                    public int COD_PESQUISA_PIV
                    {
                        get
                        {
                            return m_COD_PESQUISA_PIV;
                        }
                        set
                        {
                            m_COD_PESQUISA_PIV = value;
                        }
                    }

                    public int COD_PERGUNTA_PIV
                    {
                        get
                        {
                            return m_COD_PERGUNTA_PIV;
                        }
                        set
                        {
                            m_COD_PERGUNTA_PIV = value;
                        }
                    }

                    public string DSC_PERGUNTA_PIV
                    {
                        get
                        {
                            return m_DSC_PERGUNTA_PIV;
                        }
                        set
                        {
                            m_DSC_PERGUNTA_PIV = value;
                        }
                    }

                    public decimal TIP_RESP_PERGUNTA_PIV
                    {
                        get
                        {
                            return m_TIP_RESP_PERGUNTA_PIV;
                        }
                        set
                        {
                            m_TIP_RESP_PERGUNTA_PIV = value;
                        }
                    }

                    public decimal VAL_FAIXAINI_PIV
                    {
                        get
                        {
                            return m_VAL_FAIXAINI_PIV;
                        }
                        set
                        {
                            m_VAL_FAIXAINI_PIV = value;
                        }
                    }

                    public decimal VAL_FAIXAFIM_PIV
                    {
                        get
                        {
                            return m_VAL_FAIXAFIM_PIV;
                        }
                        set
                        {
                            m_VAL_FAIXAFIM_PIV = value;
                        }
                    }

                    public CSPesquisasMarketing.CSPesquisaMarketing.CSPerguntas.CSPergunta.CSOpcao OPCAO
                    {
                        get
                        {
                            return m_Opcao;
                        }
                        set
                        {
                            m_Opcao = value;
                        }
                    }

                    public CSPesquisasMarketing.CSPesquisaMarketing.CSPerguntas.CSPergunta.CSResposta RESPOSTA
                    {
                        get
                        {
                            return m_Resposta;
                        }
                        set
                        {
                            m_Resposta = value;
                        }
                    }

                    #endregion

                    #region [ Metodos ]

                    public CSPergunta()
                    {
                    }

                    public bool Validar(decimal valorDigitado)
                    {
                        return (valorDigitado >= this.m_VAL_FAIXAINI_PIV && valorDigitado <= this.m_VAL_FAIXAFIM_PIV);
                    }

                    #endregion

                    #region [ SubClasses ]

                    public class CSTipoResposta
                    {
                        public const int SIM_NAO = 1;
                        public const int VALOR = 2;
                        public const int NUMERICA = 3;
                        public const int ESCOLHA_UNICA = 4;
                    }

                    public class CSOpcao
                    {
                        #region [ Variaveis ]

                        private int m_COD_PESQUISA_PIV;
                        private int m_COD_PERGUNTA_PIV;
                        private string m_DSC_OPCAORESPOSTA1;
                        private string m_DSC_OPCAORESPOSTA2;
                        private string m_DSC_OPCAORESPOSTA3;
                        private string m_DSC_OPCAORESPOSTA4;
                        private string m_DSC_OPCAORESPOSTA5;

                        #endregion

                        #region [ Propriedades ]

                        public int COD_PESQUISA_PIV
                        {
                            get
                            {
                                return m_COD_PESQUISA_PIV;
                            }
                            set
                            {
                                m_COD_PESQUISA_PIV = value;
                            }
                        }

                        public int COD_PERGUNTA_PIV
                        {
                            get
                            {
                                return m_COD_PERGUNTA_PIV;
                            }
                            set
                            {
                                m_COD_PERGUNTA_PIV = value;
                            }
                        }

                        public string DSC_OPCAORESPOSTA1
                        {
                            get
                            {
                                return m_DSC_OPCAORESPOSTA1;
                            }
                            set
                            {
                                m_DSC_OPCAORESPOSTA1 = value;
                            }
                        }

                        public string DSC_OPCAORESPOSTA2
                        {
                            get
                            {
                                return m_DSC_OPCAORESPOSTA2;
                            }
                            set
                            {
                                m_DSC_OPCAORESPOSTA2 = value;
                            }
                        }

                        public string DSC_OPCAORESPOSTA3
                        {
                            get
                            {
                                return m_DSC_OPCAORESPOSTA3;
                            }
                            set
                            {
                                m_DSC_OPCAORESPOSTA3 = value;
                            }
                        }

                        public string DSC_OPCAORESPOSTA4
                        {
                            get
                            {
                                return m_DSC_OPCAORESPOSTA4;
                            }
                            set
                            {
                                m_DSC_OPCAORESPOSTA4 = value;
                            }
                        }

                        public string DSC_OPCAORESPOSTA5
                        {
                            get
                            {
                                return m_DSC_OPCAORESPOSTA5;
                            }
                            set
                            {
                                m_DSC_OPCAORESPOSTA5 = value;
                            }
                        }

                        #endregion

                        #region [ Metodos ]

                        public CSOpcao()
                        {
                        }

                        #endregion
                    }

                    public class CSResposta
                    {
                        #region [ Variaveis ]

                        private int m_COD_PESQUISA_PIV;
                        private int m_COD_PERGUNTA_PIV;
                        private int m_COD_PDV;
                        private int m_COD_EMPREGADO;
                        private decimal m_VAL_RESPOSTA_PESQUISA_PIV;
                        private DateTime m_DAT_COLETA;

                        #endregion

                        #region [ Propriedades ]

                        public int COD_PESQUISA_PIV
                        {
                            get
                            {
                                return m_COD_PESQUISA_PIV;
                            }
                            set
                            {
                                m_COD_PESQUISA_PIV = value;
                            }
                        }

                        public int COD_PERGUNTA_PIV
                        {
                            get
                            {
                                return m_COD_PERGUNTA_PIV;
                            }
                            set
                            {
                                m_COD_PERGUNTA_PIV = value;
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

                        public decimal VAL_RESPOSTA_PESQUISA_PIV
                        {
                            get
                            {
                                return m_VAL_RESPOSTA_PESQUISA_PIV;
                            }
                            set
                            {
                                m_VAL_RESPOSTA_PESQUISA_PIV = value;
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

                        #endregion

                        #region [ Metodos ]

                        public CSResposta(int COD_PESQUISA_PIV, int COD_PERGUNTA_PIV, int COD_PDV, int COD_EMPREGADO)
                        {
                            this.m_COD_PESQUISA_PIV = COD_PESQUISA_PIV;
                            this.m_COD_PERGUNTA_PIV = COD_PERGUNTA_PIV;
                            this.m_COD_PDV = COD_PDV;
                            this.m_COD_EMPREGADO = COD_EMPREGADO;
                            this.m_VAL_RESPOSTA_PESQUISA_PIV = 0;
                        }

                        public void Flush()
                        {
                            int result;

                            // Criar os parametros 
                            SQLiteParameter pCOD_PESQUISA_PIV = null;
                            SQLiteParameter pCOD_PERGUNTA_PIV = null;
                            SQLiteParameter pCOD_PDV = null;
                            SQLiteParameter pCOD_EMPREGADO = null;
                            SQLiteParameter pVAL_RESPOSTA = null;
                            SQLiteParameter pVAL_DATA_COLETA = null;

                            try
                            {
                                // Query de Insercao
                                string sqlInsertResposta =
                                    "INSERT INTO RESPOSTA_PESQUISA " +
                                    "  (COD_PESQUISA_PIV, COD_PERGUNTA_PIV, COD_PDV, COD_EMPREGADO, VAL_RESPOSTA, DAT_COLETA) " +
                                    "  VALUES(?,?,?,?,?,?) ";

                                // Criar os parametros 
                                pCOD_PESQUISA_PIV = new SQLiteParameter("@COD_PESQUISA_PIV", this.COD_PESQUISA_PIV);
                                pCOD_PERGUNTA_PIV = new SQLiteParameter("@COD_PERGUNTA_PIV", this.COD_PERGUNTA_PIV);
                                pCOD_PDV = new SQLiteParameter("@COD_PDV", this.COD_PDV);
                                pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", this.COD_EMPREGADO);

                                pVAL_RESPOSTA = new SQLiteParameter("@VAL_RESPOSTA", DbType.Decimal);
                                pVAL_RESPOSTA.Value = this.VAL_RESPOSTA_PESQUISA_PIV;

                                pVAL_DATA_COLETA = new SQLiteParameter("@VAL_DATA_COLETA", DateTime.Now);

                                // Executa a query salvando os dados
                                result = CSDataAccess.Instance.ExecuteNonQuery(sqlInsertResposta, pCOD_PESQUISA_PIV, pCOD_PERGUNTA_PIV, pCOD_PDV, pCOD_EMPREGADO, pVAL_RESPOSTA, pVAL_DATA_COLETA);
                                if (result == 0)
                                    throw new Exception("Resposta não pode ser inserida.");

                            }
                            catch (Exception ex)
                            {
                                CSGlobal.ShowMessage(ex.ToString());
                                throw new Exception("Erro no flush da resposta.", ex);
                            }
                        }

                        #endregion
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