using BleSample.Services.DeviceBluetoothLe.Tokens;
using System.Collections.Generic;
using System.Text;

namespace BleSample.Services.DeviceBluetoothLe.Commands
{
     public class CtrlLedCommand: BaseCommand
    {
        public CtrlLedCommand(bool onState, int onTime , int offTime)
        {
            Tokens = new List<IToken>
            {
                new BaseToken
                {
                    Type = TokenType.Keyword,
                    Value = "CTRL",
                    Position = 0
                },
                new BaseToken
                {
                    Type = TokenType.Keyword,
                    Value = "LED",
                    Position = 1
                },
                new BaseToken
                {
                    Type = TokenType.Value,
                    Value = onState ? "1" : "0",
                    Position = 2
                },
                new BaseToken
                {
                    Type = TokenType.Value,
                    Value = $"{onTime}",
                    Position = 3
                },
                new BaseToken
                {
                    Type = TokenType.Value,
                    Value = $"{offTime}",
                    Position = 4
                }
            };
        }
        
        public override bool IsValidCommand(List<IToken> tokens)
        {
            if (tokens.Count == 5)
            {
                if (tokens[0].Value == "CTRL")
                {
                    if (tokens[1].Value == "LED")
                    {
                        if (int.TryParse(tokens[2].Value, out _))
                        {
                            if (int.TryParse(tokens[3].Value, out _))
                            {
                                if (int.TryParse(tokens[4].Value, out _))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public override bool Validate(byte[] response)
        {
            var stringResponse = Encoding.ASCII.GetString(response);
            if (stringResponse.Contains("ACK CTRL LED")) return true;
            return false;
        }

        public override object CommandValue(byte[] response)
        {
            return null;
        }

        public override string ToString()
        {
            return string.Join(" ", Tokens);
        }
    }
}