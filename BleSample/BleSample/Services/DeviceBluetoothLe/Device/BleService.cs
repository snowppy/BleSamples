using BleSample.Services.DeviceBluetoothLe.Commands;
using Plugin.BluetoothLE;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BleSample.Services.DeviceBluetoothLe.Device
{
    public class BleService : IBleService
    {
        public const string ServiceUuid = "49535343-fe7d-4ae5-8fa9-9fafd205e455";
        public const string CharacteristictReadUuid = "49535343-1e4d-4bd9-ba61-23c647249616";
        public const string CharacteristicWriteUuid = "49535343-8841-43f4-a8d4-ecbe34729bb3";
        public const byte EndPayloadValue = 0x0A;
        private readonly List<(int level, double percentage)> baterryLevels = new List<(int level, double percentage)> { (4100, 1.0), (4000, 0.96), (3900, 0.93), (3800, 0.87), (3700, 0.82), (3600, 0.75), (3500, 0.70), (3400, 0.62), (3300, 0.52), (3200, 0.43), (3100, 0.37), (3000, 0.29), (2900, 0.21), (2800, 0.12), (2700, 0.03) };
        private readonly IDictionary<string, IDevice> locatedDevices;

        private CancellationTokenSource processingQueToken;
        private CancellationTokenSource recieveQueueToken;


        public BleService()
        {
            locatedDevices = new ConcurrentDictionary<string, IDevice>();
            RxResponses = new BlockingCollection<byte[]>();
            TxCommandQueue = new BlockingCollection<(ICommand command, CancellationToken token, int retry, Action<object> successAction, Action failureAction)>();
            RxQueue = new BlockingCollection<byte>();
            processingQueToken = new CancellationTokenSource();
            recieveQueueToken = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
            {
                while (!processingQueToken.IsCancellationRequested)
                {
                    if (SelectedDevice != null && SelectedDevice.ConnectState == DeviceState.Connected)
                    {
                        var command = TxCommandQueue.Take();
                        if (command.command != null)
                        {
                            var payload = command.command.CraftCommandPayload();

                            var tries = 0;
                            bool state = false;
                            byte[] commandResponse = null;
                            while (tries <= command.retry && !state)
                            {

                                var writeCase = SelectedDevice.Write(payload);
                                if (writeCase)
                                {
                                    commandResponse = RxResponses.Take(command.token);
                                    state = command.command.Validate(commandResponse);
                                }
                                tries++;
                            }

                            if (state)
                            {
                                //if (command.command is CtrlTrigCommand)
                                //{
                                //    commandResponse = RxResponses.Take(command.token);
                                //}
                                object commandValue = null;
                                if (command.command.ReturnsValue) commandValue = command.command.CommandValue(commandResponse);
                                command.successAction(commandValue);
                            }
                            else
                            {
                                command.failureAction();
                            }


                        }

                    }


                }

            });

            Task.Factory.StartNew(() =>
            {
                while (!recieveQueueToken.IsCancellationRequested)
                {
                    if (SelectedDevice != null && SelectedDevice.ConnectState == DeviceState.Connected)
                    {
                        var response = new List<byte>();
                        var lastVal = RxQueue.Take();
                        if (lastVal != 1)
                        {
                            while (lastVal != EndPayloadValue)
                            {
                                response.Add(lastVal);
                                lastVal = RxQueue.Take();
                            }
                        }
                        else
                        {
                            response.Add(lastVal); // Version
                            var storageType = RxQueue.Take();
                            response.Add(storageType);

                            var payloadSizeMsb2 = RxQueue.Take();
                            var payloadSizeMsb = RxQueue.Take();
                            var payloadSizeLsb = RxQueue.Take();

                            response.Add(payloadSizeMsb2);
                            response.Add(payloadSizeMsb);
                            response.Add(payloadSizeLsb);

                            var commandMsb2 = RxQueue.Take();
                            var commandMsb = RxQueue.Take();
                            var commandLsb = RxQueue.Take();

                            response.Add(commandMsb2);
                            response.Add(commandMsb);
                            response.Add(commandLsb);

                            var payloadSize = BitConverter.ToUInt16(new[] { payloadSizeLsb, payloadSizeMsb }, 0);

                            var payload = new List<byte>();
                            while (payload.Count < payloadSize)
                            {
                                var payloadValue = RxQueue.Take();
                                payload.Add(payloadValue);
                            }

                            response.AddRange(payload);

                            // Droppp triggered
                            lastVal = RxQueue.Take();
                            response.Add(lastVal);
                            while (lastVal != EndPayloadValue)
                            {
                                response.Add(lastVal);
                                lastVal = RxQueue.Take();
                            }

                        }
                        RxResponses.Add(response.ToArray());
                    }
                }
            });
        }
        public BlockingCollection<byte> RxQueue { get; set; }
        public BlockingCollection<byte[]> RxResponses { get; set; }
        public BlockingCollection<(ICommand command, CancellationToken token, int retry, Action<object> successAction, Action failureAction)> TxCommandQueue { get; set; }
        public (ICommand command, int retry, Action<object> successAction, Action failureAction, Type cancelingCommandType) ProcessingCommand { get; set; }
        public void SendCommand(ICommand command, CancellationToken token, int retryCount = 3, Action<object> successAction = null, Action failureAction = null)
        {
            TxCommandQueue.Add((command, token, retryCount, successAction, failureAction));
        }

        public IDevice SelectedDevice { get; private set; }

        public void SetSelectedDevice(IDevice device)
        {
            SelectedDevice = device;
        }

        private void Read(byte[] data)
        {
            foreach (var dataValue in data)
            {
                RxQueue.Add(dataValue);
            }
        }
        public void Connect()
        {

            SelectedDevice.Read += Read;
            try
            {
                SelectedDevice?.Connect();
            }
            catch (Exception e)
            {
                //  Crashes.TrackError(e);
            }

        }

        public void Disconnect()
        {
            // ReSharper disable once DelegateSubtraction
            if (SelectedDevice != null) SelectedDevice.Read -= Read;
            SelectedDevice?.Disconnect();

            SelectedDevice = null;
        }

        public void ScanForDevicesCommand(TimeSpan scanIntercal, Action<IDevice> scanResult)
        {
            locatedDevices.Clear();

            var devs = CrossBleAdapter.Current.GetPairedDevices();
            foreach (var dev in devs)
            {
                if (!locatedDevices.ContainsKey(dev.Uuid.ToString()))
                {
                    var device = new Device(dev);
                    if (device.Name != null)
                    {
                        locatedDevices.Add(dev.Uuid.ToString(), device);
                        scanResult?.Invoke(device);
                    }

                }
            }
            //if (!CrossBleAdapter.Current.IsScanning)
            //{
            //    CrossBleAdapter.Current.Scan(new ScanConfig { ServiceUuids = new List<Guid> { new Guid(ServiceUuid) } }).Subscribe(sr =>
            //         {
            //             if (!locatedDevices.ContainsKey(sr.Device.Uuid.ToString()))
            //             {
            //                 var device = new Device(sr.Device);
            //                 if (device.Name != null)
            //                 {
            //                     locatedDevices.Add(sr.Device.Uuid.ToString(), device);
            //                     scanResult?.Invoke(device);
            //                 }

            //             }
            //         });
            //}

        }


        public double ResolveBatteryLevel(int batteryMilivolts)
        {
            var level = baterryLevels.Last();
            for (var i = baterryLevels.Count - 1; i >= 0; i--)
            {
                if (batteryMilivolts > baterryLevels[i].level)
                {
                    level = baterryLevels[i];
                }
            }

            return level.percentage;
        }

        public void CtrlLed(bool @on, int onMilisec, int offMilisec, CancellationToken token, Action<object> doneAction, Action failedAction)
        {
            var command = new CtrlLedCommand(on, onMilisec, offMilisec);
            SendCommand(command, token, 3, doneAction, failedAction);
        }
    }
}
