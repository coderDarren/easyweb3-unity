/*
 Looking around? Ask for help. We will respond. 
 
 Email: realbrogames@gmail.com

 Consider donating to help keep our nodes running for game devs. 0x1011f61Df0E2Ad67e269f4108098c79e71868E00
*/
using System;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Nethereum.Web3;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors;
using Nethereum.Hex.HexConvertors.Extensions;
using Org.BouncyCastle.Crypto.Digests;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Util;
using Nethereum.ABI;
using Nethereum.Contracts;
using Nethereum.Contracts.Extensions;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using UnityEngine;

namespace EasyWeb3 {
    public enum ChainId {
        ETH_MAINNET,
        ETH_ROPSTEN
    }
    public class Web3ify {
        protected Web3 m_Web3;
        protected ChainId m_ChainId;
        public ChainId chainId {
            get { return m_ChainId; }
            set {
                m_ChainId = value;
                m_Web3 = new Web3(Web3Utils.GetNodeURL(m_ChainId));
            }
        }
        public Web3ify(ChainId _i) {
            m_ChainId = _i;
            m_Web3 = new Web3(Web3Utils.GetNodeURL(m_ChainId));
        }
        public async Task<BigInteger> GetChainId() {
            HexBigInteger _hex = await m_Web3.Eth.ChainId.SendRequestAsync();
            return _hex.Value;
        }
        protected void Log(string _msg) {
            if (!Constants.debug) return;
            Debug.Log(_msg);
        }
        protected void LogWarning(string _msg) {
            if (!Constants.debug) return;
            Debug.LogWarning(_msg);
        }
        protected void LogError(string _msg) {
            if (!Constants.debug) return;
            Debug.LogError(_msg);
        }
    }
    public class Encoder {
        public string Encode(string _signature, string[] _types, string[] _values) {
            string _ret = Web3Utils.FunctionHash(_signature);
            int i = 0;
            List<string> _lines = new List<string>();

            //write head
            foreach(string _in in _types) {
                switch (_in) {
                    case "": break;
                    case "address":
                        _lines.Add(Web3Utils.AddressToHexString(_values[i]));
                        break;
                    case "uint": case "uint256": case "uint128": case "uint64": case "uint32": case "uint16": case "uint8": case "bool":
                        _lines.Add(Web3Utils.StringToHexBigInteger(_values[i].ToString()));
                        break;
                    // save dynamic pointer indices
                    case "uint256[]": case "uint128[]": case "uint64[]": case "uint32[]": case "uint16[]": case "uint8[]": case "uint[]": case "bool[]":
                    case "string":
                    case "bytes":
                    case "bytes[]":
                    case "string[]":
                    case "address[]":
                        _lines.Add(i.ToString());
                        break;
                    default:
                        Debug.LogWarning("Could not encode function ["+_signature+"]. Unsupported input type found in signature ("+_in+").");
                        return _ret;
                }
                i++;
            }
            
            //write dynamics
            i = 0;
            int _head = 0;
            foreach(string _in in _types) {
                switch (_in) {
                    case "": case "address": case "uint": case "uint256": case "uint128": case "uint64": case "uint32": case "uint16": case "uint8": case "bool": break;
                    case "string":
                    case "bytes":
                        if (int.TryParse(_lines[i], out _head)) {
                            _lines[_head] = Web3Utils.StringToHexBigInteger((32*_lines.Count).ToString()); // ptr
                            _lines.Add(Web3Utils.StringToHexBigInteger(_values[i].Length.ToString()));
                            _lines.Add(Web3Utils.StringToHexString(_values[i]));
                        }
                        break;
                    case "uint256[]": case "uint128[]": case "uint64[]": case "uint32[]": case "uint16[]": case "uint8[]": case "uint[]": case "bool[]":
                        if (int.TryParse(_lines[i], out _head)) {
                            string[] _ints = GetInputParams(_values[i]);
                            _lines[_head] = Web3Utils.StringToHexBigInteger((32*_lines.Count).ToString()); // ptr
                            _lines.Add(Web3Utils.StringToHexBigInteger(_ints.Length.ToString())); // len
                            foreach (string _intstr in _ints) {
                                _lines.Add(Web3Utils.StringToHexBigInteger(_intstr));
                            }
                        }
                        break;
                    case "bytes[]":
                    case "string[]":
                        if (int.TryParse(_lines[i], out _head)) {
                            string[] _strings = GetInputParams(_values[i]);
                            _lines[_head] = Web3Utils.StringToHexBigInteger((32*_lines.Count).ToString()); // ptr
                            _lines.Add(Web3Utils.StringToHexBigInteger(_strings.Length.ToString())); // len
                            int _tmpLen = _strings.Length*64;
                            for (int k = 0; k < _strings.Length; k++) {
                                _lines.Add(Web3Utils.StringToHexBigInteger((((_tmpLen + (k * 128)) / 2).ToString()))); // ptr
                            }
                            foreach (string _s in _strings) {
                                _lines.Add(Web3Utils.StringToHexBigInteger(_s.Length.ToString()));
                                _lines.Add(Web3Utils.StringToHexString(_s));
                            }
                        }
                        break;
                    case "address[]":
                        if (int.TryParse(_lines[i], out _head)) {
                            string[] _addrs = GetInputParams(_values[i]);
                            _lines[_head] = Web3Utils.StringToHexBigInteger((32*_lines.Count).ToString()); // ptr
                            _lines.Add(Web3Utils.StringToHexBigInteger(_addrs.Length.ToString())); // len
                            foreach (string _s in _addrs) {
                                _lines.Add(Web3Utils.AddressToHexString(_s));
                            }
                        }
                        break;
                    default:
                        Debug.LogWarning("Could not encode function ["+_signature+"]. Unsupported input type found in signature ("+_in+").");
                        return _ret;
                }
                i++;
            }

            foreach(string _s in _lines) {
                _ret += _s;
            }

            return _ret;
        }
        private string[] GetInputParams(string _values) {
            int _arrstart = _values.IndexOf("(");
            if (_arrstart == -1) {
                Debug.LogWarning("Could not call function: Illegal input ["+_values+"] struct format. Expected '('.");
                throw new Exception();
            }
            int _arrend = _values.IndexOf(")");
            if (_arrend == -1) {
                Debug.LogWarning("Could not call function: Illegal input ["+_values+"] struct format. Expected ')'.");
                throw new Exception();
            }
            return _values.Substring(_arrstart+1, _arrend-_arrstart-1).Split(',');;
        }
    }
    public class Decoder {
        public List<object> Decode(string _hex, string[] _outputs, ref int _len) {
            // Debug.Log("decode: "+_hex);
            List<object> _ret = new List<object>();
            BigInteger _ptr = 0;
            int _cursor = 0;
            List<int[]> _history = new List<int[]>();
            for (int k = 0; k < _outputs.Length; k++) {
                string _out = _outputs[k];
                // Debug.Log("Checking output: "+_out+" where cursor = "+_cursor);
                if (DidAnalyze(_history, ref _cursor)) {
                    // Debug.Log("Already analyzed here. Moving cursor to "+_cursor);
                    k--;
                    continue;
                }

                string _data = "";
                BigInteger _arrlen = 0;
                switch (_out) {
                    case "bool":
                        _data = _hex.Substring(_cursor,64);
                        _ret.Add((new HexBigInteger(_data)).Value == 1 ? true : false);
                        _cursor += 64;
                        break;
                    case "bool[]":
                        _ptr = GetPointerOrLength(_hex, _cursor) * 2;
                        _arrlen = GetPointerOrLength(_hex, (int)_ptr);
                        bool[] _boolarr = new bool[(int)_arrlen];
                        for (int i = 0; i < _boolarr.Length; i++) {
                            _data = _hex.Substring((int)_ptr + 64*(i+1), 64);
                            _boolarr[i] = (new HexBigInteger(_data)).Value == 1 ? true : false;
                        }
                        _ret.Add(_boolarr);
                        AddHistory(ref _history, _ptr, 64*_boolarr.Length+64);
                        _cursor += 64;
                        break;
                    case "bytes": // dynamic
                    case "string": // dynamic
                        _ptr = GetPointerOrLength(_hex, _cursor) * 2;
                        _data = _hex.Substring((int)_ptr,128);
                        _ret.Add(Web3Utils.HexToString(_data));
                        AddHistory(ref _history, _ptr, 128);
                        _cursor += 64;
                        break;
                    case "bytes[]":
                    case "string[]":
                        _ptr = GetPointerOrLength(_hex, _cursor) * 2;
                        _arrlen = GetPointerOrLength(_hex, (int)_ptr);
                        string[] _strarr = new string[(int)_arrlen];
                        for (int i = 0; i < _strarr.Length; i++) {
                            var _subptr = GetPointerOrLength(_hex, (int)_ptr + 64*(i+1)) * 2;
                            _data = _hex.Substring((int)_ptr+(int)_subptr+128, 64);
                            _strarr[i] = Web3Utils.HexToString(_data);
                        }
                        _ret.Add(_strarr);
                        AddHistory(ref _history, _ptr, 128*_strarr.Length+64);
                        _cursor += 64;
                        break;
                    case "address":
                        _data = _hex.Substring(_cursor,64);
                        _ret.Add(Web3Utils.HexAddressToString(_data));
                        _cursor += 64;
                        break;
                    case "address[]":
                        _ptr = GetPointerOrLength(_hex, _cursor) * 2;
                        _arrlen = GetPointerOrLength(_hex, (int)_ptr);
                        string[] _addrarr = new string[(int)_arrlen];
                        for (int i = 0; i < _addrarr.Length; i++) {
                            _data = _hex.Substring((int)_ptr + 64*(i+1), 64);
                            _addrarr[i] = Web3Utils.HexAddressToString(_data);
                        }
                        _ret.Add(_addrarr);
                        AddHistory(ref _history, _ptr, 64*_addrarr.Length+64);
                        _cursor += 64;
                        break;
                    case "uint[]":
                        _ptr = GetPointerOrLength(_hex, _cursor) * 2;
                        _arrlen = GetPointerOrLength(_hex, (int)_ptr);
                        BigInteger[] _intarr = new BigInteger[(int)_arrlen];
                        for (int i = 0; i < _intarr.Length; i++) {
                            _data = _hex.Substring((int)_ptr + 64*(i+1), 64);
                            _intarr[i] = (new HexBigInteger(_data)).Value;
                        }
                        _ret.Add(_intarr);
                        AddHistory(ref _history, _ptr, 64*_intarr.Length);
                        _cursor += 64;
                        break;
                    case "uint":
                        _data = _hex.Substring(_cursor,64);
                        _ret.Add((new HexBigInteger(_data)).Value);
                        _cursor += 64;
                        break;
                    default:
                        if (_out.Contains("struct")) {
                            int _structStart = _out.IndexOf("(");
                            if (_structStart == -1) {
                                Debug.LogWarning("Could not call function: Illegal output ["+_out+"] struct format. Expected '('.");
                                return _ret;
                            }
                            int _structEnd = _out.IndexOf(")");
                            if (_structEnd == -1) {
                                Debug.LogWarning("Could not call function: Illegal output ["+_out+"] struct format. Expected ')'.");
                                return _ret;
                            }

                            string[] _structTypes = _out.Substring(_structStart+1, _structEnd-_structStart-1).Split(',');
                            
                            _ptr = GetPointerOrLength(_hex, _cursor) * 2;
                            int _l = 0;
                            List<object> _struct = new Decoder().Decode(_hex.Substring((int)_ptr), _structTypes, ref _l);
                            foreach (object _o in _struct) {
                                _ret.Add(_o);
                            }
                            AddHistory(ref _history, _ptr, _l);
                            _cursor += 64;
                        } else {
                            Debug.LogWarning("Could not decode. Unsupported output found in signature ("+_out+").");
                            return _ret;
                        }
                        break;
                }
            }
            _len = _cursor;
            return _ret;
        }
        private BigInteger GetPointerOrLength(string _data, int _cursor) {
            string _datapoint = _data.Substring(_cursor, 64);
            return (new HexBigInteger(_datapoint)).Value;
        }
        private void AddHistory(ref List<int[]> _history, BigInteger _ptr, int _length) {
            int[] _tuple = new int[2]{(int)_ptr,(int)_ptr+_length};
            _history.Add(_tuple);
        }
        private bool DidAnalyze(List<int[]> _history, ref int _cursor) {
            foreach(int[] _tuple in _history) {
                if (_cursor >= _tuple[0] && _cursor < _tuple[1]) {
                    _cursor = _tuple[1];
                    return true;
                }
            }
            return false;
        }
    }
    public class Contract : Web3ify {
        protected string m_Contract;
        public BigInteger TotalSupply {get;protected set;}
        public BigInteger Decimals {get;protected set;}
        public string Name {get;protected set;}
        public string Symbol {get;protected set;}
        public string Owner {get;protected set;}
        public Contract(string _contract, ChainId _i) : base(_i) {
            m_Contract = _contract;
        }
        // slight loss of precision
        public double ValueFromDecimals(BigInteger _value) {
            double _denom = (double)Mathf.Pow(10,(float)Decimals);
            double _val = (double)_value/_denom;
            return _val;
        }
        /*
            Inputs:
                _signature: "balanceOf(address)"
                _outputs: ["int"]
                _inputs: ["0xbd0dbb9fddc73b6ebffc7c09cfae1b19d6dece40"]
            Outputs:
                ["0"]
            All Examples at /!!TODO
         */
        public async Task<List<object>> CallFunction(string _signature, string[] _outputs, string[] _inputs=null) {
            List<object> _ret = new List<object>();
            try {
                if (_outputs.Length == 0) {
                    LogWarning("Could not call function ["+_signature+"]: No outputs specified.");
                    return _ret;
                }

                int _inputStart = _signature.IndexOf("(");
                if (_inputStart == -1) {
                    LogWarning("Could not call function ["+_signature+"]: Illegal format. Expected '('.");
                    return _ret;
                }
                int _inputEnd = _signature.IndexOf(")");
                if (_inputEnd == -1) {
                    LogWarning("Could not call function ["+_signature+"]: Illegal format. Expected ')'.");
                    return _ret;
                }

                string[] _inputTypes = _signature.Substring(_inputStart+1, _inputEnd-_inputStart-1).Split(',');
                string _encodedInput = new Encoder().Encode(_signature, _inputTypes, _inputs);
                // Debug.Log("Encoded input: "+_encodedInput);
                
                CallInput _input = new CallInput(_encodedInput, m_Contract);
                string _result = await m_Web3.Eth.Transactions.Call.SendRequestAsync(_input);
                _result = _result.Substring(2);

                int _l = 0; _ret = new Decoder().Decode(_result, _outputs, ref _l);
            } catch (System.Exception _e) {
                LogWarning("Something went wrong calling function ["+_signature+"]: "+_e);
            }

            return _ret;
        }
    }
    public class Web3Utils {
        public static byte[] CalculateKeccakHash(byte[] _value)
        {
            var _digest = new KeccakDigest(256);
            var _output = new byte[_digest.GetDigestSize()];
            _digest.BlockUpdate(_value, 0, _value.Length);
            _digest.DoFinal(_output, 0);
            return _output;
        }
        public static string FunctionHash(string _func) {
            byte[] _bytes = Encoding.ASCII.GetBytes(_func);
            _bytes = Web3Utils.CalculateKeccakHash(_bytes);
            byte[] _newBytes = new byte[4];
            for (int i = 0; i < 4; i++) {
                _newBytes[i] = _bytes[i];
            }
            return "0x"+HexByteConvertorExtensions.ToHex(_newBytes);
        }
        public static string Hash(string _str) {
            byte[] _bytes = Encoding.ASCII.GetBytes(_str);
            _bytes = Web3Utils.CalculateKeccakHash(_bytes);
            return "0x"+HexByteConvertorExtensions.ToHex(_bytes);
        }
        public static string HexToString(string _hex) {
            byte[] _bytes = HexUTF8String.CreateFromHex(_hex).ToHexByteArray();
            char[] _chars = new char[_bytes.Length];
            var i = 0;
            foreach (byte _byte in _bytes)
            {
                char _char = (char)_byte;
                int _code = (int)_char;
                if (_code > 31 && _code < 123) {
                    _chars[i] = _char;
                    i++;
                }
            }
            string _ret = new string(_chars);
            _ret = _ret.Trim();
            return _ret;
        }
        public static string HexAddressToString(string _addr) {
            return "0x"+_addr.Replace("000000000000000000000000", "");
        }
        public static string AddressToHexString(string _addr) {
            return "000000000000000000000000"+_addr.Replace("0x","");
        }
        public static string StringToHexBigInteger(string _intstr) {
            BigInteger _int = 0;
            BigInteger.TryParse(_intstr, out _int);
            string _hex = HexBigIntegerConvertorExtensions.ToHex(_int, false, false).Replace("0x","");
            string _leadingZeroes = "";
            for (var i = _hex.Length; i < 64; i++) {
                _leadingZeroes += "0";
            }
            return _leadingZeroes+_hex;
        }
        public static string StringToHexString(string _str) {
            string _hex = Encoding.UTF8.GetBytes(_str).ToHex();
            string _trailingZeroes = "";
            for (var i = _hex.Length; i < 64; i++) {
                _trailingZeroes += "0";
            }
            return _hex+_trailingZeroes;
        }
        public static string GetNodeURL(ChainId _chainId) {
            switch (_chainId) {
                case ChainId.ETH_ROPSTEN: // ropsten
                    return Constants.NODEDEFAULT_ETH_ROPSTEN;
                case ChainId.ETH_MAINNET: // mainnet
                    return Constants.NODEDEFAULT_ETH_MAINNET;
            }
            return Constants.NODEDEFAULT_ETH_ROPSTEN;
        }
    }
}

/*
*/