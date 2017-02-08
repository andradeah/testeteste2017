using System;
using System.Collections.Generic;

namespace AvanteSales.SystemFramework
{
    /// <summary>
    /// Summary description for CSListViewItem.
    /// </summary>
    public class CSListViewItem
#if ANDROID
 : Java.Lang.Object
#else
        :  System.Windows.Forms.ListViewItem
#endif

    {

        #region [ Variaveis ]

        private object valor;

        #endregion

        public CSListViewItem()
        {
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
#if ANDROID
        public string Text { get; set; }
        public List<object> SubItems { get; set; }
        public int ImageIndex { get; set; }
#endif

    }
}
