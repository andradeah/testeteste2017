using System;
using System.Runtime.InteropServices;
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


namespace AvanteSales
{
    /// <summary>
    /// Summary description for CSExceptions.
    /// </summary>
    public class CSExceptions
    {
        private static Exception current;

        public static Exception Current
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

        public class CSException : System.Exception
        {
            public override string Message
            {
                get
                {
                    return base.Message;
                }
            }

            public override string ToString()
            {
                return GetFullMessage(this);
            }

            public static string GetFullMessage(Exception exception)
            {
                string message = "";

                try
                {
                    do
                    {
                        System.Type t = exception.GetType();

                        // Passo a mensagem de erro para a tela de erro
                        message += exception.GetType().FullName;
                        message += "\r\n";
                        message += GetMessage(exception);
                        message += "\r\n";
                        message += exception.StackTrace;
                        message += "\r\n";
                        message += "----------------------------------------------";
                        message += "\r\n";

                        // Pega a exceção que gerou a exceção
                        exception = exception.InnerException;

                    } while (exception != null);

                }
                catch (Exception e)
                {
                    message += e.GetType().FullName + ":\r\n";
                    message += GetMessage(exception) + "\r\n";
                    message += "---------------------------------------------------";
                    message += "\r\n";
                    CSGlobal.ShowMessage(e.ToString());
                }

                // retorna string com o erro
                return message;
            }

            private static string GetMessage(Exception exception)
            {
                string message;

                switch (exception.GetType().FullName)
                {
                    case "System.Exception":
                        message = ((System.Exception)exception).Message;
                        break;
                    case "System.Data.SQLite.SQLiteException":
                        message = ((SQLiteException)exception).Message;
                        break;
                    case "System.OverflowException":
                        message = ((System.OverflowException)exception).Message;
                        break;
                    case "System.IO.IOException":
                        message = ((System.IO.IOException)exception).Message;
                        break;
                    default:
                        message = exception.Message;
                        break;
                }
                return message;
            }
        }
    }
}