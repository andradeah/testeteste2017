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
using System.Linq;
#endregion

namespace AvanteSales
{
    /// <summary>
    /// <history>
    /// 13/07/04 - Guilherme Magalhaes - Implementacao: IDisposable implementado
    /// </history>
    /// </summary>
    public class CSItemsPedido : CollectionBase, IDisposable
    {
        #region [ Classe de Comparacao implementar IComparer ]

        private class ComparaPorCodigo : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                string produtoX;
                string produtoY;
                long valorX;
                long valorY;

                produtoX = ((CSItemPedido)x).PRODUTO.DESCRICAO_APELIDO_PRODUTO.ToLower();
                produtoY = ((CSItemPedido)y).PRODUTO.DESCRICAO_APELIDO_PRODUTO.ToLower();

                if (!CSGlobal.IsNumeric(produtoX))
                    valorX = CSGlobal.Asc(produtoX);
                else
                    valorX = Convert.ToInt64(produtoX);

                if (!CSGlobal.IsNumeric(produtoY))
                    valorY = CSGlobal.Asc(produtoY);
                else
                    valorY = Convert.ToInt64(produtoY);

                if (valorX == valorY)
                    return 0;

                return valorX > valorY ? 1 : -1;
            }
        }

        private class ComparaPorDescricao : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                string produtoX;
                string produtoY;

                produtoX = ((CSItemPedido)x).PRODUTO.DSC_PRODUTO.ToLower();
                produtoY = ((CSItemPedido)y).PRODUTO.DSC_PRODUTO.ToLower();

                return produtoX.CompareTo(produtoY);
            }
        }

        #endregion

        private CSItemsPedido.CSItemPedido m_Current;
        private int m_COD_PEDIDO;
        private bool _disposed = true;
        private bool m_IND_CARREGA_PRODUTOS_INATIVOS = false;
        private ComparaPorCodigo comparaPorCodigo = new CSItemsPedido.ComparaPorCodigo();
        private ComparaPorDescricao comparaPorDescricao = new CSItemsPedido.ComparaPorDescricao();

        public CSItemsPedido Items
        {
            get
            {
                if (_disposed)
                    Refresh();

                return this;
            }
        }

        public CSItemsPedido.CSItemPedido this[int Index]
        {
            get
            {
                try
                {
                    if (_disposed)
                        Refresh();
                    return (CSItemsPedido.CSItemPedido)this.InnerList[Index];
                }
                catch (Exception ex)
                {
                    CSGlobal.ShowMessage(ex.ToString());
                    throw new ApplicationException("Erro na coleção de itens de pedido", ex);
                }

            }
        }

        public CSItemsPedido.CSItemPedido Current
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

        public bool IND_CARREGA_PRODUTOS_INATIVOS
        {
            get
            {
                return this.m_IND_CARREGA_PRODUTOS_INATIVOS;
            }
            set
            {
                this.m_IND_CARREGA_PRODUTOS_INATIVOS = value;
            }
        }

        public int COD_PEDIDO
        {
            get
            {
                return this.m_COD_PEDIDO;
            }
            set
            {
                this.m_COD_PEDIDO = value;
            }
        }

        #region [ Metodos ]

        public CSItemsPedido(int codigoPedido)
        {
            this.COD_PEDIDO = codigoPedido;
            this.IND_CARREGA_PRODUTOS_INATIVOS = false;

            Refresh();
        }
        public CSItemsPedido(int codigoPedido, bool carregaProdutosInativos)
        {
            this.COD_PEDIDO = codigoPedido;
            this.IND_CARREGA_PRODUTOS_INATIVOS = carregaProdutosInativos;

            Refresh();
        }

        /// <summary>
        /// Faz a populacao da lista de itens pedidos dinamicamente
        /// <history>
        /// 13/07/04 - Re-Implementacao: Guilherme Magalhaes
        /// </history>
        /// </summary>
        private void Refresh()
        {
            try
            {
                StringBuilder sqlQuery = new StringBuilder();
                SQLiteParameter pCOD_PEDIDO;

                sqlQuery.Append("SELECT VLR_UNITARIO, PRC_DESCONTO, VLR_TOTAL ");
                sqlQuery.Append("      ,VLR_ADICIONAL_FINANCEIRO, QTD_PEDIDA ");
                sqlQuery.Append("      ,VLR_DESCONTO, ITEM_PEDIDO.COD_PRODUTO, PRC_ADICIONAL_FINANCEIRO ");
                sqlQuery.Append("      ,VLR_ADICIONAL_UNITARIO, VLR_DESCONTO_UNITARIO ");
                sqlQuery.Append("      ,COD_TABELA_PRECO, IND_PROD_ESPECIFICO_CATEGORIA ");
                sqlQuery.Append("      ,VLR_INDENIZACAO, VLR_VERBA_EXTRA, VLR_VERBA_NORMAL, ITEM_PEDIDO.COD_ITEM_COMBO ");
                sqlQuery.Append("      ,ITEM_PEDIDO.QTD_INDENIZACAO, ITEM_PEDIDO.VLR_UNITARIO_INDENIZACAO, ITEM_PEDIDO.IND_UTILIZA_QTD_SUGERIDA ");
                sqlQuery.Append(CSGlobal.ValidarTopCategoria ? ", IND_PROD_TOP_CATEGORIA" : ", 0 AS IND_PROD_TOP_CATEGORIA");
                sqlQuery.Append("  FROM PDV ");
                sqlQuery.Append("  JOIN PEDIDO ");
                sqlQuery.Append("    ON PDV.COD_PDV = PEDIDO.COD_PDV ");
                sqlQuery.Append("  JOIN ITEM_PEDIDO ");
                sqlQuery.Append("    ON PEDIDO.COD_PEDIDO = ITEM_PEDIDO.COD_PEDIDO ");
                sqlQuery.Append("  LEFT JOIN PRODUTO_CATEGORIA ");
                sqlQuery.Append("    ON ITEM_PEDIDO.COD_PRODUTO = PRODUTO_CATEGORIA.COD_PRODUTO ");

                if (CSEmpresa.Current.IND_CLUSTER_PRODUTOS_PELO_TIPO_FREEZER)
                    sqlQuery.Append("   AND PDV.COD_DENVER = PRODUTO_CATEGORIA.COD_CATEGORIA ");
                else
                    sqlQuery.Append("   AND PDV.COD_CATEGORIA = PRODUTO_CATEGORIA.COD_CATEGORIA ");

                sqlQuery.Append(" WHERE PEDIDO.COD_PEDIDO=?");


                pCOD_PEDIDO = new SQLiteParameter("@COD_PEDIDO", this.COD_PEDIDO);

                // Busca todos os contatos do PDV
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString(), pCOD_PEDIDO))
                {
                    while (sqlReader.Read())
                    {
                        CSItemPedido ip = new CSItemPedido();
                        // Preenche a instancia da classe de items do pedido
                        ip.VLR_ITEM_UNIDADE = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(0));
                        ip.PRC_DESCONTO = sqlReader.GetValue(1) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(1));
                        ip.VLR_TOTAL_ITEM = sqlReader.GetValue(2) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(2));
                        ip.VLR_ADICIONAL_FINANCEIRO = sqlReader.GetValue(3) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(3));

                        // [ Verifica se deve carregar produtos inativos ]
                        if (this.IND_CARREGA_PRODUTOS_INATIVOS)
                            ip.PRODUTO = CSProdutos.GetProdutoInativo(sqlReader.GetValue(6) == System.DBNull.Value ? -1 : sqlReader.GetInt32(6));
                        else
                            ip.PRODUTO = CSProdutos.GetProduto(sqlReader.GetValue(6) == System.DBNull.Value ? -1 : sqlReader.GetInt32(6));

                        ip.QTD_PEDIDA_TOTAL = sqlReader.GetValue(4) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(4));
                        ip.VLR_DESCONTO = sqlReader.GetValue(5) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(5));
                        ip.PRC_ADICIONAL_FINANCEIRO = sqlReader.GetValue(7) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(7));
                        ip.COD_PEDIDO = this.COD_PEDIDO;
                        ip.VLR_ADICIONAL_UNITARIO = sqlReader.GetValue(8) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(8));
                        ip.VLR_DESCONTO_UNITARIO = sqlReader.GetValue(9) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(9));
                        ip.COD_TABELA_PRECO = sqlReader.GetValue(10) == System.DBNull.Value ? -1 : sqlReader.GetInt32(10);
                        ip.IND_PROD_ESPECIFICO_CATEGORIA = sqlReader.GetValue(11) == System.DBNull.Value ? false : sqlReader.GetBoolean(11);
                        ip.VLR_INDENIZACAO = sqlReader.GetValue(12) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(12));
                        ip.VLR_VERBA_EXTRA = sqlReader.GetValue(13) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(13));
                        ip.VLR_VERBA_NORMAL = sqlReader.GetValue(14) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(14));
                        ip.COD_ITEM_COMBO = sqlReader.GetValue(15) == System.DBNull.Value ? 0 : sqlReader.GetInt32(15);
                        ip.QTD_INDENIZACAO_TOTAL = sqlReader.GetValue(16) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(16));
                        ip.VLR_INDENIZACAO_UNIDADE = sqlReader.GetValue(17) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(17));
                        ip.IND_UTILIZA_QTD_SUGERIDA = sqlReader.GetValue(18) == System.DBNull.Value ? false : sqlReader.GetBoolean(18);
                        ip.IND_PROD_TOP_CATEGORIA = sqlReader.GetValue(19) == System.DBNull.Value ? false : sqlReader.GetBoolean(19);

                        ip.STATE = ObjectState.INALTERADO;

                        // Adciona os items do pedido na coleção deste pedido
                        this.InnerList.Add(ip);
                    }
                    sqlReader.Close();
                }

                _disposed = false;
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new ApplicationException("Erro no Refresh do Collection PedidosPDV", ex);
            }
        }

        /// <summary>
        /// Adiciona mais um item pedido na coleção no pedido
        /// </summary>
        /// <param name="pedido">Uma instacia do item pedido a ser adcionada</param>
        /// <returns>return a posição do item pedido na coleção</returns>
        public int Add(CSItemPedido itempedido)
        {
            return this.InnerList.Add(itempedido);
        }

        /// <summary>
        /// Salva os dados na coleção no banco
        /// </summary>
        public void Flush()
        {
            try
            {
                // Criar os parametros de salvamento
                SQLiteParameter pCOD_PEDIDO;
                SQLiteParameter pCOD_EMPREGADO;

                int result;

                string sqlDeleteItemPedido =
                    "DELETE FROM ITEM_PEDIDO " +
                    " WHERE COD_PEDIDO = ? " +
                    "   AND COD_EMPREGADO = ? ";

                StringBuilder sqlInsertItemPedido = new StringBuilder();
                sqlInsertItemPedido.AppendLine("INSERT INTO ITEM_PEDIDO");
                sqlInsertItemPedido.AppendLine("(COD_PEDIDO, COD_PRODUTO, COD_EMPREGADO, ");
                sqlInsertItemPedido.AppendLine("VLR_UNITARIO, PRC_DESCONTO, VLR_TOTAL, ");
                sqlInsertItemPedido.AppendLine("VLR_ADICIONAL_FINANCEIRO, QTD_PEDIDA,VLR_DESCONTO, ");
                sqlInsertItemPedido.AppendLine("PRC_ADICIONAL_FINANCEIRO, VLR_ADICIONAL_UNITARIO, VLR_DESCONTO_UNITARIO, ");
                sqlInsertItemPedido.AppendLine("COD_TABELA_PRECO, VLR_INDENIZACAO, VLR_VERBA_EXTRA, VLR_VERBA_NORMAL, COD_ITEM_COMBO, ");
                sqlInsertItemPedido.AppendLine("QTD_INDENIZACAO,VLR_UNITARIO_INDENIZACAO,IND_UTILIZA_QTD_SUGERIDA ");

                if (CSEmpresa.ColunaExiste("ITEM_PEDIDO", "VLR_TOTAL_IMPOSTO_BROKER"))
                    sqlInsertItemPedido.AppendLine(",VLR_TOTAL_IMPOSTO_BROKER ");

                sqlInsertItemPedido.AppendLine("   ) ");
                sqlInsertItemPedido.AppendLine("SELECT " + CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO.ToString() + " AS COD_PEDIDO,CODPRODUTO,");
                sqlInsertItemPedido.AppendLine(CSEmpregados.Current.COD_EMPREGADO + " AS COD_EMPREGADO, ");
                sqlInsertItemPedido.AppendLine("VLR_UNITARIO,PRC_DESCONTO,VLR_TOTAL, ");
                sqlInsertItemPedido.AppendLine("VLR_ADICIONAL_FINANCEIRO,QTD_PEDIDA,VLR_DESCONTO, ");
                sqlInsertItemPedido.AppendLine("PRC_ADICIONAL_FINANCEIRO,VLR_ADICIONAL_UNITARIO,VLR_DESCONTO_UNITARIO, ");
                sqlInsertItemPedido.AppendLine("COD_TABELA_PRECO,VLR_INDENIZACAO,VLR_VERBA_EXTRA,VLR_VERBA_NORMAL,COD_ITEM_COMBO, ");
                sqlInsertItemPedido.AppendLine("QTD_INDENIZACAO,VLR_UNITARIO_INDENIZACAO,IND_UTILIZA_QTD_SUGERIDA ");

                if (CSEmpresa.ColunaExiste("ITEM_PEDIDO", "VLR_TOTAL_IMPOSTO_BROKER"))
                    sqlInsertItemPedido.AppendLine(",VLR_TOTAL_IMPOSTO_BROKER ");

                sqlInsertItemPedido.AppendLine("FROM TMPITEMPEDIDO ");

                // Criar os parametros para exclusao das informações dos itens do pedido atual
                pCOD_PEDIDO = new SQLiteParameter("@COD_PEDIDO", CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO);
                pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);

                // Executa a query apagando os dados
                result = CSDataAccess.Instance.ExecuteNonQuery(sqlDeleteItemPedido, pCOD_PEDIDO, pCOD_EMPREGADO);

                if (CSPDVs.Current.PEDIDOS_PDV.Current.STATE != ObjectState.DELETADO)
                {
                    pCOD_PEDIDO = new SQLiteParameter("@COD_PEDIDO", CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO);
                    pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", CSEmpregados.Current.COD_EMPREGADO);

                    try
                    {
                        // Insere item de pedidos basedo na tabela TMPITEMPEDIDO
                        result = CSDataAccess.Instance.ExecuteNonQuery(sqlInsertItemPedido.ToString());
                    }
                    catch (Exception y)
                    {

                    }
                    if (result == 0)
                        throw new Exception("Erro no sqlInsertItemPedido");
                }

                //Atualiza saldo de estoque dos produtos
                foreach (CSItemPedido itempedido in ((System.Collections.ArrayList)(base.InnerList.Clone())))
                {
                    itempedido.PRODUTO.Flush();
                }

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro no flush dos items de pedido", ex);
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

        public void Sort(bool ordenaPorCodigo)
        {
            if (ordenaPorCodigo)
                this.InnerList.Sort(comparaPorCodigo);
            else
                this.InnerList.Sort(comparaPorDescricao);
        }

        public void Dispose()
        {
            try
            {
                string sqlQueryDelete = "DELETE FROM TMPITEMPEDIDO";

                // Libera o innerList para ser disponivel para ser GarbageCollected
                this.InnerList.Clear();
                this.InnerList.TrimToSize();

                //Apaga a tabela de imagem de item de pedidos
                CSDataAccess.Instance.ExecuteScalar(sqlQueryDelete);


                _disposed = true;
            }
            catch (Exception e)
            {
                CSGlobal.ShowMessage(e.ToString());
                throw new Exception("Ocorreu ao liberar recursos da classe de item de pedidos...", e);
            }
        }

        #endregion


        #region  [ SubClasses ]

        public class CSItemPedido
#if ANDROID
 : Java.Lang.Object
#endif
        {
            #region [ Variaveis ]

            private decimal m_VLR_ITEM_UNIDADE;
            private decimal m_VLR_ITEM_INTEIRA;
            private decimal m_VLR_TOTAL_ITEM;
            private decimal m_VLR_TABELA_BUNGE;
            private decimal m_VLR_DESCONTO;
            private decimal m_PRC_DESCONTO;
            private decimal m_VLR_ADICIONAL_FINANCEIRO;
            private decimal m_PRC_ADICIONAL_FINANCEIRO;
            private decimal m_QTD_PEDIDA_TOTAL;
            private decimal m_QTD_PEDIDA_INTEIRA;
            private int m_QTD_PEDIDA_UNIDADE;
            private int m_COD_PEDIDO;
            private ObjectState m_STATE = ObjectState.NOVO;
            private bool m_ATUALIZAR_SALDO_DESCONTO = true;
            private CSProdutos.CSProduto m_PRODUTO;
            private decimal m_VLR_DESCONTO_UNITARIO;
            private decimal m_VLR_ADICIONAL_UNITARIO;
            private int m_COD_TABELA_PRECO;
            private bool m_IND_PROD_ESPECIFICO_CATEGORIA;
            private bool m_IND_PROD_TOP_CATEGORIA;
            private bool m_IND_UTILIZA_QTD_MINIMA;
            private decimal m_VLR_TOTAL_IMPOSTO_BROKER;
            private bool m_IND_EXECUTOU_REGRA_DESCONTO = false;
            private decimal m_PRC_DESCONTO_MAXIMO;
            private decimal m_VLR_INDENIZACAO;
            private decimal m_VLR_VERBA_EXTRA;
            private decimal m_VLR_VERBA_NORMAL;
            private int m_COD_ITEM_COMBO;
            private bool m_LOCK_QTD;
            private decimal m_QTD_INDENIZACAO_TOTAL;
            private decimal m_QTD_INDENIZACAO_INTEIRA;
            private int m_QTD_INDENIZACAO_UNIDADE;
            private decimal m_VLR_INDENIZACAO_UNIDADE;
            private bool m_IND_UTILIZA_QTD_SUGERIDA;
            private decimal m_VLR_INDENIZACAO_EXIBICAO;
            private decimal m_QTD_INDENIZACAO_EXIBICAO;
            private decimal m_PRC_DESCONTO_UNITARIO;

            #endregion

            #region [ Propriedades ]

            public decimal PRC_DESCONTO_MAXIMO
            {
                get
                {
                    return m_PRC_DESCONTO_MAXIMO;
                }
                set
                {
                    m_PRC_DESCONTO_MAXIMO = value;
                }
            }

            public decimal PRC_DESCONTO_UNITARIO
            {
                get
                {
                    return m_PRC_DESCONTO_UNITARIO;
                }
                set
                {
                    m_PRC_DESCONTO_UNITARIO = value;
                }
            }

            public decimal VLR_ITEM_INTEIRA
            {
                get
                {
                    return m_VLR_ITEM_INTEIRA;
                }
                set
                {
                    m_VLR_ITEM_INTEIRA = value;
                }
            }

            public decimal VLR_ITEM_UNIDADE
            {
                get
                {
                    return m_VLR_ITEM_UNIDADE;
                }
                set
                {
                    m_VLR_ITEM_UNIDADE = value;
                }
            }

            public decimal VLR_TOTAL_ITEM
            {
                get
                {
                    return m_VLR_TOTAL_ITEM;
                }
                set
                {
                    m_VLR_TOTAL_ITEM = value;
                }
            }

            public decimal VLR_TABELA_BUNGE
            {
                get
                {
                    return m_VLR_TABELA_BUNGE;
                }
                set
                {
                    m_VLR_TABELA_BUNGE = value;
                }
            }

            public decimal VLR_TABELA_PRECO_ITEM_MENOS_PCT_VERBA_NORMAL
            {
                get
                {
                    try
                    {
                        // [ Calcula o valor do item segundo a tabela de preço ]
                        decimal valorProduto = this.PRODUTO.PRECOS_PRODUTO.GetPrecoProduto(this.COD_TABELA_PRECO).VLR_PRODUTO;

                        decimal valorTotal = valorProduto * this.QTD_PEDIDA_INTEIRA;
                        valorTotal += CSGlobal.Round(valorProduto / this.PRODUTO.QTD_UNIDADE_EMBALAGEM, 3) * this.QTD_PEDIDA_UNIDADE;
                        valorTotal = CSGlobal.Round(valorTotal, 2);
                        valorTotal = CSGlobal.Round((valorTotal / (100 + CSEmpresa.Current.PCT_VERBA_NORMAL)) * 100, 2);

                        return valorTotal;
                    }
                    catch (Exception ex)
                    {
                        return 0;
                    }
                }
            }

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

            public decimal VLR_ADICIONAL_FINANCEIRO_TOTAL
            {
                get
                {
                    return decimal.Round((this.VLR_ADICIONAL_FINANCEIRO * this.QTD_PEDIDA_TOTAL) / this.PRODUTO.UNIDADES_POR_CAIXA, 2);
                }
            }

            public decimal VLR_ADICIONAL_FINANCEIRO
            {
                get
                {
                    return m_VLR_ADICIONAL_FINANCEIRO;
                }
                set
                {
                    m_VLR_ADICIONAL_FINANCEIRO = value;
                    // Recalcula todos os valores que dependem do VLR_ADICIONAL_FINANCEIRO
                }
            }

            public decimal PRC_ADICIONAL_FINANCEIRO
            {
                get
                {
                    return m_PRC_ADICIONAL_FINANCEIRO;
                }
                set
                {
                    m_PRC_ADICIONAL_FINANCEIRO = value;
                }
            }

            public decimal QTD_PEDIDA_TOTAL
            {
                get
                {
                    try
                    {
                        decimal total = 0;

                        switch (this.PRODUTO.COD_UNIDADE_MEDIDA)
                        {
                            case "CX":
                            case "DZ":
                                total = this.QTD_PEDIDA_INTEIRA * this.PRODUTO.QTD_UNIDADE_EMBALAGEM + this.QTD_PEDIDA_UNIDADE;
                                break;
                            default:
                                total = this.QTD_PEDIDA_INTEIRA + this.QTD_PEDIDA_UNIDADE;
                                break;
                        }
                        return total;
                    }
                    catch (Exception ex)
                    {
                        return 0;
                    }
                }

                set
                {
                    try
                    {
                        decimal valueConvertido = 0;
#if ANDROID
                        if (decimal.TryParse(value.ToString(), out valueConvertido))
#else
                    bool converteu = false;
                    try
                    {
                        valueConvertido = Convert.ToInt32(value.ToString());
                        converteu = true;
                    }
                    catch (Exception)
                    {
                        converteu = false;
                    }
                    if (converteu)
#endif
                        {
                            switch (this.PRODUTO.COD_UNIDADE_MEDIDA)
                            {
                                case "CX":
                                case "DZ":
                                    this.QTD_PEDIDA_UNIDADE = Convert.ToInt32(valueConvertido) % this.PRODUTO.QTD_UNIDADE_EMBALAGEM;
                                    this.QTD_PEDIDA_INTEIRA = Convert.ToInt32((valueConvertido - this.QTD_PEDIDA_UNIDADE) / this.PRODUTO.QTD_UNIDADE_EMBALAGEM);
                                    break;
                                default:
                                    this.QTD_PEDIDA_INTEIRA = valueConvertido;
                                    this.QTD_PEDIDA_UNIDADE = 0;
                                    break;
                            }
                        }
                        else
                            m_QTD_PEDIDA_TOTAL = valueConvertido;
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            public decimal QTD_PEDIDA_INTEIRA
            {
                get
                {
                    return m_QTD_PEDIDA_INTEIRA;
                }
                set
                {
                    m_QTD_PEDIDA_INTEIRA = value;
                }
            }

            public int QTD_PEDIDA_UNIDADE
            {
                get
                {
                    return m_QTD_PEDIDA_UNIDADE;
                }
                set
                {
                    m_QTD_PEDIDA_UNIDADE = value;
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
            public int COD_ITEM_COMBO
            {
                get
                {
                    return m_COD_ITEM_COMBO;
                }
                set
                {
                    m_COD_ITEM_COMBO = value;
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

            public decimal VLR_DESCONTO_UNITARIO
            {
                get
                {
                    return m_VLR_DESCONTO_UNITARIO;
                }
                set
                {
                    m_VLR_DESCONTO_UNITARIO = value;
                }
            }

            public decimal VLR_ADICIONAL_UNITARIO
            {
                get
                {
                    return m_VLR_ADICIONAL_UNITARIO;
                }
                set
                {
                    m_VLR_ADICIONAL_UNITARIO = value;
                }
            }

            public int COD_TABELA_PRECO
            {
                get
                {
                    return m_COD_TABELA_PRECO;
                }
                set
                {
                    m_COD_TABELA_PRECO = value;
                }
            }

            public bool IND_PROD_TOP_CATEGORIA
            {
                get
                {
                    return m_IND_PROD_TOP_CATEGORIA;
                }
                set
                {
                    m_IND_PROD_TOP_CATEGORIA = value;
                }
            }

            public bool IND_PROD_ESPECIFICO_CATEGORIA
            {
                get
                {
                    return m_IND_PROD_ESPECIFICO_CATEGORIA;
                }
                set
                {
                    m_IND_PROD_ESPECIFICO_CATEGORIA = value;
                }
            }

            public bool IND_UILIZA_QTD_MINIMA
            {
                get
                {
                    return m_IND_UTILIZA_QTD_MINIMA;
                }
                set
                {
                    m_IND_UTILIZA_QTD_MINIMA = value;
                }
            }

            public decimal VLR_TOTAL_IMPOSTO_BROKER
            {
                get
                {
                    return m_VLR_TOTAL_IMPOSTO_BROKER;
                }
                set
                {
                    m_VLR_TOTAL_IMPOSTO_BROKER = value;
                }
            }
            //public bool CALCULO_DESCONTO
            //{
            //    get
            //    {
            //        decimal valorTabela = 0m;
            //        decimal desconto = 0m;

            //        valorTabela = this.PRODUTO.PRECOS_PRODUTO.Current.VLR_PRODUTO - this.VLR_DESCONTO_UNITARIO;
            //        desconto = 0m;

            //        decimal inteira = this.QTD_PEDIDA_INTEIRA;
            //        decimal partida = this.QTD_PEDIDA_UNIDADE;
            //        decimal caixa = this.PRODUTO.UNIDADES_POR_CAIXA;

            //        decimal precoUnitario = valorTabela / caixa;
            //        decimal precoUnitarioDesconto = precoUnitario * (desconto / 100);

            //        decimal PrecoComDesconto = valorTabela * ((100 - desconto) / 100);
            //        decimal PrecoComDescontoUnitario = PrecoComDesconto / caixa;
            //        decimal PrecoComAdicionalDescontoUnitario = PrecoComDescontoUnitario * (1 + (this.PRC_ADICIONAL_FINANCEIRO / 100));
            //        decimal Quantidade = this.QTD_PEDIDA_TOTAL;
            //        decimal Valor = Math.Round((Quantidade * PrecoComAdicionalDescontoUnitario), 2);

            //        if (Valor == this.VLR_TOTAL_ITEM)
            //            return false;

            //        return true;
            //    }
            //}

            public bool IND_EXECUTOU_REGRA_DESCONTO
            {
                get
                {
                    return m_IND_EXECUTOU_REGRA_DESCONTO;
                }
                set
                {
                    m_IND_EXECUTOU_REGRA_DESCONTO = value;
                }
            }

            public bool ATUALIZAR_SALDO_DESCONTO
            {
                get
                {
                    return m_ATUALIZAR_SALDO_DESCONTO;
                }

                set
                {
                    m_ATUALIZAR_SALDO_DESCONTO = value;
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
                    try
                    {
                        m_STATE = value;

                        // Se for mudado o state do item, muda tb o state do pedido para que aconteça o flush
                        if (m_STATE == ObjectState.ALTERADO || m_STATE == ObjectState.DELETADO)
                        {
                            if ((CSPDVs.Current.PEDIDOS_PDV.Current.STATE != ObjectState.DELETADO) &&
                                (CSPDVs.Current.PEDIDOS_PDV.Current.STATE != ObjectState.NOVO))
                            {
                                CSPDVs.Current.PEDIDOS_PDV.Current.STATE = ObjectState.ALTERADO;
                            }
                        }

                        if (ATUALIZAR_SALDO_DESCONTO)
                        {
                            // Se for mudado o state do item do item do pedido para deletado tem q alterar
                            // o saldo do estoque e calcula saldo desconto
                            if (m_STATE == ObjectState.DELETADO)
                            {
                                // Pedido de indenização não movimenta estoque
                                if (CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO != 20)
                                {
                                    if (CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.IND_PRONTA_ENTREGA)
                                        this.PRODUTO.QTD_ESTOQUE_PRONTA_ENTREGA += this.QTD_PEDIDA_TOTAL;
                                    else
                                        this.PRODUTO.QTD_ESTOQUE += this.QTD_PEDIDA_TOTAL;
                                }

                                // [ Pega o saldo atual do vendedor ]
                                decimal valorSaldoDesconto = CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO.VAL_SALDO_DESCONTO;

                                // [ Retira saldo relativo a verba ]
                                valorSaldoDesconto -= this.VLR_VERBA_NORMAL + this.VLR_VERBA_EXTRA;

                                // [ Atualizou o saldo anteriormente? ]
                                if (CSPDVs.Current.PEDIDOS_PDV.Current.IND_VLR_DESCONTO_ATUSALDO)
                                    valorSaldoDesconto += this.VLR_DESCONTO - this.VLR_INDENIZACAO;

                                CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO.VAL_SALDO_DESCONTO = valorSaldoDesconto;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
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

            public decimal QTD_INDENIZACAO_EXIBICAO
            {
                get
                {
                    return m_QTD_INDENIZACAO_EXIBICAO;
                }

                set
                {
                    m_QTD_INDENIZACAO_EXIBICAO = value;
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

            public decimal VLR_VERBA_EXTRA
            {
                get
                {
                    return m_VLR_VERBA_EXTRA;
                }
                set
                {
                    m_VLR_VERBA_EXTRA = value;
                }
            }

            public decimal VLR_VERBA_NORMAL
            {
                get
                {
                    return m_VLR_VERBA_NORMAL;
                }
                set
                {
                    m_VLR_VERBA_NORMAL = value;
                }
            }

            public bool LOCK_QTD
            {
                get
                {
                    return m_LOCK_QTD;
                }
                set
                {
                    m_LOCK_QTD = value;
                }
            }

            public decimal QTD_INDENIZACAO_TOTAL
            {
                get
                {
                    try
                    {
                        decimal total = 0;

                        switch (this.PRODUTO.COD_UNIDADE_MEDIDA)
                        {
                            case "CX":
                            case "DZ":
                                total = this.QTD_INDENIZACAO_INTEIRA * this.PRODUTO.QTD_UNIDADE_EMBALAGEM + this.QTD_INDENIZACAO_UNIDADE;
                                break;
                            default:
                                total = this.QTD_INDENIZACAO_INTEIRA;
                                break;
                        }
                        return total;
                    }
                    catch (Exception ex)
                    {
                        return 0;
                    }
                }

                set
                {
                    try
                    {
                        switch (this.PRODUTO.COD_UNIDADE_MEDIDA)
                        {
                            case "CX":
                            case "DZ":
                                this.QTD_INDENIZACAO_UNIDADE = Convert.ToInt32(value) % this.PRODUTO.QTD_UNIDADE_EMBALAGEM;
                                this.QTD_INDENIZACAO_INTEIRA = Convert.ToInt32((value - this.QTD_INDENIZACAO_UNIDADE) / this.PRODUTO.QTD_UNIDADE_EMBALAGEM);
                                break;
                            default:
                                this.QTD_INDENIZACAO_INTEIRA = value;
                                this.QTD_INDENIZACAO_UNIDADE = 0;
                                break;
                        }

                        m_QTD_INDENIZACAO_TOTAL = value;
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            public decimal QTD_INDENIZACAO_INTEIRA
            {
                get
                {
                    return m_QTD_INDENIZACAO_INTEIRA;
                }
                set
                {
                    m_QTD_INDENIZACAO_INTEIRA = value;
                }
            }

            public int QTD_INDENIZACAO_UNIDADE
            {
                get
                {
                    return m_QTD_INDENIZACAO_UNIDADE;
                }
                set
                {
                    m_QTD_INDENIZACAO_UNIDADE = value;
                }
            }

            public decimal VLR_INDENIZACAO_UNIDADE
            {
                get
                {
                    return m_VLR_INDENIZACAO_UNIDADE;
                }
                set
                {
                    m_VLR_INDENIZACAO_UNIDADE = value;
                }
            }

            public bool IND_UTILIZA_QTD_SUGERIDA
            {
                get
                {
                    if (m_IND_UTILIZA_QTD_SUGERIDA == null)
                        m_IND_UTILIZA_QTD_SUGERIDA = false;

                    return m_IND_UTILIZA_QTD_SUGERIDA;
                }
                set
                {
                    m_IND_UTILIZA_QTD_SUGERIDA = value;
                }
            }

            #endregion

            #region [ Metodos ]

            public CSItemPedido()
            {
                this.VLR_VERBA_EXTRA = 0;
                this.VLR_VERBA_NORMAL = 0;
                this.VLR_DESCONTO = 0;
                this.PRC_DESCONTO = 0;
                this.m_PRC_DESCONTO_MAXIMO = -1;
                this.m_STATE = ObjectState.NOVO;
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
                            CSGlobal.ShowMessage(ex.ToString());
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

            public CSItemPedido Clone()
            {
                try
                {
                    CSItemPedido novoItem = new CSItemPedido();

                    novoItem.m_VLR_ITEM_UNIDADE = this.m_VLR_ITEM_UNIDADE;
                    novoItem.m_VLR_ITEM_INTEIRA = this.m_VLR_ITEM_INTEIRA;
                    novoItem.m_VLR_TOTAL_ITEM = this.m_VLR_TOTAL_ITEM;
                    novoItem.m_VLR_DESCONTO = this.m_VLR_DESCONTO;
                    novoItem.m_PRC_DESCONTO = this.m_PRC_DESCONTO;
                    novoItem.m_VLR_ADICIONAL_FINANCEIRO = this.m_VLR_ADICIONAL_FINANCEIRO;
                    novoItem.m_PRC_ADICIONAL_FINANCEIRO = this.m_PRC_ADICIONAL_FINANCEIRO;
                    novoItem.m_QTD_PEDIDA_TOTAL = this.m_QTD_PEDIDA_TOTAL;
                    novoItem.m_QTD_PEDIDA_INTEIRA = this.m_QTD_PEDIDA_INTEIRA;
                    novoItem.m_QTD_PEDIDA_UNIDADE = this.m_QTD_PEDIDA_UNIDADE;
                    novoItem.m_COD_PEDIDO = this.m_COD_PEDIDO;
                    novoItem.m_STATE = this.m_STATE;
                    novoItem.m_PRODUTO = this.m_PRODUTO;
                    novoItem.m_VLR_DESCONTO_UNITARIO = this.m_VLR_DESCONTO_UNITARIO;
                    novoItem.m_VLR_ADICIONAL_UNITARIO = this.m_VLR_ADICIONAL_UNITARIO;
                    novoItem.m_COD_TABELA_PRECO = this.m_COD_TABELA_PRECO;
                    novoItem.m_IND_PROD_ESPECIFICO_CATEGORIA = this.m_IND_PROD_ESPECIFICO_CATEGORIA;
                    novoItem.m_PRC_DESCONTO_MAXIMO = this.m_PRC_DESCONTO_MAXIMO;
                    novoItem.m_VLR_INDENIZACAO = this.m_VLR_INDENIZACAO;
                    novoItem.m_VLR_VERBA_EXTRA = this.m_VLR_VERBA_EXTRA;
                    novoItem.m_VLR_VERBA_NORMAL = this.m_VLR_VERBA_NORMAL;
                    novoItem.m_COD_ITEM_COMBO = this.m_COD_ITEM_COMBO;
                    novoItem.m_QTD_INDENIZACAO_TOTAL = this.m_QTD_INDENIZACAO_TOTAL;
                    novoItem.m_QTD_INDENIZACAO_EXIBICAO = this.m_QTD_INDENIZACAO_EXIBICAO;
                    novoItem.m_VLR_INDENIZACAO_EXIBICAO = this.m_VLR_INDENIZACAO_EXIBICAO;
                    novoItem.m_QTD_INDENIZACAO_INTEIRA = this.m_QTD_INDENIZACAO_INTEIRA;
                    novoItem.m_QTD_INDENIZACAO_UNIDADE = this.m_QTD_INDENIZACAO_UNIDADE;
                    novoItem.m_VLR_INDENIZACAO_UNIDADE = this.m_VLR_INDENIZACAO_UNIDADE;
                    novoItem.m_IND_UTILIZA_QTD_SUGERIDA = this.m_IND_UTILIZA_QTD_SUGERIDA;
                    novoItem.m_VLR_TOTAL_IMPOSTO_BROKER = this.m_VLR_TOTAL_IMPOSTO_BROKER;

                    return novoItem;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

            public void Restore(CSItemPedido novoItem)
            {
                try
                {
                    this.m_VLR_ITEM_UNIDADE = novoItem.m_VLR_ITEM_UNIDADE;
                    this.m_VLR_ITEM_INTEIRA = novoItem.m_VLR_ITEM_INTEIRA;
                    this.m_VLR_TOTAL_ITEM = novoItem.m_VLR_TOTAL_ITEM;
                    this.m_VLR_DESCONTO = novoItem.m_VLR_DESCONTO;
                    this.m_PRC_DESCONTO = novoItem.m_PRC_DESCONTO;
                    this.m_VLR_ADICIONAL_FINANCEIRO = novoItem.m_VLR_ADICIONAL_FINANCEIRO;
                    this.m_PRC_ADICIONAL_FINANCEIRO = novoItem.m_PRC_ADICIONAL_FINANCEIRO;
                    this.m_QTD_PEDIDA_TOTAL = novoItem.m_QTD_PEDIDA_TOTAL;
                    this.m_QTD_PEDIDA_INTEIRA = novoItem.m_QTD_PEDIDA_INTEIRA;
                    this.m_QTD_PEDIDA_UNIDADE = novoItem.m_QTD_PEDIDA_UNIDADE;
                    this.m_COD_PEDIDO = novoItem.m_COD_PEDIDO;
                    this.m_STATE = novoItem.m_STATE;
                    this.m_PRODUTO = novoItem.m_PRODUTO;
                    this.m_VLR_DESCONTO_UNITARIO = novoItem.m_VLR_DESCONTO_UNITARIO;
                    this.m_VLR_ADICIONAL_UNITARIO = novoItem.m_VLR_ADICIONAL_UNITARIO;
                    this.m_COD_TABELA_PRECO = novoItem.m_COD_TABELA_PRECO;
                    this.m_IND_PROD_ESPECIFICO_CATEGORIA = novoItem.m_IND_PROD_ESPECIFICO_CATEGORIA;
                    this.m_PRC_DESCONTO_MAXIMO = novoItem.m_PRC_DESCONTO_MAXIMO;
                    this.m_VLR_INDENIZACAO = novoItem.m_VLR_INDENIZACAO;
                    this.m_VLR_VERBA_EXTRA = novoItem.m_VLR_VERBA_EXTRA;
                    this.m_VLR_VERBA_NORMAL = novoItem.m_VLR_VERBA_NORMAL;
                    this.m_COD_ITEM_COMBO = novoItem.m_COD_ITEM_COMBO;
                    this.m_QTD_INDENIZACAO_TOTAL = novoItem.m_QTD_INDENIZACAO_TOTAL;
                    this.m_VLR_INDENIZACAO_EXIBICAO = novoItem.m_VLR_INDENIZACAO_EXIBICAO;
                    this.m_QTD_INDENIZACAO_EXIBICAO = novoItem.m_QTD_INDENIZACAO_EXIBICAO;
                    this.m_QTD_INDENIZACAO_INTEIRA = novoItem.m_QTD_INDENIZACAO_INTEIRA;
                    this.m_QTD_INDENIZACAO_UNIDADE = novoItem.m_QTD_INDENIZACAO_UNIDADE;
                    this.m_VLR_INDENIZACAO_UNIDADE = novoItem.m_VLR_INDENIZACAO_UNIDADE;
                    this.m_IND_UTILIZA_QTD_SUGERIDA = novoItem.m_IND_UTILIZA_QTD_SUGERIDA;
                }
                catch (Exception ex)
                {
                }
            }

            public List<CSItemsPedido.CSItemPedido> SelecionarProdutosFreezer(int COD_DENVER)
            {
                try
                {
                    StringBuilder sqlQuery = new StringBuilder();

                    var ProdutosFreezer = new List<CSItemsPedido.CSItemPedido>();

                    sqlQuery.AppendLine("SELECT [PRODUTO_CATEGORIA].[COD_PRODUTO], ");
                    sqlQuery.AppendLine("[PRODUTO_CATEGORIA].[QTD_MINIMA] ");
                    sqlQuery.AppendLine("FROM [PRODUTO_CATEGORIA] ");
                    sqlQuery.AppendLine("WHERE [COD_PRODUTO] NOT IN(SELECT COD_PRODUTO FROM [PRODUTO_COLETA_ESTOQUE] WHERE [COD_PDV] = ? AND DATE([DAT_COLETA]) = DATE('NOW')) ");
                    sqlQuery.AppendLine("AND [PRODUTO_CATEGORIA].[COD_CATEGORIA] = ?");

                    SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", CSPDVs.Current.COD_PDV);
                    SQLiteParameter pCOD_CATEGORIA = new SQLiteParameter("@COD_CATEGORIA", COD_DENVER);

                    using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(false, sqlQuery.ToString(), pCOD_PDV, pCOD_CATEGORIA))
                    {
                        while (reader.Read())
                        {
                            CSItemsPedido.CSItemPedido item = new CSItemPedido();
                            item.PRODUTO = CSProdutos.GetProduto(reader.GetInt32(0));

                            if (item.PRODUTO != null &&
                                item.PRODUTO.COD_PRODUTO != 0)
                            {
                                item.PRODUTO.QTD_PRODUTO_SUGERIDO = reader.GetInt32(1);
                                item.PRODUTO.IND_UTILIZA_QTD_MINIMA = true;
                                item.PRODUTO.PRODUTO_SUGERIDO_REGRA_FREEZER = true;

                                ProdutosFreezer.Add(item);
                            }
                        }

                        reader.Close();
                        reader.Dispose();
                    }

                    return ProdutosFreezer;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

            public void AtualizaImagem()
            {
                int result;

                try
                {
                    if (CSPDVs.Current.PEDIDOS_PDV.Current.STATE == ObjectState.NOVO)
                        CSPDVs.Current.PEDIDOS_PDV.Current.BLOQUEAR_FOTO = true;

                    string sqlQueryDelete =
                        "DELETE FROM TMPITEMPEDIDO " +
                        " WHERE CODPRODUTO = ? ";

                    string sqlQueryInsert =
                        "INSERT INTO TMPITEMPEDIDO " +
                        " (VLR_UNITARIO,VLR_TOTAL,VLR_DESCONTO " +
                        " ,PRC_DESCONTO,VLR_ADICIONAL_FINANCEIRO,PRC_ADICIONAL_FINANCEIRO " +
                        " ,QTD_PEDIDA,STATE,CODPRODUTO,VLR_DESCONTO_UNITARIO,VLR_ADICIONAL_UNITARIO " +
                        " ,COD_TABELA_PRECO,VLR_INDENIZACAO,VLR_VERBA_EXTRA,VLR_VERBA_NORMAL,COD_ITEM_COMBO " +
                        " ,QTD_INDENIZACAO,VLR_UNITARIO_INDENIZACAO,IND_UTILIZA_QTD_SUGERIDA, VLR_TOTAL_IMPOSTO_BROKER) " +
                        "  VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?) ";

                    // Grava o item do pedido corrente na tabela de item de pedidos
                    // Criar os parametros de salvamento
                    SQLiteParameter pVLR_UNITARIO = new SQLiteParameter("@VLR_UNITARIO", this.VLR_ITEM_UNIDADE);
                    SQLiteParameter pVLR_TOTAL = new SQLiteParameter("@VLR_TOTAL", this.VLR_TOTAL_ITEM);
                    SQLiteParameter pVLR_DESCONTO = new SQLiteParameter("@VLR_DESCONTO", this.VLR_DESCONTO);

                    SQLiteParameter pPRC_DESCONTO = new SQLiteParameter("@PRC_DESCONTO", this.PRC_DESCONTO);
                    SQLiteParameter pVLR_ADICIONAL_FINANCEIRO = new SQLiteParameter("@VLR_ADICIONAL_FINANCEIRO", this.VLR_ADICIONAL_FINANCEIRO);
                    SQLiteParameter pPRC_ADICIONAL_FINANCEIRO = new SQLiteParameter("@PRC_ADICIONAL_FINANCEIRO", this.PRC_ADICIONAL_FINANCEIRO);

                    SQLiteParameter pQTD_PEDIDA = new SQLiteParameter("@QTD_PEDIDA", this.QTD_PEDIDA_TOTAL);
                    SQLiteParameter pCODPEDIDO = new SQLiteParameter("@CODPEDIDO", this.COD_PEDIDO);
                    SQLiteParameter pSTATE = new SQLiteParameter("@STATE", Convert.ToInt32(this.STATE));

                    SQLiteParameter pCODPRODUTO = new SQLiteParameter("@CODPRODUTO", this.PRODUTO.COD_PRODUTO);
                    SQLiteParameter pVLR_DESCONTO_UNITARIO = new SQLiteParameter("@VLR_DESCONTO_UNITARIO", this.VLR_DESCONTO_UNITARIO);
                    SQLiteParameter pVLR_ADICIONAL_UNITARIO = new SQLiteParameter("@VLR_ADICIONAL_UNITARIO", this.VLR_ADICIONAL_UNITARIO);

                    SQLiteParameter pCOD_TABELA_PRECO = new SQLiteParameter("@COD_TABELA_PRECO", this.COD_TABELA_PRECO);

                    SQLiteParameter pVLR_INDENIZACAO = new SQLiteParameter("@VLR_INDENIZACAO", this.VLR_INDENIZACAO);
                    SQLiteParameter pVLR_VERBA_EXTRA = new SQLiteParameter("@VLR_VERBA_EXTRA", this.VLR_VERBA_EXTRA);
                    SQLiteParameter pVLR_VERBA_NORMAL = new SQLiteParameter("@VLR_VERBA_NORMAL", this.VLR_VERBA_NORMAL);
                    SQLiteParameter pCOD_ITEM_COMBO = new SQLiteParameter("@COD_ITEM_COMBO", this.COD_ITEM_COMBO);

                    //Problema que acontecia com alguns clientes: o item possuía vlr de indenização porém com a qtd de indenização igual a 0
                    //Foi preciso fazer a condição abaixo para garantir que a qtd de indenização não seja 0 quando haver vlr de indenização
                    if (this.VLR_INDENIZACAO_UNIDADE != 0 &&
                        this.QTD_INDENIZACAO_TOTAL == 0)
                        this.QTD_INDENIZACAO_TOTAL = 1;

                    SQLiteParameter pQTD_INDENIZACAO = new SQLiteParameter("@QTD_INDENIZACAO", this.QTD_INDENIZACAO_TOTAL);
                    SQLiteParameter pVLR_UNITARIO_INDENIZACAO = new SQLiteParameter("@VLR_UNITARIO_INDENIZACAO", this.VLR_INDENIZACAO_UNIDADE);

                    if (!CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                        this.IND_UTILIZA_QTD_SUGERIDA = false;

                    SQLiteParameter pIND_UTILIZA_QTD_SUGERIDA = new SQLiteParameter("@IND_UTILIZA_QTD_SUGERIDA", Convert.ToBoolean(this.IND_UTILIZA_QTD_SUGERIDA));

                    SqliteParameter pVLR_TOTAL_IMPOSTO_BROKER = new SQLiteParameter("@VLR_TOTAL_IMPOSTO_BROKER", this.VLR_TOTAL_IMPOSTO_BROKER);

                    pVLR_UNITARIO.DbType = DbType.Decimal;
                    pVLR_TOTAL.DbType = DbType.Decimal;
                    pVLR_DESCONTO.DbType = DbType.Decimal;
                    pVLR_TOTAL_IMPOSTO_BROKER.DbType = DbType.Decimal;

                    pPRC_DESCONTO.DbType = DbType.Decimal;
                    pVLR_ADICIONAL_FINANCEIRO.DbType = DbType.Decimal;
                    pPRC_ADICIONAL_FINANCEIRO.DbType = DbType.Decimal;

                    pQTD_PEDIDA.DbType = DbType.Decimal;
                    pSTATE.DbType = DbType.Int32;

                    pCODPRODUTO.DbType = DbType.Int32;
                    pVLR_DESCONTO_UNITARIO.DbType = DbType.Decimal;
                    pVLR_ADICIONAL_UNITARIO.DbType = DbType.Decimal;

                    pCOD_TABELA_PRECO.DbType = DbType.Int32;

                    pVLR_INDENIZACAO.DbType = DbType.Decimal;
                    pVLR_VERBA_EXTRA.DbType = DbType.Decimal;
                    pVLR_VERBA_NORMAL.DbType = DbType.Decimal;
                    pCOD_ITEM_COMBO.DbType = DbType.Int32;

                    pQTD_INDENIZACAO.DbType = DbType.Decimal;
                    pVLR_UNITARIO_INDENIZACAO.DbType = DbType.Decimal;
                    pIND_UTILIZA_QTD_SUGERIDA.DbType = DbType.Boolean;

                    try
                    {
                        result = CSDataAccess.Instance.ExecuteNonQuery(sqlQueryDelete, pCODPRODUTO);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    if (this.STATE != ObjectState.DELETADO)
                    {
                        //Cria transação 
                        CSDataAccess.Instance.Transacao = CSDataAccess.Instance.Connection.BeginTransaction();

                        pCODPRODUTO = new SQLiteParameter("@CODPRODUTO", this.PRODUTO.COD_PRODUTO);

                        // Executa a query salvando os dados
                        result = CSDataAccess.Instance.ExecuteNonQuery(sqlQueryInsert, pVLR_UNITARIO, pVLR_TOTAL, pVLR_DESCONTO,
                            pPRC_DESCONTO, pVLR_ADICIONAL_FINANCEIRO, pPRC_ADICIONAL_FINANCEIRO,
                            pQTD_PEDIDA, pSTATE, pCODPRODUTO, pVLR_DESCONTO_UNITARIO, pVLR_ADICIONAL_UNITARIO,
                            pCOD_TABELA_PRECO, pVLR_INDENIZACAO, pVLR_VERBA_EXTRA, pVLR_VERBA_NORMAL, pCOD_ITEM_COMBO,
                            pQTD_INDENIZACAO, pVLR_UNITARIO_INDENIZACAO, pIND_UTILIZA_QTD_SUGERIDA, pVLR_TOTAL_IMPOSTO_BROKER);

                        if (result == 0)
                            throw new Exception("Item não pode ser inserido na tabela de imagem");

                        CSDataAccess.Instance.Transacao.Commit();
                    }

                }
                catch (Exception e)
                {
                    try
                    {
                        if (CSDataAccess.Instance.Transacao != null)
                            CSDataAccess.Instance.Transacao.Rollback();

                    }
                    catch (Exception ex)
                    {
                        CSGlobal.ShowMessage(ex.ToString());
                    }

                    CSGlobal.ShowMessage(e.ToString());
                    throw new Exception("Ocorreu um erro ao gravar na tabela de imagem de item de pedidos...", e);

                }
                finally
                {
                    if (CSDataAccess.Instance.Transacao != null)
                    {
                        CSDataAccess.Instance.Transacao.Dispose();
                        CSDataAccess.Instance.Transacao = null;
                    }
                }
            }

            public void CalcularValorBunge(int produto, int nr_notebook, int cliente, DateTime data, CSProdutos.CSProduto produtoAtual, int qtdInteira, int qtdUnitaria, decimal desconto, decimal valorUnitario)
            {
                try
                {
                    bool qtdZero = qtdInteira == 0 && qtdUnitaria == 0;

                    if (qtdZero)
                    {
                        qtdInteira = 1;
                        this.QTD_PEDIDA_INTEIRA = 1;
                    }

                    CSPoliticaBunge pricingBunge = new CSPoliticaBunge(produto, nr_notebook, cliente, data, produtoAtual, qtdInteira, qtdUnitaria, desconto, valorUnitario);
                    pricingBunge.ValorFinal();
                    this.VLR_TOTAL_ITEM = pricingBunge.ValorFinalProduto;
                    this.VLR_TABELA_BUNGE = pricingBunge.ValorTabela;
                    this.VLR_ITEM_UNIDADE = this.VLR_TOTAL_ITEM / this.QTD_PEDIDA_TOTAL;
                    this.VLR_ITEM_INTEIRA = (this.VLR_TOTAL_ITEM * this.PRODUTO.QTD_UNIDADE_EMBALAGEM) / this.QTD_PEDIDA_TOTAL;
                }
                catch (Exception ex)
                {
                }
            }

            public decimal CalcularValorTabelaBunge(int produto, int nr_notebook, int cliente, DateTime data, CSProdutos.CSProduto produtoAtual, int qtdInteira, int qtdUnitaria, decimal desconto, decimal? valorUnitario)
            {
                try
                {
                    CSPoliticaBunge pricingBunge = new CSPoliticaBunge(produto, nr_notebook, cliente, data, produtoAtual, qtdInteira, qtdUnitaria, desconto, valorUnitario);
                    pricingBunge.ValorFinal();
                    return pricingBunge.ValorFinalProduto;
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }

            public CSPoliticaBroker.TmpPricingCons[] CalculaValor()
            {
                try
                {
                    bool qtdZero = (this.QTD_PEDIDA_INTEIRA == 0 && this.QTD_PEDIDA_UNIDADE == 0);
                    decimal m_PRC_MAXIMO_DESCONTO = 0;

                    if (qtdZero)
                        this.QTD_PEDIDA_INTEIRA = 1;

                    CSPoliticaBroker.TmpPricingCons[] valor = CSPDVs.Current.POLITICA_BROKER.CalculaPreco(this.PRODUTO.COD_PRODUTO, this.PRODUTO.GRUPO_COMERCIALIZACAO.COD_SETOR_BROKER, Convert.ToInt32(this.QTD_PEDIDA_INTEIRA),
                                this.QTD_PEDIDA_UNIDADE, this.PRC_DESCONTO, this.PRODUTO.QTD_UNIDADE_EMBALAGEM);

                    this.VLR_TOTAL_ITEM = valor[valor.Length - 1].VALOR;
                    this.VLR_ITEM_UNIDADE = valor[valor.Length - 1].VALOR / this.QTD_PEDIDA_TOTAL;
                    this.VLR_ITEM_INTEIRA = (valor[valor.Length - 1].VALOR * this.PRODUTO.QTD_UNIDADE_EMBALAGEM) / this.QTD_PEDIDA_TOTAL;

                    this.PRC_DESCONTO_MAXIMO = Math.Abs(CSPDVs.Current.POLITICA_BROKER.CalculaDescontoMaximo());

                    //var aa = valor[63];

                    //this.PRC_DESCONTO_UNITARIO = Math.Abs(aa.VALOR);

                    //if (valor[valor.Length - 1].CDCONDTYP == "X066")
                    //    this.PRC_DESCONTO_UNITARIO = valor[valor.Length - 1].DADO;

                    //for (int i = 0; i <= valor.Length; i++)
                    //{
                    //    string bb;

                    //    if (valor[i].CDCONDTYP == "Z001")
                    //        bb = string.Empty;
                    //}
                    /* Retorna o desconto maximo entre a tabela de preço e o produto */
                    m_PRC_MAXIMO_DESCONTO = this.RetornaDescontoMaximo();

                    if (CSEmpresa.Current.IND_VALIDA_PCT_MAXIMO_DESCONTO == "S" &&
                         m_PRC_MAXIMO_DESCONTO > -1 &&
                         m_PRC_MAXIMO_DESCONTO < this.PRC_DESCONTO_MAXIMO)
                        this.PRC_DESCONTO_MAXIMO = m_PRC_MAXIMO_DESCONTO;

                    if (qtdZero)
                        this.QTD_PEDIDA_INTEIRA = 0;

                    return valor;

                }
                catch (Exception e)
                {
                    CSGlobal.ShowMessage("###CalculaValor: " + e.Message);
                    return null;
                }
            }

            public CSPoliticaBroker2014.TmpPricingCons[] CalculaValor2014()
            {
                try
                {
                    bool qtdZero = (this.QTD_PEDIDA_INTEIRA == 0 && this.QTD_PEDIDA_UNIDADE == 0) ? true : false;
                    decimal m_PRC_MAXIMO_DESCONTO = 0;

                    if (qtdZero)
                        this.QTD_PEDIDA_INTEIRA = 1;

                    CSPoliticaBroker2014.TmpPricingCons[] valor = CSPDVs.Current.POLITICA_BROKER_2014.CalculaPreco(this.PRODUTO.COD_PRODUTO, this.PRODUTO.GRUPO_COMERCIALIZACAO.COD_SETOR_BROKER, Convert.ToInt32(this.QTD_PEDIDA_INTEIRA),
                                this.QTD_PEDIDA_UNIDADE, this.PRC_DESCONTO, this.PRODUTO.QTD_UNIDADE_EMBALAGEM);

                    this.VLR_TOTAL_ITEM = valor[valor.Length - 1].VALOR;
                    this.VLR_ITEM_UNIDADE = valor[valor.Length - 1].VALOR / this.QTD_PEDIDA_TOTAL;
                    this.VLR_ITEM_INTEIRA = (valor[valor.Length - 1].VALOR * this.PRODUTO.QTD_UNIDADE_EMBALAGEM) / this.QTD_PEDIDA_TOTAL;

                    this.PRC_DESCONTO_MAXIMO = Math.Abs(CSPDVs.Current.POLITICA_BROKER_2014.CalculaDescontoMaximo());

                    /* Retorna o desconto maximo entre a tabela de preço e o produto */
                    m_PRC_MAXIMO_DESCONTO = this.RetornaDescontoMaximo();

                    if (CSEmpresa.Current.IND_VALIDA_PCT_MAXIMO_DESCONTO == "S" &&
                         m_PRC_MAXIMO_DESCONTO > -1 &&
                         m_PRC_MAXIMO_DESCONTO < this.PRC_DESCONTO_MAXIMO)
                        this.PRC_DESCONTO_MAXIMO = m_PRC_MAXIMO_DESCONTO;

                    if (CSEmpresa.ColunaExiste("ITEM_PEDIDO", "VLR_TOTAL_IMPOSTO_BROKER"))
                    {
                        this.VLR_TOTAL_IMPOSTO_BROKER = 0;

                        var lista = valor.Where(v => v != null && (v.DADO != 0 || v.VALOR != 0)).ToList();

                        decimal vlr_total_imposto_broker = 0m;

                        for (int i = 0; i < lista.Count; i++)
                        {
                            if (lista[i].NMCONDLIN == 769)
                            {
                                vlr_total_imposto_broker += lista[i].VALOR;
                            }
                            else if (lista[i].NMCONDLIN == 789)
                            {
                                vlr_total_imposto_broker += lista[i].VALOR;
                            }
                        }

                        this.VLR_TOTAL_IMPOSTO_BROKER = vlr_total_imposto_broker;
                    }
                    else
                        this.VLR_TOTAL_IMPOSTO_BROKER = 0;

                    if (qtdZero)
                        this.QTD_PEDIDA_INTEIRA = 0;

                    return valor;
                }
                catch (Exception e)
                {
                    CSGlobal.ShowMessage("###CalculaValor: " + e.Message);
                    return null;
                }
            }

            public void AplicaDescontoMaximoProdutoTabPreco()
            {
                try
                {
                    this.PRC_DESCONTO_MAXIMO = this.RetornaDescontoMaximo();

                    if (this.PRC_DESCONTO_MAXIMO == -1)
                        this.PRC_DESCONTO_MAXIMO = 0;
                }
                catch (Exception ex)
                {
                }
            }
            public void AplicaDescontoMaximoQuandoNaoFoiCalculaPrecoNestle()
            {
                try
                {
                    StringBuilder sqlQuery = new StringBuilder();
                    decimal m_PRC_MAXIMO_DESCONTO = 0;
                    int COD_PRODUTO = this.PRODUTO.COD_PRODUTO;

                    // [ Tratamento temporário para atender à mudança de códigos de produtos da nestle ]
                    if (COD_PRODUTO < 41000)
                        COD_PRODUTO += 410000;

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
                    sqlQuery.Append("          T4.CDCANDISTR = T0.CDCANDISTR AND T4.CDDIVISAO = '00' ");
                    sqlQuery.Append("  LEFT JOIN BRK_EPRECOA906 T1 ");
                    sqlQuery.Append("       ON T1.CDAPLICAC = 'V' AND T1.CDCONDTYP = 'X066' AND ");
                    sqlQuery.Append("          T1.CDGER0 = T0.CDGER0 AND T1.CDCANDISTR = T0.CDCANDISTR AND ");
                    sqlQuery.Append("       T1.CDCLIH IN (T0.CDCLIN4,T0.CDCLIN5,T0.CDCLIN6,T0.CDCLI) AND ");
                    sqlQuery.Append("          T1.CDPRD = T4.CDPRD AND ");
                    sqlQuery.Append("       ? BETWEEN T1.DTVIGINI AND T1.DTVIGFIM ");
                    sqlQuery.Append("  LEFT JOIN BRK_EPRECOA905 T2 ");
                    sqlQuery.Append("       ON T2.CDAPLICAC = 'V' AND T2.CDCONDTYP = 'X066' AND ");
                    sqlQuery.Append("          T2.CDGER0 = T0.CDGER0 AND T2.CDCANDISTR = T0.CDCANDISTR AND ");
                    sqlQuery.Append("       T2.CDCLIH IN (T0.CDCLIN4,T0.CDCLIN5,T0.CDCLIN6,T0.CDCLI) AND ");
                    sqlQuery.Append("          T2.CDGRPPRD = T4.CDGRPPRD AND ");
                    sqlQuery.Append("       ? BETWEEN T2.DTVIGINI AND T2.DTVIGFIM ");
                    sqlQuery.Append("  LEFT JOIN BRK_EPRECOA924 T3 ");
                    sqlQuery.Append("       ON T3.CDAPLICAC = 'V' AND T3.CDCONDTYP = 'X066' AND ");
                    sqlQuery.Append("          T3.CDGER0 = T0.CDGER0 AND T3.CDCANDISTR = T0.CDCANDISTR AND ");
                    sqlQuery.Append("       T3.CDCLIH IN (T0.CDCLIN4,T0.CDCLIN5,T0.CDCLIN6,T0.CDCLI) AND ");
                    sqlQuery.Append("          T3.CDMERCADO IN (T4.CDMERCADO, '') AND T3.CDNEGOCIO IN (T4.CDNEGOCIO, '') AND ");
                    sqlQuery.Append("          T3.CDCATEG IN (T4.CDCATEG, '') AND T3.CDSUBCATEG IN (T4.CDSUBCATEG, '') AND ");
                    sqlQuery.Append("          T3.CDSEGMENTO IN (T4.CDSEGMENTO, '') AND ");
                    sqlQuery.Append("       ? BETWEEN T3.DTVIGINI AND T3.DTVIGFIM ");
                    sqlQuery.Append(" WHERE T0.CDCLI = ? ");
                    sqlQuery.Append("   AND T0.CDGER0 = ? ");
                    sqlQuery.Append(" ORDER BY CDCLIH_906, CDCLIH_905, CDCLIH_924, T1.NMCONDREC DESC, T2.NMCONDREC DESC, IFNULL(T3.CDSEGMENTO,' ') DESC, IFNULL(T3.CDSUBCATEG,' ') DESC,IFNULL(T3.CDCATEG,' ') DESC,IFNULL(T3.CDNEGOCIO,' ') DESC,IFNULL(T3.CDMERCADO,' ') DESC,IFNULL(T3.NMCONDREC,' ') DESC ");
                    //sqlQuery.Append(" ORDER BY CDCLIH_906, CDCLIH_905, CDCLIH_924, T1.NMCONDREC DESC, T2.NMCONDREC DESC, T3.NMCONDREC DESC ");

                    SQLiteParameter[] parametros = new SQLiteParameter[06];

                    parametros[00] = new SQLiteParameter("@01PRODUTO", COD_PRODUTO.ToString().PadLeft(18, '0'));
                    parametros[01] = new SQLiteParameter("@02DATA", CSEmpresa.Current.DATA_ENTREGA.ToString("yyyy-MM-dd 00:00:00"));
                    parametros[02] = new SQLiteParameter("@03DATA", CSEmpresa.Current.DATA_ENTREGA.ToString("yyyy-MM-dd 00:00:00"));
                    parametros[03] = new SQLiteParameter("@04DATA", CSEmpresa.Current.DATA_ENTREGA.ToString("yyyy-MM-dd 00:00:00"));
                    parametros[04] = new SQLiteParameter("@05CLIENTE_SHIPTO", CSPDVs.Current.COD_PDV.ToString().PadLeft(10, '0'));
                    parametros[05] = new SQLiteParameter("@06CDGER0", this.PRODUTO.GRUPO_COMERCIALIZACAO.COD_SETOR_BROKER);

                    decimal result = 0;

                    using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(true, sqlQuery.ToString(), parametros))
                    {
                        if (reader.Read())
                        {
                            if (reader.GetValue(0) != System.DBNull.Value)
                                result = decimal.Parse(reader.GetValue(0).ToString());
                            else if (reader.GetValue(1) != System.DBNull.Value)
                                result = decimal.Parse(reader.GetValue(1).ToString());
                            else if (reader.GetValue(2) != System.DBNull.Value)
                                result = decimal.Parse(reader.GetValue(2).ToString());
                        }

                        reader.Close();
                        reader.Dispose();
                    }

                    this.PRC_DESCONTO_MAXIMO = (result * -1) / 1000;

                    /* Retorna o desconto maximo entre a tabela de preço e o produto */
                    m_PRC_MAXIMO_DESCONTO = this.RetornaDescontoMaximo();

                    if (CSEmpresa.Current.IND_VALIDA_PCT_MAXIMO_DESCONTO == "S" &&
                            m_PRC_MAXIMO_DESCONTO > -1 &&
                            m_PRC_MAXIMO_DESCONTO < this.PRC_DESCONTO_MAXIMO)
                        this.PRC_DESCONTO_MAXIMO = m_PRC_MAXIMO_DESCONTO;

                }
                catch (Exception ex)
                {
                    CSGlobal.ShowMessage(ex.ToString());
                    throw new ApplicationException("Falha ao calcular desconto máximo!", ex);
                }
            }

            private decimal RetornaDescontoMaximo()
            {
                try
                {
                    decimal result = -1;

                    if (this.LOCK_QTD && CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2)
                    {
                        return result;
                    }
                    else
                    {
                        if (CSEmpresa.Current.IND_HIERARQUIA_DESCONTO_MAXIMO)
                        {
                            result = RetornaDescontoMaximoHierarquia();
                            return result;
                        }
                        else
                        {
                            try
                            {
                                StringBuilder sqlQuery = new StringBuilder();

                                SQLiteParameter pCOD_PRODUTO1 = new SQLiteParameter("@COD_PRODUTO1", this.PRODUTO.COD_PRODUTO);
                                SQLiteParameter pCONDPGTO = new SQLiteParameter("@CONDPGTO", CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO);
                                SQLiteParameter pCATEGORIA = new SQLiteParameter("@CATEGORIA", CSPDVs.Current.COD_CATEGORIA);
                                SQLiteParameter pCOD_GRUPO_PDV = new SQLiteParameter("@COD_GRUPO_PDV", CSPDVs.Current.COD_GRUPO);
                                SQLiteParameter pCOD_GRUPO_PRODUTO = new SQLiteParameter("@COD_GRUPO_PRODUTO", this.PRODUTO.GRUPO.COD_GRUPO);
                                SQLiteParameter pFAMILIA_PRODUTO = new SQLiteParameter("@FAMILIA_PRODUTO", this.PRODUTO.FAMILIA_PRODUTO.COD_FAMILIA_PRODUTO);
                                SQLiteParameter pSEGMENTO = new SQLiteParameter("@SEGMENTO", CSPDVs.Current.COD_SEGMENTACAO);
                                SQLiteParameter pUNIDADENEGOCIO = new SQLiteParameter("@UNIDADENEGOCIO", CSPDVs.Current.COD_UNIDADE_NEGOCIO);
                                SQLiteParameter pVENDEDOR = new SQLiteParameter("@VENDEDOR", CSEmpregados.Current.COD_EMPREGADO);
                                SQLiteParameter pCOD_PRODUTO2 = new SQLiteParameter("@COD_PRODUTO2", this.PRODUTO.COD_PRODUTO);
                                SQLiteParameter pTABELA_PRECO = new SQLiteParameter("@COD_TABELA_PRECO", this.COD_TABELA_PRECO);

                                sqlQuery.Length = 0;
                                sqlQuery.Append("   SELECT T13.PRC_MAXIMO_DESCONTO_TABPRE ");
                                sqlQuery.Append("     FROM PRODUTO T1, TABELA_PRECO_PRODUTO T13, TABELA_PRECO T2 ");
                                sqlQuery.Append("    WHERE T13.COD_PRODUTO = T1.COD_PRODUTO AND ");
                                sqlQuery.Append("          T13.COD_TABELA_PRECO = T2.COD_TABELA_PRECO AND ");
                                sqlQuery.Append("          T13.COD_TABELA_PRECO NOT IN (SELECT DISTINCT T14.COD_TABELA_PRECO ");
                                sqlQuery.Append("                                         FROM BLOQUEIO_TABELA_PRECO T14 ");
                                sqlQuery.Append(" 	                                     WHERE  T14.COD_TABELA_PRECO IN (SELECT COD_TABELA_PRECO FROM TABELA_PRECO_PRODUTO WHERE COD_PRODUTO = ?) AND ");
                                sqlQuery.Append("                                               T14.TIPO_BLOQUEIO = 'B' AND ");
                                sqlQuery.Append(" 	                                          ((T14.COD_TABELA_BLOQUEIO = 1 AND T14.COD_BLOQUEIO = ?) OR ");
                                sqlQuery.Append("                                              (T14.COD_TABELA_BLOQUEIO = 2 AND T14.COD_BLOQUEIO = ?)  OR ");
                                sqlQuery.Append("                                              (T14.COD_TABELA_BLOQUEIO = 3 AND T14.COD_BLOQUEIO = ? ) OR ");
                                sqlQuery.Append("                                              (T14.COD_TABELA_BLOQUEIO = 4 AND T14.COD_BLOQUEIO = ? AND ");
                                sqlQuery.Append("                                               T14.COD_SUB_GRUPO_TABELA_BLOQUEIO = ?) OR ");
                                sqlQuery.Append("                                              (T14.COD_TABELA_BLOQUEIO = 5 AND T14.COD_BLOQUEIO = ?) OR ");
                                sqlQuery.Append("                                              (T14.COD_TABELA_BLOQUEIO = 6 AND T14.COD_BLOQUEIO = ?) OR ");
                                sqlQuery.Append("                                              (T14.COD_TABELA_BLOQUEIO = 7 AND T14.COD_BLOQUEIO = ?))) ");
                                sqlQuery.Append("     AND T1.COD_PRODUTO = ? ");
                                sqlQuery.Append("     AND T13.PRC_MAXIMO_DESCONTO_TABPRE IS NOT NULL ");

                                if (CSEmpresa.Current.IND_VALIDA_DESCMAX_TABELA_PEDIDO)
                                {
                                    sqlQuery.Append("     AND T13.COD_TABELA_PRECO = ? ");
                                }
                                else
                                {
                                    sqlQuery.Append("     /* ? */ ");
                                }

                                sqlQuery.Append(" ORDER BY T13.PRC_MAXIMO_DESCONTO_TABPRE ASC ");

                                // Busca desconto maximo                                        
                                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString(), pCOD_PRODUTO1, pCONDPGTO, pCATEGORIA, pCOD_GRUPO_PDV, pCOD_GRUPO_PRODUTO, pFAMILIA_PRODUTO, pSEGMENTO, pUNIDADENEGOCIO, pVENDEDOR, pCOD_PRODUTO2, pTABELA_PRECO))
                                {
                                    if (sqlReader.Read())
                                    {
                                        // Preenche desconto maximo
                                        if (sqlReader.GetValue(0) != System.DBNull.Value)
                                            result = Convert.ToDecimal(sqlReader.GetValue(0));
                                    }

                                    sqlReader.Close();
                                    sqlReader.Dispose();

                                    if (result != -1)
                                    {
                                        if (this.PRODUTO.PRC_MAXIMO_DESCONTO < result)
                                            result = this.PRODUTO.PRC_MAXIMO_DESCONTO;
                                    }
                                    else
                                    {
                                        if (this.PRODUTO.PRC_MAXIMO_DESCONTO > 0)
                                            result = this.PRODUTO.PRC_MAXIMO_DESCONTO;
                                    }
                                }

                                return result;

                            }
                            catch (Exception ex)
                            {
                                CSGlobal.ShowMessage(ex.ToString());
                                throw new ApplicationException("Erro na busca do desconto maximo (Classe Item de pedido)", ex);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }

            private decimal RetornaDescontoMaximoHierarquia()
            {
                decimal result = -1;

                try
                {
                    StringBuilder sqlQuery = new StringBuilder();

                    SQLiteParameter pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", this.PRODUTO.COD_PRODUTO);
                    SQLiteParameter pTABELA_PRECO = new SQLiteParameter("@COD_TABELA_PRECO", this.COD_TABELA_PRECO);
                    SQLiteParameter pCATEGORIA = new SQLiteParameter("@CATEGORIA", CSPDVs.Current.COD_CATEGORIA);
                    SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", CSPDVs.Current.COD_PDV);

                    sqlQuery.Length = 0;
                    sqlQuery.Append("   SELECT T1.PCT_MAXIMO_DESCONTO ");
                    sqlQuery.Append("     FROM CONFIGURACAO_DESCONTO_MAXIMO T1 ");
                    sqlQuery.Append("    WHERE T1.COD_PRODUTO      IN (?, 0) AND ");
                    sqlQuery.Append("	       T1.COD_TABELA_PRECO IN (?, 0) AND ");
                    sqlQuery.Append("	       T1.COD_CATEGORIA    IN (?, 0) AND ");
                    sqlQuery.Append("	       T1.COD_PDV          IN (?, 0) ");
                    sqlQuery.Append(" ORDER BY T1.COD_PDV DESC, ");
                    sqlQuery.Append("          T1.COD_CATEGORIA DESC, ");
                    sqlQuery.Append("          T1.COD_TABELA_PRECO DESC, ");
                    sqlQuery.Append("          T1.COD_PRODUTO DESC ");

                    // Busca desconto maximo                                        
                    using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString(), pCOD_PRODUTO, pTABELA_PRECO, pCATEGORIA, pCOD_PDV))
                    {
                        if (sqlReader.Read())
                        {
                            // Preenche desconto maximo
                            if (sqlReader.GetValue(0) != System.DBNull.Value)
                                result = Convert.ToDecimal(sqlReader.GetValue(0));
                        }

                        sqlReader.Close();
                        sqlReader.Dispose();

                    }
                    return result;

                }
                catch (Exception ex)
                {
                    CSGlobal.ShowMessage(ex.ToString());
                    throw new ApplicationException("Erro na busca do desconto maximo (Hierarquia)", ex);
                }
            }


            public void SalvarProduto()
            {
                decimal lblValorAdicionalFinanceiro = 0;
                decimal txtDescIncond = 0;
                decimal txtQtdeInteiro = 0;
                int txtQtdeUnidade = 0;
                decimal txtQtdeInteiroIndenizacao = 0;
                int txtQtdeUnidadeIndenizacao = 0;
                decimal txtValorFinalItemIndenizacao = 0;
                decimal lblValorTotalItem = 0;
                decimal lblValorFinalItem = 0;
                decimal lblValorDescontoUnitario = 0;
                CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto tabelaPreco = new CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto();
                string lblSaldoEstoque = "";

                CSItemsPedido.CSItemPedido itempedido = null;
                CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto precoProduto = null;
                decimal valorVerbaExtra, valorVerbaNormal, valorDesconto;

                try
                {
                    if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.NOVO)
                        itempedido = new CSItemsPedido.CSItemPedido();
                    else
                        itempedido = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current;

                    if (itempedido.STATE == ObjectState.NOVO || itempedido.STATE == ObjectState.ALTERADO || itempedido.STATE == ObjectState.NOVO_ALTERADO)
                    {
                        // [ Guarda valores antes das alterações ]
                        valorVerbaExtra = itempedido.VLR_VERBA_EXTRA;
                        valorVerbaNormal = itempedido.VLR_VERBA_NORMAL;
                        valorDesconto = itempedido.VLR_DESCONTO;

                        // Guarda qual o produto atual
                        itempedido.PRODUTO = CSProdutos.Current;

                        // Preenche o percentual do adicional financeiro a partir do current que é usado como base para os outros items criados
                        itempedido.PRC_ADICIONAL_FINANCEIRO = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_ADICIONAL_FINANCEIRO;

                        // Preenche o valor adicional financeiro
                        itempedido.VLR_ADICIONAL_FINANCEIRO = lblValorAdicionalFinanceiro;

                        // Preenche o preço de desconto
                        itempedido.PRC_DESCONTO = txtDescIncond;

                        // Preenche a quantidade pedida
                        itempedido.QTD_PEDIDA_INTEIRA = txtQtdeInteiro;
                        itempedido.QTD_PEDIDA_UNIDADE = txtQtdeUnidade;

                        // Preenche a quantidade pedida de indenização
                        itempedido.QTD_INDENIZACAO_INTEIRA = txtQtdeInteiroIndenizacao;
                        itempedido.QTD_INDENIZACAO_UNIDADE = txtQtdeUnidadeIndenizacao;

                        if (itempedido.QTD_INDENIZACAO_INTEIRA != 0 || itempedido.QTD_INDENIZACAO_UNIDADE != 0)
                            //itempedido.VLR_INDENIZACAO_UNIDADE = decimal.Round(CSGlobal.StrToDecimal(txtValorFinalItemIndenizacao.Text) / (decimal)CSProdutos.Current.QTD_UNIDADE_MEDIDA, 4);
                            itempedido.VLR_INDENIZACAO_UNIDADE = txtValorFinalItemIndenizacao;
                        else
                            itempedido.VLR_INDENIZACAO_UNIDADE = 0;

                        // Preenche o valor total
                        itempedido.VLR_TOTAL_ITEM = lblValorTotalItem;

                        // Preenche o valor unitário
                        itempedido.VLR_ITEM_UNIDADE = lblValorFinalItem;

                        // Diferanca entre o valor de tabela e o valor informado 
                        itempedido.VLR_DESCONTO_UNITARIO = ((itempedido.PRODUTO.PRECOS_PRODUTO.Current.VLR_PRODUTO + (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_ADICIONAL_FINANCEIRO / 100)) * itempedido.PRC_DESCONTO);

                        //  o percentual do adicional sobre o valor unitario
                        itempedido.VLR_ADICIONAL_UNITARIO = lblValorAdicionalFinanceiro;

                        // Preenche o valor de desconto
                        // valor do desconto total * qtde unitaria geral
                        itempedido.VLR_DESCONTO = (itempedido.VLR_DESCONTO_UNITARIO * txtQtdeInteiro) + ((itempedido.VLR_DESCONTO_UNITARIO / CSProdutos.Current.QTD_UNIDADE_EMBALAGEM) * txtQtdeUnidade);

                        //itempedido.VLR_DESCONTO = CSGlobal.StrToDecimal(itempedido.VLR_DESCONTO.ToString(CSGlobal.DecimalStringFormat));

                        // [ Preenche o codigo da tabela escolhido ]
                        precoProduto = tabelaPreco;
                        itempedido.COD_TABELA_PRECO = precoProduto.COD_TABELA_PRECO;

                        // [ Calcula do valor da verba extra ]
                        itempedido.VLR_VERBA_EXTRA = (precoProduto.VLR_VERBA_EXTRA * itempedido.QTD_PEDIDA_TOTAL) / itempedido.PRODUTO.UNIDADES_POR_CAIXA;

                        if (CSEmpresa.Current.TIPO_CALCULO_VERBA != CSEmpresa.CALCULO_VERBA.NENHUM)
                        {
                            decimal valorSaldoDesconto = 0;
                            decimal valorVenda = 0;

                            // [ Pega o valor do produto vendido sem o adicional ]
                            valorVenda = itempedido.VLR_TOTAL_ITEM - itempedido.VLR_ADICIONAL_FINANCEIRO_TOTAL;

                            // [ Pega o saldo atual do vendedor ]
                            valorSaldoDesconto = CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO.VAL_SALDO_DESCONTO;

                            // [ Calcula o valor da verba normal ]
                            switch (CSEmpresa.Current.TIPO_CALCULO_VERBA)
                            {
                                case CSEmpresa.CALCULO_VERBA.PERCENTUAL_VALOR_PEDIDO:
                                    {
                                        valorVenda += itempedido.VLR_ADICIONAL_FINANCEIRO_TOTAL;

                                        // [ Valor da venda é menor que (preço tabela - % verba)? ]
                                        if (valorVenda < itempedido.VLR_TABELA_PRECO_ITEM_MENOS_PCT_VERBA_NORMAL &&
                                            !CSEmpresa.Current.IND_ATUALIZAR_VERBA_PED_ABAIXOMINIMO)
                                        {
                                            itempedido.VLR_VERBA_NORMAL = 0;

                                        }
                                        else
                                        {
                                            itempedido.VLR_VERBA_NORMAL = CSGlobal.Round((valorVenda * CSEmpresa.Current.PCT_VERBA_NORMAL) / 100, 2);
                                        }

                                        break;
                                    }

                                case CSEmpresa.CALCULO_VERBA.DIFERENCA_VALOR_TABELA:
                                    {
                                        itempedido.VLR_VERBA_NORMAL = CSGlobal.Round(valorVenda - itempedido.VLR_TABELA_PRECO_ITEM_MENOS_PCT_VERBA_NORMAL, 2);
                                        break;
                                    }
                            }

                            // [ Permite atualização do saldo para verba normal? ]
                            if (!CSEmpresa.Current.IND_VLR_VERBA_PEDIDO_NOVO_ATUSALDO)
                                itempedido.VLR_VERBA_NORMAL = 0;

                            // [ Atualiza saldo descontando valores anteriores ]
                            valorSaldoDesconto += itempedido.VLR_VERBA_NORMAL - valorVerbaNormal;

                            // [ Permite atualização do saldo para verba extra? ]
                            if (!CSEmpresa.Current.IND_VLR_VERBA_EXTRA_ATUSALDO)
                                itempedido.VLR_VERBA_EXTRA = 0;

                            // [ Atualiza saldo descontando valores anteriores ]
                            valorSaldoDesconto += itempedido.VLR_VERBA_EXTRA - valorVerbaExtra;

                            // [ Atualizou o saldo anteriormente? ]
                            if (CSPDVs.Current.PEDIDOS_PDV.Current.IND_VLR_DESCONTO_ATUSALDO)
                                valorSaldoDesconto += valorDesconto;

                            // [ Permite atualização do saldo para desconto? ]
                            if (CSEmpresa.Current.TIPO_CALCULO_VERBA != CSEmpresa.CALCULO_VERBA.DIFERENCA_VALOR_TABELA)
                            {
                                valorSaldoDesconto -= itempedido.VLR_DESCONTO;
                                CSPDVs.Current.PEDIDOS_PDV.Current.IND_VLR_DESCONTO_ATUSALDO = true;

                            }
                            else
                            {
                                CSPDVs.Current.PEDIDOS_PDV.Current.IND_VLR_DESCONTO_ATUSALDO = false;
                            }

                            // [ Atualiza o valor do saldo ]
                            CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO.VAL_SALDO_DESCONTO = valorSaldoDesconto;
                        }

                        itempedido.STATE = ObjectState.NOVO_ALTERADO;

                        if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.NOVO)
                        {
                            // Adiciona o item de pedido na coleção
                            CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Add(itempedido);
                            CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current = itempedido;
                        }

                        if (CSPDVs.Current.PEDIDOS_PDV.Current.STATE == ObjectState.SALVO)
                            CSPDVs.Current.PEDIDOS_PDV.Current.STATE = ObjectState.ALTERADO;

                        itempedido.AtualizaImagem();

                        // Recebe o valor de estoque ja calculado (lblSaldoEstoque.Text)
                        if (CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.IND_PRONTA_ENTREGA)
                            CSProdutos.Current.QTD_ESTOQUE_PRONTA_ENTREGA = GetQtdPedidaUnidadeEstoque(lblSaldoEstoque);
                        else
                            CSProdutos.Current.QTD_ESTOQUE = GetQtdPedidaUnidadeEstoque(lblSaldoEstoque);
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            private decimal GetQtdPedidaUnidadeEstoque(string Qtd)
            {
                try
                {
                    decimal quantidade;
                    string qtdUnidade = "";
                    int posBarra;
                    int posNegativo;
                    //Verifica se a quantidade e negativa
                    posNegativo = Qtd.IndexOf("-");
                    if (posNegativo > (-1))
                        Qtd = Qtd.Substring(1, Qtd.Length - 1);

                    string qtdInteira = Qtd;

                    posBarra = Qtd.IndexOf("/");
                    if (posBarra > (-1))
                    {
                        // Pega valor antes da barra
                        qtdInteira = Qtd.Substring(0, posBarra);
                        // PEga valor depois da barra
                        try
                        {
                            qtdUnidade = Qtd.Substring(posBarra + 1, Qtd.Length - posBarra);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            qtdUnidade = Qtd.Substring(posBarra + 1, Qtd.Length - (posBarra + 1));
                        }
                    }
                    else
                    {
                        qtdInteira = Qtd;

                        var qtds = Qtd.Split('/');
                        qtdInteira = qtds[0];
                        if (qtds.Length == 2)
                            qtdUnidade = qtds[1];
                        // estava com problema do sinal de (-)
                        if (qtdInteira == "0-")
                            qtdInteira = "0";
                    }

                    switch (CSProdutos.Current.COD_UNIDADE_MEDIDA)
                    {
                        case "CX":
                        case "DZ":
                            quantidade = (CSGlobal.StrToInt(qtdInteira) * CSProdutos.Current.QTD_UNIDADE_EMBALAGEM) + CSGlobal.StrToInt(qtdUnidade);
                            break;
                        default:
                            quantidade = CSGlobal.StrToDecimal(qtdInteira) + CSGlobal.StrToDecimal(qtdUnidade);
                            break;
                    }

                    //Verifica se a quantidade é uma quantidade negativa
                    if (posNegativo > (-1))
                        quantidade = quantidade * -1;

                    return quantidade;
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }

            #endregion
        }

        #endregion

    }
}
