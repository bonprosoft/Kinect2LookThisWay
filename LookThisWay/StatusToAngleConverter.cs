using LookThisWay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LookThisWay
{
    [ValueConversion(typeof(GameStatus), typeof(int))]
    class StatusToAngleConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType,
          object parameter, System.Globalization.CultureInfo culture)
        {
            // GameStatus -> string
            var v = (GameStatus)value;
            return ((int)v - 1) * 90;
        }

        public object ConvertBack(object value, System.Type targetType,
          object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
