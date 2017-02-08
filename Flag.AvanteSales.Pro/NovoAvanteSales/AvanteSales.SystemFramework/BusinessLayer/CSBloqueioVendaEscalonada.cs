using System;
using System.Collections;
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

namespace AvanteSales
{
    public class CSBloqueiosVendasEscalonadas : CollectionBase
    {
        #region [ Variaveis ]

        private static CSBloqueiosVendasEscalonadas m_Items;

        #endregion

        #region [ Propriedades ]

        public static CSBloqueiosVendasEscalonadas Items
        {
            get
            {
                if (m_Items == null)
                    m_Items = new CSBloqueiosVendasEscalonadas();
                return m_Items;
            }
        }

        public CSBloqueiosVendasEscalonadas.CSBloqueioVendaEscalonada this[int Index]
        {
            get
            {
                return (CSBloqueiosVendasEscalonadas.CSBloqueioVendaEscalonada)this.InnerList[Index];
            }
        }

        #endregion

        #region [ Metodos ]

        public CSBloqueiosVendasEscalonadas()
        {
            try
            {
                string sqlQuery;
                sqlQuery = "SELECT COD_EMPREGADO, COD_PRODUTO, COD_SEGREGACAO, NUM_SEQUENCIA_ESCALONADA, QTD_FINAL_ESCALONADA, QTD_INICIAL_ESCALONADA, VLR_MINIMO_ESCALONADA FROM BLOQUEIO_VENDA_ESCALONADA ORDER BY QTD_INICIAL_ESCALONADA";

                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery))
                {
                    while (sqlReader.Read())
                    {
                        CSBloqueioVendaEscalonada blovendesc = new CSBloqueioVendaEscalonada();

                        blovendesc.COD_EMPREGADO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        blovendesc.COD_PRODUTO = sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1);
                        blovendesc.COD_SEGREGACAO = sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2);
                        blovendesc.NUM_SEQUENCIA_ESCALONADA = sqlReader.GetValue(3) == System.DBNull.Value ? -1 : sqlReader.GetInt32(3);
                        blovendesc.QTD_FINAL_ESCALONADA = sqlReader.GetValue(4) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(4));
                        blovendesc.QTD_INICIAL_ESCALONADA = sqlReader.GetValue(5) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(5));
                        blovendesc.VLR_MINIMO_ESCALONADA = sqlReader.GetValue(6) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(6));

                        base.InnerList.Add(blovendesc);
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos bloqueio da venda escalonada.", ex);
            }
        }

        /// <summary>
        /// Pega o preço que o produto deve ser vendido dependendo da quantidade pedida
        /// </summary>
        /// <param name="emp">Empregado que esta vendendo</param>
        /// <param name="prod">Produto que esta sendo vendido</param>
        /// <param name="QUANTIDADE_PEDIDA">Quantidades em unidades que esta sendo vendida</param>
        /// <returns>o valor da unidade para a quantidade que esta sendo vendida</returns>
        public static decimal GetFaixaEscalonada(CSProdutos.CSProduto prod, CSPDVs.CSPDV pdv, CSEmpregados.CSEmpregado emp, int QUANTIDADE_PEDIDA)
        {
            for (int I = 1; I <= 3; I++)
            {
                foreach (CSBloqueiosVendasEscalonadas.CSBloqueioVendaEscalonada blofxesc in Items)
                {
                    if ((blofxesc.COD_PRODUTO == prod.COD_PRODUTO) && (blofxesc.COD_SEGREGACAO == pdv.COD_SEGREGACAO))
                    {
                        switch (I)
                        {
                            case 1:// Empregado
                                if (blofxesc.COD_EMPREGADO == emp.COD_EMPREGADO)
                                {
                                    if ((QUANTIDADE_PEDIDA >= blofxesc.QTD_INICIAL_ESCALONADA) && (QUANTIDADE_PEDIDA <= blofxesc.QTD_FINAL_ESCALONADA))
                                    {
                                        return blofxesc.VLR_MINIMO_ESCALONADA;
                                    }
                                }
                                break;
                            case 2: // Supervisor
                                if (blofxesc.COD_EMPREGADO == emp.COD_SUPERVISOR)
                                {
                                    if ((QUANTIDADE_PEDIDA >= blofxesc.QTD_INICIAL_ESCALONADA) && (QUANTIDADE_PEDIDA <= blofxesc.QTD_FINAL_ESCALONADA))
                                    {
                                        return blofxesc.VLR_MINIMO_ESCALONADA;
                                    }
                                }
                                break;
                            case 3: // Gerente
                                if (blofxesc.COD_EMPREGADO == emp.COD_GERENTE)
                                {
                                    if ((QUANTIDADE_PEDIDA >= blofxesc.QTD_INICIAL_ESCALONADA) && (QUANTIDADE_PEDIDA <= blofxesc.QTD_FINAL_ESCALONADA))
                                    {
                                        return blofxesc.VLR_MINIMO_ESCALONADA;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            // Se não achar nada, pode qualquer valor
            return -1;
        }

        #endregion

        #region [ SubClasses ]

        public class CSBloqueioVendaEscalonada
        {
            #region [ Variaveis ]

            private int m_COD_PRODUTO;
            private int m_COD_EMPREGADO;
            private int m_COD_SEGREGACAO;
            private int m_NUM_SEQUENCIA_ESCALONADA;
            private decimal m_QTD_INICIAL_ESCALONADA;
            private decimal m_QTD_FINAL_ESCALONADA;
            private decimal m_VLR_MINIMO_ESCALONADA;

            #endregion

            #region [ Propriedades ]

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

            public int NUM_SEQUENCIA_ESCALONADA
            {
                get
                {
                    return m_NUM_SEQUENCIA_ESCALONADA;
                }
                set
                {
                    m_NUM_SEQUENCIA_ESCALONADA = value;
                }
            }

            public decimal QTD_INICIAL_ESCALONADA
            {
                get
                {
                    return m_QTD_INICIAL_ESCALONADA;
                }
                set
                {
                    m_QTD_INICIAL_ESCALONADA = value;
                }
            }

            public decimal QTD_FINAL_ESCALONADA
            {
                get
                {
                    return m_QTD_FINAL_ESCALONADA;
                }
                set
                {
                    m_QTD_FINAL_ESCALONADA = value;
                }
            }

            public decimal VLR_MINIMO_ESCALONADA
            {
                get
                {
                    return m_VLR_MINIMO_ESCALONADA;
                }
                set
                {
                    m_VLR_MINIMO_ESCALONADA = value;
                }
            }

            #endregion

            #region [ Metodos ]

            public CSBloqueioVendaEscalonada()
            {

            }

            #endregion
        }

        #endregion
    }
}