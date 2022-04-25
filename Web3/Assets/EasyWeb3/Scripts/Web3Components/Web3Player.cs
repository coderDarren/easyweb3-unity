using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UniRx.Async;
using EasyWeb3;
using UnityEngine;

public class Web3Player : MonoBehaviour
{
    public string ethAddress;
    public string bscAddress;
    public string maticAddress;
    public double ethBalance;
    public double bnbBalance;
    public double polyBalance;

    private void Start() {
        Load();
    }

    private async void Load() {
        ethBalance = await GetNativeBalanceFromChain(ChainId.ETH_MAINNET);
        bnbBalance = await GetNativeBalanceFromChain(ChainId.BSC_MAINNET);
        polyBalance = await GetNativeBalanceFromChain(ChainId.MATIC_MAINNET);
        // Debug.Log(ethBalance+" "+bnbBalance+" "+polyBalance);
    }

    private async UniTask<double> GetNativeBalanceFromChain(EasyWeb3.ChainId _chainId) {
        return await (new Wallet(GetAddressFromChain(_chainId), _chainId)).GetNativeBalance();
    }

    private async UniTask<double> GetERC20BalanceFromChain(string _contract, EasyWeb3.ChainId _chainId) {
        return await (new Wallet(GetAddressFromChain(_chainId), _chainId)).GetERC20Balance(_contract);
    }
    
    public string GetAddressFromChain(EasyWeb3.ChainId _chainId) {
        switch (_chainId) {
            case EasyWeb3.ChainId.ETH_MAINNET:
            case EasyWeb3.ChainId.ETH_ROPSTEN: return ethAddress;
            case EasyWeb3.ChainId.BSC_MAINNET:
            case EasyWeb3.ChainId.BSC_TESTNET: return bscAddress;
            case EasyWeb3.ChainId.MATIC_MAINNET:
            case EasyWeb3.ChainId.MATIC_TESTNET: return maticAddress;
            default: return ethAddress;
        }
    }
}
