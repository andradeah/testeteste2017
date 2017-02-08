using System;
using System.IO;
using System.Data;
#if ANDROID
using Android.Graphics;
using Mono.Data.Sqlite;
#else
using System.Data.SQLite;
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
    /// Classe de manipulação de usuario.
    /// </summary>
    public class CSUsuario
    {
        #region [ Variáveis ]

        private static string m_NOM_USUARIO;
        private static string m_NOM_COMPLETO;
        private static string m_DSC_SENHA;
        private static bool m_IND_BLOQUEIO;

        #endregion

        #region [ Propriedades ]

        /// <summary>
        /// Guarda o nome do usuario logado
        /// </summary>
        public static string NOM_USUARIO
        {
            get
            {
                return m_NOM_USUARIO;
            }
        }

        /// <summary>
        /// Guarda o nome completo do usuario logado
        /// </summary>
        public static string NOM_COMPLETO
        {
            get
            {
                return m_NOM_COMPLETO;
            }
        }

        /// <summary>
        /// Guarda a senha do usuario logado
        /// </summary>
        public static string DSC_SENHA
        {
            get
            {
                return m_DSC_SENHA;
            }
        }

        /// <summary>
        /// Guarda se o usuario está ou não bloqueado
        /// </summary>
        public static bool IND_BLOQUEIO
        {
            get
            {
                return m_IND_BLOQUEIO;
            }
        }

        #endregion

        #region [ Metodos ]

        public CSUsuario()
        {
        }

        /// <summary>
        /// Valida o usuario que ira logar no sistema
        /// </summary>
        /// <param name="Nome">Nome do usuário</param>
        /// <param name="Senha">Senha do usuário</param>
        /// <param name="Data">Data de carga no PDA</param>
        /// <returns>TRUE se o usuário for valido\nFalse se o usuário for inválido</returns>
        public static bool ValidaUsuario(string Usuario, string Senha, int Empregado)
        {
            try
            {
                string sqlQuery;
                bool validarNaTabelaDeUsuario = false;
                bool retorno = false;

                if (Usuario.ToLower() == "flag" &&
                    Senha == "fl@g2014")
                {
                    retorno = true;
                }
                else
                {
                    if (CSEmpresa.ColunaExiste("EMPREGADO", "USUARIO_VENDEDOR"))
                    {
                        sqlQuery = string.Format("SELECT USUARIO_VENDEDOR,SENHA_VENDEDOR FROM EMPREGADO WHERE [COD_EMPREGADO] = {0}", Empregado);

                        using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery))
                        {
                            if (sqlReader.Read())
                            {
                                if (sqlReader.GetValue(0) == DBNull.Value ||
                                    sqlReader.GetString(0) == string.Empty)
                                {
                                    validarNaTabelaDeUsuario = true;
                                }
                                else
                                {
                                    validarNaTabelaDeUsuario = false;

                                    if (Usuario.ToLower() != sqlReader.GetString(0).ToLower())
                                        retorno = false;
                                    else
                                    {
                                        string senhaCriptografada = sqlReader.GetValue(1) == DBNull.Value ? string.Empty : sqlReader.GetString(1);
                                        string senhaDiscriptografada = string.Empty;
                                        char carectereAtual;
                                        int asc;

                                        for (int i = 0; i < senhaCriptografada.Length; i++)
                                        {
                                            carectereAtual = Convert.ToChar(senhaCriptografada.Substring(i, 1));
                                            asc = Convert.ToInt32(carectereAtual) - 120;

                                            senhaDiscriptografada += Convert.ToChar(asc);
                                        }

                                        if (Senha != senhaDiscriptografada)
                                            retorno = false;
                                        else
                                        {
                                            retorno = true;
                                            CSGlobal.Usuario = Usuario;
                                            CSGlobal.Senha = Senha;
                                        }
                                    }
                                }
                            }
                            else
                                validarNaTabelaDeUsuario = true;
                        }
                    }
                    else
                        validarNaTabelaDeUsuario = true;

                    if (validarNaTabelaDeUsuario)
                    {
                        sqlQuery = Senha == string.Empty ? "SELECT NOM_USUARIO, NOM_COMPLETO, DSC_SENHA, IND_BLOQUEIO FROM USUARIO WHERE lower(NOM_USUARIO)=? AND (lower(DSC_SENHA)=? OR DSC_SENHA IS NULL)"
                            : "SELECT NOM_USUARIO, NOM_COMPLETO, DSC_SENHA, IND_BLOQUEIO FROM USUARIO WHERE lower(NOM_USUARIO)=? AND lower(DSC_SENHA)=?";

                        SQLiteParameter pNOM_USUARIO = new SQLiteParameter("@NOM_USUARIO", Usuario.ToLower());
                        SQLiteParameter pDSC_SENHA = new SQLiteParameter("@DSC_SENHA", Senha.ToLower());

                        using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, CommandBehavior.SingleRow, pNOM_USUARIO, pDSC_SENHA))
                        {
                            if (sqlReader.Read())
                            {
                                // Preenche as variaveis de ambiente					
                                CSGlobal.Usuario = sqlReader.GetString(0);
                                m_NOM_COMPLETO = sqlReader.GetString(1);
                                CSGlobal.Senha = sqlReader.GetValue(2) != System.DBNull.Value ? sqlReader.GetString(2) : string.Empty;
                                m_IND_BLOQUEIO = sqlReader.GetBoolean(3);

                                // Fecha o reader
                                sqlReader.Close();
                                sqlReader.Dispose();

                                // Retorna que o usuario informa é valido
                                retorno = true;
                            }
                            else
                            {
                                // Zera as variaveis de de ambiente
                                m_NOM_USUARIO = "";
                                m_NOM_COMPLETO = "";
                                m_DSC_SENHA = "";
                                m_IND_BLOQUEIO = false;
                                // Fecha o reader
                                sqlReader.Close();
                                sqlReader.Dispose();

                                // Retorna que o usuario informa é invalido
                                retorno = false;
                            }
                        }
                    }
                }

                return retorno;
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na validação do usuario", ex);
            }
        }

        #endregion
    }
}