#region Using directives

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
    public class CSMonitoramentosVendedoresRotas : CollectionBase
    {
        #region [ Variaveis ]

        private CSMonitoramentosVendedoresRotas.CSMonitoramentoVendedorRota m_Current;

        #endregion

        #region [ Propriedades ]

        public CSMonitoramentosVendedoresRotas.CSMonitoramentoVendedorRota Current
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

        #endregion

        #region [ Metodos ]

        public CSMonitoramentosVendedoresRotas(int COD_PDV)
        {
        }

        /// <summary>
        /// Adiciona um monitoramento na coleção
        /// </summary>
        /// <param name="c">Instacia do monitoramente a ser adcionada</param>
        /// <returns>return a posição do monitoramento adicionado na coleção</returns>
        public int Add(CSMonitoramentoVendedorRota monitoramento)
        {
            // Adiciona na coleção
            int idx = base.InnerList.Add(monitoramento);
            // Retorna a posição dele na coleção
            return idx;
        }

        /// <summary>
        /// Salva os dados na coleção no banco
        /// </summary>
        public bool Flush()
        {

            string sqlQueryInsert = "INSERT INTO MONITORAMENTO_VENDEDOR_ROTA " +
                "(COD_PDV, DAT_ENTRADA, DAT_SAIDA, IND_TIPO_ACESSO, COD_EMPREGADO, LOC_GPS_INICIAL, LOC_GPS_FINAL) " +
                " VALUES(?,?,?,?,?,?,?)";

            // Varre a coleção procurando os objetos a serem persistidos
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                CSMonitoramentoVendedorRota monitoramento = (CSMonitoramentoVendedorRota)base.InnerList[i];
                //}
                //foreach (CSMonitoramentoVendedorRota monitoramento in base.InnerList)
                //{
                // Verificacao do State do Objeto
                if (monitoramento.STATE == ObjectState.NOVO)
                {
                    // Criar os parametros de salvamento
                    SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", monitoramento.COD_PDV);
                    SQLiteParameter pDAT_ENTRADA = new SQLiteParameter("@DAT_ENTRADA", monitoramento.DAT_ENTRADA);
                    SQLiteParameter pDAT_SAIDA = new SQLiteParameter("@DAT_SAIDA", monitoramento.DAT_SAIDA);
                    SQLiteParameter pIND_TIPO_ACESSO = new SQLiteParameter("@IND_TIPO_ACESSO", monitoramento.IND_TIPO_ACESSO);
                    SQLiteParameter pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", monitoramento.COD_EMPREGADO);
                    SQLiteParameter pLOC_GPS_INICIAL = new SQLiteParameter("@LOC_GPS_INICIAL", monitoramento.LOG_GPS_INICIAL);
                    SQLiteParameter pLOC_GPS_FINAL = new SQLiteParameter("@LOC_GPS_FINAL", monitoramento.LOG_GPS_FINAL);

                    // Executa a query salvando os dados
                    CSDataAccess.Instance.ExecuteScalar(sqlQueryInsert, pCOD_PDV, pDAT_ENTRADA, pDAT_SAIDA, pIND_TIPO_ACESSO, pCOD_EMPREGADO, pLOC_GPS_INICIAL, pLOC_GPS_FINAL);
                    // Muda o state dele para ObjectState.SALVO
                    monitoramento.STATE = ObjectState.SALVO;
                    //remove este item da lista para evitar problemas na 
                    //gravação pois o pessoal da revenda esta reclamando
                    //de gravar mais de um monitoramento e tambem
                    //esta linha vai tornar o codigo mais rapido
                    //pois nao precisava de gravar os monitoramento
                    //em uma coleção
                    base.InnerList.Remove(monitoramento);

                    if (base.Count == 0)
                    {
                        return true;
                    }
                }
            }
            // Força limpar a memoria
            //GC.Collect();
            return true;
        }

        public CSMonitoramentosVendedoresRotas Items
        {
            get
            {
                return this;
            }
        }

        #endregion

        #region [ SubClasses ]

        /// <summary>
        /// Summary description for CSEmpregado.
        /// </summary>
        public class CSMonitoramentoVendedorRota
        {
            #region [ Variaveis ]

            private int m_COD_PDV = -1;
            private DateTime m_DAT_ENTRADA = new DateTime(1900, 1, 1);
            private DateTime m_DAT_SAIDA = new DateTime(1900, 1, 1);
            private string m_IND_TIPO_ACESSO = "a";
            private int m_COD_EMPREGADO = -1;
            private string m_LOC_GPS_INICIAL = "";
            private string m_LOC_GPS_FINAL = "";

            // O default deste state deve ser novo para que quando se crie um ele ja venha com este estado
            // o estado dos objetos que estao sendo buscados devem ser alterados no momento do preenchimento
            private ObjectState m_STATE = ObjectState.NOVO;

            #endregion

            #region [ Propriedades ]

            /// <summary>
            /// Guarda o codigo do PDV
            /// </summary>
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

            /// <summary>
            /// Guarda a hora de entrada
            /// </summary>
            public DateTime DAT_ENTRADA
            {
                get
                {
                    return m_DAT_ENTRADA;
                }
                set
                {
                    m_DAT_ENTRADA = value;
                }
            }

            /// <summary>
            /// Guarda a hora de saída
            /// </summary>
            public DateTime DAT_SAIDA
            {
                get
                {
                    return m_DAT_SAIDA;
                }
                set
                {
                    m_DAT_SAIDA = value;
                }
            }
            /// <summary>
            /// Guarda o tipo de acesso
            /// </summary>
            public string IND_TIPO_ACESSO
            {
                get
                {
                    return m_IND_TIPO_ACESSO;
                }
                set
                {
                    m_IND_TIPO_ACESSO = value;
                }
            }
            /// <summary>
            /// Guarda o codigo do Empregado
            /// </summary>
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
                    m_STATE = value;
                }
            }
            /// <summary>
            /// Localização GPS Inicial
            /// </summary>            
            public string LOG_GPS_INICIAL
            {
                get
                {
                    return m_LOC_GPS_INICIAL;
                }
                set
                {
                    m_LOC_GPS_INICIAL = value;
                }
            }
            /// <summary>
            /// Localização GPS Inicial
            /// </summary>            
            public string LOG_GPS_FINAL
            {
                get
                {
                    return m_LOC_GPS_FINAL;
                }
                set
                {
                    m_LOC_GPS_FINAL = value;
                }
            }

            #endregion

            #region [ Metodos ]

            public CSMonitoramentoVendedorRota()
            {
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

            #endregion
        }

        #endregion
    }
}