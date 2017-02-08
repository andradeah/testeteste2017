using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AvanteSales.BusinessRules;
using AvanteSales.Pro.Activities;
using AvanteSales.Pro.Formatters;

namespace AvanteSales.Pro.Dialogs
{
    [Activity(Theme = "@style/AvanteSalesTheme.Dialogs", ScreenOrientation = ScreenOrientation.Portrait)]
    public class DialogCliente : Activity
    {
        Button btnVoltar;
        Button btnVender;
        Button btnConsultaPedido;
        Button btnDocumentoReceber;
        Button btnSimular;
        Button btnHistoricoIndenizacao;
        TextView tvNomeFantasia;
        TextView tvCodigo;
        TextView tvDenver;
        TextView tvEndereco;
        TextView tvBairro;
        TextView tvCidade;
        TextView tvRamo;
        TextView tvTelefone;
        TextView lblReferencia;
        TextView tvReferencia;
        TextView tvPesquisa;
        TextView lblPesquisa;
        TextView tvPeriodicidade;
        TextView lblComodato;
        TextView tvComodato;
        TextView tvLinha;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.dialog_cliente);
            Title = CSPDVs.Current.DSC_RAZAO_SOCIAL;

            FindViewsById();
            Eventos();

            Inicializacao();
        }

        private void Inicializacao()
        {
            if (CSEmpresa.Current.IND_UTILIZA_INDENIZACAO == "N" ||
             !CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA.ToString().Contains("2") ||
             !CSEmpresa.ColunaExiste("INDENIZACAO", "COD_INDENIZACAO") ||
             !CSEmpresa.ColunaExiste("MOTIVO_INDENIZACAO", "COD_MOTIVO") ||
             !CSEmpresa.ColunaExiste("HISTORICO_INDENIZACAO_ELETRONICA", "COD_INDENIZACAO_ELETRONICA"))
                btnHistoricoIndenizacao.Visibility = ViewStates.Gone;

            if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA.ToString().Contains("2") && !string.IsNullOrEmpty(CSPDVs.Current.CDGER0))
            {
                TableRow trLinha = FindViewById<TableRow>(Resource.Id.trLinha);
                trLinha.Visibility = ViewStates.Visible;
                tvLinha.Text = CSPDVs.Current.LINHAS;
            }

            tvCodigo.Text = CSPDVs.Current.COD_PDV.ToString();

            tvNomeFantasia.Text = CSPDVs.Current.NOM_FANTASIA;

            if (CSPDVs.Current.COMODATOS_PDV == null ||
                CSPDVs.Current.COMODATOS_PDV.Count == 0)
            {
                lblComodato.Visibility = ViewStates.Gone;
                tvComodato.Visibility = ViewStates.Gone;
            }
            else
                tvComodato.Text = CSPDVs.Current.COMODATOS_PDV.Count.ToString() + " contrato(s)";

            string Periodicidade = string.Empty;

            switch (CSPDVs.Current.DSC_CICLO_VISITA)
            {
                case "1 3":
                case "2 4":
                    Periodicidade = "Quinzenal";
                    break;
                case "1234":
                    Periodicidade = "Semanal";
                    break;
                case "1":
                case "2":
                case "3":
                case "4":
                    Periodicidade = "Mensal";
                    break;
            }

            tvPeriodicidade.Text = Periodicidade;

            if (CSPDVs.Current.ENDERECOS_PDV.Count > 0)
            {
                tvEndereco.Text = CSPDVs.Current.ENDERECOS_PDV[0].DSC_LOGRADOURO_COMPLEMENTO.Trim().ToTitleCase();
                tvBairro.Text = CSPDVs.Current.ENDERECOS_PDV[0].DSC_BAIRRO.Trim().ToTitleCase();
                tvCidade.Text = CSPDVs.Current.ENDERECOS_PDV[0].DSC_CIDADE.ToTitleCase() + " - " + CSPDVs.Current.ENDERECOS_PDV[0].DSC_UF.Trim();
            }

            tvRamo.Text = CSPDVs.Current.DSC_CATEGORIA.ToTitleCase();
            tvDenver.Text = CSPDVs.Current.DSC_DENVER;

            if (CSPDVs.Current.TELEFONES_PDV.Items.Count > 0)
                tvTelefone.Text = "(" + ((CSTelefonesPDV.CSTelefonePDV)CSPDVs.Current.TELEFONES_PDV.Items[0]).NUM_DDD_TELEFONE + ")" + ((CSTelefonesPDV.CSTelefonePDV)CSPDVs.Current.TELEFONES_PDV.Items[0]).NUM_TELEFONE;
            else
                tvTelefone.Text = string.Empty;
            var referencia = CSPDVs.Current.DSC_PONTO_REFERENCIA;

            if (string.IsNullOrEmpty(referencia))
            {
                lblReferencia.Visibility = ViewStates.Gone;
                tvReferencia.Visibility = ViewStates.Gone;
            }
            else
            {
                tvReferencia.Text = referencia.ToTitleCase();
            }

            int QtdPesquisaMercado = CSPDVs.Current.PESQUISA_MERCADO.Count;

            if (CSPDVs.Current.IND_PERMITIR_PESQUISA &&
                QtdPesquisaMercado > 0)
            {
                bool TodasPerguntasRespondidas = true;

                foreach (CSPesquisasMercado.CSPesquisaMercado pesquisaAtual in CSPDVs.Current.PESQUISA_MERCADO)
                {
                    var marcas = pesquisaAtual.MARCAS;
                    bool pesquisaValida = true;

                    foreach (CSPesquisasMercado.CSPesquisaMercado.CSMarcas.CSMarca marcaAtual in marcas)
                    {
                        var respostas = marcaAtual.RESPOSTAS;

                        foreach (CSPesquisasMercado.CSPesquisaMercado.CSMarcas.CSMarca.CSRespostas.CSResposta respostaAtual in respostas)
                        {
                            pesquisaValida = CSGlobal.RespostaValida(respostaAtual);

                            if (!pesquisaValida)
                            {
                                TodasPerguntasRespondidas = false;
                                break;
                            }
                        }
                    }
                }

                tvPesquisa.Text = TodasPerguntasRespondidas ? "Respondida" : "Pendente";

                if (!TodasPerguntasRespondidas)
                    tvPesquisa.SetTextColor(Color.Red);
            }
            else
            {
                lblPesquisa.Visibility = ViewStates.Gone;
                tvPesquisa.Visibility = ViewStates.Gone;
            }

            BoletosAVencer();

            ListaCliente.InformacoesPdvClick = false;
        }

        private void BoletosAVencer()
        {
            string codRevenda = Convert.ToInt32(CSEmpresa.Current.CODIGO_REVENDA).ToString("00000000");
            var documentos = CSPDVs.Current.GetDocumentosAReceber(codRevenda);
            var aVencer = documentos.Cast<CSDocumentosReceberPDV.CSDocumentoReceber>().Where(d => d.TIPO_DOCUMENTO == CSDocumentosReceberPDV.CSDocumentoReceber.TipoDocumento.A_VENCER).FirstOrDefault();

            if (aVencer.QUANTIDADE > 0)
                TitleColor = Color.Red;
        }

        private void Eventos()
        {
            btnVoltar.Click += BtnVoltar_Click;
            btnVender.Click += BtnVender_Click;
            btnSimular.Click += BtnSimular_Click;
            btnConsultaPedido.Click += BtnConsultaPedido_Click;
            btnDocumentoReceber.Click += BtnDocumentoReceber_Click;
            btnHistoricoIndenizacao.Click += BtnHistoricoIndenizacao_Click;
        }

        private void BtnHistoricoIndenizacao_Click(object sender, EventArgs e)
        {
            if (CSPDVs.Current.HISTORICO_INDENIZACOES.Count > 0)
            {
                Intent i = new Intent();
                i.SetClass(this, typeof(HistoricoIndenizacao));
                StartActivity(i);
            }
            else
            {
                MessageBox.Alert(this, "Nenhum histórico de indenização disponível.");
            }
        }

        private void BtnDocumentoReceber_Click(object sender, EventArgs e)
        {
            Intent i = new Intent();
            i.SetClass(this, typeof(DocumentoReceber));
            StartActivity(i);
        }

        private void BtnConsultaPedido_Click(object sender, EventArgs e)
        {
            Intent i = new Intent();
            i.SetClass(this, typeof(RelatorioPdv));
            i.PutExtra("RelatorioPDV", (int)CSGlobal.RelatoriosPDV.UltimosPedidos);
            StartActivity(i);
        }

        private void BtnSimular_Click(object sender, EventArgs e)
        {
            Intent i = new Intent();
            i.SetClass(this, typeof(SimulacaoPreco));
            StartActivity(i);
        }

        private void BtnVender_Click(object sender, EventArgs e)
        {
            SetResult(Result.FirstUser);
            Finish();
        }

        private void BtnVoltar_Click(object sender, EventArgs e)
        {
            Finish();
        }

        private void FindViewsById()
        {
            btnHistoricoIndenizacao = (Button)FindViewById(Resource.Id.btnHistoricoIndenizacao);
            btnSimular = (Button)FindViewById(Resource.Id.btnSimular);
            btnDocumentoReceber = (Button)FindViewById(Resource.Id.btnDocumentoReceber);
            btnConsultaPedido = (Button)FindViewById(Resource.Id.btnConsultaPedido);
            btnVoltar = (Button)FindViewById(Resource.Id.btnVoltar);
            btnVender = FindViewById<Button>(Resource.Id.btnVender);
            tvNomeFantasia = FindViewById<TextView>(Resource.Id.tvNomeFantasia);
            tvCodigo = FindViewById<TextView>(Resource.Id.tvCodigo);
            tvDenver = FindViewById<TextView>(Resource.Id.tvDenver);
            tvEndereco = FindViewById<TextView>(Resource.Id.tvEndereco);
            tvBairro = FindViewById<TextView>(Resource.Id.tvBairro);
            tvCidade = FindViewById<TextView>(Resource.Id.tvCidade);
            tvRamo = FindViewById<TextView>(Resource.Id.tvRamo);
            tvTelefone = FindViewById<TextView>(Resource.Id.tvTelefone);
            lblReferencia = FindViewById<TextView>(Resource.Id.lblReferencia);
            tvReferencia = FindViewById<TextView>(Resource.Id.tvReferencia);
            tvPesquisa = FindViewById<TextView>(Resource.Id.tvPesquisa);
            lblPesquisa = FindViewById<TextView>(Resource.Id.lblPesquisa);
            tvPeriodicidade = FindViewById<TextView>(Resource.Id.tvPeriodicidade);
            lblComodato = FindViewById<TextView>(Resource.Id.lblComodato);
            tvComodato = FindViewById<TextView>(Resource.Id.tvComodato);
            tvLinha = FindViewById<TextView>(Resource.Id.tvLinha);
        }
    }
}