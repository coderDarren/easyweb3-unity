using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Nethereum.Web3;
using Nethereum.Hex.HexTypes;

namespace EasyWeb3
{
    public class Web3ify
    {
        public static string OVERLOADED_ETH_MAINNET_NODE_URL = "";
        public static string OVERLOADED_ETH_ROPSTEN_NODE_URL = "";
        public static string OVERLOADED_BSC_MAINNET_NODE_URL = "";
        public static string OVERLOADED_BSC_TESTNET_NODE_URL = "";
        public static string OVERLOADED_MATIC_MAINNET_NODE_URL = "";
        public static string OVERLOADED_MATIC_TESTNET_NODE_URL = "";

        protected Web3 m_Web3;
        protected ChainId m_ChainId;
        protected BigInteger m_LastScannedBlock;
        public ChainId chainId
        {
            get { return m_ChainId; }
            set
            {
                m_ChainId = value;
                m_Web3 = new Web3(GetNodeURL(m_ChainId));
            }
        }
        public Web3ify(ChainId _i)
        {
            m_ChainId = _i;
            m_Web3 = new Web3(GetNodeURL(m_ChainId));
        }
        public Web3ify(string _node)
        {
            m_Web3 = new Web3(_node);
        }
        public async Task<BigInteger> GetChainId()
        {
            HexBigInteger _hex = await m_Web3.Eth.ChainId.SendRequestAsync();
            return _hex.Value;
        }
        public async Task<Nethereum.RPC.Eth.DTOs.Transaction> GetTransaction(string _hash)
        {
            try
            {
                return await m_Web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(_hash);
            }
            catch (System.Exception)
            {
                return null;
            }
        }
        public async Task<Nethereum.RPC.Eth.DTOs.TransactionReceipt> GetTransactionReceipt(string _hash)
        {
            try
            {
                return await m_Web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(_hash);
            }
            catch (System.Exception)
            {
                return null;
            }
        }
        public async Task<HexBigInteger> GetBlockNumber()
        {
            return await m_Web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
        }
        public async Task<Nethereum.RPC.Eth.DTOs.BlockWithTransactions> GetTransactionsOnBlock(HexBigInteger _blockNum)
        {
            return await m_Web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(_blockNum);
        }
        public async Task<List<Nethereum.RPC.Eth.DTOs.Transaction>> ScanAll(TransactionScanDelegate _onScanComplete = null)
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
                _ret.Add(_tx);
            }
            if (_onScanComplete != null)
                _onScanComplete(_ret, _blockNum, true);
            return _ret;
        }
        protected string GetNodeURL(ChainId _chainId)
        {
            switch (_chainId)
            {
                case ChainId.ETH_ROPSTEN:
                    return OVERLOADED_ETH_ROPSTEN_NODE_URL != "" ? OVERLOADED_ETH_ROPSTEN_NODE_URL : Constants.NODE_ETH_ROPSTEN;
                case ChainId.ETH_MAINNET:
                    return OVERLOADED_ETH_MAINNET_NODE_URL != "" ? OVERLOADED_ETH_MAINNET_NODE_URL : Constants.NODE_ETH_MAINNET;
                case ChainId.BSC_TESTNET:
                    return OVERLOADED_BSC_TESTNET_NODE_URL != "" ? OVERLOADED_BSC_TESTNET_NODE_URL : Constants.NODE_BSC_TESTNET;
                case ChainId.BSC_MAINNET:
                    return OVERLOADED_BSC_MAINNET_NODE_URL != "" ? OVERLOADED_BSC_MAINNET_NODE_URL : Constants.NODE_BSC_MAINNET;
                case ChainId.MATIC_TESTNET:
                    return OVERLOADED_MATIC_TESTNET_NODE_URL != "" ? OVERLOADED_MATIC_TESTNET_NODE_URL : Constants.NODE_MATIC_TESTNET;
                case ChainId.MATIC_MAINNET:
                    return OVERLOADED_MATIC_MAINNET_NODE_URL != "" ? OVERLOADED_MATIC_MAINNET_NODE_URL : Constants.NODE_MATIC_MAINNET;
            }
            return Constants.NODE_ETH_ROPSTEN;
        }
        // protected void Log(string _msg) {
        //     if (!Constants.debug) return;
        //     Debug.Log(_msg);
        // }
        // protected void LogWarning(string _msg) {
        //     if (!Constants.debug) return;
        //     Debug.LogWarning(_msg);
        // }
        // protected void LogError(string _msg) {
        //     if (!Constants.debug) return;
        //     Debug.LogError(_msg);
        // }
    }
}