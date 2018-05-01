using System;
using System.Threading;

namespace BleSample.Services.DeviceBluetoothLe.Device
{
    public interface IHardwareApiService
    {
        void CtrlLed(bool on, int onMilisec, int offMilisec, CancellationToken token, Action<object> doneAction, Action failedAction);

    }
}
