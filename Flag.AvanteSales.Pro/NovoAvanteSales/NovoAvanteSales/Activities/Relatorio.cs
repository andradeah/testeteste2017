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
using AvanteSales.Pro.Fragments;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "ListaRelatorio", ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AvanteSalesTheme", WindowSoftInputMode = SoftInput.StateHidden, ParentActivity = typeof(Main))]
    public class Relatorio : AppCompatActivity
    {
        Android.Support.V7.Widget.Toolbar tbToolbar;
        Android.Support.V4.App.FragmentTransaction ft;
        TextView lblVendedor;
        TextView lblData;
        FrameLayout frmLayout;

        public enum TipoRelatorio
        {
            AcompanhamentoDeVendas,
            DocumentosAReceber,
            ResumoDoDia,
            ResumoPesoPedido
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.relatorio);

            FindViewsById();

            SetSupportActionBar(tbToolbar);

            Inicializacao();
        }

        internal void AbrirListaProdutos(int ultimoFragment, string txtDescontoIndenizacao, string txtAdf)
        {
            ListaProdutosPedido listaProdutos = new ListaProdutosPedido();

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
        public void AlterarFragment(TipoRelatorio tipoRelatorio)
        {
            ft = SupportFragmentManager.BeginTransaction();

            switch (tipoRelatorio)
            {
                case TipoRelatorio.AcompanhamentoDeVendas:
                    {
                        ft.Replace(frmLayout.Id, new AcompanhamentoVendas());
                        ft.AddToBackStack("AcompanhamentoVendas");
                    }
                    break;
                case TipoRelatorio.DocumentosAReceber:
                    {
                        ft.Replace(frmLayout.Id, new DocumentoReceberVendedor());
                        ft.AddToBackStack("DocumentosReceberVendedor");
                    }
                    break;
                case TipoRelatorio.ResumoDoDia:
                    {
                        ft.Replace(frmLayout.Id, new ResumoDia());
                        ft.AddToBackStack("ResumoDia");
                    }
                    break;
                case TipoRelatorio.ResumoPesoPedido:
                    {
                        ft.Replace(frmLayout.Id, new ResumoPesoPedido());
                        ft.AddToBackStack("ResumoPesoPedido");
                    }
                    break;
            }

            ft.Commit();
        }

        public override void OnBackPressed()
        {
            if (SupportFragmentManager.BackStackEntryCount == 1)
                this.Finish();
            else
                SupportFragmentManager.PopBackStack();
        }

        private void Inicializacao()
        {
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(false);

            lblVendedor.Text = string.Format("{0} - {1}", CSEmpregados.Current.COD_EMPREGADO, CSEmpregados.Current.NOM_EMPREGADO);
            lblData.Text = DateTime.Now.ToString("dd/MM/yy");

            ft = SupportFragmentManager.BeginTransaction();
            ft.Replace(frmLayout.Id, new ListaRelatorios());
            ft.AddToBackStack("ListaRelatorios");
            ft.Commit();
        }

        private void FindViewsById()
        {
            tbToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tbToolbar);
            lblVendedor = FindViewById<TextView>(Resource.Id.lblVendedor);
            lblData = FindViewById<TextView>(Resource.Id.lblData);
            frmLayout = FindViewById<FrameLayout>(Resource.Id.frmLayout);
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
    }
}