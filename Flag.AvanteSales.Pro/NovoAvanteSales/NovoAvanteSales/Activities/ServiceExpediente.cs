using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AvanteSales.Pro.Dialogs;
using Java.Lang;

namespace AvanteSales.Pro.Activities
{
    [Service]
    public class ServiceExpediente : Service
    {
        private Timer Timer { get; set; }
        private Handler handler;
        int ExpedienteRestante;

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            handler = new Handler();
            ExpedienteRestante = CSEmpregados.Current.TempoExpedienteRestante();

            if (ExpedienteRestante >= 30)
            {
                var tempoPara30min = RetornaTempoTimer(ExpedienteRestante, 30);

                ExpedienteRestante = 30;

                Timer = new Timer(PrimeiroAviso, null, new TimeSpan(0, tempoPara30min, 0), new TimeSpan(0, 0, 0));
            }
            else if (ExpedienteRestante < 30 && ExpedienteRestante >= 16)
            {
                var tempoPara15min = RetornaTempoTimer(ExpedienteRestante, 15);

                ExpedienteRestante = 15;

                Timer = new Timer(DemaisAvisos, null, new TimeSpan(0, tempoPara15min, 0), new TimeSpan(0, 1, 0));
            }
            else if (ExpedienteRestante > 0)
            {
                Timer = new Timer(DemaisAvisos, null, new TimeSpan(0, 0, 0), new TimeSpan(0, 1, 0));
            }
            else
                FinalizarExpediente();

            return base.OnStartCommand(intent, flags, startId);
        }

        public int RetornaTempoTimer(int tempoExpediente, int constante)
        {
            int repeticao = tempoExpediente;
            int resultado = 0;

            for (int valor = repeticao; valor > constante; valor--)
            {
                resultado++;
            }

            return resultado;
        }

        private void PrimeiroAviso(object state)
        {
            handler.Post(new Action(MensagemExpedienteRestante));

            ExpedienteRestante = 15;

            Timer = new Timer(DemaisAvisos, null, new TimeSpan(0, 15, 0), new TimeSpan(0, 1, 0));
        }

        //Avisos de 15 minutos e menos.
        private void DemaisAvisos(object state)
        {
            if (ExpedienteRestante > 0)
            {
                handler.Post(new Action(MensagemExpedienteRestante));

                ExpedienteRestante--;
            }
            else
                FinalizarExpediente();
        }

        private void FinalizarExpediente()
        {
            if (Timer != null)
                Timer.Dispose();

            CSEmpregados.Current.FinalizarExpediente();

            if (Cliente.monitoramento != null &&
                Cliente.monitoramento.DAT_SAIDA.Date == new DateTime(1900, 1, 1))
                Cliente.Fechar(false);

            handler.Post(new Action(MensagemExpedienteEncerrado));
        }

        private void MensagemExpedienteEncerrado()
        {
            Intent i = new Intent();
            i.SetClass(this, typeof(ServiceMessageBox));
            i.SetFlags(ActivityFlags.NewTask);
            this.StartActivity(i);
        }

        private void MensagemExpedienteRestante()
        {
            if (CSEmpregados.Current.ExpedienteFinalizado())
            {
                if (Timer != null)
                    Timer.Dispose();
            }
            else
            {
                string mensagem = string.Format("Atenção! Faltam {0} minutos para o fim do seu expediente.", ExpedienteRestante == 0 ? 1 : ExpedienteRestante);

                Notification.Builder builder = new Notification.Builder(this)
                     .SetContentTitle("Aviso de Expediente")
                     .SetContentText(mensagem)
                     .SetSmallIcon(Resource.Drawable.ic_avante_sales_pro)
                     .SetDefaults(NotificationDefaults.Sound | NotificationDefaults.Vibrate);

                Notification notification = builder.Build();

                NotificationManager notificationManager =
                    GetSystemService(Context.NotificationService) as NotificationManager;

                const int notificationId = 0;
                notificationManager.Notify(notificationId, notification);

                Toast toast = Toast.MakeText(this, mensagem, ToastLength.Long);
                View view = toast.View;
                view.SetBackgroundResource(Resource.Drawable.background_toast);
                toast.SetGravity(GravityFlags.CenterVertical, 0, 0);
                toast.Show();
            }
        }
    }
}