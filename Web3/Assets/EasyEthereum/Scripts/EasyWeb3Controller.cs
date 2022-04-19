using System.Collections;
using System.Collections.Generic;
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
            ERC20 _token = new ERC20("0x1A2933fbA0c6e959c9A2D2c933f3f8AD4aa9f06e", ChainName.ETH, ChainId.ETH_MAINNET);
            BigInteger _totalSupply = await _token.GetTotalSupply();
            Debug.Log("total supply: "+_totalSupply);
        }

        private async void GetChainId() {
            HexBigInteger _hex = await m_Web3.Eth.ChainId.SendRequestAsync();
            ulong _chain = (ulong)_hex.Value;
            Debug.Log("Chain: "+_chain);
        }

    #endregion
    }
}