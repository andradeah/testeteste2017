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
    public class CSComodatosPDV : CollectionBase, IDisposable
    {
        #region [ Variáveis ]

        private CSComodatosPDV.CSComodatoPDV m_current;

        #endregion

        #region [ Propriedades ]

        public CSComodatosPDV.CSComodatoPDV Current
        {
            get
            {
                return m_current;
            }
            set
            {
                m_current = value;
            }
        }

        #endregion

        #region [ Métodos ]

        public CSComodatosPDV(int codPdv)
        {
            StringBuilder sqlComodato = new StringBuilder();

            sqlComodato.Append("SELECT ");
            sqlComodato.AppendLine("    T1.COD_COMODATO ");
            sqlComodato.AppendLine("   ,T1.COD_PDV ");
            sqlComodato.AppendLine("   ,T1.COD_FINALIDADE ");
            sqlComodato.AppendLine("   ,T2.DSC_TIPO_COMODATO ");
            sqlComodato.AppendLine("   ,T1.DSC_RESPONSAVEL ");
            sqlComodato.AppendLine("   ,T1.NUM_CONTRATO ");
            sqlComodato.AppendLine("   ,T1.DATA_EMISSAO ");
            sqlComodato.AppendLine("   ,T1.DATA_VENCIMENTO ");
            sqlComodato.AppendLine(" FROM PDV_COMODATO T1 ");
            sqlComodato.AppendLine(" JOIN TIPO_COMODATO T2 ");
            sqlComodato.AppendLine("    ON T1.COD_TIPO_COMODATO = T2.COD_TIPO_COMODATO ");
            sqlComodato.AppendFormat(" WHERE COD_PDV = {0}", codPdv);

            StringBuilder sqlMaterial = new StringBuilder();

            sqlMaterial.Append("    SELECT ");
            sqlMaterial.AppendLine("       COD_COMODATO ");
            sqlMaterial.AppendLine("      ,COD_PDV ");
            sqlMaterial.AppendLine("      ,COD_PRODUTO ");
            sqlMaterial.AppendLine("      ,QTD_COMODATADA ");
            sqlMaterial.AppendLine("FROM PDV_COMODATO_PRODUTO ");
            sqlMaterial.AppendLine("WHERE COD_COMODATO = {0}");
            sqlMaterial.AppendLine("  AND COD_PDV = {1}");

            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlComodato.ToString()))
            {
                while (sqlReader.Read())
                {
                    CSComodatoPDV comodato = new CSComodatoPDV();
                    comodato.COD_COMODATO = sqlReader.GetValue(0) == DBNull.Value ? 0 : sqlReader.GetInt32(0);
                    comodato.COD_PDV = sqlReader.GetValue(1) == DBNull.Value ? 0 : sqlReader.GetInt32(1);
                    comodato.COD_FINALIDADE = sqlReader.GetValue(2) == DBNull.Value ? 0 : sqlReader.GetInt32(2);
                    comodato.IND_TIPO = sqlReader.GetValue(3) == DBNull.Value ? string.Empty : sqlReader.GetString(3);
                    comodato.DSC_RESPONSAVEL = sqlReader.GetValue(4) == DBNull.Value ? string.Empty : sqlReader.GetString(4);
                    comodato.NUM_CONTRATO = sqlReader.GetValue(5) == DBNull.Value ? 0 : sqlReader.GetInt32(5);
                    comodato.DATA_EMISSAO = sqlReader.GetValue(6) == DBNull.Value ? new DateTime() : Convert.ToDateTime(sqlReader.GetValue(6));
                    comodato.DATA_VENCIMENTO = sqlReader.GetValue(7) == DBNull.Value ? new DateTime() : Convert.ToDateTime(sqlReader.GetValue(7));
                    comodato.FINALIDADE = new CSFinalidade(comodato.COD_FINALIDADE);
                    comodato.MATERIAIS = new List<CSMaterial>();

                    CSMaterial material;

                    using (SQLiteDataReader sqlReaderMaterial = CSDataAccess.Instance.ExecuteReader(string.Format(sqlMaterial.ToString(), comodato.COD_COMODATO, comodato.COD_PDV)))
                    {
                        while (sqlReaderMaterial.Read())
                        {
                            material = new CSMaterial();
                            material.COD_COMODATO = sqlReaderMaterial.GetValue(0) == DBNull.Value ? 0 : sqlReaderMaterial.GetInt32(0);
                            material.COD_PDV = sqlReaderMaterial.GetValue(1) == DBNull.Value ? 0 : sqlReaderMaterial.GetInt32(1);
                            material.COD_PRODUTO = sqlReaderMaterial.GetValue(2) == DBNull.Value ? 0 : sqlReaderMaterial.GetInt32(2);
                            material.QTD_COMODATADA = sqlReaderMaterial.GetValue(3) == DBNull.Value ? 0 : sqlReaderMaterial.GetInt32(3);
                            material.PRODUTO = CSProdutos.GetProduto(material.COD_PRODUTO);

                            comodato.MATERIAIS.Add(material);
                        }
                    }

                    this.InnerList.Add(comodato);
                }
            }
        }

        public void Dispose()
        {
            this.InnerList.Clear();
            this.InnerList.TrimToSize();
        }

        #endregion

        #region [ SubClasses ]

        public class CSMaterial
        {
            #region [ Variáveis ]

            private int m_COD_COMODATO;
            private int m_COD_PDV;
            private int m_COD_PRODUTO;
            private int m_QTD_COMODATADA;
            private CSProdutos.CSProduto m_PRODUTO;

            #endregion

            #region [ Métodos ]

            public int COD_COMODATO
            {
                get
                {
                    return m_COD_COMODATO;
                }
                set
                {
                    m_COD_COMODATO = value;
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

            public int QTD_COMODATADA
            {
                get
                {
                    return m_QTD_COMODATADA;
                }
                set
                {
                    m_QTD_COMODATADA = value;
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

            #endregion
        }

        public class CSFinalidade
        {
            #region [ Variáveis ]

            private int m_COD_FINALIDADE;
            private string m_DESCRICAO;

            #endregion

            #region [ Propriedades ]

            public int COD_FINALIDADE
            {
                get
                {
                    return m_COD_FINALIDADE;
                }
                set
                {
                    m_COD_FINALIDADE = value;
                }
            }

            public string DESCRICAO
            {
                get
                {
                    return m_DESCRICAO;
                }
                set
                {
                    m_DESCRICAO = value;
                }
            }

            #endregion

            #region [ Métodos ]

            public CSFinalidade()
            {

            }

            public CSFinalidade(int codFinalidade)
            {
                StringBuilder sql = new StringBuilder();

                sql.Append(" SELECT ");
                sql.AppendLine("  COD_FINALIDADE ");
                sql.AppendLine(" ,DSC_FINALIDADE ");
                sql.AppendLine("FROM FINALIDADE ");
                sql.AppendFormat("WHERE COD_FINALIDADE = {0}", codFinalidade);

                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
                {
                    if (sqlReader.Read())
                    {
                        this.COD_FINALIDADE = sqlReader.GetValue(0) == DBNull.Value ? 0 : sqlReader.GetInt32(0);
                        this.DESCRICAO = sqlReader.GetValue(1) == DBNull.Value ? string.Empty : sqlReader.GetString(1);
                    }
                }
            }

            #endregion
        }

        public class CSComodatoPDV : Java.Lang.Object
        {
            #region [ Variáveis ]

            private int m_COD_COMODATO;
            private int m_COD_PDV;
            private int m_COD_FINALIDADE;
            private string m_IND_TIPO;
            private string m_DSC_RESPONSAVEL;
            private int m_NUM_CONTRATO;
            private DateTime m_DATA_EMISSAO;
            private DateTime m_DATA_VENCIMENTO;
            private CSFinalidade m_FINALIDADE;
            private List<CSMaterial> m_MATERIAIS;

            #endregion

            #region [ Propriedades ]

            public int COD_COMODATO
            {
                get
                {
                    return m_COD_COMODATO;
                }
                set
                {
                    m_COD_COMODATO = value;
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

            public int COD_FINALIDADE
            {
                get
                {
                    return m_COD_FINALIDADE;
                }
                set
                {
                    m_COD_FINALIDADE = value;
                }
            }

            public string IND_TIPO
            {
                get
                {
                    return m_IND_TIPO;
                }
                set
                {
                    m_IND_TIPO = value;
                }
            }

            public string DSC_RESPONSAVEL
            {
                get
                {
                    return m_DSC_RESPONSAVEL;
                }
                set
                {
                    m_DSC_RESPONSAVEL = value;
                }
            }

            public int NUM_CONTRATO
            {
                get
                {
                    return m_NUM_CONTRATO;
                }
                set
                {
                    m_NUM_CONTRATO = value;
                }
            }

            public DateTime DATA_EMISSAO
            {
                get
                {
                    return m_DATA_EMISSAO;
                }
                set
                {
                    m_DATA_EMISSAO = value;
                }
            }

            public DateTime DATA_VENCIMENTO
            {
                get
                {
                    return m_DATA_VENCIMENTO;
                }
                set
                {
                    m_DATA_VENCIMENTO = value;
                }
            }

            public CSFinalidade FINALIDADE
            {
                get
                {
                    return m_FINALIDADE;
                }
                set
                {
                    m_FINALIDADE = value;
                }
            }

            public List<CSMaterial> MATERIAIS
            {
                get
                {
                    return m_MATERIAIS;
                }
                set
                {
                    m_MATERIAIS = value;
                }
            }

            #endregion

            public CSComodatoPDV()
            {

            }
        }

        #endregion
    }
}