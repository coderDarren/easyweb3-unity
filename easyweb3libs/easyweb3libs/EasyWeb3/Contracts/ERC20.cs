using System;
using System.Numerics;
using System.Threading.Tasks;

namespace EasyWeb3
{
    public class ERC20 : Contract
    {

        public ERC20(string _contract, ChainId _i) : base(_contract, _i) { }
        public ERC20(string _contract) : base(_contract, ChainId.ETH_ROPSTEN) { }

        public async Task<bool> Load()
        {
            try { await GetDecimals(); } catch (System.Exception) { }
            try { await GetTotalSupply(); } catch (System.Exception) { }
            try { await GetName(); } catch (System.Exception) { }
            try { await GetSymbol(); } catch (System.Exception) { }
            try { await GetOwner(); } catch (System.Exception) { }
            return true;
        }

        public async Task<BigInteger> GetTotalSupply()
        {
            var _out = await CallFunction("totalSupply()", new string[] { "uint" });
            TotalSupply = (BigInteger)_out[0];
            return TotalSupply;
        }

        public async Task<BigInteger> GetDecimals()
        {
            var _out = await CallFunction("decimals()", new string[] { "uint" });
            Decimals = (BigInteger)_out[0];
            return Decimals;
        }

        public async Task<string> GetName()
        {
            var _out = await CallFunction("name()", new string[] { "string" });
            Name = (string)_out[0];
            return Name;
        }

        public async Task<string> GetSymbol()
        {
            var _out = await CallFunction("symbol()", new string[] { "string" });
            Symbol = (string)_out[0];
            return Symbol;
        }

        public async Task<string> GetOwner()
        {
            var _out = await CallFunction("owner()", new string[] { "address" });
            Owner = (string)_out[0];
            return Owner;
        }

        public async Task<BigInteger> GetBalanceOf(string _addr)
        {
            var _out = await CallFunction("balanceOf(address)", new string[] { "uint" }, new string[] { _addr });
            return (BigInteger)_out[0];
        }

        public async Task<BigInteger> GetAllowance(string _owner, string _spender)
        {
            var _out = await CallFunction("allowance(address,address)", new string[] { "uint" }, new string[] { _owner, _spender });
            return (BigInteger)_out[0];
        }
    }
}