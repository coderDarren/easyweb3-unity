
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
using Nethereum.Util;
using Nethereum.Contracts;
using Nethereum.Contracts.Extensions;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using UnityEngine;

namespace EasyWeb3 {
    // public delegate void StandardDelegate();
    // public delegate void StringDelegate(string _s);
    // public delegate void TransactionScanDelegate(List<Nethereum.RPC.Eth.DTOs.Transaction> _txs, BigInteger _blockNum, bool _newBlock);
    // public class Web3ify {
    //     protected Web3 m_Web3;
    //     protected ChainId m_ChainId;
    //     protected BigInteger m_LastScannedBlock;
    //     public ChainId chainId {
    //         get { return m_ChainId; }
    //         set {
    //             m_ChainId = value;
    //             m_Web3 = new Web3(GetNodeURL(m_ChainId));
    //         }
    //     }
    //     public Web3ify(ChainId _i) {
    //         m_ChainId = _i;
    //         m_Web3 = new Web3(GetNodeURL(m_ChainId));
    //     }
    //     public async Task<BigInteger> GetChainId() {
    //         HexBigInteger _hex = await m_Web3.Eth.ChainId.SendRequestAsync();
    //         return _hex.Value;
    //     }
    //     public async Task<Nethereum.RPC.Eth.DTOs.Transaction> GetTransaction(string _hash) {
    //         try {
    //             return await m_Web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(_hash);
    //         } catch (System.Exception) {
    //             return null;
    //         }
    //     }
    //     public async Task<Nethereum.RPC.Eth.DTOs.TransactionReceipt> GetTransactionReceipt(string _hash) {
    //         try {
    //             return await m_Web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(_hash);
    //         } catch (System.Exception) {
    //             return null;
    //         }
    //     }
    //     public async Task<HexBigInteger> GetBlockNumber() {
    //         return await m_Web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
    //     }
    //     public async Task<Nethereum.RPC.Eth.DTOs.BlockWithTransactions> GetTransactionsOnBlock(HexBigInteger _blockNum) {
    //         return await m_Web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(_blockNum);
    //     }
    //     public async Task<List<Nethereum.RPC.Eth.DTOs.Transaction>> ScanAll(TransactionScanDelegate _onScanComplete=null) {
    //         List<Nethereum.RPC.Eth.DTOs.Transaction> _ret = new List<Nethereum.RPC.Eth.DTOs.Transaction>();
    //         HexBigInteger _blockNum = await GetBlockNumber();
    //         if (_blockNum <= m_LastScannedBlock && m_LastScannedBlock != 0) {
    //             if (_onScanComplete != null)
    //                 _onScanComplete(_ret, _blockNum, false);
    //             return _ret;
    //         }
    //         m_LastScannedBlock = _blockNum;
    //         // Log("[Scan] Scanning Block "+_blockNum);
    //         var _result = await GetTransactionsOnBlock(_blockNum);
    //         foreach(Nethereum.RPC.Eth.DTOs.Transaction _tx in _result.Transactions) {
    //             _ret.Add(_tx);
    //         }
    //         if (_onScanComplete != null)
    //             _onScanComplete(_ret, _blockNum, true);
    //         return _ret;
    //     }
    //     protected string GetNodeURL(ChainId _chainId) {
    //         switch (_chainId) {
    //             case ChainId.ETH_ROPSTEN:
    //                 return Constants.NODE_ETH_ROPSTEN;
    //             case ChainId.ETH_MAINNET:
    //                 return Constants.NODE_ETH_MAINNET;
    //             case ChainId.BSC_TESTNET:
    //                 return Constants.NODE_BSC_TESTNET;
    //             case ChainId.BSC_MAINNET:
    //                 return Constants.NODE_BSC_MAINNET;
    //             case ChainId.MATIC_TESTNET:
    //                 return Constants.NODE_MATIC_TESTNET;
    //             case ChainId.MATIC_MAINNET:
    //                 return Constants.NODE_MATIC_MAINNET;
    //         }
    //         return Constants.NODE_ETH_ROPSTEN;
    //     }
    //     // protected void Log(string _msg) {
    //     //     if (!Constants.debug) return;
    //     //     Debug.Log(_msg);
    //     // }
    //     // protected void LogWarning(string _msg) {
    //     //     if (!Constants.debug) return;
    //     //     Debug.LogWarning(_msg);
    //     // }
    //     // protected void LogError(string _msg) {
    //     //     if (!Constants.debug) return;
    //     //     Debug.LogError(_msg);
    //     // }
    // }
    
    // public class Contract : Web3ify {
    //     protected string m_Contract;
    //     public BigInteger TotalSupply {get;protected set;}
    //     public BigInteger Decimals {get;protected set;}
    //     public string Name {get;protected set;}
    //     public string Symbol {get;protected set;}
    //     public string Owner {get;protected set;}
    //     public Contract(string _contract, ChainId _i) : base(_i) {
    //         m_Contract = _contract;
    //     }
    //     // slight loss of precision
    //     public double ValueFromDecimals(BigInteger _value) {
    //         double _denom = (double)Mathf.Pow(10,(float)Decimals);
    //         double _val = (double)_value/_denom;
    //         return _val;
    //     }
    //     /*
    //         Inputs:
    //             _signature: "balanceOf(address)"
    //             _outputs: ["int"]
    //             _inputs: ["0xbd0dbb9fddc73b6ebffc7c09cfae1b19d6dece40"]
    //         Outputs:
    //             ["0"]
    //         All Examples at /Assets/EasyWeb3/Scripts/Web3Components/UI/TestViewer.cs
    //      */
    //     public async Task<List<object>> CallFunction(string _signature, string[] _outputs, string[] _inputs=null) {
    //         List<object> _ret = new List<object>();
    //         try {
    //             if (_outputs.Length == 0) {
    //                 LogWarning("Could not call function ["+_signature+"]: No outputs specified.");
    //                 return _ret;
    //             }

    //             int _inputStart = _signature.IndexOf("(");
    //             if (_inputStart == -1) {
    //                 LogWarning("Could not call function ["+_signature+"]: Illegal format. Expected '('.");
    //                 return _ret;
    //             }
    //             int _inputEnd = _signature.IndexOf(")");
    //             if (_inputEnd == -1) {
    //                 LogWarning("Could not call function ["+_signature+"]: Illegal format. Expected ')'.");
    //                 return _ret;
    //             }

    //             string[] _inputTypes = _signature.Substring(_inputStart+1, _inputEnd-_inputStart-1).Split(',');
    //             string _encodedInput = new Encoder().Encode(_signature, _inputTypes, _inputs);
    //             // Debug.Log("Encoded input: "+_encodedInput+" to contract: "+m_Contract);
                
    //             CallInput _input = new CallInput(_encodedInput, m_Contract);
    //             string _result = await m_Web3.Eth.Transactions.Call.SendRequestAsync(_input);
    //             _result = _result.Substring(2);

    //             // Debug.Log("decode: "+_result);
    //             int _l = 0; _ret = new Decoder().Decode(_result, _outputs, ref _l);
    //         } catch (System.Exception _e) {
    //             LogWarning("Something went wrong calling function ["+_signature+"]: "+_e);
    //         }

    //         return _ret;
    //     }

    //     public async Task<List<Nethereum.RPC.Eth.DTOs.Transaction>> Scan(TransactionScanDelegate _onScanComplete=null) {
    //         List<Nethereum.RPC.Eth.DTOs.Transaction> _ret = new List<Nethereum.RPC.Eth.DTOs.Transaction>();
    //         HexBigInteger _blockNum = await GetBlockNumber();
    //         if (_blockNum <= m_LastScannedBlock && m_LastScannedBlock != 0) {
    //             if (_onScanComplete != null)
    //                 _onScanComplete(_ret, _blockNum, false);
    //             return _ret;
    //         }
    //         m_LastScannedBlock = _blockNum;
    //         // Log("[Scan] Scanning Block "+_blockNum);
    //         var _result = await GetTransactionsOnBlock(_blockNum);
    //         foreach(Nethereum.RPC.Eth.DTOs.Transaction _tx in _result.Transactions) {
    //             if (_tx.To.ToLower() == m_Contract.ToLower()) {
    //                 _ret.Add(_tx);
    //             }
    //         }
    //         if (_onScanComplete != null)
    //             _onScanComplete(_ret, _blockNum, true);
    //         return _ret;
    //     }
    // }
    
}