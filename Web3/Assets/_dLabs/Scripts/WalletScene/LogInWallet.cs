using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

public class LogInWallet : MonoBehaviour
{
  [SerializeField]
  private Button signInButton = null;
  [SerializeField]
  private GameObject logInWalletCanvas = null;
  [SerializeField]
  private GameObject createWalletCanvas = null;
  [SerializeField]
  private GameObject importSeedPhraseCanvas = null;

  // Metamask.jslib
  [DllImport("__Internal")] private static extern void MetamaskConnect();
  [DllImport("__Internal")] private static extern string MetamaskWalletAddress();
  [DllImport("__Internal")] private static extern string MetamaskNetwork();


  public void Start()
  {
    // if browser build then connect to metamask
#if UNITY_WEBGL && !UNITY_EDITOR
      // reset account address
      PlayerPrefs.SetString("accountAddress", ""); 
      // connect to metamask
      MetamaskConnect();
#endif
  }

  public void Update()
  {
    // if user saved address, enable login 
    if (PlayerPrefs.GetString("accountAddress") != "")
    {
      signInButton.interactable = true;
    }

    // if browser build then check for metamask address
#if UNITY_WEBGL && !UNITY_EDITOR
      if (MetamaskWalletAddress() != "") {
        // save metamask address
        PlayerPrefs.SetString("accountAddress", MetamaskWalletAddress());
      }
#endif
  }

  public void OnSignIn()
  {
    // move to main scene
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
  }

  public void OnCreateWallet()
  {
    // enable canvas
    createWalletCanvas.SetActive(true);
    // disable current canvas
    logInWalletCanvas.SetActive(false);
  }

  public void OnImportSeedPhrase()
  {
    // enable canvas
    importSeedPhraseCanvas.SetActive(true);
    // disable current canvas
    logInWalletCanvas.SetActive(false);
  }

}
