using BleSample.Services.DeviceBluetoothLe.Device;
using System;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace BleSample.Converters
{
    public class DeviceConnectionStateToVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IDevice bleDevice)
            {
                if (parameter is string state)
                {
                    var parts = state.Split(' ');
                    var result = parts.Any(p => p.Trim().Equals(bleDevice.ConnectState.ToString(), StringComparison.InvariantCultureIgnoreCase));
                    return result;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
