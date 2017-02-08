using System;
using System.Collections.Generic;
using System.Text;
using Android.Views;
using Android.Graphics;
using Android.Content;
using Android.Util;
using AvanteSales.Pro.Activities;
using AvanteSales.Pro.Fragments;

namespace AvanteSales.Pro.Controles
{
    public class Draw : View
    {
        private Paint escritoPreto;
        private Paint corBranca;
        private Paint corGrafico;
        int HeightMaximo;
        int WidthMaximo;
        
        /// <summary>
        /// Quadrado
        /// </summary>
        /// <param name="context"></param>
        public Draw(Context context)
            : base(context)
        {
            this.Focusable = true;            
        }

        public Draw(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            
        }

        public Draw(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle) { }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            HeightMaximo = Convert.ToInt32(ResumoPesoPedidoGrafico.height / 2.2);
            WidthMaximo = ResumoPesoPedidoGrafico.width;

            //Configuração para o aparelho Defy
            if (HeightMaximo == 427)
                HeightMaximo = 350;
            else if (HeightMaximo < 427)
                HeightMaximo = 350 - (427 - HeightMaximo);

            int Height = 50;
            int Width = WidthMaximo / 4;

            escritoPreto = new Paint();
            escritoPreto.SetARGB(255, 0, 0, 0);
            escritoPreto.TextSize = 50;

            corBranca = new Paint();
            corBranca.SetARGB(255, 255, 255, 255);
            
            canvas.DrawRect(Width, Height, WidthMaximo - Width, HeightMaximo, corBranca);

            if (ResumoPesoPedidoGrafico.lblOcupacaoVeiculo.Text != "-")
            {
                decimal PesoBruto = Convert.ToDecimal(ResumoPesoPedidoGrafico.lblPesoBruto.Text);
                float ParametroPctOcupacao = (float)Convert.ToDecimal(ResumoPesoPedidoGrafico.lblPctOcupacao.Text);
                decimal OcupacaoVeiculo = Convert.ToDecimal(ResumoPesoPedidoGrafico.lblOcupacaoVeiculo.Text);

                float PorcentagemOcupacaoPesoBrutoNoCaminhao = (float)Math.Round((100 * PesoBruto) / OcupacaoVeiculo, 2);

                ResumoPesoPedidoGrafico.lblPctOcupacaoVeiculo.Text = PorcentagemOcupacaoPesoBrutoNoCaminhao.ToString();

                corGrafico = new Paint();

                if (PorcentagemOcupacaoPesoBrutoNoCaminhao < ParametroPctOcupacao)
                    corGrafico.SetARGB(255, 255, 0, 0);
                else
                    corGrafico.SetARGB(255, 0, 0, 255);

                float TamanhoGraficoDivididoPorCem = (float)((HeightMaximo - Height) / 100m);

                corGrafico.SetShader(new LinearGradient(0, HeightMaximo, 0, HeightMaximo - (TamanhoGraficoDivididoPorCem * PorcentagemOcupacaoPesoBrutoNoCaminhao), PorcentagemOcupacaoPesoBrutoNoCaminhao < ParametroPctOcupacao ? Color.DarkRed : Color.Blue, PorcentagemOcupacaoPesoBrutoNoCaminhao < ParametroPctOcupacao ? Color.OrangeRed : Color.LightBlue, Shader.TileMode.Clamp));

                canvas.DrawRect(Width, HeightMaximo - (TamanhoGraficoDivididoPorCem * PorcentagemOcupacaoPesoBrutoNoCaminhao), WidthMaximo - Width, HeightMaximo, corGrafico);

                canvas.DrawText(PorcentagemOcupacaoPesoBrutoNoCaminhao.ToString() + "%", ((WidthMaximo - Width) - Width) - (((WidthMaximo - Width) - Width) / 6), HeightMaximo - ((HeightMaximo - Height) / 2), escritoPreto);
            }
            else
                canvas.DrawText("0%", ((WidthMaximo - Width) - Width) - (((WidthMaximo - Width) - Width) / 6), HeightMaximo - ((HeightMaximo - Height) / 2), escritoPreto);
        }
    }
}
