using System;
using System.Data.Common;
using System.IO;
using System.Data;
using System.Threading;
using System.Text;
using System.Xml;
using System.Net;
using System.Collections;
using System.Globalization;
using Master.CompactFramework.Sync;
using Master.CompactFramework.Util.Compression;

#if ANDROID
using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
using SQLiteCommand = Mono.Data.Sqlite.SqliteCommand;
using SQLiteDataAdapter = Mono.Data.Sqlite.SqliteDataAdapter;
using SQLiteException = Mono.Data.Sqlite.SqliteException;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteTransaction = Mono.Data.Sqlite.SqliteTransaction;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;
using AvanteSales.SyncManager.SQLLiteProvider.SQLLiteService;
using Android.Graphics;
using Path = System.IO.Path;

#else
using System.Data.SQLite;
using Master.CompactFramework.Sync.SQLLiteprovider.SQLLiteService;
using System.Drawing;
#endif


namespace Master.CompactFramework.Sync.SQLLiteProvider
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public class SQLLiteDirectProvider : ISyncProvider
    {
        // Events
        public event SyncManager.StatusChangedEventHandler CheckPointCompleted;
        public event SyncManager.StatusChangedEventHandler CheckPointStarted;
        public event SyncManager.StatusChangedEventHandler StatusChanged;
        public event EventHandler DownloadCompleted;

        // Fields
        private string _dataBaseFilePath;
        private AutoResetEvent downloadComplete;
        //private RowsetLoader rowLoader;
        private SQLLiteService ssceService;
        private string tempDownloadFile;

        #region Properties

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
                return "SQLiteDirect";
            }
        }

        public string ServerAddress
        {
            get
            {
                return this.ssceService.Url;
            }
            set
            {
                this.ssceService.Url = value;
            }
        }

        #endregion

        // Methods
        public SQLLiteDirectProvider()
        {
            this.downloadComplete = new AutoResetEvent(false);
            ServicePointManager.DefaultConnectionLimit = 100;
            this.ssceService = new SQLLiteService();
            this.ssceService.Timeout = 180000;

            //// Configura o loader
            //BaseLoader.SetLicense("Flag Intelliwan - 10 Devices", "TP2SKKNNHT");
            //this.rowLoader = new RowsetLoader();
            
            //this.rowLoader.AutoNull = true;
            //this.rowLoader.Delimiter = "|";
            //this.rowLoader.ParseDates = false;
        }

        //Usado pra alterar a cultura da aplicação durante a carga.
#if !ANDROID
        [System.Runtime.InteropServices.DllImport("coredll.dll", EntryPoint = "ConvertDefaultLocale", SetLastError = true)]
        private static extern bool SetUserDefaultLCID(Int32 LCID);
#endif

        public static void ChangeCultureInfo(System.Globalization.CultureInfo Culture)
        {
#if ANDROID
            System.Threading.Thread.CurrentThread.CurrentCulture = Culture;
#else
            if (!SetUserDefaultLCID((Int32)Culture.LCID))
            {
                throw new Exception("Erro ao mudar a linguagem do sistema para portugues-brasil.");
            }
#endif

        }

        public bool VerificaVersaoDLL(ref string mensagem, string versaoCompativel, string versaoAplicativoAvanteSales)
        {
            try
            {
                bool versaoCompativelComAvante = this.ssceService.VerificaVersaoDLL(ref mensagem, versaoCompativel, versaoAplicativoAvanteSales);

                if (!versaoCompativelComAvante)
                {
                    throw new Exception(mensagem);
                }

                return true;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public void CargaImagem(string diretorioImagens, string codRevenda, long espacoDisponivelAndroid, string[] imagensExistentes)
        {
            try
            {
                this.OnStatusChanged("Aguardando imagens no servidor...", 2, 1, null);

                string urlZipImagens = this.ssceService.CargaImagem(espacoDisponivelAndroid, codRevenda, imagensExistentes);

                if (string.IsNullOrEmpty(urlZipImagens))
                    return;

                if (urlZipImagens.Contains("MSGERRO-"))
                    throw new Exception(urlZipImagens.Replace("MSGERRO-", string.Empty));

                string cargaDirectory = diretorioImagens;
                string cargaZipFilePath = Path.Combine(cargaDirectory, "CargaAvanteSalesImagens.zip");

                this.OnStatusChanged("Aguardando imagens no servidor...", 2, 2, null);

                ChangeCultureInfo(new CultureInfo("en-US"));

                if (imagensExistentes == null)
                {
                    RecriarPastaImagens(cargaDirectory);
                }

                if (!Directory.Exists(cargaDirectory))
                {
                    Directory.CreateDirectory(cargaDirectory);
                }

                Uri uriDownload = new Uri(urlZipImagens);
                this.DownloadFile(uriDownload, cargaZipFilePath);

                CompressionHelperFunctions.UnzipFile(cargaZipFilePath, cargaDirectory);

                File.Delete(cargaZipFilePath);

                var imagens64 = new DirectoryInfo(cargaDirectory).GetFiles("*.txt");
                int imagemAtual = 0;
                int qtdImagens = imagens64.Length;

                foreach (var arquivos in imagens64)
                {
                    imagemAtual++;
                    this.OnStatusChanged("Processando imagens...", qtdImagens, imagemAtual, null);

                    StreamReader reader = new StreamReader(arquivos.FullName);

                    byte[] imageBytes = Convert.FromBase64String(reader.ReadToEnd());
                    var bitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);

                    System.IO.Stream fos = new System.IO.FileStream(arquivos.FullName.Replace(".txt", ".png"), System.IO.FileMode.Create);
                    bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 0, fos);
                    fos.Flush();
                    fos.Close();
                    bitmap.Dispose();

                    reader.Dispose();

                    File.Delete(arquivos.FullName);
                }
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void RecriarPastaImagens(string diretorio)
        {
            if (Directory.Exists(diretorio))
            {
                var arquivos = new DirectoryInfo(diretorio).GetFiles();
                int arquivoAtual = 0;
                int qtdArquivo = arquivos.Length;

                foreach (var arquivoDeletar in new DirectoryInfo(diretorio).GetFiles())
                {
                    arquivoAtual++;
                    this.OnStatusChanged("Excluindo arquivos existentes...", qtdArquivo, arquivoAtual, null);

                    File.Delete(arquivoDeletar.FullName);
                }

                Directory.Delete(diretorio);
            }
        }

        public void Carga(params string[] dataParams)
        {
            string cargaDirectory = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString());
            string cargaZipFilePath = Path.Combine(cargaDirectory, "CargaAvanteSales.zip");
            string cargaSchemaFilePath = Path.Combine(cargaDirectory, "schema.xml");

            try
            {
                ChangeCultureInfo(new CultureInfo("en-US"));
                // Cria o diretorio de carga
                Directory.CreateDirectory(cargaDirectory);
                this.OnCheckPointStarted("Iniciando carga total", 0, 0, null);
                this.OnStatusChanged("Aguardando o servidor criar o pacote de carga...", 2, 1, null);

                // Baixa o arquivo
                string returnUrl = this.ssceService.Carga(dataParams);
                Uri uriDownload = new Uri(returnUrl);
                this.DownloadFile(uriDownload, cargaZipFilePath);
                OnDownloadCompleted();

                // Descompacta o banco de dados
                this.OnStatusChanged("Descompactando os arquivos de carga...", 1, 1, null);
                CompressionHelperFunctions.UnzipFile(cargaZipFilePath, cargaDirectory);
                File.Delete(cargaZipFilePath);

                // Cria o DB com schema
                this.OnStatusChanged("Criando o banco de dados...", 0, 0, null);
                this.OnStatusChanged("Lendo o arquivo de definição...", 0, 0, null);
                DataSet dataSetSchema = new DataSet();
                dataSetSchema.ReadXmlSchema(cargaSchemaFilePath);

                this.CreateDB(this.DataBaseFilePath, dataSetSchema);

                // Carrega as tabelas
                DateTime startTime = DateTime.Now;
                int currentTablePos = 0;
                string[] tableFiles = Directory.GetFiles(cargaDirectory, "*.table");
                string dbConnString = "Data Source = " + DataBaseFilePath;

                using (var sqlConn = new SQLiteConnection(dbConnString))
                {
                    sqlConn.Open();
                    foreach (string currentFile in tableFiles)
                    {
                        this.OnStatusChanged("Processando " + Path.GetFileNameWithoutExtension(currentFile),
                                                             tableFiles.Length, currentTablePos, null);
                        currentTablePos++;
                        TextReader textReader = File.OpenText(currentFile);
                        string line = textReader.ReadLine();
                        try
                        {
                            using (DbTransaction dbTrans = sqlConn.BeginTransaction())
                            {
                                using (DbCommand cmd = sqlConn.CreateCommand())
                                {
                                    //Preparando comando de insert
                                    const string commandInfo = "pragma table_info({0})";
                                    cmd.CommandText = string.Format(commandInfo,
                                                                    Path.GetFileNameWithoutExtension(currentFile));
                                    DbDataReader reader = cmd.ExecuteReader();
                                    const string commandText = "INSERT INTO {0} ({1}) VALUES({2})";
                                    var valores = new StringBuilder();
                                    var campos = new StringBuilder();
                                    while (reader.Read())
                                    {
                                        valores.Append("?");
                                        valores.Append(", ");
                                        campos.Append(reader.GetString(1));
                                        campos.Append(", ");
                                        cmd.Parameters.Add(new SQLiteParameter(GetDbType(reader.GetString(2))));
                                    }
                                    reader.Close();
                                    valores.Remove(valores.Length - 2, 2);
                                    campos.Remove(campos.Length - 2, 2);
                                    cmd.CommandText = string.Format(commandText,
                                                                    Path.GetFileNameWithoutExtension(currentFile),
                                                                    campos, valores);

                                    while (line != null)
                                    {
                                        //    // [ Tratamento para quebras de linhas no registro ]
                                        line = line.Replace("#CHAR#10", ((char)10).ToString()).Replace("#CHAR#13",
                                                                                                        ((char)13).
                                                                                                            ToString());
                                        var values = line.Split('|');

                                        for (int index = 0; index < values.Length; index++)
                                        {
                                            string value = values[index];

                                            if (string.IsNullOrEmpty(value))
                                                cmd.Parameters[index].Value = null;
                                            else
                                            {
                                                if (cmd.Parameters[index].DbType == DbType.Boolean)
                                                {
                                                    if (value.ToUpper() == "FALSE" || value == "0")
                                                    {
                                                        value = "0";
                                                    }
                                                    else
                                                    {
                                                        value = "1";
                                                    }
                                                }

                                                if (cmd.Parameters[index].DbType == DbType.String)
                                                    cmd.Parameters[index].Value = value.Trim();
                                                else
                                                {
                                                    cmd.Parameters[index].Value = value;
                                                }

                                            }

                                        }

                                        cmd.ExecuteNonQuery();
                                        line = textReader.ReadLine();
                                    }
                                }

                                //Atualizar campo de pedido validado para os pedidos que descem na carga
                                using (DbCommand cmdPedido = sqlConn.CreateCommand())
                                {
                                    cmdPedido.CommandText = "UPDATE PEDIDO SET BOL_PEDIDO_VALIDADO = 1";
                                    cmdPedido.ExecuteNonQuery();
                                }

                                dbTrans.Commit();
                            }
                        }
                        catch (Exception ex)
                        {

                            throw ex;
                        }
                        finally
                        {
                            //// Fecha...
                            //this.rowLoader.Close();
                            textReader.Close();
                            File.Delete(currentFile);
                        }
                    }
                    sqlConn.Close();
                }

                // Excluir arquivo de Schema criado e o diretorio criado
                File.Delete(cargaSchemaFilePath);
                Directory.Delete(cargaDirectory);

                // Termina a carga
                TimeSpan spanEnd = DateTime.Now.Subtract(startTime);
                this.OnCheckPointCompleted("Completo em " + spanEnd.ToString(), 0, 0, null);
                ChangeCultureInfo(new CultureInfo("pt-BR"));
            }
            catch (System.IO.IOException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("O Imei do aparelho é inválido para o vendedor"))
                    throw new ApplicationException("O Imei do aparelho é inválido para o vendedor.", ex);
                else if (ex.Message.Contains("Banco desatualizado"))
                    throw new ApplicationException("Carga interrompida: banco desatualizado. Solicite o TI de sua empresa para atualizar o Flexx para a última versão disponível.", ex);
                else if (ex.Message.Contains("Carga única"))
                    throw new ApplicationException("Carga interrompida: você já realizou uma carga completa hoje.");
                else if (ex.Message.Contains("Início expediente"))
                    throw new ApplicationException("Carga interrompida: você ainda não se encontra dentro do expediente.");
                else if (ex.Message.Contains("Fim expediente"))
                    throw new ApplicationException("Carga interrompida: seu expediente já foi encerrado.");
                else
                    throw new ApplicationException("Erro realizando a carga do provider SQLiteDirect.", ex);
            }
        }

        private void AddRow(string line, string table, string dataBaseFilePath)
        {


        }

        public void CargaParcial(params string[] dataParams)
        {
            string cargaDirectory = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString());
            string cargaZipFilePath = Path.Combine(cargaDirectory, "CargaAvanteSales.zip");
            string cargaSchemaFilePath = Path.Combine(cargaDirectory, "schema.xml");

            try
            {
                ChangeCultureInfo(new CultureInfo("en-US"));
                // Cria o diretorio de carga
                Directory.CreateDirectory(cargaDirectory);
                this.OnCheckPointStarted("Iniciando carga parcial", 0, 0, null);
                this.OnStatusChanged("Aguardando o servidor criar o pacote de carga parcial...", 2, 1, null);

                // Baixa o arquivo
                string returnUrl = this.ssceService.Carga(dataParams);
                Uri uriDownload = new Uri(returnUrl);
                this.DownloadFile(uriDownload, cargaZipFilePath);
                OnDownloadCompleted();

                // Descompacta o banco de dados
                this.OnStatusChanged("Descompactando os arquivos de carga parcial...", 1, 1, null);
                CompressionHelperFunctions.UnzipFile(cargaZipFilePath, cargaDirectory);
                File.Delete(cargaZipFilePath);

                // Cria o DB com schema
                this.OnStatusChanged("Alterando o banco de dados...", 0, 0, null);
                this.OnStatusChanged("Lendo o arquivo de definição...", 0, 0, null);
                DataSet dataSetSchema = new DataSet();
                dataSetSchema.ReadXmlSchema(cargaSchemaFilePath);

                // Carrega as tabelas
                DateTime startTime = DateTime.Now;
                int currentTablePos = 0;
                string[] tableFiles = Directory.GetFiles(cargaDirectory, "*.table");
                string dbConnString = "Data Source = " + DataBaseFilePath;

                using (var sqlConn = new SQLiteConnection(dbConnString))
                {
                    sqlConn.Open();
                    foreach (string currentFile in tableFiles)
                    {
                        this.OnStatusChanged("Processando " + Path.GetFileNameWithoutExtension(currentFile),
                                                             tableFiles.Length, currentTablePos, null);
                        currentTablePos++;
                        TextReader textReader = File.OpenText(currentFile);
                        string line = textReader.ReadLine();
                        try
                        {
                            using (DbTransaction dbTrans = sqlConn.BeginTransaction())
                            {
                                using (DbCommand cmd = sqlConn.CreateCommand())
                                {

                                    if (TabelaNaoPodeAlterar(Path.GetFileNameWithoutExtension(currentFile)))
                                        continue;

                                    string queryDelete = string.Format("DELETE FROM {0}", Path.GetFileNameWithoutExtension(currentFile));
                                    cmd.CommandText = queryDelete;
                                    cmd.ExecuteNonQuery();

                                    //Preparando comando de insert
                                    const string commandInfo = "pragma table_info({0})";
                                    cmd.CommandText = string.Format(commandInfo,
                                                                    Path.GetFileNameWithoutExtension(currentFile));
                                    DbDataReader reader = cmd.ExecuteReader();
                                    const string commandText = "INSERT INTO {0} ({1}) VALUES({2})";
                                    var valores = new StringBuilder();
                                    var campos = new StringBuilder();
                                    while (reader.Read())
                                    {
                                        valores.Append("?");
                                        valores.Append(", ");
                                        campos.Append(reader.GetString(1));
                                        campos.Append(", ");
                                        cmd.Parameters.Add(new SQLiteParameter(GetDbType(reader.GetString(2))));
                                    }
                                    reader.Close();
                                    valores.Remove(valores.Length - 2, 2);
                                    campos.Remove(campos.Length - 2, 2);
                                    cmd.CommandText = string.Format(commandText,
                                                                    Path.GetFileNameWithoutExtension(currentFile),
                                                                    campos, valores);

                                    while (line != null)
                                    {
                                        //    // [ Tratamento para quebras de linhas no registro ]
                                        line = line.Replace("#CHAR#10", ((char)10).ToString()).Replace("#CHAR#13",
                                                                                                        ((char)13).
                                                                                                            ToString());
                                        var values = line.Split('|');

                                        for (int index = 0; index < values.Length; index++)
                                        {
                                            string value = values[index];

                                            if (string.IsNullOrEmpty(value))
                                                cmd.Parameters[index].Value = null;
                                            else
                                            {
                                                if (cmd.Parameters[index].DbType == DbType.Boolean)
                                                {
                                                    if (value.ToUpper() == "FALSE" || value == "0")
                                                    {
                                                        value = "0";
                                                    }
                                                    else
                                                    {
                                                        value = "1";
                                                    }
                                                }

                                                if (cmd.Parameters[index].DbType == DbType.String)
                                                    cmd.Parameters[index].Value = value.Trim();
                                                else
                                                {
                                                    cmd.Parameters[index].Value = value;
                                                }

                                            }

                                        }

                                        cmd.ExecuteNonQuery();
                                        line = textReader.ReadLine();
                                    }
                                }
                                dbTrans.Commit();
                            }
                        }
                        catch (Exception ex)
                        {

                            throw ex;
                        }
                        finally
                        {
                            //// Fecha...
                            //this.rowLoader.Close();
                            textReader.Close();
                            File.Delete(currentFile);
                        }
                    }
                    sqlConn.Close();
                }

                // Excluir arquivo de Schema criado e o diretorio criado
                File.Delete(cargaSchemaFilePath);
                Directory.Delete(cargaDirectory);

                // Termina a carga
                TimeSpan spanEnd = DateTime.Now.Subtract(startTime);
                this.OnCheckPointCompleted("Completo em " + spanEnd.ToString(), 0, 0, null);
                ChangeCultureInfo(new CultureInfo("pt-BR"));
            }
            catch (System.IO.IOException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("O Imei do aparelho é inválido para o vendedor"))
                    throw new ApplicationException("O Imei do aparelho é inválido para o vendedor.", ex);
                else
                    throw new ApplicationException("Erro realizando a carga do provider SQLiteDirect.", ex);
            }
        }

        private bool TabelaNaoPodeAlterar(string tabela)
        {
            ArrayList a = new ArrayList() {"PEDIDO","ITEM_PEDIDO","RESPOSTA_PESQUISA_MERCADO","PRODUTO_COLETA_ESTOQUE","PDV_EMAIL",
            "INDENIZACAO","ITEM_INDENIZACAO","HISTORICO_MOTIVO","INFORMACOES_SINCRONIZACAO","EMPREGADO_EXPEDIENTE","MONITORAMENTO_VENDEDOR_ROTA"};

            if (a.Contains(tabela))
                return true;

            return false;
        }

        private void AlterarBanco(string dbFilePath, DataSet datasetSchema)
        {
            try
            {
                // Cria o banco de dados
                this.OnStatusChanged("Criando Banco de Dados...", 0, 0, null);

                string dbConnString = "Data Source = " + dbFilePath;

                SQLiteConnection sqlConn = new SQLiteConnection(dbConnString);
                sqlConn.Open();
                SQLiteCommand sqlCommand = sqlConn.CreateCommand();

                int tableCount = 0;

                foreach (DataTable table in datasetSchema.Tables)
                {
                    this.OnStatusChanged("Criando tabela " + table.TableName, datasetSchema.Tables.Count, tableCount, null);
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append("CREATE TABLE ");
                    stringBuilder.Append(table.TableName);
                    stringBuilder.Append("(");

                    foreach (DataColumn col in table.Columns)
                    {
                        stringBuilder.Append(col.ColumnName);
                        stringBuilder.Append(" ");
                        stringBuilder.Append(DataColumnToSqlLiteDataType(col));

                        if (col != table.Columns[table.Columns.Count - 1])
                        {
                            stringBuilder.Append(", ");
                        }
                    }

                    if (table.TableName.ToUpper() == "INFORMACOES_SINCRONIZACAO")
                    {
                        stringBuilder.Append(", DATA_ULTIMA_CARGA_COMPLETA ");
                        stringBuilder.Append("DATETIME");

                    }

                    stringBuilder.Append(")");

                    sqlCommand.CommandText = stringBuilder.ToString();
                    sqlCommand.ExecuteNonQuery();
                    tableCount++;
                }

                sqlCommand.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                throw ex;
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

                string returnUrl = this.ssceService.GetCabFiles();

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

        public void CreateDB(string dbFilePath, DataSet datasetSchema)
        {
            try
            {
                // Cria o banco de dados
                this.OnStatusChanged("Criando Banco de Dados...", 0, 0, null);
                if (File.Exists(dbFilePath))
                    File.Delete(dbFilePath);

                string dbConnString = "Data Source = " + dbFilePath;
                SQLiteConnection.CreateFile(dbFilePath);

                SQLiteConnection sqlConn = new SQLiteConnection(dbConnString);
                sqlConn.Open();
                SQLiteCommand sqlCommand = sqlConn.CreateCommand();

                int tableCount = 0;

                foreach (DataTable table in datasetSchema.Tables)
                {
                    this.OnStatusChanged("Criando tabela " + table.TableName, datasetSchema.Tables.Count, tableCount, null);
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append("CREATE TABLE ");
                    stringBuilder.Append(table.TableName);
                    stringBuilder.Append("(");

                    foreach (DataColumn col in table.Columns)
                    {
                        stringBuilder.Append(col.ColumnName);
                        stringBuilder.Append(" ");
                        stringBuilder.Append(DataColumnToSqlLiteDataType(col));

                        if (col != table.Columns[table.Columns.Count - 1])
                        {
                            stringBuilder.Append(", ");
                        }
                    }

                    if (table.TableName.ToUpper() == "INFORMACOES_SINCRONIZACAO")
                    {
                        stringBuilder.Append(", DATA_ULTIMA_CARGA_COMPLETA ");
                        stringBuilder.Append("DATETIME");

                        stringBuilder.Append(", DATA_ULTIMA_CARGA_PARCIAL ");
                        stringBuilder.Append("DATETIME");

                        stringBuilder.Append(", VERSAO_LOJA ");
                        stringBuilder.Append("VARCHAR(20)");
                    }

                    stringBuilder.Append(")");

                    sqlCommand.CommandText = stringBuilder.ToString();
                    sqlCommand.ExecuteNonQuery();
                    tableCount++;
                }

                sqlCommand.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string DataColumnToSqlLiteDataType(DataColumn dc)
        {
            if (dc.DataType.Equals(typeof(string)))
            {
                if ((dc.MaxLength > 0xff) || (dc.MaxLength == -1))
                {
                    return "text";
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
                return "decimal(21,5)";
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
                return "boolean";
            }
            if (dc.DataType.Equals(typeof(Guid)))
            {
                return "guid";
            }
            if (!dc.DataType.Equals(typeof(DateTime)))
            {
                throw new Exception("Tipo n\x00e3o reconhecido: " + dc.DataType.ToString());
            }
            return "datetime";
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

                this.ssceService.Descarga(textArray1);

                TimeSpan span1 = DateTime.Now.Subtract(time1);
                this.OnCheckPointCompleted("Descarga completa em " + span1.ToString(), 0, 0, null);
            }
            catch (Exception exception1)
            {
                if (exception1.Message.Contains("Início expediente"))
                    throw new ApplicationException("Descarga interrompida: você ainda não se encontra dentro do expediente.");
                else if (exception1.Message.Contains("Fim expediente"))
                    throw new ApplicationException("Descarga interrompida: seu expediente já foi encerrado.");
                else
                    throw exception1;// new ApplicationException("Erro ao executar a descarga do provider SQLiteProvider.", exception1);
            }
        }

        public DbType GetDbType(string type)
        {
            type = type.ToUpper();
            if (type.Contains("CHAR") || type.Contains("TEXT") || type.Contains("STRING") || type == "NOTE" || type == "MEMO")
                return DbType.String;
            if (type == "INT")
                return DbType.Int32;
            if (type.Contains("NUMERIC") ||
               type.Contains("MONEY") ||
                type.Contains("DECIMAL") ||
                type.Contains("CURRENCY"))
                return DbType.Decimal;
            if (type.Contains("TIME") ||
                type.Contains("DATE"))
                return DbType.DateTime;
            if (type == "SMALLINT")
                return DbType.Int16;
            if (type == "GUID" || type == "UNIQUEIDENTIFIER")
                return DbType.Guid;
            if (type == "COUNTER" ||
                type == "AUTOINCREMENT" ||
                type == "IDENTITY" ||
                type == "LONG" ||
                type == "INTEGER" ||
                type == "BIGINT")
                return DbType.Int64;
            if (type == "DOUBLE" ||
                type == "FLOAT")
                return DbType.Double;
            if (type == "TINYINT")
                return DbType.Byte;
            if (type == "BIT" || type == "YESNO" || type.Contains("BOOL") || type == "LOGICAL")
                return DbType.Boolean;
            if (type == "REAL")
                return DbType.Single;
            return DbType.Binary;
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
                    // Klelbio excluir o arquivo do servidor.
                    //File.Delete(this.tempDownloadFile);                     

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