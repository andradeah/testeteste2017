using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Collections;

namespace AvanteSales.SystemFramework.BusinessLayer
{
    public class CSVisitas : CollectionBase
    {
        private CSVisitas m_Items;

        public CSVisitas Items
        {
            get
            {
                if (m_Items == null)
                    m_Items = CarregarVisitas();

                return m_Items;
            }
            set
            {
                m_Items = value;
            }
        }

        public CSVisitas CarregarVisitas()
        {
            try
            {
                CSVisita visita;

                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT COD_EMPREGADO ");
                sql.AppendLine("	  ,COD_PDV ");
                sql.AppendLine("	  ,DAT_VISITA ");
                sql.AppendLine("	  ,NUM_ORDEM_VISITA ");
                sql.AppendLine("	  ,IND_VISITADO ");
                sql.AppendLine("	  ,DAT_ALTERACAO ");
                sql.AppendLine("FROM PDV_VISITA ");
                sql.AppendFormat("WHERE COD_EMPREGADO = {0} ", CSEmpregados.Current.COD_EMPREGADO);
                sql.AppendFormat("      AND DATE(DAT_VISITA) = DATE('{0}')", DateTime.Now.Date.ToString("yyyy-MM-dd"));
                sql.AppendLine("ORDER BY NUM_ORDEM_VISITA ");

                using (var reader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
                {
                    while (reader.Read())
                    {
                        visita = new CSVisita();

                        visita.COD_EMPREGADO = reader.GetValue(0) == System.DBNull.Value ? 0 : reader.GetInt32(0);
                        visita.COD_PDV = reader.GetValue(1) == System.DBNull.Value ? 0 : reader.GetInt32(1);
                        visita.DAT_VISITA = reader.GetValue(2) == System.DBNull.Value ? DateTime.Now : reader.GetDateTime(2);
                        visita.NUM_ORDEM_VISITA = reader.GetValue(3) == System.DBNull.Value ? 0 : reader.GetInt32(3);
                        visita.IND_VISITADO = reader.GetValue(4) == System.DBNull.Value ? false : reader.GetBoolean(4);
                        visita.DAT_ALTERACAO = reader.GetValue(5) == System.DBNull.Value ? DateTime.Now : reader.GetDateTime(5);

                        base.InnerList.Add(visita);
                    }
                }

                return this;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro na busca de visitas do empregado.");
            }
        }

        public CSVisitas()
        {

        }

        public static void InserirVisita(ArrayList codPdvsRota)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                int codPdvAtual = 0;
                int ordemVisita = 0;
                CSVisitas.CSVisita visita;

                sql.Append("INSERT INTO PDV_VISITA ");
                sql.AppendLine("   (COD_EMPREGADO ");
                sql.AppendLine("   ,COD_PDV ");
                sql.AppendLine("   ,DAT_VISITA ");
                sql.AppendLine("   ,NUM_ORDEM_VISITA ");
                sql.AppendLine("   ,IND_VISITADO) ");
                sql.AppendLine("VALUES(? ");
                sql.AppendLine("      ,? ");
                sql.AppendLine("      ,? ");
                sql.AppendLine("      ,? ");
                sql.AppendLine("      ,0) ");

                for (int i = 0; i < codPdvsRota.Count; i++)
                {
                    visita = new CSVisita();

                    codPdvAtual = Convert.ToInt32(codPdvsRota[i]);
                    ordemVisita = i + 1;

                    visita.COD_EMPREGADO = CSEmpregados.Current.COD_EMPREGADO;
                    visita.COD_PDV = codPdvAtual;
                    visita.DAT_VISITA = DateTime.Now;
                    visita.NUM_ORDEM_VISITA = ordemVisita;
                    visita.IND_VISITADO = false;

                    CSEmpregados.Current.VISITAS_EMPREGADO.InnerList.Add(visita);

                    SQLiteParameter pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", visita.COD_EMPREGADO);
                    SQLiteParameter pCOD_PDV = new SQLiteParameter("@COD_PDV", visita.COD_PDV);
                    SQLiteParameter pDAT_VISITA = new SQLiteParameter("@DAT_VISITA", visita.DAT_VISITA);
                    SQLiteParameter pNUM_ORDEM_VISITA = new SQLiteParameter("@NUM_ORDEM_VISITA", visita.NUM_ORDEM_VISITA);

                    pCOD_EMPREGADO.DbType = System.Data.DbType.Int32;
                    pCOD_PDV.DbType = System.Data.DbType.Int32;
                    pDAT_VISITA.DbType = System.Data.DbType.DateTime;
                    pNUM_ORDEM_VISITA.DbType = System.Data.DbType.Int32;

                    CSDataAccess.Instance.ExecuteNonQuery(sql.ToString(), pCOD_EMPREGADO, pCOD_PDV, pDAT_VISITA, pNUM_ORDEM_VISITA);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public class CSVisita
        {
            #region [ Variáveis ]

            private int m_COD_EMPREGADO;
            private int m_COD_PDV;
            private DateTime m_DAT_VISITA;
            private int m_NUM_ORDEM_VISITA;
            private bool m_IND_VISITADO;
            private DateTime m_DAT_ALTERACAO;

            #endregion

            #region [ Propriedades ]

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

            public DateTime DAT_VISITA
            {
                get
                {
                    return m_DAT_VISITA;
                }
                set
                {
                    m_DAT_VISITA = value;
                }
            }

            public bool IND_VISITADO
            {
                get
                {
                    return m_IND_VISITADO;
                }
                set
                {
                    m_IND_VISITADO = value;
                }
            }

            public DateTime DAT_ALTERACAO
            {
                get
                {
                    return m_DAT_ALTERACAO;
                }
                set
                {
                    m_DAT_ALTERACAO = value;
                }
            }

            public int NUM_ORDEM_VISITA
            {
                get
                {
                    return m_NUM_ORDEM_VISITA;
                }
                set
                {
                    m_NUM_ORDEM_VISITA = value;
                }
            }

            #endregion

            public bool PdvAteriorVisitado(List<CSVisitas.CSVisita> visitas)
            {
                if (this.NUM_ORDEM_VISITA > 1)
                {
                    var visitaAnterior = visitas.Where(v => v.NUM_ORDEM_VISITA == this.NUM_ORDEM_VISITA - 1).FirstOrDefault();

                    if (!visitaAnterior.IND_VISITADO)
                        return false;
                }

                return true;
            }

            public void SetPdvVisitado()
            {
                try
                {
                    if (this.IND_VISITADO)
                        return;

                    StringBuilder sql = new StringBuilder();
                    sql.Append("UPDATE PDV_VISITA SET IND_VISITADO = 1 ");
                    sql.AppendFormat(" ,DAT_ALTERACAO = DATETIME('{0}') ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    sql.AppendFormat("WHERE COD_PDV = {0} ", this.COD_PDV);
                    sql.AppendFormat("AND DATE(DAT_VISITA) = DATE('{0}')", this.DAT_VISITA.ToString("yyyy-MM-dd"));

                    CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());

                    this.IND_VISITADO = true;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        private static void GerarArquivo(DateTime dataInformacao, int codPdv)
        {
            string arquivo;
            string pathArquivo = "";
            string latitude = CSGlobal.GetLatitudeFlexxGPS();
            string longitude = CSGlobal.GetLongitudeFlexxGPS();
            string descricaoTipoExpediente = string.Empty;

            pathArquivo = System.IO.Path.Combine("/sdcard/FLAGPS_BD/ENVIAR/", "RE" + Guid.NewGuid());

            // Codigo da Revenda
            arquivo = CSEmpresa.Current.CODIGO_REVENDA.ToString().Trim();

            // Codigo do vendedor
            arquivo += "|" + CSEmpregados.Current.COD_EMPREGADO;

            // Codigo do cliente
            arquivo += "|" + codPdv.ToString();

            // (Inicial) Latitude & Longitude
            arquivo += string.Format("|{0}|{1}", latitude, longitude);

            // (Final) Latitude & Longitude
            arquivo += string.Format("|{0}|{1}", latitude, longitude);

            // Data
            arquivo += "|" + dataInformacao.ToString("dd/MM/yyyy");

            // Horas inicial 
            arquivo += "|" + dataInformacao.ToString("HH:mm:ss");

            arquivo += "|" + dataInformacao.ToString("HH:mm:ss");

            arquivo += "|" + "Reagendado";
            arquivo += "|||||";

            arquivo += "|" + Android.OS.Build.VERSION.Release;
            arquivo += "|" + string.Format("{0} {1}", Android.OS.Build.Manufacturer, Android.OS.Build.Model);
            arquivo += "|N||";

            if (System.IO.File.Exists(pathArquivo))
                System.IO.File.Delete(pathArquivo);

            if (System.IO.File.Exists(pathArquivo + ".txt"))
                System.IO.File.Delete(pathArquivo + ".txt");

            System.IO.TextWriter fileOut = System.IO.File.CreateText(pathArquivo);

            fileOut.WriteLine(arquivo.ToString());

            fileOut.Close();

            System.IO.File.Move(pathArquivo, pathArquivo + ".txt");
        }

        public static void ReagendarVisitas(CSVisitas.CSVisita visitaReagendamento, CSVisitas.CSVisita visitaSelecionada)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("UPDATE PDV_VISITA SET NUM_ORDEM_VISITA = (NUM_ORDEM_VISITA - 1)");
                sql.AppendFormat(",DAT_ALTERACAO = DATETIME('{0}') ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sql.AppendFormat("WHERE NUM_ORDEM_VISITA BETWEEN {0} AND {1} ", visitaReagendamento.NUM_ORDEM_VISITA + 1, visitaSelecionada.NUM_ORDEM_VISITA);
                sql.AppendFormat("AND DATE(DAT_VISITA) = DATE('{0}')", DateTime.Now.ToString("yyyy-MM-dd"));

                CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());

                DateTime dataAtual = DateTime.Now;

                sql = new StringBuilder();
                sql.AppendFormat("UPDATE PDV_VISITA SET NUM_ORDEM_VISITA = {0} ", visitaSelecionada.NUM_ORDEM_VISITA);
                sql.AppendFormat(",DAT_ALTERACAO = DATETIME('{0}') ", dataAtual.ToString("yyyy-MM-dd HH:mm:ss"));
                sql.AppendFormat("WHERE COD_PDV = {0} ", visitaReagendamento.COD_PDV);
                sql.AppendFormat("AND DATE(DAT_VISITA) = DATE('{0}')", DateTime.Now.ToString("yyyy-MM-dd"));

                CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());

                GerarArquivo(dataAtual, visitaReagendamento.COD_PDV);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}