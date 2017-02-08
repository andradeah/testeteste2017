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
using System.Collections;
using System.Reflection;

namespace AvanteSales.Pro.Formatters
{
    public static class Formatters
    {
        public static string ToCNPJ(this string cnpj)
        {
            try
            {
                switch (cnpj.Length)
                {
                    case 14:
                        return long.Parse(cnpj).ToString("00\\.000\\.000\\/0000\\-00");
                    case 15:
                        return long.Parse(cnpj).ToString("#00\\.000\\.000\\/0000\\-00");
                    default:
                        return cnpj;
                }
            }
            catch (Exception)
            {
                return cnpj;
            }
        }

        public static string ToInscSocial(this string inscSocial, string uf)
        {
            string mascara = string.Empty;
            switch (uf)
            {
                case "TO":
                    mascara = "00000000000";
                    break;
                case "DF":
                    mascara = "00000000000\\-00";
                    break;
                case "MT":
                    mascara = "0000000000\\-0";
                    break;
                case "SE":
                    mascara = "000000000\\-0";
                    break;
                case "AL":
                case "AP":
                case "ES":
                case "MA":
                case "MS":
                case "PI":
                    mascara = "000000000";
                    break;
                case "RR":
                case "CE":
                case "PB":
                    mascara = "00000000\\-0";
                    break;
                case "PR":
                    mascara = "00000000\\-00";
                    break;
                case "BA":
                    mascara = "000000\\-00";
                    break;
                case "MG":
                    mascara = "000\\.000\\.000/0000";
                    break;
                case "RO":
                    mascara = "000\\.00000\\-0";
                    break;
                case "RS":
                    mascara = "000\\/0000000";
                    break;
                case "SC":
                    mascara = "000\\.000\\.000";
                    break;
                case "SP":
                    mascara = "000\\.000\\.000\\.000";
                    break;
                case "AC":
                    mascara = "00\\.000\\.000\\/000\\-00";
                    break;
                case "AM":
                case "RN":
                case "GO":
                    mascara = "00\\.000\\.000\\-0";
                    break;
                case "PA":
                    mascara = "00\\-000000\\-0";
                    break;
                case "RJ":
                    mascara = "00\\.000\\.00\\-0";
                    break;
                case "PE":
                    mascara = "00\\.0\\.000\\.0000000\\-0";
                    break;
            }

            if (!string.IsNullOrEmpty(mascara))
            {
                try
                {
                    if (inscSocial.Length.Equals(mascara.Count(c => c == '0')))
                    {
                        return long.Parse(inscSocial).ToString(mascara);
                    }
                }
                catch (Exception)
                {
                    return inscSocial;
                }
            }
            return inscSocial;
        }
    }
}