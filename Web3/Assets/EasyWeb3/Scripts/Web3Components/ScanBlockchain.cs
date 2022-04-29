using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyWeb3;

public class ScanBlockchain : MonoBehaviour
{
    public ChainId ChainId;
    
    private void Start() {
        StartCoroutine(Scan());
    }

    private void OnDisable() {
        StopAllCoroutines();
    }

    private IEnumerator Scan() {
        Web3ify _scanner = new Web3ify(ChainId);
        while (true) {
            _scanner.ScanAll(OnBlockchainScan);
            yield return new WaitForSeconds(1);
        }
    }

    private void OnBlockchainScan(List<Nethereum.RPC.Eth.DTOs.Transaction> _transactions, BigInteger _blockNum, bool _isNew) {
        Debug.Log("Waiting on next block...");
        if (!_isNew) return;
        Debug.Log("\tNew block found ("+_blockNum+")");
        Debug.Log("\tTransactions found: "+_transactions.Count);
        foreach (var _transaction in _transactions) {
            Transaction _tx = new Transaction(_transaction);
            // //List<object> _inputValues = await _tx.GetInputs(new string[]{/*ENTER_TYPES_HERE*/}); // Decodes inputs into C# objects
            string _shortHash = (new Wallet(_tx.Data.TransactionHash)).ShortAddress;
            string _methodId = _tx.MethodId;
            Debug.Log("\t\tBlock: "+_blockNum+" | Hash: "+_shortHash+" | Method: "+_methodId+" | Value: "+_tx.Data.Value);
        }
    }
}
