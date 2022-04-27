using System.Collections;
using System.Numerics;
using UnityEngine.Events;
using UnityEngine;
using EasyWeb3;

public class TransactionProgressTracker : MonoBehaviour {
    public string Hash;
    public ChainId chainId;
    public UnityEvent OnSuccess;
    public UnityEvent OnFail;

    private Web3ify m_Web3;
    private bool m_IsComplete;

    private void Start() {
        m_Web3 = new Web3ify(chainId);
        StartCoroutine(PollReceipt());
    }

    private void OnDisable() {
        StopAllCoroutines();
    }

    /// <summary>
    /// Nethereum TransactionReceipt reference:
    /// https://github.com/Nethereum/Nethereum/blob/abd05e8a3b936419f9f278bbfe57f816ee1182b0/src/Nethereum.RPC/Eth/DTOs/TransactionReceipt.cs
    /// </summary>
    private IEnumerator PollReceipt() {
        while (!m_IsComplete) {
            CheckReceipt();
            Debug.Log("[TransactionProgressTracker] Processing");
            yield return new WaitForSeconds(1);
        }
        Debug.Log("[TransactionProgressTracker] Complete");
    }

    private async void CheckReceipt() {
        var _receipt = await m_Web3.GetTransactionReceipt(Hash);
        if (_receipt == null) {
            Debug.Log("[TransactionProgressTracker] Pending");
            return;
        }
        BigInteger _status = (BigInteger)_receipt.Status;
        if (_status == 0) {
            Debug.Log("[TransactionProgressTracker] Failed");
            if (OnFail != null) {
                OnFail.Invoke();
            }
        } else {
            Debug.Log("[TransactionProgressTracker] Success");
            if (OnSuccess != null) {
                OnSuccess.Invoke();
            }
        }
        m_IsComplete = true;
    }
}