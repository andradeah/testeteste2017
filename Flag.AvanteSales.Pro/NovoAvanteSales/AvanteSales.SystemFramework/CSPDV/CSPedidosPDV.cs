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
using System.Collections.Generic;
using System.Threading;

#endregion

namespace AvanteSales
{
    public class CSPedidosPDV : CollectionBase, IDisposable
    {
        #region[ Variaveis ]
        private CSPedidoPDV m_Current;
        private int _codPDV;
        private bool _disposed = true;
        private int m_PEDIDO_POSITION;
        #endregion

        #region[ Propriedades]
        /// <summary>
        /// Retorna coleção dos pedidos do PDV 
        /// </summary>
        public CSPedidosPDV Items
        {
            get
            {
                if (_disposed)
                {
                    this.InnerList.Clear();
                    Refresh();
                }

                return this;
            }
        }

        public int PEDIDO_POSITION
        {
            get
            {
                return m_PEDIDO_POSITION;
            }
            set
            {
                m_PEDIDO_POSITION = value;
            }
        }

        public CSPedidosPDV.CSPedidoPDV this[int Index]
        {
            get
            {
                if (_disposed)
                {
                    this.InnerList.Clear();
                    Refresh();
                }

                return (CSPedidosPDV.CSPedidoPDV)this.InnerList[Index];
            }
        }

        public CSPedidosPDV.CSPedidoPDV Current
        {
            get
            {
                return m_Current;
            }
            set
            {
                int result;
                SQLiteParameter pCOD_PEDIDO;


                //Carrega item dos Pedido atual para a tabela de imagem

                string sqlQueryDelete = "DELETE FROM TMPITEMPEDIDO ";
                string sqlInsertImagem =
                    "INSERT INTO TMPITEMPEDIDO " +
                    "  (VLR_UNITARIO,VLR_TOTAL,VLR_DESCONTO " +
                    "  ,PRC_DESCONTO,VLR_ADICIONAL_FINANCEIRO " +
                    "  ,PRC_ADICIONAL_FINANCEIRO,QTD_PEDIDA,STATE " +
                    "  ,CODPRODUTO,VLR_DESCONTO_UNITARIO " +
                    "  ,VLR_ADICIONAL_UNITARIO,COD_TABELA_PRECO " +
                    "  ,VLR_INDENIZACAO,VLR_VERBA_EXTRA,VLR_VERBA_NORMAL,COD_ITEM_COMBO,QTD_INDENIZACAO,VLR_UNITARIO_INDENIZACAO, IND_UTILIZA_QTD_SUGERIDA) " +
                    "  SELECT VLR_UNITARIO,VLR_TOTAL,VLR_DESCONTO " +
                    "        ,PRC_DESCONTO,VLR_ADICIONAL_FINANCEIRO " +
                    "        ,PRC_ADICIONAL_FINANCEIRO,QTD_PEDIDA,0 AS STATE " +
                    "        ,COD_PRODUTO,VLR_DESCONTO_UNITARIO " +
                    "        ,VLR_ADICIONAL_UNITARIO,COD_TABELA_PRECO " +
                    "        ,VLR_INDENIZACAO,VLR_VERBA_EXTRA,VLR_VERBA_NORMAL,COD_ITEM_COMBO,QTD_INDENIZACAO,VLR_UNITARIO_INDENIZACAO, IND_UTILIZA_QTD_SUGERIDA " +
                    "    FROM ITEM_PEDIDO " +
                    "   WHERE COD_PEDIDO = ? ";

                m_Current = value;
                //Apaga a tabela de imagem de item de pedidos
                CSDataAccess.Instance.ExecuteScalar(sqlQueryDelete);

                pCOD_PEDIDO = new SQLiteParameter("@COD_PEDIDO", m_Current == null ? 0 : m_Current.COD_PEDIDO);

                //Carrega item dos Pedido atual para a tabela de imagem
                result = CSDataAccess.Instance.ExecuteNonQuery(sqlInsertImagem, pCOD_PEDIDO);
            }
        }
        #endregion

        #region [ Métodos ]

        /// <summary>
        /// Contrutor da classe. Busca os pedidos do PDV
        /// </summary>
        public CSPedidosPDV(int COD_PDV)
        {
            _codPDV = COD_PDV;
        }

        public void Refresh()
        {
            try
            {
                string sqlQuery =
                   "SELECT COD_OPERACAO, COD_PEDIDO, COD_CONDICAO_PAGAMENTO, DAT_PEDIDO, COD_EMPREGADO " +
                   "      ,IND_HISTORICO, VLR_TOTAL_PEDIDO, DATA_ENTREGA, NUM_DOC_INDENIZACAO " +
                   "      ,COD_TIPO_MOT_INDENIZACAO, COD_MOT_INDENIZACAO, IND_VLR_DESCONTO_ATUSALDO " +
                   "      ,IND_VLR_INDENIZACAO_ATUSALDO, STA_PEDIDO_FLEXX, COD_PDV_SOLDTO " +
                   "      ,MENSAGEM_PEDIDO, RECADO_PEDIDO, IND_FOB, COD_POLITICA_CALCULO_PRECO ";

                if (CSEmpresa.ColunaExiste("PEDIDO", "COD_MOTIVO"))
                    sqlQuery += ",COD_MOTIVO";
                else
                    sqlQuery += ",null AS 'MOTIVO'";

                if (CSEmpresa.ColunaExiste("PEDIDO", "IND_EMAIL_ENVIAR"))
                    sqlQuery += ",IND_EMAIL_ENVIAR";
                else
                    sqlQuery += ",0 AS 'EMAILENVIAR'";

                sqlQuery += ",NUM_LATITUDE_LOCALIZACAO,NUM_LONGITUDE_LOCALIZACAO";

                if (CSEmpresa.ColunaExiste("PEDIDO", "IND_PEDIDO_RETORNADO"))
                    sqlQuery += ",IND_PEDIDO_RETORNADO";
                else
                    sqlQuery += ",0 AS 'INDPEDIDORETORNADO'";

                sqlQuery += "   FROM PEDIDO " +
                            "  WHERE COD_PDV = ? " +
                            "    AND IND_HISTORICO = 0 " +
                            "    AND DATE(DAT_PEDIDO) = DATE(?) ";

                SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", _codPDV);
                pCOD_PDV.DbType = DbType.Int32;
                SQLiteParameter pDAT_PEDIDO = new SQLiteParameter("@DAT_PEDIDO", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
                pDAT_PEDIDO.DbType = DbType.DateTime;

                // Busca todos os contatos do PDV
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_PDV, pDAT_PEDIDO))
                {
                    while (sqlReader.Read())
                    {
                        CSPedidoPDV pedidoPDV = new CSPedidoPDV();
                        //CSPDVs.Current.PEDIDOS_PDV.Current = pedidoPDV;

                        // Preenche a instancia da classe de pedido do pdv
                        pedidoPDV.OPERACAO = CSOperacoes.GetOperacao(sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0));
                        pedidoPDV.COD_PEDIDO = sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1);
                        pedidoPDV.CONDICAO_PAGAMENTO = CSCondicoesPagamento.GetCondicaPagamento(sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2));
                        pedidoPDV.DAT_PEDIDO = sqlReader.GetValue(3) == System.DBNull.Value ? new DateTime(1900, 1, 1) : sqlReader.GetDateTime(3);
                        pedidoPDV.EMPREGADO = CSEmpregados.GetEmpregado(sqlReader.GetValue(4) == System.DBNull.Value ? -1 : sqlReader.GetInt32(4));
                        pedidoPDV.IND_HISTORICO = sqlReader.GetValue(5) == System.DBNull.Value ? false : sqlReader.GetBoolean(5);
                        pedidoPDV.VLR_TOTAL_PEDIDO_INALTERADO = sqlReader.GetValue(6) == System.DBNull.Value ? -1 : decimal.Parse(sqlReader.GetValue(6).ToString());
                        pedidoPDV.DATA_ENTREGA = sqlReader.GetValue(7) == System.DBNull.Value ? new DateTime(1900, 1, 1) : sqlReader.GetDateTime(7);
                        pedidoPDV.NUM_DOC_INDENIZACAO = sqlReader.GetValue(8) == System.DBNull.Value ? "" : sqlReader.GetString(8);
                        pedidoPDV.COD_TIPO_MOT_INDENIZACAO = sqlReader.GetValue(9) == System.DBNull.Value ? -1 : sqlReader.GetInt32(9);
                        pedidoPDV.COD_MOT_INDENIZACAO = sqlReader.GetValue(10) == System.DBNull.Value ? -1 : sqlReader.GetInt32(10);
                        pedidoPDV.IND_VLR_DESCONTO_ATUSALDO = sqlReader.GetValue(11) == System.DBNull.Value ? false : sqlReader.GetBoolean(11);
                        pedidoPDV.IND_VLR_INDENIZACAO_ATUSALDO = sqlReader.GetValue(12) == System.DBNull.Value ? false : sqlReader.GetBoolean(12);
                        pedidoPDV.STA_PEDIDO_FLEXX = sqlReader.GetValue(13) == System.DBNull.Value ? -1 : sqlReader.GetInt32(13);
                        pedidoPDV.COD_PDV_SOLDTO = sqlReader.GetValue(14) == System.DBNull.Value ? -1 : sqlReader.GetInt32(14);
                        pedidoPDV.MENSAGEM_PEDIDO = sqlReader.GetValue(15) == System.DBNull.Value ? "" : sqlReader.GetString(15);
                        pedidoPDV.RECADO_PEDIDO = sqlReader.GetValue(16) == System.DBNull.Value ? "" : sqlReader.GetString(16);
                        pedidoPDV.IND_FOB = sqlReader.GetValue(17) == System.DBNull.Value ? false : sqlReader.GetBoolean(17);
                        pedidoPDV.COD_POLITICA_CALCULO_PRECO = sqlReader.GetValue(18) == System.DBNull.Value ? 1 : sqlReader.GetInt32(18);

                        if (sqlReader.GetValue(19) != System.DBNull.Value)
                            pedidoPDV.COD_MOTIVO = sqlReader.GetInt32(19);

                        pedidoPDV.IND_EMAIL_ENVIAR = sqlReader.GetValue(20) == System.DBNull.Value ? false : sqlReader.GetBoolean(20);
                        pedidoPDV.NUM_LATITUDE_LOCALIZACAO = sqlReader.GetValue(21) == System.DBNull.Value ? string.Empty : sqlReader.GetString(21);
                        pedidoPDV.NUM_LONGITUDE_LOCALIZACAO = sqlReader.GetValue(22) == System.DBNull.Value ? string.Empty : sqlReader.GetString(22);
                        pedidoPDV.IND_PEDIDO_RETORNADO = sqlReader.GetValue(23) == System.DBNull.Value ? false : sqlReader.GetBoolean(23);

                        //pedidoPDV.DSC_NOME_FOTO = sqlReader.GetValue(21) == System.DBNull.Value ? string.Empty : sqlReader.GetString(21);
                        //Necessario manter o valor total do pedido inalterado para ser utilizado 
                        //na validacao do limite de credito
                        pedidoPDV.STATE = ObjectState.INALTERADO;

                        // Busca os items do pedido
                        pedidoPDV.ITEMS_PEDIDOS = new CSItemsPedido(pedidoPDV.COD_PEDIDO);

                        // Busca o pedido de indenização
                        pedidoPDV.PEDIDOS_INDENIZACAO = new CSPedidosIndenizacao(CSEmpregados.Current.COD_EMPREGADO, pedidoPDV.COD_PEDIDO);

                        // [ Recupera valor da indenização ]
                        pedidoPDV.VLR_INDENIZACAO = 0;
                        foreach (CSItemsPedido.CSItemPedido itemPedido in pedidoPDV.ITEMS_PEDIDOS.Items)
                        {
                            if (itemPedido.STATE != ObjectState.DELETADO)
                                pedidoPDV.VLR_INDENIZACAO += itemPedido.VLR_INDENIZACAO;
                        }

                        // Adciona o pedido do PDV na coleção de pedidos deste PDV
                        this.InnerList.Add(pedidoPDV);
                    }
                    sqlReader.Close();
                }

                _disposed = false;
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new ApplicationException("Erro no Refresh de dados do PedidoCollection", ex);
            }
        }

        //Altera o campo bool para true quando o ciclo do salvamento do pedido se completa
        //Se for false ou null, significa que o sistema foi abortado antes do ciclo ser completado
        public static void MARCAR_PEDIDO_SALVO_CORRETAMENTE(int COD_PEDIDO)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                SQLiteParameter pCOD_PEDIDO = new SQLiteParameter("@COD_PEDIDO", COD_PEDIDO);

                sql.AppendLine("UPDATE [PEDIDO] SET [BOL_PEDIDO_VALIDADO] = 1 WHERE COD_PEDIDO = ?");

                CSDataAccess.Instance.ExecuteNonQuery(sql.ToString(), pCOD_PEDIDO);
            }
            catch (Exception ex)
            {
            }
        }

        //Altera o campo bool para null quando o pedido é aberto para edição
        //para executar a função de validação novamente
        public static void MARCAR_PEDIDO_SALVO_CORRETAMENTE_NULO(int COD_PEDIDO)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                SQLiteParameter pCOD_PEDIDO = new SQLiteParameter("@COD_PEDIDO", COD_PEDIDO);

                sql.AppendLine("UPDATE [PEDIDO] SET [BOL_PEDIDO_VALIDADO] = NULL WHERE COD_PEDIDO = ?");

                CSDataAccess.Instance.ExecuteNonQuery(sql.ToString(), pCOD_PEDIDO);
            }
            catch (Exception ex)
            {
            }
        }

        public static bool ExistePedidosComValoresIguais(int codPDV, decimal vlrTotalPedido)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT COUNT(*) FROM PEDIDO WHERE COD_PDV = ? AND DATE(DAT_PEDIDO) = DATE('NOW') AND VLR_TOTAL_PEDIDO = ? AND IND_HISTORICO = 0");

                SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", codPDV);
                SQLiteParameter pVLR_TOTAL_PEDIDO = new SQLiteParameter("@VLR_TOTAL_PEDIDO", vlrTotalPedido);

                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString(), pCOD_PDV, pVLR_TOTAL_PEDIDO))
                {
                    while (sqlReader.Read())
                    {
                        if (sqlReader.GetInt32(0) > 1)
                            return true;
                    }
                    sqlReader.Close();
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static List<int> CodigoPedidosValoresIguais(int codPDV, decimal vlrTotalPedido)
        {
            try
            {
                List<int> Resultados = new List<int>();

                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT COD_PEDIDO FROM PEDIDO WHERE COD_PDV = ? AND DATE(DAT_PEDIDO) = DATE('NOW') AND VLR_TOTAL_PEDIDO = ? AND IND_HISTORICO = 0");

                SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", codPDV);
                SQLiteParameter pVLR_TOTAL_PEDIDO = new SQLiteParameter("@VLR_TOTAL_PEDIDO", vlrTotalPedido);

                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString(), pCOD_PDV, pVLR_TOTAL_PEDIDO))
                {
                    while (sqlReader.Read())
                    {
                        Resultados.Add(sqlReader.GetInt32(0));
                    }
                    sqlReader.Close();
                }

                return Resultados;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static ArrayList PedidosValoresIguais()
        {
            try
            {
                ArrayList pedidos = new ArrayList();

                StringBuilder sql = new StringBuilder();

                sql.AppendLine("SELECT DISTINCT A.[COD_PEDIDO],A.[COD_PDV],A.[VLR_TOTAL_PEDIDO] FROM PEDIDO A ");
                sql.AppendLine("JOIN (SELECT COD_PDV,VLR_TOTAL_PEDIDO FROM PEDIDO WHERE DATE(DAT_PEDIDO) = DATE('NOW') GROUP BY COD_PDV,VLR_TOTAL_PEDIDO HAVING COUNT(*) > 1) B ");
                sql.AppendLine("ON A.[COD_PDV] = B.[COD_PDV] ");
                sql.AppendLine("AND A.[VLR_TOTAL_PEDIDO] = B.[VLR_TOTAL_PEDIDO] ");
                sql.AppendLine("JOIN ITEM_PEDIDO C ");
                sql.AppendLine("ON A.[COD_PEDIDO] = C.[COD_PEDIDO] ");
                sql.AppendLine("WHERE DATE(A.[DAT_PEDIDO]) = DATE('NOW') ");
                sql.AppendLine("AND A.[IND_HISTORICO] = 0");

                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
                {
                    while (sqlReader.Read())
                    {
                        CSPedidoPDV pedidoAtual = new CSPedidoPDV();

                        pedidoAtual.COD_PEDIDO = sqlReader.GetInt32(0);
                        pedidoAtual.COD_PDV = sqlReader.GetInt32(1);
                        pedidoAtual.VLR_TOTAL_PEDIDO_INALTERADO = sqlReader.GetDecimal(2);
                        pedidoAtual.ITEMS_PEDIDOS = new CSItemsPedido(pedidoAtual.COD_PEDIDO);

                        pedidos.Add(pedidoAtual);
                    }
                }

                return pedidos;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static int[] EXISTE_PEDIDO_SALVAMENTO_PENDENTE()
        {
            try
            {
                int[] Resultados = new int[2];

                StringBuilder sql = new StringBuilder();
                //SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", COD_PDV);

                sql.AppendLine("SELECT COD_PEDIDO, COD_PDV FROM PEDIDO WHERE DATE([DAT_PEDIDO]) = DATE('NOW') AND (BOL_PEDIDO_VALIDADO IS NULL OR BOL_PEDIDO_VALIDADO = 0) AND IND_HISTORICO = 0");

                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
                {
                    while (sqlReader.Read())
                    {
                        Resultados[0] = sqlReader.GetInt32(0);
                        Resultados[1] = sqlReader.GetInt32(1);
                    }
                    sqlReader.Close();
                }

                return Resultados;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static bool ExistePedidoDataEspecifica(DateTime data)
        {
            bool result = false;
            try
            {
                string sqlQuery =
                    "SELECT COUNT(COD_PEDIDO) AS QTDPDD " +
                    "  FROM PEDIDO " +
                    " WHERE DATE(DAT_PEDIDO) > DATE(?) ";

                SQLiteParameter pDAT_PEDIDO = new SQLiteParameter("@DAT_PEDIDO", data);
                pDAT_PEDIDO.DbType = DbType.DateTime;

                // Busca todos os contatos do PDV
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pDAT_PEDIDO))
                {
                    while (sqlReader.Read())
                    {
                        result = sqlReader.GetValue(0) == System.DBNull.Value ? false : sqlReader.GetInt32(0) == 0 ? false : true;
                    }
                    sqlReader.Close();
                }

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na Rotina ExistePedidoDataEspecifica ", ex);
            }
            return result;
        }

        /// <summary>
        /// Adiciona mais um pedido na coleção
        /// </summary>
        /// <param name="pedido">Uma instacia do pedido a ser adcionada</param>
        /// <returns>return a posição do pedido na coleção</returns>
        public int Add(CSPedidoPDV pedido)
        {
            try
            {
                return this.InnerList.Add(pedido);
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        /// <summary>
        /// Salva os dados na coleção no banco
        /// </summary>
        public void Flush()
        {
            try
            {
                //Cria transação 
                CSDataAccess.Instance.Transacao = CSDataAccess.Instance.Connection.BeginTransaction();

                string sqlQueryInsert =
                    "INSERT INTO PEDIDO " +
                    "  (COD_PEDIDO, COD_OPERACAO, COD_EMPREGADO, COD_PDV, DAT_PEDIDO, VLR_TOTAL_PEDIDO " +
                    "  ,COD_CONDICAO_PAGAMENTO, IND_HISTORICO, IND_PRECO_ANTERIOR, DAT_ALTERACAO " +
                    "  ,DATA_ENTREGA,COD_PDV_SOLDTO, NUM_DOC_INDENIZACAO, COD_TIPO_MOT_INDENIZACAO " +
                    "  ,COD_MOT_INDENIZACAO, IND_VLR_DESCONTO_ATUSALDO, IND_VLR_INDENIZACAO_ATUSALDO, MENSAGEM_PEDIDO, RECADO_PEDIDO, IND_FOB, COD_POLITICA_CALCULO_PRECO " +
                    "  ,NUM_LATITUDE_LOCALIZACAO, NUM_LONGITUDE_LOCALIZACAO";

                if (CSEmpresa.ColunaExiste("PEDIDO", "IND_EMAIL_ENVIAR"))
                    sqlQueryInsert += ",IND_EMAIL_ENVIAR";

                if (CSEmpresa.ColunaExiste("PEDIDO", "COD_MOTIVO"))
                    sqlQueryInsert += ",COD_MOTIVO";

                if (CSEmpresa.ColunaExiste("PEDIDO", "NUM_CPF_EMPREGADO"))
                    sqlQueryInsert += ",NUM_CPF_EMPREGADO)";
                else
                    sqlQueryInsert += ")";

                sqlQueryInsert += "  VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?";

                if (CSEmpresa.ColunaExiste("PEDIDO", "IND_EMAIL_ENVIAR"))
                    sqlQueryInsert += ",?";

                if (CSEmpresa.ColunaExiste("PEDIDO", "COD_MOTIVO"))
                    sqlQueryInsert += ",?";

                if (CSEmpresa.ColunaExiste("PEDIDO", "NUM_CPF_EMPREGADO"))
                    sqlQueryInsert += ",?)";
                else
                    sqlQueryInsert += ")";


                string sqlQueryUpdate =
                    "UPDATE PEDIDO " +
                    "   SET COD_OPERACAO = ?, DAT_PEDIDO = ?, VLR_TOTAL_PEDIDO = ?, COD_CONDICAO_PAGAMENTO = ? " +
                    "      ,DAT_ALTERACAO = ?, NUM_DOC_INDENIZACAO = ?, COD_TIPO_MOT_INDENIZACAO = ? " +
                    "      ,COD_MOT_INDENIZACAO = ?, IND_VLR_DESCONTO_ATUSALDO = ?, IND_VLR_INDENIZACAO_ATUSALDO = ? " +
                    "      ,MENSAGEM_PEDIDO = ?, RECADO_PEDIDO = ?, IND_FOB = ?, COD_POLITICA_CALCULO_PRECO = ? ";

                if (CSEmpresa.ColunaExiste("PEDIDO", "IND_EMAIL_ENVIAR"))
                    sqlQueryUpdate += ",IND_EMAIL_ENVIAR = ?";

                if (CSEmpresa.ColunaExiste("PEDIDO", "COD_MOTIVO"))
                    sqlQueryUpdate += ",COD_MOTIVO = ?";

                sqlQueryUpdate += " WHERE COD_PEDIDO = ? " +
                                  "   AND COD_EMPREGADO = ? ";

                string sqlQueryDelete =
                    "DELETE FROM PEDIDO " +
                    " WHERE COD_PEDIDO = ? " +
                    "   AND COD_EMPREGADO = ? ";

                string sqlQueryInsertTmp =
                    "INSERT INTO TMP_PEDIDO_EXCLUIDO (COD_OPERACAO,COD_PEDIDO,COD_EMPREGADO,COD_PDV,DAT_PEDIDO " +
                    "                                ,VLR_TOTAL_PEDIDO,COD_CONDICAO_PAGAMENTO,IND_HISTORICO " +
                    "                                ,COD_PEDIDO_FLEXX,IND_PRECO_ANTERIOR,DAT_ALTERACAO " +
                    "                                ,BOL_ATUALIZADO_FLEXX,COD_PEDIDO_POCKET,DATA_ENTREGA " +
                    "                                ,COD_PDV_SOLDTO, NUM_DOC_INDENIZACAO, COD_TIPO_MOT_INDENIZACAO " +
                    "                                ,COD_MOT_INDENIZACAO, IND_VLR_DESCONTO_ATUSALDO, IND_VLR_INDENIZACAO_ATUSALDO " +
                    "                                ,MENSAGEM_PEDIDO, RECADO_PEDIDO, IND_FOB, COD_POLITICA_CALCULO_PRECO) " +
                    "  SELECT COD_OPERACAO,COD_PEDIDO,COD_EMPREGADO,COD_PDV,DAT_PEDIDO,VLR_TOTAL_PEDIDO " +
                    "        ,COD_CONDICAO_PAGAMENTO,IND_HISTORICO,COD_PEDIDO_FLEXX,IND_PRECO_ANTERIOR " +
                    "        ,?,BOL_ATUALIZADO_FLEXX,COD_PEDIDO_POCKET,DATA_ENTREGA " +
                    "        ,COD_PDV_SOLDTO, NUM_DOC_INDENIZACAO, COD_TIPO_MOT_INDENIZACAO " +
                    "        ,COD_MOT_INDENIZACAO, IND_VLR_DESCONTO_ATUSALDO, IND_VLR_INDENIZACAO_ATUSALDO " +
                    "        ,MENSAGEM_PEDIDO, RECADO_PEDIDO, IND_FOB, COD_POLITICA_CALCULO_PRECO " +
                    "    FROM PEDIDO " +
                    "   WHERE IND_HISTORICO = 0 AND COD_PEDIDO = ? AND COD_EMPREGADO = ? ";

                // Varre a coleção procurando os objetos a serem persistidos
                foreach (CSPedidoPDV pedido in ((System.Collections.ArrayList)(base.InnerList.Clone())))
                {
                    if (pedido.COD_PEDIDO == CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO)
                    {
                        // Criar os parametros de salvamento
                        SQLiteParameter pCOD_PEDIDO = new SQLiteParameter("@COD_PEDIDO", pedido.COD_PEDIDO);
                        SQLiteParameter pCOD_OPERACAO = new SQLiteParameter("@COD_OPERACAO", pedido.OPERACAO.COD_OPERACAO);
                        SQLiteParameter pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                        SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", CSPDVs.Current.COD_PDV);
                        SQLiteParameter pDAT_PEDIDO = new SQLiteParameter("@DAT_PEDIDO", pedido.DAT_PEDIDO);
                        SQLiteParameter pVLR_TOTAL_PEDIDO = new SQLiteParameter("@VLR_TOTAL_PEDIDO", pedido.VLR_TOTAL_PEDIDO);
                        SQLiteParameter pCOD_CONDICAO_PAGAMENTO = new SQLiteParameter("@COD_CONDICAO_PAGAMENTO", pedido.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO);
                        SQLiteParameter pIND_HISTORICO = new SQLiteParameter("@IND_HISTORICO", pedido.IND_HISTORICO == true ? 1 : 0);
                        SQLiteParameter pIND_PRECO_ANTERIOR = new SQLiteParameter("@IND_PRECO_ANTERIOR", false);
                        SQLiteParameter pDAT_ALTERACAO = new SQLiteParameter("@DAT_ALTERACAO", CSEmpresa.Current.DATA_ULTIMA_DESCARGA);
                        SQLiteParameter pDATA_ENTREGA = new SQLiteParameter("@DATA_ENTREGA", pedido.DATA_ENTREGA);
                        SQLiteParameter pCOD_PDV_SOLDTO = new SQLiteParameter("@COD_PDV_SOLDTO", pedido.COD_PDV_SOLDTO);
                        SqliteParameter pIND_EMAIL_ENVIAR = new SQLiteParameter("@IND_EMAIL_ENVIAR", pedido.IND_EMAIL_ENVIAR);
                        SqliteParameter pCOD_MOTIVO = null;
                        SqliteParameter pNUM_CPF_EMPREGADO = new SQLiteParameter("@NUM_CPF_EMPREGADO", CSEmpregados.Current.NUM_CPF_EMPREGADO);
                        SqliteParameter pNUM_LATITUDE_LOCALIZACAO = new SQLiteParameter("@NUM_LATITUDE_LOCALIZACAO", pedido.NUM_LATITUDE_LOCALIZACAO);
                        SQLiteParameter pNUM_LONGITUDE_LOCALIZACAO = new SQLiteParameter("@NUM_LONGITUDE_LOCALIZACAO", pedido.NUM_LONGITUDE_LOCALIZACAO);

                        if (!pedido.COD_MOTIVO.HasValue)
                            pCOD_MOTIVO = new SQLiteParameter("@COD_MOTIVO", DBNull.Value);
                        else
                            pCOD_MOTIVO = new SQLiteParameter("@COD_MOTIVO", pedido.COD_MOTIVO.Value);

                        SQLiteParameter pNUM_DOC_INDENIZACAO = null;
                        if (pedido.NUM_DOC_INDENIZACAO == "")
                            pNUM_DOC_INDENIZACAO = new SQLiteParameter("@NUM_DOC_INDENIZACAO", DBNull.Value);
                        else
                            pNUM_DOC_INDENIZACAO = new SQLiteParameter("@NUM_DOC_INDENIZACAO", pedido.NUM_DOC_INDENIZACAO);

                        SQLiteParameter pCOD_TIPO_MOT_INDENIZACAO = null;
                        if (pedido.COD_TIPO_MOT_INDENIZACAO == -1)
                            pCOD_TIPO_MOT_INDENIZACAO = new SQLiteParameter("@COD_TIPO_MOT_INDENIZACAO", DBNull.Value);
                        else
                            pCOD_TIPO_MOT_INDENIZACAO = new SQLiteParameter("@COD_TIPO_MOT_INDENIZACAO", pedido.COD_TIPO_MOT_INDENIZACAO);

                        SQLiteParameter pCOD_MOT_INDENIZACAO = null;
                        if (pedido.COD_MOT_INDENIZACAO == -1)
                            pCOD_MOT_INDENIZACAO = new SQLiteParameter("@COD_MOT_INDENIZACAO", DBNull.Value);
                        else
                            pCOD_MOT_INDENIZACAO = new SQLiteParameter("@COD_MOT_INDENIZACAO", pedido.COD_MOT_INDENIZACAO);

                        SQLiteParameter pIND_VLR_DESCONTO_ATUSALDO = new SQLiteParameter("@IND_VLR_DESCONTO_ATUSALDO", pedido.IND_VLR_DESCONTO_ATUSALDO);
                        SQLiteParameter pIND_VLR_INDENIZACAO_ATUSALDO = new SQLiteParameter("@IND_VLR_INDENIZACAO_ATUSALDO", pedido.IND_VLR_INDENIZACAO_ATUSALDO);
                        SQLiteParameter pMENSAGEM_PEDIDO = new SQLiteParameter("@MENSAGEM_PEDIDO", pedido.MENSAGEM_PEDIDO);
                        SQLiteParameter pRECADO_PEDIDO = new SQLiteParameter("@RECADO_PEDIDO", pedido.RECADO_PEDIDO);
                        SQLiteParameter pIND_FOB = new SQLiteParameter("@IND_FOB", pedido.IND_FOB);
                        SQLiteParameter pCOD_POLITICA_CALCULO_PRECO = new SQLiteParameter("@COD_POLITICA_CALCULO_PRECO", pedido.COD_POLITICA_CALCULO_PRECO);
                        //SqliteParameter pDSC_NOME_FOTO = new SQLiteParameter("@DSC_NOME_FOTO", pedido.DSC_NOME_FOTO);

                        pCOD_PEDIDO.DbType = DbType.Int32;
                        pCOD_OPERACAO.DbType = DbType.Int32;
                        pCOD_EMPREGADO.DbType = DbType.Int32;
                        pCOD_PDV.DbType = DbType.Int32;
                        pDAT_PEDIDO.DbType = DbType.DateTime;
                        pVLR_TOTAL_PEDIDO.DbType = DbType.Decimal;
                        pCOD_CONDICAO_PAGAMENTO.DbType = DbType.Int32;
                        pIND_PRECO_ANTERIOR.DbType = DbType.Boolean;
                        pDAT_ALTERACAO.DbType = DbType.DateTime;
                        pDATA_ENTREGA.DbType = DbType.DateTime;
                        pCOD_PDV_SOLDTO.DbType = DbType.Int32;
                        pNUM_DOC_INDENIZACAO.DbType = DbType.String;
                        pCOD_TIPO_MOT_INDENIZACAO.DbType = DbType.Int32;
                        pCOD_MOT_INDENIZACAO.DbType = DbType.Int32;
                        pIND_VLR_DESCONTO_ATUSALDO.DbType = DbType.Boolean;
                        pIND_VLR_INDENIZACAO_ATUSALDO.DbType = DbType.Boolean;
                        pMENSAGEM_PEDIDO.DbType = DbType.String;
                        pRECADO_PEDIDO.DbType = DbType.String;
                        pIND_FOB.DbType = DbType.Boolean;
                        pCOD_POLITICA_CALCULO_PRECO.DbType = DbType.Int32;
                        pIND_EMAIL_ENVIAR.DbType = DbType.Boolean;
                        pCOD_MOTIVO.DbType = DbType.Int32;
                        pNUM_CPF_EMPREGADO.DbType = DbType.String;
                        pNUM_LATITUDE_LOCALIZACAO.DbType = DbType.String;
                        pNUM_LONGITUDE_LOCALIZACAO.DbType = DbType.String;

                        switch (pedido.STATE)
                        {
                            case ObjectState.NOVO:

                                // Atualiza o objeto com o codigo do pedido gerado pelo banco
                                pCOD_PEDIDO = new SQLiteParameter("@COD_PEDIDO", GetNextCodPedido());
                                pCOD_PEDIDO.DbType = DbType.Int32;
                                pedido.COD_PEDIDO = CSGlobal.StrToInt(pCOD_PEDIDO.Value.ToString());
                                //Atualiza Data de Entrega
                                pDATA_ENTREGA.Value = CSEmpresa.Current.DATA_ENTREGA;
                                pedido.DATA_ENTREGA = CSEmpresa.Current.DATA_ENTREGA;


                                CSDataAccess.Instance.ExecuteNonQuery(sqlQueryInsert,
                                    pCOD_PEDIDO, pCOD_OPERACAO, pCOD_EMPREGADO, pCOD_PDV, pDAT_PEDIDO,
                                    pVLR_TOTAL_PEDIDO, pCOD_CONDICAO_PAGAMENTO, pIND_HISTORICO,
                                    pIND_PRECO_ANTERIOR, pDAT_ALTERACAO, pDATA_ENTREGA, pCOD_PDV_SOLDTO,
                                    pNUM_DOC_INDENIZACAO, pCOD_TIPO_MOT_INDENIZACAO, pCOD_MOT_INDENIZACAO,
                                    pIND_VLR_DESCONTO_ATUSALDO, pIND_VLR_INDENIZACAO_ATUSALDO, pMENSAGEM_PEDIDO, pRECADO_PEDIDO, pIND_FOB, pCOD_POLITICA_CALCULO_PRECO, pNUM_LATITUDE_LOCALIZACAO, pNUM_LONGITUDE_LOCALIZACAO, pIND_EMAIL_ENVIAR, pCOD_MOTIVO, CSEmpresa.ColunaExiste("EMPREGADO", "NUM_CPF_EMPREGADO") ? pNUM_CPF_EMPREGADO : null);


                                // Tambem salva os items do pedido
                                pedido.ITEMS_PEDIDOS.Flush();

                                // Atualiza Codido do pedido do pedido de Indenização                    
                                foreach (CSPedidosIndenizacao.CSPedidoIndenizacao pedidoIndenizacao in CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDOS_INDENIZACAO.Items)
                                {
                                    if (pedidoIndenizacao.COD_PEDIDO == -1 || pedido.STATE == ObjectState.NOVO)
                                        pedidoIndenizacao.COD_PEDIDO = pedido.COD_PEDIDO;

                                }

                                //Tambem salva o pedido de indenização
                                pedido.PEDIDOS_INDENIZACAO.Flush();

                                // Muda o state dele para ObjectState.SALVO
                                pedido.STATE = ObjectState.SALVO;
                                break;

                            case ObjectState.ALTERADO:

                                // Executa a query salvando os dados
                                try
                                {
                                    //if (CSEmpresa.ColunaExiste("PEDIDO", "IND_EMAIL_ENVIAR"))
                                    //{
                                    //    if (CSEmpresa.ColunaExiste("PEDIDO", "COD_MOTIVO"))
                                    //    {
                                    CSDataAccess.Instance.ExecuteNonQuery(sqlQueryUpdate, pCOD_OPERACAO, pDAT_PEDIDO,
                                                                pVLR_TOTAL_PEDIDO, pCOD_CONDICAO_PAGAMENTO, pDAT_ALTERACAO,
                                                                pNUM_DOC_INDENIZACAO, pCOD_TIPO_MOT_INDENIZACAO, pCOD_MOT_INDENIZACAO,
                                                                pIND_VLR_DESCONTO_ATUSALDO, pIND_VLR_INDENIZACAO_ATUSALDO, pMENSAGEM_PEDIDO, pRECADO_PEDIDO, pIND_FOB, pCOD_POLITICA_CALCULO_PRECO, pIND_EMAIL_ENVIAR, pCOD_MOTIVO, pCOD_PEDIDO, pCOD_EMPREGADO);
                                    //    }
                                    //    else
                                    //    {
                                    //        CSDataAccess.Instance.ExecuteNonQuery(sqlQueryUpdate, pCOD_OPERACAO, pDAT_PEDIDO,
                                    //                                    pVLR_TOTAL_PEDIDO, pCOD_CONDICAO_PAGAMENTO, pDAT_ALTERACAO,
                                    //                                    pNUM_DOC_INDENIZACAO, pCOD_TIPO_MOT_INDENIZACAO, pCOD_MOT_INDENIZACAO,
                                    //                                    pIND_VLR_DESCONTO_ATUSALDO, pIND_VLR_INDENIZACAO_ATUSALDO, pMENSAGEM_PEDIDO, pRECADO_PEDIDO, pIND_FOB, pCOD_POLITICA_CALCULO_PRECO, pIND_EMAIL_ENVIAR, pCOD_PEDIDO, pCOD_EMPREGADO);
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    if (CSEmpresa.ColunaExiste("PEDIDO", "COD_MOTIVO"))
                                    //    {
                                    //        CSDataAccess.Instance.ExecuteNonQuery(sqlQueryUpdate, pCOD_OPERACAO, pDAT_PEDIDO,
                                    //                                    pVLR_TOTAL_PEDIDO, pCOD_CONDICAO_PAGAMENTO, pDAT_ALTERACAO,
                                    //                                    pNUM_DOC_INDENIZACAO, pCOD_TIPO_MOT_INDENIZACAO, pCOD_MOT_INDENIZACAO,
                                    //                                    pIND_VLR_DESCONTO_ATUSALDO, pIND_VLR_INDENIZACAO_ATUSALDO, pMENSAGEM_PEDIDO, pRECADO_PEDIDO, pIND_FOB, pCOD_POLITICA_CALCULO_PRECO, pCOD_MOTIVO, pCOD_PEDIDO, pCOD_EMPREGADO);
                                    //    }
                                    //    else
                                    //    {
                                    //        CSDataAccess.Instance.ExecuteNonQuery(sqlQueryUpdate, pCOD_OPERACAO, pDAT_PEDIDO,
                                    //                                    pVLR_TOTAL_PEDIDO, pCOD_CONDICAO_PAGAMENTO, pDAT_ALTERACAO,
                                    //                                    pNUM_DOC_INDENIZACAO, pCOD_TIPO_MOT_INDENIZACAO, pCOD_MOT_INDENIZACAO,
                                    //                                    pIND_VLR_DESCONTO_ATUSALDO, pIND_VLR_INDENIZACAO_ATUSALDO, pMENSAGEM_PEDIDO, pRECADO_PEDIDO, pIND_FOB, pCOD_POLITICA_CALCULO_PRECO, pCOD_PEDIDO, pCOD_EMPREGADO);
                                    //    }
                                    //}
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }

                                // Tambem salva as alterações nos items do pedido
                                pedido.ITEMS_PEDIDOS.Flush();

                                // Atualiza Codido do pedido do pedido de Indenização                    
                                foreach (CSPedidosIndenizacao.CSPedidoIndenizacao pedidoIndenizacao in CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDOS_INDENIZACAO.Items)
                                {
                                    if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2 &&
                                        CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO != 20)
                                        pedidoIndenizacao.STATE = ObjectState.DELETADO;
                                }

                                // Tambem salva os alterações no pedido de indenização
                                pedido.PEDIDOS_INDENIZACAO.Flush();

                                // Muda o state dele para ObjectState.SALVO
                                pedido.STATE = ObjectState.SALVO;
                                break;

                            case ObjectState.DELETADO:

                                // Marca para apagar todos os items do pedido
                                foreach (CSItemsPedido.CSItemPedido itempedido in pedido.ITEMS_PEDIDOS)
                                {
                                    // [ Delete se já não estiver deletado ]
                                    if (itempedido.STATE != ObjectState.DELETADO)
                                        itempedido.STATE = ObjectState.DELETADO;
                                }

                                // Flush nos items do pedido, apagando primeiro os items para nao dar erro de chave no banco
                                pedido.ITEMS_PEDIDOS.Flush();

                                // Marca para apagar todos os pedido de indenização
                                foreach (CSPedidosIndenizacao.CSPedidoIndenizacao pedidoIndenizacao in pedido.PEDIDOS_INDENIZACAO.Items)
                                {
                                    // [ Delete se já não estiver deletado ]
                                    if (pedidoIndenizacao.STATE != ObjectState.DELETADO)
                                        pedidoIndenizacao.STATE = ObjectState.DELETADO;
                                }
                                // Flush no pedido de indenização, apagando primeiro o pedido para nao dar erro de chave no banco                                
                                pedido.PEDIDOS_INDENIZACAO.Flush();

                                pDAT_ALTERACAO = new SQLiteParameter("@DAT_ALTERACAO", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                                // Faz um backup do pedido que será apagado
                                CSDataAccess.Instance.ExecuteNonQuery(sqlQueryInsertTmp, pDAT_ALTERACAO, pCOD_PEDIDO, pCOD_EMPREGADO);

                                pCOD_PEDIDO = new SQLiteParameter("@COD_PEDIDO", pedido.COD_PEDIDO);
                                pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);

                                // Executa a query apagando o pedido
                                CSDataAccess.Instance.ExecuteNonQuery(sqlQueryDelete, pCOD_PEDIDO, pCOD_EMPREGADO);

                                // [ Se atualizou o saldo de verba do vendedor anteriormente ]
                                if (pedido.IND_VLR_INDENIZACAO_ATUSALDO)
                                    pedido.EMPREGADO.VAL_SALDO_DESCONTO += pedido.VLR_INDENIZACAO;

                                // Remove o pedido excluido da coleção
                                this.InnerList.Remove(pedido);
                                break;
                        }
                        break;
                    }
                }

                CSDataAccess.Instance.Transacao.Commit();

                // Varre a coleção procurando os objetos a serem persistidos
                foreach (CSPedidoPDV pedido in base.InnerList)
                {
                    //somente ira gravar o pedido corrente pois se nao fizer
                    //este tratamento vai tentar inserir item que ainda nao existe 
                    //pedido dando erro de foreign key
                    if (pedido.COD_PEDIDO == CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO)
                    {
                        pedido.VLR_TOTAL_PEDIDO_INALTERADO = pedido.VLR_TOTAL_PEDIDO;
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                try
                {
                    CSDataAccess.Instance.Transacao.Rollback();
                }
                catch (Exception e)
                {
                    CSGlobal.ShowMessage(e.ToString());
                }

                CSGlobal.ShowMessage(CSExceptions.CSException.GetFullMessage(ex));

                // FireFix: Guilherme Magalhães
                // Salva os pedidos e seus itemPedidos em XML
#if ANDROID
                TextWriter exDump = File.CreateText(CSGlobal.GetCurrentDirectory() + "/ExceptionDump " + DateTime.Now.ToString("HHmm") + ".xml");
#else
                TextWriter exDump = File.CreateText(CSGlobal.GetCurrentDirectory() + "\\ExceptionDump " + DateTime.Now.ToString("HHmm") + ".xml"); 
#endif

                Exception innerEx = ex;

                while (innerEx != null)
                {
                    exDump.WriteLine("----- Exception ------" + innerEx.ToString());
                    exDump.WriteLine(innerEx.Message);
                    exDump.WriteLine("----- Fim do Exception -----");

                    innerEx = innerEx.InnerException;
                }

                exDump.Close();


                // Loga os pedidos em XML

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateElement("Pedidos"));

                foreach (CSPedidoPDV pedido in this.List)
                {
                    // Cria o element de pedido
                    XmlElement xmlElePedido = xmlDoc.CreateElement("PEDIDO");

                    xmlElePedido.Attributes.Append(xmlDoc.CreateAttribute("COD_PEDIDO"));
                    xmlElePedido.Attributes["COD_PEDIDO"].InnerText = GetNextCodPedido().ToString();

                    xmlElePedido.Attributes.Append(xmlDoc.CreateAttribute("COD_OPERACAO"));
                    xmlElePedido.Attributes["COD_OPERACAO"].InnerText = pedido.OPERACAO.COD_OPERACAO.ToString();

                    xmlElePedido.Attributes.Append(xmlDoc.CreateAttribute("COD_EMPREGADO"));
                    xmlElePedido.Attributes["COD_EMPREGADO"].InnerText = CSEmpregados.Current.COD_EMPREGADO.ToString();

                    xmlElePedido.Attributes.Append(xmlDoc.CreateAttribute("COD_PDV"));
                    xmlElePedido.Attributes["COD_PDV"].InnerText = CSPDVs.Current.COD_PDV.ToString();

                    xmlElePedido.Attributes.Append(xmlDoc.CreateAttribute("DAT_PEDIDO"));
                    xmlElePedido.Attributes["DAT_PEDIDO"].InnerText = pedido.DAT_PEDIDO.ToString();

                    xmlElePedido.Attributes.Append(xmlDoc.CreateAttribute("VLR_TOTAL_PEDIDO"));
                    xmlElePedido.Attributes["VLR_TOTAL_PEDIDO"].InnerText = pedido.VLR_TOTAL_PEDIDO.ToString();

                    xmlElePedido.Attributes.Append(xmlDoc.CreateAttribute("COD_CONDICAO_PAGAMENTO"));
                    xmlElePedido.Attributes["COD_CONDICAO_PAGAMENTO"].InnerText = pedido.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO.ToString();

                    xmlElePedido.Attributes.Append(xmlDoc.CreateAttribute("IND_HISTORICO"));
                    xmlElePedido.Attributes["IND_HISTORICO"].InnerText = pedido.IND_HISTORICO.ToString();

                    xmlElePedido.Attributes.Append(xmlDoc.CreateAttribute("IND_PRECO_ANTERIOR"));
                    xmlElePedido.Attributes["IND_PRECO_ANTERIOR"].InnerText = false.ToString();

                    xmlElePedido.Attributes.Append(xmlDoc.CreateAttribute("COD_PDV_SOLDTO"));
                    xmlElePedido.Attributes["COD_PDV_SOLDTO"].InnerText = CSPDVs.Current.COD_PDV.ToString();

                    xmlElePedido.Attributes.Append(xmlDoc.CreateAttribute("NUM_DOC_INDENIZACAO"));
                    xmlElePedido.Attributes["NUM_DOC_INDENIZACAO"].InnerText = pedido.NUM_DOC_INDENIZACAO.ToString();

                    xmlElePedido.Attributes.Append(xmlDoc.CreateAttribute("COD_TIPO_MOT_INDENIZACAO"));
                    xmlElePedido.Attributes["COD_TIPO_MOT_INDENIZACAO"].InnerText = pedido.COD_TIPO_MOT_INDENIZACAO.ToString();

                    xmlElePedido.Attributes.Append(xmlDoc.CreateAttribute("COD_MOT_INDENIZACAO"));
                    xmlElePedido.Attributes["COD_MOT_INDENIZACAO"].InnerText = pedido.COD_MOT_INDENIZACAO.ToString();

                    xmlElePedido.Attributes.Append(xmlDoc.CreateAttribute("MENSAGEM_PEDIDO"));
                    xmlElePedido.Attributes["MENSAGEM_PEDIDO"].InnerText = pedido.MENSAGEM_PEDIDO.ToString();

                    xmlElePedido.Attributes.Append(xmlDoc.CreateAttribute("RECADO_PEDIDO"));
                    xmlElePedido.Attributes["RECADO_PEDIDO"].InnerText = pedido.RECADO_PEDIDO.ToString();

                    xmlElePedido.Attributes.Append(xmlDoc.CreateAttribute("IND_FOB"));
                    xmlElePedido.Attributes["IND_FOB"].InnerText = pedido.IND_FOB.ToString();

                    xmlElePedido.Attributes.Append(xmlDoc.CreateAttribute("COD_POLITICA_CALCULO_PRECO"));
                    xmlElePedido.Attributes["COD_POLITICA_CALCULO_PRECO"].InnerText = pedido.COD_POLITICA_CALCULO_PRECO.ToString();

                    xmlElePedido.Attributes.Append(xmlDoc.CreateAttribute("NUM_LATITUDE_LOCALIZACAO"));
                    xmlElePedido.Attributes["NUM_LATITUDE_LOCALIZACAO"].InnerText = pedido.NUM_LATITUDE_LOCALIZACAO;

                    xmlElePedido.Attributes.Append(xmlDoc.CreateAttribute("NUM_LONGITUDE_LOCALIZACAO"));
                    xmlElePedido.Attributes["NUM_LONGITUDE_LOCALIZACAO"].InnerText = pedido.NUM_LONGITUDE_LOCALIZACAO;

                    foreach (CSItemsPedido.CSItemPedido itemPedido in pedido.ITEMS_PEDIDOS)
                    {
                        XmlElement xmlEleItemPedido = xmlDoc.CreateElement("ITEM_PEDIDO");

                        xmlEleItemPedido.Attributes.Append(xmlDoc.CreateAttribute("COD_PEDIDO"));
                        xmlEleItemPedido.Attributes["COD_PEDIDO"].InnerText = CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO.ToString();

                        xmlEleItemPedido.Attributes.Append(xmlDoc.CreateAttribute("COD_PRODUTO"));
                        xmlEleItemPedido.Attributes["COD_PRODUTO"].InnerText = itemPedido.PRODUTO.COD_PRODUTO.ToString();

                        xmlEleItemPedido.Attributes.Append(xmlDoc.CreateAttribute("COD_EMPREGADO"));
                        xmlEleItemPedido.Attributes["COD_EMPREGADO"].InnerText = CSEmpregados.Current.COD_EMPREGADO.ToString();

                        xmlEleItemPedido.Attributes.Append(xmlDoc.CreateAttribute("VLR_UNITARIO"));
                        xmlEleItemPedido.Attributes["VLR_UNITARIO"].InnerText = itemPedido.VLR_ITEM_UNIDADE.ToString();

                        xmlEleItemPedido.Attributes.Append(xmlDoc.CreateAttribute("PRC_DESCONTO"));
                        xmlEleItemPedido.Attributes["PRC_DESCONTO"].InnerText = itemPedido.PRC_DESCONTO.ToString();

                        xmlEleItemPedido.Attributes.Append(xmlDoc.CreateAttribute("VLR_TOTAL"));
                        xmlEleItemPedido.Attributes["VLR_TOTAL"].InnerText = itemPedido.VLR_TOTAL_ITEM.ToString();

                        xmlEleItemPedido.Attributes.Append(xmlDoc.CreateAttribute("VLR_ADICIONAL_FINANCEIRO"));
                        xmlEleItemPedido.Attributes["VLR_ADICIONAL_FINANCEIRO"].InnerText = itemPedido.VLR_ADICIONAL_FINANCEIRO.ToString();

                        xmlEleItemPedido.Attributes.Append(xmlDoc.CreateAttribute("QTD_PEDIDA"));
                        xmlEleItemPedido.Attributes["QTD_PEDIDA"].InnerText = itemPedido.QTD_PEDIDA_TOTAL.ToString();

                        xmlEleItemPedido.Attributes.Append(xmlDoc.CreateAttribute("VLR_DESCONTO"));
                        xmlEleItemPedido.Attributes["VLR_DESCONTO"].InnerText = itemPedido.VLR_DESCONTO.ToString();

                        xmlEleItemPedido.Attributes.Append(xmlDoc.CreateAttribute("PRC_ADICIONAL_FINANCEIRO"));
                        xmlEleItemPedido.Attributes["PRC_ADICIONAL_FINANCEIRO"].InnerText = itemPedido.PRC_ADICIONAL_FINANCEIRO.ToString();

                        xmlEleItemPedido.Attributes.Append(xmlDoc.CreateAttribute("VLR_ADICIONAL_UNITARIO"));
                        xmlEleItemPedido.Attributes["VLR_ADICIONAL_UNITARIO"].InnerText = itemPedido.VLR_ADICIONAL_UNITARIO.ToString();

                        xmlEleItemPedido.Attributes.Append(xmlDoc.CreateAttribute("VLR_DESCONTO_UNITARIO"));
                        xmlEleItemPedido.Attributes["VLR_DESCONTO_UNITARIO"].InnerText = itemPedido.VLR_DESCONTO_UNITARIO.ToString();

                        xmlEleItemPedido.Attributes.Append(xmlDoc.CreateAttribute("COD_TABELA_PRECO"));
                        xmlEleItemPedido.Attributes["COD_TABELA_PRECO"].InnerText = itemPedido.COD_TABELA_PRECO.ToString();

                        xmlEleItemPedido.Attributes.Append(xmlDoc.CreateAttribute("VLR_INDENIZACAO"));
                        xmlEleItemPedido.Attributes["VLR_INDENIZACAO"].InnerText = itemPedido.VLR_INDENIZACAO.ToString();

                        xmlEleItemPedido.Attributes.Append(xmlDoc.CreateAttribute("VLR_VERBA_EXTRA"));
                        xmlEleItemPedido.Attributes["VLR_VERBA_EXTRA"].InnerText = itemPedido.VLR_VERBA_EXTRA.ToString();

                        xmlEleItemPedido.Attributes.Append(xmlDoc.CreateAttribute("VLR_VERBA_NORMAL"));
                        xmlEleItemPedido.Attributes["VLR_VERBA_NORMAL"].InnerText = itemPedido.VLR_VERBA_NORMAL.ToString();

                        xmlEleItemPedido.Attributes.Append(xmlDoc.CreateAttribute("COD_ITEM_COMBO"));
                        xmlEleItemPedido.Attributes["COD_ITEM_COMBO"].InnerText = itemPedido.COD_ITEM_COMBO.ToString();

                        xmlEleItemPedido.Attributes.Append(xmlDoc.CreateAttribute("IND_UTILIZA_QTD_SUGERIDA"));
                        xmlEleItemPedido.Attributes["IND_UTILIZA_QTD_SUGERIDA"].InnerText = itemPedido.IND_UTILIZA_QTD_SUGERIDA.ToString();

                        xmlElePedido.AppendChild(xmlEleItemPedido);
                    }

                    // Adiciona o element de pedido
                    xmlDoc.DocumentElement.AppendChild(xmlElePedido);
                }

#if ANDROID
                xmlDoc.Save(CSGlobal.GetCurrentDirectory() + "/PedidosDump " + DateTime.Now.ToString("HHmm") + ".xml");
#else
                xmlDoc.Save(CSGlobal.GetCurrentDirectory() + "\\PedidosDump " + DateTime.Now.ToString("HHmm") + ".xml"); 
#endif

                // Continua...

                throw new Exception("Erro no flush dos pedidos", ex);
            }
            finally
            {
                CSDataAccess.Instance.Transacao.Dispose();
                CSDataAccess.Instance.Transacao = null;
            }
        }

        /// <summary>
        /// Busca o codigo do ultimo pedido inserido
        /// </summary>
        /// <returns>codigo do pedido</returns>
        private int GetNextCodPedido()
        {
            int result = 0;
            int countPedido = 0;
            string sqlQuery = null;

            try
            {
                sqlQuery = "SELECT COUNT(COD_PEDIDO) FROM PEDIDO WHERE IND_HISTORICO = 0";

                using (SqliteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery))
                {
                    if (sqlReader.Read())
                    {
                        countPedido = sqlReader.GetInt32(0);
                    }
                }

                if (countPedido > 0 ||
                    CSEmpregados.Current.COD_ULTIMO_PEDIDO == -1)
                {
                    sqlQuery =
                        "SELECT MAX(COD_PEDIDO) " +
                        "  FROM PEDIDO ";

                    using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery))
                    {
                        if (sqlReader.Read())
                        {
                            result = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : int.Parse(sqlReader.GetInt64(0).ToString());
                        }

                        // Fecha o reader
                        sqlReader.Close();
                        sqlReader.Dispose();
                    }
                }
                else
                    result = CSEmpregados.Current.COD_ULTIMO_PEDIDO;

                return result + 1;

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca do codigo do pedido", ex);
            }
        }

        //public override string ToString()
        //{
        //    string ret = "";
        //    try
        //    {
        //        // Get the type of MyClass1.
        //        Type myType = this.GetType();
        //        // Get the members associated with MyClass1.
        //        PropertyInfo[] myProps = myType.GetProperties();
        //        foreach (PropertyInfo prop in myProps)
        //        {
        //            object propval;
        //            try
        //            {
        //                propval = myType.GetProperty(prop.Name).GetValue(this, null);
        //                ret += prop.Name + ": " + propval.ToString() + "\r\n";
        //            }
        //            catch (SystemException ex)
        //            {
        //                ret += prop.Name + ": " + ex.Message + "\r\n";
        //            }
        //        }

        //        return ret;
        //    }
        //    catch (Exception e)
        //    {
        //        //CSGlobal.GravarLog("CSPedidosPDV-ToString", e.Message, e.InnerException != null ? e.InnerException.ToString() : "", e.StackTrace);

        //        CSGlobal.ShowMessage(e.ToString());
        //        throw new Exception("An exception occurred...", e);
        //    }
        //}

        public void Dispose()
        {
            // Chama dispose nos child objects
            foreach (CSPedidoPDV pedido in this.InnerList)
            {
                pedido.Dispose();
            }

            this.InnerList.Clear();
            this.InnerList.TrimToSize();

            _disposed = true;
        }

        #endregion

        #region Pedido

        public class CSPedidoPDV :
#if ANDROID
 Java.Lang.Object,
#endif
 IDisposable
        {
            #region [ Variaveis ]

            private int m_COD_PEDIDO = -1;
            private CSOperacoes.CSOperacao m_OPERACAO;
            private CSEmpregados.CSEmpregado m_EMPREGADO;
            private DateTime m_DAT_PEDIDO = new DateTime(1900, 1, 1);
            private decimal m_VLR_TOTAL_PEDIDO_INALTERADO = -1;
            private CSCondicoesPagamento.CSCondicaoPagamento m_CONDICAO_PAGAMENTO;
            private bool m_IND_HISTORICO = false;
            private CSItemsPedido m_ITEMS_PEDIDOS;
            private ObjectState m_STATE = ObjectState.NOVO;
            private DateTime m_DATA_ENTREGA = new DateTime(1900, 1, 1);
            private bool m_PEDIDO_EDITADO;
            private int m_COD_PDV_SOLDTO;
            private int m_COD_PDV;
            private string m_NUM_DOC_INDENIZACAO = "";
            private int m_COD_TIPO_MOT_INDENIZACAO = -1;
            private int m_COD_MOT_INDENIZACAO = -1;
            private decimal m_VLR_INDENIZACAO = 0;
            private decimal m_VLR_INDENIZACAO_EXIBICAO;
            private bool m_IND_VLR_DESCONTO_ATUSALDO = false;
            private bool m_IND_VLR_INDENIZACAO_ATUSALDO = false;
            private int m_STA_PEDIDO_FLEXX = -1;
            private CSPedidosIndenizacao m_PEDIDOS_INDENIZACAO;
            private string m_MENSAGEM_PEDIDO = "";
            private string m_RECADO_PEDIDO = "";
            private bool m_IND_FOB = false;
            private bool m_BLOQUEAR_FOTO;
            private int m_COD_POLITICA_CALCULO_PRECO = 1;
            private decimal m_VLR_SALDO_CREDITO;
            private bool m_IND_EMAIL_ENVIAR;
            private bool? m_TODOS_SUGERIDOS_VENDIDOS;
            private bool? m_PEDIDO_SUGERIDO;
            private bool m_IND_PEDIDO_RETORNADO;
            private int? m_COD_MOTIVO;
            private string m_NUM_LATITUDE_LOCALIZACAO;
            private string m_NUM_LONGITUDE_LOCALIZACAO;
            private bool m_BOL_PEDIDO_VALIDADO;
            #endregion

            #region [ Propriedades ]

            public int? COD_MOTIVO
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

            public decimal VLR_SALDO_CREDITO
            {
                get
                {
                    return m_VLR_SALDO_CREDITO;
                }

                set
                {
                    m_VLR_SALDO_CREDITO = value;
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

            public CSEmpregados.CSEmpregado EMPREGADO
            {
                get
                {
                    return m_EMPREGADO;
                }
                set
                {
                    m_EMPREGADO = value;
                }
            }

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

            public DateTime DATA_ENTREGA
            {
                get
                {
                    return m_DATA_ENTREGA;
                }
                set
                {
                    m_DATA_ENTREGA = value;
                }
            }

            public bool PEDIDO_EDITADO
            {
                get
                {
                    return m_PEDIDO_EDITADO;
                }
                set
                {
                    m_PEDIDO_EDITADO = value;
                }
            }

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

            public decimal VLR_TOTAL_PEDIDO
            {
                get
                {
                    try
                    {
                        decimal valorTotal = 0;

                        foreach (CSItemsPedido.CSItemPedido itempedido in this.ITEMS_PEDIDOS.Items)
                        {
                            if (itempedido.STATE != ObjectState.DELETADO)
                                if (CSEmpresa.Current.IND_RATEIO_INDENIZACAO)
                                    valorTotal += (itempedido.VLR_TOTAL_ITEM - itempedido.VLR_INDENIZACAO_UNIDADE);
                                else
                                    //valorTotal += (itempedido.VLR_TOTAL_ITEM - CSGlobal.Round((itempedido.QTD_INDENIZACAO_TOTAL * itempedido.VLR_INDENIZACAO_UNIDADE),2));
                                    valorTotal += (itempedido.VLR_TOTAL_ITEM - itempedido.VLR_INDENIZACAO_UNIDADE);
                        }
                        return valorTotal;
                    }
                    catch (Exception ex)
                    {
                        return 0;
                    }
                }
            }

            public decimal VLR_ADICIONAL_FINANCEIRO
            {
                get
                {
                    try
                    {
                        decimal valorTotal = 0;

                        foreach (CSItemsPedido.CSItemPedido itempedido in this.ITEMS_PEDIDOS.Items)
                        {
                            if (itempedido.STATE != ObjectState.DELETADO && itempedido.QTD_PEDIDA_TOTAL != 0)
                                valorTotal += (itempedido.VLR_ADICIONAL_FINANCEIRO * itempedido.QTD_PEDIDA_TOTAL) / itempedido.PRODUTO.UNIDADES_POR_CAIXA;
                        }

                        return valorTotal;
                    }
                    catch (Exception ex)
                    {
                        return 0;
                    }
                }
            }

            public decimal VLR_TOTAL_PEDIDO_INALTERADO
            {
                get
                {
                    return m_VLR_TOTAL_PEDIDO_INALTERADO;
                }
                set
                {
                    m_VLR_TOTAL_PEDIDO_INALTERADO = value;
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
                    try
                    {
                        if (m_ITEMS_PEDIDOS == null)
                            m_ITEMS_PEDIDOS = new CSItemsPedido(-1);

                        return m_ITEMS_PEDIDOS;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
                set
                {
                    m_ITEMS_PEDIDOS = value;
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

            public string NUM_DOC_INDENIZACAO
            {
                get
                {
                    return m_NUM_DOC_INDENIZACAO;
                }
                set
                {
                    m_NUM_DOC_INDENIZACAO = value;
                }
            }

            public int COD_TIPO_MOT_INDENIZACAO
            {
                get
                {
                    return m_COD_TIPO_MOT_INDENIZACAO;
                }
                set
                {
                    m_COD_TIPO_MOT_INDENIZACAO = value;
                }
            }

            public int COD_MOT_INDENIZACAO
            {
                get
                {
                    return m_COD_MOT_INDENIZACAO;
                }
                set
                {
                    m_COD_MOT_INDENIZACAO = value;
                }
            }

            /// <summary>
            /// Foi solicitado pelo Diniz que se a empresa estiver configurada diferentemente de 'Abatimento', os dados de indenização e quantidade deveriam ser apresentados
            /// na tela somente como exibição.
            /// Arthur : foi criado essas propriedades '_EXIBICAO' com a finalidade de não alterar a regra de salvamento de pedido do avante para proteger as outras
            /// funcionalidades do método.
            /// </summary>
            public decimal VLR_INDENIZACAO_EXIBICAO
            {
                get
                {
                    return m_VLR_INDENIZACAO_EXIBICAO;
                }

                set
                {
                    m_VLR_INDENIZACAO_EXIBICAO = value;
                }
            }

            public decimal VLR_INDENIZACAO
            {
                get
                {
                    return m_VLR_INDENIZACAO;
                }
                set
                {
                    m_VLR_INDENIZACAO = value;
                }
            }

            public bool IND_VLR_DESCONTO_ATUSALDO
            {
                get
                {
                    return m_IND_VLR_DESCONTO_ATUSALDO;
                }
                set
                {
                    m_IND_VLR_DESCONTO_ATUSALDO = value;
                }
            }

            public bool IND_VLR_INDENIZACAO_ATUSALDO
            {
                get
                {
                    return m_IND_VLR_INDENIZACAO_ATUSALDO;
                }
                set
                {
                    m_IND_VLR_INDENIZACAO_ATUSALDO = value;
                }
            }

            public CSPedidosIndenizacao PEDIDOS_INDENIZACAO
            {
                get
                {
                    try
                    {
                        if (m_PEDIDOS_INDENIZACAO == null)
                            m_PEDIDOS_INDENIZACAO = new CSPedidosIndenizacao(this.EMPREGADO.COD_EMPREGADO, this.COD_PEDIDO);

                        return m_PEDIDOS_INDENIZACAO;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
                set
                {
                    m_PEDIDOS_INDENIZACAO = value;
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
            public string MENSAGEM_PEDIDO
            {
                get
                {
                    return m_MENSAGEM_PEDIDO;
                }
                set
                {
                    m_MENSAGEM_PEDIDO = value;
                }
            }
            public string RECADO_PEDIDO
            {
                get
                {
                    return m_RECADO_PEDIDO;
                }
                set
                {
                    m_RECADO_PEDIDO = value;
                }
            }
            public bool IND_FOB
            {
                get
                {
                    return m_IND_FOB;
                }
                set
                {
                    m_IND_FOB = value;
                }
            }

            public bool BLOQUEAR_FOTO
            {
                get
                {
                    return m_BLOQUEAR_FOTO;
                }
                set
                {
                    m_BLOQUEAR_FOTO = value;
                }
            }

            public int COD_POLITICA_CALCULO_PRECO
            {
                get
                {
                    return m_COD_POLITICA_CALCULO_PRECO;
                }
                set
                {
                    m_COD_POLITICA_CALCULO_PRECO = value;
                }
            }

            public bool IND_EMAIL_ENVIAR
            {
                get
                {
                    return m_IND_EMAIL_ENVIAR;
                }
                set
                {
                    m_IND_EMAIL_ENVIAR = value;
                }
            }

            public bool? TODOS_SUGERIDOS_VENDIDOS
            {
                get
                {
                    return m_TODOS_SUGERIDOS_VENDIDOS;
                }
                set
                {
                    m_TODOS_SUGERIDOS_VENDIDOS = value;
                }
            }

            public bool? PEDIDO_SUGERIDO
            {
                get
                {
                    return m_PEDIDO_SUGERIDO;
                }
                set
                {
                    m_PEDIDO_SUGERIDO = value;
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

            public bool IND_PEDIDO_RETORNADO
            {
                get
                {
                    return m_IND_PEDIDO_RETORNADO;
                }
                set
                {
                    m_IND_PEDIDO_RETORNADO = value;
                }
            }

            #endregion

            #region [ Eventos ]
            public delegate void ProdutoCalculado(string itemPedido);
            public static event ProdutoCalculado OnProdutoCalculado;
            public delegate void BeginRecalcProdutos(int totalProdutos);
            public static event BeginRecalcProdutos OnBeginRecalcProdutos;
            #endregion

            #region [ Metodos ]

            public CSPedidoPDV()
            {
                this.DAT_PEDIDO = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                //Se a data de entrega a ser sugerida for NULL
                if (CSEmpresa.Current.DATA_ENTREGA.Year == 1900)
                    CSEmpresa.Current.DATA_ENTREGA = this.DAT_PEDIDO;

                this.IND_VLR_DESCONTO_ATUSALDO = false;
                this.IND_VLR_INDENIZACAO_ATUSALDO = false;
            }

            public void Dispose()
            {
                this.ITEMS_PEDIDOS.Dispose();
                this.ITEMS_PEDIDOS = null;
            }

            // Descarta todas as alterações que foram feitas após ultimo flush
            public void DiscardChanges()
            {
                try
                {
                    string sqlQuery =
                        "SELECT COD_OPERACAO, COD_PEDIDO, COD_CONDICAO_PAGAMENTO, DAT_PEDIDO, COD_EMPREGADO " +
                        "      ,IND_HISTORICO, VLR_TOTAL_PEDIDO, NUM_DOC_INDENIZACAO " +
                        "      ,COD_TIPO_MOT_INDENIZACAO, COD_MOT_INDENIZACAO " +
                        "      ,IND_VLR_DESCONTO_ATUSALDO, IND_VLR_INDENIZACAO_ATUSALDO, STA_PEDIDO_FLEXX " +
                        "      ,COD_PDV_SOLDTO, MENSAGEM_PEDIDO, RECADO_PEDIDO, IND_FOB, COD_POLITICA_CALCULO_PRECO " +
                        "  FROM PEDIDO " +
                        " WHERE COD_PEDIDO = ? " +
                        "   AND COD_EMPREGADO = ? ";

                    SQLiteParameter pCOD_PEDIDO = new SQLiteParameter("@COD_PEDIDO", this.COD_PEDIDO);
                    SQLiteParameter pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);

                    // Busca todos os contatos do PDV
                    using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_PEDIDO, pCOD_EMPREGADO))
                    {
                        while (sqlReader.Read())
                        {
                            // Preenche a instancia da classe de pedido do pdv
                            this.OPERACAO = CSOperacoes.GetOperacao(sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0));
                            this.COD_PEDIDO = sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1);
                            this.CONDICAO_PAGAMENTO = CSCondicoesPagamento.GetCondicaPagamento(sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2));
                            this.DAT_PEDIDO = sqlReader.GetValue(3) == System.DBNull.Value ? new DateTime(1900, 1, 1) : sqlReader.GetDateTime(3);
                            this.EMPREGADO = CSEmpregados.GetEmpregado(sqlReader.GetValue(4) == System.DBNull.Value ? -1 : sqlReader.GetInt32(4));
                            this.IND_HISTORICO = sqlReader.GetValue(5) == System.DBNull.Value ? false : sqlReader.GetBoolean(5);
                            this.VLR_TOTAL_PEDIDO_INALTERADO = sqlReader.GetValue(6) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(6));
                            this.NUM_DOC_INDENIZACAO = sqlReader.GetValue(7) == System.DBNull.Value ? "" : sqlReader.GetString(7);
                            this.COD_TIPO_MOT_INDENIZACAO = sqlReader.GetValue(8) == System.DBNull.Value ? -1 : sqlReader.GetInt32(8);
                            this.COD_MOT_INDENIZACAO = sqlReader.GetValue(9) == System.DBNull.Value ? -1 : sqlReader.GetInt32(9);
                            this.IND_VLR_DESCONTO_ATUSALDO = sqlReader.GetValue(10) == System.DBNull.Value ? false : sqlReader.GetBoolean(10);
                            this.IND_VLR_INDENIZACAO_ATUSALDO = sqlReader.GetValue(11) == System.DBNull.Value ? false : sqlReader.GetBoolean(11);
                            this.STA_PEDIDO_FLEXX = sqlReader.GetValue(12) == System.DBNull.Value ? -1 : sqlReader.GetInt32(12);
                            this.COD_PDV_SOLDTO = sqlReader.GetValue(13) == System.DBNull.Value ? -1 : sqlReader.GetInt32(13);
                            this.MENSAGEM_PEDIDO = sqlReader.GetValue(14) == System.DBNull.Value ? "" : sqlReader.GetString(14);
                            this.RECADO_PEDIDO = sqlReader.GetValue(15) == System.DBNull.Value ? "" : sqlReader.GetString(15);
                            this.IND_FOB = sqlReader.GetValue(16) == System.DBNull.Value ? false : sqlReader.GetBoolean(16);
                            this.COD_POLITICA_CALCULO_PRECO = sqlReader.GetValue(17) == System.DBNull.Value ? 1 : sqlReader.GetInt32(17);

                            this.STATE = ObjectState.INALTERADO;

                            // Busca o pedido de indenizacao
                            this.PEDIDOS_INDENIZACAO = new CSPedidosIndenizacao(CSEmpregados.Current.COD_EMPREGADO, this.COD_PEDIDO);

                            // Busca os items do pedido
                            this.ITEMS_PEDIDOS = new CSItemsPedido(this.COD_PEDIDO);

                            // [ Recupera valor da indenização ]
                            this.VLR_INDENIZACAO = 0;
                            foreach (CSItemsPedido.CSItemPedido itemPedido in this.ITEMS_PEDIDOS.Items)
                            {
                                if (itemPedido.STATE != ObjectState.DELETADO)
                                    this.VLR_INDENIZACAO += itemPedido.VLR_INDENIZACAO;
                            }
                        }
                    }

                    // se houve alteracoes no valor de saldo de desconto descarta as alteracoes no valor do saldo
                    this.EMPREGADO.DiscardChanges();
                }
                catch (Exception ex)
                {
                    CSGlobal.ShowMessage(ex.ToString());
                    throw new Exception("Erro no DiscardChanges do pedido", ex);
                }
            }

            public void CalculaRateioIndenizacao()
            {
                try
                {
                    CSItemsPedido.CSItemPedido itemPedido = null;
                    decimal valorTotalPedido = this.VLR_TOTAL_PEDIDO;
                    decimal valorTotalIndenizacao = this.VLR_INDENIZACAO;
                    decimal valorIndenizacao = valorTotalIndenizacao;
                    decimal valorTotalItem;
                    decimal valorAdicionalFinanceiro;
                    int numeroTotalItens = 0;

                    if (CSEmpresa.Current.IND_RATEIO_INDENIZACAO)
                    {

                        // [ Conta itens válidos ]
                        for (int i = 0; i < this.ITEMS_PEDIDOS.Items.Count; i++)
                        {
                            if (this.ITEMS_PEDIDOS.Items[i].STATE != ObjectState.DELETADO &&
                                this.ITEMS_PEDIDOS.Items[i].QTD_PEDIDA_TOTAL != 0)
                                numeroTotalItens++;
                        }



                        // [ Faz o rateio de acordo com a participaçao de cada item no pedido ]
                        for (int i = 0; i < this.ITEMS_PEDIDOS.Items.Count; i++)
                        {
                            itemPedido = this.ITEMS_PEDIDOS.Items[i];

                            // [ Pula item deletado ]
                            if (itemPedido.STATE == ObjectState.DELETADO ||
                                this.ITEMS_PEDIDOS.Items[i].QTD_PEDIDA_TOTAL == 0)
                                continue;

                            //CSGlobal.ShowMessage("VLR_TOTAL_ITEM: " + itemPedido.VLR_TOTAL_ITEM + "\n" +
                            //    "VLR_DESCONTO: " + itemPedido.VLR_DESCONTO + "\n" +
                            //    "this.VLR_INDENIZACAO: " + this.VLR_INDENIZACAO + "\n" +
                            //    "VLR_INDENIZACAO: " + itemPedido.VLR_INDENIZACAO + "\n" +
                            //    "VLR_ADICIONAL_FINANCEIRO: " + itemPedido.VLR_ADICIONAL_FINANCEIRO + "\n" +
                            //    "UNIDADES_POR_CAIXA: " + itemPedido.PRODUTO.UNIDADES_POR_CAIXA);

                            valorAdicionalFinanceiro = (itemPedido.VLR_ADICIONAL_FINANCEIRO * itemPedido.QTD_PEDIDA_TOTAL) / itemPedido.PRODUTO.UNIDADES_POR_CAIXA;

                            // [ Recupera o valor do item sem o desconto ]
                            valorTotalItem = itemPedido.VLR_TOTAL_ITEM + itemPedido.VLR_DESCONTO - valorAdicionalFinanceiro;

                            // [ Verifica se é o último item ]
                            if (numeroTotalItens == 1)
                            {
                                itemPedido.VLR_INDENIZACAO = valorIndenizacao;

                            }
                            else
                            {
                                itemPedido.VLR_INDENIZACAO = CSGlobal.Round((valorTotalIndenizacao * itemPedido.VLR_TOTAL_ITEM) / valorTotalPedido, 2);
                                //itemPedido.VLR_INDENIZACAO = CSGlobal.Round(CSGlobal.Round((CSGlobal.Round((valorTotalIndenizacao * itemPedido.VLR_TOTAL_ITEM) / valorTotalPedido, 2) / itemPedido.QTD_PEDIDA_TOTAL), 2) * itemPedido.QTD_PEDIDA_TOTAL, 2);

                            }

                            // [ Verifica valor máximo da indenização ]
                            decimal valorMaximo = Math.Max(itemPedido.VLR_TOTAL_ITEM - itemPedido.VLR_DESCONTO - itemPedido.VLR_ADICIONAL_FINANCEIRO_TOTAL, 0);
                            if (itemPedido.VLR_INDENIZACAO > valorMaximo)
                                itemPedido.VLR_INDENIZACAO = valorMaximo;

                            valorIndenizacao -= itemPedido.VLR_INDENIZACAO;

                            // [ Valor final do desconto ]
                            itemPedido.VLR_DESCONTO += itemPedido.VLR_INDENIZACAO;
                            itemPedido.PRC_DESCONTO += CSGlobal.StrToDecimal((Convert.ToDouble((itemPedido.VLR_INDENIZACAO * 100) / valorTotalItem)).ToString(CSGlobal.DecimalStringFormat));

                            itemPedido.VLR_TOTAL_ITEM = valorTotalItem - itemPedido.VLR_DESCONTO + valorAdicionalFinanceiro;
                            itemPedido.VLR_ITEM_UNIDADE = CSGlobal.StrToDecimal((((itemPedido.VLR_TOTAL_ITEM * itemPedido.PRODUTO.UNIDADES_POR_CAIXA) / itemPedido.QTD_PEDIDA_TOTAL)).ToString(CSGlobal.DecimalStringFormat));
                            itemPedido.VLR_DESCONTO_UNITARIO = CSGlobal.StrToDecimal((((itemPedido.VLR_DESCONTO * itemPedido.PRODUTO.UNIDADES_POR_CAIXA) / itemPedido.QTD_PEDIDA_TOTAL)).ToString(CSGlobal.DecimalStringFormat));

                            // [ Salva alterações na tabela temporária ]
                            itemPedido.AtualizaImagem();

                            numeroTotalItens--;
                        }

                        // [ Se o valor total de indenização foi maior que o valor do pedido ]
                        // [ atualiza o valor da indenização                                 ]
                        this.VLR_INDENIZACAO -= valorIndenizacao;
                    }
                }
                catch (Exception ex)
                {
                }
            }

            public void DesfazRateioIndenizacao()
            {
                try
                {
                    CSItemsPedido.CSItemPedido itemPedido = null;
                    decimal valorTotalItem;
                    decimal valorAdicionalFinanceiro;

                    if (CSEmpresa.Current.IND_RATEIO_INDENIZACAO)
                    {

                        // [ Faz o rateio de acordo com a participaçao de cada item no pedido ]
                        for (int i = 0; i < this.ITEMS_PEDIDOS.Items.Count; i++)
                        {
                            itemPedido = this.ITEMS_PEDIDOS.Items[i];

                            // [ Pula item deletado ]
                            if (itemPedido.STATE == ObjectState.DELETADO || itemPedido.QTD_PEDIDA_TOTAL == 0)
                                continue;

                            valorAdicionalFinanceiro = (itemPedido.VLR_ADICIONAL_FINANCEIRO * itemPedido.QTD_PEDIDA_TOTAL) / itemPedido.PRODUTO.UNIDADES_POR_CAIXA;

                            // [ Recupera o valor do item sem o desconto ]
                            valorTotalItem = itemPedido.VLR_TOTAL_ITEM + itemPedido.VLR_DESCONTO - valorAdicionalFinanceiro;

                            // [ Valor final do desconto ]
                            itemPedido.VLR_DESCONTO -= itemPedido.VLR_INDENIZACAO;
                            itemPedido.PRC_DESCONTO -= CSGlobal.StrToDecimal((Convert.ToDouble((itemPedido.VLR_INDENIZACAO * 100) / valorTotalItem)).ToString(CSGlobal.DecimalStringFormat));

                            itemPedido.VLR_TOTAL_ITEM = valorTotalItem - itemPedido.VLR_DESCONTO + valorAdicionalFinanceiro;
                            itemPedido.VLR_ITEM_UNIDADE = CSGlobal.StrToDecimal((((itemPedido.VLR_TOTAL_ITEM * itemPedido.PRODUTO.UNIDADES_POR_CAIXA) / itemPedido.QTD_PEDIDA_TOTAL)).ToString(CSGlobal.DecimalStringFormat));
                            itemPedido.VLR_DESCONTO_UNITARIO = CSGlobal.StrToDecimal((((itemPedido.VLR_DESCONTO * itemPedido.PRODUTO.UNIDADES_POR_CAIXA) / itemPedido.QTD_PEDIDA_TOTAL)).ToString(CSGlobal.DecimalStringFormat));

                            // [ Salva alterações na tabela temporária ]
                            itemPedido.AtualizaImagem();
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }

            public void CalculaRateioIndenizacao(decimal valorTotalIndenizacao)
            {
                try
                {
                    DesfazRateioIndenizacao();

                    // [ Se atualizou o saldo de verba do vendedor anteriormente ]
                    if (this.IND_VLR_INDENIZACAO_ATUSALDO)
                        this.EMPREGADO.VAL_SALDO_DESCONTO += this.VLR_INDENIZACAO;

                    this.VLR_INDENIZACAO = valorTotalIndenizacao;

                    // [ Faz o recálculo do rateio ]
                    CalculaRateioIndenizacao();

                    // [ Atualiza saldo do vendedor ]
                    // [ 1 - Descontar do saldo     ]
                    // [ 2 - Não descontar          ]
                    // [ 3 - Indenização bloqueada  ] 
                    if (CSEmpresa.Current.IND_VLR_INDENIZACAO_ATUSALDO == 1)
                    {
                        this.EMPREGADO.VAL_SALDO_DESCONTO -= this.VLR_INDENIZACAO;
                        this.IND_VLR_INDENIZACAO_ATUSALDO = true;

                    }
                    else
                    {
                        this.IND_VLR_INDENIZACAO_ATUSALDO = false;
                    }
                }
                catch (Exception ex)
                {
                }
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
                    CSGlobal.ShowMessage(e.ToString());
                    throw new Exception("An exception occurred...", e);
                }
            }
#if ANDROID
            /// <summary>
            /// Recalcula o pedido apos alterar condição de pagamento (Prazo)
            /// </summary>
            public void RecalculaPedidoCondicaoPagamento()
            {
                CSItemsPedido.CSItemPedido itemPedido = null;
                decimal valorUnitarioSemADF;
                decimal percentualAdicionalFinanceiro;
                decimal valorAcrescimoUnitario;
                decimal valorAdicionalFinanceiro;


                try
                {
                    percentualAdicionalFinanceiro = this.CONDICAO_PAGAMENTO.PRC_ADICIONAL_FINANCEIRO;
                    var qtdItensPedidos = this.ITEMS_PEDIDOS.Items.Count;

                    if (OnBeginRecalcProdutos != null)
                    {
                        OnBeginRecalcProdutos(qtdItensPedidos);
                    }

                    for (int i = 0; i < qtdItensPedidos; i++)
                    {
                        //Thread.Sleep(500);

                        using (itemPedido = this.ITEMS_PEDIDOS.Items[i])
                        {
                            if (itemPedido.COD_ITEM_COMBO > 0)
                            {
                                itemPedido.LOCK_QTD = true;

                                //if (!CSGlobal.PedidoComCombo)
                                //    CSGlobal.PedidoComCombo = true;
                            }

                            itemPedido.PRODUTO.PRECOS_PRODUTO.Current = itemPedido.PRODUTO.PRECOS_PRODUTO.GetPrecoProduto(itemPedido.COD_TABELA_PRECO);

                            if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2)
                            {
                                if (itemPedido.PRODUTO.PRECOS_PRODUTO.Current == null)
                                {
                                    if (itemPedido.PRODUTO.PRECOS_PRODUTO.Count > 0)
                                    {
                                        itemPedido.PRODUTO.PRECOS_PRODUTO.Current = itemPedido.PRODUTO.PRECOS_PRODUTO.Items[0];
                                    }
                                }
                            }

                            // [ Pula item deletado ]
                            if (itemPedido.STATE == ObjectState.DELETADO || itemPedido.QTD_PEDIDA_TOTAL == 0)
                                continue;

                            if ((itemPedido.PRODUTO.PRECOS_PRODUTO == null || itemPedido.PRODUTO.PRECOS_PRODUTO.Current == null) && !CSGlobal.PedidoComCombo)
                                continue;

                            if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2 &&
                                CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 3)
                            {
                                //if (itemPedido.PRC_ADICIONAL_FINANCEIRO != percentualAdicionalFinanceiro)
                                //{
                                // Pega o valor unitário do produto atual
                                valorUnitarioSemADF = itemPedido.PRODUTO.PRECOS_PRODUTO.Current.VLR_PRODUTO;

                                //********************************************************************************************
                                // Se tiver valor acrescimo quantidade unitaria o valor da tabela tem que o valor da tabela + acrescimo                        
                                if (itemPedido.QTD_PEDIDA_UNIDADE > 0)
                                {
                                    valorAcrescimoUnitario = (valorUnitarioSemADF * itemPedido.PRODUTO.PRC_ACRESCIMO_QTDE_UNITARIA) / 100;
                                    // muda o valor de tabela acrescentando o valor de acrescimo por te aberto a caixa
                                    valorUnitarioSemADF += valorAcrescimoUnitario;
                                }

                                valorAdicionalFinanceiro = (valorUnitarioSemADF * (percentualAdicionalFinanceiro / 100));

                                decimal tabela = itemPedido.PRODUTO.PRECOS_PRODUTO.Current.VLR_PRODUTO;
                                decimal desconto = itemPedido.PRC_DESCONTO;

                                //CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current = itemPedido;

                                //if (!CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.IND_EXECUTOU_REGRA_DESCONTO)
                                //{
                                //    if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.CALCULO_DESCONTO)
                                //        CSGlobal.Vlr_Desconto = true;
                                //    else
                                //        CSGlobal.Vlr_Desconto = false;

                                //    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.IND_EXECUTOU_REGRA_DESCONTO = true;
                                //}

                                //if (!CSGlobal.Vlr_Desconto)
                                //{
                                tabela = tabela - itemPedido.VLR_DESCONTO_UNITARIO;
                                //itemPedido.VLR_DESCONTO = ((itemPedido.VLR_DESCONTO_UNITARIO / itemPedido.PRODUTO.UNIDADES_POR_CAIXA) * (itemPedido.QTD_PEDIDA_TOTAL));
                                //desconto = 0m;
                                //}

                                // Se tiver informado percentual de desconto recalculo
                                //if (itemPedido.PRC_DESCONTO != 0 &&
                                //    CSGlobal.Vlr_Desconto)
                                //{
                                decimal caixa = itemPedido.PRODUTO.UNIDADES_POR_CAIXA;

                                //decimal precoUnitario = itemPedido.PRODUTO.PRECOS_PRODUTO.Current.VLR_PRODUTO / caixa;
                                //decimal precoUnitarioDesconto = precoUnitario * (itemPedido.PRC_DESCONTO / 100);
                                //decimal descontoTotal = precoUnitarioDesconto * itemPedido.QTD_PEDIDA_TOTAL;

                                //itemPedido.VLR_DESCONTO_UNITARIO = precoUnitarioDesconto * caixa;
                                itemPedido.VLR_DESCONTO = Math.Round(((itemPedido.VLR_DESCONTO_UNITARIO / caixa) * (itemPedido.QTD_PEDIDA_TOTAL)), 2, MidpointRounding.AwayFromZero);
                                //}

                                decimal PrecoComDesconto = tabela;
                                decimal PrecoComDescontoUnitario = PrecoComDesconto / itemPedido.PRODUTO.UNIDADES_POR_CAIXA;
                                decimal PrecoComAdicionalDescontoUnitario = PrecoComDescontoUnitario * (1 + (percentualAdicionalFinanceiro / 100));
                                decimal Quantidade = itemPedido.QTD_PEDIDA_TOTAL;
                                decimal Valor = Math.Round((Quantidade * PrecoComAdicionalDescontoUnitario), 2, MidpointRounding.AwayFromZero);

                                //itemPedido.VLR_DESCONTO_UNITARIO = itemPedido.PRODUTO.PRECOS_PRODUTO.Current.VLR_PRODUTO -  Math.Round((PrecoComAdicionalDescontoUnitario * itemPedido.PRODUTO.UNIDADES_POR_CAIXA),2);

                                itemPedido.VLR_ADICIONAL_UNITARIO = Math.Round((valorAdicionalFinanceiro / caixa), 2, MidpointRounding.AwayFromZero);
                                //itemPedido.VLR_TOTAL_ITEM = CSGlobal.StrToDecimal(((decimal)((((valorUnitarioSemADF + valorAdicionalFinanceiro) - itemPedido.VLR_DESCONTO_UNITARIO) / itemPedido.PRODUTO.UNIDADES_POR_CAIXA) * itemPedido.QTD_PEDIDA_TOTAL)).ToString(CSGlobal.DecimalStringFormat));
                                itemPedido.VLR_TOTAL_ITEM = Valor;
                                itemPedido.VLR_ITEM_UNIDADE = CSGlobal.StrToDecimal(((decimal)(((valorUnitarioSemADF + valorAdicionalFinanceiro) - itemPedido.VLR_DESCONTO_UNITARIO))).ToString(CSGlobal.DecimalStringFormat));
                                //itemPedido.VLR_DESCONTO_UNITARIO = CSGlobal.StrToDecimal(((decimal)((itemPedido.VLR_DESCONTO * itemPedido.PRODUTO.UNIDADES_POR_CAIXA) / itemPedido.QTD_PEDIDA_TOTAL)).ToString(CSGlobal.DecimalStringFormat));
                                //}
                            }
                            else if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2)
                            {
                                if (CSEmpresa.Current.IND_UTILIZA_PRICE_2014)
                                    itemPedido.CalculaValor2014();
                                else
                                    itemPedido.CalculaValor();
                            }
                            //else
                            //itemPedido.CalcularValorBunge(itemPedido.PRODUTO.COD_PRODUTO, CSEmpresa.Current.COD_NOTEBOOK1, CSPDVs.Current.COD_PDV, DateTime.Now, itemPedido.PRODUTO, Convert.ToInt32(itemPedido.QTD_PEDIDA_INTEIRA), itemPedido.QTD_PEDIDA_UNIDADE, itemPedido.PRC_DESCONTO, itemPedido.VLR_ITEM_UNIDADE);

                            // [ Salva alterações na tabela temporária ]
                            itemPedido.AtualizaImagem();

                            if (OnProdutoCalculado != null)
                            {
                                OnProdutoCalculado(itemPedido.PRODUTO.DSC_APELIDO_PRODUTO);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception("Erro no Recalcula do pedido", ex);
                }

            }
#endif
            #endregion
        }

        #endregion

        public void DeletarPedidos()
        {
            try
            {
                CSDataAccess.Instance.Transacao = CSDataAccess.Instance.Connection.BeginTransaction();

                string sqlQueryDelete =
                        "DELETE FROM PEDIDO " +
                        " WHERE COD_PEDIDO = ? " +
                        "   AND COD_EMPREGADO = ? ";

                foreach (CSPedidoPDV pedido in ((System.Collections.ArrayList)(base.InnerList.Clone())))
                {
                    foreach (CSItemsPedido.CSItemPedido itempedido in pedido.ITEMS_PEDIDOS)
                    {
                        // [ Delete se já não estiver deletado ]
                        if (itempedido.STATE != ObjectState.DELETADO)
                            itempedido.STATE = ObjectState.DELETADO;
                    }

                    // Flush nos items do pedido, apagando primeiro os items para nao dar erro de chave no banco
                    pedido.ITEMS_PEDIDOS.Flush();

                    SQLiteParameter pCOD_PEDIDO = new SQLiteParameter("@COD_PEDIDO", pedido.COD_PEDIDO);
                    SQLiteParameter pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);

                    pCOD_PEDIDO.DbType = DbType.Int32;
                    pCOD_EMPREGADO.DbType = DbType.Int32;

                    pCOD_PEDIDO = new SQLiteParameter("@COD_PEDIDO", pedido.COD_PEDIDO);
                    pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);

                    // Executa a query apagando o pedido
                    CSDataAccess.Instance.ExecuteNonQuery(sqlQueryDelete, pCOD_PEDIDO, pCOD_EMPREGADO);
                }

                CSDataAccess.Instance.Transacao.Commit();
            }
            catch (Exception ex)
            {
                CSDataAccess.Instance.Transacao.Rollback();
            }
            finally
            {
                CSDataAccess.Instance.Transacao.Dispose();
                CSDataAccess.Instance.Transacao = null;
            }
        }
    }
}