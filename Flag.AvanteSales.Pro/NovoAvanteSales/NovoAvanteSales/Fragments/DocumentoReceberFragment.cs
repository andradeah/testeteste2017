using System;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.Design.Widget;

namespace AvanteSales.Pro.Fragments
{
    public class DocumentoReceberFragment : Android.Support.V4.App.Fragment
    {
        TextView lblCodPdv;
        TextView lblNomePdv;
        Android.Support.Design.Widget.TabLayout tblTab;
        Android.Support.V4.View.ViewPager vwptblTab;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.documento_receber_fragment, container, false);
            FindViewsById(view);
            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            vwptblTab.Adapter = new CustomAdapter(ChildFragmentManager);

            tblTab.SetOnTabSelectedListener(new Listener(vwptblTab));
            tblTab.SetupWithViewPager(vwptblTab);

            base.OnViewCreated(view, savedInstanceState);
        }

        private void FindViewsById(View view)
        {
            lblCodPdv = view.FindViewById<TextView>(Resource.Id.lblCodPdv);
            lblNomePdv = view.FindViewById<TextView>(Resource.Id.lblNomePdv);
            tblTab = view.FindViewById<Android.Support.Design.Widget.TabLayout>(Resource.Id.tblTab);
            vwptblTab = view.FindViewById<Android.Support.V4.View.ViewPager>(Resource.Id.vwpTab);
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