using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SupportActionBarDrawerToggle = Android.Support.V7.App.ActionBarDrawerToggle;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Android.Support.V4.Widget;

namespace AvanteSales.Pro.Controles
{
    public class MyActionBarDawerToggle: SupportActionBarDrawerToggle
    {
        private AppCompatActivity HostActivity;
        private int OpenedResource;
        private int ClosedResource;

        public MyActionBarDawerToggle(AppCompatActivity host, DrawerLayout drawerLayout)
            : base(host,drawerLayout,Resource.String.abc_action_bar_home_description,Resource.String.abc_action_bar_home_description)
        {
            HostActivity = host;
            OpenedResource = Resource.String.abc_action_bar_home_description;
            ClosedResource = Resource.String.abc_action_bar_home_description;
        }

        public override void OnDrawerOpened(View drawerView)
        {
            base.OnDrawerOpened(drawerView);
        }

        public override void OnDrawerClosed(View drawerView)
        {
            base.OnDrawerClosed(drawerView);
        }

        public override void OnDrawerSlide(View drawerView, float slideOffset)
        {
            base.OnDrawerSlide(drawerView, slideOffset);
        }
    }
}