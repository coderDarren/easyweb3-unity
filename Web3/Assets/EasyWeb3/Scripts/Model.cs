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
using UniRx.Async;
using UnityEngine;

namespace EasyWeb3 {
    public delegate void StandardDelegate();
    public delegate void StringDelegate(string _s);
    public delegate void TransactionScanDelegate(List<Nethereum.RPC.Eth.DTOs.Transaction> _txs);
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
                            int _addedLen = 0;
                            for (int k = 0; k < _strings.Length; k++) {
                                _lines.Add(Web3Utils.StringToHexBigInteger((((_tmpLen + (_addedLen*64) + (k * 128)) / 2).ToString()))); // ptr
                                _addedLen += (int)((_strings[k].Length-1) / 32);
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
                int _byterows = 0;
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
                        _arrlen = GetPointerOrLength(_hex, (int)_ptr);
                        _byterows = 1 + ((int)_arrlen-1)/32;
                        _data = _hex.Substring((int)_ptr+64,64*_byterows);
                        _ret.Add(Web3Utils.HexToString(_data));
                        string _str = Web3Utils.HexToString(_data);
                        AddHistory(ref _history, _ptr, 64 + 64*_byterows);
                        _cursor += 64;
                        break;
                    case "bytes[]":
                    case "string[]":
                        _ptr = GetPointerOrLength(_hex, _cursor) * 2;
                        _arrlen = GetPointerOrLength(_hex, (int)_ptr);
                        int _historylen = 64;
                        string[] _strarr = new string[(int)_arrlen];
                        for (int i = 0; i < _strarr.Length; i++) {
                            var _subptr = GetPointerOrLength(_hex, (int)_ptr + 64*(i+1)) * 2;
                            _arrlen = GetPointerOrLength(_hex, (int)_ptr+(int)_subptr+64);
                            _byterows = 1 + ((int)_arrlen-1)/32;
                            _data = _hex.Substring((int)_ptr+(int)_subptr+128, 64*_byterows);
                            _strarr[i] = Web3Utils.HexToString(_data);
                            _historylen += (64 + _byterows*64);
                        }
                        _ret.Add(_strarr);
                        AddHistory(ref _history, _ptr, _historylen);
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
        private BigInteger m_LastScannedBlock;
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
        public async UniTask<List<object>> CallFunction(string _signature, string[] _outputs, string[] _inputs=null) {
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

        public async UniTask<List<Nethereum.RPC.Eth.DTOs.Transaction>> Scan(TransactionScanDelegate _onScanComplete=null) {
            List<Nethereum.RPC.Eth.DTOs.Transaction> _ret = new List<Nethereum.RPC.Eth.DTOs.Transaction>();
            HexBigInteger _blockNum = await m_Web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            if (_blockNum <= m_LastScannedBlock) {
                if (_onScanComplete != null)
                    _onScanComplete(_ret);
                return _ret;
            }
            m_LastScannedBlock = _blockNum;
            Log("[Scan] Scanning Block "+_blockNum);
            var _result = await m_Web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync((HexBigInteger)_blockNum);
            foreach(Nethereum.RPC.Eth.DTOs.Transaction _tx in _result.Transactions) {
                if (_tx.To.ToLower() == m_Contract.ToLower()) {
                    UnityEngine.Debug.Log("Tx found: "+_tx.TransactionHash);
                    _ret.Add(_tx);
                }
            }
            if (_onScanComplete != null)
                _onScanComplete(_ret);
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
            List<char> _chars = new List<char>();
            var i = 0;
            foreach (byte _byte in _bytes)
            {
                char _char = (char)_byte;
                int _code = (int)_char;
                if (_code > 31 && _code < 123) {
                    _chars.Add(_char);
                    i++;
                }
            }
            string _ret = new string(_chars.ToArray());
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
            int _byteRows = 1 + (_hex.Length-1)/64;
            for (var i = _hex.Length; i < 64*_byteRows; i++) {
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
0000000000000000000000000000000000000000000000000000000000000020
0000000000000000000000000000000000000000000000000000000000000001
0000000000000000000000000000000000000000000000000000000000000020
0000000000000000000000000000000000000000000000000000000000000032
6162637165737472776572776572776572776572776572776566646676646676
6664676766686173646661736466686766680000000000000000000000000000

0000000000000000000000000000000000000000000000000000000000000020
0000000000000000000000000000000000000000000000000000000000000001
0000000000000000000000000000000000000000000000000000000000000020
0000000000000000000000000000000000000000000000000000000000000035
6162637165737472776572776572776572776572776572776566646676646676
6664676767676866686173646661736466686766680000000000000000000000

0000000000000000000000000000000000000000000000000000000000000020
0000000000000000000000000000000000000000000000000000000000000002
0000000000000000000000000000000000000000000000000000000000000040
0000000000000000000000000000000000000000000000000000000000000080
0000000000000000000000000000000000000000000000000000000000000021
6173667364667364666773666467736466676473626361736673646673646667
6100000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000000000000000000000003
3132330000000000000000000000000000000000000000000000000000000000

*/