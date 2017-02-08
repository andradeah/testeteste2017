using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AvanteSales.BusinessRules;
using AvanteSales.Pro.Activities;

namespace AvanteSales.Pro.Fragments
{
    public class ListaPesquisaMercado : Android.Support.V4.App.Fragment
    {
        private const int PesquisaDeMercado = 1;
        ListView lvwPesquisa;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.lista_pesquisa_mercado, container, false);
            FindViewsById(view);
            Eventos();
            return view;
        }

        private void Eventos()
        {
            lvwPesquisa.ItemClick += LvwPesquisa_ItemClick;
        }

        private void LvwPesquisa_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            CSPDVs.Current.PESQUISA_MERCADO.Current = CSPDVs.Current.PESQUISA_MERCADO[e.Position];

            ((Cliente)Activity).AbrirPesquisaMercado();
        }

        private void FindViewsById(View view)
        {
            lvwPesquisa = view.FindViewById<ListView>(Resource.Id.lvwPesquisa);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            
            CarregarListViewPesquisaMercado();
        }

        private void CarregarListViewPesquisaMercado()
        {
            lvwPesquisa.Adapter = null;

            var pesquisas = CSPDVs.Current.PESQUISA_MERCADO.Cast<CSPesquisasMercado.CSPesquisaMercado>().ToList();

            lvwPesquisa.Adapter = new ListaPesquisaMercadoAdapter(Activity, Resource.Layout.lista_pesquisa_mercado_row, pesquisas);
        }

        class ListaPesquisaMercadoAdapter : ArrayAdapter<CSPesquisasMercado.CSPesquisaMercado>
        {
            Context context;
            IList<CSPesquisasMercado.CSPesquisaMercado> pesquisa;
            int resourceId;

            public ListaPesquisaMercadoAdapter(Context c, int textViewResourceId, IList<CSPesquisasMercado.CSPesquisaMercado> objects)
                : base(c, textViewResourceId, objects)
            {
                context = c;
                pesquisa = objects;
                resourceId = textViewResourceId;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSPesquisasMercado.CSPesquisaMercado pesquisaMercado = pesquisa[position];

                LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                View linha = layout.Inflate(resourceId, null);

                if (pesquisaMercado != null)
                {
                    TextView lblPesquisa = linha.FindViewById<TextView>(Resource.Id.lblPesquisa);
                    TextView lblQtdPerguntas = linha.FindViewById<TextView>(Resource.Id.lblQtdPerguntas);
                    TextView lblStatus = linha.FindViewById<TextView>(Resource.Id.lblStatus);

                    lblPesquisa.Text = pesquisaMercado.DSC_PESQUISA;
                    lblQtdPerguntas.Text = pesquisaMercado.PERGUNTAS.Count.ToString();

                    bool TodasPerguntasRespondidas = true;
                    int qtdMarcas = pesquisaMercado.MARCAS.Count;

                    for (int i = 0; i < qtdMarcas; i++)
                    {
                        bool respostaValida;

                        foreach (CSPesquisasMercado.CSPesquisaMercado.CSMarcas.CSMarca.CSRespostas.CSResposta respostaAtual in pesquisaMercado.MARCAS[i].RESPOSTAS)
                        {
                            var a = respostaAtual;
                            respostaValida = CSGlobal.RespostaValida(respostaAtual);

                            if (!respostaValida)
                            {
                                TodasPerguntasRespondidas = false;
                                break;
                            }
                        }
                    }

                    if (!TodasPerguntasRespondidas)
                    {
                        lblStatus.Text = "Pendente";
                        lblStatus.SetTextColor(Color.Red);
                    }
                    else
                    {
                        lblStatus.Text = "Coletado";
                        lblStatus.SetTextColor(Color.LightBlue);
                    }
                }
                return linha;
            }

        }
    }
}