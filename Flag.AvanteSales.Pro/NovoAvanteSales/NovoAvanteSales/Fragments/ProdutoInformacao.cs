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
using OxyPlot.Xamarin.Android;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace AvanteSales.Pro.Fragments
{
    public class ProdutoInformacao : Android.Support.V4.App.Fragment
    {

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.produto_informacao, container, false);

            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            PlotView plot = view.FindViewById<PlotView>(Resource.Id.plot_view);
            //PlotView plot2 = view.FindViewById<PlotView>(Resource.Id.plot_view2);

            //plot.Model = CreatePlotModel();
            plot.Model = CreatePlotModel2();
        }

        private PlotModel CreatePlotModel2()
        {
            var model = new PlotModel
            {
                Title = "Quantidade vendida",
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendBorderThickness = 0
            };

            var s1 = new BarSeries { Title = "Quantidade", FillColor = OxyColor.Parse("#03a9f4"), StrokeColor = OxyColors.Black, StrokeThickness = 1, LabelPlacement = LabelPlacement.Inside, LabelFormatString = "{0}" };
            s1.Items.Add(new BarItem { Value = 25 });
            s1.Items.Add(new BarItem { Value = 137 });
            s1.Items.Add(new BarItem { Value = 18 });
            s1.Items.Add(new BarItem { Value = 40 });

            var categoryAxis = new CategoryAxis { Position = AxisPosition.Left };
            categoryAxis.IsZoomEnabled = false;
            categoryAxis.IsPanEnabled = false;
           
            categoryAxis.Labels.Add("15/01/17");
            categoryAxis.Labels.Add("15/12/16");
            categoryAxis.Labels.Add("15/11/16");
            categoryAxis.Labels.Add("Atual");
            var valueAxis = new LinearAxis { Position = AxisPosition.Bottom, MinimumPadding = 0, MaximumPadding = 0.06, AbsoluteMinimum = 0 };
            model.Series.Add(s1);
            //model.Series.Add(s2);
            model.Axes.Add(categoryAxis);
            model.Axes.Add(valueAxis);

            return model;
        }

        private PlotModel CreatePlotModel()
        {
            var plotModel = new PlotModel { Title = "Percentual de venda" };

            dynamic seriesP1 = new PieSeries { StrokeThickness = 2.0, InsideLabelPosition = 0.8, AngleSpan = 360, StartAngle = 0 };

            seriesP1.Slices.Add(new PieSlice("20/01/17", 1030) { IsExploded = false, Fill = OxyColors.PaleVioletRed });
            seriesP1.Slices.Add(new PieSlice("20/12/16", 929) { IsExploded = true });
            seriesP1.Slices.Add(new PieSlice("20/11/16", 4157) { IsExploded = true });
            seriesP1.Slices.Add(new PieSlice("Atual", 739) { IsExploded = true });

            plotModel.Series.Add(seriesP1);

            return plotModel;
        }
    }
}