using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace 上位机温度控制系统.Converter
{
    public class IsOpenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isOpen = (bool)value;
            if (isOpen)
            {
                return "关闭";
            }
            else
                return "打开";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = (string)value;
            if (status == "打开")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
