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
using System.Collections.Generic;
#endif

namespace AvanteSales
{
    /// <summary>
    /// Summary description for CSEmpregados.
    /// </summary>
    public class CSMotivos : CollectionBase
    {
        #region [ Variaveis ]

        private static CSMotivos m_Items;
        private static List<CSMOTIVO_NAO_COMPRA_PRODUTO_INDICADO> m_ItemsMotivoIndicados;

        #endregion

        #region [ Propriedades ]

        // Retorna um array dos Motivos
        public static CSMotivos Items
        {
            get
            {
                if (m_Items == null)
                    m_Items = new CSMotivos();
                return m_Items;
            }
        }

        public static List<CSMOTIVO_NAO_COMPRA_PRODUTO_INDICADO> ItemsMotivoIndicados
        {
            get
            {
                if (m_ItemsMotivoIndicados == null)
                    m_ItemsMotivoIndicados = MotivoIndicados();

                return m_ItemsMotivoIndicados;
            }
        }

        #endregion

        #region [ Metodos ]

        public static List<CSMOTIVO_NAO_COMPRA_PRODUTO_INDICADO> MotivoIndicados()
        {
            List<CSMOTIVO_NAO_COMPRA_PRODUTO_INDICADO> Motivos = new List<CSMOTIVO_NAO_COMPRA_PRODUTO_INDICADO>();

            if (CSEmpresa.ColunaExiste("MOTIVO_NAO_COMPRA_PRODUTO_INDICADO", "COD_MOTIVO"))
            {
                string query = "SELECT COD_MOTIVO,DSC_MOTIVO FROM MOTIVO_NAO_COMPRA_PRODUTO_INDICADO";

                CSMOTIVO_NAO_COMPRA_PRODUTO_INDICADO motivo;

                using (SqliteDataReader reader = CSDataAccess.Instance.ExecuteReader(query.ToString()))
                {
                    while (reader.Read())
                    {
                        motivo = new CSMOTIVO_NAO_COMPRA_PRODUTO_INDICADO();
                        motivo.COD_MOTIVO = reader.GetInt32(0);
                        motivo.DSC_MOTIVO = reader.GetString(1);

                        Motivos.Add(motivo);
                    }
                }
            }

            return Motivos;
        }

        public CSMotivos()
        {

            try
            {
                string sqlQuery;
                sqlQuery = "SELECT COD_MOTIVO, COD_TIPO_MOTIVO, DSC_MOTIVO FROM MOTIVO WHERE DSC_MOTIVO IS NOT NULL";

                // Busca todos os PDVs
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery))
                {
                    while (sqlReader.Read())
                    {
                        CSMotivo motivo = new CSMotivo();
                        // Preenche a instancia da classe motivo
                        motivo.COD_MOTIVO = sqlReader.GetInt32(0);
                        motivo.COD_TIPO_MOTIVO = sqlReader.GetInt32(1);
                        motivo.DSC_MOTIVO = sqlReader.GetString(2);
                        // Adiciona a instancia da classe motivo na coleção de motivos
                        base.InnerList.Add(motivo);
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Erro na busca dos motivos", ex);
            }
        }

        public static CSMotivos.CSMotivo GetMotivo(int COD_MOTIVO)
        {
            CSMotivos.CSMotivo ret = null;

            foreach (CSMotivos.CSMotivo motivo in Items.InnerList)
            {
                if (motivo.COD_MOTIVO == COD_MOTIVO)
                {
                    ret = motivo;
                    break;
                }
            }
            return ret;
        }

        #endregion

        #region [ SubClasses ]

        public class CSTipoMotivo
        {
            public const int NAO_POSITIVACAO_CLIENTE = 2;
            public const int NAO_PESQUISA_MARKETING = 14;
            public const int NAO_PESQUISA_MERCADO = 17;
            public const int NAO_COMPRA_PRODUTOS_INDICADOS = 20;
        }

        /// <summary>
        /// Summary description for CSEmpregado.
        /// </summary>
        public class CSMotivo
        {
            #region [ Variaveis ]

            private int m_COD_MOTIVO;
            private int m_COD_TIPO_MOTIVO;
            private string m_DSC_MOTIVO;

            #endregion

            #region [ Propriedades ]

            /// <summary>
            /// Guarda o codigo do motivo
            /// </summary>
            public int COD_MOTIVO
            {
                get
                {
                    return m_COD_MOTIVO;
                }
                set
                {
                    m_COD_MOTIVO = value;
                }
            }

            /// <summary>
            /// Guarda o codigo do tipo do motivo
            /// </summary>
            public int COD_TIPO_MOTIVO
            {
                get
                {
                    return m_COD_TIPO_MOTIVO;
                }
                set
                {
                    m_COD_TIPO_MOTIVO = value;
                }
            }

            /// <summary>
            /// Guarda a descrição do motivo
            /// </summary>
            public string DSC_MOTIVO
            {
                get
                {
                    return m_DSC_MOTIVO;
                }
                set
                {
                    m_DSC_MOTIVO = value;
                }
            }

            #endregion

            #region [ Metodos ]

            public CSMotivo()
            {
            }

            #endregion
        }

        public class CSMOTIVO_NAO_COMPRA_PRODUTO_INDICADO
        {
            private int m_COD_MOTIVO;
            private string m_DSC_MOTIVO;

            public int COD_MOTIVO
            {
                get
                {
                    return m_COD_MOTIVO;
                }
                set
                {
                    m_COD_MOTIVO = value;
                }
            }

            public string DSC_MOTIVO
            {
                get
                {
                    return m_DSC_MOTIVO;
                }
                set
                {
                    m_DSC_MOTIVO = value;
                }
            }
        }
        #endregion
    }
}