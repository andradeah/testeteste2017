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
using AvanteSales.SystemFramework.CSPDV;

namespace AvanteSales.Pro.Fragments
{
    public class ListaEmail : Android.Support.V4.App.Fragment
    {
        static bool isLoading;
        public static LinearLayout HeaderEmail;
        private static ProgressDialog progress;
        private static ListView lvwEmailPdv;
        static Android.Support.V4.App.FragmentActivity ActivityContext;
        public static bool CarregarEmail = false;
        LayoutInflater thisLayoutInflater;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.lista_email, container, false);
            isLoading = true;
            FindViewsById(view);
            Evetos();
            ActivityContext = Activity;
            thisLayoutInflater = inflater;
            return view;
        }

        private void FindViewsById(View view)
        {
            HeaderEmail = view.FindViewById<LinearLayout>(Resource.Id.HeaderEmail);
            lvwEmailPdv = view.FindViewById<ListView>(Resource.Id.lvwEmailPdv);
        }
        private void Evetos()
        {
            HeaderEmail.Click += HeaderEmail_Click;
            lvwEmailPdv.ItemClick += LvwEmailPdv_ItemClick;
        }

        private void LvwEmailPdv_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (CSPDVs.Current.EMAILS.Count > 0)
            {
                CSPDVEmails.Current = CSPDVEmails.Items[e.Position];

                Intent i = new Intent();
                i.SetClass(ActivityContext, typeof(DialogDigitacaoEmail));
                ActivityContext.StartActivity(i);
            }
        }

        public override void OnResume()
        {
            base.OnResume();

            if (isLoading ||
               CarregarEmail)
            {
                progress = new ProgressDialogCustomizado(ActivityContext, thisLayoutInflater).Customizar();
                progress.Show();

                new ThreadCarregarEmails().Execute();
            }
        }

        private void HeaderEmail_Click(object sender, EventArgs e)
        {
            Intent i = new Intent();
            i.SetClass(ActivityContext, typeof(DialogDigitacaoEmail));
            ActivityContext.StartActivity(i);
        }

        private class ThreadCarregarEmails : AsyncTask
        {
            List<CSPDVEmails.CSPDVEmail> emails;
            List<CSContatosPDV.CSContatoPDV> contatos;

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                emails = CSPDVs.Current.EMAILS.Cast<CSPDVEmails.CSPDVEmail>().ToList();
                contatos = CSPDVs.Current.CONTATOS_PDV.Cast<CSContatosPDV.CSContatoPDV>().ToList();

                return 0;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                lvwEmailPdv.Adapter = new LitemItemAdapterEmail(ActivityContext, Resource.Layout.lista_telefone_email_row, emails);
                
                if (lvwEmailPdv.Adapter.IsEmpty)
                {
                    var list = new List<CSPDVEmails.CSPDVEmail>();
                    list.Add(new CSPDVEmails.CSPDVEmail() { DSC_EMAIL = "Nenhum e-mail cadastrado", DSC_TIPO_EMAIL = "-" });
                    lvwEmailPdv.Adapter = new LitemItemAdapterEmail(ActivityContext, Resource.Layout.lista_telefone_email_row, list);
                }

                CarregarEmail = false;
                isLoading = false;

                progress.Dismiss();

                base.OnPostExecute(result);
            }
        }

        private class LitemItemAdapterEmail : ArrayAdapter<CSPDVEmails.CSPDVEmail>
        {
            Context context;
            IList<CSPDVEmails.CSPDVEmail> pdvs;
            int resourceId;

            public LitemItemAdapterEmail(Context c, int textViewResourceId, IList<CSPDVEmails.CSPDVEmail> objects)
                : base(c, textViewResourceId, objects)
            {
                context = c;
                pdvs = objects;
                resourceId = textViewResourceId;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSPDVEmails.CSPDVEmail cp = pdvs[position];

                LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                View linha = layout.Inflate(resourceId, null);

                if (cp != null)
                {

                    TextView tvEmail = linha.FindViewById<TextView>(Resource.Id.tvString);
                    TextView tvTipo = linha.FindViewById<TextView>(Resource.Id.tvTipo);

                    tvEmail.Text = cp.DSC_EMAIL;
                    tvTipo.Text = cp.DSC_TIPO_EMAIL;
                }
                return linha;
            }
        }
    }
}