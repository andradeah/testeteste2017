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
using System.Collections;
using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
using SQLiteCommand = Mono.Data.Sqlite.SqliteCommand;
using SQLiteDataAdapter = Mono.Data.Sqlite.SqliteDataAdapter;
using SQLiteException = Mono.Data.Sqlite.SqliteException;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteTransaction = Mono.Data.Sqlite.SqliteTransaction;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;

namespace AvanteSales.SystemFramework.CSPDV
{
    public class CSPDVEmails : CollectionBase
    {
        #region [ Variáveis ]

        private static CSPDVEmails m_Items;
        private static CSPDVEmails.CSPDVEmail m_Current;

        #endregion

        #region [ Propriedade ]

        public static CSPDVEmails.CSPDVEmail Current
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

        public static CSPDVEmails Items
        {
            get
            {
                m_Items = new CSPDVEmails();

                return m_Items;
            }
        }

        public CSPDVEmails.CSPDVEmail this[int Index]
        {
            get
            {
                return (CSPDVEmails.CSPDVEmail)this.InnerList[Index];
            }
        }

        #endregion

        public CSPDVEmails()
        {
            try
            {
                StringBuilder sqlQuery = new StringBuilder();
                sqlQuery.AppendLine("SELECT COD_SEQUENCIA ");
                sqlQuery.AppendLine(" ,PDV_EMAIL.COD_PDV                 ");
                sqlQuery.AppendLine(" ,PDV_EMAIL.COD_TIPO_EMAIL          ");
                sqlQuery.AppendLine(" ,TIPO_EMAIL.DSC_TIPO_EMAIL         ");
                sqlQuery.AppendLine(" ,PDV_EMAIL.DSC_EMAIL               ");
                sqlQuery.AppendLine(" ,PDV_EMAIL.IND_ALTERADO            ");
                sqlQuery.AppendLine("FROM PDV_EMAIL                      ");
                sqlQuery.AppendLine("JOIN TIPO_EMAIL                     ");
                sqlQuery.AppendLine("     ON PDV_EMAIL.[COD_TIPO_EMAIL] = TIPO_EMAIL.[COD_TIPO_EMAIL] ");
                sqlQuery.AppendFormat(" WHERE PDV_EMAIL.COD_PDV = {0} AND PDV_EMAIL.DSC_EMAIL IS NOT NULL", CSPDVs.Current.COD_PDV);

                using (SQLiteDataReader result = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString()))
                {
                    while (result.Read())
                    {
                        CSPDVEmail email = new CSPDVEmail();
                        email.COD_SEQUENCIA = result.GetInt32(0);
                        email.COD_PDV = result.GetInt32(1);
                        email.COD_TIPO_EMAIL = result.GetInt32(2);
                        email.DSC_TIPO_EMAIL = result.GetString(3);
                        email.DSC_EMAIL = result.GetString(4);
                        email.IND_ALTERADO = result.GetInt32(5) == 1 ? true : false;

                        base.InnerList.Add(email);
                    }

                    result.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao executar busca de e-mails do PDV.");
            }
        }

        public class CSTiposEmail
        {
            public int COD_TIPO { get; set; }
            public string DSC_TIPO { get; set; }

            private static List<CSTiposEmail> m_items;

            public static List<CSTiposEmail> Items
            {
                get
                {
                    if (m_items == null)
                        CarregarTipos();

                    return m_items;
                }
                set
                {
                    m_items = value;
                }
            }

            private static void CarregarTipos()
            {
                Items = new List<CSTiposEmail>();

                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT COD_TIPO_EMAIL,DSC_TIPO_EMAIL FROM TIPO_EMAIL");

                using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
                {
                    while (reader.Read())
                    {
                        CSTiposEmail tipo = new CSTiposEmail();
                        tipo.COD_TIPO = reader.GetInt32(0);
                        tipo.DSC_TIPO = reader.GetString(1);

                        Items.Add(tipo);
                    }
                }
            }
        }

        public class CSPDVEmail
        {
            #region [ Variáveis ]

            private int m_COD_SEQUENCIA;
            private int m_COD_PDV;
            private int m_COD_TIPO_EMAIL;
            private string m_DSC_TIPO_EMAIL;
            private string m_DSC_EMAIL;
            private bool m_IND_ALTERADO;

            #endregion

            #region [ Propriedades ]

            public int COD_SEQUENCIA
            {
                get
                {
                    return m_COD_SEQUENCIA;
                }
                set
                {
                    m_COD_SEQUENCIA = value;
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

            public int COD_TIPO_EMAIL
            {
                get
                {
                    return m_COD_TIPO_EMAIL;
                }
                set
                {
                    m_COD_TIPO_EMAIL = value;
                }
            }

            public string DSC_TIPO_EMAIL
            {
                get
                {
                    return m_DSC_TIPO_EMAIL;
                }
                set
                {
                    m_DSC_TIPO_EMAIL = value;
                }
            }

            public string DSC_EMAIL
            {
                get
                {
                    return m_DSC_EMAIL;
                }
                set
                {
                    m_DSC_EMAIL = value;
                }
            }

            public bool IND_ALTERADO
            {
                get
                {
                    return m_IND_ALTERADO;
                }
                set
                {
                    m_IND_ALTERADO = value;
                }
            }

            #endregion

            public CSPDVEmail()
            {

            }

            public void AdicionarEmail(CSPDVEmail novoEmail)
            {
                try
                {
                    int sequencia = 0;

                    StringBuilder sqlSequencia = new StringBuilder();
                    sqlSequencia.AppendFormat("SELECT MAX(COD_SEQUENCIA) FROM PDV_EMAIL WHERE COD_PDV = {0}", CSPDVs.Current.COD_PDV);

                    var result = CSDataAccess.Instance.ExecuteScalar(sqlSequencia.ToString());

                    if (string.IsNullOrEmpty(result.ToString()))
                        sequencia = 1;
                    else
                        sequencia = Convert.ToInt32(result) + 1;

                    StringBuilder sql = new StringBuilder();
                    sql.Append("INSERT INTO PDV_EMAIL(IND_ALTERADO,COD_SEQUENCIA,COD_PDV,COD_TIPO_EMAIL,DSC_EMAIL,DAT_ALTERACAO) VALUES ");
                    sql.AppendFormat("(1,{0} ", sequencia);
                    sql.AppendFormat(",{0},{1},'{2}',DATETIME('{3}'))", CSPDVs.Current.COD_PDV, novoEmail.COD_TIPO_EMAIL, novoEmail.DSC_EMAIL, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());
                }
                catch (Exception ex)
                {
                    throw new Exception("Erro na inclusão de e-mail.");
                }
            }

            public void EditarEmail()
            {
                try
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendFormat("UPDATE PDV_EMAIL SET IND_ALTERADO = 1,DSC_EMAIL = '{0}',COD_TIPO_EMAIL = {1},DAT_ALTERACAO = DATETIME('{2}') ",
                                                                                                                           Current.DSC_EMAIL,
                                                                                                                           Current.COD_TIPO_EMAIL,
                                                                                                                           DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    sql.AppendFormat(" WHERE COD_PDV = {0} AND COD_SEQUENCIA = {1}", Current.COD_PDV, COD_SEQUENCIA);

                    CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());
                }
                catch (Exception ex)
                {
                    throw new Exception("Erro na edição de e-mail.");
                }
            }
        }
    }
}