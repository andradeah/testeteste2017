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
using AvanteSales.Pro.Formatters;
using AvanteSales.Pro.Activities;

namespace AvanteSales.Pro.Fragments
{
    public class ProdutoAbatimento : Android.Support.V4.App.Fragment
    {
        private bool IgnorarEvento = false;
        TextView lblCodigoProduto;
        TextView lblProduto;
        public static EditText txtValorFinalItemIndenizacao;
        public static EditText txtQtdeInteiroIndenizacao;
        public static EditText txtQtdeUnidadeIndenizacao;
        static Android.Support.V4.App.FragmentActivity ActivityContext;

        private static bool m_IsDirty = false;
        private static bool IsDirty
        {
            get
            {
                return m_IsDirty;
            }
            set
            {
                m_IsDirty = value;
            }
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.produto_abatimento, container, false);

            ActivityContext = ((Cliente)Activity);
            FindViewsById(view);
            Eventos();

            return view;
        }

        private void Eventos()
        {
            txtValorFinalItemIndenizacao.TextChanged += TxtValorFinalItemIndenizacao_TextChanged;
            txtQtdeInteiroIndenizacao.TextChanged += TxtQtdeInteiroIndenizacao_TextChanged;
            txtQtdeUnidadeIndenizacao.TextChanged += TxtQtdeUnidadeIndenizacao_TextChanged;
        }

        private void TxtQtdeUnidadeIndenizacao_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                if (CSProdutos.Current.COD_UNIDADE_MEDIDA == "UN")
                {
                    MessageBox.ShowShortMessageCenter(Activity, "Este produto só pode ser indenizado inteiro.");

                    //if (txtQtdeUnidadeIndenizacao.Text.Length == 1)
                    //    txtQtdeUnidadeIndenizacao.Text = string.Empty;

                    return;
                }

                if (IgnorarEvento)
                    return;

                // Marca que foi alterado
                IsDirty = true;

                // Marca que o objeto foi alterado e deve ser salvo durante o flush
                if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.INALTERADO || CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.SALVO)
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE = ObjectState.ALTERADO;

                // nao deixa que entre em um loop de eventos ao se modificar...
                IgnorarEvento = true;

                // [ Se não for broker e bunge... ]
                if ((CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2 &&
                    CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 3) ||
                    CSGlobal.PedidoSugerido)
                {

                    // calcula e mostra o valor total      
                    txtValorFinalItemIndenizacao.Text = CalculaValorTotalItemIndenizacao().ToString(CSGlobal.DecimalStringFormat);
                }

                IgnorarEvento = false;
            }
            catch (System.Exception)
            {

            }
        }

        private static decimal CalculaValorTotalItemIndenizacao()
        {
            decimal valorTotalItem = 0;
            decimal valorTotalUnitario = 0;
            decimal valorFinalItem = 0;

            try
            {
                //if (DesconsiderarDesconto)
                //{
                if (ProdutoVenda.txtValorFinalItem != null)
                    valorFinalItem = CSGlobal.StrToDecimal(ProdutoVenda.txtValorFinalItem.Text);
                //}
                //else
                //{
                //    valorFinalItem = CSGlobal.StrToDecimal(lblValorTabela.Text);
                //}

                valorTotalUnitario = decimal.Round(valorFinalItem / CSProdutos.Current.QTD_UNIDADE_EMBALAGEM, 4);

                // Calculo dos produtos com a caixa fechada
                valorTotalItem = valorFinalItem * CSGlobal.StrToDecimal(txtQtdeInteiroIndenizacao.Text);

                // Calculo dos produtos com a caixa aberta
                valorTotalItem += (valorTotalUnitario * CSGlobal.StrToDecimal(txtQtdeUnidadeIndenizacao.Text));

            }
            catch (System.Exception ex)
            {
                MessageBox.AlertErro(ActivityContext, ex.Message);
            }

            return valorTotalItem;
        }

        private void TxtValorFinalItemIndenizacao_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                if (!ValidaFormatacaoNumerica())
                    return;

                if (!string.IsNullOrEmpty(txtValorFinalItemIndenizacao.Text))
                    if (txtValorFinalItemIndenizacao.Text.Contains("."))
                    {
                        txtValorFinalItemIndenizacao.Text = txtValorFinalItemIndenizacao.Text.Replace(".", ",");
                        txtValorFinalItemIndenizacao.SetSelection(txtValorFinalItemIndenizacao.Text.Length);
                    }

                if (txtValorFinalItemIndenizacao.Text.Contains(','))
                {
                    int posicao = txtValorFinalItemIndenizacao.Text.IndexOf(',');

                    if (txtValorFinalItemIndenizacao.Text.Substring(posicao + 1, txtValorFinalItemIndenizacao.Text.Length - posicao - 1).Length > 2)
                    {
                        txtValorFinalItemIndenizacao.Text = txtValorFinalItemIndenizacao.Text.Remove(txtValorFinalItemIndenizacao.Text.Length - 1);
                        txtValorFinalItemIndenizacao.SetSelection(txtValorFinalItemIndenizacao.Text.Length);
                        return;
                    }
                }

                if (IgnorarEvento)
                    return;

                // Marca que foi alterado
                IsDirty = true;

                // Marca que o objeto foi alterado e deve ser salvo durante o flush
                if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.INALTERADO || CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.SALVO)
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE = ObjectState.ALTERADO;

                IgnorarEvento = false;
            }
            catch (OverflowException)
            {
                MessageBox.AlertErro(Activity, "Número de caracteres máximo atingido.");
            }
        }

        private bool ValidaFormatacaoNumerica()
        {
            try
            {
                if (txtValorFinalItemIndenizacao.Text != string.Empty &&
                    StringFormatter.NaoDecimal(txtValorFinalItemIndenizacao.Text))
                {
                    txtValorFinalItemIndenizacao.Text = txtValorFinalItemIndenizacao.Text.Remove(txtValorFinalItemIndenizacao.Text.Length - 1);
                    txtValorFinalItemIndenizacao.SetSelection(txtValorFinalItemIndenizacao.Text.Length);
                    return false;
                }

                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private void TxtQtdeInteiroIndenizacao_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                if (IgnorarEvento)
                    return;

                // Marca que foi alterado
                IsDirty = true;

                // Marca que o objeto foi alterado e deve ser salvo durante o flush
                if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.INALTERADO || CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.SALVO)
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE = ObjectState.ALTERADO;

                // nao deixa que entre em um loop de eventos ao se modificar...
                IgnorarEvento = true;

                //[Se não for broker e bunge... ]
                if ((CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2 &&
                     CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 3) ||
                    CSGlobal.PedidoSugerido)
                {
                    txtValorFinalItemIndenizacao.Text = CalculaValorTotalItemIndenizacao().ToString(CSGlobal.DecimalStringFormat);
                }

                IgnorarEvento = false;
            }
            catch (System.Exception)
            {

            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            lblCodigoProduto.Text = CSProdutos.Current.DESCRICAO_APELIDO_PRODUTO;
            lblProduto.Text = CSProdutos.Current.DSC_PRODUTO;

            Inicializacao();
        }

        private void Inicializacao()
        {
            if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_INDENIZACAO_INTEIRA > 0)
                txtQtdeInteiroIndenizacao.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_INDENIZACAO_INTEIRA.ToString();
            else
                txtQtdeInteiroIndenizacao.Text = "";

            if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_INDENIZACAO_UNIDADE > 0)
                txtQtdeUnidadeIndenizacao.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_INDENIZACAO_UNIDADE.ToString();

            if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_INDENIZACAO_UNIDADE != 0)
                txtValorFinalItemIndenizacao.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_INDENIZACAO_UNIDADE.ToString(CSGlobal.DecimalStringFormat);
            else
                txtValorFinalItemIndenizacao.Text = "";

            CSGlobal.Focus(Activity, txtQtdeInteiroIndenizacao);
        }

        private void FindViewsById(View view)
        {
            lblCodigoProduto = view.FindViewById<TextView>(Resource.Id.lblCodigoProduto);
            lblProduto = view.FindViewById<TextView>(Resource.Id.lblProduto);
            txtQtdeInteiroIndenizacao = view.FindViewById<EditText>(Resource.Id.txtQtdeInteiroIndenizacao);
            txtQtdeUnidadeIndenizacao = view.FindViewById<EditText>(Resource.Id.txtQtdeUnidadeIndenizacao);
            txtValorFinalItemIndenizacao = view.FindViewById<EditText>(Resource.Id.txtValorFinalItemIndenizacao);
        }
    }
}