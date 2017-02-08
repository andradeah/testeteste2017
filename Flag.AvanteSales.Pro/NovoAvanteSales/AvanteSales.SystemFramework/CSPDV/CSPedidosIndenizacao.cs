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
    /// Summary description for CSPedidosIndenizacao.
    /// </summary>
    public class CSPedidosIndenizacao : CollectionBase, IDisposable
    {
        #region [ Variáveis ]

        private CSPedidosIndenizacao.CSPedidoIndenizacao current;

        #endregion

        #region [ Propriedades ]

        public CSPedidosIndenizacao.CSPedidoIndenizacao Current
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

        public CSPedidosIndenizacao(int COD_EMPREGADO, int COD_PEDIDO)
        {
            string sqlQuery = null;
            SQLiteParameter pCOD_EMPREGADO = null;
            SQLiteParameter pCOD_PEDIDO = null;

            try
            {
                sqlQuery =
                    "SELECT COD_PEDIDO, COD_EMPREGADO, NUM_DOCDVL_IDZ, COD_SERDOCDVL_IDZ, VLR_DOCDVL_IDZ, DAT_EMSDLCDVL_IDZ, NUM_VOLDOCDVL_IDZ, COD_MTVDTN_IDZ, COD_TPOMTVDTN_IDZ, COD_MTVCNDRTR_IDZ, COD_TPOMTVCNDRTR_IDZ, COD_MTVRESPBM_IDZ, COD_TPOMTVRESPBM_IDZ, NUM_PEDCET_IDZ, NOM_CNT_IDZ " +
                    "  FROM PEDIDO_INDENIZACAO " +
                    " WHERE COD_EMPREGADO = ? " +
                    "   AND COD_PEDIDO = ?";

                pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", COD_EMPREGADO);
                pCOD_PEDIDO = new SQLiteParameter("@COD_PEDIDO", COD_PEDIDO);

                // Busca todos os Pedido de Indenização
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_EMPREGADO, pCOD_PEDIDO))
                {
                    while (sqlReader.Read())
                    {
                        CSPedidosIndenizacao.CSPedidoIndenizacao pedidoIndenizacao = new CSPedidosIndenizacao.CSPedidoIndenizacao();

                        pedidoIndenizacao.COD_PEDIDO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        pedidoIndenizacao.COD_EMPREGADO = sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1);
                        pedidoIndenizacao.NUM_DOCUMENTO_DEVOLUCAO = sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2);
                        pedidoIndenizacao.SERIE_DOCUMENTO_DEVOLUCAO = sqlReader.GetValue(3) == System.DBNull.Value ? "" : sqlReader.GetString(3);
                        pedidoIndenizacao.VALOR_DOCUMENTO_DEVOLUCAO = sqlReader.GetValue(4) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(4));
                        pedidoIndenizacao.DAT_EMISSAO_DOCUMENTO_DEVOLUCAO = sqlReader.GetValue(5) == System.DBNull.Value ? new DateTime(1900, 1, 1) : sqlReader.GetDateTime(5);
                        pedidoIndenizacao.NUM_VOLUME_DOCUMENTO_DEVOLUCAO = sqlReader.GetValue(6) == System.DBNull.Value ? -1 : sqlReader.GetInt32(6);
                        pedidoIndenizacao.COD_MOTIVO_DESTINO = sqlReader.GetValue(7) == System.DBNull.Value ? -1 : sqlReader.GetInt32(7);
                        pedidoIndenizacao.COD_TIPO_MOTIVO_DESTINO = sqlReader.GetValue(8) == System.DBNull.Value ? -1 : sqlReader.GetInt32(8);
                        pedidoIndenizacao.COD_MOTIVO_CONDICAO_RETIRADA = sqlReader.GetValue(9) == System.DBNull.Value ? -1 : sqlReader.GetInt32(9);
                        pedidoIndenizacao.COD_TIPO_MOTIVO_CONDICAO_RETIRADA = sqlReader.GetValue(10) == System.DBNull.Value ? -1 : sqlReader.GetInt32(10);
                        pedidoIndenizacao.COD_MOTIVO_RESUMO_PROBLEMA = sqlReader.GetValue(11) == System.DBNull.Value ? -1 : sqlReader.GetInt32(11);
                        pedidoIndenizacao.COD_TIPO_MOTIVO_RESUMO_PROBLEMA = sqlReader.GetValue(12) == System.DBNull.Value ? -1 : sqlReader.GetInt32(12);
                        pedidoIndenizacao.NUM_PEDIDO_CLIENTE = sqlReader.GetValue(13) == System.DBNull.Value ? "" : sqlReader.GetString(13);
                        pedidoIndenizacao.NOME_CONTATO = sqlReader.GetValue(14) == System.DBNull.Value ? "" : sqlReader.GetString(14);
                        pedidoIndenizacao.STATE = ObjectState.INALTERADO;

                        // Adiciona o pedido de indenização na coleção de PedidosIndenizacao
                        base.InnerList.Add(pedidoIndenizacao);
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos pedidos de indenização do PEDIDO", ex);
            }
        }

        /// <summary>
        /// Salva os dados na coleção no banco
        /// </summary>
        public bool Flush()
        {
            // Criar os parametros de salvamento
            SQLiteParameter pCOD_PEDIDO = null;
            SQLiteParameter pCOD_EMPREGADO = null;
            SQLiteParameter pNUM_DOCDVL_IDZ = null;
            SQLiteParameter pCOD_SERDOCDVL_IDZ = null;
            SQLiteParameter pVLR_DOCDVL_IDZ = null;
            SQLiteParameter pDAT_EMSDLCDVL_IDZ = null;
            SQLiteParameter pNUM_VOLDOCDVL_IDZ = null;
            SQLiteParameter pCOD_MTVDTN_IDZ = null;
            SQLiteParameter pCOD_TPOMTVDTN_IDZ = null;
            SQLiteParameter pCOD_MTVCNDRTR_IDZ = null;
            SQLiteParameter pCOD_TPOMTVCNDRTR_IDZ = null;
            SQLiteParameter pCOD_MTVRESPBM_IDZ = null;
            SQLiteParameter pCOD_TPOMTVRESPBM_IDZ = null;
            SQLiteParameter pNUM_PEDCET_IDZ = null;
            SQLiteParameter pNOM_CNT_IDZ = null;

            string sqlQueryInsert =
                "INSERT INTO PEDIDO_INDENIZACAO " +
                "  (COD_PEDIDO, COD_EMPREGADO, NUM_DOCDVL_IDZ, COD_SERDOCDVL_IDZ, VLR_DOCDVL_IDZ, DAT_EMSDLCDVL_IDZ, NUM_VOLDOCDVL_IDZ, COD_MTVDTN_IDZ, COD_TPOMTVDTN_IDZ, COD_MTVCNDRTR_IDZ, COD_TPOMTVCNDRTR_IDZ, COD_MTVRESPBM_IDZ, COD_TPOMTVRESPBM_IDZ, NUM_PEDCET_IDZ, NOM_CNT_IDZ) " +
                "  VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?) ";

            string sqlQueryUpdate =
                "UPDATE PEDIDO_INDENIZACAO " +
                "   SET NUM_DOCDVL_IDZ = ? " +
                "       ,COD_SERDOCDVL_IDZ = ?" +
                "       ,VLR_DOCDVL_IDZ = ?" +
                "       ,DAT_EMSDLCDVL_IDZ = ?" +
                "       ,NUM_VOLDOCDVL_IDZ = ?" +
                "       ,COD_MTVDTN_IDZ = ?" +
                "       ,COD_TPOMTVDTN_IDZ = ?" +
                "       ,COD_MTVCNDRTR_IDZ = ?" +
                "       ,COD_TPOMTVCNDRTR_IDZ = ?" +
                "       ,COD_MTVRESPBM_IDZ = ?" +
                "       ,COD_TPOMTVRESPBM_IDZ = ?" +
                "       ,NUM_PEDCET_IDZ = ?" +
                "       ,NOM_CNT_IDZ = ?" +
                " WHERE COD_PEDIDO = ? " +
                "   AND COD_EMPREGADO = ? ";

            string sqlQueryDelete =
                "DELETE FROM PEDIDO_INDENIZACAO " +
                " WHERE COD_PEDIDO = ? " +
                "   AND COD_EMPREGADO = ? ";

            // Varre a coleção procurando os objetos a serem persistidos
            foreach (CSPedidoIndenizacao pedido in ((System.Collections.ArrayList)(base.InnerList.Clone())))
            //foreach (CSPedidoIndenizacao pedido in base.InnerList)
            {
                switch (pedido.STATE)
                {
                    case ObjectState.NOVO:

                        pCOD_PEDIDO = new SQLiteParameter("@COD_PEDIDO", pedido.COD_PEDIDO);
                        pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", pedido.COD_EMPREGADO);
                        pNUM_DOCDVL_IDZ = new SQLiteParameter("@NUM_DOCDVL_IDZ", pedido.NUM_DOCUMENTO_DEVOLUCAO);
                        pCOD_SERDOCDVL_IDZ = new SQLiteParameter("@COD_SERDOCDVL_IDZ", pedido.SERIE_DOCUMENTO_DEVOLUCAO);

                        pVLR_DOCDVL_IDZ = new SQLiteParameter("@VLR_DOCDVL_IDZ", pedido.VALOR_DOCUMENTO_DEVOLUCAO);
                        pVLR_DOCDVL_IDZ.DbType = DbType.Decimal;
                        //TODO: Verificar precisione scale para bancos de dados.
                        //pVLR_DOCDVL_IDZ.Precision = 9;
                        //pVLR_DOCDVL_IDZ.Scale = 2;

                        pDAT_EMSDLCDVL_IDZ = new SQLiteParameter("@DAT_EMSDLCDVL_IDZ", pedido.DAT_EMISSAO_DOCUMENTO_DEVOLUCAO);
                        pNUM_VOLDOCDVL_IDZ = new SQLiteParameter("@NUM_VOLDOCDVL_IDZ", pedido.NUM_VOLUME_DOCUMENTO_DEVOLUCAO);
                        pCOD_MTVDTN_IDZ = new SQLiteParameter("@COD_MTVDTN_IDZ", pedido.COD_MOTIVO_DESTINO);
                        pCOD_TPOMTVDTN_IDZ = new SQLiteParameter("@COD_TPOMTVDTN_IDZ", pedido.COD_TIPO_MOTIVO_DESTINO);
                        pCOD_MTVCNDRTR_IDZ = new SQLiteParameter("@COD_MTVCNDRTR_IDZ", pedido.COD_MOTIVO_CONDICAO_RETIRADA);
                        pCOD_TPOMTVCNDRTR_IDZ = new SQLiteParameter("@COD_TPOMTVCNDRTR_IDZ", pedido.COD_TIPO_MOTIVO_CONDICAO_RETIRADA);
                        pCOD_MTVRESPBM_IDZ = new SQLiteParameter("@COD_MTVRESPBM_IDZ", pedido.COD_MOTIVO_RESUMO_PROBLEMA);
                        pCOD_TPOMTVRESPBM_IDZ = new SQLiteParameter("@COD_TPOMTVRESPBM_IDZ", pedido.COD_TIPO_MOTIVO_RESUMO_PROBLEMA);
                        pNUM_PEDCET_IDZ = new SQLiteParameter("@NUM_PEDCET_IDZ", pedido.NUM_PEDIDO_CLIENTE);
                        pNOM_CNT_IDZ = new SQLiteParameter("@NOM_CNT_IDZ", pedido.NOME_CONTATO);


                        // Grava apenas motivo de indenização Broker
                        if (pedido.COD_MOTIVO_DESTINO != -1)
                        {
                            // Executa a query salvando os dados
                            CSDataAccess.Instance.ExecuteScalar(sqlQueryInsert, pCOD_PEDIDO, pCOD_EMPREGADO, pNUM_DOCDVL_IDZ, pCOD_SERDOCDVL_IDZ, pVLR_DOCDVL_IDZ, pDAT_EMSDLCDVL_IDZ, pNUM_VOLDOCDVL_IDZ, pCOD_MTVDTN_IDZ, pCOD_TPOMTVDTN_IDZ, pCOD_MTVCNDRTR_IDZ, pCOD_TPOMTVCNDRTR_IDZ, pCOD_MTVRESPBM_IDZ, pCOD_TPOMTVRESPBM_IDZ, pNUM_PEDCET_IDZ, pNOM_CNT_IDZ);

                        }

                        // Muda o state dele para ObjectState.SALVO
                        pedido.STATE = ObjectState.SALVO;
                        break;

                    case ObjectState.ALTERADO:

                        pCOD_PEDIDO = new SQLiteParameter("@COD_PEDIDO", pedido.COD_PEDIDO);
                        pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", pedido.COD_EMPREGADO);
                        pNUM_DOCDVL_IDZ = new SQLiteParameter("@NUM_DOCDVL_IDZ", pedido.NUM_DOCUMENTO_DEVOLUCAO);
                        pCOD_SERDOCDVL_IDZ = new SQLiteParameter("@COD_SERDOCDVL_IDZ", pedido.SERIE_DOCUMENTO_DEVOLUCAO);

                        pVLR_DOCDVL_IDZ = new SQLiteParameter("@VLR_DOCDVL_IDZ", pedido.VALOR_DOCUMENTO_DEVOLUCAO);
                        pVLR_DOCDVL_IDZ.DbType = DbType.Decimal;
                        //TODO: Verificar necessidade de precision e scale para decimal
                        //pVLR_DOCDVL_IDZ.Precision = 9;
                        //pVLR_DOCDVL_IDZ.Scale = 2;

                        pDAT_EMSDLCDVL_IDZ = new SQLiteParameter("@DAT_EMSDLCDVL_IDZ", pedido.DAT_EMISSAO_DOCUMENTO_DEVOLUCAO);
                        pNUM_VOLDOCDVL_IDZ = new SQLiteParameter("@NUM_VOLDOCDVL_IDZ", pedido.NUM_VOLUME_DOCUMENTO_DEVOLUCAO);
                        pCOD_MTVDTN_IDZ = new SQLiteParameter("@COD_MTVDTN_IDZ", pedido.COD_MOTIVO_DESTINO);
                        pCOD_TPOMTVDTN_IDZ = new SQLiteParameter("@COD_TPOMTVDTN_IDZ", pedido.COD_TIPO_MOTIVO_DESTINO);
                        pCOD_MTVCNDRTR_IDZ = new SQLiteParameter("@COD_MTVCNDRTR_IDZ", pedido.COD_MOTIVO_CONDICAO_RETIRADA);
                        pCOD_TPOMTVCNDRTR_IDZ = new SQLiteParameter("@COD_TPOMTVCNDRTR_IDZ", pedido.COD_TIPO_MOTIVO_CONDICAO_RETIRADA);
                        pCOD_MTVRESPBM_IDZ = new SQLiteParameter("@COD_MTVRESPBM_IDZ", pedido.COD_MOTIVO_RESUMO_PROBLEMA);
                        pCOD_TPOMTVRESPBM_IDZ = new SQLiteParameter("@COD_TPOMTVRESPBM_IDZ", pedido.COD_TIPO_MOTIVO_RESUMO_PROBLEMA);
                        pNUM_PEDCET_IDZ = new SQLiteParameter("@NUM_PEDCET_IDZ", pedido.NUM_PEDIDO_CLIENTE);
                        pNOM_CNT_IDZ = new SQLiteParameter("@NOM_CNT_IDZ", pedido.NOME_CONTATO);

                        // Executa a query salvando os dados
                        CSDataAccess.Instance.ExecuteScalar(sqlQueryUpdate, pNUM_DOCDVL_IDZ, pCOD_SERDOCDVL_IDZ, pVLR_DOCDVL_IDZ, pDAT_EMSDLCDVL_IDZ, pNUM_VOLDOCDVL_IDZ, pCOD_MTVDTN_IDZ, pCOD_TPOMTVDTN_IDZ, pCOD_MTVCNDRTR_IDZ, pCOD_TPOMTVCNDRTR_IDZ, pCOD_MTVRESPBM_IDZ, pCOD_TPOMTVRESPBM_IDZ, pNUM_PEDCET_IDZ, pNOM_CNT_IDZ, pCOD_PEDIDO, pCOD_EMPREGADO);

                        // Muda o state dele para ObjectState.SALVO
                        pedido.STATE = ObjectState.SALVO;
                        break;

                    case ObjectState.DELETADO:

                        pCOD_PEDIDO = new SQLiteParameter("@COD_PEDIDO", pedido.COD_PEDIDO);
                        pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", pedido.COD_EMPREGADO);

                        // Executa a query apagando os dados
                        CSDataAccess.Instance.ExecuteScalar(sqlQueryDelete, pCOD_PEDIDO, pCOD_EMPREGADO);

                        // Remove o historico da coleção
                        this.InnerList.Remove(pedido);
                        break;
                }
            }

            return true;
        }

        public void Dispose()
        {
            this.InnerList.Clear();
            this.InnerList.TrimToSize();
        }

        public int Add(CSPedidosIndenizacao.CSPedidoIndenizacao pedido)
        {
            return this.InnerList.Add(pedido);
        }

        // Retorna a coleção dos pedidos de indenizacao
        public CSPedidosIndenizacao Items
        {
            get
            {
                return this;
            }
        }

        #endregion

        #region [ SubClasses ]
        /// <summary>
        /// Summary description for CSPedidoIndenizacao.
        /// </summary>        
        public class CSPedidoIndenizacao
        {
            #region [ Variáveis ]

            private int m_COD_PEDIDO = -1;
            private int m_COD_EMPREGADO = -1;
            private int m_NUM_DOCUMENTO_DEVOLUCAO = 0;
            private string m_SERIE_DOCUMENTO_DEVOLUCAO = "";
            private decimal m_VALOR_DOCUMENTO_DEVOLUCAO = 0;
            private DateTime m_DAT_EMISSAO_DOCUMENTO_DEVOLUCAO = DateTime.Now;
            private int m_NUM_VOLUME_DOCUMENTO_DEVOLUCAO = 0;
            private int m_COD_MOTIVO_DESTINO = -1;
            private int m_COD_TIPO_MOTIVO_DESTINO = -1;
            private int m_COD_MOTIVO_CONDICAO_RETIRADA = -1;
            private int m_COD_TIPO_MOTIVO_CONDICAO_RETIRADA = -1;
            private int m_COD_MOTIVO_RESUMO_PROBLEMA = -1;
            private int m_COD_TIPO_MOTIVO_RESUMO_PROBLEMA = -1;
            private string m_NUM_PEDIDO_CLIENTE = "";
            private string m_NOME_CONTATO = "";
            private ObjectState m_STATE = ObjectState.NOVO;

            #endregion

            #region [ Propriedades ]

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
            public int NUM_DOCUMENTO_DEVOLUCAO
            {
                get
                {
                    return m_NUM_DOCUMENTO_DEVOLUCAO;
                }
                set
                {
                    m_NUM_DOCUMENTO_DEVOLUCAO = value;
                }
            }
            public string SERIE_DOCUMENTO_DEVOLUCAO
            {
                get
                {
                    return m_SERIE_DOCUMENTO_DEVOLUCAO;
                }
                set
                {
                    m_SERIE_DOCUMENTO_DEVOLUCAO = value;
                }
            }
            public decimal VALOR_DOCUMENTO_DEVOLUCAO
            {
                get
                {
                    return m_VALOR_DOCUMENTO_DEVOLUCAO;
                }
                set
                {
                    m_VALOR_DOCUMENTO_DEVOLUCAO = value;
                }
            }
            public DateTime DAT_EMISSAO_DOCUMENTO_DEVOLUCAO
            {
                get
                {
                    return m_DAT_EMISSAO_DOCUMENTO_DEVOLUCAO;
                }
                set
                {
                    m_DAT_EMISSAO_DOCUMENTO_DEVOLUCAO = value;
                }
            }
            public int NUM_VOLUME_DOCUMENTO_DEVOLUCAO
            {
                get
                {
                    return m_NUM_VOLUME_DOCUMENTO_DEVOLUCAO;
                }
                set
                {
                    m_NUM_VOLUME_DOCUMENTO_DEVOLUCAO = value;
                }
            }
            public int COD_MOTIVO_DESTINO
            {
                get
                {
                    return m_COD_MOTIVO_DESTINO;
                }
                set
                {
                    m_COD_MOTIVO_DESTINO = value;
                }
            }
            public int COD_TIPO_MOTIVO_DESTINO
            {
                get
                {
                    return m_COD_TIPO_MOTIVO_DESTINO;
                }
                set
                {
                    m_COD_TIPO_MOTIVO_DESTINO = value;
                }
            }
            public int COD_MOTIVO_CONDICAO_RETIRADA
            {
                get
                {
                    return m_COD_MOTIVO_CONDICAO_RETIRADA;
                }
                set
                {
                    m_COD_MOTIVO_CONDICAO_RETIRADA = value;
                }
            }
            public int COD_TIPO_MOTIVO_CONDICAO_RETIRADA
            {
                get
                {
                    return m_COD_TIPO_MOTIVO_CONDICAO_RETIRADA;
                }
                set
                {
                    m_COD_TIPO_MOTIVO_CONDICAO_RETIRADA = value;
                }
            }
            public int COD_MOTIVO_RESUMO_PROBLEMA
            {
                get
                {
                    return m_COD_MOTIVO_RESUMO_PROBLEMA;
                }
                set
                {
                    m_COD_MOTIVO_RESUMO_PROBLEMA = value;
                }
            }
            public int COD_TIPO_MOTIVO_RESUMO_PROBLEMA
            {
                get
                {
                    return m_COD_TIPO_MOTIVO_RESUMO_PROBLEMA;
                }
                set
                {
                    m_COD_TIPO_MOTIVO_RESUMO_PROBLEMA = value;
                }
            }
            public string NUM_PEDIDO_CLIENTE
            {
                get
                {
                    return m_NUM_PEDIDO_CLIENTE;
                }
                set
                {
                    m_NUM_PEDIDO_CLIENTE = value;
                }
            }
            public string NOME_CONTATO
            {
                get
                {
                    return m_NOME_CONTATO;
                }
                set
                {
                    m_NOME_CONTATO = value;
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

            public CSPedidoIndenizacao()
            {
                this.STATE = ObjectState.NOVO;
            }

            #endregion

        }

        #endregion

    }
}
