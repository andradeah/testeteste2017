#region Using directives

using System;
using System.Collections;
using System.Data;
using System.Data.SqlTypes;
using System.Reflection;
using System.IO;
using System.Text;

using AvanteSales.BusinessRules;
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

namespace AvanteSales
{
    public class CSColetaEstoquePDV : CollectionBase, IDisposable
    {

        #region [ Variaveis ]

        private CSColetaEstoquePDV.CSColetaEstoqueProduto m_Current;
        private bool m_existeColunaQTD_PERDA;
        private bool m_existeColunaQTD_GIRO_SELLOUT;
        private bool m_existeColunaSOM_UTM_QTD_GIRO_SELLOUT;

        #endregion

        #region [ Propriedades ]

        public CSColetaEstoquePDV.CSColetaEstoqueProduto Current
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

        public CSColetaEstoquePDV(int COD_PDV)
        {
            try
            {
                // Adiciona produto da categoria para a coleta de estoque
                //AdicionaProdutoCategoria(COD_PDV);

                StringBuilder sqlQuery = new StringBuilder();

                sqlQuery.AppendLine("SELECT DAT_COLETA, COD_EMPREGADO, COD_PDV, COD_PRODUTO, QTD_COLETADA");

                if (CSEmpresa.Current.UtilizaTabelaNova &&
                    CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                {
                    sqlQuery.AppendLine(", QTD_PERDA");
                    sqlQuery.AppendLine(", QTD_GIRO_SELLOUT");
                    sqlQuery.AppendLine(", SOM_UTM_QTD_GIRO_SELLOUT ");
                    sqlQuery.AppendLine(", QTD_COLETA_SELLOUT ");
                    sqlQuery.AppendLine(", NUM_COLETA_ESTOQUE ");
                }
                sqlQuery.AppendLine("  FROM PRODUTO_COLETA_ESTOQUE ");
                sqlQuery.AppendLine(" WHERE COD_PDV = ? ");
                sqlQuery.AppendLine("   AND COD_EMPREGADO = ? ");
                //sqlQuery.AppendLine("   AND IND_HISTORICO = 0 ");
                sqlQuery.AppendLine("   AND date(DAT_COLETA) = date('now') ");

                SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", COD_PDV);
                SQLiteParameter pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);

                // Busca todos os produtos coletados para o PDV
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString(), pCOD_PDV, pCOD_EMPREGADO))
                {
                    while (sqlReader.Read())
                    {
                        CSColetaEstoquePDV.CSColetaEstoqueProduto produtoColetado = new CSColetaEstoquePDV.CSColetaEstoqueProduto();

                        // Preenche a instancia da classe coleta de estoque
                        produtoColetado.DAT_COLETA = sqlReader.GetValue(0) == System.DBNull.Value ? new DateTime(1900, 1, 1, 1, 1, 1) : sqlReader.GetDateTime(0);
                        produtoColetado.COD_EMPREGADO = sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1);
                        produtoColetado.COD_PDV = sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2);
                        produtoColetado.PRODUTO = CSProdutos.GetProduto(sqlReader.GetValue(3) == System.DBNull.Value ? -1 : sqlReader.GetInt32(3));
                        produtoColetado.QTD_COLETADA_TOTAL = sqlReader.GetValue(4) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(4));

                        if (CSEmpresa.Current.UtilizaTabelaNova &&
                            CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                        {
                            produtoColetado.QTD_PERDA_TOTAL = sqlReader.GetValue(5) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(5));
                            produtoColetado.QTD_COLETA_SELLOUT = (sqlReader.GetValue(8) == System.DBNull.Value ? 0 : sqlReader.GetInt32(8));
                            produtoColetado.NUM_COLETA_ESTOQUE = (sqlReader.GetValue(9) == System.DBNull.Value ? 0 : sqlReader.GetInt32(9));
                            produtoColetado.PRODUTO.QTD_GIRO_SELLOUT = sqlReader.GetValue(6) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(6));
                            produtoColetado.SOM_UTM_QTD_GIRO_SELLOUT = (sqlReader.GetValue(7) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(7)));
                            //produtoColetado.PRODUTO.IND_UTILIZA_QTD_MINIMA = UtilizaQtdMinima(produtoColetado);
                            produtoColetado.PRODUTO.QTD_PRODUTO_SUGERIDO = CalculaQuantidadeSugerida(produtoColetado);
                        }
                        produtoColetado.STATE = ObjectState.INALTERADO;

                        // Adciona o produtos coletao do PDV na coleção de Coleta de estoque
                        base.InnerList.Add(produtoColetado);
                    }
                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos produtos coletado do PDV", ex);
            }
        }

        public CSColetaEstoquePDV(int COD_PDV, int COD_PRODUTO)
        {
            try
            {
                // Adiciona produto da categoria para a coleta de estoque
                //AdicionaProdutoCategoria(COD_PDV);

                StringBuilder sqlQuery = new StringBuilder();

                sqlQuery.AppendLine("SELECT DAT_COLETA, COD_EMPREGADO, COD_PDV, COD_PRODUTO, QTD_COLETADA");

                if (CSEmpresa.Current.UtilizaTabelaNova &&
                    CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                {
                    sqlQuery.AppendLine(", QTD_PERDA");
                    sqlQuery.AppendLine(", QTD_GIRO_SELLOUT");
                    sqlQuery.AppendLine(", SOM_UTM_QTD_GIRO_SELLOUT ");
                    sqlQuery.AppendLine(", QTD_COLETA_SELLOUT ");
                    sqlQuery.AppendLine(", NUM_COLETA_ESTOQUE ");
                }
                sqlQuery.AppendLine("  FROM PRODUTO_COLETA_ESTOQUE ");
                sqlQuery.AppendLine(" WHERE COD_PDV = ? ");
                sqlQuery.AppendLine("   AND COD_EMPREGADO = ? ");
                //sqlQuery.AppendLine("   AND IND_HISTORICO = 0 ");
                sqlQuery.AppendLine("   AND date(DAT_COLETA) = date('now') ");
                sqlQuery.AppendLine("   AND COD_PRODUTO = ? ");

                SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", COD_PDV);
                SQLiteParameter pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                SQLiteParameter pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", COD_PRODUTO);

                // Busca todos os produtos coletados para o PDV
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString(), pCOD_PDV, pCOD_EMPREGADO, pCOD_PRODUTO))
                {
                    while (sqlReader.Read())
                    {
                        CSColetaEstoquePDV.CSColetaEstoqueProduto produtoColetado = new CSColetaEstoquePDV.CSColetaEstoqueProduto();

                        // Preenche a instancia da classe coleta de estoque
                        produtoColetado.DAT_COLETA = sqlReader.GetValue(0) == System.DBNull.Value ? new DateTime(1900, 1, 1) : sqlReader.GetDateTime(0);
                        produtoColetado.COD_EMPREGADO = sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1);
                        produtoColetado.COD_PDV = sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2);
                        produtoColetado.PRODUTO = CSProdutos.GetProduto(sqlReader.GetValue(3) == System.DBNull.Value ? -1 : sqlReader.GetInt32(3));
                        produtoColetado.QTD_COLETADA_TOTAL = sqlReader.GetValue(4) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(4));

                        if (CSEmpresa.Current.UtilizaTabelaNova &&
                            CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                        {
                            produtoColetado.QTD_PERDA_TOTAL = sqlReader.GetValue(5) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(5));
                            produtoColetado.QTD_COLETA_SELLOUT = (sqlReader.GetValue(8) == System.DBNull.Value ? 0 : sqlReader.GetInt32(8));
                            produtoColetado.NUM_COLETA_ESTOQUE = (sqlReader.GetValue(9) == System.DBNull.Value ? 0 : sqlReader.GetInt32(9));
                            produtoColetado.PRODUTO.QTD_GIRO_SELLOUT = sqlReader.GetValue(6) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(6));
                            produtoColetado.SOM_UTM_QTD_GIRO_SELLOUT = (sqlReader.GetValue(7) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(7)));
                            //produtoColetado.PRODUTO.IND_UTILIZA_QTD_MINIMA = UtilizaQtdMinima(produtoColetado);
                            produtoColetado.PRODUTO.QTD_PRODUTO_SUGERIDO = CalculaQuantidadeSugerida(produtoColetado);
                        }
                        produtoColetado.STATE = ObjectState.INALTERADO;

                        // Adciona o produtos coletao do PDV na coleção de Coleta de estoque
                        base.InnerList.Add(produtoColetado);
                    }
                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos produtos coletado do PDV", ex);
            }
        }

        public static CSColetaEstoquePDV.CSColetaEstoqueProduto PEGAR_DADOS_COLETA_PRODUTO(int COD_PRODUTO)
        {
            try
            {
                StringBuilder sqlQuery = new StringBuilder();

                sqlQuery.AppendLine("SELECT DAT_COLETA, COD_EMPREGADO, COD_PDV, COD_PRODUTO, QTD_COLETADA");

                if (CSEmpresa.Current.UtilizaTabelaNova &&
                    CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                {
                    sqlQuery.AppendLine(", QTD_PERDA");
                    sqlQuery.AppendLine(", QTD_GIRO_SELLOUT");
                    sqlQuery.AppendLine(", SOM_UTM_QTD_GIRO_SELLOUT ");
                    sqlQuery.AppendLine(", QTD_COLETA_SELLOUT ");
                    sqlQuery.AppendLine(", NUM_COLETA_ESTOQUE ");
                }
                sqlQuery.AppendLine("  FROM PRODUTO_COLETA_ESTOQUE ");
                sqlQuery.AppendLine("  WHERE COD_PDV = ? ");
                sqlQuery.AppendLine("   AND COD_EMPREGADO = ? ");
                sqlQuery.AppendLine("   AND date(DAT_COLETA) = date('now') ");
                sqlQuery.AppendLine("   AND COD_PRODUTO = ? ");

                SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", CSPDVs.Current.COD_PDV);
                SQLiteParameter pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                SQLiteParameter pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", COD_PRODUTO);

                // Busca todos os produtos coletados para o PDV
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString(), pCOD_PDV, pCOD_EMPREGADO, pCOD_PRODUTO))
                {
                    if (sqlReader.Read())
                    {
                        CSColetaEstoquePDV.CSColetaEstoqueProduto produtoColetado = new CSColetaEstoquePDV.CSColetaEstoqueProduto();

                        // Preenche a instancia da classe coleta de estoque
                        produtoColetado.DAT_COLETA = sqlReader.GetValue(0) == System.DBNull.Value ? new DateTime(1900, 1, 1) : sqlReader.GetDateTime(0);
                        produtoColetado.COD_EMPREGADO = sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1);
                        produtoColetado.COD_PDV = sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2);
                        produtoColetado.PRODUTO = CSProdutos.GetProduto(sqlReader.GetValue(3) == System.DBNull.Value ? -1 : sqlReader.GetInt32(3));
                        produtoColetado.QTD_COLETADA_TOTAL = sqlReader.GetValue(4) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(4));

                        if (CSEmpresa.Current.UtilizaTabelaNova &&
                            CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                        {
                            produtoColetado.QTD_PERDA_TOTAL = sqlReader.GetValue(5) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(5));
                            produtoColetado.QTD_COLETA_SELLOUT = (sqlReader.GetValue(8) == System.DBNull.Value ? 0 : sqlReader.GetInt32(8));
                            produtoColetado.NUM_COLETA_ESTOQUE = (sqlReader.GetValue(9) == System.DBNull.Value ? 0 : sqlReader.GetInt32(9));
                            produtoColetado.PRODUTO.QTD_GIRO_SELLOUT = sqlReader.GetValue(6) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(6));
                            produtoColetado.SOM_UTM_QTD_GIRO_SELLOUT = (sqlReader.GetValue(7) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(7)));
                            produtoColetado.PRODUTO.QTD_PRODUTO_SUGERIDO = CalculaQuantidadeSugerida(produtoColetado);
                        }
                        produtoColetado.STATE = ObjectState.INALTERADO;

                        // Adciona o produtos coletao do PDV na coleção de Coleta de estoque

                        // Fecha o reader
                        sqlReader.Close();
                        sqlReader.Dispose();

                        return produtoColetado;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos dados de coletado do produto", ex);
            }
        }

        private int SelecionarNumeroDeColetasSellOut(int Produto)
        {
            int Qtd_Coletas = 0;

            StringBuilder sqlQuery = new StringBuilder();

            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT QTD_COLETA_SELLOUT FROM [PRODUTO_COLETA_ESTOQUE] WHERE IND_HISTORICO = 1 AND COD_PDV = '" + CSPDVs.Current.COD_PDV + "' AND COD_PRODUTO = '" + Produto + "' ORDER BY DAT_COLETA DESC LIMIT 1");

            using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString()))
            {
                while (reader.Read())
                {
                    Qtd_Coletas = (reader.GetValue(0) == System.DBNull.Value) ? 0 : reader.GetInt32(0);
                    break;
                }

                reader.Close();
                reader.Dispose();
            }

            return Qtd_Coletas;
        }
        public static bool UtilizaQtdMinima(CSColetaEstoquePDV.CSColetaEstoqueProduto produtoColetado)
        {
            int ParametroEmpresa = 0;

            if (produtoColetado.PRODUTO.QTD_GIRO_SELLOUT > 0)
                ParametroEmpresa = produtoColetado.QTD_COLETA_SELLOUT + 1;
            else
                ParametroEmpresa = produtoColetado.QTD_COLETA_SELLOUT;

            if (ParametroEmpresa > CSEmpresa.Current.IND_QTD_COLETAS_REALIZADAS)
                ParametroEmpresa = CSEmpresa.Current.IND_QTD_COLETAS_REALIZADAS;

            decimal giroMedio = 0;

            giroMedio = CalculaGiroMedio(produtoColetado);

            var intervaloDiasPlanejado = 0;
            switch (CSPDVs.Current.DSC_CICLO_VISITA.Trim().Length)
            {
                case 1:
                    intervaloDiasPlanejado = 30;
                    break;
                case 2:
                    intervaloDiasPlanejado = 15;
                    break;
                case 4:
                    intervaloDiasPlanejado = 7;
                    break;
            }

            decimal PCT_SEGURANCA = 0m;

            if (produtoColetado.QTD_COLETADA_TOTAL != 0)
                PCT_SEGURANCA = CSEmpresa.Current.PCT_ESTOQUE_SEGURANCA;
            else
                PCT_SEGURANCA = ((CSEmpresa.Current.PCT_ESTOQUE_SEGURANCA * CSEmpresa.Current.PCT_SEGURANCA_PRODUTO_ESTOQUE) / 100) + CSEmpresa.Current.PCT_ESTOQUE_SEGURANCA;

            var percentualDesconto = (PCT_SEGURANCA / 100) * intervaloDiasPlanejado;
            var m_QTD_PRODUTO_SUGERIDO = Convert.ToInt32((giroMedio * (intervaloDiasPlanejado + percentualDesconto)) - produtoColetado.QTD_COLETADA_TOTAL);

            int codCategoriaQuantidadeMinima = -1;
            if (CSEmpresa.Current.IND_CLUSTER_PRODUTOS_PELO_TIPO_FREEZER)
                codCategoriaQuantidadeMinima = CSPDVs.Current.COD_DENVER;
            else
                codCategoriaQuantidadeMinima = CSPDVs.Current.COD_CATEGORIA;

            if (produtoColetado.PRODUTO.DESCRICAO_APELIDO_PRODUTO == "147")
                percentualDesconto = 0;

            if (CSPDVs.Current.IND_PRIMEIRA_VEZ_PEDIDO_SUGERIDO ||
                produtoColetado.PRIMEIRA_COLETA_PRODUTO ||
                m_QTD_PRODUTO_SUGERIDO < 0)
                m_QTD_PRODUTO_SUGERIDO = 0;

            if (!CSEmpresa.Current.IND_ASSUME_QTD_SUGERIDA_SOBRE_MINIMA ||
                m_QTD_PRODUTO_SUGERIDO == 0)
            {
                if (produtoColetado.PRODUTO.HT_QTD_MINIMA != null)
                {
                    if (m_QTD_PRODUTO_SUGERIDO <= Convert.ToInt32(produtoColetado.PRODUTO.HT_QTD_MINIMA[codCategoriaQuantidadeMinima]) ||
                        giroMedio == 0)
                    {
                        m_QTD_PRODUTO_SUGERIDO = Convert.ToInt32(produtoColetado.PRODUTO.HT_QTD_MINIMA[codCategoriaQuantidadeMinima]);

                        if (m_QTD_PRODUTO_SUGERIDO > 0)
                            return true;
                        else
                            return false;
                    }
                }
            }

            return false;
        }

        public static int CalculaQuantidadeSugerida(CSColetaEstoquePDV.CSColetaEstoqueProduto produtoColetado)
        {
            //decimal qtdHistorico = BuscaQuantidadeHistoricoProduto(produtoColetado);

            int ParametroEmpresa = 0;

            if (produtoColetado.PRODUTO.QTD_GIRO_SELLOUT > 0)
                ParametroEmpresa = produtoColetado.QTD_COLETA_SELLOUT + 1;
            else
                ParametroEmpresa = produtoColetado.QTD_COLETA_SELLOUT;

            if (ParametroEmpresa > CSEmpresa.Current.IND_QTD_COLETAS_REALIZADAS)
                ParametroEmpresa = CSEmpresa.Current.IND_QTD_COLETAS_REALIZADAS;

            decimal giroMedio = 0;

            giroMedio = CalculaGiroMedio(produtoColetado);

            var intervaloDiasPlanejado = 0;
            switch (CSPDVs.Current.DSC_CICLO_VISITA.Trim().Length)
            {
                case 1:
                    intervaloDiasPlanejado = 30;
                    break;
                case 2:
                    intervaloDiasPlanejado = 15;
                    break;
                case 4:
                    intervaloDiasPlanejado = 7;
                    break;
            }

            decimal PCT_SEGURANCA = 0m;

            if (produtoColetado.QTD_COLETADA_TOTAL != 0)
                PCT_SEGURANCA = CSEmpresa.Current.PCT_ESTOQUE_SEGURANCA;
            else
                PCT_SEGURANCA = ((CSEmpresa.Current.PCT_ESTOQUE_SEGURANCA * CSEmpresa.Current.PCT_SEGURANCA_PRODUTO_ESTOQUE) / 100) + CSEmpresa.Current.PCT_ESTOQUE_SEGURANCA;

            var percentualDesconto = (PCT_SEGURANCA / 100) * intervaloDiasPlanejado;
            var m_QTD_PRODUTO_SUGERIDO = Convert.ToInt32((giroMedio * (intervaloDiasPlanejado + percentualDesconto)) - produtoColetado.QTD_COLETADA_TOTAL);

            if (CSPDVs.Current.IND_PRIMEIRA_VEZ_PEDIDO_SUGERIDO ||
                produtoColetado.PRIMEIRA_COLETA_PRODUTO ||
                m_QTD_PRODUTO_SUGERIDO < 0)
                m_QTD_PRODUTO_SUGERIDO = 0;

            int codCategoriaQuantidadeMinima = -1;
            if (CSEmpresa.Current.IND_CLUSTER_PRODUTOS_PELO_TIPO_FREEZER)
                codCategoriaQuantidadeMinima = CSPDVs.Current.COD_DENVER;
            else
                codCategoriaQuantidadeMinima = CSPDVs.Current.COD_CATEGORIA;

            if (!CSEmpresa.Current.IND_ASSUME_QTD_SUGERIDA_SOBRE_MINIMA ||
                m_QTD_PRODUTO_SUGERIDO == 0)
            {
                if (produtoColetado.PRODUTO.HT_QTD_MINIMA != null)
                {
                    if (m_QTD_PRODUTO_SUGERIDO <= Convert.ToInt32(produtoColetado.PRODUTO.HT_QTD_MINIMA[codCategoriaQuantidadeMinima]))
                    {
                        m_QTD_PRODUTO_SUGERIDO = Convert.ToInt32(produtoColetado.PRODUTO.HT_QTD_MINIMA[codCategoriaQuantidadeMinima]);
                    }
                }
            }

            produtoColetado.PRODUTO.IND_UTILIZA_QTD_MINIMA = UtilizaQtdMinima(produtoColetado);

            return m_QTD_PRODUTO_SUGERIDO;
        }

        public static string[] CalculaQuantidadeSugeridaDetalhada(CSColetaEstoquePDV.CSColetaEstoqueProduto produto)
        {
            string[] Variaveis = new string[8];

            int ParametroEmpresa = 0;

            if (produto.PRODUTO.QTD_GIRO_SELLOUT > 0)
                ParametroEmpresa = produto.QTD_COLETA_SELLOUT + 1;
            else
                ParametroEmpresa = produto.QTD_COLETA_SELLOUT;

            if (ParametroEmpresa > CSEmpresa.Current.IND_QTD_COLETAS_REALIZADAS)
                ParametroEmpresa = CSEmpresa.Current.IND_QTD_COLETAS_REALIZADAS;

            decimal giroMedio = 0;

            giroMedio = CalculaGiroMedio(produto);

            Variaveis[0] = ParametroEmpresa.ToString();
            Variaveis[1] = (produto.SOM_UTM_QTD_GIRO_SELLOUT + produto.PRODUTO.QTD_GIRO_SELLOUT).ToString(CSGlobal.DecimalStringFormat);
            //Variáveis[2] = produto.PRODUTO.QTD_GIRO_SELLOUT.ToString(CSGlobal.DecimalStringFormat);
            Variaveis[2] = giroMedio.ToString(CSGlobal.DecimalStringFormat);

            var intervaloDiasPlanejado = 0;
            switch (CSPDVs.Current.DSC_CICLO_VISITA.Trim().Length)
            {
                case 1:
                    intervaloDiasPlanejado = 30;
                    break;
                case 2:
                    intervaloDiasPlanejado = 15;
                    break;
                case 4:
                    intervaloDiasPlanejado = 7;
                    break;
                //(próxima visita - atual + 1)
            }

            decimal PCT_SEGURANCA = 0m;

            if (produto.QTD_COLETADA_TOTAL != 0)
                PCT_SEGURANCA = CSEmpresa.Current.PCT_ESTOQUE_SEGURANCA;
            else
                PCT_SEGURANCA = ((CSEmpresa.Current.PCT_ESTOQUE_SEGURANCA * CSEmpresa.Current.PCT_SEGURANCA_PRODUTO_ESTOQUE) / 100) + CSEmpresa.Current.PCT_ESTOQUE_SEGURANCA;

            var percentualDesconto = (PCT_SEGURANCA / 100) * intervaloDiasPlanejado;

            Variaveis[3] = intervaloDiasPlanejado.ToString(CSGlobal.DecimalStringFormat);
            Variaveis[4] = percentualDesconto.ToString(CSGlobal.DecimalStringFormat);
            Variaveis[5] = produto.QTD_COLETADA_TOTAL.ToString();
            Variaveis[6] = "(" + giroMedio.ToString(CSGlobal.DecimalStringFormat) + "*(" + intervaloDiasPlanejado.ToString(CSGlobal.DecimalStringFormat) + " + " + percentualDesconto.ToString(CSGlobal.DecimalStringFormat) + ")) - " + produto.QTD_COLETADA_TOTAL.ToString();

            int Resultado;

            Resultado = Convert.ToInt32((giroMedio * (intervaloDiasPlanejado + percentualDesconto)) - produto.QTD_COLETADA_TOTAL);

            if (Resultado <= 0)
            {
                Resultado = 0;
            }

            Variaveis[7] = Resultado.ToString();

            return Variaveis;
        }

        public int Add(CSColetaEstoquePDV.CSColetaEstoqueProduto produtoColetado)
        {
            if (CSEmpresa.Current.UtilizaTabelaNova &&
                CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2 &&
                CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
            {
                CalculaGiroSellout(produtoColetado);
                CalculaQuantidadeSugerida(produtoColetado);
            }

            // Adiciona na coleção
            int idx = base.InnerList.Add(produtoColetado);
            // Retorna a posição dele na coleção
            return idx;
        }

        public void CalculaGiroSellout(CSColetaEstoquePDV.CSColetaEstoqueProduto produtoColetado)
        {
            var estoqueAnterior = BuscaQuantidadeHistoricoProduto(produtoColetado);

            decimal[] Giro;

            Giro = produtoColetado.PRODUTO.GetRetornaGiroProduto(produtoColetado.PRODUTO.COD_PRODUTO);

            if (Giro[4] != 0)
                produtoColetado.PRODUTO.QTD_GIRO_SELLOUT = ((estoqueAnterior + Giro[2]) - produtoColetado.QTD_COLETADA_TOTAL - produtoColetado.QTD_PERDA_TOTAL) / Giro[4];
            else
                produtoColetado.PRODUTO.QTD_GIRO_SELLOUT = 0;

            if (produtoColetado.PRODUTO.QTD_GIRO_SELLOUT < 0 || CSPDVs.Current.IND_PRIMEIRA_VEZ_PEDIDO_SUGERIDO || produtoColetado.PRIMEIRA_COLETA_PRODUTO)
                produtoColetado.PRODUTO.QTD_GIRO_SELLOUT = 0;

            produtoColetado.PRODUTO.QTD_GIRO_MEDIO = CalculaGiroMedio(produtoColetado);
        }

        public static decimal CalculaGiroMedio(CSColetaEstoquePDV.CSColetaEstoqueProduto produtoColetado)
        {
            int ParametroEmpresa = 0;
            var giroMedio = 0m;

            if (produtoColetado.PRODUTO.QTD_GIRO_SELLOUT > 0)
                ParametroEmpresa = produtoColetado.QTD_COLETA_SELLOUT + 1;
            else
                ParametroEmpresa = produtoColetado.QTD_COLETA_SELLOUT;

            if (ParametroEmpresa > CSEmpresa.Current.IND_QTD_COLETAS_REALIZADAS)
                ParametroEmpresa = CSEmpresa.Current.IND_QTD_COLETAS_REALIZADAS;

            if (!CSPDVs.Current.IND_PRIMEIRA_VEZ_PEDIDO_SUGERIDO && ParametroEmpresa > 0)
                giroMedio = ((produtoColetado.SOM_UTM_QTD_GIRO_SELLOUT + produtoColetado.PRODUTO.QTD_GIRO_SELLOUT) / ParametroEmpresa);

            if (giroMedio < 0 || CSPDVs.Current.IND_PRIMEIRA_VEZ_PEDIDO_SUGERIDO || produtoColetado.PRIMEIRA_COLETA_PRODUTO)
                giroMedio = 0;

            return giroMedio;
        }

        public static string[] CalculoGiroDetalhado(CSColetaEstoquePDV.CSColetaEstoqueProduto produto)
        {
            string[] Variaveis = new string[6];

            var estoqueAnterior = BuscaQuantidadeHistoricoProduto(produto);

            decimal[] Giro;

            Giro = produto.PRODUTO.GetRetornaGiroProduto(produto.PRODUTO.COD_PRODUTO);

            Variaveis[0] = estoqueAnterior.ToString();
            Variaveis[1] = produto.QTD_COLETADA_TOTAL.ToString();
            Variaveis[2] = produto.QTD_PERDA_TOTAL.ToString();
            Variaveis[3] = Giro[4].ToString();
            Variaveis[5] = Giro[2].ToString();

            if (Giro[4] != 0)
                Variaveis[4] = (((estoqueAnterior + Giro[2]) - produto.QTD_COLETADA_TOTAL - produto.QTD_PERDA_TOTAL) / Giro[4]).ToString(CSGlobal.DecimalStringFormat);
            else
                Variaveis[4] = "0";

            if (Convert.ToDecimal(Variaveis[4]) < 0)
                Variaveis[4] = "0";

            return Variaveis;
        }

        private static decimal BuscaQuantidadeHistoricoProduto(CSColetaEstoquePDV.CSColetaEstoqueProduto produtoColetado)
        {
            string sqlQuery = "SELECT QTD_COLETADA FROM PRODUTO_COLETA_ESTOQUE WHERE COD_EMPREGADO = ? AND COD_PDV = ? AND COD_PRODUTO = ? AND IND_HISTORICO = 1";
            SQLiteParameter pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
            SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", produtoColetado.COD_PDV);
            SQLiteParameter pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", produtoColetado.PRODUTO.COD_PRODUTO);


            // Busca todos os produtos da categoria para coletado do PDV
            object retorno = CSDataAccess.Instance.ExecuteScalar(sqlQuery, pCOD_EMPREGADO, pCOD_PDV, pCOD_PRODUTO);
            return Convert.ToDecimal(retorno);
        }

        public bool Flush()
        {
            StringBuilder sqlQueryInsert = new StringBuilder();
            sqlQueryInsert.AppendLine("INSERT INTO PRODUTO_COLETA_ESTOQUE ");
            sqlQueryInsert.AppendLine("(DAT_COLETA, COD_EMPREGADO, COD_PDV, COD_PRODUTO, QTD_COLETADA, IND_HISTORICO");

            if (EXISTE_CAMPO_PERDA_GIRO() ||
                CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
            {
                sqlQueryInsert.AppendLine(", QTD_PERDA");
                sqlQueryInsert.AppendLine(", QTD_GIRO_SELLOUT");
                sqlQueryInsert.AppendLine(", SOM_UTM_QTD_GIRO_SELLOUT");
            }

            if (EXISTE_CAMPO_PERDA_GIRO() ||
                CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                sqlQueryInsert.AppendFormat(") VALUES(?,?,?,?,?,0,?,?,?)");
            else
                sqlQueryInsert.AppendFormat(") VALUES(?,?,?,?,?,0)");

            StringBuilder sqlQueryUpdate = new StringBuilder();
            sqlQueryUpdate.AppendLine("UPDATE PRODUTO_COLETA_ESTOQUE SET DAT_COLETA=?,QTD_COLETADA=?");

            if (EXISTE_CAMPO_PERDA_GIRO() ||
                CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                sqlQueryUpdate.AppendLine(", QTD_PERDA=?, QTD_GIRO_SELLOUT=?, SOM_UTM_QTD_GIRO_SELLOUT=?");

            sqlQueryUpdate.AppendLine(" WHERE DATE(DAT_COLETA)=DATE(?) AND COD_EMPREGADO=? AND COD_PDV=? AND COD_PRODUTO=? ");

            string sqlQueryDelete =
                "DELETE FROM PRODUTO_COLETA_ESTOQUE WHERE DAT_COLETA=? AND COD_EMPREGADO=? AND COD_PDV=? AND COD_PRODUTO=? ";

            // Varre a coleção procurando os objetos a serem persistidos
            foreach (CSColetaEstoquePDV.CSColetaEstoqueProduto coletaEstoque in base.InnerList)
            {
                if (coletaEstoque.QTD_COLETADA_INTEIRA == -1 && coletaEstoque.QTD_PERDA_INTEIRA == -1)
                    continue;

                coletaEstoque.DAT_COLETA = DateTime.Now;

                // Criar os parametros de salvamento
                SQLiteParameter pDAT_COLETA = new SQLiteParameter("@DAT_COLETA", coletaEstoque.DAT_COLETA);
                SQLiteParameter pDAT_COLETADOIS = new SQLiteParameter("@DAT_COLETADOIS", coletaEstoque.DAT_COLETA);
                SQLiteParameter pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", coletaEstoque.COD_PDV);
                SQLiteParameter pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", coletaEstoque.PRODUTO.COD_PRODUTO);
                SQLiteParameter pQTD_COLETADA = new SQLiteParameter("@QTD_COLETADA", coletaEstoque.QTD_COLETADA_TOTAL);

                SQLiteParameter pQTD_PERDA = null;
                SQLiteParameter pQTD_GIRO_SELLOUT = null;
                SQLiteParameter pSOM_UTM_QTD_GIRO_SELLOUT = null;

                if (EXISTE_CAMPO_PERDA_GIRO() ||
                    CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                {
                    pQTD_PERDA = new SQLiteParameter("@QTD_PERDA", coletaEstoque.QTD_PERDA_TOTAL);
                    pQTD_GIRO_SELLOUT = new SQLiteParameter("@QTD_GIRO_SELLOUT", coletaEstoque.PRODUTO.QTD_GIRO_SELLOUT);
                    pSOM_UTM_QTD_GIRO_SELLOUT = new SQLiteParameter("@SOM_UTM_QTD_GIRO_SELLOUT", coletaEstoque.SOM_UTM_QTD_GIRO_SELLOUT);
                }

                switch (coletaEstoque.STATE)
                {
                    case ObjectState.NOVO:

                        // Executa a query salvando os dados
                        if (EXISTE_CAMPO_PERDA_GIRO() ||
                            CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                            CSDataAccess.Instance.ExecuteScalar(sqlQueryInsert.ToString(), pDAT_COLETA, pCOD_EMPREGADO, pCOD_PDV, pCOD_PRODUTO, pQTD_COLETADA, pQTD_PERDA, pQTD_GIRO_SELLOUT, pSOM_UTM_QTD_GIRO_SELLOUT);
                        else
                            CSDataAccess.Instance.ExecuteScalar(sqlQueryInsert.ToString(), pDAT_COLETA, pCOD_EMPREGADO, pCOD_PDV, pCOD_PRODUTO, pQTD_COLETADA);

                        // Muda o state dele para ObjectState.SALVO
                        coletaEstoque.STATE = ObjectState.SALVO;
                        break;
                    case ObjectState.ALTERADO:
                        // Executa a query salvando os dados
                        if (EXISTE_CAMPO_PERDA_GIRO() ||
                            CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                            CSDataAccess.Instance.ExecuteScalar(sqlQueryUpdate.ToString(), pDAT_COLETA, pQTD_COLETADA, pQTD_PERDA, pQTD_GIRO_SELLOUT, pSOM_UTM_QTD_GIRO_SELLOUT, pDAT_COLETADOIS, pCOD_EMPREGADO, pCOD_PDV, pCOD_PRODUTO);
                        else
                            CSDataAccess.Instance.ExecuteScalar(sqlQueryUpdate.ToString(), pDAT_COLETA, pQTD_COLETADA, pDAT_COLETADOIS, pCOD_EMPREGADO, pCOD_PDV, pCOD_PRODUTO);
                        // Muda o state dele para ObjectState.SALVO
                        coletaEstoque.STATE = ObjectState.SALVO;
                        break;
                    case ObjectState.DELETADO:
                        // Executa a query apagando os dados
                        CSDataAccess.Instance.ExecuteScalar(sqlQueryDelete, pDAT_COLETA, pCOD_EMPREGADO, pCOD_PDV, pCOD_PRODUTO);
                        break;
                }
            }

            return true;
        }

        public bool EXISTE_CAMPO_PERDA_GIRO()
        {
            if (!CSEmpresa.ColunaExiste("PRODUTO_COLETA_ESTOQUE", "QTD_PERDA"))
                return false;
            if (!CSEmpresa.ColunaExiste("PRODUTO_COLETA_ESTOQUE", "QTD_GIRO_SELLOUT"))
                return false;
            if (!CSEmpresa.ColunaExiste("PRODUTO_COLETA_ESTOQUE", "SOM_UTM_QTD_GIRO_SELLOUT"))
                return false;

            return true;
        }

        public CSColetaEstoquePDV EstoqueColetadoPDV
        {
            get
            {
                return this;
            }
        }

        public CSColetaEstoquePDV.CSColetaEstoqueProduto GetProdutoColetado(int COD_PRODUTO)
        {
            CSColetaEstoquePDV.CSColetaEstoqueProduto ret = new CSColetaEstoquePDV.CSColetaEstoqueProduto();

            // Procura pelo produto na coleta de estoque do PDV
            foreach (CSColetaEstoquePDV.CSColetaEstoqueProduto produto in base.InnerList)
            {
                if (produto.PRODUTO.COD_PRODUTO == COD_PRODUTO)
                {
                    ret = produto;
                    return ret;
                }
            }
            // retorna nulo se não for encontrado
            return null;
        }

        public void Dispose()
        {
            this.InnerList.Clear();
            this.InnerList.TrimToSize();
        }

        private void AdicionaProdutoCategoria(int COD_PDV)
        {
            try
            {
                string sqlQuery = " SELECT T2.COD_PDV " +
                                  "       ,T1.COD_PRODUTO " +
                                  "   FROM PRODUTO_CATEGORIA T1 " +
                                  "        INNER JOIN PDV T2 ";

                if (CSEmpresa.Current.IND_CLUSTER_PRODUTOS_PELO_TIPO_FREEZER)
                    sqlQuery += "           ON T1.COD_CATEGORIA = T2.COD_DENVER ";
                else
                    sqlQuery += "           ON T1.COD_CATEGORIA = T2.COD_CATEGORIA ";

                sqlQuery += "        INNER JOIN PRODUTO T3 " +
                              "           ON T3.COD_PRODUTO = T1.COD_PRODUTO " +
                              "        LEFT JOIN PRODUTO_COLETA_ESTOQUE T4 " +
                              "          ON T4.COD_PRODUTO = T1.COD_PRODUTO AND " +
                              "             T4.COD_PDV  = T2.COD_PDV AND " +
                    //"             T4.IND_HISTORICO = 0 AND " +
                              "             DATE(T4.DAT_COLETA) = DATE('NOW') AND " +
                              "             T4.COD_EMPREGADO = ? " +
                              "  WHERE T4.COD_PRODUTO IS NULL " +
                              "    AND T3.IND_ATIVO = 'A' " +
                              "    AND T3.IND_ITEM_COMBO = 0 " +
                              "    AND T3.COD_PRODUTO <> 99999 " +
                              "    AND T2.COD_PDV = ? ";

                SQLiteParameter pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);
                SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", COD_PDV);

                // Busca todos os produtos da categoria para coletado do PDV
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_EMPREGADO, pCOD_PDV))
                {
                    while (sqlReader.Read())
                    {
                        CSColetaEstoquePDV.CSColetaEstoqueProduto produtoColetado = new CSColetaEstoquePDV.CSColetaEstoqueProduto();

                        // Preenche a instancia da classe coleta de estoque                        
                        produtoColetado.COD_EMPREGADO = CSEmpregados.Current.COD_EMPREGADO;
                        produtoColetado.COD_PDV = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        produtoColetado.PRODUTO = CSProdutos.GetProduto(sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1));
                        produtoColetado.QTD_COLETADA_TOTAL = -1;
                        produtoColetado.QTD_PERDA_TOTAL = -1;
                        produtoColetado.STATE = ObjectState.NOVO;

                        // Adciona o produtos coletao do PDV na coleção de Coleta de estoque
                        base.InnerList.Add(produtoColetado);
                    }
                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro na busca dos produtos da categoria para coletado do PDV", ex);
            }
        }

        #endregion

        #region [ SubClasses ]

        public class CSColetaEstoqueProduto
        {
            #region [ Variaveis ]

            private DateTime m_DAT_COLETA = DateTime.Now;
            private int m_COD_EMPREGADO = CSEmpregados.Current.COD_EMPREGADO;
            private int m_COD_PDV = CSPDVs.Current.COD_PDV;
            private CSProdutos.CSProduto m_PRODUTO;
            private decimal m_QTD_COLETADA_TOTAL;
            private decimal m_QTD_COLETADA_INTEIRA;
            private int m_QTD_COLETADA_UNIDADE;
            private decimal m_QTD_PERDA_TOTAL;
            private decimal m_QTD_PERDA_INTEIRA;
            private int m_QTD_PERDA_UNIDADE;
            private decimal m_SOM_UTM_QTD_GIRO_SELLOUT;
            private string m_MOTIVO_COLETA = string.Empty;
            private bool m_ListaVenda3Meses;
            private int m_QTD_COLETA_SELLOUT;
            private int m_NUM_COLETA_ESTOQUE;
            private bool m_PRIMEIRA_COLETA_PRODUTO = false;
            private ObjectState m_STATE = ObjectState.NOVO;

            #endregion

            #region [ Propriedades ]

            public DateTime DAT_COLETA
            {
                get
                {
                    return m_DAT_COLETA;
                }
                set
                {
                    m_DAT_COLETA = value;
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

            public CSProdutos.CSProduto PRODUTO
            {
                get
                {
                    return m_PRODUTO;
                }
                set
                {
                    m_PRODUTO = value;
                }
            }

            public decimal QTD_COLETADA_TOTAL
            {
                get
                {
                    decimal total = 0;

                    switch (this.PRODUTO.COD_UNIDADE_MEDIDA)
                    {
                        case "CX":
                        case "DZ":
                            total = this.QTD_COLETADA_INTEIRA * this.PRODUTO.QTD_UNIDADE_EMBALAGEM + this.QTD_COLETADA_UNIDADE;
                            break;
                        default:
                            total = this.QTD_COLETADA_INTEIRA + this.QTD_COLETADA_UNIDADE;
                            break;
                    }
                    return total;
                }

                set
                {
                    switch (this.PRODUTO.COD_UNIDADE_MEDIDA)
                    {
                        case "CX":
                        case "DZ":
                            this.QTD_COLETADA_UNIDADE = int.Parse(value.ToString()) % this.PRODUTO.QTD_UNIDADE_EMBALAGEM;
                            this.QTD_COLETADA_INTEIRA = ((int.Parse(value.ToString()) - this.QTD_COLETADA_UNIDADE) / this.PRODUTO.QTD_UNIDADE_EMBALAGEM);
                            break;
                        default:
                            this.QTD_COLETADA_INTEIRA = int.Parse(value.ToString());
                            this.QTD_COLETADA_UNIDADE = 0;
                            break;
                    }
                    m_QTD_COLETADA_TOTAL = value;
                }
            }

            public decimal QTD_COLETADA_INTEIRA
            {
                get
                {
                    return m_QTD_COLETADA_INTEIRA;
                }
                set
                {
                    m_QTD_COLETADA_INTEIRA = value;
                }
            }

            public int QTD_COLETADA_UNIDADE
            {
                get
                {
                    return m_QTD_COLETADA_UNIDADE;
                }
                set
                {
                    m_QTD_COLETADA_UNIDADE = value;
                }
            }

            public decimal QTD_PERDA_TOTAL
            {
                get
                {
                    decimal total = 0;

                    switch (this.PRODUTO.COD_UNIDADE_MEDIDA)
                    {
                        case "CX":
                        case "DZ":
                            total = this.QTD_PERDA_INTEIRA * this.PRODUTO.QTD_UNIDADE_EMBALAGEM + this.QTD_PERDA_UNIDADE;
                            break;
                        default:
                            total = this.QTD_PERDA_INTEIRA + this.QTD_PERDA_UNIDADE;
                            break;
                    }
                    return total;
                }

                set
                {
                    switch (this.PRODUTO.COD_UNIDADE_MEDIDA)
                    {
                        case "CX":
                        case "DZ":
                            this.QTD_PERDA_UNIDADE = int.Parse(value.ToString()) % this.PRODUTO.QTD_UNIDADE_EMBALAGEM;
                            this.QTD_PERDA_INTEIRA = ((int.Parse(value.ToString()) - this.QTD_PERDA_UNIDADE) / this.PRODUTO.QTD_UNIDADE_EMBALAGEM);
                            break;
                        default:
                            this.QTD_PERDA_INTEIRA = int.Parse(value.ToString());
                            this.QTD_PERDA_UNIDADE = 0;
                            break;
                    }
                    m_QTD_PERDA_TOTAL = value;
                }
            }

            public decimal QTD_PERDA_INTEIRA
            {
                get
                {
                    return m_QTD_PERDA_INTEIRA;
                }
                set
                {
                    m_QTD_PERDA_INTEIRA = value;
                }
            }

            public int QTD_PERDA_UNIDADE
            {
                get
                {
                    return m_QTD_PERDA_UNIDADE;
                }
                set
                {
                    m_QTD_PERDA_UNIDADE = value;
                }
            }

            public decimal SOM_UTM_QTD_GIRO_SELLOUT
            {
                get
                {
                    return m_SOM_UTM_QTD_GIRO_SELLOUT;
                }
                set
                {
                    m_SOM_UTM_QTD_GIRO_SELLOUT = value;
                }
            }

            public string MOTIVO_COLETA
            {
                get
                {
                    return m_MOTIVO_COLETA;
                }
                set
                {
                    m_MOTIVO_COLETA = value;
                }
            }

            //Retorna se o produto está na lista de vendas dos últimos três meses do PDV
            public bool ListaVenda3Meses
            {
                get
                {
                    bool ListaVendas = false;

                    StringBuilder sqlQuery = new StringBuilder();

                    sqlQuery.Length = 0;
                    sqlQuery.Append("       SELECT *                                                                  ");
                    sqlQuery.Append("		FROM [PEDIDO]                                                           ");
                    sqlQuery.Append("		JOIN [ITEM_PEDIDO]                                                      ");
                    sqlQuery.Append("		ON [PEDIDO].[COD_PEDIDO] = [ITEM_PEDIDO].[COD_PEDIDO]                   ");
                    sqlQuery.Append("		JOIN [PRODUTO]                                                          ");
                    sqlQuery.Append("		ON [PRODUTO].[COD_PRODUTO] = [ITEM_PEDIDO].[COD_PRODUTO]                ");
                    sqlQuery.Append("		JOIN [TABELA_PRECO_PRODUTO]                                             ");
                    sqlQuery.Append("		ON [PRODUTO].[COD_PRODUTO] = [TABELA_PRECO_PRODUTO].[COD_PRODUTO]       ");
                    sqlQuery.Append("		WHERE [PEDIDO].[IND_HISTORICO] = 1                                      ");
                    sqlQuery.Append("		AND (JULIANDAY(DATE('NOW')) - JULIANDAY([PEDIDO].DAT_PEDIDO)) <= 90     ");
                    sqlQuery.Append("		AND [PEDIDO].[COD_PDV] = ?                                              ");
                    sqlQuery.Append("		AND [PRODUTO].[IND_ATIVO] = 'A'                                         ");
                    sqlQuery.Append("       AND [PRODUTO].[COD_PRODUTO] = ?                                                  ");

                    SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", CSPDVs.Current.COD_PDV);
                    SQLiteParameter pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", this.PRODUTO.COD_PRODUTO);

                    using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(false, sqlQuery.ToString(), pCOD_PDV, pCOD_PRODUTO))
                    {
                        while (reader.Read())
                        {
                            ListaVendas = true;
                            break;
                        }

                        reader.Close();
                        reader.Dispose();
                    }

                    return ListaVendas;
                }
            }

            public int QTD_COLETA_SELLOUT
            {
                get
                {
                    return m_QTD_COLETA_SELLOUT;
                }

                set
                {
                    m_QTD_COLETA_SELLOUT = value;
                }
            }

            public int NUM_COLETA_ESTOQUE
            {
                get
                {
                    return m_NUM_COLETA_ESTOQUE;
                }

                set
                {
                    m_NUM_COLETA_ESTOQUE = value;
                }
            }

            public bool PRIMEIRA_COLETA_PRODUTO
            {
                get
                {
                    bool PRIMEIRA_COLETA = true;

                    StringBuilder sqlQuery = new StringBuilder();

                    sqlQuery.Length = 0;
                    sqlQuery.Append("SELECT [COD_PRODUTO] ");
                    sqlQuery.Append("FROM [PRODUTO_COLETA_ESTOQUE] ");
                    sqlQuery.Append(" WHERE [COD_PDV] = ? ");
                    sqlQuery.Append(" AND [COD_PRODUTO] = ? ");
                    //sqlQuery.Append("  AND DATE([DAT_COLETA]) = (SELECT MAX(DATE(DAT_COLETA)) FROM [PRODUTO_COLETA_ESTOQUE] ");
                    //sqlQuery.Append("   WHERE [COD_PDV] = ? ");
                    //sqlQuery.Append("    AND [IND_HISTORICO] = 1 ");
                    sqlQuery.Append("     AND DATE([DAT_COLETA]) <> DATE('NOW')");

                    SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", CSPDVs.Current.COD_PDV);
                    SQLiteParameter pCOD_PDV2 = new SQLiteParameter("@COD_PDV2", CSPDVs.Current.COD_PDV);
                    SQLiteParameter pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", this.PRODUTO.COD_PRODUTO);

                    using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(false, sqlQuery.ToString(), pCOD_PDV, pCOD_PRODUTO))
                    {
                        while (reader.Read())
                        {
                            PRIMEIRA_COLETA = false;
                            break;
                        }

                        reader.Close();
                        reader.Dispose();
                    }

                    return PRIMEIRA_COLETA;
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

            #region [ Metodos ]

            #endregion

        }

        #endregion

    }
}
