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
using AvanteSales.Pro.Formatters;
using AvanteSales.Pro.Activities;
using AvanteSales.Pro.Dialogs;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;

namespace AvanteSales.Pro.Fragments
{
    public class ContatoCliente : Android.Support.V4.App.Fragment
    {
        static string mensagemBunge;
        static List<CSGruposComercializacao.CSGrupoComercializacao> Linhas;
        static GridView grdLinhas;
        static ProgressDialog progress;
        LayoutInflater thisLayoutInflater;
        static Android.Support.V4.App.FragmentActivity ActivityContext;
        ImageView imgContato;
        Button btnContatoAnterior;
        Button btnProximoContato;
        int PositionContatoAtual;
        TextView lblComprador;
        TextView lblDscCategoria;
        TextView lblDscDenver;
        Button btnListaCombo;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (CSPDVs.Current.PEDIDOS_PDV.Current == null)
            {
                NovoPedido();
            }
        }

        private static bool ValidarBunge()
        {
            try
            {
                bool vendaValidaBunge = true;
                StringBuilder sql = new StringBuilder();
                mensagemBunge = "Venda para política BUNGE bloqueada para este PDV que não está na tabela ";

                sql.AppendLine("SELECT CASE WHEN SUBSTR(NUM_CGC,1,8) ");
                sql.AppendLine("		IN (SELECT CD_BASE_CLIENTE FROM BNG_LOJA_CLIENTE_AGV_SAP) ");
                sql.AppendLine("		 THEN 1 ELSE 0 END AS 'LOJA', ");
                sql.AppendLine("       CASE WHEN SUBSTR(NUM_CGC,1,8) ");
                sql.AppendLine("        IN (SELECT CD_BASE_CLIENTE FROM BNG_ZONA_VDA_LJA_CLI_AGV_SAP) ");
                sql.AppendLine("         THEN 1 ELSE 0 END AS 'ZONA' ");
                sql.AppendFormat("  FROM PDV WHERE COD_PDV = {0}", CSPDVs.Current.COD_PDV);

                using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
                {
                    if (reader.Read())
                    {
                        if (reader.GetInt32(0) == 0 &&
                            reader.GetInt32(1) == 0)
                        {
                            vendaValidaBunge = false;
                            mensagemBunge += "LOJA_CLIENTE E ZONA_VDA.";
                        }
                        else if (reader.GetInt32(0) == 0)
                        {
                            vendaValidaBunge = false;
                            mensagemBunge += "LOJA_CLIENTE.";
                        }
                        else if (reader.GetInt32(1) == 0)
                        {
                            vendaValidaBunge = false;
                            mensagemBunge += "ZONA_VDA.";
                        }
                        else
                        {
                            vendaValidaBunge = true;
                        }
                    }
                }

                return vendaValidaBunge;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static void NovoPedido()
        {
            var pedido = new CSPedidosPDV.CSPedidoPDV();
            CSPDVs.Current.PEDIDOS_PDV.Current = pedido;
            CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS = null;
            CSPDVs.Current.PEDIDOS_PDV.Current.STATE = ObjectState.NOVO;
            CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO = CSEmpregados.Current;
            CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO = CSPDVs.Current.OPERACOES.Cast<CSOperacoes.CSOperacao>().Where(o => o.COD_OPERACAO_CFO == 1).FirstOrDefault();
            CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO = CSCondicoesPagamento.GetCondicaPagamento(CSPDVs.Current.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO);

            if (CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO == null)
            {
                MessageBox.Alert(ActivityContext, "Não foi encontrado um tipo de operação. Venda não permitida.", "OK", (_sender, _e) => { ((Cliente)ActivityContext).OnBackPressed(); }, false);
            }

            if (CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO == null)
            {
                MessageBox.Alert(ActivityContext, "Não foi encontrado um tipo de condição de pagamento. Venda não permitida.", "OK", (_sender, _e) => { ((Cliente)ActivityContext).OnBackPressed(); }, false);
            }
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnResume()
        {
            base.OnResume();

            if (ActivityContext != null)
            {
                ActivityContext.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
                ActivityContext.Window.SetSoftInputMode(SoftInput.AdjustPan);
            }
        }

        private void Inicializacao()
        {
            ((Cliente)Activity).ValidarPoliticaPreco = false;
            ((Cliente)Activity).MenuLateral();
            CarregarCategoriaDenver();
            VerificarContatosDisponiveis();
            PositionContatoAtual = 0;
            MostrarContato();

            progress = new ProgressDialogCustomizado(ActivityContext, thisLayoutInflater).Customizar();
            progress.Show();

            new ThreadLinhas().Execute();
        }

        private void CarregarCategoriaDenver()
        {
            lblDscCategoria.Text = CSPDVs.Current.DSC_CATEGORIA;
            lblDscDenver.Text = CSPDVs.Current.DSC_DENVER;
        }

        private void MostrarContato()
        {
            if (CSPDVs.Current.CONTATOS_PDV != null &&
                CSPDVs.Current.CONTATOS_PDV.Count > 0)
            {
                string nomeContato = CSPDVs.Current.CONTATOS_PDV[PositionContatoAtual].NOM_CONTATO_PDV;
                string dataAniversario = string.Format("{0}/{1}",
                    CSPDVs.Current.CONTATOS_PDV[PositionContatoAtual].DIA_NIV_CONTATO_PDV.ToString(),
                    CSPDVs.Current.CONTATOS_PDV[PositionContatoAtual].MES_NIV_CONTATO_PDV.ToString());
                lblComprador.Text = string.Format("Olá {0}.\nSeu aniversário é {1}.", nomeContato, dataAniversario);
            }
            else
                lblComprador.Text = "Nenhum contato cadastrado.";
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.contato_cliente, container, false);
            thisLayoutInflater = inflater;
            FindViewsById(view);
            Eventos();
            ActivityContext = ((Cliente)Activity);
            return view;
        }

        private void Eventos()
        {
            imgContato.Click += ImgContato_Click;
            btnContatoAnterior.Click += btnContatoAnterior_Click;
            btnProximoContato.Click += btnProximoContato_Click;
            grdLinhas.ItemClick += grdLinhas_ItemClick;
            btnListaCombo.Click += BtnListaCombo_Click;
        }

        private void BtnListaCombo_Click(object sender, EventArgs e)
        {
            if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS != null &&
                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Count > 0)
            {
                foreach (CSItemsPedido.CSItemPedido itens_pedido in CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS)
                {
                    if (itens_pedido.STATE == ObjectState.DELETADO)
                        continue;

                    MessageBox.Alert(Activity, "Para utilizar um combo não pode haver venda de outro produto neste pedido.");
                }
            }
            else
                ((Cliente)Activity).AbrirListaCombos();
        }

        private bool IsBroker()
        {
            return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
        }

        private void grdLinhas_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (!CSGlobal.PedidoComCombo)
            {
                Cliente cliente = (Cliente)Activity;
                if (IsBroker() &&
                    cliente.LinhaSelecionada != null &&
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Items.Count > 0 &&
                    cliente.LinhaSelecionada.COD_GRUPO_COMERCIALIZACAO_FILTRADO != Linhas[e.Position].COD_GRUPO_COMERCIALIZACAO_FILTRADO)
                {
                    MessageBox.Alert(Activity, "Somente permitido uma linha por pedido.");
                }
                else
                {
                    cliente.LinhaSelecionada = Linhas[e.Position];
                    cliente.GrupoSelecionado = 0;
                    cliente.ProximoPasso(false);
                }
            }
            else
                MessageBox.Alert(ActivityContext, "Não é possível adicionar produtos em um pedido combo.");
        }

        private void btnProximoContato_Click(object sender, EventArgs e)
        {
            PositionContatoAtual++;
            VerificarContatosDisponiveis();
            MostrarContato();
        }

        private void btnContatoAnterior_Click(object sender, EventArgs e)
        {
            PositionContatoAtual--;
            VerificarContatosDisponiveis();
            MostrarContato();
        }

        private void VerificarContatosDisponiveis()
        {
            if (CSPDVs.Current.CONTATOS_PDV == null ||
                CSPDVs.Current.CONTATOS_PDV.Count == 0)
            {
                btnContatoAnterior.Visibility = ViewStates.Invisible;
                btnProximoContato.Visibility = ViewStates.Invisible;
            }

            if (PositionContatoAtual == 0)
                btnContatoAnterior.Visibility = ViewStates.Invisible;
            else
                btnContatoAnterior.Visibility = ViewStates.Visible;

            if (CSPDVs.Current.CONTATOS_PDV.Count > 1)
            {
                if (PositionContatoAtual + 1 == CSPDVs.Current.CONTATOS_PDV.Count)
                    btnProximoContato.Visibility = ViewStates.Invisible;
                else
                    btnProximoContato.Visibility = ViewStates.Visible;
            }
            else
                btnProximoContato.Visibility = ViewStates.Invisible;
        }

        private void ImgContato_Click(object sender, EventArgs e)
        {

        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            if (CSEmpresa.Current.IND_POLITICA_CALCULO_PRECO_MISTA &&
                ((Cliente)Activity).ValidarPoliticaPreco)
            {
                if (CSPDVs.Current.DSC_CLIPPING_INFORMATIVO.ToUpper() != "BUNGE")
                {
                    if (CSPDVs.Current.CDGER0 == string.Empty)
                    {
                        CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 1;
                        Inicializacao();
                    }
                    else
                    {

                        MessageBox.Alert(Activity, "Pedido utilizando politica de preço FlexX?", "FlexX",
                            (sender, e) =>
                            {
                                CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 1;
                                Inicializacao();
                            }, "Broker",
                            (sender, e) =>
                            {
                                CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 2;
                                Inicializacao();
                            }, false);
                    }
                }
                else
                {

                    if (CSPDVs.Current.CDGER0 != string.Empty)
                    {
                        MessageBox.Alert(Activity, "Escolha a política de preço a utilizar:",
                          "Flexx", (_e, sender) =>
                          {
                              CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 1;
                              Inicializacao();
                          },
                          "Broker", (_e, sender) =>
                          {
                              CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 2;
                              Inicializacao();
                          },
                          "Bunge", (_e, sender) =>
                          {
                              if (ValidarBunge())
                              {
                                  CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 3;
                                  Inicializacao();
                              }
                              else
                                  MessageBox.Alert(Activity, mensagemBunge);
                          }, false);
                    }
                    else
                    {
                        if (CSEmpresa.Current.IND_PDV_BUNGE_COMPRA_OUTRAS_MARCAS == "S")
                        {
                            MessageBox.Alert(Activity, "Pedido utilizando politica de preço FlexX?", "FlexX",
                            (sender, e) =>
                            {
                                CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 1;
                                Inicializacao();
                            }, "Bunge",
                                (sender, e) =>
                                {
                                    if (ValidarBunge())
                                    {
                                        CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 3;
                                        Inicializacao();
                                    }
                                    else
                                        MessageBox.Alert(Activity, mensagemBunge);
                                }, false);
                        }
                        else
                        {
                            if (ValidarBunge())
                            {
                                CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 3;
                                Inicializacao();
                            }
                            else
                                MessageBox.Alert(Activity, mensagemBunge);
                        }
                    }
                }
            }
            else
                Inicializacao();
        }

        private void FindViewsById(View view)
        {
            grdLinhas = view.FindViewById<GridView>(Resource.Id.grdLinhas);
            imgContato = view.FindViewById<ImageView>(Resource.Id.imgContato);
            btnContatoAnterior = view.FindViewById<Button>(Resource.Id.imgContatoAnterior);
            btnProximoContato = view.FindViewById<Button>(Resource.Id.imgProximoContato);
            lblComprador = view.FindViewById<TextView>(Resource.Id.lblComprador);
            lblDscCategoria = view.FindViewById<TextView>(Resource.Id.lblDscCategoria);
            lblDscDenver = view.FindViewById<TextView>(Resource.Id.lblDscDenver);
            btnListaCombo = view.FindViewById<Button>(Resource.Id.btnListaCombo);
        }

        private class grdLinhasAdapter : ArrayAdapter<CSGruposComercializacao.CSGrupoComercializacao>
        {
            Context context;
            IList<CSGruposComercializacao.CSGrupoComercializacao> linhas;
            int resourceId;
            TextView lblLinha;
            public grdLinhasAdapter(Context c, int textViewResourceId, IList<CSGruposComercializacao.CSGrupoComercializacao> objects)
                : base(c, textViewResourceId, objects)
            {
                context = c;
                linhas = objects;
                resourceId = textViewResourceId;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSGruposComercializacao.CSGrupoComercializacao linha = linhas[position];

                LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);

                if (convertView == null)
                    convertView = layout.Inflate(resourceId, null);

                if (linha != null)
                {
                    lblLinha = convertView.FindViewById<TextView>(Resource.Id.lblLinha);
                    lblLinha.Text = linha.DES_GRUPO_COMERCIALIZACAO_FILTRADO;
                }
                return convertView;
            }
        }

        private class ThreadLinhas : AsyncTask
        {
            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                try
                {
                    CSGruposProduto.Items.Count.ToString();

                    CSFamiliasProduto.Items.Count.ToString();

                    CarregaLinhas();

                    return true;
                }
                catch
                {
                    progress.Dismiss();
                    return false;
                }
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                try
                {
                    base.OnPostExecute(result);

                    grdLinhas.Adapter = new grdLinhasAdapter(ActivityContext, Resource.Layout.contato_cliente_row, Linhas);
                }
                finally
                {
                    if (progress != null)
                        progress.Dismiss();
                }
            }

            private bool IsBroker()
            {
                return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
            }

            private void CarregaLinhas()
            {
                try
                {
                    Linhas = new List<CSGruposComercializacao.CSGrupoComercializacao>();

                    if (!IsBroker())
                    {
                        CSGruposComercializacao.CSGrupoComercializacao linhaTodos = new CSGruposComercializacao.CSGrupoComercializacao();
                        linhaTodos.COD_GRUPO_COMERCIALIZACAO = -1;
                        linhaTodos.COD_GRUPO_COMERCIALIZACAO_FILTRADO = -1;
                        linhaTodos.DES_GRUPO_COMERCIALIZACAO = "TODOS";
                        linhaTodos.DES_GRUPO_COMERCIALIZACAO_FILTRADO = "TODOS";
                        Linhas.Add(linhaTodos);
                    }

                    CSGruposComercializacao classeGrupoComercializacao = new CSGruposComercializacao();
                    Linhas.AddRange(classeGrupoComercializacao.GrupoComercializacaoFiltrado());
                }
                catch (Exception ex)
                {
                    //CSGlobal.GravarLog("Produto-CarregaComboBoxGrupoComercializacao", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                }
            }
        }
    }
}