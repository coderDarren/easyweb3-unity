using System.Collections;
using System.Collections.Generic;
using System;
using System.Numerics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nethereum.Web3.Accounts;
using Nethereum.Web3;
using Nethereum.KeyStore;
using Nethereum.KeyStore.Model;
using Nethereum.HdWallet;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.Transactions;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using NBitcoin;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class EasyWeb3Wallet : MonoBehaviour
{
    private readonly string NODEDEFAULT_ETH_MAINNET = "http://ec2-44-200-18-204.compute-1.amazonaws.com:8545";
    private readonly string NODEDEFAULT_ETH_ROPSTEN = "http://ec2-44-203-28-1.compute-1.amazonaws.com:8545";
    private readonly string SAVEPREF_WORDLIST = "easyweb3_wordlist";
    private readonly string SAVEPREF_PASSWORD = "easyweb3_password";
    private readonly string SAVEPREF_KEYSTORE = "easyweb3_keystore";
    private readonly string SAVEPREF_WALLET_COUNT = "easyweb3_walletCount";
    private readonly string SAVEPREF_SELECTED_ACCOUNT = "easyweb3_selectedAccount";
    private readonly string SAVEPREF_IMPORTED_ACCOUNTS = "easyweb3_accounts";
    private readonly string SAVEPREF_CHAIN = "easyweb3_chain";

    private string ERC20ABI = "[{\"constant\":true,\"inputs\":[],\"name\":\"name\",\"outputs\":[{\"name\":\"\",\"type\":\"string\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_spender\",\"type\":\"address\"},{\"name\":\"_value\",\"type\":\"uint256\"}],\"name\":\"approve\",\"outputs\":[{\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"totalSupply\",\"outputs\":[{\"name\":\"\",\"type\":\"uint256\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_from\",\"type\":\"address\"},{\"name\":\"_to\",\"type\":\"address\"},{\"name\":\"_value\",\"type\":\"uint256\"}],\"name\":\"transferFrom\",\"outputs\":[{\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"decimals\",\"outputs\":[{\"name\":\"\",\"type\":\"uint8\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"name\":\"_owner\",\"type\":\"address\"}],\"name\":\"balanceOf\",\"outputs\":[{\"name\":\"balance\",\"type\":\"uint256\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"symbol\",\"outputs\":[{\"name\":\"\",\"type\":\"string\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_to\",\"type\":\"address\"},{\"name\":\"_value\",\"type\":\"uint256\"}],\"name\":\"transfer\",\"outputs\":[{\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"name\":\"_owner\",\"type\":\"address\"},{\"name\":\"_spender\",\"type\":\"address\"}],\"name\":\"allowance\",\"outputs\":[{\"name\":\"\",\"type\":\"uint256\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"payable\":true,\"stateMutability\":\"payable\",\"type\":\"fallback\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"name\":\"owner\",\"type\":\"address\"},{\"indexed\":true,\"name\":\"spender\",\"type\":\"address\"},{\"indexed\":false,\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"Approval\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"name\":\"from\",\"type\":\"address\"},{\"indexed\":true,\"name\":\"to\",\"type\":\"address\"},{\"indexed\":false,\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"Transfer\",\"type\":\"event\"}]";
    private delegate void StandardFunc();

#region Data Structs
    private class Chain : JSONify {
        public string name;
        public int id;
        public Chain(string _n, int _id) {
            name = _n;
            id = _id;
        }
    }
    private class AccountPair : JSONify {
        public string name;
        public string wallet;
        public string privateKey;
        public bool imported;
        public AccountPair(string _n, string _w, string _k, bool _imported) {
            name = _n;
            wallet = _w;
            privateKey = _k;
            imported = _imported;
        }
        public string ShortWallet() {
            return wallet.Substring(0, 8) + "..." + wallet.Substring(wallet.Length - 6);
        }
    }

    private class Accounts : JSONify {
        public List<AccountPair> accounts;
        public Accounts() {
            accounts = new List<AccountPair>();
        }
    }

    [System.Serializable]
    public struct ImportWalletProperties {
        public InputField privateKey;
    }

    [System.Serializable]
    public struct CreateWalletProperties {
        public Text wordlist;
        public InputField password;
    }
#endregion

#region Inspector Properties
    [Header("Header Fields")]
    public Dropdown chainDropdown;
    public Dropdown walletDropdown;

    [Header("Wallet Home")]
    public Text balanceLabel;

    [Header("Import Wallet")]
    public ImportWalletProperties importWalletProps;

    [Header("Create Wallet")]
    public CreateWalletProperties createWalletProps;

    private Web3 m_Web3;
    private Account m_Account;
    private KeyStoreScryptService m_KeyStore;
    private Chain m_Chain;
    private Accounts m_Accounts;
#endregion

#region Utilities
    private class JSONify {
        public string ToJsonString() {
            string _ret = string.Empty;
            try {
                _ret = JsonConvert.SerializeObject(this).Replace("\n","").Replace("\t","");
            } catch (System.Exception _e) {
                Debug.LogWarning("Failed to serialize "+this.GetType()+": "+_e);
            }
            return _ret;
        }

        public static T FromJsonStr<T>(string _json) {
            try { 
                return JsonConvert.DeserializeObject<T>(_json);
            } catch (System.Exception _e) {
                Debug.LogWarning("Failed to deserialize json "+_json+": "+_e);
                return default(T);
            }
        }
    }
#endregion

#region Unity Standard Functions
    private void Start() {
        m_KeyStore = new KeyStoreScryptService();
        Load();
    }
#endregion

#region Public UI Accessor Functions
    public void Submit_ImportAccount() {
        ImportAccount(()=>{
            m_Web3 = new Web3(m_Account, GetNodeURL());
            Debug.Log("Successfully imported wallet: "+m_Account.Address);
            GetBalance();
            GetTransactionCount();
            // GetTransaction("0x2cace7ed6260ce66784eb948a4c11439c141a71eeba78c739af43ea42d3bed2f");
            // GetTransactionReceipt("0x2cace7ed6260ce66784eb948a4c11439c141a71eeba78c739af43ea42d3bed2f");
            // GetTotalSupply("0x1A2933fbA0c6e959c9A2D2c933f3f8AD4aa9f06e");
            // GetChainId();
            // TransferEth("0x9E9afF402641B749e0f20D6285Aed457982EAa9A", 0.001m, 2);
        }, ()=>{
            Debug.Log("Failed to import wallet.");
        });
    }

    public void Submit_ChainDropdown() {
        UpdateChain(()=>{
            Debug.Log("Updated chain");
        }, ()=>{
            Debug.Log("Failed to update chain.");
        });
    }

    public void Submit_WalletDropdown() {
        SelectAccount(walletDropdown.value);
    }

    public void Submit_CreateWallet() {
        CreateAccount();
    }
#endregion

#region Private Functions
    private string GetNodeURL() {
        switch (m_Chain.name) {
            case "ETH":
                switch (m_Chain.id) {
                    case 0: // ropsten
                        return NODEDEFAULT_ETH_ROPSTEN;
                    case 1: // mainnet
                        return NODEDEFAULT_ETH_MAINNET;
                }
                break;
            default:
                Debug.LogWarning("Unable to load web3. Chain name did not match supported options");
                break;
        }
        return NODEDEFAULT_ETH_ROPSTEN;
    }
    
    private void LoadChain() {
        string _savedChain = PlayerPrefs.GetString(SAVEPREF_CHAIN);
        if (_savedChain != null && _savedChain.Length != 0) {
            m_Chain = JSONify.FromJsonStr<Chain>(_savedChain);
        } else {
            m_Chain = new Chain("ETH", 3);
        }
        m_Web3 = new Web3(GetNodeURL());
    }

    private void LoadAccounts() {
        string _savedAccounts = PlayerPrefs.GetString(SAVEPREF_IMPORTED_ACCOUNTS);
        Debug.Log(_savedAccounts);
        if (_savedAccounts != null && _savedAccounts.Length != 0) {
            m_Accounts = JSONify.FromJsonStr<Accounts>(_savedAccounts);
        } else {
            m_Accounts = new Accounts();
        }

        walletDropdown.options = new List<Dropdown.OptionData>();
        foreach (AccountPair _a in m_Accounts.accounts) {
            walletDropdown.options.Add(new Dropdown.OptionData(_a.ShortWallet()));
            // if account is not imported, cycle through all created wallets
        }

        string _savedAccountSelection = PlayerPrefs.GetString(SAVEPREF_SELECTED_ACCOUNT);
        if (_savedAccountSelection != null && _savedAccountSelection.Length != 0) {

        } else {
            // load create or import page
        }

        if (m_Accounts.accounts.Count > 0)
            SelectAccount(1);
    }

    private void SelectAccount(int _dropdownIdx) {
        walletDropdown.value = _dropdownIdx;
        AccountPair _acct = m_Accounts.accounts[_dropdownIdx];
        m_Account = new Account(_acct.privateKey, m_Chain.id);
        GetBalance();
    }

    private void Load() {
        LoadChain();
        LoadAccounts();
    }

    private async void UpdateChain(StandardFunc _onSuccess, StandardFunc _onFailure) {
        try {
            switch (chainDropdown.value) {
                case 0: // ropsten
                    m_Web3 = new Web3(NODEDEFAULT_ETH_ROPSTEN);
                    m_Chain = new Chain("ETH", 3);
                    PlayerPrefs.SetString(SAVEPREF_CHAIN, m_Chain.ToJsonString());
                    SelectAccount(chainDropdown.value);
                    break;
                case 1: // mainnet
                    m_Web3 = new Web3(NODEDEFAULT_ETH_MAINNET);
                    m_Chain = new Chain("ETH", 1);
                    PlayerPrefs.SetString(SAVEPREF_CHAIN, m_Chain.ToJsonString());
                    SelectAccount(chainDropdown.value);
                    break;
                default:
                    Debug.Log("Unsupported chain.");
                    break;
            }
            _onSuccess();
        } catch (System.Exception _e) {
            Debug.Log("Failed to update chain: "+_e);
            _onFailure();
        }
    }

    private void ImportAccount(StandardFunc _onSuccess, StandardFunc _onFailure) {
        try {
            var _privateKey = new string(importWalletProps.privateKey.text.Where(c => char.IsLetterOrDigit(c) || c == ' ').ToArray());
            m_Account = new Account(_privateKey, m_Chain.id);
            bool _exists = false;
            foreach(AccountPair _acct in m_Accounts.accounts) {
                if (_acct.privateKey == _privateKey)
                    _exists = true;
            }
            if (!_exists) {
                Debug.Log("Adding new wallet");
                AddAccount(true);
            }
            _onSuccess();
        } catch (System.Exception _e) {
            Debug.Log("Something went wrong: "+_e);
            _onFailure();
        }
    }

    private void CreateAccount() {
        // check if a non-imported wallet exists
        string _savedWordlist = PlayerPrefs.GetString(SAVEPREF_WORDLIST);
        string _savedPassword = PlayerPrefs.GetString(SAVEPREF_PASSWORD);
        string _password = createWalletProps.password.text;

        // subsequent wallet
        if (_savedWordlist != null && _savedWordlist.Length > 0) {
            // load using a password and increment wallet count
            int _wordCount = PlayerPrefs.GetInt(SAVEPREF_WALLET_COUNT, 0);
            if (_password == _savedPassword) {
                m_Account = new Wallet(_savedWordlist, _password).GetAccount(_wordCount);
                m_Account = new Account(m_Account.PrivateKey, m_Chain.id); // reinit to force chain
                PlayerPrefs.SetInt(SAVEPREF_WALLET_COUNT, _wordCount+1);
            } else {
                // err
                return;
            }
        } 
        // genesis wallet
        else if (_password.Length > 6) {
            var _mnemo = new Mnemonic(Wordlist.English, WordCount.Twelve);
            createWalletProps.wordlist.text = _mnemo.ToString();

            var _wallet = new Wallet(_mnemo.ToString(), "");
            m_Account = _wallet.GetAccount(0);
            m_Account = new Account(m_Account.PrivateKey, m_Chain.id); // reinit to force chain

            var _keyStoreService = new Nethereum.KeyStore.KeyStoreScryptService();
            var _scryptParams = new ScryptParams { Dklen = 32, N = 262144, R = 1, P = 8 };
            var _ecKey = new Nethereum.Signer.EthECKey(m_Account.PrivateKey);
            var _keyStore = _keyStoreService.EncryptAndGenerateKeyStore(_password, _ecKey.GetPrivateKeyAsBytes(), _ecKey.GetPublicAddress(), _scryptParams);
            string _json = _keyStoreService.SerializeKeyStoreToJson(_keyStore);

            PlayerPrefs.SetString(SAVEPREF_WORDLIST, _mnemo.ToString());
            PlayerPrefs.SetString(SAVEPREF_PASSWORD, _password);
            PlayerPrefs.SetString(SAVEPREF_KEYSTORE, _json);
            PlayerPrefs.SetInt(SAVEPREF_WALLET_COUNT, 0);
            AddAccount(false);
        } else {
            // password must be at least 7 characters long

        }
    }

    private void AddAccount(bool _imported) {
        AccountPair _a = new AccountPair("Wallet "+m_Accounts.accounts.Count, m_Account.Address, m_Account.PrivateKey, _imported);
        m_Accounts.accounts.Add(_a);
        walletDropdown.options.Add(new Dropdown.OptionData(_a.ShortWallet()));
        // SelectAccount(walletDropdown.options.Count);
        PlayerPrefs.SetString(SAVEPREF_IMPORTED_ACCOUNTS, m_Accounts.ToJsonString());
    }

    private async void GetChainId() {
        HexBigInteger _hex = await m_Web3.Eth.ChainId.SendRequestAsync();
        ulong _chain = (ulong)_hex.Value;
        Debug.Log("Chain: "+_chain);
    }

    private async void GetBalance() {
        HexBigInteger _big = await m_Web3.Eth.GetBalance.SendRequestAsync(m_Account.Address);
        ulong _bal = (ulong)_big.Value;
        string _balStr = (_bal/Mathf.Pow(10,18)).ToString();
        balanceLabel.text = "ETH: "+_balStr;
        Debug.Log("ETH Balance: "+_balStr);
    }

    private async void GetTransactionCount() {
        HexBigInteger _big = await m_Web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(m_Account.Address);
        ulong _count = (ulong)_big.Value;
        Debug.Log("TX Count: "+_count);
    }

    private async void GetTransaction(string _hash) {
        try {
            Nethereum.RPC.Eth.DTOs.Transaction _tx = await m_Web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(_hash);
            Debug.Log(_tx.Value);
        } catch (System.Exception _e) {
            Debug.Log("An error occurred getting tx: "+_e);
        }
    }

    private async void GetTransactionReceipt(string _hash) {
        try {
            TransactionReceipt _receipt = await m_Web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(_hash);
            Debug.Log(_receipt.BlockNumber);
        } catch (System.Exception _e) {
            Debug.Log("An error occurred getting tx receipt: "+_e);
        }
    }

    private async void GetTotalSupply(string _addr) {
        try {
            var _contract = m_Web3.Eth.GetContract(ERC20ABI, _addr);
            var _func = _contract.GetFunction("totalSupply");
            var _result = await m_Web3.Eth.Transactions.Call.SendRequestAsync(new CallInput("0x18160ddd", _addr));
            Debug.Log("Result: "+_result);
            Debug.Log("From Hex: "+(new HexBigInteger(_result)).Value);
        } catch (System.Exception _e) {
            Debug.Log("An error occurred getting supply: "+_e);
        }
    }

    private async void TransferEth(string _toAddr, decimal _amount, decimal _gwei=2) {
        try {
            if (_gwei != 2) {
                // get gas price
            }
            Debug.Log("account: "+m_Account.Address);
            Debug.Log("tx account: "+m_Web3.Eth.TransactionManager.Account.Address);
            var _tx = await m_Web3.Eth.GetEtherTransferService().TransferEtherAsync(_toAddr.ToLower(), _amount);
            Debug.Log("sent");
            Debug.Log(_tx);
        } catch (System.Exception _e) {
            Debug.Log("An error occurred transferring eth: "+_e);
        }
    }
#endregion
}
