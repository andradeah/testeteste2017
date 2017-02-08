using System;

namespace AvanteSales.SystemFramework
{
    /// <summary>
    /// Summary description for ItemCombo.
    /// </summary>
    public class CSItemCombo
#if ANDROID
 : Java.Lang.Object
#endif
    {

        #region [ Variaveis ]

        private object valor;
        private string texto;

        #endregion

        public CSItemCombo()
        {
        }

        public CSItemCombo(string texto, object valor)
        {
            this.Texto = texto;
            this.Valor = valor;
        }

        public object Valor
        {
            get
            {
                return valor;
            }
            set
            {
                valor = value;
            }
        }

        public string Texto
        {
            get
            {
                return texto;
            }
            set
            {
                texto = value;
            }
        }

        public override string ToString()
        {
            return Texto;
        }

        public override bool Equals(object obj)
        {
            if (obj is CSItemCombo)
            {
                return ((CSItemCombo)obj).Valor.Equals(Valor);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
