using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AvanteSales.Pro.Controles;
using AvanteSales.Pro.Dialogs;
using AvanteSales.Pro.Formatters;
using Java.Lang;
using Java.Net;
using Master.CompactFramework.Sync;
using Master.CompactFramework.Sync.SQLLiteProvider;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "Avante Sales", ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AvanteSalesTheme", WindowSoftInputMode = SoftInput.StateHidden)]
    public class Sincronizacao : AppCompatActivity, TextView.IOnEditorActionListener
    {
        Android.Support.V7.Widget.Toolbar tbToolbar;
        TextView lblVersao;
        EditText txtImei;
        static EditText txtServidor;
        Button btnSair;
        static Button btnCargaParcial;
        Button btnEditarDados;
        static Button btnCargaCompleta;
        Button btnApagarBanco;
        Button btnCargaImagens;
        Button btnLogin;
        static ImageView imgIconeServidor;
        static ImageView imgIconeEmpresa;
        static TextInputLayout tilServidor;
        static TextInputLayout tilEmpresa;
        static TextInputLayout tilVendedor;
        static Spinner spnEmpresa;
        static EditText txtVendedor;
        static bool TestandoConexao;
        public static string Vendedor;
        private bool ModificandoServidor;
        private static Activity CurrentActivity;
        public static bool cargaParcial;
        private static string Imei;
        private static ISyncProvider syncProvider;
        static ProgressDialog progressBar;
        private const int frmImportacaoArquivo = 1;
        private const int frmSenhaArquivo = 2;
        string IpImportado;
        private static string VersaoAvante;
        private static bool executandoCargaTotal = false;
        private delegate void delegateUpdateStatus(object sender, EventArgs e);
        public delegate string delegateGetString();
        //private static bool bolOcorreuErroAtualizarData = false;
        //private static bool SomenteDescarga;
        //private static string m_Imei;
        //private static string versaoCompatibilidadeAvante;


        static ProgressDialog progressTesteConexao;
        static ProgressDialog progressBuscaEmpresa;
        private static string DatabaseFilePath
        {
            get
            {
                return System.IO.Path.Combine(CSGlobal.GetCurrentDirectoryDB(), CSConfiguracao.GetConfig("dbFile") + GetEmpresa() + ".sdf");
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.sincronizacao);

            FindViewsById();

            Eventos();
            TestandoConexao = false;
            SetSupportActionBar(tbToolbar);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(false);

            Inicializacao();
        }

        private void Eventos()
        {
            btnSair.Click += btnSair_Click;
            txtServidor.SetOnEditorActionListener(this);
            txtServidor.TextChanged += txtServidor_TextChanged;
            txtServidor.FocusChange += TxtServidor_FocusChange;
            btnEditarDados.Click += btnEditarDados_Click;
            btnCargaCompleta.Click += btnCargaCompleta_Click;
            btnCargaParcial.Click += btnCargaParcial_Click;
            btnApagarBanco.Click += btnApagarBanco_Click;
            btnCargaImagens.Click += btnCargaImagens_Click;
            btnLogin.Click += btnLogin_Click;
        }

        private void TxtServidor_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (!e.HasFocus &&
                !string.IsNullOrEmpty(txtServidor.Text))
            {
                if (!TestandoConexao)
                {
                    progressTesteConexao = new ProgressDialogCustomizado(this, LayoutInflater).Customizar();
                    progressTesteConexao.Show();
                    TestandoConexao = true;
                    TesteConexao(true);
                }
            }
        }

        void btnLogin_Click(object sender, EventArgs e)
        {
            AbrirLogin();
        }

        private void AbrirLogin()
        {
            Intent i = new Intent();
            i.SetClass(this, (typeof(Login)));
            StartActivity(i);
            this.Finish();
        }

        void btnCargaImagens_Click(object sender, EventArgs e)
        {
            try
            {
                if (CSDataAccess.DataBaseExists(CSGlobal.COD_REVENDA))
                {
                    CSDataAccess.Instance.AbreConexao();

                    if (CSEmpresa.ColunaExiste("EMPREGADO", "IND_PERMITIR_CARGA_IMAGEM"))
                    {
                        if (PermitirCargaImagemVendedor())
                        {
                            MessageBox.Alert(this, "Deseja realizar a carga completa das imagens ou parcial (apenas as não existentes no celular)?",
                                "Completa", (_sender, _e) =>
                                {
                                    MessageBox.Alert(this, "A carga completa irá apagar todas as imagens e realizar o download completo novamente. Deseja continuar?", "Continuar",
                                        (_sender2, _e2) => { IniciarCargaImagem(true); }, "Cancelar", null, true);
                                },
                                "Parcial", (_sender, _e) =>
                                {
                                    IniciarCargaImagem(false);
                                }, "Cancelar", null, true);
                        }
                        else
                            MessageBox.AlertErro(this, "Carga de imagem bloqueada: vendedor sem permissão.");
                    }
                    else
                        MessageBox.AlertErro(this, "Carga de imagem bloqueada: banco de dados sem parâmetro criado.");
                }
                else
                    MessageBox.AlertErro(this, "Para realizar carga de imagens é necessário uma carga completa.");
            }
            catch (System.Exception ex)
            {
                MessageBox.AlertErro(this, ex.Message);
            }
        }

        private void IniciarCargaImagem(bool todasImagens)
        {
            UpdateProvider();

            Sincronizacao.progressBar = new ProgressDialog(this);
            Sincronizacao.progressBar.SetProgressStyle(ProgressDialogStyle.Horizontal);
            Sincronizacao.progressBar.SetTitle("Carga Imagens");
            Sincronizacao.progressBar.SetMessage("Realizando download...");
            Sincronizacao.progressBar.SetCancelable(false);
            Sincronizacao.progressBar.Show();

            new ThreadCargaImagem(todasImagens).Execute();
        }

        private class ThreadCargaImagem : AsyncTask
        {
            DateTime inicioCarga;
            bool CargaTodasImagens;

            public ThreadCargaImagem(bool todasImagens)
            {
                CargaTodasImagens = todasImagens;
            }

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                CargaImagem();

                return 0;
            }

            private void CargaImagem()
            {
                try
                {
                    inicioCarga = DateTime.Now;
                    CSGlobal.COD_REVENDA = GetEmpresa();
                    Java.IO.File path = Android.OS.Environment.ExternalStorageDirectory;
                    StatFs stat = new StatFs(path.Path);
                    long blockSize = stat.BlockSizeLong;
                    long availableBlocks = stat.AvailableBlocksLong;
                    var espacoDisponivel = (availableBlocks * blockSize) / 1024;

                    var diretorioAvante = CSGlobal.GetCurrentDirectory();
                    int indexDiretorioAvante = diretorioAvante.ToLower().IndexOf("avante");
                    var diretorioImagens = System.IO.Path.Combine(diretorioAvante.Substring(0, indexDiretorioAvante), "ImagensProdutosAvante");

                    if (CargaTodasImagens)
                        syncProvider.CargaImagem(diretorioImagens, CSGlobal.COD_REVENDA, espacoDisponivel, null);
                    else
                        syncProvider.CargaImagem(diretorioImagens, CSGlobal.COD_REVENDA, espacoDisponivel, RetornaImagensExistentes(diretorioImagens));

                    var tempoCarga = DateTime.Now.Subtract(inicioCarga);

                    Sincronizacao.progressBar.Dismiss();
                    UpdateStatusCompleto("Tempo total gasto: " + tempoCarga.Hours.ToString("00") + ":" + tempoCarga.Minutes.ToString("00") + ":" + tempoCarga.Seconds.ToString("00"), 0, 0, null);
                }
                catch (System.Exception ex)
                {
                    Sincronizacao.progressBar.Dismiss();
                    MessageBox.Alert(CurrentActivity, ex.Message);
                }
            }

            private string[] RetornaImagensExistentes(string diretorio)
            {
                string[] imagens = null;

                if (Directory.Exists(diretorio))
                {
                    var arquivosJpg = new DirectoryInfo(diretorio).GetFiles("*.jpg");
                    var arquivosPng = new DirectoryInfo(diretorio).GetFiles("*.png");
                    var arquivosJpeg = new DirectoryInfo(diretorio).GetFiles("*.jpeg");

                    var totalImagens = arquivosJpg.Count() + arquivosPng.Count() + arquivosJpeg.Count();

                    imagens = new string[totalImagens];
                    int index = 0;

                    foreach (var arquivoJpg in arquivosJpg)
                    {
                        imagens[index] = arquivoJpg.Name.Replace(arquivoJpg.Extension.ToLower(), string.Empty);
                        index++;
                    }

                    foreach (var arquivoPng in arquivosPng)
                    {
                        imagens[index] = arquivoPng.Name.Replace(arquivoPng.Extension.ToLower(), string.Empty);
                        index++;
                    }

                    foreach (var arquivoJpeg in arquivosJpeg)
                    {
                        imagens[index] = arquivoJpeg.Name.Replace(arquivoJpeg.Extension.ToLower(), string.Empty);
                        index++;
                    }
                }

                return imagens;
            }

        }

        private bool PermitirCargaImagemVendedor()
        {
            int codVendedor = RetornarCodigoVendedor();

            if (codVendedor == 0)
                return false;
            else
            {
                var query = string.Format("SELECT IND_PERMITIR_CARGA_IMAGEM FROM EMPREGADO WHERE COD_EMPREGADO = {0}", codVendedor);

                var result = CSDataAccess.Instance.ExecuteScalar(query);

                if (result == System.DBNull.Value ||
                    Convert.ToInt32(result) == 0)
                    return false;
            }

            return true;
        }

        private int RetornarCodigoVendedor()
        {
            try
            {
                return Convert.ToInt32(CSConfiguracao.GetConfig("vendedorDefault"));
            }
            catch (System.Exception)
            {
                if (string.IsNullOrEmpty(txtVendedor.Text))
                    return 0;

                return Convert.ToInt32(txtVendedor.Text);
            }
        }

        void btnApagarBanco_Click(object sender, EventArgs e)
        {
            if (Vendedor == "0")
                MessageBox.AlertErro(this, "Nenhum banco encontrado.");
            else
            {
                if (CSDataAccess.DataBaseExists(CSGlobal.COD_REVENDA))
                {
                    CSDataAccess.Instance.AbreConexao();

                    if (CSDescarga.ExistemDadosNaoDescarregados(PackageManager.GetPackageInfo(PackageName, 0).VersionName))
                    {
                        MessageBox.Alert(this, "Com a exclusão do banco, TODOS os dados e os PEDIDOS serão perdidos. Confirma a exclusão?", "Confirmar", (_sender, _e) =>
                        {
                            Intent i = new Intent();
                            i.SetClass(this, typeof(DialogAutenticar));
                            i.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                            i.PutExtra("codigoVendedorAtual", Vendedor);
                            this.StartActivity(i);

                        }, "Cancelar", (_sender, _e) => { }, true);
                    }
                    else
                    {
                        Intent i = new Intent();
                        i.SetClass(this, typeof(DialogAutenticar));
                        i.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                        i.PutExtra("codigoVendedorAtual", Vendedor);
                        this.StartActivity(i);
                    }
                }
            }
        }

        private Android.Text.InputTypes TipoDoCampo(EditText campo)
        {
            return campo.InputType;
        }

        void txtServidor_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (ModificandoServidor)
                return;

            ModificandoServidor = true;

            if (e.BeforeCount == 1 && TipoDoCampo(txtServidor) != Android.Text.InputTypes.ClassText)
            {
                txtServidor.Text = string.Empty;
                ServidorPassword(false);
                imgIconeServidor.SetImageResource(Resource.Drawable.ic_simbolo_close);
                imgIconeEmpresa.SetImageResource(Resource.Drawable.ic_simbolo_close);

                spnEmpresa.Clear();
            }
            //else
            //{
            //    if (txtServidor.Enabled == false)
            //        return;

            //    if (ValidaServidor())
            //    {
            //        CSConfiguracao.SetConfig("internetURL", Criptografar(txtServidor.Text));
            //    }

            //    ModificandoServidor = false;
            //}

            ModificandoServidor = false;
        }

        private void btnCargaParcial_Click(object sender, EventArgs e)
        {
            VerificaTagBD();

            if (CSDataAccess.DataBaseExists(CSGlobal.COD_REVENDA))
            {
                if (!EmpregadoHorarioDeAlmoco())
                {
                    IniciarRotinaCargaParcial();
                }
                else
                {
                    MessageBox.Alert(this, "Não é possível realizar carga parcial dentro do horário de almoço. Informe o fim do horário dentro do sistema antes de prosseguir.");
                }
            }
            else
                MessageBox.Alert(this, "Banco não existente.");
        }

        void btnCargaCompleta_Click(object sender, EventArgs e)
        {
            VerificaTagBD();

            if (CSDataAccess.DataBaseExists(CSGlobal.COD_REVENDA))
            {
                if (!EmpregadoHorarioDeAlmoco())
                {
                    IniciarRotinaCargaCompleta();
                }
                else
                {
                    MessageBox.Alert(this, "Não é possível realizar carga completa dentro do horário de almoço. Informe o fim do horário dentro do sistema antes de prosseguir.");
                }
            }
            else
                IniciarRotinaCargaCompleta();
        }

        private void VerificaTagBD()
        {
            CSConfiguracao.VerificaTagDBFILE();
        }

        private bool EmpregadoHorarioDeAlmoco()
        {
            if (CSEmpresa.ColunaExiste("EMPREGADO_EXPEDIENTE", "COD_EMPREGADO"))
            {
                System.Text.StringBuilder sql = new System.Text.StringBuilder();
                sql.AppendFormat("SELECT DAT_INICIO_EXPEDIENTE,DAT_FIM_EXPEDIENTE FROM EMPREGADO_EXPEDIENTE WHERE COD_EMPREGADO = {0} AND IND_TIPO_EXPEDIENTE = 'A'", txtVendedor.Text);

                using (var reader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
                {
                    if (reader.Read())
                    {
                        if (reader.GetValue(0) != System.DBNull.Value)
                        {
                            if (reader.GetValue(1) == System.DBNull.Value)
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        private void IniciarRotinaCargaParcial()
        {
            tilVendedor.Error = null;
            tilVendedor.ErrorEnabled = false;

            Save();

            if (string.IsNullOrEmpty(txtVendedor.Text))
            {
                FuncoesView.SetarLabelErroControles(this, tilVendedor);
            }
            else
            {
                this.HideKeyboard(txtVendedor);
                cargaParcial = false;
                MessageBox.Alert(this, "Confirma realizar a Carga Parcial?", "Carga Parcial", (_sender, _e) => { CargaParcial(); }, "Cancelar", null, true);
            }
        }

        private void IniciarRotinaCargaCompleta()
        {
            tilVendedor.Error = null;
            tilVendedor.ErrorEnabled = false;
            Save();

            if (string.IsNullOrEmpty(txtVendedor.Text))
            {
                FuncoesView.SetarLabelErroControles(this, tilVendedor);
            }
            else
            {
                this.HideKeyboard(txtVendedor);
                cargaParcial = false;
                MessageBox.Alert(this, "Todos os dados atuais serão apagados. Confirma realizar a Carga Total?", "Carga Total", (_sender, _e) => { CargaCompleta(); }, "Cancelar", null, true);
            }
        }

        private void CargaParcial()
        {
            try
            {
                cargaParcial = true;

                progressBar = new ProgressDialog(this);
                progressBar.SetProgressStyle(ProgressDialogStyle.Horizontal);
                progressBar.SetTitle("Carga Parcial");
                progressBar.SetMessage("Realizando carga...");
                progressBar.SetCancelable(false);
                progressBar.Show();

                UpdateProvider();

                new ThreadBeginCarga().Execute();
            }
            catch (System.Exception ex)
            {
                MessageBox.AlertErro(this, ex.Message);
            }
        }

        private void CargaCompleta()
        {
            try
            {
                progressBar = new ProgressDialog(this);
                progressBar.SetProgressStyle(ProgressDialogStyle.Horizontal);
                progressBar.SetTitle("Carga Total");
                progressBar.SetMessage("Realizando carga...");
                progressBar.SetCancelable(false);
                progressBar.Show();

                UpdateProvider();

                new ThreadBeginCarga().Execute();
            }
            catch (System.Exception ex)
            {
                MessageBox.AlertErro(this, ex.Message);
            }
        }

        private static bool AtualizaDataDoSistema()
        {
            // Altera a data do sistema
            try
            {
                WebService.AvanteSales ws = new WebService.AvanteSales();

                ws.Url = ws.Url.Replace("localhost", Descriptografar(CSConfiguracao.GetConfig("internetURL")));
                DateTime d = ws.GetServerDate();

                // Funcao para alterar a data
                CSGlobal.MudaData(d);

                return true;

            }
            catch (System.Exception)
            {
                MessageBox.AlertErro(CurrentActivity, "Falha na conexão");
                return false;
            }
        }

        private static bool ValidaCodigoEmpregado(string strCodigoEmpregado)
        {
            // verifica se o codigo do empregado informado é valido
            try
            {
                int codigoEmpregado = 0;
                if (int.TryParse(strCodigoEmpregado, out codigoEmpregado))
                {
                    // Verifica se o codigo informado é valido
                    WebService.AvanteSales ws = new WebService.AvanteSales();
                    ws.Url = ws.Url.Replace("localhost", Descriptografar(CSConfiguracao.GetConfig("internetURL")));

                    if (!ws.IsEmpregado(codigoEmpregado, CSGlobal.COD_REVENDA))
                    {
                        MessageBox.AlertErro(CurrentActivity, "O código do vendedor não é válido!\nFavor informar um código válido.");
                        return false;
                    }

                    return true;
                }
                throw new System.Exception("O código do empregado não está com um valor numérico válido.");
            }
            catch (System.Exception)
            {
                MessageBox.AlertErro(CurrentActivity, "Empregado Inválido");
                return false;
            }
        }

        private class ThreadBeginCarga : AsyncTask
        {
            bool cargaExecutada = false;

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                AtualizaCodigoRevenda();

                if (!AtualizaDataDoSistema())
                {
                    Sincronizacao.progressBar.Dismiss();
                    return 0;
                }

                // Verifica se existe nova versao do sistema
                //if (HasNewVersion())
                //{
                //    RealizaAtualizacao();

                //    Cursor.Current = Cursors.Default;
                //    // Sai da função
                //    return;
                //}

                // [ Verifica se o codigo do empregado informado é valido ]
                if (!ValidaCodigoEmpregado(txtVendedor.Text))
                {
                    Sincronizacao.progressBar.Dismiss();
                    return 0;
                }

                // Verifica se este banco fisico
                if (CSDataAccess.DataBaseExists(CSGlobal.COD_REVENDA) &&
                    !cargaParcial)
                {
                    CSDataAccess.Instance.AbreConexao();

                    if (CSDescarga.ExistemDadosNaoDescarregados(""))
                    {
                        Sincronizacao.progressBar.Dismiss();
                        MessageBox.AlertErro(CurrentActivity,
                            "Existem dados novos ou que foram alterados apartir de \'" +
                            CSDescarga.DataUltimaDescarga.ToString("dd/MM/yyyy HH:mm:ss") +
                            "\' , data da última descarga realizada. Descarregue, e faça a carga novamente.");
                        return 0;
                    }
                }

                CSDataAccess.Instance.FechaConexao();
                // [ Apaga bancos de versões anteriores do avante ou de outras empresas ]
                ApagaBancosNaoUtilizados();

                BeginCarga();
                return 0;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                base.OnPostExecute(result);

                if (cargaExecutada)
                {
                    if (!cargaParcial)
                    {
                        AtualizarUltimaCargaCompleta();
                    }
                    else
                    {
                        AtualizarUltimaCargaParcial();
                    }
                }
            }

            /// <summary>
            /// Entrypoint para o worker thread de carga
            /// </summary>
            private void BeginCarga()
            {
                try
                {
                    syncProvider.DataBaseFilePath = DatabaseFilePath;
                    CSGlobal.COD_REVENDA = GetEmpresa();
                    CSConfiguracao.SetConfig("vendedor" + CSGlobal.COD_REVENDA, GetVendedor());
                    DateTime inicioCargaTotal = DateTime.Now;
                    TimeSpan tempoCarga;

                    CSDataAccess.UpdateStatus objDelegate = new CSDataAccess.UpdateStatus(UpdateStatus);

                    executandoCargaTotal = true;

                    CSDataAccess.Instance.FechaConexao();
                    DisableButtons();

                    string mensagem = string.Empty;
                    syncProvider.VerificaVersaoDLL(ref mensagem, "", VersaoAvante);

                    if (cargaParcial)
                        syncProvider.CargaParcial(syncProvider.ProviderName, CSConfiguracao.GetConfig("vendedor" + CSGlobal.COD_REVENDA), CSGlobal.COD_REVENDA, Imei, "P");
                    else
                        syncProvider.Carga(syncProvider.ProviderName, CSConfiguracao.GetConfig("vendedor" + CSGlobal.COD_REVENDA), CSGlobal.COD_REVENDA, Imei, "C");

                    // Esse metodo foi inserido para evitar que o banco se corrompa, ocorreu na revenda Conteda
                    if (!cargaParcial)
                        CSDataAccess.Instance.ConstroiIndices(objDelegate);

                    CSDataAccess.Instance.CompressDB();
                    tempoCarga = DateTime.Now.Subtract(inicioCargaTotal);

                    CurrentActivity.RunOnUiThread(() => { Sincronizacao.progressBar.Dismiss(); imgIconeEmpresa.SetImageResource(Resource.Drawable.ic_simbolo_ok); });

                    if (string.IsNullOrEmpty(mensagem))
                        UpdateStatusCompleto("Tempo total gasto: " + tempoCarga.Hours.ToString("00") + ":" + tempoCarga.Minutes.ToString("00") + ":" + tempoCarga.Seconds.ToString("00"), 0, 0, null);
                    else
                        UpdateStatusCompleto(mensagem, 0, 0, null);

                    //if (!cargaParcial)
                    //{
                    //    CurrentActivity.RunOnUiThread(() =>
                    //        {
                    //            AtualizarUltimaCargaCompleta();
                    //        });
                    //}
                    //else
                    //{
                    //    CurrentActivity.RunOnUiThread(() =>
                    //    {
                    //        AtualizarUltimaCargaParcial();
                    //    });
                    //}

                    // [ Marca o banco para atualizado = S ]
                    CSConfiguracao.SetConfig("ATUALIZADO_" + CSGlobal.COD_REVENDA, CSGlobal.STATUS_ATUALIZACAO.SUCESSO);

                    // [ Apresenta mensagem de sucesso e fecha o sistema ]
                    //CurrentActivity.ShowShortMessage("Carga Total realizada com sucesso!");

                    EnableButtons();

                    cargaExecutada = true;

                }
                catch (System.Exception ex)
                {
                    CurrentActivity.RunOnUiThread(() => { Sincronizacao.progressBar.Dismiss(); });
                    MessageBox.AlertErro(CurrentActivity, ex.Message);

                    EnableButtons();
                    executandoCargaTotal = false;
                    UpdateStatus("Erro ao executar a Carga Total", 0, 0, null);
                }
            }

            private static void EnableButtons()
            {
                CurrentActivity.RunOnUiThread(() =>
                {
                    //btnCargaParcial.Enabled = true;
                    btnCargaCompleta.Enabled = true;
                    btnCargaParcial.Enabled = true;

                    //Desabilita o botao de carga parcial para o provider SSCEDirectProvider
                    //if (CSConfiguracao.GetConfig("syncProvider") == "Master.CompactFramework.Sync.SQLLiteProvider.SQLLiteDirectProvider")
                    //    btnCargaParcial.Visibility = ViewStates.Invisible;

                    if (CSDataAccess.DataBaseExists(CSGlobal.COD_REVENDA))
                        //Abre a conexao com o banco de dados apos fazer uma carga inicial ou parcial
                        CSDataAccess.Instance.AbreConexao();
                });
            }

            // Thread invoke helper function
            private static void DisableButtons()
            {
                CurrentActivity.RunOnUiThread(() =>
                {
                    btnCargaParcial.Enabled = false;
                    btnCargaCompleta.Enabled = false;
                });
            }

            private void AtualizarUltimaCargaCompleta()
            {
                if (CSEmpresa.ColunaExiste("EMPRESA", "IND_UTILIZA_CARGA_PARCIAL"))
                {
                    CSDataAccess.Instance.AbreConexao();

                    string query = string.Format("UPDATE INFORMACOES_SINCRONIZACAO SET DATA_ULTIMA_CARGA_COMPLETA = DATETIME('{0}')", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    CSDataAccess.Instance.ExecuteNonQuery(query);

                    VerificarCargaParcial();

                    CSDataAccess.Instance.FechaConexao();
                }
                else
                    btnCargaParcial.Visibility = ViewStates.Gone;
            }

            private void AtualizarUltimaCargaParcial()
            {
                CSDataAccess.Instance.AbreConexao();

                if (CSEmpresa.ColunaExiste("EMPRESA", "IND_UTILIZA_CARGA_PARCIAL"))
                {
                    string query = string.Format("UPDATE INFORMACOES_SINCRONIZACAO SET DATA_ULTIMA_CARGA_PARCIAL = DATETIME('{0}')", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    CSDataAccess.Instance.ExecuteNonQuery(query);
                }

                CSDataAccess.Instance.FechaConexao();
            }

            private string GetVendedor()
            {
                return txtVendedor.Text.Trim();
            }

        }

        private static void UpdateProvider()
        {
            string empresaFile = System.IO.Path.Combine(CSGlobal.GetCurrentDirectory(), "empresas.xml");
            if (CSConfiguracao.GetConfig("syncProvider") != "" && CSConfiguracao.GetConfig("internetURL") != "" && File.Exists(empresaFile))
            {
                syncProvider = new SQLLiteDirectProvider();

                if (syncProvider == null)
                {
                    MessageBox.AlertErro(CurrentActivity, "Erro criando o provider de sincronização. Por favor, verifique a integridade do sistema.");
                    return;
                }

                syncProvider.StatusChanged += new Master.CompactFramework.Sync.SyncManager.StatusChangedEventHandler(syncProvider_StatusChanged);
                syncProvider.CheckPointCompleted += new Master.CompactFramework.Sync.SyncManager.StatusChangedEventHandler(syncProvider_CheckPointCompleted);
                syncProvider.DownloadCompleted += new System.EventHandler(syncProvider_DownloadCompleted);

                try
                {
                    var internetURL = Descriptografar(CSConfiguracao.GetConfig("internetURL"));
                    Uri serverAdress;
                    if (Uri.TryCreate("http://" + internetURL + "/AvanteSales/WSSales/AvanteSales.asmx", UriKind.RelativeOrAbsolute, out serverAdress))
                    {
                        syncProvider.ServerAddress = serverAdress.ToString();
                    }
                    else
                    {
                        MessageBox.AlertErro(CurrentActivity, "Endereço do servidor inválido. Por favor configure-o novamente.");
                        //CurrentActivity.StartActivity(new Intent(CurrentActivity, new Configuracao().Class));
                    }

                }
                catch (System.Exception ex)
                {
                    MessageBox.AlertErro(CurrentActivity, ex.Message);
                }
            }
            else
            {
                //CurrentActivity.StartActivity(new Intent(CurrentActivity, new Configuracao().Class));
            }
        }

        private static void ServidorPassword(bool alterarParaPassword)
        {
            if (!alterarParaPassword)
                txtServidor.InputType = Android.Text.InputTypes.ClassText;
            else
                txtServidor.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextVariationPassword;
        }

        void btnEditarDados_Click(object sender, EventArgs e)
        {

            MessageBox.Alert(this, "Deseja digitar o IP manualmente ou importar um arquivo?",
                "Digitar", (_sender, _e) =>
                {
                    txtServidor.Enabled = true;
                    imgIconeServidor.SetImageResource(Resource.Drawable.ic_simbolo_close);

                    txtVendedor.Enabled = true;
                    CSGlobal.Focus(this, txtServidor);

                    if (string.IsNullOrEmpty(txtServidor.Text))
                        ServidorPassword(false);

                    CSGlobal.SetSelection(txtServidor);
                },
                "Cancelar", (_sender, _e) =>
                {
                },
                "Importar", (_sender, _e) =>
                {
                    imgIconeServidor.SetImageResource(Resource.Drawable.ic_simbolo_close);
                    imgIconeEmpresa.SetImageResource(Resource.Drawable.ic_simbolo_close);

                    spnEmpresa.Clear();

                    ServidorPassword(true);
                    ImportarArquivoConfiguracao();
                }, true);

            //txtVendedor.Enabled = true;
            //spnEmpresa.Enabled = true;
            //btnEditarDados.Visibility = ViewStates.Gone;

        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                switch (requestCode)
                {
                    case frmImportacaoArquivo:
                        {
                            if (resultCode == Result.Ok)
                            {
                                FileInfo file = new FileInfo(data.Data.Path);

                                if (file.Extension.ToLower() == ".txt")
                                {
                                    FileStream arquivo = new FileStream(file.FullName, FileMode.OpenOrCreate);

                                    StreamReader sr = new StreamReader(arquivo);
                                    var conteudoCriptografado = sr.ReadToEnd();
                                    sr.Close();

                                    var conteudoDescriptografado = Descriptografar(conteudoCriptografado);

                                    if (conteudoDescriptografado.Contains("|"))
                                    {
                                        string[] chaves = conteudoDescriptografado.Split('|').ToArray();

                                        IpImportado = chaves[0];

                                        Intent i = new Intent();
                                        i.SetClass(this, typeof(DialogSenhaArquivo));
                                        i.PutExtra("senha", chaves[1]);
                                        this.StartActivityForResult(i, frmSenhaArquivo);
                                    }
                                    else
                                    {
                                        MessageBox.AlertErro(this, "Arquivo incorreto.");
                                    }
                                }
                                else
                                {
                                    MessageBox.AlertErro(this, "Arquivo incorreto.");
                                }
                            }

                            break;
                        }
                    case frmSenhaArquivo:
                        {
                            if (resultCode == Result.Ok)
                            {
                                txtServidor.Text = IpImportado;
                                Save();

                                if (!TestandoConexao)
                                {
                                    progressTesteConexao = new ProgressDialogCustomizado(this, LayoutInflater).Customizar();
                                    progressTesteConexao.Show();
                                    TestandoConexao = true;
                                    TesteConexao(true);
                                }
                            }
                            else
                                IpImportado = string.Empty;

                            break;
                        }
                }

                base.OnActivityResult(requestCode, resultCode, data);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message);
            }
        }

        private void ImportarArquivoConfiguracao()
        {
            Intent intent = new Intent(Intent.ActionGetContent);
            intent.SetType("text/plain");
            intent.AddCategory(Intent.CategoryOpenable);

            this.StartActivityForResult(
                    Intent.CreateChooser(intent, "Selecione o arquivo para importação"),
                    frmImportacaoArquivo);
        }

        void btnSair_Click(object sender, EventArgs e)
        {
            Sair();
        }

        private static void Sair()
        {
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }

        private void FindViewsById()
        {
            tbToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tbToolbar);
            lblVersao = FindViewById<TextView>(Resource.Id.lblVersao);
            txtImei = FindViewById<EditText>(Resource.Id.txtImei);
            txtServidor = FindViewById<EditText>(Resource.Id.txtServidor);
            btnSair = FindViewById<Button>(Resource.Id.btnSair);
            btnCargaCompleta = FindViewById<Button>(Resource.Id.btnCargaCompleta);
            btnCargaParcial = FindViewById<Button>(Resource.Id.btnCargaParcial);
            btnEditarDados = FindViewById<Button>(Resource.Id.btnEditarDados);
            btnApagarBanco = FindViewById<Button>(Resource.Id.btnApagarBanco);
            btnCargaImagens = FindViewById<Button>(Resource.Id.btnCargaImagens);
            btnLogin = FindViewById<Button>(Resource.Id.btnLogin);
            imgIconeServidor = FindViewById<ImageView>(Resource.Id.imgIconeServidor);
            imgIconeEmpresa = FindViewById<ImageView>(Resource.Id.imgIconeEmpresa);
            tilServidor = FindViewById<TextInputLayout>(Resource.Id.tilServidor);
            tilEmpresa = FindViewById<TextInputLayout>(Resource.Id.tilEmpresa);
            tilVendedor = FindViewById<TextInputLayout>(Resource.Id.tilVendedor);
            txtVendedor = FindViewById<EditText>(Resource.Id.txtVendedor);
            spnEmpresa = FindViewById<Spinner>(Resource.Id.spnEmpresa);
        }

        private void Inicializacao()
        {
            try
            {
                CurrentActivity = this;
                ModificandoServidor = false;

                txtServidor.Text = Descriptografar(CSConfiguracao.GetConfig("internetURL"));
                txtVendedor.Text = CSConfiguracao.GetConfig("vendedorDefault");
                lblVersao.Text = VersaoAvante = string.Format("v{0}", PackageManager.GetPackageInfo(PackageName, 0).VersionName);
                txtImei.Text = Imei = CSGlobal.GetDeviceId(this);
                txtImei.Enabled = false;

                if (string.IsNullOrEmpty(txtServidor.Text))
                {
                    CSGlobal.Focus(this, txtServidor);
                    ServidorPassword(false);
                }

                CarregaComboBoxEmpresa();

                if (txtServidor.Text != string.Empty)
                {
                    txtServidor.Enabled = false;
                }

                if (txtVendedor.Text != string.Empty)
                {
                    txtVendedor.Enabled = false;
                }

                if (CSDataAccess.DataBaseExists(CSGlobal.COD_REVENDA))
                {
                    if (CSEmpresa.ColunaExiste("PEDIDO", "BOL_PEDIDO_VALIDADO"))
                    {
                        int[] Resultados = new int[2];

                        Resultados = CSPedidosPDV.EXISTE_PEDIDO_SALVAMENTO_PENDENTE();

                        if (Resultados[0] != 0)
                            MessageBox.Alert(this, "O pedido de código " + Resultados[0].ToString() + ", referente ao PDV " + Resultados[1].ToString() + " não foi validado corretamente. Faça o login no sistema novamente para corrigí-lo.", "Login", (_sender, _e) => { AbrirLogin(); }, false);
                    }

                    VerificarCargaParcial();
                }
                else
                    btnCargaParcial.Visibility = ViewStates.Gone;

                Vendedor = Intent.GetStringExtra("codigoVendedor");

                if (!TestandoConexao)
                {
                    if (!string.IsNullOrEmpty(txtServidor.Text))
                    {
                        TestandoConexao = true;
                        if (spnEmpresa.Adapter != null &&
                            spnEmpresa.Adapter.Count > 0)
                        {
                            progressTesteConexao = new ProgressDialogCustomizado(this, LayoutInflater).Customizar();
                            progressTesteConexao.Show();

                            TesteConexao(true);
                        }
                        else
                        {
                            progressTesteConexao = new ProgressDialogCustomizado(this, LayoutInflater).Customizar();
                            progressTesteConexao.Show();

                            TesteConexao(false);
                        }
                    }
                }
            }
            catch (System.Exception)
            {

            }
            finally
            {
                if (progressTesteConexao != null)
                {
                    TestandoConexao = false;
                    progressTesteConexao.Dismiss();
                }
            }
        }

        private static void VerificarCargaParcial()
        {
            bool utilizaCargaParcial = false;

            if (CSEmpresa.ColunaExiste("EMPRESA", "IND_UTILIZA_CARGA_PARCIAL"))
            {
                string query = "SELECT IND_UTILIZA_CARGA_PARCIAL FROM EMPRESA";

                utilizaCargaParcial = CSDataAccess.Instance.ExecuteScalar(query).ToString().ToUpper() == "S" ? true : false;

                if (utilizaCargaParcial)
                {
                    query = "SELECT DATA_ULTIMA_CARGA_COMPLETA FROM INFORMACOES_SINCRONIZACAO";

                    var result = CSDataAccess.Instance.ExecuteScalar(query);

                    if (result == System.DBNull.Value ||
                        Convert.ToDateTime(result).Date != DateTime.Now.Date)
                        utilizaCargaParcial = false;
                }
            }

            btnCargaParcial.Visibility = utilizaCargaParcial ? ViewStates.Visible : ViewStates.Gone;
        }

        #region IOnEditorActionListener Members

        public bool OnEditorAction(TextView v, Android.Views.InputMethods.ImeAction actionId, KeyEvent e)
        {
            if (v == txtServidor &&
                !string.IsNullOrEmpty(txtServidor.Text))
            {
                if (!TestandoConexao)
                {
                    progressTesteConexao = new ProgressDialogCustomizado(this, LayoutInflater).Customizar();
                    progressTesteConexao.Show();
                    TestandoConexao = true;
                    TesteConexao(true);
                }

                return true;
            }

            return false;
        }

        private static string Criptografar(string conteudoDescriptografado)
        {
            char carectereAtual;
            string conteudoCriptografado = string.Empty;

            for (int i = 0; i < conteudoDescriptografado.Length; i++)
            {
                carectereAtual = Convert.ToChar(conteudoDescriptografado.Substring(i, 1));

                var byteChar = Encoding.UTF8.GetBytes(carectereAtual.ToString())[0];

                conteudoCriptografado += ((byteChar) + 120).ToString() + " ";
            }

            string criptografado = conteudoCriptografado.TrimEnd();

            return criptografado;
        }

        private static void Save()
        {
            CSConfiguracao.SetConfig("internetURL", Criptografar(txtServidor.Text));
            CSConfiguracao.SetConfig("vendedorDefault", txtVendedor.Text);
            CSConfiguracao.SetConfig("syncProvider", "SQLLiteDirectProvider");
        }

        private void TesteConexao(bool buscarEmpresasTambem)
        {
            try
            {
                LimparErros();

                imgIconeServidor.SetImageResource(Resource.Drawable.ic_simbolo_close);
                imgIconeEmpresa.SetImageResource(Resource.Drawable.ic_simbolo_close);

                new ThreadTesteConexao(buscarEmpresasTambem, LayoutInflater).Execute();
            }
            catch (System.Exception ex)
            {
                FuncoesView.SetarLabelErroControles(CurrentActivity, tilServidor, ex.Message);
            }
        }

        private static void LimparErros()
        {
            tilServidor.Error = null;
            tilServidor.ErrorEnabled = false;

            tilEmpresa.Error = null;
            tilEmpresa.ErrorEnabled = false;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            base.OnCreateOptionsMenu(menu);
            MenuInflater inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.menu_sincronizacao, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.itmConexao:
                    {
                        if (!TestandoConexao)
                        {
                            progressTesteConexao = new ProgressDialogCustomizado(this, LayoutInflater).Customizar();
                            progressTesteConexao.Show();
                            TestandoConexao = true;
                            TesteConexao(false);
                        }

                        return true;
                    }
                case Resource.Id.itmBuscaEmpresa:
                    {
                        progressBuscaEmpresa = new ProgressDialogCustomizado(CurrentActivity, LayoutInflater).Customizar();
                        progressBuscaEmpresa.Show();

                        new ThreadBuscarEmpresas().Execute();

                        return true;
                    }
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        private static bool ValidaServidor()
        {
            int i = 0;

            if (int.TryParse(txtServidor.Text.Substring(0, 1), out i))
            {
                IPAddress ipConvertido;

                if (!IPAddress.TryParse(txtServidor.Text.Split(':')[0], out ipConvertido))
                {
                    FuncoesView.SetarLabelErroControles(CurrentActivity, tilServidor, "Endereço de IP inválido.");
                    return false;
                }
            }

            else if (!(int.TryParse(txtServidor.Text.Substring(0, 1), out i)))
            {
                Uri uriConvertido;

                if (!Uri.TryCreate("http://" + txtServidor.Text + "/AvanteSales.Pro/WSSales/AvanteSales.Pro.asmx", UriKind.RelativeOrAbsolute, out uriConvertido))
                {
                    FuncoesView.SetarLabelErroControles(CurrentActivity, tilServidor, "Endereço de IP inválido.");
                    return false;
                }
            }

            if (txtServidor.Text.Contains(':'))
            {
                var porta = txtServidor.Text.Split(':')[1];
                int portaConvertida = 0;
                if (!int.TryParse(porta, out portaConvertida))
                {
                    FuncoesView.SetarLabelErroControles(CurrentActivity, tilServidor, "Endereço de porta inválido.");
                    return false;
                }
            }

            Uri uriConvertida;
            if (!Uri.TryCreate("http://" + txtServidor.Text + "/AvanteSales.Pro/WSSales/AvanteSales.Pro.asmx", UriKind.RelativeOrAbsolute, out uriConvertida))
            {
                FuncoesView.SetarLabelErroControles(CurrentActivity, tilServidor, "Endereço do servidor inválido.");
                return false;
            }

            return true;
        }

        private class ThreadTesteConexao : AsyncTask
        {
            bool BuscarEmpresa;
            LayoutInflater LayoutInflater;

            public ThreadTesteConexao(bool buscarEmpresa, LayoutInflater inflater)
            {
                this.BuscarEmpresa = buscarEmpresa;
                this.LayoutInflater = inflater;
            }

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                try
                {
                    LimparErros();

                    imgIconeServidor.SetImageResource(Resource.Drawable.ic_simbolo_close);
                    imgIconeEmpresa.SetImageResource(Resource.Drawable.ic_simbolo_close);

                    if (ValidaServidor())
                    {
                        Save();

                        WebService.AvanteSales ws = new WebService.AvanteSales();
                        ws.Url = Regex.Replace(ws.Url, "localhost", Descriptografar(CSConfiguracao.GetConfig("internetURL")));
                        ws.Timeout = 15000;

                        DateTime d = ws.GetServerDate();

                        return true;
                    }

                    return false;
                }
                catch (System.Exception)
                {
                    return false;
                }
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                base.OnPostExecute(result);

                if (Convert.ToBoolean(result) == true)
                {
                    imgIconeServidor.SetImageResource(Resource.Drawable.ic_simbolo_ok);

                    CSGlobal.Focus(CurrentActivity, txtVendedor);
                    ServidorPassword(true);
                    txtServidor.Enabled = false;

                    if (BuscarEmpresa)
                    {
                        progressBuscaEmpresa = new ProgressDialogCustomizado(CurrentActivity, LayoutInflater).Customizar();
                        progressBuscaEmpresa.Show();

                        new ThreadBuscarEmpresas().Execute();
                    }
                }
                else
                {
                    FuncoesView.SetarLabelErroControles(CurrentActivity, tilServidor, "Servidor inválido ou sem conexão com internet.");
                }

                TestandoConexao = false;
                progressTesteConexao.Dismiss();
            }
        }

        //private void ThreadTestConexao(bool buscarEmpresaTambem)
        //{
        //    try
        //    {
        //        TesteDeConexao(buscarEmpresaTambem);
        //    }
        //    catch (System.Net.WebException ex)
        //    {
        //        string mensagemUsuario = string.Empty;

        //        try
        //        {
        //            switch (ex.Message)
        //            {
        //                case "Error: NameResolutionFailure":
        //                    mensagemUsuario = "Não foi possível conectar ao servidor. Verifique endereço do servidor.";
        //                    break;
        //                case "The request timed out":
        //                    mensagemUsuario = "O tempo limite para teste da conexão foi esgotado. Verifique o endereço do servidor.";
        //                    break;
        //                default:
        //                    mensagemUsuario = ex.Message;
        //                    break;
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            mensagemUsuario = "Erro desconhecido no teste da conexão. Favor entrar em contato com o suporte.";
        //        }

        //        this.RunOnUiThread(() =>
        //            {
        //                FuncoesView.SetarLabelErroControles(this, tilServidor, mensagemUsuario);
        //            });
        //    }
        //    catch (Exception)
        //    {
        //        this.RunOnUiThread(() =>
        //            {
        //                FuncoesView.SetarLabelErroControles(this, tilServidor, "Servidor inválido.");
        //            });
        //    }
        //    finally
        //    {
        //        progressTesteConexao.Dismiss();
        //    }
        //}

        private class ThreadBuscarEmpresas : AsyncTask
        {
            ArrayAdapter adapter;
            int index;

            private void CarregaComboBoxEmpresa()
            {
                try
                {
                    foreach (var item in CSConfiguracao.GetEmpresas())
                    {
                        adapter.Add(item.ToString());
                    }

                    if (CSGlobal.COD_REVENDA == "XXXXXXXX" && adapter.Count > 0)
                    {
                        CSGlobal.COD_REVENDA = adapter.GetItem(0).ToString().Substring(0, 8);
                    }

                    index = -1;

                    // [ Seleciona a primeira empresa no qual o banco de dados existe ]
                    for (int i = 0; i < adapter.Count; i++)
                    {
                        if (adapter.GetItem(i).ToString().Substring(0, 8) == CSGlobal.COD_REVENDA)
                        {
                            index = i;

                            return;
                        }
                    }

                    if (index == -1)
                        index = 0;

                    AtualizaCodigoRevenda();
                }
                catch (System.Exception ex)
                {
                    throw new System.Exception(ex.Message);
                }
            }

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                try
                {
                    adapter = new ArrayAdapter(CurrentActivity, Android.Resource.Layout.SimpleSpinnerItem);
                    adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

                    WebService.AvanteSales ws = new WebService.AvanteSales();
                    ws.Url = Regex.Replace(ws.Url, "localhost", Descriptografar(CSConfiguracao.GetConfig("internetURL")));
                    ws.Timeout = 15000;
                    string empresaFile = System.IO.Path.Combine(CSGlobal.GetCurrentDirectory(), "empresas.xml");
                    System.Data.DataSet ds = ws.GetEmpresas();

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        ds.WriteXml(empresaFile);
                        CarregaComboBoxEmpresa();
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (System.Exception)
                {
                    return false;
                }

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                try
                {
                    base.OnPostExecute(result);

                    if (Convert.ToBoolean(result) == true)
                    {
                        if (adapter.Count > 0)
                        {
                            spnEmpresa.Adapter = adapter;
                            spnEmpresa.SetSelection(index);
                            imgIconeEmpresa.SetImageResource(Resource.Drawable.ic_simbolo_ok);
                        }
                        else
                            imgIconeEmpresa.SetImageResource(Resource.Drawable.ic_simbolo_close);
                    }
                    else
                    {
                        FuncoesView.SetarLabelErroControles(CurrentActivity, tilEmpresa, "Busca de empresas falhou, verifique se o nome do servidor IIS está correto.");
                    }

                    if (progressBuscaEmpresa != null)
                        progressBuscaEmpresa.Dismiss();
                }
                catch (System.Exception)
                {

                }
            }
        }

        private void CarregaComboBoxEmpresa()
        {
            try
            {
                // [ Carrega dados ]
                var adapter = spnEmpresa.SetDefaultAdapter();

                foreach (var item in CSConfiguracao.GetEmpresas())
                {
                    adapter.Add(item.ToString());
                }

                if (CSGlobal.COD_REVENDA == "XXXXXXXX" && adapter.Count > 0)
                {
                    CSGlobal.COD_REVENDA = adapter.GetItem(0).ToString().Substring(0, 8);
                }

                // [ Seleciona a primeira empresa no qual o banco de dados existe ]
                for (int index = 0; index < adapter.Count; index++)
                {
                    if (spnEmpresa.GetItemAtPosition(index).ToString().Substring(0, 8) == CSGlobal.COD_REVENDA)
                    {
                        spnEmpresa.SetSelection(index);
                        imgIconeEmpresa.SetImageResource(Resource.Drawable.ic_simbolo_ok);
                        return;
                    }
                }

                // [ Seleciona a primeira no caso de não haver banco de dados ]
                if (spnEmpresa.Adapter.Count > 0)
                    spnEmpresa.SetSelection(0);

                AtualizaCodigoRevenda();

                if (adapter.Count > 0)
                    imgIconeEmpresa.SetImageResource(Resource.Drawable.ic_simbolo_ok);
            }
            catch (System.Exception ex)
            {
                if (ex.Message.Contains("Document element did not appear"))
                {
                    txtServidor.RequestFocus();
                    txtVendedor.RequestFocus();
                }
                else
                {
                    this.RunOnUiThread(() =>
                    {
                        MessageBox.AlertErro(this, ex.Message); ;
                    });
                }
            }
        }

        public static string GetEmpresa()
        {
            var empresaSelecionada = spnEmpresa.SelectedItem.ToString().Substring(0, 8);
            return empresaSelecionada;
        }

        private static void AtualizaCodigoRevenda()
        {
            if (spnEmpresa.Adapter != null && spnEmpresa.Adapter.Count > 0)
            {
                CSGlobal.COD_REVENDA = GetEmpresa();
            }
        }

        public override void OnBackPressed()
        {
            MessageBox.Alert(this, "Deseja Sair?", "Sair", (_sender, _e) => { Sair(); }, "Cancelar", (_sender, _e) => { }, true);
        }

        private static string Descriptografar(string conteudoCriptografado)
        {
            try
            {
                string conteudoDescriptografado = string.Empty;

                if (conteudoCriptografado.Contains(' '))
                {
                    if (!conteudoCriptografado.Contains(' '))
                        return conteudoCriptografado;

                    string[] charSenha = conteudoCriptografado.Split(' ').ToArray();

                    for (int i = 0; i < charSenha.Length; i++)
                    {
                        int byteAtual = Convert.ToInt32(charSenha[i]);

                        conteudoDescriptografado += Convert.ToChar(Convert.ToInt32(Convert.ToInt32(byteAtual) - 120));
                    }
                }
                else
                    conteudoDescriptografado = conteudoCriptografado;

                return conteudoDescriptografado;
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message);
            }
        }

        #endregion

        #region [ Progress Bar ]

        private static void syncProvider_StatusChanged(string statusMessage, int maxProgress, int currentProgress, Bitmap icon)
        {
            UpdateStatus(statusMessage, maxProgress, currentProgress, icon);
        }

        private static void syncProvider_CheckPointCompleted(string statusMessage, int maxProgress, int currentProgress, Bitmap icon)
        {
            UpdateStatus(statusMessage, maxProgress, currentProgress, icon);
        }

        private static void syncProvider_DownloadCompleted(object sender, System.EventArgs e)
        {

            //bolOcorreuErroAtualizarData = false;
            try
            {
                if (executandoCargaTotal)
                {
                    //// [ Se não for carga para atualização de versão, marca o banco para atualizado = FALHA ]
                    //if (CSConfiguracao.GetConfig("ATUALIZADO_" + CSGlobal.COD_REVENDA) != CSGlobal.STATUS_ATUALIZACAO.ATUALIZACAO_VERSAO)
                    //    CSConfiguracao.SetConfig("ATUALIZADO_" + CSGlobal.COD_REVENDA, CSGlobal.STATUS_ATUALIZACAO.FALHA_CARGA_TOTAL);
                }
                else
                {
                    // [ Se não for carga para atualização de versão, marca o banco para atualizado = FALHA ]
                    //if (CSConfiguracao.GetConfig("ATUALIZADO_" + CSGlobal.COD_REVENDA) != CSGlobal.STATUS_ATUALIZACAO.ATUALIZACAO_VERSAO)
                    //    CSConfiguracao.SetConfig("ATUALIZADO_" + CSGlobal.COD_REVENDA, CSGlobal.STATUS_ATUALIZACAO.FALHA_CARGA_PARCIAL);

                    // gravar log
                    // somente para carga parcial grava o Log aqui no lado do pocket para nao haver problema na 
                    // sincronizacao o evento DownloadCompleted nao esta sendo disparado pelo componente direct
                    CSGlobal.GravaLogSync("S", "OK");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.AlertErro(CurrentActivity, ex.Message);
                //bolOcorreuErroAtualizarData = true;
            }
        }

        // Novo metodo para evitar travamento
        private static void UpdateStatusCompleto(string statusMessage, int maxProgress, int currentProgress, Bitmap icon)
        {
            new ThreadControleAcesso().Execute();
            MessageBox.Alert(CurrentActivity, "Sincronização completa", "Ok", null, true);
            //CurrentActivity.RunOnUiThread(() =>
            //    {
            //        Sincronizacao.progressDialog.SetMessage(statusMessage);
            //        Sincronizacao.progressDialog.Max = maxProgress;
            //        Sincronizacao.progressDialog.Progress = currentProgress;
            //    });
        }

        private class ThreadControleAcesso : AsyncTask
        {
            public ThreadControleAcesso()
            {

            }

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                EnviarInformacoesAcesso();
                return null;
            }

            private void EnviarInformacoesAcesso()
            {
                try
                {
                    var informacoes = InformacoesAcesso();

                    string urlConexao;

                    //urlConexao = "http://192.168.1.107/ControleAcesso/api/acesso";
                    urlConexao = "http://clientes.flag.com.br/ControleAcesso/api/acesso";

                    URL url = new URL(urlConexao);
                    HttpURLConnection conexao =
                      (HttpURLConnection)url.OpenConnection();

                    conexao.RequestMethod = "POST";
                    conexao.AddRequestProperty(
                    "Content-type", "application/json");

                    conexao.DoOutput = true;

                    conexao.Connect();

                    System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

                    Java.IO.OutputStream os = new Java.IO.BufferedOutputStream(conexao.OutputStream);

                    string informacao = string.Empty;
                    informacao = "{'CodDistribuidor':'" + informacoes[0] + "',";
                    informacao += "'NomeDistribuidor':'" + informacoes[1] + "',";
                    informacao += "'CodEmpregado':'" + informacoes[2] + "',";
                    informacao += "'NomeEmpregado':'" + informacoes[3] + "',";
                    informacao += "'Imei':'" + informacoes[4] + "',";
                    informacao += "'VersaoAvante':'" + informacoes[5] + "',";
                    informacao += "'VersaoWebService':'" + informacoes[6] + "',";
                    informacao += "'VersaoAndroid':'" + informacoes[7] + "',";
                    informacao += "'Modelo':'" + informacoes[8] + "'}";

                    os.Write(encoding.GetBytes(informacao.ToString()));
                    os.Flush();

                    Java.IO.InputStream imp = new Java.IO.BufferedInputStream(conexao.InputStream);
                }
                catch (System.Exception)
                {

                }
            }

            private List<string> InformacoesAcesso()
            {
                List<string> informacoes;

                string query = string.Format(@"SELECT COD_REVENDA
                                                       ,DSC_EMPRESA       
                                                       ,COD_EMPREGADO       
                                                       ,NOM_EMPREGADO
                                                       ,'{0}' AS IMEI
                                                       ,'{1}' AS VERSAO_AVANTE       
                                                       ,VERSAO_WEBSERVICE       
                                                       ,'{2}' AS VERSAO_ANDROID       
                                                       ,'{3}' AS MODELO
                                                FROM EMPRESA
                                                JOIN EMPREGADO
                                                WHERE COD_EMPREGADO = {4}", Imei,
                                                                            CurrentActivity.PackageManager.GetPackageInfo(CurrentActivity.PackageName, 0).VersionName,
                                                                            Android.OS.Build.VERSION.Release,
                                                                            string.Format("{0} {1}", Android.OS.Build.Manufacturer,
                                                                                                    Android.OS.Build.Model),
                                                                            txtVendedor.Text);


                informacoes = CSEmpresa.InformacoesAcesso(query);

                return informacoes;
            }
        }

        private static void UpdateStatus(string statusMessage, int maxProgress, int currentProgress, Bitmap icon)
        {
            CurrentActivity.RunOnUiThread(() =>
            {
                if (statusMessage.StartsWith("Recebendo arquivo... "))
                {
                    Sincronizacao.progressBar.SetTitle("Recebendo arquivo");
                    Sincronizacao.progressBar.SetMessage(statusMessage.Replace("Recebendo arquivo... ", ""));
                }
                else if (statusMessage.StartsWith("Criando tabela "))
                {
                    Sincronizacao.progressBar.SetTitle("Criando tabelas");
                    Sincronizacao.progressBar.SetMessage(statusMessage.Replace("Criando tabela ", ""));
                }
                else if (statusMessage.StartsWith("Processando ") && !statusMessage.Contains("imagens"))
                {
                    Sincronizacao.progressBar.SetTitle("Processando tabelas");
                    Sincronizacao.progressBar.SetMessage(statusMessage.Replace("Processando ", ""));
                }
                else if (statusMessage.StartsWith("Construindo Indice da Tabela "))
                {
                    Sincronizacao.progressBar.SetTitle("Construindo índices das tabelas");
                    Sincronizacao.progressBar.SetMessage(statusMessage.Replace("Construindo Indice da Tabela ", ""));
                }
                else if (statusMessage.StartsWith("Descarga completa"))
                {
                    Sincronizacao.progressBar.Dismiss();
                    MessageBox.Alert(CurrentActivity, statusMessage);
                }
                else if (statusMessage.StartsWith("Executar Carga Parcial"))
                {
                    Sincronizacao.progressBar.SetTitle("Executar Carga Parcial...");
                    Sincronizacao.progressBar.SetMessage(statusMessage.Replace("Executar Carga Parcial...", ""));
                }
                else
                {
                    Sincronizacao.progressBar.SetMessage(statusMessage);
                }

                Sincronizacao.progressBar.Max = maxProgress;
                Sincronizacao.progressBar.Progress = currentProgress;

                if (statusMessage.Contains("Descompactando"))
                    CurrentActivity.RunOnUiThread(() => { Toast.MakeText(CurrentActivity, "A conexão com o servidor já pode ser desfeita.", ToastLength.Long).Show(); });
                //lblMenssagemDesconectar.Visibility = ViewStates.Visible;

                //if (statusMessage.Contains("Completo em"))
                //    MessageBox.Show(CurrentActivity, "Sincronização completa", statusMessage);
            });
        }

        #endregion

        private static void ApagaBancosNaoUtilizados()
        {
            string file = System.IO.Path.Combine(CSGlobal.GetCurrentDirectoryDB(), "AvanteSales.Pro.sdf");

            if (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                }
                catch (System.Exception ex)
                {
                    Sincronizacao.progressBar.Dismiss();
                    MessageBox.AlertErro(CurrentActivity, ex.Message);
                }
            }
        }
    }
}