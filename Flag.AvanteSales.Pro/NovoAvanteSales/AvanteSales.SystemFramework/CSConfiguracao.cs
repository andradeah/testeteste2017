using System;
using System.IO;
using System.Data;
using System.Xml;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;

namespace AvanteSales
{
    /// <summary>
    /// Classe de configurações do sistema
    /// </summary>
    public class CSConfiguracao
    {
        private static string configFile = Path.Combine(CSGlobal.GetCurrentDirectory(), "config.xml");

        public static bool ArquivoConfigCriado()
        {
            return File.Exists(configFile);
        }

        public static void CriarArquivoConfig()
        {
            GetConfigDataset();
        }

        private static DataSet GetConfigDataset()
        {
            try
            {
                DataSet ds = new DataSet("Config");
                ds.Tables.Add("Config");
                ds.Tables["Config"].Columns.Add("configKey", typeof(string));
                ds.Tables["Config"].Columns.Add("configValue", typeof(string));
                ds.AcceptChanges();

                if (ArquivoConfigCriado())
                {
                    try
                    {
                        ds.ReadXml(configFile);
                    }
                    catch (XmlException ex)
                    {
                        if (ex.Message.StartsWith("Document element did not appear."))
                        {
                            File.Delete(configFile);
                            ds.WriteXml(configFile);
                        }
                    }
                }
                else
                {
                    ds.WriteXml(configFile);
                }

                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static string GetConfig(string configKey)
        {
            try
            {
                DataSet ds = GetConfigDataset();

                foreach (DataRow dr in ds.Tables["Config"].Rows)
                {
                    if ((string)dr["configKey"] == configKey)
                    {
                        return (string)dr["configValue"];
                    }
                }

                return "";
            }
            catch (Exception ex)
            {
                throw new Exception("");
            }
        }

        public static void SetConfig(string configKey, string configValue)
        {
            try
            {
                DataSet ds = GetConfigDataset();

                foreach (DataRow dr in ds.Tables["Config"].Rows)
                {
                    if ((string)dr["configKey"] == configKey)
                    {
                        dr["configValue"] = configValue;
                        ds.AcceptChanges();
                        ds.WriteXml(configFile);
                        return;
                    }
                }

                // Config nao existente, adiciona

                DataRow newRow = ds.Tables["Config"].NewRow();

                newRow["configKey"] = configKey;
                newRow["configValue"] = configValue;

                ds.Tables["Config"].Rows.Add(newRow);
                ds.AcceptChanges();
                ds.WriteXml(configFile);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static System.Collections.ArrayList GetEmpresas()
        {
            try
            {
                ArrayList empresas = new ArrayList();
                string empresaFile = Path.Combine(CSGlobal.GetCurrentDirectory(), "empresas.xml");

                if (File.Exists(empresaFile))
                {
                    DataSet ds = new DataSet();

                    ds.ReadXml(empresaFile);

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        if (CSGlobal.COD_REVENDA == null) CSGlobal.COD_REVENDA = dr["COD_REVENDA"].ToString();
                        empresas.Add(dr["COD_REVENDA"] + "-" + dr["NOME_EMPRESA"].ToString().Trim());
                    }
                }

                return empresas;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static System.Collections.ArrayList GetEmpresasQuePossuemBancoNoAparelho()
        {
            try
            {
                var diretorioDaAplicacao = CSGlobal.GetCurrentDirectory();
                ArrayList empresas = new ArrayList();

                var arquivos = Directory.GetFiles(diretorioDaAplicacao, "*.sdf");

                for (int i = 0; i < arquivos.Count(); i++)
                {
                    arquivos[i] = arquivos[i].Replace(diretorioDaAplicacao + "/AvanteSales", "").Replace(".sdf", "");
                    //= .Replace(diretorioDaAplicacao, "").Replace("AvanteSales", "").Replace(".sdf", "");
                }

                arquivos = arquivos.Distinct().ToArray();

                var empresaFile = Path.Combine(diretorioDaAplicacao, "empresas.xml");

                if (File.Exists(empresaFile))
                {
                    DataSet ds = new DataSet();
                    ds.ReadXml(empresaFile);

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        if (arquivos.Contains(dr["COD_REVENDA"].ToString()))
                        {
                            if (CSGlobal.COD_REVENDA == null || CSGlobal.COD_REVENDA == "XXXXXXXX")
                                CSGlobal.COD_REVENDA = dr["COD_REVENDA"].ToString();

                            empresas.Add(dr["COD_REVENDA"] + "-" + dr["NOME_EMPRESA"].ToString().Trim());
                        }
                    }
                }

                return empresas;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void VerificaTagDBFILE()
        {
            try
            {
                DataSet ds = GetConfigDataset();

                foreach (DataRow dr in ds.Tables["Config"].Rows)
                {
                    if ((string)dr["configKey"] == "dbFile")
                    {
                        return;
                    }
                }

                // Config nao existente, adiciona
                DataRow newRow = ds.Tables["Config"].NewRow();

                newRow["configKey"] = "dbFile";
                newRow["configValue"] = "AvanteSales";

                ds.Tables["Config"].Rows.Add(newRow);
                ds.AcceptChanges();
                ds.WriteXml(configFile);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}