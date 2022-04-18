using System;
using UnityEngine;

public class UnityWallet
{
  public static string PrivateKey(string _password = "")
  {
    string json = PlayerPrefs.GetString("privateKey");
    if (json == "") return "";
    var keyStoreService = new Nethereum.KeyStore.KeyStoreScryptService();
    return BitConverter.ToString(keyStoreService.DecryptKeyStoreFromJson(_password, json)).Replace("-", string.Empty).Replace("0x", string.Empty).ToLower();
  }

  public static string Account()
  {
    return PlayerPrefs.GetString("accountAddress");
  }
}
