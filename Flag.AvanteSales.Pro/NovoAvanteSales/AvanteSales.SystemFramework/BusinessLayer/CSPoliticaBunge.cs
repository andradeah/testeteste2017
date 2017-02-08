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
    public class CSPoliticaBunge
    {
        public decimal ValorTabela { get; set; }
        public decimal ValorFinalProduto { get; set; }

        #region [ Variáveis ]

        DateTime DATA;
        int PRODUTO;
        int CLIENTE;
        int NR_NOTEBOOK;
        string CONDPAGTO;
        int QTDINTEIRA;
        int QTDFRACAO;
        decimal PERCDESCONTO;
        int QTDUVCAIXA;

        decimal PRECO_FINAL = 0m;
        decimal PRECO_UNITARIO = 0m;
        decimal DESCONTO = 0m;
        decimal PIS = 0m;
        decimal COFINS = 0m;
        decimal ICMS = 0m;
        decimal? VLR_UNITARIO = 0m;
        int QT_DIAS;
        string NOTIFICACAO;
        int PRODUTO_BUNGE;
        string CD_NEGOCIO;
        string CD_CLASSIFICACAO_FISCAL;

        int SEQUENCIA;
        int CD_GRUPO_IMPOSTOS;
        string CD_CAMPO_CHAVE1;
        string CD_CAMPO_CHAVE2;
        string CD_CAMPO_CHAVE3;

        decimal? PR_TAXA_PIS = null;
        decimal PR_BASE_PIS = 0m;
        decimal? PR_TAXA_COFINS = null;
        decimal PR_BASE_COFINS = 0m;
        decimal? PR_TAXA_ICMS = null;
        decimal PR_BASE_ICMS = 0m;

        static decimal DESCONTO_ANTERIOR = 0m;
        static decimal VALOR_ANTERIOR = 0m;

        #endregion

        public CSPoliticaBunge(int produto, int nr_notebook, int cliente, DateTime data, CSProdutos.CSProduto produtoAtual, int qtdInteira, int qtdUnitaria, decimal desconto, decimal? valorUnitario)
        {
            InicializarVariaveis();

            NR_NOTEBOOK = nr_notebook;
            CLIENTE = cliente;
            DATA = data;
            QTDINTEIRA = qtdInteira;
            QTDFRACAO = qtdUnitaria;
            QTDUVCAIXA = produtoAtual.QTD_UNIDADE_EMBALAGEM;
            PERCDESCONTO = desconto;
            PRODUTO = produto;
            VLR_UNITARIO = valorUnitario;

            DESCONTO = PERCDESCONTO;
        }

        public CSPoliticaBunge(int produto, int nr_notebook)
        {
            PRODUTO = produto;
            NR_NOTEBOOK = nr_notebook;
        }

        //Validação superficial para validação do PREÇO do produto existente na pricing
        public bool ValidacaoPrecoProduto()
        {
            bool retorno = true;

            DadosProduto(PRODUTO, NR_NOTEBOOK);

            if (PRODUTO_BUNGE == 0)
                retorno = false;

            if (retorno)
            {
                StringBuilder sql = new StringBuilder();

                sql.AppendLine("SELECT (CASE  WHEN VL_PRECO_UNITARIO_FOB = 0 ");
                sql.AppendLine("                              THEN VL_PRECO_UNITARIO_CIF ");
                sql.AppendLine("              ELSE VL_PRECO_UNITARIO_FOB ");
                sql.AppendLine("END) AS PRECO ");
                sql.AppendLine(" FROM BNG_SGT_PRECO_ITEM_TAB_AGV_SAP ");
                sql.AppendFormat(" WHERE CAST(CD_ITEM AS INT) = {0} ", PRODUTO_BUNGE);
                sql.AppendLine(" AND DATE('now') BETWEEN DATE(DT_INICIO_VIGENCIA) AND IFNULL(DT_FIM_VIGENCIA,DATE('now')) ");
                sql.AppendLine(" ORDER BY CD_ITEM ");
                sql.AppendLine(", CD_BASE_CLIENTE ");
                sql.AppendLine(", CD_LOJA_CLIENTE ");
                sql.AppendLine(", CD_GRUPO_SEGMENTO DESC ");
                sql.AppendLine(", CD_FILIAL ");
                sql.AppendLine(", CD_MICRORREGIAO_CML ");
                sql.AppendLine(", CD_UNIDADE_FEDERACAO DESC ");
                sql.AppendLine(", CD_REGIONAL_VENDA  DESC ");
                sql.AppendLine(", DT_INICIO_VIGENCIA DESC ");
                sql.AppendLine(" LIMIT 1");

                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
                {
                    if (sqlReader.Read())
                        retorno = true;
                    else
                        retorno = false;
                }
            }

            return retorno;
        }

        private void InicializarVariaveis()
        {
            PRECO_FINAL = 0m;
            PRECO_UNITARIO = 0m;
            DESCONTO = 0m;
            PIS = 0m;
            COFINS = 0m;
            ICMS = 0m;
            VLR_UNITARIO = 0m;
            PR_TAXA_PIS = null;
            PR_BASE_PIS = 0m;
            PR_TAXA_COFINS = null;
            PR_BASE_COFINS = 0m;
            PR_TAXA_ICMS = null;
            PR_BASE_ICMS = 0m;
        }

        public void DadosIniciais()
        {
            DadosProduto(PRODUTO, NR_NOTEBOOK);

            ValidarProduto();

            CSDataAccess.Instance.ClearTable("TMPVARIAVEIS_BNG");

            InserirTabelaTemporaria();

            ValidarCliente();

            PrecoDoItem();

            ValidaPrecoItem();
        }

        public void ValorFinal()
        {
            DadosIniciais();

            PrimeiraTentativaImpostos();

            CalculoImpostos();

            CalcularValorFinal();
        }

        private void ValidarProduto()
        {
            if (PRODUTO_BUNGE == 0)
                throw new Exception("Produto não existente na princing.");
        }

        public decimal CalcularValorTabela()
        {
            return PRECO_UNITARIO = (PRECO_UNITARIO / ((100 - (ICMS + PIS + COFINS)) / 100));
        }

        private void CalcularValorFinal()
        {
            ValorTabela = PRECO_UNITARIO = CalcularValorTabela();

            if (DESCONTO_ANTERIOR != DESCONTO ||
                VALOR_ANTERIOR != VLR_UNITARIO)
            {
                if (DESCONTO == 0)
                {
                    if (VLR_UNITARIO.HasValue &&
                        VLR_UNITARIO.Value != 0)
                    {
                        //VLR_UNITARIO = PRECO_UNITARIO;

                        if (VLR_UNITARIO < Math.Round(PRECO_UNITARIO, 2, MidpointRounding.AwayFromZero))
                            throw new Exception("Valor unitário menor que o valor da pricing.");
                        else if (VLR_UNITARIO > (Math.Round((PRECO_UNITARIO * 2), 2, MidpointRounding.AwayFromZero)))
                            throw new Exception("Valor unitário maior que o permitido");
                        else if (Math.Round(PRECO_UNITARIO, 2, MidpointRounding.AwayFromZero) != VLR_UNITARIO)
                            PRECO_UNITARIO = VLR_UNITARIO.Value;
                    }
                }
                else
                {
                    if (VLR_UNITARIO > (Math.Round((PRECO_UNITARIO * 2), 2, MidpointRounding.AwayFromZero)))
                        throw new Exception("Valor unitário maior que o permitido");
                    else if (Math.Round(PRECO_UNITARIO, 2, MidpointRounding.AwayFromZero) != VLR_UNITARIO)
                        PRECO_UNITARIO = VLR_UNITARIO.Value;
                }
            }
            else
                if (VALOR_ANTERIOR != 0m)
                    PRECO_UNITARIO = VALOR_ANTERIOR;

            ///* Cálculo do Desconto */
            //if (DESCONTO > PERCDESCONTO && PERCDESCONTO > 0)
            //    DESCONTO = PERCDESCONTO;

            if (DESCONTO > 0 &&
                VALOR_ANTERIOR != VLR_UNITARIO ||
                DESCONTO_ANTERIOR != DESCONTO)
                PRECO_UNITARIO = PRECO_UNITARIO * (1 + (DESCONTO * -1) / 100);

            /* Cálculo Final */
            PRECO_FINAL = (PRECO_UNITARIO * QTDINTEIRA);

            if (QTDFRACAO > 0)
                PRECO_FINAL = PRECO_FINAL + ((PRECO_UNITARIO / QTDUVCAIXA) * QTDFRACAO);

            DESCONTO_ANTERIOR = DESCONTO;
            VALOR_ANTERIOR = Math.Round(PRECO_UNITARIO, 2, MidpointRounding.AwayFromZero);

            ValorFinalProduto = PRECO_FINAL;
        }

        private void CalculoImpostos()
        {
            if (PR_TAXA_PIS == null)
                SegundaTentativaPIS();

            if (PR_TAXA_PIS != null)
                PIS = (PR_TAXA_PIS.Value * (PR_BASE_PIS / 100));
            else
                PIS = 0m;

            if (PR_TAXA_COFINS == null)
                SegundaTentativaCOFINS();

            if (PR_TAXA_COFINS != null)
                COFINS = (PR_TAXA_COFINS.Value * (PR_BASE_COFINS / 100));
            else
                COFINS = 0m;

            if (PR_TAXA_ICMS == null)
                SegundaTentativaICMS();

            if (PR_TAXA_ICMS != null)
                ICMS = (PR_TAXA_ICMS.Value * (PR_BASE_ICMS / 100));
            else
                ICMS = 0m;
        }

        private void SegundaTentativaICMS()
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("SELECT T1.PR_TAXA_IMPOSTO ");
            sql.AppendLine(", 100");
            sql.AppendLine("FROM BNG_RGR_FISCAL_ORG_DST_AGV_SAP T1 ");
            sql.AppendLine("WHERE T1.CD_UNIDADE_FEDERACAO_ORI = (SELECT CD_UNIDADE_FEDERACAO FROM TMPVARIAVEIS_BNG) ");
            sql.AppendLine("AND T1.CD_UNIDADE_FEDERACAO_DST = (SELECT CD_UNIDADE_FEDERACAO FROM TMPVARIAVEIS_BNG) ");
            sql.AppendFormat("AND DATE('now') BETWEEN T1.DT_INICIO_VIGENCIA AND '9999-12-31'");

            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
            {
                while (sqlReader.Read())
                {
                    PR_TAXA_ICMS = sqlReader.GetValue(0) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(0));
                    PR_BASE_ICMS = sqlReader.GetValue(1) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(1));
                }
            }
        }

        private void SegundaTentativaCOFINS()
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("SELECT T1.PR_TAXA_IMPOSTO,100 ");
            sql.AppendLine("FROM BNG_TB_ESTRUTURA T1 ");
            sql.AppendLine("WHERE T1.COD_IMPOSTO = 5 ");

            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
            {
                while (sqlReader.Read())
                {
                    PR_TAXA_COFINS = sqlReader.GetValue(0) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(0));
                    PR_BASE_COFINS = sqlReader.GetValue(1) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(1));
                }
            }
        }

        private void SegundaTentativaPIS()
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("SELECT T1.PR_TAXA_IMPOSTO,100 ");
            sql.AppendLine("FROM BNG_TB_ESTRUTURA T1 ");
            sql.AppendLine("WHERE T1.COD_IMPOSTO = 4 ");

            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
            {
                while (sqlReader.Read())
                {
                    PR_TAXA_PIS = sqlReader.GetValue(0) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(0));
                    PR_BASE_PIS = sqlReader.GetValue(1) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(1));
                }
            }
        }

        private void PrimeiraTentativaImpostos()
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("SELECT T1.CD_GRUPO_IMPOSTO ");
            //sql.AppendLine(", T1.SEQUENCIA ");
            //sql.AppendLine(", T1.CD_CAMPO_CHAVE1 ");
            //sql.AppendLine(", T1.CD_CAMPO_CHAVE2 ");
            //sql.AppendLine(", T1.CD_CAMPO_CHAVE3 ");
            sql.AppendLine("FROM BNG_TB_SEQUENCIA_IMPOSTO T1 ");
            sql.AppendLine("ORDER BY T1.SEQUENCIA ");

            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
            {
                while (sqlReader.Read())
                {
                    CD_GRUPO_IMPOSTOS = sqlReader.GetInt32(0);

                    if (PR_TAXA_PIS == null)
                        BuscaPIS();

                    if (PR_TAXA_COFINS == null)
                        BuscaCOFINS();

                    if (PR_TAXA_ICMS == null)
                        BuscarICMS();

                    if (PR_TAXA_PIS.HasValue &&
                        PR_TAXA_COFINS.HasValue &&
                        PR_TAXA_ICMS.HasValue)
                        break;
                }
            }
        }

        private void BuscarICMS()
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("SELECT T1.PR_TAXA_IMPOSTO ");
            sql.AppendLine(", IFNULL(T1.PR_BASE_IMPOSTO ,100.000000) ");
            sql.AppendLine("FROM BNG_RGR_FSC_ECC_DNM_AGV_SAP T1 ");
            sql.AppendLine("INNER JOIN BNG_TB_SEQUENCIA_IMPOSTO T2 ");
            sql.AppendLine("ON T1.CD_GRUPO_IMPOSTO = T2.CD_GRUPO_IMPOSTO ");
            sql.AppendLine("WHERE T1.CD_UNIDADE_FEDERACAO_ORI = (SELECT CD_UNIDADE_FEDERACAO FROM TMPVARIAVEIS_BNG) ");
            sql.AppendLine("AND T1.CD_UNIDADE_FEDERACAO_DST = (SELECT CD_UNIDADE_FEDERACAO FROM TMPVARIAVEIS_BNG) ");
            sql.AppendLine("AND DATE('now') BETWEEN DATE(T1.DT_INICIO_VIGENCIA) AND DATE(T1.DT_FIM_VIGENCIA) ");
            sql.AppendLine("AND T1.PR_TAXA_IMPOSTO IS NOT NULL ");
            sql.AppendFormat("AND T1.CD_GRUPO_IMPOSTO = '{0}' ", CD_GRUPO_IMPOSTOS);

            switch (CD_GRUPO_IMPOSTOS)
            {
                case 10:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_IMPOSTO FROM TMPVARIAVEIS_BNG) /* (MWSKZ) */ ");
                    }
                    break;
                case 15:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_CLIENTE    FROM TMPVARIAVEIS_BNG) /* (KUNNR) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE2 = (SELECT PRODUTO_BUNGE FROM TMPVARIAVEIS_BNG) /* (MATNR) */ ");
                    }
                    break;
                case 16:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_CLIENTE              FROM TMPVARIAVEIS_BNG) /* (KUNNR) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE2 = (SELECT CD_CLASSIFICACAO_FISCAL FROM TMPVARIAVEIS_BNG) /* (NBM) */ ");
                    }
                    break;
                case 11:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_FILIAL           FROM TMPVARIAVEIS_BNG) /* (WERKS) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE2 = (SELECT CD_SETOR_INDUSTRIAL FROM TMPVARIAVEIS_BNG) /* (BRSCH) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE3 = (SELECT PRODUTO_BUNGE       FROM TMPVARIAVEIS_BNG) /* (MATNR) */ ");
                    }
                    break;
                case 12:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_FILIAL     FROM TMPVARIAVEIS_BNG) /* (WERKS) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE2 = (SELECT PRODUTO_BUNGE FROM TMPVARIAVEIS_BNG) /* (MATNR) */ ");
                    }
                    break;
                case 20:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_SETOR_INDUSTRIAL FROM TMPVARIAVEIS_BNG) /* (BRSCH) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE2 = (SELECT PRODUTO_BUNGE       FROM TMPVARIAVEIS_BNG) /* (MATNR) */ ");
                    }
                    break;
                case 25:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_SETOR_INDUSTRIAL FROM TMPVARIAVEIS_BNG) /* (BRSCH) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE2 = (SELECT CD_NEGOCIO          FROM TMPVARIAVEIS_BNG) /* (MATKL) */ ");
                    }
                    break;
                case 60:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_CLIENTE FROM TMPVARIAVEIS_BNG) /* (KUNNR) */ ");
                    }
                    break;
                case 65:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT PRODUTO_BUNGE FROM TMPVARIAVEIS_BNG) /* (MATNR) */ ");
                    }
                    break;
                case 70:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_NEGOCIO FROM TMPVARIAVEIS_BNG) /* (MATKL) */ ");
                    }
                    break;
                case 85:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_CLASSIFICACAO_FISCAL FROM TMPVARIAVEIS_BNG) /* (NBM) */ ");
                    }
                    break;
                case 27:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_SETOR_INDUSTRIAL FROM TMPVARIAVEIS_BNG) /* (BRSCH) */ ");
                    }
                    break;
            }

            sql.AppendLine(" ORDER BY T2.SEQUENCIA, T1.DT_INICIO_VIGENCIA DESC");

            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
            {
                while (sqlReader.Read())
                {
                    if (sqlReader.GetValue(0) != System.DBNull.Value)
                    {
                        PR_TAXA_ICMS = Convert.ToDecimal(sqlReader.GetValue(0));
                        PR_BASE_ICMS = sqlReader.GetValue(1) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(1));
                    }
                }
            }
        }

        private void BuscaCOFINS()
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("SELECT T1.PR_TAXA_IMPOSTO ");
            sql.AppendLine(", IFNULL(T1.PR_BASE_IMPOSTO ,100.000000) ");
            sql.AppendLine("FROM BNG_RGR_FSC_ECC_PIS_COF_AGV_SAP T1 ");
            sql.AppendLine("INNER JOIN BNG_TB_SEQUENCIA_IMPOSTO T2 ");
            sql.AppendLine("ON T1.CD_GRUPO_IMPOSTO = T2.CD_GRUPO_IMPOSTO ");
            sql.AppendLine("WHERE T1.ID_PIS_COFINS = 'C' /* COFINS */ ");
            sql.AppendLine("AND DATE('now') BETWEEN DATE(T1.DT_INICIO_VIGENCIA) AND DATE(T1.DT_FIM_VIGENCIA) ");
            sql.AppendLine("AND T1.PR_TAXA_IMPOSTO IS NOT NULL ");
            sql.AppendFormat("AND T1.CD_GRUPO_IMPOSTO = '{0}' ", CD_GRUPO_IMPOSTOS);

            switch (CD_GRUPO_IMPOSTOS)
            {
                case 10:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_IMPOSTO FROM TMPVARIAVEIS_BNG) /* (MWSKZ) */ ");
                    }
                    break;
                case 15:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_CLIENTE    FROM TMPVARIAVEIS_BNG) /* (KUNNR) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE2 = (SELECT PRODUTO_BUNGE FROM TMPVARIAVEIS_BNG) /* (MATNR) */ ");
                    }
                    break;
                case 16:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_CLIENTE              FROM TMPVARIAVEIS_BNG) /* (KUNNR) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE2 = (SELECT CD_CLASSIFICACAO_FISCAL FROM TMPVARIAVEIS_BNG) /* (NBM) */ ");
                    }
                    break;
                case 11:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_FILIAL           FROM TMPVARIAVEIS_BNG) /* (WERKS) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE2 = (SELECT CD_SETOR_INDUSTRIAL FROM TMPVARIAVEIS_BNG) /* (BRSCH) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE3 = (SELECT PRODUTO_BUNGE       FROM TMPVARIAVEIS_BNG) /* (MATNR) */ ");
                    }
                    break;
                case 12:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_FILIAL     FROM TMPVARIAVEIS_BNG) /* (WERKS) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE2 = (SELECT PRODUTO_BUNGE FROM TMPVARIAVEIS_BNG) /* (MATNR) */ ");
                    }
                    break;
                case 20:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_SETOR_INDUSTRIAL FROM TMPVARIAVEIS_BNG) /* (BRSCH) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE2 = (SELECT PRODUTO_BUNGE       FROM TMPVARIAVEIS_BNG) /* (MATNR) */ ");
                    }
                    break;
                case 25:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_SETOR_INDUSTRIAL FROM TMPVARIAVEIS_BNG) /* (BRSCH) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE2 = (SELECT CD_NEGOCIO          FROM TMPVARIAVEIS_BNG) /* (MATKL) */ ");
                    }
                    break;
                case 60:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_CLIENTE FROM TMPVARIAVEIS_BNG) /* (KUNNR) */ ");
                    }
                    break;
                case 65:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT PRODUTO_BUNGE FROM TMPVARIAVEIS_BNG) /* (MATNR) */ ");
                    }
                    break;
                case 70:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_NEGOCIO FROM TMPVARIAVEIS_BNG) /* (MATKL) */ ");
                    }
                    break;
                case 85:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_CLASSIFICACAO_FISCAL FROM TMPVARIAVEIS_BNG) /* (NBM) */ ");
                    }
                    break;
                case 27:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_SETOR_INDUSTRIAL FROM TMPVARIAVEIS_BNG) /* (BRSCH) */ ");
                    }
                    break;
            }

            sql.AppendLine("ORDER BY T2.SEQUENCIA, T1.DT_INICIO_VIGENCIA DESC");

            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
            {
                while (sqlReader.Read())
                {
                    if (sqlReader.GetValue(0) != System.DBNull.Value)
                    {
                        PR_TAXA_COFINS = Convert.ToDecimal(sqlReader.GetValue(0));
                        PR_BASE_COFINS = sqlReader.GetValue(1) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(1));
                    }
                }
            }
        }

        private void BuscaPIS()
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("SELECT T1.PR_TAXA_IMPOSTO AS PR_TAXA_PIS");
            sql.AppendLine(", IFNULL(T1.PR_BASE_IMPOSTO ,100.000000) AS PR_BASE_PIS ");
            sql.AppendLine("FROM BNG_RGR_FSC_ECC_PIS_COF_AGV_SAP T1 ");
            sql.AppendLine("INNER JOIN BNG_TB_SEQUENCIA_IMPOSTO T2 ");
            sql.AppendLine("ON T1.CD_GRUPO_IMPOSTO = T2.CD_GRUPO_IMPOSTO ");
            sql.AppendLine("WHERE T1.ID_PIS_COFINS = 'P' /* PIS */ ");
            sql.AppendLine("AND DATE('now') BETWEEN DATE(T1.DT_INICIO_VIGENCIA) AND DATE(T1.DT_FIM_VIGENCIA) ");
            sql.AppendLine("AND T1.PR_TAXA_IMPOSTO IS NOT NULL ");
            sql.AppendFormat("AND T1.CD_GRUPO_IMPOSTO = '{0}'", CD_GRUPO_IMPOSTOS);

            switch (CD_GRUPO_IMPOSTOS)
            {
                case 10:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_IMPOSTO FROM TMPVARIAVEIS_BNG) /* (MWSKZ) */ ");
                    }
                    break;
                case 15:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_CLIENTE    FROM TMPVARIAVEIS_BNG) /* (KUNNR) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE2 = (SELECT PRODUTO_BUNGE FROM TMPVARIAVEIS_BNG) /* (MATNR) */ ");
                    }
                    break;
                case 16:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_CLIENTE              FROM TMPVARIAVEIS_BNG) /* (KUNNR) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE2 = (SELECT CD_CLASSIFICACAO_FISCAL FROM TMPVARIAVEIS_BNG) /* (NBM) */ ");
                    }
                    break;
                case 11:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_FILIAL           FROM TMPVARIAVEIS_BNG) /* (WERKS) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE2 = (SELECT CD_SETOR_INDUSTRIAL FROM TMPVARIAVEIS_BNG) /* (BRSCH) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE3 = (SELECT PRODUTO_BUNGE       FROM TMPVARIAVEIS_BNG) /* (MATNR) */ ");
                    }
                    break;
                case 12:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_FILIAL     FROM TMPVARIAVEIS_BNG) /* (WERKS) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE2 = (SELECT PRODUTO_BUNGE FROM TMPVARIAVEIS_BNG) /* (MATNR) */ ");
                    }
                    break;
                case 20:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_SETOR_INDUSTRIAL FROM TMPVARIAVEIS_BNG) /* (BRSCH) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE2 = (SELECT PRODUTO_BUNGE       FROM TMPVARIAVEIS_BNG) /* (MATNR) */ ");
                    }
                    break;
                case 25:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_SETOR_INDUSTRIAL FROM TMPVARIAVEIS_BNG) /* (BRSCH) */ ");
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE2 = (SELECT CD_NEGOCIO          FROM TMPVARIAVEIS_BNG) /* (MATKL) */ ");
                    }
                    break;
                case 60:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_CLIENTE FROM TMPVARIAVEIS_BNG) /* (KUNNR) */ ");
                    }
                    break;
                case 65:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT PRODUTO_BUNGE FROM TMPVARIAVEIS_BNG) /* (MATNR) */ ");
                    }
                    break;
                case 70:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_NEGOCIO FROM TMPVARIAVEIS_BNG) /* (MATKL) */ ");
                    }
                    break;
                case 85:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_CLASSIFICACAO_FISCAL FROM TMPVARIAVEIS_BNG) /* (NBM) */ ");
                    }
                    break;
                case 27:
                    {
                        sql.AppendLine("AND T1.CD_CAMPO_CHAVE1 = (SELECT CD_SETOR_INDUSTRIAL FROM TMPVARIAVEIS_BNG) /* (BRSCH) */ ");
                    }
                    break;
            }

            sql.AppendLine("ORDER BY T2.SEQUENCIA, T1.DT_INICIO_VIGENCIA DESC");

            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
            {
                while (sqlReader.Read())
                {
                    if (sqlReader.GetValue(0) != System.DBNull.Value)
                    {
                        PR_TAXA_PIS = Convert.ToDecimal(sqlReader.GetValue(0));
                        PR_BASE_PIS = sqlReader.GetValue(1) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(1));
                    }
                }
            }
        }

        private void ValidaPrecoItem()
        {
            if (PRECO_UNITARIO == 0m)
                throw new Exception("Preço do Item não encontrado na pricing.");
        }

        private void PrecoDoItem()
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("SELECT (CASE  WHEN T1.VL_PRECO_UNITARIO_FOB = 0 ");
            sql.AppendLine("                              THEN T1.VL_PRECO_UNITARIO_CIF ");
            sql.AppendLine("              ELSE T1.VL_PRECO_UNITARIO_FOB ");
            sql.AppendLine("END) AS PRECO ");
            sql.AppendLine("FROM BNG_SGT_PRECO_ITEM_TAB_AGV_SAP T1 ");
            sql.AppendLine("INNER JOIN TMPVARIAVEIS_BNG T2 ");
            sql.AppendLine("ON (T1.NR_NOTEBOOK = T2.NR_NOTEBOOK OR T1.NR_NOTEBOOK = 9999) ");
            sql.AppendFormat("WHERE CAST(T1.CD_ITEM AS INT) = {0} ", PRODUTO_BUNGE);
            sql.AppendLine("AND (T1.CD_BASE_CLIENTE = T2.CD_BASE_CLIENTE OR T1.CD_BASE_CLIENTE = '999999999') ");
            sql.AppendLine("AND (T1.CD_LOJA_CLIENTE = T2.CD_LOJA_CLIENTE OR T1.CD_LOJA_CLIENTE = '9999') ");
            sql.AppendLine("AND (T1.CD_GRUPO_SEGMENTO = T2.CD_SEGMENTO_CLIENTE OR T1.CD_GRUPO_SEGMENTO = '99') ");
            sql.AppendLine("AND (T1.CD_FILIAL = T2.CD_FILIAL OR T1.CD_FILIAL = '9999') ");
            sql.AppendLine("/* AND (T1.CD_MICRORREGIAO_CML  = <CD_MICRORREGIAO_CML> OR T1.CD_MICRORREGIAO_CML = '99') */");
            sql.AppendLine("AND (T1.CD_UNIDADE_FEDERACAO = T2.CD_UNIDADE_FEDERACAO OR T1.CD_UNIDADE_FEDERACAO = '999') ");
            sql.AppendLine("AND (T1.CD_REGIONAL_VENDA = T2.CD_REGIONAL_VENDA OR T1.CD_REGIONAL_VENDA = '9999999999') ");
            sql.AppendFormat("AND DATE('now') BETWEEN DATE(T1.DT_INICIO_VIGENCIA) AND IFNULL(T1.DT_FIM_VIGENCIA,DATE('now')) ");
            sql.AppendLine("ORDER BY T1.CD_ITEM ");
            sql.AppendLine(", T1.CD_BASE_CLIENTE ");
            sql.AppendLine(", T1.CD_LOJA_CLIENTE ");
            sql.AppendLine(", T1.CD_GRUPO_SEGMENTO DESC ");
            sql.AppendLine(", T1.CD_FILIAL ");
            sql.AppendLine(", T1.CD_MICRORREGIAO_CML ");
            sql.AppendLine(", T1.CD_UNIDADE_FEDERACAO DESC ");
            sql.AppendLine(", T1.CD_REGIONAL_VENDA  DESC ");
            sql.AppendLine(", DT_INICIO_VIGENCIA DESC ");
            sql.AppendLine(" LIMIT 1");

            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
            {
                while (sqlReader.Read())
                {
                    PRECO_UNITARIO = sqlReader.GetValue(0) == System.DBNull.Value ? 0m : Convert.ToDecimal(sqlReader.GetValue(0));
                }
            }
        }

        private void ValidarCliente()
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("SELECT 1 FROM TMPVARIAVEIS_BNG");

            var resultado = CSDataAccess.Instance.ExecuteReader(sql.ToString());

            if (!resultado.HasRows)
                throw new Exception("Cliente não existente na pricing.");
        }

        private void InserirTabelaTemporaria()
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("INSERT INTO TMPVARIAVEIS_BNG ");
            sql.AppendLine("SELECT ");
            sql.AppendLine("  T1.NR_NOTEBOOK ");
            sql.AppendLine(", T1.CD_BASE_CLIENTE ");
            sql.AppendLine(", T1.CD_LOJA_CLIENTE ");
            sql.AppendLine(", T1.CD_CLIENTE ");
            sql.AppendLine(", T1.CD_UNIDADE_FEDERACAO ");
            sql.AppendLine(", T1.CD_SEGMENTO_CLIENTE ");
            sql.AppendLine(", T1.CD_SETOR_INDUSTRIAL ");
            sql.AppendLine(", T2.CD_ORG_VENDAS ");
            sql.AppendLine(", T2.CD_CAN_DISTRIBUICAO ");
            sql.AppendLine(", T2.CD_SET_ATIVIDADES ");
            sql.AppendLine(", T2.CD_REGIONAL_VENDA ");
            sql.AppendLine(", '2306' AS CD_FILIAL ");
            sql.AppendLine(",'5001005' AS CD_GERENCIA_COMERCIAL ");
            sql.AppendLine(", 'CIF' AS CD_TIPO_FRETE ");
            sql.AppendFormat(",'{0}' AS PRODUTO_BUNGE ", PRODUTO_BUNGE);
            sql.AppendFormat(", '{0}' AS CD_NEGOCIO ", CD_NEGOCIO);
            sql.AppendFormat(", '{0}' AS CD_CLASSIFICACAO_FISCAL ", CD_CLASSIFICACAO_FISCAL);
            sql.AppendLine(", '' AS CD_IMPOSTO ");
            sql.AppendLine(", '' AS CD_MICRORREGIAO ");
            sql.AppendLine("FROM BNG_LOJA_CLIENTE_AGV_SAP T1 ");
            sql.AppendLine("INNER JOIN BNG_ZONA_VDA_LJA_CLI_AGV_SAP T2 ");
            sql.AppendLine("ON T1.CD_BASE_CLIENTE = T2.CD_BASE_CLIENTE  AND ");
            sql.AppendLine("T1.CD_LOJA_CLIENTE = T2.CD_LOJA_CLIENTE  AND ");
            sql.AppendLine("(T1.NR_NOTEBOOK = T2.NR_NOTEBOOK OR T1.NR_NOTEBOOK = 9999) ");
            sql.AppendLine("INNER JOIN  (SELECT SUBSTR(T1.NUM_CGC,1,8) AS CD_BASE_CLIENTE ");
            sql.AppendLine(", SUBSTR(T1.NUM_CGC,9,4) AS CD_LOJA_CLIENTE ");
            sql.AppendLine("FROM PDV T1 ");
            //sql.AppendLine("WHERE ISNUMERIC(T1.CODCGCCPFCET) = 1 ");
            sql.AppendLine("WHERE LENGTH(RTRIM(LTRIM(T1.NUM_CGC))) > 11 ");
            sql.AppendFormat("AND T1.COD_PDV = '{0}') AS TMP_CLIENTE_FLEXX ", CLIENTE);
            sql.AppendLine("ON T1.CD_BASE_CLIENTE = TMP_CLIENTE_FLEXX.CD_BASE_CLIENTE AND ");
            sql.AppendLine("T1.CD_LOJA_CLIENTE = TMP_CLIENTE_FLEXX.CD_LOJA_CLIENTE ");
            sql.AppendFormat("WHERE T1.NR_NOTEBOOK = '{0}' ", NR_NOTEBOOK);
            sql.AppendLine("LIMIT 1");

            try
            {
                CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void DadosProduto(int produto, int nr_notebook)
        {
            StringBuilder sql = new StringBuilder();
            SqliteParameter pPRODUTO = new SQLiteParameter("@PRODUTO", produto);
            SQLiteParameter pNR_NOTEBOOK = new SQLiteParameter("@NR_NOTEBOOK", nr_notebook);

            sql.AppendLine("SELECT T2.CD_ITEM ");
            sql.AppendLine(", T2.CD_NEGOCIO ");
            sql.AppendLine(", T2.CD_CLASSIFICACAO_FISCAL ");
            sql.AppendLine("FROM PRODUTO T1 ");
            sql.AppendLine("INNER JOIN BNG_ITEM_AGV_SAP T2 ");
            sql.AppendLine("ON CAST(T1.DESCRICAO_APELIDO_PRODUTO AS INT) = T2.CD_ITEM ");
            sql.AppendLine("WHERE T1.COD_PRODUTO = ? ");
            sql.AppendLine("AND T2.NR_NOTEBOOK = ?");

            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString(), pPRODUTO, pNR_NOTEBOOK))
            {
                while (sqlReader.Read())
                {
                    PRODUTO_BUNGE = Convert.ToInt32(sqlReader.GetString(0));
                    CD_NEGOCIO = sqlReader.GetString(1);
                    CD_CLASSIFICACAO_FISCAL = sqlReader.GetString(2);
                }
            }
        }
    }
}
