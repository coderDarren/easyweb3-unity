using System.Numerics;
using UnityEngine;

public class EthereumBlockHeightExample : MonoBehaviour
{
  async void Start()
  {
    string network = "rinkeby"; // mainnet ropsten kovan rinkeby goerli
    Ethereum ethereum = new Ethereum(network);
    BigInteger blockHeight = await ethereum.BlockHeight();
    print(blockHeight);
  }
}
