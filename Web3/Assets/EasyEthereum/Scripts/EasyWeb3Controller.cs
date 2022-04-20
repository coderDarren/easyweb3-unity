using System.Collections;
using System;
using System.Numerics;
using System.Linq;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.Transactions;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using UnityEngine;

namespace EasyWeb3 {
    public class EasyWeb3Controller : MonoBehaviour
    {
        private Web3 m_Web3;

    #region Unity Standard Functions
        private void Start() {
            Load();
        }
    #endregion

    #region Public UI Accessor Functions

    #endregion

    #region Private Functions
        private async void Load() {
            // ERC20 _token = new ERC20("0x1A2933fbA0c6e959c9A2D2c933f3f8AD4aa9f06e", ChainId.ETH_MAINNET);
            ERC20 _token = new ERC20("0x4C9B1DB55c8A89cb0312C8fD84B0cB650E648618", ChainId.ETH_ROPSTEN);
            // method 1
            // Debug.Log("\nMethod 1 ERC20 Data Extraction");
            await _token.Load();
            Debug.Log("decimals: "+_token.Decimals);
            Debug.Log("name: "+_token.Name);
            Debug.Log("symbol: "+_token.Symbol);
            Debug.Log("owner: "+_token.Owner);
            Debug.Log("total supply: "+_token.ValueFromDecimals(_token.TotalSupply));

            // // method 2
            // Debug.Log("\nMethod 2 ERC20 Data Extraction");
            // BigInteger _decimals = await _token.GetDecimals();
            // BigInteger _totalSupply = await _token.GetTotalSupply();
            // string _name = await _token.GetName();
            // string _symbol = await _token.GetSymbol();
            // string _owner = await _token.GetOwner();
            // Debug.Log("decimals: "+_decimals);
            // Debug.Log("name: "+_name);
            // Debug.Log("symbol: "+_symbol);
            // Debug.Log("owner: "+_owner);
            // Debug.Log("total supply: "+_token.ValueFromDecimals(_totalSupply));

            // // standard calls
            // Debug.Log("\nStandard ERC20 Data");
            // BigInteger _balance = await _token.GetBalanceOf("0x34221445c2dd9fd3f41a8a8bfa7d49ec898e0ef4");
            // BigInteger _allowance = await _token.GetAllowance("0xbd0dbb9fddc73b6ebffc7c09cfae1b19d6dece40", Constants.ADDR_UNISWAPV2);
            // Debug.Log("balance: "+_token.ValueFromDecimals(_balance));
            // Debug.Log("allowance: "+_token.ValueFromDecimals(_allowance));

            // var _out = await _token.CallFunction("isSniper(address)", new string[]{"bool"}, new string[]{"0x34221445c2dd9fd3f41a8a8bfa7d49ec898e0ef4"});
            // bool _isSniper = (bool)_out[0];
            // Debug.Log(_isSniper);

            // var _out = await _token.CallFunction("totalSupply()", new string[]{"int"});
            // BigInteger _supply = (BigInteger)_out[0];
            // Debug.Log(_out[0]);
            
            // Debug.Log("TEST: getUint256()");
            // var _out = await _token.CallFunction("getUint256()", new string[]{"uint256"});
            // Debug.Log("returned: "+_out[0]);

            // Debug.Log("TEST: getUint128()");
            // _out = await _token.CallFunction("getUint128()", new string[]{"uint128"});
            // Debug.Log("returned: "+_out[0]);

            // Debug.Log("TEST: getUint64()");
            // _out = await _token.CallFunction("getUint64()", new string[]{"uint64"});
            // Debug.Log("returned: "+_out[0]);

            // Debug.Log("TEST: getUint32()");
            // _out = await _token.CallFunction("getUint32()", new string[]{"uint32"});
            // Debug.Log("returned: "+_out[0]);

            // Debug.Log("TEST: getUint16()");
            // _out = await _token.CallFunction("getUint16()", new string[]{"uint16"});
            // Debug.Log("returned: "+_out[0]);

            // Debug.Log("TEST: getUint8()");
            // _out = await _token.CallFunction("getUint8()", new string[]{"uint8"});
            // Debug.Log("returned: "+_out[0]);

            // Debug.Log("TEST: getUint()");
            // _out = await _token.CallFunction("getUint()", new string[]{"uint"});
            // Debug.Log("returned: "+_out[0]);

            // Debug.Log("TEST: getDataStruct()");
            // var _out = await _token.CallFunction("getDataStruct()", new string[]{"uint256","bool","address","string"});
            // Debug.Log("returned: "+_out[0]+" | "+_out[1]+" | "+_out[2]+" | "+_out[3]);
            
            // Debug.Log("TEST: getPrimitiveMultiReturnData()");
            // var _out = await _token.CallFunction("getPrimitiveMultiReturnData()", new string[]{"uint256","string","bool","address"});
            // Debug.Log("returned length: "+_out.Count);
            // Debug.Log(_out[0]);
            // Debug.Log(_out[1]);
            // Debug.Log(_out[2]);
            // Debug.Log(_out[3]);

            Debug.Log("TEST: getStructMultiReturnData()");
            var _out = await _token.CallFunction("getStructMultiReturnData()", new string[]{"uint256","string","bool","address","struct","uint256","string","bool","address"});
            foreach(object _o in _out) {
                Debug.Log(_o);
            }

            // Debug.Log((new HexBigInteger("0x0000000000000000000000000000000000000000000000000000000000000060")).Value);
            // string _data = "0000000000000000000000000000000000000000000000000000000000000060000000000000000000000000000000000000000000000000000000000000000100000000000000000000000000000000000000000000000000000000000000a0000000000000000000000000000000000000000000000000000000000000000464617665000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000003000000000000000000000000000000000000000000000000000000000000000100000000000000000000000000000000000000000000000000000000000000020000000000000000000000000000000000000000000000000000000000000003";
            // Debug.Log(_data.Substring(192));
        }

        private async void GetChainId() {
            HexBigInteger _hex = await m_Web3.Eth.ChainId.SendRequestAsync();
            ulong _chain = (ulong)_hex.Value;
            Debug.Log("Chain: "+_chain);
        }

    #endregion
    }
}