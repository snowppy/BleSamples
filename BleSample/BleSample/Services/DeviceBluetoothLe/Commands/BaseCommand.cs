using BleSample.Services.DeviceBluetoothLe.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BleSample.Services.DeviceBluetoothLe.Commands
{
    public abstract class BaseCommand : ICommand
    {

        public List<IToken> Tokens { get; set; }

        public byte[] CraftCommandPayload()
        {
            var paylaod = new byte[Tokens.Sum(t=>t.CraftToken().Length) + Tokens.Count - 1];
            var offset = 0;
            foreach (var token in Tokens)
            {
                var tokenVal = token.CraftToken();
                Buffer.BlockCopy(tokenVal, 0, paylaod, offset, tokenVal.Length);
                offset += tokenVal.Length;
                if (offset < paylaod.Length - 1)
                {
                    paylaod[offset] = Convert.ToByte(' ');
                    offset++;
                }

            }
            return paylaod;
        }

        public abstract bool IsValidCommand(List<IToken> tokens);
        public abstract bool Validate(byte[] response);

        public bool ReturnsValue { get; set; }

        public abstract object CommandValue(byte[] response);

    }
}
