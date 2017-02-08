using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvanteSales.Pro.Formatters;
using AvanteSales.Pro.Dialogs;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AvanteSales.SystemFramework.CSPDV;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "HistoricoIndenizacao")]
    public class HistoricoIndenizacao : Android.Support.V7.App.AppCompatActivity
    {
        Android.Support.V7.Widget.Toolbar tbToolbar;
        TextView lblCodPdv;
        TextView lblNomePdv;
        private const int frmListaProdutosIndenizacao = 0;
        private RadioGroup rgpRadioGroup;
        private TextView lblCondicao;
        private TextView lblValorTotal;
        private TextView lblStatusIndenizacao;
        private TextView lblSerie;
        private TextView lblDataCadastro;
        private TextView lblDataDevolvido;
        private TextView lblResponsavel;
        private TextView lblDataSAP;
        private Button btnListarIndenizacao;
        private TableLayout tblHistoricoIndenizacao;
        private TextView lblNFPagamento;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.historico_indenizacao);

            FindViewsById();

            SetSupportActionBar(tbToolbar);

            Eventos();

            tblHistoricoIndenizacao.Visibility = ViewStates.Gone;

            CarregarRadioIndenizacoes();
        }

        private void Eventos()
        {
            btnListarIndenizacao.Click += BtnListarIndenizacao_Click;
            rgpRadioGroup.CheckedChange += RgpRadioGroup_CheckedChange;
        }

        private void RgpRadioGroup_CheckedChange(object sender, RadioGroup.CheckedChangeEventArgs e)
        {
            var indenizacao = UltimaIndenizacaoSelecionada(e.CheckedId);
            ExibirIndenizacao(indenizacao);
            tblHistoricoIndenizacao.Visibility = ViewStates.Visible;
        }

        private void ExibirIndenizacao(CSHistoricoIndenizacoesPDV.CSHistoricoIndenizacaoPDV indenizacao)
        {
            try
            {
                // Seta qual é o pedido atual
                CSPDVs.Current.HISTORICO_INDENIZACOES.Current = indenizacao;

                var condicao = CSCondicoesPagamento.Items.Cast<CSCondicoesPagamento.CSCondicaoPagamento>()
                    .Where(c => c.COD_CONDICAO_PAGAMENTO == CSPDVs.Current.HISTORICO_INDENIZACOES.Current.COD_CONDICAO_PAGAMENTO).FirstOrDefault();

                if (condicao != null)
                {
                    // Preenche o textBox de condições de pagamento
                    lblCondicao.Text = condicao.DSC_CONDICAO_PAGAMENTO;
                }

                else
                {
                    lblCondicao.Text = "-";
                }

                lblResponsavel.Text = indenizacao.RESPONSAVEL;
                lblDataCadastro.Text = indenizacao.DAT_INDENIZACAO.ToString("dd/MM/yyyy");
                lblDataDevolvido.Text = indenizacao.DAT_DOCUMENTO_PAGAMENTO.HasValue ? indenizacao.DAT_DOCUMENTO_PAGAMENTO.Value.ToString("dd/MM/yyyy") : "-";
                lblSerie.Text = string.IsNullOrEmpty(indenizacao.NUM_SERIE_DOCUMENTO_PAGAMENTO) ? "-" : indenizacao.NUM_SERIE_DOCUMENTO_PAGAMENTO;
                lblStatusIndenizacao.Text = indenizacao.DSC_STATUS;
                lblDataSAP.Text = indenizacao.DATA_ENVIO_SAP.HasValue ? indenizacao.DATA_ENVIO_SAP.Value.ToString("dd/MM/yyyy") : "-";
                lblNFPagamento.Text = indenizacao.NUM_DOCUMENTO_PAGAMENTO == 0 ? "-" : indenizacao.NUM_DOCUMENTO_PAGAMENTO.ToString();

                if (CSPDVs.Current.HISTORICO_INDENIZACOES.Current.ITEMS_INDENIZACAO.Items.Count > 0)
                {
                    lblValorTotal.Text = indenizacao.ITEMS_INDENIZACAO.Cast<CSItemsHistoricoIndenizacao.CSItemHistoricoIndenizacao>().Sum(i => i.VLR_INDENIZACAO).ToString(CSGlobal.DecimalStringFormat);
                    btnListarIndenizacao.Visibility = ViewStates.Visible;
                    btnListarIndenizacao.Text = string.Format("Lista de produtos ({0})", indenizacao.ITEMS_INDENIZACAO.Count);
                }
                else
                {
                    lblValorTotal.Text = "-";
                    btnListarIndenizacao.Visibility = ViewStates.Invisible;
                    MessageBox.Alert(this,"Não foi possível carregar os itens da indenização escolhida.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.AlertErro(this, ex.Message);
            }
        }

        private CSHistoricoIndenizacoesPDV.CSHistoricoIndenizacaoPDV UltimaIndenizacaoSelecionada(int codIndenizacao)
        {
            return CSPDVs.Current.HISTORICO_INDENIZACOES.Items.Cast<CSHistoricoIndenizacoesPDV.CSHistoricoIndenizacaoPDV>()
                .Where(p => p.COD_INDENIZACAO == codIndenizacao).FirstOrDefault();
        }

        private void BtnListarIndenizacao_Click(object sender, EventArgs e)
        {
            if (ExisteIndenizacaoSelecionada())
            {
                Intent i = new Intent();
                i.SetClass(this, typeof(ListaProdutosIndenizacao));
                i.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                i.PutExtra("ultimaActivity", (int)Controles.ActivitiesNames.HistoricoIndenizacao);
                this.StartActivity(i);
            }
            else
            {
                Dialogs.MessageBox.Alert(this, "Selecione uma indenização");
            }
        }

        private bool ExisteIndenizacaoSelecionada()
        {
            return rgpRadioGroup.CheckedRadioButtonId != -1;
        }

        private void FindViewsById()
        {
            tbToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tbToolbar);
            lblCodPdv = FindViewById<TextView>(Resource.Id.lblCodPdv);
            lblNomePdv = FindViewById<TextView>(Resource.Id.lblNomePdv);
            rgpRadioGroup = FindViewById<RadioGroup>(Resource.Id.rgpRadioGroup);
            lblCondicao = FindViewById<TextView>(Resource.Id.lblCondicao);
            lblValorTotal = FindViewById<TextView>(Resource.Id.lblValorTotal);
            lblStatusIndenizacao = FindViewById<TextView>(Resource.Id.lblStatusIndenizacao);
            lblSerie = FindViewById<TextView>(Resource.Id.lblSerie);
            lblDataDevolvido = FindViewById<TextView>(Resource.Id.lblDataDevolvido);
            lblDataCadastro = FindViewById<TextView>(Resource.Id.lblDataCadastro);
            lblResponsavel = FindViewById<TextView>(Resource.Id.lblResponsavel);
            btnListarIndenizacao = FindViewById<Button>(Resource.Id.btnListarIndenizacao);
            tblHistoricoIndenizacao = FindViewById<TableLayout>(Resource.Id.tblHistoricoIndenizacao);
            lblDataSAP = FindViewById<TextView>(Resource.Id.lblDataSAP);
            lblNFPagamento = FindViewById<TextView>(Resource.Id.lblNFPagamento);
        }

        private void CarregarRadioIndenizacoes()
        {
            LimpaTela();

            foreach (SystemFramework.CSPDV.CSHistoricoIndenizacoesPDV.CSHistoricoIndenizacaoPDV historicoIndenizacao in CSPDVs.Current.HISTORICO_INDENIZACOES.Items)
            {
                string cod_indenizacao = historicoIndenizacao.COD_INDENIZACAO.ToString();
                string data = historicoIndenizacao.DAT_INDENIZACAO.ToString("dd/MM/yyyy");

                RadioButton rdb = new RadioButton(this);
                var lp = new LinearLayout.LayoutParams(WindowManagerLayoutParams.MatchParent, Resource.Dimension.widgets_height);

                rdb.LayoutParameters = lp;
                rdb.SetPadding(75, 0, 0, 0);
                rdb.Text = "Indenização " + cod_indenizacao + " - " + data;
                rdb.Id = Convert.ToInt32(cod_indenizacao);
                rgpRadioGroup.AddView(rdb);
            }
        }

        private void LimpaTela()
        {
            lblCondicao.Text = string.Empty;
            lblValorTotal.Text = string.Empty;
            lblStatusIndenizacao.Text = string.Empty;
            lblSerie.Text = string.Empty;
            lblDataDevolvido.Text = string.Empty;
            lblCondicao.Text = string.Empty;
        }
    }
}