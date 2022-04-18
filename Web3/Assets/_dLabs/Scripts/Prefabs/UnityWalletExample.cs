using UnityEngine;

public class UnityWalletExample : MonoBehaviour
{
  void Start()
  {
    // If you're using Token/Scenes/WalletScene, this is how you access the private key and account
    // password to unlock wallet 
    string password = "";
    string privateKey = UnityWallet.PrivateKey(password);
    print(privateKey);

    string account = UnityWallet.Account();
    print(account);
  }
}
