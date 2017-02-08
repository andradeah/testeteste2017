using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Globalization;

namespace AvanteSales.Pro.Formatters
{
	public static class StringFormatter
	{
		public static string ToTitleCase(this string str)
		{
			return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
		}

		public static bool NaoNumerico(string Texto)
		{
			int i = 0;

			if (!int.TryParse(Texto, out i))
				return true;

			else
				return false;
		}

		public static bool NaoDecimal(string Texto)
		{
			decimal x = 0;

			if (!decimal.TryParse(Texto, out x))
				return true;

			else
				return false;
		}
	}
}