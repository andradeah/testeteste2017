#region Using directives

using System;
using System.Data;
using System.Data.SqlTypes;
using System.Text;
using System.IO;
using System.Collections;
using AvanteSales.SystemFramework;
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

namespace AvanteSales.BusinessRules
{
    /// <summary>
    /// Classe responsável pelo cálculo do preço do produto na nova política de preços broker
    /// </summary>
    public class CSPoliticaBroker
    {
        #region [ Variáveis ]

        /// <summary>
        /// Classe responsável por armazenar as variáveis básicas para cálculo do preço
        /// </summary>
        private class TmpVariaveis
        {
            public string CLIENTE_SOLDTO;
            public string CLIENTE_SHIPTO;
            public DateTime DATA;
            public string PRODUTO;
            public string CDGER0;
            public string CDCANDISTR;
            public string CDREGFISCACLI;
            public string CDREGFISCAFIL;
            public string CDCLASFISC;
            public string IDCLIIMP;
            public string IDPRDIMP;
            public string IDISENICMS;
            public string CDGRPPRC;
            public string CDGRPPRCCL;
            public string CDGRPPRD;
            public string CDCLASCLI;
            public string CDTPNEG;
            public string CDPONTOVEN;
            public string CDMERCADO;
            public string CDNEGOCIO;
            public string CDCATEG;
            public string CDSUBCATEG;
            public string CDSEGMENTO;
            public string CDCLIN4;
            public string CDCLIN5;
            public string CDCLIN6;
            public string CONDPAGTO;
            public string CDGRCSUTR;
            public string IDISENIPI;
            public string CDAGPRDESC;
            public decimal PESOTOTAL;
            public string CDREGFISCACLICOMP;
            public string CDREGFISCAFILCOMP;
            public string CDFILFAT;

        }

        /// <summary>
        /// Classe responsável por armazenar os passos do cálculo
        /// </summary>
        public class TmpPricingCons
        {
            public int NMCONDLIN;
            public string CDCONDTYP;
            public decimal DADO;
            public decimal VALOR;
            public string CDCONDUNID;
        }

        public class Sequencia
        {
            public int NMCONDLIN;
            public string CDCONDTYP;
            public int NMCONDINI;
            public int NMCONDFIM;
            public string CDCONDSEQ;
            public string DSFORMULA;
            public int NMSEQTAB;
            public string DSTABELA;
        }

        private DateTime data;
        private int clienteSoldto;
        private int clienteShipto;
        private string condicaoPagamento = "";
        private string m_COD_GER_BROKER = "";
        private bool m_INDENIZACAO = false;

        private int produto;
        private string grupoComercializacao;
        private int quantidadeInteira;
        private int quantidadeFracao;
        private decimal percentualDesconto;
        private int quantidadeCaixa;

        private StringBuilder sqlQuery = null;

        private int NMCONDLIN;
        private string CDCONDTYP = "";
        private int NMCONDINI;
        private int NMCONDFIM;
        private string CDCONDSEQ = "";
        private string DSFORMULA = "";
        private int NMSEQTAB;
        private string DSTABELA = "";

        private int RETORNOULINTABANT_SEQUENCE;
        private int ROWCOUNT;

        private decimal VALOR;
        private string CDCONDUNID = "";
        private decimal VLLIMINF;
        private decimal VLLIMSUP;
        private decimal? VALORRET;

        private string CDCONDSEQ_ANT = "";
        private string CDCONDTYP_ANT = "";
        private string CDCONDUNID_ANT = "";
        private int NMCONDLIN_ANT;
        private string DSFORMULA_ANT = "";
        private int NMCONDINI_ANT;
        private int NMCONDFIM_ANT;
        private int FATORFRACAO;

        private TmpPricingCons[] tmpPricingCons = null;
        private TmpVariaveis tmpVariaveis;
        private bool sucesso;
        private int iteration;
        private decimal descontoMaximo;
        private int maiorNMCONDLIN = 0;

        private static ArrayList arraySequencia = null;
        private static ArrayList arrayNMCONDLIN = null;
        private static int currentSequencia = 0;

#if TRACE && !ANDROID
        private StreamWriter streamLog = null;
        private DateTime h1;
#endif

        #endregion

        #region [ Propriedades ]

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
        public bool INDENIZACAO
        {
            get
            {
                return m_INDENIZACAO;
            }
            set
            {
                m_INDENIZACAO = value;
            }
        }
        #endregion

        #region [ Métodos Públicos ]

        public CSPoliticaBroker(DateTime data, int clienteSoldto, int clienteShipto, string condicaoPagamento)
        {
            this.data = data;
            this.clienteSoldto = clienteSoldto;
            this.clienteShipto = clienteShipto;
            this.condicaoPagamento = condicaoPagamento;

            this.produto = -1;
            this.grupoComercializacao = "";
            this.quantidadeInteira = -1;
            this.quantidadeFracao = -1;
            this.percentualDesconto = -1;
            this.quantidadeCaixa = -1;
            this.descontoMaximo = -1;

            this.sucesso = false;

            sqlQuery = new StringBuilder();

#if TRACE && !ANDROID
            h1 = DateTime.Now;
            streamLog = new StreamWriter("Prep - " + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt");
            streamLog.WriteLine(DateTime.Now.Subtract(h1).ToString() + " - Preprocessamento");
            streamLog.Flush();
#endif

            // [ Executa as regras associadas apenas ao cliente para agilizar o cálculo dos preços ]
            PreprocessaCliente();

#if TRACE && !ANDROID
            streamLog.Flush();
            streamLog.Close();
#endif
        }

        /// <summary>
        /// Realiza o cálculo do preço
        /// </summary>
        /// <param name="produto">Produto</param>
        /// <param name="quantidadeInteira">Quantidade inteira</param>
        /// <param name="quantidadeFracao">Quantidade fracionaria</param>
        /// <param name="percentualDesconto">Percentual de desconto</param>
        /// <param name="quantidadeCaixa">Quantidade de unidades por caixa</param>
        /// <returns>Array com os passos do cálculo do preço</returns>
        public TmpPricingCons[] CalculaPreco(int produto, string grupoComercializacao, int quantidadeInteira, int quantidadeFracao, decimal percentualDesconto, int quantidadeCaixa)
        {
            bool loop;
            bool mesmoProduto;
            iteration = 0;

            // [ Verifica se é indenização]
            if (m_INDENIZACAO == true)
            {
                quantidadeFracao = quantidadeFracao + (quantidadeInteira * quantidadeCaixa);
                quantidadeInteira = 0;

            }

            // [ Tratamento temporário para atender à mudança de códigos de produtos da nestle ]
            if (produto < 41000)
                produto += 410000;

            // [ Verifica se está repetindo o mesmo cálculo... ]
            if (this.produto == produto && this.grupoComercializacao == grupoComercializacao &&
                    this.quantidadeInteira == quantidadeInteira && this.quantidadeFracao == quantidadeFracao &&
                    this.percentualDesconto == percentualDesconto && this.quantidadeCaixa == quantidadeCaixa && sucesso)
            {
                return tmpPricingCons;
            }

            // [ Verifica se o cálculo é sobre o mesmo produto anterior ]
            mesmoProduto = (this.produto == produto && this.grupoComercializacao == grupoComercializacao);

            this.produto = produto;
            this.grupoComercializacao = grupoComercializacao;
            this.quantidadeInteira = quantidadeInteira;
            this.quantidadeFracao = quantidadeFracao;
            this.percentualDesconto = percentualDesconto;
            this.quantidadeCaixa = quantidadeCaixa;

#if TRACE && !ANDROID
            h1 = DateTime.Now;
            streamLog = new StreamWriter("Calc" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt");
            streamLog.WriteLine(DateTime.Now.Subtract(h1).ToString() + " - Inicializa");
            streamLog.Flush();
#endif

            try
            {
                // [ Verifica produto informado ]
                if (!mesmoProduto || !sucesso)
                {
#if TRACE && !ANDROID
                    h1 = DateTime.Now;
#endif

                    // [ Popula tabela de variáveis para os cálculos ]
                    InicializaTabelaVariaveis(produto, grupoComercializacao);

#if TRACE && !ANDROID
                    streamLog.WriteLine(DateTime.Now.Subtract(h1).ToString() + " - Variaveis");
                    streamLog.Flush();
#endif

                    descontoMaximo = -1;

                    // [ Descarta cache de comandos ]
                    CSDataAccess.Instance.DisposePreparedCommands(TypedHashtable.HashtableEntryType.Temporary);

                    // [ Descarta resultados que são associados ao produto ]
                    CSDataAccess.Instance.DisposeCachedResults(TypedHashtable.HashtableEntryType.Temporary);
                }

                // [ Inicializa vetor de resultados dos cálculos ]
                InicializaArrayCalculos();

                // [ Recupera array com a sequência do cálculo ]
                PreparaSequencia();

                RETORNOULINTABANT_SEQUENCE = 0;

                loop = LeSequencia();
                while (loop)
                {
                    if (RETORNOULINTABANT_SEQUENCE != 0)
                    {
#if TRACE && !ANDROID
                        h1 = DateTime.Now;
#endif
                        loop = ExecutaNovaCondition();

#if TRACE && !ANDROID
                        streamLog.WriteLine(DateTime.Now.Subtract(h1).ToString() + " - NOVAC");
                        streamLog.Flush();
#endif

                    }
                    else
                    {
#if TRACE && !ANDROID
                        h1 = DateTime.Now;
#endif
                        ExecutaRegra(DSTABELA);

#if TRACE && !ANDROID
                        h1 = DateTime.Now;
#endif
                        loop = ExecutaNovaCondition();

#if TRACE && !ANDROID
                        streamLog.WriteLine(DateTime.Now.Subtract(h1).ToString() + " - NOVAC");
                        streamLog.Flush();
#endif
                    }

                    iteration++;
                }
                sucesso = true;

                // Klelbio
                //horaStop = DateTime.Now;
                //CSGlobal.ShowMessage("Tempo (loop) : " + horaStop.Subtract(horaStart).ToString());                

#if TRACE && !ANDROID
                streamLog.Flush();
                streamLog.Close();
#endif

                return tmpPricingCons;

            }
            catch (Exception e)
            {
                sucesso = false;
                throw new Exception(e.Message + "\nLoop: " + iteration, e);
            }
        }

        /// <summary>
        /// Calcula do valor do desconto máximo permitido para o produto
        /// </summary>
        /// <returns>Decimal com o valor máximo do desconto</returns>
        public decimal CalculaDescontoMaximo()
        {
            try
            {
                if (descontoMaximo == -1)
                {
                    sqlQuery.Length = 0;
                    sqlQuery.Append("SELECT T1.VLLIMINF AS R1, T2.VLLIMINF AS R2, T3.VLLIMINF AS R3 ");
                    sqlQuery.Append("      ,CASE T1.CDCLIH WHEN T0.CDCLIN4 THEN 4 ");
                    sqlQuery.Append("                      WHEN T0.CDCLIN5 THEN 3 ");
                    sqlQuery.Append("                      WHEN T0.CDCLIN6 THEN 2 ");
                    sqlQuery.Append("                      WHEN T0.CDCLI THEN 1 ");
                    sqlQuery.Append("       END AS CDCLIH_906 ");
                    sqlQuery.Append("     ,CASE T2.CDCLIH WHEN T0.CDCLIN4 THEN 4 ");
                    sqlQuery.Append("                     WHEN T0.CDCLIN5 THEN 3 ");
                    sqlQuery.Append("                     WHEN T0.CDCLIN6 THEN 2 ");
                    sqlQuery.Append("                     WHEN T0.CDCLI THEN 1 ");
                    sqlQuery.Append("      END AS CDCLIH_905 ");
                    sqlQuery.Append("     ,CASE T3.CDCLIH WHEN T0.CDCLIN4 THEN 4 ");
                    sqlQuery.Append("                     WHEN T0.CDCLIN5 THEN 3 ");
                    sqlQuery.Append("                     WHEN T0.CDCLIN6 THEN 2 ");
                    sqlQuery.Append("                     WHEN T0.CDCLI THEN 1 ");
                    sqlQuery.Append("      END AS CDCLIH_924 ");
                    sqlQuery.Append("  FROM BRK_ECLIENTE T0 ");
                    sqlQuery.Append("  JOIN BRK_EPRODORG T4 ");
                    sqlQuery.Append("       ON T4.CDPRD = ? AND T4.CDGER0 = T0.CDGER0 AND ");
                    sqlQuery.Append("          T4.CDCANDISTR = ? AND T4.CDDIVISAO = '00' ");
                    sqlQuery.Append("  LEFT JOIN BRK_EPRECOA906 T1 ");
                    sqlQuery.Append("       ON T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = 'X066' AND ");
                    sqlQuery.Append("          T1.CDGER0 = T0.CDGER0 AND T1.CDCANDISTR = ? AND ");
                    sqlQuery.Append("       T1.CDCLIH IN (T0.CDCLIN4,T0.CDCLIN5,T0.CDCLIN6,T0.CDCLI) AND ");
                    sqlQuery.Append("          T1.CDPRD = T4.CDPRD AND ");
                    sqlQuery.Append("       ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
                    sqlQuery.Append("  LEFT JOIN BRK_EPRECOA905 T2 ");
                    sqlQuery.Append("       ON T2.CDAPLICAC = 'V' AND T2.CDCONDTYP = 'X066' AND ");
                    sqlQuery.Append("          T2.CDGER0 = T0.CDGER0 AND T2.CDCANDISTR = ? AND ");
                    sqlQuery.Append("      T2.CDCLIH IN (T0.CDCLIN4,T0.CDCLIN5,T0.CDCLIN6,T0.CDCLI) AND ");
                    sqlQuery.Append("          T2.CDGRPPRD = T4.CDGRPPRD AND ");
                    sqlQuery.Append("       ? BETWEEN T2.DTVIGINI AND T2.DTVIGFIM ");
                    sqlQuery.Append("  LEFT JOIN BRK_EPRECOA924 T3 ");
                    sqlQuery.Append("       ON T3.CDAPLICAC = 'V' AND T3.CDCONDTYP = 'X066' AND ");
                    sqlQuery.Append("          T3.CDGER0 = T0.CDGER0 AND T3.CDCANDISTR = ? AND ");
                    sqlQuery.Append("       T3.CDCLIH IN (T0.CDCLIN4,T0.CDCLIN5,T0.CDCLIN6,T0.CDCLI) AND ");
                    sqlQuery.Append("          T3.CDMERCADO IN (T4.CDMERCADO, '') AND T3.CDNEGOCIO IN (T4.CDNEGOCIO, '') AND ");
                    sqlQuery.Append("          T3.CDCATEG IN (T4.CDCATEG, '') AND T3.CDSUBCATEG IN (T4.CDSUBCATEG, '') AND ");
                    sqlQuery.Append("          T3.CDSEGMENTO IN (T4.CDSEGMENTO, '') AND ");
                    sqlQuery.Append("       ? BETWEEN T3.DTVIGINI AND T3.DTVIGFIM ");
                    sqlQuery.Append(" WHERE T0.CDCLI = ? ");
                    sqlQuery.Append("   AND T0.CDGER0 = ? ");
                    sqlQuery.Append(" ORDER BY CDCLIH_906, CDCLIH_905, CDCLIH_924, T1.NMCONDREC DESC, T2.NMCONDREC DESC, IFNULL(T3.CDSEGMENTO,' ') DESC, IFNULL(T3.CDSUBCATEG,' ') DESC,IFNULL(T3.CDCATEG,' ') DESC,IFNULL(T3.CDNEGOCIO,' ') DESC,IFNULL(T3.CDMERCADO,' ') DESC,IFNULL(T3.NMCONDREC,' ') DESC ");
                    //sqlQuery.Append(" ORDER BY CDCLIH_906, CDCLIH_905, CDCLIH_924, T1.NMCONDREC DESC, T2.NMCONDREC DESC, T3.NMCONDREC DESC ");

                    SQLiteParameter[] parametros = new SQLiteParameter[10];

                    parametros[00] = new SQLiteParameter("@01PRODUTO", tmpVariaveis.PRODUTO);
                    parametros[01] = new SQLiteParameter("@02CDCANDISTR", tmpVariaveis.CDCANDISTR);
                    parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
                    parametros[03] = new SQLiteParameter("@04DATA", tmpVariaveis.DATA);
                    parametros[04] = new SQLiteParameter("@05CDCANDISTR", tmpVariaveis.CDCANDISTR);
                    parametros[05] = new SQLiteParameter("@06DATA", tmpVariaveis.DATA);
                    parametros[06] = new SQLiteParameter("@07CDCANDISTR", tmpVariaveis.CDCANDISTR);
                    parametros[07] = new SQLiteParameter("@08DATA", tmpVariaveis.DATA);
                    parametros[08] = new SQLiteParameter("@09CLIENTE_SHIPTO", tmpVariaveis.CLIENTE_SHIPTO);
                    parametros[09] = new SQLiteParameter("@10CDGER0", tmpVariaveis.CDGER0);

                    using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(true, sqlQuery.ToString(), parametros))
                    {
                        decimal result = 0;

                        if (reader.Read())
                        {
                            if (reader.GetValue(0) != System.DBNull.Value)
                                result = Convert.ToDecimal(reader.GetValue(0));
                            else if (reader.GetValue(1) != System.DBNull.Value)
                                result = Convert.ToDecimal(reader.GetValue(1));
                            else if (reader.GetValue(2) != System.DBNull.Value)
                                result = Convert.ToDecimal(reader.GetValue(2));
                        }
                        reader.Close();
                        reader.Dispose();

                        descontoMaximo = (result * -1) / 1000;
                    }
                }

                return descontoMaximo;

            }
            catch
            {
                sucesso = false;
                throw new Exception("Falha ao calcular desconto máximo!");
            }
        }

        #endregion

        #region [ Métodos Privados ]

        private void InicializaArrayCalculos()
        {
            // [ Inicializa array se necessário ]
            if (tmpPricingCons == null)
            {
                int size = 0;
                int nmcondlin_aux = 0;
                arrayNMCONDLIN = new ArrayList();

                sqlQuery.Length = 0;
                sqlQuery.Append("SELECT NMCONDLIN ");
                sqlQuery.Append("  FROM BRK_EPRECOFORM ");
                sqlQuery.Append(" ORDER BY NMCONDLIN ");

                using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString()))
                {
                    while (reader.Read())
                    {
                        nmcondlin_aux = (reader.GetValue(0) == System.DBNull.Value) ? 0 : reader.GetInt32(0);
                        arrayNMCONDLIN.Add(nmcondlin_aux);
                        maiorNMCONDLIN = nmcondlin_aux;
                        size++;

                    }
                    reader.Close();
                    reader.Dispose();
                }

                // [ verifica o tamanho do array necessário ]                
                tmpPricingCons = new TmpPricingCons[size];

            }
            // [ Zera todas as posições ]
            for (int i = 0; i < tmpPricingCons.Length; i++)
                tmpPricingCons[i] = null;
        }
        /// <summary>
        /// Retorna localização de uma NMCONDLIN no vetor
        /// </summary>
        /// <param name="nmcondlin_aux">NMCONDLIN</param>
        /// <returns>int com a localização da NMCONDLIN no vetor ou -1 quando não encontra</returns>
        private int RetornaLocalizacaoNMCONDLIN(int nmcondlin_aux)
        {
            int result = -1;

            if (arrayNMCONDLIN != null)
                result = arrayNMCONDLIN.IndexOf(nmcondlin_aux);

            return result;
        }

        /// <summary>
        /// Realiza a soma das posições do array
        /// </summary>
        /// <param name="posIni">Posição inicial</param>
        /// <param name="posFim">Posição final</param>
        /// <returns>decimal com o resultado da soma</returns>
        private decimal? GetSumTmpPricingCons(int posIni, int posFim)
        {
            decimal? result = 0;
            int tmpLocalizacao = 0;
            bool hasValue = false;

            // [ Valida índices inválidos ]
            if (posIni < 0)
                posIni = 0;

            if (posFim < 0)
                posFim = 0;

            if (posFim > maiorNMCONDLIN)
                posFim = maiorNMCONDLIN;

            if (posIni > posFim && posIni < maiorNMCONDLIN)
                posFim = posIni;

            // [ Realiza a soma ]
            for (int i = posIni; i <= posFim; i++)
            {
                // [ Acha a localização no vetor ]
                tmpLocalizacao = RetornaLocalizacaoNMCONDLIN(i);

                if (tmpLocalizacao != -1)
                {
                    if (tmpPricingCons[tmpLocalizacao] != null)
                    {
                        hasValue = true;
                        result += tmpPricingCons[tmpLocalizacao].VALOR;
                    }
                }
            }

            try
            {
                if (hasValue)
                {
                    return decimal.Round(result.Value, 2);
                }
                else
                    return null;

            }
            //catch (OverflowException)
            //{
            //    string strResultado = result.ToString();

            //    if (strResultado.Contains("."))
            //        strResultado = strResultado.Substring(0, 29);
            //    else
            //        strResultado = strResultado.Substring(0, 28);

            //    decimal resultado = new decimal(decimal.Parse(strResultado, new System.Globalization.CultureInfo("en-US")));

            //    return (hasValue ? decimal.ConvertToPrecScale(resultado, 11, 2) : decimal.Null);
            //}
            //catch (SqlTruncateException)
            //{
            //    string strResultado = result.ToString();

            //    if (strResultado.Contains("."))
            //    {
            //        if (strResultado.Length > 29)
            //            strResultado = strResultado.Substring(0, 29);
            //    }
            //    else
            //    {
            //        if (strResultado.Length > 28)
            //            strResultado = strResultado.Substring(0, 28);
            //    }

            //    decimal resultado = new decimal(decimal.Parse(strResultado, new System.Globalization.CultureInfo("en-US")));

            //    return (hasValue ? decimal.ConvertToPrecScale(resultado, 11, 2) : decimal.Null);
            //}
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private decimal GetDadoTmpPricingCons(int posIni)
        {
            decimal result = 0;

            // [ Acha a localização no vetor ]
            posIni = RetornaLocalizacaoNMCONDLIN(posIni);

            // [ Valida índices inválidos ]
            if (posIni > 0 && posIni <= maiorNMCONDLIN)
            {
                // [ Retorna dado ]
                if (tmpPricingCons[posIni] != null)
                {
                    result = tmpPricingCons[posIni].DADO;

                    if (tmpPricingCons[posIni].CDCONDUNID == "%")
                    {
                        result = (tmpPricingCons[posIni].DADO / 100m);
                    }
                }
            }
            return decimal.Round(result, 4);
        }

        private decimal? GetSumTmpPricingCons(int posFim1, int posIni2, int posFim2, int posIni3, int posFim3)
        {
            decimal? result = null;
            decimal? r;

            if (NMCONDINI_ANT == 0 && NMCONDFIM_ANT == 0)
            {
                r = GetSumTmpPricingCons(1, posFim1);
                if (r.HasValue)
                    result = r;
            }

            if (NMCONDINI_ANT > 0 && NMCONDFIM_ANT > 0)
            {
                r = GetSumTmpPricingCons(posIni2, posFim2);
                if (r.HasValue)
                    result = (result.HasValue ? r + result : r);
            }

            if (NMCONDINI_ANT > 0 && NMCONDFIM_ANT == 0)
            {
                if (CDCONDUNID == "BRL")
                    r = GetSumTmpPricingCons(NMCONDLIN, NMCONDLIN);
                else
                    r = GetSumTmpPricingCons(posIni3, posFim3);
                if (r.HasValue)
                    result = (result.HasValue ? r + result : r);
            }

            return result;
        }

        /// <summary>
        /// Seta posição no array de resultados dos cálculos
        /// </summary>
        /// <param name="tmpNMCONDLIN"></param>
        /// <param name="tmpCDCONDTYP"></param>
        /// <param name="tmpDADO"></param>
        /// <param name="tmpVALOR"></param>
        private void SetPosTmpPricingCons(int tmpNMCONDLIN, string tmpCDCONDTYP, string tmpCDCONDUNID, decimal tmpDADO, decimal? tmpVALOR)
        {
            TmpPricingCons item = null;

            // [ Acha a localização no vetor ]
            int tmpLocalizacao = RetornaLocalizacaoNMCONDLIN(tmpNMCONDLIN);

            if (!tmpVALOR.HasValue)
                tmpVALOR = 0m;

            if (tmpLocalizacao != -1)
            {
                if (tmpPricingCons[tmpLocalizacao] == null)
                {
                    item = new TmpPricingCons();
                    tmpPricingCons[tmpLocalizacao] = item;
                    item.DADO = 0m;
                    item.VALOR = 0m;

                }
                else
                {
                    item = tmpPricingCons[tmpLocalizacao];
                }
            }

            item.NMCONDLIN = tmpNMCONDLIN;
            item.CDCONDTYP = tmpCDCONDTYP;
            item.DADO += tmpDADO;
            item.VALOR += tmpVALOR.Value;
            item.CDCONDUNID = tmpCDCONDUNID;
        }

        /// <summary>
        /// Inicializa as variáveis básicas do cálculo do preço
        /// </summary>
        /// <param name="produto"></param>
        private void InicializaTabelaVariaveis(int produto, string grupoComercializacao)
        {
            string filialFaturadoraCliente = "";

            if (m_COD_GER_BROKER != "" && grupoComercializacao.ToUpper() != "BRG1")
                grupoComercializacao = m_COD_GER_BROKER;

            filialFaturadoraCliente = RetornaFilialFaturadoraCliente(grupoComercializacao.ToUpper());
            SQLiteParameter[] parametros = new SQLiteParameter[08];

            parametros[00] = new SQLiteParameter("@01DATA", data.ToString("yyyy-MM-dd 00:00:00"));
            parametros[01] = new SQLiteParameter("@02CONDPAGTO", condicaoPagamento);
            parametros[02] = new SQLiteParameter("@03PRODUTO", produto.ToString().PadLeft(18, '0'));
            parametros[03] = new SQLiteParameter("@04CDFILFAT", filialFaturadoraCliente);
            parametros[04] = new SQLiteParameter("@05PRODUTO", produto.ToString().PadLeft(18, '0'));
            parametros[05] = new SQLiteParameter("@06CDFILFAT", filialFaturadoraCliente);
            parametros[06] = new SQLiteParameter("@07COD_GRUPO_COMERCIALIZACAO", grupoComercializacao.ToUpper());
            parametros[07] = new SQLiteParameter("@08CLIENTE_SOLDTO", clienteSoldto.ToString().PadLeft(10, '0'));

            sqlQuery.Length = 0;
            sqlQuery.Append("  SELECT BRK_ECLIENTE.CDCLI AS CLIENTE_SOLDTO, BRK_ECLIENTE.CDCLI AS CLIENTE_SHIPTO, ? AS DATA, BRK_EPRODORG.CDPRD AS PRODUTO ");
            sqlQuery.Append("        ,BRK_ECLIENTE.CDGER0 AS CDGER0, BRK_ECLIENTE.CDCANDISTR AS CDCANDISTR ");
            sqlQuery.Append("        ,SUBSTR(BRK_ECLIENTBAS.CDREGFISCA,1,3) AS CDREGFISCACLI, SUBSTR(BRK_TFILFAT.CDREGFISCA,1,3) AS CDREGFISCAFIL ");
            sqlQuery.Append("        ,BRK_EPRODFIL.CDCLASFISC AS CDCLASFISC, BRK_ECLIIMP.IDCLIIMP AS IDCLIIMP ");
            sqlQuery.Append("        ,BRK_EPRODTPIMP.IDPRDIMP AS IDPRDIMP, BRK_ECLIENTBAS.IDISENICMS AS IDISENICMS ");
            sqlQuery.Append("        ,BRK_EPRODORG.CDGRPPRC AS CDGRPPRC, BRK_ECLIENTE.CDGRPPRCCL AS CDGRPPRCCL ");
            sqlQuery.Append("        ,BRK_EPRODORG.CDGRPPRD AS CDGRPPRD, BRK_ECLIENTBAS.CDCLASCLI AS CDCLASCLI ");
            sqlQuery.Append("        ,BRK_ECLIENTBAS.CDTPNEG AS CDTPNEG,BRK_ECLIENTBAS.CDPONTOVEN AS CDPONTOVEN ");
            sqlQuery.Append("        ,BRK_EPRODORG.CDMERCADO AS CDMERCADO, BRK_EPRODORG.CDNEGOCIO AS CDNEGOCIO ");
            sqlQuery.Append("        ,BRK_EPRODORG.CDCATEG AS CDCATEG, BRK_EPRODORG.CDSUBCATEG AS CDSUBCATEG ");
            sqlQuery.Append("        ,BRK_EPRODORG.CDSEGMENTO AS CDSEGMENTO ");
            sqlQuery.Append("        ,BRK_ECLIENTE.CDCLIN4 AS CDCLIN4, BRK_ECLIENTE.CDCLIN5 AS CDCLIN5,BRK_ECLIENTE.CDCLIN6 AS CDCLIN6 ");
            sqlQuery.Append("        ,LTRIM(?) AS CONDPAGTO,BRK_ECLIENTBAS.CDGRCSUTR AS CDGRCSUTR ");
            sqlQuery.Append("        ,BRK_ECLIENTBAS.IDISENIPI AS IDISENIPI, BRK_EPRODUTO.CDAGPRDESC AS CDAGPRDESC ");
            sqlQuery.Append("        ,ROUND(BRK_EUNIDMED.VLDENCONV,2) / ROUND(BRK_EUNIDMED.VLFATCONV,2) AS PESOTOTAL ");
            sqlQuery.Append("        ,BRK_ECLIENTBAS.CDREGFISCA AS CDREGFISCACLICOMP, BRK_TFILFAT.CDREGFISCA AS CDREGFISCAFILCOMP, BRK_TFILFAT.CDFILFAT ");
            sqlQuery.Append("    FROM BRK_ECLIENTE ");
            sqlQuery.Append("    LEFT JOIN BRK_EEXFIL ON BRK_EEXFIL.CDPRD = ? AND  BRK_EEXFIL.CDCLI = BRK_ECLIENTE.CDCLI ");
            sqlQuery.Append("    LEFT JOIN BRK_TFILFAT ON BRK_TFILFAT.CDFILFAT = ? ");
            sqlQuery.Append("    LEFT JOIN BRK_EPRODFIL ON BRK_EPRODFIL.CDPRD = ? AND BRK_EPRODFIL.CDFILFAT = ? ");
            sqlQuery.Append("    JOIN BRK_ECLIENTBAS ON BRK_ECLIENTBAS.CDCLI = BRK_ECLIENTE.CDCLI ");
            sqlQuery.Append("    LEFT JOIN BRK_ECLIIMP ON BRK_ECLIIMP.CDCLI = BRK_ECLIENTE.CDCLI AND BRK_ECLIIMP.CDCATIMP = 'IBRX' ");
            sqlQuery.Append("    LEFT JOIN BRK_EPRODTPIMP ON BRK_EPRODTPIMP.CDPRD = BRK_EPRODFIL.CDPRD AND BRK_EPRODTPIMP.CDPAIS = 'BR' ");
            sqlQuery.Append("    LEFT JOIN BRK_EPRODORG ");
            sqlQuery.Append("      ON BRK_EPRODORG.CDPRD = BRK_EPRODFIL.CDPRD ");
            sqlQuery.Append("     AND BRK_EPRODORG.CDGER0 = BRK_ECLIENTE.CDGER0 ");
            sqlQuery.Append("     AND BRK_EPRODORG.CDCANDISTR = BRK_ECLIENTE.CDCANDISTR ");
            sqlQuery.Append("     AND BRK_EPRODORG.CDDIVISAO = '00' ");
            sqlQuery.Append("     AND BRK_EPRODORG.CDGER0 = ? ");
            sqlQuery.Append("    LEFT JOIN BRK_EPRODUTO ON BRK_EPRODUTO.CDPRD = BRK_EPRODFIL.CDPRD ");
            sqlQuery.Append("    LEFT JOIN BRK_EUNIDMED ON BRK_EUNIDMED.CDPRD = BRK_EPRODFIL.CDPRD AND BRK_EUNIDMED.CDUNIDMED = 'KGM' ");
            sqlQuery.Append("   WHERE BRK_ECLIENTE.CDCLI = ? ");

            tmpVariaveis = null;

            using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(true, sqlQuery.ToString(), parametros))
            {
                while (reader.Read())
                {
                    tmpVariaveis = new TmpVariaveis();
                    tmpVariaveis.CDGER0 = (reader.GetValue(4) == System.DBNull.Value) ? "" : reader.GetString(4);

                    // [ Se o grupo de comercialização for o procurado ou está preprocessando... ]
                    if ((grupoComercializacao.Length > 0 && tmpVariaveis.CDGER0.ToUpper() == grupoComercializacao.ToUpper()) || produto == -1)
                    {
                        tmpVariaveis.CLIENTE_SOLDTO = (reader.GetValue(0) == System.DBNull.Value) ? "" : reader.GetString(0);
                        tmpVariaveis.CLIENTE_SHIPTO = (reader.GetValue(1) == System.DBNull.Value) ? "" : reader.GetString(1);
                        tmpVariaveis.DATA = (reader.GetValue(2) == System.DBNull.Value) ? DateTime.Now : reader.GetDateTime(2);
                        tmpVariaveis.PRODUTO = (reader.GetValue(3) == System.DBNull.Value) ? "" : reader.GetString(3);
                        tmpVariaveis.CDCANDISTR = (reader.GetValue(5) == System.DBNull.Value) ? "" : reader.GetString(5);
                        tmpVariaveis.CDREGFISCACLI = (reader.GetValue(6) == System.DBNull.Value) ? "" : reader.GetString(6);
                        tmpVariaveis.CDREGFISCAFIL = (reader.GetValue(7) == System.DBNull.Value) ? "" : reader.GetString(7);
                        tmpVariaveis.CDCLASFISC = (reader.GetValue(8) == System.DBNull.Value) ? "" : reader.GetString(8);
                        tmpVariaveis.IDCLIIMP = (reader.GetValue(9) == System.DBNull.Value) ? "" : reader.GetString(9);
                        tmpVariaveis.IDPRDIMP = (reader.GetValue(10) == System.DBNull.Value) ? "" : reader.GetString(10);
                        tmpVariaveis.IDISENICMS = (reader.GetValue(11) == System.DBNull.Value) ? "" : reader.GetString(11);
                        tmpVariaveis.CDGRPPRC = (reader.GetValue(12) == System.DBNull.Value) ? "" : reader.GetString(12);
                        tmpVariaveis.CDGRPPRCCL = (reader.GetValue(13) == System.DBNull.Value) ? "" : reader.GetString(13);
                        tmpVariaveis.CDGRPPRD = (reader.GetValue(14) == System.DBNull.Value) ? "" : reader.GetString(14);
                        tmpVariaveis.CDCLASCLI = (reader.GetValue(15) == System.DBNull.Value) ? "" : reader.GetString(15);
                        tmpVariaveis.CDTPNEG = (reader.GetValue(16) == System.DBNull.Value) ? "" : reader.GetString(16);
                        tmpVariaveis.CDPONTOVEN = (reader.GetValue(17) == System.DBNull.Value) ? "" : reader.GetString(17);
                        tmpVariaveis.CDMERCADO = (reader.GetValue(18) == System.DBNull.Value) ? "" : reader.GetString(18);
                        tmpVariaveis.CDNEGOCIO = (reader.GetValue(19) == System.DBNull.Value) ? "" : reader.GetString(19);
                        tmpVariaveis.CDCATEG = (reader.GetValue(20) == System.DBNull.Value) ? "" : reader.GetString(20);
                        tmpVariaveis.CDSUBCATEG = (reader.GetValue(21) == System.DBNull.Value) ? "" : reader.GetString(21);
                        tmpVariaveis.CDSEGMENTO = (reader.GetValue(22) == System.DBNull.Value) ? "" : reader.GetString(22);
                        tmpVariaveis.CDCLIN4 = (reader.GetValue(23) == System.DBNull.Value) ? "" : reader.GetString(23);
                        tmpVariaveis.CDCLIN5 = (reader.GetValue(24) == System.DBNull.Value) ? "" : reader.GetString(24);
                        tmpVariaveis.CDCLIN6 = (reader.GetValue(25) == System.DBNull.Value) ? "" : reader.GetString(25);
                        tmpVariaveis.CONDPAGTO = (reader.GetValue(26) == System.DBNull.Value) ? "" : reader.GetString(26);
                        tmpVariaveis.CDGRCSUTR = (reader.GetValue(27) == System.DBNull.Value) ? "" : reader.GetString(27);
                        tmpVariaveis.IDISENIPI = (reader.GetValue(28) == System.DBNull.Value) ? "" : reader.GetString(28);
                        tmpVariaveis.CDAGPRDESC = (reader.GetValue(29) == System.DBNull.Value) ? "" : reader.GetString(29);
                        tmpVariaveis.PESOTOTAL = (reader.GetValue(30) == System.DBNull.Value) ? 0m : Convert.ToDecimal(reader.GetValue(30));
                        tmpVariaveis.CDREGFISCACLICOMP = (reader.GetValue(31) == System.DBNull.Value) ? "" : reader.GetString(31);
                        tmpVariaveis.CDREGFISCAFILCOMP = (reader.GetValue(32) == System.DBNull.Value) ? "" : reader.GetString(32);
                        tmpVariaveis.CDFILFAT = (reader.GetValue(33) == System.DBNull.Value) ? "" : reader.GetString(33);

                        // Inicializa os campos CDCLIN4, CDCLIN5 e CDCLIN6 para zero caso não tem classificação no arquivo
                        // Pois o campo e convertido para numerico e ocorre erro de run-time quando o mesmo e passado nos
                        // parametros com espaçoes quando e inicializado a primeira parte da politica referente ao cliente.
                        tmpVariaveis.CDCLIN4 = (tmpVariaveis.CDCLIN4.Trim() == "") ? "0" : tmpVariaveis.CDCLIN4;
                        tmpVariaveis.CDCLIN5 = (tmpVariaveis.CDCLIN5.Trim() == "") ? "0" : tmpVariaveis.CDCLIN5;
                        tmpVariaveis.CDCLIN6 = (tmpVariaveis.CDCLIN6.Trim() == "") ? "0" : tmpVariaveis.CDCLIN6;

                        break;

                    }
                    else
                        tmpVariaveis = null;
                }

                reader.Close();
                reader.Dispose();

            }

            if (tmpVariaveis != null && ((produto == -1) || (produto != -1 && tmpVariaveis.PRODUTO != "" && tmpVariaveis.CLIENTE_SHIPTO != "" && tmpVariaveis.CLIENTE_SOLDTO != "")))
            {
                return;
            }

            throw new Exception("Falha ao inicializar tabela de variáveis!");
        }

        /// <summary>
        /// Retorna a filial faturadora do cliente considerando a exceção (EEXFIL)
        /// </summary>
        /// <returns></returns>
        private string RetornaFilialFaturadoraCliente(string grupoComercializacao)
        {
            string result = "";

            SQLiteParameter[] parametros = new SQLiteParameter[03];

            parametros[00] = new SQLiteParameter("@01PRODUTO", produto.ToString().PadLeft(18, '0'));
            parametros[01] = new SQLiteParameter("@02CLIENTE_SOLDTO", clienteSoldto.ToString().PadLeft(10, '0'));
            parametros[02] = new SQLiteParameter("@03COD_GRUPO_COMERCIALIZACAO", grupoComercializacao);

            sqlQuery.Length = 0;
            sqlQuery.Append(" SELECT BRK_ECLIENTE.CDFILFAT  ");
            sqlQuery.Append("       ,BRK_EEXFIL.CDFILFAT  ");
            sqlQuery.Append("   FROM BRK_ECLIENTE  ");
            sqlQuery.Append("        LEFT JOIN BRK_EEXFIL  ");
            sqlQuery.Append("          ON BRK_EEXFIL.CDCLI = BRK_ECLIENTE.CDCLI ");
            sqlQuery.Append("         AND BRK_EEXFIL.CDPRD = ? ");
            sqlQuery.Append("  WHERE BRK_ECLIENTE.CDCLI = ? ");
            sqlQuery.Append("    AND BRK_ECLIENTE.CDGER0 = ? ");

            using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(false, sqlQuery.ToString(), parametros))
            {
                if (reader.Read())
                {
                    if (reader.GetValue(1) != System.DBNull.Value)
                        result = reader.GetString(1);
                    else if (reader.GetValue(0) != System.DBNull.Value)
                        result = reader.GetString(0);
                }

                reader.Close();
                reader.Dispose();
            }

            return result;
        }

        private void PreparaSequencia()
        {
            if (arraySequencia == null)
            {
                sqlQuery.Length = 0;
                sqlQuery.Append("SELECT COUNT(*) ");
                sqlQuery.Append("  FROM BRK_EPRECOFORM T1 ");
                sqlQuery.Append("  LEFT JOIN BRK_EPRECOSEQ T2 ON T2.CDCONDSEQ = T1.CDCONDSEQ ");
                sqlQuery.Append(" WHERE ((T1.CDCONDSEQ IS NULL AND T2.DSTABELA IS NULL) OR (T2.DSTABELA IS NOT NULL) OR (T1.CDCONDSEQ = '')) ");

                // [ verifica o tamanho do array necessário ]
                int size = int.Parse(CSDataAccess.Instance.ExecuteScalar(sqlQuery.ToString()).ToString());

                sqlQuery.Length = 0;
                sqlQuery.Append("SELECT T1.NMCONDLIN, T1.CDCONDTYP, T1.NMCONDINI, T1.NMCONDFIM, T1.CDCONDSEQ, T1.DSFORMULA ");
                sqlQuery.Append("      ,T2.NMSEQTAB, T2.DSTABELA ");
                sqlQuery.Append("  FROM BRK_EPRECOFORM T1 ");
                sqlQuery.Append("  LEFT JOIN BRK_EPRECOSEQ T2 ON T2.CDCONDSEQ = T1.CDCONDSEQ ");
                sqlQuery.Append(" WHERE ((T1.CDCONDSEQ IS NULL AND T2.DSTABELA IS NULL) OR (T2.DSTABELA IS NOT NULL) OR (T1.CDCONDSEQ = '')) ");
                sqlQuery.Append(" ORDER BY T1.NMCONDLIN, T2.NMSEQTAB ");

                arraySequencia = new ArrayList(size);

                using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString()))
                {
                    while (reader.Read())
                    {
                        Sequencia seq = new Sequencia();

                        seq.NMCONDLIN = (reader.GetValue(0) == System.DBNull.Value) ? 0 : reader.GetInt32(0);
                        seq.CDCONDTYP = (reader.GetValue(1) == System.DBNull.Value) ? "" : reader.GetString(1).Trim();
                        seq.NMCONDINI = (reader.GetValue(2) == System.DBNull.Value) ? 0 : reader.GetInt32(2);
                        seq.NMCONDFIM = (reader.GetValue(3) == System.DBNull.Value) ? 0 : reader.GetInt32(3);
                        seq.CDCONDSEQ = (reader.GetValue(4) == System.DBNull.Value) ? "" : reader.GetString(4).Trim();
                        seq.DSFORMULA = (reader.GetValue(5) == System.DBNull.Value) ? "" : reader.GetString(5).Trim();
                        seq.NMSEQTAB = (reader.GetValue(6) == System.DBNull.Value) ? 0 : reader.GetInt32(6);
                        seq.DSTABELA = (reader.GetValue(7) == System.DBNull.Value) ? "" : reader.GetString(7).Trim();

                        arraySequencia.Add(seq);
                    }
                    reader.Close();
                    reader.Dispose();
                }

            }

            currentSequencia = 0;
        }

        private bool LeSequencia()
        {
            if (currentSequencia < arraySequencia.Count)
            {
                Sequencia seq = (Sequencia)arraySequencia[currentSequencia];

                NMCONDLIN = seq.NMCONDLIN;
                CDCONDTYP = seq.CDCONDTYP;
                NMCONDINI = seq.NMCONDINI;
                NMCONDFIM = seq.NMCONDFIM;
                CDCONDSEQ = seq.CDCONDSEQ;
                DSFORMULA = seq.DSFORMULA;
                NMSEQTAB = seq.NMSEQTAB;
                DSTABELA = seq.DSTABELA;

                currentSequencia++;

                return true;
            }

            // [ Se for a primeira iteração... ]
            if (iteration == 0)
                throw new Exception("Falha ao iniciar sequência de cálculos!");

            return false;
        }

        /// <summary>
        /// Executa regras de cálculo
        /// </summary>
        /// <param name="tabela">Identificação da regra</param>
        private void ExecutaRegra(string tabela)
        {
            if (!AchoItem())
            {
                switch (tabela)
                {
                    case "EPA004":
                        ExecutaRegraEPA004();
                        break;
                    case "EPA951":
                        ExecutaRegraEPA951();
                        break;
                    case "EPA350":
                        ExecutaRegraEPA350();
                        break;
                    case "EPA872":
                        ExecutaRegraEPA872();
                        break;
                    case "EPA813":
                        ExecutaRegraEPA813();
                        break;
                    case "EPA811":
                        ExecutaRegraEPA811();
                        break;
                    case "EPA871":
                        ExecutaRegraEPA871();
                        break;
                    case "EPA346":
                        ExecutaRegraEPA346();
                        break;
                    case "EPA121":
                        ExecutaRegraEPA121();
                        break;
                    case "EPA807":
                        ExecutaRegraEPA807();
                        break;
                    case "EPA291":
                        ExecutaRegraEPA291();
                        break;
                    case "EPA808":
                        ExecutaRegraEPA808();
                        break;
                    case "EPA002":
                        ExecutaRegraEPA002();
                        break;
                    case "EPA810":
                        ExecutaRegraEPA810();
                        break;
                    case "EPA814":
                        ExecutaRegraEPA814();
                        break;
                    case "EPA815":
                        ExecutaRegraEPA815();
                        break;
                    case "EPA809":
                        ExecutaRegraEPA809();
                        break;
                    case "EPA928":
                        ExecutaRegraEPA928();
                        break;
                    case "EPA870":
                        ExecutaRegraEPA870();
                        break;
                    case "EPA817":
                        ExecutaRegraEPA817();
                        break;
                    case "EPA806":
                        ExecutaRegraEPA806();
                        break;
                    case "EPA812":
                        ExecutaRegraEPA812();
                        break;
                    case "EPA850":
                        ExecutaRegraEPA850();
                        break;
                    case "EPA851":
                        ExecutaRegraEPA851();
                        break;
                    case "EPA852":
                        ExecutaRegraEPA852();
                        break;
                    case "EPA191":
                        ExecutaRegraEPA191();
                        break;
                    case "EPA390":
                        ExecutaRegraEPA390();
                        break;
                    case "EPA917":
                        ExecutaRegraEPA917();
                        break;
                    case "EPA030":
                        ExecutaRegraEPA030();
                        break;
                    case "EPA032":
                        ExecutaRegraEPA032();
                        break;
                    case "EPA031":
                        ExecutaRegraEPA031();
                        break;
                    case "EPA916":
                        ExecutaRegraEPA916();
                        break;
                    case "EPA925":
                        ExecutaRegraEPA925();
                        break;
                    case "EPA903":
                        ExecutaRegraEPA903();
                        break;
                    case "EPA945":
                        ExecutaRegraEPA945();
                        break;
                    case "EPA905":
                        ExecutaRegraEPA905();
                        break;
                    case "EPA924":
                        ExecutaRegraEPA924();
                        break;
                    case "EPA910":
                        ExecutaRegraEPA910();
                        break;
                    case "EPA943":
                        ExecutaRegraEPA943();
                        break;
                    case "EPA803":
                        ExecutaRegraEPA803();
                        break;
                    case "EPA906":
                        ExecutaRegraEPA906();
                        break;
                    case "EPA915":
                        ExecutaRegraEPA915();
                        break;
                    case "EPA904":
                        ExecutaRegraEPA904();
                        break;
                    case "EPA947":
                        ExecutaRegraEPA947();
                        break;
                    case "EPA816":
                        ExecutaRegraEPA816();
                        break;
                    case "EPA382":
                        ExecutaRegraEPA382();
                        break;
                    case "EPA392":
                        ExecutaRegraEPA392();
                        break;
                    case "EPA823":
                        ExecutaRegraEPA823();
                        break;
                    case "EPA832":
                        ExecutaRegraEPA832();
                        break;
                    case "EPA833":
                        ExecutaRegraEPA833();
                        break;
                    case "EPA834":
                        ExecutaRegraEPA834();
                        break;
                    case "EPA835":
                        ExecutaRegraEPA835();
                        break;
                    case "EPA836":
                        ExecutaRegraEPA836();
                        break;
                    case "EPA837":
                        ExecutaRegraEPA837();
                        break;
                    case "EPA880":
                        ExecutaRegraEPA880();
                        break;
                    case "EPA341":
                        ExecutaRegraEPA341();
                        break;
                    case "EPA394":
                        ExecutaRegraEPA394();
                        break;
                    case "EPA778":
                        ExecutaRegraEPA778();
                        break;
                    case "EPA779":
                        ExecutaRegraEPA779();
                        break;
                    case "EPA793":
                        ExecutaRegraEPA793();
                        break;
                    case "EPA293":
                        ExecutaRegraEPA293();
                        break;
                }
            }
        }
        private bool AchoItem()
        {
            bool result = false;

            if (tmpPricingCons != null && CDCONDTYP != "")
            {
                for (int i = 0; i < tmpPricingCons.Length; i++)
                {
                    if (tmpPricingCons[i] != null)
                    {
                        //O false inativou o if
                        if (tmpPricingCons[i].CDCONDTYP == CDCONDTYP && false)
                        {
                            ROWCOUNT = 1;
                            VALOR = tmpPricingCons[i].DADO;
                            CDCONDUNID = tmpPricingCons[i].CDCONDUNID;
                            result = true;
                            break;
                        }
                    }
                }
            }

            return result;
        }
        /// <summary>
        /// Executa a regra de cálculo
        /// </summary>
        /// <param name="sql">Query SQL da regra</param>
        /// <param name="temporary">Diz se o resultado será temporariamente armazenado</param>
        /// <param name="parametros">Parâmetros da query SQL</param>
        private void ExecutaRegra(string sql, bool temporary, SQLiteParameter[] parametros)
        {
            object[] result;

            if (DSTABELA != "EPA905" && DSTABELA != "EPA906" && DSTABELA != "EPA924")
                result = CSDataAccess.Instance.ExecuteReaderCached(sql, NMCONDLIN + " - " + NMSEQTAB + " - " + DSTABELA, temporary, parametros);
            else
            {

                if ((CDCONDTYP == "X066") || (CDCONDTYP == "Z054" && INDENIZACAO == true))
                {
                    result = new object[4];
                    result[0] = Convert.ToDecimal(percentualDesconto * -1);
                    result[1] = "%";
                    result[2] = Convert.ToDecimal(0);
                    result[3] = Convert.ToDecimal(0);

                }
                else
                    result = CSDataAccess.Instance.ExecuteReaderCached(sql, NMCONDLIN + " - " + NMSEQTAB + " - " + DSTABELA + " - " + percentualDesconto.ToString().Replace(',', '.'), temporary, parametros);
            }

            if (result != null)
            {
                ROWCOUNT = 1;
                // [ Lê valores da tabela ]
                VALOR = Convert.ToDecimal(result[0]);
                CDCONDUNID = result[1].ToString().Trim();
                VLLIMINF = Convert.ToDecimal(result[2]);
                VLLIMSUP = Convert.ToDecimal(result[3]);

            }
            else
            {
                ROWCOUNT = 0;

                VALOR = 0m;
                CDCONDUNID = "";
                VLLIMINF = 0m;
                VLLIMSUP = 0m;

            }

#if TRACE && !ANDROID
            streamLog.WriteLine(DateTime.Now.Subtract(h1).ToString() + " - " + NMCONDLIN + " - " + DSTABELA + " - " + VALOR.ToString() + " - " + NMSEQTAB.ToString());
            streamLog.Flush();
#endif
        }

        private bool ExecutaNovaCondition()
        {
            string a;

            if (NMCONDLIN == 247)
                a = "S";

            RETORNOULINTABANT_SEQUENCE = ROWCOUNT;
            CDCONDSEQ_ANT = CDCONDSEQ;
            CDCONDTYP_ANT = CDCONDTYP;
            NMCONDLIN_ANT = NMCONDLIN;
            DSFORMULA_ANT = DSFORMULA;
            NMCONDINI_ANT = NMCONDINI;
            NMCONDFIM_ANT = NMCONDFIM;
            CDCONDUNID_ANT = CDCONDUNID;

            if (CDCONDUNID == "0" || CDCONDUNID == "")
                CDCONDUNID = (DSTABELA == "" || (DSTABELA != "" && ROWCOUNT == 0) ? "BRL" : "%");

            if (DSFORMULA_ANT == "F960F323")
                DSFORMULA_ANT = "F960";

            bool hasNext = LeSequencia();

            if (CDCONDSEQ_ANT != CDCONDSEQ)
                RETORNOULINTABANT_SEQUENCE = 0;

            if (NMCONDLIN_ANT != NMCONDLIN || !hasNext)
            {
                FATORFRACAO = (quantidadeFracao > 0) ? quantidadeCaixa : 0;

                if (CDCONDTYP_ANT == "" || DSFORMULA_ANT == "F164" || DSFORMULA_ANT == "F320" ||
                    DSFORMULA_ANT == "F323" || DSFORMULA_ANT == "F905" || DSFORMULA_ANT == "F907" ||
                    DSFORMULA_ANT == "F912" || DSFORMULA_ANT == "F913" || DSFORMULA_ANT == "F960" ||
                    DSFORMULA_ANT == "F961")
                {
                    AplicaFormulaPasso1();
                }

                VALORRET = CalculaValorRetorno();

                AplicaFormulaPasso2();

                RETORNOULINTABANT_SEQUENCE = 0;
                VALOR = 0m;
                CDCONDUNID = "";
                VLLIMINF = 0m;
                VLLIMSUP = 0m;
            }

            return hasNext;
        }

        private void AplicaFormulaPasso1()
        {
            decimal? passo1VALOR = null;

            switch (DSFORMULA_ANT)
            {
                case "F164":
                    passo1VALOR = CalculaFormulaF164();
                    break;

                case "F320":
                    passo1VALOR = CalculaFormulaF320();
                    break;

                case "F905":
                    passo1VALOR = CalculaFormulaF905();
                    break;

                case "F907":
                    passo1VALOR = CalculaFormulaF907();
                    break;

                case "F960":
                    passo1VALOR = CalculaFormulaF960();
                    break;

                case "F961":
                    passo1VALOR = CalculaFormulaF961();
                    break;

                default:
                    passo1VALOR = CalculaFormulaDefaultPasso1();
                    break;
            }

            SetPosTmpPricingCons(NMCONDLIN_ANT, CDCONDTYP_ANT, CDCONDUNID_ANT, 0, passo1VALOR);

            ROWCOUNT = 1;
        }

        private decimal? CalculaValorRetorno()
        {
            return GetSumTmpPricingCons(0, NMCONDINI_ANT, NMCONDFIM_ANT, NMCONDINI_ANT, NMCONDINI_ANT);
        }
        private decimal? SetNovoValorParametro()
        {
            decimal? result = null;

            try
            {

                // NOVAPRICING 
                if (CDCONDTYP_ANT == "ZCMI")
                {
                    decimal? val = GetSumTmpPricingCons(300, 300); // 300 = ICMI
                    decimal formulaVal1 = (val.HasValue) ? val.Value : 0m;

                    result = formulaVal1 * (quantidadeInteira + (quantidadeFracao != 0m ? 1m : 0m));
                    result = CSGlobal.Round(result.Value, 2);

                }

                if (CDCONDTYP_ANT == "YPI3")
                {
                    decimal? val = GetSumTmpPricingCons(738, 738); // 718 = BX20 
                    decimal formulaVal1 = (val.HasValue) ? val.Value : 0m;

                    val = GetDadoTmpPricingCons(241); // 241 = IPVA)
                    formulaVal1 = formulaVal1 * ((val.HasValue) ? val.Value : 0m);
                    result = CSGlobal.Round(formulaVal1, 2);

                }
            }
            catch
            {
                result = null;
            }

            return result;

        }
        private void AplicaFormulaPasso2()
        {
            decimal? passo2VALOR = 0;

            if (CDCONDUNID == "%")
            {
                switch (DSFORMULA_ANT)
                {
                    case "F616":
                        passo2VALOR = CalculaFormulaF616();
                        break;

                    case "F904":
                        passo2VALOR = CalculaFormulaF904();
                        break;

                    case "F906":
                        passo2VALOR = CalculaFormulaF906();
                        break;

                    case "F969":
                        passo2VALOR = CalculaFormulaF969();
                        break;

                    case "F973":
                        passo2VALOR = CalculaFormulaF973();
                        break;

                    case "F615":
                        passo2VALOR = CalculaFormulaF615();
                        break;

                    case "F701": //Essa formula realmente utiliza a mesma regra da formula 615
                        passo2VALOR = CalculaFormulaF615();
                        break;

                    case "F974":
                        passo2VALOR = CalculaFormulaF974();
                        break;

                    case "F975":
                        passo2VALOR = CalculaFormulaF975();
                        break;

                    default:
                        passo2VALOR = CalculaFormulaDefaultPasso2();
                        break;
                }
            }
            else
            {
                passo2VALOR = CalculaFormulaBRL();
            }

            if (CDCONDTYP_ANT == "YPI3" || CDCONDTYP_ANT == "ZCMI")
                passo2VALOR = SetNovoValorParametro();

            if (!passo2VALOR.HasValue)
            {
                passo2VALOR = CalculaFormulaIsNullPasso2();
            }

            SetPosTmpPricingCons(NMCONDLIN_ANT, CDCONDTYP_ANT, CDCONDUNID_ANT, VALOR, passo2VALOR);
        }

        /// <summary>
        /// Realiza o preprocessamento do cliente para agilizar os cálculos
        /// </summary>
        private void PreprocessaCliente()
        {
            SQLiteDataReader reader = null;

#if TRACE && !ANDROID
            h1 = DateTime.Now;
#endif

            // [ Descarta cache de resultados ]
            bool primeiroCliente = (CSDataAccess.Instance.DisposeCachedResults(TypedHashtable.HashtableEntryType.All) == 0);

#if TRACE && !ANDROID
            streamLog.WriteLine(DateTime.Now.Subtract(h1).ToString() + " - DisposeCachedResults");
            streamLog.Flush();

            h1 = DateTime.Now;
#endif

            // [ Popula tabela de variáveis para os cálculos ]
            InicializaTabelaVariaveis(produto, grupoComercializacao);

#if TRACE && !ANDROID
            streamLog.WriteLine(DateTime.Now.Subtract(h1).ToString() + " - Variaveis");
            streamLog.Flush();
#endif

            // [ Armazena resultados das regras que são associadas somente ao cliente ]
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT DISTINCT T1.NMCONDLIN, T1.CDCONDTYP, T2.DSTABELA, T2.NMSEQTAB ");
            sqlQuery.Append("  FROM BRK_EPRECOFORM T1 ");
            sqlQuery.Append("  JOIN BRK_EPRECOSEQ T2 ON T2.CDCONDSEQ = T1.CDCONDSEQ ");
            sqlQuery.Append(" WHERE RTRIM(T2.DSTABELA) IN ('EPA291','EPA293','EPA390','EPA392','EPA806' ");
            sqlQuery.Append("                      ,'EPA871','EPA880','EPA807' ");
            sqlQuery.Append("                      ,'EPA809','EPA812','EPA814') ");

            using (reader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString()))
            {
                while (reader.Read())
                {
                    NMCONDLIN = reader.GetInt32(0);
                    CDCONDTYP = (reader.GetValue(1) == System.DBNull.Value) ? "" : reader.GetString(1).Trim();
                    DSTABELA = (reader.GetValue(2) == System.DBNull.Value) ? "" : reader.GetString(2).Trim();
                    NMSEQTAB = reader.GetInt32(3);

#if TRACE && !ANDROID
                h1 = DateTime.Now;
#endif
                    ExecutaRegra(DSTABELA);
                }
                reader.Close();
                reader.Dispose();
            }

            // [ Executar os comando seguintes apenas se for a primeira execução dos commands ]
            if (!primeiroCliente)
                return;

#if TRACE && !ANDROID
            h1 = DateTime.Now;
#endif

            CalculaDescontoMaximo();

#if TRACE && !ANDROID
            streamLog.WriteLine(DateTime.Now.Subtract(h1).ToString() + " - CalculaDescontoMaximo");
            streamLog.Flush();
#endif

#if TRACE && !ANDROID
            h1 = DateTime.Now;
#endif

            PreparaSequencia();

#if TRACE && !ANDROID
            streamLog.WriteLine(DateTime.Now.Subtract(h1).ToString() + " - PreparaSequencia");
            streamLog.Flush();
#endif

            // [ Força limpeza ]
            //GC.Collect();
        }

        #region [ Regras de Cálculo ]

        private void ExecutaRegraEPA004()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000, T1.CDCONDUNID, T1.VLLIMINF, T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA004 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDGER0 = ? AND ");
            sqlQuery.Append("       T1.CDCANDISTR = ? AND T1.CDPRD = ? AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[05];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04PRODUTO", tmpVariaveis.PRODUTO);
            parametros[04] = new SQLiteParameter("@05DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA951()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA951 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDGER0 = ? AND ");
            sqlQuery.Append("       T1.CDPRD = ? AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[04];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03PRODUTO", tmpVariaveis.PRODUTO);
            parametros[03] = new SQLiteParameter("@04DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA350()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA350 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDGER0 = ? AND ");
            sqlQuery.Append("       ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[03];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), false, parametros);
        }

        private void ExecutaRegraEPA872()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000, T1.CDCONDUNID, T1.VLLIMINF, T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA872 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDPAIS = 'BR' ");
            sqlQuery.Append("   AND T1.CDREGFORIG = ? ");
            sqlQuery.Append("   AND T1.CDREGFDEST = ? ");
            sqlQuery.Append("   AND T1.CDCLASFISC = ? ");
            sqlQuery.Append("   AND T1.IDSITUACAO = '' ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[05];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDREGFORIG", tmpVariaveis.CDREGFISCAFIL);
            parametros[02] = new SQLiteParameter("@03CDREGFDEST", tmpVariaveis.CDREGFISCACLI);
            parametros[03] = new SQLiteParameter("@CDCLASFISC", tmpVariaveis.CDCLASFISC);
            parametros[04] = new SQLiteParameter("@05DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA813()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000, T1.CDCONDUNID, T1.VLLIMINF, T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA813 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDPAIS = 'BR' ");
            sqlQuery.Append("   AND T1.CDREGFORIG = ? ");
            sqlQuery.Append("   AND T1.CDREGFDEST = ? ");
            sqlQuery.Append("   AND T1.CDPRD = ? ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[05];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDREGFORIG", tmpVariaveis.CDREGFISCAFIL);
            parametros[02] = new SQLiteParameter("@03CDREGFDEST", tmpVariaveis.CDREGFISCACLI);
            parametros[03] = new SQLiteParameter("@04CDPRD", tmpVariaveis.PRODUTO);
            parametros[04] = new SQLiteParameter("@05DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA811()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000, T1.CDCONDUNID, T1.VLLIMINF, T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA811 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDPAIS = 'BR' ");
            sqlQuery.Append("   AND T1.CDREGFORIG = ? ");
            sqlQuery.Append("   AND T1.CDREGFDEST = ? ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[04];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);

            if (CDCONDSEQ == "ZSTC")
                parametros[01] = new SQLiteParameter("@02CDREGFORIG", tmpVariaveis.CDREGFISCACLI.PadLeft(3));
            else
                parametros[01] = new SQLiteParameter("@02CDREGFORIG", tmpVariaveis.CDREGFISCAFIL.PadLeft(3));

            parametros[02] = new SQLiteParameter("@03CDREGFDEST", tmpVariaveis.CDREGFISCACLI);
            parametros[03] = new SQLiteParameter("@04DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), false, parametros);
        }

        private void ExecutaRegraEPA871()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA871 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDPAIS = 'BR' AND T1.CDTIPOPED = 'OR' AND ");
            sqlQuery.Append("T1.IDSITUACAO = '' AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[02];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), false, parametros);
        }
        private void ExecutaRegraEPA346()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000, T1.CDCONDUNID, T1.VLLIMINF, T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA346 T1  ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDPAIS = 'BR' ");
            sqlQuery.Append("   AND T1.CDDINAMIC1 = ? ");
            sqlQuery.Append("   AND T1.CDDINAMIC2 = ? ");
            sqlQuery.Append("   AND T1.CDDINAMIC3 = ? ");
            sqlQuery.Append("   AND T1.CDGRPDINAM = ? ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[06];
            bool found = false;

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[05] = new SQLiteParameter("@06DATA", tmpVariaveis.DATA);

            if (CDCONDSEQ == "ZPBS" || CDCONDSEQ == "ZPVA")
            {
                switch (NMSEQTAB)
                {
                    case 2:
                    case 20:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.CDFILFAT);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", tmpVariaveis.PRODUTO);
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 75);

                        found = true;
                        break;

                    case 3:
                    case 30:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.CDFILFAT);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", tmpVariaveis.CDCLASFISC);
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 80);

                        found = true;
                        break;

                    case 4:
                    case 35:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 14);

                        found = true;
                        break;

                    case 5:
                    case 40:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.PRODUTO);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", "SD");
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 45);

                        found = true;
                        break;

                    case 6:
                    case 50:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 15);

                        found = true;
                        break;

                    case 7:
                    case 60:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.CDCLASFISC);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 25);

                        found = true;
                        break;

                    case 8:
                    case 65:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 33);

                        found = true;
                        break;

                    case 9:
                    case 70:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", "X");
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 35);

                        found = true;
                        break;

                    case 10:
                    case 72:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 37);

                        found = true;
                        break;

                    case 11:
                    case 74:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.CDCLASFISC);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 38);

                        found = true;
                        break;

                    case 12:
                    case 76:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", "X");
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 39);

                        found = true;
                        break;

                    case 13:
                    case 80:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CDAGPRDESC);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", "X");
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 65);

                        found = true;
                        break;

                    case 14:
                    case 90:

                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[01] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[01] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[01] = new SQLiteParameter("@02CDDINAMIC1", "BR10");

                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 82);

                        found = true;
                        break;

                    case 15:
                    case 100:

                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[01] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[01] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[01] = new SQLiteParameter("@02CDDINAMIC1", "BR10");
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.CDCLASFISC);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 83);

                        found = true;
                        break;

                    case 16:
                    case 110:

                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[01] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[01] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[01] = new SQLiteParameter("@02CDDINAMIC1", "BR10");
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", "X");
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 84);

                        found = true;
                        break;
                }
            }
            else if (CDCONDSEQ == "ZPOB")
            {
                switch (NMSEQTAB)
                {
                    case 5:
                    case 50:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.CDFILFAT);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", tmpVariaveis.PRODUTO);
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 75);

                        found = true;
                        break;

                    case 6:
                    case 60:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.CDFILFAT);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", tmpVariaveis.CDCLASFISC);
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 80);

                        found = true;
                        break;

                    case 7:
                    case 65:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 14);

                        found = true;
                        break;

                    case 8:
                    case 70:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.PRODUTO);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", "SD");
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 45);

                        found = true;
                        break;

                    case 9:
                    case 80:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 15);

                        found = true;
                        break;

                    case 10:
                    case 90:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.CDCLASFISC);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 25);

                        found = true;
                        break;

                    case 11:
                    case 95:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 33);

                        found = true;
                        break;

                    case 12:
                    case 100:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", "X");
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 35);

                        found = true;
                        break;

                    case 13:
                    case 102:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 37);

                        found = true;
                        break;

                    case 14:
                    case 104:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.CDCLASFISC);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 38);

                        found = true;
                        break;

                    case 15:
                    case 106:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", "X");
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 39);

                        found = true;
                        break;

                    case 16:
                    case 110:

                        parametros[01] = new SQLiteParameter("@02CDDINAMIC1", tmpVariaveis.CDAGPRDESC);
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", "X");
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 65);

                        found = true;
                        break;

                    case 17:
                    case 120:

                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[01] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[01] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[01] = new SQLiteParameter("@02CDDINAMIC1", "BR10");
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 82);

                        found = true;
                        break;

                    case 18:
                    case 130:

                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[01] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[01] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[01] = new SQLiteParameter("@02CDDINAMIC1", "BR10");
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", tmpVariaveis.CDCLASFISC);
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 83);

                        found = true;
                        break;

                    case 19:
                    case 140:

                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[01] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[01] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[01] = new SQLiteParameter("@02CDDINAMIC1", "BR10");
                        parametros[02] = new SQLiteParameter("@03CDDINAMIC2", "X");
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "X");
                        parametros[04] = new SQLiteParameter("@05CDGRPDINAM", 84);

                        found = true;
                        break;
                }

            }

            if (!found)
            {
                parametros[01] = new SQLiteParameter("@02CDDINAMIC1", "");
                parametros[02] = new SQLiteParameter("@03CDDINAMIC2", "");
                parametros[03] = new SQLiteParameter("@04CDDINAMIC3", "");
                parametros[04] = new SQLiteParameter("@05CDGRPDINAM", -1);
            }

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA121()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA121 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDPAIS = 'BR' ");
            sqlQuery.Append("   AND T1.CDPRD = ? ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[03];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02PRODUTO", tmpVariaveis.PRODUTO);
            parametros[02] = new SQLiteParameter("@03DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA807()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA807 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDPAIS = 'BR' AND ");
            sqlQuery.Append("T1.CDCLASFISC = ? AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[03];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDCLASFISC", tmpVariaveis.CDCLASFISC);
            parametros[02] = new SQLiteParameter("@03DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA291()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA291 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDPAIS = 'BR' AND T1.CDIMPOSTO = 'I3' AND ");
            sqlQuery.Append("? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[02];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), false, parametros);
        }

        private void ExecutaRegraEPA293()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT (T1.VLCONDTYP * -1) / 1000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA293 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDPAIS = 'BR' AND T1.IDCLIIMP = ? AND ");
            sqlQuery.Append("? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[03];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02IDCLIIMP", tmpVariaveis.IDISENIPI);
            parametros[02] = new SQLiteParameter("@03DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), false, parametros);
        }

        private void ExecutaRegraEPA808()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA808 T1  ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDPAIS = 'BR' ");
            sqlQuery.Append("   AND T1.CDREGFORIG = ? ");
            sqlQuery.Append("   AND T1.IDCLIIMP = ? ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[04];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDREGFISCAFIL", tmpVariaveis.CDREGFISCAFIL);
            parametros[02] = new SQLiteParameter("@03IDCLIIMP", tmpVariaveis.IDCLIIMP);
            parametros[03] = new SQLiteParameter("@04DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), false, parametros);
        }

        private void ExecutaRegraEPA002()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA002 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDPAIS = 'BR' AND T1.IDCLIIMP = ? AND ");
            sqlQuery.Append("T1.IDPRDIMP = ? AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[04];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02IDCLIIMP", tmpVariaveis.IDCLIIMP);
            parametros[02] = new SQLiteParameter("@03IDPRDIMP", tmpVariaveis.IDPRDIMP);
            parametros[03] = new SQLiteParameter("@04DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA810()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA810 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDPAIS = 'BR' AND ");
            sqlQuery.Append("T1.IDCLIIMP = ? AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[03];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02IDPRDIMP", tmpVariaveis.IDPRDIMP);
            parametros[02] = new SQLiteParameter("@03DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA814()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA814 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDPAIS = 'BR' ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[02];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), false, parametros);
        }

        private void ExecutaRegraEPA815()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000, T1.CDCONDUNID, T1.VLLIMINF, T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA815 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDPAIS = 'BR' ");
            sqlQuery.Append("   AND T1.CDGRCSUTR = ? ");
            sqlQuery.Append("   AND T1.CDREGFORIG = ? ");
            sqlQuery.Append("   AND T1.CDREGFDEST = ? ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[05];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGRCSUTR", tmpVariaveis.CDGRCSUTR);
            parametros[02] = new SQLiteParameter("@03CDREGFORIG", tmpVariaveis.CDREGFISCAFIL);
            parametros[03] = new SQLiteParameter("@04CDREGFDEST", tmpVariaveis.CDREGFISCACLI);
            parametros[04] = new SQLiteParameter("@05DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), false, parametros);
        }

        private void ExecutaRegraEPA809()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA809 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDTPVENDA = 'OR' AND ");
            sqlQuery.Append("? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[02];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), false, parametros);
        }

        private void ExecutaRegraEPA928()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA928 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDGER0 = ? AND ");
            sqlQuery.Append("T1.CDCANDISTR = ? AND T1.CDCLI = ? AND ");
            sqlQuery.Append("? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[05];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);
            parametros[04] = new SQLiteParameter("@05DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), false, parametros);
        }

        private void ExecutaRegraEPA870()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA870 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDPAIS = 'BR' AND T1.CDREGFORIG = ? AND ");
            sqlQuery.Append("T1.IDCLIIMP = ? AND ");
            sqlQuery.Append("T1.IDSITUACAO = '' AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[04];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDREGFISCAFIL", tmpVariaveis.CDREGFISCAFIL);
            parametros[02] = new SQLiteParameter("@03IDCLIIMP", tmpVariaveis.IDCLIIMP);
            parametros[03] = new SQLiteParameter("@05DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA817()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000, T1.CDCONDUNID, T1.VLLIMINF, T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA817 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDPAIS = 'BR' ");
            sqlQuery.Append("   AND T1.CDREGFDEST = ? ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[03];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDREGFDEST", tmpVariaveis.CDREGFISCACLI);
            parametros[02] = new SQLiteParameter("@03DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), false, parametros);
        }

        private void ExecutaRegraEPA806()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000, T1.CDCONDUNID, T1.VLLIMINF, T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA806 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDPAIS = 'BR' ");
            sqlQuery.Append("   AND T1.IDCLIIMP = ? AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[03];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02IDISENIPI", tmpVariaveis.IDISENIPI);
            parametros[02] = new SQLiteParameter("@03DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), false, parametros);
        }

        private void ExecutaRegraEPA812()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000, T1.CDCONDUNID, T1.VLLIMINF, T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA812 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDPAIS = 'BR' AND ");
            sqlQuery.Append("T1.IDCLIIMP = ? AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[03];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02IDISENICMS", tmpVariaveis.IDISENICMS);
            parametros[02] = new SQLiteParameter("@03DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), false, parametros);
        }

        private void ExecutaRegraEPA850()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000, T1.CDCONDUNID, T1.VLLIMINF, T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA850 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDPAIS = 'BR' ");
            sqlQuery.Append("   AND T1.IDTIPODOC = '0' ");
            sqlQuery.Append("   AND T1.CDREGFORIG = ? ");
            sqlQuery.Append("   AND T1.CDREGFDEST = ? ");
            sqlQuery.Append("   AND T1.CDDINAMIC1 = ? ");
            sqlQuery.Append("   AND T1.CDDINAMIC2 = ? ");
            sqlQuery.Append("   AND T1.CDDINAMIC3 = ? ");
            sqlQuery.Append("   AND T1.CDGRPDINAM = ? ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[08];
            bool found = false;

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDREGFORIG", tmpVariaveis.CDREGFISCAFIL);
            parametros[02] = new SQLiteParameter("@03CDREGFDEST", tmpVariaveis.CDREGFISCACLI);
            parametros[07] = new SQLiteParameter("@08DATA", tmpVariaveis.DATA);

            switch (CDCONDSEQ)
            {
                case "ZSTI":
                    if (NMSEQTAB == 2 || NMSEQTAB == 20)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.CDFILFAT);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", tmpVariaveis.PRODUTO);
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 75);
                        found = true;
                    }
                    else if (NMSEQTAB == 3 || NMSEQTAB == 30)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.CDFILFAT);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", tmpVariaveis.CDCLASFISC);
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 80);
                        found = true;
                    }
                    else if (NMSEQTAB == 5 || NMSEQTAB == 50)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CDAGPRDESC);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", "X");
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 65);
                        found = true;
                    }
                    else if (NMSEQTAB == 6 || NMSEQTAB == 60)
                    {
                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR10");

                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 82);
                        found = true;
                    }
                    else if (NMSEQTAB == 7 || NMSEQTAB == 70)
                    {
                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR10");
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.CDCLASFISC);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 83);
                        found = true;
                    }
                    else if (NMSEQTAB == 8 || NMSEQTAB == 80)
                    {
                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR10");
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", "X");
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 84);
                        found = true;
                    }
                    else if (NMSEQTAB == 99)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.PRODUTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", "X");
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 70);
                        found = true;
                    }
                    break;
                case "ZCBS":
                    if (NMSEQTAB == 2 || NMSEQTAB == 20)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.CDFILFAT);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", tmpVariaveis.PRODUTO);
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 75);
                        found = true;
                    }
                    else if (NMSEQTAB == 3 || NMSEQTAB == 30)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.CDFILFAT);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", tmpVariaveis.CDCLASFISC);
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 80);
                        found = true;
                    }
                    else if (NMSEQTAB == 4 || NMSEQTAB == 35)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 14);
                        found = true;
                    }
                    else if (NMSEQTAB == 5 || NMSEQTAB == 40)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 15);
                        found = true;
                    }
                    else if (NMSEQTAB == 6 || NMSEQTAB == 50)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.CDCLASFISC);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 25);
                        found = true;
                    }
                    else if (NMSEQTAB == 7 || NMSEQTAB == 55)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 33);
                        found = true;
                    }
                    else if (NMSEQTAB == 8 || NMSEQTAB == 60)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", "X");
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 35);
                        found = true;
                    }
                    else if (NMSEQTAB == 9 || NMSEQTAB == 62)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 37);
                        found = true;
                    }
                    else if (NMSEQTAB == 10 || NMSEQTAB == 64)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.CDCLASFISC);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 38);
                        found = true;
                    }
                    else if (NMSEQTAB == 11 || NMSEQTAB == 66)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", "X");
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 39);
                        found = true;
                    }
                    else if (NMSEQTAB == 12 || NMSEQTAB == 70)
                    {
                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR10");
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 82);
                        found = true;
                    }
                    else if (NMSEQTAB == 13 || NMSEQTAB == 80)
                    {
                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR10");
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.CDCLASFISC);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 83);
                        found = true;
                    }
                    else if (NMSEQTAB == 14 || NMSEQTAB == 90)
                    {
                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR10");
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", "X");
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 84);
                        found = true;
                    }
                    else if (NMSEQTAB == 16 || NMSEQTAB == 110)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CDAGPRDESC);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", "X");
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 65);
                        found = true;
                    }
                    else if (NMSEQTAB == 17 || NMSEQTAB == 120)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CDCLASFISC);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", "X");
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 55);
                        found = true;
                    }
                    break;
                case "ZCVA":
                    if (NMSEQTAB == 5 || NMSEQTAB == 45)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 14);
                        found = true;
                    }
                    else if (NMSEQTAB == 6 || NMSEQTAB == 50)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 15);
                        found = true;
                    }
                    else if (NMSEQTAB == 7 || NMSEQTAB == 60)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.CDCLASFISC);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 25);
                        found = true;
                    }
                    else if (NMSEQTAB == 8 || NMSEQTAB == 65)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 33);
                        found = true;
                    }
                    else if (NMSEQTAB == 9 || NMSEQTAB == 70)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", "X");
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 35);
                        found = true;
                    }
                    else if (NMSEQTAB == 10 || NMSEQTAB == 72)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 37);
                        found = true;
                    }
                    else if (NMSEQTAB == 11 || NMSEQTAB == 74)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.CDCLASFISC);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 38);
                        found = true;
                    }
                    else if (NMSEQTAB == 12 || NMSEQTAB == 76)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", "X");
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 39);
                        found = true;
                    }
                    else if (NMSEQTAB == 18 || NMSEQTAB == 130)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CDCLASFISC);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", "X");
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 55);
                        found = true;
                    }
                    else if (NMSEQTAB == 19 || NMSEQTAB == 120)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CDAGPRDESC);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", "X");
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 65);
                        found = true;
                    }
                    else if (NMSEQTAB == 3 || NMSEQTAB == 30)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.CDFILFAT);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", tmpVariaveis.PRODUTO);
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 75);
                        found = true;
                    }
                    else if (NMSEQTAB == 4 || NMSEQTAB == 40)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.CDFILFAT);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", tmpVariaveis.CDCLASFISC);
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 80);
                        found = true;
                    }
                    else if (NMSEQTAB == 13 || NMSEQTAB == 80)
                    {
                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR10");
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 82);
                        found = true;
                    }
                    else if (NMSEQTAB == 14 || NMSEQTAB == 90)
                    {
                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR10");
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.CDCLASFISC);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 83);
                        found = true;
                    }
                    else if (NMSEQTAB == 15 || NMSEQTAB == 100)
                    {
                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR10");
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", "X");
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 84);
                        found = true;
                    }
                    break;
                case "ZSTC":
                    if (NMSEQTAB == 1 || NMSEQTAB == 10)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.CDFILFAT);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", tmpVariaveis.PRODUTO);
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 75);
                        found = true;
                    }
                    else if (NMSEQTAB == 2 || NMSEQTAB == 20)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.CDFILFAT);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", tmpVariaveis.CDCLASFISC);
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 80);
                        found = true;
                    }
                    else if (NMSEQTAB == 3 || NMSEQTAB == 30)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 15);
                        found = true;
                    }
                    else if (NMSEQTAB == 4 || NMSEQTAB == 40)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.CDCLASFISC);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 25);
                        found = true;
                    }
                    else if (NMSEQTAB == 5 || NMSEQTAB == 50)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", "X");
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 35);
                        found = true;
                    }
                    else if (NMSEQTAB == 6 || NMSEQTAB == 60)
                    {
                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR10");
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 82);
                        found = true;
                    }
                    else if (NMSEQTAB == 7 || NMSEQTAB == 70)
                    {
                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR10");
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", tmpVariaveis.CDCLASFISC);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 83);
                        found = true;
                    }
                    else if (NMSEQTAB == 8 || NMSEQTAB == 80)
                    {
                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[03] = new SQLiteParameter("@02CDDINAMIC1", "BR10");
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", "X");
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 84);
                        found = true;
                    }
                    else if (NMSEQTAB == 10 || NMSEQTAB == 100)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CDAGPRDESC);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", "X");
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 65);
                        found = true;
                    }
                    else if (NMSEQTAB == 11 || NMSEQTAB == 110)
                    {
                        parametros[03] = new SQLiteParameter("@04CDDINAMIC1", tmpVariaveis.CDCLASFISC);
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC2", "X");
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "X");
                        parametros[06] = new SQLiteParameter("@07CDGRPDINAM", 55);
                        found = true;
                    }
                    break;
            }

            if (!found)
            {
                parametros[03] = new SQLiteParameter("@04CDDINAMIC1", "");
                parametros[04] = new SQLiteParameter("@05CDDINAMIC2", "");
                parametros[05] = new SQLiteParameter("@06CDDINAMIC3", "");
                parametros[06] = new SQLiteParameter("@07CDGRPDINAM", -1);
            }

            ExecutaRegra(sqlQuery.ToString(), true, parametros);

        }
        private void ExecutaRegraEPA851()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000, T1.CDCONDUNID, T1.VLLIMINF, T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA851 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDPAIS = 'BR' ");
            sqlQuery.Append("   AND T1.CDREGFORIG = ? ");
            sqlQuery.Append("   AND T1.CDREGFDEST = ? ");
            sqlQuery.Append("   AND T1.CDGRCSUTR = ? ");
            sqlQuery.Append("   AND T1.CDDINAMIC1 = ? ");
            sqlQuery.Append("   AND T1.CDDINAMIC2 = ? ");
            sqlQuery.Append("   AND T1.CDDINAMIC3 = ? ");
            sqlQuery.Append("   AND T1.CDGRPDINAM = ? ");
            sqlQuery.Append("   AND T1.IDSITUACAO = '' ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[09];
            bool found = false;

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDREGFORIG", tmpVariaveis.CDREGFISCAFIL);
            parametros[02] = new SQLiteParameter("@03CDREGFDEST", tmpVariaveis.CDREGFISCACLI);
            parametros[03] = new SQLiteParameter("@04CDGRCSUTR", tmpVariaveis.CDGRCSUTR);
            parametros[08] = new SQLiteParameter("@09DATA", tmpVariaveis.DATA);

            if (CDCONDSEQ == "ZSTV" || CDCONDSEQ == "ISTV")
            {
                switch (NMSEQTAB)
                {
                    case 2:
                    case 20:
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC2", tmpVariaveis.CDFILFAT);
                        parametros[06] = new SQLiteParameter("@07CDDINAMIC3", tmpVariaveis.PRODUTO);
                        parametros[07] = new SQLiteParameter("@08CDGRPDINAM", 75);
                        found = true;
                        break;
                    case 3:
                    case 30:
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC2", tmpVariaveis.CDFILFAT);
                        parametros[06] = new SQLiteParameter("@07CDDINAMIC3", tmpVariaveis.CDCLASFISC);
                        parametros[07] = new SQLiteParameter("@08CDGRPDINAM", 80);
                        found = true;
                        break;
                    case 4:
                    case 35:
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC2", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[06] = new SQLiteParameter("@07CDDINAMIC3", "X");
                        parametros[07] = new SQLiteParameter("@08CDGRPDINAM", 14);
                        found = true;
                        break;
                    case 5:
                    case 40:
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[06] = new SQLiteParameter("@07CDDINAMIC3", "X");
                        parametros[07] = new SQLiteParameter("@08CDGRPDINAM", 15);
                        found = true;
                        break;
                    case 6:
                    case 50:
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC2", tmpVariaveis.CDCLASFISC);
                        parametros[06] = new SQLiteParameter("@07CDDINAMIC3", "X");
                        parametros[07] = new SQLiteParameter("@08CDGRPDINAM", 25);
                        found = true;
                        break;
                    case 7:
                    case 55:
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[06] = new SQLiteParameter("@07CDDINAMIC3", "X");
                        parametros[07] = new SQLiteParameter("@08CDGRPDINAM", 33);
                        found = true;
                        break;
                    case 8:
                    case 60:
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC1", tmpVariaveis.CLIENTE_SHIPTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC2", "X");
                        parametros[06] = new SQLiteParameter("@07CDDINAMIC3", "X");
                        parametros[07] = new SQLiteParameter("@08CDGRPDINAM", 35);
                        found = true;
                        break;
                    case 9:
                    case 62:
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[06] = new SQLiteParameter("@07CDDINAMIC3", "X");
                        parametros[07] = new SQLiteParameter("@08CDGRPDINAM", 37);
                        found = true;
                        break;
                    case 10:
                    case 64:
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC2", tmpVariaveis.CDCLASFISC);
                        parametros[06] = new SQLiteParameter("@07CDDINAMIC3", "X");
                        parametros[07] = new SQLiteParameter("@08CDGRPDINAM", 38);
                        found = true;
                        break;
                    case 11:
                    case 66:
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC1", tmpVariaveis.CDFILFAT);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC2", "X");
                        parametros[06] = new SQLiteParameter("@07CDDINAMIC3", "X");
                        parametros[07] = new SQLiteParameter("@08CDGRPDINAM", 39);
                        found = true;
                        break;
                    case 99:
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC1", tmpVariaveis.PRODUTO);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC2", "SD");
                        parametros[06] = new SQLiteParameter("@07CDDINAMIC3", "X");
                        parametros[07] = new SQLiteParameter("@08CDGRPDINAM", 45);
                        found = true;
                        break;
                    case 16:
                    case 110:
                        parametros[04] = new SQLiteParameter("@05CDDINAMIC1", tmpVariaveis.CDCLASFISC);
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC2", "X");
                        parametros[06] = new SQLiteParameter("@07CDDINAMIC3", "X");
                        parametros[07] = new SQLiteParameter("@08CDGRPDINAM", 55);
                        found = true;
                        break;
                    case 12:
                    case 70:
                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[04] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[04] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[04] = new SQLiteParameter("@02CDDINAMIC1", "BR10");

                        parametros[05] = new SQLiteParameter("@06CDDINAMIC2", tmpVariaveis.PRODUTO);
                        parametros[06] = new SQLiteParameter("@07CDDINAMIC3", "X");
                        parametros[07] = new SQLiteParameter("@08CDGRPDINAM", 82);
                        found = true;
                        break;
                    case 13:
                    case 80:
                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[04] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[04] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[04] = new SQLiteParameter("@02CDDINAMIC1", "BR10");
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC2", tmpVariaveis.CDCLASFISC);
                        parametros[06] = new SQLiteParameter("@07CDDINAMIC3", "X");
                        parametros[07] = new SQLiteParameter("@08CDGRPDINAM", 83);
                        found = true;
                        break;
                    case 14:
                    case 90:
                        /*Empresa*/
                        if (tmpVariaveis.CDGER0 == "BRG1")
                            parametros[04] = new SQLiteParameter("@02CDDINAMIC1", "BR23");
                        else
                            if (tmpVariaveis.CDGER0 == "BRD1")
                                parametros[04] = new SQLiteParameter("@02CDDINAMIC1", "BR24");
                            else
                                parametros[04] = new SQLiteParameter("@02CDDINAMIC1", "BR10");
                        parametros[05] = new SQLiteParameter("@06CDDINAMIC2", "X");
                        parametros[06] = new SQLiteParameter("@07CDDINAMIC3", "X");
                        parametros[07] = new SQLiteParameter("@08CDGRPDINAM", 84);
                        found = true;
                        break;
                }
            }

            if (!found)
            {
                parametros[04] = new SQLiteParameter("@05CDDINAMIC1", "");
                parametros[05] = new SQLiteParameter("@06CDDINAMIC2", "");
                parametros[06] = new SQLiteParameter("@07CDDINAMIC3", "");
                parametros[07] = new SQLiteParameter("@08CDGRPDINAM", -1);
            }

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA852()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000, T1.CDCONDUNID, T1.VLLIMINF, T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA852 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDGRPDINAM = 88 ");
            sqlQuery.Append("   AND T1.CDCLASFISC = ? ");
            sqlQuery.Append("   AND T1.CDREGFDEST = ? ");
            sqlQuery.Append("   AND T1.IDSITUACAO = '' ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[04];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDCLASFISC", tmpVariaveis.CDCLASFISC);
            parametros[02] = new SQLiteParameter("@03CDREGFDEST", tmpVariaveis.CDREGFISCACLI);
            parametros[03] = new SQLiteParameter("@04DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA191()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA191 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDPAIS = 'BR' AND T1.CDPAISDEST = 'BR' AND ");
            sqlQuery.Append("T1.IDPRDIMP = ? AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[03];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02IDPRDIMP", tmpVariaveis.IDPRDIMP);
            parametros[02] = new SQLiteParameter("@03DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA390()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA390 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDPAIS = 'BR' ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[02];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), false, parametros);
        }

        private void ExecutaRegraEPA917()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA917 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDGER0 = ? ");
            sqlQuery.Append("   AND T1.CDCANDISTR = ? ");
            sqlQuery.Append("   AND T1.CDCLI = ? ");
            sqlQuery.Append("   AND T1.CDPRD = ? ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[06];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);
            parametros[04] = new SQLiteParameter("@05PRODUTO", tmpVariaveis.PRODUTO);
            parametros[05] = new SQLiteParameter("@06DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA030()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA030 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDGER0 = ? AND T1.CDCANDISTR = ? AND ");
            sqlQuery.Append("T1.CDCLI = ? AND T1.CDGRPPRC = ? AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[06];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);
            parametros[04] = new SQLiteParameter("@05CDGRPPRC", tmpVariaveis.CDGRPPRC);
            parametros[05] = new SQLiteParameter("@06DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), false, parametros);
        }

        private void ExecutaRegraEPA032()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA032 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDGER0 = ? AND T1.CDCANDISTR = ? AND ");
            sqlQuery.Append("T1.CDGRPPRCCL = ? AND T1.CDPRD = ? AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[06];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CDGRPPRCCL", tmpVariaveis.CDGRPPRCCL);
            parametros[04] = new SQLiteParameter("@05PRODUTO", tmpVariaveis.PRODUTO);
            parametros[05] = new SQLiteParameter("@06DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA031()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA031 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDGER0 = ? ");
            sqlQuery.Append("   AND T1.CDCANDISTR = ? ");
            sqlQuery.Append("   AND T1.CDGRPPRCCL = ? ");
            sqlQuery.Append("   AND T1.CDGRPPRC = ? ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[06];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CDGRPPRCCL", tmpVariaveis.CDGRPPRCCL);
            parametros[04] = new SQLiteParameter("@05CDGRPPRC", tmpVariaveis.CDGRPPRC);
            parametros[05] = new SQLiteParameter("@06DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA916()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA916 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDGER0 = ? AND ");
            sqlQuery.Append("T1.CDCANDISTR = ? AND T1.CDPRD = ? AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[05];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04PRODUTO", tmpVariaveis.PRODUTO);
            parametros[04] = new SQLiteParameter("@05DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA925()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA925 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDGER0 = ? AND ");
            sqlQuery.Append("T1.CDCANDISTR = ? AND T1.CDGRPPRD = ? AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[05];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CDGRPPRD", tmpVariaveis.CDGRPPRD);
            parametros[04] = new SQLiteParameter("@05DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA903()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA903 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDGER0 = ? ");
            sqlQuery.Append("   AND T1.CDCANDISTR = ? ");
            sqlQuery.Append("   AND T1.CDCLIH IN (?,?,?,?) ");
            sqlQuery.Append("   AND T1.CDPRD = ? ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY CASE T1.CDCLIH WHEN ? THEN 4 ");
            sqlQuery.Append("                         WHEN ? THEN 3 ");
            sqlQuery.Append("                         WHEN ? THEN 2 ");
            sqlQuery.Append("                         WHEN ? THEN 1 ");
            sqlQuery.Append("          END, T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[13];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CDCLIN4", tmpVariaveis.CDCLIN4);
            parametros[04] = new SQLiteParameter("@05CDCLIN5", tmpVariaveis.CDCLIN5);
            parametros[05] = new SQLiteParameter("@06CDCLIN6", tmpVariaveis.CDCLIN6);
            parametros[06] = new SQLiteParameter("@07CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);
            parametros[07] = new SQLiteParameter("@08PRODUTO", tmpVariaveis.PRODUTO);
            parametros[08] = new SQLiteParameter("@09DATA", tmpVariaveis.DATA);
            parametros[09] = new SQLiteParameter("@10CDCLIN4", tmpVariaveis.CDCLIN4);
            parametros[10] = new SQLiteParameter("@11CDCLIN5", tmpVariaveis.CDCLIN5);
            parametros[11] = new SQLiteParameter("@12CDCLIN6", tmpVariaveis.CDCLIN6);
            parametros[12] = new SQLiteParameter("@13CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA945()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA945 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDGER0 = ? AND T1.CDCANDISTR = ? AND ");
            sqlQuery.Append("T1.CDCLASCLI = ? AND T1.CDTPNEG = ? AND T1.CDPONTOVEN = ? AND ");
            sqlQuery.Append("T1.CDPRD = ? AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[08];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CDCLASCLI", tmpVariaveis.CDCLASCLI);
            parametros[04] = new SQLiteParameter("@05CDTPNEG", tmpVariaveis.CDTPNEG);
            parametros[05] = new SQLiteParameter("@06CDPONTOVEN", tmpVariaveis.CDPONTOVEN);
            parametros[06] = new SQLiteParameter("@07PRODUTO", tmpVariaveis.PRODUTO);
            parametros[07] = new SQLiteParameter("@08DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA905()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT CASE WHEN T1.VLCONDTYP <> 0 THEN T1.VLCONDTYP / 1000.000 ELSE CASE ? WHEN 'X066' THEN (-1 * ?) ELSE 0 END END ");
            sqlQuery.Append("      ,T1.CDCONDUNID, T1.VLLIMINF, T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA905 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDGER0 = ? ");
            sqlQuery.Append("   AND T1.CDCANDISTR = ? ");
            sqlQuery.Append("   AND T1.CDCLIH IN (?,?,?,?) ");
            sqlQuery.Append("   AND T1.CDGRPPRD = ? ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY CASE T1.CDCLIH WHEN ? THEN 4 ");
            sqlQuery.Append("                         WHEN ? THEN 3 ");
            sqlQuery.Append("                         WHEN ? THEN 2 ");
            sqlQuery.Append("                         WHEN ? THEN 1 ");
            sqlQuery.Append("          END, T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[15];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02PERCDESCONTO", percentualDesconto.ToString().Replace(',', '.'));
            parametros[02] = new SQLiteParameter("@02CDCONDTYP", CDCONDTYP);
            parametros[03] = new SQLiteParameter("@03CDGER0", tmpVariaveis.CDGER0);
            parametros[04] = new SQLiteParameter("@04CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[05] = new SQLiteParameter("@06CDCLIN4", tmpVariaveis.CDCLIN4);
            parametros[06] = new SQLiteParameter("@07CDCLIN5", tmpVariaveis.CDCLIN5);
            parametros[07] = new SQLiteParameter("@08CDCLIN6", tmpVariaveis.CDCLIN6);
            parametros[08] = new SQLiteParameter("@09CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);
            parametros[09] = new SQLiteParameter("@10CDGRPPRD", tmpVariaveis.CDGRPPRD);
            parametros[10] = new SQLiteParameter("@11DATA", tmpVariaveis.DATA);
            parametros[11] = new SQLiteParameter("@12CDCLIN4", tmpVariaveis.CDCLIN4);
            parametros[12] = new SQLiteParameter("@13CDCLIN5", tmpVariaveis.CDCLIN5);
            parametros[13] = new SQLiteParameter("@14CDCLIN6", tmpVariaveis.CDCLIN6);
            parametros[14] = new SQLiteParameter("@15CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA924()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT CASE WHEN T1.VLCONDTYP <> 0 THEN T1.VLCONDTYP / 1000.000 ELSE CASE ? WHEN 'X066' THEN (-1 * ?) ELSE 0 END END ");
            sqlQuery.Append("      ,T1.CDCONDUNID, T1.VLLIMINF, T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA924 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDGER0 = ? ");
            sqlQuery.Append("   AND T1.CDCANDISTR = ? ");
            sqlQuery.Append("   AND T1.CDCLIH IN (?,?,?,?) ");
            sqlQuery.Append("   AND T1.CDMERCADO IN (?, '') ");
            sqlQuery.Append("   AND T1.CDNEGOCIO IN (?, '') ");
            sqlQuery.Append("   AND T1.CDCATEG IN (?, '') ");
            sqlQuery.Append("   AND T1.CDSUBCATEG IN (?, '') ");
            sqlQuery.Append("   AND T1.CDSEGMENTO IN (?, '') ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY CASE T1.CDCLIH WHEN ? THEN 4 ");
            sqlQuery.Append("                         WHEN ? THEN 3 ");
            sqlQuery.Append("                         WHEN ? THEN 2 ");
            sqlQuery.Append("                         WHEN ? THEN 1 ");
            sqlQuery.Append("          END ");
            sqlQuery.Append("          ,T1.CDSEGMENTO DESC");
            sqlQuery.Append("          ,T1.CDSUBCATEG DESC");
            sqlQuery.Append("          ,T1.CDCATEG DESC");
            sqlQuery.Append("          ,T1.CDNEGOCIO DESC");
            sqlQuery.Append("          ,T1.CDMERCADO DESC");
            sqlQuery.Append("          ,T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[19];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02PERCDESCONTO", percentualDesconto.ToString().Replace(',', '.'));
            parametros[02] = new SQLiteParameter("@03CDCONDTYP", CDCONDTYP);
            parametros[03] = new SQLiteParameter("@04CDGER0", tmpVariaveis.CDGER0);
            parametros[04] = new SQLiteParameter("@05CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[05] = new SQLiteParameter("@06CDCLIN4", tmpVariaveis.CDCLIN4);
            parametros[06] = new SQLiteParameter("@07CDCLIN5", tmpVariaveis.CDCLIN5);
            parametros[07] = new SQLiteParameter("@08CDCLIN6", tmpVariaveis.CDCLIN6);
            parametros[08] = new SQLiteParameter("@09CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);
            parametros[09] = new SQLiteParameter("@10CDMERCADO", tmpVariaveis.CDMERCADO);
            parametros[10] = new SQLiteParameter("@11CDNEGOCIO", tmpVariaveis.CDNEGOCIO);
            parametros[11] = new SQLiteParameter("@12CDCATEG", tmpVariaveis.CDCATEG);
            parametros[12] = new SQLiteParameter("@13CDSUBCATEG", tmpVariaveis.CDSUBCATEG);
            parametros[13] = new SQLiteParameter("@14CDSEGMENTO", tmpVariaveis.CDSEGMENTO);
            parametros[14] = new SQLiteParameter("@15DATA", tmpVariaveis.DATA);
            parametros[15] = new SQLiteParameter("@16CDCLIN4", tmpVariaveis.CDCLIN4);
            parametros[16] = new SQLiteParameter("@17CDCLIN5", tmpVariaveis.CDCLIN5);
            parametros[17] = new SQLiteParameter("@18CDCLIN6", tmpVariaveis.CDCLIN6);
            parametros[18] = new SQLiteParameter("@19CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA910()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA910 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDGER0 = ? AND T1.CDCANDISTR = ? AND ");
            sqlQuery.Append("T1.CDCLASCLI IN (?, '') AND T1.CDTPNEG IN (?, '') AND ");
            sqlQuery.Append("T1.CDPONTOVEN IN (?, '') AND T1.CDMERCADO IN (?, '') AND ");
            sqlQuery.Append("T1.CDNEGOCIO IN (?, '') AND T1.CDCATEG IN (?, '') AND ");
            sqlQuery.Append("T1.CDSUBCATEG IN (?, '') AND T1.CDSEGMENTO IN (?, '') AND ");
            sqlQuery.Append("? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[12];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CDCLASCLI", tmpVariaveis.CDCLASCLI);
            parametros[04] = new SQLiteParameter("@05CDTPNEG", tmpVariaveis.CDTPNEG);
            parametros[05] = new SQLiteParameter("@06CDPONTOVEN", tmpVariaveis.CDPONTOVEN);
            parametros[06] = new SQLiteParameter("@07CDMERCADO", tmpVariaveis.CDMERCADO);
            parametros[07] = new SQLiteParameter("@08CDNEGOCIO", tmpVariaveis.CDNEGOCIO);
            parametros[08] = new SQLiteParameter("@09CDCATEG", tmpVariaveis.CDCATEG);
            parametros[09] = new SQLiteParameter("@10CDSUBCATEG", tmpVariaveis.CDSUBCATEG);
            parametros[10] = new SQLiteParameter("@11CDSEGMENTO", tmpVariaveis.CDSEGMENTO);
            parametros[11] = new SQLiteParameter("@12DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA943()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA943 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDGER0 = ? AND T1.CDCANDISTR = ? AND ");
            sqlQuery.Append("T1.CDMERCADO IN (?, '') AND T1.CDNEGOCIO IN (?, '') AND ");
            sqlQuery.Append("T1.CDCATEG IN (?, '') AND T1.CDSUBCATEG IN (?, '') AND ");
            sqlQuery.Append("T1.CDSEGMENTO IN (?, '') AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[09];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CDMERCADO", tmpVariaveis.CDMERCADO);
            parametros[04] = new SQLiteParameter("@05CDNEGOCIO", tmpVariaveis.CDNEGOCIO);
            parametros[05] = new SQLiteParameter("@06CDCATEG", tmpVariaveis.CDCATEG);
            parametros[06] = new SQLiteParameter("@07CDSUBCATEG", tmpVariaveis.CDSUBCATEG);
            parametros[07] = new SQLiteParameter("@08CDSEGMENTO", tmpVariaveis.CDSEGMENTO);
            parametros[08] = new SQLiteParameter("@09DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA803()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID, T1.VLLIMINF, T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA803 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDGER0 = ? ");
            sqlQuery.Append("   AND T1.CDCANDISTR = ? ");
            sqlQuery.Append("   AND T1.CDCLIH IN (?,?,?,?) ");
            sqlQuery.Append("   AND T1.CDGRPPRC = ? ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY CASE T1.CDCLIH WHEN ? THEN 4 ");
            sqlQuery.Append("                         WHEN ? THEN 3 ");
            sqlQuery.Append("                         WHEN ? THEN 2 ");
            sqlQuery.Append("                         WHEN ? THEN 1 ");
            sqlQuery.Append("          END, T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[13];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CDCLIN4", tmpVariaveis.CDCLIN4);
            parametros[04] = new SQLiteParameter("@05CDCLIN5", tmpVariaveis.CDCLIN5);
            parametros[05] = new SQLiteParameter("@06CDCLIN6", tmpVariaveis.CDCLIN6);
            parametros[06] = new SQLiteParameter("@07CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);
            parametros[07] = new SQLiteParameter("@08CDGRPPRC", tmpVariaveis.CDGRPPRC);
            parametros[08] = new SQLiteParameter("@09DATA", tmpVariaveis.DATA);
            parametros[09] = new SQLiteParameter("@10CDCLIN4", tmpVariaveis.CDCLIN4);
            parametros[10] = new SQLiteParameter("@11CDCLIN5", tmpVariaveis.CDCLIN5);
            parametros[11] = new SQLiteParameter("@12CDCLIN6", tmpVariaveis.CDCLIN6);
            parametros[12] = new SQLiteParameter("@13CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);

            ExecutaRegra(sqlQuery.ToString(), false, parametros);
        }

        private void ExecutaRegraEPA906()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT CASE WHEN T1.VLCONDTYP <> 0 THEN T1.VLCONDTYP / 1000.000 ELSE CASE ? WHEN 'X066' THEN (-1 * ?) ELSE 0 END END ");
            sqlQuery.Append("      ,T1.CDCONDUNID, T1.VLLIMINF, T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA906 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDGER0 = ? ");
            sqlQuery.Append("   AND T1.CDCANDISTR = ? ");
            sqlQuery.Append("   AND T1.CDCLIH IN (?, ?, ?, ?) ");
            sqlQuery.Append("   AND T1.CDPRD = ? ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY CASE T1.CDCLIH WHEN ? THEN 4 ");
            sqlQuery.Append("                         WHEN ? THEN 3 ");
            sqlQuery.Append("                         WHEN ? THEN 2 ");
            sqlQuery.Append("                         WHEN ? THEN 1 ");
            sqlQuery.Append("          END, T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[15];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02PERCDESCONTO", percentualDesconto.ToString().Replace(',', '.'));
            parametros[02] = new SQLiteParameter("@03CDCONDTYP", CDCONDTYP);
            parametros[03] = new SQLiteParameter("@04CDGER0", tmpVariaveis.CDGER0);
            parametros[04] = new SQLiteParameter("@05CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[05] = new SQLiteParameter("@06CDCLIN4", tmpVariaveis.CDCLIN4);
            parametros[06] = new SQLiteParameter("@07CDCLIN5", tmpVariaveis.CDCLIN5);
            parametros[07] = new SQLiteParameter("@08CDCLIN6", tmpVariaveis.CDCLIN6);
            parametros[08] = new SQLiteParameter("@09CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);
            parametros[09] = new SQLiteParameter("@10PRODUTO", tmpVariaveis.PRODUTO);
            parametros[10] = new SQLiteParameter("@11DATA", tmpVariaveis.DATA);
            parametros[11] = new SQLiteParameter("@12CDCLIN4", tmpVariaveis.CDCLIN4);
            parametros[12] = new SQLiteParameter("@13CDCLIN5", tmpVariaveis.CDCLIN5);
            parametros[13] = new SQLiteParameter("@14CDCLIN6", tmpVariaveis.CDCLIN6);
            parametros[14] = new SQLiteParameter("@15CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA915()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA915 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDGER0 = ? AND T1.CDCANDISTR = ? AND ");
            sqlQuery.Append("T1.CDDIVISAO = '00' AND T1.CDCLI = ? AND T1.CDMERCADO IN (?, '') AND ");
            sqlQuery.Append("T1.CDNEGOCIO IN (?, '') AND T1.CDCATEG IN (?, '') AND ");
            sqlQuery.Append("T1.CDSUBCATEG IN (?, '') AND T1.CDSEGMENTO IN (?, '') AND ");
            sqlQuery.Append("T1.IDSITUACAO = '' AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[10];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);
            parametros[04] = new SQLiteParameter("@05CDMERCADO", tmpVariaveis.CDMERCADO);
            parametros[05] = new SQLiteParameter("@06CDNEGOCIO", tmpVariaveis.CDNEGOCIO);
            parametros[06] = new SQLiteParameter("@07CDCATEG", tmpVariaveis.CDCATEG);
            parametros[07] = new SQLiteParameter("@08CDSUBCATEG", tmpVariaveis.CDSUBCATEG);
            parametros[08] = new SQLiteParameter("@09CDSEGMENTO", tmpVariaveis.CDSEGMENTO);
            parametros[09] = new SQLiteParameter("@10DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA904()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA904 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDGER0 = ? AND T1.CDCANDISTR = ? AND ");
            sqlQuery.Append("T1.CDCLI = ? AND T1.CDGRPPRD = ? AND ");
            sqlQuery.Append("? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[06];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);
            parametros[04] = new SQLiteParameter("@05CDGRPPRD", tmpVariaveis.CDGRPPRD);
            parametros[05] = new SQLiteParameter("@06DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA947()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA947 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDGER0 = ? ");
            sqlQuery.Append("   AND T1.CDCANDISTR = ? ");
            sqlQuery.Append("   AND T1.CDCLIH IN (?,?,?,?) ");
            sqlQuery.Append("   AND T1.CDMERCADO IN (?, '') ");
            sqlQuery.Append("   AND T1.CDNEGOCIO IN (?, '') ");
            sqlQuery.Append("   AND T1.CDCATEG IN (?, '') ");
            sqlQuery.Append("   AND T1.CDSUBCATEG IN (?, '') ");
            sqlQuery.Append("   AND T1.CDSEGMENTO IN (?, '') ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY CASE T1.CDCLIH WHEN ? THEN 4 ");
            sqlQuery.Append("                         WHEN ? THEN 3 ");
            sqlQuery.Append("                         WHEN ? THEN 2 ");
            sqlQuery.Append("                         WHEN ? THEN 1 ");
            sqlQuery.Append("          END");
            sqlQuery.Append("          ,T1.CDSEGMENTO DESC");
            sqlQuery.Append("          ,T1.CDSUBCATEG DESC");
            sqlQuery.Append("          ,T1.CDCATEG DESC");
            sqlQuery.Append("          ,T1.CDNEGOCIO DESC");
            sqlQuery.Append("          ,T1.CDMERCADO DESC");
            sqlQuery.Append("          ,T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[17];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CDCLIN4", tmpVariaveis.CDCLIN4);
            parametros[04] = new SQLiteParameter("@05CDCLIN5", tmpVariaveis.CDCLIN5);
            parametros[05] = new SQLiteParameter("@06CDCLIN6", tmpVariaveis.CDCLIN6);
            parametros[06] = new SQLiteParameter("@07CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);
            parametros[07] = new SQLiteParameter("@08CDMERCADO", tmpVariaveis.CDMERCADO);
            parametros[08] = new SQLiteParameter("@09CDNEGOCIO", tmpVariaveis.CDNEGOCIO);
            parametros[09] = new SQLiteParameter("@10CDCATEG", tmpVariaveis.CDCATEG);
            parametros[10] = new SQLiteParameter("@11CDSUBCATEG", tmpVariaveis.CDSUBCATEG);
            parametros[11] = new SQLiteParameter("@12CDSEGMENTO", tmpVariaveis.CDSEGMENTO);
            parametros[12] = new SQLiteParameter("@13DATA", tmpVariaveis.DATA);
            parametros[13] = new SQLiteParameter("@14CDCLIN4", tmpVariaveis.CDCLIN4);
            parametros[14] = new SQLiteParameter("@15CDCLIN5", tmpVariaveis.CDCLIN5);
            parametros[15] = new SQLiteParameter("@16CDCLIN6", tmpVariaveis.CDCLIN6);
            parametros[16] = new SQLiteParameter("@17CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA816()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000, T1.CDCONDUNID, T1.VLLIMINF, T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA816 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDPAIS = 'BR' ");
            sqlQuery.Append("   AND T1.CDGRCSUTR = ? ");
            sqlQuery.Append("   AND T1.CDREGFORIG = ? ");
            sqlQuery.Append("   AND T1.CDREGFDEST = ? ");
            sqlQuery.Append("   AND T1.CDPRD = ? ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[06];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGRCSUTR", tmpVariaveis.CDGRCSUTR);
            parametros[02] = new SQLiteParameter("@03CDREGFORIG", tmpVariaveis.CDREGFISCAFIL);
            parametros[03] = new SQLiteParameter("@04CDREGFDEST", tmpVariaveis.CDREGFISCACLI);
            parametros[04] = new SQLiteParameter("@05CDPRD", tmpVariaveis.PRODUTO);
            parametros[05] = new SQLiteParameter("@06DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA382()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000, T1.CDCONDUNID, T1.VLLIMINF, T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA382 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDPAIS = 'BR' ");
            sqlQuery.Append("   AND T1.IDTIPODOC = '0' ");
            sqlQuery.Append("   AND T1.CDREGFORIG = ? ");
            sqlQuery.Append("   AND T1.CDREGFDEST = ? ");
            sqlQuery.Append("   AND ((? = 'ZSTI' AND ? = 2 ");
            sqlQuery.Append("   AND ((T1.CDDINAMIC1 = ? ");
            sqlQuery.Append("   AND T1.CDDINAMIC2 = 'X' AND T1.CDDINAMIC3 = 'X' AND T1.CDGRPDINAM = '5')))) ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[07];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDREGFORIG", tmpVariaveis.CDREGFISCAFIL);
            parametros[02] = new SQLiteParameter("@03CDREGFDEST", tmpVariaveis.CDREGFISCACLI);
            parametros[03] = new SQLiteParameter("@04CDCONDSEQ", CDCONDSEQ);
            parametros[04] = new SQLiteParameter("@05NMSEQTAB", NMSEQTAB);
            parametros[05] = new SQLiteParameter("@06CDDINAMIC1", tmpVariaveis.PRODUTO);
            parametros[06] = new SQLiteParameter("@07DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA392()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA392 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDPAIS = 'BR' AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[02];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), false, parametros);
        }

        private void ExecutaRegraEPA823()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA823 T1  ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDPAIS = 'BR' ");
            sqlQuery.Append("   AND T1.CDPRD = ? ");
            sqlQuery.Append("   AND T1.IDSITUACAO = '' ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[03];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02PRODUTO", tmpVariaveis.PRODUTO);
            parametros[02] = new SQLiteParameter("@03DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA832()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA832 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDGER0 = ? AND T1.CDCANDISTR = ? ");
            sqlQuery.Append("   AND T1.CDPRZPAG = ? AND T1.CDCLI = ? ");
            sqlQuery.Append("   AND T1.CDPRD = ? AND T1.IDSITUACAO = '' ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[07];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CONDPAGTO", tmpVariaveis.CONDPAGTO);
            parametros[04] = new SQLiteParameter("@05CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);
            parametros[05] = new SQLiteParameter("@06PRODUTO", tmpVariaveis.PRODUTO);
            parametros[06] = new SQLiteParameter("@07DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA833()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA833 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDGER0 = ? ");
            sqlQuery.Append("   AND T1.CDCANDISTR = ? ");
            sqlQuery.Append("   AND T1.CDPRZPAG = ? ");
            sqlQuery.Append("   AND T1.CDCLIH IN (?,?,?,?) ");
            sqlQuery.Append("   AND T1.CDMERCADO IN (?, '') ");
            sqlQuery.Append("   AND T1.CDNEGOCIO IN (?, '') ");
            sqlQuery.Append("   AND T1.CDCATEG IN (?, '') ");
            sqlQuery.Append("   AND T1.CDSUBCATEG IN (?, '') ");
            sqlQuery.Append("   AND T1.CDSEGMENTO IN (?, '') ");
            sqlQuery.Append("   AND T1.IDSITUACAO = '' ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY CASE T1.CDCLIH WHEN ? THEN 4 ");
            sqlQuery.Append("                         WHEN ? THEN 3 ");
            sqlQuery.Append("                         WHEN ? THEN 2 ");
            sqlQuery.Append("                         WHEN ? THEN 1 ");
            sqlQuery.Append("          END");
            sqlQuery.Append("          ,T1.CDSEGMENTO DESC");
            sqlQuery.Append("          ,T1.CDSUBCATEG DESC");
            sqlQuery.Append("          ,T1.CDCATEG DESC");
            sqlQuery.Append("          ,T1.CDNEGOCIO DESC");
            sqlQuery.Append("          ,T1.CDMERCADO DESC");
            sqlQuery.Append("          ,T1.NMCONDREC DESC ");


            SQLiteParameter[] parametros = new SQLiteParameter[18];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CONDPAGTO", tmpVariaveis.CONDPAGTO);
            parametros[04] = new SQLiteParameter("@05CDCLIN4", tmpVariaveis.CDCLIN4);
            parametros[05] = new SQLiteParameter("@06CDCLIN5", tmpVariaveis.CDCLIN5);
            parametros[06] = new SQLiteParameter("@07CDCLIN6", tmpVariaveis.CDCLIN6);
            parametros[07] = new SQLiteParameter("@08CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);
            parametros[08] = new SQLiteParameter("@09CDMERCADO", tmpVariaveis.CDMERCADO);
            parametros[09] = new SQLiteParameter("@10CDNEGOCIO", tmpVariaveis.CDNEGOCIO);
            parametros[10] = new SQLiteParameter("@11CDCATEG", tmpVariaveis.CDCATEG);
            parametros[11] = new SQLiteParameter("@12CDSUBCATEG", tmpVariaveis.CDSUBCATEG);
            parametros[12] = new SQLiteParameter("@13CDSEGMENTO", tmpVariaveis.CDSEGMENTO);
            parametros[13] = new SQLiteParameter("@14DATA", tmpVariaveis.DATA);
            parametros[14] = new SQLiteParameter("@15CDCLIN4", tmpVariaveis.CDCLIN4);
            parametros[15] = new SQLiteParameter("@16CDCLIN5", tmpVariaveis.CDCLIN5);
            parametros[16] = new SQLiteParameter("@17CDCLIN6", tmpVariaveis.CDCLIN6);
            parametros[17] = new SQLiteParameter("@18CLIENTE_SOLDTO", tmpVariaveis.CLIENTE_SOLDTO);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA834()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID, T1.VLLIMINF, T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA834 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDGER0 = ? ");
            sqlQuery.Append("   AND T1.CDCANDISTR = ? ");
            sqlQuery.Append("   AND T1.CDPRZPAG = ? ");
            sqlQuery.Append("   AND T1.CDMERCADO IN (?, '') ");
            sqlQuery.Append("   AND T1.CDNEGOCIO IN (?, '') ");
            sqlQuery.Append("   AND T1.CDCATEG IN (?, '') ");
            sqlQuery.Append("   AND T1.CDSUBCATEG IN (?, '') ");
            sqlQuery.Append("   AND T1.IDSITUACAO = '' ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[09];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CONDPAGTO", tmpVariaveis.CONDPAGTO);
            parametros[04] = new SQLiteParameter("@05CDMERCADO", tmpVariaveis.CDMERCADO);
            parametros[05] = new SQLiteParameter("@06CDNEGOCIO", tmpVariaveis.CDNEGOCIO);
            parametros[06] = new SQLiteParameter("@07CDCATEG", tmpVariaveis.CDCATEG);
            parametros[07] = new SQLiteParameter("@08CDSUBCATEG", tmpVariaveis.CDSUBCATEG);
            parametros[08] = new SQLiteParameter("@09DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA835()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA835 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDGER0 = ? ");
            sqlQuery.Append("   AND T1.CDCANDISTR = ? ");
            sqlQuery.Append("   AND T1.CDPRZPAG = ? ");
            sqlQuery.Append("   AND T1.CDCLASCLI = ? ");
            sqlQuery.Append("   AND T1.CDTPNEG = ? ");
            sqlQuery.Append("   AND T1.CDPONTOVEN = ? ");
            sqlQuery.Append("   AND T1.CDPRD = ? ");
            sqlQuery.Append("   AND T1.IDSITUACAO = '' ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[09];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CONDPAGTO", tmpVariaveis.CONDPAGTO);
            parametros[04] = new SQLiteParameter("@05CDCLASCLI", tmpVariaveis.CDCLASCLI);
            parametros[05] = new SQLiteParameter("@06CDTPNEG", tmpVariaveis.CDTPNEG);
            parametros[06] = new SQLiteParameter("@07CDPONTOVEN", tmpVariaveis.CDPONTOVEN);
            parametros[07] = new SQLiteParameter("@08PRODUTO", tmpVariaveis.PRODUTO);
            parametros[08] = new SQLiteParameter("@09DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA836()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("  FROM BRK_EPRECOA836 T1 ");
            sqlQuery.Append(" WHERE T1.CDAPLICAC = 'V' ");
            sqlQuery.Append("   AND T1.CDCONDTYP = ? ");
            sqlQuery.Append("   AND T1.CDGER0 = ? ");
            sqlQuery.Append("   AND T1.CDCANDISTR = ? ");
            sqlQuery.Append("   AND T1.CDPRZPAG = ? ");
            sqlQuery.Append("   AND T1.CDCLASCLI = ? ");
            sqlQuery.Append("   AND T1.CDTPNEG = ? ");
            sqlQuery.Append("   AND T1.CDPONTOVEN = ? ");
            sqlQuery.Append("   AND T1.CDMERCADO IN (?, '') ");
            sqlQuery.Append("   AND T1.CDNEGOCIO IN (?, '') ");
            sqlQuery.Append("   AND T1.CDCATEG IN (?, '') ");
            sqlQuery.Append("   AND T1.CDSUBCATEG IN (?, '') ");
            sqlQuery.Append("   AND T1.CDSEGMENTO IN (?, '') ");
            sqlQuery.Append("   AND T1.IDSITUACAO = '' ");
            sqlQuery.Append("   AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[13];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CONDPAGTO", tmpVariaveis.CONDPAGTO);
            parametros[04] = new SQLiteParameter("@05CDCLASCLI", tmpVariaveis.CDCLASCLI);
            parametros[05] = new SQLiteParameter("@06CDTPNEG", tmpVariaveis.CDTPNEG);
            parametros[06] = new SQLiteParameter("@07CDPONTOVEN", tmpVariaveis.CDPONTOVEN);
            parametros[07] = new SQLiteParameter("@08CDMERCADO", tmpVariaveis.CDMERCADO);
            parametros[08] = new SQLiteParameter("@09CDNEGOCIO", tmpVariaveis.CDNEGOCIO);
            parametros[09] = new SQLiteParameter("@10CDCATEG", tmpVariaveis.CDCATEG);
            parametros[10] = new SQLiteParameter("@11CDSUBCATEG", tmpVariaveis.CDSUBCATEG);
            parametros[11] = new SQLiteParameter("@12CDSEGMENTO", tmpVariaveis.CDSEGMENTO);
            parametros[12] = new SQLiteParameter("@13DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA837()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA837 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDGER0 = ? AND T1.CDCANDISTR = ? AND ");
            sqlQuery.Append("T1.CDPRZPAG = ? AND T1.IDSITUACAO = '' AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[05];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDGER0", tmpVariaveis.CDGER0);
            parametros[02] = new SQLiteParameter("@03CDCANDISTR", tmpVariaveis.CDCANDISTR);
            parametros[03] = new SQLiteParameter("@04CONDPAGTO", tmpVariaveis.CONDPAGTO);
            parametros[04] = new SQLiteParameter("@05DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA880()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA880 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDPAIS = 'BR' AND ");
            sqlQuery.Append("T1.CDREGFISCA = ? AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[03];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDREGFISCACLI", tmpVariaveis.CDREGFISCACLICOMP);
            parametros[02] = new SQLiteParameter("@03DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), false, parametros);
        }
        private void ExecutaRegraEPA341()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA341 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDPAIS = 'BR' AND ");
            sqlQuery.Append("T1.IDCLIIMP = ? AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[03];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02IDCLIIMP", tmpVariaveis.IDCLIIMP);
            parametros[02] = new SQLiteParameter("@03DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA394()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA394 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDPAIS = 'BR' AND ");
            sqlQuery.Append("T1.CDREGFDEST = ? AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[03];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDREGFDEST", tmpVariaveis.CDREGFISCACLI);
            parametros[02] = new SQLiteParameter("@03DATA", tmpVariaveis.DATA);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA778()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA778 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDPAIS = 'BR' AND ");
            sqlQuery.Append("T1.CDREGFDEST = ? AND T1.CDCLASFISC = ? AND T1.IDSITUACAO = '' AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM AND T1.CDFILFAT = ? ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[05];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDREGFDEST", tmpVariaveis.CDREGFISCACLI);
            parametros[02] = new SQLiteParameter("@03CDCLASFISC", tmpVariaveis.CDCLASFISC);
            parametros[03] = new SQLiteParameter("@04DATA", tmpVariaveis.DATA);
            parametros[04] = new SQLiteParameter("@05CDFILFAT", tmpVariaveis.CDFILFAT);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA779()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA779 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDPAIS = 'BR' AND ");
            sqlQuery.Append("T1.CDREGFDEST = ? AND T1.CDPRD = ? AND T1.IDSITUACAO = '' AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM AND T1.CDFILFAT = ? ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[05];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDREGFDEST", tmpVariaveis.CDREGFISCACLI);
            parametros[02] = new SQLiteParameter("@03PRODUTO", tmpVariaveis.PRODUTO);
            parametros[03] = new SQLiteParameter("@04DATA", tmpVariaveis.DATA);
            parametros[04] = new SQLiteParameter("@05CDFILFAT", tmpVariaveis.CDFILFAT);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        private void ExecutaRegraEPA793()
        {
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T1.VLCONDTYP / 1000.000,T1.CDCONDUNID,T1.VLLIMINF,T1.VLLIMSUP ");
            sqlQuery.Append("FROM BRK_EPRECOA793 T1 ");
            sqlQuery.Append("WHERE T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = ? AND T1.CDPAIS = 'BR' AND ");
            sqlQuery.Append("T1.CDREGFDEST = ? AND T1.IDCLIIMP = ? AND T1.IDSITUACAO = '' AND ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM AND T1.CDFILFAT = ? ");
            sqlQuery.Append(" ORDER BY T1.NMCONDREC DESC ");

            SQLiteParameter[] parametros = new SQLiteParameter[05];

            parametros[00] = new SQLiteParameter("@01CDCONDTYP", CDCONDTYP);
            parametros[01] = new SQLiteParameter("@02CDREGFDEST", tmpVariaveis.CDREGFISCACLI);
            parametros[02] = new SQLiteParameter("@03IDCLIIMP", tmpVariaveis.IDCLIIMP);
            parametros[03] = new SQLiteParameter("@04DATA", tmpVariaveis.DATA);
            parametros[04] = new SQLiteParameter("@05CDFILFAT", tmpVariaveis.CDFILFAT);

            ExecutaRegra(sqlQuery.ToString(), true, parametros);
        }

        #endregion

        #region [ Fórmulas ]

        private decimal? CalculaFormulaF616()
        {
            decimal? result = null;

            try
            {
                result = (CSGlobal.Round((((CSGlobal.Round(((VALORRET.Value / ((quantidadeInteira * quantidadeCaixa) + FATORFRACAO)) *
                    quantidadeCaixa), 2)) * VALOR) / 100m), 2) / quantidadeCaixa *
                    ((quantidadeInteira * quantidadeCaixa) + FATORFRACAO) * ((VALOR == 0) ? 0m : 1m));

            }
            catch
            {
                result = null;
            }

            return result;
        }

        private decimal? CalculaFormulaF904()
        {
            decimal? result = null;

            try
            {
                result = (CSGlobal.Round((((CSGlobal.Round(((VALORRET.Value / ((quantidadeInteira * quantidadeCaixa) + FATORFRACAO)) *
                    quantidadeCaixa), 2)) * VALOR) / 100m), 2) / quantidadeCaixa *
                    ((quantidadeInteira * quantidadeCaixa) + FATORFRACAO) * ((VALOR == 0) ? 0m : 1m));

            }
            catch
            {
                result = null;
            }

            return result;
        }

        private decimal? CalculaFormulaF906()
        {
            decimal? result = null;

            try
            {
                if (VALOR == 0m)
                {
                    result = 0m;

                }
                else
                {
                    if (VALOR == 1)
                        result = GetSumTmpPricingCons(480, 483);
                    else
                        result = GetSumTmpPricingCons(485, 485);
                }

            }
            catch
            {
                result = null;
            }

            return result;
        }

        private decimal? CalculaFormulaF969()
        {
            decimal? result = null;

            try
            {
                if (VALOR == 0m)
                {
                    result = GetSumTmpPricingCons(NMCONDINI_ANT, NMCONDFIM_ANT);

                }
                else
                {
                    result = GetSumTmpPricingCons(NMCONDINI_ANT, NMCONDFIM_ANT);

                    if (!result.HasValue)
                        result = 0m;

                    result = CSGlobal.Round((1m - (VALOR / 100m)) * result.Value, 2);
                }

            }
            catch
            {
                result = null;
            }

            return result;
        }

        private decimal? CalculaFormulaF973()
        {
            decimal? result = null;

            try
            {
                result = (VALOR == 0m) ? 0m : GetSumTmpPricingCons(NMCONDINI_ANT, NMCONDFIM_ANT);

            }
            catch
            {
                result = null;
            }

            return result;
        }

        private decimal? CalculaFormulaF615()
        {
            decimal? result = null;

            try
            {
                result = CSGlobal.Round(((((VALORRET.Value / ((quantidadeInteira * quantidadeCaixa) + FATORFRACAO)) * (quantidadeInteira * quantidadeCaixa)) *
                    VALOR) / 100m), 2) + ((quantidadeFracao > 0) ? CSGlobal.Round((((CSGlobal.Round((VALORRET.Value / ((quantidadeInteira * quantidadeCaixa) + FATORFRACAO)), 2) *
                    quantidadeFracao) * VALOR) / 100m), 2) : 0m) * ((VALOR == 0) ? 0 : 1m);

            }
            catch
            {
                result = null;
            }

            return result;
        }
        private decimal? CalculaFormulaF974()
        {
            decimal? result = null;
            decimal? val = 0m;
            decimal formulaVal = 0m;

            try
            {
                val = GetSumTmpPricingCons(284, 284); // 284 = ISTF
                formulaVal = (val.HasValue) ? val.Value : 0m;

                if (formulaVal > 0m)
                {
                    //result = VALORRET.Value;
                    result = CSGlobal.Round(formulaVal, 2);

                }
                else
                {
                    val = GetSumTmpPricingCons(NMCONDLIN_ANT, NMCONDINI_ANT, NMCONDFIM_ANT, NMCONDINI_ANT, NMCONDINI_ANT);
                    formulaVal = (val.HasValue) ? val.Value : 0m;
                    result = CSGlobal.Round(formulaVal, 2);
                }
            }
            catch
            {
                result = null;
            }

            return result;
        }
        private decimal? CalculaFormulaF975()
        {
            decimal? result = null;

            try
            {
                if (GetDadoTmpPricingCons(285) != 0 || //ISTB
                    GetDadoTmpPricingCons(281) != 0)  // ISTS
                {
                    decimal? val = GetSumTmpPricingCons(NMCONDLIN_ANT, NMCONDINI_ANT, NMCONDFIM_ANT, NMCONDINI_ANT, NMCONDINI_ANT);
                    decimal formulaVal = (val.HasValue) ? val.Value : 0m;

                    result = CSGlobal.Round(formulaVal, 2) * (1m - (VALOR / 100m));
                }
                else
                    result = 0;
            }
            catch
            {
                result = null;
            }

            return result;
        }


        private decimal? CalculaFormulaDefaultPasso2()
        {
            decimal? result = null;

            try
            {
                result = CSGlobal.Round((((CSGlobal.Round((VALORRET.HasValue ? VALORRET.Value : 0m), 2)) * VALOR) / 100m), 2) * ((VALOR == 0m) ? 0m : 1m);

            }
            catch
            {
                result = null;
            }

            return result;
        }

        private decimal? CalculaFormulaBRL()
        {
            decimal? result = null;

            try
            {
                if (CDCONDTYP_ANT == "IPIP")
                {
                    //result = ((VALORRET.HasValue) ? VALORRET.Value : VALOR);
                    result = ((VALORRET.HasValue) ? VALORRET.Value : VALOR) * (tmpVariaveis.PESOTOTAL * (quantidadeInteira * quantidadeCaixa + quantidadeFracao));
                }
                else
                {
                    // NOVAPRICING
                    //31/01/2013 - FOI RETIRADO A CONDIÇÃO 'NMCONDLIN_ANT == 247', POIS APRESENTAVA DIVERGÊNCIA COM O FLEXX
                    if (CDCONDTYP_ANT == "YX23" || CDCONDTYP_ANT == "YX41" || CDCONDTYP_ANT == "YYTA" || NMCONDLIN_ANT == 764 || NMCONDLIN_ANT == 771) //776
                    //if (CDCONDTYP_ANT == "YX23" || CDCONDTYP_ANT == "YX41" || CDCONDTYP_ANT == "YYTA" || DSFORMULA_ANT == "F905" || DSFORMULA_ANT == "F907")                        
                    {
                        //Calcula IPI */
                        result = VALORRET;

                    }
                    else
                    {
                        if (VALORRET.HasValue)
                        {
                            result = CSGlobal.Round(((((VALORRET.Value / ((quantidadeInteira * quantidadeCaixa) + FATORFRACAO)) * (quantidadeInteira * quantidadeCaixa)) * VALOR) / 100m), 2) +
                                ((quantidadeFracao > 0) ? CSGlobal.Round((((CSGlobal.Round((VALORRET.Value / ((quantidadeInteira * quantidadeCaixa) + FATORFRACAO)), 2) *
                                quantidadeFracao) * VALOR) / 100m), 2) : 0m);
                        }
                    }
                }
            }
            catch
            {
                result = null;
            }

            return result;
        }

        private decimal? CalculaFormulaIsNullPasso2()
        {
            decimal? result = null;

            try
            {
                if (VALOR == 0m)
                    result = 0m;
                else
                {
                    if (DSFORMULA_ANT == "F608")
                        result = (VALOR / quantidadeCaixa) * ((quantidadeInteira * quantidadeCaixa) + ((quantidadeFracao > 0) ? quantidadeCaixa : 0m));
                    else
                    {
                        if (DSFORMULA_ANT == "F971")
                            result = VALOR;
                        else
                            result = (VALOR / quantidadeCaixa) * ((quantidadeInteira * quantidadeCaixa) + quantidadeFracao);
                    }
                }
            }
            catch
            {
                result = null;
            }

            return result;
        }

        private decimal? CalculaFormulaF164()
        {
            decimal? result = null;

            try
            {
                decimal? val = GetSumTmpPricingCons(NMCONDLIN_ANT, NMCONDINI_ANT, NMCONDFIM_ANT, NMCONDINI_ANT, NMCONDLIN_ANT);
                decimal formulaVal = (val.HasValue) ? val.Value : 0;

                result = (tmpVariaveis.IDISENIPI == "X") ? formulaVal * -1m : 0m;

            }
            catch
            {
                result = null;
            }

            return result;
        }

        private decimal? CalculaFormulaF320()
        {
            decimal? result = null;

            try
            {
                decimal? val = 0m;
                decimal formulaVal1 = 0m;

                if (CDCONDTYP_ANT == "BX10")
                {
                    if (GetDadoTmpPricingCons(202) != 0m) // 202 = ICBS
                    {
                        val = GetSumTmpPricingCons(726, 726); // 700 = IBRX
                        formulaVal1 = (val.HasValue) ? val.Value : 0m;

                        val = GetDadoTmpPricingCons(202); // 202 = ICBS
                        formulaVal1 = Convert.ToDecimal(formulaVal1 * val);
                    }
                }
                if (CDCONDTYP_ANT == "BX11")
                {
                    val = GetSumTmpPricingCons(726, 726); // 700 = IBRX
                    formulaVal1 = (val.HasValue) ? val.Value : 0m;

                    if (GetDadoTmpPricingCons(202) != 0m) // 202 = ICBS
                    {
                        val = GetDadoTmpPricingCons(202); // 202 = ICBS
                        val = (val.HasValue) ? val.Value : 0m;

                        formulaVal1 = Convert.ToDecimal(formulaVal1 - (formulaVal1 * val));
                    }
                }

                if (CDCONDTYP_ANT == "BX12" ||
                    CDCONDTYP_ANT == "BX14" ||
                    CDCONDTYP_ANT == "BX15" ||
                    CDCONDTYP_ANT == "BX16" ||
                    CDCONDTYP_ANT == "BX42" ||
                    CDCONDTYP_ANT == "BX43" ||
                    CDCONDTYP_ANT == "BX44" ||
                    CDCONDTYP_ANT == "YXZF")
                {
                    formulaVal1 = 0;
                }
                if (CDCONDTYP_ANT == "BX13")
                {
                    if (GetDadoTmpPricingCons(202) != 0m) // 202 = ICBS
                    {
                        val = GetSumTmpPricingCons(697, 697); // 697 = ICVA
                        formulaVal1 = (val.HasValue) ? val.Value : 0m;
                    }
                }
                if (CDCONDTYP_ANT == "BX20")
                {
                    if (GetDadoTmpPricingCons(242) != 0m) // 242 = IPBS
                    {
                        val = GetSumTmpPricingCons(726, 726); // 700 = IBRX
                        formulaVal1 = (val.HasValue) ? val.Value : 0m;
                    }
                }
                if (CDCONDTYP_ANT == "BX21")
                {
                    if (GetDadoTmpPricingCons(247) != 0m) // 247 = IPXC
                    {
                        val = GetSumTmpPricingCons(726, 726); // 700 = IBRX
                        formulaVal1 = (val.HasValue) ? val.Value : 0m;
                    }
                }
                if (CDCONDTYP_ANT == "BX22")
                {
                    if (GetDadoTmpPricingCons(243) != 0m) // 243 = IPOB
                    {
                        val = GetSumTmpPricingCons(726, 726); // 700 = IBRX
                        formulaVal1 = (val.HasValue) ? val.Value : 0m;
                    }
                }
                if (CDCONDTYP_ANT == "BX40")
                {
                    if (GetSumTmpPricingCons(284, 284) == 0 && GetDadoTmpPricingCons(281) != 0m) // 284 = ISTF | 281 = ISTS  
                    {
                        val = GetSumTmpPricingCons(726, 726); // 726 = IBRX
                        formulaVal1 = (val.HasValue) ? val.Value : 0m;

                        val = GetDadoTmpPricingCons(281); // 281 = ISTS
                        formulaVal1 = formulaVal1 * (1m + ((val.HasValue) ? val.Value : 0m));

                        val = GetDadoTmpPricingCons(285); // 285 = ISTB
                        formulaVal1 = formulaVal1 * (1m - ((val.HasValue) ? val.Value : 0m));

                    }
                    else
                    {
                        val = GetSumTmpPricingCons(284, 284); // 284 = ISTF
                        formulaVal1 = (val.HasValue) ? val.Value : 0m;

                        val = GetDadoTmpPricingCons(281); // 281 = ISTS
                        formulaVal1 = formulaVal1 * (1m + ((val.HasValue) ? val.Value : 0m));

                    }
                }
                if (CDCONDTYP_ANT == "BX41")
                {
                    if (GetSumTmpPricingCons(742, 742) != 0m)
                    {
                        val = GetSumTmpPricingCons(742, 742); // 726 = BX40
                        formulaVal1 = (val.HasValue) ? val.Value : 0m;

                        val = GetDadoTmpPricingCons(201); // 201 = ICVA
                        formulaVal1 = formulaVal1 * ((val.HasValue) ? val.Value : 0m);

                        val = GetSumTmpPricingCons(733, 733); // 708 = BX13
                        formulaVal1 = formulaVal1 - ((val.HasValue) ? val.Value : 0m);
                    }
                    else
                        formulaVal1 = 0m;
                }

                if (CDCONDTYP_ANT == "YXZF")
                {
                    if (VALOR == 100)
                    {
                        val = GetSumTmpPricingCons(733, 733); // 708 = BX13
                        formulaVal1 = (val.HasValue) ? (val.Value * -1) : 0m;
                    }
                    else
                        formulaVal1 = 0;
                }

                if (CDCONDTYP_ANT == "ICMI")
                {

                    if (GetDadoTmpPricingCons(230) != 0m) // 230 = DISI
                    {
                        val = GetDadoTmpPricingCons(201); // 201 = ICVA
                        formulaVal1 = (val.HasValue) ? val.Value : 0m;

                        val = GetDadoTmpPricingCons(202); // 202 = ICBS
                        formulaVal1 = (1m - (formulaVal1 * ((val.HasValue) ? val.Value : 0m)));

                        val = GetSumTmpPricingCons(140, 140); // 140 = Nulo
                        formulaVal1 = ((val.HasValue) ? val.Value : 0m) / formulaVal1;
                        formulaVal1 = CSGlobal.Round(formulaVal1, 2);

                    }
                    else
                    {
                        if (GetDadoTmpPricingCons(248) == 0m) // 248 = IPIP
                        {
                            val = GetDadoTmpPricingCons(201); // 201 = ICVA
                            formulaVal1 = (val.HasValue) ? val.Value : 0m;

                            val = GetDadoTmpPricingCons(202); // 202 = ICBS
                            formulaVal1 = (formulaVal1 * ((val.HasValue) ? val.Value : 0m));

                            val = GetDadoTmpPricingCons(241); // 241 = IPVA
                            val = (1m + ((val.HasValue) ? val.Value : 0m));
                            formulaVal1 = Convert.ToDecimal(1m - (formulaVal1 * val.Value));

                            val = GetSumTmpPricingCons(140, 140); // 140 = Nulo
                            formulaVal1 = (((val.HasValue) ? val.Value : 0m) / formulaVal1);
                            formulaVal1 = CSGlobal.Round(formulaVal1, 2);

                        }
                        else
                        {

                            val = GetDadoTmpPricingCons(201); // 201 = ICVA
                            formulaVal1 = (val.HasValue) ? val.Value : 0m;

                            val = GetDadoTmpPricingCons(202); // 202 = ICBS
                            decimal formulaVal1Ant = (1m - (formulaVal1 * ((val.HasValue) ? val.Value : 0m)));

                            val = GetSumTmpPricingCons(140, 140); // 140 = Nulo
                            formulaVal1 = ((val.HasValue) ? val.Value : 0m) / formulaVal1Ant;
                            formulaVal1 = CSGlobal.Round(formulaVal1, 2);

                            val = GetSumTmpPricingCons(248, 248); // 248 = IPIP
                            formulaVal1 = formulaVal1 + (((val.HasValue) ? val.Value : 0m) * (CSGlobal.Round((1 / formulaVal1Ant), 2) - 1m));

                        }
                    }
                }
                if (CDCONDTYP_ANT == "IBRX")
                {
                    //val = GetSumTmpPricingCons(NMCONDINI_ANT, NMCONDFIM_ANT); 
                    //formulaVal1 = (val.IsNull) ? 0 : val.Value;

                }

                result = formulaVal1;

            }
            catch
            {
                result = null;
            }

            return result;
        }

        private decimal? CalculaFormulaF905()
        {
            decimal? result = null;

            try
            {
                //decimal val = GetSumTmpPricingCons(772, 772);
                decimal? val = GetSumTmpPricingCons(776, 776); //780
                decimal formulaVal1 = (val.HasValue) ? val.Value : 0m;

                //val = GetSumTmpPricingCons(768, 768);
                val = GetSumTmpPricingCons(773, 773); //778
                decimal formulaVal2 = (val.HasValue) ? val.Value : 0m;

                result = CSGlobal.Round(formulaVal1 - formulaVal2, 2);

                if (result.Value < 0)
                    result = 0m;

            }
            catch
            {
                result = null;
            }

            return result;
        }

        private decimal? CalculaFormulaF907()
        {
            decimal? result = null;
            decimal? val = null;
            decimal valor_ISTT = 0m;
            decimal valor_YSTT = 0m;
            decimal valor_ISTM = 0m;
            decimal valor_YSTB = 0m;
            decimal valor_BaseMinima = 0m;
            decimal valor_basePadrao = 0m;

            decimal valor_ISTI = 0m;
            decimal valor_ISTN = 100m;

            try
            {
                val = GetSumTmpPricingCons(283, 283); // 283 = ISTM
                valor_ISTM = (val.HasValue) ? val.Value : 0m;

                val = GetSumTmpPricingCons(770, 770); // 770 = ISTT
                valor_YSTT = (val.HasValue) ? val.Value : 0m;

                val = GetDadoTmpPricingCons(770); // 770 = ISTT
                valor_ISTT = (val.HasValue) ? val.Value : 0m;
                decimal? ISTN = GetDadoTmpPricingCons(288);

                ISTN = (ISTN.HasValue ? ISTN.Value : 0m);

                if (valor_ISTT * 100 == 3)
                {
                    NMCONDINI_ANT = 0;
                    NMCONDFIM_ANT = 0;

                    //288 = ISTN
                    if (ISTN != 0)
                    {
                        valor_basePadrao = valor_YSTT;

                        valor_BaseMinima = (ISTN.Value * valor_ISTM); // /100

                        if (valor_basePadrao >= valor_BaseMinima)
                            result = GetSumTmpPricingCons(767, 767); //772
                        else
                            result = GetSumTmpPricingCons(283, 283);
                    }
                    else
                    {
                        val = GetSumTmpPricingCons(767, 767); // 772 = ISTB
                        valor_YSTB = (val.HasValue) ? val.Value : 0m;

                        valor_BaseMinima = ((valor_ISTM * valor_ISTN) / 100m);

                        if (valor_ISTI == 0)
                            valor_basePadrao = valor_YSTB;
                        else
                            valor_basePadrao = valor_YSTT;

                        if (valor_basePadrao < valor_BaseMinima)
                            result = valor_BaseMinima;
                        else
                            result = valor_basePadrao;
                    }
                }

            }
            catch
            {
                result = null;
            }

            return result;
        }

        private decimal? CalculaFormulaF960()
        {
            decimal? result = null;

            try
            {
                decimal? val = GetSumTmpPricingCons(NMCONDLIN_ANT, NMCONDINI_ANT, NMCONDFIM_ANT, NMCONDINI_ANT, NMCONDLIN_ANT);
                decimal formulaVal = (val.HasValue) ? val.Value : 0;

                if (formulaVal < 100m)
                    result = 100m;
                else if (formulaVal >= 100m && formulaVal <= 999.99m)
                    result = 1000m;
                else
                    result = 10000m;

            }
            catch
            {
                result = null;
            }

            return result;
        }

        private decimal? CalculaFormulaF961()
        {
            decimal? result = null;

            try
            {
                decimal? val = GetSumTmpPricingCons(87, 87);
                decimal formulaVal1 = (val.HasValue) ? val.Value : 0m;

                val = GetSumTmpPricingCons(85, 85);
                decimal formulaVal2 = (val.HasValue) ? val.Value : 0m;

                val = GetSumTmpPricingCons(30, 30);
                decimal formulaVal3 = (val.HasValue) ? val.Value : 0m;

                result = CSGlobal.Round(CSGlobal.Round(formulaVal1 / (quantidadeInteira + ((quantidadeFracao > 0) ? 1m : 0m)), 2) /
                    (formulaVal2 / formulaVal3), 2) * (quantidadeInteira + ((quantidadeFracao > 0) ? 1m : 0m));

                //result = result;

            }
            catch
            {
                result = null;
            }

            return result;
        }

        private decimal? CalculaFormulaDefaultPasso1()
        {
            decimal? result = null;

            try
            {
                decimal? val = GetSumTmpPricingCons(NMCONDLIN_ANT, NMCONDINI_ANT, NMCONDFIM_ANT, NMCONDINI_ANT, NMCONDINI_ANT);
                decimal formulaVal = (val.HasValue) ? val.Value : 0m;

                if (DSFORMULA_ANT == "F912")
                    result = (tmpVariaveis.CDGRCSUTR == "50") ? 0m : formulaVal;
                else if (DSFORMULA_ANT == "F913")
                    result = (tmpVariaveis.CDGRCSUTR == "50") ? formulaVal : 0m;
                else
                    result = formulaVal;

            }
            catch
            {
                result = null;
            }

            return result;
        }

        #endregion

        #endregion
    }
}