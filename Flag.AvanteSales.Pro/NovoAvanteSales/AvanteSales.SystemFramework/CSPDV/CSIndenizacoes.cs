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
    public class CSIndenizacoes : CollectionBase, IDisposable
    {
        #region[ Variaveis ]
        private CSIndenizacao m_Current;
        private bool _disposed = true;
        private int _codPDV;
        #endregion

        #region[ Propriedades ]

        public CSIndenizacoes Items
        {
            get
            {
                if (_disposed)
                    Refresh();

                return this;
            }
        }

        public int Add(CSIndenizacao indenizacao)
        {
            return this.InnerList.Add(indenizacao);
        }

        public void Flush()
        {
            StringBuilder sqlInsert = new StringBuilder();

            sqlInsert.AppendLine("INSERT INTO INDENIZACAO(COD_INDENIZACAO, COD_PDV, COD_GRUPO_COMERCIALIZACAO, DAT_CADASTRO, NUM_NOTA_DEVOLUCAO, SERIE_NOTA, DAT_NOTA_DEVOLUCAO, ");
            sqlInsert.AppendLine("COD_VENDEDOR, VOLUME_INDENIZACAO, NOME_RESPONSAVEL, STATUS, VLR_TOTAL, CONDICAO_PAGAMENTO, PESO_BRUTO) VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?)");

            StringBuilder sqlUpdate = new StringBuilder();

            sqlUpdate.AppendLine("UPDATE INDENIZACAO SET DAT_CADASTRO = ?, NUM_NOTA_DEVOLUCAO = ?, SERIE_NOTA = ?, DAT_NOTA_DEVOLUCAO = ?, ");
            sqlUpdate.AppendLine("VOLUME_INDENIZACAO = ?, NOME_RESPONSAVEL = ?, STATUS = ?, VLR_TOTAL = ?, CONDICAO_PAGAMENTO = ?, PESO_BRUTO = ? ");
            sqlUpdate.AppendLine("WHERE COD_INDENIZACAO = ?");


            StringBuilder sqlDelete = new StringBuilder();
            sqlDelete.AppendLine("DELETE FROM INDENIZACAO WHERE COD_PDV = ? AND COD_INDENIZACAO = ?");

            foreach (CSIndenizacao indenizacao in ((System.Collections.ArrayList)(base.InnerList.Clone())))
            {
                if (indenizacao.COD_INDENIZACAO == CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.COD_INDENIZACAO)
                {
                    SqliteParameter pCOD_INDENIZACAO = new SQLiteParameter("@COD_INDENIZACAO", indenizacao.COD_INDENIZACAO);
                    SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", CSPDVs.Current.COD_PDV);
                    SQLiteParameter pCOD_GRUPO_COMERCIALIZACAO = new SQLiteParameter("@COD_GRUPO_COMERCIALIZACAO", indenizacao.COD_GRUPO_COMERCIALIZACAO);
                    SQLiteParameter pDAT_CADASTRO = new SQLiteParameter("@DAT_CADASTRO", indenizacao.DAT_CADASTRO);
                    SQLiteParameter pNUM_NOTA_DEVOLUCAO = new SQLiteParameter("@NUM_NOTA_DEVOLUCAO", indenizacao.NUM_NOTA_DEVOLUCAO);
                    SQLiteParameter pSERIE_NOTA = new SQLiteParameter("@SERIE_NOTA", indenizacao.SERIE_NOTA);
                    SQLiteParameter pDAT_NOTA_DEVOLUCAO = new SQLiteParameter("@DAT_NOTA_DEVOLUCAO", indenizacao.DAT_NOTA_DEVOLUCAO);
                    SQLiteParameter pCOD_VENDEDOR = new SQLiteParameter("@COD_VENDEDOR", CSEmpregados.Current.COD_EMPREGADO);
                    SQLiteParameter pVOLUME_INDENIZACAO = new SQLiteParameter("@VOLUME_INDENIZACAO", indenizacao.VOLUME_INDENIZACAO);
                    SQLiteParameter pNOME_RESPONSAVEL = new SQLiteParameter("@NOME_RESPONSAVEL", indenizacao.NOME_RESPONSAVEL);
                    SQLiteParameter pSTATUS = new SQLiteParameter("@STATUS", indenizacao.STATUS);
                    SQLiteParameter pVLR_TOTAL = new SQLiteParameter("@VLR_TOTAL", indenizacao.VLR_TOTAL);
                    SqliteParameter pCONDICAO_PAGAMENTO = new SQLiteParameter("@CONDICAO_PAGAMENTO", indenizacao.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO);
                    SQLiteParameter pPESO_BRUTO = new SQLiteParameter("@PESO_BRUTO", indenizacao.PESO_BRUTO);

                    pDAT_NOTA_DEVOLUCAO.DbType = DbType.DateTime;

                    switch (indenizacao.STATE)
                    {
                        case ObjectState.NOVO:
                            {
                                CSDataAccess.Instance.ExecuteNonQuery(sqlInsert.ToString(), pCOD_INDENIZACAO, pCOD_PDV, pCOD_GRUPO_COMERCIALIZACAO, pDAT_CADASTRO, pNUM_NOTA_DEVOLUCAO, pSERIE_NOTA, pDAT_NOTA_DEVOLUCAO
                                    , pCOD_VENDEDOR, pVOLUME_INDENIZACAO, pNOME_RESPONSAVEL, pSTATUS, pVLR_TOTAL, pCONDICAO_PAGAMENTO, pPESO_BRUTO);

                                indenizacao.ITEMS_INDENIZACAO.Flush();

                                indenizacao.STATE = ObjectState.SALVO;
                            }
                            break;
                        case ObjectState.ALTERADO:
                            {
                                CSDataAccess.Instance.ExecuteNonQuery(sqlUpdate.ToString(), pDAT_CADASTRO, pNUM_NOTA_DEVOLUCAO, pSERIE_NOTA, pDAT_NOTA_DEVOLUCAO, pVOLUME_INDENIZACAO, pNOME_RESPONSAVEL,
                                    pSTATUS, pVLR_TOTAL, pCONDICAO_PAGAMENTO, pPESO_BRUTO, pCOD_INDENIZACAO);

                                indenizacao.ITEMS_INDENIZACAO.Flush();

                                indenizacao.STATE = ObjectState.SALVO;
                            }
                            break;
                        case ObjectState.DELETADO:
                            {
                                foreach (CSItemsIndenizacao.CSItemIndenizacao itemIndenizacao in indenizacao.ITEMS_INDENIZACAO)
                                {
                                    // [ Delete se já não estiver deletado ]
                                    if (indenizacao.STATE != ObjectState.DELETADO)
                                        indenizacao.STATE = ObjectState.DELETADO;
                                }

                                // Flush nos items do pedido, apagando primeiro os items para nao dar erro de chave no banco
                                indenizacao.ITEMS_INDENIZACAO.Flush();

                                CSDataAccess.Instance.ExecuteNonQuery(sqlDelete.ToString(), pCOD_PDV, pCOD_INDENIZACAO);

                                this.InnerList.Remove(indenizacao);
                            }
                            break;
                    }
                }
            }
        }

        private void Refresh()
        {
            try
            {
                StringBuilder sql = new StringBuilder();

                sql.AppendLine("SELECT COD_INDENIZACAO, COD_PDV, COD_GRUPO_COMERCIALIZACAO, DAT_CADASTRO, NUM_NOTA_DEVOLUCAO, SERIE_NOTA, DAT_NOTA_DEVOLUCAO, ");
                sql.AppendLine("COD_VENDEDOR, VOLUME_INDENIZACAO, NOME_RESPONSAVEL, STATUS, VLR_TOTAL, CONDICAO_PAGAMENTO, PESO_BRUTO,IND_DESCARREGADO");
                sql.AppendLine(" FROM INDENIZACAO WHERE COD_PDV = ? AND date(DAT_CADASTRO) = date(?)");

                SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", _codPDV);
                SQLiteParameter pDAT_CADASTRO = new SQLiteParameter("@DAT_CADASTRO", DateTime.Now.Date);

                using (SqliteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString(), pCOD_PDV, pDAT_CADASTRO))
                {
                    while (sqlReader.Read())
                    {
                        CSIndenizacao pedidoIndenizacao = new CSIndenizacao();
                        CSPDVs.Current.PEDIDOS_INDENIZACAO.Current = pedidoIndenizacao;

                        pedidoIndenizacao.COD_INDENIZACAO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        pedidoIndenizacao.COD_PDV = sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1);
                        pedidoIndenizacao.COD_GRUPO_COMERCIALIZACAO = sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2);
                        pedidoIndenizacao.DAT_CADASTRO = sqlReader.GetValue(3) == System.DBNull.Value ? new DateTime(1900, 1, 1) : sqlReader.GetDateTime(3);
                        pedidoIndenizacao.NUM_NOTA_DEVOLUCAO = sqlReader.GetValue(4) == System.DBNull.Value ? -1 : sqlReader.GetInt32(4);
                        pedidoIndenizacao.SERIE_NOTA = sqlReader.GetValue(5) == System.DBNull.Value ? string.Empty : sqlReader.GetString(5);
                        pedidoIndenizacao.DAT_NOTA_DEVOLUCAO = sqlReader.GetValue(6) == System.DBNull.Value ? new DateTime(1900, 1, 1) : sqlReader.GetDateTime(6);
                        pedidoIndenizacao.COD_VENDEDOR = sqlReader.GetValue(7) == System.DBNull.Value ? -1 : sqlReader.GetInt32(7);
                        pedidoIndenizacao.VOLUME_INDENIZACAO = sqlReader.GetValue(8) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(8));
                        pedidoIndenizacao.NOME_RESPONSAVEL = sqlReader.GetValue(9) == System.DBNull.Value ? string.Empty : sqlReader.GetString(9);
                        pedidoIndenizacao.STATUS = sqlReader.GetValue(10) == System.DBNull.Value ? string.Empty : sqlReader.GetString(10);
                        pedidoIndenizacao.VLR_TOTAL = sqlReader.GetValue(11) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(11));
                        pedidoIndenizacao.CONDICAO_PAGAMENTO = CSCondicoesPagamento.GetCondicaPagamento(sqlReader.GetInt32(12));
                        pedidoIndenizacao.PESO_BRUTO = sqlReader.GetValue(13) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(13));
                        pedidoIndenizacao.IND_DESCARREGADO = sqlReader.GetValue(14) == System.DBNull.Value ? 0 : sqlReader.GetInt32(14);
                        pedidoIndenizacao.STATE = ObjectState.ALTERADO;
                        pedidoIndenizacao.ITEMS_INDENIZACAO = new CSItemsIndenizacao(pedidoIndenizacao.COD_INDENIZACAO);

                        this.InnerList.Add(pedidoIndenizacao);
                    }
                }

                _disposed = false;
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new ApplicationException("Erro no Refresh de dados do PedidoIndenizacao", ex);
            }
        }

        public static int ProximoCodigoIndenizacao()
        {
            int result = 0;
            string sqlQuery = null;

            try
            {
                sqlQuery =
                    "SELECT MAX(COD_INDENIZACAO) " +
                    "  FROM INDENIZACAO ";

                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery))
                {
                    if (sqlReader.Read())
                    {
                        result = sqlReader.GetValue(0) == System.DBNull.Value ? 0 : int.Parse(sqlReader.GetInt64(0).ToString());
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }

                return result + 1;
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca do codigo do pedido", ex);
            }
        }

        public CSIndenizacoes.CSIndenizacao this[int Index]
        {
            get
            {
                if (_disposed)
                    Refresh();

                return (CSIndenizacoes.CSIndenizacao)this.InnerList[Index];
            }
        }

        public CSIndenizacoes.CSIndenizacao Current
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

        public CSIndenizacoes(int COD_PDV)
        {
            _codPDV = COD_PDV;
        }

        public void Dispose()
        {
            // Chama dispose nos child objects
            foreach (CSIndenizacao indenizacao in this.InnerList)
            {
                indenizacao.Dispose();
            }

            this.InnerList.Clear();
            this.InnerList.TrimToSize();

            _disposed = true;
        }

        public class CSIndenizacao :
#if ANDROID
 Java.Lang.Object,
#endif
 IDisposable
        {
            #region[ Variáveis ]

            private int m_COD_INDENIZACAO = -1;
            private int m_COD_PDV;
            private int m_COD_GRUPO_COMERCIALIZACAO;
            private int m_IND_DESCARREGADO;
            private DateTime m_DAT_CADASTRO;
            private int m_NUM_NOTA_DEVOLUCAO;
            private string m_SERIE_NOTA;
            private DateTime? m_DAT_NOTA_DEVOLUCAO;
            private int m_COD_VENDEDOR;
            private decimal m_VOLUME_TOTAL_INDENIZACAO;
            private string m_NOME_RESPONSAVEL;
            private string m_STATUS;
            private decimal m_VLR_TOTAL;
            private decimal m_PESO_BRUTO;
            private CSItemsIndenizacao m_ITEMS_INDENIZACAO;
            private ObjectState m_STATE;
            private CSOperacoes.CSOperacao m_OPERACAO;
            private CSCondicoesPagamento.CSCondicaoPagamento m_CONDICAO_PAGAMENTO;

            #endregion

            #region[ Propriedades ]

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

            public DateTime DAT_CADASTRO
            {
                get
                {
                    return m_DAT_CADASTRO;
                }
                set
                {
                    m_DAT_CADASTRO = value;
                }
            }

            public int IND_DESCARREGADO
            {
                get
                {
                    return m_IND_DESCARREGADO;
                }
                set
                {
                    m_IND_DESCARREGADO = value;
                }
            }

            public int NUM_NOTA_DEVOLUCAO
            {
                get
                {
                    return m_NUM_NOTA_DEVOLUCAO;
                }
                set
                {
                    m_NUM_NOTA_DEVOLUCAO = value;
                }
            }

            public string SERIE_NOTA
            {
                get
                {
                    return m_SERIE_NOTA;
                }
                set
                {
                    m_SERIE_NOTA = value;
                }
            }

            public DateTime? DAT_NOTA_DEVOLUCAO
            {
                get
                {
                    if (!m_DAT_NOTA_DEVOLUCAO.HasValue)
                        return null;
                    else
                    {
                        if (m_DAT_NOTA_DEVOLUCAO.Value.ToString("dd-MM-yyyy") == "01-01-1900")
                            return null;
                        else
                            return m_DAT_NOTA_DEVOLUCAO;
                    }
                }
                set
                {
                    m_DAT_NOTA_DEVOLUCAO = value;
                }
            }

            public int COD_VENDEDOR
            {
                get
                {
                    return m_COD_VENDEDOR;
                }
                set
                {
                    m_COD_VENDEDOR = value;
                }
            }

            public decimal VOLUME_INDENIZACAO
            {
                get
                {
                    return m_VOLUME_TOTAL_INDENIZACAO;
                }
                set
                {
                    m_VOLUME_TOTAL_INDENIZACAO = value;
                }
            }

            public string NOME_RESPONSAVEL
            {
                get
                {
                    return m_NOME_RESPONSAVEL;
                }
                set
                {
                    m_NOME_RESPONSAVEL = value;
                }
            }

            public string STATUS
            {
                get
                {
                    return m_STATUS;
                }
                set
                {
                    m_STATUS = value;
                }
            }

            public decimal VLR_TOTAL
            {
                get
                {
                    return m_VLR_TOTAL;
                }
                set
                {
                    m_VLR_TOTAL = value;
                }
            }

            public CSItemsIndenizacao ITEMS_INDENIZACAO
            {
                get
                {
                    if (m_ITEMS_INDENIZACAO == null)
                        m_ITEMS_INDENIZACAO = new CSItemsIndenizacao(-1);

                    return m_ITEMS_INDENIZACAO;
                }
                set
                {
                    m_ITEMS_INDENIZACAO = value;
                }
            }

            public CSOperacoes.CSOperacao OPERACAO
            {
                get
                {
                    return m_OPERACAO;
                }
                set
                {
                    m_OPERACAO = value;
                }
            }

            public CSCondicoesPagamento.CSCondicaoPagamento CONDICAO_PAGAMENTO
            {
                get
                {
                    return m_CONDICAO_PAGAMENTO;
                }
                set
                {
                    m_CONDICAO_PAGAMENTO = value;
                }
            }

            public decimal PESO_BRUTO
            {
                get
                {
                    return m_PESO_BRUTO;
                }
                set
                {
                    m_PESO_BRUTO = value;
                }
            }

            #endregion
        }
    }
}
