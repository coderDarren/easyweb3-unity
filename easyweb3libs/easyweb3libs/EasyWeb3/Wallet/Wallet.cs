using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;

namespace EasyWeb3
{
    public class Wallet : Web3ify
    {
        private string m_Address;

        public string ShortAddress
        {
            get
            {
                return m_Address.Substring(0, 8) + "..." + m_Address.Substring(m_Address.Length - 6);
            }
        }

        public Wallet(string _addr, ChainId _i) : base(_i)
        {
            m_Address = _addr;
        }

        public Wallet(string _addr) : base(ChainId.ETH_ROPSTEN)
        {
            m_Address = _addr;
        }

        public async Task<double> GetNativeBalance()
        {
            HexBigInteger _big = await m_Web3.Eth.GetBalance.SendRequestAsync(m_Address);
            double _bal = (double)_big.Value;
            return _bal / (double)Math.Pow(10, 18);
        }

        public async Task<double> GetERC20Balance(string _contract)
        {
            ERC20 _token = new ERC20(_contract, m_ChainId);
            var _out = await _token.CallFunction("balanceOf(address)", new string[] { "uint" }, new string[] { m_Address });
            double _bal = (double)_out[0];
            _out = await _token.CallFunction("decimals()", new string[] { "uint" });
            int _decimals = (int)_out[0];
            return _bal / (double)Math.Pow(10, _decimals);
        }

        public async Task<BigInteger> GetTransactionCount()
        {
            HexBigInteger _big = await m_Web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(m_Address);
            BigInteger _count = (BigInteger)_big.Value;
            return _count;
        }
    }
}