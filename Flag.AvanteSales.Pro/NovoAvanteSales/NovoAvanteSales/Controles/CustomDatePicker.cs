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

namespace AvanteSales.Pro.Controles
{
    public class CustomDatePicker : EditText
    {
        private DateTime _data;
        private DateTime Data
        {
            get
            {
                if (_data == DateTime.MinValue)
                {
                    if (Text == string.Empty)
                        Text = DateTime.Now.ToString("dd/MM/yyyy");

                    DateTime data;
                    if (DateTime.TryParse(Text, out data))
                    {
                        _data = data;
                    }
                    else
                    {
                        Toast.MakeText(_context, "Formato de data inválido!", ToastLength.Short).Show();
                    }
                }
                return _data;
            }
            set
            {
                _data = value;
            }
        }

        private Context _context;

        public CustomDatePicker(Context context)
            : base(context)
        {
            DefaultConstructor(context);
        }

        public CustomDatePicker(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            DefaultConstructor(context);
        }

        public CustomDatePicker(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            DefaultConstructor(context);
        }

        void DefaultConstructor(Context context)
        {
            _context = context;
            Click += new EventHandler(btnEscolherData_Click);
        }

        void btnEscolherData_Click(object sender, EventArgs e)
        {
            DateSetEvent += new EventHandler<DatePickerDialog.DateSetEventArgs>(OnDateSet);
            DatePickerDialog dialog = new DatePickerDialog(_context, DateSetEvent, Data.Year, Data.Month - 1, Data.Day);
            dialog.Show();
        }

        public delegate void DateSetHandler(Object view, DatePickerDialog.DateSetEventArgs e);

        public EventHandler<DatePickerDialog.DateSetEventArgs> DateSetEvent;

        public void OnDateSet(Object view, DatePickerDialog.DateSetEventArgs e)
        {
            Data = new DateTime(e.Year, e.Month + 1, e.DayOfMonth);
            UpdateDisplay();

        }

        private void UpdateDisplay()
        {

            SetText(Data.ToString("dd/MM/yyyy"), TextView.BufferType.Normal);
            //new StringBuilder()
            //    .Append(_dia).Append("/")
            //    .Append(_mes + 1).Append("/")
            //    .Append(_ano).ToString(), TextView.BufferType.Normal);
        }
    }
}