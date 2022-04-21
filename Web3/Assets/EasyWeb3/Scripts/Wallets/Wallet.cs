using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using UnityEngine;

namespace EasyWeb3 {
    public class Wallet : Web3ify {
        private string m_Address;

        public Wallet(string _addr, ChainId _i) : base(_i) {
            m_Address = _addr;
        }

        public async Task<ulong> GetBalance(string _addr) {
            HexBigInteger _big = await m_Web3.Eth.GetBalance.SendRequestAsync(_addr);
            ulong _bal = (ulong)_big.Value;
            return _bal/(ulong)Mathf.Pow(10,18);
        }

        public async Task<ulong> GetTransactionCount(string _addr) {
            HexBigInteger _big = await m_Web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(_addr);
            ulong _count = (ulong)_big.Value;
            return _count;
        }
    }
}