using System;
using System.Data;
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
using System.Collections;
using System.Text;
using AvanteSales.SystemFramework.BusinessLayer;

namespace AvanteSales
{
    /// <summary>
    /// Summary description for CSEmpregados.
    /// </summary>
    public class CSEmpregados : CollectionBase
    {
        #region [ Variaveis ]

        private static CSEmpregados m_Items;
        private static CSEmpregados.CSEmpregado m_Current;

        #endregion

        #region [ Propriedades ]

        /// <summary>
        /// Guarda a instancia do empregado atual
        /// </summary>
        public static CSEmpregados.CSEmpregado Current
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

        #region [ Funções ]

        public CSEmpregados()
        {
            try
            {
                StringBuilder sqlQuery = new StringBuilder();
                sqlQuery.Append("SELECT EMPREGADO.COD_EMPREGADO, NOM_EMPREGADO, COD_SUPERVISOR, NOM_SUPERVISOR ");
                sqlQuery.AppendLine("      ,COD_GERENTE, NOM_GERENTE, IND_EMPREGADO_PADRAO, COD_GRUPO_COMERCIALIZACAO ");
                sqlQuery.AppendLine("      ,VAL_SALDO_DESCONTO ");

                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "IND_UTILIZA_PEDIDO_SUGERIDO") ? ",IND_UTILIZA_PEDIDO_SUGERIDO" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "COD_ULTIMO_PEDIDO") ? ",COD_ULTIMO_PEDIDO" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "IND_UTILIZA_FLEXXGPS") ? ",IND_UTILIZA_FLEXXGPS" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "IND_BLOQUEADO_VENDA_FORA_ROTA") ? ",IND_BLOQUEADO_VENDA_FORA_ROTA" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "PCT_MAX_PEDIDO_FORA_ROTA") ? ",PCT_MAX_PEDIDO_FORA_ROTA" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "IND_FOTO_OBRIGATORIA") ? ",IND_FOTO_OBRIGATORIA" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "IND_PERMITIR_FOTO") ? ",IND_PERMITIR_FOTO" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "NUM_CPF_EMPREGADO") ? ",NUM_CPF_EMPREGADO" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "DAT_HORA_INICIO_EXPEDIENTE") ? ",DAT_HORA_INICIO_EXPEDIENTE" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "DAT_HORA_FIM_EXPEDIENTE") ? ",DAT_HORA_FIM_EXPEDIENTE" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO_EXPEDIENTE", "DAT_INICIO_EXPEDIENTE") ? ",EMPREGADO_EXPEDIENTE.DAT_INICIO_EXPEDIENTE" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO_EXPEDIENTE", "DAT_FIM_EXPEDIENTE") ? ",EMPREGADO_EXPEDIENTE.DAT_FIM_EXPEDIENTE" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "IND_VALIDAR_LOCALIZACAO") ? ",IND_VALIDAR_LOCALIZACAO" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "NUM_MINUTOS_TOTAL_EXPEDIENTE") ? ",NUM_MINUTOS_TOTAL_EXPEDIENTE" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "NUM_MINUTOS_INTERVALO_ALMOCO") ? ",NUM_MINUTOS_INTERVALO_ALMOCO" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO_EXPEDIENTE", "DAT_INICIO_EXPEDIENTE") ? ",EMPREGADO_TRABALHO.DAT_INICIO_EXPEDIENTE" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO_EXPEDIENTE", "DAT_FIM_EXPEDIENTE") ? ",EMPREGADO_TRABALHO.DAT_FIM_EXPEDIENTE" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "IND_VALIDAR_EXPEDIENTE") ? ",EMPREGADO.IND_VALIDAR_EXPEDIENTE" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "IND_ORDEM_VISITA_OBRIGATORIA") ? ",EMPREGADO.IND_ORDEM_VISITA_OBRIGATORIA" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "IND_BLOQUEAR_INDENIZACAO") ? ",EMPREGADO.IND_BLOQUEAR_INDENIZACAO" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "IND_PERMITIR_ROTINA_IMAGEM") ? ",EMPREGADO.IND_PERMITIR_ROTINA_IMAGEM" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "IND_PERMITIR_ROTINA_VENDA_RAPIDA") ? ",EMPREGADO.IND_PERMITIR_ROTINA_VENDA_RAPIDA" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "USAR_CACHE_PRICE") ? ",EMPREGADO.USAR_CACHE_PRICE" : string.Empty);
                sqlQuery.AppendLine(CSEmpresa.ColunaExiste("EMPREGADO", "IND_FINALIZA_JORNADA_AUTOMATICA") ? ",IND_FINALIZA_JORNADA_AUTOMATICA" : string.Empty);

                sqlQuery.AppendLine("  FROM EMPREGADO ");

                if (CSEmpresa.ColunaExiste("EMPREGADO_EXPEDIENTE", "COD_EMPREGADO"))
                {
                    sqlQuery.AppendLine(" LEFT JOIN EMPREGADO_EXPEDIENTE ");
                    sqlQuery.AppendLine("       ON EMPREGADO_EXPEDIENTE.COD_EMPREGADO = EMPREGADO.COD_EMPREGADO ");
                    sqlQuery.AppendFormat(" AND DATE(EMPREGADO_EXPEDIENTE.DAT_INICIO_EXPEDIENTE) = DATE('{0}') ", DateTime.Now.ToString("yyyy-MM-dd"));
                    sqlQuery.AppendFormat(" AND EMPREGADO_EXPEDIENTE.IND_TIPO_EXPEDIENTE = 'A'");

                    sqlQuery.AppendLine(" LEFT JOIN EMPREGADO_EXPEDIENTE EMPREGADO_TRABALHO ");
                    sqlQuery.AppendLine("       ON EMPREGADO_TRABALHO.COD_EMPREGADO = EMPREGADO.COD_EMPREGADO ");
                    sqlQuery.AppendFormat(" AND DATE(EMPREGADO_TRABALHO.DAT_INICIO_EXPEDIENTE) = DATE('{0}') ", DateTime.Now.ToString("yyyy-MM-dd"));
                    sqlQuery.AppendFormat(" AND EMPREGADO_TRABALHO.IND_TIPO_EXPEDIENTE = 'T'");
                }

                sqlQuery.AppendLine(" WHERE COD_GERENTE != 0 ");

                sqlQuery.AppendLine(" ORDER BY EMPREGADO.COD_EMPREGADO");

                // Busca todos os PDVs
                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString()))
                {
                    while (sqlReader.Read())
                    {
                        CSEmpregado emp = new CSEmpregado();

                        // Preenche a instancia da classe pdv
                        emp.COD_EMPREGADO = sqlReader.GetInt32(0);
                        emp.NOM_EMPREGADO = sqlReader.GetString(1);
                        emp.COD_SUPERVISOR = sqlReader.GetInt32(2);
                        emp.NOM_SUPERVISOR = sqlReader.GetString(3);
                        emp.COD_GERENTE = sqlReader.GetInt32(4);
                        emp.NOM_GERENTE = sqlReader.GetString(5);
                        emp.IND_EMPREGADO_PADRAO = sqlReader.GetValue(6) == System.DBNull.Value ? false : sqlReader.GetBoolean(6);
                        emp.COD_GRUPO_COMERCIALIZACAO = sqlReader.GetInt32(7);
                        emp.VAL_SALDO_DESCONTO = sqlReader.GetValue(8) == System.DBNull.Value ? 0 : decimal.Parse(sqlReader.GetValue(8).ToString());

                        int ultimoCampo = 8;

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "IND_UTILIZA_PEDIDO_SUGERIDO"))
                        {
                            ultimoCampo++;
                            emp.IND_UTILIZA_PEDIDO_SUGERIDO = sqlReader.GetValue(ultimoCampo) == System.DBNull.Value ? false : sqlReader.GetBoolean(ultimoCampo);
                        }
                        else
                            emp.IND_UTILIZA_PEDIDO_SUGERIDO = false;

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "COD_ULTIMO_PEDIDO"))
                        {
                            ultimoCampo++;
                            emp.COD_ULTIMO_PEDIDO = sqlReader.GetValue(ultimoCampo) == System.DBNull.Value ? -1 : sqlReader.GetInt32(ultimoCampo);
                        }
                        else
                            emp.COD_ULTIMO_PEDIDO = -1;

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "IND_UTILIZA_FLEXXGPS"))
                        {
                            ultimoCampo++;
                            emp.IND_VALIDA_FLEXXGPS_INSTALADO = sqlReader.GetValue(ultimoCampo) == System.DBNull.Value ? false : sqlReader.GetBoolean(ultimoCampo);
                        }
                        else
                            emp.IND_VALIDA_FLEXXGPS_INSTALADO = false;

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "IND_BLOQUEADO_VENDA_FORA_ROTA"))
                        {
                            ultimoCampo++;
                            emp.IND_BLOQUEADO_VENDA_FORA_ROTA = sqlReader.GetValue(ultimoCampo) == System.DBNull.Value ? true : sqlReader.GetBoolean(ultimoCampo);
                        }
                        else
                            emp.IND_BLOQUEADO_VENDA_FORA_ROTA = false;

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "PCT_MAX_PEDIDO_FORA_ROTA"))
                        {
                            ultimoCampo++;
                            emp.PCT_MAX_PEDIDO_FORA_ROTA = sqlReader.GetValue(ultimoCampo) == System.DBNull.Value ? 0m : sqlReader.GetDecimal(ultimoCampo);
                        }
                        else
                            emp.PCT_MAX_PEDIDO_FORA_ROTA = 0m;

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "IND_FOTO_OBRIGATORIA"))
                        {
                            ultimoCampo++;
                            emp.IND_FOTO_OBRIGATORIA = sqlReader.GetValue(ultimoCampo) == System.DBNull.Value ? false : sqlReader.GetBoolean(ultimoCampo);
                        }
                        else
                            emp.IND_FOTO_OBRIGATORIA = false;

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "IND_PERMITIR_FOTO"))
                        {
                            ultimoCampo++;
                            emp.IND_PERMITIR_FOTO = sqlReader.GetValue(ultimoCampo) == System.DBNull.Value ? false : sqlReader.GetBoolean(ultimoCampo);
                        }
                        else
                            emp.IND_PERMITIR_FOTO = false;

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "NUM_CPF_EMPREGADO"))
                        {
                            ultimoCampo++;
                            emp.NUM_CPF_EMPREGADO = sqlReader.GetValue(ultimoCampo) == System.DBNull.Value ? string.Empty : sqlReader.GetString(ultimoCampo);
                        }
                        else
                            emp.NUM_CPF_EMPREGADO = string.Empty;

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "DAT_HORA_INICIO_EXPEDIENTE"))
                        {
                            ultimoCampo++;

                            if (sqlReader.GetValue(ultimoCampo) != System.DBNull.Value)
                                emp.DAT_HORA_INICIO_EXPEDIENTE = sqlReader.GetDateTime(ultimoCampo);
                        }

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "DAT_HORA_FIM_EXPEDIENTE"))
                        {
                            ultimoCampo++;

                            if (sqlReader.GetValue(ultimoCampo) != System.DBNull.Value)
                                emp.DAT_HORA_FIM_EXPEDIENTE = sqlReader.GetDateTime(ultimoCampo);
                        }

                        if (CSEmpresa.ColunaExiste("EMPREGADO_EXPEDIENTE", "DAT_INICIO_EXPEDIENTE"))
                        {
                            ultimoCampo++;

                            if (sqlReader.GetValue(ultimoCampo) != System.DBNull.Value)
                                emp.DAT_INICIO_ALMOCO = sqlReader.GetDateTime(ultimoCampo);
                        }

                        if (CSEmpresa.ColunaExiste("EMPREGADO_EXPEDIENTE", "DAT_FIM_EXPEDIENTE"))
                        {
                            ultimoCampo++;

                            if (sqlReader.GetValue(ultimoCampo) != System.DBNull.Value)
                                emp.DAT_FIM_ALMOCO = sqlReader.GetDateTime(ultimoCampo);
                        }

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "IND_VALIDAR_LOCALIZACAO"))
                        {
                            ultimoCampo++;

                            emp.IND_VALIDAR_LOCALIZACAO = sqlReader.GetValue(ultimoCampo) == System.DBNull.Value ? false : sqlReader.GetBoolean(ultimoCampo);
                        }
                        else
                            emp.IND_VALIDAR_LOCALIZACAO = false;

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "NUM_MINUTOS_TOTAL_EXPEDIENTE"))
                        {
                            ultimoCampo++;

                            emp.NUM_MINUTOS_TOTAL_EXPEDIENTE = sqlReader.GetValue(ultimoCampo) == System.DBNull.Value ? 0 : sqlReader.GetInt32(ultimoCampo);
                        }

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "NUM_MINUTOS_INTERVALO_ALMOCO"))
                        {
                            ultimoCampo++;

                            emp.NUM_MINUTOS_INTERVALO_ALMOCO = sqlReader.GetValue(ultimoCampo) == System.DBNull.Value ? 0 : sqlReader.GetInt32(ultimoCampo);
                        }


                        if (CSEmpresa.ColunaExiste("EMPREGADO_EXPEDIENTE", "DAT_INICIO_EXPEDIENTE"))
                        {
                            ultimoCampo++;

                            if (sqlReader.GetValue(ultimoCampo) != System.DBNull.Value)
                                emp.DAT_INICIO_TRABALHO = sqlReader.GetDateTime(ultimoCampo);
                        }

                        if (CSEmpresa.ColunaExiste("EMPREGADO_EXPEDIENTE", "DAT_FIM_EXPEDIENTE"))
                        {
                            ultimoCampo++;

                            if (sqlReader.GetValue(ultimoCampo) != System.DBNull.Value)
                                emp.DAT_FIM_TRABALHO = sqlReader.GetDateTime(ultimoCampo);
                        }

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "IND_VALIDAR_EXPEDIENTE"))
                        {
                            ultimoCampo++;

                            emp.IND_VALIDAR_EXPEDIENTE = sqlReader.GetValue(ultimoCampo) == System.DBNull.Value ? false : sqlReader.GetBoolean(ultimoCampo);
                        }
                        else
                            emp.IND_VALIDAR_EXPEDIENTE = false;

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "IND_ORDEM_VISITA_OBRIGATORIA"))
                        {
                            ultimoCampo++;
                            emp.IND_ORDEM_VISITA_OBRIGATORIA = sqlReader.GetValue(ultimoCampo) == System.DBNull.Value ? false : sqlReader.GetBoolean(ultimoCampo);
                        }
                        else
                            emp.IND_ORDEM_VISITA_OBRIGATORIA = false;

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "IND_BLOQUEAR_INDENIZACAO"))
                        {
                            ultimoCampo++;
                            emp.IND_BLOQUEAR_INDENIZACAO = sqlReader.GetValue(ultimoCampo) == System.DBNull.Value ? false : sqlReader.GetBoolean(ultimoCampo);
                        }
                        else
                            emp.IND_BLOQUEAR_INDENIZACAO = false;

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "IND_PERMITIR_ROTINA_IMAGEM"))
                        {
                            ultimoCampo++;
                            emp.IND_PERMITIR_ROTINA_IMAGEM = sqlReader.GetValue(ultimoCampo) == System.DBNull.Value ? true : sqlReader.GetBoolean(ultimoCampo);
                        }
                        else
                            emp.IND_PERMITIR_ROTINA_IMAGEM = true;

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "IND_PERMITIR_ROTINA_VENDA_RAPIDA"))
                        {
                            ultimoCampo++;
                            emp.IND_PERMITIR_ROTINA_VENDA_RAPIDA = sqlReader.GetValue(ultimoCampo) == System.DBNull.Value ? true : sqlReader.GetBoolean(ultimoCampo);
                        }
                        else
                            emp.IND_PERMITIR_ROTINA_VENDA_RAPIDA = true;

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "USAR_CACHE_PRICE"))
                        {
                            ultimoCampo++;
                            emp.USAR_CACHE_PRICE = sqlReader.GetValue(ultimoCampo) == System.DBNull.Value ? true : sqlReader.GetBoolean(ultimoCampo);
                        }
                        else
                            emp.USAR_CACHE_PRICE = true;

                        if (CSEmpresa.ColunaExiste("EMPREGADO", "IND_FINALIZA_JORNADA_AUTOMATICA"))
                        {
                            ultimoCampo++;
                            emp.IND_FINALIZA_JORNADA_AUTOMATICA = sqlReader.GetValue(ultimoCampo) == System.DBNull.Value ? false : sqlReader.GetBoolean(ultimoCampo);
                        }
                        else
                            emp.IND_FINALIZA_JORNADA_AUTOMATICA = false;

                        // Adiciona a instancia da classe pdv na coleção de PDVs
                        base.InnerList.Add(emp);
                    }

                    // Fecha o reader
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                // Colocado este teste para nao mostrar mensagens de erro desnecessárias
                //para que nao possam confundir o usuario
                if (CSDataAccess.Instance.TableExists("PEDIDO"))
                {
                    CSGlobal.ShowMessage(ex.ToString());
                    throw new Exception("Erro na busca dos empregados", ex);
                }
            }
        }

        public static bool PossuiPedidosPendentes()
        {
            if ((CSEmpresa.Current.IND_EMPRESA_FERIADO ||
                !CSEmpregados.Current.IND_VALIDAR_EXPEDIENTE) ||
                (CSEmpregados.Current.NUM_MINUTOS_TOTAL_EXPEDIENTE > 0 &&
                !CSEmpregados.Current.ExpedienteIniciado()) ||
                (CSEmpregados.Current.NUM_MINUTOS_TOTAL_EXPEDIENTE == 0 &&
                !CSEmpregados.Current.DAT_HORA_INICIO_EXPEDIENTE.HasValue))
                return false;

            var result = CSDataAccess.Instance.ExecuteScalar(
                string.Format("SELECT COUNT(*) " +
                   "  FROM PEDIDO " +
                   " WHERE IND_HISTORICO = 0 " +
                   "   AND DATE(DAT_ALTERACAO) >= DATE('{0}') AND BOL_PEDIDO_VALIDADO = 1", CSDescarga.DataUltimaDescarga.ToString("yyyy-MM-dd")));

            return Convert.ToInt32(result) > 0 ? true : false;
        }

        /// <summary>
        /// Adiciona mais um cliente na coleção
        /// </summary>
        /// <param name="c">Instacia do cliente a ser adcionada</param>
        /// <returns>return a posição do cliente na coleção</returns>
        public static int Add(CSEmpregado c)
        {
            return m_Items.InnerList.Add(c);
        }

        // Retorna um arrary dos empregados
        public static CSEmpregados Items
        {
            get
            {
                if (m_Items == null || m_Items.Count == 0)
                {
                    m_Items = new CSEmpregados();
                }
                return m_Items;
            }
        }

        public static bool ValidaEmpregado(int codigoEmpregado)
        {
            try
            {
                string sqlQuery =
                    "SELECT COD_EMPREGADO " +
                    "  FROM EMPREGADO " +
                    " WHERE COD_EMPREGADO = ? ";

                SQLiteParameter pCOD_EMPREGADO = new SQLiteParameter("@pCOD_EMPREGADO", codigoEmpregado);

                return (CSDataAccess.Instance.ExecuteScalar(sqlQuery, pCOD_EMPREGADO) != null);

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na validação do empregado", ex);
            }
        }

        /// <summary>
        /// Busca o empregado pelo codigo
        /// </summary>
        /// <param name="COD_OPERACAO">Codigo do empregado a ser procurado</param>
        /// <returns>Retorna o objeto do empregado</returns>
        public static CSEmpregados.CSEmpregado GetEmpregado(int COD_EMPREGADO)
        {
            CSEmpregados.CSEmpregado ret = null;
            // Procura pela operação
            foreach (CSEmpregados.CSEmpregado emp in m_Items.InnerList)
            {
                if (emp.COD_EMPREGADO == COD_EMPREGADO)
                {
                    ret = emp;
                    break;
                }
            }
            // retorna o objeto do empregado
            return ret;
        }

        #endregion

        /// <summary>
        /// Summary description for CSEmpregado.
        /// </summary>
        public class CSEmpregado
        {
            #region [ Variaveis ]

            private int m_COD_ULTIMO_PEDIDO;
            private int m_COD_EMPREGADO;
            private bool m_USAR_CACHE_PRICE;
            private bool m_IND_FINALIZA_JORNADA_AUTOMATICA;
            private int m_COD_EMPREGADO_FONTE;
            private int m_COD_SUPERVISOR;
            private int m_COD_GERENTE;
            private string m_NOM_EMPREGADO;
            private string m_NOM_SUPERVISOR;
            private string m_NUM_CPF_EMPREGADO;
            private string m_NOM_GERENTE;
            private bool m_IND_EMPREGADO_PADRAO;
            private int m_COD_GRUPO_COMERCIALIZACAO;
            private CSEmpregado.CSDocumentosReceberEmpregados m_DOCUMENTOS_RECEBER_EMPREGADO;
            private decimal m_VAL_SALDO_DESCONTO;
            private bool m_IND_UTILIZA_PEDIDO_SUGERIDO;
            private bool m_IND_VALIDA_FLEXXGPS_INSTALADO;
            private bool m_IND_VALIDAR_EXPEDIENTE;
            private bool m_IND_BLOQUEADO_VENDA_FORA_ROTA;
            private bool m_IND_ORDEM_VISITA_OBRIGATORIA;
            private bool m_IND_BLOQUEAR_INDENIZACAO;
            private bool m_IND_PERMITIR_ROTINA_IMAGEM;
            private bool m_IND_PERMITIR_ROTINA_VENDA_RAPIDA;
            private decimal m_PCT_MAX_PEDIDO_FORA_ROTA;
            private int m_QTD_MAX_VISITA_FORA_ROTA_POSITIVADA;
            private bool m_IND_FOTO_OBRIGATORIA;
            private bool m_IND_PERMITIR_FOTO;
            private bool m_IND_VALIDAR_LOCALIZACAO;
            private int m_NUM_MINUTOS_TOTAL_EXPEDIENTE;
            private int m_NUM_MINUTOS_INTERVALO_ALMOCO;
            private double m_VLR_META_EMPREGADO;
            private CSVisitas m_VISITAS_EMPREGADO;
            private DateTime? m_DAT_HORA_INICIO_EXPEDIENTE;
            private DateTime? m_DAT_HORA_FIM_EXPEDIENTE;
            private DateTime? m_DAT_INICIO_ALMOCO;
            private DateTime? m_DAT_FIM_ALMOCO;
            private DateTime? m_DAT_INICIO_TRABALHO;
            private DateTime? m_DAT_FIM_TRABALHO;

            #endregion

            #region [ Propriedades ]

            public CSVisitas VISITAS_EMPREGADO
            {
                get
                {
                    if (CSEmpresa.ColunaExiste("PDV_VISITA", "COD_EMPREGADO"))
                    {
                        if (m_VISITAS_EMPREGADO == null)
                            m_VISITAS_EMPREGADO = new CSVisitas().Items;
                    }
                    else
                    {
                        if (m_VISITAS_EMPREGADO == null)
                            m_VISITAS_EMPREGADO = new CSVisitas();
                    }

                    return m_VISITAS_EMPREGADO;
                }
                set
                {
                    m_VISITAS_EMPREGADO = value;
                }
            }

            /// <summary>
            /// Guarda o codigo do usuario logado
            /// </summary>
            public int COD_EMPREGADO
            {
                get
                {
                    if (m_COD_EMPREGADO_FONTE != 0)
                        return m_COD_EMPREGADO_FONTE;

                    return m_COD_EMPREGADO;
                }
                set
                {
                    m_COD_EMPREGADO = value;
                }
            }

            public bool USAR_CACHE_PRICE
            {
                get
                {
                    return m_USAR_CACHE_PRICE;
                }
                set
                {
                    m_USAR_CACHE_PRICE = value;
                }
            }

            public bool IND_FINALIZA_JORNADA_AUTOMATICA
            {
                get
                {
                    return m_IND_FINALIZA_JORNADA_AUTOMATICA;
                }
                set
                {
                    m_IND_FINALIZA_JORNADA_AUTOMATICA = value;
                }
            }

            /// <summary>
            /// Guarda o codigo do usuario que realizou o pedido
            /// </summary>
            public int COD_EMPREGADO_FONTE
            {
                get
                {
                    return m_COD_EMPREGADO_FONTE;
                }
                set
                {
                    m_COD_EMPREGADO_FONTE = value;
                }
            }

            /// <summary>
            /// Guarda o código do último pedido descarregado do empregado
            /// </summary>
            public int COD_ULTIMO_PEDIDO
            {
                get
                {
                    return m_COD_ULTIMO_PEDIDO;
                }
                set
                {
                    m_COD_ULTIMO_PEDIDO = value;
                }
            }

            /// <summary>
            /// Guarda o codigo do supervisor do empregado
            /// </summary>
            public int COD_SUPERVISOR
            {
                get
                {
                    return m_COD_SUPERVISOR;
                }
                set
                {
                    m_COD_SUPERVISOR = value;
                }
            }

            /// <summary>
            /// Guarda o codigo do gerente do empregado
            /// </summary>
            public int COD_GERENTE
            {
                get
                {
                    return m_COD_GERENTE;
                }
                set
                {
                    m_COD_GERENTE = value;
                }
            }

            /// <summary>
            /// Guarda o nome do empregado
            /// </summary>
            public string NOM_EMPREGADO
            {
                get
                {
                    return m_NOM_EMPREGADO;
                }
                set
                {
                    m_NOM_EMPREGADO = value;
                }
            }

            /// <summary>
            /// Guarda o nome do supervisor do empregado
            /// </summary>
            public string NOM_SUPERVISOR
            {
                get
                {
                    return m_NOM_SUPERVISOR;
                }
                set
                {
                    m_NOM_SUPERVISOR = value;
                }
            }

            /// <summary>
            /// Guarda o nome do gerente do empregado
            /// </summary>
            public string NOM_GERENTE
            {
                get
                {
                    return m_NOM_GERENTE;
                }
                set
                {
                    m_NOM_GERENTE = value;
                }
            }

            /// <summary>
            /// Guarda o status do empregado
            /// </summary>
            public bool IND_EMPREGADO_PADRAO
            {
                get
                {
                    return m_IND_EMPREGADO_PADRAO;
                }
                set
                {
                    m_IND_EMPREGADO_PADRAO = value;
                }
            }

            public int COD_GRUPO_COMERCIALIZACAO
            {
                get
                {
                    return m_COD_GRUPO_COMERCIALIZACAO;
                }
                set
                {
                    m_COD_GRUPO_COMERCIALIZACAO = value;
                }
            }

            public bool IND_VALIDA_FLEXXGPS_INSTALADO
            {
                get
                {
                    return m_IND_VALIDA_FLEXXGPS_INSTALADO;
                }
                set
                {
                    m_IND_VALIDA_FLEXXGPS_INSTALADO = value;
                }
            }

            public bool IND_VALIDAR_EXPEDIENTE
            {
                get
                {
                    return m_IND_VALIDAR_EXPEDIENTE;
                }
                set
                {
                    m_IND_VALIDAR_EXPEDIENTE = value;
                }
            }

            public bool IND_ORDEM_VISITA_OBRIGATORIA
            {
                get
                {
                    if (!CSEmpresa.ColunaExiste("PDV_VISITA", "COD_EMPREGADO"))
                        return false;
                    else
                        return m_IND_ORDEM_VISITA_OBRIGATORIA;
                }
                set
                {
                    m_IND_ORDEM_VISITA_OBRIGATORIA = value;
                }
            }

            public bool IND_BLOQUEAR_INDENIZACAO
            {
                get
                {
                    return m_IND_BLOQUEAR_INDENIZACAO;
                }
                set
                {
                    m_IND_BLOQUEAR_INDENIZACAO = value;
                }
            }

            public bool IND_PERMITIR_ROTINA_IMAGEM
            {
                get
                {
                    return m_IND_PERMITIR_ROTINA_IMAGEM;
                }
                set
                {
                    m_IND_PERMITIR_ROTINA_IMAGEM = value;
                }
            }

            public bool IND_PERMITIR_ROTINA_VENDA_RAPIDA
            {
                get
                {
                    return false;
                    //return m_IND_PERMITIR_ROTINA_VENDA_RAPIDA;
                }
                set
                {
                    m_IND_PERMITIR_ROTINA_VENDA_RAPIDA = value;
                }
            }

            public CSEmpregados.CSEmpregado.CSDocumentosReceberEmpregados DOCUMENTOS_RECEBER_EMPREGADO
            {
                get
                {
                    if (m_DOCUMENTOS_RECEBER_EMPREGADO == null)
                        m_DOCUMENTOS_RECEBER_EMPREGADO = new CSDocumentosReceberEmpregados(this.COD_EMPREGADO);
                    return m_DOCUMENTOS_RECEBER_EMPREGADO;
                }
                set
                {
                    m_DOCUMENTOS_RECEBER_EMPREGADO = value;
                }
            }

            public string NUM_CPF_EMPREGADO
            {
                get
                {
                    return m_NUM_CPF_EMPREGADO;
                }
                set
                {
                    m_NUM_CPF_EMPREGADO = value;
                }
            }

            public DateTime? DAT_HORA_INICIO_EXPEDIENTE
            {
                get
                {
                    return m_DAT_HORA_INICIO_EXPEDIENTE;
                }
                set
                {
                    m_DAT_HORA_INICIO_EXPEDIENTE = value;
                }
            }

            public DateTime? DAT_HORA_FIM_EXPEDIENTE
            {
                get
                {
                    return m_DAT_HORA_FIM_EXPEDIENTE;
                }
                set
                {
                    m_DAT_HORA_FIM_EXPEDIENTE = value;
                }
            }

            public DateTime? DAT_INICIO_ALMOCO
            {
                get
                {
                    return m_DAT_INICIO_ALMOCO;
                }
                set
                {
                    m_DAT_INICIO_ALMOCO = value;
                }
            }

            public DateTime? DAT_FIM_ALMOCO
            {
                get
                {
                    return m_DAT_FIM_ALMOCO;
                }
                set
                {
                    m_DAT_FIM_ALMOCO = value;
                }
            }

            public DateTime? DAT_INICIO_TRABALHO
            {
                get
                {
                    return m_DAT_INICIO_TRABALHO;
                }
                set
                {
                    m_DAT_INICIO_TRABALHO = value;
                }
            }

            public DateTime? DAT_FIM_TRABALHO
            {
                get
                {
                    return m_DAT_FIM_TRABALHO;
                }
                set
                {
                    m_DAT_FIM_TRABALHO = value;
                }
            }

            public bool AlmocoIniciado()
            {
                return this.DAT_INICIO_ALMOCO.HasValue;
            }

            public void FinalizarExpedienteAnterior()
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendFormat("UPDATE EMPREGADO_EXPEDIENTE SET DAT_FIM_EXPEDIENTE = DATETIME('{0}') ", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                sql.AppendFormat("WHERE IND_TIPO_EXPEDIENTE = 'T' AND DATE(DAT_INICIO_EXPEDIENTE) <> DATE('{0}')", DateTime.Now.ToString("yyyy-MM-dd"));

                CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());
            }

            /// <summary>
            /// Retorna se existe um expediente do dia anterior em aberto
            /// </summary>
            /// <returns></returns>
            public bool ExpedienteAnteriorExistente()
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT COD_EMPREGADO FROM EMPREGADO_EXPEDIENTE WHERE IND_TIPO_EXPEDIENTE = 'T' AND DAT_FIM_EXPEDIENTE IS NULL AND ");
                sql.AppendFormat("DATE(DAT_INICIO_EXPEDIENTE) <> DATE('{0}')", DateTime.Now.ToString("yyyy-MM-dd"));

                if (CSDataAccess.Instance.ExecuteScalar(sql.ToString()) != null)
                    return true;

                return false;
            }

            public bool ExpedienteIniciado()
            {
                return this.DAT_INICIO_TRABALHO.HasValue;
            }

            public bool AlmocoIniciadoNaoFinalizado()
            {
                return this.DAT_INICIO_ALMOCO.HasValue && !this.DAT_FIM_ALMOCO.HasValue;
            }

            public bool ExpedienteIniciadoNaoFinalizado()
            {
                return this.DAT_INICIO_TRABALHO.HasValue && !this.DAT_FIM_TRABALHO.HasValue;
            }

            public bool ExpedienteFinalizado()
            {
                return this.DAT_FIM_TRABALHO.HasValue;
            }

            public bool AlmocoFinalizado()
            {
                return this.DAT_FIM_ALMOCO.HasValue;
            }

            /// <summary>
            /// Guarda o saldo de desconto
            /// </summary>
            public decimal VAL_SALDO_DESCONTO
            {
                get
                {
                    return m_VAL_SALDO_DESCONTO;
                }
                set
                {
                    m_VAL_SALDO_DESCONTO = value;
                }
            }

            public bool IND_UTILIZA_PEDIDO_SUGERIDO
            {
                get
                {
                    return m_IND_UTILIZA_PEDIDO_SUGERIDO;
                }
                set
                {
                    m_IND_UTILIZA_PEDIDO_SUGERIDO = value;
                }
            }

            public bool IND_BLOQUEADO_VENDA_FORA_ROTA
            {
                get
                {
                    return m_IND_BLOQUEADO_VENDA_FORA_ROTA;
                }
                set
                {
                    m_IND_BLOQUEADO_VENDA_FORA_ROTA = value;
                }
            }

            public decimal PCT_MAX_PEDIDO_FORA_ROTA
            {
                get
                {
                    return m_PCT_MAX_PEDIDO_FORA_ROTA;
                }
                set
                {
                    m_PCT_MAX_PEDIDO_FORA_ROTA = value;
                }
            }

            public int QTD_MAX_VISITA_FORA_ROTA_POSITIVADA
            {
                get
                {
                    return m_QTD_MAX_VISITA_FORA_ROTA_POSITIVADA;
                }
                set
                {
                    if (m_QTD_MAX_VISITA_FORA_ROTA_POSITIVADA == 0)
                        m_QTD_MAX_VISITA_FORA_ROTA_POSITIVADA = value;
                }
            }

            public bool IND_FOTO_OBRIGATORIA
            {
                get
                {
                    return m_IND_FOTO_OBRIGATORIA;
                }
                set
                {
                    m_IND_FOTO_OBRIGATORIA = value;
                }
            }

            public bool IND_PERMITIR_FOTO
            {
                get
                {
                    return false;
                    //return m_IND_PERMITIR_FOTO;
                }
                set
                {
                    m_IND_PERMITIR_FOTO = value;
                }
            }

            public bool IND_VALIDAR_LOCALIZACAO
            {
                get
                {
                    return m_IND_VALIDAR_LOCALIZACAO;
                }
                set
                {
                    m_IND_VALIDAR_LOCALIZACAO = value;
                }
            }

            public int NUM_MINUTOS_TOTAL_EXPEDIENTE
            {
                get
                {
                    return m_NUM_MINUTOS_TOTAL_EXPEDIENTE;
                }
                set
                {
                    m_NUM_MINUTOS_TOTAL_EXPEDIENTE = value;
                }
            }

            public int NUM_MINUTOS_INTERVALO_ALMOCO
            {
                get
                {
                    return m_NUM_MINUTOS_INTERVALO_ALMOCO;
                }
                set
                {
                    m_NUM_MINUTOS_INTERVALO_ALMOCO = value;
                }
            }

            public double VLR_META_EMPREGADO
            {
                get
                {
                    return RETORNAR_META_EMPREGADO();
                }
            }

            private double RETORNAR_META_EMPREGADO()
            {
                double qtd = 0;

                StringBuilder sqlQuery = new StringBuilder();
                sqlQuery.AppendLine("SELECT SUM(VLR_OBJETIVO) FROM EMPREGADO JOIN PDV_OBJETIVO ON EMPREGADO.COD_EMPREGADO = PDV_OBJETIVO.COD_EMPREGADO ");
                sqlQuery.AppendFormat(" WHERE EMPREGADO.COD_EMPREGADO = {0} AND DATE(PDV_OBJETIVO.DAT_OBJETIVO) = DATE('{1}')", this.COD_EMPREGADO, DateTime.Now.ToString("yyyy-MM-dd"));

                var result = CSDataAccess.Instance.ExecuteScalar(sqlQuery.ToString());

                if (result == System.DBNull.Value)
                    result = 0;

                qtd = Convert.ToDouble(result);

                return qtd;
            }

            public int NUM_PEDIDOS_DIA
            {
                get
                {
                    return RETORNAR_PEDIDOS_DIA();
                }
            }

            public int NUM_SKUS_DIA
            {
                get
                {
                    return RETORNAR_SKUS_DIA();
                }
            }

            private int RETORNAR_SKUS_DIA()
            {
                int qtd = 0;

                StringBuilder sqlQuery = new StringBuilder();
                sqlQuery.AppendLine("SELECT COUNT(ITEM_PEDIDO.COD_PEDIDO) FROM PEDIDO JOIN ITEM_PEDIDO ON PEDIDO.COD_PEDIDO = ITEM_PEDIDO.COD_PEDIDO ");
                sqlQuery.AppendFormat(" WHERE DATE(PEDIDO.DAT_PEDIDO) = DATE('{0}')", DateTime.Now.ToString("yyyy-MM-dd"));

                var result = CSDataAccess.Instance.ExecuteScalar(sqlQuery.ToString());

                qtd = Convert.ToInt32(result);

                return qtd;
            }

            private int RETORNAR_PEDIDOS_DIA()
            {
                int qtd = 0;

                StringBuilder sqlQuery = new StringBuilder();
                sqlQuery.AppendFormat("SELECT COUNT(*) FROM PEDIDO WHERE DATE(DAT_PEDIDO) = DATE('{0}')", DateTime.Now.ToString("yyyy-MM-dd"));

                var result = CSDataAccess.Instance.ExecuteScalar(sqlQuery.ToString());

                qtd = Convert.ToInt32(result);

                return qtd;
            }

            #endregion

            #region [ Funções ]

            public CSEmpregado()
            {
            }

            /// <summary>
            /// Salva o valor do saldo para desconto
            /// </summary>
            public void Flush()
            {
                try
                {
                    /* Atualiza Saldo do Estoque */

                    string sqlQueryUpdate =
                        "UPDATE EMPREGADO " +
                        "   SET VAL_SALDO_DESCONTO = ? " +
                        " WHERE COD_EMPREGADO = ? ";

                    // Criar os parametros de salvamento
                    SQLiteParameter pVAL_SALDO_DESCONTO = new SQLiteParameter("@VAL_SALDO_DESCONTO", this.VAL_SALDO_DESCONTO);
                    SQLiteParameter pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", this.COD_EMPREGADO);

                    pVAL_SALDO_DESCONTO.DbType = DbType.Decimal;
                    pCOD_EMPREGADO.DbType = DbType.Int32;

                    // Executa a query salvando os dados
                    CSDataAccess.Instance.ExecuteNonQuery(sqlQueryUpdate, pVAL_SALDO_DESCONTO, pCOD_EMPREGADO);

                }
                catch (Exception ex)
                {
                    CSGlobal.ShowMessage(ex.ToString());
                    throw new Exception("Erro no flush de Empregado", ex);
                }
            }

            /// <summary>
            /// descarta alteracao no valor do saldo para desconto
            /// </summary>
            public void DiscardChanges()
            {
                try
                {
                    /* Atualiza Saldo do Estoque */

                    string sqlQuery =
                        "SELECT VAL_SALDO_DESCONTO " +
                        "  FROM EMPREGADO " +
                        " WHERE COD_EMPREGADO = ? ";

                    // Criar os parametros de salvamento
                    SQLiteParameter pCOD_EMPREGADO = new SQLiteParameter("@COD_EMPREGADO", this.COD_EMPREGADO);
                    pCOD_EMPREGADO.DbType = DbType.Int32;

                    // Busca todos os contatos do PDV
                    using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery, pCOD_EMPREGADO))
                    {
                        while (sqlReader.Read())
                        {
                            // Preenche a instancia da classe de pedido do pdv
                            this.VAL_SALDO_DESCONTO = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : Convert.ToDecimal(sqlReader.GetValue(0));
                        }

                        // Fecha o reader
                        sqlReader.Close();
                        sqlReader.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    CSGlobal.ShowMessage(ex.ToString());
                    throw new Exception("Erro no DiscardChanges de Empregado", ex);
                }
            }
            #endregion

            #region [ SubClasses ]


            public class CSDocumentosReceberEmpregados : CollectionBase
            {
                #region[Propriedades]
                /// <summary>
                /// Retorna coleção dos Documentos à Receber dos Empregados
                /// </summary>
                public CSDocumentosReceberEmpregados Items
                {
                    get
                    {
                        return this;
                    }
                }

                public CSDocumentosReceberEmpregados.CSDocumentoReceberEmpregado this[int Index]
                {
                    get
                    {
                        return (CSDocumentosReceberEmpregados.CSDocumentoReceberEmpregado)this.InnerList[Index];
                    }
                }


                #endregion

                public CSDocumentosReceberEmpregados(int COD_EMPREGADO)
                {
                    CSDocumentoReceberEmpregado docrecVencidos = new CSDocumentoReceberEmpregado();
                    CSDocumentoReceberEmpregado docrecVencer = new CSDocumentoReceberEmpregado();
                    CSDocumentoReceberEmpregado listagemDocReceber = new CSDocumentoReceberEmpregado();
                    CSDocumentoReceberEmpregado docrecTotais = new CSDocumentoReceberEmpregado();

                    CSEmpregados.CSEmpregado empregadoAtual = CSEmpregados.Current;

                    /* ------------------------------------------------------
                             * Bloco de retorno de valor, quantidade - Inicial
                     -----------------------------------------------------*/
                    // Resumo
                    // Retorno de documentos vencidos.
                    string sqlQueryVencido =
                        "SELECT SUM(VLR_DOCUMENTO_RECEBER + VLR_MULTA + VLR_JUROS + VLR_ENCARGO - VLR_DESCONTO - VLR_RECEBIDO) RESULTADO " +
                        "      ,COUNT(COD_DOCUMENTO_RECEBER) QTD " +
                        "  FROM DOCUMENTO_RECEBER " +
                        " WHERE (VLR_RECEBIDO + VLR_DESCONTO) < (VLR_DOCUMENTO_RECEBER + VLR_MULTA + VLR_JUROS + VLR_ENCARGO) " +
                        "   AND DATE(DAT_VENCIMENTO) < date('now') " +
                        "   AND COD_EMPREGADO = ? " +
                        "   AND COD_REVENDA = ? ";

                    SQLiteParameter pCOD_EMPREGADO_VENCIDO = new SQLiteParameter("@COD_EMPREGADO", COD_EMPREGADO);
                    SQLiteParameter pCOD_REVENDA = new SQLiteParameter("@COD_REVENDA", CSGlobal.COD_REVENDA);

                    // Busca todos os documentos a vencer do Empregado
                    using (SQLiteDataReader sqlReaderVencido = CSDataAccess.Instance.ExecuteReader(sqlQueryVencido, CommandBehavior.SingleResult, pCOD_EMPREGADO_VENCIDO, pCOD_REVENDA))
                    {
                        while (sqlReaderVencido.Read())
                        {
                            docrecVencidos.TIPO_DOCUMENTO = CSDocumentoReceberEmpregado.TipoDocumento.VENCIDO;
                            docrecVencidos.VALOR = sqlReaderVencido.GetValue(0) == System.DBNull.Value ? 0 : sqlReaderVencido.GetDecimal(0);
                            docrecVencidos.QUANTIDADE = sqlReaderVencido.GetValue(1) == System.DBNull.Value ? 0 : sqlReaderVencido.GetInt32(1);

                            // Adciona o valor do documento vencido
                            base.InnerList.Add(docrecVencidos);
                        }
                        // Fecha o reader
                        sqlReaderVencido.Close();
                        sqlReaderVencido.Dispose();
                    }

                    // Retorno de documentos à vencer.
                    string sqlQueryVencer =
                        "SELECT SUM(VLR_DOCUMENTO_RECEBER + VLR_MULTA + VLR_JUROS + VLR_ENCARGO - VLR_DESCONTO - VLR_RECEBIDO) RESULTADO " +
                        "      ,COUNT(COD_DOCUMENTO_RECEBER) QTD " +
                        "  FROM DOCUMENTO_RECEBER " +
                        " WHERE (VLR_RECEBIDO + VLR_DESCONTO) < (VLR_DOCUMENTO_RECEBER + VLR_MULTA + VLR_JUROS + VLR_ENCARGO) " +
                        "   AND DATE(DAT_VENCIMENTO) >= date('now') " +
                        "   AND COD_EMPREGADO = ? " +
                        "   AND COD_REVENDA = ? ";

                    SQLiteParameter pCOD_EMPREGADO_VENCER = new SQLiteParameter("@COD_EMPREGADO", COD_EMPREGADO);
                    pCOD_REVENDA = new SQLiteParameter("@COD_REVENDA", CSGlobal.COD_REVENDA);

                    // Busca todos os documentos a vencer do Empregado
                    using (SQLiteDataReader sqlReaderVencer = CSDataAccess.Instance.ExecuteReader(sqlQueryVencer, pCOD_EMPREGADO_VENCER, pCOD_REVENDA))
                    {
                        while (sqlReaderVencer.Read())
                        {
                            docrecVencer.TIPO_DOCUMENTO = CSDocumentoReceberEmpregado.TipoDocumento.A_VENCER;
                            docrecVencer.VALOR = sqlReaderVencer.GetValue(0) == System.DBNull.Value ? 0 : sqlReaderVencer.GetDecimal(0);
                            docrecVencer.QUANTIDADE = sqlReaderVencer.GetValue(1) == System.DBNull.Value ? 0 : sqlReaderVencer.GetInt32(1);

                            // Adciona o documento a vencer
                            base.InnerList.Add(docrecVencer);
                        }

                        // Fecha o reader
                        sqlReaderVencer.Close();
                        sqlReaderVencer.Dispose();
                    }

                    /* ------------------------------------------------------
                        * Bloco de retorno de valor, quantidade - Final
                    -----------------------------------------------------*/

                    // Valores Total
                    docrecTotais.TIPO_DOCUMENTO = CSDocumentoReceberEmpregado.TipoDocumento.TOTAL;
                    docrecTotais.TOTAL_QUANTIDADE = docrecVencidos.QUANTIDADE + docrecVencer.QUANTIDADE;
                    docrecTotais.TOTAL_VALOR = docrecVencidos.VALOR + docrecVencer.VALOR;

                    // Adiciona na colecao
                    base.InnerList.Add(docrecTotais);
                }

                public class CSDocumentoReceberEmpregado
                {
                    #region [ Enuns ]

                    public enum TipoDocumento
                    {
                        VENCIDO,
                        A_VENCER,
                        TOTAL,
                        LIMITE_CREDITO,
                        SALDO,
                        CREDITO
                    };

                    #endregion

                    #region [Variaveis]

                    private int m_COD_DOCUMENTO_RECEBER;
                    private int m_COD_CLASSE_DOCUMENTO_RECEBER;
                    private int m_COD_STATUS_DOCUMENTO_RECEBER;
                    private int m_COD_DOCUMENTO_ORIGEM;
                    private string m_NUM_CGC_CPF_PORTADOR_DOCUMENTO;
                    private decimal m_PRC_MULTA;
                    private decimal m_PRC_JUROS;
                    private decimal m_PRC_DESCONTO;
                    private decimal m_VLR_MULTA;
                    private decimal m_VLR_JUROS;
                    private decimal m_VLR_ENCARGO;
                    private decimal m_VLR_DESCONTO;
                    private decimal m_VLR_RECEBIDO;
                    private decimal m_VLR_DOCUMENTO_RECEBER;
                    private DateTime m_DAT_CADASTRO;
                    private DateTime m_DAT_EMISSAO;
                    private DateTime m_DAT_VENCIMENTO;
                    private DateTime m_DAT_BAIXA;
                    private string m_DSC_OBSERVACAO;
                    private CSDocumentoReceberEmpregado.TipoDocumento m_TIPO_DOCUMENTO;
                    private int m_QUANTIDADE;
                    private decimal m_VALOR;
                    private decimal m_VALOR_ABERTO;
                    private decimal m_TOTAL_QUANTIDADE;
                    private decimal m_TOTAL_VALOR;
                    private decimal m_VLR_LIMITE_CREDITO_PDV;
                    private decimal m_SALDO_CREDITO;

                    #endregion

                    #region[Propriedades]

                    /// <summary>
                    /// Guarda o tipo do documento a receber. Usada para controle na hora de mostrar na tela
                    /// </summary>
                    public TipoDocumento TIPO_DOCUMENTO
                    {
                        get
                        {
                            return m_TIPO_DOCUMENTO;
                        }
                        set
                        {
                            m_TIPO_DOCUMENTO = value;
                        }
                    }

                    /// <summary>
                    /// Guarda o codigo do documento a receber
                    /// </summary>
                    public int COD_DOCUMENTO_RECEBER
                    {
                        get
                        {
                            return m_COD_DOCUMENTO_RECEBER;
                        }
                        set
                        {
                            m_COD_DOCUMENTO_RECEBER = value;
                        }
                    }

                    /// <summary>
                    /// Guarda o codigo da classe do documento a receber
                    /// </summary>
                    public int COD_CLASSE_DOCUMENTO_RECEBER
                    {
                        get
                        {
                            return m_COD_CLASSE_DOCUMENTO_RECEBER;
                        }
                        set
                        {
                            m_COD_CLASSE_DOCUMENTO_RECEBER = value;
                        }
                    }

                    /// <summary>
                    /// Guarda o codigo do status do documento a receber
                    /// </summary>
                    public int COD_STATUS_DOCUMENTO_RECEBER
                    {
                        get
                        {
                            return m_COD_STATUS_DOCUMENTO_RECEBER;
                        }
                        set
                        {
                            m_COD_STATUS_DOCUMENTO_RECEBER = value;
                        }
                    }

                    /// <summary>
                    /// Guarda o codigo do documento de origem
                    /// </summary>
                    public int COD_DOCUMENTO_ORIGEM
                    {
                        get
                        {
                            return m_COD_DOCUMENTO_ORIGEM;
                        }
                        set
                        {
                            m_COD_DOCUMENTO_ORIGEM = value;
                        }
                    }

                    /// <summary>
                    /// Guarda o numero do cgc, cpf do portador do documento
                    /// </summary>				
                    public string NUM_CGC_CPF_PORTADOR_DOCUMENTO
                    {
                        get
                        {
                            return m_NUM_CGC_CPF_PORTADOR_DOCUMENTO;
                        }
                        set
                        {
                            m_NUM_CGC_CPF_PORTADOR_DOCUMENTO = value;
                        }
                    }

                    /// <summary>
                    /// Guarda a porcentagem da multa
                    /// </summary>
                    public decimal PRC_MULTA
                    {
                        get
                        {
                            return m_PRC_MULTA;
                        }
                        set
                        {
                            m_PRC_MULTA = value;
                        }
                    }

                    /// <summary>
                    /// Guarda a porcentagem dos juros
                    /// </summary>
                    public decimal PRC_JUROS
                    {
                        get
                        {
                            return m_PRC_JUROS;
                        }
                        set
                        {
                            m_PRC_JUROS = value;
                        }
                    }

                    /// <summary>
                    /// Guarda a porcentagem de desconto
                    /// </summary>
                    public decimal PRC_DESCONTO
                    {
                        get
                        {
                            return m_PRC_DESCONTO;
                        }
                        set
                        {
                            m_PRC_DESCONTO = value;
                        }
                    }

                    /// <summary>
                    /// Guarda o valor da multa
                    /// </summary>
                    public decimal VLR_MULTA
                    {
                        get
                        {
                            return m_VLR_MULTA;
                        }
                        set
                        {
                            m_VLR_MULTA = value;
                        }
                    }

                    /// <summary>
                    /// Guarda o valor do juros
                    /// </summary>
                    public decimal VLR_JUROS
                    {
                        get
                        {
                            return m_VLR_JUROS;
                        }
                        set
                        {
                            m_VLR_JUROS = value;
                        }
                    }
                    /// <summary>
                    /// Guarda o valor de encargos
                    /// </summary>
                    public decimal VLR_ENCARGO
                    {
                        get
                        {
                            return m_VLR_ENCARGO;
                        }
                        set
                        {
                            m_VLR_ENCARGO = value;
                        }
                    }
                    /// <summary>
                    /// Guarda o valor do desconto
                    /// </summary>
                    public decimal VLR_DESCONTO
                    {
                        get
                        {
                            return m_VLR_DESCONTO;
                        }
                        set
                        {
                            m_VLR_DESCONTO = value;
                        }
                    }
                    /// <summary>
                    /// Guarda o valor do recebido
                    /// </summary>
                    public decimal VLR_RECEBIDO
                    {
                        get
                        {
                            return m_VLR_RECEBIDO;
                        }
                        set
                        {
                            m_VLR_RECEBIDO = value;
                        }
                    }
                    /// <summary>
                    /// Guarda o valor de documentos a receber
                    /// </summary>
                    public decimal VLR_DOCUMENTO_RECEBER
                    {
                        get
                        {
                            return m_VLR_DOCUMENTO_RECEBER;
                        }
                        set
                        {
                            m_VLR_DOCUMENTO_RECEBER = value;
                        }
                    }
                    /// <summary>
                    /// Guarda a data de cadastro
                    /// </summary>
                    public DateTime DAT_CADASTRO
                    {
                        get
                        {
                            return m_DAT_CADASTRO;
                        }
                        set
                        {
                            m_DAT_CADASTRO = value;
                        }
                    }
                    /// <summary>
                    /// Guarda a data de emissao
                    /// </summary>
                    public DateTime DAT_EMISSAO
                    {
                        get
                        {
                            return m_DAT_EMISSAO;
                        }
                        set
                        {
                            m_DAT_EMISSAO = value;
                        }
                    }
                    /// <summary>
                    /// Guarda a data de vencimento
                    /// </summary>
                    public DateTime DAT_VENCIMENTO
                    {
                        get
                        {
                            return m_DAT_VENCIMENTO;
                        }
                        set
                        {
                            m_DAT_VENCIMENTO = value;
                        }
                    }
                    /// <summary>
                    /// Guarda a data da baixa
                    /// </summary>
                    public DateTime DAT_BAIXA
                    {
                        get
                        {
                            return m_DAT_BAIXA;
                        }
                        set
                        {
                            m_DAT_BAIXA = value;
                        }
                    }
                    /// <summary>
                    /// Guarda a descricao de observaçao
                    /// </summary>
                    public string DSC_OBSERVACAO
                    {
                        get
                        {
                            return m_DSC_OBSERVACAO;
                        }
                        set
                        {
                            m_DSC_OBSERVACAO = value;
                        }
                    }
                    /// <summary>
                    /// Guarda a quantidade
                    /// </summary>
                    public int QUANTIDADE
                    {
                        get
                        {
                            return m_QUANTIDADE;
                        }
                        set
                        {
                            m_QUANTIDADE = value;
                        }
                    }
                    /// <summary>
                    /// Guarda o valor
                    /// </summary>
                    public decimal VALOR
                    {
                        get
                        {
                            return m_VALOR;
                        }
                        set
                        {
                            m_VALOR = value;
                        }
                    }
                    /// <summary>
                    /// Guarda o valor aberto
                    /// </summary>
                    public decimal VALOR_ABERTO
                    {
                        get
                        {
                            return m_VALOR_ABERTO;
                        }
                        set
                        {
                            m_VALOR_ABERTO = value;
                        }
                    }
                    /// <summary>
                    /// Guarda o total de valor
                    /// </summary>
                    public decimal TOTAL_VALOR
                    {
                        get
                        {
                            return m_TOTAL_VALOR;
                        }
                        set
                        {
                            m_TOTAL_VALOR = value;
                        }
                    }
                    /// <summary>
                    /// Guarda o total de quantidade
                    /// </summary>
                    public decimal TOTAL_QUANTIDADE
                    {
                        get
                        {
                            return m_TOTAL_QUANTIDADE;
                        }
                        set
                        {
                            m_TOTAL_QUANTIDADE = value;
                        }
                    }
                    /// <summary>
                    /// Guarda o valor do limite de credito do pdv
                    /// </summary>
                    public decimal VLR_LIMITE_CREDITO_PDV
                    {
                        get
                        {
                            return m_VLR_LIMITE_CREDITO_PDV;
                        }
                        set
                        {
                            m_VLR_LIMITE_CREDITO_PDV = value;
                        }
                    }
                    /// <summary>
                    /// Guarda o valor do saldo de credito
                    /// </summary>
                    public decimal SALDO_CREDITO
                    {
                        get
                        {
                            return m_SALDO_CREDITO;
                        }
                        set
                        {
                            m_SALDO_CREDITO = value;
                        }
                    }
                    #endregion
                }
            }
            #endregion

            public int TempoAlmocoRestante()
            {
                var tempoRestante = Convert.ToInt32(CSEmpregados.Current.NUM_MINUTOS_INTERVALO_ALMOCO - IntervaloAlmocoCompleto());

                if (tempoRestante == 0)
                    tempoRestante = 1;

                return tempoRestante;
            }

            /// <summary>
            /// Retorna quantos minutos se passaram do intervalo de almoço do vendedor.
            /// </summary>
            /// <returns></returns>
            public double IntervaloAlmocoCompleto()
            {
                var inicioAlmoco = CSEmpregados.Current.DAT_INICIO_ALMOCO.Value.TimeOfDay;
                var horaAtual = Convert.ToDateTime(DateTime.Now.ToString("HH:mm")).TimeOfDay;

                var intervalo = horaAtual.TotalSeconds - inicioAlmoco.TotalSeconds;

                return intervalo / 60;
            }

            /// <summary>
            /// Retorna se o intervalo de almoço do vendedor foi realizado.
            /// </summary>
            /// <returns></returns>
            public bool IntervaloAlmocoEncerrado()
            {
                if (this.NUM_MINUTOS_INTERVALO_ALMOCO > 0)
                {
                    if (IntervaloAlmocoCompleto() >= CSEmpregados.Current.NUM_MINUTOS_INTERVALO_ALMOCO)
                        return true;
                    else
                        return false;
                }
                else
                    return true;
            }

            public void FinalizarAlmoco()
            {
                try
                {
                    if (this.NUM_MINUTOS_INTERVALO_ALMOCO > 0)
                        this.DAT_FIM_ALMOCO = this.DAT_INICIO_ALMOCO.Value.AddMinutes(this.NUM_MINUTOS_INTERVALO_ALMOCO);
                    else
                        this.DAT_FIM_ALMOCO = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

                    StringBuilder sql = new StringBuilder();
                    sql.AppendFormat("UPDATE EMPREGADO_EXPEDIENTE SET DAT_FIM_EXPEDIENTE = DATETIME('{0}') WHERE IND_TIPO_EXPEDIENTE = 'A'", this.DAT_FIM_ALMOCO.Value.ToString("yyyy-MM-dd HH:mm"));
                    sql.AppendFormat(" AND DATE(DAT_INICIO_EXPEDIENTE) = DATE('{0}')", DateTime.Now.ToString("yyyy-MM-dd"));
                    sql.AppendFormat(" AND COD_EMPREGADO = {0}", this.COD_EMPREGADO);

                    CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());

                    if (CSEmpresa.Current.IND_UTILIZA_FLEXX_GPS == "S" &&
                        CSEmpregados.Current.IND_VALIDAR_EXPEDIENTE)
                        GerarArquivo(TipoGeracaoArquivo.FIM_ALMOCO, this.DAT_FIM_ALMOCO.Value);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            public void IniciarAlmoco()
            {
                try
                {
                    this.DAT_INICIO_ALMOCO = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

                    StringBuilder sql = new StringBuilder();
                    sql.Append("INSERT INTO EMPREGADO_EXPEDIENTE (COD_EMPREGADO,DAT_INICIO_EXPEDIENTE,IND_TIPO_EXPEDIENTE) VALUES (");
                    sql.AppendFormat(" {0}", this.COD_EMPREGADO);
                    sql.AppendFormat(",DATETIME('{0}')", this.DAT_INICIO_ALMOCO.Value.ToString("yyyy-MM-dd HH:mm"));
                    sql.AppendFormat(",'A'");
                    sql.Append(")");

                    CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());

                    if (CSEmpresa.Current.IND_UTILIZA_FLEXX_GPS == "S" &&
                        CSEmpregados.Current.IND_VALIDAR_EXPEDIENTE)
                        GerarArquivo(TipoGeracaoArquivo.INICIO_ALMOCO, this.DAT_INICIO_ALMOCO.Value);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            public enum TipoGeracaoArquivo
            {
                INICIO_ALMOCO,
                FIM_ALMOCO,
                INICIO_EXPEDIENTE,
                FIM_EXPEDIENTE
            }

            public void GerarArquivo(TipoGeracaoArquivo tipo, DateTime dataInformacao)
            {
                string arquivo;
                string pathArquivo = "";
                string latitude = CSGlobal.GetLatitudeFlexxGPS();
                string longitude = CSGlobal.GetLongitudeFlexxGPS();
                string descricaoTipoExpediente = string.Empty;

                pathArquivo = System.IO.Path.Combine("/sdcard/FLAGPS_BD/ENVIAR/", "EX" + Guid.NewGuid());

                // Codigo da Revenda
                arquivo = CSEmpresa.Current.CODIGO_REVENDA.ToString().Trim();

                // Codigo do vendedor
                arquivo += "|" + CSEmpregados.Current.COD_EMPREGADO;

                // Codigo do cliente
                arquivo += "|";

                // (Inicial) Latitude & Longitude
                arquivo += string.Format("|{0}|{1}", latitude, longitude);

                // (Final) Latitude & Longitude
                arquivo += string.Format("|{0}|{1}", latitude, longitude);

                // Data
                arquivo += "|" + dataInformacao.ToString("dd/MM/yyyy");

                // Horas inicial 
                arquivo += "|" + dataInformacao.ToString("HH:mm:ss");

                switch (tipo)
                {
                    case TipoGeracaoArquivo.FIM_ALMOCO:
                        descricaoTipoExpediente = "Fim de Almoço";
                        break;
                    case TipoGeracaoArquivo.FIM_EXPEDIENTE:
                        descricaoTipoExpediente = "Fim de Jornada";
                        break;
                    case TipoGeracaoArquivo.INICIO_ALMOCO:
                        descricaoTipoExpediente = "Início de Almoço";
                        break;
                    case TipoGeracaoArquivo.INICIO_EXPEDIENTE:
                        descricaoTipoExpediente = "Início de Jornada";
                        break;
                }

                arquivo += "|" + descricaoTipoExpediente;
                arquivo += "|||||||||";

                if (System.IO.File.Exists(pathArquivo))
                    System.IO.File.Delete(pathArquivo);

                if (System.IO.File.Exists(pathArquivo + ".txt"))
                    System.IO.File.Delete(pathArquivo + ".txt");

                System.IO.TextWriter fileOut = System.IO.File.CreateText(pathArquivo);

                fileOut.WriteLine(arquivo.ToString());

                fileOut.Close();

                System.IO.File.Move(pathArquivo, pathArquivo + ".txt");
            }

            public int TempoRestanteFimExpediente()
            {
                if (CSEmpregados.Current.NUM_MINUTOS_TOTAL_EXPEDIENTE > 0)
                {
                    return CSEmpregados.Current.TempoExpedienteRestante();
                }
                else
                {
                    if (!this.DAT_HORA_INICIO_EXPEDIENTE.HasValue)
                        return -1;

                    TimeSpan horarioFim = Convert.ToDateTime(this.DAT_HORA_FIM_EXPEDIENTE.Value.ToString("HH:mm")).TimeOfDay;
                    TimeSpan horarioAtual = Convert.ToDateTime(DateTime.Now.ToString("HH:mm")).TimeOfDay;

                    return Convert.ToInt32(horarioFim.TotalMinutes - horarioAtual.TotalMinutes);
                }
            }

            public bool VendedorDentroExpediente(ref string alerta)
            {
                if (this.IND_VALIDAR_EXPEDIENTE == false ||
                    (!this.DAT_HORA_INICIO_EXPEDIENTE.HasValue &&
                    this.NUM_MINUTOS_TOTAL_EXPEDIENTE == 0))
                    return true;
                else
                {
                    if (this.NUM_MINUTOS_TOTAL_EXPEDIENTE == 0)
                    {
                        TimeSpan horarioInicio = Convert.ToDateTime(this.DAT_HORA_INICIO_EXPEDIENTE.Value.ToString("HH:mm")).TimeOfDay;
                        TimeSpan horarioFim = Convert.ToDateTime(this.DAT_HORA_FIM_EXPEDIENTE.Value.ToString("HH:mm")).TimeOfDay;
                        TimeSpan horarioAtual = Convert.ToDateTime(DateTime.Now.ToString("HH:mm")).TimeOfDay;

                        if (horarioAtual < horarioInicio)
                        {
                            alerta = "Você não se encontra no início do seu expediente.";
                            return false;
                        }

                        if (horarioFim <= horarioAtual)
                        {
                            alerta = "Seu expediente já foi encerrado.";
                            return false;
                        }
                    }
                    else
                    {
                        if (!this.DAT_INICIO_TRABALHO.HasValue)
                        {
                            alerta = "Você não iniciou o seu expediente.";
                            return false;
                        }
                        else if (this.DAT_FIM_TRABALHO.HasValue)
                        {
                            alerta = "Seu expediente já foi encerrado.";
                            return false;
                        }
                    }

                    return true;
                }
            }

            public bool HorarioExpedienteEncerrado()
            {
                return MinutosExpedienteCompleto() >= MinutosExpedienteTotal();
            }

            /// <summary>
            /// Retorna o expediente total do empregado, a soma do expediente e horário de almoço
            /// </summary>
            /// <returns></returns>
            public int MinutosExpedienteTotal()
            {
                var minutosExpediente = this.NUM_MINUTOS_TOTAL_EXPEDIENTE;
                var minutosAlmoco = this.NUM_MINUTOS_INTERVALO_ALMOCO;

                return minutosExpediente + minutosAlmoco;
            }

            /// <summary>
            /// Retorna quanto tempo do expediente já foram completados
            /// </summary>
            /// <returns></returns>
            public double MinutosExpedienteCompleto()
            {
                var inicioExpediente = this.DAT_INICIO_TRABALHO.Value.TimeOfDay.TotalMinutes;
                var atual = Convert.ToDateTime(DateTime.Now.ToString("HH:mm")).TimeOfDay.TotalMinutes;

                return atual - inicioExpediente;
            }

            public void FinalizarExpediente()
            {
                try
                {
                    this.DAT_FIM_TRABALHO = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

                    StringBuilder sql = new StringBuilder();
                    sql.AppendFormat("UPDATE EMPREGADO_EXPEDIENTE SET DAT_FIM_EXPEDIENTE = DATETIME('{0}') WHERE IND_TIPO_EXPEDIENTE = 'T'", this.DAT_FIM_TRABALHO.Value.ToString("yyyy-MM-dd HH:mm"));
                    sql.AppendFormat(" AND DATE(DAT_INICIO_EXPEDIENTE) = DATE('{0}')", DateTime.Now.ToString("yyyy-MM-dd"));
                    sql.AppendFormat(" AND COD_EMPREGADO = {0}", this.COD_EMPREGADO);

                    CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());

                    if (CSEmpresa.Current.IND_UTILIZA_FLEXX_GPS == "S" &&
                        CSEmpregados.Current.IND_VALIDAR_EXPEDIENTE &&
                        CSEmpregados.Current.NUM_MINUTOS_TOTAL_EXPEDIENTE > 0)
                        GerarArquivo(TipoGeracaoArquivo.FIM_EXPEDIENTE, this.DAT_FIM_TRABALHO.Value);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            public int TempoExpedienteRestante()
            {
                var tempoRestante = Convert.ToInt32(MinutosExpedienteTotal() - MinutosExpedienteCompleto());

                if (tempoRestante == 0)
                    tempoRestante = 1;

                return tempoRestante;
            }

            public void IniciarExpediente()
            {
                try
                {
                    this.DAT_INICIO_TRABALHO = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

                    StringBuilder sql = new StringBuilder();
                    sql.Append("INSERT INTO EMPREGADO_EXPEDIENTE (COD_EMPREGADO,DAT_INICIO_EXPEDIENTE,IND_TIPO_EXPEDIENTE) VALUES (");
                    sql.AppendFormat(" {0}", this.COD_EMPREGADO);
                    sql.AppendFormat(",DATETIME('{0}')", this.DAT_INICIO_TRABALHO.Value.ToString("yyyy-MM-dd HH:mm"));
                    sql.AppendFormat(",'T'");
                    sql.Append(")");

                    CSDataAccess.Instance.ExecuteNonQuery(sql.ToString());

                    if (CSEmpresa.Current.IND_UTILIZA_FLEXX_GPS == "S" &&
                        CSEmpregados.Current.IND_VALIDAR_EXPEDIENTE)
                        GerarArquivo(TipoGeracaoArquivo.INICIO_EXPEDIENTE, this.DAT_INICIO_TRABALHO.Value);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            public void RefreshVisitas()
            {
                m_VISITAS_EMPREGADO = null;
            }
        }
    }
}