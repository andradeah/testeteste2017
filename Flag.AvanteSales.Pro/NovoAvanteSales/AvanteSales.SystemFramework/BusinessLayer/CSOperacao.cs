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
    public class CSOperacoes : CollectionBase
    {
        #region [ Variáveis ]

        private static CSOperacoes m_Items;

        #endregion

        #region [ Propriedades ]

        /// <summary>
        /// Retorna coleção das operações
        /// </summary>
        public static CSOperacoes Items
        {
            get
            {
                if (m_Items == null)
                    m_Items = new CSOperacoes();

                return m_Items;
            }
        }

        public CSOperacoes.CSOperacao this[int Index]
        {
            get
            {
                return (CSOperacoes.CSOperacao)this.InnerList[Index];
            }
        }

        /// <summary>
        /// Busca a operacao pelo codigo
        /// </summary>
        /// <param name="COD_OPERACAO">Codigo da operacao a ser procurada</param>
        /// <returns>Retorna o objeto da operação</returns>
        public static CSOperacao GetOperacao(int COD_OPERACAO)
        {
            // Procura pela operação
            foreach (CSOperacao operacao in Items.InnerList)
            {
                if (operacao.COD_OPERACAO == COD_OPERACAO)
                {
                    return operacao;
                }
            }

            return null;
        }

        #endregion

        #region [ Métodos ]

        /// <summary>
        /// Contrutor da classe. Busca as operções
        /// </summary>
        public CSOperacoes()
        {
            try
            {
                string sqlQuery =
                    "SELECT COD_OPERACAO, DSC_OPERACAO " +
                    "      ,COD_OPERACAO_CFO, COD_CFO, SEQ_CFO " +
                    "      ,IND_MOVIMENTA_VERBA, IND_VRPEDIDO_ABATER_VERBA, IND_VALIDA_PRECO_MINMAX, IND_PRONTA_ENTREGA " +
                    "  FROM OPERACAO " +
                    " WHERE IND_ATIVO = 1 ";

                // Busca todas as operações
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery))
                {
                    while (sqlReader.Read())
                    {
                        CSOperacao operacao = new CSOperacao();

                        // Preenche a instancia da classe de telefone do pdv
                        operacao.COD_OPERACAO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        operacao.DSC_OPERACAO = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1);
                        operacao.COD_OPERACAO_CFO = sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2);
                        operacao.COD_CFO = sqlReader.GetValue(3) == System.DBNull.Value ? -1 : sqlReader.GetInt32(3);
                        operacao.SEQ_CFO = sqlReader.GetValue(4) == System.DBNull.Value ? "" : sqlReader.GetString(4);
                        operacao.IND_MOVIMENTA_VERBA = sqlReader.GetBoolean(5); ;
                        operacao.IND_VRPEDIDO_ABATER_VERBA = sqlReader.GetBoolean(6);
                        operacao.IND_VALIDA_PRECO_MINMAX = sqlReader.GetBoolean(7);
                        operacao.IND_PRONTA_ENTREGA = sqlReader.GetBoolean(8);

                        // Adciona a operação na coleção de operações
                        base.InnerList.Add(operacao);
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());

                throw new Exception("Erro na busca das operações", ex);
            }
        }

        #endregion

        #region [ SubClasses ]

        /// <summary>
        /// Guarda a informaçõa sobre telefone do PDB
        /// </summary>
        public class CSOperacao
#if ANDROID
 : Java.Lang.Object
#endif
        {
            #region [ Variáveis ]

            private int m_COD_OPERACAO;
            private string m_DSC_OPERACAO;
            private int m_COD_OPERACAO_CFO;
            private int m_COD_CFO;
            private string m_SEQ_CFO;
            private bool m_IND_MOVIMENTA_VERBA;
            private bool m_IND_VRPEDIDO_ABATER_VERBA;
            private bool m_IND_VALIDA_PRECO_MINMAX;
            private bool m_IND_PRONTA_ENTREGA;

            private static CSOperacoes.CSOperacao m_Current;

            #endregion

            #region .:: Propriedades ::.

            /// <summary>
            /// Guarda o codigo da operação
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
            /// Guarda a descrição da operação
            /// </summary>
            public string DSC_OPERACAO
            {
                get
                {
                    return m_DSC_OPERACAO;
                }
                set
                {
                    m_DSC_OPERACAO = value.Trim();
                }
            }

            public int COD_OPERACAO_CFO
            {
                get
                {
                    return m_COD_OPERACAO_CFO;
                }
                set
                {
                    m_COD_OPERACAO_CFO = value;
                }
            }

            public int COD_CFO
            {
                get
                {
                    return m_COD_CFO;
                }
                set
                {
                    m_COD_CFO = value;
                }
            }

            public string SEQ_CFO
            {
                get
                {
                    return m_SEQ_CFO;
                }
                set
                {
                    m_SEQ_CFO = value;
                }
            }
            public bool IND_MOVIMENTA_VERBA
            {
                get
                {
                    return m_IND_MOVIMENTA_VERBA;
                }
                set
                {
                    m_IND_MOVIMENTA_VERBA = value;
                }
            }
            public bool IND_VRPEDIDO_ABATER_VERBA
            {
                get
                {
                    return m_IND_VRPEDIDO_ABATER_VERBA;
                }
                set
                {
                    m_IND_VRPEDIDO_ABATER_VERBA = value;
                }
            }
            public bool IND_VALIDA_PRECO_MINMAX
            {
                get
                {
                    return m_IND_VALIDA_PRECO_MINMAX;
                }
                set
                {
                    m_IND_VALIDA_PRECO_MINMAX = value;
                }
            }
            public bool IND_PRONTA_ENTREGA
            {
                get
                {
                    return m_IND_PRONTA_ENTREGA;
                }
                set
                {
                    m_IND_PRONTA_ENTREGA = value;
                }
            }

            /// <summary>
            /// Guarda a operação atual. A operação escolhida para aquela coleta de pedido
            /// </summary>
            public static CSOperacoes.CSOperacao Current
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

            public CSOperacao()
            {

            }

            #endregion
        }

        #endregion
    }
}