using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EasyWeb3;

public class ChainViewer : MonoBehaviour
{
    public ChainId chainId;
    public Text Feed;
    public ScrollRect Scroller;
    public int ScanRate=5;

    private void Start()
    {
        StartCoroutine("Scan");
    }

    private void OnDisable() {
        StopAllCoroutines();
    }

    private IEnumerator Scan() {
        var _web3 = new Web3ify(chainId);
        while (true) {
            _web3.ScanAll((_txs, _blockNum, _isNew)=>{
                if (_isNew) {
                    Feed.text += "\nLatest block "+_blockNum+"\n\tTransactions: "+_txs.Count;
                    Scroller.verticalScrollbar.value = 0;
                    Canvas.ForceUpdateCanvases();
                }
                foreach(Nethereum.RPC.Eth.DTOs.Transaction _nethTx in _txs) {
                    Transaction _tx = new Transaction(_nethTx); // wrapper
                    var _inputHex = _tx.Data.Input; // Access all Nethereum properties through the Data property
                    // process the hex...
                }
            });
            yield return new WaitForSeconds(ScanRate);
        }
    }
}
