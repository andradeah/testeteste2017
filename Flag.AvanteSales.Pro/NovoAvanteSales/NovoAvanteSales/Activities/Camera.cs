using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;
using AvanteSales.Pro.Dialogs;
using Android.Provider;
using Android.Graphics;
using Android.Util;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "Camera", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class Camera : Activity
    {
        string Diretorio = CSGlobal.DiretorioImagem();
        string NomeImagem;
        bool FotoTirada;
        ImageView imgFoto;
        Button btnAbrirCamera;
        private const int intentCamera = 10;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.camera);

            ResetarFoto();

            FindViewsById();

            Eventos();

            if (!System.IO.Directory.Exists(Diretorio))
                Directory.CreateDirectory(Diretorio);

            CarregarFoto();

            System.Threading.ThreadPool.QueueUserWorkItem(o => CronometroFoto());
        }

        private void Eventos()
        {
            btnAbrirCamera.Click += BtnAbrirCamera_Click;

            if ((CSPDVs.Current.PEDIDOS_PDV.Current != null &&
                CSPDVs.Current.PEDIDOS_PDV.Current.STATE == ObjectState.NOVO &&
                !CSPDVs.Current.PEDIDOS_PDV.Current.BLOQUEAR_FOTO) ||
                (CSPDVs.Current.HISTORICOS_MOTIVO.Current != null &&
                CSPDVs.Current.HISTORICOS_MOTIVO.Current.STATE == ObjectState.NOVO) ||
                (CSPDVs.Current.PEDIDOS_PDV.Current == null && CSPDVs.Current.HISTORICOS_MOTIVO.Current == null))
                imgFoto.Click += ImgFoto_Click;
        }

        private void ImgFoto_Click(object sender, EventArgs e)
        {
            MessageBox.Alert(this, "Após a exclusão será necessário tirar outra foto. Deseja excluir?", "Excluir",
               (_sender, _e) =>
               {
                   System.IO.File.Delete(System.IO.Path.Combine(Diretorio, NomeImagem));
                   NomeImagem = string.Empty;
                   FotoTirada = false;
                   imgFoto.SetImageBitmap(null);
                   btnAbrirCamera.Visibility = ViewStates.Visible;
               },"Cancelar",
               (_sender, _e) =>
               {

               },true);
        }

        private void BtnAbrirCamera_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);

            NomeImagem = GerarNomeImagem();

            var _file = new Java.IO.File(Diretorio, String.Format("{0}", NomeImagem));

            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(_file));

            StartActivityForResult(intent, intentCamera);
        }

        private void ResetarFoto()
        {
            if (!string.IsNullOrEmpty(CSPDVs.Current.DSC_NOME_FOTO) &&
                !CSPDVs.Current.BOL_FOTO_VALIDADA)
            {
                CSPDVs.Current.DSC_NOME_FOTO = string.Empty;
                CSPDVs.Current.ResetarNomeImagem();
            }
        }

        private void CronometroFoto()
        {
            Java.Lang.Thread.Sleep(60000);

            FinishActivity(intentCamera);
            SetResult(Result.Canceled);
            Finish();
        }

        private void CarregarFoto()
        {
            try
            {
                FotoTirada = false;

                if (CSPDVs.Current != null)
                {
                    CarregarFotoSetParametros(CSPDVs.Current.DSC_NOME_FOTO);
                }
            }
            catch (Exception)
            {

            }
        }

        private string GerarNomeImagem()
        {
            return string.Format("{0}_{1}_{2}_{3}_{4}_.jpg", CSEmpresa.Current.CODIGO_REVENDA, CSEmpregados.Current.COD_EMPREGADO, DateTime.Now.Date.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmss"), CSPDVs.Current.COD_PDV);
        }

        private void FindViewsById()
        {
            imgFoto = FindViewById<ImageView>(Resource.Id.imgFoto);
            btnAbrirCamera = FindViewById<Button>(Resource.Id.btnAbrirCamera);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                DisplayMetrics displaymetrics = new DisplayMetrics();
                WindowManager.DefaultDisplay.GetMetrics(displaymetrics);
                int screenWidth = displaymetrics.WidthPixels;
                int screenHeight = displaymetrics.HeightPixels;

                Bitmap btmp = System.IO.Path.Combine(Diretorio, NomeImagem).LoadAndResizeBitmap(screenWidth, screenHeight);
                imgFoto.SetImageBitmap(btmp);

                File.Delete(System.IO.Path.Combine(Diretorio, NomeImagem));

                System.IO.Stream fos = new System.IO.FileStream(System.IO.Path.Combine(Diretorio, NomeImagem), System.IO.FileMode.Create);
                btmp.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg, 20, fos);
                fos.Flush();
                fos.Close();
                btmp.Dispose();

                btnAbrirCamera.Visibility = ViewStates.Gone;
                FotoTirada = true;
            }
        }

        public override void OnBackPressed()
        {
            if (FotoTirada)
            {
                SetResult(Result.Ok);
                CSPDVs.Current.DSC_NOME_FOTO = NomeImagem;
                CSPDVs.Current.NUM_LATITUDE_FOTO = CSGlobal.GetLatitudeFlexxGPS();
                CSPDVs.Current.NUM_LONGITUDE_FOTO = CSGlobal.GetLongitudeFlexxGPS();
                CSPDVs.Current.GravarNomeImagem();
                Finish();
            }
            else
            {
                if (CSEmpregados.Current.IND_FOTO_OBRIGATORIA)
                {
                    MessageBox.Alert(this, "Você não tirou uma foto da fachada do PDV. Deseja voltar e cancelar a operação?", "Voltar",
                        (_sender, _e) =>
                        {
                            SetResult(Result.Canceled);
                            Finish();
                        }, true);
                }
                else
                {
                    SetResult(Result.Ok);
                    Finish();
                }
            }
        }

        private void CarregarFotoSetParametros(string caminhoFoto)
        {
            if (!string.IsNullOrEmpty(caminhoFoto))
            {
                NomeImagem = caminhoFoto;

                DisplayMetrics displaymetrics = new DisplayMetrics();
                WindowManager.DefaultDisplay.GetMetrics(displaymetrics);
                int screenWidth = displaymetrics.WidthPixels;
                int screenHeight = displaymetrics.HeightPixels;

                Android.Graphics.Bitmap btmp = System.IO.Path.Combine(Diretorio, caminhoFoto).LoadAndResizeBitmap(screenWidth, screenHeight);
                imgFoto.SetImageBitmap(btmp);

                FotoTirada = true;

                btnAbrirCamera.Visibility = ViewStates.Gone;
                btmp.Dispose();
            }
        }
    }

    public static class BitmapHelpers
    {
        public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height)
        {
            try
            {
                // First we get the the dimensions of the file on disk
                BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
                BitmapFactory.DecodeFile(fileName, options);

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
                Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

                return resizedBitmap;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}