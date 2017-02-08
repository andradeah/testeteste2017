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

namespace AvanteSales.Pro.Fragments
{
    public class ListaTelefone : Android.Support.V4.App.Fragment
    {
        static bool isLoading;
        LinearLayout HeaderTelefone;
        static ListView lvwTelefonePdv;
        static ProgressDialog progress;
        static Android.Support.V4.App.FragmentActivity ActivityContext;
        LayoutInflater thisLayoutInflater;
        public static bool AbriuDialogTelefone;
        public static bool CarregarTelefone = false;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.lista_telefone, container, false);
            isLoading = true;
            FindViewsById(view);
            Eventos();
            ActivityContext = Activity;
            thisLayoutInflater = inflater;

            return view;
        }

        private void Eventos()
        {
            HeaderTelefone.Click += HeaderTelefone_Click;
            lvwTelefonePdv.ItemClick += LvwTelefonePdv_ItemClick;
        }

        private void LvwTelefonePdv_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (CSPDVs.Current.TELEFONES_PDV.Count > 0)
            {
                CSTelefonesPDV.Current = CSPDVs.Current.TELEFONES_PDV[e.Position];
                AbriuDialogTelefone = true;
                Intent i = new Intent();
                i.SetClass(ActivityContext, typeof(DialogDigitacaoTelefone));
                ActivityContext.StartActivity(i);
            }
        }

        private void HeaderTelefone_Click(object sender, EventArgs e)
        {
            CSTelefonesPDV.Current = null;
            AbriuDialogTelefone = true;
            Intent i = new Intent();
            i.SetClass(ActivityContext, typeof(DialogDigitacaoTelefone));
            ActivityContext.StartActivity(i);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            AbriuDialogTelefone = false;
        }

        public override void OnResume()
        {
            base.OnResume();

            if (isLoading ||
                CarregarTelefone)
            {
                progress = new ProgressDialogCustomizado(ActivityContext, thisLayoutInflater).Customizar();
                progress.Show();

                new ThreadCarregarTelefone().Execute();
            }
        }

        private void FindViewsById(View view)
        {
            HeaderTelefone = view.FindViewById<LinearLayout>(Resource.Id.HeaderTelefone);
            lvwTelefonePdv = view.FindViewById<ListView>(Resource.Id.lvwTelefonePdv);
        }

        private class ThreadCarregarTelefone : AsyncTask
        {
            List<CSTelefonesPDV.CSTelefonePDV> telefones;

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                telefones = CSPDVs.Current.TELEFONES_PDV.Cast<CSTelefonesPDV.CSTelefonePDV>().ToList();

                return 0;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                isLoading = false;
                CarregarTelefone = false;
                lvwTelefonePdv.Adapter = new ListemItemAdapterTelefone(ActivityContext, Resource.Layout.lista_telefone_email_row, telefones);

                if (CSEmpregados.Current.IND_UTILIZA_PEDIDO_SUGERIDO)
                    CSPDVs.Current.COLETA_OBRIGATORIA = true;

                if (progress != null)
                    progress.Dismiss();
            }
        }

        private class ListemItemAdapterTelefone : ArrayAdapter<CSTelefonesPDV.CSTelefonePDV>
        {
            Context context;
            IList<CSTelefonesPDV.CSTelefonePDV> pdvs;
            int resourceId;

            public ListemItemAdapterTelefone(Context c, int textViewResourceId, IList<CSTelefonesPDV.CSTelefonePDV> objects)
                : base(c, textViewResourceId, objects)
            {
                context = c;
                pdvs = objects;
                resourceId = textViewResourceId;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSTelefonesPDV.CSTelefonePDV cp = pdvs[position];

                LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                View linha = layout.Inflate(resourceId, null);

                if (cp != null)
                {

                    TextView tvTelefone = linha.FindViewById<TextView>(Resource.Id.tvString);
                    TextView tvTipo = linha.FindViewById<TextView>(Resource.Id.tvTipo);

                    string telefone = cp.NUM_DDD_TELEFONE.ToString() + cp.NUM_TELEFONE;

                    if (telefone.Length >= 10)
                    {
                        if (telefone.Length == 10)
                            tvTelefone.Text = string.Format("({0}){1}-{2}", telefone.Substring(0, 2), telefone.Substring(2, 4), telefone.Substring(6, telefone.Length - 6));
                        else
                            tvTelefone.Text = string.Format("({0}){1}-{2}", telefone.Substring(0, 2), telefone.Substring(2, 5), telefone.Substring(7, 4));
                    }
                    else
                        tvTelefone.Text = telefone;

                    tvTipo.Text = cp.DSC_TIPO_TELEFONE;
                }
                return linha;
            }
        }
    }
}