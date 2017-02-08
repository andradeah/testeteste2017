using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AvanteSales.Pro.Fragments;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "DocumentoReceber", ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AvanteSalesTheme")]
    public class DocumentoReceber : AppCompatActivity
    {
        Android.Support.V7.Widget.Toolbar tbToolbar;
        TextView lblCodPdv;
        TextView lblNomePdv;
        Android.Support.Design.Widget.TabLayout tblTab;
        Android.Support.V4.View.ViewPager vwptblTab;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.documento_receber);

            FindViewsById();

            SetSupportActionBar(tbToolbar);

            Inicializacao();
        }

        private void Inicializacao()
        {
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(false);

            lblCodPdv.Text = CSPDVs.Current.COD_PDV.ToString();
            lblNomePdv.Text = CSPDVs.Current.DSC_RAZAO_SOCIAL;

            vwptblTab.Adapter = new CustomAdapter(SupportFragmentManager);

            tblTab.SetOnTabSelectedListener(new Listener(vwptblTab));
            tblTab.SetupWithViewPager(vwptblTab);

            ListaCliente.SituacaoFinanceiraClick = false;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.Finish();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FindViewsById()
        {
            tbToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tbToolbar);
            lblCodPdv = FindViewById<TextView>(Resource.Id.lblCodPdv);
            lblNomePdv = FindViewById<TextView>(Resource.Id.lblNomePdv);
            tblTab = FindViewById<Android.Support.Design.Widget.TabLayout>(Resource.Id.tblTab);
            vwptblTab = FindViewById<Android.Support.V4.View.ViewPager>(Resource.Id.vwpTab);
        }

        public class Listener : Android.Support.Design.Widget.TabLayout.IOnTabSelectedListener
        {
            ViewPager ViewPager;

            public Listener(ViewPager viewPager)
            {
                ViewPager = viewPager;
            }

            public IntPtr Handle
            {
                get
                {
                    return IntPtr.Zero;
                }
            }

            public void Dispose()
            {

            }

            public void OnTabReselected(TabLayout.Tab tab)
            {
                ViewPager.SetCurrentItem(tab.Position, true);
            }

            public void OnTabSelected(TabLayout.Tab tab)
            {
                ViewPager.SetCurrentItem(tab.Position, true);
            }

            public void OnTabUnselected(TabLayout.Tab tab)
            {
                ViewPager.SetCurrentItem(tab.Position, true);
            }
        }

        private class CustomAdapter : FragmentPagerAdapter
        {
            private Android.Support.V4.App.FragmentManager supportFragmentManager;
            string[] fragments = { "Resumo", "Listagem" };

            public CustomAdapter(Android.Support.V4.App.FragmentManager fm) : base(fm)
            {
                supportFragmentManager = fm;
            }

            public override int Count
            {
                get
                {
                    return fragments.Length;
                }
            }

            public override Android.Support.V4.App.Fragment GetItem(int position)
            {
                switch (position)
                {
                    case 0:
                        return new DocumentoReceberResumo();
                    case 1:
                        return new DocumentoReceberListagem();
                    default:
                        return null;
                }
            }

            public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
            {
                return CharSequence.ArrayFromStringArray(fragments)[position];
            }
        }
    }
}