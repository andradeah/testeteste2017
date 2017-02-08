using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AvanteSales.Pro.Dialogs;
using AvanteSales.SystemFramework;
using AvanteSales.Pro.Formatters;
using AvanteSales.BusinessRules;

namespace AvanteSales.Pro.Fragments
{
    public class PesquisaMercado : Android.Support.V4.App.Fragment
    {
        bool formatandoTelefone;
        private const int frmMotivoNaoResposta = 1;

        public Spinner cboMarca;
        private TextView lblPesquisaMercado;
        private TextView lblNumeroPergunta;
        private TextView lblPergunta;
        private TextView lblIntrucoes;
        private TextView lblMarca;
        private TextView lblResposta;
        private EditText txtResposta;
        private Button btnResponder;
        private LinearLayout llResposta;
        private int PosicaoResposta = 0;
        private ListView lvwPesquisa;
        LayoutInflater thisLayoutInflater;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.pesquisa_mercado, container, false);
            FindViewsById(view);
            thisLayoutInflater = inflater;
            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            Inicializacao();
        }

        private void Inicializacao()
        {
            formatandoTelefone = false;

            CarregarComboMarcas();

            llResposta.RemoveAllViews();

            lblPesquisaMercado.Text = string.Format("{0} ({1} a {2})", CSPDVs.Current.PESQUISA_MERCADO.Current.DSC_PESQUISA, CSPDVs.Current.PESQUISA_MERCADO.Current.DATINI_PESQUISA_MERC.ToString("dd/MM/yyyy"), CSPDVs.Current.PESQUISA_MERCADO.Current.DATFIM_PESQUISA_MERC.ToString("dd/MM/yyyy"));

            CSPesquisasMercado.CSPesquisaMercado.CSPerguntas.CSPergunta pergunta = CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS[0];

            CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current = pergunta;

            PreencherDados();
        }

        private void CarregarComboMarcas()
        {
            var adapter = new ArrayAdapter(Activity, Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            cboMarca.Adapter = adapter;

            for (int i = 0; i < CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS.Count; i++)
            {
                CSItemCombo ic = new CSItemCombo();
                ic.Texto = CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS[i].DSC_MARCA_PESQUISA_MERC;
                ic.Valor = CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS[i];

                adapter.Add(ic);
            }
        }

        private void FindViewsById(View view)
        {
            cboMarca = view.FindViewById<Spinner>(Resource.Id.cboMarca);
            lblPesquisaMercado = view.FindViewById<TextView>(Resource.Id.lblPesquisaMercado);
            lblNumeroPergunta = view.FindViewById<TextView>(Resource.Id.lblNumeroPergunta);
            lblPergunta = view.FindViewById<TextView>(Resource.Id.lblPergunta);
            lblIntrucoes = view.FindViewById<TextView>(Resource.Id.lblIntrucoes);
            lblMarca = view.FindViewById<TextView>(Resource.Id.lblMarca);
            lblResposta = view.FindViewById<TextView>(Resource.Id.lblResposta);
            btnResponder = view.FindViewById<Button>(Resource.Id.btnResponder);
            llResposta = view.FindViewById<LinearLayout>(Resource.Id.llResposta);
            lvwPesquisa = view.FindViewById<ListView>(Resource.Id.lvwPesquisa);
        }

        private void Eventos()
        {
            cboMarca.ItemSelected += CboMarca_ItemSelected;
            btnResponder.Click += BtnResponder_Click;
            lvwPesquisa.ItemClick += LvwPesquisa_ItemClick;
        }

        private void LvwPesquisa_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            llResposta.RemoveAllViews();
            SetandoPerguntaAtualEPreencherDados(e.Position);
        }

        private void BtnResponder_Click(object sender, EventArgs e)
        {
            bool respostaValida = true;

            if (CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC != 1 &&
                CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC != 6 &&
                CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC != 4 &&
                CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC != 5 &&
                (txtResposta != null &&
                string.IsNullOrEmpty(txtResposta.Text)))
            {
                MessageBox.ShowShortMessageCenter(Activity,"Resposta inválida");
                respostaValida = false;
            }
            else
            {
                if (CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC == 2 ||
                    CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC == 3)
                {
                    if (CSGlobal.StrToDecimal(txtResposta.Text) < CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.VAL_FAIXAINI_MERC)
                    {
                        MessageBox.ShowShortMessageCenter(Activity, "Resposta abaixo do mínimo permitido");
                        respostaValida = false;
                    }
                    else if (CSGlobal.StrToDecimal(txtResposta.Text) > CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.VAL_FAIXAFIM_MERC)
                    {
                        MessageBox.ShowShortMessageCenter(Activity, "Resposta acima do máximo permitido");
                        respostaValida = false;
                    }
                }
                else if (CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC == 4)
                {
                    if (!string.IsNullOrEmpty(txtResposta.Text) &&
                        (txtResposta.Text.Length < 13 ||
                        txtResposta.Text.Length > 14))
                    {
                        MessageBox.ShowShortMessageCenter(Activity, "Telefone inválido");
                        respostaValida = false;
                    }
                }
                else if (CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC == 1 ||
                        CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC == 6)
                {
                    if (llResposta.FindViewById<Spinner>(Resource.Id.cboResposta).Adapter.Count == 0)
                    {
                        MessageBox.ShowShortMessageCenter(Activity, "Não há respostas cadastradas.");
                        respostaValida = false;
                    }
                }
            }

            if (respostaValida)
            {
                if (CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC == 1 ||
                    CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC == 6)
                    CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS[cboMarca.SelectedItemPosition].RESPOSTAS[PosicaoResposta].VAL_RESPOSTA = ((CSItemCombo)llResposta.FindViewById<Spinner>(Resource.Id.cboResposta).SelectedItem).Valor.ToString();
                else
                    CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS[cboMarca.SelectedItemPosition].RESPOSTAS[PosicaoResposta].VAL_RESPOSTA = txtResposta.Text;

                CSPDVs.Current.PESQUISA_MERCADO.Current.Flush();

                if (txtResposta != null)
                    Activity.HideKeyboard(txtResposta);

                AtualizarLista();

                ProximaPergunta();
            }
        }

        private void ProximaPergunta()
        {
            llResposta.RemoveAllViews();

            int QuantidadeMarcas = CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS.Count;
            int QuantidadePerguntasMarcaAtual = CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS[cboMarca.SelectedItemPosition].PESQUISA.PERGUNTAS.Count;
            int PositionMarcaAtual = cboMarca.SelectedItemPosition;
            int PositionPerguntaAtual = PosicaoResposta;

            if (QuantidadePerguntasMarcaAtual > 0)
            {
                if (PositionPerguntaAtual + 1 < QuantidadePerguntasMarcaAtual)
                {
                    SetandoPerguntaAtualEPreencherDados(PositionPerguntaAtual + 1);
                }
                else
                {
                    if (QuantidadeMarcas > 1)
                    {
                        if (PositionMarcaAtual + 1 < QuantidadeMarcas)
                        {
                            MessageBox.Alert(Activity, "Deseja ir para a próxima marca?", "Próxima marca", (sender, e) =>
                            {
                                cboMarca.SetSelection(PositionMarcaAtual + 1);
                            },"Cancelar",
                            (sender, e) =>
                            { },false);
                        }
                        else
                        {
                            var marcas = CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS;
                            bool pesquisaValida = true;

                            foreach (CSPesquisasMercado.CSPesquisaMercado.CSMarcas.CSMarca marcaAtual in marcas)
                            {
                                var respostas = marcaAtual.RESPOSTAS;

                                foreach (CSPesquisasMercado.CSPesquisaMercado.CSMarcas.CSMarca.CSRespostas.CSResposta respostaAtual in respostas)
                                {
                                    pesquisaValida = CSGlobal.RespostaValida(respostaAtual);
                                }
                            }

                            if (pesquisaValida)
                                MessageBox.Alert(Activity, "Pesquisa respondida com sucesso.");
                        }

                    }
                    else
                    {
                        var marcas = CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS;
                        bool pesquisaValida = true;

                        foreach (CSPesquisasMercado.CSPesquisaMercado.CSMarcas.CSMarca marcaAtual in marcas)
                        {
                            var respostas = marcaAtual.RESPOSTAS;

                            foreach (CSPesquisasMercado.CSPesquisaMercado.CSMarcas.CSMarca.CSRespostas.CSResposta respostaAtual in respostas)
                            {
                                pesquisaValida = CSGlobal.RespostaValida(respostaAtual);
                            }
                        }

                        if (pesquisaValida)
                            MessageBox.Alert(Activity, "Pesquisa respondida com sucesso.");
                    }
                }
            }
        }

        private void AtualizarLista()
        {
            lvwPesquisa.Adapter = null;
            CarregaLista();
        }

        private void CboMarca_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            lblMarca.Text = cboMarca.SelectedItem.ToString();

            CarregaLista();

            SetandoPerguntaAtualEPreencherDados(0);
        }

        private void SetandoPerguntaAtualEPreencherDados(int position)
        {
            View view;
            CSPesquisasMercado.CSPesquisaMercado.CSPerguntas.CSPergunta pergunta = CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS[position];

            CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current = pergunta;
            PosicaoResposta = position;

            switch (pergunta.TIP_RESP_PERGUNTA_MERC)
            {
                case 1:
                    {
                        view = thisLayoutInflater.Inflate(Resource.Layout.pesquisa_mercado_combo, null);
                        llResposta.AddView(view);
                        Spinner combo = llResposta.FindViewById<Spinner>(Resource.Id.cboResposta);
                        CarregarComboResposta(combo);
                        combo.RequestFocus();
                    }
                    break;
                case 2:
                    {
                        view = thisLayoutInflater.Inflate(Resource.Layout.pesquisa_mercado_valor, null);
                        llResposta.AddView(view);
                        txtResposta = llResposta.FindViewById<EditText>(Resource.Id.txtResposta);
                        txtResposta.TextChanged += TxtResposta_TextChanged;
                    }
                    break;
                case 3:
                    {
                        view = thisLayoutInflater.Inflate(Resource.Layout.pesquisa_mercado_quantidade, null);
                        llResposta.AddView(view);
                        txtResposta = llResposta.FindViewById<EditText>(Resource.Id.txtResposta);
                    }
                    break;
                case 4:
                    {
                        view = thisLayoutInflater.Inflate(Resource.Layout.pesquisa_mercado_telefone, null);
                        llResposta.AddView(view);
                        txtResposta = llResposta.FindViewById<EditText>(Resource.Id.txtResposta);
                        txtResposta.TextChanged += TxtTelefone_TextChanged;
                    }
                    break;
                case 5:
                    {
                        view = thisLayoutInflater.Inflate(Resource.Layout.pesquisa_mercado_texto, null);
                        llResposta.AddView(view);
                        txtResposta = llResposta.FindViewById<EditText>(Resource.Id.txtResposta);
                    }
                    break;
                case 6:
                    {
                        view = thisLayoutInflater.Inflate(Resource.Layout.pesquisa_mercado_combo, null);
                        llResposta.AddView(view);
                        Spinner combo = llResposta.FindViewById<Spinner>(Resource.Id.cboResposta);
                        CarregarComboRespostaOpcoes(combo, pergunta);
                        combo.RequestFocus();
                    }
                    break;
            }

            PreencherDados();

            Activity.ShowKeyboard();

            if (txtResposta != null)
            {
                if (pergunta.TIP_RESP_PERGUNTA_MERC != 1)
                    txtResposta.RequestFocus();
            }

        }

        private void TxtTelefone_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (e.AfterCount >= 1)
            {
                if (!formatandoTelefone)
                {
                    if (!string.IsNullOrEmpty(txtResposta.Text))
                    {
                        formatandoTelefone = true;

                        if (txtResposta.Text.Length == 3)
                        {
                            txtResposta.Text = string.Format("({0}){1}", txtResposta.Text.Substring(0, 2), txtResposta.Text.Substring(2, 1));
                        }
                        if (txtResposta.Text.Length == 9)
                        {
                            if (txtResposta.Text.Contains("-"))
                                txtResposta.Text = txtResposta.Text.Replace("-", string.Empty);
                            else
                                txtResposta.Text = string.Format("{0}-{1}", txtResposta.Text.Substring(0, 8), txtResposta.Text.Substring(8, 1));
                        }
                        if (txtResposta.Text.Length == 14)
                        {
                            txtResposta.Text = txtResposta.Text.Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty);
                            txtResposta.Text = string.Format("({0}){1}-{2}", txtResposta.Text.Substring(0, 2), txtResposta.Text.Substring(2, 5), txtResposta.Text.Substring(7, 4));
                        }

                        txtResposta.SetSelection(txtResposta.Text.Length);

                        formatandoTelefone = false;
                    }
                }
            }
        }

        private void TxtResposta_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtResposta.Text))
                if (txtResposta.Text.Contains("."))
                {
                    txtResposta.Text = txtResposta.Text.Replace(".", ",");
                    txtResposta.SetSelection(txtResposta.Text.Length);
                }

            if (txtResposta.Text.Contains(","))
            {
                int posicao = txtResposta.Text.IndexOf(',');

                if (txtResposta.Text.Substring(posicao + 1, txtResposta.Text.Length - posicao - 1).Length > 2)
                {
                    txtResposta.Text = txtResposta.Text.Remove(txtResposta.Text.Length - 1);
                    txtResposta.SetSelection(txtResposta.Text.Length);
                }

                else
                    ValidarFormatacao();
            }
            else
                ValidarFormatacao();
        }

        private void ValidarFormatacao()
        {
            if (txtResposta.Text != string.Empty &&
                StringFormatter.NaoDecimal(txtResposta.Text))
            {
                txtResposta.Text = txtResposta.Text.Remove(txtResposta.Text.Length - 1);
                txtResposta.SetSelection(txtResposta.Text.Length);
            }
        }

        private void CarregarComboResposta(Spinner combo)
        {
            var adapter = new ArrayAdapter(Activity, Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            combo.Adapter = adapter;

            CSItemCombo ic = new CSItemCombo();
            ic.Texto = "Sim";
            ic.Valor = "Sim";

            adapter.Add(ic);

            if (ic.Valor.ToString() == CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS[cboMarca.SelectedItemPosition].RESPOSTAS[PosicaoResposta].VAL_RESPOSTA)
                combo.SetSelection(0);

            ic = new CSItemCombo();
            ic.Texto = "Não";
            ic.Valor = "Não";

            adapter.Add(ic);

            if (ic.Valor.ToString() == CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS[cboMarca.SelectedItemPosition].RESPOSTAS[PosicaoResposta].VAL_RESPOSTA)
                combo.SetSelection(1);
        }

        private void CarregarComboRespostaOpcoes(Spinner combo, CSPesquisasMercado.CSPesquisaMercado.CSPerguntas.CSPergunta pergunta)
        {
            var adapter = new ArrayAdapter(Activity, Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            combo.Adapter = adapter;
            int index = 0;

            CSItemCombo ic;

            foreach (var opcao in pergunta.OPCOES_RESPOSTA)
            {
                ic = new CSItemCombo();
                ic.Texto = string.Format("{0}-{1}", opcao.COD_LISTA, opcao.DSC_OPCAO_LISTA);
                //ic.Texto = opcao.DSC_OPCAO_LISTA;
                ic.Valor = opcao.DSC_OPCAO_LISTA;

                adapter.Add(ic);

                if (ic.Valor.ToString() == CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS[cboMarca.SelectedItemPosition].RESPOSTAS[PosicaoResposta].VAL_RESPOSTA)
                    combo.SetSelection(index);

                index++;
            }
        }
        private void PreencherDados()
        {
            lblNumeroPergunta.Text = "Pergunta " + CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.COD_PERGUNTA_MERC.ToString() + ":";
            lblPergunta.Text = CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.DSC_PERGUNTA_MERC;

            if (CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC == 3)
            {
                lblIntrucoes.Text = (CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC == 2 ? "Valor R$" : "Quant.") + " - Mín: " + Convert.ToInt32(CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.VAL_FAIXAINI_MERC).ToString()

                                                                                                                         + " e Máx: " + Convert.ToInt32(CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.VAL_FAIXAFIM_MERC).ToString();
            }
            else if (CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC == 2)
            {
                lblIntrucoes.Text = (CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC == 2 ? "Valor R$" : "Quant.") + " - Mín: " + CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.VAL_FAIXAINI_MERC.ToString(CSGlobal.DecimalStringFormat)

                                                                                                                         + " e Máx: " + CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.VAL_FAIXAFIM_MERC.ToString(CSGlobal.DecimalStringFormat);
            }
            else if (CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC == 1)
                lblIntrucoes.Text = "Sim/Não";
            else if (CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC == 4)
                lblIntrucoes.Text = "Telefone";
            else if (CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC == 6)
                lblIntrucoes.Text = "Lista";
            else
                lblIntrucoes.Text = "Texto";

            if (txtResposta != null)
            {
                if (CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC != 1 &&
                    CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC != 6)
                {
                    if (CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS[cboMarca.SelectedItemPosition].RESPOSTAS[PosicaoResposta].VAL_RESPOSTA == null)
                        txtResposta.Text = string.Empty;
                    else
                    {
                        if (CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC == 2)
                        {
                            if (cboMarca.SelectedItemPosition != -1)
                                txtResposta.Text = Convert.ToDecimal(CSGlobal.StrToDecimal(CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS[cboMarca.SelectedItemPosition].RESPOSTAS[PosicaoResposta].VAL_RESPOSTA)).ToString(CSGlobal.DecimalStringFormat);
                            else
                                txtResposta.Text = Convert.ToDecimal(CSGlobal.StrToDecimal(CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS[0].RESPOSTAS[PosicaoResposta].VAL_RESPOSTA)).ToString(CSGlobal.DecimalStringFormat);

                            if (CSGlobal.StrToDecimal(txtResposta.Text) == 0)
                                txtResposta.Text = string.Empty;
                        }
                        else if (CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC == 3)
                        {
                            if (cboMarca.SelectedItemPosition != -1)
                                txtResposta.Text = Convert.ToDecimal(CSGlobal.StrToDecimal(CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS[cboMarca.SelectedItemPosition].RESPOSTAS[PosicaoResposta].VAL_RESPOSTA)).ToString();
                            else
                                txtResposta.Text = Convert.ToDecimal(CSGlobal.StrToDecimal(CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS[0].RESPOSTAS[PosicaoResposta].VAL_RESPOSTA)).ToString();

                            if (CSGlobal.StrToDecimal(txtResposta.Text) == 0)
                                txtResposta.Text = string.Empty;
                        }
                        else
                            txtResposta.Text = CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS[0].RESPOSTAS[PosicaoResposta].VAL_RESPOSTA;
                    }
                }
                else
                {
                    if (CSPDVs.Current.PESQUISA_MERCADO.Current.PERGUNTAS.Current.TIP_RESP_PERGUNTA_MERC == 1)
                    {
                        if (llResposta.FindViewById<Spinner>(Resource.Id.cboResposta) != null)
                            llResposta.FindViewById<Spinner>(Resource.Id.cboResposta).SetSelection((CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS[0].RESPOSTAS[PosicaoResposta].VAL_RESPOSTA == "Sim" ? 0 : 1));
                    }
                }
            }
        }

        private void CarregaLista()
        {
            var respostas = CSPDVs.Current.PESQUISA_MERCADO.Current.MARCAS[cboMarca.SelectedItemPosition].RESPOSTAS;

            List<CSListViewItem> listItem = new List<CSListViewItem>();

            if (respostas.Count > 0)
            {
                for (int i = 0; i < respostas.Count; i++)
                {
                    CSListViewItem item = new CSListViewItem();

                    item.Text = respostas[i].PERGUNTA.COD_PERGUNTA_MERC.ToString() + " - " + respostas[i].PERGUNTA.DSC_PERGUNTA_MERC;
                    item.Valor = respostas[i];

                    item.SubItems = new List<object>();

                    if (respostas[i].VAL_RESPOSTA == null)
                        item.SubItems.Add(string.Empty);
                    else
                    {
                        if (respostas[i].PERGUNTA.TIP_RESP_PERGUNTA_MERC == 2)
                            item.SubItems.Add(Convert.ToDecimal(CSGlobal.StrToDecimal(respostas[i].VAL_RESPOSTA)).ToString(CSGlobal.DecimalStringFormat));
                        else if (respostas[i].PERGUNTA.TIP_RESP_PERGUNTA_MERC == 3)
                            item.SubItems.Add(Convert.ToDecimal(CSGlobal.StrToDecimal(respostas[i].VAL_RESPOSTA)).ToString());
                        else
                            item.SubItems.Add(respostas[i].VAL_RESPOSTA);
                    }

                    listItem.Add(item);
                }
            }

            lvwPesquisa.Adapter = new ListarPerguntasRespostas(Activity, Resource.Layout.pesquisa_mercado_row, listItem);
        }

        class ListarPerguntasRespostas : ArrayAdapter<CSListViewItem>
        {
            Activity act;
            IList<CSListViewItem> produto;
            int resourceId;

            public ListarPerguntasRespostas(Activity c, int textViewResourceId, IList<CSListViewItem> objects)
                : base(c, textViewResourceId, objects)
            {
                act = c;
                produto = objects;
                resourceId = textViewResourceId;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSListViewItem item = produto[position];

                LayoutInflater layout = (LayoutInflater)act.GetSystemService(Context.LayoutInflaterService);
                View linha = layout.Inflate(resourceId, null);

                try
                {
                    TextView tvPRG = linha.FindViewById<TextView>(Resource.Id.tvPRG);
                    TextView tvResposta = linha.FindViewById<TextView>(Resource.Id.tvResposta);

                    tvPRG.Text = item.Text;
                    tvResposta.Text = item.SubItems[0].ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.AlertErro(act, ex.Message);
                }

                return linha;
            }

        }
    }
}