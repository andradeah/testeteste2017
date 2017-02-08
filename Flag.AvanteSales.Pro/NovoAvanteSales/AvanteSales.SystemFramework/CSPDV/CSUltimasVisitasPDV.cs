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
    public class CSUltimasVisitasPDV : CollectionBase
    {

        #region [ Variaveis ]

        private CSUltimasVisitasPDV.CSUltimaVisitaPDV m_Current;

        #endregion

        #region[Propriedades]
        /// <summary>
        /// Retorna coleção de HISTORICO de pedidos do PDV
        /// </summary>
        public CSUltimasVisitasPDV Items
        {
            get
            {
                return this;
            }
        }

        public CSUltimasVisitasPDV.CSUltimaVisitaPDV this[int Index]
        {
            get
            {
                return (CSUltimasVisitasPDV.CSUltimaVisitaPDV)this.InnerList[Index];
            }
        }
        public CSUltimasVisitasPDV.CSUltimaVisitaPDV Current
        {
            get
            {
                return m_Current;
            }
            set
            {
                m_Current = value;

                if (m_Current != null && m_Current.ITEMS_PEDIDOS == null)
                    m_Current.ITEMS_PEDIDOS = new CSItemsPedido(m_Current.COD_PEDIDO, true);
            }
        }


        #endregion

        #region [ Funções ]
        public CSUltimasVisitasPDV(int COD_PDV)
        {
            // Instancia do PDV Atual
            CSPDVs.CSPDV pdvAtual = CSPDVs.Current;

            string sqlQuery =
                "SELECT COD_PEDIDO, COD_OPERACAO, COD_EMPREGADO, COD_PDV " +
                "      ,DAT_PEDIDO, VLR_TOTAL_PEDIDO, COD_CONDICAO_PAGAMENTO " +
                "      ,IND_HISTORICO, STA_PEDIDO_FLEXX " +
                "  FROM PEDIDO " +
                " WHERE IND_HISTORICO = 1 " +
                "   AND COD_PDV = ? " +
                "   AND COD_EMPREGADO = ? ";

            SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", COD_PDV);
            SQLiteParameter pCOD_VENDEDOR = new SQLiteParameter("@COD_EMPREGADO", CSConfiguracao.GetConfig("vendedor" + CSGlobal.COD_REVENDA));

            // Busca todos os Pedidos do PDV com IND_HISTORICO = 0
            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_PDV, pCOD_VENDEDOR))
            {
                while (sqlReader.Read())
                {
                    // Variavel do tipo CSUltimaVisita
                    CSUltimaVisitaPDV ultimaVisita = new CSUltimaVisitaPDV();

                    ultimaVisita.COD_PEDIDO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                    ultimaVisita.COD_OPERACAO = sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1);
                    ultimaVisita.COD_EMPREGADO = sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2);
                    ultimaVisita.COD_PDV = sqlReader.GetValue(3) == System.DBNull.Value ? -1 : sqlReader.GetInt32(3);
                    ultimaVisita.DAT_PEDIDO = sqlReader.GetValue(4) == System.DBNull.Value ? new DateTime(1900, 1, 1) : sqlReader.GetDateTime(4);
                    ultimaVisita.VLR_TOTAL_PEDIDO = sqlReader.GetValue(5) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(5));
                    ultimaVisita.COD_CONDICAO_PAGAMENTO = sqlReader.GetValue(6) == System.DBNull.Value ? -1 : sqlReader.GetInt32(6);
                    ultimaVisita.IND_HISTORICO = sqlReader.GetValue(7) == System.DBNull.Value ? false : sqlReader.GetBoolean(7);
                    ultimaVisita.STA_PEDIDO_FLEXX = sqlReader.GetValue(8) == System.DBNull.Value ? -1 : sqlReader.GetInt32(8);

                    // Adciona as ultimas visitas
                    base.InnerList.Add(ultimaVisita);
                }
                // Fecha o reader
                sqlReader.Close();
            }
        }
        #endregion

        #region [ SubClasses ]

        public class CSUltimaVisitaPDV
        {
            #region[ Variáveis ]

            private int m_COD_PEDIDO;
            private int m_COD_OPERACAO;
            private int m_COD_PDV;
            private int m_COD_EMPREGADO;
            private DateTime m_DAT_PEDIDO;
            private decimal m_VLR_TOTAL_PEDIDO;
            private int m_COD_CONDICAO_PAGAMENTO;
            private bool m_IND_HISTORICO;
            private int m_STA_PEDIDO_FLEXX;
            private CSItemsPedido m_ITEMS_PEDIDOS = null;

            #endregion

            #region [ Propriedades ]

            /// <summary>
            /// Guarda o Codigo do Pedido do PDV
            /// </summary>
            public int COD_PEDIDO
            {
                get
                {
                    return m_COD_PEDIDO;
                }
                set
                {
                    m_COD_PEDIDO = value;
                }
            }
            /// <summary>
            /// Guarda o Codigo da Operacao do Pedido do PDV
            /// </summary>
            public int COD_OPERACAO
            {
                get
                {
                    return m_COD_OPERACAO;
                }
                set
                {
                    m_COD_OPERACAO = value;
                }
            }
            /// <summary>
            /// Guarda o Codigo do empregado que realizou o Pedido do PDV
            /// </summary>
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
            /// <summary>
            /// Guarda o Codigo do PDV que realizou o pedido
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
            /// Guarda o a data do Pedido do PDV
            /// </summary>
            public DateTime DAT_PEDIDO
            {
                get
                {
                    return m_DAT_PEDIDO;
                }
                set
                {
                    m_DAT_PEDIDO = value;
                }
            }
            /// <summary>
            /// Guarda o valor total do Pedido do PDV
            /// </summary>
            public decimal VLR_TOTAL_PEDIDO
            {
                get
                {
                    return m_VLR_TOTAL_PEDIDO;
                }
                set
                {
                    m_VLR_TOTAL_PEDIDO = value;
                }
            }
            /// <summary>
            /// Guarda o codigo de condição de pagamento do pedido do PDV
            /// </summary>
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
            /// <summary>
            /// Guarda o Status do historico de pedidos do PDV
            /// </summary>
            public bool IND_HISTORICO
            {
                get
                {
                    return m_IND_HISTORICO;
                }
                set
                {
                    m_IND_HISTORICO = value;
                }
            }

            public CSItemsPedido ITEMS_PEDIDOS
            {
                get
                {
                    return m_ITEMS_PEDIDOS;
                }
                set
                {
                    m_ITEMS_PEDIDOS = value;
                }
            }

            public int STA_PEDIDO_FLEXX
            {
                get
                {
                    return m_STA_PEDIDO_FLEXX;
                }
                set
                {
                    m_STA_PEDIDO_FLEXX = value;
                }
            }

            #endregion

        }

        #endregion
    }
}
