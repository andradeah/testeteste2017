#region Using directives

using System;
using System.Text;
using System.Collections;
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
using AvanteSales.SystemFramework;
using System.Reflection;

#endregion

namespace AvanteSales.BusinessRules
{
    /// <summary>
    /// Summary description for CSPesquisaMerchandising.
    /// </summary>
    public class CSPesquisaMerchandising : CollectionBase, IDisposable
    {
        #region [ Variáveis ]

        private CSPDVs.CSPDV pdv;
        private CSPesquisaMerchandising.CSMaterialMerchandising current;

        #endregion

        #region [ Propriedades ]

        public CSPDVs.CSPDV Pdv
        {
            get
            {
                return pdv;
            }
            set
            {
                pdv = value;
            }
        }

        public CSPesquisaMerchandising.CSMaterialMerchandising Current
        {
            get
            {
                return current;
            }
            set
            {
                current = value;
            }
        }

        public CSPesquisaMerchandising.CSMaterialMerchandising this[int index]
        {
            get
            {
                if (index >= 0 && index < this.InnerList.Count)
                    return (CSPesquisaMerchandising.CSMaterialMerchandising)this.InnerList[index];
                else
                    return null;
            }
        }

        public CSPesquisaMerchandising Items
        {
            get
            {
                return this;
            }
        }

        #endregion

        #region [ Métodos ]

        public CSPesquisaMerchandising(CSPDVs.CSPDV pdv)
        {
            StringBuilder sqlQuery = null;
            SQLiteParameter pCOD_PDV = null;
            SQLiteDataReader reader = null;

            try
            {
                this.Pdv = pdv;

                sqlQuery = new StringBuilder();

                // [ Carrega materiais a serem pesquisados ]
                sqlQuery.Length = 0;
                sqlQuery.Append("SELECT T3.COD_PRODUTO, T4.QTDE_COLETADA, T4.DATA_COLETA ");
                sqlQuery.Append("  FROM PESQUISA_MERCHAN_PRODCATEGORIA T1 ");
                sqlQuery.Append("  JOIN PDV T2 ON T1.COD_CATEGORIA = T2.COD_CATEGORIA ");
                sqlQuery.Append("  JOIN PRODUTO T3 ON T1.COD_PRODUTO = T3.COD_PRODUTO ");
                sqlQuery.Append("  LEFT JOIN PESQUISA_MERCHAN_PDV T4 ON T1.COD_PRODUTO = T4.COD_PRODUTO ");
                sqlQuery.Append("   AND T2.COD_PDV = T4.COD_PDV ");
                sqlQuery.Append(" WHERE T2.COD_PDV = ? ");
                sqlQuery.Append("   AND T3.IND_ATIVO = 'A' ");
                sqlQuery.Append(" ORDER BY T3.COD_PRODUTO ");

                pCOD_PDV = new SQLiteParameter("@COD_PDV", this.Pdv.COD_PDV);

                using (reader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString(), pCOD_PDV))
                {
                    while (reader.Read())
                    {
                        CSMaterialMerchandising material = new CSMaterialMerchandising();

                        material.COD_PDV = this.Pdv.COD_PDV;
                        material.PRODUTO = CSProdutos.GetProduto((reader.GetValue(0) == System.DBNull.Value) ? -1 : reader.GetInt32(0));
                        material.QTDE_COLETADA = (reader.GetValue(1) == System.DBNull.Value) ? 0 : reader.GetDecimal(1);
                        material.DATA_COLETA = (reader.GetValue(2) == System.DBNull.Value) ? new DateTime(1900, 1, 1) : reader.GetDateTime(2);

                        material.STATE = ObjectState.INALTERADO;

                        // [ Adiciona novo material ]
                        base.InnerList.Add(material);
                    }

                    // [ Fecha e libera reader ]
                    reader.Close();
                    reader.Dispose();
                }

            }
            catch (Exception e)
            {
                throw new Exception("Falha ao carregar pesquisas de merchandising", e);
            }
        }

        public ArrayList GetMateriais(CSGruposProduto.CSGrupoProduto grupo, CSFamiliasProduto.CSFamiliaProduto familia, bool somenteColetados)
        {
            ArrayList result = null;
            DateTime data;

            try
            {
                result = new ArrayList();
                data = new DateTime(1900, 1, 1);

                // [ Percorre a lista de produtos da pesquisa ]
                foreach (CSMaterialMerchandising material in this.InnerList)
                {
                    // [ Procura por material que atenda aos critérios da pesqusia ]
                    if ((grupo.COD_GRUPO == -1 || material.PRODUTO.GRUPO.COD_GRUPO == grupo.COD_GRUPO) &&
                        (familia.COD_FAMILIA_PRODUTO == -1 ||
                        (material.PRODUTO.FAMILIA_PRODUTO.COD_FAMILIA_PRODUTO == familia.COD_FAMILIA_PRODUTO &&
                        material.PRODUTO.GRUPO.COD_GRUPO == familia.GRUPO.COD_GRUPO)) &&
                        (!somenteColetados || material.DATA_COLETA != data))
                    {
                        // [ Adiciona material no list de resultado ]
                        result.Add(material);
                    }
                }

                return result;

            }
            catch (Exception e)
            {
                throw new Exception("Falha ao filtrar produtos!", e);
            }
        }

        public bool HasChanges()
        {
            // [ Varre a coleção procurando os objetos alterados ]
            foreach (CSMaterialMerchandising material in base.InnerList)
            {
                if (material.STATE == ObjectState.ALTERADO)
                    return true;
            }

            return false;
        }

        public void Flush()
        {
            string sqlQueryInsert = null;
            string sqlQueryUpdate = null;

            SQLiteParameter pCOD_PDV = null;
            SQLiteParameter pCOD_PRODUTO = null;
            SQLiteParameter pQTDE_COLETADA = null;
            SQLiteParameter pDATA_COLETA = null;

            try
            {
                sqlQueryInsert =
                    "INSERT INTO PESQUISA_MERCHAN_PDV " +
                    "       (COD_PDV, COD_PRODUTO, QTDE_COLETADA, DATA_COLETA) " +
                    "VALUES (?, ?, ?, ?) ";

                sqlQueryUpdate =
                    "UPDATE PESQUISA_MERCHAN_PDV " +
                    "   SET QTDE_COLETADA = ? " +
                    "      ,DATA_COLETA = ? " +
                    " WHERE COD_PDV = ? " +
                    "   AND COD_PRODUTO = ? ";

                // [ Varre a coleção procurando os objetos a serem persistidos ]
                foreach (CSMaterialMerchandising material in base.InnerList)
                {
                    if (material.STATE == ObjectState.ALTERADO)
                    {
                        // Criar os parametros de salvamento
                        pCOD_PDV = new SQLiteParameter("@COD_PDV", material.COD_PDV);
                        pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", material.PRODUTO.COD_PRODUTO);
                        pQTDE_COLETADA = new SQLiteParameter("@QTDE_COLETADA", material.QTDE_COLETADA);
                        pDATA_COLETA = new SQLiteParameter("@DATA_COLETA", material.DATA_COLETA);

                        // Executa a query atualizando os dados
                        if (CSDataAccess.Instance.ExecuteNonQuery(sqlQueryUpdate, pQTDE_COLETADA, pDATA_COLETA, pCOD_PDV, pCOD_PRODUTO) == 0)
                        {
                            // Criar os parametros de salvamento
                            pCOD_PDV = new SQLiteParameter("@COD_PDV", material.COD_PDV);
                            pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", material.PRODUTO.COD_PRODUTO);
                            pQTDE_COLETADA = new SQLiteParameter("@QTDE_COLETADA", material.QTDE_COLETADA);
                            pDATA_COLETA = new SQLiteParameter("@DATA_COLETA", material.DATA_COLETA);

                            // Executa a query salvando os dados
                            CSDataAccess.Instance.ExecuteNonQuery(sqlQueryInsert, pCOD_PDV, pCOD_PRODUTO, pQTDE_COLETADA, pDATA_COLETA);
                        }

                        // Muda o state para ObjectState.SALVO
                        material.STATE = ObjectState.SALVO;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Falha ao gravar pesquisa de merchandising", e);
            }
        }

        public void Dispose()
        {
            this.InnerList.Clear();
            this.InnerList.TrimToSize();
        }

        #endregion

        #region [ SubClasses ]

        public class CSMaterialMerchandising
        {
            #region [ Variáveis ]

            private int m_COD_PDV;
            private CSProdutos.CSProduto m_PRODUTO;
            private decimal m_QTDE_COLETADA;
            private DateTime m_DATA_COLETA;


            private ObjectState m_STATE;

            #endregion

            #region [ Propriedades ]

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

            public decimal QTDE_COLETADA
            {
                get
                {
                    return m_QTDE_COLETADA;
                }
                set
                {
                    // [ Controla o status do material. Necessário para utilização com o EditableDataGrid ]
                    if (m_QTDE_COLETADA != value)
                    {
                        STATE = ObjectState.ALTERADO;
                        DATA_COLETA = DateTime.Now;

                        m_QTDE_COLETADA = value;
                    }
                }
            }

            public DateTime DATA_COLETA
            {
                get
                {
                    return m_DATA_COLETA;
                }
                set
                {
                    m_DATA_COLETA = value;
                }
            }

            public string DSC_DATA_COLETA
            {
                get
                {
                    if (DATA_COLETA == new DateTime(1900, 1, 1))
                        return "";
                    else
                        return DATA_COLETA.ToString("dd/MM/yyyy");
                }
            }

            public string DSC_UNIDADE_MEDIDA
            {
                get
                {
                    return this.PRODUTO.DSC_UNIDADE_MEDIDA;
                }
            }

            public string DESCRICAO_APELIDO_PRODUTO
            {
                get
                {
                    return this.PRODUTO.DESCRICAO_APELIDO_PRODUTO.Trim().PadLeft(8, '0');
                }
            }

            public string DSC_PRODUTO
            {
                get
                {
#if !ANDROID
                    return FontDesigner.Instance.FormatString(this.PRODUTO.DSC_PRODUTO);
#else
                    return this.PRODUTO.DSC_PRODUTO;
#endif
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

            #endregion

            #region [ Métodos ]

            public CSMaterialMerchandising()
            {
            }

            #endregion

            #region [ Subclasses ]

            public class CSMaterialMerchandisingComparer : IComparer
            {
                private string mappingName;
                private bool ascendent;

                public CSMaterialMerchandisingComparer(string mappingName, bool ascendent)
                {
                    this.mappingName = mappingName;
                    this.ascendent = ascendent;
                }

                int IComparer.Compare(object x, object y)
                {
                    int result = 0;

                    try
                    {
                        PropertyInfo p1 = x.GetType().GetProperty(mappingName);
                        PropertyInfo p2 = y.GetType().GetProperty(mappingName);

                        object o1 = p1.GetValue(x, null);
                        object o2 = p2.GetValue(y, null);

                        if (o1 != null && o2 != null)
                        {
                            Type type = o1.GetType();

                            if (type == typeof(string))
                                result = o1.ToString().CompareTo(o2.ToString());
                            else if (type == typeof(decimal))
                                result = (Convert.ToDecimal(o1)).CompareTo(o2);
                            else if (type == typeof(DateTime))
                                result = (Convert.ToDateTime(o1)).CompareTo(o2);
                        }

                        return result * (ascendent ? 1 : -1);

                    }
                    catch
                    {
                        return 0;
                    }
                }
            }

            #endregion

        }

        #endregion

    }
}