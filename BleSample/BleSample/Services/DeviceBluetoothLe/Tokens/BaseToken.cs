using System.Text;

namespace BleSample.Services.DeviceBluetoothLe.Tokens
{
    public class BaseToken : IToken
    {
        public string Value { get; set; }
        public byte[] RawValue { get; set; }
        public byte[] CraftToken()
        {

            var value = Encoding.UTF8.GetBytes(Value);
            return value;
        }

        public TokenType Type { get; set; }
        public int Position { get; set; }
    }
}
