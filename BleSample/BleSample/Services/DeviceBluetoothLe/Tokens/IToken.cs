using System;
using System.Collections.Generic;
using System.Text;

namespace BleSample.Services.DeviceBluetoothLe.Tokens
{
    public enum TokenType { Keyword, Identifier, Position, Value}
    public interface IToken
    {
        string Value { get; set; }
        byte[] RawValue { get; set; }
        byte[] CraftToken();
        TokenType Type { get; set; }
        int Position { get; set; }
    }
}
