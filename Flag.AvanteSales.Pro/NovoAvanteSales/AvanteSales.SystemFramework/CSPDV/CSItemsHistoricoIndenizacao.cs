using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
using SQLiteCommand = Mono.Data.Sqlite.SqliteCommand;
using SQLiteDataAdapter = Mono.Data.Sqlite.SqliteDataAdapter;
using SQLiteException = Mono.Data.Sqlite.SqliteException;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteTransaction = Mono.Data.Sqlite.SqliteTransaction;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;
using Mono.Data.Sqlite;
using System.Collections;

namespace AvanteSales.SystemFramework.CSPDV
{
    public class CSItemsHistoricoIndenizacao : CollectionBase, IDisposable
    {
        private CSItemsHistoricoIndenizacao.CSItemHistoricoIndenizacao m_Current;
        private int m_COD_INDENIZACAO;
        private bool _disposed = true;

        public CSItemsHistoricoIndenizacao Items
        {
            get
            {
                if (_disposed)
                    HistoricoItensIndenizacao();

                return this;
            }
        }

        public CSItemsHistoricoIndenizacao.CSItemHistoricoIndenizacao this[int Index]
        {
            get
            {
                try
                {
                    if (_disposed)
                        HistoricoItensIndenizacao();
                    return (CSItemsHistoricoIndenizacao.CSItemHistoricoIndenizacao)this.InnerList[Index];
                }
                catch (Exception ex)
                {
                    CSGlobal.ShowMessage(ex.ToString());
                    throw new ApplicationException("Erro na coleção de itens de indenização", ex);
                }

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

        public CSItemsHistoricoIndenizacao(int COD_INDENIZACAO)
        {
            this.m_COD_INDENIZACAO = COD_INDENIZACAO;

            HistoricoItensIndenizacao();
        }

        private void HistoricoItensIndenizacao()
        {
            string sql = string.Format(@"SELECT  T1.COD_PRODUTO,
                                                 T1.COD_MOTIVO_INDENIZACAO_ELETRONICA,        
                                                 T1.QTD_ITEM_INDENIZACAO_ELETRONICA,        
                                                 T1.VLR_TAXA_INDENIZACAO_ELETRONICA,        
                                                 T1.VLR_ITEM_INDENIZACAO_ELETRONICA
                                         FROM ITEM_INDENIZACAO_ELETRONICA T1
                                         JOIN PRODUTO T2
                                                ON T1.COD_PRODUTO = T2.COD_PRODUTO
                                         WHERE COD_INDENIZACAO_ELETRONICA = {0} AND T2.IND_ATIVO = 'A'", this.COD_INDENIZACAO);

            using (SqliteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql))
            {
                while (sqlReader.Read())
                {
                    CSItemHistoricoIndenizacao itemIndenizacao = new CSItemHistoricoIndenizacao();

                    itemIndenizacao.COD_INDENIZACAO = this.COD_INDENIZACAO;
                    itemIndenizacao.COD_PRODUTO = sqlReader.GetInt32(0);
                    itemIndenizacao.MOTIVO_INDENIZACAO = sqlReader.GetInt32(1);
                    itemIndenizacao.PRODUTO = CSProdutos.GetProduto(itemIndenizacao.COD_PRODUTO);
                    itemIndenizacao.QTD_INDENIZACAO = sqlReader.GetInt32(2);
                    itemIndenizacao.PCT_TAXA_INDENIZACAO = sqlReader.GetDecimal(3);
                    itemIndenizacao.VLR_INDENIZACAO = sqlReader.GetDecimal(4);

                    this.InnerList.Add(itemIndenizacao);
                }

                sqlReader.Close();
            }

            _disposed = false;
        }

        public class CSItemHistoricoIndenizacao
        {
            private int m_COD_INDENIZACAO;
            private int m_COD_PRODUTO;
            private int m_MOTIVO_INDENIZACAO;
            private decimal m_QTD_INDENIZACAO;
            private decimal m_QTD_INDENIZACAO_INTEIRA;
            private int m_QTD_INDENIZACAO_UNIDADE;
            private decimal m_PCT_TAXA_INDENIZACAO;
            private decimal m_VLR_INDENIZACAO;
            private CSProdutos.CSProduto m_PRODUTO;

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

                    if (decimal.TryParse(value.ToString(), out valueConvertido))
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
        }

        public void Dispose()
        {
            this.InnerList.Clear();
            this.InnerList.TrimToSize();
        }
    }
}