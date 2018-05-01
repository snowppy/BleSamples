using BleSample.Services.DeviceBluetoothLe.Device;
using Prism.Commands;
using System.Collections.ObjectModel;

namespace BleSample.ViewModels
{
    public interface IDeviceInformationPageViewModel
    {
        ObservableCollection<string> AvailableDevices { get; set; }
        string SelectedDeviceKey { get; set; }
        IDevice SelectedDevice { get; set; }
        //bool DeterminingBatteryLevel { get; set; }
        //double BatteryLevel { get; set; }

        bool Connecting { get; set; }
        bool Disconnecting { get; set; } 

        bool ScanningForDevices { get; set; }
        string CommandResponse { get; set; }

        //bool DeterminingSystemInfo { get; set; }

        //bool DeterminingProgramInfo { get; set; }

        //DelegateCommand CheckForUpdatesCommand { get; }
        //DelegateCommand CheckBatteryLevelCommand { get; }

        DelegateCommand ScanForDevicesCommand { get; }
        DelegateCommand ConnectDeviceCommand { get; }
        DelegateCommand DisconnectDeviceCommand { get; }

        DelegateCommand ToggleLedCommand { get; }
        //DelegateCommand RefreshDeviceInfoCommand { get; }

        //string DeviceSerialNumber { get; set; }
        //string LoadCellSerialNumber { get; set; }
        //string GeophoneOneSerialNumber { get; set; }
        //string GeophoneTwoSerialNumber { get; set; }
        //string GeophoneThreeSerialNumber { get; set; }

        //string ProgramDate { get; set; }
        //string Version { get; set; }
        //string DropCount { get; set; }
    }
}
