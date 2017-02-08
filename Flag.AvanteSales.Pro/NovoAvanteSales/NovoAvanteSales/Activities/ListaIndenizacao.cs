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

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "ListaIndenizacao", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ListaIndenizacao : AppCompatActivity
    {
        TextView lblCodPdv;
        TextView lblNomePdv;
        Android.Support.V7.Widget.Toolbar tbToolbar;
        ListView lvwIndenizacao;
        Button btnNovaIndenizacao;
        private int idxIndenizacao = -1;
        private bool _IsDirty;

        private bool IsDirty
        {
            get
            {
                return _IsDirty;
            }
            set
            {
                _IsDirty = value;
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            IsDirty = true;

            SetContentView(Resource.Layout.lista_indenizacao);

            FindViewsById();

            lblCodPdv.Text = CSPDVs.Current.COD_PDV.ToString();
            lblNomePdv.Text = CSPDVs.Current.DSC_RAZAO_SOCIAL;

            CarregarIndenizacoes();

            VerificarNovaIndenizacao();

            btnNovaIndenizacao.Click += new EventHandler(btnNovaIndenizacao_Click);
            lvwIndenizacao.ItemClick += LvwIndenizacao_ItemClick;

            SetSupportActionBar(tbToolbar);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void LvwIndenizacao_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            idxIndenizacao = e.Position;

            CSPDVs.Current.PEDIDOS_INDENIZACAO.Current = (CSIndenizacoes.CSIndenizacao)lvwIndenizacao.Adapter.GetItem(e.Position);

            if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.IND_DESCARREGADO == 0)
            {
                Android.Support.V7.App.AlertDialog.Builder b = new Android.Support.V7.App.AlertDialog.Builder(this);
                b.SetTitle(string.Format("Indenização código: {0}", CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.COD_INDENIZACAO));
                b.SetMessage("Seleciona a opção desejada");
                b.SetPositiveButton("Editar", MostraDialogOpcoesIndenizacao_Click_Editar);
                b.SetNegativeButton("Excluir", MostraDialogOpcoesIndenizacao_Click_Excluir);
                b.Show();
            }
            else
            {
                Android.Support.V7.App.AlertDialog.Builder b = new Android.Support.V7.App.AlertDialog.Builder(this);
                b.SetTitle(string.Format("Indenização código: {0}", CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.COD_INDENIZACAO));
                b.SetMessage("Indenização já descarregada.Não é possível editar e excluir.");
                b.Show();
            }
        }

        void btnNovaIndenizacao_Click(object sender, EventArgs e)
        {
            MostraIndenizacao(new CSIndenizacoes.CSIndenizacao());
        }

        public override void OnBackPressed()
        {
            CSPDVs.Current.PEDIDOS_INDENIZACAO.Current = null;
            base.OnBackPressed();
        }

        private void CarregarIndenizacoes()
        {
            lvwIndenizacao.Adapter = null;
            if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Items.Count > 0)
            {
                ListarIndenizacoes();
            }
            else
                MostraIndenizacao(new CSIndenizacoes.CSIndenizacao());

            lvwIndenizacao.SetSelection(CSPDVs.Current.PEDIDOS_PDV.PEDIDO_POSITION);
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (!IsDirty)
                ListarIndenizacoes();

            IsDirty = false;
        }

        private void ListarIndenizacoes()
        {
            // Lista as indenizações existentes do PDV
            var indenizacoesExistentes = CSPDVs.Current.PEDIDOS_INDENIZACAO.Items.Cast<CSIndenizacoes.CSIndenizacao>().Where(p => p.STATE != ObjectState.DELETADO).ToList();
            lvwIndenizacao.Adapter = new ListaIndenizacaoAdapter(this, Resource.Layout.lista_indenizacao_row, indenizacoesExistentes);
        }

        private void VerificarNovaIndenizacao()
        {
            if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Items.Count == 0)
            {
                // Mostra a tela de indenização
                MostraIndenizacao(new CSIndenizacoes.CSIndenizacao());
            }
        }

        private void MostraIndenizacao(CSIndenizacoes.CSIndenizacao indenizacao)
        {
            if (indenizacao.COD_INDENIZACAO == -1)
            {
                // Adiciona um novo pedido na coleção de Pedidos do PDV
                indenizacao.STATE = ObjectState.NOVO;
                idxIndenizacao = CSPDVs.Current.PEDIDOS_INDENIZACAO.Add(indenizacao);
            }
            // Seta qual é a indenizacao atual

            CSPDVs.Current.PEDIDOS_INDENIZACAO.Current = indenizacao;

            Intent i = new Intent();
            i.SetClass(this, typeof(ColetarIndenizacao));
            this.StartActivity(i);
        }

        private void FindViewsById()
        {
            lblCodPdv = FindViewById<TextView>(Resource.Id.lblCodPdv);
            lblNomePdv = FindViewById<TextView>(Resource.Id.lblNomePdv);
            tbToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tbToolbar);
            lvwIndenizacao = FindViewById<ListView>(Resource.Id.lvwIndenizacao);
            btnNovaIndenizacao = FindViewById<Button>(Resource.Id.btnNovaIndenizacao);
        }

        protected void MostraDialogOpcoesIndenizacao_Click_Editar(object sender, DialogClickEventArgs e)
        {
            CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.STATE = ObjectState.ALTERADO;
            // Mostra atela de pedido passando um pedido
            MostraIndenizacao(CSPDVs.Current.PEDIDOS_INDENIZACAO.Current);
        }

        protected void MostraDialogOpcoesIndenizacao_Click_Excluir(object sender, DialogClickEventArgs e)
        {
            if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.STATE != ObjectState.NOVO)
            {
                ((CSIndenizacoes.CSIndenizacao)lvwIndenizacao.Adapter.GetItem(idxIndenizacao)).STATE = ObjectState.DELETADO;

                // Flush apagando a indenização exlcuido
                CSPDVs.Current.PEDIDOS_INDENIZACAO.Flush();

                ListarIndenizacoes();
            }
        }

        class ListaIndenizacaoAdapter : ArrayAdapter<CSIndenizacoes.CSIndenizacao>
        {
            Context context;
            IList<CSIndenizacoes.CSIndenizacao> indenizacoes;
            int resourceId;

            public ListaIndenizacaoAdapter(Context c, int textViewResourceId, IList<CSIndenizacoes.CSIndenizacao> objects)
                : base(c, textViewResourceId, objects)
            {
                context = c;
                indenizacoes = objects;
                resourceId = textViewResourceId;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSIndenizacoes.CSIndenizacao indenizacao = indenizacoes[position];

                LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                View linha = layout.Inflate(resourceId, null);

                if (indenizacao != null)
                {
                    TextView lblPedido = linha.FindViewById<TextView>(Resource.Id.lblPedido);
                    TextView lblStatus = linha.FindViewById<TextView>(Resource.Id.lblStatus);
                    TextView lblNota = linha.FindViewById<TextView>(Resource.Id.lblNota);
                    TextView lblValor = linha.FindViewById<TextView>(Resource.Id.lblValor);

                    lblPedido.Text = indenizacao.COD_INDENIZACAO.ToString();
                    lblStatus.Text = indenizacao.STATUS;
                    lblNota.Text = indenizacao.NUM_NOTA_DEVOLUCAO.ToString();
                    lblValor.Text = indenizacao.VLR_TOTAL.ToString(CSGlobal.DecimalStringFormat);
                }
                return linha;
            }
        }
    }
}