using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Org.BouncyCastle.Crypto.Digests;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace EasyWeb3
{
    public class Web3Utils
    {
        public static byte[] CalculateKeccakHash(byte[] _value)
        {
            var _digest = new KeccakDigest(256);
            var _output = new byte[_digest.GetDigestSize()];
            _digest.BlockUpdate(_value, 0, _value.Length);
            _digest.DoFinal(_output, 0);
            return _output;
        }
        public static string FunctionHash(string _func)
        {
            byte[] _bytes = Encoding.ASCII.GetBytes(_func);
            _bytes = CalculateKeccakHash(_bytes);
            byte[] _newBytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                _newBytes[i] = _bytes[i];
            }
            return "0x" + HexByteConvertorExtensions.ToHex(_newBytes);
        }
        public static string HexToString(string _hex)
        {
            byte[] _bytes = HexUTF8String.CreateFromHex(_hex).ToHexByteArray();
            List<char> _chars = new List<char>();
            var i = 0;
            foreach (byte _byte in _bytes)
            {
                char _char = (char)_byte;
                int _code = (int)_char;
                if (_code > 31 && _code < 123)
                {
                    _chars.Add(_char);
                    i++;
                }
            }
            string _ret = new string(_chars.ToArray());
            return _ret;
        }
        public static string HexAddressToString(string _addr)
        {
            return "0x" + _addr.Replace("000000000000000000000000", "");
        }
        public static string AddressToHexString(string _addr)
        {
            return "000000000000000000000000" + _addr.Replace("0x", "");
        }
        public static string StringToHexBigInteger(string _intstr)
        {
            BigInteger _int = 0;
            BigInteger.TryParse(_intstr, out _int);
            string _hex = HexBigIntegerConvertorExtensions.ToHex(_int, false, false).Replace("0x", "");
            string _leadingZeroes = "";
            for (var i = _hex.Length; i < 64; i++)
            {
                _leadingZeroes += "0";
            }
            return _leadingZeroes + _hex;
        }
        public static string StringToHexString(string _str)
        {
            string _hex = Encoding.UTF8.GetBytes(_str).ToHex();
            string _trailingZeroes = "";
            int _byteRows = 1 + (_hex.Length - 1) / 64;
            for (var i = _hex.Length; i < 64 * _byteRows; i++)
            {
                _trailingZeroes += "0";
            }
            return _hex + _trailingZeroes;
        }
    }
}