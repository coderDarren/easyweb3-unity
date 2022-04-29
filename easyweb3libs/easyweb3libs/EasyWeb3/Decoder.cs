using Nethereum.Hex.HexTypes;
using System.Collections.Generic;
using System.Numerics;

namespace EasyWeb3
{
    public class Decoder
    {
        public List<object> Decode(string _hex, string[] _outputs, ref int _len)
        {
            List<object> _ret = new List<object>();
            BigInteger _ptr = 0;
            int _cursor = 0;
            List<int[]> _history = new List<int[]>();
            for (int k = 0; k < _outputs.Length; k++)
            {
                string _out = _outputs[k];
                // Debug.Log("Checking output: "+_out+" where cursor = "+_cursor);
                if (DidAnalyze(_history, ref _cursor))
                {
                    // Debug.Log("Already analyzed here. Moving cursor to "+_cursor);
                    k--;
                    continue;
                }

                string _data = "";
                BigInteger _arrlen = 0;
                int _byterows = 0;
                switch (_out)
                {
                    case "bool":
                        _data = _hex.Substring(_cursor, 64);
                        _ret.Add((new HexBigInteger(_data)).Value == 1 ? true : false);
                        _cursor += 64;
                        break;
                    case "bool[]":
                        _ptr = GetPointerOrLength(_hex, _cursor) * 2;
                        _arrlen = GetPointerOrLength(_hex, (int)_ptr);
                        bool[] _boolarr = new bool[(int)_arrlen];
                        for (int i = 0; i < _boolarr.Length; i++)
                        {
                            _data = _hex.Substring((int)_ptr + 64 * (i + 1), 64);
                            _boolarr[i] = (new HexBigInteger(_data)).Value == 1 ? true : false;
                        }
                        _ret.Add(_boolarr);
                        AddHistory(ref _history, _ptr, 64 * _boolarr.Length + 64);
                        _cursor += 64;
                        break;
                    case "bytes":
                    case "string": // dynamic
                        _ptr = GetPointerOrLength(_hex, _cursor) * 2;
                        _arrlen = GetPointerOrLength(_hex, (int)_ptr);
                        _byterows = 1 + ((int)_arrlen - 1) / 32;
                        _data = _hex.Substring((int)_ptr + 64, 64 * _byterows);
                        _ret.Add(Web3Utils.HexToString(_data));
                        string _str = Web3Utils.HexToString(_data);
                        AddHistory(ref _history, _ptr, 64 + 64 * _byterows);
                        _cursor += 64;
                        break;
                    case "bytes[]":
                    case "string[]":
                        _ptr = GetPointerOrLength(_hex, _cursor) * 2;
                        _arrlen = GetPointerOrLength(_hex, (int)_ptr);
                        int _historylen = 64;
                        string[] _strarr = new string[(int)_arrlen];
                        for (int i = 0; i < _strarr.Length; i++)
                        {
                            var _subptr = GetPointerOrLength(_hex, (int)_ptr + 64 * (i + 1)) * 2;
                            _arrlen = GetPointerOrLength(_hex, (int)_ptr + (int)_subptr + 64);
                            _byterows = 1 + ((int)_arrlen - 1) / 32;
                            _data = _hex.Substring((int)_ptr + (int)_subptr + 128, 64 * _byterows);
                            _strarr[i] = Web3Utils.HexToString(_data);
                            _historylen += (64 + _byterows * 64);
                        }
                        _ret.Add(_strarr);
                        AddHistory(ref _history, _ptr, _historylen);
                        _cursor += 64;
                        break;
                    case "simplebytes":
                    case "address":
                        _data = _hex.Substring(_cursor, 64);
                        _ret.Add(Web3Utils.HexAddressToString(_data));
                        _cursor += 64;
                        break;
                    case "address[]":
                        _ptr = GetPointerOrLength(_hex, _cursor) * 2;
                        _arrlen = GetPointerOrLength(_hex, (int)_ptr);
                        string[] _addrarr = new string[(int)_arrlen];
                        for (int i = 0; i < _addrarr.Length; i++)
                        {
                            _data = _hex.Substring((int)_ptr + 64 * (i + 1), 64);
                            _addrarr[i] = Web3Utils.HexAddressToString(_data);
                        }
                        _ret.Add(_addrarr);
                        AddHistory(ref _history, _ptr, 64 * _addrarr.Length + 64);
                        _cursor += 64;
                        break;
                    case "uint[]":
                        _ptr = GetPointerOrLength(_hex, _cursor) * 2;
                        _arrlen = GetPointerOrLength(_hex, (int)_ptr);
                        BigInteger[] _intarr = new BigInteger[(int)_arrlen];
                        for (int i = 0; i < _intarr.Length; i++)
                        {
                            _data = _hex.Substring((int)_ptr + 64 * (i + 1), 64);
                            _intarr[i] = (new HexBigInteger(_data)).Value;
                        }
                        _ret.Add(_intarr);
                        AddHistory(ref _history, _ptr, 64 * _intarr.Length);
                        _cursor += 64;
                        break;
                    case "uint":
                        _data = _hex.Substring(_cursor, 64);
                        _ret.Add((new HexBigInteger(_data)).Value);
                        _cursor += 64;
                        break;
                    default:
                        if (_out.Contains("struct"))
                        {
                            int _structStart = _out.IndexOf("(");
                            if (_structStart == -1)
                            {
                                //Debug.LogWarning("Could not call function: Illegal output [" + _out + "] struct format. Expected '('.");
                                return _ret;
                            }
                            int _structEnd = _out.IndexOf(")");
                            if (_structEnd == -1)
                            {
                                //Debug.LogWarning("Could not call function: Illegal output [" + _out + "] struct format. Expected ')'.");
                                return _ret;
                            }

                            string[] _structTypes = _out.Substring(_structStart + 1, _structEnd - _structStart - 1).Split(',');

                            _ptr = GetPointerOrLength(_hex, _cursor) * 2;
                            int _l = 0;
                            List<object> _struct = new Decoder().Decode(_hex.Substring((int)_ptr), _structTypes, ref _l);
                            foreach (object _o in _struct)
                            {
                                _ret.Add(_o);
                            }
                            AddHistory(ref _history, _ptr, _l);
                            _cursor += 64;
                        }
                        else
                        {
                            //Debug.LogWarning("Could not decode. Unsupported output found in signature (" + _out + ").");
                            return _ret;
                        }
                        break;
                }
            }
            _len = _cursor;
            return _ret;
        }
        private BigInteger GetPointerOrLength(string _data, int _cursor)
        {
            string _datapoint = _data.Substring(_cursor, 64);
            return (new HexBigInteger(_datapoint)).Value;
        }
        private void AddHistory(ref List<int[]> _history, BigInteger _ptr, int _length)
        {
            int[] _tuple = new int[2] { (int)_ptr, (int)_ptr + _length };
            _history.Add(_tuple);
        }
        private bool DidAnalyze(List<int[]> _history, ref int _cursor)
        {
            foreach (int[] _tuple in _history)
            {
                if (_cursor >= _tuple[0] && _cursor < _tuple[1])
                {
                    _cursor = _tuple[1];
                    return true;
                }
            }
            return false;
        }
    }
}