using UnityEngine;

public class EthereumSendExample : MonoBehaviour
{
  async void Start()
  {
    string network = "rinkeby"; // mainnet ropsten kovan rinkeby goerli
    string privateKey = "0000000000000000000000000000000000000000000000000000000000000001"; // remove if using WalletScene
    Ethereum ethereum = new Ethereum(network, privateKey);

    string toAccount = "0x72b8Df71072E38E8548F9565A322B04b9C752932";
    decimal ethAmount = 0.01m;

    string transactionHash = await ethereum.Send(toAccount, ethAmount);
    print(transactionHash);
  }
}
