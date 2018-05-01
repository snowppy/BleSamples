using System;
namespace BleSample.Services.DeviceBluetoothLe.Device
{
    public enum DeviceState {Disconnected, Disconnecting, Connected, Connecting}
    public delegate void HandleReadDelegate(byte[] data);
    public interface IDevice
    {
        string Name { get; set; }
        DeviceState ConnectState { get; }
        void Connect();
        void Disconnect();
        bool Configured { get; }
        event EventHandler<DeviceState> StateChanged;
        bool Write(byte[] bytes);
        HandleReadDelegate Read { get; set; }
    }
}
