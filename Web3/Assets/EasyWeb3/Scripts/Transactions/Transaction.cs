using System.Collections.Generic;
using Nethereum.Web3;
using Nethereum.RPC;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors.Extensions;

namespace EasyWeb3 {
    public class Transaction {
        private Nethereum.RPC.Eth.DTOs.Transaction m_Tx;

        public string MethodId {
            get {
                return m_Tx.Input.Substring(0,10);
            }
        }

        public string TransactionHash {
            get {
                return m_Tx.TransactionHash;
            }
        }
        
        /// <summary>
        /// TX Object: https://github.com/Nethereum/Nethereum/blob/abd05e8a3b936419f9f278bbfe57f816ee1182b0/src/Nethereum.RPC/Eth/DTOs/Transaction.cs
        /// </summary>
        public Transaction(Nethereum.RPC.Eth.DTOs.Transaction _tx) {
            m_Tx = _tx;
        }

        public List<object> GetInputs(string[] _types) {
            int _l = 0; 
            List<object> _ret = new Decoder().Decode(m_Tx.Input.Substring(10), _types, ref _l);
            return _ret;
        }

        // private async void GetTransaction(string _hash) {
        //     try {
        //         Nethereum.RPC.Eth.DTOs.Transaction _tx = await m_Web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(_hash);
        //         //Debug.Log(_tx.Value);
        //     } catch (System.Exception) {
        //         //Debug.Log("An error occurred getting tx: "+_e);
        //     }
        // }

        // private async void GetTransactionReceipt(string _hash) {
        //     try {
        //         Nethereum.RPC.Eth.DTOs.TransactionReceipt _receipt = await m_Web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(_hash);
        //         //Debug.Log(_receipt.BlockNumber);
        //     } catch (System.Exception) {
        //         //Debug.Log("An error occurred getting tx receipt: "+_e);
        //     }
        // }
    }
}