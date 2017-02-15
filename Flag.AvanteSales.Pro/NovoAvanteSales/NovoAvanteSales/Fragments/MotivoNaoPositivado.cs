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
using AvanteSales.BusinessRules;
using AvanteSales.Pro.Activities;
using AvanteSales.Pro.Dialogs;
using AvanteSales.Pro.Formatters;

namespace AvanteSales.Pro.Fragments
{
    public class MotivoNaoPositivado : Android.Support.V4.App.Fragment
    {
        private const int FOTO = 1;
        private static RadioGroup rdgMotivos;
        private TextView lblTitulo;
        private static int tipoMotivo;
        static View thisView;
        static Android.Support.V4.App.FragmentActivity ActivityContext;

        public static int TipoMotivo
        {
            get
            {
                return tipoMotivo;
            }
            set
            {
                tipoMotivo = value;
            }
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.motivo_nao_positivado, container, false);
            FindViewsById(view);
            TipoMotivo = Arguments.GetInt("TipoMotivo");
            thisView = view;
            ActivityContext = ((Cliente)Activity);
            return view;
        }

        private static void Fechar()
        {
            RadioButton rdb = thisView.FindViewById<RadioButton>(rdgMotivos.CheckedRadioButtonId);

            CSMotivos.CSMotivo motivo = new CSMotivos.CSMotivo()
            {
                COD_MOTIVO = (int)rdb.Tag,
                DSC_MOTIVO = rdb.Text,
                COD_TIPO_MOTIVO = TipoMotivo
            };

            if (TipoMotivo == CSMotivos.CSTipoMotivo.NAO_POSITIVACAO_CLIENTE)
            {
                if (CSPDVs.Current.HISTORICOS_MOTIVO.Current == null)
                {
                    // [ Cria um novo historico de motivo ]
                    CSHistoricosMotivo.CSHistoricoMotivo hismot = new CSHistoricosMotivo.CSHistoricoMotivo();

                    // [ Preenche com os valores a serem salvos na classe de historico ]
                    hismot.COD_PDV = CSPDVs.Current.COD_PDV;
                    hismot.COD_MOTIVO = motivo.COD_MOTIVO;
                    hismot.COD_TIPO_MOTIVO = motivo.COD_TIPO_MOTIVO;
                    hismot.DAT_HISTORICO_MOTIVO = DateTime.Now;
                    hismot.DAT_ALTERACAO = hismot.DAT_HISTORICO_MOTIVO;

                    if (CSEmpresa.Current.IND_UTILIZA_FLEXX_GPS == "S")
                    {
                        hismot.NUM_LATITUDE_LOCALIZACAO = CSGlobal.GetLatitudeFlexxGPS();
                        hismot.NUM_LONGITUDE_LOCALIZACAO = CSGlobal.GetLongitudeFlexxGPS();
                    }
                    //hismot.DSC_NOME_FOTO = NomeFoto;

                    hismot.MOTIVO = motivo;

                    // [ Adiciona o historico do motivo na coleção de historicos do motivo ]
                    CSPDVs.Current.HISTORICOS_MOTIVO.Add(hismot);
                }
                else
                {
                    CSPDVs.Current.HISTORICOS_MOTIVO.Current.COD_MOTIVO = motivo.COD_MOTIVO;
                    CSPDVs.Current.HISTORICOS_MOTIVO.Current.DAT_ALTERACAO = DateTime.Now;
                    CSPDVs.Current.HISTORICOS_MOTIVO.Current.STATE = ObjectState.ALTERADO;
                    //CSPDVs.Current.HISTORICOS_MOTIVO.Current.DSC_NOME_FOTO = NomeFoto;
                    CSPDVs.Current.HISTORICOS_MOTIVO.Add(CSPDVs.Current.HISTORICOS_MOTIVO.Current);
                }

                // Dispara o metodo de salvamento dos dados no banco
                CSPDVs.Current.HISTORICOS_MOTIVO.Flush();

            }
            else if (TipoMotivo == CSMotivos.CSTipoMotivo.NAO_PESQUISA_MARKETING)
            {
                if (CSPDVs.Current.MOTIVOS_NAO_PESQUISA.Current == null)
                {
                    // [ Cria um novo motivo ]
                    CSMotivosNaoPesquisa.CSMotivoNaoPesquisa motivoNaoPesquisa = new CSMotivosNaoPesquisa.CSMotivoNaoPesquisa();

                    // [ Preenche com os valores a serem salvos na classe de historico ]
                    motivoNaoPesquisa.COD_PESQUISA = CSPDVs.Current.PESQUISAS_PIV.Current.COD_PESQUISA_PIV;
                    motivoNaoPesquisa.COD_PDV = CSPDVs.Current.COD_PDV;
                    motivoNaoPesquisa.COD_MOTIVO = motivo.COD_MOTIVO;
                    motivoNaoPesquisa.COD_TIPO_MOTIVO = motivo.COD_TIPO_MOTIVO;
                    motivoNaoPesquisa.COD_EMPREGADO = CSEmpregados.Current.COD_EMPREGADO;
                    motivoNaoPesquisa.DAT_COLETA = DateTime.Now;
                    motivoNaoPesquisa.MOTIVO = motivo;

                    // [ Adiciona o historico do motivo na coleção de historicos do motivo ]
                    CSPDVs.Current.MOTIVOS_NAO_PESQUISA.Add(motivoNaoPesquisa);

                }
                else
                {
                    CSPDVs.Current.MOTIVOS_NAO_PESQUISA.Current.COD_MOTIVO = motivo.COD_MOTIVO;
                    CSPDVs.Current.MOTIVOS_NAO_PESQUISA.Current.DAT_COLETA = DateTime.Now;
                    CSPDVs.Current.MOTIVOS_NAO_PESQUISA.Current.STATE = ObjectState.ALTERADO;
                }

                // Dispara o metodo de salvamento dos dados no banco
                CSPDVs.Current.MOTIVOS_NAO_PESQUISA.Flush();

            }
            else if (TipoMotivo == CSMotivos.CSTipoMotivo.NAO_PESQUISA_MERCADO)
            {
                if (CSPDVs.Current.MOTIVOS_NAO_PESQUISA_MERCADO.Current == null)
                {
                    // [ Cria um novo motivo ]
                    CSMotivosNaoPesquisa.CSMotivoNaoPesquisa motivoNaoPesquisa = new CSMotivosNaoPesquisa.CSMotivoNaoPesquisa();

                    // [ Preenche com os valores a serem salvos na classe de historico ]
                    motivoNaoPesquisa.COD_PESQUISA = CSPDVs.Current.PESQUISA_MERCADO.Current.COD_PESQUISA_MERC;
                    motivoNaoPesquisa.COD_PDV = CSPDVs.Current.COD_PDV;
                    motivoNaoPesquisa.COD_MOTIVO = motivo.COD_MOTIVO;
                    motivoNaoPesquisa.COD_TIPO_MOTIVO = motivo.COD_TIPO_MOTIVO;
                    motivoNaoPesquisa.DAT_COLETA = DateTime.Now;
                    motivoNaoPesquisa.MOTIVO = motivo;

                    // [ Adiciona o historico do motivo na coleção de historicos do motivo ]
                    CSPDVs.Current.MOTIVOS_NAO_PESQUISA.Add(motivoNaoPesquisa);
                }
                else
                {
                    CSPDVs.Current.MOTIVOS_NAO_PESQUISA.Current.COD_MOTIVO = motivo.COD_MOTIVO;
                    CSPDVs.Current.MOTIVOS_NAO_PESQUISA.Current.DAT_COLETA = DateTime.Now;
                    CSPDVs.Current.MOTIVOS_NAO_PESQUISA.Current.STATE = ObjectState.ALTERADO;
                }

                // Dispara o metodo de salvamento dos dados no banco
                CSPDVs.Current.MOTIVOS_NAO_PESQUISA.FlushMercado();
            }
            else if (TipoMotivo == CSMotivos.CSTipoMotivo.NAO_COMPRA_PRODUTOS_INDICADOS)
            {
                CSPDVs.Current.PEDIDOS_PDV.Current.COD_MOTIVO = motivo.COD_MOTIVO;
                CSPDVs.Current.PEDIDOS_PDV.Current.STATE = ObjectState.ALTERADO;
            }
        }

       public static void GravarMotivo()
       {
            if (ValidaCampos())
            {
                Fechar();
                ((Cliente)ActivityContext).MotivoInformado = true;

                if (CSEmpresa.Current.IND_UTILIZA_FLEXX_GPS == "S" &&
                    TipoMotivo == CSMotivos.CSTipoMotivo.NAO_POSITIVACAO_CLIENTE)
                {
                    if (!string.IsNullOrEmpty(CSPDVs.Current.DSC_NOME_FOTO) &&
                        !CSPDVs.Current.BOL_FOTO_VALIDADA)
                        CSPDVs.Current.GravarImagemValidada();
                }
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            PreenchimentoMotivo();
        }

        private static bool ValidaCampos()
        {
            if (rdgMotivos.CheckedRadioButtonId == -1)
            {
                MessageBox.ShowShortMessageCenter(ActivityContext,"Selecione um motivo.");
                return false;
            }

            return true;
        }

        private void FindViewsById(View view)
        {
            rdgMotivos = view.FindViewById<RadioGroup>(Resource.Id.rdgMotivos);
            lblTitulo = view.FindViewById<TextView>(Resource.Id.lblTitulo);
        }

        private void PreenchimentoMotivo()
        {
            int codigoMotivo = -1;

            if (TipoMotivo == CSMotivos.CSTipoMotivo.NAO_POSITIVACAO_CLIENTE)
            {
                lblTitulo.Text = "Motivo não positivação";

                var HistoricoPdv = new CSHistoricosMotivo(CSPDVs.Current.COD_PDV).Cast<CSHistoricosMotivo.CSHistoricoMotivo>().FirstOrDefault();

                if (HistoricoPdv != null)
                {
                    CSPDVs.Current.HISTORICOS_MOTIVO.Current = HistoricoPdv;
                    codigoMotivo = HistoricoPdv.COD_MOTIVO;

                    CSPDVs.Current.HISTORICOS_MOTIVO.Clear();
                }
                else
                    CSPDVs.Current.HISTORICOS_MOTIVO.Current = null;
            }
            else if (TipoMotivo == CSMotivos.CSTipoMotivo.NAO_COMPRA_PRODUTOS_INDICADOS)
            {
                lblTitulo.Text = "Motivo não compra indicado(s)";

                foreach (CSMotivos.CSMOTIVO_NAO_COMPRA_PRODUTO_INDICADO motivo in CSMotivos.ItemsMotivoIndicados)
                {
                    RadioButton rdb = new RadioButton(Activity);
                    var lp = new LinearLayout.LayoutParams(WindowManagerLayoutParams.MatchParent, Resource.Dimension.widgets_height);
                    rdb.LayoutParameters = lp;
                    rdb.SetPadding(70, 0, 0, 0);
                    rdb.Text = motivo.DSC_MOTIVO.ToTitleCase();
                    rdb.Tag = motivo.COD_MOTIVO;
                    rdgMotivos.AddView(rdb);

                    if (CSPDVs.Current.PEDIDOS_PDV.Current.COD_MOTIVO.HasValue)
                    {
                        if (motivo.COD_MOTIVO == CSPDVs.Current.PEDIDOS_PDV.Current.COD_MOTIVO)
                            rdb.Checked = true;
                    }
                }
            }
            else if (TipoMotivo == CSMotivos.CSTipoMotivo.NAO_PESQUISA_MERCADO)
            {
                lblTitulo.Text = "Motivo não pesquisa mercado";

                CSPDVs.Current.MOTIVOS_NAO_PESQUISA_MERCADO.Current = null;

                int codPesquisa = -1;
                codPesquisa = CSPDVs.Current.PESQUISA_MERCADO.Current.COD_PESQUISA_MERC;

                // [ Buscar a partir do PDV se existe motivo de não pesquisa ]
                foreach (CSMotivosNaoPesquisa.CSMotivoNaoPesquisa motivo in CSPDVs.Current.MOTIVOS_NAO_PESQUISA_MERCADO)
                {
                    if (motivo.COD_TIPO_MOTIVO == TipoMotivo && codPesquisa == motivo.COD_PESQUISA)
                    {
                        CSPDVs.Current.MOTIVOS_NAO_PESQUISA_MERCADO.Current = motivo;
                        codigoMotivo = motivo.COD_MOTIVO;
                        break;
                    }
                }
            }
            else
            {
                lblTitulo.Text = "Motivo não pesquisa";

                CSPDVs.Current.MOTIVOS_NAO_PESQUISA.Current = null;


                int codPesquisa = -1;
                if (TipoMotivo == CSMotivos.CSTipoMotivo.NAO_PESQUISA_MARKETING)
                    codPesquisa = CSPDVs.Current.PESQUISAS_PIV.Current.COD_PESQUISA_PIV;
                else
                    codPesquisa = CSPDVs.Current.PESQUISA_MERCADO.Current.COD_PESQUISA_MERC;

                // [ Buscar a partir do PDV se existe motivo de não pesquisa ]
                foreach (CSMotivosNaoPesquisa.CSMotivoNaoPesquisa motivo in CSPDVs.Current.MOTIVOS_NAO_PESQUISA)
                {
                    if (motivo.COD_TIPO_MOTIVO == TipoMotivo && codPesquisa == motivo.COD_PESQUISA)
                    {
                        CSPDVs.Current.MOTIVOS_NAO_PESQUISA.Current = motivo;
                        codigoMotivo = motivo.COD_MOTIVO;
                        break;
                    }
                }
            }

            if (TipoMotivo != CSMotivos.CSTipoMotivo.NAO_COMPRA_PRODUTOS_INDICADOS)
            {
                // [ Preenche os motivos do tipo escolhido para serem selecionados ]
                foreach (CSMotivos.CSMotivo motivo in CSMotivos.Items.Cast<CSMotivos.CSMotivo>().OrderBy(c => c.DSC_MOTIVO))
                {
                    if (motivo.COD_TIPO_MOTIVO == TipoMotivo)
                    {
                        RadioButton rdb = new RadioButton(Activity);
                        var lp = new LinearLayout.LayoutParams(WindowManagerLayoutParams.MatchParent, Resource.Dimension.widgets_height);
                        rdb.LayoutParameters = lp;
                        rdb.SetPadding(70, 0, 0, 0);
                        rdb.Text = motivo.DSC_MOTIVO.ToTitleCase();
                        rdb.Tag = motivo.COD_MOTIVO;
                        rdgMotivos.AddView(rdb);

                        if (codigoMotivo == motivo.COD_MOTIVO)
                            rdb.Checked = true;
                    }
                }
            }
        }
    }
}