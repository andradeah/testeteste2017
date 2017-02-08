using System;
using System.Collections.Generic;
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

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "Relatorios", ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AvanteSalesTheme", WindowSoftInputMode = SoftInput.AdjustNothing)]
    public class RelatorioPdv : AppCompatActivity
    {
        Android.Support.V7.Widget.Toolbar tbToolbar;
        FrameLayout frmLayout;
        TextView lblCodPdv;
        TextView lblNomePdv;
        Android.Support.V4.App.FragmentTransaction ft;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.relatorio_pdv);

            FindViewsById();

            SetSupportActionBar(tbToolbar);

            Inicializacao();
        }

        private void Inicializacao()
        {
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            lblCodPdv.Text = CSPDVs.Current.COD_PDV.ToString();
            lblNomePdv.Text = CSPDVs.Current.DSC_RAZAO_SOCIAL;

            CSGlobal.RelatoriosPDV tipoRelatorioPDV = (CSGlobal.RelatoriosPDV)Intent.GetIntExtra("RelatorioPDV", 0);

            AbrirRelatorioCorrespondente(tipoRelatorioPDV);
        }

        private void AbrirRelatorioCorrespondente(CSGlobal.RelatoriosPDV tipoRelatorioPDV)
        {
            ft = SupportFragmentManager.BeginTransaction();

            switch (tipoRelatorioPDV)
            {
                case CSGlobal.RelatoriosPDV.UltimosPedidos:
                    {
                        ft.Replace(frmLayout.Id, new Fragments.UltimasVisitas());
                    }
                    break;
            }

            ft.Commit();
        }

        internal void AbrirListaProdutos(int ultimoFragment, string txtDescontoIndenizacao, string txtAdf)
        {
            Fragments.ListaProdutosPedido listaProdutos = new Fragments.ListaProdutosPedido();

            Bundle bundle = new Bundle();
            bundle.PutInt("ultimaActivity", ultimoFragment);
            bundle.PutString("txtDescontoIndenizacao", txtDescontoIndenizacao);
            bundle.PutString("txtAdf", txtAdf);
            listaProdutos.Arguments = bundle;

            ft = SupportFragmentManager.BeginTransaction();
            ft.Replace(frmLayout.Id, listaProdutos);
            ft.AddToBackStack("ListaProdutosPedido");
            ft.Commit();
        }

        public override void OnBackPressed()
        {
            this.Finish();
        }

        private void FindViewsById()
        {
            tbToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tbToolbar);
            frmLayout = FindViewById<FrameLayout>(Resource.Id.frmLayout);
            lblCodPdv = FindViewById<TextView>(Resource.Id.lblCodPdv);
            lblNomePdv = FindViewById<TextView>(Resource.Id.lblNomePdv);
        }
    }
}