using System;
using System.Data;
using Master.CompactFramework.Sync;
using System.Threading;
using System.Xml;
using System.Net;
using System.Text;
using System.IO;
using System.Globalization;
using Master.CompactFramework.Util.Compression;

#if ANDROID
using Mono.Data.Sqlite;
using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
using SQLiteCommand = Mono.Data.Sqlite.SqliteCommand;
using SQLiteDataAdapter = Mono.Data.Sqlite.SqliteDataAdapter;
using SQLiteException = Mono.Data.Sqlite.SqliteException;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteTransaction = Mono.Data.Sqlite.SqliteTransaction;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;
using Android.Graphics;
using AvanteSales.SyncManager.XmlProvider.XMLService;
using Path = System.IO.Path;

#else
using System.Data.SQLite;
using System.Drawing;
#endif

namespace Master.CompactFramework.Sync.XMLProvider
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public class XMLProvider : ISyncProvider
    {
        // Events
        public event SyncManager.StatusChangedEventHandler CheckPointCompleted;
        public event SyncManager.StatusChangedEventHandler CheckPointStarted;
        public event SyncManager.StatusChangedEventHandler StatusChanged;
        public event EventHandler DownloadCompleted;

        #region Properties

        // Properties
        public string DataBaseFilePath
        {
            get
            {
                return this._dataBaseFilePath;
            }
            set
            {
                this._dataBaseFilePath = value;
            }
        }

        public string ProviderName
        {
            get
            {
                return "XML Sync";
            }
        }

        public string ServerAddress
        {
            get
            {
                return this.xmlService.Url;
            }
            set
            {
                this.xmlService.Url = value;
            }
        }

        #endregion

        // Fields
        private string _dataBaseFilePath;
        private const string CARGA_FILE_PATH = @"\CargaAvanteSales.zip";
        private const string CARGA_PARCIAL_FILE_PATH = @"\CargaParcialAvanteSales.zip";
        private const string DADOS_PATH = @"\dados.xml";
        private const string DESCARGA_FILE_PATH = @"\DescargaAvanteSales.gz";
        private AutoResetEvent downloadComplete;
        private const string SCHEMA_PATH = @"\schema.xml";
        private string tempDownloadFile;
#if ANDROID
        private XMLService xmlService;
#else
        private XMLService.XMLService xmlService;
#endif

        // Methods
        public XMLProvider()
        {
            this.downloadComplete = new AutoResetEvent(false);
            ServicePointManager.DefaultConnectionLimit = 100;
#if ANDROID
            this.xmlService = new XMLService();
#else
            this.xmlService = new Master.CompactFramework.Sync.XMLProvider.XMLService.XMLService();
#endif
        }

        #region SQL Related Stuff

        private static void AddCommandParameter(int paramIndex, string paramValue, SQLiteCommand sqlCommand)
        {
            if (paramValue == "")
            {
                sqlCommand.Parameters[paramIndex].Value = DBNull.Value;
            }
            else
            {
                CultureInfo info1 = CultureInfo.InvariantCulture;
                DbType type1 = sqlCommand.Parameters[paramIndex].DbType;
                if (type1 == DbType.Int32)
                {
                    sqlCommand.Parameters[paramIndex].Value = int.Parse(paramValue, info1);
                }
                else if (type1 == DbType.Int64)
                {
                    sqlCommand.Parameters[paramIndex].Value = paramValue;
                }
                else if (type1 == DbType.DateTime)
                {
                    sqlCommand.Parameters[paramIndex].Value = DateTime.Parse(paramValue, info1);
                }
                else if (type1 == DbType.String)
                {
                    sqlCommand.Parameters[paramIndex].Value = paramValue;
                }
                else if (type1 == DbType.String)
                {
                    sqlCommand.Parameters[paramIndex].Value = paramValue;
                }
                else if (type1 == DbType.Double)
                {
                    sqlCommand.Parameters[paramIndex].Value = double.Parse(paramValue, info1);
                }
                else if (type1 == DbType.Double)
                {
                    sqlCommand.Parameters[paramIndex].Value = float.Parse(paramValue, info1);
                }
                else if (type1 == DbType.Boolean)
                {
                    sqlCommand.Parameters[paramIndex].Value = paramValue == "False" ? 0 : 1;
                }
                else if (type1 == DbType.Double)
                {
                    sqlCommand.Parameters[paramIndex].Value = paramValue;
                }
                else
                {
                    if (type1 != DbType.Decimal)
                    {
                        throw new ApplicationException("Tipo n\x00e3o reconhecido");
                    }
                    sqlCommand.Parameters[paramIndex].Value = decimal.Parse(paramValue, info1);
                }
            }
        }


        public void CreateDB(string dbFilePath, DataSet datasetSchema)
        {
            StringBuilder builder1 = null;

            try
            {
                this.OnStatusChanged("Criando Banco de Dados...", 0, 0, null);
                if (File.Exists(dbFilePath))
                {
                    File.Delete(dbFilePath);
                }
                string text1 = "Data Source = " + dbFilePath;
                SQLiteConnection connection1 = new SQLiteConnection(text1);
                connection1.Open();
                SQLiteCommand command1 = connection1.CreateCommand();
                int num1 = 0;
                builder1 = new StringBuilder();

                foreach (DataTable table1 in datasetSchema.Tables)
                {
                    this.OnStatusChanged("Criando tabela " + table1.TableName, datasetSchema.Tables.Count, num1, null);

                    builder1.Length = 0;
                    builder1.Append("CREATE TABLE ");
                    builder1.Append(table1.TableName);
                    builder1.Append("(");

                    foreach (DataColumn column1 in table1.Columns)
                    {
                        builder1.Append(column1.ColumnName);
                        builder1.Append(" ");
                        builder1.Append(Master.CompactFramework.Sync.XMLProvider.XMLProvider.DataColumnToSqlCeDataType(column1));

                        if (column1 != table1.Columns[table1.Columns.Count - 1])
                        {
                            builder1.Append(", ");
                        }
                    }

                    builder1.Append(")");
                    command1.CommandText = builder1.ToString();
                    command1.ExecuteNonQuery();
                    num1++;
                }
                command1.Dispose();
                connection1.Close();
            }
            catch (Exception exception1)
            {
                throw exception1;
            }
        }


        private static SQLiteCommand CreateDeleteCommand(DataTable dt)
        {
            SQLiteCommand command1 = new SQLiteCommand();
            StringBuilder builder1 = new StringBuilder();
            builder1.Append("DELETE FROM ");
            builder1.Append(dt.TableName);
            if (dt.PrimaryKey.Length <= 0)
            {
                throw new ApplicationException("Erro ao criar o comando de Delete. A tabela " + dt.TableName + " precisa de possuir ao menos um primary key.");
            }
            builder1.Append(" WHERE ");
            DataColumn[] columnArray1 = dt.PrimaryKey;
            for (int num1 = 0; num1 < columnArray1.Length; num1++)
            {
                DataColumn column1 = columnArray1[num1];
                if (column1 != dt.PrimaryKey[dt.PrimaryKey.Length - 1])
                {
                    builder1.Append(column1.ColumnName + " = ? AND ");
                }
                else
                {
                    builder1.Append(column1.ColumnName + " = ?");
                }
                command1.Parameters.Add(column1.ColumnName, Master.CompactFramework.Sync.XMLProvider.XMLProvider.DataColumnToSqlDbType(column1));
            }
            command1.CommandText = builder1.ToString();
            return command1;
        }


        private static SQLiteCommand CreateInsertCommand(DataTable dt)
        {
            SQLiteCommand command1 = new SQLiteCommand();
            StringBuilder builder1 = new StringBuilder();
            builder1.Append("INSERT INTO ");
            builder1.Append(dt.TableName);
            builder1.Append(" VALUES(");
            for (int num1 = 0; num1 < dt.Columns.Count; num1++)
            {
                if (num1 == (dt.Columns.Count - 1))
                {
                    builder1.Append("?)");
                }
                else
                {
                    builder1.Append("?, ");
                }
                if (dt.Columns[num1].MaxLength > 0)
                {
                    command1.Parameters.Add(dt.Columns[num1].ColumnName, Master.CompactFramework.Sync.XMLProvider.XMLProvider.DataColumnToSqlDbType(dt.Columns[num1]), dt.Columns[num1].MaxLength);
                }
                else
                {
                    command1.Parameters.Add(dt.Columns[num1].ColumnName, Master.CompactFramework.Sync.XMLProvider.XMLProvider.DataColumnToSqlDbType(dt.Columns[num1]));
                }
            }
            command1.CommandText = builder1.ToString();
            return command1;
        }


        private static SQLiteCommand CreateUpdateCommand(DataTable dt)
        {
            SQLiteCommand command1 = new SQLiteCommand();
            StringBuilder builder1 = new StringBuilder();
            builder1.Append("UPDATE ");
            builder1.Append(dt.TableName);
            builder1.Append(" SET ");
            for (int num1 = 0; num1 < dt.Columns.Count; num1++)
            {
                if (num1 == (dt.Columns.Count - 1))
                {
                    builder1.Append(dt.Columns[num1].ColumnName + " = ? ");
                }
                else
                {
                    builder1.Append(dt.Columns[num1].ColumnName + " = ?, ");
                }
                if (dt.Columns[num1].MaxLength > 0)
                {
                    command1.Parameters.Add(dt.Columns[num1].ColumnName, Master.CompactFramework.Sync.XMLProvider.XMLProvider.DataColumnToSqlDbType(dt.Columns[num1]), dt.Columns[num1].MaxLength);
                }
                else
                {
                    command1.Parameters.Add(dt.Columns[num1].ColumnName, Master.CompactFramework.Sync.XMLProvider.XMLProvider.DataColumnToSqlDbType(dt.Columns[num1]));
                }
            }
            if (dt.PrimaryKey.Length <= 0)
            {
                throw new ApplicationException("Erro ao criar o comando de Update. A tabela " + dt.TableName + " precisa de possuir ao menos um primary key.");
            }
            builder1.Append(" WHERE ");
            DataColumn[] columnArray1 = dt.PrimaryKey;
            for (int num2 = 0; num2 < columnArray1.Length; num2++)
            {
                DataColumn column1 = columnArray1[num2];
                if (column1 != dt.PrimaryKey[dt.PrimaryKey.Length - 1])
                {
                    builder1.Append(column1.ColumnName + " = ? AND ");
                }
                else
                {
                    builder1.Append(column1.ColumnName + " = ?");
                }
                command1.Parameters.Add(column1.ColumnName, Master.CompactFramework.Sync.XMLProvider.XMLProvider.DataColumnToSqlDbType(column1));
            }
            command1.CommandText = builder1.ToString();
            return command1;
        }

        private static string DataColumnToSqlCeDataType(DataColumn dc)
        {
            if (dc.DataType.Equals(typeof(string)))
            {
                if ((dc.MaxLength > 0xff) || (dc.MaxLength == -1))
                {
                    return "ntext";
                }
                return ("nvarchar(" + dc.MaxLength.ToString() + ")");
            }
            if (dc.DataType.Equals(typeof(int)))
            {
                return "int";
            }
            if (dc.DataType.Equals(typeof(byte[])))
            {
                return "image";
            }
            if (dc.DataType.Equals(typeof(long)))
            {
                return "bigint";
            }
            if (dc.DataType.Equals(typeof(ulong)))
            {
                return "bigint";
            }
            if (dc.DataType.Equals(typeof(decimal)))
            {
                return "numeric(21,5)";
            }
            if (dc.DataType.Equals(typeof(char)))
            {
                return "nchar";
            }
            if (dc.DataType.Equals(typeof(float)))
            {
                return "float";
            }
            if (dc.DataType.Equals(typeof(double)))
            {
                return "real";
            }
            if (dc.DataType.Equals(typeof(bool)))
            {
                return "bit";
            }
            if (dc.DataType.Equals(typeof(Guid)))
            {
                return "uniqueidentifier";
            }
            if (!dc.DataType.Equals(typeof(DateTime)))
            {
                throw new Exception("Tipo n\x00e3o reconhecido: " + dc.DataType.ToString());
            }
            return "datetime";
        }

        private static DbType DataColumnToSqlDbType(DataColumn dc)
        {
            if (dc.DataType.Equals(typeof(string)))
            {
                if ((dc.MaxLength <= 0xff) && (dc.MaxLength != -1))
                {
                    return DbType.String;
                }
                return DbType.String;
            }
            if (dc.DataType.Equals(typeof(int)))
            {
                return DbType.Int32;
            }
            if (dc.DataType.Equals(typeof(byte[])))
            {
                return DbType.Binary;
            }
            if (dc.DataType.Equals(typeof(long)))
            {
                return DbType.Int64;
            }
            if (dc.DataType.Equals(typeof(ulong)))
            {
                return DbType.UInt64;
            }
            if (dc.DataType.Equals(typeof(decimal)))
            {
                return DbType.Decimal;
            }
            if (dc.DataType.Equals(typeof(char)))
            {
                return DbType.String;
            }
            if (dc.DataType.Equals(typeof(float)))
            {
                return DbType.Decimal;
            }
            if (dc.DataType.Equals(typeof(double)))
            {
                return DbType.Double;
            }
            if (dc.DataType.Equals(typeof(bool)))
            {
                return DbType.Boolean;
            }
            if (dc.DataType.Equals(typeof(Guid)))
            {
                return DbType.Guid;
            }
            if (!dc.DataType.Equals(typeof(DateTime)))
            {
                throw new Exception("Tipo n\x00e3o reconhecido: " + dc.DataType.ToString());
            }
            return DbType.DateTime;
        }

        public void PopulateDB(string dbFilePath, string datasetFilePath, DataSet datasetSchema)
        {
            SQLiteConnection connection1 = null;
            SQLiteCommand command1 = null;
            SQLiteCommand command2 = null;
            SQLiteCommand command3 = null;
            XmlTextReader reader1 = null;

            try
            {
                string text1 = "Data Source = " + dbFilePath;
                connection1 = new SQLiteConnection(text1);
                connection1.Open();
                int num1 = 0;
                reader1 = new XmlTextReader(File.OpenRead(datasetFilePath));
                reader1.WhitespaceHandling = WhitespaceHandling.None;
                while (reader1.Read())
                {
                    if ((reader1.Depth == 2) && (reader1.NodeType == XmlNodeType.Element))
                    {
                        SQLiteCommand command4 = null;
                        string text2 = reader1.GetAttribute("s");
                        if ((text2 == "A") || (text2 == "U"))
                        {
                            command4 = command1;
                            for (int num2 = 0; num2 < (reader1.AttributeCount - 1); num2++)
                            {
                                Master.CompactFramework.Sync.XMLProvider.XMLProvider.AddCommandParameter(num2, reader1.GetAttribute(num2), command4);
                            }
                        }
                        else if (text2 == "D")
                        {
                            command4 = command3;
                            int num3 = 0;
                            for (int num4 = 0; num4 < (reader1.AttributeCount - 1); num4++)
                            {
                                reader1.MoveToAttribute(num4);
                                if (reader1.Name[0] == 'p')
                                {
                                    Master.CompactFramework.Sync.XMLProvider.XMLProvider.AddCommandParameter(num3, reader1.GetAttribute(num4), command4);
                                    num3++;
                                }
                            }
                        }
                        else if (text2 == "M")
                        {
                            command4 = command3;
                            int num5 = 0;
                            for (int num6 = 0; num6 < (reader1.AttributeCount - 1); num6++)
                            {
                                reader1.MoveToAttribute(num6);
                                if (reader1.Name[0] == 'p')
                                {
                                    Master.CompactFramework.Sync.XMLProvider.XMLProvider.AddCommandParameter(num5, reader1.GetAttribute(num6), command4);
                                    num5++;
                                }
                            }
                            try
                            {
                                command4.ExecuteNonQuery();
                            }
                            catch
                            {
                            }
                            command4 = command1;
                            for (int num7 = 0; num7 < (reader1.AttributeCount - 1); num7++)
                            {
                                Master.CompactFramework.Sync.XMLProvider.XMLProvider.AddCommandParameter(num7, reader1.GetAttribute(num7), command4);
                            }
                        }
                        command4.ExecuteNonQuery();
                    }
                    if (((reader1.Depth == 1) && (reader1.NodeType == XmlNodeType.Element)) && !reader1.IsEmptyElement)
                    {
                        this.OnStatusChanged("Processando " + reader1.Name, datasetSchema.Tables.Count, num1, null);
                        if (command1 != null)
                        {
                            command1.Dispose();
                        }
                        if (command2 != null)
                        {
                            command2.Dispose();
                        }
                        if (command3 != null)
                        {
                            command3.Dispose();
                        }
                        command1 = Master.CompactFramework.Sync.XMLProvider.XMLProvider.CreateInsertCommand(datasetSchema.Tables[reader1.Name]);
                        command1.Connection = connection1;
                        command1.Prepare();
                        try
                        {
                            command2 =
                                Master.CompactFramework.Sync.XMLProvider.XMLProvider.CreateUpdateCommand(
                                    datasetSchema.Tables[reader1.Name]);
                            command2.Connection = connection1;
                            command2.Prepare();
                        }
                        catch
                        {
                        } try
                        {
                            command3 =
                                Master.CompactFramework.Sync.XMLProvider.XMLProvider.CreateDeleteCommand(
                                    datasetSchema.Tables[reader1.Name]);
                            command3.Connection = connection1;
                            command3.Prepare();
                        }
                        catch
                        {
                        }
                        num1++;
                        string text3 = reader1.Name;
                    }
                }
            }
            catch (Exception exception1)
            {
                throw new ApplicationException("Erro ao executar o m\x00e9todo PopulateDB do provider XML", exception1);
            }
            finally
            {
                if (reader1 != null)
                {
                    reader1.Close();
                }
                if (command1 != null)
                {
                    command1.Dispose();
                }
                if (command2 != null)
                {
                    command2.Dispose();
                }
                if (command3 != null)
                {
                    command3.Dispose();
                }
                if ((connection1 != null) && (connection1.State != ConnectionState.Closed))
                {
                    connection1.Close();
                }
            }
        }

        #endregion

        public bool VerificaVersaoDLL(ref string mensagem, string versaoCompativel, string versaoAplicativoAvanteSales)
        {
            return true;
        }

        public void CargaImagem(string diretorioImagens,string codRevenda, long espacoDisponivelAndroid, string[] imagensExistentes)
        {

        }

        public void Carga(params string[] dataParams)
        {
            DateTime time1 = DateTime.Now;
            try
            {
                this.OnCheckPointStarted("Iniciando carga total", 0, 0, null);
                this.OnStatusChanged("Aguardando o servidor criar o pacote de carga...", 2, 1, null);
                string cargaPackFileURL = this.xmlService.Carga(dataParams);
                Uri cargaURI = new Uri(cargaPackFileURL);

                this.DownloadFile(cargaURI, @"\CargaAvanteSales.zip");
                OnDownloadCompleted();

                this.OnStatusChanged("Descompactando os arquivos de carga...", 1, 1, null);
                CompressionHelperFunctions.UnzipFile(@"\CargaAvanteSales.zip", @"\");
                File.Delete(@"\CargaAvanteSales.zip");

                this.OnStatusChanged("Criando o banco de dados...", 0, 0, null);
                this.OnStatusChanged("Lendo o arquivo de definição...", 0, 0, null);
                DataSet set1 = new DataSet();
                set1.ReadXmlSchema(@"\schema.xml");

                this.CreateDB(this.DataBaseFilePath, set1);
                this.PopulateDB(this.DataBaseFilePath, @"\dados.xml", set1);
                TimeSpan span1 = DateTime.Now.Subtract(time1);
                this.OnCheckPointCompleted("Carga total completa em " + span1.ToString(), 0, 0, null);
            }
            catch (IOException ex)
            {
                throw new ApplicationException("Erro ao executar o método de Carga no Provider XML. Ocorreu um IOException. Por favor, verifique se seu dispositivo possui memória livre.", ex);
            }
            catch (Exception ex)
            {
                if (ex is ApplicationException)
                    throw ex;
                else
                    throw new ApplicationException("Erro ao executar o método de Carga no Provider de XML", ex);
            }
            finally
            {
                if (File.Exists(@"\dados.xml"))
                    File.Delete(@"\dados.xml");

                if (File.Exists(@"\schema.xml"))
                    File.Delete(@"\schema.xml");
            }
        }

        public static void ChangeCultureInfo(System.Globalization.CultureInfo Culture)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = Culture;
        }

        public void CargaParcial(params string[] dataParams)
        {
            DateTime time1 = DateTime.Now;
            try
            {
                string cargaDirectory = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString());
                string cargaZipFilePath = Path.Combine(cargaDirectory, "CargaAvanteSales.zip");
                string cargaSchemaFilePath = Path.Combine(cargaDirectory, "schema.xml");

                ChangeCultureInfo(new CultureInfo("en-US"));
                // Cria o diretorio de carga
                Directory.CreateDirectory(cargaDirectory);

                this.OnCheckPointStarted("Iniciando carga parcial", 0, 0, null);
                this.OnStatusChanged("Aguardando o servidor criar o pacote de carga parcial...", 2, 1, null);
                string text1 = this.xmlService.CargaParcial(dataParams);
                Uri uri1 = new Uri(text1);

                this.DownloadFile(uri1, cargaZipFilePath);
                OnDownloadCompleted();

                this.OnStatusChanged("Descompactando os arquivos de carga parcial...", 1, 1, null);

                CompressionHelperFunctions.UnzipFile(cargaZipFilePath, cargaDirectory);
                File.Delete(cargaZipFilePath);
                this.OnStatusChanged("Lendo o arquivo de definição...", 0, 0, null);
                DataSet set1 = new DataSet();
                set1.ReadXmlSchema(cargaSchemaFilePath);
                this.PopulateDB(this.DataBaseFilePath, cargaSchemaFilePath, set1);
                //this.OnCheckPointCompleted("Carga parcial completa em " + span1.ToString(), 0, 0, null);
            }
            catch (Exception ex)
            {
                if (ex is ApplicationException)
                    throw ex;
                else
                    throw new ApplicationException("Erro ao executar o método de Carga no Provider de XML", ex);
            }
            finally
            {
                if (File.Exists(@"\dados.xml"))
                    File.Delete(@"\dados.xml");

                if (File.Exists(@"\schema.xml"))
                    File.Delete(@"\schema.xml");
            }
        }

        public void Descarga(DataSet dadosDescarga, params string[] dataParams)
        {
            DateTime time1 = DateTime.Now;
            try
            {
                this.OnCheckPointStarted("Iniciando descarga", 0, 0, null);
                string text1 = Path.GetTempFileName();
                string text2 = Path.GetTempFileName();
                XmlTextWriter writer1 = new XmlTextWriter(text1, Encoding.UTF8);
                dadosDescarga.WriteXml(writer1, XmlWriteMode.WriteSchema);
                writer1.Close();
                CompressionHelperFunctions.GZipFile(text2, text1);
                File.Delete(text1);
                FileStream stream1 = File.OpenRead(text2);
                byte[] buffer1 = new byte[stream1.Length];
                stream1.Read(buffer1, 0, (int)stream1.Length);
                stream1.Close();
                File.Delete(text2);
                string[] textArray1 = new string[dataParams.Length + 1];
                dataParams.CopyTo(textArray1, 1);
                textArray1[0] = Convert.ToBase64String(buffer1, 0, buffer1.Length);
                this.xmlService.Descarga(textArray1);
                TimeSpan span1 = DateTime.Now.Subtract(time1);
                this.OnCheckPointCompleted("Descarga completa em " + span1.ToString(), 0, 0, null);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro ao executar a descarga do provider XML.", ex);
            }
        }

        public void AtualizaCabs()
        {
            string tempDir = null;
            string zipFile = null;
            string[] files = null;
            DateTime startTime;

            try
            {
                startTime = DateTime.Now;
                tempDir = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString());
                zipFile = Path.Combine(tempDir, "CabFiles.zip");

                // Cria o diretorio temporário
                Directory.CreateDirectory(tempDir);
                this.OnCheckPointStarted("Iniciando atualização", 0, 0, null);
                this.OnStatusChanged("Aguardando resposta do servidor...", 2, 1, null);

                string returnUrl = this.xmlService.GetCabFiles();

                if (returnUrl != null)
                {
                    Uri uriDownload = new Uri(returnUrl);

                    // Baixa o arquivo
                    DownloadFile(uriDownload, zipFile);
                    OnDownloadCompleted();

                    // Descompacta o arquivo
                    this.OnStatusChanged("Descompactando arquivo de atualização...", 1, 1, null);
                    CompressionHelperFunctions.UnzipFile(zipFile, tempDir);
                    File.Delete(zipFile);

                    files = Directory.GetFiles(tempDir);

                    if (files != null && files.Length > 0)
                    {
                        for (int i = 0; i < files.Length; i++)
                        {
                            string file = files[i];
                            this.OnStatusChanged("Processando " + Path.GetFileNameWithoutExtension(file), files.Length, i, null);

                            Shell.ExecuteFile(file);
                            //Shell.WaitForSingleObject(Shell.ExecuteFile(file));
                        }
                    }
                }

                // Termina a carga
                TimeSpan spanEnd = DateTime.Now.Subtract(startTime);
                this.OnCheckPointCompleted("Completo em " + spanEnd.ToString(), 0, 0, null);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro realizando a atualização de arquivos.", ex);

            }
            finally
            {
                // [ deleta arquivos baixados ]
                if (tempDir != null)
                {
                    try
                    {
                        // Klelbio Delete
                        Directory.Delete(tempDir, true);
                    }
                    catch
                    {
                    }
                }
            }
        }

        #region Download Related Stuff

        private void DownloadFile(Uri downloadUri, string outFileName)
        {
            int retryCount = 0;

            while (retryCount <= 3)
            {
                try
                {
                    WebRequest webRequest = WebRequest.Create(downloadUri);
                    webRequest.Method = "GET";
                    webRequest.BeginGetResponse(new AsyncCallback(this.OnResponseReceived), webRequest);

                    this.downloadComplete.WaitOne();

                    if (File.Exists(outFileName))
                        File.Delete(outFileName);

                    if (!File.Exists(this.tempDownloadFile))
                    {
                        retryCount++;
                        continue;
                    }

                    File.Move(this.tempDownloadFile, outFileName);

                    return;
                }
                catch (IOException)
                {
                    // IOex do BeginGetResponse, ignorar já que o arquivo de saida não existirá
                    retryCount++;
                }
            }

            throw new ApplicationException("Erro ao efetuar o download do arquivo por três vezes. Por favor, tente novamente.");
        }

        private void OnResponseReceived(IAsyncResult ar)
        {
            this.tempDownloadFile = Path.GetTempFileName();
            FileStream fileOut = File.Create(this.tempDownloadFile);

            try
            {
                // Pega o response
                HttpWebRequest webRequest = (HttpWebRequest)ar.AsyncState;
                WebResponse webResponse = webRequest.EndGetResponse(ar);

                Stream responseStream = webResponse.GetResponseStream();

                // Muda o status
                long contentLen = webResponse.ContentLength;
                this.OnStatusChanged("Recebendo o arquivo de Carga...", (int)contentLen, 0, null);


                // Faz o loop lendo o stream de resposta
                int received = 0;
                int bufferSize = 1024 * 10;
                byte[] buffer = new byte[bufferSize];

                while (true)
                {
                    int read = responseStream.Read(buffer, 0, bufferSize);
                    received += read;

                    // Muda o status
                    string status = string.Format("Recebendo arquivo... {0}kb de {1}kb", (received / 1024), (contentLen / 1024));
                    this.OnStatusChanged(status, (int)contentLen, received, null);

                    // Se read == 0, terminou o stream
                    if (read == 0)
                        break;

                    fileOut.Write(buffer, 0, read);
                }

                fileOut.Close();

            }
            catch
            {
                // Deleta o arquivo de saida, o download não foi bem sucedido. O caller deverá assumir que se o arquivo
                // não existe o download não foi bem concluido

                fileOut.Close();
                File.Delete(tempDownloadFile);

            }
            finally
            {
                this.downloadComplete.Set();
            }
        }

        #endregion

        private void OnCheckPointCompleted(string statusMessage, int maxProgress, int currentProgress, Bitmap icon)
        {
            if (this.CheckPointCompleted != null)
                this.CheckPointCompleted(statusMessage, maxProgress, currentProgress, icon);
        }

        private void OnCheckPointStarted(string statusMessage, int maxProgress, int currentProgress, Bitmap icon)
        {
            if (this.CheckPointStarted != null)
                this.CheckPointStarted(statusMessage, maxProgress, currentProgress, icon);
        }

        private void OnStatusChanged(string statusMessage, int maxProgress, int currentProgress, Bitmap icon)
        {
            if (this.StatusChanged != null)
                this.StatusChanged(statusMessage, maxProgress, currentProgress, icon);
        }

        private void OnDownloadCompleted()
        {
            if (this.DownloadCompleted != null)
                this.DownloadCompleted(this, EventArgs.Empty);
        }
    }
}