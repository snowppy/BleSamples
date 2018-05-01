using BleSample.Services.DeviceBluetoothLe.Commands;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace BleSample.Services.DeviceBluetoothLe.Device
{
    public interface ICommandTransmitter
    {
        BlockingCollection<byte> RxQueue { get; set; }
        BlockingCollection<byte[]> RxResponses { get; set; }
        BlockingCollection<(ICommand command, CancellationToken token, int retry, Action<object> successAction, Action failureAction) > TxCommandQueue { get; set; }

        (ICommand command, int retry, Action<object> successAction, Action failureAction, Type cancelingCommandType) ProcessingCommand { get; set; }

        void SendCommand(ICommand command, CancellationToken token, int retryCount = 3, Action<object> successAction = null, Action failureAction = null);
    }
}
