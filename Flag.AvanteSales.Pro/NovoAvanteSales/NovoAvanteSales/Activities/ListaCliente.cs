using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AvanteSales.Pro.Dialogs;
using AvanteSales.Pro.Formatters;
using AvanteSales.SystemFramework.BusinessLayer;
using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
using SQLiteCommand = Mono.Data.Sqlite.SqliteCommand;
using SQLiteDataAdapter = Mono.Data.Sqlite.SqliteDataAdapter;
using SQLiteException = Mono.Data.Sqlite.SqliteException;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteTransaction = Mono.Data.Sqlite.SqliteTransaction;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;
using AvanteSales.BusinessRules;
using Java.Lang;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "Cliente", ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AvanteSalesTheme", WindowSoftInputMode = SoftInput.AdjustNothing, ParentActivity = typeof(Main))]
    public class ListaCliente : AppCompatActivity
    {
        static Android.Support.V4.App.FragmentManager thisManager;
        const int frmPesquisaMercado = 3;
        const int frmCliente = 1;
        const int frmDialogCliente = 2;
        static ListView listPdvs;
        Android.Support.V7.Widget.Toolbar tbToolbar;
        Button btnDiaSemana;
        EditText txtPesquisa;
        TextView lblMeta;
        TextView lblPedidos;
        TextView lblSkus;
        public static bool SituacaoFinanceiraClick;
        public static bool InformacoesPdvClick;
        public static bool MetaVendaClick;
        const int frmFotoMotivoNaoCompra = 5;
        const int frmFotoPedido = 6;
        private bool Reagendamento;
        private static int DiaDaSemanaSelecionado;
        private static ProgressDialog progressDialog;
        private static ProgressDialog progressVisita;
        static List<CSVisitas.CSVisita> Visitas;
        static AppCompatActivity CurrentActivity;
        static List<CSPDVs.CSPDV> AdapterAtual;
        public static bool ExibirNomeFantasia { get; set; }
        private static IList<CSPDVs.CSPDV> PDVs
        {
            set
            {
                if (Visitas == null)
                    Visitas = CSEmpregados.Current.VISITAS_EMPREGADO.Cast<CSVisitas.CSVisita>().ToList();

                AdapterAtual = value.ToList();

                CurrentActivity.RunOnUiThread(() =>
                {
                    listPdvs.Adapter = new LitemItemAdapter(CurrentActivity, Resource.Layout.lista_cliente_row, value, ExibirNomeFantasia);
                });
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    base.OnBackPressed();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private static void PdvVisitado()
        {
            if (!CSEmpresa.Current.IND_EMPRESA_FERIADO &&
                 CSEmpregados.Current.IND_ORDEM_VISITA_OBRIGATORIA &&
                 CSPDVs.Current.PDVDentroRota())
                CSEmpregados.Current.VISITAS_EMPREGADO.Cast<CSVisitas.CSVisita>().Where(v => v.COD_PDV == CSPDVs.Current.COD_PDV).FirstOrDefault().SetPdvVisitado();
        }

        public void SetPositivacao(Intent data)
        {
            PdvVisitado();

            CSPDVs.Current.PEDIDOS_PDV = null;
            CSPDVs.Current = null;

            if (data == null || listPdvs.Adapter == null || listPdvs.Adapter.Count == 0)
                return;

            try
            {
                int codPDV = data.GetIntExtra("cod_pdv", -1);
                double vlr_venda = data.GetDoubleExtra("vlr_venda", 0);

                if (codPDV == -1)
                {
                    MessageBox.ShowShortMessageCenter(this, "Não foi possível alterar o status de positivação do pdv!");
                    return;
                }

                var indexPDV = ((LitemItemAdapter)listPdvs.Adapter).GetIndex(codPDV);
                if (indexPDV != -1)
                {
                    var pdv = ((LitemItemAdapter)listPdvs.Adapter).GetItem(indexPDV);

                    int first = listPdvs.FirstVisiblePosition;

                    View linhaASerAtualizada = listPdvs.GetChildAt(indexPDV - first);

                    if (linhaASerAtualizada == null)
                        return;

                    TextView liRazaoSocial = linhaASerAtualizada.FindViewById<TextView>(Resource.Id.liRazaoSocial);
                    Button btnTotalVendas = linhaASerAtualizada.FindViewById<Button>(Resource.Id.btnTotalVendas);

                    if (vlr_venda == 0)
                        btnTotalVendas.Text = "-";
                    else
                        btnTotalVendas.Text = string.Format("R$: {0}", vlr_venda.ToString("0.00"));

                    if (pdv.IND_POSITIVADO)
                    {
                        liRazaoSocial.SetTextColor(Color.ParseColor("#03a9f4"));
                        liRazaoSocial.SetTypeface(null, TypefaceStyle.Bold);
                    }
                    else
                    {
                        int qtdHistoricoNaoPositivado = pdv.HISTORICOS_MOTIVO.Items.Count;

                        if (qtdHistoricoNaoPositivado == 0)
                        {
                            liRazaoSocial.SetTextColor(Color.ParseColor("#03a9f4"));
                            liRazaoSocial.SetTypeface(null, TypefaceStyle.Normal);
                        }
                        else
                        {
                            liRazaoSocial.SetTextColor(Color.Red);
                            liRazaoSocial.SetTypeface(null, TypefaceStyle.Normal);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                MessageBox.AlertErro(this, ex.Message);
#endif
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            switch (requestCode)
            {
                case frmCliente:
                    SetPositivacao(data);
                    break;
                case frmDialogCliente:
                    {
                        if (resultCode == Result.FirstUser)
                        {
                            CSPDVs.Current.PEDIDOS_PDV = null;
                            ValidarExpedienteAlmoco();
                        }
                    }
                    break;
                case frmFotoPedido:
                    if (resultCode == Result.Ok)
                    {
                        AbrirCliente(false);
                    }
                    break;
                case frmFotoMotivoNaoCompra:
                    if (resultCode == Result.Ok)
                    {
                        AbrirCliente(true);
                    }
                    break;
                default:
                    break;
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        private void ValidarExpedienteAlmoco()
        {
            if (!CSEmpresa.Current.IND_EMPRESA_FERIADO &&
                CSPDVs.Current.PDVDentroRota() &&
                CSEmpregados.Current.IND_ORDEM_VISITA_OBRIGATORIA)
            {
                var visitas = CSEmpregados.Current.VISITAS_EMPREGADO.Cast<CSVisitas.CSVisita>().ToList();

                var visitaPdv = visitas.Where(v => v.COD_PDV == CSPDVs.Current.COD_PDV).FirstOrDefault();

                if (!visitaPdv.PdvAteriorVisitado(visitas))
                {
                    MessageBox.Alert(this, "Você não visitou o PDV da ordem de visita anterior. Faça o reagendamento ou visite o cliente anterior.");
                    return;
                }
            }

            string alerta = string.Empty;

            if (!CSEmpregados.Current.VendedorDentroExpediente(ref alerta))
            {
                MessageBox.Alert(this, alerta);
                return;
            }
            else if (CSEmpregados.Current.AlmocoIniciadoNaoFinalizado())
            {
                if (CSEmpregados.Current.NUM_MINUTOS_INTERVALO_ALMOCO > 0)
                {
                    if (CSEmpregados.Current.IntervaloAlmocoEncerrado())
                    {
                        CSEmpregados.Current.FinalizarAlmoco();

                        CSPDVs.Current.PEDIDOS_PDV = null;
                        ValidacoesPreVenda();
                    }
                    else
                        MessageBox.Alert(this, string.Format("Não é possível realizar venda dentro do horário de almoco. Falta(m) aproximadamente {0} minuto(s) para o encerramento do seu horário de almoço.", CSEmpregados.Current.TempoAlmocoRestante()));
                }
                else
                {
                    MessageBox.Alert(this, "Você não pode realizar venda dentro do horário de almoço. Deseja informar fim do horário de almoço?", "Finalizar almoço",
                        (_sender, _e) =>
                        {
                            CSEmpregados.Current.FinalizarAlmoco();

                            CSPDVs.Current.PEDIDOS_PDV = null;
                            ValidacoesPreVenda();
                        }
                    , "Cancelar", (_sender, _e) =>
                    {

                    }, true);
                }
            }
            else
            {
                ValidacoesPreVenda();
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SituacaoFinanceiraClick = false;
                MetaVendaClick = false;
                SetContentView(Resource.Layout.lista_cliente);
                thisManager = SupportFragmentManager;

                FindViewsById();

                SetSupportActionBar(tbToolbar);

                Eventos();

                Inicializacao();
            }
            catch (System.Exception ex)
            {

            }
        }

        private void Inicializacao()
        {
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(false);

            lblMeta.Text = CSEmpregados.Current.VLR_META_EMPREGADO == 0 ? "-" : CSEmpregados.Current.VLR_META_EMPREGADO.ToString(".00");
            lblPedidos.Text = CSEmpregados.Current.NUM_PEDIDOS_DIA.ToString();
            lblSkus.Text = CSEmpregados.Current.NUM_SKUS_DIA.ToString();
            CurrentActivity = this;
            Reagendamento = false;
            CSPDVs.Current = null;
            DiaDaSemanaSelecionado = (int)DateTime.Now.DayOfWeek;
            
            //if (CSEmpresa.Current.IND_PERMITIR_VENDA_FORA_ROTA.ToUpper() == "N" ||
            //    CSEmpregados.Current.IND_BLOQUEADO_VENDA_FORA_ROTA)
            //    btnDiaSemana.Enabled = false;

            PreencheListPDVs();
        }

        private void Eventos()
        {
            listPdvs.ItemClick += listPdvs_ItemClick;
            listPdvs.ItemLongClick += listPdvs_ItemLongClick;
            btnDiaSemana.Click += btnDiaSemana_Click;
            txtPesquisa.TextChanged += txtPesquisa_TextChanged;
        }

        private void listPdvs_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            string alerta = string.Empty;

            if (!CSEmpregados.Current.VendedorDentroExpediente(ref alerta))
            {
                MessageBox.Alert(this, alerta);
                return;
            }
            else if (CSEmpregados.Current.AlmocoIniciadoNaoFinalizado())
            {
                if (CSEmpregados.Current.NUM_MINUTOS_INTERVALO_ALMOCO > 0)
                {
                    if (CSEmpregados.Current.IntervaloAlmocoEncerrado())
                    {
                        CSEmpregados.Current.FinalizarAlmoco();
                    }
                    else
                        MessageBox.Alert(this, string.Format("Não é possível realizar reagendamento dentro do horário de almoco. Falta(m) aproximadamente {0} minuto(s) para o encerramento do seu horário de almoço.", CSEmpregados.Current.TempoAlmocoRestante()));
                }
                else
                {
                    MessageBox.Alert(this, "Você não pode realizar venda dentro do horário de almoço. Deseja informar fim do horário de almoço?", "Finalizar almoço",
                        (_sender, _e) =>
                        {
                            CSEmpregados.Current.FinalizarAlmoco();
                        }
                    , "Cancelar", (_sender, _e) =>
                    {

                    }, true);
                }
            }
            else
            {
                CSPDVs.Current = (CSPDVs.CSPDV)listPdvs.Adapter.GetItem(e.Position);
                CSPDVs.Current.PEDIDOS_PDV = null;
                if (!CSEmpresa.Current.IND_EMPRESA_FERIADO &&
                    CSPDVs.Current.PDVDentroRota() &&
                    CSEmpregados.Current.IND_ORDEM_VISITA_OBRIGATORIA)
                {
                    OperacaoReagendamento();
                }
                else
                {
                    AbrirCliente(false);
                }
            }
        }

        private void OperacaoReagendamento()
        {
            var visitaPdv = Visitas.Where(v => v.COD_PDV == CSPDVs.Current.COD_PDV).FirstOrDefault();

            if (visitaPdv.IND_VISITADO)
                AbrirCliente(false);
            else
            {
                if (!visitaPdv.PdvAteriorVisitado(Visitas))
                {
                    MessageBox.AlertErro(this, "Não é possível reagendar sem visitar o PDV da ordem de visita anterior.");
                    return;
                }
                else if (!CSPDVs.Current.EmpregadoDentroPdv())
                {
                    MessageBox.AlertErro(this, string.Format("Você está fora da localização do PDV. Data da ultima localização selecionada: {0}", CSGlobal.GetDataUltimaLocalizacaoGPS()));
                    return;
                }
                else
                {
                    MessageBox.Alert(this, string.Format("Deseja continuar com o reagendamento do cliente {0}?", (ExibirNomeFantasia ? CSPDVs.Current.NOM_FANTASIA : CSPDVs.Current.DSC_RAZAO_SOCIAL)), "Continuar",
                        (_sender, _e) =>
                        {
                            MessageBox.Alert(this, "Escolha o cliente em que você quer efetuar o reagendamento após sua ordem atual de visita.",
                                "Escolher",
                            (_sender2, _e2) => { Reagendamento = true; },
                            "Cancelar",
                            (_sender3, _e3) => { Reagendamento = false; },
                            false);
                        },
                        "Cancelar",
                        (_sender, _e) => { Reagendamento = false; }, false);
                }
            }
        }

        private void txtPesquisa_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                List<CSPDVs.CSPDV> pdvsFiltrados = new List<CSPDVs.CSPDV>();

                if (!string.IsNullOrEmpty(txtPesquisa.Text))
                {
                    int codigo = 0;

                    if (int.TryParse(txtPesquisa.Text, out codigo))
                    {
                        pdvsFiltrados = AdapterAtual.Where(p => p.COD_PDV.ToString().StartsWith(txtPesquisa.Text) ||
                                                       p.NUM_CGC.StartsWith(txtPesquisa.Text) ||
                                                       p.NUM_INSCRICAO_ESTADUAL.StartsWith(txtPesquisa.Text) ||
                                                       p.TELEFONES_PDV.Cast<CSTelefonesPDV.CSTelefonePDV>().Count(t => t.NUM_TELEFONE.ToString().StartsWith(txtPesquisa.Text)) > 0)
                                                       .ToList();
                    }
                    else
                    {
                        pdvsFiltrados = AdapterAtual.Where(p => p.DSC_RAZAO_SOCIAL.ToUpper().Contains(txtPesquisa.Text.ToUpper()) ||
                                                                p.NOM_FANTASIA.ToUpper().Contains(txtPesquisa.Text.ToUpper()) ||
                                                                p.ENDERECOS_PDV.Cast<CSEnderecosPDV.CSEnderecoPDV>().Count(end => end.DSC_LOGRADOURO_COMPLEMENTO.ToString().ToUpper().Contains(txtPesquisa.Text.ToUpper())) > 0)
                                                       .ToList();
                    }

                    listPdvs.Adapter = new LitemItemAdapter(CurrentActivity, Resource.Layout.lista_cliente_row, pdvsFiltrados, ExibirNomeFantasia);
                }
                else
                {
                    listPdvs.Adapter = new LitemItemAdapter(CurrentActivity, Resource.Layout.lista_cliente_row, AdapterAtual, ExibirNomeFantasia);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.AlertErro(this, ex.Message);
            }
        }

        void btnDiaSemana_Click(object sender, EventArgs e)
        {
            MostrarDiasDaSemana();
        }

        private void MostrarDiasDaSemana()
        {
            int itemsID = Resource.Array.DiasDaSemana;
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);

            builder.SetTitle("Selecione o dia da semana");
            if (DiaDaSemanaSelecionado == -1)
            {
                builder.SetSingleChoiceItems(itemsID, 7, mnuDiaVista_Click);
            }
            else
            {
                builder.SetSingleChoiceItems(itemsID, DiaDaSemanaSelecionado, mnuDiaVista_Click);
            }
            builder.SetView(new TextView(this) { TextSize = 10 });
            Android.Support.V7.App.AlertDialog alert = builder.Create();
            alert.Show();
        }

        public void mnuDiaVista_Click(object sender, DialogClickEventArgs e)
        {
            Android.Support.V7.App.AlertDialog a = (Android.Support.V7.App.AlertDialog)sender;
            string weekDay = a.ListView.GetItemAtPosition((int)e.Which).ToString();
            btnDiaSemana.Text = weekDay;

            // Preenche os PDVs de acordo com o dia da semana checado
            if (a.ListView.CheckedItemPosition != 7)
                DiaDaSemanaSelecionado = a.ListView.CheckedItemPosition;

            else
                DiaDaSemanaSelecionado = -1;

            PreencheListPDVs();
            a.Dismiss();
        }

        void listPdvs_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string alerta = string.Empty;
            var pdvSelecionado = (CSPDVs.CSPDV)listPdvs.Adapter.GetItem(e.Position);

            if (Reagendamento)
            {
                if (pdvSelecionado.COD_PDV == CSPDVs.Current.COD_PDV)
                {
                    MessageBox.AlertErro(this, "Não é possível fazer o reagendamento para o mesmo PDV selecionado. Operação cancelada.");
                    Reagendamento = false;
                    return;
                }

                var visitaSelecionada = Visitas.Where(v => v.COD_PDV == pdvSelecionado.COD_PDV).FirstOrDefault();

                MessageBox.Alert(this, string.Format("Deseja confirmar a nova ordem de visita {0}, após o cliente {1}?", visitaSelecionada.NUM_ORDEM_VISITA, ExibirNomeFantasia ? pdvSelecionado.NOM_FANTASIA : pdvSelecionado.DSC_RAZAO_SOCIAL),
                    "Confirmar",
                    (_sender, _e) =>
                    {
                        MessageBox.Alert(this, "A operação de reagendamento não poderá ser desfeita após continuar."
                            , "Continuar", (_sender2, _e2) =>
                            {
                                var visitaReagendamento = Visitas.Where(v => v.COD_PDV == CSPDVs.Current.COD_PDV).FirstOrDefault();

                                CSVisitas.ReagendarVisitas(visitaReagendamento, visitaSelecionada);

                                CSEmpregados.Current.RefreshVisitas();

                                Visitas = null;
                                Visitas = CSEmpregados.Current.VISITAS_EMPREGADO.Cast<CSVisitas.CSVisita>().ToList();

                                Reagendamento = false;

                                listPdvs.Adapter = null;

                                PreencheListPDVs();
                            }
                            , "Cancelar", (_sender3, _e3) => { Reagendamento = false; }, false);
                    },
                    "Cancelar",
                    (_sender, _e) =>
                    {
                        Reagendamento = false;
                    }, false);
            }
            else
            {
                CSPDVs.Current = pdvSelecionado;

                if (!CSEmpresa.Current.IND_EMPRESA_FERIADO &&
                   pdvSelecionado.PDVDentroRota() &&
                   CSEmpregados.Current.IND_ORDEM_VISITA_OBRIGATORIA)
                {
                    var visitas = CSEmpregados.Current.VISITAS_EMPREGADO.Cast<CSVisitas.CSVisita>().ToList();

                    var visitaPdv = visitas.Where(v => v.COD_PDV == pdvSelecionado.COD_PDV).FirstOrDefault();

                    if (!visitaPdv.PdvAteriorVisitado(visitas))
                    {
                        MessageBox.Alert(this, "Você não visitou o PDV da ordem de visita anterior. Faça o reagendamento ou visite o cliente anterior.");
                        return;
                    }
                }

                if (!CSEmpregados.Current.VendedorDentroExpediente(ref alerta))
                {
                    MessageBox.Alert(this, alerta);
                    return;
                }
                else if (CSEmpregados.Current.AlmocoIniciadoNaoFinalizado())
                {
                    if (CSEmpregados.Current.NUM_MINUTOS_INTERVALO_ALMOCO > 0)
                    {
                        if (CSEmpregados.Current.IntervaloAlmocoEncerrado())
                        {
                            CSEmpregados.Current.FinalizarAlmoco();

                            CSPDVs.Current = pdvSelecionado;
                            CSPDVs.Current.PEDIDOS_PDV = null;
                            ValidacoesPreVenda();
                        }
                        else
                            MessageBox.Alert(this, string.Format("Não é possível realizar venda dentro do horário de almoco. Falta(m) aproximadamente {0} minuto(s) para o encerramento do seu horário de almoço.", CSEmpregados.Current.TempoAlmocoRestante()));
                    }
                    else
                    {
                        MessageBox.Alert(this, "Você não pode realizar venda dentro do horário de almoço. Deseja informar fim do horário de almoço?", "Finalizar almoço",
                            (_sender, _e) =>
                            {
                                CSEmpregados.Current.FinalizarAlmoco();

                                CSPDVs.Current = pdvSelecionado;
                                CSPDVs.Current.PEDIDOS_PDV = null;
                                ValidacoesPreVenda();
                            }
                        , "Cancelar", (_sender, _e) =>
                        {

                        }, true);
                    }
                }
                else
                {
                    CSPDVs.Current = pdvSelecionado;
                    CSPDVs.Current.PEDIDOS_PDV = null;
                    ValidacoesPreVenda();
                }
            }
        }

        private void ValidacoesPreVenda()
        {
            CSGlobal.PedidoSugerido = false;

            if (CSEmpregados.Current.QTD_MAX_VISITA_FORA_ROTA_POSITIVADA > 0 &&
                !CSEmpregados.Current.IND_BLOQUEADO_VENDA_FORA_ROTA &&
                !CSPDVs.Current.PDVDentroRota() &&
                CSPDVs.Current.PEDIDOS_PDV.Items.Cast<CSPedidosPDV.CSPedidoPDV>().Where(p => p.STATE != ObjectState.DELETADO).Count() == 0)
            {
                progressVisita = new ProgressDialogCustomizado(this, LayoutInflater).Customizar();
                progressVisita.Show();

                new ThreadVisitasForaRota().Execute();
            }
            else
                VerificaSePDVEstaBloqueado();
        }

        private static void VerificaSePDVEstaBloqueado()
        {
            if (CSPDVs.Current.IND_BLOQUEADO == true)
            {
                if (CSEmpresa.Current.IND_LIBERA_CLIENTE_BLOQUEADO == "S")
                {
                    MessageBox.Alert(CurrentActivity, "O cliente está bloqueado para venda.\nDeseja continuar?", "Continuar", (sender, e) => { VerificaClienteInadimplente(); }, "Cancelar", null, true);
                }
                else
                {
                    MessageBox.Alert(CurrentActivity, "O cliente está bloqueado para venda.\nDeseja informar um motivo de não positivação?", "Informar motivo", (sender, e) =>
                    {
                        if (CSPDVs.Current.PDVDentroRota() &&
                            (CSEmpregados.Current.IND_PERMITIR_FOTO || CSEmpregados.Current.IND_FOTO_OBRIGATORIA) &&
                            (string.IsNullOrEmpty(CSPDVs.Current.DSC_NOME_FOTO) || !CSPDVs.Current.BOL_FOTO_VALIDADA) &&
                            (CSPDVs.Current.PEDIDOS_PDV.Items.Count == 0 &&
                            CSPDVs.Current.HISTORICOS_MOTIVO.Count == 0))
                        {
                            if (CSPDVs.Current.IND_PDV_LOCALIZACAO_VERIFICADA)
                            {
                                if (!CSPDVs.Current.EmpregadoDentroPdv())
                                {
                                    string dataUltimaLocalizacao = CSGlobal.GetDataUltimaLocalizacaoGPS();

                                    MessageBox.Alert(CurrentActivity, string.Format("Você está fora da localização do PDV. Data da ultima localização selecionada: {0}", dataUltimaLocalizacao));
                                    return;
                                }
                                else
                                    AbrirCliente(true);
                            }
                            else
                            {
                                if (CSEmpregados.Current.IND_FOTO_OBRIGATORIA)
                                {
                                    MessageBox.Alert(CurrentActivity, "Você deve tirar uma foto da fachada do PDV antes de informar o motivo de não compra.", "Foto",
                                        (_sender, _e) =>
                                        {
                                            AbrirActivityCamera(false);
                                        }, "Cancelar", null, true);
                                }
                                else
                                {
                                    MessageBox.Alert(CurrentActivity, "Deseja tirar uma foto da fachada do PDV antes de informar o motivo de não compra?.", "Foto",
                                       (_sender, _e) =>
                                       {
                                           AbrirActivityCamera(false);
                                       }, "Cancelar",
                                       (_sender, _e) =>
                                       {
                                           AbrirCliente(true);
                                       }, false);
                                }
                            }
                        }
                        else
                            AbrirCliente(true);

                    }, "Cancelar", null, true);
                }
            }
            else
            {
                VerificaClienteInadimplente();
            }
        }

        private static void AbrirActivityCamera(bool abrirRotinaPedido)
        {
            string dataUltimaLocalizacao = CSGlobal.GetDataUltimaLocalizacaoGPS();

            double minutos = 0;

            if (!string.IsNullOrEmpty(dataUltimaLocalizacao))
            {
                var diferencaAtualUltimaLocalizacao = DateTime.Now.Subtract(Convert.ToDateTime(Convert.ToDateTime(dataUltimaLocalizacao).ToString("yyyy-MM-dd HH:mm:ss")));

                minutos = diferencaAtualUltimaLocalizacao.TotalMinutes;
            }

            if (minutos >= 3)
            {
                MessageBox.Alert(CurrentActivity, "Aconselhamos a buscar uma melhor área de cobertura para melhorar a localização da foto. Deseja continuar mesmo assim?", "Aviso",
                    (_sender, _e) =>
                    {
                        CSPDVs.Current.IND_FOTO_DUVIDOSA = "S";
                        CSPDVs.Current.GravarImagemDuvidosa();
                        AbrirCamera(abrirRotinaPedido);
                    }, "Cancelar", null, true);
            }
            else
            {
                AbrirCamera(abrirRotinaPedido);
            }
        }

        private static void AbrirCamera(bool abrirRotinaPedido)
        {
            Intent i = new Intent();
            i.SetClass(CurrentActivity, typeof(Camera));

            if (abrirRotinaPedido)
                CurrentActivity.StartActivityForResult(i, frmFotoPedido);
            else
                CurrentActivity.StartActivityForResult(i, frmFotoMotivoNaoCompra);
        }

        private static void VerificaClienteInadimplente()
        {
            if (CSPDVs.Current.IND_INADIMPLENTE && CSPDVs.Current.VLR_SALDO_DEVEDOR != 0)
            {
                MessageBox.ShowShortMessageBottom(CurrentActivity, string.Format("Cliente encontra-se com o saldo devedor de: {0}", CSPDVs.Current.VLR_SALDO_DEVEDOR.ToString(CSGlobal.DecimalStringFormat)));
            }

            // Entrada no PDV
            if (CSPDVs.Current.PDVDentroRota() &&
                (CSEmpregados.Current.IND_PERMITIR_FOTO || CSEmpregados.Current.IND_FOTO_OBRIGATORIA) &&
                (string.IsNullOrEmpty(CSPDVs.Current.DSC_NOME_FOTO) || !CSPDVs.Current.BOL_FOTO_VALIDADA) &&
                (CSPDVs.Current.PEDIDOS_PDV.Items.Count == 0 &&
                CSPDVs.Current.HISTORICOS_MOTIVO.Count == 0))
            {
                if (CSPDVs.Current.IND_PDV_LOCALIZACAO_VERIFICADA)
                {
                    if (!CSPDVs.Current.EmpregadoDentroPdv())
                    {
                        string dataUltimaLocalizacao = CSGlobal.GetDataUltimaLocalizacaoGPS();

                        MessageBox.Alert(CurrentActivity, string.Format("Você está fora da localização do PDV. Data da ultima localização selecionada: {0}", dataUltimaLocalizacao));
                        return;
                    }
                    else
                        AbrirCliente(false);
                }
                else
                {
                    if (CSEmpregados.Current.IND_FOTO_OBRIGATORIA)
                    {
                        MessageBox.Alert(CurrentActivity, "Você deve tirar uma foto da fachada do PDV antes de iniciar os pedidos.", "Foto",
                            (_sender, _e) =>
                            {
                                AbrirActivityCamera(true);
                            }, "Cancelar", null, true);
                    }
                    else
                    {
                        MessageBox.Alert(CurrentActivity, "Deseja tirar uma foto da fachada do PDV antes de iniciar os pedidos?", "Foto",
                           (_sender, _e) =>
                           {
                               AbrirActivityCamera(true);
                           }, "Cancelar",
                           (_sender, _e) =>
                           {
                               AbrirCliente(false);
                           }, false);
                    }
                }
            }
            else
                AbrirCliente(false);
        }

        private static void AbrirCliente(bool motivoNaoCompra)
        {
            CSGlobal.PedidoComCombo = false;
            Intent i = new Intent();
            i.SetClass(CurrentActivity, typeof(Cliente));

            if (motivoNaoCompra)
            {
                i.PutExtra("motivoNaoCompra", true);
            }

            CurrentActivity.StartActivityForResult(i, frmCliente);
        }

        private class ThreadVisitasForaRota : AsyncTask<int, int, decimal>
        {
            private int DayOfWeek
            {
                get
                {
                    int day = (int)DateTime.Now.DayOfWeek;
                    return (day == 0) ? 7 : day;
                }
            }

            protected override decimal RunInBackground(params int[] @params)
            {
                var paramDAT_CICLO = new SQLiteParameter("@DAT_CICLO", DateTime.Now.Date);
                var paramCOD_DIA_VISITA = new SQLiteParameter("@COD_DIA_VISITA", DayOfWeek);
                var paramDAT_PEDIDO = new SQLiteParameter("@DAT_PEDIDO", DateTime.Now.Date);
                int totalPositivadosForaRota;

                var result = CSPDVs.RetornaPdvsPositivadosForaRota(paramCOD_DIA_VISITA, paramDAT_CICLO, paramDAT_PEDIDO);

                if (result != null)
                    totalPositivadosForaRota = int.Parse(result.ToString());
                else
                    totalPositivadosForaRota = 0;

                if (totalPositivadosForaRota + 1 > CSEmpregados.Current.QTD_MAX_VISITA_FORA_ROTA_POSITIVADA)
                {
                    MessageBox.Alert(CurrentActivity, "Limite de PDV's fora de rota excedida. Não é possível realizar venda para este cliente.");
                }
                else
                    AbrirCliente(false);

                return 0;
            }

            protected override void OnPostExecute(decimal result)
            {
                progressVisita.Dismiss();
            }
        }

        private void FindViewsById()
        {
            tbToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tbToolbar);
            listPdvs = FindViewById<ListView>(Resource.Id.listPdvs);
            btnDiaSemana = FindViewById<Button>(Resource.Id.btnDiaSemana);
            txtPesquisa = FindViewById<EditText>(Resource.Id.txtPesquisa);
            lblMeta = FindViewById<TextView>(Resource.Id.lblMeta);
            lblPedidos = FindViewById<TextView>(Resource.Id.lblPedidos);
            lblSkus = FindViewById<TextView>(Resource.Id.lblSkus);
        }

        private void PreencheListPDVs()
        {
            AtualizarBotaoDiaVisita();

            // [ Se broker, executa o prepare dos commands para o cálculo... ]
            if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2 &&
                CSDataAccess.Instance.preparedCommands.Size() == 0)
            {
            }

            //progressDialog = new ProgressDialogCustomizado(this, LayoutInflater).Customizar();
            //progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
            //progressDialog.SetTitle("Processando...");
            //progressDialog.SetCancelable(false);
            //progressDialog.SetMessage("Buscando lista de PDVs...");
            //progressDialog.Show();

            ////RunInBackground();
            //ThreadPool.QueueUserWorkItem(o => RunInBackground());

            progressDialog = new ProgressDialogCustomizado(this, LayoutInflater).Customizar();
            progressDialog.Show();

            new ThreadCarregarPdv().Execute();
        }

        private class ThreadCarregarPdv : AsyncTask
        {
            List<CSPDVs.CSPDV> pdvsConvertidos;

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                if (DiaDaSemanaSelecionado == 0)
                {
                    DiaDaSemanaSelecionado = 7;
                }

                CSPDVs.Items = new CSPDVs(DiaDaSemanaSelecionado);

                CalcularQtdMaxVisitaForaRotaPositivada(CSPDVs.Items.Count);

                pdvsConvertidos = CSPDVs.Items.Cast<CSPDVs.CSPDV>().ToList();

                if (Visitas == null)
                    Visitas = CSEmpregados.Current.VISITAS_EMPREGADO.Cast<CSVisitas.CSVisita>().ToList();

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                listPdvs.Adapter = new LitemItemAdapter(CurrentActivity, Resource.Layout.lista_cliente_row, pdvsConvertidos, ExibirNomeFantasia);
                AdapterAtual = pdvsConvertidos;

                progressDialog.Dismiss();
            }
        }

        private void AtualizarBotaoDiaVisita()
        {
            if (DiaDaSemanaSelecionado == -1)
            {
                btnDiaSemana.Text = "Todos";
            }
            else
            {
                btnDiaSemana.Text = CultureInfo.CreateSpecificCulture("pt-BR").DateTimeFormat.GetDayName((DayOfWeek)DiaDaSemanaSelecionado).ToTitleCase();
            }
        }

        private void RunInBackground()
        {
            try
            {
                if (DiaDaSemanaSelecionado == 0)
                {
                    DiaDaSemanaSelecionado = 7;
                }

                CSPDVs.Items = new CSPDVs(DiaDaSemanaSelecionado);

                CalcularQtdMaxVisitaForaRotaPositivada(CSPDVs.Items.Count);

                var pdvsConvertidos = CSPDVs.Items.Cast<CSPDVs.CSPDV>().ToList();
                PDVs = pdvsConvertidos;
            }
            catch (System.Exception ex)
            {
                MessageBox.AlertErro(this, ex.Message);
            }
            finally
            {
                if (progressDialog != null)
                {
                    progressDialog.Dismiss();
                    progressDialog.Dispose();
                }
            }
        }

        private static void CalcularQtdMaxVisitaForaRotaPositivada(int qtdPdvsRota)
        {
            if (CSEmpregados.Current.PCT_MAX_PEDIDO_FORA_ROTA > 0)
            {
                decimal pctMaxConfigurada = CSEmpregados.Current.PCT_MAX_PEDIDO_FORA_ROTA;
                double calculoFinal = Convert.ToDouble(qtdPdvsRota * (pctMaxConfigurada / 100m));
                int qtdMaxVendaForaRota = Convert.ToInt32(System.Math.Ceiling(calculoFinal));

                CSEmpregados.Current.QTD_MAX_VISITA_FORA_ROTA_POSITIVADA = qtdMaxVendaForaRota;
            }
            else
                CSEmpregados.Current.QTD_MAX_VISITA_FORA_ROTA_POSITIVADA = 0;
        }

        class LitemItemAdapter : ArrayAdapter<CSPDVs.CSPDV>
        {
            Context context;
            IList<CSPDVs.CSPDV> pdvs;
            int resourceId;
            bool exibirNomeFantasia;

            private TextView RazaoSocial;
            private ImageView imgFoto;
            private ImageView imgSituacaoFinanceira;
            private ImageView imgInformacoesPdv;
            private TextView lblBairro;
            private TextView lblPolitica;
            private Button btnMeta;
            private Button btnTotalVendas;

            public LitemItemAdapter(Context c, int textViewResourceId, IList<CSPDVs.CSPDV> objects, bool _exibirNomeFantasia)
                : base(c, textViewResourceId, objects)
            {
                context = c;
                pdvs = objects;
                resourceId = textViewResourceId;
                exibirNomeFantasia = _exibirNomeFantasia;
            }

            internal int GetIndex(int codPDV)
            {
                var pdv = pdvs.Where(p => p.COD_PDV == codPDV).FirstOrDefault();
                return base.GetPosition(pdv);
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSPDVs.CSPDV pdv = pdvs[position];

                LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);

                if (convertView == null)
                    convertView = layout.Inflate(resourceId, null);

                if (pdv != null)
                {
                    RazaoSocial = convertView.FindViewById<TextView>(Resource.Id.liRazaoSocial);
                    imgFoto = convertView.FindViewById<ImageView>(Resource.Id.imgFoto);
                    imgSituacaoFinanceira = convertView.FindViewById<ImageView>(Resource.Id.imgSituacaoFinanceira);
                    imgInformacoesPdv = convertView.FindViewById<ImageView>(Resource.Id.imgInformacoesPdv);
                    lblBairro = convertView.FindViewById<TextView>(Resource.Id.lblBairro);
                    btnMeta = convertView.FindViewById<Button>(Resource.Id.btnMeta);
                    btnTotalVendas = convertView.FindViewById<Button>(Resource.Id.btnTotalVendas);
                    lblPolitica = convertView.FindViewById<TextView>(Resource.Id.lblPolitica);

                    if (pdv.IND_INADIMPLENTE)
                        imgSituacaoFinanceira.SetImageResource(Resource.Drawable.ic_situacao_financeira_vermelho_select);
                    else
                        imgSituacaoFinanceira.SetImageResource(Resource.Drawable.ic_situacao_financeira_select);

                    imgInformacoesPdv.Tag = position;
                    imgSituacaoFinanceira.Tag = position;
                    btnMeta.Tag = position;
                    btnTotalVendas.Tag = position;

                    imgFoto.Click += ImgFoto_Click;
                    imgSituacaoFinanceira.Click += ImgSituacaoFinanceira_Click;
                    imgInformacoesPdv.Click += imgInformacoesPdv_Click;

                    if (RazaoSocial != null)
                    {
                        if (CSEmpresa.Current.IND_POLITICA_CALCULO_PRECO_MISTA)
                        {
                            lblPolitica.Visibility = ViewStates.Visible;
                            lblPolitica.SetTypeface(null, TypefaceStyle.Bold);

                            if (pdv.DSC_CLIPPING_INFORMATIVO.ToUpper() == "BUNGE")
                                lblPolitica.Text = "B";
                            else
                            {
                                if (pdv.CDGER0 != string.Empty)
                                    lblPolitica.Text = "G";
                                else
                                    lblPolitica.Text = "F";
                            }
                        }
                        else
                            lblPolitica.Visibility = ViewStates.Gone;

                        if (pdv.ENDERECOS_PDV != null &&
                            pdv.ENDERECOS_PDV.Count > 0)
                            lblBairro.Text = pdv.ENDERECOS_PDV[0].DSC_BAIRRO;
                        else
                            lblBairro.Text = "-";

                        if (pdv.VLR_META_PDV == 0)
                            btnMeta.Text = "M: -";
                        else
                        {
                            btnMeta.Text = string.Format("M: {0}", pdv.VLR_META_PDV.ToString("0.00"));
                        }

                        if (pdv.VLR_TOTAL_VENDAS == 0)
                            btnTotalVendas.Text = "-";
                        else
                            btnTotalVendas.Text = string.Format("R$: {0}", pdv.VLR_TOTAL_VENDAS.ToString("0.00"));

                        btnMeta.Click += BtnMeta_Click;
                        btnTotalVendas.Click += BtnTotalVendas_Click;

                        RazaoSocial.Text = string.Empty;

                        var visitaPdv = Visitas.Where(v => v.COD_PDV == pdv.COD_PDV).FirstOrDefault();

                        if (visitaPdv != null &&
                            !CSEmpresa.Current.IND_EMPRESA_FERIADO &&
                            CSEmpregados.Current.IND_ORDEM_VISITA_OBRIGATORIA)
                        {
                            RazaoSocial.Text = string.Format("{0} ", visitaPdv.NUM_ORDEM_VISITA.ToString());
                        }

                        if (exibirNomeFantasia)
                        {
                            RazaoSocial.Text += pdv.NOM_FANTASIA.ToTitleCase();
                        }
                        else
                        {
                            RazaoSocial.Text += pdv.DSC_RAZAO_SOCIAL.ToTitleCase();
                        }

                        if (pdv.IND_POSITIVADO)
                        {
                            RazaoSocial.SetTextColor(Color.ParseColor("#03a9f4"));
                            RazaoSocial.SetTypeface(null, TypefaceStyle.Bold);
                        }
                        else if (pdv.HISTORICOS_MOTIVO.Items.Count > 0)
                        {
                            RazaoSocial.SetTextColor(Color.Red);
                            RazaoSocial.SetTypeface(null, TypefaceStyle.Normal);
                        }
                        else
                        {
                            RazaoSocial.SetTextColor(Color.ParseColor("#03a9f4"));
                            RazaoSocial.SetTypeface(null, TypefaceStyle.Normal);
                        }
                    }
                }
                return convertView;
            }

            private void BtnTotalVendas_Click(object sender, EventArgs e)
            {
                int position = Convert.ToInt32(((Button)sender).Tag);

                if (((CSPDVs.CSPDV)listPdvs.Adapter.GetItem(position)).VLR_META_PDV != 0)
                {
                    if (MetaVendaClick)
                        return;

                    MetaVendaClick = true;

                    CSPDVs.Current = (CSPDVs.CSPDV)listPdvs.Adapter.GetItem(position);

                    Android.Support.V4.App.DialogFragment df = new DialogMetaLinha();
                    df.Show(thisManager, "DialogFragmentProdutoComum");
                }
            }

            private void BtnMeta_Click(object sender, EventArgs e)
            {
                int position = Convert.ToInt32(((Button)sender).Tag);

                if (((CSPDVs.CSPDV)listPdvs.Adapter.GetItem(position)).VLR_META_PDV != 0)
                {
                    if (MetaVendaClick)
                        return;

                    MetaVendaClick = true;

                    CSPDVs.Current = (CSPDVs.CSPDV)listPdvs.Adapter.GetItem(position);

                    Android.Support.V4.App.DialogFragment df = new DialogMetaLinha();
                    df.Show(thisManager, "DialogFragmentProdutoComum");
                }
            }

            private void imgInformacoesPdv_Click(object sender, EventArgs e)
            {
                if (InformacoesPdvClick)
                    return;

                InformacoesPdvClick = true;

                int position = Convert.ToInt32(((ImageView)sender).Tag);

                CSPDVs.Current = (CSPDVs.CSPDV)listPdvs.Adapter.GetItem(position);

                Intent i = new Intent();
                //i.SetClass(CurrentActivity, typeof(SimulacaoPreco));
                //CurrentActivity.StartActivity(i);
                i.SetClass(CurrentActivity, typeof(DialogCliente));
                CurrentActivity.StartActivityForResult(i, frmDialogCliente);
            }

            private void ImgSituacaoFinanceira_Click(object sender, EventArgs e)
            {
                if (SituacaoFinanceiraClick)
                    return;

                SituacaoFinanceiraClick = true;

                int position = Convert.ToInt32(((ImageView)sender).Tag);

                CSPDVs.Current = (CSPDVs.CSPDV)listPdvs.Adapter.GetItem(position);

                Intent i = new Intent();
                i.SetClass(CurrentActivity, typeof(DocumentoReceber));
                CurrentActivity.StartActivity(i);
            }

            private void ImgFoto_Click(object sender, EventArgs e)
            {

            }
        }
    }
}