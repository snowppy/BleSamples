using BleSample.Services.DeviceBluetoothLe.Tokens;
using System.Collections.Generic;

namespace BleSample.Services.DeviceBluetoothLe.Commands
{
    public interface ICommand
    {
        List<IToken> Tokens { get; set; }
        byte[] CraftCommandPayload();
        bool IsValidCommand(List<IToken> tokens);


        bool Validate(byte[] response);

        bool ReturnsValue { get; set; }
        object CommandValue(byte[] response);
    }
}
