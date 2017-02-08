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

#endregion

namespace AvanteSales
{
    public class CSEnderecosPDV : CollectionBase
    {
        #region  variaveis

        public static bool bolExecutou = false;

        #endregion

        #region [ Metodos ]

        /// <summary>
        /// Contrutor da classe. Busca o Endereço do PDV
        /// </summary>
        public CSEnderecosPDV(int COD_PDV)
        {
            //SQLiteDataReader sqlReader;

            try
            {
                string sqlQuery =
                    "SELECT EP.COD_ENDERECO, EP.COD_UF, C.COD_CIDADE, C.DSC_CIDADE, EP.COD_BAIRRO, B.DSC_BAIRRO " +
                    "      ,EP.COD_TIPO_ENDERECO, T.DSC_TIPO_ENDERECO, EP.NUM_CEP, EP.DSC_LOGRADOURO_COMPLEMENTO " +
                    "  FROM ENDERECO_PDV EP  " +
                    "  JOIN PDV P " +
                    "    ON EP.COD_PDV = P.COD_PDV " +
                    "  JOIN TIPO_ENDERECO T " +
                    "    ON EP.COD_TIPO_ENDERECO = T.COD_TIPO_ENDERECO " +
                    "  JOIN CIDADE C " +
                    "    ON UPPER(EP.COD_UF) = UPPER(C.COD_UF) " +
                    "   AND EP.COD_CIDADE = C.COD_CIDADE " +
                    "  JOIN BAIRRO B " +
                    "    ON UPPER(EP.COD_UF) = UPPER(B.COD_UF) " +
                    "   AND EP.COD_CIDADE = B.COD_CIDADE " +
                    "   AND EP.COD_BAIRRO = B.COD_BAIRRO " +
                    " WHERE EP.COD_PDV = ? ";

                SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", COD_PDV);

                // Busca todos os PDVs
                //using (SQLiteDataReader sqlReader = !bolExecutou ? CSDataAccess.Instance.ExecuteReaderEndPDV(sqlQuery, pCOD_PDV) : CSDataAccess.Instance.ExecuteReaderEnderecos(pCOD_PDV))
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReaderEndPDV(sqlQuery, pCOD_PDV))
                {
                    while (sqlReader.Read())
                    {
                        CSEnderecosPDV.CSEnderecoPDV pdv = new CSEnderecoPDV();
                        // Preenche a instancia da classe pdv
                        pdv.COD_ENDERECO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                        pdv.COD_UF = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1);
                        pdv.DSC_UF = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1);
                        pdv.COD_CIDADE = sqlReader.GetValue(2) == System.DBNull.Value ? -1 : sqlReader.GetInt32(2);
                        pdv.DSC_CIDADE = sqlReader.GetValue(3) == System.DBNull.Value ? "" : sqlReader.GetString(3);
                        pdv.COD_BAIRRO = sqlReader.GetValue(4) == System.DBNull.Value ? -1 : sqlReader.GetInt32(4);
                        pdv.DSC_BAIRRO = sqlReader.GetValue(5) == System.DBNull.Value ? "" : sqlReader.GetString(5);
                        pdv.COD_TIPO_ENDERECO = sqlReader.GetValue(6) == System.DBNull.Value ? -1 : sqlReader.GetInt32(6);
                        pdv.DSC_TIPO_ENDERECO = sqlReader.GetValue(7) == System.DBNull.Value ? "" : sqlReader.GetString(7);
                        pdv.NUM_CEP = sqlReader.GetValue(8) == System.DBNull.Value ? "" : sqlReader.GetString(8);
                        pdv.DSC_LOGRADOURO_COMPLEMENTO = sqlReader.GetValue(9) == System.DBNull.Value ? "" : sqlReader.GetString(9);
                        // Adciona o endereco do PDV na coleção de enderecos deste PDV
                        base.InnerList.Add(pdv);

                        bolExecutou = true;
                    }
                    // Fecha o reader
                    sqlReader.Close();
                    //sqlReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na busca do endereço do PDV", ex);
            }
        }

        #endregion

        #region [ Propriedades ]

        /// <summary>
        /// Retorna coleção dos telefones do PDV 
        /// </summary>
        public CSEnderecosPDV Items
        {
            get
            {
                return this;
            }
        }

        public CSEnderecosPDV.CSEnderecoPDV this[int Index]
        {
            get
            {
                if (this.InnerList.Count > 0)
                    return (CSEnderecosPDV.CSEnderecoPDV)this.InnerList[Index];
                else
                    return null;
            }
        }

        #endregion

        #region [ SubClasses ]

        /// <summary>
        /// Guarda a informação sobre endereço do PDV
        /// </summary>
        public class CSEnderecoPDV
        {
            #region [ Variaveis ]

            private int m_COD_ENDERECO;
            private string m_COD_UF;
            private string m_DSC_UF;
            private int m_COD_CIDADE;
            private string m_DSC_CIDADE;
            private int m_COD_BAIRRO;
            private string m_DSC_BAIRRO;
            private int m_COD_TIPO_ENDERECO;
            private string m_DSC_LOGRADOURO_COMPLEMENTO;
            private string m_NUM_CEP;
            private string m_DSC_TIPO_ENDERECO;

            #endregion

            #region [ Propriedades ]

            /// <summary>
            /// Guarda o codigo do endereço
            /// </summary>
            public int COD_ENDERECO
            {
                get
                {
                    return m_COD_ENDERECO;
                }
                set
                {
                    m_COD_ENDERECO = value;
                }
            }

            /// <summary>
            /// Guarda o codigo do endereço
            /// </summary>
            public string COD_UF
            {
                get
                {
                    return m_COD_UF;
                }
                set
                {
                    m_COD_UF = value;
                }
            }

            /// <summary>
            /// Guarda o codigo do endereço
            /// </summary>
            public string DSC_UF
            {
                get
                {
                    return m_DSC_UF;
                }
                set
                {
                    m_DSC_UF = value.Trim();
                }
            }

            /// <summary>
            /// Guarda o codigo do endereço
            /// </summary>
            public int COD_CIDADE
            {
                get
                {
                    return m_COD_CIDADE;
                }
                set
                {
                    m_COD_CIDADE = value;
                }
            }

            /// <summary>
            /// Guarda o codigo do endereço
            /// </summary>
            public string DSC_CIDADE
            {
                get
                {
                    return m_DSC_CIDADE;
                }
                set
                {
                    m_DSC_CIDADE = value.Trim();
                }
            }

            /// <summary>
            /// Guarda o codigo do endereço
            /// </summary>
            public int COD_BAIRRO
            {
                get
                {
                    return m_COD_BAIRRO;
                }
                set
                {
                    m_COD_BAIRRO = value;
                }
            }

            /// <summary>
            /// Guarda o codigo do endereço
            /// </summary>
            public string DSC_BAIRRO
            {
                get
                {
                    return m_DSC_BAIRRO;
                }
                set
                {
                    m_DSC_BAIRRO = value.Trim();
                }
            }

            /// <summary>
            /// Guarda o codigo do endereço
            /// </summary>
            public int COD_TIPO_ENDERECO
            {
                get
                {
                    return m_COD_TIPO_ENDERECO;
                }
                set
                {
                    m_COD_TIPO_ENDERECO = value;
                }
            }

            /// <summary>
            /// Guarda o codigo do endereço
            /// </summary>
            public string DSC_LOGRADOURO_COMPLEMENTO
            {
                get
                {
                    return m_DSC_LOGRADOURO_COMPLEMENTO;
                }
                set
                {
                    m_DSC_LOGRADOURO_COMPLEMENTO = value.Trim();
                }
            }

            /// <summary>
            /// Guarda o codigo do endereço
            /// </summary>
            public string NUM_CEP
            {
                get
                {
                    return m_NUM_CEP;
                }
                set
                {
                    m_NUM_CEP = value.Trim();
                }
            }

            /// <summary>
            /// Guarda o codigo do endereço
            /// </summary>
            public string DSC_TIPO_ENDERECO
            {
                get
                {
                    return m_DSC_TIPO_ENDERECO;
                }
                set
                {
                    m_DSC_TIPO_ENDERECO = value.Trim();
                }
            }

            #endregion

            #region [ Metodos ]

            public CSEnderecoPDV()
            {

            }

            #endregion
        }

        #endregion
    }
}