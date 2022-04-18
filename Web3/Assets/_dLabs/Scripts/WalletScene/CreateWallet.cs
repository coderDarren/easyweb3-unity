using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using NBitcoin;
using Nethereum.HdWallet;
using Nethereum.KeyStore.Model;

public class CreateWallet : MonoBehaviour
{
  [SerializeField]
  private Text seedPhraseText = null;
  [SerializeField]
  private GameObject logInWalletCanvas = null;
  [SerializeField]
  private GameObject createWalletCanvas = null;
  private Mnemonic mnemo;

  void Start()
  {
    // generate priv key
    mnemo = new Mnemonic(Wordlist.English, WordCount.Twelve);
    // display seed phrase
    seedPhraseText.text = mnemo.ToString();
  }

  public void OnNextButton()
  {
    // create wallet
    var wallet = new Wallet(mnemo.ToString(), "");
    var account = wallet.GetAccount(0);
    // save address to player prefs
    PlayerPrefs.SetString("accountAddress", account.Address);

    // encrypt private key with password
    string password = "";
    var keyStoreService = new Nethereum.KeyStore.KeyStoreScryptService();
    var scryptParams = new ScryptParams { Dklen = 32, N = 262144, R = 1, P = 8 };
    var ecKey = new Nethereum.Signer.EthECKey(account.PrivateKey);
    var keyStore = keyStoreService.EncryptAndGenerateKeyStore(password, ecKey.GetPrivateKeyAsBytes(), ecKey.GetPublicAddress(), scryptParams);
    string json = keyStoreService.SerializeKeyStoreToJson(keyStore);

    // save json to player prefs
    PlayerPrefs.SetString("privateKey", json);

    // load next scene
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
  }

  public void OnBackButton()
  {
    // enable canvas
    logInWalletCanvas.SetActive(true);
    // disable current canvas
    createWalletCanvas.SetActive(false);
  }

}
