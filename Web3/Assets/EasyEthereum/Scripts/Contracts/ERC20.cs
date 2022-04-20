using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;

namespace EasyWeb3 {
    public class ERC20 : Contract {
        private string ERC20ABI = "[{\"constant\":true,\"inputs\":[],\"name\":\"name\",\"outputs\":[{\"name\":\"\",\"type\":\"string\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_spender\",\"type\":\"address\"},{\"name\":\"_value\",\"type\":\"uint256\"}],\"name\":\"approve\",\"outputs\":[{\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"totalSupply\",\"outputs\":[{\"name\":\"\",\"type\":\"uint256\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_from\",\"type\":\"address\"},{\"name\":\"_to\",\"type\":\"address\"},{\"name\":\"_value\",\"type\":\"uint256\"}],\"name\":\"transferFrom\",\"outputs\":[{\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"decimals\",\"outputs\":[{\"name\":\"\",\"type\":\"uint8\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"name\":\"_owner\",\"type\":\"address\"}],\"name\":\"balanceOf\",\"outputs\":[{\"name\":\"balance\",\"type\":\"uint256\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"symbol\",\"outputs\":[{\"name\":\"\",\"type\":\"string\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_to\",\"type\":\"address\"},{\"name\":\"_value\",\"type\":\"uint256\"}],\"name\":\"transfer\",\"outputs\":[{\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"name\":\"_owner\",\"type\":\"address\"},{\"name\":\"_spender\",\"type\":\"address\"}],\"name\":\"allowance\",\"outputs\":[{\"name\":\"\",\"type\":\"uint256\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"payable\":true,\"stateMutability\":\"payable\",\"type\":\"fallback\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"name\":\"owner\",\"type\":\"address\"},{\"indexed\":true,\"name\":\"spender\",\"type\":\"address\"},{\"indexed\":false,\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"Approval\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"name\":\"from\",\"type\":\"address\"},{\"indexed\":true,\"name\":\"to\",\"type\":\"address\"},{\"indexed\":false,\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"Transfer\",\"type\":\"event\"}]";
        
        public ERC20(string _contract, ChainId _i) : base(_contract,ChainName.ETH,_i) {
            
        }

        public async Task<bool> Load() {
            await GetDecimals();
            await GetTotalSupply();
            await GetName();
            await GetSymbol();
            await GetOwner();
            return true;
        }

        public async Task<BigInteger> GetTotalSupply() {
            var _out = await CallFunction("totalSupply()", new string[]{"uint"});
            TotalSupply = (BigInteger)_out[0];
            return TotalSupply;
        }

        public async Task<BigInteger> GetDecimals() {
            var _out = await CallFunction("decimals()", new string[]{"uint"});
            Decimals = (BigInteger)_out[0];
            return Decimals;
        }

        public async Task<string> GetName() {
            var _out = await CallFunction("name()", new string[]{"string"});
            Name = (string)_out[0];
            return Name;
        }

        public async Task<string> GetSymbol() {
            var _out = await CallFunction("symbol()", new string[]{"string"});
            Symbol = (string)_out[0];
            return Symbol;
        }

        public async Task<string> GetOwner() {
            var _out = await CallFunction("owner()", new string[]{"address"});
            Owner = (string)_out[0];
            return Owner;
        }

        public async Task<BigInteger> GetBalanceOf(string _addr) {
            var _out = await CallFunction("balanceOf(address)", new string[]{"uint"}, new string[]{_addr});
            return (BigInteger)_out[0];
        }

        public async Task<BigInteger> GetAllowance(string _owner, string _spender) {
            var _out = await CallFunction("allowance(address,address)", new string[]{"uint"}, new string[]{_owner,_spender});
            return (BigInteger)_out[0];
        }
    }
}