using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AvanteSales.Pro.Dialogs;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "Expediente", ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AvanteSalesTheme")]
    public class Expediente : AppCompatActivity
    {
        Android.Support.V7.Widget.Toolbar tbToolbar;
        TextView lblVendedor;
        TextView lblData;
        TextView lblInicioExpediente;
        TextView lblFimExpediente;
        TextView lblInicioAlmoco;
        TextView lblFimAlmoco;
        TableLayout tblAlmoco;
        Button btnAlmoco;
        Button btnExpediente;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.expediente);

            FindViewsById();

            Eventos();

            SetSupportActionBar(tbToolbar);

            Inicializacao();
        }

        private void Eventos()
        {
            try
            {
                if (CSEmpresa.ColunaExiste("EMPREGADO_EXPEDIENTE", "COD_EMPREGADO"))
                    btnAlmoco.Click += BtnAlmoco_Click;
                else
                    btnAlmoco.Visibility = ViewStates.Gone;

                btnExpediente.Click += BtnExpediente_Click;
            }
            catch (Exception ex)
            {
                MessageBox.AlertErro(this, ex.Message);
            }
        }

        private void BtnExpediente_Click(object sender, EventArgs e)
        {
            if (CSEmpregados.Current.ExpedienteAnteriorExistente())
            {
                MessageBox.Alert(this, "Voc� possui um expediente que ainda n�o foi finalizado. Para prosseguir, voc� deve finaliz�-lo. Deseja finalizar?", "Finalizar",
                    (_sender, _e) =>
                    {
                        CSEmpregados.Current.FinalizarExpedienteAnterior();
                        RotinaExpediente();
                    }, "Cancelar", (_sender, _e) => { }, true);
            }
            else
                RotinaExpediente();
        }

        private void RotinaExpediente()
        {
            if (CSEmpregados.Current.ExpedienteIniciadoNaoFinalizado())
            {
                if (CSEmpregados.Current.AlmocoIniciadoNaoFinalizado())
                {
                    MessageBox.AlertErro(this, "N�o � permitido finalizar expediente sem finalizar seu hor�rio de almo�o.");
                }
                else
                {
                    MessageBox.Alert(this, "Deseja finalizar o seu expediente?", "Finalizar",
                        (_sender, _e) =>
                        {
                            if (CSEmpregados.Current.HorarioExpedienteEncerrado())
                            {
                                FinalizarExpediente();
                                Intent i = new Intent();
                                i.SetClass(this, typeof(ServiceExpediente));
                                StopService(i);
                            }
                            else
                                MessageBox.Alert(this, string.Format("Falta(m) aproximadamente {0} minuto(s) para o encerramento do seu hor�rio de expediente. Deseja continuar e finalizar sua jornada de trabalho?", CSEmpregados.Current.TempoExpedienteRestante()),
                                    "Continuar e finalizar",
                                    (_sender1, _e1) =>
                                    {
                                        FinalizarExpediente();
                                        Intent i = new Intent();
                                        i.SetClass(this, typeof(ServiceExpediente));
                                        StopService(i);

                                    }, "Cancelar", (_sender2, _e2) => { }, false);
                        }, "Cancelar",
                        (_sender, _e) => { }, true);
                }
            }
            else
            {
                MessageBox.Alert(this, "A opera��o de venda ser� habilitada ap�s in�cio do expediente. Deseja iniciar seu expediente?", "Iniciar",
                    (_sender, _e) =>
                    {
                        CSEmpregados.Current.IniciarExpediente();
                        TrocarTextoJornada();

                        if (CSEmpregados.Current.IND_FINALIZA_JORNADA_AUTOMATICA)
                        {
                            Intent i = new Intent();
                            i.SetClass(this, typeof(ServiceExpediente));
                            StartService(i);
                        }

                    }, "Cancelar",
                        (_sender, _e) => { }, true);
            }
        }

        private void FinalizarExpediente()
        {
            CSEmpregados.Current.FinalizarExpediente();
            TrocarTextoJornada();

            if (!CSEmpregados.Current.AlmocoIniciado())
                MessageBox.Alert(this, "Expediente finalizado sem in�cio de hor�rio de almo�o.");
            else
                MessageBox.Alert(this, "Expediente finalizado. Voc� n�o pode efetuar opera��o de venda.");
        }

        private void TrocarTextoJornada()
        {
            try
            {
                if (CSEmpregados.Current.ExpedienteIniciadoNaoFinalizado())
                {
                    lblInicioExpediente.Text = CSEmpregados.Current.DAT_INICIO_TRABALHO.Value.TimeOfDay.ToString().Substring(0, 5);
                    lblFimExpediente.Text = "-";
                    btnExpediente.Text = "Finalizar Expediente";
                }
                else if (CSEmpregados.Current.ExpedienteFinalizado())
                {
                    lblInicioExpediente.Text = CSEmpregados.Current.DAT_INICIO_TRABALHO.Value.TimeOfDay.ToString().Substring(0, 5);
                    lblFimExpediente.Text = CSEmpregados.Current.DAT_FIM_TRABALHO.Value.TimeOfDay.ToString().Substring(0, 5);
                    btnExpediente.Text = "Expediente Finalizado";
                    btnExpediente.Enabled = false;
                }
                else
                {
                    btnExpediente.Text = "Iniciar Expediente";
                }
            }
            catch (Exception ex)
            {
                MessageBox.AlertErro(this, ex.Message);
            }
        }

        private void BtnAlmoco_Click(object sender, EventArgs e)
        {
            try
            {
                string alerta = string.Empty;

                if (!CSEmpregados.Current.VendedorDentroExpediente(ref alerta))
                {
                    MessageBox.AlertErro(this, alerta);
                    return;
                }
                else
                {
                    if (CSEmpregados.Current.AlmocoIniciadoNaoFinalizado())
                    {
                        MessageBox.Alert(this, "Deseja finalizar o hor�rio de almo�o?", "Finalizar",
                            (_sender, _e) =>
                            {
                                if (CSEmpregados.Current.IntervaloAlmocoEncerrado())
                                {
                                    CSEmpregados.Current.FinalizarAlmoco();
                                    TrocarTextoAlmoco();
                                    MessageBox.Alert(this, "Almo�o finalizado. Opera��o de venda habilitada novamente.");
                                }
                                else
                                    MessageBox.Alert(this, string.Format("Falta(m) aproximadamente {0} minuto(s) para o encerramento do seu hor�rio de almo�o.", CSEmpregados.Current.TempoAlmocoRestante()));
                            }, "Cancelar",
                            (_sender, _e) => { }, true);
                    }
                    else
                    {
                        MessageBox.Alert(this, "Voc� n�o poder� realizar fun��o de venda dentro do hor�rio de almo�o. Deseja continuar?", "Continuar",
                            (_sender, _e) =>
                            {
                                CSEmpregados.Current.IniciarAlmoco();
                                TrocarTextoAlmoco();
                            }, "Cancelar",
                                (_sender, _e) => { }, true);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.AlertErro(this, ex.Message);
            }
        }

        private void TrocarTextoAlmoco()
        {
            try
            {
                if (CSEmpregados.Current.AlmocoIniciadoNaoFinalizado())
                {
                    tblAlmoco.Visibility = ViewStates.Visible;
                    lblInicioAlmoco.Text = CSEmpregados.Current.DAT_INICIO_ALMOCO.Value.TimeOfDay.ToString().Substring(0, 5);
                    lblFimAlmoco.Text = "-";
                    btnAlmoco.Text = "Finalizar Almo�o";
                }
                else if (CSEmpregados.Current.AlmocoFinalizado())
                {
                    tblAlmoco.Visibility = ViewStates.Visible;
                    lblInicioAlmoco.Text = CSEmpregados.Current.DAT_INICIO_ALMOCO.Value.TimeOfDay.ToString().Substring(0, 5);
                    lblFimAlmoco.Text = CSEmpregados.Current.DAT_FIM_ALMOCO.Value.TimeOfDay.ToString().Substring(0, 5);
                    btnAlmoco.Text = "Almo�o Finalizado";
                    btnAlmoco.Enabled = false;
                }
                else
                {
                    tblAlmoco.Visibility = ViewStates.Gone;
                    btnAlmoco.Text = "Iniciar Almo�o";
                }
            }
            catch (Exception ex)
            {
                MessageBox.AlertErro(this, ex.Message);
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.Finish();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void Inicializacao()
        {
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(false);

            lblVendedor.Text = string.Format("{0} - {1}", CSEmpregados.Current.COD_EMPREGADO, CSEmpregados.Current.NOM_EMPREGADO);
            lblData.Text = DateTime.Now.ToString("dd/MM/yy");

            if (CSEmpregados.Current.NUM_MINUTOS_TOTAL_EXPEDIENTE == 0)
                btnExpediente.Visibility = ViewStates.Gone;

            ValidarFimAlmoco();

            PreencherCampos();
        }

        private void ValidarFimAlmoco()
        {
            if (CSEmpregados.Current.AlmocoIniciadoNaoFinalizado())
            {
                if (CSEmpregados.Current.NUM_MINUTOS_INTERVALO_ALMOCO > 0)
                {
                    if (CSEmpregados.Current.IntervaloAlmocoEncerrado())
                        CSEmpregados.Current.FinalizarAlmoco();
                }
            }
        }

        private void PreencherCampos()
        {
            try
            {
                if (CSEmpregados.Current.DAT_HORA_INICIO_EXPEDIENTE.HasValue)
                {
                    lblInicioExpediente.Text = CSEmpregados.Current.DAT_HORA_INICIO_EXPEDIENTE.Value.TimeOfDay.ToString().Substring(0, 5);
                    lblFimExpediente.Text = CSEmpregados.Current.DAT_HORA_FIM_EXPEDIENTE.Value.TimeOfDay.ToString().Substring(0, 5);
                }
                else
                {
                    lblInicioExpediente.Text = "-";
                    lblFimExpediente.Text = "-";
                }

                TrocarTextoAlmoco();
                TrocarTextoJornada();
            }
            catch (Exception ex)
            {
                MessageBox.AlertErro(this, ex.Message);
            }
        }

        private void FindViewsById()
        {
            tbToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tbToolbar);
            lblVendedor = FindViewById<TextView>(Resource.Id.lblVendedor);
            lblData = FindViewById<TextView>(Resource.Id.lblData);
            lblInicioExpediente = FindViewById<TextView>(Resource.Id.lblInicioExpediente);
            lblFimExpediente = FindViewById<TextView>(Resource.Id.lblFimExpediente);
            lblInicioAlmoco = FindViewById<TextView>(Resource.Id.lblInicioAlmoco);
            lblFimAlmoco = FindViewById<TextView>(Resource.Id.lblFimAlmoco);
            tblAlmoco = FindViewById<TableLayout>(Resource.Id.tblAlmoco);
            btnAlmoco = FindViewById<Button>(Resource.Id.btnAlmoco);
            btnExpediente = FindViewById<Button>(Resource.Id.btnExpediente);
        }
    }
}