using System;

namespace BleSample.Services.DeviceBluetoothLe.Device
{
    public interface IBleService: ICommandTransmitter, IHardwareApiService
    {
        IDevice SelectedDevice { get; }

        void ScanForDevicesCommand(TimeSpan scanIntercal, Action<IDevice> scanResult);

        double ResolveBatteryLevel(int batteryMilivolts);
        void SetSelectedDevice(IDevice device);
        void Connect();
        void Disconnect();
    }
}
