using System;
using System.Numerics;
using UniRx.Async;

namespace EasyWeb3 {
    public class ERC20 : Contract {
        
        public ERC20(string _contract, ChainId _i) : base(_contract,_i) {}
        public ERC20(string _contract) : base(_contract,ChainId.ETH_ROPSTEN) {}

        public async UniTask<bool> Load() {
            await GetDecimals();
            await GetTotalSupply();
            await GetName();
            await GetSymbol();
            await GetOwner();
            return true;
        }

        public async UniTask<BigInteger> GetTotalSupply() {
            var _out = await CallFunction("totalSupply()", new string[]{"uint"});
            TotalSupply = (BigInteger)_out[0];
            return TotalSupply;
        }

        public async UniTask<BigInteger> GetDecimals() {
            var _out = await CallFunction("decimals()", new string[]{"uint"});
            Decimals = (BigInteger)_out[0];
            return Decimals;
        }

        public async UniTask<string> GetName() {
            var _out = await CallFunction("name()", new string[]{"string"});
            Name = (string)_out[0];
            return Name;
        }

        public async UniTask<string> GetSymbol() {
            var _out = await CallFunction("symbol()", new string[]{"string"});
            Symbol = (string)_out[0];
            return Symbol;
        }

        public async UniTask<string> GetOwner() {
            var _out = await CallFunction("owner()", new string[]{"address"});
            Owner = (string)_out[0];
            return Owner;
        }

        public async UniTask<BigInteger> GetBalanceOf(string _addr) {
            var _out = await CallFunction("balanceOf(address)", new string[]{"uint"}, new string[]{_addr});
            return (BigInteger)_out[0];
        }

        public async UniTask<BigInteger> GetAllowance(string _owner, string _spender) {
            var _out = await CallFunction("allowance(address,address)", new string[]{"uint"}, new string[]{_owner,_spender});
            return (BigInteger)_out[0];
        }
    }
}