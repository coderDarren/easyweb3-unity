using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyWeb3;
using UnityEngine;

public class Web3Player : MonoBehaviour
{
    public string ethAddress;
    public string bscAddress;
    public string maticAddress;

    public async Task<double> GetNativeBalanceFromChain(EasyWeb3.ChainId _chainId) {
        return await (new Wallet(GetAddressFromChain(_chainId), _chainId)).GetNativeBalance();
    }

    public async Task<double> GetERC20BalanceFromChain(string _contract, EasyWeb3.ChainId _chainId) {
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
