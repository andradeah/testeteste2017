using System;
using System.Data;
using System.Collections;
#if ANDROID
using Mono.Data.Sqlite;
#else
using System.Data.SQLite;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
#endif

#if ANDROID
using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
using SQLiteCommand = Mono.Data.Sqlite.SqliteCommand;
using SQLiteDataAdapter = Mono.Data.Sqlite.SqliteDataAdapter;
using SQLiteException = Mono.Data.Sqlite.SqliteException;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteTransaction = Mono.Data.Sqlite.SqliteTransaction;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;
using System.Text;
using System.Collections.Generic;
#endif

namespace AvanteSales
{
    public class CSEmpresa
    {
        #region [ Variaveis ]

        private int m_COD_EMPRESA;
        private string m_IND_LIMITE_CREDITO;
        private string m_COD_ESTADO;
        private string m_COD_CGC;
        private bool m_IND_LIMITE_DESCONTO;
        private string m_DES_INFORMACAO;
        private string m_DIRETORIO_INTERFACE_PALMTOP;
        private string m_IND_BLOQUEIO_VALOR_MINIMO;
        private string m_IND_VALIDA_QTDMULTIPLA_NAO_VENDA;
        private string m_IND_LIBERA_CLIENTE_BLOQUEADO;
        private string m_IND_VALIDA_PCT_MAXIMO_DESCONTO;
        private string m_IND_PERMITIR_VENDA_FORA_ROTA;
        private int m_TIPO_CALCULO_LUCRATIVIDADE;
        private char m_CONFIGURACAO_LUCRATIVIDADE_PEDIDO;
        private char m_CONFIGURACAO_LUCRATIVIDADE_ITEM_PEDIDO;
        private decimal m_PCT_MINIMO_LUCRATIVIDADE;
        private string m_IND_UTILIZA_ENVIO_EMAIL;
        private DateTime m_DATA_ENTREGA = new DateTime(1900, 1, 1);
        private DateTime m_DATA_ULTIMA_DESCARGA;
        private string m_IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE;
        private string m_IND_TRABALHA_CONFIGURACAO_MULTIPLA_EMPRESA;
        private string m_IND_CONSOLIDAR_FINANCEIRO;
        private string m_CODIGO_REVENDA;
        private int m_TIPO_CALCULO_VERBA;
        private bool m_IND_VLR_VERBA_EXTRA_ATUSALDO;
        private int m_IND_VLR_INDENIZACAO_ATUSALDO;
        private bool m_IND_VLR_VERBA_PEDIDO_NOVO_ATUSALDO;
        private bool m_IND_ATUALIZAR_VERBA_PED_ABAIXOMINIMO;
        private decimal m_PCT_MAXIMO_INDENIZACAO;
        private decimal m_PCT_VERBA_NORMAL;
        private int m_TPO_LICENCA;
        private decimal m_VAL_LIMNF_IDZ;
        private string m_IND_ALTERA_VENDEDOR;
        private string m_IND_VISUALIZA_LUCRATIVIDADE;
        private string m_IND_LISTA_PROD_NAOVENDIDO;
        private string m_IND_UTILIZA_FLEXX_GPS;
        private string m_CONEXAO_FLEXX_GPS;
        private int m_TEMPO_LEITURA;
        private int m_TEMPO_TRANSFERENCIA;
        private bool m_IND_VALIDA_DESCMAX_TABELA_PEDIDO;
        private bool m_IND_RATEIO_INDENIZACAO;
        private bool m_IND_INDENIZACAO_MAIORVENDA;
        private bool m_IND_HIERARQUIA_DESCONTO_MAXIMO;
        private bool m_IND_CLUSTER_PRODUTOS_PELO_TIPO_FREEZER;
        private bool m_IND_POLITICA_CALCULO_PRECO_MISTA;
        private bool m_IND_TRABALHA_PEDIDO_SUGERIDO;
        private decimal m_PCT_TAXA_OCUPACAO_IDEAL_VEICULO_MODELO;
        private int m_TIPO_TROCA;
        private bool m_IND_ASSUME_QTD_SUGERIDA_SOBRE_MINIMA;
        private int m_IND_GRUPO_COMERCIALIZACAO_DIFERENTE_PEDIDO;
        private int m_IND_QTD_COLETAS_REALIZADAS;
        private decimal m_PCT_ESTOQUE_SEGURANCA;
        private decimal m_PCT_SEGURANCA_PRODUTO_ESTOQUE;
        private string m_VERSAO_AVANTE_SALES_COMPATIBILIDADE;
        private bool m_UtilizaBancoNovo;
        private bool m_UtilizaNovoAtributoPedido;
        private bool m_UtilizaDescricaoDenver;
        private string m_IND_UTILIZA_INDENIZACAO;
        private string m_VERSAO_WEBSERVICE;
        private string m_IND_MOSTRAR_PRODUTO_BLOQUEADO;
        private string m_IND_PERMITIR_VENDA_SOMENTE_LAYOUT;
        private int m_QTD_MAX_VENDA_OUTROS_DANONE;
        private int m_COD_NOTEBOOK1;
        private int m_COD_NOTEBOOK2;
        private int m_NUM_RAIO_LOCALIZACAO;
        private string m_IND_PDV_BUNGE_COMPRA_OUTRAS_MARCAS;
        private bool m_IND_UTILIZA_PRICE_2014;
        private bool m_IND_EMPRESA_FERIADO;
        private string m_IND_CADASTRO_CONTATO_PDV_OBRIGATORIO;
        private static CSEmpresa m_Current;

        public struct CALCULO_VERBA
        {
            public const int NENHUM = 0;
            public const int PERCENTUAL_VALOR_PEDIDO = 1;
            public const int DIFERENCA_VALOR_TABELA = 2;
        }

        #endregion

        #region [ Propriedades ]

        public int COD_EMPRESA
        {
            get
            {
                return m_COD_EMPRESA;
            }
            set
            {
                m_COD_EMPRESA = value;
            }
        }

        public string IND_LIMITE_CREDITO
        {
            get
            {
                return m_IND_LIMITE_CREDITO;
            }
            set
            {
                m_IND_LIMITE_CREDITO = value;
            }
        }

        public string IND_UTILIZA_ENVIO_EMAIL
        {
            get
            {
                return m_IND_UTILIZA_ENVIO_EMAIL;
            }
            set
            {
                m_IND_UTILIZA_ENVIO_EMAIL = value;
            }
        }

        public string IND_BLOQUEIO_VALOR_MINIMO
        {
            get
            {
                return m_IND_BLOQUEIO_VALOR_MINIMO;
            }
            set
            {
                m_IND_BLOQUEIO_VALOR_MINIMO = value;
            }
        }

        public string IND_VALIDA_QTDMULTIPLA_NAO_VENDA
        {
            get
            {
                return m_IND_VALIDA_QTDMULTIPLA_NAO_VENDA;
            }
            set
            {
                m_IND_VALIDA_QTDMULTIPLA_NAO_VENDA = value;
            }
        }

        public string IND_LIBERA_CLIENTE_BLOQUEADO
        {
            get
            {
                return m_IND_LIBERA_CLIENTE_BLOQUEADO;
            }
            set
            {
                m_IND_LIBERA_CLIENTE_BLOQUEADO = value;
            }
        }

        public string IND_VALIDA_PCT_MAXIMO_DESCONTO
        {
            get
            {
                return m_IND_VALIDA_PCT_MAXIMO_DESCONTO;
            }
            set
            {
                m_IND_VALIDA_PCT_MAXIMO_DESCONTO = value;
            }
        }

        public string IND_PERMITIR_VENDA_FORA_ROTA
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

        public string IND_CADASTRO_CONTATO_PDV_OBRIGATORIO
        {
            get
            {
                return m_IND_CADASTRO_CONTATO_PDV_OBRIGATORIO;
            }
            set
            {
                m_IND_CADASTRO_CONTATO_PDV_OBRIGATORIO = value;
            }
        }

        public string CODIGO_REVENDA
        {
            get
            {
                return m_CODIGO_REVENDA;
            }
            set
            {
                m_CODIGO_REVENDA = value;
            }
        }

        public string IND_TRABALHA_CONFIGURACAO_MULTIPLA_EMPRESA
        {
            get
            {
                return m_IND_TRABALHA_CONFIGURACAO_MULTIPLA_EMPRESA;
            }
            set
            {
                m_IND_TRABALHA_CONFIGURACAO_MULTIPLA_EMPRESA = value;
            }
        }

        public string IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE
        {
            get
            {
                if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current == null)
                    return m_IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE;
                else
                    return "S";
            }
            set
            {
                m_IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE = value;
            }
        }

        public string IND_CONSOLIDAR_FINANCEIRO
        {
            get
            {
                return m_IND_CONSOLIDAR_FINANCEIRO;
            }
            set
            {
                m_IND_CONSOLIDAR_FINANCEIRO = value;
            }
        }

        public string COD_ESTADO
        {
            get
            {
                return m_COD_ESTADO;
            }
            set
            {
                m_COD_ESTADO = value;
            }
        }

        public string COD_CGC
        {
            get
            {
                return m_COD_CGC;
            }
            set
            {
                m_COD_CGC = value;
            }
        }

        public static CSEmpresa Current
        {
            get
            {
                if (m_Current == null)
                    m_Current = new CSEmpresa();
                return m_Current;
            }
            set
            {
                m_Current = value;
            }
        }

        public bool IND_LIMITE_DESCONTO
        {
            get
            {
                return m_IND_LIMITE_DESCONTO;
            }
            set
            {
                m_IND_LIMITE_DESCONTO = value;
            }
        }
        public string DES_INFORMACAO
        {
            get
            {
                return m_DES_INFORMACAO;
            }
            set
            {
                m_DES_INFORMACAO = value;
            }
        }

        public string DIRETORIO_INTERFACE_PALMTOP
        {
            get
            {
                return m_DIRETORIO_INTERFACE_PALMTOP;
            }
            set
            {
                m_DIRETORIO_INTERFACE_PALMTOP = value;
            }
        }

        public int TIPO_CALCULO_LUCRATIVIDADE
        {
            get
            {
                return m_TIPO_CALCULO_LUCRATIVIDADE;
            }
            set
            {
                m_TIPO_CALCULO_LUCRATIVIDADE = value;
            }
        }

        public char CONFIGURACAO_LUCRATIVIDADE_PEDIDO
        {
            get
            {
                return m_CONFIGURACAO_LUCRATIVIDADE_PEDIDO;
            }
            set
            {
                m_CONFIGURACAO_LUCRATIVIDADE_PEDIDO = value;
            }
        }

        public char CONFIGURACAO_LUCRATIVIDADE_ITEM_PEDIDO
        {
            get
            {
                return m_CONFIGURACAO_LUCRATIVIDADE_ITEM_PEDIDO;
            }
            set
            {
                m_CONFIGURACAO_LUCRATIVIDADE_ITEM_PEDIDO = value;
            }
        }

        public decimal PCT_MINIMO_LUCRATIVIDADE
        {
            get
            {
                return m_PCT_MINIMO_LUCRATIVIDADE;
            }
            set
            {
                m_PCT_MINIMO_LUCRATIVIDADE = value;
            }
        }

        //Contem a data de entrega a ser sugerida ao digitar um novo pedido, ultima data de entrega informada
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

        public DateTime DATA_ULTIMA_DESCARGA
        {
            get
            {
                return m_DATA_ULTIMA_DESCARGA;
            }
            set
            {
                m_DATA_ULTIMA_DESCARGA = value;
            }
        }

        public int TIPO_CALCULO_VERBA
        {
            get
            {
                return m_TIPO_CALCULO_VERBA;
            }
            set
            {
                m_TIPO_CALCULO_VERBA = value;
            }
        }

        public bool IND_VLR_VERBA_EXTRA_ATUSALDO
        {
            get
            {
                return m_IND_VLR_VERBA_EXTRA_ATUSALDO;
            }
            set
            {
                m_IND_VLR_VERBA_EXTRA_ATUSALDO = value;
            }
        }

        public int IND_VLR_INDENIZACAO_ATUSALDO
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

        public bool IND_VLR_VERBA_PEDIDO_NOVO_ATUSALDO
        {
            get
            {
                return m_IND_VLR_VERBA_PEDIDO_NOVO_ATUSALDO;
            }
            set
            {
                m_IND_VLR_VERBA_PEDIDO_NOVO_ATUSALDO = value;
            }
        }

        public bool IND_ATUALIZAR_VERBA_PED_ABAIXOMINIMO
        {
            get
            {
                return m_IND_ATUALIZAR_VERBA_PED_ABAIXOMINIMO;
            }
            set
            {
                m_IND_ATUALIZAR_VERBA_PED_ABAIXOMINIMO = value;
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

        public decimal PCT_VERBA_NORMAL
        {
            get
            {
                return m_PCT_VERBA_NORMAL;
            }
            set
            {
                m_PCT_VERBA_NORMAL = value;
            }
        }

        public int TPO_LICENCA
        {
            get
            {
                return m_TPO_LICENCA;
            }
            set
            {
                m_TPO_LICENCA = value;
            }
        }

        public decimal VAL_LIMNF_IDZ
        {
            get
            {
                return m_VAL_LIMNF_IDZ;
            }
            set
            {
                m_VAL_LIMNF_IDZ = value;
            }
        }
        public string IND_ALTERA_VENDEDOR
        {
            get
            {
                return m_IND_ALTERA_VENDEDOR;
            }
            set
            {
                m_IND_ALTERA_VENDEDOR = value;
            }
        }
        public string IND_VISUALIZA_LUCRATIVIDADE
        {
            get
            {
                return m_IND_VISUALIZA_LUCRATIVIDADE;
            }
            set
            {
                m_IND_VISUALIZA_LUCRATIVIDADE = value;
            }
        }
        public string IND_LISTA_PROD_NAOVENDIDO
        {
            get
            {
                return m_IND_LISTA_PROD_NAOVENDIDO;
            }
            set
            {
                m_IND_LISTA_PROD_NAOVENDIDO = value;
            }
        }
        public string IND_UTILIZA_FLEXX_GPS
        {
            get
            {
                return m_IND_UTILIZA_FLEXX_GPS;
            }
            set
            {
                m_IND_UTILIZA_FLEXX_GPS = value;
            }
        }
        public string CONEXAO_FLEXX_GPS
        {
            get
            {
                return m_CONEXAO_FLEXX_GPS;
            }
            set
            {
                m_CONEXAO_FLEXX_GPS = value;
            }
        }
        public int TEMPO_LEITURA
        {
            get
            {
                return m_TEMPO_LEITURA;
            }
            set
            {
                m_TEMPO_LEITURA = value;
            }
        }
        public int TEMPO_TRANSFERENCIA
        {
            get
            {
                return m_TEMPO_TRANSFERENCIA;
            }
            set
            {
                m_TEMPO_TRANSFERENCIA = value;
            }
        }
        public bool IND_VALIDA_DESCMAX_TABELA_PEDIDO
        {
            get
            {
                return m_IND_VALIDA_DESCMAX_TABELA_PEDIDO;
            }
            set
            {
                m_IND_VALIDA_DESCMAX_TABELA_PEDIDO = value;
            }
        }

        public bool IND_RATEIO_INDENIZACAO
        {
            get
            {
                return m_IND_RATEIO_INDENIZACAO;
            }
            set
            {
                m_IND_RATEIO_INDENIZACAO = value;
            }
        }

        public bool IND_INDENIZACAO_MAIORVENDA
        {
            get
            {
                return m_IND_INDENIZACAO_MAIORVENDA;
            }
            set
            {
                m_IND_INDENIZACAO_MAIORVENDA = value;
            }
        }

        public bool IND_HIERARQUIA_DESCONTO_MAXIMO
        {
            get
            {
                return m_IND_HIERARQUIA_DESCONTO_MAXIMO;
            }
            set
            {
                m_IND_HIERARQUIA_DESCONTO_MAXIMO = value;
            }
        }

        public bool IND_CLUSTER_PRODUTOS_PELO_TIPO_FREEZER
        {
            get
            {
                return m_IND_CLUSTER_PRODUTOS_PELO_TIPO_FREEZER;
            }
            set
            {
                m_IND_CLUSTER_PRODUTOS_PELO_TIPO_FREEZER = value;
            }
        }

        public bool IND_POLITICA_CALCULO_PRECO_MISTA
        {
            get
            {
                return m_IND_POLITICA_CALCULO_PRECO_MISTA;
            }
            set
            {
                m_IND_POLITICA_CALCULO_PRECO_MISTA = value;
            }
        }

        public bool IND_TRABALHA_PEDIDO_SUGERIDO
        {
            get
            {
                return m_IND_TRABALHA_PEDIDO_SUGERIDO;
            }
            set
            {
                m_IND_TRABALHA_PEDIDO_SUGERIDO = value;
            }
        }

        public decimal PCT_TAXA_OCUPACAO_IDEAL_VEICULO_MODELO
        {
            get
            {
                return m_PCT_TAXA_OCUPACAO_IDEAL_VEICULO_MODELO;
            }
            set
            {
                m_PCT_TAXA_OCUPACAO_IDEAL_VEICULO_MODELO = value;
            }
        }

        public bool IND_ASSUME_QTD_SUGERIDA_SOBRE_MINIMA
        {
            get
            {
                return m_IND_ASSUME_QTD_SUGERIDA_SOBRE_MINIMA;
            }
            set
            {
                m_IND_ASSUME_QTD_SUGERIDA_SOBRE_MINIMA = value;
            }
        }


        public int TIPO_TROCA
        {
            get
            {
                return m_TIPO_TROCA;
            }
            set
            {
                m_TIPO_TROCA = value;
            }
        }

        public int IND_QTD_COLETAS_REALIZADAS
        {
            get
            {
                return m_IND_QTD_COLETAS_REALIZADAS;
            }
            set
            {
                m_IND_QTD_COLETAS_REALIZADAS = value;
            }
        }

        public decimal PCT_ESTOQUE_SEGURANCA
        {
            get
            {
                return m_PCT_ESTOQUE_SEGURANCA;
            }
            set
            {
                m_PCT_ESTOQUE_SEGURANCA = value;
            }
        }

        public decimal PCT_SEGURANCA_PRODUTO_ESTOQUE
        {
            get
            {
                return m_PCT_SEGURANCA_PRODUTO_ESTOQUE;
            }
            set
            {
                m_PCT_SEGURANCA_PRODUTO_ESTOQUE = value;
            }
        }

        public string IND_UTILIZA_INDENIZACAO
        {
            get
            {
                return m_IND_UTILIZA_INDENIZACAO;
            }
            set
            {
                m_IND_UTILIZA_INDENIZACAO = value;
            }
        }

        public string VERSAO_WEBSERVICE
        {
            get
            {
                return m_VERSAO_WEBSERVICE;
            }
            set
            {
                m_VERSAO_WEBSERVICE = value;
            }
        }

        public string IND_MOSTRAR_PRODUTO_BLOQUEADO
        {
            get
            {
                return m_IND_MOSTRAR_PRODUTO_BLOQUEADO;
            }
            set
            {
                m_IND_MOSTRAR_PRODUTO_BLOQUEADO = value;
            }
        }

        public string IND_PERMITIR_VENDA_SOMENTE_LAYOUT
        {
            get
            {
                return m_IND_PERMITIR_VENDA_SOMENTE_LAYOUT;
            }
            set
            {
                m_IND_PERMITIR_VENDA_SOMENTE_LAYOUT = value;
            }
        }

        public int QTD_MAX_VENDA_OUTROS_DANONE
        {
            get
            {
                return m_QTD_MAX_VENDA_OUTROS_DANONE;
            }
            set
            {
                m_QTD_MAX_VENDA_OUTROS_DANONE = value;
            }
        }

        public string VERSAO_AVANTE_SALES_COMPATIBILIDADE
        {
            get
            {
                if (string.IsNullOrEmpty(m_VERSAO_AVANTE_SALES_COMPATIBILIDADE))
                {
                    BuscaVersaoAvanteSalesCompatibilidade();
                }
                return m_VERSAO_AVANTE_SALES_COMPATIBILIDADE;
            }
            set { m_VERSAO_AVANTE_SALES_COMPATIBILIDADE = value; }
        }

        public int NUM_RAIO_LOCALIZACAO
        {
            get
            {
                return m_NUM_RAIO_LOCALIZACAO;
            }
            set
            {
                m_NUM_RAIO_LOCALIZACAO = value;
            }
        }
        /// <summary>
        /// Data: 15/06/2012
        /// Inclusão dos campos nas tabelas abaixo:
        /// EMPRESA: TIPO_TROCA, IND_QTD_COLETAS_REALIZADAS, PCT_ESTOQUE_SEGURANCA ---
        /// PRODUTO_COLETA_ESTOQUE: QTD_PERDA, QTD_GIRO_SELLOUT, SOM_UTM_QTD_GIRO_SELLOUT ---
        /// PRODUTO_CATEGORIA: QTD_MINIMA
        /// </summary>
        public bool UtilizaTabelaNova
        {
            get { return m_UtilizaBancoNovo; }
            set { m_UtilizaBancoNovo = value; }
        }

        public static bool UtilizaNovoAtributoPedido
        {
            get
            {
                return ColunaExiste("PEDIDO", "BOL_PEDIDO_VALIDADO");
            }
        }

        /// <summary>
        /// Data: 31/07/2012
        /// Inclusão do campo de descrição Denver (DSC_DENVER) na tabela de PDV
        /// </summary>
        public bool UtilizaDescricaoDenver
        {
            get { return m_UtilizaDescricaoDenver; }
            set { m_UtilizaDescricaoDenver = value; }
        }

        public int COD_NOTEBOOK1
        {
            get
            {
                return m_COD_NOTEBOOK1;
            }
            set
            {
                m_COD_NOTEBOOK1 = value;
            }
        }

        public int COD_NOTEBOOK2
        {
            get
            {
                return m_COD_NOTEBOOK2;
            }
            set
            {
                m_COD_NOTEBOOK2 = value;
            }
        }

        public string IND_PDV_BUNGE_COMPRA_OUTRAS_MARCAS
        {
            get
            {
                return m_IND_PDV_BUNGE_COMPRA_OUTRAS_MARCAS;
            }
            set
            {
                m_IND_PDV_BUNGE_COMPRA_OUTRAS_MARCAS = value;
            }
        }

        public bool IND_UTILIZA_PRICE_2014
        {
            get
            {
                return m_IND_UTILIZA_PRICE_2014;
            }
            set
            {
                m_IND_UTILIZA_PRICE_2014 = value;
            }
        }

        public bool IND_EMPRESA_FERIADO
        {
            get
            {
                return m_IND_EMPRESA_FERIADO;
            }
            set
            {
                m_IND_EMPRESA_FERIADO = value;
            }
        }

        #endregion

        #region [ Metodos ]

        public CSEmpresa()
        {
            ATUALIZA_EMPRESA();

        }

        public string BuscaVersaoAvanteSalesCompatibilidade()
        {
            if (ColunaExiste("EMPRESA", "VERSAO_AVANTE_SALES_COMPATIBILIDADE"))
            {
                m_VERSAO_AVANTE_SALES_COMPATIBILIDADE = CSDataAccess.Instance.ExecuteScalar("SELECT VERSAO_AVANTE_SALES_COMPATIBILIDADE FROM EMPRESA").ToString();
                return m_VERSAO_AVANTE_SALES_COMPATIBILIDADE;
            }
            return null;
        }

        public void ATUALIZA_EMPRESA()
        {
            try
            {
#if ANDROID
                bool colunaConexaoFlexXGpsAndroidCriada = ColunaExiste("EMPRESA", "CONEXAO_FLEXX_GPS_ANDROID");
#endif

                StringBuilder sqlQuery = new StringBuilder();
                sqlQuery.Append("SELECT IND_LIMITE_CREDITO, COD_ESTADO, COD_CGC, COD_EMPRESA, IND_LIMITE_DESCONTO ");
                sqlQuery.Append("      ,DES_INFORMACAO, DIRETORIO_INTERFACE_PALMTOP,IND_BLOQUEIO_VALOR_MINIMO ");
                sqlQuery.Append("      ,IND_VALIDA_QTDMULTIPLA_NAO_VENDA, IND_LIBERA_CLIENTE_BLOQUEADO ");
                sqlQuery.Append("      ,IND_VALIDA_PCT_MAXIMO_DESCONTO,'' AS VENDAFORAROTA ");
                sqlQuery.Append("      ,TIPO_CALCULO_LUCRATIVIDADE,CONFIGURACAO_LUCRATIVIDADE_PEDIDO ");
                sqlQuery.Append("      ,PCT_MINIMO_LUCRATIVIDADE,CONFIGURACAO_LUCRATIVIDADE_ITEM_PEDIDO ");
                sqlQuery.Append("      ,INFORMACOES_SINCRONIZACAO.DATA_ULTIMA_SINCRONIZACAO ");
                sqlQuery.Append("      ,EMPRESA.IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE ");
                sqlQuery.Append("      ,EMPRESA.IND_TRABALHA_CONFIGURACAO_MULTIPLA_EMPRESA ");
                sqlQuery.Append("      ,EMPRESA.IND_CONSOLIDAR_FINANCEIRO,EMPRESA.COD_REVENDA ");
                sqlQuery.Append("      ,TIPO_CALCULO_VERBA,IND_VLR_VERBA_EXTRA_ATUSALDO,IND_VLR_INDENIZACAO_ATUSALDO ");
                sqlQuery.Append("      ,IND_VLR_VERBA_PEDIDO_NOVO_ATUSALDO,IND_ATUALIZAR_VERBA_PED_ABAIXOMINIMO ");
                sqlQuery.Append("      ,PCT_MAXIMO_INDENIZACAO,PCT_VERBA_NORMAL,TPO_LICENCA, VAL_LIMNF_IDZ ");
                sqlQuery.Append("      ,IND_ALTERA_VENDEDOR, IND_VISUALIZA_LUCRATIVIDADE, IND_LISTA_PROD_NAOVENDIDO, IND_UTILIZA_FLEXX_GPS");
#if ANDROID
                if (colunaConexaoFlexXGpsAndroidCriada)
                    sqlQuery.Append("      ,CONEXAO_FLEXX_GPS_ANDROID ");
                else
                    sqlQuery.Append("      ,CONEXAO_FLEXX_GPS ");

#else
                sqlQuery.Append("      ,CONEXAO_FLEXX_GPS ");
#endif
                sqlQuery.Append("      ,TEMPO_LEITURA, TEMPO_TRANSFERENCIA, IND_VALIDA_DESCMAX_TABELA_PEDIDO ");
                sqlQuery.Append("      ,IND_RATEIO_INDENIZACAO, IND_INDENIZACAO_MAIORVENDA, IND_HIERARQUIA_DESCONTO_MAXIMO ");
                sqlQuery.Append("      ,IND_CLUSTER_PRODUTOS_PELO_TIPO_FREEZER, IND_POLITICA_CALCULO_PRECO_MISTA, IND_TRABALHA_PEDIDO_SUGERIDO ");

                UtilizaTabelaNova = ColunaExiste("EMPRESA", "PCT_SEGURANCA_PRODUTO_ESTOQUE");
                if (UtilizaTabelaNova)
                {
                    sqlQuery.Append("      ,TIPO_TROCA");
                    sqlQuery.Append("      , IND_QTD_COLETAS_REALIZADAS");
                    sqlQuery.Append("      , PCT_ESTOQUE_SEGURANCA");
                    sqlQuery.Append("      , PCT_SEGURANCA_PRODUTO_ESTOQUE");
                    sqlQuery.Append("      , IND_ASSUME_QTD_SUGERIDA_SOBRE_MINIMA");
                }

                bool compatibilidadeDeVersao = ColunaExiste("EMPRESA", "VERSAO_AVANTE_SALES_COMPATIBILIDADE");
                if (compatibilidadeDeVersao)
                {
                    sqlQuery.Append("      , VERSAO_AVANTE_SALES_COMPATIBILIDADE");
                }

                bool OcupacaoIdealVeiculo = ColunaExiste("EMPRESA", "PCT_TAXA_OCUPACAO_IDEAL_VEICULO_MODELO");
                if (OcupacaoIdealVeiculo)
                {
                    sqlQuery.Append("      , PCT_TAXA_OCUPACAO_IDEAL_VEICULO_MODELO");
                }

                if (ColunaExiste("EMPRESA", "IND_UTILIZA_INDENIZACAO"))
                    sqlQuery.Append(", IND_UTILIZA_INDENIZACAO");

                if (ColunaExiste("EMPRESA", "COD_NOTEBOOK1"))
                    sqlQuery.Append(", COD_NOTEBOOK1");

                if (ColunaExiste("EMPRESA", "COD_NOTEBOOK2"))
                    sqlQuery.Append(", COD_NOTEBOOK2");

                if (ColunaExiste("EMPRESA", "IND_PDV_BUNGE_COMPRA_OUTRAS_MARCAS"))
                    sqlQuery.Append(", IND_PDV_BUNGE_COMPRA_OUTRAS_MARCAS");

                if (ColunaExiste("EMPRESA", "IND_MOSTRAR_PRODUTO_BLOQUEADO"))
                    sqlQuery.Append(", IND_MOSTRAR_PRODUTO_BLOQUEADO");

                if (ColunaExiste("EMPRESA", "VERSAO_WEBSERVICE"))
                    sqlQuery.Append(", VERSAO_WEBSERVICE");

                if (ColunaExiste("EMPRESA", "IND_UTILIZA_ENVIO_EMAIL"))
                    sqlQuery.Append(", IND_UTILIZA_ENVIO_EMAIL");

                if (ColunaExiste("EMPRESA", "IND_PERMITIR_VENDA_SOMENTE_LAYOUT"))
                    sqlQuery.Append(", IND_PERMITIR_VENDA_SOMENTE_LAYOUT");

                if (ColunaExiste("EMPRESA", "QTD_MAX_VENDA_OUTROS_DANONE"))
                    sqlQuery.Append(", QTD_MAX_VENDA_OUTROS_DANONE");

                if (ColunaExiste("EMPRESA", "IND_CADASTRO_CONTATO_PDV_OBRIGATORIO"))
                    sqlQuery.Append(", IND_CADASTRO_CONTATO_PDV_OBRIGATORIO");

                if (ColunaExiste("EMPRESA", "NUM_RAIO_LOCALIZACAO"))
                    sqlQuery.Append(", NUM_RAIO_LOCALIZACAO");

                if (ColunaExiste("EMPRESA", "IND_EMPRESA_FERIADO"))
                    sqlQuery.Append(", IND_EMPRESA_FERIADO");

                sqlQuery.Append("  FROM EMPRESA, INFORMACOES_SINCRONIZACAO ");

                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString(), CommandBehavior.SingleResult))
                {
                    while (sqlReader.Read())
                    {
                        this.IND_LIMITE_CREDITO = sqlReader.GetValue(0) == System.DBNull.Value ? "N" : sqlReader.GetString(0).ToUpper();
                        this.COD_ESTADO = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1);
                        this.COD_CGC = sqlReader.GetValue(2) == System.DBNull.Value ? "" : sqlReader.GetString(2);
                        this.COD_EMPRESA = sqlReader.GetInt32(3);
                        this.IND_LIMITE_DESCONTO = sqlReader.GetBoolean(4);
                        this.DES_INFORMACAO = sqlReader.GetValue(5) == System.DBNull.Value ? "" : sqlReader.GetString(5);
                        this.DIRETORIO_INTERFACE_PALMTOP = sqlReader.GetValue(6) == System.DBNull.Value ? "" : sqlReader.GetString(6);
                        this.IND_BLOQUEIO_VALOR_MINIMO = sqlReader.GetValue(7) == System.DBNull.Value ? "N" : sqlReader.GetString(7).ToUpper();
                        this.IND_VALIDA_QTDMULTIPLA_NAO_VENDA = sqlReader.GetValue(8) == System.DBNull.Value ? "N" : sqlReader.GetString(8).ToUpper();
                        this.IND_LIBERA_CLIENTE_BLOQUEADO = sqlReader.GetValue(9) == System.DBNull.Value ? "N" : sqlReader.GetString(9).ToUpper();
                        this.IND_VALIDA_PCT_MAXIMO_DESCONTO = sqlReader.GetValue(10) == System.DBNull.Value ? "N" : sqlReader.GetString(10).ToUpper();
                        //this.IND_PERMITIR_VENDA_FORA_ROTA = sqlReader.GetValue(11) == System.DBNull.Value ? "N" : sqlReader.GetString(11).ToUpper();
                        this.TIPO_CALCULO_LUCRATIVIDADE = sqlReader.GetValue(12) == System.DBNull.Value ? 1 : sqlReader.GetInt32(12);
                        this.CONFIGURACAO_LUCRATIVIDADE_PEDIDO = sqlReader.GetValue(13) == System.DBNull.Value ? 'A' : Convert.ToChar(sqlReader.GetString(13));
                        this.PCT_MINIMO_LUCRATIVIDADE = sqlReader.GetValue(14) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(14));
                        this.CONFIGURACAO_LUCRATIVIDADE_ITEM_PEDIDO = sqlReader.GetValue(15) == System.DBNull.Value ? 'A' : System.Convert.ToChar(sqlReader.GetString(15));
                        this.m_DATA_ULTIMA_DESCARGA = sqlReader.GetString(16) == "" ? DateTime.Now : Convert.ToDateTime(sqlReader.GetString(16));
                        this.IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE = sqlReader.GetValue(17) == System.DBNull.Value ? "" : sqlReader.GetString(17).ToUpper();
                        this.IND_TRABALHA_CONFIGURACAO_MULTIPLA_EMPRESA = sqlReader.GetValue(18) == System.DBNull.Value ? "" : sqlReader.GetString(18).ToUpper();
                        this.IND_CONSOLIDAR_FINANCEIRO = sqlReader.GetValue(19) == System.DBNull.Value ? "" : sqlReader.GetString(19).ToUpper();
                        this.CODIGO_REVENDA = sqlReader.GetValue(20) == System.DBNull.Value ? "" : sqlReader.GetString(20).ToUpper();
                        this.TIPO_CALCULO_VERBA = sqlReader.GetValue(21) == System.DBNull.Value ? 0 : sqlReader.GetInt32(21);
                        this.IND_VLR_VERBA_EXTRA_ATUSALDO = sqlReader.GetValue(22) == System.DBNull.Value ? false : (sqlReader.GetString(22).ToLower() == "s");
                        this.IND_VLR_INDENIZACAO_ATUSALDO = sqlReader.GetValue(23) == System.DBNull.Value ? 3 : int.Parse(sqlReader.GetString(23));
                        this.IND_VLR_VERBA_PEDIDO_NOVO_ATUSALDO = sqlReader.GetValue(24) == System.DBNull.Value ? false : (sqlReader.GetString(24).ToLower() == "s");
                        this.IND_ATUALIZAR_VERBA_PED_ABAIXOMINIMO = sqlReader.GetValue(25) == System.DBNull.Value ? false : (sqlReader.GetString(25).ToLower() == "s");
                        this.PCT_MAXIMO_INDENIZACAO = sqlReader.GetValue(26) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(26));
                        this.PCT_VERBA_NORMAL = sqlReader.GetValue(27) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(27));
                        this.TPO_LICENCA = sqlReader.GetValue(28) == System.DBNull.Value ? 0 : sqlReader.GetInt32(28);
                        this.VAL_LIMNF_IDZ = sqlReader.GetValue(29) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(29));
                        this.IND_ALTERA_VENDEDOR = sqlReader.GetValue(30) == System.DBNull.Value ? "N" : sqlReader.GetString(30).ToUpper();
                        this.IND_VISUALIZA_LUCRATIVIDADE = sqlReader.GetValue(31) == System.DBNull.Value ? "N" : sqlReader.GetString(31).ToUpper();
                        this.IND_LISTA_PROD_NAOVENDIDO = sqlReader.GetValue(32) == System.DBNull.Value ? "N" : sqlReader.GetString(32).ToUpper();
                        this.IND_UTILIZA_FLEXX_GPS = sqlReader.GetValue(33) == System.DBNull.Value ? "N" : sqlReader.GetString(33).ToUpper();

#if ANDROID
                        if (colunaConexaoFlexXGpsAndroidCriada)
                            this.CONEXAO_FLEXX_GPS = sqlReader.GetValue(34) == System.DBNull.Value ? "http://flexx2.flexxgps.com.br/" : sqlReader.GetString(34);
                        else
                            this.CONEXAO_FLEXX_GPS = "http://flexx2.flexxgps.com.br/";
#else
                        this.CONEXAO_FLEXX_GPS = sqlReader.GetValue(34) == System.DBNull.Value ? "" : sqlReader.GetString(34);
#endif

                        this.TEMPO_LEITURA = sqlReader.GetValue(35) == System.DBNull.Value ? 0 : sqlReader.GetInt32(35);
                        this.TEMPO_TRANSFERENCIA = sqlReader.GetValue(36) == System.DBNull.Value ? 0 : sqlReader.GetInt32(36);
                        this.IND_VALIDA_DESCMAX_TABELA_PEDIDO = sqlReader.GetBoolean(37);
                        this.IND_RATEIO_INDENIZACAO = sqlReader.GetValue(38) == System.DBNull.Value ? false : (sqlReader.GetString(38).ToLower() == "s");
                        this.IND_INDENIZACAO_MAIORVENDA = sqlReader.GetValue(39) == System.DBNull.Value ? false : (sqlReader.GetString(39).ToLower() == "s");
                        this.IND_HIERARQUIA_DESCONTO_MAXIMO = sqlReader.GetValue(40) == System.DBNull.Value ? false : (sqlReader.GetString(40).ToLower() == "s");
                        this.IND_CLUSTER_PRODUTOS_PELO_TIPO_FREEZER = sqlReader.GetValue(41) == System.DBNull.Value ? false : (sqlReader.GetString(41).ToLower() == "s");
                        this.IND_POLITICA_CALCULO_PRECO_MISTA = sqlReader.GetValue(42) == System.DBNull.Value ? false : (sqlReader.GetString(42).ToLower() == "s");
                        this.IND_TRABALHA_PEDIDO_SUGERIDO = sqlReader.GetValue(43) == System.DBNull.Value ? false : (sqlReader.GetString(43).ToLower() == "s");

                        if (UtilizaTabelaNova)
                        {
                            this.TIPO_TROCA = sqlReader.GetValue(44) == System.DBNull.Value ? 0 : sqlReader.GetInt32(44);
                            this.IND_QTD_COLETAS_REALIZADAS = sqlReader.GetValue(45) == System.DBNull.Value ? 0 : sqlReader.GetInt32(45);
                            this.PCT_ESTOQUE_SEGURANCA = sqlReader.GetValue(46) == System.DBNull.Value ? 0 : decimal.Parse(sqlReader.GetValue(46).ToString());
                            this.PCT_SEGURANCA_PRODUTO_ESTOQUE = sqlReader.GetValue(47) == System.DBNull.Value ? 0 : decimal.Parse(sqlReader.GetValue(47).ToString());
                            this.IND_ASSUME_QTD_SUGERIDA_SOBRE_MINIMA = sqlReader.GetValue(48) == System.DBNull.Value ? false : (sqlReader.GetString(48).ToLower() == "s");
                        }
                        if (compatibilidadeDeVersao)
                        {
                            this.VERSAO_AVANTE_SALES_COMPATIBILIDADE = sqlReader.GetValue(49) == System.DBNull.Value ? "" : sqlReader.GetString(49);
                        }

                        int valor = compatibilidadeDeVersao ? 50 : 49;

                        if (OcupacaoIdealVeiculo)
                        {
                            this.PCT_TAXA_OCUPACAO_IDEAL_VEICULO_MODELO = sqlReader.GetValue(valor) == System.DBNull.Value ? 0m : Convert.ToDecimal(sqlReader.GetValue(valor));

                            if (ColunaExiste("EMPRESA", "IND_UTILIZA_INDENIZACAO"))
                            {
                                valor++;
                                this.IND_UTILIZA_INDENIZACAO = sqlReader.GetValue(valor) == System.DBNull.Value ? "N" : sqlReader.GetString(valor);
                            }
                        }
                        else
                        {
                            if (ColunaExiste("EMPRESA", "IND_UTILIZA_INDENIZACAO"))
                                this.IND_UTILIZA_INDENIZACAO = sqlReader.GetValue(50) == System.DBNull.Value ? "N" : sqlReader.GetString(50);
                        }

                        if (ColunaExiste("EMPRESA", "COD_NOTEBOOK1"))
                        {
                            valor++;
                            this.COD_NOTEBOOK1 = sqlReader.GetValue(valor) == System.DBNull.Value ? 0 : sqlReader.GetInt32(valor);
                        }

                        if (ColunaExiste("EMPRESA", "COD_NOTEBOOK2"))
                        {
                            valor++;
                            this.COD_NOTEBOOK2 = sqlReader.GetValue(valor) == System.DBNull.Value ? 0 : sqlReader.GetInt32(valor);
                        }

                        if (ColunaExiste("EMPRESA", "IND_PDV_BUNGE_COMPRA_OUTRAS_MARCAS"))
                        {
                            valor++;
                            this.IND_PDV_BUNGE_COMPRA_OUTRAS_MARCAS = sqlReader.GetValue(valor) == System.DBNull.Value ? "N" : sqlReader.GetString(valor);
                        }

                        if (ColunaExiste("EMPRESA", "IND_MOSTRAR_PRODUTO_BLOQUEADO"))
                        {
                            valor++;
                            this.IND_MOSTRAR_PRODUTO_BLOQUEADO = sqlReader.GetValue(valor) == System.DBNull.Value ? "S" : sqlReader.GetString(valor);
                        }

                        if (ColunaExiste("EMPRESA", "VERSAO_WEBSERVICE"))
                        {
                            valor++;
                            this.VERSAO_WEBSERVICE = sqlReader.GetValue(valor) == System.DBNull.Value ? string.Empty : sqlReader.GetString(valor);
                        }

                        if (ColunaExiste("EMPRESA", "IND_UTILIZA_ENVIO_EMAIL"))
                        {
                            valor++;

                            if (ColunaExiste("PDV_EMAIL", "COD_PDV"))
                                this.IND_UTILIZA_ENVIO_EMAIL = sqlReader.GetValue(valor) == System.DBNull.Value ? "N" : sqlReader.GetString(valor);
                            else
                                this.IND_UTILIZA_ENVIO_EMAIL = "N";
                        }
                        else
                            this.IND_UTILIZA_ENVIO_EMAIL = "N";

                        if (ColunaExiste("EMPRESA", "IND_PERMITIR_VENDA_SOMENTE_LAYOUT"))
                        {
                            valor++;
                            this.IND_PERMITIR_VENDA_SOMENTE_LAYOUT = sqlReader.GetValue(valor) == System.DBNull.Value ? "N" : sqlReader.GetString(valor);
                        }

                        if (ColunaExiste("EMPRESA", "QTD_MAX_VENDA_OUTROS_DANONE"))
                        {
                            valor++;
                            this.QTD_MAX_VENDA_OUTROS_DANONE = sqlReader.GetValue(valor) == System.DBNull.Value ? 0 : sqlReader.GetInt32(valor);
                        }

                        if (ColunaExiste("EMPRESA", "IND_CADASTRO_CONTATO_PDV_OBRIGATORIO"))
                        {
                            valor++;
                            this.IND_CADASTRO_CONTATO_PDV_OBRIGATORIO = sqlReader.GetValue(valor) == System.DBNull.Value ? "S" : sqlReader.GetString(valor);
                        }
                        else
                            this.IND_CADASTRO_CONTATO_PDV_OBRIGATORIO = "N";

                        if (ColunaExiste("EMPRESA", "NUM_RAIO_LOCALIZACAO"))
                        {
                            valor++;
                            this.NUM_RAIO_LOCALIZACAO = sqlReader.GetValue(valor) == System.DBNull.Value ? 100 : sqlReader.GetInt32(valor);
                        }
                        else
                            this.NUM_RAIO_LOCALIZACAO = 100;

                        if (ColunaExiste("EMPRESA", "IND_EMPRESA_FERIADO"))
                        {
                            valor++;
                            this.IND_EMPRESA_FERIADO = sqlReader.GetValue(valor) == System.DBNull.Value ? false : sqlReader.GetString(valor) == "S";
                        }
                        else
                            this.IND_EMPRESA_FERIADO = false;

                        this.IND_UTILIZA_PRICE_2014 = ColunaExiste("BRK_EPRODAVAL", "CDPRD");
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca da empresa", ex);
            }
        }

        public static bool ColunaExiste(string tableName, string columnName)
        {
            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(string.Format("PRAGMA TABLE_INFO('{0}')", tableName)))
            {
                while (sqlReader.Read())
                {
                    var column_name = sqlReader.GetString(1);
                    if (column_name == columnName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        public static List<string> InformacoesAcesso(string query)
        {
            List<string> informacoes = new List<string>();

            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(query))
            {
                while (sqlReader.Read())
                {
                    informacoes.Add(sqlReader.GetString(0));
                    informacoes.Add(sqlReader.GetString(1));
                    informacoes.Add(sqlReader.GetValue(2).ToString());
                    informacoes.Add(sqlReader.GetString(3));
                    informacoes.Add(sqlReader.GetString(4));
                    informacoes.Add(sqlReader.GetString(5));
                    informacoes.Add(sqlReader.GetString(6));
                    informacoes.Add(sqlReader.GetString(7));
                    informacoes.Add(sqlReader.GetString(8));
                }
            }

            return informacoes;
        }
    }
}