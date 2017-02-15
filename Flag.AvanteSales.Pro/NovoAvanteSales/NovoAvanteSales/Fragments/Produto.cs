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
using Java.Lang;

namespace AvanteSales.Pro.Fragments
{
    public class Produto : Android.Support.V4.App.Fragment
    {
        Android.Support.Design.Widget.TabLayout tblProduto;
        ViewPager vwpProduto;
        Cliente cliente;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.produto, container, false);

            FindViewsById(view);
            cliente = (Cliente)Activity;

            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                vwpProduto.Adapter = new CustomAdapter(ChildFragmentManager, cliente.ApplicationContext);
                vwpProduto.OffscreenPageLimit = 2;

                tblProduto.SetOnTabSelectedListener(new Listener(vwpProduto));
                tblProduto.SetupWithViewPager(vwpProduto);
            }
            catch (System.Exception)
            {

            }
        }

        private void FindViewsById(View view)
        {
            tblProduto = view.FindViewById<Android.Support.Design.Widget.TabLayout>(Resource.Id.tblProduto);
            vwpProduto = view.FindViewById<ViewPager>(Resource.Id.vwpProduto);
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

        public static bool IsBroker()
        {
            return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
        }

        private static bool IsBunge()
        {
            return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 3;
        }

        private class CustomAdapter : FragmentPagerAdapter
        {
            private Context applicationContext;
            private Android.Support.V4.App.FragmentManager supportFragmentManager;
            string[] fragments;// = { "Venda", "Vencimento" };

            public CustomAdapter(Android.Support.V4.App.FragmentManager fm, Context context) : base(fm)
            {
                applicationContext = context;
                supportFragmentManager = fm;

                if (!IsBroker() &&
                    !IsBunge())
                {
                    fragments = new string[3];
                    //fragments[0] = "Info";
                    fragments[0] = "Venda";
                    fragments[1] = "Abatimento";
                    fragments[2] = "Vencimento";
                }
                else
                {
                    fragments = new string[2];
                    //fragments[0] = "Info";
                    fragments[0] = "Venda";
                    fragments[1] = "Vencimento";
                }
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
                if (!IsBroker() &&
                    !IsBunge())
                {
                    switch (position)
                    {
                        //case 0:
                        //    return new ProdutoInformacao();
                        case 0:
                            return new ProdutoVenda();
                        case 1:
                            return new ProdutoAbatimento();
                        case 2:
                            return new ProdutoVencimento();
                        default:
                            return null;
                    }
                }
                else
                {
                    switch (position)
                    {
                        //case 0:
                        //    return new ProdutoInformacao();
                        case 0:
                            return new ProdutoVenda();
                        case 1:
                            return new ProdutoVencimento();
                        default:
                            return null;

                    }
                }
            }

            public override ICharSequence GetPageTitleFormatted(int position)
            {
                return CharSequence.ArrayFromStringArray(fragments)[position];
            }
        }
    }
}