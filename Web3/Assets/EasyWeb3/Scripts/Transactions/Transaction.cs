using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.RPC;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors.Extensions;

namespace EasyWeb3 {
    public class Transaction : Web3ify {
        private string m_Hash;
        public Transaction(string _hash, ChainId _i) : base(_i) {
            m_Hash = _hash;
        }

        private async void GetTransaction(string _hash) {
            try {
                Nethereum.RPC.Eth.DTOs.Transaction _tx = await m_Web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(_hash);
                //Debug.Log(_tx.Value);
            } catch (System.Exception) {
                //Debug.Log("An error occurred getting tx: "+_e);
            }
        }

        private async void GetTransactionReceipt(string _hash) {
            try {
                Nethereum.RPC.Eth.DTOs.TransactionReceipt _receipt = await m_Web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(_hash);
                //Debug.Log(_receipt.BlockNumber);
            } catch (System.Exception) {
                //Debug.Log("An error occurred getting tx receipt: "+_e);
            }
        }
    }
}