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
using AvanteSales.BusinessRules;

namespace AvanteSales.Pro.Fragments
{
    public class DetalhePreco : Android.Support.V4.App.DialogFragment
    {
        private ListView lvwBroker;
        private TextView txtDescMax;
        private TextView txtDescontoTotal;
        private TextView txtADFTotal;
        private TextView lblDescontoTotal;
        private TextView lblAdicionalTotal;
        private TextView txtDescMaxNaoBroker;
        string Inteira;
        string Partida;
        string AdfUnitario;
        string DescontoMaximo;
        string ValorTabela;
        string Desconto;
        string DescontoIncond;
        string QtdeInteiro;
        string QtdeUnidade;
        string DescMaximo;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view;

            if (IsBroker())
                view = inflater.Inflate(Resource.Layout.detalhe_preco_broker, container, false);
            else
                view = inflater.Inflate(Resource.Layout.detalhe_preco, container, false);

            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);

            FindViewsByIds(view);

            Inteira = Arguments.GetString("txtQtdeInteiro","0");
            Partida = Arguments.GetString("txtQtdeUnidade","0");
            AdfUnitario = Arguments.GetString("lblValorAdicionalFinanceiro","0");
            DescontoMaximo = Arguments.GetString("descMaximo","0");
            ValorTabela = Arguments.GetString("lblValorTabela","0");
            Desconto = Arguments.GetString("lblValorDesconto","0");
            DescontoIncond = Arguments.GetString("txtDescIncond","0");
            QtdeInteiro = Arguments.GetString("txtQtdeInteiro", "0");
            QtdeUnidade = Arguments.GetString("txtQtdeUnidade", "0");
            DescMaximo = Arguments.GetString("descMaximo", "0");

            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsBroker())
                CarregaValoresBroker();
            else
                CarregaValores();
        }

        private void CarregaValores()
        {
            decimal inteira = CSGlobal.StrToDecimal(Inteira);
            decimal partida = CSGlobal.StrToDecimal(Partida);

            decimal vlrADFUnt = CSGlobal.StrToDecimal(AdfUnitario);
            decimal vlrADFTot = 0;
            decimal descMaximo = CSGlobal.StrToDecimal(DescontoMaximo);
            vlrADFTot = vlrADFUnt * inteira;
            vlrADFTot += ((vlrADFUnt / CSProdutos.Current.QTD_UNIDADE_EMBALAGEM) * partida);

            decimal valorTabela = CSGlobal.StrToDecimal(ValorTabela);
            decimal caixa = CSProdutos.Current.UNIDADES_POR_CAIXA;
            decimal desconto = CSGlobal.StrToDecimal(Desconto);
            decimal precoUnitario = valorTabela / caixa;
            decimal descontoTotal = desconto * ((inteira * caixa) + partida);

            txtDescontoTotal.Text = descontoTotal.ToString(CSGlobal.DecimalStringFormat);
            txtADFTotal.Text = vlrADFTot.ToString(CSGlobal.DecimalStringFormat);
            txtDescMaxNaoBroker.Text = descMaximo.ToString(CSGlobal.DecimalStringFormat);
        }

        private void CarregaValoresBroker()
        {
            decimal descIncond = CSGlobal.StrToDecimal(DescontoIncond);
            int qtdeInteiro = CSGlobal.StrToInt(QtdeInteiro);
            int qtdeUnidade = CSGlobal.StrToInt(QtdeUnidade);
            decimal descMaximo = CSGlobal.StrToDecimal(DescMaximo);

            txtDescMax.Visibility = ViewStates.Visible;

            if (CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO == 20)
            {
                if (CSEmpresa.Current.IND_UTILIZA_PRICE_2014)
                    CSPDVs.Current.POLITICA_BROKER_2014.INDENIZACAO = true;
                else
                    CSPDVs.Current.POLITICA_BROKER.INDENIZACAO = true;
            }
            else
            {
                if (CSEmpresa.Current.IND_UTILIZA_PRICE_2014)
                    CSPDVs.Current.POLITICA_BROKER_2014.INDENIZACAO = false;
                else
                    CSPDVs.Current.POLITICA_BROKER.INDENIZACAO = false;
            }

            lvwBroker.Adapter = null;

            CSItemsPedido.CSItemPedido item = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current;

            item.PRC_DESCONTO = descIncond;
            item.QTD_PEDIDA_INTEIRA = qtdeInteiro;
            item.QTD_PEDIDA_UNIDADE = qtdeUnidade;

            var valores = item.CalculaValor2014().Where(v => v != null && (v.DADO != 0 || v.VALOR != 0)).ToList();
            lvwBroker.Adapter = new ListViewBroker2014Adapter(Activity, Resource.Layout.detalhe_preco_broker_row, valores);

            txtDescMax.Text = descMaximo.ToString(CSGlobal.DecimalStringFormat);
        }

        private class ListViewBroker2014Adapter : ArrayAdapter<CSPoliticaBroker2014.TmpPricingCons>
        {
            Context context;
            IList<CSPoliticaBroker2014.TmpPricingCons> valores;
            int resourceId;

            public ListViewBroker2014Adapter(Context c, int textViewResourceId, IList<CSPoliticaBroker2014.TmpPricingCons> objects)
                : base(c, textViewResourceId, objects)
            {
                context = c;
                valores = objects;
                resourceId = textViewResourceId;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSPoliticaBroker2014.TmpPricingCons valor = valores[position];

                LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                View linha = layout.Inflate(resourceId, null);

                if (valor != null)
                {
                    TextView tvSeq = linha.FindViewById<TextView>(Resource.Id.tvSeq);
                    TextView tvCondicao = linha.FindViewById<TextView>(Resource.Id.tvCondicao);
                    TextView tvDado = linha.FindViewById<TextView>(Resource.Id.tvDado);
                    TextView tvValorResultado = linha.FindViewById<TextView>(Resource.Id.tvValorResultado);

                    tvSeq.Text = valor.NMCONDLIN.ToString();
                    tvCondicao.Text = valor.CDCONDTYP;
                    tvDado.Text = valor.DADO.ToString(CSGlobal.DecimalStringFormat);
                    tvValorResultado.Text = valor.VALOR.ToString(CSGlobal.DecimalStringFormat);
                }
                return linha;
            }
        }

        private void FindViewsByIds(View view)
        {
            if (IsBroker())
            {
                txtDescMax = view.FindViewById<TextView>(Resource.Id.txtDescMax);
                lvwBroker = view.FindViewById<ListView>(Resource.Id.lvwBroker);
            }
            else
            {
                lblDescontoTotal = view.FindViewById<TextView>(Resource.Id.lblDescontoTotal);
                txtDescontoTotal = view.FindViewById<TextView>(Resource.Id.txtDescontoTotal);
                lblAdicionalTotal = view.FindViewById<TextView>(Resource.Id.lblAdicionalTotal);
                txtADFTotal = view.FindViewById<TextView>(Resource.Id.txtADFTotal);
                txtDescMaxNaoBroker = view.FindViewById<TextView>(Resource.Id.txtDescMax);
            }
        }

        private static bool IsBroker()
        {
            return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
        }
    }
}