using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AvanteSales.Pro.Dialogs;
using AvanteSales.Pro.Controles;
using AvanteSales.Pro.Formatters;
using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
using SQLiteCommand = Mono.Data.Sqlite.SqliteCommand;
using SQLiteDataAdapter = Mono.Data.Sqlite.SqliteDataAdapter;
using SQLiteException = Mono.Data.Sqlite.SqliteException;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteTransaction = Mono.Data.Sqlite.SqliteTransaction;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;
using AvanteSales.SystemFramework;
using System.Globalization;
using System.Runtime.InteropServices;
using Android.Support.V7.App;
using Android.Support.Design.Widget;
using System.Threading;
using System.Net;
using System.IO;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "Avante Sales", ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AvanteSalesTheme")]
    public class Login : Activity, TextView.IOnEditorActionListener
    {
        private static bool isLoadingEmpresas;
        private static bool CarregandoInformacoes;
        private static bool LoginClick;

        static Button btnSair;
        static Button btnSincronizar;
        static Button btnLogin;
        static Spinner spnEmpresa;
        static Spinner spnEmpregado;
        static EditText txtUsuario;
        static EditText txtSenha;
        static CustomDatePicker txtDataVisita;
        static CustomDatePicker txtDataEntrega;
        TextView lblVersao;
        TextInputLayout tilUsuario;
        TextInputLayout tilDataEntrega;
        TextInputLayout tilDataVisita;
        TextInputLayout tilSenha;
        static ProgressDialog progress;
        private static Activity CurrentActivity;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CarregandoInformacoes = true;
            CSGlobal.Context = this.Application.ApplicationContext;
            SetContentView(Resource.Layout.login);
            CurrentActivity = this;
            FindViewsById();
            Eventos();

            // Muda a cultura do sistema pra portugues...
            CSGlobal.ChangeCultureInfo(new CultureInfo("pt-BR"));

            string dataAtual = DateTime.Now.ToString("dd/MM/yyyy");
            txtDataVisita.Text = dataAtual;
            txtDataEntrega.Text = dataAtual;

            //Apenas enquanto é versão em desenvolvimento.
#if DEBUG
            txtUsuario.Text = "flag";
            txtSenha.Text = "fl@g2014";
#endif
            Versao();
            UpdateData();
        }

        private void Eventos()
        {
            btnSair.Click += btnSair_Click;
            btnSincronizar.Click += btnSincronizar_Click;
            btnLogin.Click += btnLogin_Click;
            txtSenha.SetOnEditorActionListener(this);
            spnEmpresa.ItemSelected += SpnEmpresa_ItemSelected;
        }

        private void SpnEmpresa_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (spnEmpresa.SelectedItem != null)
                CSGlobal.COD_REVENDA = spnEmpresa.SelectedItem.ToString().Substring(0, 8);

            if (!CarregandoInformacoes)
            {
                CSEmpresa.Current = null;
                CarregandoInformacoes = true;
                UpdateData();
            }
        }

        void btnLogin_Click(object sender, EventArgs e)
        {
            EfetuarLogin();
        }

        private void LimparErros()
        {
            tilUsuario.Error = null;
            tilUsuario.ErrorEnabled = false;

            tilSenha.Error = null;
            tilSenha.ErrorEnabled = false;

            tilDataEntrega.Error = null;
            tilDataEntrega.ErrorEnabled = false;

            tilDataVisita.Error = null;
            tilDataVisita.ErrorEnabled = false;
        }

        private void EfetuarLogin()
        {
            LimparErros();

            if (!VerificaDados())
                return;

            this.HideKeyboard(txtSenha);

            // verifica se a data foi alterada...
            ValidaDataVisita();
        }

        void btnSincronizar_Click(object sender, EventArgs e)
        {
            AbrirSincronizacao();
        }

        private bool VerificaDados()
        {
            DateTime datEntrega;
            DateTime datVisita;

            if (spnEmpresa.Adapter.Count == 0 || spnEmpresa.SelectedItem.ToString().Length == 0)
            {
                LoginClick = false;
                MessageBox.AlertErro(this, "Informe a empresa para logar no sistema.");
                spnEmpresa.RequestFocus();
                return false;
            }

            if (txtUsuario.Text.Length == 0)
            {
                LoginClick = false;
                CSGlobal.Focus(this, txtUsuario);
                FuncoesView.SetarLabelErroControles(this, tilUsuario, "Informe o usuário.");
                return false;
            }

            if (spnEmpresa.SelectedItemPosition == -1)
            {
                LoginClick = false;
                MessageBox.AlertErro(this, "Selecione o empregado.");
                spnEmpresa.RequestFocus();
                return false;
            }

            datVisita = DateTime.Parse(txtDataVisita.Text);
            datEntrega = DateTime.Parse(txtDataEntrega.Text);

            //Valida Data de Entrega
            if (DateTime.Compare(datEntrega, datVisita) < 0)
            {
                LoginClick = false;
                string mensagem = string.Format("A data de entrega: {0} não pode ser menor que a data de visita: {1}.", datEntrega.ToString("dd/MM/yyyy"), datVisita.ToString("dd/MM/yyyy"));
                CSGlobal.Focus(this, txtDataEntrega);
                FuncoesView.SetarLabelErroControles(this, tilDataEntrega, mensagem);

                return false;
            }

            //Atualiza a Data de Entrega da Empresa
            CSEmpresa.Current.DATA_ENTREGA = datEntrega;

            return true;
        }

        private void ValidaDataVisita()
        {
            DateTime dataVisita;
            try
            {
                dataVisita = DateTime.Parse(txtDataVisita.Text);
                if (DateTime.Now.Date != dataVisita)
                {
                    string mensagem = string.Format("Deseja alterar a data do dia de visita de {0} para {1}?", DateTime.Now.ToString("dd/MMM/yyyy"), dataVisita.ToString("dd/MMM/yyyy"));
                    MessageBox.Alert(this, mensagem, "Alterar", (_sender, _e) => { MudarDataDiaVisita_Click_Yes(); }, "Não", null, true);
                }
                else
                {
                    //Continua valicação com o usuário.
                    ValidaUsuario();
                }
            }
            catch (FormatException)
            {
                CSGlobal.Focus(this, txtDataVisita);
                FuncoesView.SetarLabelErroControles(this, tilDataVisita, "A data informada não foi reconhecida como um valor válido de data.");
            }
        }

        protected void MudarDataDiaVisita_Click_Yes()
        {
            DateTime dataVisita = DateTime.Parse(txtDataVisita.Text);

            if (CSPedidosPDV.ExistePedidoDataEspecifica(dataVisita))
            {
                StringBuilder mensagem = new StringBuilder();
                mensagem.AppendFormat("Já existem pedidos com data superior a {0}", dataVisita.ToString("dd/MM/yyyy"));
                mensagem.AppendLine();
                mensagem.AppendFormat("Confirma mudar a data do dia de visita de {0} para {1}?", DateTime.Now.ToString("dd/MMM/yyyy"), dataVisita.ToString("dd/MMM/yyyy"));
                LoginClick = false;
                // Chama a rotina que altera a data caso responda que sim
                MessageBox.Alert(this, mensagem.ToString(), "Alterar", (_sender, _e) => { MudaDataEValidaUsuario(dataVisita); }, "Não", null, true);
            }
            else
            {
                MudaDataEValidaUsuario(dataVisita);
            }
        }

        private void MudaDataEValidaUsuario(DateTime dataVisita)
        {
            // Chama a rotina que altera a data
            CSGlobal.MudaData(dataVisita);

            //Continua valicação com o usuário.
            ValidaUsuario();
        }

        private void ValidaUsuario()
        {
            if (CSUsuario.ValidaUsuario(txtUsuario.Text.Trim(), txtSenha.Text.Trim(), ((CSEmpregados.CSEmpregado)((CSItemCombo)spnEmpregado.SelectedItem).Valor).COD_EMPREGADO) == false)
            {
                LoginClick = false;
                CSGlobal.Focus(this, txtSenha);
                FuncoesView.SetarLabelErroControles(this, tilSenha, "Usuário ou senha inválidos.");
            }
            else
                ValidaEmpregado();
        }

        private void ValidaEmpregado()
        {
            if (CSEmpregados.ValidaEmpregado((int)((CSEmpregados.CSEmpregado)((CSItemCombo)spnEmpregado.SelectedItem).Valor).COD_EMPREGADO))
            {
                // Guarda o empregado atual
                CSEmpregados.Current = (CSEmpregados.CSEmpregado)((CSItemCombo)spnEmpregado.SelectedItem).Valor;

                // FlexX GPS
                if (!flexxGPSInstalado())
                {
                    LoginClick = false;
                    MessageBox.AlertErro(this, "Login bloqueado: o FlexxGPS não está instalado.");
                    return;
                }

                flexXGPS();

                // [ Cria tabelas auxiliares do sistema ]
                CSGlobal.CriaTabelasAuxiliares();

                if (!CSEmpresa.ColunaExiste("INFORMACOES_SINCRONIZACAO", "VERSAO_LOJA"))
                    AbrirMain();
                else
                {
                    progress = new ProgressDialogCustomizado(this, LayoutInflater).Customizar();
                    progress.Show();

                    ThreadPool.QueueUserWorkItem(o => ThreadVerificarVersao());
                }
            }
            else
            {
                LoginClick = false;
                throw new Exception("Não foi possivel encontrar informações sobre o empregado.");
            }
        }

        public void AbrirMain()
        {
            LoginClick = false;
            Intent i = new Intent();
            i.SetClass(this, typeof(Main));
            this.StartActivity(i);
            this.Finish();
        }

        private void ThreadVerificarVersao()
        {
            try
            {
                var versaoLoja = RetornarVersaoLoja();

                if (string.IsNullOrEmpty(versaoLoja))
                    AbrirMain();
                else
                {
                    var versaoDetalhadaLoja = versaoLoja.Split(".".ToCharArray());
                    int valorVersaoLojaConvertido = (Convert.ToInt32(versaoDetalhadaLoja[0]) * 1000000) +
                                                (Convert.ToInt32(versaoDetalhadaLoja[1]) * 10000) +
                                                (Convert.ToInt32(versaoDetalhadaLoja[2]) * 100) +
                                                (Convert.ToInt32(versaoDetalhadaLoja[3]) * 1);

                    var versaoDetalhadaAPK = this.PackageManager.GetPackageInfo(this.PackageName, 0).VersionName.Split(".".ToCharArray());
                    int valorVersaoAPKConvertido = (Convert.ToInt32(versaoDetalhadaAPK[0]) * 1000000) +
                                                (Convert.ToInt32(versaoDetalhadaAPK[1]) * 10000) +
                                                (Convert.ToInt32(versaoDetalhadaAPK[2]) * 100) +
                                                (Convert.ToInt32(versaoDetalhadaAPK[3]) * 1);


                    if (valorVersaoLojaConvertido > valorVersaoAPKConvertido)
                    {
                        MessageBox.Alert(this, string.Format("Esta versão {0} está diferente a última publicada {1}. Entrar em contato com o T.I da sua empresa.",
                            this.PackageManager.GetPackageInfo(this.PackageName, 0).VersionName,
                            versaoLoja), "Ok",
                            (_sender, _e) => { AbrirMain(); }, true);
                    }
                    else
                        AbrirMain();
                }
            }
            catch (Exception ex)
            {
                AbrirMain();
                LoginClick = false;
            }
        }

        private string RetornarVersaoLoja()
        {
            try
            {
                string versao = string.Empty;

                string select = "SELECT VERSAO_LOJA FROM INFORMACOES_SINCRONIZACAO";

                var resultadoSelect = CSDataAccess.Instance.ExecuteScalar(select);

                if (resultadoSelect == System.DBNull.Value)
                {
                    string versaoAtual = RetornarVersaoLojaAtual();
                    versao = versaoAtual;

                    if (!string.IsNullOrEmpty(versaoAtual))
                    {
                        string insert = string.Format("UPDATE INFORMACOES_SINCRONIZACAO SET VERSAO_LOJA = '{0}'", versaoAtual);
                        CSDataAccess.Instance.ExecuteNonQuery(insert);
                    }
                }
                else
                    versao = resultadoSelect.ToString();

                return versao;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private string RetornarVersaoLojaAtual()
        {
            try
            {
                string urlConexao;

                urlConexao = "https://play.google.com/store/apps/details?id=AvanteSales.Pro.AvanteSales.Pro";

                var request = (HttpWebRequest)WebRequest.Create(urlConexao);
                request.Timeout = 3000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);

                var html = reader.ReadToEnd();

                int positionVersao = html.IndexOf("Version");

                var tagVersao = html.Substring(positionVersao, 21);

                int positionVersaoNumerica = tagVersao.IndexOf('>');

                var versao = tagVersao.Substring(positionVersaoNumerica + 1, tagVersao.Length - (positionVersaoNumerica + 1)).Replace("<", string.Empty).Replace(" ", string.Empty);

                return versao;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void flexXGPS()
        {
            if (CSEmpresa.Current.IND_UTILIZA_FLEXX_GPS.ToUpper() == "S")
            {
                try
                {
                    CSGlobal.criaDiretoriosFlexxGPS();
                    CSGlobal.GetCriaArquivoConfigFlexXGPS(this);
                }
                catch (Exception ex)
                {
#if DEBUG
                    MessageBox.Alert(this, ex.Message, "Ok", (sender, e) => { flexXGPS(); }, false);
#endif
                }
            }
        }

        private bool flexxGPSInstalado()
        {
            bool AppInstalado;

            try
            {
                if (CSEmpresa.Current.IND_UTILIZA_FLEXX_GPS.ToUpper() == "S" &&
                    CSEmpresa.ColunaExiste("EMPREGADO", "IND_UTILIZA_FLEXXGPS"))
                {
                    if (CSEmpregados.Current.IND_VALIDA_FLEXXGPS_INSTALADO)
                    {
                        PackageManager pm = PackageManager;

                        pm.GetPackageInfo("flag.flexxgps.initialize", PackageInfoFlags.Activities);
                        AppInstalado = true;
                    }
                    else
                        AppInstalado = true;
                }
                else
                    AppInstalado = true;
            }
            catch (PackageManager.NameNotFoundException e)
            {
                AppInstalado = false;
            }
            catch (Exception ex)
            {
                throw new Exception("Favor relatar o problema ao Help Desk Flag");
            }

            return AppInstalado;
        }

        private static void AbrirSincronizacao()
        {
            Intent i = new Intent();
            i.SetClass(CurrentActivity, (typeof(Sincronizacao)));
            CurrentActivity.StartActivity(i);
            CurrentActivity.Finish();
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
            btnSair = FindViewById<Button>(Resource.Id.btnSair);
            btnSincronizar = FindViewById<Button>(Resource.Id.btnSincronizar);
            btnLogin = FindViewById<Button>(Resource.Id.btnLogin);
            spnEmpresa = FindViewById<Spinner>(Resource.Id.spnEmpresa);
            spnEmpregado = FindViewById<Spinner>(Resource.Id.spnEmpregado);
            txtUsuario = FindViewById<EditText>(Resource.Id.txtUsuario);
            txtDataVisita = FindViewById<CustomDatePicker>(Resource.Id.txtDataVisita);
            txtDataEntrega = FindViewById<CustomDatePicker>(Resource.Id.txtDataEntrega);
            txtSenha = FindViewById<EditText>(Resource.Id.txtSenha);
            lblVersao = FindViewById<TextView>(Resource.Id.lblVersao);
            tilUsuario = FindViewById<TextInputLayout>(Resource.Id.tilUsuario);
            tilDataEntrega = FindViewById<TextInputLayout>(Resource.Id.tilDataEntrega);
            tilDataVisita = FindViewById<TextInputLayout>(Resource.Id.tilDataVisita);
            tilSenha = FindViewById<TextInputLayout>(Resource.Id.tilSenha);
        }

        private void Versao()
        {
            lblVersao.Text = string.Format("v{0}", PackageManager.GetPackageInfo(PackageName, 0).VersionName);
        }

        public override void OnBackPressed()
        {
            MessageBox.Alert(this, "Deseja Sair?", "Sair", (_sender, _e) => { Sair(); }, "Cancelar", (_sender, _e) => { }, true);
        }

        private static void DataEntregaPrice()
        {
            if (CSEmpresa.ColunaExiste("EMPRESA", "DATA_ENTREGA_PRICE"))
            {
                string query = "SELECT DATA_ENTREGA_PRICE FROM EMPRESA";

                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(query))
                {
                    if (sqlReader.Read())
                    {
                        DateTime? data = null;

                        if (sqlReader.GetValue(0) != DBNull.Value &&
                            sqlReader.GetDateTime(0).ToString("dd/MM/yyyy") != "01/01/1900")
                        {
                            data = sqlReader.GetDateTime(0);
                            txtDataEntrega.Text = data.Value.ToString("dd/MM/yyyy");
                            txtDataEntrega.Enabled = false;
                        }
                        else
                        {
                            txtDataEntrega.Text = DateTime.Now.ToString("dd/MM/yyyy");
                            txtDataEntrega.Enabled = true;
                        }
                    }
                }
            }
        }

        public void UpdateData()
        {
            progress = new ProgressDialogCustomizado(this, LayoutInflater).Customizar();
            progress.Show();

            new ThreadBuscarEmpresa().Execute();
        }

        private static void VerificarBancoExistente()
        {
            if (!CSDataAccess.DataBaseExists(CSGlobal.COD_REVENDA) ||
                                            CSConfiguracao.GetConfig("ATUALIZADO_" + CSGlobal.COD_REVENDA) != CSGlobal.STATUS_ATUALIZACAO.SUCESSO)
            {
                // Chama a tela de sincronização
                AbrirSincronizacao();
            }
            else
            {
                DataEntregaPrice();
                DataVisita();

                txtUsuario.RequestFocus();
            }
        }

        private class ThreadBuscarEmpresa : AsyncTask
        {
            // [ Carrega dados ]            
            ArrayAdapter adapter = new ArrayAdapter(CurrentActivity, Android.Resource.Layout.SimpleSpinnerItem, CSConfiguracao.GetEmpresasQuePossuemBancoNoAparelho());
            int PositionSelect;
            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                PositionSelect = 0;
                CarregaComboBoxEmpresa();

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                if ((bool)result)
                {
                    adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                    spnEmpresa.Adapter = adapter;

                    if (spnEmpresa.Adapter != null)
                    {
                        // [ Carrega combo de empregados se houverem empresas ]
                        if (spnEmpresa.Adapter.Count > 0 && CSDataAccess.DataBasesExists() && spnEmpresa.SelectedItem != null)
                        {
                            CSDataAccess.Instance.FechaConexao();
                            CSDataAccess.Instance.AbreConexao();

                            new ThreadBuscarEmpregado().Execute();
                        }
                        else
                        {
                            VerificarBancoExistente();

                            if (progress != null)
                                progress.Dismiss();

                            CarregandoInformacoes = false;
                        }
                    }
                    else
                    {
                        VerificarBancoExistente();

                        if (progress != null)
                            progress.Dismiss();

                        CarregandoInformacoes = false;
                    }

                    spnEmpresa.SetSelection(PositionSelect);
                }
            }

            private void CarregaComboBoxEmpresa()
            {
                isLoadingEmpresas = true;

                string empresaAtual = null;

                // [ Armazena a seleção atual do combo ]
                if (spnEmpresa.Adapter != null)
                {
                    if (spnEmpresa.Adapter.Count > 0)
                        empresaAtual = (string)spnEmpresa.SelectedItem;
                    else
                        empresaAtual = null;
                }

                // [ Se existe banco de dados e está atualizado... ]
                if (empresaAtual != null &&
                    CSDataAccess.DataBaseExists(empresaAtual.Substring(0, 8)) &&
                    CSConfiguracao.GetConfig("ATUALIZADO_" + empresaAtual.Substring(0, 8)) == CSGlobal.STATUS_ATUALIZACAO.SUCESSO)
                {
                    CSGlobal.COD_REVENDA = empresaAtual.Substring(0, 8);
                    for (int i = 0; i < spnEmpresa.Adapter.Count; i++)
                    {
                        if (empresaAtual == spnEmpresa.Adapter.GetItem(i).ToString())
                        {
                            PositionSelect = i;

                            break;
                        }
                    }

                }
                else
                {
                    if (spnEmpresa.Adapter != null)
                    {
                        // [ Seleciona a primeira empresa no qual o banco de dados existe ]
                        for (int index = 0; index < spnEmpresa.Adapter.Count; index++)
                        {
                            // [ Atualiza o código da revenda ]
                            CSGlobal.COD_REVENDA = spnEmpresa.Adapter.GetItem(index).ToString().Substring(0, 8);

                            // [ Verifica se o banco existe e está atualizado ]
                            if (CSDataAccess.DataBaseExists(CSGlobal.COD_REVENDA) &&
                                CSConfiguracao.GetConfig("ATUALIZADO_" + CSGlobal.COD_REVENDA) == CSGlobal.STATUS_ATUALIZACAO.SUCESSO)
                            {
                                PositionSelect = index;

                                break;
                            }
                        }
                    }
                }

                isLoadingEmpresas = false;
            }
        }

        private class ThreadBuscarEmpregado : AsyncTask
        {
            ArrayAdapter adapter = new ArrayAdapter(CurrentActivity, Android.Resource.Layout.SimpleSpinnerItem);
            int indexEmpregadoRevenda = -1;

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                CarregaComboBoxEmpregado();

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                spnEmpregado.Adapter = adapter;

                // [ Se encontrou empregados... ]
                if (indexEmpregadoRevenda != -1)
                    spnEmpregado.SetSelection(indexEmpregadoRevenda);

                VerificarBancoExistente();

                if (progress != null)
                    progress.Dismiss();

                CarregandoInformacoes = false;
            }

            private void CarregaComboBoxEmpregado()
            {
                try
                { 
                    var codigoVendedorRevenda = CSConfiguracao.GetConfig("vendedor" + CSGlobal.COD_REVENDA);
                    if (string.IsNullOrEmpty(codigoVendedorRevenda))
                    {
                        codigoVendedorRevenda = CSConfiguracao.GetConfig("vendedorDefault");
                    }
                    int codigoEmpregadoRevenda = CSGlobal.StrToInt(codigoVendedorRevenda);

                    CSEmpregados.Items.Clear();
                    CSGlobal.COD_VENDEDOR_DADOS = codigoEmpregadoRevenda;
                    adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

                    int i = 0;
                    // [ Preenche o combo de empregados ]
                    foreach (CSEmpregados.CSEmpregado empregado in CSEmpregados.Items)
                    {
                        string codigo, nome;

                        codigo = empregado.COD_EMPREGADO.ToString();
                        nome = empregado.NOM_EMPREGADO;

                        CSItemCombo ic = new CSItemCombo();
                        ic.Texto = string.Concat(codigo, " ", nome);
                        ic.Valor = empregado;

                        if ((CSEmpresa.Current.IND_ALTERA_VENDEDOR.Trim() == "N" && empregado.COD_EMPREGADO == codigoEmpregadoRevenda) ||
                            CSEmpresa.Current.IND_ALTERA_VENDEDOR.Trim() == "S")
                        {
                            adapter.Add(ic);
                            i++;
                        }

                        if (empregado.COD_EMPREGADO == codigoEmpregadoRevenda)
                            indexEmpregadoRevenda = i - 1;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private static void DataVisita()
        {
            if (CSEmpresa.ColunaExiste("EMPRESA", "IND_BLOQUEAR_DATA_VISITA"))
            {
                string query = "SELECT IND_BLOQUEAR_DATA_VISITA FROM EMPRESA";

                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(query))
                {
                    if (sqlReader.Read())
                    {
                        if (sqlReader.GetValue(0) != DBNull.Value &&
                            sqlReader.GetString(0).ToUpper() == "S")
                            txtDataVisita.Enabled = false;
                        else
                            txtDataVisita.Enabled = true;
                    }
                }
            }
        }

        protected void UpdateData_Click_Yes(object sender, DialogClickEventArgs e)
        {
            CSConfiguracao.SetConfig("StoredCard", (1 == 2).ToString());
            // Chama a tela de sincronização
            AbrirSincronizacao();
        }

        protected void UpdateData_Click_No(object sender, DialogClickEventArgs e)
        {
            CSDataAccess.Instance.FechaConexao();
            Sair();
        }

        #region IOnEditorActionListener Members

        public bool OnEditorAction(TextView v, Android.Views.InputMethods.ImeAction actionId, KeyEvent e)
        {
            if (v == txtSenha &&
                actionId == Android.Views.InputMethods.ImeAction.Go)
            {
                EfetuarLogin();
            }

            return true;
        }

        #endregion
    }
}