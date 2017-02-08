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
using System.Collections.Generic;
using System.Text;
#endif
namespace AvanteSales.BusinessRules
{
    public class CSPesquisasMercado : CollectionBase, IDisposable
    {
        #region [ Variáveis ]

        private CSPesquisaMercado current = null;
        private static ArrayList pesquisasCache = CarregaPesquisas();

        #endregion

        #region [ Propriedades ]

        public CSPesquisaMercado Current
        {
            get
            {
                return this.current;
            }
            set
            {
                this.current = value;
            }
        }

        public CSPesquisasMercado.CSPesquisaMercado this[int Index]
        {
            get
            {
                return (CSPesquisasMercado.CSPesquisaMercado)this.InnerList[Index];
            }
        }

        #endregion

        #region [ Métodos ]

        /// <summary>
        /// Contrutor da classe. 
        /// </summary>
        public CSPesquisasMercado(CSPDVs.CSPDV pdv)
        {
            // [ copia pesquisa do cache ]
            //this.InnerList.AddRange((ICollection)pesquisasCache.Clone());
            this.InnerList.AddRange(CarregaPesquisas());
        }

        public static ArrayList CarregaPesquisas()
        {
            ArrayList pesquisas = null;
            CSPesquisaMercado pesquisa = null;
            SQLiteDataReader reader = null;
            StringBuilder sqlQuery = null;

            try
            {
                pesquisas = new ArrayList();

                // busca dados necessarios
                sqlQuery = new StringBuilder();
                sqlQuery.AppendLine("SELECT T1.COD_PESQUISA_MERC, T1.DATINI_PESQUISA_MERC, T1.DATFIM_PESQUISA_MERC ");

                if (CSEmpresa.ColunaExiste("PESQUISA_MERCADO", "DSC_PESQUISA"))
                    sqlQuery.AppendLine(" ,T1.DSC_PESQUISA ");
                else
                    sqlQuery.AppendLine(" ,CAST (T1.COD_PESQUISA_MERC AS VARCHAR) AS DSCPESQ ");

                if (CSEmpresa.ColunaExiste("PESQUISA_MERCADO", "IND_OBR_PESQUISA"))
                    sqlQuery.AppendLine(" ,T1.IND_OBR_PESQUISA ");
                else
                    sqlQuery.AppendLine(" ,0 AS INDOBR ");

                sqlQuery.AppendLine("  FROM PESQUISA_MERCADO T1 ");
                sqlQuery.AppendLine(" WHERE DATE('NOW') BETWEEN DATE(T1.DATINI_PESQUISA_MERC) AND DATE(T1.DATFIM_PESQUISA_MERC) ");

                // Verifica se existe pesquisa vigente a ser realizada para o PDV
                using (reader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString()))
                {
                    while (reader.Read())
                    {
                        pesquisa = new CSPesquisaMercado();

                        pesquisa.COD_PESQUISA_MERC = (reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0));
                        pesquisa.DATINI_PESQUISA_MERC = (reader.GetValue(1) == DBNull.Value ? new DateTime(1900, 1, 1) : reader.GetDateTime(1));
                        pesquisa.DATFIM_PESQUISA_MERC = (reader.GetValue(2) == DBNull.Value ? new DateTime(1900, 1, 1) : reader.GetDateTime(2));
                        pesquisa.DSC_PESQUISA = reader.GetValue(3) == DBNull.Value ? string.Empty : reader.GetString(3);
                        pesquisa.IND_OBR_PESQUISA = reader.GetValue(4) == DBNull.Value ? false : reader.GetBoolean(4);
                        pesquisa.PERGUNTAS = new CSPesquisaMercado.CSPerguntas(pesquisa);
                        pesquisa.MARCAS = new CSPesquisasMercado.CSPesquisaMercado.CSMarcas(pesquisa, CSPDVs.Current);

                        pesquisas.Add(pesquisa);
                    }

                    reader.Close();
                    reader.Dispose();
                }

                return pesquisas;

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca das pesquisas de mercado!", ex);
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
        /// Summary description for CSPesquisaMercado.
        /// </summary>
        /// 
        public class CSPesquisaMercado
        {
            #region [ Variaveis ]

            private int m_COD_PESQUISA_MERC;
            private DateTime m_DATINI_PESQUISA_MERC;
            private DateTime m_DATFIM_PESQUISA_MERC;
            private string m_DSC_PESQUISA;
            private bool m_IND_OBR_PESQUISA;
            private CSPesquisasMercado.CSPesquisaMercado.CSPerguntas m_Perguntas = null;
            private CSPesquisasMercado.CSPesquisaMercado.CSMarcas m_Marcas = null;

            #endregion

            #region [ Propriedades ]

            public int COD_PESQUISA_MERC
            {
                get
                {
                    return m_COD_PESQUISA_MERC;
                }
                set
                {
                    m_COD_PESQUISA_MERC = value;
                }
            }

            public DateTime DATINI_PESQUISA_MERC
            {
                get
                {
                    return m_DATINI_PESQUISA_MERC;
                }
                set
                {
                    m_DATINI_PESQUISA_MERC = value;
                }
            }

            public DateTime DATFIM_PESQUISA_MERC
            {
                get
                {
                    return m_DATFIM_PESQUISA_MERC;
                }
                set
                {
                    m_DATFIM_PESQUISA_MERC = value;
                }
            }

            public CSPesquisaMercado.CSPerguntas PERGUNTAS
            {
                get
                {
                    return m_Perguntas;
                }
                set
                {
                    m_Perguntas = value;
                }
            }

            public CSPesquisaMercado.CSMarcas MARCAS
            {
                get
                {
                    return m_Marcas;
                }
                set
                {
                    m_Marcas = value;
                }
            }

            public string DSC_PESQUISA
            {
                get
                {
                    return m_DSC_PESQUISA;
                }
                set
                {
                    m_DSC_PESQUISA = value;
                }
            }

            public bool IND_OBR_PESQUISA
            {
                get
                {
                    return m_IND_OBR_PESQUISA;
                }
                set
                {
                    m_IND_OBR_PESQUISA = value;
                }
            }

            public bool PESQUISA_RESPONDIDA
            {
                get
                {
                    SQLiteParameter pCOD_PESQUISA_MERC = null;
                    SQLiteParameter pCOD_PDV = null;
                    string sqlQuery = null;

                    try
                    {
                        // busca resposta
                        sqlQuery =
                            "SELECT T1.COD_PESQUISA_MERC " +
                            "  FROM RESPOSTA_PESQUISA_MERCADO T1 " +
                            " WHERE T1.COD_PESQUISA_MERC = ? " +
                            "   AND T1.COD_PDV = ? ";

                        pCOD_PESQUISA_MERC = new SQLiteParameter("@COD_PESQUISA_MERC", this.COD_PESQUISA_MERC);
                        pCOD_PDV = new SQLiteParameter("@COD_PDV", this.MARCAS[0].Pdv.COD_PDV);

                        object result = CSDataAccess.Instance.ExecuteScalar(sqlQuery, pCOD_PESQUISA_MERC, pCOD_PDV);

                        return result != null && result != DBNull.Value;

                    }
                    catch (Exception ex)
                    {
                        CSGlobal.ShowMessage(ex.ToString());
                        throw new Exception("Erro na busca de pesquisa de mercado respondida!", ex);
                    }
                }
            }

            public CSMotivosNaoPesquisa.CSMotivoNaoPesquisa MOTIVO_NAO_PESQUISA_MERCADO
            {
                get
                {
                    foreach (CSMotivosNaoPesquisa.CSMotivoNaoPesquisa motivo in CSPDVs.Current.MOTIVOS_NAO_PESQUISA_MERCADO)
                    {
                        if (motivo.COD_TIPO_MOTIVO == CSMotivos.CSTipoMotivo.NAO_PESQUISA_MERCADO &&
                            motivo.COD_PESQUISA == this.COD_PESQUISA_MERC)
                        {
                            return motivo;
                            //break;
                        }
                    }

                    return null;
                }
            }

            public CSMotivosNaoPesquisa.CSMotivoNaoPesquisa MOTIVO_NAO_PESQUISA
            {
                get
                {
                    foreach (CSMotivosNaoPesquisa.CSMotivoNaoPesquisa motivo in CSPDVs.Current.MOTIVOS_NAO_PESQUISA)
                    {
                        if (motivo.COD_TIPO_MOTIVO == CSMotivos.CSTipoMotivo.NAO_PESQUISA_MERCADO &&
                            motivo.COD_PESQUISA == this.COD_PESQUISA_MERC)
                        {
                            return motivo;
                            //break;
                        }
                    }

                    return null;
                }
            }

            #endregion

            #region [ Metodos ]

            public CSPesquisaMercado()
            {
            }

            public void Flush()
            {
                foreach (CSPesquisasMercado.CSPesquisaMercado.CSMarcas.CSMarca marca in this.MARCAS)
                    marca.RESPOSTAS.Flush();
            }

            public void DiscardChanges()
            {
                foreach (CSPesquisasMercado.CSPesquisaMercado.CSMarcas.CSMarca marca in this.MARCAS)
                    marca.RESPOSTAS.DiscardChanges();
            }

            #endregion

            #region [ SubClasses ]

            public class CSItemPergunta
            {
                private CSPerguntas.CSPergunta pergunta;
                private ArrayList respostas;

                public int COD_PERGUNTA
                {
                    get
                    {
                        return this.pergunta.COD_PERGUNTA_MERC;
                    }
                }

                public string RESPOSTA_MARCA_1
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[0];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[0];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (value == "S")
                        //        resposta.VAL_RESPOSTA = 1;
                        //    else if (value == "N")
                        //        resposta.VAL_RESPOSTA = 2;
                        //    else
                        //        resposta.VAL_RESPOSTA = 0;
                        //}
                        //else
                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public string RESPOSTA_MARCA_2
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[1];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[1];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (value == "S")
                        //        resposta.VAL_RESPOSTA = 1;
                        //    else if (value == "N")
                        //        resposta.VAL_RESPOSTA = 2;
                        //    else
                        //        resposta.VAL_RESPOSTA = 0;
                        //}
                        //else
                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public string RESPOSTA_MARCA_3
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[2];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[2];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (value == "S")
                        //        resposta.VAL_RESPOSTA = 1;
                        //    else if (value == "N")
                        //        resposta.VAL_RESPOSTA = 2;
                        //    else
                        //        resposta.VAL_RESPOSTA = 0;
                        //}
                        //else
                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public string RESPOSTA_MARCA_4
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[3];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[3];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (value == "S")
                        //        resposta.VAL_RESPOSTA = 1;
                        //    else if (value == "N")
                        //        resposta.VAL_RESPOSTA = 2;
                        //    else
                        //        resposta.VAL_RESPOSTA = 0;
                        //}
                        //else
                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public string RESPOSTA_MARCA_5
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[4];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[4];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (value == "S")
                        //        resposta.VAL_RESPOSTA = 1;
                        //    else if (value == "N")
                        //        resposta.VAL_RESPOSTA = 2;
                        //    else
                        //        resposta.VAL_RESPOSTA = 0;
                        //}
                        //else
                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public string RESPOSTA_MARCA_6
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[5];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[5];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (value == "S")
                        //        resposta.VAL_RESPOSTA = 1;
                        //    else if (value == "N")
                        //        resposta.VAL_RESPOSTA = 2;
                        //    else
                        //        resposta.VAL_RESPOSTA = 0;
                        //}
                        //else
                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public string RESPOSTA_MARCA_7
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[6];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[6];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (value == "S")
                        //        resposta.VAL_RESPOSTA = 1;
                        //    else if (value == "N")
                        //        resposta.VAL_RESPOSTA = 2;
                        //    else
                        //        resposta.VAL_RESPOSTA = 0;
                        //}
                        //else
                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public string RESPOSTA_MARCA_8
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[7];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[7];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (value == "S")
                        //        resposta.VAL_RESPOSTA = 1;
                        //    else if (value == "N")
                        //        resposta.VAL_RESPOSTA = 2;
                        //    else
                        //        resposta.VAL_RESPOSTA = 0;
                        //}
                        //else
                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public string RESPOSTA_MARCA_9
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[8];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[8];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (value == "S")
                        //        resposta.VAL_RESPOSTA = 1;
                        //    else if (value == "N")
                        //        resposta.VAL_RESPOSTA = 2;
                        //    else
                        //        resposta.VAL_RESPOSTA = 0;
                        //}
                        //else
                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public string RESPOSTA_MARCA_10
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[9];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[9];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (value == "S")
                        //        resposta.VAL_RESPOSTA = 1;
                        //    else if (value == "N")
                        //        resposta.VAL_RESPOSTA = 2;
                        //    else
                        //        resposta.VAL_RESPOSTA = 0;
                        //}
                        //else
                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public string RESPOSTA_MARCA_11
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[10];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[10];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (value == "S")
                        //        resposta.VAL_RESPOSTA = 1;
                        //    else if (value == "N")
                        //        resposta.VAL_RESPOSTA = 2;
                        //    else
                        //        resposta.VAL_RESPOSTA = 0;
                        //}
                        //else
                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public string RESPOSTA_MARCA_12
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[11];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[11];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (value == "S")
                        //        resposta.VAL_RESPOSTA = 1;
                        //    else if (value == "N")
                        //        resposta.VAL_RESPOSTA = 2;
                        //    else
                        //        resposta.VAL_RESPOSTA = 0;
                        //}
                        //else
                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public string RESPOSTA_MARCA_13
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[12];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[12];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (value == "S")
                        //        resposta.VAL_RESPOSTA = 1;
                        //    else if (value == "N")
                        //        resposta.VAL_RESPOSTA = 2;
                        //    else
                        //        resposta.VAL_RESPOSTA = 0;
                        //}
                        //else
                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public string RESPOSTA_MARCA_14
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[13];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[13];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (value == "S")
                        //        resposta.VAL_RESPOSTA = 1;
                        //    else if (value == "N")
                        //        resposta.VAL_RESPOSTA = 2;
                        //    else
                        //        resposta.VAL_RESPOSTA = 0;
                        //}
                        //else
                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public string RESPOSTA_MARCA_15
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[14];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[14];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (value == "S")
                        //        resposta.VAL_RESPOSTA = 1;
                        //    else if (value == "N")
                        //        resposta.VAL_RESPOSTA = 2;
                        //    else
                        //        resposta.VAL_RESPOSTA = 0;
                        //}
                        //else
                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public string RESPOSTA_MARCA_16
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[15];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[15];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (value == "S")
                        //        resposta.VAL_RESPOSTA = 1;
                        //    else if (value == "N")
                        //        resposta.VAL_RESPOSTA = 2;
                        //    else
                        //        resposta.VAL_RESPOSTA = 0;
                        //}
                        //else
                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public string RESPOSTA_MARCA_17
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[16];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[16];

                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public string RESPOSTA_MARCA_18
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[17];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[17];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (value == "S")
                        //        resposta.VAL_RESPOSTA = 1;
                        //    else if (value == "N")
                        //        resposta.VAL_RESPOSTA = 2;
                        //    else
                        //        resposta.VAL_RESPOSTA = 0;
                        //}
                        //else
                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public string RESPOSTA_MARCA_19
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[18];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[18];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (value == "S")
                        //        resposta.VAL_RESPOSTA = 1;
                        //    else if (value == "N")
                        //        resposta.VAL_RESPOSTA = 2;
                        //    else
                        //        resposta.VAL_RESPOSTA = 0;
                        //}
                        //else
                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public string RESPOSTA_MARCA_20
                {
                    get
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[19];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (resposta.VAL_RESPOSTA == 1)
                        //        return "S";
                        //    else if (resposta.VAL_RESPOSTA == 2)
                        //        return "N";
                        //    else
                        //        return " ";
                        //}
                        //else
                        return resposta.VAL_RESPOSTA;
                    }
                    set
                    {
                        CSMarcas.CSMarca.CSRespostas.CSResposta resposta =
                            (CSMarcas.CSMarca.CSRespostas.CSResposta)this.respostas[19];

                        //if (this.pergunta.TIP_RESP_PERGUNTA_MERC == CSPerguntas.CSPergunta.CSTipoResposta.SIM_NAO)
                        //{
                        //    if (value == "S")
                        //        resposta.VAL_RESPOSTA = 1;
                        //    else if (value == "N")
                        //        resposta.VAL_RESPOSTA = 2;
                        //    else
                        //        resposta.VAL_RESPOSTA = 0;
                        //}
                        //else
                        resposta.VAL_RESPOSTA = value;
                    }
                }

                public CSItemPergunta(CSPerguntas.CSPergunta pergunta)
                {
                    this.pergunta = pergunta;
                    this.respostas = new ArrayList();
                }

                public void AddResposta(CSMarcas.CSMarca.CSRespostas.CSResposta resposta)
                {
                    this.respostas.Add(resposta);
                }
            }

            public class CSPerguntas : CollectionBase, IDisposable
            {
                #region [ Variaveis ]

                private CSPerguntas.CSPergunta current;
                private CSPesquisaMercado m_Pesquisa = null;

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

                public CSPesquisaMercado PESQUISA
                {
                    get
                    {
                        return m_Pesquisa;
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

                public CSPerguntas(CSPesquisaMercado pesquisa)
                {
                    CSPergunta pergunta = null;
                    SQLiteParameter pCOD_PESQUISA_MERC = null;
                    SQLiteDataReader reader = null;
                    StringBuilder sqlQuery = null;

                    try
                    {
                        this.m_Pesquisa = pesquisa;

                        // busca dados necessarios
                        sqlQuery = new StringBuilder();
                        sqlQuery.AppendLine("SELECT T1.COD_PERGUNTA_MERC, T1.DSC_PERGUNTA_MERC ");
                        sqlQuery.AppendLine("      ,T1.TIP_RESP_PERGUNTA_MERC, T1.VAL_FAIXAINI_MERC, T1.VAL_FAIXAFIM_MERC ");
                        sqlQuery.AppendLine("  FROM PERGUNTA_PESQUISA_MERCADO T1 ");
                        sqlQuery.AppendLine(" WHERE T1.COD_PESQUISA_MERC = ? ");
                        sqlQuery.AppendLine(" ORDER BY ");

                        if (CSEmpresa.ColunaExiste("PERGUNTA_PESQUISA_MERCADO", "ORD_PESQUISA_MERC"))
                            sqlQuery.AppendLine("T1.ORD_PESQUISA_MERC, ");

                        sqlQuery.AppendLine(" T1.COD_PERGUNTA_MERC ");

                        pCOD_PESQUISA_MERC = new SQLiteParameter("@COD_PESQUISA_MERC", this.PESQUISA.COD_PESQUISA_MERC);

                        using (reader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString(), pCOD_PESQUISA_MERC))
                        {
                            while (reader.Read())
                            {
                                pergunta = new CSPergunta(this.PESQUISA);

                                pergunta.COD_PERGUNTA_MERC = (reader.GetValue(0) == DBNull.Value ? -1 : reader.GetInt32(0));
                                pergunta.DSC_PERGUNTA_MERC = (reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1));
                                pergunta.TIP_RESP_PERGUNTA_MERC = (reader.GetValue(2) == DBNull.Value ? 0 : reader.GetInt32(2));
                                pergunta.VAL_FAIXAINI_MERC = (reader.GetValue(3) == DBNull.Value ? -1 : reader.GetDecimal(3));
                                pergunta.VAL_FAIXAFIM_MERC = (reader.GetValue(4) == DBNull.Value ? -1 : reader.GetDecimal(4));

                                if (pergunta.TIP_RESP_PERGUNTA_MERC == 6)
                                {
                                    pergunta.OPCOES_RESPOSTA = new CSOpcaoResposta().RetornarOpcoes(pergunta.COD_PERGUNTA_MERC, this.PESQUISA.COD_PESQUISA_MERC);
                                }

                                this.InnerList.Add(pergunta);
                            }

                            reader.Close();
                            reader.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        CSGlobal.ShowMessage(ex.ToString());
                        throw new Exception("Erro na busca das perguntas da pesquisa!", ex);
                    }
                }

                #endregion

                #region [ SubClasses ]

                public class CSOpcaoResposta
                {
                    #region [ Variaveis ]

                    private int m_COD_OPCAO_LISTA;
                    private int m_COD_LISTA;
                    private int m_COD_PESQUISA_MERC;
                    private int m_COD_PERGUNTA_MERC;
                    private string m_DSC_OPCAO_LISTA;

                    public CSOpcaoResposta()
                    {

                    }

                    public List<CSOpcaoResposta> RetornarOpcoes(int codPergunta, int codPesquisa)
                    {
                        List<CSOpcaoResposta> opcoes = new List<CSOpcaoResposta>();

                        StringBuilder sql = new StringBuilder();
                        sql.AppendLine("SELECT COD_OPCAO_LISTA,COD_PESQUISA_MERC,COD_PERGUNTA_MERC,DSC_OPCAO_LISTA,COD_LISTA ");
                        sql.AppendLine("    FROM LISTA_OPCAO_PERGUNTA_PESQUISA_MERCADO ");
                        sql.AppendFormat(" WHERE COD_PERGUNTA_MERC = {0} AND COD_PESQUISA_MERC = {1}", codPergunta, codPesquisa);

                        using (SqliteDataReader reader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
                        {
                            while (reader.Read())
                            {
                                CSOpcaoResposta opcao = new CSOpcaoResposta();
                                opcao.COD_OPCAO_LISTA = reader.GetInt32(0);
                                opcao.COD_PESQUISA_MERC = reader.GetInt32(1);
                                opcao.COD_PERGUNTA_MERC = reader.GetInt32(2);
                                opcao.DSC_OPCAO_LISTA = reader.GetString(3);
                                opcao.COD_LISTA = reader.GetInt32(4);

                                opcoes.Add(opcao);
                            }
                        }

                        return opcoes;
                    }

                    #endregion

                    #region [ PROPRIEDADES ]

                    public int COD_OPCAO_LISTA
                    {
                        get
                        {
                            return m_COD_OPCAO_LISTA;
                        }
                        set
                        {
                            m_COD_OPCAO_LISTA = value;
                        }
                    }

                    public int COD_LISTA
                    {
                        get
                        {
                            return m_COD_LISTA;
                        }
                        set
                        {
                            m_COD_LISTA = value;
                        }
                    }

                    public int COD_PESQUISA_MERC
                    {
                        get
                        {
                            return m_COD_PESQUISA_MERC;
                        }
                        set
                        {
                            m_COD_PESQUISA_MERC = value;
                        }
                    }

                    public int COD_PERGUNTA_MERC
                    {
                        get
                        {
                            return m_COD_PERGUNTA_MERC;
                        }
                        set
                        {
                            m_COD_PERGUNTA_MERC = value;
                        }
                    }

                    public string DSC_OPCAO_LISTA
                    {
                        get
                        {
                            return m_DSC_OPCAO_LISTA;
                        }
                        set
                        {
                            m_DSC_OPCAO_LISTA = value;
                        }
                    }

                    #endregion
                }

                public class CSPergunta
                {
                    #region [ Variaveis ]

                    private CSPesquisasMercado.CSPesquisaMercado m_Pesquisa;

                    private int m_COD_PERGUNTA_MERC;
                    private string m_DSC_PERGUNTA_MERC;
                    private int m_TIP_RESP_PERGUNTA_MERC;
                    private decimal m_VAL_FAIXAINI_MERC;
                    private decimal m_VAL_FAIXAFIM_MERC;
                    private List<CSOpcaoResposta> m_OPCOES_RESPOSTA;

                    #endregion

                    #region [ Propriedades ]

                    public List<CSOpcaoResposta> OPCOES_RESPOSTA
                    {
                        get
                        {
                            return m_OPCOES_RESPOSTA;
                        }
                        set
                        {
                            m_OPCOES_RESPOSTA = value;
                        }
                    }

                    public CSPesquisasMercado.CSPesquisaMercado PESQUISA
                    {
                        get
                        {
                            return m_Pesquisa;
                        }
                    }

                    public int COD_PERGUNTA_MERC
                    {
                        get
                        {
                            return m_COD_PERGUNTA_MERC;
                        }
                        set
                        {
                            m_COD_PERGUNTA_MERC = value;
                        }
                    }

                    public string DSC_PERGUNTA_MERC
                    {
                        get
                        {
                            return m_DSC_PERGUNTA_MERC;
                        }
                        set
                        {
                            m_DSC_PERGUNTA_MERC = value;
                        }
                    }

                    public int TIP_RESP_PERGUNTA_MERC
                    {
                        get
                        {
                            return m_TIP_RESP_PERGUNTA_MERC;
                        }
                        set
                        {
                            m_TIP_RESP_PERGUNTA_MERC = value;
                        }
                    }

                    public decimal VAL_FAIXAINI_MERC
                    {
                        get
                        {
                            return m_VAL_FAIXAINI_MERC;
                        }
                        set
                        {
                            m_VAL_FAIXAINI_MERC = value;
                        }
                    }

                    public decimal VAL_FAIXAFIM_MERC
                    {
                        get
                        {
                            return m_VAL_FAIXAFIM_MERC;
                        }
                        set
                        {
                            m_VAL_FAIXAFIM_MERC = value;
                        }
                    }

                    #endregion

                    #region [ Metodos ]

                    public CSPergunta(CSPesquisasMercado.CSPesquisaMercado pesquisa)
                    {
                        this.m_Pesquisa = pesquisa;
                    }

                    public bool ValidaResposta(CSMarcas.CSMarca.CSRespostas.CSResposta resposta)
                    {
                        //if (this.TIP_RESP_PERGUNTA_MERC == CSTipoResposta.SIM_NAO)
                        //    return (resposta.VAL_RESPOSTA == 1 || resposta.VAL_RESPOSTA == 2);
                        if (this.TIP_RESP_PERGUNTA_MERC == CSTipoResposta.TEXTO)
                            return ((string.IsNullOrEmpty(resposta.VAL_RESPOSTA)) || (resposta.VAL_RESPOSTA.Length <= this.VAL_FAIXAFIM_MERC));
                        else if (this.TIP_RESP_PERGUNTA_MERC == CSTipoResposta.VALOR ||
                                this.TIP_RESP_PERGUNTA_MERC == CSTipoResposta.NUMERICA)
                            return (Convert.ToDecimal(resposta.VAL_RESPOSTA) >= this.VAL_FAIXAINI_MERC &&
                                    Convert.ToDecimal(resposta.VAL_RESPOSTA) <= this.VAL_FAIXAFIM_MERC);
                        else if (this.TIP_RESP_PERGUNTA_MERC == CSTipoResposta.TELEFONE)
                            return ((string.IsNullOrEmpty(resposta.VAL_RESPOSTA)) || (resposta.VAL_RESPOSTA.Length >= 13 && resposta.VAL_RESPOSTA.Length <= 14));
                        else
                            return true;
                    }

                    #endregion

                    #region [ SubClasses ]

                    public class CSTipoResposta
                    {
                        public const int SIM_NAO = 1;
                        public const int VALOR = 2;
                        public const int NUMERICA = 3;
                        public const int TELEFONE = 4;
                        public const int TEXTO = 5;
                    }

                    #endregion
                }

                #endregion
            }

            public class CSMarcas : CollectionBase, IDisposable
            {
                #region [ Variaveis ]

                private CSMarcas.CSMarca current = null;
                private CSPesquisaMercado m_PESQUISA = null;
                private ArrayList m_Array_Perguntas = null;

                #endregion

                #region [ Propriedades ]

                /// <summary>
                /// Retorna coleção de marcas
                /// </summary>
                public CSMarcas Items
                {
                    get
                    {
                        return this;
                    }
                }

                public CSMarcas.CSMarca this[int Index]
                {
                    get
                    {
                        return (CSMarcas.CSMarca)this.InnerList[Index];
                    }
                }

                public CSMarcas.CSMarca Current
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

                public CSPesquisaMercado PESQUISA
                {
                    get
                    {
                        return this.m_PESQUISA;
                    }
                    set
                    {
                        this.m_PESQUISA = value;
                    }
                }

                public ArrayList ARRAY_PERGUNTAS
                {
                    get
                    {
                        if (this.m_Array_Perguntas == null)
                        {
                            ArrayList array = null;
                            CSItemPergunta itemPergunta = null;
                            int i;

                            try
                            {
                                array = new ArrayList();
                                i = 0;
                                foreach (CSPerguntas.CSPergunta pergunta in this.PESQUISA.PERGUNTAS)
                                {
                                    itemPergunta = new CSItemPergunta(pergunta);

                                    foreach (CSMarcas.CSMarca marca in this.InnerList)
                                        itemPergunta.AddResposta(marca.RESPOSTAS[i]);

                                    array.Add(itemPergunta);
                                    i++;
                                }

                                this.m_Array_Perguntas = array;

                            }
                            catch (Exception ex)
                            {
                                CSGlobal.ShowMessage(ex.ToString());
                                throw ex;
                            }
                        }

                        return this.m_Array_Perguntas;
                    }
                    set
                    {
                        this.m_Array_Perguntas = value;
                    }
                }

                #endregion

                #region [ Metodos ]

                public int Add(CSMarcas.CSMarca marca)
                {
                    return this.InnerList.Add(marca);
                }

                public void Dispose()
                {
                    this.InnerList.Clear();
                    this.InnerList.TrimToSize();
                }

                public CSMarcas(CSPesquisasMercado.CSPesquisaMercado pesquisa, CSPDVs.CSPDV pdv)
                {
                    CSMarca marca = null;
                    SQLiteParameter pCOD_PESQUISA_MERC = null;
                    SQLiteDataReader reader = null;
                    string sqlQuery = null;

                    try
                    {
                        this.m_PESQUISA = pesquisa;

                        // busca dados necessarios
                        sqlQuery =
                            "SELECT T1.COD_MARCA_PESQUISA_MERC, T1.DSC_MARCA_PESQUISA_MERC " +
                            "      ,T1.ORD_MARCA_PESQUISA_MERC " +
                            "  FROM MARCA_PESQUISA_MERCADO T1 " +
                            " WHERE T1.COD_PESQUISA_MERC = ? " +
                            " ORDER BY T1.ORD_MARCA_PESQUISA_MERC ";

                        pCOD_PESQUISA_MERC = new SQLiteParameter("@COD_PESQUISA_MERC", pesquisa.COD_PESQUISA_MERC);

                        // [ Carrega marcas ]
                        using (reader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_PESQUISA_MERC))
                        {
                            while (reader.Read())
                            {
                                marca = new CSMarca(pesquisa, pdv);

                                marca.COD_MARCA_PESQUISA_MERC = (reader.GetValue(0) == DBNull.Value ? -1 : reader.GetInt32(0));
                                marca.DSC_MARCA_PESQUISA_MERC = (reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1));
                                marca.ORD_MARCA_PESQUISA_MERC = (reader.GetValue(2) == DBNull.Value ? 0 : reader.GetInt32(2));

                                this.InnerList.Add(marca);
                            }

                            reader.Close();
                            reader.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        CSGlobal.ShowMessage(ex.ToString());
                        throw new Exception("Erro na busca das marcas da pesquisa!", ex);
                    }
                }

                #endregion

                #region [ SubClasses ]

                public class CSMarca
                {
                    #region [ Variaveis ]

                    private CSPesquisasMercado.CSPesquisaMercado m_Pesquisa = null;
                    private CSRespostas m_Respostas = null;
                    private CSPDVs.CSPDV m_Pdv = null;
                    private int m_COD_MARCA_PESQUISA_MERC;
                    private string m_DSC_MARCA_PESQUISA_MERC;
                    private int m_ORD_MARCA_PESQUISA_MERC;

                    #endregion

                    #region [ Propriedades ]

                    public CSPesquisasMercado.CSPesquisaMercado PESQUISA
                    {
                        get
                        {
                            return m_Pesquisa;
                        }
                    }

                    public CSPDVs.CSPDV Pdv
                    {
                        get
                        {
                            return m_Pdv;
                        }
                    }

                    public CSRespostas RESPOSTAS
                    {
                        get
                        {
                            if (m_Respostas == null)
                                m_Respostas = new CSMarca.CSRespostas(this);

                            return m_Respostas;
                        }
                        set
                        {
                            m_Respostas = value;
                        }
                    }

                    public int COD_MARCA_PESQUISA_MERC
                    {
                        get
                        {
                            return m_COD_MARCA_PESQUISA_MERC;
                        }
                        set
                        {
                            m_COD_MARCA_PESQUISA_MERC = value;
                        }
                    }

                    public string DSC_MARCA_PESQUISA_MERC
                    {
                        get
                        {
                            return m_DSC_MARCA_PESQUISA_MERC;
                        }
                        set
                        {
                            m_DSC_MARCA_PESQUISA_MERC = value;
                        }
                    }

                    public int ORD_MARCA_PESQUISA_MERC
                    {
                        get
                        {
                            return m_ORD_MARCA_PESQUISA_MERC;
                        }
                        set
                        {
                            m_ORD_MARCA_PESQUISA_MERC = value;
                        }
                    }

                    #endregion

                    #region [ Metodos ]

                    public CSMarca(CSPesquisasMercado.CSPesquisaMercado pesquisa, CSPDVs.CSPDV pdv)
                    {
                        this.m_Pesquisa = pesquisa;
                        this.m_Pdv = pdv;
                    }

                    #endregion

                    #region [ SubClasses ]

                    public class CSRespostas : CollectionBase, IDisposable
                    {
                        #region [ Variaveis ]

                        private CSRespostas.CSResposta current = null;
                        private CSPesquisasMercado.CSPesquisaMercado.CSMarcas.CSMarca m_Marca = null;

                        #endregion

                        #region [ Propriedades ]

                        /// <summary>
                        /// Retorna coleção de marcas
                        /// </summary>
                        public CSRespostas Items
                        {
                            get
                            {
                                return this;
                            }
                        }

                        public CSRespostas.CSResposta this[int Index]
                        {
                            get
                            {
                                return (CSRespostas.CSResposta)this.InnerList[Index];
                            }
                        }

                        public CSRespostas.CSResposta Current
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

                        public CSPesquisasMercado.CSPesquisaMercado.CSMarcas.CSMarca MARCA
                        {
                            get
                            {
                                return m_Marca;
                            }
                        }

                        #endregion

                        #region [ Metodos ]

                        public int Add(CSRespostas.CSResposta resposta)
                        {
                            return this.InnerList.Add(resposta);
                        }

                        public void Dispose()
                        {
                            this.InnerList.Clear();
                            this.InnerList.TrimToSize();
                        }

                        public CSRespostas(CSMarca marca)
                        {
                            CSResposta resposta = null;
                            this.m_Marca = marca;

                            foreach (CSPerguntas.CSPergunta pergunta in marca.PESQUISA.PERGUNTAS)
                            {
                                resposta = new CSResposta(marca, pergunta);

                                this.InnerList.Add(resposta);
                            }

                            SQLiteParameter pCOD_PESQUISA_MERC = null;
                            SQLiteParameter pCOD_PDV = null;
                            SQLiteParameter pCOD_MARCA = null;

                            SQLiteDataReader reader = null;
                            string sqlQuery = null;

                            try
                            {
                                // busca resposta
                                sqlQuery =
                                    "SELECT T1.COD_PERGUNTA_MERC, T1.VAL_RESPOSTA " +
                                    "  FROM RESPOSTA_PESQUISA_MERCADO T1 " +
                                    " WHERE T1.COD_PESQUISA_MERC = ? " +
                                    "   AND T1.COD_PDV = ? " +
                                    "   AND T1.COD_MARCA = ? ";

                                pCOD_PESQUISA_MERC = new SQLiteParameter("@COD_PESQUISA_MERC", marca.PESQUISA.COD_PESQUISA_MERC);
                                pCOD_PDV = new SQLiteParameter("@COD_PDV", CSPDVs.Current.COD_PDV);
                                pCOD_MARCA = new SQLiteParameter("@COD_MARCA", marca.COD_MARCA_PESQUISA_MERC);

                                using (reader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_PESQUISA_MERC, pCOD_PDV, pCOD_MARCA))
                                {
                                    while (reader.Read())
                                    {
                                        int pergunta = reader.GetInt32(0);

                                        foreach (CSResposta r in this.InnerList)
                                        {
                                            if (r.PERGUNTA.COD_PERGUNTA_MERC == pergunta)
                                            {
                                                r.VAL_RESPOSTA = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
                                                r.STATE = CSResposta.ObjectState.SALVO;
                                                break;
                                            }
                                        }
                                    }

                                    reader.Close();
                                    reader.Dispose();
                                }
                            }
                            catch (Exception ex)
                            {
                                CSGlobal.ShowMessage(ex.ToString());
                                throw new Exception("Erro na busca de respostas para pesquisa de mercado!", ex);
                            }
                        }

                        public void Flush()
                        {
                            // Criar os parametros 
                            SQLiteParameter pCOD_PESQUISA_MERC = null;
                            SQLiteParameter pCOD_PERGUNTA_MERC = null;
                            SQLiteParameter pCOD_PDV = null;
                            SQLiteParameter pCOD_MARCA = null;
                            SQLiteParameter pVAL_RESPOSTA = null;
                            SQLiteParameter pDAT_COLETA = null;
                            SqliteParameter pCOD_EMPREGADO = null;

                            try
                            {
                                // Query de Insercao
                                string sqlQueryInsert =
                                    "INSERT INTO RESPOSTA_PESQUISA_MERCADO " +
                                    "  (COD_PESQUISA_MERC, COD_PERGUNTA_MERC, COD_PDV, COD_MARCA, VAL_RESPOSTA, DAT_COLETA, COD_EMPREGADO) " +
                                    "  VALUES(?, ?, ?, ?, ?, ?, ?) ";

                                string sqlQueryUpdate =
                                    "UPDATE RESPOSTA_PESQUISA_MERCADO " +
                                    "   SET VAL_RESPOSTA = ? " +
                                    "      ,DAT_COLETA = ? " +
                                    " WHERE COD_PESQUISA_MERC = ? " +
                                    "   AND COD_PERGUNTA_MERC = ? " +
                                    "   AND COD_PDV = ? " +
                                    "   AND COD_MARCA = ? ";

                                foreach (CSResposta resposta in this.InnerList)
                                {
                                    if (resposta.STATE == CSResposta.ObjectState.ALTERADO)
                                    {
                                        // Criar os parametros 
                                        pCOD_PESQUISA_MERC = new SQLiteParameter("@COD_PESQUISA_MERC", resposta.PERGUNTA.PESQUISA.COD_PESQUISA_MERC);
                                        pCOD_PERGUNTA_MERC = new SQLiteParameter("@COD_PERGUNTA_MERC", resposta.PERGUNTA.COD_PERGUNTA_MERC);
                                        pCOD_PDV = new SQLiteParameter("@COD_PDV", resposta.MARCA.Pdv.COD_PDV);
                                        pCOD_MARCA = new SQLiteParameter("@COD_MARCA", resposta.MARCA.COD_MARCA_PESQUISA_MERC);

                                        pVAL_RESPOSTA = new SQLiteParameter("@VAL_RESPOSTA", DbType.String);
                                        pVAL_RESPOSTA.Value = resposta.VAL_RESPOSTA;

                                        pDAT_COLETA = new SQLiteParameter("@DAT_COLETA", resposta.DAT_COLETA);
                                        pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                                        // Executa a query salvando os dados
                                        if (CSDataAccess.Instance.ExecuteNonQuery(sqlQueryUpdate, pVAL_RESPOSTA, pDAT_COLETA, pCOD_PESQUISA_MERC, pCOD_PERGUNTA_MERC, pCOD_PDV, pCOD_MARCA) == 0)
                                        {
                                            // Criar os parametros 
                                            pCOD_PESQUISA_MERC = new SQLiteParameter("@COD_PESQUISA_MERC", resposta.PERGUNTA.PESQUISA.COD_PESQUISA_MERC);
                                            pCOD_PERGUNTA_MERC = new SQLiteParameter("@COD_PERGUNTA_MERC", resposta.PERGUNTA.COD_PERGUNTA_MERC);
                                            pCOD_PDV = new SQLiteParameter("@COD_PDV", resposta.MARCA.Pdv.COD_PDV);
                                            pCOD_MARCA = new SQLiteParameter("@COD_MARCA", resposta.MARCA.COD_MARCA_PESQUISA_MERC);

                                            pVAL_RESPOSTA = new SQLiteParameter("@VAL_RESPOSTA", DbType.String);
                                            pVAL_RESPOSTA.Value = resposta.VAL_RESPOSTA;

                                            pDAT_COLETA = new SQLiteParameter("@DAT_COLETA", resposta.DAT_COLETA);

                                            CSDataAccess.Instance.ExecuteNonQuery(sqlQueryInsert, pCOD_PESQUISA_MERC, pCOD_PERGUNTA_MERC, pCOD_PDV, pCOD_MARCA, pVAL_RESPOSTA, pDAT_COLETA, pCOD_EMPREGADO);
                                        }

                                        resposta.STATE = CSResposta.ObjectState.SALVO;
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                CSGlobal.ShowMessage(ex.ToString());
                                throw new Exception("Erro no flush da resposta!", ex);
                            }
                        }

                        public void DiscardChanges()
                        {
                            foreach (CSResposta resposta in this.InnerList)
                            {
                                if (resposta.STATE != CSResposta.ObjectState.SALVO)
                                {
                                    resposta.VAL_RESPOSTA = string.Empty;
                                    resposta.STATE = CSResposta.ObjectState.NOVO;
                                }
                            }
                        }

                        #endregion

                        #region [ SubClasses ]

                        public class CSResposta
                        {
                            #region [ Variaveis ]

                            private CSPesquisasMercado.CSPesquisaMercado.CSMarcas.CSMarca m_Marca = null;
                            private CSPesquisasMercado.CSPesquisaMercado.CSPerguntas.CSPergunta m_Pergunta = null;
                            private string m_VAL_RESPOSTA;
                            private DateTime m_DAT_COLETA;
                            private ObjectState m_STATE;
                            private int m_COD_EMPREGADO;

                            public enum ObjectState
                            {
                                INALTERADO,
                                ALTERADO,
                                NOVO,
                                NOVO_ALTERADO,
                                DELETADO,
                                SALVO,
                            }

                            #endregion

                            #region [ Propriedades ]

                            public CSPesquisasMercado.CSPesquisaMercado.CSMarcas.CSMarca MARCA
                            {
                                get
                                {
                                    return m_Marca;
                                }
                            }

                            public CSPesquisasMercado.CSPesquisaMercado.CSPerguntas.CSPergunta PERGUNTA
                            {
                                get
                                {
                                    return m_Pergunta;
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

                            public string VAL_RESPOSTA
                            {
                                get
                                {
                                    return m_VAL_RESPOSTA;
                                }
                                set
                                {
                                    m_VAL_RESPOSTA = value;

                                    this.DAT_COLETA = DateTime.Now;
                                    this.STATE = ObjectState.ALTERADO;
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

                            #region [ Metodos ]

                            public CSResposta(CSMarca marca, CSPerguntas.CSPergunta pergunta)
                            {
                                this.m_Marca = marca;
                                this.m_Pergunta = pergunta;

                                this.VAL_RESPOSTA = null;
                                this.STATE = ObjectState.NOVO;
                            }

                            //public bool IsValid()
                            //{
                            //    return this.PERGUNTA.ValidaResposta(this);
                            //}

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

        #endregion
    }
}