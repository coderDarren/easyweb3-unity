using System.Numerics;
using UnityEngine;
using EasyWeb3;

public class BalanceOf : MonoBehaviour
{
    public ChainId ChainId;
    public string Token;
    public string HolderAddress;

    private ERC20 m_ERC20;

    private void Start() {
        GetBalance();
    }

    private async void GetBalance() {
        m_ERC20 = new ERC20(Token, ChainId);
        BigInteger _bal = await m_ERC20.GetBalanceOf(HolderAddress);
        Debug.Log("Balance of "+HolderAddress+": "+_bal);
    }
}
