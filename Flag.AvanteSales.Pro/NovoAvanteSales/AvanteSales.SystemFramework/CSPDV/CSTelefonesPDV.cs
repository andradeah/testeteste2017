#region Using directives
using System.Collections.Generic;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlTypes;
using System.Reflection;
using System.Xml;
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
    public class CSTelefonesPDV : CollectionBase
    {
        #region [ Variaveis ]

        private static CSTelefonesPDV.CSTelefonePDV m_Current;

        #endregion

        #region [ Propriedades ]

        public static CSTelefonesPDV.CSTelefonePDV Current
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

        /// <summary>
        /// Retorna coleção dos telefones do PDV 
        /// </summary>
        public CSTelefonesPDV Items
        {
            get
            {
                return this;
            }
        }

        public CSTelefonesPDV.CSTelefonePDV this[int Index]
        {
            get
            {
                return (CSTelefonesPDV.CSTelefonePDV)this.InnerList[Index];
            }
        }

        #endregion

        #region [ Metodos ]

        /// <summary>
        /// Contrutor da classe. Busca os telefones do PDV
        /// </summary>
        public CSTelefonesPDV(int COD_PDV)
        {
            try
            {
                string sqlQuery =
                    "SELECT TP.COD_TIPO_TELEFONE, TP.NUM_DDD_TELEFONE " +
                    "      ,TP.NUM_TELEFONE, TT.DSC_TIPO_TELEFONE " +
                    "  FROM TELEFONE_PDV TP " +
                    " INNER JOIN TIPO_TELEFONE TT " +
                    "    ON TP.COD_TIPO_TELEFONE = TT.COD_TIPO_TELEFONE " +
                    " WHERE TP.COD_PDV = ? " +
                    " ORDER BY TP.COD_TIPO_TELEFONE ";

                SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", COD_PDV);

                // Busca todos os telefones do PDV
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_PDV))
                {

                    while (sqlReader.Read())
                    {
                        CSTelefonePDV telPDV = new CSTelefonePDV();
                        // Preenche a instancia da classe de telefone do pdv
                        telPDV.COD_TIPO_TELEFONE = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        telPDV.NUM_DDD_TELEFONE = sqlReader.GetValue(1) == System.DBNull.Value ? -1 : sqlReader.GetInt32(1);
                        telPDV.NUM_TELEFONE = sqlReader.GetValue(2) == System.DBNull.Value ? "" : sqlReader.GetString(2);
                        telPDV.DSC_TIPO_TELEFONE = sqlReader.GetValue(3) == System.DBNull.Value ? "" : sqlReader.GetString(3);
                        // Adciona o telefone do PDV na coleção de telefones deste PDV
                        base.InnerList.Add(telPDV);
                    }
                    // Fecha o reader
                    sqlReader.Close();
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca dos telefones do PDV", ex);
            }
        }

        #endregion

        #region [ SubClasses ]

        public class CSTipoTelefone
        {
            private int m_COD_TIPO_TELEFONE;
            private string m_DSC_TIPO_TELEFONE;
            private static List<CSTipoTelefone> m_Items;

            public static List<CSTipoTelefone> Items
            {
                get
                {
                    if (m_Items == null)
                        CarregarTipos();

                    return m_Items;
                }
                set
                {
                    m_Items = value;
                }
            }

            public int COD_TIPO_TELEFONE
            {
                get
                {
                    return m_COD_TIPO_TELEFONE;
                }
                set
                {
                    m_COD_TIPO_TELEFONE = value;
                }
            }

            public string DSC_TIPO_TELEFONE
            {
                get
                {
                    return m_DSC_TIPO_TELEFONE;
                }
                set
                {
                    m_DSC_TIPO_TELEFONE = value;
                }
            }

            private static void CarregarTipos()
            {
                Items = new List<CSTipoTelefone>();

                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT COD_TIPO_TELEFONE,DSC_TIPO_TELEFONE FROM TIPO_TELEFONE");

                using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
                {
                    while (reader.Read())
                    {
                        CSTipoTelefone tipo = new CSTipoTelefone();
                        tipo.COD_TIPO_TELEFONE = reader.GetInt32(0);
                        tipo.DSC_TIPO_TELEFONE = reader.GetString(1);

                        Items.Add(tipo);
                    }
                }
            }
        }

        /// <summary>
        /// Guarda a informaçõa sobre telefone do PDB
        /// </summary>
        public class CSTelefonePDV
        {
            #region [ Variaveis ]

            private int m_COD_TIPO_TELEFONE;
            private int m_NUM_DDD_TELEFONE;
            private string m_NUM_TELEFONE;
            private string m_DSC_TIPO_TELEFONE;

            #endregion

            #region [ Propriedades ]

            /// <summary>
            /// Guarda o codigo do endereço
            /// </summary>
            public int COD_TIPO_TELEFONE
            {
                get
                {
                    return m_COD_TIPO_TELEFONE;
                }
                set
                {
                    m_COD_TIPO_TELEFONE = value;
                }
            }

            /// <summary>
            /// Guarda o codigo do endereço
            /// </summary>
            public int NUM_DDD_TELEFONE
            {
                get
                {
                    return m_NUM_DDD_TELEFONE;
                }
                set
                {
                    m_NUM_DDD_TELEFONE = value;
                }
            }

            /// <summary>
            /// Guarda o codigo do endereço
            /// </summary>
            public string NUM_TELEFONE
            {
                get
                {
                    return m_NUM_TELEFONE;
                }
                set
                {
                    m_NUM_TELEFONE = value;
                }
            }

            /// <summary>
            /// Guarda a descrição do tipo do telefone
            /// </summary>
            public string DSC_TIPO_TELEFONE
            {
                get
                {
                    return m_DSC_TIPO_TELEFONE;
                }
                set
                {
                    m_DSC_TIPO_TELEFONE = value;
                }
            }

            #endregion

            #region [ Metodos ]

            public CSTelefonePDV()
            {

            }

            #endregion

            public void AdicionarTelefone(CSTelefonePDV novoTelefone)
            {
                try
                {
                    StringBuilder sqlVerificacao = new StringBuilder();
                    sqlVerificacao.AppendFormat("SELECT * FROM TELEFONE_PDV WHERE COD_PDV = {0} AND COD_TIPO_TELEFONE = {1}",
                        CSPDVs.Current.COD_PDV,
                        novoTelefone.COD_TIPO_TELEFONE);

                    if (CSDataAccess.Instance.ExecuteReader(sqlVerificacao.ToString()).Read())
                        throw new Exception("É permitido apenas um telefone por tipo.");

                    StringBuilder sql = new StringBuilder();
                    sql.Append("INSERT INTO TELEFONE_PDV(IND_ALTERADO,COD_PDV,COD_TIPO_TELEFONE,NUM_DDD_TELEFONE,NUM_TELEFONE) VALUES ");
                    sql.AppendFormat("(1,{0} ", CSPDVs.Current.COD_PDV);
                    sql.AppendFormat(",{0},{1},'{2}')", novoTelefone.COD_TIPO_TELEFONE, novoTelefone.NUM_DDD_TELEFONE, novoTelefone.NUM_TELEFONE);

                    CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            public void EditarTelefone()
            {
                try
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendFormat("UPDATE TELEFONE_PDV SET IND_ALTERADO = 1,NUM_TELEFONE = '{0}',NUM_DDD_TELEFONE = {1}",
                                                                                                                           Current.NUM_TELEFONE,
                                                                                                                           Current.NUM_DDD_TELEFONE);
                    sql.AppendFormat(" WHERE COD_PDV = {0} AND COD_TIPO_TELEFONE = {1}", CSPDVs.Current.COD_PDV, Current.COD_TIPO_TELEFONE);

                    CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());
                }
                catch (Exception ex)
                {
                    throw new Exception("Erro na edição de telefone.");
                }
            }
        }

        #endregion
    }
}