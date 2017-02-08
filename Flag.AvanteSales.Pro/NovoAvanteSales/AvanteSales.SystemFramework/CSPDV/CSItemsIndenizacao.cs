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
    public class CSItemsIndenizacao : CollectionBase, IDisposable
    {
        private CSItemsIndenizacao.CSItemIndenizacao m_Current;
        private int m_COD_INDENIZACAO;
        private bool _disposed = true;

        public CSItemsIndenizacao Items
        {
            get
            {
                if (_disposed)
                    Refresh();

                return this;
            }
        }

        public CSItemsIndenizacao.CSItemIndenizacao this[int Index]
        {
            get
            {
                try
                {
                    if (_disposed)
                        Refresh();
                    return (CSItemsIndenizacao.CSItemIndenizacao)this.InnerList[Index];
                }
                catch (Exception ex)
                {
                    CSGlobal.ShowMessage(ex.ToString());
                    throw new ApplicationException("Erro na coleção de itens de indenização", ex);
                }

            }
        }

        public void Dispose()
        {
            try
            {
                string sqlQueryDelete = "DELETE FROM TMPITEMINDENIZACAO";

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

        public CSItemsIndenizacao.CSItemIndenizacao Current
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

        public int Add(CSItemIndenizacao itemIndenizacao)
        {
            return this.InnerList.Add(itemIndenizacao);
        }

        public void Flush()
        {
            try
            {
                StringBuilder sqlQueryInsert = new StringBuilder();
                StringBuilder sqlQueryDelete = new StringBuilder();
                StringBuilder sqlQueryDeleteTabelaTemporaria = new StringBuilder();
                StringBuilder sqlQueryDeleteItem = new StringBuilder();
                StringBuilder sqlQueryVerificarTemporaria = new StringBuilder();
                string sqlQueryDeleteItemIndenizacao = string.Empty;

                sqlQueryInsert.AppendLine("INSERT INTO ITEM_INDENIZACAO ");
                sqlQueryInsert.AppendLine("(COD_INDENIZACAO, ");
                sqlQueryInsert.AppendLine("COD_PRODUTO, ");
                sqlQueryInsert.AppendLine("MOTIVO_INDENIZACAO, ");
                sqlQueryInsert.AppendLine("QTD_INDENIZACAO, ");
                sqlQueryInsert.AppendLine("VOLUME_INDENIZACAO, ");
                sqlQueryInsert.AppendLine("PCT_TAXA_INDENIZACAO, ");
                sqlQueryInsert.AppendLine("VLR_INDENIZACAO, ");
                sqlQueryInsert.AppendLine("PESO, ");
                sqlQueryInsert.AppendLine("VLR_UNITARIO_INDENIZACAO) ");
                sqlQueryInsert.AppendLine("SELECT ");
                sqlQueryInsert.AppendLine("COD_INDENIZACAO, ");
                sqlQueryInsert.AppendLine("COD_PRODUTO, ");
                sqlQueryInsert.AppendLine("MOTIVO_INDENIZACAO, ");
                sqlQueryInsert.AppendLine("QTD_INDENIZACAO, ");
                sqlQueryInsert.AppendLine("VOLUME_INDENIZACAO, ");
                sqlQueryInsert.AppendLine("PCT_TAXA_INDENIZACAO, ");
                sqlQueryInsert.AppendLine("VLR_INDENIZACAO, ");
                sqlQueryInsert.AppendLine("PESO, ");
                sqlQueryInsert.AppendLine("VLR_UNITARIO_INDENIZACAO ");
                sqlQueryInsert.AppendLine("FROM TMPITEMINDENIZACAO");

                sqlQueryDelete.AppendLine("DELETE FROM ITEM_INDENIZACAO WHERE COD_INDENIZACAO = ?");

                sqlQueryDeleteItem.AppendLine("DELETE FROM ITEM_INDENIZACAO WHERE COD_INDENIZACAO = ? AND COD_PRODUTO = ?");

                sqlQueryDeleteTabelaTemporaria.AppendLine("DELETE FROM TMPITEMINDENIZACAO ");

                sqlQueryVerificarTemporaria.AppendFormat("SELECT COD_PRODUTO FROM TMPITEMINDENIZACAO WHERE COD_INDENIZACAO = {0}", CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.COD_INDENIZACAO);

                SqliteParameter pCOD_INDENIZACAO = new SQLiteParameter("@COD_INDENIZACAO", CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.COD_INDENIZACAO);

                if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current != null)
                {
                    SqliteParameter pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.PRODUTO.COD_PRODUTO));

                    if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.STATE != ObjectState.NOVO)
                        CSDataAccess.Instance.ExecuteNonQuery(sqlQueryDeleteItem.ToString(), pCOD_INDENIZACAO, pCOD_PRODUTO);
                }

                if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.STATE == ObjectState.DELETADO)
                    CSDataAccess.Instance.ExecuteNonQuery(sqlQueryDelete.ToString(), pCOD_INDENIZACAO);

                if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.STATE != ObjectState.DELETADO)
                {
                    using (SqliteDataReader sqlTmp = CSDataAccess.Instance.ExecuteReader(sqlQueryVerificarTemporaria.ToString()))
                    {
                        while (sqlTmp.Read())
                        {
                            sqlQueryDeleteItemIndenizacao = string.Format("DELETE FROM ITEM_INDENIZACAO WHERE COD_INDENIZACAO = {0} AND COD_PRODUTO = {1}", CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.COD_INDENIZACAO, sqlTmp.GetInt32(0));
                            CSDataAccess.Instance.ExecuteNonQuery(sqlQueryDeleteItemIndenizacao);
                        }
                    }

                    CSDataAccess.Instance.ExecuteNonQuery(sqlQueryInsert.ToString());
                    CSDataAccess.Instance.ExecuteNonQuery(sqlQueryDeleteTabelaTemporaria.ToString());
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro no flush dos items de indenização", ex);
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

        public CSItemsIndenizacao(int COD_INDENIZACAO)
        {
            this.COD_INDENIZACAO = COD_INDENIZACAO;

            if (COD_INDENIZACAO != -1)
                Refresh();
        }

        private void Refresh()
        {
            try
            {
                StringBuilder sql = new StringBuilder();

                sql.AppendLine("SELECT [ITEM_INDENIZACAO].COD_PRODUTO, [ITEM_INDENIZACAO].MOTIVO_INDENIZACAO, [ITEM_INDENIZACAO].QTD_INDENIZACAO, [ITEM_INDENIZACAO].VOLUME_INDENIZACAO, [ITEM_INDENIZACAO].PCT_TAXA_INDENIZACAO, [ITEM_INDENIZACAO].VLR_INDENIZACAO, [ITEM_INDENIZACAO].PESO, [ITEM_INDENIZACAO].VLR_UNITARIO_INDENIZACAO");
                sql.AppendLine("FROM [ITEM_INDENIZACAO] ");
                sql.AppendLine("JOIN [INDENIZACAO] ");
                sql.AppendLine("ON [ITEM_INDENIZACAO].COD_INDENIZACAO = [INDENIZACAO].COD_INDENIZACAO ");
                sql.AppendLine("WHERE [INDENIZACAO].COD_INDENIZACAO = ?");

                SqliteParameter pCOD_INDENIZACAO = new SQLiteParameter("@COD_INDENIZACAO", this.COD_INDENIZACAO);

                using (SqliteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString(), pCOD_INDENIZACAO))
                {
                    while (sqlReader.Read())
                    {
                        CSItemIndenizacao itemIndenizacao = new CSItemIndenizacao();

                        itemIndenizacao.COD_INDENIZACAO = this.COD_INDENIZACAO;
                        itemIndenizacao.COD_PRODUTO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        itemIndenizacao.MOTIVO_INDENIZACAO = sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1);
                        itemIndenizacao.VOLUME_INDENIZACAO = sqlReader.GetValue(3) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(3));
                        itemIndenizacao.PCT_TAXA_INDENIZACAO = sqlReader.GetValue(4) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(4));
                        itemIndenizacao.VLR_INDENIZACAO = sqlReader.GetValue(5) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(5));
                        itemIndenizacao.PESO = sqlReader.GetValue(6) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(6));
                        itemIndenizacao.VLR_UNITARIO_INDENIZACAO = sqlReader.GetValue(7) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(7));
                        itemIndenizacao.PRODUTO = CSProdutos.GetProduto(itemIndenizacao.COD_PRODUTO);
                        itemIndenizacao.QTD_INDENIZACAO = sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2);
                        itemIndenizacao.STATE = ObjectState.INALTERADO;

                        this.InnerList.Add(itemIndenizacao);
                    }

                    sqlReader.Close();
                }

                _disposed = false;
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new ApplicationException("Erro no Refresh do Collection ItemsIndenizacao", ex);
            }
        }

        public class CSItemIndenizacao
#if ANDROID
 : Java.Lang.Object
#endif
        {
            #region[ Variáveis ]

            private int m_COD_INDENIZACAO;
            private int m_COD_PRODUTO;
            private int m_MOTIVO_INDENIZACAO;
            private decimal m_QTD_INDENIZACAO;
            private decimal m_VOLUME_INDENIZACAO;
            private decimal m_PCT_TAXA_INDENIZACAO;
            private decimal m_VLR_INDENIZACAO;
            private decimal m_VLR_ITEM_UNIDADE;
            private decimal m_PESO;
            private decimal m_VLR_UNITARIO_INDENIZACAO;

            private ObjectState m_STATE;
            private CSProdutos.CSProduto m_PRODUTO;
            private decimal m_QTD_INDENIZACAO_INTEIRA;
            private int m_QTD_INDENIZACAO_UNIDADE;

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

            public int COD_PRODUTO
            {
                get
                {
                    return m_COD_PRODUTO;
                }
                set
                {
                    m_COD_PRODUTO = value;
                }
            }

            public int MOTIVO_INDENIZACAO
            {
                get
                {
                    return m_MOTIVO_INDENIZACAO;
                }
                set
                {
                    m_MOTIVO_INDENIZACAO = value;
                }
            }

            public decimal QTD_INDENIZACAO
            {
                get
                {
                    return m_QTD_INDENIZACAO;
                }
                set
                {
                    m_QTD_INDENIZACAO = value;

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
                                this.QTD_INDENIZACAO_UNIDADE = Convert.ToInt32(valueConvertido) % this.PRODUTO.QTD_UNIDADE_EMBALAGEM;
                                this.QTD_INDENIZACAO_INTEIRA = Convert.ToInt32((valueConvertido - this.QTD_INDENIZACAO_UNIDADE) / this.PRODUTO.QTD_UNIDADE_EMBALAGEM);
                                break;
                            default:
                                this.QTD_INDENIZACAO_INTEIRA = valueConvertido;
                                this.QTD_INDENIZACAO_UNIDADE = 0;
                                break;
                        }
                    }
                }
            }

            public decimal VOLUME_INDENIZACAO
            {
                get
                {
                    return m_VOLUME_INDENIZACAO;
                }
                set
                {
                    m_VOLUME_INDENIZACAO = value;
                }
            }

            public decimal PCT_TAXA_INDENIZACAO
            {
                get
                {
                    return m_PCT_TAXA_INDENIZACAO;
                }
                set
                {
                    m_PCT_TAXA_INDENIZACAO = value;
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

            public decimal PESO
            {
                get
                {
                    return m_PESO;
                }
                set
                {
                    m_PESO = value;
                }
            }

            public decimal VLR_UNITARIO_INDENIZACAO
            {
                get
                {
                    return m_VLR_UNITARIO_INDENIZACAO;
                }
                set
                {
                    m_VLR_UNITARIO_INDENIZACAO = value;
                }
            }

            #endregion

            public CSItemIndenizacao()
            {
                this.m_STATE = ObjectState.NOVO;
            }

            public void AtualizaImagem()
            {
                try
                {
                    StringBuilder sqlQueryInsert = new StringBuilder();

                    sqlQueryInsert.AppendLine("INSERT INTO TMPITEMINDENIZACAO ");
                    sqlQueryInsert.AppendLine("(COD_INDENIZACAO, ");
                    sqlQueryInsert.AppendLine("COD_PRODUTO, ");
                    sqlQueryInsert.AppendLine("MOTIVO_INDENIZACAO, ");
                    sqlQueryInsert.AppendLine("QTD_INDENIZACAO, ");
                    sqlQueryInsert.AppendLine("VOLUME_INDENIZACAO, ");
                    sqlQueryInsert.AppendLine("PCT_TAXA_INDENIZACAO, ");
                    sqlQueryInsert.AppendLine("VLR_INDENIZACAO, ");
                    sqlQueryInsert.AppendLine("PESO, ");
                    sqlQueryInsert.AppendLine("VLR_UNITARIO_INDENIZACAO)");
                    sqlQueryInsert.AppendLine("VALUES(?,?,?,?,?,?,?,?,?)");

                    SQLiteParameter pCOD_INDENIZACAO = new SQLiteParameter("@COD_INDENIZACAO", this.COD_INDENIZACAO);
                    SQLiteParameter pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", this.COD_PRODUTO);
                    SQLiteParameter pMOTIVO_INDENIZACAO = new SQLiteParameter("@MOTIVO_INDENIZACAO", this.MOTIVO_INDENIZACAO);
                    SQLiteParameter pQTD_INDENIZACAO = new SQLiteParameter("@QTD_INDENIZACAO", this.QTD_INDENIZACAO);
                    SQLiteParameter pVOLUME_INDENIZACAO = new SQLiteParameter("@VOLUME_INDENIZACAO", this.VOLUME_INDENIZACAO);
                    SQLiteParameter pPCT_TAXA_INDENIZACAO = new SQLiteParameter("@PCT_TAXA_INDENIZACAO", this.PCT_TAXA_INDENIZACAO);
                    SQLiteParameter pVLR_INDENIZACAO = new SQLiteParameter("@VLR_INDENIZACAO", this.VLR_INDENIZACAO);
                    SqliteParameter pPESO = new SQLiteParameter("@PESO", this.PESO);
                    SQLiteParameter pVLR_UNITARIO_INDENIZACAO = new SQLiteParameter("@VLR_UNITARIO_INDENIZACAO", this.VLR_UNITARIO_INDENIZACAO);

                    DeletarImagem();

                    CSDataAccess.Instance.ExecuteNonQuery(sqlQueryInsert.ToString(), pCOD_INDENIZACAO, pCOD_PRODUTO, pMOTIVO_INDENIZACAO, pQTD_INDENIZACAO, pVOLUME_INDENIZACAO, pPCT_TAXA_INDENIZACAO, pVLR_INDENIZACAO, pPESO, pVLR_UNITARIO_INDENIZACAO);
                }
                catch (Exception ex)
                {
                    CSGlobal.ShowMessage(ex.ToString());
                }
            }

            public void DeletarImagem()
            {
                StringBuilder sqlQueryDelete = new StringBuilder();

                sqlQueryDelete.AppendLine("DELETE FROM TMPITEMINDENIZACAO ");
                sqlQueryDelete.AppendLine("WHERE COD_PRODUTO = '" + this.COD_PRODUTO + "'");

                CSDataAccess.Instance.ExecuteNonQuery(sqlQueryDelete.ToString());
            }

            public CSPoliticaBroker2014.TmpPricingCons[] CalculaValor2014()
            {
                try
                {
                    CSPoliticaBroker2014.TmpPricingCons[] valor = CSPDVs.Current.POLITICA_BROKER_2014.CalculaPreco(this.PRODUTO.COD_PRODUTO, this.PRODUTO.GRUPO_COMERCIALIZACAO.COD_SETOR_BROKER, 1,
                               0, 0, this.PRODUTO.QTD_UNIDADE_EMBALAGEM);

                    this.VLR_INDENIZACAO = valor[valor.Length - 1].VALOR;
                    this.VLR_ITEM_UNIDADE = valor[valor.Length - 1].VALOR / ((this.QTD_INDENIZACAO_INTEIRA * this.PRODUTO.QTD_UNIDADE_EMBALAGEM) + this.QTD_INDENIZACAO_UNIDADE);

                    return valor;
                }
                catch (Exception e)
                {
                    CSGlobal.ShowMessage("###CalculaValor: " + e.Message);
                    return null;
                }
            }

            public CSPoliticaBroker.TmpPricingCons[] CalculaValor()
            {
                try
                {
                    //bool qtdZero = (this.QTD_INDENIZACAO_INTEIRA == 0 && this.QTD_INDENIZACAO_UNIDADE == 0);
                    ////decimal m_PRC_MAXIMO_DESCONTO = 0;

                    //if (qtdZero)
                    //    this.QTD_INDENIZACAO_INTEIRA = 1;

                    CSPoliticaBroker.TmpPricingCons[] valor = CSPDVs.Current.POLITICA_BROKER.CalculaPreco(this.PRODUTO.COD_PRODUTO, this.PRODUTO.GRUPO_COMERCIALIZACAO.COD_SETOR_BROKER, 1,
                               0, 0, this.PRODUTO.QTD_UNIDADE_EMBALAGEM);

                    this.VLR_INDENIZACAO = valor[valor.Length - 1].VALOR;
                    this.VLR_ITEM_UNIDADE = valor[valor.Length - 1].VALOR / ((this.QTD_INDENIZACAO_INTEIRA * this.PRODUTO.QTD_UNIDADE_EMBALAGEM) + this.QTD_INDENIZACAO_UNIDADE);

                    //this.VLR_ITEM_INTEIRA = (valor[valor.Length - 1].VALOR * this.PRODUTO.QTD_UNIDADE_EMBALAGEM) / this.QTD_PEDIDA_TOTAL;

                    //this.PRC_DESCONTO_MAXIMO = Math.Abs(CSPDVs.Current.POLITICA_BROKER.CalculaDescontoMaximo());

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
                    //m_PRC_MAXIMO_DESCONTO = this.RetornaDescontoMaximo();

                    //if (CSEmpresa.Current.IND_VALIDA_PCT_MAXIMO_DESCONTO == "S" &&
                    //m_PRC_MAXIMO_DESCONTO > -1 &&
                    //m_PRC_MAXIMO_DESCONTO < this.PRC_DESCONTO_MAXIMO)
                    //this.PRC_DESCONTO_MAXIMO = m_PRC_MAXIMO_DESCONTO;

                    //if (qtdZero)
                    //this.QTD_PEDIDA_INTEIRA = 0;

                    return valor;

                }
                catch (Exception e)
                {
                    CSGlobal.ShowMessage("###CalculaValor: " + e.Message);
                    return null;
                }
            }

            public void DeletarFlush()
            {
                StringBuilder sqlQueryDelete = new StringBuilder();

                sqlQueryDelete.AppendLine("DELETE FROM ITEM_INDENIZACAO ");
                sqlQueryDelete.AppendLine("WHERE COD_PRODUTO = '" + this.COD_PRODUTO + "'");

                CSDataAccess.Instance.ExecuteNonQuery(sqlQueryDelete.ToString());
            }
        }
    }
}