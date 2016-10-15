using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Synapse.Core;

namespace Synapse.Wpf.Dialogs
{
    public class ResultStatusColorConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color color = Colors.DarkGray;

            if( value == null )
                return color;

            StatusType status = (StatusType)Enum.Parse( typeof( StatusType ), value.ToString(), true );
            switch( status )
            {
                case StatusType.New:
                case StatusType.Initializing:
                {
                    color = Colors.Blue;
                    break;
                }
                case StatusType.Running:
                case StatusType.Waiting:
                {
                    color = Colors.Black;
                    break;
                }
                case StatusType.Complete:
                {
                    color = Colors.Green;
                    break;
                }
                case StatusType.CompletedWithErrors:
                {
                    color = Colors.Orange;
                    break;
                }
                case StatusType.Failed:
                {
                    color = Colors.Red;
                    break;
                }
                default:
                {
                    color = Colors.DarkGray;
                    break;
                }
            }

            return color.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
