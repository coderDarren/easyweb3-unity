using System;
using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Hex.HexTypes;

namespace EasyWeb3
{
    public class Contract : Web3ify
    {
        protected string m_Contract;
        public BigInteger TotalSupply { get; protected set; }
        public BigInteger Decimals { get; protected set; }
        public string Name { get; protected set; }
        public string Symbol { get; protected set; }
        public string Owner { get; protected set; }
        public Contract(string _contract, ChainId _i) : base(_i)
        {
            m_Contract = _contract;
        }
        public Contract(string _contract, string _node) : base(_node)
        {
            m_Contract = _contract;
        }
        // slight loss of precision
        public double ValueFromDecimals(BigInteger _value)
        {
            double _denom = (double)Math.Pow(10, (float)Decimals);
            double _val = (double)_value / _denom;
            return _val;
        }
        /*
            Inputs:
                _signature: "balanceOf(address)"
                _outputs: ["int"]
                _inputs: ["0xbd0dbb9fddc73b6ebffc7c09cfae1b19d6dece40"]
            Outputs:
                ["0"]
            All Examples at /Assets/EasyWeb3/Scripts/Web3Components/UI/TestViewer.cs
         */
        public async Task<List<object>> CallFunction(string _signature, string[] _outputs, string[] _inputs = null)
        {
            List<object> _ret = new List<object>();
            try
            {
                if (_outputs.Length == 0)
                {
                    //LogWarning("Could not call function [" + _signature + "]: No outputs specified.");
                    return _ret;
                }

                int _inputStart = _signature.IndexOf("(");
                if (_inputStart == -1)
                {
                    //LogWarning("Could not call function [" + _signature + "]: Illegal format. Expected '('.");
                    return _ret;
                }
                int _inputEnd = _signature.IndexOf(")");
                if (_inputEnd == -1)
                {
                    //LogWarning("Could not call function [" + _signature + "]: Illegal format. Expected ')'.");
                    return _ret;
                }

                string[] _inputTypes = _signature.Substring(_inputStart + 1, _inputEnd - _inputStart - 1).Split(',');
                string _encodedInput = new Encoder().Encode(_signature, _inputTypes, _inputs);
                // Debug.Log("Encoded input: "+_encodedInput+" to contract: "+m_Contract);

                CallInput _input = new CallInput(_encodedInput, m_Contract);
                string _result = await m_Web3.Eth.Transactions.Call.SendRequestAsync(_input);
                _result = _result.Substring(2);

                // Debug.Log("decode: "+_result);
                int _l = 0; _ret = new Decoder().Decode(_result, _outputs, ref _l);
            }
            catch (System.Exception)
            {
                //LogWarning("Something went wrong calling function [" + _signature + "]: " + _e);
            }

            return _ret;
        }

        public async Task<List<Nethereum.RPC.Eth.DTOs.Transaction>> Scan(TransactionScanDelegate _onScanComplete = null)
        {
            List<Nethereum.RPC.Eth.DTOs.Transaction> _ret = new List<Nethereum.RPC.Eth.DTOs.Transaction>();
            HexBigInteger _blockNum = await GetBlockNumber();
            if (_blockNum <= m_LastScannedBlock && m_LastScannedBlock != 0)
            {
                if (_onScanComplete != null)
                    _onScanComplete(_ret, _blockNum, false);
                return _ret;
            }
            m_LastScannedBlock = _blockNum;
            // Log("[Scan] Scanning Block "+_blockNum);
            var _result = await GetTransactionsOnBlock(_blockNum);
            foreach (Nethereum.RPC.Eth.DTOs.Transaction _tx in _result.Transactions)
            {
                if (_tx.To.ToLower() == m_Contract.ToLower())
                {
                    _ret.Add(_tx);
                }
            }
            if (_onScanComplete != null)
                _onScanComplete(_ret, _blockNum, true);
            return _ret;
        }
    }
}