using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyWeb3;

public class NodeURLOverloader : MonoBehaviour
{
    public string ETHMainnetURL;
    public string ETHRopstenURL;
    public string BSCMainnetURL;
    public string BSCTestnetURL;
    public string MATICMainnetURL;
    public string MATICTestnetURL;

    // If Url is blank, the provided default url will be used for that chain
    private void Awake() {
        Web3ify.OVERLOADED_ETH_MAINNET_NODE_URL = ETHMainnetURL;
        Web3ify.OVERLOADED_ETH_ROPSTEN_NODE_URL = ETHRopstenURL;
        Web3ify.OVERLOADED_BSC_MAINNET_NODE_URL = BSCMainnetURL;
        Web3ify.OVERLOADED_BSC_TESTNET_NODE_URL = BSCTestnetURL;
        Web3ify.OVERLOADED_MATIC_MAINNET_NODE_URL = MATICMainnetURL;
        Web3ify.OVERLOADED_MATIC_TESTNET_NODE_URL = MATICTestnetURL;
    }
}
