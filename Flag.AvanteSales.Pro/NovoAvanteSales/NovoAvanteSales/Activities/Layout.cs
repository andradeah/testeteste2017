using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "Layout", ScreenOrientation = ScreenOrientation.SensorLandscape, Theme = "@style/AvanteSalesTheme", WindowSoftInputMode = SoftInput.AdjustNothing)]
    public class Layout : Activity
    {
        ImageView imgLayout;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.layout);

            imgLayout = FindViewById<ImageView>(Resource.Id.imgLayout);

            CarregarLayout();
        }

        private void CarregarLayout()
        {
            DisplayMetrics displaymetrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetMetrics(displaymetrics);
            int screenWidth = displaymetrics.WidthPixels;
            int screenHeight = displaymetrics.HeightPixels;
                                                        
            Android.Graphics.Bitmap btmp = null;

            if (Pdv1a4CK())
                btmp = Resource.Drawable.layout_1a4ck.LoadAndResizeLayout(this.Resources, screenWidth, screenHeight);
            else if (Pdv5a9CK())
                btmp = Resource.Drawable.layout_5a9ck.LoadAndResizeLayout(this.Resources, screenWidth, screenHeight);
            else if (PdvConveniencia())
                btmp = Resource.Drawable.layout_conveniencia.LoadAndResizeLayout(this.Resources, screenWidth, screenHeight);
            else if (PdvLanchonete())
                btmp = Resource.Drawable.layout_lanchonete.LoadAndResizeLayout(this.Resources, screenWidth, screenHeight);
            else if (PdvPadaria())
                btmp = Resource.Drawable.layout_padaria.LoadAndResizeLayout(this.Resources, screenWidth, screenHeight);

            imgLayout.SetImageBitmap(btmp);
        }

        private bool Pdv1a4CK()
        {
            return CSPDVs.Current.COD_CATEGORIA == 10301 || CSPDVs.Current.COD_CATEGORIA == 10302 || CSPDVs.Current.COD_CATEGORIA == 10303 || CSPDVs.Current.COD_CATEGORIA == 10304;
        }

        private bool Pdv5a9CK()
        {
            return CSPDVs.Current.COD_CATEGORIA == 10305;
        }

        private bool PdvPadaria()
        {
            return CSPDVs.Current.COD_CATEGORIA == 30101 || CSPDVs.Current.COD_CATEGORIA == 30102;
        }

        private bool PdvConveniencia()
        {
            return CSPDVs.Current.COD_CATEGORIA == 20101;
        }

        private bool PdvLanchonete()
        {
            return CSPDVs.Current.COD_CATEGORIA == 60402;
        }
    }

    public static class BitmapLayout
    {
        public static Bitmap LoadAndResizeLayout(this int imagem, Android.Content.Res.Resources res, int width, int height)
        {
            try
            {
                // First we get the the dimensions of the file on disk
                BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
                BitmapFactory.DecodeResource(res, imagem, options);

                // Next we calculate the ratio that we need to resize the image by
                // in order to fit the requested dimensions.
                int outHeight = options.OutHeight;
                int outWidth = options.OutWidth;
                int inSampleSize = 1;

                if (outHeight > height || outWidth > width)
                {
                    inSampleSize = outWidth > outHeight
                                       ? outHeight / height
                                       : outWidth / width;
                }

                // Now we will load the image and have BitmapFactory resize it for us.
                options.InSampleSize = 3;
                options.InJustDecodeBounds = false;
                Bitmap resizedBitmap = BitmapFactory.DecodeResource(res, imagem, options);

                return resizedBitmap;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}