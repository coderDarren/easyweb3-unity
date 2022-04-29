using System;
using System.Collections.Generic;

namespace EasyWeb3
{
    public class Encoder
    {
        public string Encode(string _signature, string[] _types, string[] _values)
        {
            string _ret = Web3Utils.FunctionHash(_signature);
            int i = 0;
            List<string> _lines = new List<string>();

            //write head
            foreach (string _in in _types)
            {
                switch (_in)
                {
                    case "": break;
                    case "address":
                        _lines.Add(Web3Utils.AddressToHexString(_values[i]));
                        break;
                    case "uint":
                    case "uint256":
                    case "uint128":
                    case "uint64":
                    case "uint32":
                    case "uint16":
                    case "uint8":
                    case "bool":
                        _lines.Add(Web3Utils.StringToHexBigInteger(_values[i].ToString()));
                        break;
                    // save dynamic pointer indices
                    case "uint256[]":
                    case "uint128[]":
                    case "uint64[]":
                    case "uint32[]":
                    case "uint16[]":
                    case "uint8[]":
                    case "uint[]":
                    case "bool[]":
                    case "string":
                    case "bytes":
                    case "bytes[]":
                    case "string[]":
                    case "address[]":
                        _lines.Add(i.ToString());
                        break;
                    default:
                        //Debug.LogWarning("Could not encode function [" + _signature + "]. Unsupported input type found in signature (" + _in + ").");
                        return _ret;
                }
                i++;
            }

            //write dynamics
            i = 0;
            int _head = 0;
            foreach (string _in in _types)
            {
                switch (_in)
                {
                    case "": case "address": case "uint": case "uint256": case "uint128": case "uint64": case "uint32": case "uint16": case "uint8": case "bool": break;
                    case "string":
                    case "bytes":
                        if (int.TryParse(_lines[i], out _head))
                        {
                            _lines[_head] = Web3Utils.StringToHexBigInteger((32 * _lines.Count).ToString()); // ptr
                            _lines.Add(Web3Utils.StringToHexBigInteger(_values[i].Length.ToString()));
                            _lines.Add(Web3Utils.StringToHexString(_values[i]));
                        }
                        break;
                    case "uint256[]":
                    case "uint128[]":
                    case "uint64[]":
                    case "uint32[]":
                    case "uint16[]":
                    case "uint8[]":
                    case "uint[]":
                    case "bool[]":
                        if (int.TryParse(_lines[i], out _head))
                        {
                            string[] _ints = GetInputParams(_values[i]);
                            _lines[_head] = Web3Utils.StringToHexBigInteger((32 * _lines.Count).ToString()); // ptr
                            _lines.Add(Web3Utils.StringToHexBigInteger(_ints.Length.ToString())); // len
                            foreach (string _intstr in _ints)
                            {
                                _lines.Add(Web3Utils.StringToHexBigInteger(_intstr));
                            }
                        }
                        break;
                    case "bytes[]":
                    case "string[]":
                        if (int.TryParse(_lines[i], out _head))
                        {
                            string[] _strings = GetInputParams(_values[i]);
                            _lines[_head] = Web3Utils.StringToHexBigInteger((32 * _lines.Count).ToString()); // ptr
                            _lines.Add(Web3Utils.StringToHexBigInteger(_strings.Length.ToString())); // len
                            int _tmpLen = _strings.Length * 64;
                            int _addedLen = 0;
                            for (int k = 0; k < _strings.Length; k++)
                            {
                                _lines.Add(Web3Utils.StringToHexBigInteger((((_tmpLen + (_addedLen * 64) + (k * 128)) / 2).ToString()))); // ptr
                                _addedLen += (int)((_strings[k].Length - 1) / 32);
                            }
                            foreach (string _s in _strings)
                            {
                                _lines.Add(Web3Utils.StringToHexBigInteger(_s.Length.ToString()));
                                _lines.Add(Web3Utils.StringToHexString(_s));
                            }
                        }
                        break;
                    case "address[]":
                        if (int.TryParse(_lines[i], out _head))
                        {
                            string[] _addrs = GetInputParams(_values[i]);
                            _lines[_head] = Web3Utils.StringToHexBigInteger((32 * _lines.Count).ToString()); // ptr
                            _lines.Add(Web3Utils.StringToHexBigInteger(_addrs.Length.ToString())); // len
                            foreach (string _s in _addrs)
                            {
                                _lines.Add(Web3Utils.AddressToHexString(_s));
                            }
                        }
                        break;
                    default:
                        //Debug.LogWarning("Could not encode function [" + _signature + "]. Unsupported input type found in signature (" + _in + ").");
                        return _ret;
                }
                i++;
            }

            foreach (string _s in _lines)
            {
                _ret += _s;
            }

            return _ret;
        }
        private string[] GetInputParams(string _values)
        {
            int _arrstart = _values.IndexOf("(");
            if (_arrstart == -1)
            {
                //Debug.LogWarning("Could not call function: Illegal input [" + _values + "] struct format. Expected '('.");
                throw new Exception();
            }
            int _arrend = _values.IndexOf(")");
            if (_arrend == -1)
            {
                //Debug.LogWarning("Could not call function: Illegal input [" + _values + "] struct format. Expected ')'.");
                throw new Exception();
            }
            return _values.Substring(_arrstart + 1, _arrend - _arrstart - 1).Split(','); ;
        }
    }
}