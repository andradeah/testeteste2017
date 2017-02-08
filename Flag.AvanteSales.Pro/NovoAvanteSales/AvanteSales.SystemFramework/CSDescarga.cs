using System;
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

using System.IO;
using System.Text;


namespace AvanteSales
{
    /// <summary>
    /// Summary description for CSDescarga.
    /// </summary>
    public class CSDescarga
    {
        public CSDescarga()
        {
        }

        public static DataSet Descarga(string versaoAvante)
        {
            DataSet ds = null;

            try
            {
                // [ Monta o DataSet ]
                ds = MontaDataSetDescarga(true, versaoAvante);

                // [ Grava no log ]
                GravaXmlLog(ds);

                // [ retorna o dataset para o provider ]
                return ds;

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro no envio do dataset de descarga.", ex);
            }
        }

        public static DataSet MontaDataSetDescarga(bool Descarga,string versaoAvante)
        {
            SQLiteParameter pDAT_ALTERACAO = null;
            SQLiteParameter pDAT_HISTORICO_MOTIVO = null;
            SQLiteParameter pDAT_COLETA = null;
            SQLiteParameter pDAT_ENTRADA = null;

            DataTable dtPedido = null;
            DataTable dtItemPedido = null;
            DataTable dtPedidoIndenizao = null;
            DataTable dtPedidoExcluido = null;
            DataTable dtHistoricoMotivo = null;
            DataTable dtRespostaPesquisaMercado = null;
            DataTable dtRespostaPesquisa = null;
            DataTable dtMotivoNaoPesquisa = null;
            DataTable dtPesquisaMerchanPdv = null;
            DataTable dtMonitoramento = null;
            DataTable dtProdutoColetaEstoque = null;
            DataTable dtEntradaCreditoProduto = null;
            DataTable dtColetaIndenizacao = null;
            DataTable dtItemColetaIndenizacao = null;
            DataTable dtLog = null;
            DataTable dtPdv_email = null;
            DataTable dtTelefone_pdv = null;
            DataTable dtMotivoNaoPesquisaMercado = null;
            DataTable dtAlmocoEmpregado = null;
            DataTable dtPdvVisita = null;
            DataTable dtProdutoValidade = null;
            DataTable dtGrupoMarkup = null;

            DataSet ds = null;
            DateTime ultimaDescarga;

            try
            {
                ds = new DataSet("Descarga");

                //CSDataAccess.Instance.AbreConexao();

                // [ Cria tabelas auxiliares do sistema ]
                CSGlobal.CriaTabelasAuxiliares();

                // Busca a data da ultima descarga
                ultimaDescarga = DataUltimaDescarga;

                // todos pedidos cuja data de alteracao seja maior que ultima descarga
                pDAT_ALTERACAO = new SQLiteParameter("@DAT_ALTERACAO", ultimaDescarga);

#if ANDROID
                StringBuilder sqlPedido = new StringBuilder();
                sqlPedido.AppendLine("SELECT * ");
                sqlPedido.AppendLine("  FROM PEDIDO ");
                sqlPedido.AppendLine(" WHERE IND_HISTORICO = 0 ");
                sqlPedido.AppendLine("   AND DAT_ALTERACAO >= ? AND BOL_PEDIDO_VALIDADO = 1 ");

                if (CSEmpresa.ColunaExiste("PEDIDO", "IND_PEDIDO_RETORNADO"))
                    sqlPedido.AppendLine(" AND (IND_PEDIDO_RETORNADO = 0 OR IND_PEDIDO_RETORNADO IS NULL) ");

                sqlPedido.AppendLine(" ORDER BY COD_PEDIDO DESC");

                dtPedido = CSDataAccess.Instance.ExecuteDataTable(sqlPedido.ToString(), pDAT_ALTERACAO);
#else
                dtPedido = CSDataAccess.Instance.ExecuteDataTable(
                   "SELECT * " +
                   "  FROM PEDIDO " +
                   " WHERE IND_HISTORICO = 0 " +
                   "   AND DAT_ALTERACAO >= ?", pDAT_ALTERACAO);
#endif
                if (dtPedido == null)
                    dtPedido = new DataTable();

                dtPedido.TableName = "PEDIDO";
                ds.Tables.Add(dtPedido);

                // todos itens de pedido
                pDAT_ALTERACAO = new SQLiteParameter("@DAT_ALTERACAO", ultimaDescarga);

#if ANDROID
                StringBuilder sqlItemPedido = new StringBuilder();

                sqlItemPedido.AppendLine("SELECT T1.* ");
                sqlItemPedido.AppendLine("  FROM ITEM_PEDIDO T1 ");
                sqlItemPedido.AppendLine("  JOIN PEDIDO T2 ON T1.COD_PEDIDO = T2.COD_PEDIDO ");
                sqlItemPedido.AppendLine(" WHERE T2.IND_HISTORICO = 0 ");
                sqlItemPedido.AppendLine("   AND T2.DAT_ALTERACAO >= ? AND T2.BOL_PEDIDO_VALIDADO = 1 ");

                if (CSEmpresa.ColunaExiste("PEDIDO", "IND_PEDIDO_RETORNADO"))
                    sqlItemPedido.AppendLine(" AND (T2.IND_PEDIDO_RETORNADO = 0 OR T2.IND_PEDIDO_RETORNADO IS NULL) ");

                sqlItemPedido.AppendLine(" ORDER BY T1.COD_PEDIDO DESC ");

                dtItemPedido = CSDataAccess.Instance.ExecuteDataTable(sqlItemPedido.ToString(), pDAT_ALTERACAO);
#else
                dtItemPedido = CSDataAccess.Instance.ExecuteDataTable(
                    "SELECT T1.* " +
                    "  FROM ITEM_PEDIDO T1 " +
                    "  JOIN PEDIDO T2 ON T1.COD_PEDIDO = T2.COD_PEDIDO " +
                    " WHERE T2.IND_HISTORICO = 0 " +
                    "   AND T2.DAT_ALTERACAO >= ?", pDAT_ALTERACAO);

#endif
                if (dtItemPedido == null)
                    dtItemPedido = new DataTable();

                dtItemPedido.TableName = "ITEM_PEDIDO";
                ds.Tables.Add(dtItemPedido);

                if (CSDataAccess.Instance.TableExists("INDENIZACAO"))
                {
                    dtColetaIndenizacao = CSDataAccess.Instance.ExecuteDataTable(
                        "SELECT * " +
                        "FROM INDENIZACAO " +
                        "WHERE IND_DESCARREGADO != 1 OR IND_DESCARREGADO IS NULL " +
                        "ORDER BY COD_INDENIZACAO DESC");

                    if (dtColetaIndenizacao == null)
                        dtColetaIndenizacao = new DataTable();

                    dtColetaIndenizacao.TableName = "INDENIZACAO";
                    ds.Tables.Add(dtColetaIndenizacao);

                    dtItemColetaIndenizacao = CSDataAccess.Instance.ExecuteDataTable(
                        "SELECT T1.* " +
                        "FROM ITEM_INDENIZACAO T1 " +
                        "JOIN INDENIZACAO T2 ON T1.COD_INDENIZACAO = T2.COD_INDENIZACAO " +
                        "WHERE T2.IND_DESCARREGADO != 1 OR IND_DESCARREGADO IS NULL");

                    if (dtItemColetaIndenizacao == null)
                        dtItemColetaIndenizacao = new DataTable();

                    dtItemColetaIndenizacao.TableName = "ITEM_INDENIZACAO";
                    ds.Tables.Add(dtItemColetaIndenizacao);
                }

                if (CSDataAccess.Instance.TableExists("PEDIDO_INDENIZACAO"))
                {
                    // todos pedido de indenização
                    pDAT_ALTERACAO = new SQLiteParameter("@DAT_ALTERACAO", ultimaDescarga);
                    dtPedidoIndenizao = CSDataAccess.Instance.ExecuteDataTable(
                        "SELECT T1.* " +
                        "  FROM PEDIDO_INDENIZACAO T1 " +
                        "  JOIN PEDIDO T2 ON T1.COD_PEDIDO = T2.COD_PEDIDO " +
                        " WHERE T2.IND_HISTORICO = 0 " +
                        "   AND T2.DAT_ALTERACAO >= ? ", pDAT_ALTERACAO);

                    if (dtPedidoIndenizao == null)
                        dtPedidoIndenizao = new DataTable();

                    dtPedidoIndenizao.TableName = "PEDIDO_INDENIZACAO";
                    ds.Tables.Add(dtPedidoIndenizao);
                }

                if (CSDataAccess.Instance.TableExists("PRODUTO_COLETA_ESTOQUE"))
                {
                    // todos pedido de indenização
                    pDAT_ALTERACAO = new SQLiteParameter("@DAT_ALTERACAO", ultimaDescarga);
                    dtProdutoColetaEstoque = CSDataAccess.Instance.ExecuteDataTable(
                        "SELECT T1.* " +
                        "  FROM PRODUTO_COLETA_ESTOQUE T1 " +
                        " WHERE T1.IND_HISTORICO = 0 " +
                        "   AND T1.DAT_COLETA >= ? " +
                        "   AND T1.QTD_COLETADA >= 0", pDAT_ALTERACAO);

                    if (dtProdutoColetaEstoque == null)
                        dtProdutoColetaEstoque = new DataTable();

                    dtProdutoColetaEstoque.TableName = "PRODUTO_COLETA_ESTOQUE";
                    ds.Tables.Add(dtProdutoColetaEstoque);
                }

                if (CSDataAccess.Instance.TableExists("ENTRADA_CREDITO_PRODUTO"))
                {
                    // todos pedido de indenização
                    pDAT_ALTERACAO = new SQLiteParameter("@DAT_ALTERACAO", ultimaDescarga);
                    dtEntradaCreditoProduto = CSDataAccess.Instance.ExecuteDataTable(
                        "SELECT T1.* " +
                        "  FROM ENTRADA_CREDITO_PRODUTO T1 " +
                        " WHERE T1.DAT_ATIVACAO >= ? ", pDAT_ALTERACAO);

                    if (dtEntradaCreditoProduto == null)
                        dtEntradaCreditoProduto = new DataTable();

                    dtEntradaCreditoProduto.TableName = "ENTRADA_CREDITO_PRODUTO";
                    ds.Tables.Add(dtEntradaCreditoProduto);
                }

                pDAT_ALTERACAO = new SQLiteParameter("@DAT_ALTERACAO", ultimaDescarga);
                dtPedidoExcluido = CSDataAccess.Instance.ExecuteDataTable(
                    "SELECT * " +
                    "  FROM TMP_PEDIDO_EXCLUIDO " +
                    " WHERE DAT_ALTERACAO >= ? ", pDAT_ALTERACAO);

                if (dtPedidoExcluido == null)
                    dtPedidoExcluido = new DataTable();

                dtPedidoExcluido.TableName = "PEDIDO_EXCLUIDO";
                ds.Tables.Add(dtPedidoExcluido);

                StringBuilder sqlHistoricoMotivo = new StringBuilder();
                sqlHistoricoMotivo.Append("SELECT COD_EMPREGADO ");
                sqlHistoricoMotivo.AppendLine("  ,COD_PDV ");
                sqlHistoricoMotivo.AppendLine("  ,COD_TIPO_MOTIVO ");
                sqlHistoricoMotivo.AppendLine("  ,COD_MOTIVO ");
                sqlHistoricoMotivo.AppendLine("  ,DAT_HISTORICO_MOTIVO ");
                sqlHistoricoMotivo.AppendLine("  ,IND_HISTORICO ");
                sqlHistoricoMotivo.AppendLine(CSEmpresa.ColunaExiste("HISTORICO_MOTIVO", "NUM_CPF_EMPREGADO")
                    ? ",NUM_CPF_EMPREGADO" : string.Empty);
                sqlHistoricoMotivo.Append("FROM HISTORICO_MOTIVO ");
                sqlHistoricoMotivo.Append("WHERE DAT_HISTORICO_MOTIVO >= ? AND (IND_HISTORICO <> 1 OR IND_HISTORICO IS NULL) ");
                sqlHistoricoMotivo.Append("GROUP BY COD_EMPREGADO, COD_PDV, COD_TIPO_MOTIVO, COD_MOTIVO, IND_HISTORICO");

                pDAT_HISTORICO_MOTIVO = new SQLiteParameter("@DAT_HISTORICO_MOTIVO", ultimaDescarga);
                dtHistoricoMotivo = CSDataAccess.Instance.ExecuteDataTable(sqlHistoricoMotivo.ToString(), pDAT_HISTORICO_MOTIVO);
                //"SELECT DISTINCT * " +
                //"  FROM HISTORICO_MOTIVO " +
                //" WHERE DAT_HISTORICO_MOTIVO >= ? ", pDAT_HISTORICO_MOTIVO);

                if (dtHistoricoMotivo == null)
                    dtHistoricoMotivo = new DataTable();

                dtHistoricoMotivo.TableName = "HISTORICO_MOTIVO";
                ds.Tables.Add(dtHistoricoMotivo);

                pDAT_ENTRADA = new SQLiteParameter("@pDAT_ENTRADA", ultimaDescarga.Date);
                dtMonitoramento = CSDataAccess.Instance.ExecuteDataTable(
                    "SELECT * " +
                    "  FROM MONITORAMENTO_VENDEDOR_ROTA " +
                    " WHERE DAT_ENTRADA >= ? ", pDAT_ENTRADA);

                if (dtMonitoramento == null)
                    dtMonitoramento = new DataTable();

                dtMonitoramento.TableName = "MONITORAMENTO_VENDEDOR_ROTA";
                ds.Tables.Add(dtMonitoramento);

                if (CSDataAccess.Instance.TableExists("RESPOSTA_PESQUISA_MERCADO"))
                {
                    pDAT_COLETA = new SQLiteParameter("@DAT_COLETA", ultimaDescarga);
                    dtRespostaPesquisaMercado = CSDataAccess.Instance.ExecuteDataTable(
                        "SELECT * " +
                        "  FROM RESPOSTA_PESQUISA_MERCADO " +
                        " WHERE DAT_COLETA >= ? ", pDAT_COLETA);

                    if (dtRespostaPesquisaMercado == null)
                        dtRespostaPesquisaMercado = new DataTable();

                    dtRespostaPesquisaMercado.TableName = "RESPOSTA_PESQUISA_MERCADO";
                    ds.Tables.Add(dtRespostaPesquisaMercado);
                }

                if (CSDataAccess.Instance.TableExists("RESPOSTA_PESQUISA"))
                {
                    pDAT_COLETA = new SQLiteParameter("@DAT_COLETA", ultimaDescarga);
                    dtRespostaPesquisa = CSDataAccess.Instance.ExecuteDataTable(
                        "SELECT * " +
                        "  FROM RESPOSTA_PESQUISA " +
                        " WHERE DAT_COLETA >= ? ", pDAT_COLETA);

                    if (dtRespostaPesquisa == null)
                        dtRespostaPesquisa = new DataTable();

                    dtRespostaPesquisa.TableName = "RESPOSTA_PESQUISA";
                    ds.Tables.Add(dtRespostaPesquisa);
                }


                if (CSDataAccess.Instance.TableExists("MOTIVO_NAORESP_PESQ"))
                {
                    pDAT_COLETA = new SQLiteParameter("@DAT_COLETA", ultimaDescarga);
                    dtMotivoNaoPesquisa = CSDataAccess.Instance.ExecuteDataTable(
                        "SELECT * " +
                        "  FROM MOTIVO_NAORESP_PESQ " +
                        " WHERE DAT_COLETA >= ? ", pDAT_COLETA);

                    if (dtMotivoNaoPesquisa == null)
                        dtMotivoNaoPesquisa = new DataTable();

                    dtMotivoNaoPesquisa.TableName = "MOTIVO_NAORESP_PESQ";
                    ds.Tables.Add(dtMotivoNaoPesquisa);
                }


                if (CSDataAccess.Instance.TableExists("PESQUISA_MERCHAN_PDV"))
                {
                    pDAT_COLETA = new SQLiteParameter("@DATA_COLETA", ultimaDescarga);
                    dtPesquisaMerchanPdv = CSDataAccess.Instance.ExecuteDataTable(
                        "SELECT * " +
                        "  FROM PESQUISA_MERCHAN_PDV " +
                        " WHERE DATA_COLETA >= ? ", pDAT_COLETA);

                    if (dtPesquisaMerchanPdv == null)
                        dtPesquisaMerchanPdv = new DataTable();

                    dtPesquisaMerchanPdv.TableName = "PESQUISA_MERCHAN_PDV";
                    ds.Tables.Add(dtPesquisaMerchanPdv);
                }

                if (CSDataAccess.Instance.TableExists("VERSAO"))
                {
                    dtLog = CSDataAccess.Instance.ExecuteDataTable(string.Format("SELECT '{0}' AS 'DSC_VERSAO_PDA'", versaoAvante));

                    if (dtLog == null)
                        dtLog = new DataTable();

                    dtLog.TableName = "LOG";
                    ds.Tables.Add(dtLog);
                    CSDataAccess.Instance.ClearTable("VERSAO");
                }

                if (CSDataAccess.Instance.TableExists("PDV_EMAIL"))
                {
                    dtPdv_email = CSDataAccess.Instance.ExecuteDataTable(
                        string.Format("SELECT * FROM PDV_EMAIL WHERE IND_ALTERADO = 1 AND DATETIME(DAT_ALTERACAO) >= DATETIME('{0}')", ultimaDescarga.ToString("yyyy-MM-dd HH:mm:ss")));

                    if (dtPdv_email == null)
                        dtPdv_email = new DataTable();

                    dtPdv_email.TableName = "PDV_EMAIL";
                    ds.Tables.Add(dtPdv_email);
                }

                if (CSDataAccess.Instance.TableExists("TELEFONE_PDV"))
                {
                    dtTelefone_pdv = CSDataAccess.Instance.ExecuteDataTable("SELECT * FROM TELEFONE_PDV WHERE IND_ALTERADO = 1");

                    if (dtTelefone_pdv == null)
                        dtTelefone_pdv = new DataTable();

                    dtTelefone_pdv.TableName = "TELEFONE_PDV";
                    ds.Tables.Add(dtTelefone_pdv);
                }

                if (CSDataAccess.Instance.TableExists("MOTIVO_NAO_PESQUISA"))
                {
                    dtMotivoNaoPesquisaMercado = CSDataAccess.Instance.ExecuteDataTable(
                        string.Format("SELECT * FROM MOTIVO_NAO_PESQUISA WHERE DATETIME(DAT_COLETA) >= DATETIME('{0}')", ultimaDescarga.ToString("yyyy-MM-dd HH:mm:ss")));

                    if (dtMotivoNaoPesquisaMercado == null)
                        dtMotivoNaoPesquisaMercado = new DataTable();

                    dtMotivoNaoPesquisaMercado.TableName = "MOTIVO_NAO_PESQUISA";
                    ds.Tables.Add(dtMotivoNaoPesquisaMercado);
                }

                if (CSDataAccess.Instance.TableExists("EMPREGADO_EXPEDIENTE"))
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine("SELECT * FROM EMPREGADO_EXPEDIENTE");

                    dtAlmocoEmpregado = CSDataAccess.Instance.ExecuteDataTable(sql.ToString());

                    if (dtAlmocoEmpregado == null)
                        dtAlmocoEmpregado = new DataTable();

                    dtAlmocoEmpregado.TableName = "EMPREGADO_EXPEDIENTE";
                    ds.Tables.Add(dtAlmocoEmpregado);
                }

                if (CSDataAccess.Instance.TableExists("PDV_VISITA"))
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append("SELECT * FROM PDV_VISITA ");

                    if (!Descarga)
                        sql.AppendFormat("WHERE DATETIME(DAT_ALTERACAO) >= DATETIME('{0}')", ultimaDescarga.ToString("yyyy-MM-dd HH:mm:ss"));

                    dtPdvVisita = CSDataAccess.Instance.ExecuteDataTable(sql.ToString());

                    if (dtPdvVisita == null)
                        dtPdvVisita = new DataTable();

                    dtPdvVisita.TableName = "PDV_VISITA";
                    ds.Tables.Add(dtPdvVisita);
                }

                if (CSDataAccess.Instance.TableExists("PDV_PRODUTO_VALIDADE"))
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append("SELECT * FROM PDV_PRODUTO_VALIDADE ");
                    sql.AppendFormat(" WHERE DATETIME(DAT_COLETA) >= DATETIME('{0}')", ultimaDescarga.ToString("yyyy-MM-dd HH:mm:ss"));

                    dtProdutoValidade = CSDataAccess.Instance.ExecuteDataTable(sql.ToString());

                    dtProdutoValidade.TableName = "PDV_PRODUTO_VALIDADE";
                    ds.Tables.Add(dtProdutoValidade);
                }

                if (CSDataAccess.Instance.TableExists("GRUPO_MARKUP"))
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append("SELECT * FROM GRUPO_MARKUP ");
                    sql.AppendFormat(" WHERE DATETIME(DAT_COLETA) >= DATETIME('{0}')", ultimaDescarga.ToString("yyyy-MM-dd HH:mm:ss"));

                    dtGrupoMarkup = CSDataAccess.Instance.ExecuteDataTable(sql.ToString());

                    dtGrupoMarkup.TableName = "GRUPO_MARKUP";
                    ds.Tables.Add(dtGrupoMarkup);
                }

                //Store the ticks from UTC in the ExtendedProperties collection of the DataSet
                DateTime clientDateTime = DateTime.Now;
                ds.ExtendedProperties["UTCDifference"] = TimeZone.CurrentTimeZone.GetUtcOffset(clientDateTime).Ticks.ToString();

                ds.AcceptChanges();

                return ds;

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Grava xml de log dos dados que serão transmitidos.
        /// </summary>
        /// <param name="ds">DataSet com os dados que serão transmitidos.</param>
        private static void GravaXmlLog(DataSet ds)
        {
            try
            {
                // Recupera o caminho do diretorio da aplicacao
                string configFile = Path.Combine(CSGlobal.GetCurrentDirectory(), "Dados_" + DateTime.Now.ToString("dd.MM.yyyy HH.mm.ss") + ".xml");

                System.Xml.XmlTextWriter xw = new System.Xml.XmlTextWriter(configFile, System.Text.Encoding.UTF8);
                ds.WriteXml(xw, XmlWriteMode.WriteSchema);
                xw.Flush();
                xw.Close();

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Diz se existem dados que precisam ser descarregados antes de se realizar uma nova carga.
        /// </summary>
        /// <returns></returns>
        public static bool ExistemDadosNaoDescarregados(string versaoAvante)
        {
            DataSet ds = MontaDataSetDescarga(false, versaoAvante);

            foreach (DataTable table in ds.Tables)
            {
                if (table.Rows.Count > 0 &&
                    table.TableName != "HISTORICO_MOTIVO" &&
                    table.TableName != "MONITORAMENTO_VENDEDOR_ROTA" &&
                    table.TableName != "TELEFONE_PDV" &&
                    table.TableName != "EMPREGADO_EXPEDIENTE")
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Retorna a data da última descarga de dados realizada.
        /// </summary>
        public static DateTime DataUltimaDescarga
        {
            get
            {
                object objData = null;
                DateTime data;

                //CSDataAccess.Instance.AbreConexao();

                try
                {
                    // Busca a data da ultima descarga
                    //var configEmpregado = CSConfiguracao.GetConfig("vendedor" + CSGlobal.COD_REVENDA);
                    var codigoVendedorRevenda = CSConfiguracao.GetConfig("vendedor" + CSGlobal.COD_REVENDA);
                    if (string.IsNullOrEmpty(codigoVendedorRevenda))
                    {
                        codigoVendedorRevenda = CSConfiguracao.GetConfig("vendedorDefault");
                    }

                    if (string.IsNullOrEmpty(codigoVendedorRevenda))
                    {
                        return DateTime.Now.Date;
                    }
                    var query = "SELECT DATA_ULTIMA_SINCRONIZACAO FROM INFORMACOES_SINCRONIZACAO WHERE COD_EMPREGADO = " + codigoVendedorRevenda;
                    objData = CSDataAccess.Instance.ExecuteScalar(query);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                if (objData == null ||
                    !DateTime.TryParse(objData.ToString(), out data))
                    objData = DateTime.Now;

                return Convert.ToDateTime(objData);
            }
        }
    }
}