using System;
using System.Collections;
using System.Data;
#if ANDROID
using Android.Graphics;
using Mono.Data.Sqlite;
#else
using System.Data.SQLite;
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
    public class CSTiposDistribPolicitcaPrecos : CollectionBase
    {
        #region [ Variaveis ]

        private static CSTiposDistribPolicitcaPrecos items;
        private static CSTiposDistribPolicitcaPrecos.CSTipoDistribPolicitcaPreco current;

        #endregion

        #region [ Propriedades ]

        public static CSTiposDistribPolicitcaPrecos Items
        {
            get
            {
                //if (items == null)
                items = new CSTiposDistribPolicitcaPrecos();
                return items;
            }
        }

        public CSTiposDistribPolicitcaPrecos.CSTipoDistribPolicitcaPreco this[int Index]
        {
            get
            {
                return (CSTiposDistribPolicitcaPrecos.CSTipoDistribPolicitcaPreco)this.InnerList[Index];
            }
        }

        public static CSTiposDistribPolicitcaPrecos.CSTipoDistribPolicitcaPreco Current
        {
            get
            {
                if (current == null)
                    current = GetTipoDistribPolicitcaPreco(GetPoliticaPreco());

                return current;
            }
            set
            {
                current = value;
            }
        }

        #endregion

        #region [ Metodos ]

        /// <summary>
        /// Contrutor da classe. Busca as politicas
        /// </summary>
        public CSTiposDistribPolicitcaPrecos()
        {
            try
            {
                string sqlQuery =
                    "SELECT COD_TIPO_DISTRIBUICAO_POLITICA, DSC_TIPO_DISTRIBUICAO_POLITICA " +
                    "  FROM TIPO_DISTRIB_POLITICA_PRECO";

                // Busca todas as politicas
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery))
                {
                    while (sqlReader.Read())
                    {
                        CSTipoDistribPolicitcaPreco dispol = new CSTipoDistribPolicitcaPreco();

                        dispol.COD_TIPO_DISTRIBUICAO_POLITICA = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        dispol.DSC_TIPO_DISTRIBUICAO_POLITICA = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1);

                        base.InnerList.Add(dispol);
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca das distribuições politicas", ex);
            }
        }

        public static CSTipoDistribPolicitcaPreco GetTipoDistribPolicitcaPreco(int COD_TIPO_DISTRIBUICAO_POLITICA)
        {
            CSTipoDistribPolicitcaPreco ret = null;

            foreach (CSTipoDistribPolicitcaPreco dispol in Items.InnerList)
            {
                if (dispol.COD_TIPO_DISTRIBUICAO_POLITICA == COD_TIPO_DISTRIBUICAO_POLITICA)
                {
                    ret = dispol;
                    break;
                }
            }

            if (ret == null)
            {
                CSTipoDistribPolicitcaPreco politicaPreco = new CSTipoDistribPolicitcaPreco();
                politicaPreco.COD_TIPO_DISTRIBUICAO_POLITICA = COD_TIPO_DISTRIBUICAO_POLITICA;
                politicaPreco.DSC_TIPO_DISTRIBUICAO_POLITICA = GetDescricaoPolitica(COD_TIPO_DISTRIBUICAO_POLITICA);

                ret = politicaPreco;
            }
            return ret;
        }

        private static string GetDescricaoPolitica(int COD_TIPO_DISTRIBUICAO_POLITICA)
        {
            string retorno;

            if (COD_TIPO_DISTRIBUICAO_POLITICA.ToString().Contains("3"))
                retorno = "BUNGE";
            else
                retorno = "BROKER";

            return retorno;
        }

        public static int GetPoliticaPreco()
        {
            string sqlQueryPolFlexx;
            string sqlQueryPolBroker;
            string sqlQueryPolMista;
            string sqlQueryPolBunge;

            int qtdPolFlexx = 0;
            int qtdPolBroker = 0;
            int qtdPolBunge = 0;
            bool polMista = false;

            int Somapolitica = 0;
            int politicaPreco = 0;

            sqlQueryPolFlexx = "SELECT COUNT(COD_PRODUTO) AS QTDPOLFLEXX FROM PRODUTO WHERE COD_TIPO_DISTRIBUICAO_POLITICA=1 AND IND_ATIVO = 'A'";
            sqlQueryPolBroker = "SELECT COUNT(COD_PRODUTO) AS QTDPOLBROKER FROM PRODUTO WHERE COD_TIPO_DISTRIBUICAO_POLITICA=2 AND IND_ATIVO = 'A'";
            sqlQueryPolBunge = "SELECT COUNT(COD_PRODUTO) AS QTDPOLBROKER FROM PRODUTO WHERE COD_TIPO_DISTRIBUICAO_POLITICA=3 AND IND_ATIVO = 'A'";
            sqlQueryPolMista = "SELECT IND_POLITICA_CALCULO_PRECO_MISTA FROM EMPRESA";

            try
            {
                // Busca todas as politicas
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQueryPolFlexx))
                {
                    while (sqlReader.Read())
                    {
                        qtdPolFlexx = (sqlReader.GetValue(0) == System.DBNull.Value ? 1 : sqlReader.GetInt32(0));

                        if (qtdPolFlexx > 0)
                            Somapolitica += 100;
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
                // Busca todas as politicas
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQueryPolBroker))
                {
                    while (sqlReader.Read())
                    {
                        qtdPolBroker = (sqlReader.GetValue(0) == System.DBNull.Value ? 0 : sqlReader.GetInt32(0));

                        if (qtdPolBroker > 0)
                            Somapolitica += 20;
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }

                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQueryPolBunge))
                {
                    while (sqlReader.Read())
                    {
                        qtdPolBunge = (sqlReader.GetValue(0) == System.DBNull.Value ? 0 : sqlReader.GetInt32(0));

                        if (qtdPolBunge > 0)
                            Somapolitica += 3;
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }

                // Busca status de politica mista 
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQueryPolMista))
                {
                    while (sqlReader.Read())
                    {
                        polMista = (sqlReader.GetValue(0) == System.DBNull.Value ? false : (sqlReader.GetString(0).ToLower() == "s"));

                        if (polMista)
                            Somapolitica += 9000;
                    }
                    //if (polMista)
                    //    qtdPolBroker += qtdPolFlexx;

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro ao buscar politica de preço padrão.", ex);
            }

            if (Somapolitica < 9000)
            {
                if (qtdPolFlexx > qtdPolBroker &&
                    qtdPolFlexx > qtdPolBunge)
                    politicaPreco = 1;
                else if (qtdPolBroker > qtdPolFlexx &&
                    qtdPolBroker > qtdPolBunge)
                    politicaPreco = 2;
                else
                    politicaPreco = 3;
            }
            else
                politicaPreco = Somapolitica;

            //return qtdPolFlexx > qtdPolBroker ? 1 : 2;

            return politicaPreco;
        }

        #endregion

        #region [ SubClasses ]

        public class CSTipoDistribPolicitcaPreco
        {
            #region [ Variaveis ]

            private int m_COD_TIPO_DISTRIBUICAO_POLITICA;
            private string m_DSC_TIPO_DISTRIBUICAO_POLITICA;

            #endregion

            #region [ Propriedades ]

            public int COD_TIPO_DISTRIBUICAO_POLITICA
            {
                get
                {
                    return m_COD_TIPO_DISTRIBUICAO_POLITICA;
                }
                set
                {
                    m_COD_TIPO_DISTRIBUICAO_POLITICA = value;
                }
            }

            public string DSC_TIPO_DISTRIBUICAO_POLITICA
            {
                get
                {
                    return m_DSC_TIPO_DISTRIBUICAO_POLITICA;
                }
                set
                {
                    m_DSC_TIPO_DISTRIBUICAO_POLITICA = value.Trim();
                }
            }

            #endregion

            #region [ Metodos ]

            public CSTipoDistribPolicitcaPreco()
            {
            }

            #endregion
        }

        #endregion
    }
}