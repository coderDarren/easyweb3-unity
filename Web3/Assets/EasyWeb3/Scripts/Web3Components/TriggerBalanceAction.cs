using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using EasyWeb3;

public class TriggerBalanceAction : MonoBehaviour
{
    public string Token;
    public ChainId ChainId;
    public int BalanceRequirement;
    public UnityEvent OnRequirementMet;

    private void OnTriggerEnter(Collider _col) {
        Web3Player _player = _col.gameObject.GetComponent<Web3Player>();
        if (_player != null) {
            CheckBalance(_player);
        }
    }

    private async void CheckBalance(Web3Player _player) {
        string _addr = ChainId == ChainId.BSC_MAINNET ? _player.bscAddress : ChainId == ChainId.MATIC_MAINNET ? _player.maticAddress : _player.ethAddress;
        ERC20 _token = new ERC20(Token,ChainId);
        BigInteger _bal = await _token.GetBalanceOf(_addr);
        if (_bal >= BalanceRequirement) {
            OnRequirementMet.Invoke();
        }
    }
}
