using Plugin.BluetoothLE;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;

namespace BleSample.Services.DeviceBluetoothLe.Device
{
    public class Device : BindableBase, IDevice
    {

        private readonly Plugin.BluetoothLE.IDevice device;
        private IGattCharacteristic readCharacteristic;
        private IGattCharacteristic writeCharacteristic;

        private IDisposable chararacteristicListener;
        private IDisposable stateListener;
        private IDisposable readCharacteristicObserver;


        public Device(Plugin.BluetoothLE.IDevice dev)
        {
            device = dev;
            Name = dev.Name;

            chararacteristicListener = device.WhenAnyCharacteristicDiscovered().Subscribe(characteristic =>
                {
                    if (characteristic.Uuid.ToString() == BleService.CharacteristictReadUuid)
                    {
                        readCharacteristic = characteristic;
                        OnStateChanged(DeviceState.Connected);
                        RaisePropertyChanged(nameof(ConnectState));
                        if (readCharacteristic.CanNotify())
                        {

                            readCharacteristicObserver = characteristic.RegisterAndNotify(true).Subscribe(r =>
                            {
                                Read?.Invoke(r.Data);
                            },
                            exception =>
                            {

                                //   Crashes.TrackError(exception);
                            });

                        }
                    }

                    if (characteristic.Uuid.ToString() == BleService.CharacteristicWriteUuid)
                    {
                        if (characteristic.CanWrite())
                        {
                            writeCharacteristic = characteristic;
                            OnStateChanged(DeviceState.Connected);
                            RaisePropertyChanged(nameof(ConnectState));
                        }
                    }
                }, exception =>
                {
                  //  Crashes.TrackError(exception);
                });
            stateListener = device.WhenStatusChanged().Subscribe(f =>
            {
                OnStateChanged(ConnectState);

            });
        }

        public string Name { get; set; }
        public DeviceState ConnectState
        {
            get
            {
                var state = ((DeviceState)((int)device.Status));
                if (state == DeviceState.Connected)
                {
                    return !Configured ? DeviceState.Connecting : ((DeviceState)((int)device.Status));
                }
                else
                {
                    return ((DeviceState)((int)device.Status));
                }
            }
        }
        public void Connect()
        {
            if (ConnectState != DeviceState.Connected || ConnectState != DeviceState.Connecting)
            {

                try
                {
                    device.Connect(new ConnectionConfig { AndroidConnectionPriority = ConnectionPriority.High, AutoConnect = false });

                    OnStateChanged(DeviceState.Connecting);
                }
                catch (Exception e)
                {
                    //  Crashes.TrackError(e);
                }
            }
        }

        public void Disconnect()
        {
            if (ConnectState != DeviceState.Disconnecting || ConnectState != DeviceState.Disconnected)
            {
                try
                {
                    readCharacteristicObserver?.Dispose();
                }
                catch (Exception e)
                {
                    // Crashes.TrackError(e);
                }
                try
                {
                    chararacteristicListener?.Dispose();
                }
                catch (Exception e)
                {
                    //  Crashes.TrackError(e);
                }
                try
                {
                    stateListener?.Dispose();
                }
                catch (Exception e)
                {
                    //   Crashes.TrackError(e);
                }

                try
                {
                    device.CancelConnection();
                }
                catch (Exception e)
                {
                    //  Crashes.TrackError(e);
                }


                readCharacteristic = null;
                writeCharacteristic = null;
            }
        }

        public bool Configured
        {
            get { return readCharacteristic != null && writeCharacteristic != null; }
        }

        public event EventHandler<DeviceState> StateChanged;


        public bool Write(byte[] bytes)
        {
            if (ConnectState == DeviceState.Connected)
            {
                try
                {
                    if (writeCharacteristic != null)
                    {
                        var pload = new byte[bytes.Length + 1];
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            pload[i] = bytes[i];
                        }

                        pload[bytes.Length] = 0x0a;
                        var writeResult = true;
                        var recievedResult = false;
                        //writeCharacteristic.Write().Subscribe()
                        //var obs=  writeCharacteristic.Write(pload).Subscribe(result =>
                        //      {
                        //          writeResult = true;
                        //         // recievedResult = true;
                        //          // you don't really need to do anything with the result
                        //      },
                        //      exception =>
                        //      {
                        //          Crashes.TrackError(exception);
                        //          writeResult = false;
                        //         // recievedResult = true;
                        //          // writes can error!
                        //      });

                        writeCharacteristic.Write(pload).Subscribe(result =>
                            {
                                writeResult = true;
                                recievedResult = true;
                                // you don't really need to do anything with the result
                            },
                            exception =>
                            {
                                //Crashes.TrackError(exception);
                                writeResult = false;
                                recievedResult = true;
                                // writes can error!
                            });

                        while (!recievedResult)
                        {
                            Task.Delay(TimeSpan.FromMilliseconds(200)).Wait();
                        }
                        //obs.Dispose();
                        return writeResult;
                    }
                }
                catch (Exception e)
                {
                  //  Crashes.TrackError(e);
                }
            }

            return false;
        }

        public HandleReadDelegate Read { get; set; }

        protected virtual void OnStateChanged(DeviceState e)
        {
            if (e == DeviceState.Connected)
            {
                StateChanged?.Invoke(this, !Configured ? DeviceState.Connecting : e);

            }
            else
            {
                StateChanged?.Invoke(this, e);
            }
            RaisePropertyChanged(nameof(ConnectState));
        }
    }
}