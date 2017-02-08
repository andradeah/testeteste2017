using System;
using System.Collections;
using System.Data;
using System.Data.SqlTypes;
using System.Reflection;
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
using AvanteSales.SystemFramework.CSPDV;
using AvanteSales.SystemFramework.BusinessLayer;

namespace AvanteSales
{
    #region [ Enuns ]

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

    /// <summary>
    /// Guarda a coleção de PDVs a serem visitados e suas informações referentes
    /// </summary>
    public class CSPDVs : CollectionBase, IDisposable
    {
        #region [ Variaveis ]

        private static ArrayList m_CodPdvsRota;
        private static CSPDVs m_Items;
        private static CSPDVs.CSPDV m_Current;
        public delegate void AddNewPDV(CSPDV pdv);
        public delegate void BeginGetPDVs(int TotalPDVs);
        public delegate void EndGetPDVs();

        public delegate void BeginGetProdutos(int TotalProdutos);
        public delegate void TickNewProduto();
        public delegate void EndGetProdutos();

        public delegate void InitGrupoProdutos();
        public delegate void InitFamiliaProdutos();

        public delegate void ChangePDV();

        #endregion

        #region [ Eventos ]

        public static event BeginGetPDVs OnBeginGetPDVs;
        public static event AddNewPDV OnAddNewPDV;
        public static event EndGetPDVs OnEndGetPDVs;

        public static event BeginGetProdutos OnBeginProdutos;
        public static event TickNewProduto OnTickNewProduto;
        public static event EndGetProdutos OnEndProdutos;

        public static event InitGrupoProdutos OnInitGrupoProdutos;
        public static event InitFamiliaProdutos OnInitFamiliaProdutos;

        public static event ChangePDV OnChangePDV;

        #endregion

        #region [ Metodos ]

        public static ArrayList CodPdvsRota
        {
            get
            {
                return m_CodPdvsRota;
            }
            set
            {
                if (m_CodPdvsRota == null)
                    m_CodPdvsRota = value;
            }
        }

        public static object RetornaPdvsPositivadosForaRota(SQLiteParameter paramCOD_DIA_VISITA, SQLiteParameter paramDAT_CICLO, SQLiteParameter paramDAT_PEDIDO)
        {
            StringBuilder sqlQuery = new StringBuilder();

            //sqlQuery.Append("SELECT COUNT(*) ");
            //sqlQuery.Append("  FROM PDV ");
            //sqlQuery.Append(" WHERE COD_PDV IN (SELECT DISTINCT T1.COD_PDV ");
            //sqlQuery.Append("                     FROM PDV T1 ");
            //sqlQuery.Append("                     JOIN CADASTRO_DIA_VISITA T2 ON T2.COD_PDV = T1.COD_PDV ");
            //sqlQuery.Append("                     JOIN PEDIDO T3 ON T3.COD_PDV = T1.COD_PDV AND T3.IND_HISTORICO = 0 ");
            //sqlQuery.Append("                     JOIN OPERACAO T6 ON T6.COD_OPERACAO = T3.COD_OPERACAO ");
            //sqlQuery.Append("                      AND T6.COD_OPERACAO_CFO IN (1, 21) ");
            //sqlQuery.Append("                    INNER JOIN PDV_GRUPO_COMERCIALIZACAO T4 ON T1.COD_PDV = T4.COD_PDV ");
            //sqlQuery.Append("                     JOIN DAT_REFERENCIA_CICLO_VISITA T5 ON (T4.DSC_CICLO_VISITA NOT LIKE '%' || T5.COD_CICLO || '%' ");
            //sqlQuery.Append("                      AND ? BETWEEN T5.DAT_INICIO_CICLO AND T5.DAT_FINAL_CICLO ");
            //sqlQuery.Append("                       OR T2.COD_DIA_VISITA != ? ) ");
            //sqlQuery.Append("                      AND T3.DAT_PEDIDO = ?) ");

            sqlQuery.Append("SELECT COUNT(*) ");
            sqlQuery.Append("  FROM PDV ");
            sqlQuery.Append("JOIN (SELECT DISTINCT T1.COD_PDV ");
            sqlQuery.Append("                     FROM PDV T1 ");
            sqlQuery.Append("                     JOIN CADASTRO_DIA_VISITA T2 ON T2.COD_PDV = T1.COD_PDV ");
            sqlQuery.Append("                     JOIN PEDIDO T3 ON T3.COD_PDV = T1.COD_PDV AND T3.IND_HISTORICO = 0 ");
            sqlQuery.Append("                     JOIN OPERACAO T6 ON T6.COD_OPERACAO = T3.COD_OPERACAO ");
            sqlQuery.Append("                      AND T6.COD_OPERACAO_CFO IN (1, 21) ");
            sqlQuery.Append("                    INNER JOIN PDV_GRUPO_COMERCIALIZACAO T4 ON T1.COD_PDV = T4.COD_PDV ");
            sqlQuery.Append("                     JOIN DAT_REFERENCIA_CICLO_VISITA T5 ON (T4.DSC_CICLO_VISITA NOT LIKE '%' || T5.COD_CICLO || '%' ");
            sqlQuery.Append("                      AND DATE(?) BETWEEN DATE(T5.DAT_INICIO_CICLO) AND DATE(T5.DAT_FINAL_CICLO) ");
            sqlQuery.Append("                       OR T2.COD_DIA_VISITA != ? ) ");
            sqlQuery.Append("                      AND DATE(T3.DAT_PEDIDO) = DATE(?)) T1 ");
            sqlQuery.Append(" ON T1.COD_PDV = PDV.COD_PDV");

            var result = CSDataAccess.Instance.ExecuteScalar(sqlQuery.ToString(), paramDAT_CICLO, paramCOD_DIA_VISITA, paramDAT_PEDIDO);

            return result;
        }

        public CSPDVs(int COD_DIA_VISITA)
        {
            StringBuilder sqlQuery = null;
            DateTime dataCicloVisita;
            int cod_dia_visita_pdv = -1;

            ArrayList pdv_carregado = new ArrayList();

            this.InnerList.Clear();
            this.InnerList.TrimToSize();

            bool InserirDadosVisita = CSEmpresa.Current.IND_EMPRESA_FERIADO == false && CSEmpregados.Current.VISITAS_EMPREGADO.Count == 0;

            try
            {
                sqlQuery = new StringBuilder();
                dataCicloVisita = DateTime.Now;

                sqlQuery.Length = 0;
                sqlQuery.Append("SELECT COUNT(*) ");
                sqlQuery.Append("  FROM PDV ");
                sqlQuery.Append(" WHERE COD_PDV IN (SELECT T1.COD_PDV ");
                sqlQuery.Append("                     FROM PDV T1 ");
                sqlQuery.Append("                    INNER JOIN PDV_GRUPO_COMERCIALIZACAO T2 ON T1.COD_PDV = T2.COD_PDV ");
                sqlQuery.Append("                    INNER JOIN CATEGORIA T3 ON T1.COD_CATEGORIA = T3.COD_CATEGORIA ");
                sqlQuery.Append("                    INNER JOIN CONDICAO_PAGAMENTO T4 ON T2.COD_CONDICAO_PAGAMENTO = T4.COD_CONDICAO_PAGAMENTO ");
                sqlQuery.Append("                    INNER JOIN CADASTRO_DIA_VISITA T5 ON T2.COD_PDV = T5.COD_PDV ");
                sqlQuery.Append("                      AND T2.COD_GRUPO_COMERCIALIZACAO = T5.COD_GRUPO_COMERCIALIZACAO ");

                // Se o codigo do dia de visita diferente de menos um coloca o filtro de dia, senao busca todos os PDVs
                if (COD_DIA_VISITA != -1)
                {
                    sqlQuery.Append("   AND T5.COD_DIA_VISITA = ? ");
                    sqlQuery.Append(
                        " INNER JOIN DAT_REFERENCIA_CICLO_VISITA T6 ON T2.DSC_CICLO_VISITA LIKE '%' || T6.COD_CICLO || '%' ");
                    sqlQuery.Append("   AND DATE(?) BETWEEN DATE(T6.DAT_INICIO_CICLO) AND DATE(T6.DAT_FINAL_CICLO) ");

                    int dayOfWeek1 = (int)DateTime.Now.DayOfWeek;
                    int dayOfWeek2 = (int)(COD_DIA_VISITA == 7 ? 0 : COD_DIA_VISITA);
                    dataCicloVisita = DateTime.Now.AddDays(dayOfWeek2 - dayOfWeek1);
                }

                sqlQuery.Append(" ) ");

                // Nao devera passar mais o codigo do empregado pois se 
                // selecionar outro vendedor para fazer a venda os clientes
                // do vendedor que foi feito a carga nao irao aparecer na lista
                // SQLiteParameter pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                // Retirado pq tem cliente que o grupo de vendedor e diferente do grupo de comercialização que ele esta relacionado ao cliente
                //SQLiteParameter pCOD_GRUPO_COMERCIALIZACAO_VEDEDOR = new SQLiteParameter("@COD_GRUPO_COMERCIALIZACAO_VEDEDOR", CSEmpregados.Current.COD_GRUPO_COMERCIALIZACAO);
                SQLiteParameter pCOD_DIA_VISITA = new SQLiteParameter("@COD_DIA_VISITA", COD_DIA_VISITA);
                SQLiteParameter pDAT_CICLO = new SQLiteParameter("@DAT_CICLO", dataCicloVisita.ToString("yyyy-MM-dd"));

                // Busca o numero de PDVs
                object objResult = CSDataAccess.Instance.ExecuteScalar(sqlQuery.ToString(), pCOD_DIA_VISITA, pDAT_CICLO);
                int numTicks = (objResult == null ? 0 : int.Parse(objResult.ToString()));

                if (numTicks > 0)
                {
                    // Soma mais um para a busca dos grupos
                    numTicks++;
                    // Soma mais um para a busca das familias
                    numTicks++;
                    // Dispara o evento de inicio do preenchimento do coleção
                    if (OnBeginGetPDVs != null)
                        OnBeginGetPDVs(numTicks);
                }

                pCOD_DIA_VISITA = new SQLiteParameter("@COD_DIA_VISITA", COD_DIA_VISITA);
                pDAT_CICLO = new SQLiteParameter("@DAT_CICLO", dataCicloVisita.ToString("yyyy-MM-dd"));

                CSEmpresa.Current.UtilizaDescricaoDenver = CSEmpresa.ColunaExiste("PDV", "DSC_DENVER");

                CSTiposDistribPolicitcaPrecos.Current = CSTiposDistribPolicitcaPrecos.GetTipoDistribPolicitcaPreco(CSTiposDistribPolicitcaPrecos.GetPoliticaPreco());

                // Se for Broker...
                if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2)
                {
                    sqlQuery.Length = 0;
                    sqlQuery.Append("SELECT DISTINCT P.COD_PDV, P.COD_CATEGORIA, C.DSC_CATEGORIA, PG.COD_SEGMENTACAO, PG.COD_EMPREGADO, PG.COD_CONDICAO_PAGAMENTO, CP.DSC_CONDICAO_PAGAMENTO ");
                    sqlQuery.Append("      ,P.DSC_RAZAO_SOCIAL, P.NOM_FANTASIA, P.NUM_INSCRICAO_ESTADUAL, P.NUM_CGC, P.IND_EXCLUSIVO, P.IND_BLOQUEADO, P.VLR_LIMITE_CREDITO ");
                    sqlQuery.Append("      ,P.VLR_SALDO_DEVEDOR, PG.DSC_CICLO_VISITA, P.DSC_CLIPPING_INFORMATIVO, P.IND_INADIMPLENTE, P.DSC_PONTO_REFERENCIA ");
                    sqlQuery.Append("      ,CDV.NUM_ORDEM_VISITA_ROTA, PG.COD_SEGREGACAO, 0 AS CODCNDPGT ");
                    sqlQuery.Append("      ,CASE WHEN IBC.CDCLI IS NULL THEN 0 ELSE IBC.CDCLI END AS FLIVND ");
                    sqlQuery.Append("      ,SGT.DSC_SEGMENTACAO,GPOCET.COD_GRUPO,GPOCET.DESCRICAO_GRUPO, CSFGPOCET.COD_CLASSIFICACAO,CSFGPOCET.DESCRICAO_CLASSIFICACAO ");
                    sqlQuery.Append("      ,UNINGC.COD_UNIDADE_NEGOCIO,UNINGC.DESCRICAO_UNIDADE_NEGOCIO,PG.COD_TABPRECO_PADRAO,CSFGPOCET.COD_GER_BROKER,BRK_ECLIENTE.CDFORMPAGI, BRK_ECLIENTE.CDGER0,'' AS CD_CLIENTE");
                    sqlQuery.Append("      ,P.IND_COBERTOMES ,P.NUM_POSITIVACOES ,C.IND_PERMITIR_VENDA_FORA_ROTA ,CDV.COD_DIA_VISITA, P.COD_DENVER ");

                    if (CSEmpresa.ColunaExiste("PDV", "COD_CFO"))
                        sqlQuery.Append(", P.COD_CFO, P.SEQ_CFO");
                    else
                        sqlQuery.Append(",0 AS CODCFO,0 AS SEQCFO");

                    if (CSEmpresa.Current.UtilizaDescricaoDenver)
                        sqlQuery.Append("      ,P.DSC_DENVER ");
                    else
                        sqlQuery.Append("      ,'' AS DSCDENVER");

                    if (CSEmpresa.ColunaExiste("PESQUISA_MERCADO_CATEGORIA", "COD_PESQUISA_MERC"))
                        sqlQuery.Append(" , PC.COD_CATEGORIA");
                    else
                        sqlQuery.Append(" ,0 AS CATEGORIA");

                    if (CSEmpresa.ColunaExiste("PDV", "IND_ESPECIAL_INDENIZACAO_BROKER"))
                        sqlQuery.Append(" ,P.IND_ESPECIAL_INDENIZACAO_BROKER");
                    else
                        sqlQuery.Append(" ,0 AS PDVESPECIAL");

                    if (CSEmpresa.ColunaExiste("PDV", "PEDIDOS_PRAZO_DIA"))
                        sqlQuery.Append(" ,P.PEDIDOS_PRAZO_DIA");
                    else
                        sqlQuery.Append(" ,0 AS PEDIDOS_PRAZO_DIA");

                    sqlQuery.Append(",DSC_NOME_FOTO");

                    if (CSEmpresa.ColunaExiste("PDV", "NUM_LATITUDE_LOCALIZACAO"))
                        sqlQuery.Append(" ,P.NUM_LATITUDE_LOCALIZACAO");
                    else
                        sqlQuery.Append(" ,'' AS NUM_LATITUDE_LOCALIZACAO");

                    if (CSEmpresa.ColunaExiste("PDV", "NUM_LONGITUDE_LOCALIZACAO"))
                        sqlQuery.Append(" ,P.NUM_LONGITUDE_LOCALIZACAO");
                    else
                        sqlQuery.Append(" ,'' AS NUM_LONGITUDE_LOCALIZACAO");

                    if (CSEmpresa.ColunaExiste("PDV", "IND_PDV_LOCALIZACAO_VERIFICADA"))
                        sqlQuery.Append(" ,P.IND_PDV_LOCALIZACAO_VERIFICADA");
                    else
                        sqlQuery.Append(" ,0 AS IND_PDV_LOCALIZACAO_VERIFICADA");

                    if (CSEmpresa.ColunaExiste("CATEGORIA", "IND_BLOQUEAR_INDENIZACAO"))
                        sqlQuery.Append(" ,C.IND_BLOQUEAR_INDENIZACAO ");
                    else
                        sqlQuery.Append(" ,0 AS IND_BLOQUEAR_INDENIZACAO ");

                    sqlQuery.Append(", IND_FOTO_DUVIDOSA");
                    sqlQuery.Append(", BOL_FOTO_VALIDADA");
                    sqlQuery.Append(", NUM_LATITUDE_FOTO");
                    sqlQuery.Append(", NUM_LONGITUDE_FOTO");

                    if (CSEmpresa.ColunaExiste("PDV_OBJETIVO", "COD_PDV"))
                        sqlQuery.Append(", OBJ.META");
                    else
                        sqlQuery.Append(", 0 AS META ");

                    sqlQuery.Append(", PDD.TOTAL_VENDAS");

                    if (CSEmpresa.ColunaExiste("PDV", "PCT_MAXIMO_INDENIZACAO"))
                        sqlQuery.Append(", PCT_MAXIMO_INDENIZACAO ");
                    else
                        sqlQuery.Append(", 0 AS PCT_MAXIMO_INDENIZACAO ");

                    sqlQuery.Append("  FROM PDV P ");
                    sqlQuery.Append("  JOIN PDV_GRUPO_COMERCIALIZACAO PG ");
                    sqlQuery.Append("       ON P.COD_PDV = PG.COD_PDV ");
                    //sqlQuery.Append("   AND PG.COD_GRUPO_COMERCIALIZACAO = ? ");
                    sqlQuery.Append("  JOIN CADASTRO_DIA_VISITA CDV ");
                    sqlQuery.Append("       ON PG.COD_PDV = CDV.COD_PDV ");
                    sqlQuery.Append("   AND PG.COD_GRUPO_COMERCIALIZACAO = CDV.COD_GRUPO_COMERCIALIZACAO ");
                    sqlQuery.Append("  JOIN CONDICAO_PAGAMENTO CP ");
                    sqlQuery.Append("       ON PG.COD_CONDICAO_PAGAMENTO = CP.COD_CONDICAO_PAGAMENTO ");
                    sqlQuery.Append("  JOIN SEGMENTACAO SGT ");
                    sqlQuery.Append("       ON SGT.COD_SEGMENTACAO = PG.COD_SEGMENTACAO ");
                    sqlQuery.Append("  JOIN CLASSIFICACAO_GRUPO_CLIENTE CSFGPOCET ");
                    sqlQuery.Append("       ON P.COD_GRUPO = CSFGPOCET.COD_GRUPO ");
                    sqlQuery.Append("   AND P.COD_CLASSIFICACAO = CSFGPOCET.COD_CLASSIFICACAO ");
                    sqlQuery.Append("  JOIN UNIDADE_NEGOCIO UNINGC ");
                    sqlQuery.Append("       ON UNINGC.COD_UNIDADE_NEGOCIO = P.COD_UNIDADE_NEGOCIO ");
                    sqlQuery.Append("  JOIN CATEGORIA C ");
                    sqlQuery.Append("       ON P.COD_CATEGORIA = C.COD_CATEGORIA ");
                    sqlQuery.Append("  JOIN GRUPO_CLIENTE GPOCET ");
                    sqlQuery.Append("       ON GPOCET.COD_GRUPO = P.COD_GRUPO ");
                    sqlQuery.Append("  LEFT JOIN BRK_ECLIENTE ");
                    sqlQuery.Append("       ON P.COD_PDV = BRK_ECLIENTE.CDCLI ");
                    sqlQuery.Append("  LEFT JOIN BRK_ECLIENTBAS IBC ");
                    sqlQuery.Append("       ON BRK_ECLIENTE.CDCLI = IBC.CDCLI ");

                }
                else if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 1)
                {
                    sqlQuery.Length = 0;
                    sqlQuery.Append("SELECT DISTINCT P.COD_PDV, P.COD_CATEGORIA, C.DSC_CATEGORIA, PG.COD_SEGMENTACAO, PG.COD_EMPREGADO, PG.COD_CONDICAO_PAGAMENTO, CP.DSC_CONDICAO_PAGAMENTO ");
                    sqlQuery.Append("      ,P.DSC_RAZAO_SOCIAL, P.NOM_FANTASIA, P.NUM_INSCRICAO_ESTADUAL, P.NUM_CGC, P.IND_EXCLUSIVO, P.IND_BLOQUEADO, P.VLR_LIMITE_CREDITO ");
                    sqlQuery.Append("      ,P.VLR_SALDO_DEVEDOR, PG.DSC_CICLO_VISITA, P.DSC_CLIPPING_INFORMATIVO, P.IND_INADIMPLENTE, P.DSC_PONTO_REFERENCIA ");
                    sqlQuery.Append("      ,CDV.NUM_ORDEM_VISITA_ROTA, PG.COD_SEGREGACAO, 0 AS CODCNDPGT, 0 AS FLIVND ");
                    sqlQuery.Append("      ,SGT.DSC_SEGMENTACAO,GPOCET.COD_GRUPO,GPOCET.DESCRICAO_GRUPO ");
                    sqlQuery.Append("      ,CSFGPOCET.COD_CLASSIFICACAO,CSFGPOCET.DESCRICAO_CLASSIFICACAO ");
                    sqlQuery.Append("      ,UNINGC.COD_UNIDADE_NEGOCIO,UNINGC.DESCRICAO_UNIDADE_NEGOCIO,PG.COD_TABPRECO_PADRAO,CSFGPOCET.COD_GER_BROKER,'' AS CDFORMPAGI,'' AS CDGER0, '' AS CD_CLIENTE");
                    sqlQuery.Append("      ,P.IND_COBERTOMES ,P.NUM_POSITIVACOES ,C.IND_PERMITIR_VENDA_FORA_ROTA ,CDV.COD_DIA_VISITA, P.COD_DENVER ");

                    if (CSEmpresa.ColunaExiste("PDV", "COD_CFO"))
                        sqlQuery.Append(", P.COD_CFO, P.SEQ_CFO");
                    else
                        sqlQuery.Append(", 0 AS CODCFO,0 AS SEQCFO");

                    if (CSEmpresa.Current.UtilizaDescricaoDenver)
                        sqlQuery.Append("      , P.DSC_DENVER");
                    else
                        sqlQuery.Append("      ,'' AS DSCDENVER");

                    if (CSEmpresa.ColunaExiste("PESQUISA_MERCADO_CATEGORIA", "COD_PESQUISA_MERC"))
                        sqlQuery.Append(" , PC.COD_CATEGORIA");
                    else
                        sqlQuery.Append(" ,0 AS CODCATEGORIA");

                    if (CSEmpresa.ColunaExiste("PDV", "IND_ESPECIAL_INDENIZACAO_BROKER"))
                        sqlQuery.Append(" ,P.IND_ESPECIAL_INDENIZACAO_BROKER");
                    else
                        sqlQuery.Append(" ,0 AS PDVESPECIAL");

                    if (CSEmpresa.ColunaExiste("PDV", "PEDIDOS_PRAZO_DIA"))
                        sqlQuery.Append(" ,P.PEDIDOS_PRAZO_DIA");
                    else
                        sqlQuery.Append(" ,0 AS PEDIDOS_PRAZO_DIA");

                    sqlQuery.Append(",DSC_NOME_FOTO");

                    if (CSEmpresa.ColunaExiste("PDV", "NUM_LATITUDE_LOCALIZACAO"))
                        sqlQuery.Append(" ,P.NUM_LATITUDE_LOCALIZACAO");
                    else
                        sqlQuery.Append(" ,'' AS NUM_LATITUDE_LOCALIZACAO");

                    if (CSEmpresa.ColunaExiste("PDV", "NUM_LONGITUDE_LOCALIZACAO"))
                        sqlQuery.Append(" ,P.NUM_LONGITUDE_LOCALIZACAO");
                    else
                        sqlQuery.Append(" ,'' AS NUM_LONGITUDE_LOCALIZACAO");

                    if (CSEmpresa.ColunaExiste("PDV", "IND_PDV_LOCALIZACAO_VERIFICADA"))
                        sqlQuery.Append(" ,P.IND_PDV_LOCALIZACAO_VERIFICADA");
                    else
                        sqlQuery.Append(" ,0 AS IND_PDV_LOCALIZACAO_VERIFICADA");

                    if (CSEmpresa.ColunaExiste("CATEGORIA", "IND_BLOQUEAR_INDENIZACAO"))
                        sqlQuery.Append(" ,C.IND_BLOQUEAR_INDENIZACAO ");
                    else
                        sqlQuery.Append(" ,0 AS IND_BLOQUEAR_INDENIZACAO ");

                    sqlQuery.Append(", IND_FOTO_DUVIDOSA");
                    sqlQuery.Append(", BOL_FOTO_VALIDADA");
                    sqlQuery.Append(", NUM_LATITUDE_FOTO");
                    sqlQuery.Append(", NUM_LONGITUDE_FOTO");

                    if (CSEmpresa.ColunaExiste("PDV_OBJETIVO", "COD_PDV"))
                        sqlQuery.Append(", OBJ.META");
                    else
                        sqlQuery.Append(", 0 AS META ");

                    sqlQuery.Append(", PDD.TOTAL_VENDAS");

                    if (CSEmpresa.ColunaExiste("PDV", "PCT_MAXIMO_INDENIZACAO"))
                        sqlQuery.Append(", PCT_MAXIMO_INDENIZACAO ");
                    else
                        sqlQuery.Append(", 0 AS PCT_MAXIMO_INDENIZACAO ");

                    sqlQuery.Append("  FROM PDV P  ");
                    sqlQuery.Append("  JOIN PDV_GRUPO_COMERCIALIZACAO PG  ");
                    sqlQuery.Append("    ON P.COD_PDV = PG.COD_PDV ");
                    sqlQuery.Append("  JOIN CADASTRO_DIA_VISITA CDV  ");
                    sqlQuery.Append("    ON P.COD_PDV = CDV.COD_PDV ");
                    sqlQuery.Append("   AND PG.COD_GRUPO_COMERCIALIZACAO = CDV.COD_GRUPO_COMERCIALIZACAO ");
                    sqlQuery.Append("  JOIN CONDICAO_PAGAMENTO CP  ");
                    sqlQuery.Append("    ON PG.COD_CONDICAO_PAGAMENTO = CP.COD_CONDICAO_PAGAMENTO ");
                    sqlQuery.Append("  JOIN SEGMENTACAO SGT ");
                    sqlQuery.Append("    ON SGT.COD_SEGMENTACAO = PG.COD_SEGMENTACAO ");
                    sqlQuery.Append("  JOIN CLASSIFICACAO_GRUPO_CLIENTE CSFGPOCET ");
                    sqlQuery.Append("    ON P.COD_GRUPO = CSFGPOCET.COD_GRUPO ");
                    sqlQuery.Append("   AND P.COD_CLASSIFICACAO = CSFGPOCET.COD_CLASSIFICACAO ");
                    sqlQuery.Append("  JOIN UNIDADE_NEGOCIO UNINGC ");
                    sqlQuery.Append("    ON UNINGC.COD_UNIDADE_NEGOCIO = P.COD_UNIDADE_NEGOCIO ");
                    sqlQuery.Append("  JOIN CATEGORIA C ");
                    sqlQuery.Append("    ON P.COD_CATEGORIA=C.COD_CATEGORIA ");
                    sqlQuery.Append("  JOIN GRUPO_CLIENTE GPOCET ");
                    sqlQuery.Append("    ON GPOCET.COD_GRUPO = P.COD_GRUPO ");
                }
                else
                {
                    sqlQuery.Length = 0;
                    sqlQuery.Append("SELECT DISTINCT P.COD_PDV, P.COD_CATEGORIA, C.DSC_CATEGORIA, PG.COD_SEGMENTACAO, PG.COD_EMPREGADO, PG.COD_CONDICAO_PAGAMENTO, CP.DSC_CONDICAO_PAGAMENTO ");
                    sqlQuery.Append("      ,P.DSC_RAZAO_SOCIAL, P.NOM_FANTASIA, P.NUM_INSCRICAO_ESTADUAL, P.NUM_CGC, P.IND_EXCLUSIVO, P.IND_BLOQUEADO, P.VLR_LIMITE_CREDITO ");
                    sqlQuery.Append("      ,P.VLR_SALDO_DEVEDOR, PG.DSC_CICLO_VISITA, P.DSC_CLIPPING_INFORMATIVO, P.IND_INADIMPLENTE, P.DSC_PONTO_REFERENCIA ");
                    sqlQuery.Append("      ,CDV.NUM_ORDEM_VISITA_ROTA, PG.COD_SEGREGACAO, 0 AS CODCNDPGT, 0 AS FLIVND ");
                    sqlQuery.Append("      ,SGT.DSC_SEGMENTACAO,GPOCET.COD_GRUPO,GPOCET.DESCRICAO_GRUPO ");
                    sqlQuery.Append("      ,CSFGPOCET.COD_CLASSIFICACAO,CSFGPOCET.DESCRICAO_CLASSIFICACAO ");
                    sqlQuery.Append("      ,UNINGC.COD_UNIDADE_NEGOCIO,UNINGC.DESCRICAO_UNIDADE_NEGOCIO,PG.COD_TABPRECO_PADRAO,CSFGPOCET.COD_GER_BROKER,'' AS CDFORMPAGI");

                    if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA.ToString().Contains("2"))
                        sqlQuery.Append(",BRK_ECLIENTE.CDGER0 AS CDGER0 ");
                    else
                        sqlQuery.Append(",'' AS CDGER0");

                    if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA.ToString().Contains("3"))
                        sqlQuery.Append(",BNG.CD_CLIENTE AS CD_CLIENTE");
                    else
                        sqlQuery.Append(",'' AS CD_CLIENTE");

                    sqlQuery.Append("      ,P.IND_COBERTOMES ,P.NUM_POSITIVACOES ,C.IND_PERMITIR_VENDA_FORA_ROTA ,CDV.COD_DIA_VISITA, P.COD_DENVER ");

                    if (CSEmpresa.ColunaExiste("PDV", "COD_CFO"))
                        sqlQuery.Append(", P.COD_CFO, P.SEQ_CFO");
                    else
                        sqlQuery.Append(", 0 AS CODCFO,0 AS SEQCFO");

                    if (CSEmpresa.Current.UtilizaDescricaoDenver)
                        sqlQuery.Append("      , P.DSC_DENVER");
                    else
                        sqlQuery.Append("      ,'' AS DSCDENVER");

                    if (CSEmpresa.ColunaExiste("PESQUISA_MERCADO_CATEGORIA", "COD_PESQUISA_MERC"))
                        sqlQuery.Append(" , PC.COD_CATEGORIA");
                    else
                        sqlQuery.Append(" ,0 AS CODCATEGORIA");

                    if (CSEmpresa.ColunaExiste("PDV", "IND_ESPECIAL_INDENIZACAO_BROKER"))
                        sqlQuery.Append(" ,P.IND_ESPECIAL_INDENIZACAO_BROKER");
                    else
                        sqlQuery.Append(" ,0 AS PDVESPECIAL");

                    if (CSEmpresa.ColunaExiste("PDV", "PEDIDOS_PRAZO_DIA"))
                        sqlQuery.Append(" ,P.PEDIDOS_PRAZO_DIA");
                    else
                        sqlQuery.Append(" ,0 AS PEDIDOS_PRAZO_DIA");

                    sqlQuery.Append(",DSC_NOME_FOTO");

                    if (CSEmpresa.ColunaExiste("PDV", "NUM_LATITUDE_LOCALIZACAO"))
                        sqlQuery.Append(" ,P.NUM_LATITUDE_LOCALIZACAO");
                    else
                        sqlQuery.Append(" ,'' AS NUM_LATITUDE_LOCALIZACAO");

                    if (CSEmpresa.ColunaExiste("PDV", "NUM_LONGITUDE_LOCALIZACAO"))
                        sqlQuery.Append(" ,P.NUM_LONGITUDE_LOCALIZACAO");
                    else
                        sqlQuery.Append(" ,'' AS NUM_LONGITUDE_LOCALIZACAO");

                    if (CSEmpresa.ColunaExiste("PDV", "IND_PDV_LOCALIZACAO_VERIFICADA"))
                        sqlQuery.Append(" ,P.IND_PDV_LOCALIZACAO_VERIFICADA");
                    else
                        sqlQuery.Append(" ,0 AS IND_PDV_LOCALIZACAO_VERIFICADA");

                    if (CSEmpresa.ColunaExiste("CATEGORIA", "IND_BLOQUEAR_INDENIZACAO"))
                        sqlQuery.Append(" ,C.IND_BLOQUEAR_INDENIZACAO ");
                    else
                        sqlQuery.Append(" ,0 AS IND_BLOQUEAR_INDENIZACAO ");

                    sqlQuery.Append(", IND_FOTO_DUVIDOSA");
                    sqlQuery.Append(", BOL_FOTO_VALIDADA");
                    sqlQuery.Append(", NUM_LATITUDE_FOTO");
                    sqlQuery.Append(", NUM_LONGITUDE_FOTO");

                    if (CSEmpresa.ColunaExiste("PDV_OBJETIVO", "COD_PDV"))
                        sqlQuery.Append(", OBJ.META");
                    else
                        sqlQuery.Append(", 0 AS META ");

                    sqlQuery.Append(", PDD.TOTAL_VENDAS");

                    if (CSEmpresa.ColunaExiste("PDV", "PCT_MAXIMO_INDENIZACAO"))
                        sqlQuery.Append(", PCT_MAXIMO_INDENIZACAO ");
                    else
                        sqlQuery.Append(", 0 AS PCT_MAXIMO_INDENIZACAO ");

                    sqlQuery.Append("  FROM PDV P  ");
                    sqlQuery.Append("  JOIN PDV_GRUPO_COMERCIALIZACAO PG  ");
                    sqlQuery.Append("    ON P.COD_PDV = PG.COD_PDV ");
                    sqlQuery.Append("  JOIN CADASTRO_DIA_VISITA CDV  ");
                    sqlQuery.Append("    ON P.COD_PDV = CDV.COD_PDV ");
                    sqlQuery.Append("   AND PG.COD_GRUPO_COMERCIALIZACAO = CDV.COD_GRUPO_COMERCIALIZACAO ");
                    sqlQuery.Append("  JOIN CONDICAO_PAGAMENTO CP  ");
                    sqlQuery.Append("    ON PG.COD_CONDICAO_PAGAMENTO = CP.COD_CONDICAO_PAGAMENTO ");
                    sqlQuery.Append("  JOIN SEGMENTACAO SGT ");
                    sqlQuery.Append("    ON SGT.COD_SEGMENTACAO = PG.COD_SEGMENTACAO ");
                    sqlQuery.Append("  JOIN CLASSIFICACAO_GRUPO_CLIENTE CSFGPOCET ");
                    sqlQuery.Append("    ON P.COD_GRUPO = CSFGPOCET.COD_GRUPO ");
                    sqlQuery.Append("   AND P.COD_CLASSIFICACAO = CSFGPOCET.COD_CLASSIFICACAO ");
                    sqlQuery.Append("  JOIN UNIDADE_NEGOCIO UNINGC ");
                    sqlQuery.Append("    ON UNINGC.COD_UNIDADE_NEGOCIO = P.COD_UNIDADE_NEGOCIO ");
                    sqlQuery.Append("  JOIN CATEGORIA C ");
                    sqlQuery.Append("    ON P.COD_CATEGORIA=C.COD_CATEGORIA ");
                    sqlQuery.Append("  JOIN GRUPO_CLIENTE GPOCET ");
                    sqlQuery.Append("    ON GPOCET.COD_GRUPO = P.COD_GRUPO ");

                    if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA.ToString().Contains("3"))
                    {
                        sqlQuery.Append("  LEFT JOIN BNG_LOJA_CLIENTE_AGV_SAP BNG");
                        sqlQuery.Append("       ON BNG.[CD_BASE_CLIENTE] = substr(P.NUM_CGC,1,8)");
                        sqlQuery.Append("       AND BNG.CD_LOJA_CLIENTE =  substr(P.NUM_CGC,9,4)");
                    }

                    if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA.ToString().Contains("2"))
                    {
                        sqlQuery.Append("  LEFT JOIN BRK_ECLIENTE ");
                        sqlQuery.Append("       ON P.COD_PDV = BRK_ECLIENTE.CDCLI ");
                    }
                }

                if (CSEmpresa.ColunaExiste("PESQUISA_MERCADO_CATEGORIA", "COD_PESQUISA_MERC"))
                {
                    sqlQuery.AppendLine(" LEFT JOIN PESQUISA_MERCADO_CATEGORIA PC ON P.COD_CATEGORIA = PC.COD_CATEGORIA ");
                    sqlQuery.AppendLine(" LEFT JOIN PESQUISA_MERCADO PM ON PM.COD_PESQUISA_MERC = PC.COD_PESQUISA_MERC ");
                }

                if (OrdenarPorPdvVisita(InserirDadosVisita, COD_DIA_VISITA))
                {
                    sqlQuery.AppendLine(" JOIN PDV_VISITA ON P.COD_PDV = PDV_VISITA.COD_PDV ");
                    sqlQuery.AppendFormat("AND DATE(DAT_VISITA) = DATE('{0}') ", DateTime.Now.ToString("yyyy-MM-dd"));
                }

                if (CSEmpresa.ColunaExiste("PDV_OBJETIVO", "COD_PDV"))
                {
                    sqlQuery.Append(" LEFT JOIN (");
                    sqlQuery.Append("SELECT COD_PDV, SUM(VLR_OBJETIVO) AS 'META' FROM PDV_OBJETIVO GROUP BY COD_PDV ");
                    sqlQuery.Append(" ) OBJ ");
                    sqlQuery.Append(" ON P.COD_PDV = OBJ.COD_PDV ");
                }

                sqlQuery.Append(" LEFT JOIN( ");
                sqlQuery.AppendFormat("SELECT COD_PDV, SUM(VLR_TOTAL_PEDIDO)AS 'TOTAL_VENDAS' FROM PEDIDO WHERE DATE(DAT_PEDIDO) = DATE('{0}') GROUP BY COD_PDV "
                                        , DateTime.Now.ToString("yyy-MM-dd"));
                sqlQuery.Append(" ) PDD ");
                sqlQuery.Append(" ON P.COD_PDV = PDD.COD_PDV ");

                if (COD_DIA_VISITA != -1)
                {
                    sqlQuery.Append(" INNER JOIN DAT_REFERENCIA_CICLO_VISITA CIC ON PG.DSC_CICLO_VISITA LIKE '%' || CIC.COD_CICLO || '%' ");
                    sqlQuery.Append("   AND DATE(?) BETWEEN DATE(CIC.DAT_INICIO_CICLO) AND DATE(CIC.DAT_FINAL_CICLO) ");
                    sqlQuery.Append(" WHERE CDV.COD_DIA_VISITA = ?  ");
                }

                if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2)
                {
                    if (OrdenarPorPdvVisita(InserirDadosVisita, COD_DIA_VISITA))
                    {
                        sqlQuery.Append(" ORDER BY PDV_VISITA.NUM_ORDEM_VISITA, P.COD_PDV, BRK_ECLIENTE.CDFORMPAGI DESC ");
                    }
                    else if (COD_DIA_VISITA != -1)
                        sqlQuery.Append(" ORDER BY CDV.NUM_ORDEM_VISITA_ROTA, P.COD_PDV, BRK_ECLIENTE.CDFORMPAGI DESC ");
                    else
                        sqlQuery.Append(" ORDER BY P.COD_PDV, CDV.NUM_ORDEM_VISITA_ROTA, BRK_ECLIENTE.CDFORMPAGI DESC ");
                }
                else
                {
                    if (OrdenarPorPdvVisita(InserirDadosVisita, COD_DIA_VISITA))
                    {
                        sqlQuery.Append(" ORDER BY PDV_VISITA.NUM_ORDEM_VISITA, P.DSC_RAZAO_SOCIAL ");
                    }
                    else
                        sqlQuery.Append(" ORDER BY CDV.NUM_ORDEM_VISITA_ROTA, P.DSC_RAZAO_SOCIAL ");
                }

                using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString(), pDAT_CICLO, pCOD_DIA_VISITA))
                {
                    // Busca todos os PDVs
                    while (reader.Read())
                    {
                        if (pdv_carregado.IndexOf(Convert.ToInt32(reader.GetValue(0))) == -1)
                        {
                            CSPDV pdv = new CSPDV();

                            // Preenche a instancia da classe pdv
                            pdv.COD_PDV = reader.GetInt32(0);
                            pdv.COD_CATEGORIA = reader.GetValue(1) == System.DBNull.Value ? -1 : Convert.ToInt32(reader.GetValue(1));
                            pdv.DSC_CATEGORIA = reader.GetValue(2) == System.DBNull.Value ? "" : reader.GetString(2);
                            pdv.COD_SEGMENTACAO = reader.GetValue(3) == System.DBNull.Value ? -1 : Convert.ToInt32(reader.GetValue(3));
                            pdv.COD_EMPREGADO = reader.GetValue(4) == System.DBNull.Value ? -1 : Convert.ToInt32(reader.GetValue(4));
                            pdv.CONDICAO_PAGAMENTO = CSCondicoesPagamento.GetCondicaPagamento(reader.GetValue(5) == System.DBNull.Value ? -1 : Convert.ToInt32(reader.GetValue(5)));
                            pdv.DSC_CONDICAO_PAGAMENTO = reader.GetValue(6) == System.DBNull.Value ? "" : reader.GetString(6).Trim();
                            pdv.DSC_RAZAO_SOCIAL = reader.GetValue(7) == System.DBNull.Value ? "" : reader.GetString(7).Trim();
                            pdv.NOM_FANTASIA = reader.GetValue(8) == System.DBNull.Value ? "" : reader.GetString(8).Trim();
                            pdv.NUM_INSCRICAO_ESTADUAL = reader.GetValue(9) == System.DBNull.Value ? "" : reader.GetString(9).Trim();
                            pdv.NUM_CGC = reader.GetValue(10) == System.DBNull.Value ? "" : reader.GetString(10).Trim();
                            pdv.IND_EXCLUSIVO = reader.GetValue(11) == System.DBNull.Value ? false : Convert.ToBoolean(reader.GetValue(11));
                            pdv.IND_BLOQUEADO = reader.GetValue(12) == System.DBNull.Value ? false : Convert.ToBoolean(reader.GetValue(12));
                            pdv.VLR_LIMITE_CREDITO = reader.GetValue(13) == System.DBNull.Value ? 0 : Convert.ToDecimal(reader.GetValue(13));
                            pdv.VLR_SALDO_DEVEDOR = reader.GetValue(14) == System.DBNull.Value ? 0 : Convert.ToDecimal(reader.GetValue(14));
                            pdv.DSC_CICLO_VISITA = reader.GetValue(15) == System.DBNull.Value ? "" : reader.GetString(15);
                            pdv.DSC_CLIPPING_INFORMATIVO = reader.GetValue(16) == System.DBNull.Value ? "" : reader.GetString(16).Trim();
                            pdv.IND_INADIMPLENTE = reader.GetValue(17) == System.DBNull.Value ? false : Convert.ToBoolean(reader.GetValue(17));
                            pdv.DSC_PONTO_REFERENCIA = reader.GetValue(18) == System.DBNull.Value ? "" : reader.GetString(18);
                            pdv.NUM_ORDEM_VISITA_ROTA = reader.GetValue(19) == System.DBNull.Value ? -1 : Convert.ToInt32(reader.GetValue(19));
                            pdv.COD_SEGREGACAO = reader.GetValue(20) == System.DBNull.Value ? -1 : Convert.ToInt32(reader.GetValue(20));
                            pdv.CODCNDPGT = reader.GetValue(21) == System.DBNull.Value ? 0 : Convert.ToInt32(reader.GetValue(21));
                            pdv.FLIVND = reader.GetValue(22) == System.DBNull.Value ? 0 : Convert.ToInt32(reader.GetValue(22));
                            pdv.DSC_SEGMENTACAO = reader.GetValue(23) == System.DBNull.Value ? "" : reader.GetString(23);
                            pdv.COD_GRUPO = reader.GetValue(24) == System.DBNull.Value ? 0 : Convert.ToInt32(reader.GetValue(24));
                            pdv.DESCRICAO_GRUPO = reader.GetValue(25) == System.DBNull.Value ? "" : reader.GetString(25);
                            pdv.COD_CLASSIFICACAO = reader.GetValue(26) == System.DBNull.Value ? 0 : Convert.ToInt32(reader.GetValue(26));
                            pdv.DESCRICAO_CLASSIFICACAO = reader.GetValue(27) == System.DBNull.Value ? "" : reader.GetString(27);
                            pdv.COD_UNIDADE_NEGOCIO = reader.GetValue(28) == System.DBNull.Value ? 0 : Convert.ToInt32(reader.GetValue(28));
                            pdv.DESCRICAO_UNIDADE_NEGOCIO = reader.GetValue(29) == System.DBNull.Value ? "" : reader.GetString(29);
                            pdv.COD_TABPRECO_PADRAO = reader.GetValue(30) == System.DBNull.Value ? 0 : Convert.ToInt32(reader.GetValue(30));
                            pdv.COD_GER_BROKER = reader.GetValue(31) == System.DBNull.Value ? "" : reader.GetString(31).Trim();
                            pdv.CDFORMPAGI = reader.GetValue(32) == System.DBNull.Value ? "" : reader.GetString(32).Trim();
                            pdv.CDGER0 = reader.GetValue(33) == System.DBNull.Value ? "" : reader.GetString(33).Trim();
                            pdv.CD_CLIENTE = reader.GetValue(34) == System.DBNull.Value ? "" : reader.GetString(34).Trim();
                            pdv.IND_COBERTOMES = reader.GetValue(35) == System.DBNull.Value ? false : Convert.ToBoolean(reader.GetValue(35));
                            pdv.NUM_POSITIVACOES = reader.GetValue(36) == System.DBNull.Value ? 0 : Convert.ToInt32(reader.GetValue(36));
                            pdv.IND_PERMITIR_VENDA_FORA_ROTA = reader.GetValue(37) == System.DBNull.Value ? false : Convert.ToBoolean(reader.GetValue(37));
                            cod_dia_visita_pdv = reader.GetValue(38) == System.DBNull.Value ? -1 : Convert.ToInt32(reader.GetValue(38));
                            pdv.COD_DENVER = reader.GetValue(39) == System.DBNull.Value ? 0 : Convert.ToInt32(reader.GetValue(39));
                            pdv.DSC_DENVER = reader.GetValue(42) == System.DBNull.Value ? "" : reader.GetString(42);

                            if (CSEmpresa.ColunaExiste("PESQUISA_MERCADO_CATEGORIA", "COD_PESQUISA_MERC"))
                                pdv.IND_PERMITIR_PESQUISA = reader.GetValue(43) == DBNull.Value ? (ExisteDadosNaTabela() ? false : true) : true;
                            else
                                pdv.IND_PERMITIR_PESQUISA = true;

                            pdv.IND_ESPECIAL_INDENIZACAO_BROKER = reader.GetValue(44) == System.DBNull.Value ? false : Convert.ToBoolean(reader.GetValue(44));
                            pdv.PEDIDOS_PRAZO_DIA = reader.GetValue(45) == System.DBNull.Value ? 0 : reader.GetDecimal(45);
                            pdv.DSC_NOME_FOTO = reader.GetValue(46) == System.DBNull.Value ? string.Empty : reader.GetString(46);
                            pdv.NUM_LATITUDE_LOCALIZACAO = reader.GetValue(47) == System.DBNull.Value ? string.Empty : reader.GetString(47);
                            pdv.NUM_LONGITUDE_LOCALIZACAO = reader.GetValue(48) == System.DBNull.Value ? string.Empty : reader.GetString(48);
                            pdv.IND_PDV_LOCALIZACAO_VERIFICADA = reader.GetValue(49) == System.DBNull.Value ? false : reader.GetBoolean(49);
                            pdv.IND_BLOQUEAR_INDENIZACAO = reader.GetValue(50) == System.DBNull.Value ? false : reader.GetBoolean(50);
                            pdv.IND_FOTO_DUVIDOSA = reader.GetValue(51) == System.DBNull.Value ? "N" : reader.GetString(51);
                            pdv.BOL_FOTO_VALIDADA = reader.GetValue(52) == System.DBNull.Value ? false : reader.GetBoolean(52);
                            pdv.NUM_LATITUDE_FOTO = reader.GetValue(53) == System.DBNull.Value ? string.Empty : reader.GetString(53);
                            pdv.NUM_LONGITUDE_FOTO = reader.GetValue(54) == System.DBNull.Value ? string.Empty : reader.GetString(54);
                            pdv.VLR_META_PDV = reader.GetValue(55) == System.DBNull.Value ? 0 : reader.GetDouble(55);
                            pdv.VLR_TOTAL_VENDAS = reader.GetValue(56) == System.DBNull.Value ? 0 : reader.GetDouble(56);
                            pdv.PCT_MAXIMO_INDENIZACAO = reader.GetValue(57) == System.DBNull.Value ? 0 : reader.GetDecimal(57);

                            // Adiciona a instancia da classe pdv na coleção de PDVs
                            base.InnerList.Add(pdv);

                            pdv_carregado.Add(pdv.COD_PDV);
                            // Dispara o evento passando o pdv para ser adiciona no listview
                            if ((!CSEmpregados.Current.IND_BLOQUEADO_VENDA_FORA_ROTA) ||
                                (CSEmpregados.Current.IND_BLOQUEADO_VENDA_FORA_ROTA && pdv.IND_PERMITIR_VENDA_FORA_ROTA) ||
                                (cod_dia_visita_pdv == ((int)DateTime.Today.DayOfWeek == 0 ? 7 : (int)DateTime.Today.DayOfWeek)))

                                if (OnAddNewPDV != null)
                                    OnAddNewPDV(pdv);
                        }
                    }

                    // Fecha o reader
                    reader.Close();
                    reader.Dispose();
                    CodPdvsRota = pdv_carregado;
                    pdv_carregado = null;

                    if (CSEmpregados.Current.IND_ORDEM_VISITA_OBRIGATORIA &&
                        InserirDadosVisita)
                    {
                        CSVisitas.InserirVisita(CodPdvsRota);
                    }
                }

                // Dispara o evento de busca dos produtos
                if (OnInitGrupoProdutos != null)
                {
                    OnInitGrupoProdutos();
                }


                // Acorda a classe de grupos de produtos...
                CSGruposProduto.Items.Count.ToString();

                // Dispara o evento de busca das familias dos produtos
                if (OnInitFamiliaProdutos != null)
                {
                    OnInitFamiliaProdutos();
                }

                // Acorda a classe das familias dos produtos
                CSFamiliasProduto.Items.Count.ToString();

                // Dispara o evento de fim da importação
                if (OnEndGetPDVs != null)
                {
                    OnEndGetPDVs();
                }


                // Acorda a classe de produtos. Quando receber o evento dos produtos repassa para o PDV
                CSProdutos.OnBeginProdutos += new AvanteSales.CSProdutos.BeginGetProdutos(CSProdutos_OnBeginProdutos);
                CSProdutos.OnTickNewProduto += new AvanteSales.CSProdutos.TickNewProduto(CSProdutos_OnTickNewProduto);
                CSProdutos.OnEndProdutos += new AvanteSales.CSProdutos.EndGetProdutos(CSProdutos_OnEndProdutos);
                CSProdutos.Items.Count.ToString();

                // Dispara o evento. Pois quando ja buscou os produtos nao dispara os eventos novamente
                if (OnEndProdutos != null)
                {
                    OnEndProdutos();
                }

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());

                throw new Exception("Erro na busca dos PDVs", ex);
            }

        }

        private bool OrdenarPorPdvVisita(bool inserirDadosVisita, int codDiaVisita)
        {
            if (CSEmpresa.ColunaExiste("PDV_VISITA", "COD_EMPREGADO") &&
                !inserirDadosVisita &&
                codDiaVisita == (int)DateTime.Now.DayOfWeek &&
                !CSEmpresa.Current.IND_EMPRESA_FERIADO &&
                CSEmpregados.Current.IND_ORDEM_VISITA_OBRIGATORIA)
                return true;

            return false;
        }

        private bool ExisteDadosNaTabela()
        {
            var sql = "SELECT COUNT(*) FROM PESQUISA_MERCADO_CATEGORIA PC JOIN PESQUISA_MERCADO PM ON PC.COD_PESQUISA_MERC = PM.COD_PESQUISA_MERC";
            bool existe = true;

            using (SqliteDataReader result = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
            {
                if (result.Read())
                {
                    if (result.GetInt32(0) == 0)
                        existe = false;
                }
            }

            return existe;
        }

        public void Dispose()
        {
            this.InnerList.Clear();
            this.InnerList.TrimToSize();
        }

        public static string GetRazaoSocial(int codPDV)
        {
            string RazaoSocial = string.Empty;

            StringBuilder sql = new StringBuilder();
            SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", codPDV);

            sql.Length = 0;
            sql.AppendLine("SELECT [DSC_RAZAO_SOCIAL] FROM [PDV] WHERE [COD_PDV] = ?");

            using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(sql.ToString(), pCOD_PDV))
            {
                while (reader.Read())
                {
                    RazaoSocial = reader.GetString(0);
                }
            }

            return RazaoSocial;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TotalProdutos"></param>
        private void CSProdutos_OnBeginProdutos(int TotalProdutos)
        {
            try
            {
                // Dispara o evento de inicio da importação dos dados dos produtos
                if (OnBeginProdutos != null)
                    OnBeginProdutos(TotalProdutos);

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void CSProdutos_OnTickNewProduto()
        {
            try
            {
                // Dispara o evento de mais um produto processado
                if (OnTickNewProduto != null)
                    OnTickNewProduto();

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void CSProdutos_OnEndProdutos()
        {
            try
            {
                // Dispara o evento de fim da importação
                if (OnEndProdutos != null)
                    OnEndProdutos();

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
            }
        }

        #endregion

        #region [ Propriedades ]

        /// <summary>
        /// Devolve um objeto da coleção pelo indice informado
        /// </summary>
        public CSPDVs.CSPDV this[int Index]
        {
            get
            {
                if (this.InnerList.Count > 0)
                    return (CSPDVs.CSPDV)this.InnerList[Index];
                else
                    return null;
            }
        }

        /// <summary>
        /// Guarda o PDV atual
        /// </summary>
        public static CSPDVs.CSPDV Current
        {
            get
            {
                return m_Current;
            }
            set
            {
                m_Current = value;
                // Dispara o evento de mudança do PDV atual
                try
                {
                    if (OnChangePDV != null)
                    {
                        OnChangePDV();
                    }
                }
                catch
                {
                };
            }
        }


        public static CSPDVs Items
        {
            get
            {
                return m_Items;
            }
            set
            {
                m_Items = value;
            }
        }

        #endregion

        #region [ SubClasses ]

        /// <summary>
        /// Guarda informações sobre um PDV especifico
        /// </summary>
        public class CSPDV
#if ANDROID
 : Java.Lang.Object
#endif

        {
            #region [ Variaveis ]

            private string m_DSC_NOME_FOTO;
            private string m_NUM_LATITUDE_LOCALIZACAO;
            private string m_NUM_LONGITUDE_FOTO;
            private string m_NUM_LATITUDE_FOTO;
            private string m_NUM_LONGITUDE_LOCALIZACAO;
            private bool m_IND_PDV_LOCALIZACAO_VERIFICADA;
            private bool m_IND_BLOQUEAR_INDENIZACAO;
            private int m_COD_PDV;
            private bool m_POSSUI_PEDIDO_PENDENTE = false;
            private int m_COD_CATEGORIA;
            private int m_COD_SEGMENTACAO;
            private int m_COD_SEGREGACAO;
            private int m_COD_EMPREGADO;
            private CSCondicoesPagamento.CSCondicaoPagamento m_CONDICAO_PAGAMENTO;
            private string m_DSC_CONDICAO_PAGAMENTO;
            private string m_DSC_RAZAO_SOCIAL;
            private string m_NOM_FANTASIA;
            private string m_NUM_INSCRICAO_ESTADUAL;
            private string m_NUM_CGC;
            private bool m_IND_EXCLUSIVO;
            private bool m_IND_BLOQUEADO;
            private bool m_COLETA_OBRIGATORIA;
            private decimal m_VLR_LIMITE_CREDITO;
            private decimal m_VLR_SALDO_CREDITO_FIXO;
            private decimal m_VLR_SALDO_DEVEDOR;
            private decimal m_PEDIDOS_PRAZO_DIA;
            private string m_DSC_CICLO_VISITA;
            private string m_DSC_CLIPPING_INFORMATIVO;
            private string m_DSC_PONTO_REFERENCIA;
            private bool m_IND_INADIMPLENTE;
            private bool m_IND_POSITIVADO;
            private bool m_IND_ESPECIAL_INDENIZACAO_BROKER;
            private int m_NUM_ORDEM_VISITA_ROTA;
            private CSEnderecosPDV m_ENDERECOS_PDV;
            private CSTelefonesPDV m_TELEFONES_PDV;
            private CSContatosPDV m_CONTATOS_PDV;
            private CSPedidosPDV m_PEDIDOS_PDV = null;
            private CSIndenizacoes m_PEDIDOS_INDENIZACAO = null;
            private string m_DSC_CATEGORIA;
            private CSHistoricosMotivo m_HISTORICOS_MOTIVO;
            private CSUltimasVisitasPDV m_ULTIMAS_VISITAS;
            private CSPDVEmails m_EMAILS;
            private CSHistoricoIndenizacoesPDV m_HISTORICO_INDENIZACOES;
            private CSMonitoramentosVendedoresRotas m_MONITORAMENTOS;
            private ObjectState m_STATE;
            private int m_CODCNDPGT;
            private int m_FLIVND;
            private int m_COD_GRUPO;
            private int m_COD_CLASSIFICACAO;
            private int m_COD_UNIDADE_NEGOCIO;
            private string m_DSC_SEGMENTACAO;
            private string m_DESCRICAO_GRUPO;
            private string m_DESCRICAO_CLASSIFICACAO;
            private string m_DESCRICAO_UNIDADE_NEGOCIO;
            private CSPoliticaBroker m_POLITICA_BROKER = null;
            private CSPoliticaBroker2014 m_POLITICA_BROKER_2014 = null;
            private int m_COD_TABPRECO_PADRAO;
            private CSPesquisasMarketing m_PESQUISAS_PIV = null;
            private CSMotivosNaoPesquisa m_MOTIVOS_NAO_PESQUISA = null;
            private CSPesquisaMerchandising m_PESQUISA_MERCHANDISING = null;
            private CSPesquisasMercado m_PESQUISA_MERCADO = null;
            private CSComodatosPDV m_COMODATOS_PDV;
            private CSPesquisasMercado.CSPesquisaMercado.CSMarcas m_MARCAS = null;
            private string m_COD_GER_BROKER;
            private string m_CDFORMPAGI;
            private CSEmissoresPDV m_EMISSORES_PDV;
            private string m_CDGER0;
            private string m_CD_CLIENTE;
            private bool m_IND_COBERTOMES;
            private int m_NUM_POSITIVACOES;
            private bool m_IND_PERMITIR_VENDA_FORA_ROTA = false;
            private int m_COD_DENVER;
            private bool m_IND_PERMITIR_PESQUISA;
            private string m_DSC_DENVER;
            private bool m_IND_PRIMEIRA_VEZ_PEDIDO_SUGERIDO = true;
            private int m_COD_CFO;
            private int m_SEQ_CFO;
            private string m_LINHAS;
            private string m_IND_FOTO_DUVIDOSA;
            private bool m_BOL_FOTO_VALIDADA;
            private double m_VLR_META_PDV;
            private double m_VLR_TOTAL_VENDAS;
            private decimal m_PCT_MAXIMO_INDENIZACAO;
            #endregion

            #region [ Propriedades ]

            public CSComodatosPDV COMODATOS_PDV
            {
                get
                {
                    if (CSEmpresa.ColunaExiste("PDV_COMODATO", "COD_COMODATO"))
                    {
                        if (m_COMODATOS_PDV == null)
                        {
                            m_COMODATOS_PDV = new CSComodatosPDV(this.COD_PDV);
                        }
                    }
                    else
                        m_COMODATOS_PDV = null;

                    return m_COMODATOS_PDV;
                }
                set
                {
                    m_COMODATOS_PDV = value;
                }
            }

            public string LINHAS
            {
                get
                {
                    if (m_LINHAS == null)
                    {
                        var listaLinhas = GetLinhasVendaPdv();

                        if (listaLinhas.Count > 0)
                        {
                            for (int i = 0; i < listaLinhas.Count; i++)
                            {
                                if (i + 1 == listaLinhas.Count)
                                    m_LINHAS += listaLinhas[i];
                                else
                                    m_LINHAS += listaLinhas[i] + ",";
                            }
                        }
                        else
                            m_LINHAS = "-";
                    }

                    return m_LINHAS;
                }
                set
                {
                    m_LINHAS = value;
                }
            }

            private ArrayList GetLinhasVendaPdv()
            {
                ArrayList linhas = new ArrayList();

                string pdv = this.COD_PDV.ToString().PadLeft(10, '0');

                StringBuilder query = new StringBuilder();
                query.AppendFormat("SELECT CDGER0 FROM BRK_ECLIENTE WHERE CDCLI = '{0}'", pdv);

                using (SqliteDataReader reader = CSDataAccess.Instance.ExecuteReader(query.ToString()))
                {
                    while (reader.Read())
                    {
                        if (reader.GetValue(0) != System.DBNull.Value &&
                            !string.IsNullOrEmpty(reader.GetString(0)))
                        {
                            linhas.Add(reader.GetString(0));
                        }
                    }
                }

                return linhas;
            }

            public bool IND_ESPECIAL_INDENIZACAO_BROKER
            {
                get
                {
                    return m_IND_ESPECIAL_INDENIZACAO_BROKER;
                }
                set
                {
                    m_IND_ESPECIAL_INDENIZACAO_BROKER = value;
                }
            }

            public decimal PEDIDOS_PRAZO_DIA
            {
                get
                {
                    return m_PEDIDOS_PRAZO_DIA;
                }
                set
                {
                    m_PEDIDOS_PRAZO_DIA = value;
                }
            }

            public CSPesquisasMercado PESQUISA_MERCADO
            {
                get
                {
                    if (m_PESQUISA_MERCADO == null)
                    {
                        m_PESQUISA_MERCADO = new CSPesquisasMercado(this);

                        if (m_PESQUISA_MERCADO.Count > 0)
                        {
                            CSPesquisasMercado.CSPesquisaMercado pesquisa = (CSPesquisasMercado.CSPesquisaMercado)m_PESQUISA_MERCADO[0];

                            // [ Carrega marcas e respostas da pesquisa ]
                            m_MARCAS = new CSPesquisasMercado.CSPesquisaMercado.CSMarcas(pesquisa, this);
                        }
                    }

                    if (m_PESQUISA_MERCADO.Count > 0)
                    {
                        CSPesquisasMercado.CSPesquisaMercado pesquisa = (CSPesquisasMercado.CSPesquisaMercado)m_PESQUISA_MERCADO[0];

                        pesquisa.MARCAS = m_MARCAS;
                    }

                    return m_PESQUISA_MERCADO;
                }
                set
                {
                    m_PESQUISA_MERCADO = value;
                }
            }

            public CSPesquisaMerchandising PESQUISA_MERCHANDISING
            {
                get
                {
                    if (m_PESQUISA_MERCHANDISING == null)
                        m_PESQUISA_MERCHANDISING = new CSPesquisaMerchandising(this);

                    return m_PESQUISA_MERCHANDISING;
                }
                set
                {
                    m_PESQUISA_MERCHANDISING = value;
                }
            }

            public CSMotivosNaoPesquisa MOTIVOS_NAO_PESQUISA_MERCADO
            {
                get
                {
                    if (m_MOTIVOS_NAO_PESQUISA == null)
                        m_MOTIVOS_NAO_PESQUISA = new CSMotivosNaoPesquisa(this.COD_PDV, true);

                    return m_MOTIVOS_NAO_PESQUISA;
                }
                set
                {
                    m_MOTIVOS_NAO_PESQUISA = value;
                }
            }

            public CSMotivosNaoPesquisa MOTIVOS_NAO_PESQUISA
            {
                get
                {
                    if (m_MOTIVOS_NAO_PESQUISA == null)
                        m_MOTIVOS_NAO_PESQUISA = new CSMotivosNaoPesquisa(this.COD_PDV);

                    return m_MOTIVOS_NAO_PESQUISA;
                }
                set
                {
                    m_MOTIVOS_NAO_PESQUISA = value;
                }
            }

            /// <summary>
            /// Retorna coleção de pesquisas
            /// </summary>
            public CSPesquisasMarketing PESQUISAS_PIV
            {
                get
                {
                    if (m_PESQUISAS_PIV == null)
                    {
                        m_PESQUISAS_PIV = new CSPesquisasMarketing(this);
                    }
                    return m_PESQUISAS_PIV;
                }
            }

            /// <summary>
            /// Guarda o codigo do PDV
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

            public int SEQ_CFO
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

            public bool POSSUI_PEDIDO_PENDENTE
            {
                get
                {
                    return m_POSSUI_PEDIDO_PENDENTE;
                }
                set
                {
                    m_POSSUI_PEDIDO_PENDENTE = value;
                }
            }

            public bool COLETA_OBRIGATORIA
            {
                get
                {
                    return m_COLETA_OBRIGATORIA;
                }
                set
                {
                    m_COLETA_OBRIGATORIA = value;
                }
            }

            /// <summary>
            /// Guarda o codigo da tabela de preço padrão do pdv
            /// </summary>
            public int COD_TABPRECO_PADRAO
            {
                get
                {
                    return m_COD_TABPRECO_PADRAO;
                }
                set
                {
                    m_COD_TABPRECO_PADRAO = value;
                }
            }

            /// <summary>
            /// Retorna classe para cálculo de preços na política broker
            /// </summary>
            public CSPoliticaBroker POLITICA_BROKER
            {
                get
                {
                    if (m_POLITICA_BROKER == null)
                    {
                        if (this.PEDIDOS_PDV.Current != null)
                        {
                            m_POLITICA_BROKER = new CSPoliticaBroker(CSEmpresa.Current.DATA_ENTREGA,
                                this.COD_PDV, this.COD_PDV,
                                this.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO.CODPRZPGT);
                        }
                        else if (this.PEDIDOS_INDENIZACAO.Current != null)
                        {
                            m_POLITICA_BROKER = new CSPoliticaBroker(CSEmpresa.Current.DATA_ENTREGA,
                                this.COD_PDV, this.COD_PDV,
                                this.PEDIDOS_INDENIZACAO.Current.CONDICAO_PAGAMENTO.CODPRZPGT);
                        }

                        m_POLITICA_BROKER.COD_GER_BROKER = this.COD_GER_BROKER;

                    }
                    return m_POLITICA_BROKER;
                }
                set
                {
                    m_POLITICA_BROKER = value;
                }
            }

            public CSPoliticaBroker2014 POLITICA_BROKER_2014
            {
                get
                {
                    if (m_POLITICA_BROKER_2014 == null)
                    {
                        if (this.PEDIDOS_PDV.Current != null)
                        {
                            m_POLITICA_BROKER_2014 = new CSPoliticaBroker2014(CSEmpresa.Current.DATA_ENTREGA,
                                this.COD_PDV, this.COD_PDV,
                                this.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO.CODPRZPGT);
                        }
                        else if (this.PEDIDOS_INDENIZACAO.Current != null)
                        {
                            m_POLITICA_BROKER_2014 = new CSPoliticaBroker2014(CSEmpresa.Current.DATA_ENTREGA,
                                this.COD_PDV, this.COD_PDV,
                                this.PEDIDOS_INDENIZACAO.Current.CONDICAO_PAGAMENTO.CODPRZPGT);
                        }

                        m_POLITICA_BROKER_2014.COD_GER_BROKER = this.COD_GER_BROKER;

                    }
                    return m_POLITICA_BROKER_2014;
                }
                set
                {
                    m_POLITICA_BROKER_2014 = value;
                }
            }

            /// <summary>
            /// Guarda o codigo do categoria
            /// </summary>
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

            /// <summary>
            /// Guarda a descricao da categoria
            /// </summary>
            public string DSC_CATEGORIA
            {
                get
                {
                    return m_DSC_CATEGORIA;
                }
                set
                {
                    m_DSC_CATEGORIA = value.Trim();
                }
            }

            /// <summary>
            /// Guarda se a categoria permiti venda fora de rota
            /// </summary>
            public bool IND_PERMITIR_VENDA_FORA_ROTA
            {
                get
                {
                    return m_IND_PERMITIR_VENDA_FORA_ROTA;
                }
                set
                {
                    m_IND_PERMITIR_VENDA_FORA_ROTA = value;
                }
            }

            /// <summary>
            /// Guarda o codigo da segmentacao
            /// </summary>
            public int COD_SEGMENTACAO
            {
                get
                {
                    return m_COD_SEGMENTACAO;
                }
                set
                {
                    m_COD_SEGMENTACAO = value;
                }
            }

            /// <summary>
            /// Guarda a descricao da segmentacao
            /// </summary>
            public string DSC_SEGMENTACAO
            {
                get
                {
                    return m_DSC_SEGMENTACAO;
                }
                set
                {
                    m_DSC_SEGMENTACAO = value;
                }
            }



            /// <summary>
            /// Guarda o codigo da segregacao
            /// </summary>
            public int COD_SEGREGACAO
            {
                get
                {
                    return m_COD_SEGREGACAO;
                }
                set
                {
                    m_COD_SEGREGACAO = value;
                }
            }

            /// <summary>
            /// Guarda o codigo do empregado
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
            /// Guarda o codigo da condicao de pagamento
            /// </summary>
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

            /// <summary>
            /// Guarda o codigo da condicao de pagamento
            /// </summary>
            public string DSC_CONDICAO_PAGAMENTO
            {
                get
                {
                    return m_DSC_CONDICAO_PAGAMENTO;
                }
                set
                {
                    m_DSC_CONDICAO_PAGAMENTO = value.Trim();
                }
            }

            /// <summary>
            /// Guarda o nome da razao social do PDV
            /// </summary>
            public string DSC_RAZAO_SOCIAL
            {
                get
                {
                    return m_DSC_RAZAO_SOCIAL;
                }
                set
                {
                    m_DSC_RAZAO_SOCIAL = value.Trim();
                }
            }

            /// <summary>
            /// Guarda o nome fantasia social do PDV
            /// </summary>
            public string NOM_FANTASIA
            {
                get
                {
                    return m_NOM_FANTASIA;
                }
                set
                {
                    m_NOM_FANTASIA = value.Trim();
                }
            }

            /// <summary>
            /// Guarda o numero da inscricao estadual do PDV
            /// </summary>
            public string NUM_INSCRICAO_ESTADUAL
            {
                get
                {
                    return m_NUM_INSCRICAO_ESTADUAL;
                }
                set
                {
                    m_NUM_INSCRICAO_ESTADUAL = value.Trim();
                }
            }

            /// <summary>
            /// Guarda o numero do CGC do PDV
            /// </summary>
            public string NUM_CGC
            {
                get
                {
                    return m_NUM_CGC;
                }
                set
                {
                    m_NUM_CGC = value;
                }
            }

            /// <summary>
            /// Guarda o PDV é exclusivo
            /// </summary>
            public bool IND_EXCLUSIVO
            {
                get
                {
                    return m_IND_EXCLUSIVO;
                }
                set
                {
                    m_IND_EXCLUSIVO = value;
                }
            }

            /// <summary>
            /// Guarda se o PDV esta bloqueado
            /// </summary>
            public bool IND_BLOQUEADO
            {
                get
                {
                    return m_IND_BLOQUEADO;
                }
                set
                {
                    m_IND_BLOQUEADO = value;
                }
            }

            /// <summary>
            /// Guarda o valor do limite de credito do PDV 
            /// </summary>
            public decimal VLR_LIMITE_CREDITO
            {
                get
                {
                    return m_VLR_LIMITE_CREDITO;
                }
                set
                {
                    m_VLR_LIMITE_CREDITO = value;
                }
            }

            /// <summary>
            /// Guarda o valor do saldo devedor do PDV
            /// </summary>
            public decimal VLR_SALDO_DEVEDOR
            {
                get
                {
                    return m_VLR_SALDO_DEVEDOR;
                }
                set
                {
                    m_VLR_SALDO_DEVEDOR = value;
                }
            }

            /// <summary>
            /// Guarda o dia do ciclo de visita do PDV
            /// </summary>
            public string DSC_CICLO_VISITA
            {
                get
                {
                    return m_DSC_CICLO_VISITA;
                }
                set
                {
                    m_DSC_CICLO_VISITA = value;
                }
            }

            /// <summary>
            /// Guarda se o clipping informativo do PDV
            /// </summary>
            public string DSC_CLIPPING_INFORMATIVO
            {
                get
                {
                    return m_DSC_CLIPPING_INFORMATIVO;
                }
                set
                {
                    m_DSC_CLIPPING_INFORMATIVO = value;
                }
            }

            /// <summary>
            /// Guarda se o PDV esta inadimplente ou nao
            /// </summary>
            public bool IND_INADIMPLENTE
            {
                get
                {
                    return m_IND_INADIMPLENTE;
                }
                set
                {
                    m_IND_INADIMPLENTE = value;
                }
            }

            /// <summary>
            /// Guarda a descricao do ponto de referencia
            /// </summary>
            public string DSC_PONTO_REFERENCIA
            {
                get
                {
                    return m_DSC_PONTO_REFERENCIA;
                }
                set
                {
                    m_DSC_PONTO_REFERENCIA = value;
                }
            }

            /// <summary>
            /// Guarda se o PDV foi positivado ou nao
            /// </summary>
            public bool IND_POSITIVADO
            {
                get
                {
                    string sqlQuery =
                        "SELECT COUNT(*) " +
                        "  FROM PEDIDO " +
                        " WHERE COD_PDV = ? " +
                        "   AND IND_HISTORICO = 0 " +
                        "   AND COD_MOT_INDENIZACAO IS NULL " +
                        "   AND DAT_PEDIDO = ? ";

                    SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", this.COD_PDV);
                    pCOD_PDV.DbType = DbType.Int32;
                    SQLiteParameter pDAT_PEDIDO = new SQLiteParameter("@DAT_PEDIDO", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
                    pDAT_PEDIDO.DbType = DbType.DateTime;

                    // Busca todos os contatos do PDV
                    using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, CommandBehavior.SingleRow, pCOD_PDV, pDAT_PEDIDO))
                    {
                        if (sqlReader.Read())
                        {
                            m_IND_POSITIVADO = Convert.ToBoolean(sqlReader.GetInt32(0));

                        }
                    }

                    return m_IND_POSITIVADO;
                }

                set
                {
                    m_IND_POSITIVADO = value;
                }
            }

            /// <summary>
            /// Guarda se o PDV foi positivado ou nao
            /// </summary>
            public int NUM_ORDEM_VISITA_ROTA
            {
                get
                {
                    return m_NUM_ORDEM_VISITA_ROTA;
                }
                set
                {
                    m_NUM_ORDEM_VISITA_ROTA = value;
                }
            }

            public int CODCNDPGT
            {
                get
                {
                    return m_CODCNDPGT;
                }
                set
                {
                    m_CODCNDPGT = value;
                }
            }

            public int FLIVND
            {
                get
                {
                    return m_FLIVND;
                }
                set
                {
                    m_FLIVND = value;
                }
            }

            /// <summary>
            /// Guarda a descricao do grupo do cliente
            /// </summary>
            public string DESCRICAO_GRUPO
            {
                get
                {
                    return m_DESCRICAO_GRUPO;
                }
                set
                {
                    m_DESCRICAO_GRUPO = value;
                }
            }


            /// <summary>
            /// Guarda o grupo do cliente
            /// </summary>
            public int COD_GRUPO
            {
                get
                {
                    return m_COD_GRUPO;
                }
                set
                {
                    m_COD_GRUPO = value;
                }
            }


            /// <summary>
            /// Guarda o grupo classificacao do cliente
            /// </summary>
            public int COD_CLASSIFICACAO
            {
                get
                {
                    return m_COD_CLASSIFICACAO;
                }
                set
                {
                    m_COD_CLASSIFICACAO = value;
                }
            }

            /// <summary>
            /// Guarda a descricao da classificacao do cliente
            /// </summary>
            public string DESCRICAO_CLASSIFICACAO
            {
                get
                {
                    return m_DESCRICAO_CLASSIFICACAO;
                }
                set
                {
                    m_DESCRICAO_CLASSIFICACAO = value;
                }
            }


            /// <summary>
            /// Guarda o codigo da unidade de negocio do cliente
            /// </summary>
            public int COD_UNIDADE_NEGOCIO
            {
                get
                {
                    return m_COD_UNIDADE_NEGOCIO;
                }
                set
                {
                    m_COD_UNIDADE_NEGOCIO = value;
                }
            }


            /// <summary>
            /// Guarda a descrição da unidade de negocio do cliente
            /// </summary>
            public string DESCRICAO_UNIDADE_NEGOCIO
            {
                get
                {
                    return m_DESCRICAO_UNIDADE_NEGOCIO;
                }
                set
                {
                    m_DESCRICAO_UNIDADE_NEGOCIO = value;
                }
            }
            public string COD_GER_BROKER
            {
                get
                {
                    return m_COD_GER_BROKER;
                }
                set
                {
                    m_COD_GER_BROKER = value;
                }

            }
            public string CDGER0
            {
                get
                {
                    return m_CDGER0;
                }
                set
                {
                    m_CDGER0 = value;
                }
            }
            public string CD_CLIENTE
            {
                get
                {
                    return m_CD_CLIENTE;
                }
                set
                {
                    m_CD_CLIENTE = value;
                }
            }
            public string CDFORMPAGI
            {
                get
                {
                    return m_CDFORMPAGI;
                }
                set
                {
                    m_CDFORMPAGI = value;
                }

            }
            public bool IND_COBERTOMES
            {
                get
                {
                    return m_IND_COBERTOMES;
                }
                set
                {
                    m_IND_COBERTOMES = value;
                }
            }
            public int NUM_POSITIVACOES
            {
                get
                {
                    return m_NUM_POSITIVACOES;
                }
                set
                {
                    m_NUM_POSITIVACOES = value;
                }
            }
            public int COD_DENVER
            {
                get
                {
                    return m_COD_DENVER;
                }
                set
                {
                    m_COD_DENVER = value;
                }
            }

            public bool IND_PERMITIR_PESQUISA
            {
                get
                {
                    return m_IND_PERMITIR_PESQUISA;
                }
                set
                {
                    m_IND_PERMITIR_PESQUISA = value;
                }
            }
            public string DSC_DENVER
            {
                get
                {
                    return m_DSC_DENVER;
                }
                set
                {
                    m_DSC_DENVER = value;
                }
            }

            public bool BOL_FOTO_VALIDADA
            {
                get
                {
                    return m_BOL_FOTO_VALIDADA;
                }
                set
                {
                    m_BOL_FOTO_VALIDADA = value;
                }
            }

            public string IND_FOTO_DUVIDOSA
            {
                get
                {
                    return m_IND_FOTO_DUVIDOSA;
                }
                set
                {
                    m_IND_FOTO_DUVIDOSA = value;
                }
            }

            public string DSC_NOME_FOTO
            {
                get
                {
                    return m_DSC_NOME_FOTO;
                }
                set
                {
                    m_DSC_NOME_FOTO = value;
                }
            }

            public string NUM_LATITUDE_FOTO
            {
                get
                {
                    return m_NUM_LATITUDE_FOTO;
                }
                set
                {
                    m_NUM_LATITUDE_FOTO = value;
                }
            }

            public string NUM_LONGITUDE_FOTO
            {
                get
                {
                    return m_NUM_LONGITUDE_FOTO;
                }
                set
                {
                    m_NUM_LONGITUDE_FOTO = value;
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
                    if (value.Contains("."))
                        value = value.Replace(".", ",");

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
                    if (value.Contains("."))
                        value = value.Replace(".", ",");

                    m_NUM_LONGITUDE_LOCALIZACAO = value;
                }
            }

            public bool IND_PDV_LOCALIZACAO_VERIFICADA
            {
                get
                {
                    return m_IND_PDV_LOCALIZACAO_VERIFICADA;
                }
                set
                {
                    m_IND_PDV_LOCALIZACAO_VERIFICADA = value;
                }
            }

            public bool IND_BLOQUEAR_INDENIZACAO
            {
                get
                {
                    return m_IND_BLOQUEAR_INDENIZACAO;
                }
                set
                {
                    m_IND_BLOQUEAR_INDENIZACAO = value;
                }
            }

            public bool IND_PRIMEIRA_VEZ_PEDIDO_SUGERIDO
            {
                get
                {
                    StringBuilder sqlQuery = new StringBuilder();

                    sqlQuery.AppendLine("SELECT COUNT(0) ");
                    sqlQuery.AppendLine("FROM PRODUTO_COLETA_ESTOQUE ");
                    sqlQuery.AppendLine("WHERE COD_PDV = " + COD_PDV);
                    sqlQuery.AppendLine(" AND IND_HISTORICO = 1 ");
                    var resultado = Convert.ToInt32(CSDataAccess.Instance.ExecuteScalar(sqlQuery.ToString()));

                    return m_IND_PRIMEIRA_VEZ_PEDIDO_SUGERIDO = resultado == 0;
                }
            }

            /// <summary>
            /// Devolve a string que será desenhada no top de cada form
            /// </summary>
            /// 
            public string TITULO_FORM
            {
                get
                {
#if !ANDROID
                    string ret, nome;
                    Image img = null;
                    Graphics gfx = null;
                    SizeF s;

                    try
                    {
                        img = new Bitmap(10, 10);
                        gfx = Graphics.FromImage(img);

                        if (CSGlobal.TipoNomePDV == 1)
                            nome = this.NOM_FANTASIA;
                        else
                            nome = this.DSC_RAZAO_SOCIAL;

                        ret = this.NUM_ORDEM_VISITA_ROTA.ToString("###000") + "-";
                        ret += nome + "-";
                        ret += this.COD_PDV.ToString();

                        s = gfx.MeasureString(ret, new Font("Tahoma", 8, FontStyle.Bold));

                        var widthFactor = (float)CSGlobal.GetVisibleDesktop().Width / 240;
                        var tamanhoMaximo = CSGlobal.GetPixelFactor(185, widthFactor);
                        while (s.Width >= tamanhoMaximo)
                        {
                            ret = this.NUM_ORDEM_VISITA_ROTA.ToString("###000") + "-";
                            nome = nome.Substring(0, nome.Length - 1);
                            ret += nome + "...-";
                            ret += this.COD_PDV.ToString();
                            s = gfx.MeasureString(ret, new Font("Tahoma", 8, FontStyle.Bold));
                        }

                        // Libera a memoria
                        gfx.Dispose();
                        img.Dispose();
                        return ret;

                    }
                    catch
                    {
                        return "";
                    }
#else
                    return "Avante Sales";
#endif
                }
            }

            /// <summary>
            /// Guarda as informações de endereço do PDV
            /// </summary>
            public CSEnderecosPDV ENDERECOS_PDV
            {
                get
                {
                    // Preenche a classe do endereco do PDV somente na hora que for usar pela primeira vez
                    if (m_ENDERECOS_PDV == null)
                        m_ENDERECOS_PDV = new CSEnderecosPDV(COD_PDV);
                    return m_ENDERECOS_PDV;
                }
                set
                {
                    m_ENDERECOS_PDV = value;
                }
            }

            /// <summary>
            /// Guarda as informações de telefones do PDV
            /// </summary>
            public CSTelefonesPDV TELEFONES_PDV
            {
                get
                {
                    // Preenche a classe do endereco do PDV somente na hora que for usar pela primeira vez
                    if (m_TELEFONES_PDV == null)
                        m_TELEFONES_PDV = new CSTelefonesPDV(COD_PDV);
                    return m_TELEFONES_PDV;
                }
                set
                {
                    m_TELEFONES_PDV = value;
                }
            }

            /// <summary>
            /// Guarda as informações de telefones do PDV
            /// </summary>
            public CSContatosPDV CONTATOS_PDV
            {
                get
                {
                    // Preenche a classe do endereco do PDV somente na hora que for usar pela primeira vez
                    if (m_CONTATOS_PDV == null)
                        m_CONTATOS_PDV = new CSContatosPDV(COD_PDV);
                    return m_CONTATOS_PDV;
                }
                set
                {
                    m_CONTATOS_PDV = value;
                }
            }

            /// <summary>
            /// Guarda as informações de pedidos do PDV
            /// </summary>
            public CSPedidosPDV PEDIDOS_PDV
            {
                get
                {
                    // Preenche a classe do endereco do PDV somente na hora que for usar pela primeira vez
                    if (m_PEDIDOS_PDV == null)
                        m_PEDIDOS_PDV = new CSPedidosPDV(COD_PDV);

                    return m_PEDIDOS_PDV;
                }
                set
                {
                    m_PEDIDOS_PDV = value;
                }
            }

            public CSIndenizacoes PEDIDOS_INDENIZACAO
            {
                get
                {
                    if (m_PEDIDOS_INDENIZACAO == null)
                        m_PEDIDOS_INDENIZACAO = new CSIndenizacoes(COD_PDV);

                    return m_PEDIDOS_INDENIZACAO;
                }
                set
                {
                    m_PEDIDOS_INDENIZACAO = value;
                }
            }

            /// <summary>
            /// Guarda os historicos de motivo não positivação do PDV
            /// </summary>
            public CSHistoricosMotivo HISTORICOS_MOTIVO
            {
                get
                {
                    if (m_HISTORICOS_MOTIVO == null)
                        m_HISTORICOS_MOTIVO = new CSHistoricosMotivo(COD_PDV);
                    return m_HISTORICOS_MOTIVO;
                }
                set
                {
                    m_HISTORICOS_MOTIVO = value;
                }
            }
            /// <summary>
            /// Guarda monitoramento do PDV
            /// </summary>
            public CSMonitoramentosVendedoresRotas MONITORAMENTOS
            {
                get
                {
                    if (m_MONITORAMENTOS == null)
                        m_MONITORAMENTOS = new CSMonitoramentosVendedoresRotas(COD_PDV);
                    return m_MONITORAMENTOS;
                }
                set
                {
                    m_MONITORAMENTOS = value;
                }
            }

            public CSPDVEmails EMAILS
            {
                get
                {
                    m_EMAILS = new CSPDVEmails();

                    return m_EMAILS;
                }
                set
                {
                    m_EMAILS = value;
                }
            }

            /// <summary>
            /// Guarda as Ultimas Visitas do PDV
            /// </summary>
            public CSUltimasVisitasPDV ULTIMAS_VISITAS
            {
                get
                {
                    if (m_ULTIMAS_VISITAS == null)
                        m_ULTIMAS_VISITAS = new CSUltimasVisitasPDV(this.COD_PDV);
                    return m_ULTIMAS_VISITAS;
                }
                set
                {
                    m_ULTIMAS_VISITAS = value;
                }
            }

            /// <summary>
            /// Guarda as Ultimas Visitas do PDV
            /// </summary>
            public CSHistoricoIndenizacoesPDV HISTORICO_INDENIZACOES
            {
                get
                {
                    if (m_HISTORICO_INDENIZACOES == null)
                        m_HISTORICO_INDENIZACOES = new CSHistoricoIndenizacoesPDV(this.COD_PDV);
                    return m_HISTORICO_INDENIZACOES;
                }
                set
                {
                    m_HISTORICO_INDENIZACOES = value;
                }
            }

            /// <summary>
            /// Guarda se o PDV foi positivado ou nao
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

            /// <summary>
            /// Calcula o valor do saldo de credito do PDV
            /// </summary>
            /// <returns></returns>
            public decimal VLR_SALDO_CREDITO
            {
                get
                {
                    string sqlQuery;
                    decimal valorSaldoCredito = 0;
                    decimal valorTotalPedidosPrazo = 0;
                    DateTime dataUltimaDescarga;

                    sqlQuery =
                        "SELECT MAX(DAT_CARGA_DESCARGA) " +
                        "  FROM LOG_CARGA_DESCARGA " +
                        " WHERE TIPO_OPERACAO = 'D' ";

                    // Data da Ultima Descarga
                    dataUltimaDescarga = DateTime.Parse(CSDataAccess.Instance.ExecuteScalar(sqlQuery).ToString());

                    sqlQuery =
                        "SELECT SUM(VLR_TOTAL_PEDIDO) " +
                        "  FROM PEDIDO T1 " +
                        "  JOIN CONDICAO_PAGAMENTO T2 ON T1.COD_CONDICAO_PAGAMENTO = T2.COD_CONDICAO_PAGAMENTO " +
                        " WHERE COD_TIPO_CONDICAO_PAGAMENTO = 2 " +
                        "   AND IND_HISTORICO = 0 " +
                        "   AND (DAT_ALTERACAO >= '" + dataUltimaDescarga.ToString("MM/dd/yyyy") + "' " +
                        "    OR DATE(DAT_PEDIDO) >= '" + dataUltimaDescarga.ToString("MM/dd/yyyy") + "') " +
                        "   AND COD_PDV = " + this.COD_PDV;

                    // Somatorio do valor total dos pedidos a prazo que ainda nao foram descarregados
                    using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery))
                    {
                        if (sqlReader.Read())
                        {
                            // Preenche a instancia da classe de pedido do pdv
                            valorTotalPedidosPrazo = sqlReader.GetValue(0) == System.DBNull.Value ? 0 : decimal.Parse(sqlReader.GetValue(0).ToString());
                        }

                        // Fecha o reader
                        sqlReader.Close();
                        sqlReader.Dispose();
                    }

                    // VLR_LIMITE_CREDITO - SOMA(PEDIDOS A PRAZO EFETUADOS P/PDV NO DIA)
                    //Procura valor do saldo de credito
                    // [ Recupera documentos ]
                    CSDocumentosReceberPDV documentosAReceber = this.GetDocumentosAReceber(CSGlobal.COD_REVENDA);

                    foreach (CSDocumentosReceberPDV.CSDocumentoReceber docReceber in documentosAReceber.Items)
                    {
                        if (docReceber.TIPO_DOCUMENTO == CSDocumentosReceberPDV.CSDocumentoReceber.TipoDocumento.SALDO)
                        {
                            valorSaldoCredito = docReceber.SALDO_CREDITO;
                            break;
                        }
                    }

                    valorSaldoCredito -= valorTotalPedidosPrazo;
                    valorSaldoCredito = CSGlobal.StrToDecimal(valorSaldoCredito.ToString(CSGlobal.DecimalStringFormat));

                    return valorSaldoCredito;
                }
            }

            /// <summary>
            /// Calcula o valor do saldo dos pedidos do dia
            /// </summary>
            public decimal VLR_SALDO_PEDIDO
            {
                get
                {
                    string sqlQuery;
                    decimal valorTotalPedidosPrazo = 0;
                    DateTime dataUltimaDescarga;

                    sqlQuery =
                        "SELECT MAX(DAT_CARGA_DESCARGA) " +
                        "  FROM LOG_CARGA_DESCARGA " +
                        " WHERE TIPO_OPERACAO = 'D' ";

                    // Data da Ultima Descarga
                    dataUltimaDescarga = DateTime.Parse(CSDataAccess.Instance.ExecuteScalar(sqlQuery).ToString());

                    sqlQuery =
                        "SELECT SUM(VLR_TOTAL_PEDIDO) " +
                        "  FROM PEDIDO T1 " +
                        "  JOIN CONDICAO_PAGAMENTO T2 ON T1.COD_CONDICAO_PAGAMENTO = T2.COD_CONDICAO_PAGAMENTO " +
                        " WHERE COD_TIPO_CONDICAO_PAGAMENTO = 2 " +
                        "   AND IND_HISTORICO = 0 " +
                        "   AND (DAT_ALTERACAO >= '" + dataUltimaDescarga.ToString("MM/dd/yyyy") + "' " +
                        "    OR DATE(DAT_PEDIDO) >= '" + dataUltimaDescarga.ToString("MM/dd/yyyy") + "') " +
                        "   AND COD_PDV = " + this.COD_PDV;

                    // Somatorio do valor total dos pedidos a prazo que ainda nao foram descarregados
                    using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery))
                    {
                        if (sqlReader.Read())
                        {
                            // Preenche a instancia da classe de pedido do pdv
                            valorTotalPedidosPrazo = sqlReader.GetValue(0) == System.DBNull.Value ? 0 : decimal.Parse(sqlReader.GetValue(0).ToString());
                        }

                        // Fecha o reader
                        sqlReader.Close();
                        sqlReader.Dispose();
                    }

                    return valorTotalPedidosPrazo;
                }
            }

            /// <summary>
            /// Calcula o valor do saldo do credito do PDV sem alterações
            /// </summary>
            public decimal VLR_SALDO_CREDITO_ATUALIZADO
            {
                get
                {
                    decimal valorSaldoCredito = 0;
                    decimal valorPedido = 0m;

                    foreach (CSPedidosPDV.CSPedidoPDV pedido in CSPDVs.Current.PEDIDOS_PDV)
                    {
                        CSPedidosPDV.CSPedidoPDV pedidoAtual;

                        pedidoAtual = pedido;

                        foreach (CSItemsPedido.CSItemPedido itemPedido in pedidoAtual.ITEMS_PEDIDOS)
                        {
                            if (pedidoAtual.CONDICAO_PAGAMENTO.COD_TIPO_CONDICAO_PAGAMENTO == 2)
                            {
                                if (itemPedido.STATE != ObjectState.DELETADO)
                                    valorPedido += itemPedido.VLR_TOTAL_ITEM;
                            }
                        }
                    }

                    // VLR_LIMITE_CREDITO - SOMA(PEDIDOS A PRAZO EFETUADOS P/PDV NO DIA)
                    //Procura valor do saldo de credito
                    // [ Recupera documentos ]

                    CSDocumentosReceberPDV documentosAReceber = this.GetDocumentosAReceber(CSGlobal.COD_REVENDA);

                    foreach (CSDocumentosReceberPDV.CSDocumentoReceber docReceber in documentosAReceber.Items)
                    {
                        if (docReceber.TIPO_DOCUMENTO == CSDocumentosReceberPDV.CSDocumentoReceber.TipoDocumento.SALDO)
                        {
                            valorSaldoCredito = docReceber.SALDO_CREDITO;
                            break;
                        }
                    }

                    valorPedido += CSPDVs.Current.PEDIDOS_PRAZO_DIA;

                    valorSaldoCredito -= valorPedido;
                    valorSaldoCredito = CSGlobal.StrToDecimal(valorSaldoCredito.ToString(CSGlobal.DecimalStringFormat));

                    return valorSaldoCredito;
                }
            }

            public string POPUP_TEXT
            {
                get
                {
                    string texto = "";
                    texto += "Cliente: " + this.NUM_ORDEM_VISITA_ROTA.ToString("###000") + "-" + this.COD_PDV.ToString();
                    texto += "\n" + ((CSGlobal.TipoNomePDV == 1) ? this.NOM_FANTASIA : this.DSC_RAZAO_SOCIAL);
                    texto += "\n" + "Categoria:";
                    texto += "\n" + this.DSC_CATEGORIA;
                    if (CSEmpresa.Current.UtilizaDescricaoDenver)
                    {
                        texto += "\nDescrição Denver:";
                        texto += "\n" + this.DSC_DENVER;
                    }
                    return texto;
                }
            }

            /// <summary>
            /// Busca operacões pelo codigo de UF
            /// </summary>
            /// <returns>Retorna as operação permitidas</returns>
            public ArrayList OPERACOES
            {
                get
                {
                    ArrayList result = new ArrayList();

                    foreach (CSEnderecosPDV.CSEnderecoPDV endereco in this.ENDERECOS_PDV)
                    {
                        // [ Procura endereço comercial ]
                        if (endereco.COD_TIPO_ENDERECO == 1)
                        {
                            //SE CODIGO DE REVENDA FOR IGUAL A  188300 (LA BASQUE), LIBERAR TODAS OPERAÇÕES INDEPENDETE DO COD_CFO
                            // [ Verifica inicial do código do cfo ]
                            string cfo = (CSEmpresa.Current.COD_ESTADO == endereco.COD_UF) ? "5" : "6";

                            // Procura pela operação
                            foreach (CSOperacoes.CSOperacao operacao in CSOperacoes.Items)
                            {
                                if (CSEmpresa.Current.CODIGO_REVENDA == "188300" ||
                                    operacao.COD_CFO.ToString().StartsWith(cfo))
                                    result.Add(operacao);
                            }

                            break;
                        }
                    }
                    return result;
                }
            }

            /// <summary>
            /// Guarda as informações de emissores do PDV
            /// </summary>
            public CSEmissoresPDV EmissoresPDV
            {
                get
                {
                    // Preenche a classe do emissores do PDV somente na hora que for usar pela primeira vez
                    if (m_EMISSORES_PDV == null)
                        m_EMISSORES_PDV = new CSEmissoresPDV(COD_PDV);
                    return m_EMISSORES_PDV;
                }
                set
                {
                    m_EMISSORES_PDV = value;
                }
            }

            public double VLR_META_PDV
            {
                get
                {
                    return m_VLR_META_PDV;
                }
                set
                {
                    m_VLR_META_PDV = value;
                }
            }

            public double VLR_TOTAL_VENDAS
            {
                get
                {
                    return m_VLR_TOTAL_VENDAS;
                }
                set
                {
                    m_VLR_TOTAL_VENDAS = value;
                }
            }

            public decimal PCT_MAXIMO_INDENIZACAO
            {
                get
                {
                    return m_PCT_MAXIMO_INDENIZACAO;
                }
                set
                {
                    m_PCT_MAXIMO_INDENIZACAO = value;
                }
            }

            #endregion

            #region [ Metodos ]

            // Construtor da classe
            public CSPDV()
            {

            }

            public bool PDVDentroRota()
            {
                return CSPDVs.CodPdvsRota.Contains(CSPDVs.Current.COD_PDV);
            }

            /// <summary>
            /// Método responsável por preencher a tabela de Estoque do dia referente aos produtos da última coleta
            /// </summary>
            public void PrepararProdutoParaColeta()
            {
                try
                {
                    StringBuilder sqlQuery = new StringBuilder();

                    sqlQuery.Length = 0;
                    sqlQuery.Append("INSERT INTO [PRODUTO_COLETA_ESTOQUE] ");
                    sqlQuery.Append("(DAT_COLETA, ");
                    sqlQuery.Append("COD_EMPREGADO, ");
                    sqlQuery.Append("COD_PDV, ");
                    sqlQuery.Append("COD_PRODUTO, ");
                    sqlQuery.Append("QTD_COLETADA, ");
                    sqlQuery.Append("IND_HISTORICO, ");
                    sqlQuery.Append("QTD_PERDA, ");
                    sqlQuery.Append("QTD_GIRO_SELLOUT, ");
                    sqlQuery.Append("SOM_UTM_QTD_GIRO_SELLOUT, ");
                    sqlQuery.Append("QTD_COLETA_SELLOUT, ");
                    sqlQuery.Append("NUM_COLETA_ESTOQUE) ");
                    sqlQuery.Append("SELECT DATE('NOW') AS 'DATA', ");
                    sqlQuery.Append("[PRODUTO_COLETA_ESTOQUE].COD_EMPREGADO, ");
                    sqlQuery.Append("[PRODUTO_COLETA_ESTOQUE].COD_PDV, ");
                    sqlQuery.Append("[PRODUTO_COLETA_ESTOQUE].COD_PRODUTO, ");
                    sqlQuery.Append("-1 AS 'COLETA', ");
                    sqlQuery.Append("0 AS 'HISTORICO', ");
                    sqlQuery.Append("-1 AS 'PERDA', ");
                    sqlQuery.Append("0 AS 'GIRO', ");
                    sqlQuery.Append("[PRODUTO_COLETA_ESTOQUE].SOM_UTM_QTD_GIRO_SELLOUT, ");
                    sqlQuery.Append("[PRODUTO_COLETA_ESTOQUE].QTD_COLETA_SELLOUT, ");
                    sqlQuery.Append("[PRODUTO_COLETA_ESTOQUE].NUM_COLETA_ESTOQUE ");
                    sqlQuery.Append("FROM [PRODUTO_COLETA_ESTOQUE] ");
                    sqlQuery.Append("JOIN [TABELA_PRECO_PRODUTO] ");
                    sqlQuery.Append("ON [PRODUTO_COLETA_ESTOQUE].[COD_PRODUTO] = [TABELA_PRECO_PRODUTO].[COD_PRODUTO] ");
                    sqlQuery.Append("JOIN [PRODUTO] ON [PRODUTO].[COD_PRODUTO] = [PRODUTO_COLETA_ESTOQUE].[COD_PRODUTO] ");
                    sqlQuery.Append("WHERE [PRODUTO_COLETA_ESTOQUE].COD_PDV = ? AND [PRODUTO_COLETA_ESTOQUE].COD_EMPREGADO = ? ");
                    //sqlQuery.Append("AND DATE([PRODUTO_COLETA_ESTOQUE].DAT_COLETA) = (SELECT DATE(MAX(DAT_COLETA)) FROM [PRODUTO_COLETA_ESTOQUE] WHERE COD_PDV = ? AND COD_EMPREGADO = ? AND IND_HISTORICO = 1) ");
                    sqlQuery.Append("AND (([PRODUTO_COLETA_ESTOQUE].NUM_COLETA_ESTOQUE = 1) OR (([PRODUTO_COLETA_ESTOQUE].SOM_UTM_QTD_GIRO_SELLOUT) > 0)) ");
                    sqlQuery.Append("AND [PRODUTO].[IND_ATIVO] = 'A' ");
                    sqlQuery.Append("GROUP BY [PRODUTO_COLETA_ESTOQUE].[COD_PRODUTO]");

                    SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", this.COD_PDV);
                    SQLiteParameter pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                    SQLiteParameter pCOD_PDV2 = new SQLiteParameter("@COD_PDV2", this.COD_PDV);
                    SQLiteParameter pCOD_EMPREGADO2 = new SQLiteParameter("@COD_EMPREGADO2", CSEmpregados.Current.COD_EMPREGADO);

                    CSDataAccess.Instance.ExecuteNonQuery(sqlQuery.ToString(), pCOD_PDV, pCOD_EMPREGADO, pCOD_PDV2, pCOD_EMPREGADO2);

                    CSDataAccess.Instance.Transacao = CSDataAccess.Instance.Connection.BeginTransaction();

                    CSDataAccess.Instance.Transacao.Commit();
                }

                catch
                {
                    CSDataAccess.Instance.Transacao.Rollback();
                }

                finally
                {
                    CSDataAccess.Instance.Transacao.Dispose();
                    CSDataAccess.Instance.Transacao = null;
                }
            }

            /// <summary>
            /// Método responsável para preencher a tabela de coleta de estoque quando o PDV estiver fazendo a primeira coleta
            /// </summary>
            /// <returns></returns>
            public void PreencherColetaParaPDVAtual(bool PrimeiraColeta)
            {
                try
                {
                    StringBuilder sqlQuery = new StringBuilder();

                    sqlQuery.Length = 0;
                    sqlQuery.Append("INSERT INTO [PRODUTO_COLETA_ESTOQUE] ");
                    sqlQuery.Append("(DAT_COLETA, ");
                    sqlQuery.Append("COD_EMPREGADO, ");
                    sqlQuery.Append("COD_PDV, ");
                    sqlQuery.Append("COD_PRODUTO, ");
                    sqlQuery.Append("QTD_COLETADA, ");
                    sqlQuery.Append("IND_HISTORICO, ");
                    sqlQuery.Append("QTD_PERDA, ");
                    sqlQuery.Append("QTD_GIRO_SELLOUT, ");
                    sqlQuery.Append("SOM_UTM_QTD_GIRO_SELLOUT, ");
                    sqlQuery.Append("QTD_COLETA_SELLOUT) ");
                    sqlQuery.Append("SELECT DATE('NOW') AS 'DATA', ");
                    sqlQuery.Append("? AS 'EMPREGADO', ");
                    sqlQuery.Append("? AS 'PDV', ");
                    sqlQuery.Append("[PRODUTO].COD_PRODUTO, ");
                    sqlQuery.Append("-1 AS 'COLETA', ");
                    sqlQuery.Append("0 AS 'HISTORICO', ");
                    sqlQuery.Append("-1 AS 'PERDA', ");
                    sqlQuery.Append("0 AS 'SELLOUT', ");
                    sqlQuery.Append("0 AS 'SOMASELLOUT', ");
                    sqlQuery.Append("0 AS 'QTD_SELLOUT' ");
                    sqlQuery.Append("FROM [PEDIDO] ");
                    sqlQuery.Append("JOIN [ITEM_PEDIDO] ");
                    sqlQuery.Append("ON [PEDIDO].[COD_PEDIDO] = [ITEM_PEDIDO].[COD_PEDIDO] ");
                    sqlQuery.Append("JOIN [PRODUTO] ");
                    sqlQuery.Append("ON [PRODUTO].[COD_PRODUTO] = [ITEM_PEDIDO].[COD_PRODUTO] ");
                    sqlQuery.Append("JOIN [TABELA_PRECO_PRODUTO] ");
                    sqlQuery.Append("ON [PRODUTO].[COD_PRODUTO] = [TABELA_PRECO_PRODUTO].[COD_PRODUTO] ");
                    sqlQuery.Append("WHERE [PEDIDO].[IND_HISTORICO] = 1 ");
                    sqlQuery.Append("AND (JULIANDAY(DATE('NOW')) - JULIANDAY([PEDIDO].DAT_PEDIDO)) <= 90 ");
                    sqlQuery.Append("AND [PEDIDO].[COD_PDV] = ? ");
                    sqlQuery.Append("AND [PRODUTO].[IND_ATIVO] = 'A' ");

                    if (!PrimeiraColeta)
                        sqlQuery.Append("AND [ITEM_PEDIDO].[COD_PRODUTO] NOT IN(SELECT [COD_PRODUTO] FROM [PRODUTO_COLETA_ESTOQUE] WHERE [COD_PDV] = ? AND DATE([DAT_COLETA]) = DATE('NOW'))");

                    sqlQuery.Append("GROUP BY [PRODUTO].COD_PRODUTO ");
                    sqlQuery.Append("ORDER BY [PRODUTO].COD_PRODUTO");

                    SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", this.COD_PDV);
                    SQLiteParameter pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                    SQLiteParameter pCOD_PDV2 = new SQLiteParameter("@COD_PDV2", this.COD_PDV);
                    SQLiteParameter pCOD_PDV3 = new SQLiteParameter("@COD_PDV3", this.COD_PDV);

                    CSDataAccess.Instance.ExecuteNonQuery(sqlQuery.ToString(), pCOD_EMPREGADO, pCOD_PDV, pCOD_PDV2, pCOD_PDV3);

                    CSDataAccess.Instance.Transacao = CSDataAccess.Instance.Connection.BeginTransaction();

                    CSDataAccess.Instance.Transacao.Commit();
                }

                catch
                {
                    CSDataAccess.Instance.Transacao.Rollback();
                }

                finally
                {
                    CSDataAccess.Instance.Transacao.Dispose();
                    CSDataAccess.Instance.Transacao = null;
                }
            }

            public bool EmpregadoDentroPdv()
            {
                if (CSEmpregados.Current.IND_VALIDAR_LOCALIZACAO &&
                this.IND_PDV_LOCALIZACAO_VERIFICADA &&
                !string.IsNullOrEmpty(this.NUM_LATITUDE_LOCALIZACAO))
                {
                    if (CalculoDistancia() > CSEmpresa.Current.NUM_RAIO_LOCALIZACAO)
                    {
                        return false;
                    }
                }

                return true;
            }

            public double CalculoDistancia()
            {
                double metros = 0;

                double raio = 6371;

                double latitude1 = Convert.ToDouble(this.NUM_LATITUDE_LOCALIZACAO);
                double longitude1 = Convert.ToDouble(this.NUM_LONGITUDE_LOCALIZACAO);
                double latitude2 = Convert.ToDouble(CSGlobal.GetLatitudeFlexxGPS());
                double longitude2 = Convert.ToDouble(CSGlobal.GetLongitudeFlexxGPS());

                double distanciaLatitude = ((latitude2 - latitude1) * Math.PI) / 180;
                double distanciaLongitude = ((longitude2 - longitude1) * Math.PI) / 180;

                double a = Math.Sin(distanciaLatitude / 2) * Math.Sin(distanciaLatitude / 2) + Math.Cos((latitude1 * Math.PI) / 180) * Math.Cos((latitude2 * Math.PI) / 180) * Math.Sin(distanciaLongitude / 2) * Math.Sin(distanciaLongitude / 2);
                double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

                metros = Math.Round(raio * c * 1000, 0);
                return metros;
            }

            /// <summary>
            /// Retorna se o PDV está realizando a primeira coleta
            /// </summary>
            /// <returns></returns>
            public bool PrimeiraColeta()
            {
                bool primeiraColeta = false;

                StringBuilder sqlQuery = new StringBuilder();

                sqlQuery.Length = 0;
                sqlQuery.Append("SELECT COUNT(*) FROM [PRODUTO_COLETA_ESTOQUE] WHERE [COD_PDV] = ?");

                SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", this.COD_PDV);

                using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(false, sqlQuery.ToString(), pCOD_PDV))
                {
                    while (reader.Read())
                    {
                        primeiraColeta = reader.GetInt32(0) == 0;
                        break;
                    }

                    reader.Close();
                    reader.Dispose();
                }

                return primeiraColeta;
            }

            /// <summary>
            /// Retorna se já existem produtos presentes na coleta de estoque
            /// </summary>
            /// <returns></returns>
            public bool ExistemProdutosComDataAtual()
            {
                bool existeProdutos = false;

                StringBuilder sqlQuery = new StringBuilder();

                sqlQuery.Length = 0;
                sqlQuery.Append("SELECT COUNT(*) FROM [PRODUTO_COLETA_ESTOQUE] WHERE [COD_PDV] = ? AND DATE(DAT_COLETA) = DATE('NOW')");

                SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", this.COD_PDV);

                using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(false, sqlQuery.ToString(), pCOD_PDV))
                {
                    while (reader.Read())
                    {
                        existeProdutos = reader.GetInt32(0) > 0;
                        break;
                    }

                    reader.Close();
                    reader.Dispose();
                }

                return existeProdutos;
            }

            /// <summary>
            /// Verifica se existe alguma operação com o código CFO igual a troca
            /// </summary>
            /// <returns></returns>
            public bool PossuiOperacaoTroca()
            {
                bool PossuiOperacao = false;

                string sqlQuery = "SELECT COUNT(*) FROM [OPERACAO] WHERE [COD_OPERACAO_CFO] = 6";

                // Busca todas as operações
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery))
                {
                    while (sqlReader.Read())
                    {
                        PossuiOperacao = sqlReader.GetInt32(0) > 0;
                        break;
                    }

                    sqlReader.Close();
                    sqlReader.Dispose();
                }

                return PossuiOperacao;
            }

            /// <summary>
            /// Verifica se é permitido venda de combo para o PDV ou a Categoria
            /// </summary>
            /// <param name="cod_produto"></param>
            /// <returns></returns>
            public bool PermiteVendaCombo(int cod_produto)
            {

                bool achouPdv = false;
                bool achouCategoria = false;
                bool permite = false;
                StringBuilder sqlQuery = null;

                sqlQuery = new StringBuilder();

                sqlQuery.Length = 0;
                sqlQuery.Append("  SELECT COD_PDV ");
                sqlQuery.Append("    FROM PRODUTO_REGRA_COMBO ");
                sqlQuery.Append("   WHERE COD_PRODUTO = ? ");
                sqlQuery.Append("     AND COD_PDV = ? ");

                SQLiteParameter pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", cod_produto);
                SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", this.COD_PDV);

                using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString(), pCOD_PRODUTO, pCOD_PDV))
                {
                    while (reader.Read())
                    {
                        achouPdv = true;
                        permite = true;
                        break;
                    }

                    reader.Close();
                    reader.Dispose();
                }

                if (!achouPdv)
                {

                    sqlQuery.Length = 0;
                    sqlQuery.Append("  SELECT COD_CATEGORIA_RESTRICAO ");
                    sqlQuery.Append("    FROM PRODUTO_REGRA_COMBO ");
                    sqlQuery.Append("   WHERE COD_PRODUTO = ? ");
                    sqlQuery.Append("     AND COD_CATEGORIA_RESTRICAO = ? ");

                    SQLiteParameter pCOD_PRODUTO2 = new SQLiteParameter("@COD_PRODUTO", cod_produto);
                    SQLiteParameter pCOD_CATEGORIA = new SQLiteParameter("@COD_CATEGORIA", this.COD_CATEGORIA);

                    using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(false, sqlQuery.ToString(), pCOD_PRODUTO2, pCOD_CATEGORIA))
                    {
                        while (reader.Read())
                        {
                            achouCategoria = true;
                            break;
                        }

                        reader.Close();
                        reader.Dispose();
                    }

                    if (!achouCategoria)
                        permite = true;
                }

                return permite;
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

            /// <summary>
            /// Retorna documentos a receber do PDV e da empresa corrente
            /// </summary>
            public CSDocumentosReceberPDV GetDocumentosAReceber(string codigoRevenda)
            {
                return new CSDocumentosReceberPDV(COD_PDV, codigoRevenda, false);
            }

            /// <summary>
            /// Retorna documentos a receber do PDV consolidados
            /// </summary>
            public CSDocumentosReceberPDV GetDocumentosAReceberConsolidado()
            {
                return new CSDocumentosReceberPDV(COD_PDV, null, true);
            }

            /// <summary>
            /// Persiste os dados em banco
            /// </summary>
            public void Flush()
            {
                string sqlQueryUpdate =
                    "UPDATE PDV " +
                    "   SET IND_POSITIVADO = ? " +
                    " WHERE COD_PDV = ? ";

                // Da o update nos PDVs que foram modificados
                if (this.STATE == ObjectState.ALTERADO)
                {
                    // Criar os parametros de salvamento
                    SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", this.COD_PDV);
                    SQLiteParameter pIND_POSITIVADO = new SQLiteParameter("@IND_POSITIVADO", (this.IND_POSITIVADO ? 1 : 0));

                    CSDataAccess.Instance.ExecuteScalar(sqlQueryUpdate, pIND_POSITIVADO, pCOD_PDV);
                    this.STATE = ObjectState.SALVO;

                }
            }

            #endregion

            public void GravarImagemDuvidosa()
            {
                try
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendFormat("UPDATE PDV SET IND_FOTO_DUVIDOSA = 'S' WHERE COD_PDV = {0}", this.COD_PDV);

                    CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            public void GravarNomeImagem()
            {
                try
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendFormat("UPDATE PDV SET DSC_NOME_FOTO = '{0}',BOL_FOTO_VALIDADA = 0,NUM_LATITUDE_FOTO = '{1}',NUM_LONGITUDE_FOTO = '{2}' WHERE COD_PDV = {3}",
                                                                                                                            this.DSC_NOME_FOTO,
                                                                                                                            this.NUM_LATITUDE_FOTO,
                                                                                                                            this.NUM_LONGITUDE_FOTO,
                                                                                                                            this.COD_PDV);

                    CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            public void ResetarNomeImagem()
            {
                try
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendFormat("UPDATE PDV SET DSC_NOME_FOTO = '',BOL_FOTO_VALIDADA = 0 WHERE COD_PDV = {0}", this.COD_PDV);

                    CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            public void GravarImagemValidada()
            {
                try
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendFormat("UPDATE PDV SET BOL_FOTO_VALIDADA = 1 WHERE COD_PDV = {0}", this.COD_PDV);

                    CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
        #endregion
    }
}