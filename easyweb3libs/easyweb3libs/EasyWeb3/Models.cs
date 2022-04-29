using System.Numerics;
using System.Collections.Generic;

namespace EasyWeb3
{
    public delegate void StandardDelegate();
    public delegate void StringDelegate(string _s);
    public delegate void TransactionScanDelegate(List<Nethereum.RPC.Eth.DTOs.Transaction> _txs, BigInteger _blockNum, bool _newBlock);

    public enum ChainId
    {
        ETH_MAINNET,
        ETH_ROPSTEN,
        BSC_MAINNET,
        BSC_TESTNET,
        MATIC_MAINNET,
        MATIC_TESTNET
    }
}
