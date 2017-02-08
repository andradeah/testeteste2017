using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AvanteSales.Pro.Dialogs;
using AvanteSales.Pro.Formatters;
using Java.Lang;
using Master.CompactFramework.Sync;
using Master.CompactFramework.Sync.SQLLiteProvider;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "Main", ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AvanteSalesTheme")]
    public class Main : AppCompatActivity
    {
        Android.Support.V7.Widget.Toolbar tbToolbar;
        TextView lblVendedor;
        TextView lblData;
        TextView lblInformativo;
        Button btnListaCliente;
        Button btnSair;
        Button btnListaRelatorio;
        Button btnSobre;
        Button btnExpediente;
        Button btnDescarga;
        static ProgressDialog progress;
        static AppCompatActivity CurrentActivity;
        private static ISyncProvider syncProvider;
        static string VersaoAvante;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.main);

            FindViewsById();

            Eventos();

            SetSupportActionBar(tbToolbar);

            Inicializacao();
        }

        public override void OnBackPressed()
        {
            Sair();
        }

        private void Sair()
        {
            MessageBox.Alert(this, "Deseja Sair?", "Sair", (_sender, _e) =>
            {
                Intent i = new Intent();
                i.SetClass(this, typeof(ServiceExpediente));
                StopService(i);

                Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
            }, "Cancelar", (_sender, _e) => { }, true);
        }

        private static string DatabaseFilePath
        {
            get
            {
                return Path.Combine(CSGlobal.GetCurrentDirectoryDB(), CSConfiguracao.GetConfig("dbFile") + CSEmpresa.Current.CODIGO_REVENDA + ".sdf");
            }
        }

        private void Eventos()
        {
            btnListaCliente.Click += btnListaCliente_Click;
            btnListaRelatorio.Click += BtnListaRelatorio_Click;
            btnSair.Click += btnSair_Click;
            btnSobre.Click += BtnSobre_Click;
            btnExpediente.Click += BtnListaExpediente_Click;
            btnDescarga.Click += BtnDescarga_Click;
        }

        private void BtnDescarga_Click(object sender, EventArgs e)
        {
            MessageBox.Alert(this, "Deseja efetuar a descarga?", "Descarga", (_sender, _e) => { Descarga(); }, "Cancelar", null, true);
        }

        private void Descarga()
        {
            progress = new ProgressDialogCustomizado(this, LayoutInflater).Customizar();
            progress.Show();

            new ThreadDescarga().Execute();
        }

        private void BtnListaExpediente_Click(object sender, EventArgs e)
        {
            AbrirExpediente();
        }

        private void BtnSobre_Click(object sender, EventArgs e)
        {
            AbrirSobre();
        }

        private void AbrirSobre()
        {
            Intent i = new Intent();
            i.SetClass(this, typeof(Sobre));
            this.StartActivity(i);
        }

        private void BtnListaRelatorio_Click(object sender, EventArgs e)
        {
            AbrirListaRelatorio();
        }

        private void AbrirListaRelatorio()
        {
            Intent i = new Intent();
            i.SetClass(this, typeof(Relatorio));
            this.StartActivity(i);
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            Sair();
        }

        void btnListaCliente_Click(object sender, EventArgs e)
        {
            AbrirListaCliente();
        }

        private void AbrirListaCliente()
        {
            Intent i = new Intent();
            i.SetClass(this, typeof(ListaCliente));
            this.StartActivity(i);
        }

        private void Inicializacao()
        {
            VersaoAvante = PackageManager.GetPackageInfo(PackageName, 0).VersionName;
            CSGlobal.ValidarTopCategoria = CSEmpresa.ColunaExiste("PRODUTO_CATEGORIA", "IND_PROD_TOP_CATEGORIA");
            CSEmpresa.Current.DATA_ULTIMA_DESCARGA = CSDescarga.DataUltimaDescarga;
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            CurrentActivity = this;
            if (!CSEmpregados.Current.IND_VALIDAR_EXPEDIENTE ||
               (CSEmpregados.Current.IND_VALIDAR_EXPEDIENTE &&
               !CSEmpregados.Current.DAT_HORA_INICIO_EXPEDIENTE.HasValue &&
               CSEmpregados.Current.NUM_MINUTOS_TOTAL_EXPEDIENTE == 0))
                btnExpediente.Visibility = ViewStates.Gone;

            lblVendedor.Text = string.Format("{0} - {1}", CSEmpregados.Current.COD_EMPREGADO, CSEmpregados.Current.NOM_EMPREGADO);
            lblData.Text = DateTime.Now.ToString("dd/MM/yy");
            lblInformativo.Text = CSEmpresa.Current.DES_INFORMACAO.Trim();

            if (CSEmpregados.Current.COD_EMPREGADO != CSGlobal.COD_VENDEDOR_DADOS)
                MessageBox.Alert(this, string.Format("Login efetuado com o vendedor {0}-{1}, e base dados com carga do vendedor {2}-{3}", CSEmpregados.Current.COD_EMPREGADO, CSEmpregados.Current.NOM_EMPREGADO, CSGlobal.COD_VENDEDOR_DADOS, CSEmpregados.Items.Cast<CSEmpregados.CSEmpregado>().Where(e => e.COD_EMPREGADO == CSGlobal.COD_VENDEDOR_DADOS).FirstOrDefault().NOM_EMPREGADO));

            if (CSEmpregados.Current.NUM_MINUTOS_TOTAL_EXPEDIENTE > 0 &&
                CSEmpregados.Current.ExpedienteIniciadoNaoFinalizado() &&
                CSEmpregados.Current.IND_FINALIZA_JORNADA_AUTOMATICA)
            {
                Intent i = new Intent();
                i.SetClass(this, typeof(ServiceExpediente));
                StartService(i);
            }
        }
        private void AbrirExpediente()
        {
            Intent i = new Intent();
            i.SetClass(this, typeof(Expediente));
            this.StartActivity(i);
        }

        private void FindViewsById()
        {
            tbToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tbToolbar);
            lblVendedor = FindViewById<TextView>(Resource.Id.lblVendedor);
            lblData = FindViewById<TextView>(Resource.Id.lblData);
            lblInformativo = FindViewById<TextView>(Resource.Id.lblInformativo);
            btnListaCliente = FindViewById<Button>(Resource.Id.btnListaCliente);
            btnSair = FindViewById<Button>(Resource.Id.btnSair);
            btnListaRelatorio = FindViewById<Button>(Resource.Id.btnListaRelatorio);
            btnSobre = FindViewById<Button>(Resource.Id.btnSobre);
            btnExpediente = FindViewById<Button>(Resource.Id.btnExpediente);
            btnDescarga = FindViewById<Button>(Resource.Id.btnDescarga);
        }

        private class ThreadDescarga : AsyncTask
        {
            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                try
                {
                    DateTime data_ultima_sincronizacao;
                    string codigoVendedorRevenda;

                    try
                    {
                        if (CSEmpresa.Current.IND_UTILIZA_FLEXX_GPS == "S")
                        {
                            CSGlobal.LimpaArquivosFlexxGPS("/sdcard/FLAGPS_BD/ENVIADOS", "*.*");
                            CSGlobal.LimpaArquivosFlexxGPS("/sdcard/FLAGPS_BD/ENVIADOS_GPS", "*.*");
                            CSGlobal.LimpaArquivosFlexxGPS("/sdcard/FLAGPS_BD/MENSAGEM/ENVIADOS", "*.*");
                            CSGlobal.LimpaArquivosFlexxGPS("/sdcard/FLAGPS_BD/MENSAGEM/LIDOS", "*.*");
                        }
                    }
                    catch (System.Exception)
                    {
                        MessageBox.AlertErro(CurrentActivity, "Erro ao tentar limpar arquivos enviados para o FlexX GPS. O processo de descarga continuará normalmente.");
                    }

                    if (!AtualizaDataDoSistema())
                    {
                        return 0;
                    }

                    codigoVendedorRevenda = CSConfiguracao.GetConfig("vendedor" + CSGlobal.COD_REVENDA);
                    if (string.IsNullOrEmpty(codigoVendedorRevenda))
                    {
                        codigoVendedorRevenda = CSConfiguracao.GetConfig("vendedorDefault");
                    }

                    if (!ValidaCodigoEmpregado(codigoVendedorRevenda))
                    {
                        return 0;
                    }

                    CSGlobal.COD_REVENDA = CSEmpresa.Current.CODIGO_REVENDA.PadLeft(8,'0');

                    UpdateProvider();

                    syncProvider.DataBaseFilePath = DatabaseFilePath;

                    var dsDescarga = CSDescarga.Descarga(VersaoAvante);

                    var verCompatibilidade = CSEmpresa.Current.VERSAO_AVANTE_SALES_COMPATIBILIDADE;
                    syncProvider.Descarga(dsDescarga, syncProvider.ProviderName, codigoVendedorRevenda, CSGlobal.COD_REVENDA, verCompatibilidade);

                    data_ultima_sincronizacao = DateTime.Now;

                    UpdateDataSincronizacao(data_ultima_sincronizacao, codigoVendedorRevenda);

                    try
                    {
                        string[] recebeFiles = Directory.GetFiles(CSGlobal.GetCurrentDirectory(), "Dados*");
                        DateTime dataCriacao;

                        foreach (string file in recebeFiles)
                        {
                            dataCriacao = File.GetCreationTime(file);

                            if (dataCriacao.Date != DateTime.Now.Date)
                            {
                                try
                                {
                                    File.Delete(file);
                                }
                                catch (System.Exception ex)
                                {
                                    MessageBox.AlertErro(CurrentActivity, ex.Message);
                                }
                            }
                        }
                        MessageBox.Alert(CurrentActivity, "Descarga realizada com sucesso.");
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.AlertErro(CurrentActivity, ex.Message);
                    }
                }
                catch (System.Exception ex)
                {
                    if (!TratamentoErroEmail(ex))
                    {
                        MessageBox.AlertErro(CurrentActivity, ex.Message);
                    }
                    else
                        UpdateDataSincronizacao(DateTime.Now, string.IsNullOrEmpty(CSConfiguracao.GetConfig("vendedor" + CSGlobal.COD_REVENDA)) ? CSConfiguracao.GetConfig("vendedorDefault") : CSConfiguracao.GetConfig("vendedor" + CSGlobal.COD_REVENDA));
                }

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                progress.Dismiss();
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
                        }

                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.AlertErro(CurrentActivity, ex.Message);
                    }
                }
            }

            private static bool TratamentoErroEmail(System.Exception ex)
            {
                if (ex.Message.Contains("E-mail não enviado. Dados incompletos no cadastro Flexx."))
                {
                    MessageBox.AlertErro(CurrentActivity, "Descarga realizada com e-mail não enviado: dados incompletos no cadastro Flexx.");
                    return true;
                }
                else if (ex.Message.Contains("E-mail não enviado. Dados incorretos no cadastro Flexx."))
                {
                    MessageBox.AlertErro(CurrentActivity, "Descarga realizada com e-mail não enviado: dados incorretos no cadastro Flexx.");
                    return true;
                }

                return false;
            }

            private static void UpdateDataSincronizacao(DateTime data_ultima_sincronizacao, string codigoVendedorRevenda)
            {
                CSDataAccess.Instance.ExecuteScalar("UPDATE INFORMACOES_SINCRONIZACAO SET DATA_ULTIMA_SINCRONIZACAO = datetime('" + data_ultima_sincronizacao.ToString("yyyy-MM-dd HH:mm:ss") +
                                                    "') WHERE COD_EMPREGADO = " + codigoVendedorRevenda);

                CSEmpresa.Current.DATA_ULTIMA_DESCARGA = data_ultima_sincronizacao;

                if (CSEmpresa.ColunaExiste("INDENIZACAO", "COD_INDENIZACAO"))
                    CSDataAccess.Instance.ExecuteNonQuery("UPDATE INDENIZACAO SET IND_DESCARREGADO = 1");
            }
        }

        private static bool ValidaCodigoEmpregado(string strCodigoEmpregado)
        {
            try
            {
                int codigoEmpregado = 0;
                if (int.TryParse(strCodigoEmpregado, out codigoEmpregado))
                {
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
            catch (System.Exception ex)
            {
                MessageBox.AlertErro(CurrentActivity, "Empregado Inválido");
                return false;
            }
        }

        private static string Descriptografar(string conteudoCriptografado)
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

        private static bool AtualizaDataDoSistema()
        {
            // Altera a data do sistema
            try
            {
                WebService.AvanteSales ws = new WebService.AvanteSales();

                ws.Url = ws.Url.Replace("localhost", Descriptografar(CSConfiguracao.GetConfig("internetURL")));
                ws.Timeout = 15000;
                DateTime d = ws.GetServerDate();

                // Funcao para alterar a data
                CSGlobal.MudaData(d);

                return true;

            }
            catch (System.Exception ex)
            {
                MessageBox.AlertErro(CurrentActivity, "Falha na conexão");
                return false;
            }
        }
    }
}