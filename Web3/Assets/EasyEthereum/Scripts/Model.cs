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
    public enum ChainName {
        ETH,
        BSC
    }
    public enum ChainId {
        ETH_MAINNET,
        ETH_ROPSTEN
    }
    public class Contract : Web3ify {
        protected string m_Contract;
        public BigInteger TotalSupply {get;protected set;}
        public BigInteger Decimals {get;protected set;}
        public string Name {get;protected set;}
        public string Symbol {get;protected set;}
        public string Owner {get;protected set;}
        public Contract(string _contract, ChainName _n, ChainId _i) : base(_n,_i) {
            m_Contract = _contract;
        }
        // slight loss of precision
        public double ValueFromDecimals(BigInteger _value) {
            double _denom = (double)Mathf.Pow(10,(float)Decimals);
            double _val = (double)_value/_denom;
            return _val;
        }
        /*
            Supported output spec:
                int
                string
                bool

            Inputs:
                _signature: "balanceOf(address)"
                _outputs: ["int"]
                _inputs: ["0xbd0dbb9fddc73b6ebffc7c09cfae1b19d6dece40"]
            Outputs:
                ["0"]
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
                string _inputStr = FunctionHash(_signature);
                int i = 0;
                foreach(string _in in _inputTypes) {
                    switch (_in) {
                        case "": break;
                        case "address":
                            _inputStr += EncodeAddress(_inputs[i]);
                            break;
                        default:
                            LogWarning("Could not call function ["+_signature+"]. Unsupported input found in signature ("+_in+").");
                            return _ret;
                    }
                    i++;
                }
                
                CallInput _input = new CallInput(_inputStr, m_Contract);
                string _result = await m_Web3.Eth.Transactions.Call.SendRequestAsync(_input);
                _result = _result.Substring(2);
                Debug.Log(_signature+": "+_result);

                BigInteger _ptr = 0;
                int _cursor = 0;
                List<int[]> _history = new List<int[]>();
                for (int k = 0; k < _outputs.Length; k++) {
                    string _out = _outputs[k];
                    Debug.Log("Checking output: "+_out+" where cursor = "+_cursor);
                    if (DidAnalyze(_history, ref _cursor)) {
                        Debug.Log("Already analyzed here. Moving cursor to "+_cursor);
                        k--;
                        continue;
                    }
                    string _data = "";
                    switch (_out) {
                        case "struct":
                            // parse data and insert into return list
                            break;
                        case "bool":
                            _data = _result.Substring(_cursor,64);
                            _ret.Add((new HexBigInteger(_data)).Value == 1 ? true : false);
                            _cursor += 64;
                            break;
                        case "string": // dynamic
                            _ptr = GetPointerOrLength(_result, _cursor) * 2;
                            _data = _result.Substring((int)_ptr,128);
                            _ret.Add(HexToString(_data));
                            AddHistory(ref _history, _ptr, 128);
                            _cursor += 64;
                            break;
                        case "address":
                            _data = _result.Substring(_cursor,64);
                            _ret.Add(HexAddressToString(_data));
                            _cursor += 64;
                            break;
                        case "int[]":
                        case "uint[]":
                        case "uint256[]":
                        case "uint128[]":
                        case "uint64[]":
                        case "uint32[]":
                        case "uint16[]":
                        case "uint8[]":
                            break;
                        case "int":
                        case "uint":
                        case "uint256":
                        case "uint128":
                        case "uint64":
                        case "uint32":
                        case "uint16":
                        case "uint8":
                            _data = _result.Substring(_cursor,64);
                            _ret.Add((new HexBigInteger(_data)).Value);
                            _cursor += 64;
                            break;
                        default:
                            LogWarning("Could not call function ["+_signature+"]. Unsupported output found in signature ("+_out+").");
                            return _ret;
                    }
                }
            } catch (System.Exception _e) {
                LogWarning("Something went wrong calling function ["+_signature+"]: "+_e);
            }

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
    public class Web3ify {
        protected Web3 m_Web3;
        protected ChainName m_ChainName;
        protected ChainId m_ChainId;
        public ChainName chainName {
            get { return m_ChainName; }
            set {
                m_ChainName = value;
                m_Web3 = new Web3(GetNodeURL(m_ChainName, m_ChainId));
            }
        }
        public ChainId chainId {
            get { return m_ChainId; }
            set {
                m_ChainId = value;
                m_Web3 = new Web3(GetNodeURL(m_ChainName, m_ChainId));
            }
        }
        public Web3ify(ChainName _n, ChainId _i) {
            m_ChainName = _n;
            m_ChainId = _i;
            m_Web3 = new Web3(GetNodeURL(m_ChainName, m_ChainId));
        }
        private string GetNodeURL(ChainName _chainName, ChainId _chainId) {
            switch (_chainName) {
                case ChainName.ETH:
                    switch (_chainId) {
                        case ChainId.ETH_ROPSTEN: // ropsten
                            return Constants.NODEDEFAULT_ETH_ROPSTEN;
                        case ChainId.ETH_MAINNET: // mainnet
                            return Constants.NODEDEFAULT_ETH_MAINNET;
                    }
                    break;
            }
            return Constants.NODEDEFAULT_ETH_ROPSTEN;
        }
        private byte[] CalculateHash(byte[] _value)
        {
            var _digest = new KeccakDigest(256);
            var _output = new byte[_digest.GetDigestSize()];
            _digest.BlockUpdate(_value, 0, _value.Length);
            _digest.DoFinal(_output, 0);
            return _output;
        }
        protected string FunctionHash(string _func) {
            byte[] _bytes = Encoding.ASCII.GetBytes(_func);
            _bytes = CalculateHash(_bytes);
            byte[] _newBytes = new byte[4];
            for (int i = 0; i < 4; i++) {
                _newBytes[i] = _bytes[i];
            }
            return "0x"+HexByteConvertorExtensions.ToHex(_newBytes);
        }
        protected string EncodeAddress(string _str) {
            return new ABIEncode().GetABIParamsEncoded(new InputAddress(){Value=_str}).ToHex();
        }
        protected string HexToString(string _hex) {
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
        protected string HexAddressToString(string _addr) {
            return "0x"+_addr.Replace("000000000000000000000000", "");
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
    public class InputAddress {
        [Parameter("address", "_any")]
        public string Value {get;set;}
    }
}