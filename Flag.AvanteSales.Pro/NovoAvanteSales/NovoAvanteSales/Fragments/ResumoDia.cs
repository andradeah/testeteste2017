using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using AvanteSales.Pro.Activities;

namespace AvanteSales.Pro.Fragments
{
    public class ResumoDia : Android.Support.V4.App.Fragment
    {
        Android.Support.Design.Widget.TabLayout tblTab;
        Android.Support.V4.View.ViewPager vwptblTab;
        Relatorio relatorio;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.resumo_dia, container, false);
            FindViewsById(view);
            relatorio = (Relatorio)Activity;
            return view;
        }

        private void FindViewsById(View view)
        {
            tblTab = view.FindViewById<Android.Support.Design.Widget.TabLayout>(Resource.Id.tblTab);
            vwptblTab = view.FindViewById<Android.Support.V4.View.ViewPager>(Resource.Id.vwpTab);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            vwptblTab.Adapter = new CustomAdapter(ChildFragmentManager, relatorio.ApplicationContext);
            
            tblTab.SetOnTabSelectedListener(new Listener(vwptblTab));
            tblTab.SetupWithViewPager(vwptblTab);
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
            private Context applicationContext;
            private Android.Support.V4.App.FragmentManager supportFragmentManager;
            string[] fragments = { "PDV", "Pedido", "Produto", "Motivo" };

            public CustomAdapter(Android.Support.V4.App.FragmentManager fm, Context context) : base(fm)
            {
                applicationContext = context;
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
                        return new ResumoDiaPDV();
                    case 1:
                        return new ResumoDiaPedido();
                    case 2:
                        return new ResumoDiaProduto();
                    case 3:
                        return new ResumoDiaMotivo();
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