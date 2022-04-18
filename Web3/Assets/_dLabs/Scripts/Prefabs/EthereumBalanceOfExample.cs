using System.Numerics;
using Nethereum.Web3;
using UnityEngine;

public class EthereumBalanceOfExample : MonoBehaviour
{
  async void Start()
  {
    string network = "rinkeby"; // mainnet ropsten kovan rinkeby goerli 
    Ethereum ethereum = new Ethereum(network);
    string account = "0xaca9b6d9b1636d99156bb12825c75de1e5a58870";
    BigInteger wei = await ethereum.BalanceOf(account);
    print("wei: " + wei);
    print("eth: " + Web3.Convert.FromWei(wei));
  }
}
