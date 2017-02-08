using System;
using System.Reflection;
using System.Data;
#if ANDROID
using Android.Graphics;
using Mono.Data.Sqlite;
#else
using System.Data.SQLite;
using System.Windows.Forms;
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
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Specialized;
using System.Xml;
using System.Collections;
using AvanteSales.SystemFramework;
using Path = System.IO.Path;

namespace AvanteSales
{
    /// <summary>
    /// Summary description for DataAccess.
    /// </summary>
    public sealed class CSDataAccess
    {
        #region [ Variáveis ]

        // Conexão ao banco de dados
        private string dataBaseFile;
        private string connectionString;
        private SQLiteConnection sqlConnection = null;
        private SQLiteTransaction transaction = null;
        //private SQLiteCommand sqlComm = null;
        private SQLiteCommand sqlComInterno = null;
        private SQLiteCommand sqlComInterno2 = null;

        // [ Tabela hash para armazenar objetos SQLiteCommand preparados ]
        public TypedHashtable preparedCommands = null;
        private const int preparedCommandsCapacity = 60;

        public TypedHashtable cachedResults = null;
        private const int cachedResultsCapacity = 300;

        // Singleton Class...
        public static readonly CSDataAccess Instance = new CSDataAccess();

        // Delegates
        public delegate void UpdateStatus(string statusMessage, int maxProgress, int currentProgress,
#if !ANDROID
 System.Drawing.Bitmap icon
#else
 Bitmap icon
#endif

);

        #endregion

        #region Properties

        public SQLiteConnection Connection
        {
            get
            {
                return sqlConnection;
            }
        }

        public SQLiteTransaction Transacao
        {
            get
            {
                return transaction;
            }
            set
            {
                transaction = value;
            }
        }

        #endregion

        /// <summary>
        /// Conecta no banco. Se o banco não existir cria um.
        /// </summary>
        private CSDataAccess()
        {
            AbreConexao();

            // [ Inicializa hash de comandos preparados ]
            preparedCommands = new TypedHashtable(preparedCommandsCapacity);
            cachedResults = new TypedHashtable(cachedResultsCapacity);
        }

        #region ExecuteNonQuery - OK 22/07/2003 Guilherme Magalhães

        public int ExecuteNonQuery(string sqlQuery)
        {
            SQLiteCommand sqlComm = new SQLiteCommand(sqlQuery, sqlConnection);
            try
            {
                if (this.Transacao != null)
                    sqlComm.Transaction = this.Transacao;

                return sqlComm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return 0;
            }
            finally
            {
                sqlComm.Dispose();
            }
        }

        public int ExecuteNonQuery(string sqlQuery, params SQLiteParameter[] sqlParams)
        {
            SQLiteCommand sqlComm = new SQLiteCommand(sqlQuery, sqlConnection);
            try
            {
                //sqlComm = sqlConnection.CreateCommand();
                //sqlComm.CommandText = sqlQuery;

                if (this.Transacao != null)
                    sqlComm.Transaction = this.Transacao;

                foreach (SQLiteParameter sqlParam in sqlParams)
                {
                    if (sqlParam != null)
                        sqlComm.Parameters.Add(sqlParam);
                }

                sqlComm.Prepare();
                return sqlComm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return 0;
            }
            finally
            {
                sqlComm.Dispose();
            }
        }

        #endregion

        #region ExecuteReader - OK 22/07/2003 Guilherme Magalhães

        /// <summary>
        /// Executa a query SQL e retorna um SQLiteDataReader
        /// </summary>
        /// <param name="sqlQuery">Query SQL</param>
        /// <returns>SQLiteDataReader com o resultado da Query</returns>
        public SQLiteDataReader ExecuteReader(string sqlQuery)
        {
            try
            {
                SQLiteCommand sqlComm = new SQLiteCommand(sqlQuery, sqlConnection);
                //if (sqlComm != null)
                //    sqlComm.Dispose();

                //sqlComm = sqlConnection.CreateCommand();
                //sqlComm.CommandText = sqlQuery;

                if (sqlConnection != null &&
                    sqlConnection.State != ConnectionState.Open)
                    sqlConnection.Open();

                SQLiteDataReader sqlDataReader = sqlComm.ExecuteReader();
                //sqlComm.Dispose();

                return sqlDataReader;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Executa a query SQL e retorna um SQLiteDataReader
        /// </summary>
        /// <param name="sqlQuery">Query SQL</param>
        /// <param name="sqlParams">Parms SQL</param>
        /// bool preparado indica executar um reader preparado
        /// <returns>SQLiteDataReader com o resultado da Query</returns>
        public SQLiteDataReader ExecuteReader(string sqlQuery, bool preparado, params SQLiteParameter[] sqlParams)
        {
            try
            {
                if (sqlComInterno != null)
                    sqlComInterno.Dispose();
                sqlComInterno = sqlConnection.CreateCommand();
                sqlComInterno.CommandText = sqlQuery;

                foreach (SQLiteParameter sqlParam in sqlParams)
                {
                    sqlComInterno.Parameters.Add(sqlParam);
                }

                sqlComInterno.Prepare();
                return sqlComInterno.ExecuteReader();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Executa a query SQL e retorna um SQLiteDataReader
        /// </summary>
        /// <param name="sqlQuery">Query SQL</param>
        /// <param name="sqlParams">Parms SQL</param>
        /// <returns>SQLiteDataReader com o resultado da Query</returns>
        public SQLiteDataReader ExecuteReader(string sqlQuery, params SQLiteParameter[] sqlParams)
        {
            try
            {
                SQLiteCommand sqlComm = new SQLiteCommand(sqlQuery, sqlConnection);
                //if (sqlComm != null)
                //    sqlComm.Dispose();
                //sqlComm = sqlConnection.CreateCommand();
                //sqlComm.CommandText = sqlQuery;

                foreach (SQLiteParameter sqlParam in sqlParams)
                {
                    sqlComm.Parameters.Add(sqlParam);
                }

                sqlComm.Prepare();
                return sqlComm.ExecuteReader();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public SQLiteDataReader ExecuteReader(ref SQLiteCommand sqlComm, string sqlQuery, params SQLiteParameter[] sqlParams)
        {
            try
            {
                SQLiteDataReader sqlDataReader;

                if (sqlComm == null)
                {
                    sqlComm = sqlConnection.CreateCommand();
                    sqlComm.CommandText = sqlQuery;

                    if (this.Transacao != null)
                        sqlComm.Transaction = this.Transacao;

                    foreach (SQLiteParameter sqlParam in sqlParams)
                    {
                        sqlComm.Parameters.Add(sqlParam);
                    }

                    sqlComm.Prepare();

                }
                else
                {
                    foreach (SQLiteParameter sqlParam in sqlParams)
                    {
                        sqlComm.Parameters[sqlParam.ParameterName].Value = sqlParam.Value;
                    }
                }

                sqlDataReader = sqlComm.ExecuteReader();

                return sqlDataReader;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Executa a query SQL e retorna um SQLiteDataReader
        /// </summary>
        /// <param name="sqlQuery">Query SQL</param>
        /// <param name="sqlParams">Parms SQL</param>
        /// <param name="commBehavior">CommandBehavior da Query</param>
        /// <returns>SQLiteDataReader com o resultado da Query</returns>
        public SQLiteDataReader ExecuteReader(string sqlQuery, CommandBehavior commBehavior, params SQLiteParameter[] sqlParams)
        {
            try
            {
                SQLiteCommand sqlComm = new SQLiteCommand(sqlQuery, sqlConnection);
                //if (sqlComm != null)
                //    sqlComm.Dispose();

                //sqlComm = sqlConnection.CreateCommand();
                //sqlComm.CommandText = sqlQuery;

                foreach (SQLiteParameter sqlParam in sqlParams)
                {
                    sqlComm.Parameters.Add(sqlParam);
                }

                sqlComm.Prepare();
                return sqlComm.ExecuteReader(commBehavior);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // Novos metodos
        public SQLiteDataReader ExecuteReaderEndPDV(string sqlQuery, params SQLiteParameter[] sqlParams)
        {
            try
            {
                SQLiteDataReader sqlReaderReturn;

                if (sqlComInterno2 != null)
                    sqlComInterno2.Dispose();

                sqlComInterno2 = sqlConnection.CreateCommand();
                sqlComInterno2.CommandText = sqlQuery;

                foreach (SQLiteParameter sqlParam in sqlParams)
                {
                    sqlComInterno2.Parameters.Add(sqlParam);
                }

                sqlComInterno2.Prepare();
                sqlReaderReturn = sqlComInterno2.ExecuteReader();

                return sqlReaderReturn;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public SQLiteDataReader ExecuteReaderEnderecos(params SQLiteParameter[] sqlParams)
        {
            try
            {
                foreach (SQLiteParameter sqlParam in sqlParams)
                {
                    sqlComInterno2.Parameters[sqlParam.ParameterName].Value = sqlParam.Value;
                }

                return sqlComInterno2.ExecuteReader();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion

        #region ExecuteScalar - OK 22/07/2003 Guilherme Magalhães

        public object ExecuteScalar(string sqlQuery)
        {
            SQLiteCommand sqlComm = new SQLiteCommand(sqlQuery, sqlConnection);
            try
            {
                //if (sqlComm != null)
                //    sqlComm.Dispose();
                //sqlComm = sqlConnection.CreateCommand();
                ////SQLiteCommand sqlComm = sqlConnection.CreateCommand();
                //sqlComm.CommandText = sqlQuery;
                if (this.Transacao != null)
                    sqlComm.Transaction = this.Transacao;

                return sqlComm.ExecuteScalar();
            }
            catch (Exception ex)
            {
                return 0;
            }
            finally
            {
                sqlComm.Dispose();
            }
        }

        public object ExecuteScalar(string sqlQuery, params SQLiteParameter[] sqlParams)
        {
            SQLiteCommand sqlComm = new SQLiteCommand(sqlQuery, sqlConnection);
            try
            {
                //if (sqlComm != null)
                //    sqlComm.Dispose();
                //sqlComm = sqlConnection.CreateCommand();
                ////SQLiteCommand sqlComm = sqlConnection.CreateCommand();
                //sqlComm.CommandText = sqlQuery;
                if (this.Transacao != null)
                    sqlComm.Transaction = this.Transacao;

                foreach (SQLiteParameter sqlParam in sqlParams)
                {
                    if (sqlParam != null)
                        sqlComm.Parameters.Add(sqlParam);
                }

                return sqlComm.ExecuteScalar();
            }
            catch (Exception ex)
            {
                return 0;
            }
            finally
            {
                sqlComm.Dispose();
            }
        }

        #endregion

        #region ExecuteDataTable - OK 22/07/2003 Guilherme Magalhães

        /// <summary>
        /// Retorna os resultados em um DataTable
        /// OBS: Uso de memória extremo! Use apenas quando necessário
        /// </summary>
        /// <param name="sqlQuery">Query SQL</param>
        /// <returns>DataTable com o resultado da Query</returns>
        public DataTable ExecuteDataTable(string sqlQuery)
        {
            SQLiteCommand sqlComm = new SQLiteCommand(sqlQuery, sqlConnection);
            //sqlAdapter.Fill(dataTable);
            try
            {
                return Fill(sqlComm);
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                sqlComm.Dispose();
            }

            // Delete o Adapter
            //sqlAdapter.Dispose();            
        }
        public DataTable ExecuteDataTableTransform(string sqlQuery, params SQLiteParameter[] sqlParams)
        {
            SQLiteCommand sqlComm = new SQLiteCommand(sqlQuery, sqlConnection);
            SQLiteDataAdapter sqlAdapter = new SQLiteDataAdapter(sqlComm);
            try
            {
                DataTable dataTable = new DataTable();
                foreach (SQLiteParameter sqlParam in sqlParams)
                {
                    sqlComm.Parameters.Add(sqlParam);
                }

                sqlAdapter.FillSchema(dataTable, System.Data.SchemaType.Source);
                foreach (DataColumn dc in dataTable.Columns)
                {
                    if (dc.DataType == System.Type.GetType("System.DateTime"))
                        dc.DataType = System.Type.GetType("System.String");
                }
                dataTable = Fill(sqlComm);

                return dataTable;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                // Delete o Adapter
                sqlComm.Dispose();
                sqlAdapter.Dispose();
            }
        }
        /// <summary>
        /// Retorna os resultados em um DataTable
        /// OBS: Uso de memória extremo! Use apenas quando necessário
        /// </summary>
        /// <param name="sqlQuery">Query SQL</param>
        /// <param name="sqlParams">Params SQL</param>
        /// <returns>DataTable com o resultado da Query</returns>
        public DataTable ExecuteDataTable(string sqlQuery, params SQLiteParameter[] sqlParams)
        {
            SQLiteCommand sqlComm = new SQLiteCommand(sqlQuery, sqlConnection);
            SQLiteDataAdapter sqlAdapter = new SQLiteDataAdapter(sqlComm);
            try
            {

                foreach (SQLiteParameter sqlParam in sqlParams)
                {
                    sqlComm.Parameters.Add(sqlParam);
                }

                return Fill(sqlComm);
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                sqlComm.Dispose();
                sqlAdapter.Dispose();
            }
        }

        private static DataTable Fill(SQLiteCommand sqlComm)
        {
            DataTable dt = new DataTable();
            SQLiteDataReader reader = sqlComm.ExecuteReader();

            // Add all the columns.
            try
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    DataColumn col = new DataColumn();
                    col.DataType = reader.GetFieldType(i);
                    col.ColumnName = reader.GetName(i);
                    dt.Columns.Add(col);
                }

                while (reader.Read())
                {
                    DataRow row = dt.NewRow();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        // Ignore Null fields.
                        if (reader.IsDBNull(i)) continue;

                        if (reader.GetFieldType(i) == typeof(String))
                        {
                            row[dt.Columns[i].ColumnName] = reader.GetString(i);
                        }
                        else if (reader.GetFieldType(i) == typeof(Int16))
                        {
                            row[dt.Columns[i].ColumnName] = reader.GetInt16(i);
                        }
                        else if (reader.GetFieldType(i) == typeof(Int32))
                        {
                            row[dt.Columns[i].ColumnName] = int.Parse(reader.GetValue(i).ToString());
                        }
                        else if (reader.GetFieldType(i) == typeof(Int64))
                        {
                            row[dt.Columns[i].ColumnName] = reader.GetInt64(i);
                        }
                        else if (reader.GetFieldType(i) == typeof(Boolean))
                        {
                            row[dt.Columns[i].ColumnName] = reader.GetBoolean(i); ;
                        }
                        else if (reader.GetFieldType(i) == typeof(Byte))
                        {
                            row[dt.Columns[i].ColumnName] = reader.GetByte(i);
                        }
                        else if (reader.GetFieldType(i) == typeof(Char))
                        {
                            row[dt.Columns[i].ColumnName] = reader.GetChar(i);
                        }
                        else if (reader.GetFieldType(i) == typeof(DateTime))
                        {
                            row[dt.Columns[i].ColumnName] = reader.GetDateTime(i);
                        }
                        else if (reader.GetFieldType(i) == typeof(Decimal))
                        {
                            row[dt.Columns[i].ColumnName] = decimal.Parse(reader.GetValue(i).ToString());
                        }
                        else if (reader.GetFieldType(i) == typeof(Double))
                        {
                            row[dt.Columns[i].ColumnName] = double.Parse(reader.GetValue(i).ToString());
                        }
                        else if (reader.GetFieldType(i) == typeof(float))
                        {
                            row[dt.Columns[i].ColumnName] = float.Parse(reader.GetValue(i).ToString());
                        }
                        else if (reader.GetFieldType(i) == typeof(Guid))
                        {
                            row[dt.Columns[i].ColumnName] = reader.GetGuid(i);
                        }
                    }

                    dt.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                reader.Close();
            }

            return dt;
        }

        #endregion

        /// <summary>
        /// Comprime o banco de dados
        /// </summary>
        public void CompressDB()
        {
            //TODO: Acertar o metodo que compacta o banco de dados
            //try
            //{
            //    if (sqlConnection != null && sqlConnection.State == ConnectionState.Open)
            //    {
            //        sqlConnection.Close();
            //        sqlConnection.Dispose();
            //    }

            //    GC.Collect();

            //    dataBaseFile = Path.Combine(CSGlobal.GetCurrentDirectoryDB(), CSConfiguracao.GetConfig("dbFile") + CSGlobal.COD_REVENDA + ".sdf");                
            //    string dataBaseTmp = CSGlobal.GetCurrentDirectoryDB() + "\\TmpDb.sdf";                

            //    connectionString = "Data Source = " + dataBaseFile;

            //    /* ---------- Compacta o banco se ele exisitir ---------- */
            //    SQLiteEngine sqlEngine = new SQLiteEngine(connectionString);

            //    // Apaga o banco temporario
            //    if (File.Exists(dataBaseTmp))
            //        File.Delete(dataBaseTmp);

            //    try
            //    {
            //        // Faz a compactacao do banco para o banco temporario
            //        sqlEngine.Compact("Data Source=" + dataBaseTmp);
            //        sqlEngine.Dispose();

            //    } catch
            //    {
            //        // Apaga o banco temporario
            //        if (File.Exists(dataBaseTmp))
            //            File.Delete(dataBaseTmp);

            //        return;
            //    }

            //    // Copia o banco temporario em cima do banco antigo
            //    File.Delete(dataBaseFile);
            //    File.Move(dataBaseTmp, dataBaseFile);

            //    // Apaga o banco temporario
            //    if (File.Exists(dataBaseTmp))
            //        File.Delete(dataBaseTmp);

            //} catch (Exception ex)
            //{
            //    //System.Windows.Forms.SqliteDataAdapter(ex.ToString());    
            //    throw new Exception("Erro ao compactar o banco de dados. \n" + ex.Message, ex);
            //}
        }

        /// <summary>
        /// Apaga fisicamente o banco de dados e recria todas as informações
        /// </summary>
        public void ResetaBancoDeDados()
        {
            try
            {
                if (sqlConnection != null)
                {
                    // Fecha a conexão
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }

                if (File.Exists(dataBaseFile))
                {
                    // Cria um backup do banco de dados antigo para que se der qualquer erro possa se voltado
#if ANDROID
                    File.Move(dataBaseFile, CSGlobal.GetCurrentDirectoryDB() + "/AvanteSales_Backup.sdf");
#else
                    File.Move(dataBaseFile, CSGlobal.GetCurrentDirectoryDB() + "\\AvanteSales_Backup.sdf");
#endif
                    // Apaga o banco atual
                    File.Delete(dataBaseFile);
                }

                // Cria um novo banco
                SQLiteConnection.CreateFile(dataBaseFile);
                SQLiteConnection sqlEngine = new SQLiteConnection(connectionString);
                sqlEngine.Dispose();

                // Reabre a conexao
                sqlConnection = new SQLiteConnection(connectionString);
                sqlConnection.Open();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao resetar o banco de dados.", ex);
            }
        }

        /// <summary>
        /// Fecha a conexao. Usada para fazer a carga do sistema
        /// </summary>
        public void FechaConexao()
        {
            if (sqlConnection == null)
                return;

            // [ libera tabela hash de comandos preparados ]
            DisposePreparedCommands(TypedHashtable.HashtableEntryType.All);

            // [ libera tabela hash de cache de resultados ]
            DisposeCachedResults(TypedHashtable.HashtableEntryType.All);

            if (sqlConnection.State == ConnectionState.Open)
                sqlConnection.Close();
        }

        public void DisposeConexao()
        {
            if (sqlConnection == null)
                return;

            // [ libera tabela hash de comandos preparados ]
            DisposePreparedCommands(TypedHashtable.HashtableEntryType.All);

            // [ libera tabela hash de cache de resultados ]
            DisposeCachedResults(TypedHashtable.HashtableEntryType.All);

            sqlConnection.Dispose();
        }

        /// <summary>
        /// Abre a conexao novamente.
        /// </summary>
        public void AbreConexao()
        {
            try
            {
                string dataBaseFile = "Data Source = " + Path.Combine(CSGlobal.GetCurrentDirectoryDB(), CSConfiguracao.GetConfig("dbFile") + CSGlobal.COD_REVENDA + ".sdf; MultipleActiveResultSets=True");

                //if (sqlConnection == null)
                //{threading
                // Abre uma conexão com o banco
                if (sqlConnection != null)
                    sqlConnection.Dispose();


                sqlConnection = new SQLiteConnection(dataBaseFile);
                //}


                if (sqlConnection.State == ConnectionState.Closed && CSDataAccess.DataBaseExists(CSGlobal.COD_REVENDA))
                {
                    sqlConnection.ConnectionString = dataBaseFile;

                    try
                    {
                        sqlConnection.Open();

                    }
                    catch (Exception e)
                    {
#if !ANDROID
                    MessageBox.Show("O banco de dados parece estar corrompido!\nO AvanteSales tentará recuperar o arquivo de banco de dados agora...", "Banco de Dados", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                    Cursor.Current = Cursors.WaitCursor;
#endif
                        try
                        {
                            // [ Tenta recuperar o banco caso esteja corrompido ]
                            CompressDB();

                            sqlConnection.Open();
#if !ANDROID

                        Cursor.Current = Cursors.Default;
                        MessageBox.Show("O banco de dados foi recuperado com sucesso!", "Banco de Dados", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
#endif

                        }
                        catch
                        {
#if !ANDROID
                        Cursor.Current = Cursors.Default;
                        MessageBox.Show("Não foi possível recuperar o banco de dados!\nEntre em contato com a Flag IntelliWan.", "Banco de Dados", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);

                        // [ Força sair da aplicação ]
                        Program.Stack.ExitApp();
#endif

                            throw e;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Cria os indice no banco de dados
        /// </summary>
        public void CriaIndiceBanco(string nomeTabela, string nomeIndice, string camposIndice)
        {
            if (TableExists(nomeTabela))
            {
                //ExecuteNonQuery("DROP INDEX " + nomeTabela + "." + nomeIndice);

                ExecuteNonQuery("CREATE INDEX " + nomeIndice + " ON " + nomeTabela + "(" + camposIndice + ")");
            }
        }

        /// <summary>
        /// Cria todos os indice necessarios para o banco de dados AvanteSales
        /// </summary>
        public void ConstroiIndices(UpdateStatus objMsg)
        {
            AbreConexao();

            // [ Verifica qual política de preços será utilizada ]
            string politica = CSTiposDistribPolicitcaPrecos.GetPoliticaPreco().ToString();

            int numeroTabelasTotal = 0;

            switch (politica)
            {
                case "1":
                    numeroTabelasTotal = 63;
                    break;
                case "2":
                    numeroTabelasTotal = 134;
                    break;
                case "3":
                    numeroTabelasTotal = 11;
                    break;
                case "9023":
                    numeroTabelasTotal = 82;
                    break;
                case "9123":
                    numeroTabelasTotal = 145;
                    break;
                case "9103":
                    numeroTabelasTotal = 74;
                    break;
                case "9120":
                    numeroTabelasTotal = 134;
                    break;
            }

            int numeroTabelaAtual = 1;

            //************** Inicio Construindo Indice da tabela TIPO_TELEFONE *************************
            objMsg("Construindo Indice da Tabela " + "TIPO_TELEFONE", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("TIPO_TELEFONE", "IDX_TIPO_TELEFONE_1", "COD_TIPO_TELEFONE");
            //************** Fim Construindo Indice da tabela TIPO_TELEFONE *************************

            //************** Inicio Construindo Indice da tabela TIPO LAZER  *************************
            objMsg("Construindo Indice da Tabela " + "TIPO_LAZER", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("TIPO_LAZER", "IDX_TIPO_LAZER_1", "COD_TIPO_LAZER");
            //************** Fim Construindo Indice da tabela TIPO_LAZER *************************

            //************** Inicio Construindo Indice da tabela TIPO_ENDERECO *************************
            objMsg("Construindo Indice da Tabela " + "TIPO_ENDERECO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("TIPO_ENDERECO", "IDX_TIPO_ENDERECO_1", "COD_TIPO_ENDERECO");
            //************** Fim Construindo Indice da tabela TIPO_ENDERECO *************************

            //************** Inicio Construindo Indice da tabela TIPO_DISTRIB_POLITICA_PRECO *************************
            objMsg("Construindo Indice da Tabela " + "TIPO_DISTRIB_POLITICA_PRECO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("TIPO_DISTRIB_POLITICA_PRECO", "IDX_TIPO_DISTRIB_POLITICA_PRECO_1", "COD_TIPO_DISTRIBUICAO_POLITICA");
            //************** Fim Construindo Indice da tabela TIPO_DISTRIB_POLITICA_PRECO *************************

            //************** Inicio Construindo Indice da tabela TELEFONE_PDV *************************
            objMsg("Construindo Indice da Tabela " + "TELEFONE_PDV", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("TELEFONE_PDV", "IDX_TELEFONE_PDV_1", "COD_PDV,COD_TIPO_TELEFONE");
            //************** Fim Construindo Indice da tabela TELEFONE_PDV  *************************

            //************** Inicio Construindo Indice da tabela TABELA_PRECO *************************
            objMsg("Construindo Indice da Tabela " + "TABELA_PRECO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("TABELA_PRECO", "IDX_TABELA_PRECO_1", "COD_TABELA_PRECO");
            //************** Fim Construindo Indice da tabela TABELA_PRECO  *************************

            //************** Inicio Construindo Indice da tabela SEGMENTACAO *************************
            objMsg("Construindo Indice da Tabela " + "SEGMENTACAO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("SEGMENTACAO", "IDX_SEGMENTACAO_1", "COD_SEGMENTACAO");
            //************** Fim Construindo Indice da tabela SEGMENTACAO  *************************

            //************** Inicio Construindo Indice da tabela PRODUTO_CATEGORIA *************************
            objMsg("Construindo Indice da Tabela " + "PRODUTO_CATEGORIA", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("PRODUTO_CATEGORIA", "IDX_PRODUTO_CATEGORIA_1", "COD_PRODUTO,COD_CATEGORIA");
            //************** Fim Construindo Indice da tabela PRODUTO_CATEGORIA  *************************

            //************** Inicio Construindo Indice da tabela PRODUTO *************************
            objMsg("Construindo Indice da Tabela " + "PRODUTO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("PRODUTO", "IDX_PRODUTO_1", "COD_PRODUTO,COD_GRUPO,IND_ATIVO,COD_TIPO_DISTRIBUICAO_POLITICA,IND_PRODUTO_COM_PRECO,QTD_ESTOQUE");
            CriaIndiceBanco("PRODUTO", "IDX_PRODUTO_2", "COD_GRUPO,COD_FAMILIA_PRODUTO");
            CriaIndiceBanco("PRODUTO", "IDX_PRODUTO_3", "COD_TIPO_DISTRIBUICAO_POLITICA");
            CriaIndiceBanco("PRODUTO", "IDX_PRODUTO_4", "IND_ATIVO");
            //************** Fim Construindo Indice da tabela PRODUTO *************************

            //************** Inicio Construindo Indice da tabela PRODUTO_CONJUNTO *************************
            objMsg("Construindo Indice da Tabela " + "PRODUTO_CONJUNTO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("PRODUTO_CONJUNTO", "IDX_PRODUTO_CONJUNTO_1", "COD_PRODUTO_CONJUNTO,COD_PRODUTO_COMPOSICAO");
            //************** Fim Construindo Indice da tabela PRODUTO_CONJUNTO  *************************

            //************** Inicio Construindo Indice da tabela PRODUTO_REGRA_COMBO *************************
            objMsg("Construindo Indice da Tabela " + "PRODUTO_REGRA_COMBO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("PRODUTO_REGRA_COMBO", "IDX_PRODUTO_REGRA_COMBO_1", "COD_PRODUTO,COD_PDV");
            CriaIndiceBanco("PRODUTO_REGRA_COMBO", "IDX_PRODUTO_REGRA_COMBO_2", "COD_PRODUTO,COD_CATEGORIA_RESTRICAO");
            //************** Fim Construindo Indice da tabela PRODUTO_REGRA_COMBO  *************************

            //************** Inicio Construindo Indice da tabela PRODUTO_COLETA_ESTOQUE *************************
            objMsg("Construindo Indice da Tabela " + "PRODUTO_COLETA_ESTOQUE", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("PRODUTO_COLETA_ESTOQUE", "IDX_PRODUTO_COLETA_ESTOQUE_1", "DAT_COLETA,COD_EMPREGADO,COD_PDV,COD_PRODUTO");
            //************** Fim Construindo Indice da tabela PRODUTO_COLETA_ESTOQUE  *************************

            //************** Inicio Construindo Indice da tabela BLOQUEIO_DESCONTO_MAXIMO *************************
            objMsg("Construindo Indice da Tabela " + "BLOQUEIO_DESCONTO_MAXIMO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("BLOQUEIO_DESCONTO_MAXIMO", "IDX_BLOQUEIO_DESCONTO_MAXIMO_1", "COD_PRODUTO, COD_TABELA_PRECO, COD_CATEGORIA, COD_PDV, COD_TABELA_BLOQUEIO, COD_BLOQUEIO, COD_SEGMENTO_TABELA_BLOQUEIO");
            //************** Fim Construindo Indice da tabela BLOQUEIO_DESCONTO_MAXIMO  *************************

            //************** Inicio Construindo Indice da tabela BLOQUEIO_PRODUTO_TABELA_PRECO *************************
            objMsg("Construindo Indice da Tabela " + "BLOQUEIO_PRODUTO_TABELA_PRECO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("BLOQUEIO_PRODUTO_TABELA_PRECO", "IDX_BLOQUEIO_PRODUTO_TABELA_PRECO_1", "COD_EMPRESA,COD_PRODUTO,COD_TABELA_PRECO,COD_TABELA_BLOQUEIO,COD_BLOQUEIO,COD_SUB_GRUPO_TABELA_BLOQUEIO");
            //************** Fim Construindo Indice da tabela BLOQUEIO_PRODUTO_TABELA_PRECO  *************************

            //************** Inicio Construindo Indice da tabela PEDIDO *************************
            objMsg("Construindo Indice da Tabela " + "PEDIDO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("PEDIDO", "IDX__PEDIDO_1", "COD_PEDIDO,COD_EMPREGADO,IND_HISTORICO");
            CriaIndiceBanco("PEDIDO", "IDX__PEDIDO_2", "COD_PDV,IND_HISTORICO,DAT_PEDIDO");
            CriaIndiceBanco("PEDIDO", "IDX__PEDIDO_3", "IND_HISTORICO,DAT_PEDIDO");
            //************** Fim Construindo Indice da tabela PEDIDO  *************************

            //************** Inicio Construindo Indice da tabela PDV_PRODUTO_MUR *************************
            objMsg("Construindo Indice da Tabela " + "PDV_PRODUTO_MUR", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("PDV_PRODUTO_MUR", "IDX_PDV_PRODUTO_MUR_1", "COD_PDV,COD_PRODUTO");
            //************** Fim Construindo Indiceda tabela PDV_PRODUTO_MUR  *************************

            //************** Inicio Construindo Indice da tabela OPERACAO *************************
            objMsg("Construindo Indice da Tabela" + "OPERACAO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("OPERACAO", "IDX__OPERACAO_1", "IND_ATIVO");
            //************** Fim Construindo Indice da tabela OPERACAO  *************************

            //************** Inicio Construindo Indice da tabela MOTIVO *************************
            objMsg("Construindo Indice da Tabela" + "MOTIVO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("MOTIVO", "IDX_MOTIVO_1", "COD_MOTIVO,COD_TIPO_MOTIVO,DSC_MOTIVO");
            //************** Fim Construindo Indice da tabela MOTIVO  *************************

            //************** Inicio Construindo Indice da tabela MONITORAMENTO_VENDEDOR_ROTA *************************
            objMsg("Construindo Indice da Tabela " + "MONITORAMENTO_VENDEDOR_ROTA", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("MONITORAMENTO_VENDEDOR_ROTA", "IDX__MONITORAMENTO_VENDEDOR_ROTA_1", "DAT_ENTRADA");
            //************** Fim Construindo Indice da tabela MONITORAMENTO_VENDEDOR_ROTA  *************************

            //************** Inicio Construindo Indice da tabela LAZER_CONTATO *************************
            objMsg("Construindo Indice da Tabela " + "LAZER_CONTATO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("LAZER_CONTATO", "IDX_LAZER_CONTATO_1", "COD_CONTATO_PDV,COD_LAZER");
            //************** Fim Construindo Indice da tabela LAZER_CONTATO  *************************

            //************** Inicio Construindo Indice da tabela LAZER *************************
            objMsg("Construindo Indice da Tabela " + "LAZER", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("LAZER", "IDX_LAZER_1", "COD_LAZER,COD_TIPO_LAZER");
            //************** Fim Construindo Indice da tabela LAZER *************************

            //************** Inicio Construindo Indice da tabela ITEM_PEDIDO *************************
            objMsg("Construindo Indice da Tabela " + "ITEM_PEDIDO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("ITEM_PEDIDO", "IDX_ITEM_PEDIDO_1", "COD_EMPREGADO,COD_PEDIDO,COD_PRODUTO");
            CriaIndiceBanco("ITEM_PEDIDO", "IDX_ITEM_PEDIDO_2", "COD_PRODUTO");
            CriaIndiceBanco("ITEM_PEDIDO", "IDX_ITEM_PEDIDO_3", "COD_PEDIDO");
            //************** Fim Construindo Indice da tabela ITEM_PEDIDO *************************

            //************** Inicio Construindo Indice da tabela HISTORICO_MOTIVO *************************
            objMsg("Construindo Indice da Tabela " + "HISTORICO_MOTIVO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("HISTORICO_MOTIVO", "IDX_HISTORICO_MOTIVO_1", "COD_PDV,DAT_HISTORICO_MOTIVO,COD_TIPO_MOTIVO");
            //************** Fim Construindo Indice da tabela HISTORICO_MOTIVO  *************************

            //************** Inicio Construindo Indice da tabela GRUPO_PRODUTO *************************
            objMsg("Construindo Indice da Tabela " + "GRUPO_PRODUTO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("GRUPO_PRODUTO", "IDX_GRUPO_PRODUTO_1", "COD_GRUPO,DSC_GRUPO");
            //************** Fim Construindo Indice da tabela GRUPO_PRODUTO  *************************

            //************** Inicio Construindo Indice da tabela FAMILIA_PRODUTO *************************
            objMsg("Construindo Indice da Tabela " + "FAMILIA_PRODUTO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("FAMILIA_PRODUTO", "IDX_FAMILIA_PRODUTO_1", "COD_FAMILIA_PRODUTO,COD_GRUPO,DSC_FAMILIA_PRODUTO");
            //************** Fim Construindo Indice da tabela FAMILIA_PRODUTO  *************************

            //************** Inicio Construindo Indice da tabela ENDERECO_PDV *************************
            objMsg("Construindo Indice da Tabela " + "ENDERECO_PDV", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("ENDERECO_PDV", "IDX_ENDERECO_PDV_1", "COD_PDV,COD_UF,COD_CIDADE,COD_BAIRRO,COD_TIPO_ENDERECO");
            //************** Fim Construindo Indice da tabela ENDERECO_PDV  *************************

            //************** Inicio Construindo Indice da tabela EMPRESA *************************
            objMsg("Construindo Indice da Tabela " + "EMPRESA", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("EMPRESA", "IDX_EMPRESA_1", "COD_EMPRESA");
            //************** Fim Construindo Indice da tabela EMPRESA  *************************

            //************** Inicio Construindo Indice da tabela EMPREGADO *************************
            objMsg("Construindo Indice da Tabela " + "EMPREGADO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("EMPREGADO", "IDX_EMPREGADO_1", "COD_EMPREGADO");
            //************** Fim Construindo Indice da tabela EMPREGADO  *************************

            //************** Inicio Construindo Indice da tabela DOCUMENTO_RECEBER *************************
            objMsg("Construindo Indice da Tabela " + "DOCUMENTO_RECEBER", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("DOCUMENTO_RECEBER", "IDX_DOCUMENTO_RECEBER_1", "COD_DOCUMENTO_RECEBER,COD_CLASSE_DOCUMENTO_RECEBER");
            CriaIndiceBanco("DOCUMENTO_RECEBER", "IDX_DOCUMENTO_RECEBER_2", "COD_PDV");
            //************** Fim Construindo Indice da tabela DOCUMENTO_RECEBER  *************************

            //************** Inicio Construindo Indice da tabela CONTATO_PDV *************************
            objMsg("Construindo Indice da Tabela " + "CONTATO_PDV", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("CONTATO_PDV", "IDX_CONTATO_PDV_1", "COD_PDV,COD_CONTATO_PDV");
            //************** Fim Construindo Indice da tabela CONTATO_PDV  *************************

            //************** Inicio Construindo Indice da tabela CONDICAO_PAGAMENTO *************************
            objMsg("Construindo Indice da Tabela " + "CONDICAO_PAGAMENTO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("CONDICAO_PAGAMENTO", "IDX_CONDICAO_PAGAMENTO_1", "COD_TIPO_CONDICAO_PAGAMENTO");
            CriaIndiceBanco("CONDICAO_PAGAMENTO", "IDX_CONDICAO_PAGAMENTO_2", "COD_CONDICAO_PAGAMENTO");
            //************** Fim Construindo Indice da tabela CONDICAO_PAGAMENTO  *************************

            //************** Inicio Construindo Indice da tabela CLASSE_DOCUMENTO_RECEBER *************************
            objMsg("Construindo Indice da Tabela " + "CLASSE_DOCUMENTO_RECEBER", numeroTabelasTotal, 35, null);
            CriaIndiceBanco("CLASSE_DOCUMENTO_RECEBER", "IDX_CLASSE_DOCUMENTO_RECEBER_1", "COD_CLASSE_DOCUMENTO_RECEBER");
            //************** Fim Construindo Indice da tabela CLASSE_DOCUMENTO_RECEBER  *************************

            //************** Inicio Construindo Indice da tabela CIDADE *************************
            objMsg("Construindo Indice da Tabela " + "CIDADE", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("CIDADE", "IDX_CIDADE_2", "COD_UF,COD_CIDADE");
            //************** Fim Construindo Indice da tabela CIDADE  *************************

            //************** Inicio Construindo Indice da tabela CATEGORIA *************************
            objMsg("Construindo Indice da Tabela " + "CATEGORIA", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("CATEGORIA", "IDX_CATEGORIA_1", "COD_CATEGORIA");
            //************** Fim Construindo Indice da tabela CATEGORIA  *************************

            //************** Inicio Construindo Indice da tabela CADASTRO_DIA_VISITA *************************
            objMsg("Construindo Indice da Tabela " + "CADASTRO_DIA_VISITA", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("CADASTRO_DIA_VISITA", "IDX_CADASTRO_DIA_VISITA_1", "COD_PDV,COD_GRUPO_COMERCIALIZACAO,COD_DIA_VISITA,NUM_ORDEM_VISITA_ROTA");
            //************** Fim Construindo Indice da tabela CADASTRO_DIA_VISITA  *************************

            //************** Inicio Construindo Indice da tabela BLOQUEIO_VENDA_ESCALONADA *************************
            objMsg("Construindo Indice da Tabela " + "BLOQUEIO_VENDA_ESCALONADA", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("BLOQUEIO_VENDA_ESCALONADA", "IDX_BLOQUEIO_VENDA_ESCALONADA_1", "QTD_INICIAL_ESCALONADA");
            //************** Fim Construindo Indice da tabela BLOQUEIO_VENDA_ESCALONADA  *************************

            //************** Inicio Construindo Indice da tabela BAIRRO *************************
            objMsg("Construindo Indice da Tabela " + "BAIRRO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("BAIRRO", "IDX_BAIRRO_2", "COD_UF,COD_CIDADE,COD_BAIRRO");
            //************** Fim Construindo Indice da tabela BAIRRO *************************

            //************** Inicio Construindo Indice da tabela TABELA_PRECO_PRODUTO *************************
            objMsg("Construindo Indice da Tabela " + "TABELA_PRECO_PRODUTO ", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("TABELA_PRECO_PRODUTO", "IDX_TABELA_PRECO_PRODUTO_1", "COD_TABELA_PRECO,COD_PRODUTO,VLR_PRODUTO");
            CriaIndiceBanco("TABELA_PRECO_PRODUTO", "IDX_TABELA_PRECO_PRODUTO_2", "COD_PRODUTO,VLR_PRODUTO");
            //************** Fim Construindo Indice da tabela TABELA_PRECO_PRODUTO *************************

            //************** Inicio Construindo Indice da tabela PDV  *************************
            objMsg("Construindo Indice da Tabela " + "PDV", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("PDV", "IDX_PDV_1", "COD_PDV,COD_CATEGORIA");
            //************** Fim Construindo Indice da tabela PDV *************************

            //************** Inicio Construindo Indice da tabela PDV_GRUPO_COMERCIALIZACAO *************************
            objMsg("Construindo Indice da Tabela " + "PDV_GRUPO_COMERCIALIZACAO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("PDV_GRUPO_COMERCIALIZACAO", "IDX_PDV_GRUPO_COMERCIALIZACAO_1", "COD_PDV,COD_GRUPO_COMERCIALIZACAO,COD_CONDICAO_PAGAMENTO,COD_SEGMENTACAO");
            //************** Fim Construindo Indice da tabela PDV_GRUPO_COMERCIALIZACAO  *************************

            //************** Inicio Construindo Indice da tabela GRUPO_COMERCIALIZACAO *************************
            objMsg("Construindo Indice da Tabela " + "GRUPO_COMERCIALIZACAO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("GRUPO_COMERCIALIZACAO", "IDX_GRUPO_COMERCIALIZACAO_1", "COD_GRUPO_COMERCIALIZACAO");
            //************** Fim Construindo Indice da tabela GRUPO_COMERCIALIZACAO  *************************

            //************** Inicio Construindo Indice da tabela BLOQUEIO_TABELA_PRECO *************************
            objMsg("Construindo Indice da Tabela " + "BLOQUEIO_TABELA_PRECO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("BLOQUEIO_TABELA_PRECO", "IDX_BLOQUEIO_TABELA_PRECO_1", "COD_EMPRESA,COD_TABELA_PRECO,COD_TABELA_BLOQUEIO,COD_BLOQUEIO,COD_SUB_GRUPO_TABELA_BLOQUEIO");
            CriaIndiceBanco("BLOQUEIO_TABELA_PRECO", "IDX_BLOQUEIO_TABELA_PRECO_2", "COD_TABELA_PRECO,COD_TABELA_BLOQUEIO,TIPO_BLOQUEIO,COD_BLOQUEIO");

            //************** Fim Construindo Indice da tabela BLOQUEIO_TABELA_PRECO *************************

            //************** Inicio Construindo Indice da tabela GRUPO_CLIENTE *************************
            objMsg("Construindo Indice da Tabela " + "GRUPO_CLIENTE", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("GRUPO_CLIENTE", "IDX_GRUPO_CLIENTE_1", "COD_GRUPO");
            //************** Fim Construindo Indice da tabela GRUPO_CLIENTE *************************

            //************** Inicio Construindo Indice da tabela CLASSIFICACAO_GRUPO_CLIENTE *************************
            objMsg("Construindo Indice da Tabela " + "CLASSIFICACAO_GRUPO_CLIENTE", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("CLASSIFICACAO_GRUPO_CLIENTE", "IDX_CLASSIFICACAO_GRUPO_CLIENTE_1", "COD_GRUPO,COD_CLASSIFICACAO");
            //************** Fim Construindo Indice da tabela CLASSIFICACAO_GRUPO_CLIENTE *************************

            //************** Inicio Construindo Indice da tabela UNIDADE_NEGOCIO *************************
            objMsg("Construindo Indice da Tabela " + "UNIDADE_NEGOCIO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("UNIDADE_NEGOCIO", "IDX_UNIDADE_NEGOCIO_1", "COD_UNIDADE_NEGOCIO");
            //************** Fim Construindo Indice da tabela UNIDADE_NEGOCIO *************************

            //************** Inicio Construindo Indice da tabela FILIAL *************************
            objMsg("Construindo Indice da Tabela " + "FILIAL", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("FILIAL", "IDX_FILIAL_1", "COD_REVENDA");
            //************** Fim Construindo Indice da tabela FILIAL *************************

            //************** Inicio Construindo Indice da tabela DAT_REFERENCIA_CICLO_VISITA *************************
            objMsg("Construindo Indice da Tabela " + "DAT_REFERENCIA_CICLO_VISITA", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("DAT_REFERENCIA_CICLO_VISITA", "IDX_DAT_REFERENCIA_CICLO_VISITA_1", "DAT_INICIO_CICLO,DAT_FINAL_CICLO");
            //************** Fim Construindo Indice da tabela DAT_REFERENCIA_CICLO_VISITA *************************

            //************** Inicio Construindo Indice da tabela PESQUISA_MERCADO *************************
            objMsg("Construindo Indice da Tabela " + "PESQUISA_MERCADO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("PESQUISA_MERCADO", "IDX_PESQUISA_MERCADO_1", "DATINI_PESQUISA_MERC,DATFIM_PESQUISA_MERC");
            //************** Fim Construindo Indice da tabela PESQUISA_MERCADO *************************

            //************** Inicio Construindo Indice da tabela PERGUNTA_PESQUISA_MERCADO *************************
            objMsg("Construindo Indice da Tabela " + "PERGUNTA_PESQUISA_MERCADO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("PERGUNTA_PESQUISA_MERCADO", "IDX_PERGUNTA_PESQUISA_MERCADO_1", "COD_PESQUISA_MERC,COD_PERGUNTA_MERC");
            //************** Fim Construindo Indice da tabela PERGUNTA_PESQUISA_MERCADO *************************

            //************** Inicio Construindo Indice da tabela MARCA_PESQUISA_MERCADO *************************
            objMsg("Construindo Indice da Tabela " + "MARCA_PESQUISA_MERCADO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("MARCA_PESQUISA_MERCADO", "IDX_MARCA_PESQUISA_MERCADO_1", "COD_PESQUISA_MERC,ORD_MARCA_PESQUISA_MERC");
            //************** Fim Construindo Indice da tabela MARCA_PESQUISA_MERCADO *************************

            //************** Inicio Construindo Indice da tabela RESPOSTA_PESQUISA_MERCADO *************************
            objMsg("Construindo Indice da Tabela " + "RESPOSTA_PESQUISA_MERCADO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("RESPOSTA_PESQUISA_MERCADO", "IDX_RESPOSTA_PESQUISA_MERCADO_1", "COD_PESQUISA_MERC,COD_PDV");
            //************** Fim Construindo Indice da tabela RESPOSTA_PESQUISA_MERCADO *************************

            //************** Inicio Construindo Indice da tabela PESQUISA_PIV *************************
            objMsg("Construindo Indice da Tabela " + "PESQUISA_PIV", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("PESQUISA_PIV", "IDX_PESQUISA_PIV_1", "COD_PESQUISA_PIV");
            //************** Fim Construindo Indice da tabela PESQUISA_PIV *************************

            //************** Inicio Construindo Indice da tabela PERGUNTA_PESQUISA_PIV *************************
            objMsg("Construindo Indice da Tabela " + "PERGUNTA_PESQUISA_PIV", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("PERGUNTA_PESQUISA_PIV", "IDX_PERGUNTA_PESQUISA_PIV_1", "COD_PESQUISA_PIV,COD_PERGUNTA_PIV");
            //************** Fim Construindo Indice da tabela PERGUNTA_PESQUISA_PIV *************************

            //************** Inicio Construindo Indice da tabela PERGUNTA_CONFIG_OPCAO *************************
            objMsg("Construindo Indice da Tabela " + "PERGUNTA_CONFIG_OPCAO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("PERGUNTA_CONFIG_OPCAO", "IDX_PERGUNTA_CONFIG_OPCAO_1", "COD_PESQUISA_PIV,COD_PERGUNTA_PIV");
            //************** Fim Construindo Indice da tabela PERGUNTA_CONFIG_OPCAO *************************

            //************** Inicio Construindo Indice da tabela RESPOSTA_PESQUISA *************************
            objMsg("Construindo Indice da Tabela " + "RESPOSTA_PESQUISA", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("RESPOSTA_PESQUISA", "IDX_RESPOSTA_PESQUISA_1", "COD_PESQUISA_PIV,COD_PERGUNTA_PIV,COD_PDV");
            //************** Fim Construindo Indice da tabela RESPOSTA_PESQUISA *************************

            //************** Inicio Construindo Indice da tabela MOTIVO_NAORESP_PESQ *************************
            objMsg("Construindo Indice da Tabela " + "MOTIVO_NAORESP_PESQ", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("MOTIVO_NAORESP_PESQ", "IDX_MOTIVO_NAORESP_PESQ_1", "COD_PESQUISA_PIV,COD_PDV");
            //************** Fim Construindo Indice da tabela MOTIVO_NAORESP_PESQ *************************

            //************** Inicio Construindo Indice da tabela PESQUISA_MERCHAN_PDV *************************
            objMsg("Construindo Indice da Tabela " + "PESQUISA_MERCHAN_PDV", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("PESQUISA_MERCHAN_PDV", "IDX_PESQUISA_MERCHAN_PDV_1", "COD_PDV,COD_PRODUTO");
            //************** Fim Construindo Indice da tabela PESQUISA_MERCHAN_PDV *************************

            //************** Inicio Construindo Indice da tabela PESQUISA_MERCHAN_PRODCATEGORIA *************************
            objMsg("Construindo Indice da Tabela " + "PESQUISA_MERCHAN_PRODCATEGORIA", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("PESQUISA_MERCHAN_PRODCATEGORIA", "IDX_PESQUISA_MERCHAN_PRODCATEGORIA_1", "COD_PRODUTO,COD_CATEGORIA");
            //************** Fim Construindo Indice da tabela PESQUISA_MERCHAN_PRODCATEGORIA *************************

            //************** Inicio Construindo Indice da tabela SALDO_PRONTA_ENTREGA_ITEM *************************
            objMsg("Construindo Indice da Tabela " + "SALDO_PRONTA_ENTREGA_ITEM", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("SALDO_PRONTA_ENTREGA_ITEM", "IDX_SALDO_PRONTA_ENTREGA_ITEM_1", "COD_EMPREGADO, COD_PRODUTO");
            //************** Fim Construindo Indice da tabela SALDO_PRONTA_ENTREGA_ITEM *************************

            //************** Inicio Construindo Indice da tabela ENTRADA_CREDITO_PRODUTO *************************
            objMsg("Construindo Indice da Tabela " + "ENTRADA_CREDITO_PRODUTO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("ENTRADA_CREDITO_PRODUTO", "IDX_ENTRADA_CREDITO_PRODUTO_1", "COD_EMPREGADO, COD_CREDITO, DAT_ATIVACAO, COD_CHAVE, COD_PRODUTO");
            //************** Fim Construindo Indice da tabela SALDO_PRONTA_ENTREGA_ITEM *************************

            //************** Inicio Construindo Indice da tabela PDV_PRODUTO_MAIOR_VENDA *************************
            objMsg("Construindo Indice da Tabela " + "PDV_PRODUTO_MAIOR_VENDA", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("PDV_PRODUTO_MAIOR_VENDA", "IDX_PDV_PRODUTO_MAIOR_VENDA_1", "COD_EMPREGADO, COD_PDV, COD_PRODUTO");
            //************** Fim Construindo Indice da tabela SALDO_PRONTA_ENTREGA_ITEM *************************            

            //************** Inicio Construindo Indice da tabela MODELO_VEICULO *************************
            objMsg("Construindo Indice da Tabela " + "MODELO_VEICULO", numeroTabelasTotal, numeroTabelaAtual++, null);
            CriaIndiceBanco("MODELO_VEICULO", "IDX_MODELO_VEICULO_1", "COD_TIPO_MODELO_VEICULO");
            //************** Fim Construindo Indice da tabela TIPO_TELEFONE *************************

            if (politica.Contains("2"))
            {
                //////////////////////////////////////////////////////////////////////////////
                // [ Tabelas para o novo cálculo do preço ] //////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////

                //************** Inicio Construindo Indice da tabela BRK_ECLIENTBAS *************************
                objMsg("Construindo Indice da Tabela " + "BRK_ECLIENTBAS", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_ECLIENTBAS", "IDX_BRK_ECLIENTBAS_1", "CDCLI");

                //************** Inicio Construindo Indice da tabela BRK_ECLIENTENT *************************
                objMsg("Construindo Indice da Tabela " + "BRK_ECLIENTENT", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_ECLIENTENT", "IDX_BRK_ECLIENTENT_1", "CDCLIENTR");

                //************** Inicio Construindo Indice da tabela BRK_ECLIENTE *************************
                objMsg("Construindo Indice da Tabela " + "BRK_ECLIENTE", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_ECLIENTE", "IDX_BRK_ECLIENTE_1", "CDCLI,CDCLIN4,CDCLIN5,CDCLIN6");
                CriaIndiceBanco("BRK_ECLIENTE", "IDX_BRK_ECLIENTE_2", "CDFILFAT");

                //************** Inicio Construindo Indice da tabela BRK_ECLIIMP *************************
                objMsg("Construindo Indice da Tabela " + "BRK_ECLIIMP", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_ECLIIMP", "IDX_BRK_ECLIIMP_1", "CDCLI,CDCATIMP");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA002 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA002", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA002", "IDX_BRK_EPRECOA002_1", "CDAPLICAC,CDCONDTYP,CDPAIS,IDCLIIMP,IDPRDIMP,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA004 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA004", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA004", "IDX_BRK_EPRECOA004_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDPRD,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA030 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA030", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA030", "IDX_BRK_EPRECOA030_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDCLI,CDGRPPRC,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA031 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA031", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA031", "IDX_BRK_EPRECOA031_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDGRPPRCCL,CDGRPPRC,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA032 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA032", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA032", "IDX_BRK_EPRECOA032_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDGRPPRCCL,CDPRD,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA121 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA121", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA121", "IDX_BRK_EPRECOA121_1", "CDAPLICAC,CDCONDTYP,CDPAIS,CDPRD,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA191 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA191", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA191", "IDX_BRK_EPRECOA191_1", "CDAPLICAC,CDCONDTYP,CDPAIS,CDPAISDEST,IDPRDIMP,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA291 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA291", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA291", "IDX_BRK_EPRECOA291_1", "CDAPLICAC,CDCONDTYP,CDPAIS,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA346 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA346", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA346", "IDX_BRK_EPRECOA346_1", "CDAPLICAC,CDCONDTYP,CDPAIS,CDDINAMIC1,CDDINAMIC2,CDDINAMIC3,CDGRPDINAM,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA350 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA350", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA350", "IDX_BRK_EPRECOA350_1", "CDAPLICAC,CDCONDTYP,CDGER0,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA382 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA382", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA382", "IDX_BRK_EPRECOA382_1", "CDAPLICAC,CDCONDTYP,CDPAIS,IDTIPODOC,CDREGFORIG,CDREGFDEST,CDDINAMIC1,CDDINAMIC2,CDDINAMIC3,CDGRPDINAM");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA390 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA390", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA390", "IDX_BRK_EPRECOA390_1", "CDAPLICAC,CDCONDTYP,CDPAIS,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA392 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA392", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA392", "IDX_BRK_EPRECOA392_1", "CDAPLICAC,CDCONDTYP,CDPAIS,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA803 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA803", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA803", "IDX_BRK_EPRECOA803_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDGRPPRC,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA806 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA806", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA806", "IDX_BRK_EPRECOA806_1", "CDAPLICAC,CDCONDTYP,CDPAIS,IDCLIIMP,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA807 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA807", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA807", "IDX_BRK_EPRECOA807_1", "CDAPLICAC,CDCONDTYP,CDPAIS,CDCLASFISC,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA808 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA808", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA808", "IDX_BRK_EPRECOA808_1", "CDAPLICAC,CDCONDTYP,CDPAIS,CDREGFORIG,IDCLIIMP,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA809 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA809", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA809", "IDX_BRK_EPRECOA809_1", "CDAPLICAC,CDCONDTYP,CDTPVENDA,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA810 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA810", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA810", "IDX_BRK_EPRECOA810_1", "CDAPLICAC,CDCONDTYP,CDPAIS,IDCLIIMP,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA811 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA811", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA811", "IDX_BRK_EPRECOA811_1", "CDAPLICAC,CDCONDTYP,CDPAIS,CDREGFORIG,CDREGFDEST,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA812 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA812", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA812", "IDX_BRK_EPRECOA812_1", "CDAPLICAC,CDCONDTYP,CDPAIS,IDCLIIMP,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA813 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA813", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA813", "IDX_BRK_EPRECOA813_1", "CDAPLICAC,CDCONDTYP,CDPAIS,CDREGFORIG,CDREGFDEST,CDPRD,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA814 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA814", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA814", "IDX_BRK_EPRECOA814_1", "CDAPLICAC,CDCONDTYP,CDPAIS,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA815 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA815", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA815", "IDX_BRK_EPRECOA815_1", "CDAPLICAC,CDCONDTYP,CDPAIS,CDGRCSUTR,CDREGFORIG,CDREGFDEST,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA816 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA816", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA816", "IDX_BRK_EPRECOA816_1", "CDAPLICAC,CDCONDTYP,CDPAIS,CDGRCSUTR,CDREGFORIG,CDREGFDEST,CDPRD,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA817 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA817", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA817", "IDX_BRK_EPRECOA817_1", "CDAPLICAC,CDCONDTYP,CDPAIS,CDREGFDEST,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA823 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA823", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA823", "IDX_BRK_EPRECOA823_1", "CDAPLICAC,CDCONDTYP,CDPAIS,CDPRD,IDSITUACAO,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA832 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA832", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA832", "IDX_BRK_EPRECOA832_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDPRZPAG,CDCLI,CDPRD,IDSITUACAO,DTVIGINI,DTVIGFIM");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA833 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA833", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA833", "IDX_BRK_EPRECOA833_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDPRZPAG,CDMERCADO,CDNEGOCIO,CDCATEG,CDSUBCATEG,CDSEGMENTO");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA834 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA834", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA834", "IDX_BRK_EPRECOA834_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDPRZPAG,CDMERCADO,CDNEGOCIO,CDCATEG,CDSUBCATEG,IDSITUACAO");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA835 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA835", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA835", "IDX_BRK_EPRECOA835_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDPRZPAG,CDCLASCLI,CDTPNEG,CDPONTOVEN,CDPRD,IDSITUACAO");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA836 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA836", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA836", "IDX_BRK_EPRECOA836_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDPRZPAG,CDCLASCLI,CDTPNEG,CDPONTOVEN,CDMERCADO,CDNEGOCIO");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA837 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA837", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA837", "IDX_BRK_EPRECOA837_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDPRZPAG,IDSITUACAO,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA850 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA850", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA850", "IDX_BRK_EPRECOA850_1", "CDAPLICAC,CDCONDTYP,CDPAIS,IDTIPODOC,CDREGFORIG,CDREGFDEST,CDDINAMIC1,CDDINAMIC2,CDDINAMIC3,CDGRPDINAM");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA851 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA851", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA851", "IDX_BRK_EPRECOA851_1", "CDAPLICAC,CDCONDTYP,CDPAIS,CDREGFORIG,CDREGFDEST,CDGRCSUTR,CDDINAMIC1,CDDINAMIC2,CDDINAMIC3,CDGRPDINAM");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA852 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA852", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA852", "IDX_BRK_EPRECOA852_1", "CDAPLICAC,CDCONDTYP,CDGRPDINAM,CDCLASFISC,CDREGFDEST,IDSITUACAO,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA860 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA860", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA860", "IDX_BRK_EPRECOA860_1", "CDCONDTYP, NMCONDREC, CDAPLICAC, CDGER0, CDCANDISTR, CDUF, CDGRPCLI, CDRANGE, CDMERCADO, CDNEGOCIO, CDCATEG, CDSUBCATEG, CDSEGMENTO, CDATRIBUTO, DTVIGFIM DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA870 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA870", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA870", "IDX_BRK_EPRECOA870_1", "CDAPLICAC,CDCONDTYP,CDPAIS,CDREGFORIG,IDCLIIMP,CDCLASFISC,IDSITUACAO,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA871 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA871", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA871", "IDX_BRK_EPRECOA871_1", "CDAPLICAC,CDCONDTYP,CDPAIS,CDTIPOPED,IDSITUACAO,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA872 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA872", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA872", "IDX_BRK_EPRECOA872_1", "CDAPLICAC,CDCONDTYP,CDPAIS,CDREGFORIG,CDREGFDEST,CDCLASFISC,IDSITUACAO,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA880 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA880", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA880", "IDX_BRK_EPRECOA880_1", "CDAPLICAC,CDCONDTYP,CDPAIS,CDREGFISCA,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA903 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA903", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA903", "IDX_BRK_EPRECOA903_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDPRD,DTVIGINI,DTVIGFIM");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA904 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA904", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA904", "IDX_BRK_EPRECOA904_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDCLI,CDGRPPRD,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA905 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA905", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA905", "IDX_BRK_EPRECOA905_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDGRPPRD,DTVIGINI,DTVIGFIM,NMCONDREC DESC,CDCLIH");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA906 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA906", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA906", "IDX_BRK_EPRECOA906_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDPRD,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA910 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA910", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA910", "IDX_BRK_EPRECOA910_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDCLASCLI,CDTPNEG,CDPONTOVEN,CDMERCADO,CDNEGOCIO,CDCATEG");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA915 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA915", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA915", "IDX_BRK_EPRECOA915_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDDIVISAO,CDCLI,CDMERCADO,CDNEGOCIO,CDCATEG,CDSUBCATEG");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA916 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA916", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA916", "IDX_BRK_EPRECOA916_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDPRD,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA917 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA917", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA917", "IDX_BRK_EPRECOA917_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDCLI,CDPRD,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA924 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA924", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA924", "IDX_BRK_EPRECOA924_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDMERCADO,CDNEGOCIO,CDCATEG,CDSUBCATEG,CDSEGMENTO,DTVIGINI");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA925 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA925", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA925", "IDX_BRK_EPRECOA925_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDGRPPRD,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA928 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA928", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA928", "IDX_BRK_EPRECOA928_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDCLI,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA943 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA943", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA943", "IDX_BRK_EPRECOA943_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDMERCADO,CDNEGOCIO,CDCATEG,CDSUBCATEG,CDSEGMENTO,DTVIGINI");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA945 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA945", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA945", "IDX_BRK_EPRECOA945_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDCLASCLI,CDTPNEG,CDPONTOVEN,CDPRD,DTVIGINI,DTVIGFIM");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA947 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA947", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA947", "IDX_BRK_EPRECOA947_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDCANDISTR,CDMERCADO,CDNEGOCIO,CDCATEG,CDSUBCATEG,CDSEGMENTO,DTVIGINI");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOA951 *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA951", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA951", "IDX_BRK_EPRECOA951_1", "CDAPLICAC,CDCONDTYP,CDGER0,CDPRD,DTVIGINI,DTVIGFIM,NMCONDREC DESC");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOFORM *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOFORM", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOFORM", "IDX_BRK_EPRECOFORM_1", "CDCONDSEQ,NMCONDLIN");

                //************** Inicio Construindo Indice da tabela BRK_EPRECOSEQ *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOSEQ", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOSEQ", "IDX_BRK_EPRECOSEQ_1", "CDCONDSEQ,DSTABELA,NMSEQTAB");

                //************** Inicio Construindo Indice da tabela BRK_EPRODFIL *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRODFIL", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRODFIL", "IDX_BRK_EPRODFIL_1", "CDPRD,CDFILFAT");

                //************** Inicio Construindo Indice da tabela BRK_EPRODORG *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRODORG", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRODORG", "IDX_BRK_EPRODORG_1", "CDPRD,CDGER0,CDCANDISTR,CDDIVISAO");
                CriaIndiceBanco("BRK_EPRODORG", "IDX_BRK_EPRODORG_2", "CDGRPPRD");
                CriaIndiceBanco("BRK_EPRODORG", "IDX_BRK_EPRODORG_3", "CDMERCADO,CDNEGOCIO,CDCATEG,CDSUBCATEG,CDSEGMENTO");

                //************** Inicio Construindo Indice da tabela BRK_EPRODTPIMP *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRODTPIMP", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRODTPIMP", "IDX_BRK_EPRODTPIMP_1", "CDPRD,CDPAIS");

                //************** Inicio Construindo Indice da tabela BRK_EPRODUTO *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRODUTO", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRODUTO", "IDX_BRK_EPRODUTO_1", "CDPRD");

                //************** Inicio Construindo Indice da tabela BRK_EUNIDMED *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EUNIDMED", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EUNIDMED", "IDX_BRK_EUNIDMED_1", "CDPRD,CDUNIDMED");

                //************** Inicio Construindo Indice da tabela BRK_TFILFAT *************************
                objMsg("Construindo Indice da Tabela " + "BRK_TFILFAT", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_TFILFAT", "IDX_BRK_TFILFAT_1", "CDFILFAT,CDREGFISCA");

                //************** Inicio Construindo Indice da tabela BRK_EEXFIL *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EEXFIL", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EEXFIL", "IDX_BRK_EEXFIL_1", "CDGER0,CDDIVISAO,CDCLI,CDPRD,CDFILFAT");

                //*** Inicio Construindo Indice da tabela BRK_EPRECOA293*
                objMsg("Construindo Indice da Tabela " + "BRK_EPRECOA293", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRECOA293", "IDX_BRK_EPRECOA293_1", "CDAPLICAC,CDCONDTYP,CDPAIS,IDCLIIMP");

                //************** Inicio Construindo Indice da tabela BRK_EPRODAVAL *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPRODAVAL", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPRODAVAL", "IDX_BRK_EPRODAVAL_1", "CDPRD");
                CriaIndiceBanco("BRK_EPRODAVAL", "IDX_BRK_EPRODAVAL_2", "CDPRD,CDFILFAT");

                //************** Inicio Construindo Indice da tabela BRK_EPROMGRATIS *************************
                objMsg("Construindo Indice da Tabela " + "BRK_EPROMGRATIS", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BRK_EPROMGRATIS", "IDX_BRK_EPROMGRATIS_1", "CDAPLICAC, CDCONDTYP, CDGER0, CDCANDISTR, CDCLIH, CDPRD, CDCAMPANHA, DTVIGFIM");
                //////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////
            }

            if (politica.Contains("3"))
            {
                //*** Inicio Construindo Indice da tabela BNG_LOJA_CLIENTE_AGV_SAP*
                objMsg("Construindo Indice da Tabela " + "BNG_LOJA_CLIENTE_AGV_SAP", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BNG_LOJA_CLIENTE_AGV_SAP", "IDX_BNG_LOJA_CLIENTE_AGV_SAP_1", "NR_NOTEBOOK,CD_BASE_CLIENTE,CD_LOJA_CLIENTE");

                //*** Inicio Construindo Indice da tabela BNG_ZONA_VDA_LJA_CLI_AGV_SAP*
                objMsg("Construindo Indice da Tabela " + "BNG_ZONA_VDA_LJA_CLI_AGV_SAP", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BNG_ZONA_VDA_LJA_CLI_AGV_SAP", "IDX_BNG_ZONA_VDA_LJA_CLI_AGV_SAP_1", "NR_NOTEBOOK,CD_BASE_CLIENTE,CD_LOJA_CLIENTE,CD_ORG_VENDAS,CD_CAN_DISTRIBUICAO,CD_SET_ATIVIDADES,CD_REGIONAL_VENDA");

                //*** Inicio Construindo Indice da tabela BNG_CONDICAO_PAGTO_AGV_SAP*
                objMsg("Construindo Indice da Tabela " + "BNG_CONDICAO_PAGTO_AGV_SAP", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BNG_CONDICAO_PAGTO_AGV_SAP", "IDX_BNG_CONDICAO_PAGTO_AGV_SAP_1", "NR_NOTEBOOK,CD_CONDICAO_PAGAMENTO,QT_DIAS");

                //*** Inicio Construindo Indice da tabela BNG_ITEM_AGV_SAP*
                objMsg("Construindo Indice da Tabela " + "BNG_ITEM_AGV_SAP", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BNG_ITEM_AGV_SAP", "IDX_BNG_ITEM_AGV_SAP_1", "NR_NOTEBOOK,CD_ITEM");

                //*** Inicio Construindo Indice da tabela BNG_TB_CHAVE_CAMPO*
                objMsg("Construindo Indice da Tabela " + "BNG_TB_CHAVE_CAMPO", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BNG_TB_CHAVE_CAMPO", "IDX_BNG_TB_CHAVE_CAMPO_1", "CHAVE");

                //*** Inicio Construindo Indice da tabela BNG_TB_ESTRUTURA*
                objMsg("Construindo Indice da Tabela " + "BNG_TB_ESTRUTURA", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BNG_TB_ESTRUTURA", "IDX_BNG_TB_ESTRUTURA_1", "COD_IMPOSTO");

                //*** Inicio Construindo Indice da tabela BNG_TB_SEQUENCIA_IMPOSTO*
                objMsg("Construindo Indice da Tabela " + "BNG_TB_SEQUENCIA_IMPOSTO", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BNG_TB_SEQUENCIA_IMPOSTO", "IDX_BNG_TB_SEQUENCIA_IMPOSTO_1", "SEQUENCIA,CD_GRUPO_IMPOSTO");

                //*** Inicio Construindo Indice da tabela BNG_RGR_FISCAL_ORG_DST_AGV_SAP*
                objMsg("Construindo Indice da Tabela " + "BNG_RGR_FISCAL_ORG_DST_AGV_SAP", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BNG_RGR_FISCAL_ORG_DST_AGV_SAP", "IDX_BNG_RGR_FISCAL_ORG_DST_AGV_SAP_1", "NR_NOTEBOOK,CD_UNIDADE_FEDERACAO_ORI,CD_UNIDADE_FEDERACAO_DST,DT_INICIO_VIGENCIA");

                //*** Inicio Construindo Indice da tabela BNG_RGR_FSC_ECC_DNM_AGV_SAP*
                objMsg("Construindo Indice da Tabela " + "BNG_RGR_FSC_ECC_DNM_AGV_SAP", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BNG_RGR_FSC_ECC_DNM_AGV_SAP", "IDX_BNG_RGR_FSC_ECC_DNM_AGV_SAP_1", "NR_NOTEBOOK,CD_GRUPO_IMPOSTO,CD_UNIDADE_FEDERACAO_ORI,CD_UNIDADE_FEDERACAO_DST,DT_INICIO_VIGENCIA,DT_FIM_VIGENCIA");

                //*** Inicio Construindo Indice da tabela BNG_RGR_FSC_ECC_PIS_COF_AGV_SAP*
                objMsg("Construindo Indice da Tabela " + "BNG_RGR_FSC_ECC_PIS_COF_AGV_SAP", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BNG_RGR_FSC_ECC_PIS_COF_AGV_SAP", "IDX_BNG_RGR_FSC_ECC_PIS_COF_AGV_SAP_1", "NR_NOTEBOOK,CD_GRUPO_IMPOSTO,ID_PIS_COFINS,DT_INICIO_VIGENCIA,DT_FIM_VIGENCIA");

                //*** Inicio Construindo Indice da tabela BNG_SGT_PRECO_ITEM_TAB_AGV_SAP*
                objMsg("Construindo Indice da Tabela " + "BNG_SGT_PRECO_ITEM_TAB_AGV_SAP", numeroTabelasTotal, numeroTabelaAtual++, null);
                CriaIndiceBanco("BNG_SGT_PRECO_ITEM_TAB_AGV_SAP", "IDX_BNG_SGT_PRECO_ITEM_TAB_AGV_SAP_1", "NR_NOTEBOOK,CD_ITEM,CD_BASE_CLIENTE,CD_LOJA_CLIENTE,CD_GRUPO_SEGMENTO,CD_FILIAL,CD_MICRORREGIAO_CML,CD_UNIDADE_FEDERACAO,CD_REGIONAL_VENDA,DT_INICIO_VIGENCIA,DT_FIM_VIGENCIA");
            }

            objMsg("Compactando Banco de Dados ", 0, 0, null);

        }

        /// <summary>
        /// Dropa uma tabela do banco
        /// </summary>
        /// <param name="szTableName">Nome da tabela a dropar</param>
        public void DropTable(string tableName)
        {
            try
            {
                if (TableExists(tableName))
                {
                    ExecuteNonQuery("DROP TABLE " + tableName);
                }
            }
            catch (Exception)
            {
                //Ignore
            }
        }

        /// <summary>
        /// Define se um banco de dados existe fisicamente
        /// </summary>
        public static bool DataBaseExists(string dbFile)
        {
            // [ Verifica se existe banco físico para a revenda atual ]
            string[] recebeFile = System.IO.Directory.GetFiles(CSGlobal.GetCurrentDirectoryDB(), "AvanteSales" + dbFile + ".sdf");

            // [ Encontrou arquivos? ]
            return (recebeFile.Length > 0);
        }

        /// <summary>
        /// Define se existe algum banco para as empresas disponívies
        /// </summary>
        public static bool DataBasesExists()
        {
            foreach (string strRevenda in CSConfiguracao.GetEmpresas())
            {
                // Verifica se este banco fisico
                string[] recebeFile = System.IO.Directory.GetFiles(CSGlobal.GetCurrentDirectoryDB(), "AvanteSales" + strRevenda.Substring(0, 8) + ".sdf");

                if (recebeFile.Length > 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Define se uma tabela existe no banco
        /// </summary>
        /// <param name="tableName">Nome da tabela</param>
        public bool TableExists(string tableName)
        {

            string sqlQuery = "SELECT name FROM sqlite_master WHERE name='" + tableName + "'";
            //TODO: ALterar isto apra ser dependente do provider
            //string sqlQuery =
            //    "SELECT TABLE_NAME " +
            //    "  FROM INFORMATION_SCHEMA.TABLES " +
            //    " WHERE TABLE_NAME = '" + tableName + "'";

            return (ExecuteScalar(sqlQuery) != null);
        }


        /// <summary>
        /// Deleta todos os rows de uma tabela
        /// </summary>
        /// <param name="tableName">Nome da tabela a ser limpa</param>		
        public void ClearTable(string tableName)
        {
            ExecuteNonQuery("DELETE FROM " + tableName);
        }

        private static DbType DataColumnToDbType(DataColumn dc)
        {
            if (dc.DataType.Equals(typeof(string)))
            {
                if (dc.MaxLength > 255 || dc.MaxLength == -1)
                    return DbType.String;
                else
                    return DbType.String;
            }
            else if (dc.ColumnName.CompareTo("CGC") == 0 || dc.ColumnName.CompareTo("FILCGC") == 0)
                return DbType.Int64;
            else if (dc.DataType.Equals(typeof(int)))
                return DbType.Int32;
            else if (dc.DataType.Equals(typeof(byte[])))
                return DbType.Binary;
            else if (dc.DataType.Equals(typeof(long)))
                return DbType.Int64;
            else if (dc.DataType.Equals(typeof(ulong)))
                return DbType.UInt64;
            else if (dc.DataType.Equals(typeof(decimal)))
                return DbType.Decimal;
            else if (dc.DataType.Equals(typeof(char)))
                return DbType.String;
            else if (dc.DataType.Equals(typeof(float)))
                return DbType.Decimal;
            else if (dc.DataType.Equals(typeof(double)))
                return DbType.Double;
            else if (dc.DataType.Equals(typeof(bool)))
                return DbType.Boolean;
            else if (dc.DataType.Equals(typeof(System.Guid)))
                return DbType.Guid;
            else if (dc.DataType.Equals(typeof(DateTime)))
                return DbType.DateTime;
            else
                throw new Exception("Tipo não reconhecido: " + dc.DataType.ToString());
        }

        /// <summary>
        /// Cria uma tabela no banco
        /// </summary>
        /// <param name="tableName">Nome da tabela</param>
        /// <param name="sqlTable">Script de criacao da tabela</param>
        public void CreateTable(string tableName, string sqlTable)
        {
            // Apaga a tabela de ela existir
            DropTable(tableName);

            CreateTable(sqlTable);
        }

        public void CreateTable(string sqlTable)
        {
            // Cria a tabela
            ExecuteNonQuery(sqlTable);
        }

        /// <summary>
        /// Executa uma query SQL e retorna um SQLiteDataReader, permitindo deixar o comando preparado armazenado em memória
        /// </summary>
        /// <param name="prepared">Diz se o comando preparado deve ser armazenado em memória</param>
        /// <param name="sqlQuery">Query SQL</param>
        /// <param name="sqlParams">Parâmetros da query</param>
        /// <returns>SQLiteDataReader com o resultado da Query</returns>
        public SQLiteDataReader ExecuteReader(bool prepared, string sqlQuery, params SQLiteParameter[] sqlParams)
        {
            // [ Comando preparado? ]
            if (prepared)
            {
                // [ Verifica se o comando já foi preparado antes ]
                SQLiteCommand command = GetPreparedCommand(sqlQuery);

                if (command == null)
                {
                    // [ Prepara novo comando ]
                    command = PrepareCommand(sqlQuery, sqlParams);

                    SetPreparedCommand(command, false);

                }
                else
                {
                    // [ Atualiza valores dos parâmetros ]
                    for (int j = 0; j < sqlParams.Length; j++)
                        command.Parameters[j].Value = sqlParams[j].Value;
                }

                return command.ExecuteReader();

            }
            else
                return ExecuteReader(sqlQuery, sqlParams);
        }

        /// <summary>
        /// Executa uma query SQL, permitindo deixar o comando preparado armazenado em memória
        /// </summary>
        /// <param name="prepared">Diz se o comando preparado deve ser armazenado em memória</param>
        /// <param name="sqlQuery">Query SQL</param>
        /// <param name="sqlParams">Parâmetros da query</param>
        /// <returns>Int com o número de registros afetados pela execução da query</returns>
        public int ExecuteNonQuery(bool prepared, string sqlQuery, params SQLiteParameter[] sqlParams)
        {
            // [ Comando preparado? ]
            if (prepared)
            {
                // [ Verifica se o comando já foi preparado antes ]
                SQLiteCommand command = GetPreparedCommand(sqlQuery);

                if (command == null)
                {
                    // [ Prepara novo comando ]
                    command = PrepareCommand(sqlQuery, sqlParams);

                    SetPreparedCommand(command, false);

                }
                else
                {
                    // [ Atualiza valores dos parâmetros ]
                    for (int j = 0; j < sqlParams.Length; j++)
                        command.Parameters[j].Value = sqlParams[j].Value;
                }

                return command.ExecuteNonQuery();

            }
            else
                return ExecuteNonQuery(sqlQuery, sqlParams);
        }

        /// <summary>
        /// Executa uma query SQL e retorna o valor do primeiro campo, permitindo deixar o comando preparado armazenado em memória
        /// </summary>
        /// <param name="prepared">Diz se o comando preparado deve ser armazenado em memória</param>
        /// <param name="sqlQuery">Query SQL</param>
        /// <param name="sqlParams">Parâmetros da query</param>
        /// <returns>Object com o valor do primeiro campo</returns>
        public object ExecuteScalar(bool prepared, string sqlQuery, params SQLiteParameter[] sqlParams)
        {
            // [ Comando preparado? ]
            if (prepared)
            {
                // [ Verifica se o comando já foi preparado antes ]
                SQLiteCommand command = GetPreparedCommand(sqlQuery);

                if (command == null)
                {
                    // [ Prepara novo comando ]
                    command = PrepareCommand(sqlQuery, sqlParams);

                    SetPreparedCommand(command, false);

                }
                else
                {
                    // [ Atualiza valores dos parâmetros ]
                    for (int j = 0; j < sqlParams.Length; j++)
                        command.Parameters[j].Value = sqlParams[j].Value;
                }

                return command.ExecuteScalar();

            }
            else
                return ExecuteScalar(sqlQuery, sqlParams);
        }

        /// <summary>
        /// Executa uma query SQL, permitindo deixar o resultado do comando armazenado em memória.
        /// Método criado para ser usado no cálculo das regras de política de preços broker.
        /// </summary>
        /// <param name="sqlQuery">Query SQL</param>
        /// <param name="cacheKey">Chave de identificação do resultado</param>
        /// <param name="temporary">Diz se o resultado será temporariamente armazenado</param>
        /// <param name="sqlParams">Parâmetros da query</param>
        /// <returns>Array de object com o resultado da query</returns>
        public object[] ExecuteReaderCached(string sqlQuery, string cacheKey, bool temporary, params SQLiteParameter[] sqlParams)
        {

            // [ Array responsável por armazenar o resultado ]
            object[] res = null;

            if (CSEmpregados.Current.USAR_CACHE_PRICE)
            {
                // [ Resultado será guardado em cache? ]
                if (cacheKey != null)
                {
                    // [ Verifica se o resultado já foi armazenado antes ]
                    res = GetCachedResult(cacheKey);

                    if (res != null)
                        return (CSEmpresa.Current.IND_UTILIZA_PRICE_2014 ? res.Length == 5 : res.Length == 4) ? res : null;

                }
            }

            // [ Verifica se o comando já foi preparado antes ]
            SQLiteCommand command = null;

            if (CSEmpregados.Current.USAR_CACHE_PRICE)
                command = GetPreparedCommand(sqlQuery);

            if (command == null)
            {
                // [ Prepara novo comando ]
                command = PrepareCommand(sqlQuery, sqlParams);

                // [ Armazena novo command preparado na tabela hash ]
                if (CSEmpregados.Current.USAR_CACHE_PRICE)
                    SetPreparedCommand(command, !temporary);

            }
            else
            {
                // [ Atualiza valores dos parâmetros ]
                for (int j = 0; j < sqlParams.Length; j++)
                    command.Parameters[j].Value = sqlParams[j].Value;
            }

            using (SQLiteDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
            {
                // [ Encontrou resultado? ]
                if (reader.Read())
                {
                    res = CSEmpresa.Current.IND_UTILIZA_PRICE_2014 ? new object[5] : new object[4];
                    res[0] = (reader.GetFieldType(0) == typeof(decimal)) ? reader.GetDecimal(0) : Convert.ToDecimal(reader.GetDouble(0));
                    res[1] = reader.GetString(1);
                    res[2] = Convert.ToDecimal(reader.GetDouble(2));
                    res[3] = reader.GetDecimal(3);

                    if (CSEmpresa.Current.IND_UTILIZA_PRICE_2014)
                        res[4] = reader.GetString(4);
                }
                else
                {
                    // [ Resultado não encontrado, armazena array de tamanho zero ]
                    res = new object[0];
                }
                reader.Close();
                reader.Dispose();
            }

            // [ Guarda o resultado em cache se for o caso ]
            if (CSEmpregados.Current.USAR_CACHE_PRICE)
            {
                if (cacheKey != null)
                    SetCachedResult(cacheKey, res, temporary);
            }

            return (CSEmpresa.Current.IND_UTILIZA_PRICE_2014 ? res.Length == 5 : res.Length == 4) ? res : null;

        }

        public object ExecuteScalar(ref SQLiteCommand command, string sqlQuery, params SQLiteParameter[] sqlParams)
        {
            object objReturn;

            if (command == null)
            {
                // [ Prepara novo comando ]
                command = PrepareCommand(sqlQuery, sqlParams);

            }
            else
            {
                // [ Atualiza valores dos parâmetros ]
                for (int j = 0; j < sqlParams.Length; j++)
                    command.Parameters[j].Value = sqlParams[j].Value;
            }

            objReturn = command.ExecuteScalar();

            return objReturn;
        }

        /// <summary>
        /// Recupera comandos preparados da memória
        /// </summary>
        /// <param name="id">Identificação do comando (pode ser utilizada a query SQL)</param>
        /// <returns>SQLiteCommand preparado</returns>
        public SQLiteCommand GetPreparedCommand(string id)
        {
            return (SQLiteCommand)preparedCommands.Get(id);
        }

        /// <summary>
        /// Armazena comando preparado em memória
        /// </summary>
        /// <param name="command">Comando a ser armazenado</param>
        /// <param name="temporary">Diz se o comando será armazenado temporariamente</param>
        private void SetPreparedCommand(SQLiteCommand command, bool temporary)
        {
            preparedCommands.Put(command.CommandText, command, temporary ? TypedHashtable.HashtableEntryType.Temporary : TypedHashtable.HashtableEntryType.Permanent);
        }

        /// <summary>
        /// Recupera resultados armazenados em memória
        /// </summary>
        /// <param name="id">Identificação do resultado</param>
        /// <returns>Array de object com o resultado</returns>
        private object[] GetCachedResult(string id)
        {
            return (object[])cachedResults.Get(id);
        }

        /// <summary>
        /// Armazena resultado em memória
        /// </summary>
        /// <param name="id">Identificação do resultado</param>
        /// <param name="result">Array de object com o resultado</param>
        /// <param name="temporary">Diz se o resultado será armazenado temporariamente</param>
        private void SetCachedResult(string id, object[] result, bool temporary)
        {
            cachedResults.Put(id, result, temporary ? TypedHashtable.HashtableEntryType.Temporary : TypedHashtable.HashtableEntryType.Permanent);
        }

        /// <summary>
        /// Descarta comandos preparados da memória de acordo com o seu tipo
        /// </summary>
        /// <param name="type">Tipo do comando</param>
        public int DisposePreparedCommands(TypedHashtable.HashtableEntryType type)
        {
            return preparedCommands.Clear(type);
        }

        /// <summary>
        /// Descarta resultado em cache da memória de acordo com o seu tipo
        /// </summary>
        /// <param name="type">Tipo do resultado</param>
        public int DisposeCachedResults(TypedHashtable.HashtableEntryType type)
        {
            return cachedResults.Clear(type);
        }

        /// <summary>
        /// Prepara um novo SQLiteCommand
        /// </summary>
        /// <param name="sqlQuery">Query SQL</param>
        /// <param name="temporary">Diz se o comando será armazenado temporariamente</param>
        /// <param name="sqlParams">Parâmetros da query SQL</param>
        /// <returns>SQLiteCommand preparado</returns>
        public SQLiteCommand PrepareCommand(string sqlQuery, params SQLiteParameter[] sqlParams)
        {
            SQLiteCommand command = sqlConnection.CreateCommand();
            command.CommandText = sqlQuery;

            foreach (SQLiteParameter sqlParam in sqlParams)
                command.Parameters.Add(sqlParam);

            command.Prepare();

            return command;
        }
    }
}