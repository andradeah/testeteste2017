using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Collections;

using Mono.Data.Sqlite;
using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
using SQLiteCommand = Mono.Data.Sqlite.SqliteCommand;
using SQLiteDataAdapter = Mono.Data.Sqlite.SqliteDataAdapter;
using SQLiteException = Mono.Data.Sqlite.SqliteException;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteTransaction = Mono.Data.Sqlite.SqliteTransaction;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;


namespace AvanteSales.SystemFramework.CSPDV
{
    public class CSHistoricoIndenizacoesPDV : CollectionBase
    {
        #region [ Variaveis ]

        private CSHistoricoIndenizacoesPDV.CSHistoricoIndenizacaoPDV m_Current;

        #endregion

        #region[Propriedades]
        /// <summary>
        /// Retorna coleção de HISTORICO de pedidos do PDV
        /// </summary>
        public CSHistoricoIndenizacoesPDV Items
        {
            get
            {
                return this;
            }
        }

        public CSHistoricoIndenizacoesPDV.CSHistoricoIndenizacaoPDV this[int Index]
        {
            get
            {
                return (CSHistoricoIndenizacoesPDV.CSHistoricoIndenizacaoPDV)this.InnerList[Index];
            }
        }
        public CSHistoricoIndenizacoesPDV.CSHistoricoIndenizacaoPDV Current
        {
            get
            {
                return m_Current;
            }
            set
            {
                m_Current = value;

                if (m_Current != null && m_Current.ITEMS_INDENIZACAO == null)
                    m_Current.ITEMS_INDENIZACAO = new CSItemsHistoricoIndenizacao(m_Current.COD_INDENIZACAO);
            }
        }


        #endregion

        public CSHistoricoIndenizacoesPDV(int COD_PDV)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT   COD_INDENIZACAO_ELETRONICA, ");
            query.AppendLine("                                    COD_PDV, ");
            query.AppendLine("                                    COD_GRUPO_COMERCIALIZACAO,      ");
            query.AppendLine("                                    COD_EMPREGADO,                  ");
            query.AppendLine("                                    NUM_SERIE_DOCUMENTO_DEVOLVIDO,  ");
            query.AppendLine("                                    DAT_CADASTRO_INDENIZACAO,       ");
            query.AppendLine("                                    DAT_DOCUMENTO_DEVOLVIDO,        ");
            query.AppendLine("                                    DSC_RESPONSAVEL_INDENIZACAO,    ");
            query.AppendLine("                                    COD_STATUS_INDENIZACAO,         ");
            query.AppendLine("                                    DSC_STATUS_INDENIZACAO,         ");
            query.AppendLine("                                    COD_CONDICAO_PAGAMENTO,         ");
            query.AppendLine("                                    COD_USUARIO,                    ");
            query.AppendLine("                                    DAT_ENTREGA_INDENIZACAO,        ");
            query.AppendLine("                                    DAT_ENVIO_SAP,                  ");
            query.AppendLine("                                    NUM_DOCUMENTO_DEVOLVIDO,        ");

            if (CSEmpresa.ColunaExiste("HISTORICO_INDENIZACAO_ELETRONICA", "NUM_DOCUMENTO_PAGAMENTO"))
                query.AppendLine(" NUM_DOCUMENTO_PAGAMENTO, ");
            else
                query.AppendLine(" 0 AS DOCPAG, ");

            if (CSEmpresa.ColunaExiste("HISTORICO_INDENIZACAO_ELETRONICA", "NUM_SERIE_DOCUMENTO_PAGAMENTO"))
                query.AppendLine(" NUM_SERIE_DOCUMENTO_PAGAMENTO,  ");
            else
                query.AppendLine(" ' ' AS SERIEPAG, ");

            if (CSEmpresa.ColunaExiste("HISTORICO_INDENIZACAO_ELETRONICA", "DAT_DOCUMENTO_PAGAMENTO"))
                query.AppendLine(" DAT_DOCUMENTO_PAGAMENTO         ");
            else
                query.AppendLine(" NULL AS DATPAG ");

            query.AppendLine("                            FROM HISTORICO_INDENIZACAO_ELETRONICA   ");
            query.AppendFormat("                          WHERE COD_PDV = {0}", COD_PDV);

            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(query.ToString()))
            {
                while (sqlReader.Read())
                {
                    CSHistoricoIndenizacaoPDV historicoIndenizacao = new CSHistoricoIndenizacaoPDV();

                    historicoIndenizacao.COD_INDENIZACAO = sqlReader.GetValue(0) == System.DBNull.Value ? 0 : sqlReader.GetInt32(0);
                    historicoIndenizacao.COD_PDV = sqlReader.GetValue(1) == System.DBNull.Value ? 0 : sqlReader.GetInt32(1);
                    historicoIndenizacao.COD_GRUPO_COMERCIALIZACAO = sqlReader.GetValue(2) == System.DBNull.Value ? 0 : sqlReader.GetInt32(2);
                    historicoIndenizacao.COD_EMPREGADO = sqlReader.GetValue(3) == System.DBNull.Value ? 0 : sqlReader.GetInt32(3);
                    historicoIndenizacao.NUM_SERIE = sqlReader.GetValue(4) == System.DBNull.Value ? string.Empty : sqlReader.GetString(4);

                    if (sqlReader.GetValue(5) != System.DBNull.Value)
                        historicoIndenizacao.DAT_INDENIZACAO = sqlReader.GetDateTime(5);

                    if (sqlReader.GetValue(6) != System.DBNull.Value)
                        historicoIndenizacao.DAT_INDENIZACAO_DEVOLUCAO = sqlReader.GetDateTime(6);

                    historicoIndenizacao.RESPONSAVEL = sqlReader.GetValue(7) == System.DBNull.Value ? string.Empty : sqlReader.GetString(7);
                    historicoIndenizacao.COD_STATUS = sqlReader.GetValue(8) == System.DBNull.Value ? 0 : sqlReader.GetInt32(8);
                    historicoIndenizacao.DSC_STATUS = sqlReader.GetValue(9) == System.DBNull.Value ? string.Empty : sqlReader.GetString(9);
                    historicoIndenizacao.COD_CONDICAO_PAGAMENTO = sqlReader.GetValue(10) == System.DBNull.Value ? 0 : sqlReader.GetInt32(10);
                    historicoIndenizacao.COD_USUARIO = sqlReader.GetValue(11) == DBNull.Value ? string.Empty : sqlReader.GetString(11);

                    if (sqlReader.GetValue(12) != System.DBNull.Value)
                        historicoIndenizacao.DATA_INDENIZACAO_ENTREGA = sqlReader.GetDateTime(12);

                    if (sqlReader.GetValue(13) != System.DBNull.Value)
                        historicoIndenizacao.DATA_ENVIO_SAP = sqlReader.GetDateTime(13);

                    historicoIndenizacao.NUM_DOCUMENTO_DEVOLVIDO = sqlReader.GetValue(14) == System.DBNull.Value ? 0 : sqlReader.GetInt32(14);
                    historicoIndenizacao.NUM_DOCUMENTO_PAGAMENTO = sqlReader.GetValue(15) == System.DBNull.Value ? 0 : sqlReader.GetInt32(15);
                    historicoIndenizacao.NUM_SERIE_DOCUMENTO_PAGAMENTO = sqlReader.GetValue(16) == System.DBNull.Value ? string.Empty : sqlReader.GetString(16);

                    if (sqlReader.GetValue(17) != System.DBNull.Value)
                        historicoIndenizacao.DAT_DOCUMENTO_PAGAMENTO = sqlReader.GetDateTime(17);

                    if (historicoIndenizacao.COD_INDENIZACAO != 0)
                        historicoIndenizacao.ITEMS_INDENIZACAO = new CSItemsHistoricoIndenizacao(historicoIndenizacao.COD_INDENIZACAO);

                    base.InnerList.Add(historicoIndenizacao);
                }

                sqlReader.Close();
            }
        }

        public class CSHistoricoIndenizacaoPDV
        {
            #region[ Variáveis ]

            private int m_COD_INDENIZACAO;
            private int m_COD_PDV;
            private int m_COD_GRUPO_COMERCIALIZACAO;
            private int m_COD_EMPREGADO;
            private string m_NUM_SERIE;
            private int m_NUM_DOCUMENTO_DEVOLVIDO;
            private DateTime m_DAT_INDENIZACAO;
            private DateTime? m_DAT_INDENIZACAO_DEVOLUCAO;
            private decimal m_VLR_TOTAL_INDENIZACAO;
            private string m_RESPONSAVEL;
            private int m_COD_STATUS;
            private string m_DSC_STATUS;
            private int m_COD_CONDICAO_PAGAMENTO;
            private string m_COD_USUARIO;
            private DateTime? m_DATA_INDENIZACAO_ENTREGA;
            private DateTime? m_DATA_ENVIO_SAP;
            private CSItemsHistoricoIndenizacao m_ITEMS_INDENIZACAO = null;
            private int m_NUM_DOCUMENTO_PAGAMENTO;
            private string m_NUM_SERIE_DOCUMENTO_PAGAMENTO;
            private DateTime? m_DAT_DOCUMENTO_PAGAMENTO;

            #endregion

            public int COD_INDENIZACAO
            {
                get
                {
                    return m_COD_INDENIZACAO;
                }
                set
                {
                    m_COD_INDENIZACAO = value;
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

            public string NUM_SERIE
            {
                get
                {
                    return m_NUM_SERIE;
                }
                set
                {
                    m_NUM_SERIE = value;
                }
            }

            public int NUM_DOCUMENTO_DEVOLVIDO
            {
                get
                {
                    return m_NUM_DOCUMENTO_DEVOLVIDO;
                }
                set
                {
                    m_NUM_DOCUMENTO_DEVOLVIDO = value;
                }
            }

            public DateTime DAT_INDENIZACAO
            {
                get
                {
                    return m_DAT_INDENIZACAO;
                }
                set
                {
                    m_DAT_INDENIZACAO = value;
                }
            }

            public DateTime? DAT_INDENIZACAO_DEVOLUCAO
            {
                get
                {
                    return m_DAT_INDENIZACAO_DEVOLUCAO;
                }
                set
                {
                    m_DAT_INDENIZACAO_DEVOLUCAO = value;
                }
            }

            public decimal VLR_TOTAL_INDENIZACAO
            {
                get
                {
                    return m_VLR_TOTAL_INDENIZACAO;
                }
                set
                {
                    m_VLR_TOTAL_INDENIZACAO = value;
                }
            }

            public string RESPONSAVEL
            {
                get
                {
                    return m_RESPONSAVEL;
                }
                set
                {
                    m_RESPONSAVEL = value;
                }
            }

            public int COD_STATUS
            {
                get
                {
                    return m_COD_STATUS;
                }
                set
                {
                    m_COD_STATUS = value;
                }
            }

            public string DSC_STATUS
            {
                get
                {
                    return m_DSC_STATUS;
                }
                set
                {
                    m_DSC_STATUS = value;
                }
            }

            public int COD_CONDICAO_PAGAMENTO
            {
                get
                {
                    return m_COD_CONDICAO_PAGAMENTO;
                }
                set
                {
                    m_COD_CONDICAO_PAGAMENTO = value;
                }
            }

            public string COD_USUARIO
            {
                get
                {
                    return m_COD_USUARIO;
                }
                set
                {
                    m_COD_USUARIO = value;
                }
            }

            public DateTime? DATA_INDENIZACAO_ENTREGA
            {
                get
                {
                    return m_DATA_INDENIZACAO_ENTREGA;
                }
                set
                {
                    m_DATA_INDENIZACAO_ENTREGA = value;
                }
            }

            public DateTime? DATA_ENVIO_SAP
            {
                get
                {
                    return m_DATA_ENVIO_SAP;
                }
                set
                {
                    m_DATA_ENVIO_SAP = value;
                }
            }

            public CSItemsHistoricoIndenizacao ITEMS_INDENIZACAO
            {
                get
                {
                    if (m_ITEMS_INDENIZACAO == null)
                        m_ITEMS_INDENIZACAO = new CSItemsHistoricoIndenizacao(this.COD_INDENIZACAO);

                    return m_ITEMS_INDENIZACAO;
                }
                set
                {
                    m_ITEMS_INDENIZACAO = value;
                }
            }

            public int NUM_DOCUMENTO_PAGAMENTO
            {
                get
                {
                    return m_NUM_DOCUMENTO_PAGAMENTO;
                }
                set
                {
                    m_NUM_DOCUMENTO_PAGAMENTO = value;
                }
            }

            public string NUM_SERIE_DOCUMENTO_PAGAMENTO
            {
                get
                {
                    return m_NUM_SERIE_DOCUMENTO_PAGAMENTO;
                }
                set
                {
                    m_NUM_SERIE_DOCUMENTO_PAGAMENTO = value;
                }
            }

            public DateTime? DAT_DOCUMENTO_PAGAMENTO
            {
                get
                {
                    return m_DAT_DOCUMENTO_PAGAMENTO;
                }
                set
                {
                    m_DAT_DOCUMENTO_PAGAMENTO = value;
                }
            }
        }
    }
}