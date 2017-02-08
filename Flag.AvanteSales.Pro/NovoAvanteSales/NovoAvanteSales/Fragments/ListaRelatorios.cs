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
using AvanteSales.Pro.Activities;

namespace AvanteSales.Pro.Fragments
{
    public class ListaRelatorios : Android.Support.V4.App.Fragment
    {
        GridView grdRelatorios;
        Android.Support.V4.App.FragmentActivity ActivityContext;
        Relatorio relatorio;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnResume()
        {
            base.OnResume();

            if (ActivityContext != null)
                ActivityContext.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.lista_relatorios, container, false);
            FindViewsById(view);
            Eventos();
            ActivityContext = Activity;
            relatorio = (Relatorio)Activity;
            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            Inicializacao();
        }

        private void Inicializacao()
        {
            List<string> relatorios = new List<string>() {  "Acompanhamento de Vendas",
                                                            "Documentos a Receber",
                                                            "Resumo do Dia",
                                                            "Resumo Peso Pedido"};

            grdRelatorios.Adapter = new grdRelatoriosAdapter(ActivityContext, Resource.Layout.lista_relatorios_row, relatorios);

        }

        private void Eventos()
        {
            grdRelatorios.ItemClick += GrdRelatorios_ItemClick;
        }

        private void GrdRelatorios_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            switch (e.Position)
            {
                case 0:
                    relatorio.AlterarFragment(Relatorio.TipoRelatorio.AcompanhamentoDeVendas);
                    break;
                case 1:
                    relatorio.AlterarFragment(Relatorio.TipoRelatorio.DocumentosAReceber);
                    break;
                case 2:
                    relatorio.AlterarFragment(Relatorio.TipoRelatorio.ResumoDoDia);
                    break;
                case 3:
                    relatorio.AlterarFragment(Relatorio.TipoRelatorio.ResumoPesoPedido);
                    break;
            }
        }

        private void FindViewsById(View view)
        {
            grdRelatorios = view.FindViewById<GridView>(Resource.Id.grdRelatorios);
        }

        private class grdRelatoriosAdapter : ArrayAdapter<string>
        {
            Context context;
            IList<string> relatorios;
            int resourceId;
            TextView lblRelatorio;
            public grdRelatoriosAdapter(Context c, int textViewResourceId, IList<string> objects)
                : base(c, textViewResourceId, objects)
            {
                context = c;
                relatorios = objects;
                resourceId = textViewResourceId;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                string relatorio = relatorios[position];

                LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);

                if (convertView == null)
                    convertView = layout.Inflate(resourceId, null);

                if (relatorio != null)
                {
                    lblRelatorio = convertView.FindViewById<TextView>(Resource.Id.lblRelatorio);
                    lblRelatorio.Text = relatorio.ToUpper();
                }
                return convertView;
            }
        }
    }
}