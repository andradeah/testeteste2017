using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AvanteSales.Pro.Activities;
using AvanteSales.Pro.Dialogs;
using AvanteSales.Pro.Formatters;

namespace AvanteSales.Pro.Fragments
{
    public class ListaPedido : Android.Support.V4.App.Fragment
    {
        private static int idxPedido = -1;
        private static ProgressDialog progress;
        static ListView listPedidos;
        LayoutInflater thisLayoutInflater;
        static Android.Support.V4.App.FragmentActivity ActivityContext;
        Button btnNovoPedido;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.lista_pedido, container, false);
            thisLayoutInflater = inflater;
            ActivityContext = ((Cliente)Activity);
            FindViewsById(view);
            Eventos();
            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            Inicializacao();
        }

        public override void OnResume()
        {
            ((Cliente)ActivityContext).ListaPedidosAberto = true;
            base.OnResume();
        }

        public override void OnStop()
        {
            ((Cliente)ActivityContext).ListaPedidosAberto = false;
            base.OnStop();
        }

        private void Inicializacao()
        {
            progress = new ProgressDialogCustomizado(Activity, thisLayoutInflater).Customizar();
            progress.Show();

            new ThreadInicializacao().Execute();
        }

        private void Eventos()
        {
            listPedidos.ItemClick += ListPedidos_ItemClick;
            btnNovoPedido.Click += BtnNovoPedido_Click;
        }

        private void BtnNovoPedido_Click(object sender, EventArgs e)
        {
            CSGlobal.BloquearSaidaCliente = false;
            CSPDVs.Current.PEDIDOS_PDV.Current = null;
            ((Cliente)Activity).IniciarNovoPedido();
        }

        private void ListPedidos_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                MostraDialogOpcoesPedido(e.Position);
            }
            catch (Exception ex)
            {
                MessageBox.AlertErro(ActivityContext, ex.Message);
            }
        }
        private void MostraDialogOpcoesPedido(int position)
        {
            try
            {
                var pedidoClicado = (CSPedidosPDV.CSPedidoPDV)listPedidos.Adapter.GetItem(position);

                if (pedidoClicado.IND_PEDIDO_RETORNADO)
                {
                    MessageBox.Alert(Activity, "Não é possível fazer alterações em pedidos realizados e descarregados anteriormente à carga atual.");
                    return;
                }

                CSPDVs.Current.PEDIDOS_PDV.PEDIDO_POSITION = listPedidos.FirstVisiblePosition;

                idxPedido = position;
                CSGlobal.PedidoComCombo = false;

                MessageBox.Alert(Activity, "Selecione a opção desejada.",
                "Editar",
                 (_sender, _e) => { FuncaoEditar(pedidoClicado); },
                "Excluir",
                (_sender, _e) => { FuncaoExcluir(pedidoClicado); }, true);
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("ListaPedidos-MostraDialogOpcoesPedido", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void FuncaoEditar(CSPedidosPDV.CSPedidoPDV pedido)
        {
            try
            {
                CSPDVs.Current.PEDIDOS_PDV.Current = pedido;
                CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = CSPDVs.Current.PEDIDOS_PDV.Current.COD_POLITICA_CALCULO_PRECO;

                // Verificar se tem item que é combo no pedido.
                foreach (CSItemsPedido.CSItemPedido itemPedido in CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Items)
                {
                    if (itemPedido.COD_ITEM_COMBO > 0)
                    {
                        itemPedido.LOCK_QTD = true;

                        if (!CSGlobal.PedidoComCombo)
                            CSGlobal.PedidoComCombo = true;
                    }
                }

                if (CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO.COD_EMPREGADO != CSEmpregados.Current.COD_EMPREGADO)
                {
                    MessageBox.Alert(ActivityContext, string.Format("Este pedido foi realizado pelo vendedor {0}. Qualquer alteração neste pedido continuará com o mesmo vendedor. Deseja continuar?",
                        CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO.COD_EMPREGADO), "Continuar", (_sender, _e) =>
                        {
                            CSEmpregados.Current.COD_EMPREGADO_FONTE = CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO.COD_EMPREGADO;
                            ContinuarComEdicao();
                        }, "Cancelar", (_sender, _e) => { }, true);
                }
                else
                {
                    CSEmpregados.Current.COD_EMPREGADO_FONTE = 0;
                    ContinuarComEdicao();
                }
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("ListaPedidos-MostrarDialogOpcoesPedidoClickEditar", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void FuncaoExcluir(CSPedidosPDV.CSPedidoPDV pedido)
        {
            CSPDVs.Current.PEDIDOS_PDV.Current = pedido;

            try
            {
                if (CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO != -1)
                {
                    if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.IND_UTILIZA_QTD_SUGERIDA).Count() > 0)
                    {
                        if (CSPDVs.Current.PEDIDOS_PDV.Cast<CSPedidosPDV.CSPedidoPDV>().Where(p => p.STATE != ObjectState.DELETADO).Count() > 1)
                        {
                            MessageBox.Alert(ActivityContext, "Deletando um pedido sugerido resultará na exclusão de TODOS os demais pedidos. Deseja confirmar a operação?", "Confirmar",
                                (_sender, _e) =>
                                {
                                    var pedidosPdv = CSPDVs.Current.PEDIDOS_PDV;

                                    CSPDVs.Current.PEDIDOS_PDV.DeletarPedidos();

                                    if (CSEmpresa.Current.IND_LIMITE_DESCONTO)
                                        CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO.Flush();

                                    CSPDVs.Current.PEDIDOS_PDV.Clear();

                                    CarregaListViewPedidos();
                                }, "Cancelar",
                                (_sender, _e) => { }, true);
                        }
                        else
                        {
                            MessageBox.Alert(ActivityContext, "Deseja excluir o pedido sugerido?", "Excluir",
                                (_sender, _e) =>
                                {
                                    var pedidosPdv = CSPDVs.Current.PEDIDOS_PDV;

                                    CSPDVs.Current.PEDIDOS_PDV.DeletarPedidos();

                                    if (CSEmpresa.Current.IND_LIMITE_DESCONTO)
                                        CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO.Flush();

                                    CSPDVs.Current.PEDIDOS_PDV.Clear();

                                    CarregaListViewPedidos();
                                }, "Cancelar",
                                (_sender, _e) => { }, true);
                        }
                    }
                    else
                    {
                        ((CSPedidosPDV.CSPedidoPDV)listPedidos.Adapter.GetItem(idxPedido)).STATE = ObjectState.DELETADO;

                        // Flush apagando o pedido exlcuido
                        CSPDVs.Current.PEDIDOS_PDV.Flush();

                        if (CSEmpresa.Current.IND_LIMITE_DESCONTO)
                            CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO.Flush();

                        CarregaListViewPedidos();
                    }
                }

                ((Cliente)Activity).AtualizarValorParcial();
                CSGlobal.BloquearSaidaCliente = false;
                CSPDVs.Current.PEDIDOS_PDV.Current = null;
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("ListaPedidos-MostrarOpcoesPedidoClickExcluir", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void ContinuarComEdicao()
        {
            try
            {
                CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDO_EDITADO = true;
                // Mostra atela de pedido passando um pedido

                if (CSEmpresa.UtilizaNovoAtributoPedido)
                    CSPedidosPDV.MARCAR_PEDIDO_SALVO_CORRETAMENTE_NULO(CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO);

                if (!CSGlobal.PedidoComCombo)
                    ((Cliente)Activity).LinhaSelecionada = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS[0].PRODUTO.GRUPO_COMERCIALIZACAO;

                CSGlobal.BloquearSaidaCliente = true;
                ((Cliente)Activity).MenuLateral();
                ((Cliente)Activity).MenuClicado = false;
                ((Cliente)Activity).AtualizarValorParcial();
                ((Cliente)Activity).NavegarParaPasso(10, true);
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("ListaPedidos-ContinuarComEdicao", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private static void CarregaListViewPedidos()
        {
            try
            {
                // Limpa o listview
                listPedidos.Adapter = null;

                if (CSPDVs.Current.PEDIDOS_PDV.Items.Count > 0)
                {
                    // Lista os pedido existentes do PDV
                    var pedidosExistentes = CSPDVs.Current.PEDIDOS_PDV.Items.Cast<CSPedidosPDV.CSPedidoPDV>().Where(p => p.STATE != ObjectState.DELETADO).ToList();
                    listPedidos.Adapter = new ListaPedidosAdapter(ActivityContext, Resource.Layout.lista_pedido_row, pedidosExistentes);
                }

                listPedidos.SetSelection(CSPDVs.Current.PEDIDOS_PDV.PEDIDO_POSITION);
            }
            catch (Exception)
            {
                //CSGlobal.GravarLog("ListaPedidos-CarregarListViewPedidos", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
            }
        }

        private void FindViewsById(View view)
        {
            listPedidos = view.FindViewById<ListView>(Resource.Id.listPedidos);
            btnNovoPedido = view.FindViewById<Button>(Resource.Id.btnNovoPedido);
        }

        class ListaPedidosAdapter : ArrayAdapter<CSPedidosPDV.CSPedidoPDV>
        {
            Context context;
            IList<CSPedidosPDV.CSPedidoPDV> pedidos;
            int resourceId;

            public ListaPedidosAdapter(Context c, int textViewResourceId, IList<CSPedidosPDV.CSPedidoPDV> objects)
                : base(c, textViewResourceId, objects)
            {
                context = c;
                pedidos = objects;
                resourceId = textViewResourceId;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSPedidosPDV.CSPedidoPDV pedido = pedidos[position];

                LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                View linha = layout.Inflate(resourceId, null);

                if (pedido != null)
                {
                    TextView lblPedido = linha.FindViewById<TextView>(Resource.Id.lblPedido);
                    TextView lblOperacao = linha.FindViewById<TextView>(Resource.Id.lblOperacao);
                    TextView lblCondicao = linha.FindViewById<TextView>(Resource.Id.lblCondicao);
                    TextView lblValor = linha.FindViewById<TextView>(Resource.Id.lblValor);
                    TextView lblEmailPergunta = linha.FindViewById<TextView>(Resource.Id.lblEmailPergunta);
                    TextView lblEmailResposta = linha.FindViewById<TextView>(Resource.Id.lblEmailResposta);

                    if (CSEmpresa.Current.IND_UTILIZA_ENVIO_EMAIL.ToUpper() == "N")
                    {
                        lblEmailPergunta.Visibility = ViewStates.Gone;
                        lblEmailResposta.Visibility = ViewStates.Gone;
                    }
                    else
                        lblEmailResposta.Text = pedido.IND_EMAIL_ENVIAR ? "Sim" : "Não";

                    if (CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                    {
                        if (pedido.PEDIDO_EDITADO)
                            lblPedido.SetTextColor(Color.Red);
                        else
                            lblPedido.SetTextColor(Color.White);
                    }

                    lblPedido.Text = pedido.COD_PEDIDO.ToString();
                    lblOperacao.Text = pedido.OPERACAO.DSC_OPERACAO;
                    lblCondicao.Text = pedido.CONDICAO_PAGAMENTO.DSC_CONDICAO_PAGAMENTO;
                    lblValor.Text = pedido.VLR_TOTAL_PEDIDO.ToString(CSGlobal.DecimalStringFormat);
                }
                return linha;
            }

        }

        private class ThreadInicializacao : AsyncTask
        {
            ArrayAdapter adapter;

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                if (!CSPDVs.Current.POSSUI_PEDIDO_PENDENTE)
                    CSGlobal.PedidoComCombo = false;

                CSPDVs.Current.PEDIDOS_PDV.Dispose();

                if (CSPDVs.Current.PEDIDOS_PDV.Items.Count > 0)
                {
                    var pedidosExistentes = CSPDVs.Current.PEDIDOS_PDV.Items.Cast<CSPedidosPDV.CSPedidoPDV>().Where(p => p.STATE != ObjectState.DELETADO).ToList();
                    adapter = new ListaPedidosAdapter(ActivityContext, Resource.Layout.lista_pedido_row, pedidosExistentes);
                }

                return 0;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                if (adapter != null)
                {
                    listPedidos.Adapter = adapter;
                    listPedidos.SetSelection(CSPDVs.Current.PEDIDOS_PDV.PEDIDO_POSITION);
                }

                CSPDVs.Current.PEDIDOS_PDV.PEDIDO_POSITION = 0;

                progress.Dismiss();

                base.OnPostExecute(result);
            }
        }
    }
}