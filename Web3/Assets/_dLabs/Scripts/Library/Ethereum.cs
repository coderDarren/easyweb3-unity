using System;
using System.Threading.Tasks;
using System.Numerics;
using Nethereum.Web3;

public class Ethereum
{
  private Web3 web3;

  public Ethereum(string _network, string _privateKey = "0000000000000000000000000000000000000000000000000000000000000001")
  {
    string privateKey = UnityWallet.PrivateKey() != "" ? UnityWallet.PrivateKey() : _privateKey;
    var account = new Nethereum.Web3.Accounts.Account(privateKey);
    string url = "https://" + _network + ".infura.io/v3/fbc0597d7f784931a68acca3eb26f65b";
    this.web3 = new Web3(account, url);
  }

  async public Task<BigInteger> BalanceOf(string _account)
  {
    var balance = await this.web3.Eth.GetBalance.SendRequestAsync(_account);
    return balance.Value;
  }

  async public Task<string> Send(string _toAccount, decimal _ethAmount)
  {
    var transaction = await web3.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(_toAccount, _ethAmount);
    // https://github.com/Nethereum/Nethereum/blob/master/src/Nethereum.RPC/Eth/DTOs/TransactionReceipt.cs
    return transaction.TransactionHash;
  }

  async public Task<BigInteger> BlockHeight()
  {
    var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
    return blockNumber.Value;
  }

}
