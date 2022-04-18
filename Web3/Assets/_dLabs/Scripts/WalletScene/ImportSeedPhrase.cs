using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Nethereum.HdWallet;
using Nethereum.KeyStore.Model;

public class ImportSeedPhrase : MonoBehaviour
{
  private string seedPhrase;
  [SerializeField]
  private InputField seedPhraseInputField = null;
  [SerializeField]
  private Button seedPhraseNextButton = null;
  [SerializeField]
  private GameObject logInWalletCanvas = null;
  [SerializeField]
  private GameObject importSeedPhraseCanvas = null;

  void Update()
  {
    // enable Next button when user enters 12+ words
    seedPhrase = seedPhraseInputField.text;
    if (seedPhrase.Split(' ').Length >= 12)
    {
      seedPhraseNextButton.interactable = true;
    }
    else
    {
      seedPhraseNextButton.interactable = false;
    }
  }

  public void OnNextButton()
  {
    try
    {
      SaveSeedPhrase();
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    catch
    {
      seedPhraseInputField.text = "Invalid Seed Phrase";
      seedPhraseNextButton.interactable = false;
    }
  }

  private void SaveSeedPhrase()
  {
    // http://docs.nethereum.com/en/latest/nethereum-managing-hdwallets/#retrieving-the-account-using-the-mnemonic-backup-seed-words
    var wallet = new Wallet(seedPhrase, "");
    var account = wallet.GetAccount(1);
    // save priv key and address to player prefs
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
  }

  public void OnBackButton()
  {
    // enable canvas
    logInWalletCanvas.SetActive(true);
    // disable current canvas
    importSeedPhraseCanvas.SetActive(false);
  }
}
