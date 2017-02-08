using System;
using System.Collections;
using System.Data;
using System.Collections.Generic;
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

namespace AvanteSales.SystemFramework.BusinessLayer
{
    public class CSMotivosIndenizacao : CollectionBase
    {
        #region [ Variáveis ]

        private static CSMotivosIndenizacao m_Items;

        #endregion

        #region [ Propriedades ]

        /// <summary>
        /// Retorna coleção dos grupos de comercializacao
        /// </summary>
        public static CSMotivosIndenizacao Items
        {
            get
            {
                if (m_Items == null)
                    m_Items = new CSMotivosIndenizacao();
                return m_Items;
            }
        }

        public CSMotivosIndenizacao.CSMotivoIndenizacao this[int Index]
        {
            get
            {
                return (CSMotivosIndenizacao.CSMotivoIndenizacao)this.InnerList[Index];
            }
        }

        #endregion

        #region [ Métodos ]

        public CSMotivosIndenizacao()
        {
            string sqlQuery = "SELECT * FROM [MOTIVO_INDENIZACAO]";

            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery))
            {
                while (sqlReader.Read())
                {
                    CSMotivoIndenizacao motivo = new CSMotivoIndenizacao();

                    // Preenche a instancia da classe dos grupos
                    motivo.COD_MOTIVO_INDENIZACAO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                    motivo.DSC_MOTIVO_INDENIZACAO = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1).Trim();

                    // Adcionao grupo na coleção
                    base.InnerList.Add(motivo);
                }

                // Fecha o reader
                sqlReader.Close();
                sqlReader.Dispose();
            }
        }

        #endregion

        #region [ SubClasses ]

        public class CSMotivoIndenizacao
        {
            #region [ Variáveis ]

            private int m_COD_MOTIVO_INDENIZACAO;
            private string m_DSC_MOTIVO_INDENIZACAO;
            private string m_STATUS;

            #endregion

            #region [ Propriedades ]

            public int COD_MOTIVO_INDENIZACAO
            {
                get
                {
                    return m_COD_MOTIVO_INDENIZACAO;
                }
                set
                {
                    m_COD_MOTIVO_INDENIZACAO = value;
                }
            }
            public string DSC_MOTIVO_INDENIZACAO
            {
                get
                {
                    return m_DSC_MOTIVO_INDENIZACAO;
                }
                set
                {
                    m_DSC_MOTIVO_INDENIZACAO = value;
                }
            }

            #endregion
        }

        #endregion
    }
}