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
    public class CSDocumentosReceberPDV : CollectionBase
    {
        #region [Variaveis]

        #endregion

        #region[Propriedades]
        /// <summary>
        /// Retorna coleção dos Documentos à Receber do PDV 
        /// </summary>
        public CSDocumentosReceberPDV Items
        {
            get
            {
                return this;
            }
        }

        public CSDocumentosReceberPDV.CSDocumentoReceber this[int Index]
        {
            get
            {
                return (CSDocumentosReceberPDV.CSDocumentoReceber)this.InnerList[Index];
            }
        }


        #endregion

        #region [ Métodos ]


        public CSDocumentosReceberPDV(int codigoPDV, string codigoRevenda, bool consolidaDados)
        {
            CSDocumentoReceber docrecVencidos = null;
            CSDocumentoReceber docrecVencer = null;
            CSDocumentoReceber docrecTotais = null;
            CSDocumentoReceber docrecLimiteCredito = null;
            CSDocumentoReceber docrecSaldoCredito = null;

            SQLiteParameter paramCodigoPDV = null;
            SQLiteParameter paramCodigoRevenda = null;

            SQLiteDataReader sqlReader = null;
            string sqlQuery = null;

            CSPDVs.CSPDV pdvAtual = null;

            string codigoRevendaAnterior = null;
            bool databaseExists = false;

            try
            {
                pdvAtual = CSPDVs.Current;
                databaseExists = CSDataAccess.DataBaseExists(codigoRevenda);

                // [ Recuperar dados de outra empresa sem consolidar? ]
                codigoRevendaAnterior = CSGlobal.COD_REVENDA;
                if (codigoRevenda != codigoRevendaAnterior && consolidaDados == false && databaseExists)
                {
                    // [ Fecha banco atual ]
                    CSDataAccess.Instance.FechaConexao();

                    // [ Abre o banco da outra empresa ]
                    CSGlobal.COD_REVENDA = codigoRevenda;
                    CSDataAccess.Instance.AbreConexao();
                }

                // [ Resumo ]
                // [ Retorno de documentos vencidos ]
                sqlQuery =
                    "SELECT SUM(VLR_DOCUMENTO_RECEBER + VLR_MULTA + VLR_JUROS + VLR_ENCARGO - VLR_DESCONTO - VLR_RECEBIDO) RESULTADO " +
                    "      ,COUNT(COD_DOCUMENTO_RECEBER) QTD " +
                    "  FROM DOCUMENTO_RECEBER " +
                    " WHERE (VLR_RECEBIDO + VLR_DESCONTO) < (VLR_DOCUMENTO_RECEBER + VLR_MULTA + VLR_JUROS + VLR_ENCARGO) " +
                    "   AND DATE(DAT_VENCIMENTO) < DATE('now') " +
                    "   AND COD_PDV = ? ";

                paramCodigoPDV = new SQLiteParameter("@COD_PDV", codigoPDV);

                // [ Busca todos os documentos vencidos do PDV ]
                if (consolidaDados)
                {
                    sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, paramCodigoPDV);

                }
                else
                {
                    sqlQuery += " AND COD_REVENDA = ? ";

                    paramCodigoRevenda = new SQLiteParameter("@COD_REVENDA", codigoRevenda);

                    sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, paramCodigoPDV, paramCodigoRevenda);
                }

                docrecVencidos = new CSDocumentoReceber();

                while (sqlReader.Read())
                {
                    //CSDocumentoReceber docrecVencidos = new CSDocumentoReceber();
                    docrecVencidos.TIPO_DOCUMENTO = CSDocumentoReceber.TipoDocumento.VENCIDO;
                    docrecVencidos.VALOR = sqlReader.GetValue(0) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(0));
                    docrecVencidos.QUANTIDADE = sqlReader.GetValue(1) == System.DBNull.Value ? 0 : sqlReader.GetInt32(1);

                    // Adciona o valor do documento vencido
                    base.InnerList.Add(docrecVencidos);
                }
                // Fecha o reader
                sqlReader.Close();
                sqlReader.Dispose();

                // [ Retorno de documentos à vencer ]
                sqlQuery =
                    "SELECT SUM(VLR_DOCUMENTO_RECEBER + VLR_MULTA + VLR_JUROS + VLR_ENCARGO - VLR_DESCONTO - VLR_RECEBIDO) RESULTADO " +
                    "      ,COUNT(COD_DOCUMENTO_RECEBER) QTD " +
                    "  FROM DOCUMENTO_RECEBER " +
                    " WHERE (VLR_RECEBIDO + VLR_DESCONTO) < (VLR_DOCUMENTO_RECEBER + VLR_MULTA + VLR_JUROS + VLR_ENCARGO) " +
                    "   AND DATE(DAT_VENCIMENTO) >= DATE('now') " +
                    "   AND COD_PDV = ? ";

                paramCodigoPDV = new SQLiteParameter("@COD_PDV", codigoPDV);

                // Busca todos os documentos a vencer do PDV
                if (consolidaDados)
                {
                    sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, paramCodigoPDV);

                }
                else
                {
                    sqlQuery += " AND COD_REVENDA = ? ";

                    paramCodigoRevenda = new SQLiteParameter("@COD_REVENDA", codigoRevenda);

                    sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, paramCodigoPDV, paramCodigoRevenda);
                }

                docrecVencer = new CSDocumentoReceber();
                while (sqlReader.Read())
                {
                    //CSDocumentoReceber docrecVencer = new CSDocumentoReceber();
                    docrecVencer.TIPO_DOCUMENTO = CSDocumentoReceber.TipoDocumento.A_VENCER;
                    docrecVencer.VALOR = sqlReader.GetValue(0) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(0));
                    docrecVencer.QUANTIDADE = sqlReader.GetValue(1) == System.DBNull.Value ? 0 : sqlReader.GetInt32(1);

                    // Adciona o documento a vencer
                    base.InnerList.Add(docrecVencer);
                }
                // Fecha o reader
                sqlReader.Close();
                sqlReader.Dispose();


                // [ Total ]
                docrecTotais = new CSDocumentoReceber();
                docrecTotais.TIPO_DOCUMENTO = CSDocumentoReceber.TipoDocumento.TOTAL;
                docrecTotais.TOTAL_VALOR = docrecVencidos.VALOR + docrecVencer.VALOR;
                docrecTotais.TOTAL_QUANTIDADE = docrecVencidos.QUANTIDADE + docrecVencer.QUANTIDADE;

                // Adiciona na coleção
                base.InnerList.Add(docrecTotais);

                // [ Se não estiver consolidando retira cálculo de limite e saldo ]
                // [ Verifica se o banco da empresa selecionada existe ]
                if (!consolidaDados && databaseExists)
                {
                    // [ Limite de credito ]
                    sqlQuery = "SELECT VLR_LIMITE_CREDITO FROM PDV WHERE COD_PDV = ?";

                    paramCodigoPDV = new SQLiteParameter("@COD_PDV", codigoPDV);

                    docrecLimiteCredito = new CSDocumentoReceber();
                    docrecLimiteCredito.TIPO_DOCUMENTO = CSDocumentoReceber.TipoDocumento.LIMITE_CREDITO;
                    object limite = CSDataAccess.Instance.ExecuteScalar(sqlQuery, paramCodigoPDV);

                    if (limite != null && limite != DBNull.Value)
                    {
                        docrecLimiteCredito.VLR_LIMITE_CREDITO_PDV = decimal.Parse(limite.ToString());
                    }
                    else
                    {
                        docrecLimiteCredito.VLR_LIMITE_CREDITO_PDV = 0;
                    }

                    // Adiciona na coleção
                    base.InnerList.Add(docrecLimiteCredito);

                    docrecSaldoCredito = new CSDocumentoReceber();
                    docrecSaldoCredito.TIPO_DOCUMENTO = CSDocumentoReceber.TipoDocumento.PEDIDOS;
                    docrecSaldoCredito.VALOR = CSPDVs.Current.VLR_SALDO_PEDIDO;

                    base.InnerList.Add(docrecSaldoCredito);

                    docrecSaldoCredito = new CSDocumentoReceber();
                    docrecSaldoCredito.TIPO_DOCUMENTO = CSDocumentoReceber.TipoDocumento.PEDIDOS_DESCARREGADOS;
                    docrecSaldoCredito.VALOR = CSPDVs.Current.PEDIDOS_PRAZO_DIA;

                    base.InnerList.Add(docrecSaldoCredito);

                    // [ Saldo de crédito ]
                    docrecSaldoCredito = new CSDocumentoReceber();
                    docrecSaldoCredito.TIPO_DOCUMENTO = CSDocumentoReceber.TipoDocumento.SALDO;
                    docrecSaldoCredito.SALDO_CREDITO = docrecLimiteCredito.VLR_LIMITE_CREDITO_PDV - docrecTotais.TOTAL_VALOR;

                    // Adiciona na coleção
                    base.InnerList.Add(docrecSaldoCredito);
                }


                // [ Busca valores da tabela ]
                //Na stored procedure ja traz somente documentos em aberto
                sqlQuery =
                    "SELECT DR.DAT_VENCIMENTO, DR.COD_DOCUMENTO_RECEBER, DR.COD_DOCUMENTO_ORIGEM " +
                    "      ,DR.COD_CLASSE_DOCUMENTO_RECEBER, CDR.DSC_CLASSE_DOCUMENTO_RECEBER " +
                    "      ,(VLR_DOCUMENTO_RECEBER + VLR_MULTA + VLR_JUROS + VLR_ENCARGO - VLR_DESCONTO - VLR_RECEBIDO) AS VLR_RECEBER " +
                    "      ,DR.VLR_JUROS, DR.VLR_MULTA, DR.VLR_ENCARGO, DR.VLR_DESCONTO, DR.COD_EMPREGADO, FIL.NOME_EMPRESA " +
                    "  FROM DOCUMENTO_RECEBER DR " +
                    " INNER JOIN CLASSE_DOCUMENTO_RECEBER CDR " +
                    "    ON DR.COD_CLASSE_DOCUMENTO_RECEBER = CDR.COD_CLASSE_DOCUMENTO_RECEBER " +
                    "   AND DR.COD_REVENDA = CDR.COD_REVENDA " +
                    " INNER JOIN FILIAL FIL ON DR.COD_REVENDA = FIL.COD_REVENDA " +
                    " WHERE DR.COD_PDV = ? " +
                    ((consolidaDados) ? "" : " AND DR.COD_REVENDA = ? ") +
                    "   AND COD_STATUS_DOCUMENTO_RECEBER <> 1 " +
                    " ORDER BY DR.DAT_VENCIMENTO, DR.COD_REVENDA ";

                paramCodigoPDV = new SQLiteParameter("@COD_PDV", codigoPDV);

                // Busca todos os campos necessarios do Doc. receber
                if (consolidaDados)
                {
                    sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, paramCodigoPDV);

                }
                else
                {
                    paramCodigoRevenda = new SQLiteParameter("@COD_REVENDA", codigoRevenda);
                    sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, paramCodigoPDV, paramCodigoRevenda);
                }

                while (sqlReader.Read())
                {
                    // Criacao de um novo objeto, para que tenha referencia do numero de documentos.
                    CSDocumentoReceber listagemDocReceber = new CSDocumentoReceber();

                    // Preenche a instancia da classe de doc receber do pdv
                    listagemDocReceber.TIPO_DOCUMENTO = CSDocumentoReceber.TipoDocumento.OUTROS;
                    listagemDocReceber.DAT_VENCIMENTO = sqlReader.GetValue(0) == System.DBNull.Value ? new DateTime(1900, 1, 1) : sqlReader.GetDateTime(0);
                    listagemDocReceber.COD_DOCUMENTO_RECEBER = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1).Trim();
                    listagemDocReceber.COD_DOCUMENTO_ORIGEM = sqlReader.GetValue(2) == System.DBNull.Value ? "" : sqlReader.GetString(2).Trim();
                    listagemDocReceber.COD_CLASSE_DOCUMENTO_RECEBER = sqlReader.GetValue(3) == System.DBNull.Value ? 0 : sqlReader.GetInt32(3);
                    listagemDocReceber.DSC_CLASSE_DOCUMENTO_RECEBER = sqlReader.GetValue(4) == System.DBNull.Value ? "" : sqlReader.GetString(4);
                    listagemDocReceber.VALOR_ABERTO = sqlReader.GetValue(5) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(5));
                    listagemDocReceber.VLR_JUROS = sqlReader.GetValue(6) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(6));
                    listagemDocReceber.VLR_MULTA = sqlReader.GetValue(7) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(7));
                    listagemDocReceber.VLR_ENCARGO = sqlReader.GetValue(8) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(8));
                    listagemDocReceber.VLR_DESCONTO = sqlReader.GetValue(9) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(9));
                    listagemDocReceber.COD_VENDEDOR = sqlReader.GetValue(10) == System.DBNull.Value ? 0 : sqlReader.GetInt32(10);
                    listagemDocReceber.NOME_EMPRESA = sqlReader.GetValue(11) == System.DBNull.Value ? "" : sqlReader.GetString(11);

                    // Adiciona a listagem de documentos a receber na coleção de pedidos deste PDV
                    base.InnerList.Add(listagemDocReceber);
                }
                // Fecha o reader
                sqlReader.Close();
                sqlReader.Dispose();

            }
            finally
            {
                // [ Recuperou dados de outra empresa? ]
                if (codigoRevenda != codigoRevendaAnterior)
                {
                    // [ Fecha banco atual ]
                    CSDataAccess.Instance.FechaConexao();

                    // [ Abre o banco da outra empresa ]
                    CSGlobal.COD_REVENDA = codigoRevendaAnterior;
                    CSDataAccess.Instance.AbreConexao();
                }
            }
        }

        #endregion

        public class CSDocumentoReceber
        {
            #region [ Enuns ]

            public enum TipoDocumento
            {
                VENCIDO,
                A_VENCER,
                TOTAL,
                LIMITE_CREDITO,
                SALDO,
                OUTROS,
                PEDIDOS,
                PEDIDOS_DESCARREGADOS
            };

            #endregion

            #region [Variaveis]

            private string m_COD_DOCUMENTO_RECEBER;
            private int m_COD_CLASSE_DOCUMENTO_RECEBER;
            private string m_DSC_CLASSE_DOCUMENTO_RECEBER;
            private int m_COD_STATUS_DOCUMENTO_RECEBER;
            private string m_COD_DOCUMENTO_ORIGEM;
            private string m_NUM_CGC_CPF_PORTADOR_DOCUMENTO;
            private decimal m_PRC_MULTA;
            private decimal m_PRC_JUROS;
            private decimal m_PRC_DESCONTO;
            private decimal m_VLR_MULTA;
            private decimal m_VLR_JUROS;
            private decimal m_VLR_ENCARGO;
            private decimal m_VLR_DESCONTO;
            private decimal m_VLR_RECEBIDO;
            private decimal m_VLR_DOCUMENTO_RECEBER;
            private DateTime m_DAT_CADASTRO;
            private DateTime m_DAT_EMISSAO;
            private DateTime m_DAT_VENCIMENTO;
            private DateTime m_DAT_BAIXA;
            private string m_DSC_OBSERVACAO;
            private CSDocumentoReceber.TipoDocumento m_TIPO_DOCUMENTO;
            private int m_QUANTIDADE;
            private decimal m_VALOR;
            private decimal m_VALOR_ABERTO;
            private decimal m_TOTAL_QUANTIDADE;
            private decimal m_TOTAL_VALOR;
            private decimal m_VLR_LIMITE_CREDITO_PDV;
            private decimal m_SALDO_CREDITO;
            private int m_COD_VENDEDOR;
            private string m_NOME_EMPRESA;

            #endregion

            #region[Propriedades]

            /// <summary>
            /// Guarda o tipo do documento a receber. Usada para controle na hora de mostrar na tela
            /// </summary>
            public TipoDocumento TIPO_DOCUMENTO
            {
                get
                {
                    return m_TIPO_DOCUMENTO;
                }
                set
                {
                    m_TIPO_DOCUMENTO = value;
                }
            }

            /// <summary>
            /// Guarda o codigo do documento a receber
            /// </summary>
            public string COD_DOCUMENTO_RECEBER
            {
                get
                {
                    return m_COD_DOCUMENTO_RECEBER;
                }
                set
                {
                    m_COD_DOCUMENTO_RECEBER = value;
                }
            }

            /// <summary>
            /// Guarda o codigo da classe do documento a receber
            /// </summary>
            public int COD_CLASSE_DOCUMENTO_RECEBER
            {
                get
                {
                    return m_COD_CLASSE_DOCUMENTO_RECEBER;
                }
                set
                {
                    m_COD_CLASSE_DOCUMENTO_RECEBER = value;
                }
            }

            /// <summary>
            /// Guarda a descricao da classe do documento a receber
            /// </summary>
            public string DSC_CLASSE_DOCUMENTO_RECEBER
            {
                get
                {
                    return m_DSC_CLASSE_DOCUMENTO_RECEBER;
                }
                set
                {
                    m_DSC_CLASSE_DOCUMENTO_RECEBER = value;
                }
            }

            /// <summary>
            /// Guarda o codigo do status do documento a receber
            /// </summary>
            public int COD_STATUS_DOCUMENTO_RECEBER
            {
                get
                {
                    return m_COD_STATUS_DOCUMENTO_RECEBER;
                }
                set
                {
                    m_COD_STATUS_DOCUMENTO_RECEBER = value;
                }
            }

            /// <summary>
            /// Guarda o codigo do documento de origem
            /// </summary>
            public string COD_DOCUMENTO_ORIGEM
            {
                get
                {
                    return m_COD_DOCUMENTO_ORIGEM;
                }
                set
                {
                    m_COD_DOCUMENTO_ORIGEM = value;
                }
            }

            /// <summary>
            /// Guarda o numero do cgc, cpf do portador do documento
            /// </summary>
            public string NUM_CGC_CPF_PORTADOR_DOCUMENTO
            {
                get
                {
                    return m_NUM_CGC_CPF_PORTADOR_DOCUMENTO;
                }
                set
                {
                    m_NUM_CGC_CPF_PORTADOR_DOCUMENTO = value;
                }
            }

            /// <summary>
            /// Guarda a porcentagem da multa
            /// </summary>
            public decimal PRC_MULTA
            {
                get
                {
                    return m_PRC_MULTA;
                }
                set
                {
                    m_PRC_MULTA = value;
                }
            }

            /// <summary>
            /// Guarda a porcentagem dos juros
            /// </summary>
            public decimal PRC_JUROS
            {
                get
                {
                    return m_PRC_JUROS;
                }
                set
                {
                    m_PRC_JUROS = value;
                }
            }

            /// <summary>
            /// Guarda a porcentagem de desconto
            /// </summary>
            public decimal PRC_DESCONTO
            {
                get
                {
                    return m_PRC_DESCONTO;
                }
                set
                {
                    m_PRC_DESCONTO = value;
                }
            }

            /// <summary>
            /// Guarda o valor da multa
            /// </summary>
            public decimal VLR_MULTA
            {
                get
                {
                    return m_VLR_MULTA;
                }
                set
                {
                    m_VLR_MULTA = value;
                }
            }

            /// <summary>
            /// Guarda o valor do juros
            /// </summary>
            public decimal VLR_JUROS
            {
                get
                {
                    return m_VLR_JUROS;
                }
                set
                {
                    m_VLR_JUROS = value;
                }
            }
            /// <summary>
            /// Guarda o valor de encargos
            /// </summary>
            public decimal VLR_ENCARGO
            {
                get
                {
                    return m_VLR_ENCARGO;
                }
                set
                {
                    m_VLR_ENCARGO = value;
                }
            }
            /// <summary>
            /// Guarda o valor do desconto
            /// </summary>
            public decimal VLR_DESCONTO
            {
                get
                {
                    return m_VLR_DESCONTO;
                }
                set
                {
                    m_VLR_DESCONTO = value;
                }
            }
            /// <summary>
            /// Guarda o valor do recebido
            /// </summary>
            public decimal VLR_RECEBIDO
            {
                get
                {
                    return m_VLR_RECEBIDO;
                }
                set
                {
                    m_VLR_RECEBIDO = value;
                }
            }
            /// <summary>
            /// Guarda o valor de documentos a receber
            /// </summary>
            public decimal VLR_DOCUMENTO_RECEBER
            {
                get
                {
                    return m_VLR_DOCUMENTO_RECEBER;
                }
                set
                {
                    m_VLR_DOCUMENTO_RECEBER = value;
                }
            }
            /// <summary>
            /// Guarda a data de cadastro
            /// </summary>
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
            /// <summary>
            /// Guarda a data de emissao
            /// </summary>
            public DateTime DAT_EMISSAO
            {
                get
                {
                    return m_DAT_EMISSAO;
                }
                set
                {
                    m_DAT_EMISSAO = value;
                }
            }
            /// <summary>
            /// Guarda a data de vencimento
            /// </summary>
            public DateTime DAT_VENCIMENTO
            {
                get
                {
                    return m_DAT_VENCIMENTO;
                }
                set
                {
                    m_DAT_VENCIMENTO = value;
                }
            }
            /// <summary>
            /// Guarda a data da baixa
            /// </summary>
            public DateTime DAT_BAIXA
            {
                get
                {
                    return m_DAT_BAIXA;
                }
                set
                {
                    m_DAT_BAIXA = value;
                }
            }
            /// <summary>
            /// Guarda a descricao de observaçao
            /// </summary>
            public string DSC_OBSERVACAO
            {
                get
                {
                    return m_DSC_OBSERVACAO;
                }
                set
                {
                    m_DSC_OBSERVACAO = value;
                }
            }
            /// <summary>
            /// Guarda a quantidade
            /// </summary>
            public int QUANTIDADE
            {
                get
                {
                    return m_QUANTIDADE;
                }
                set
                {
                    m_QUANTIDADE = value;
                }
            }
            /// <summary>
            /// Guarda o valor
            /// </summary>
            public decimal VALOR
            {
                get
                {
                    return m_VALOR;
                }
                set
                {
                    m_VALOR = value;
                }
            }
            /// <summary>
            /// Guarda o valor aberto
            /// </summary>
            public decimal VALOR_ABERTO
            {
                get
                {
                    return m_VALOR_ABERTO;
                }
                set
                {
                    m_VALOR_ABERTO = value;
                }
            }
            /// <summary>
            /// Guarda o total de valor
            /// </summary>
            public decimal TOTAL_VALOR
            {
                get
                {
                    return m_TOTAL_VALOR;
                }
                set
                {
                    m_TOTAL_VALOR = value;
                }
            }
            /// <summary>
            /// Guarda o total de quantidade
            /// </summary>
            public decimal TOTAL_QUANTIDADE
            {
                get
                {
                    return m_TOTAL_QUANTIDADE;
                }
                set
                {
                    m_TOTAL_QUANTIDADE = value;
                }
            }
            /// <summary>
            /// Guarda o valor do limite de credito do pdv
            /// </summary>
            public decimal VLR_LIMITE_CREDITO_PDV
            {
                get
                {
                    return m_VLR_LIMITE_CREDITO_PDV;
                }
                set
                {
                    m_VLR_LIMITE_CREDITO_PDV = value;
                }
            }
            /// <summary>
            /// Guarda o valor do saldo de credito
            /// </summary>
            public decimal SALDO_CREDITO
            {
                get
                {
                    return m_SALDO_CREDITO;
                }
                set
                {
                    m_SALDO_CREDITO = value;
                }
            }

            /// <summary>
            /// Guarda o codigo do vendedor
            /// </summary>
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

            /// <summary>
            /// Guarda o nome da empresa
            /// </summary>
            public string NOME_EMPRESA
            {
                get
                {
                    return m_NOME_EMPRESA;
                }
                set
                {
                    m_NOME_EMPRESA = value;
                }
            }

            #endregion
        }
    }
}