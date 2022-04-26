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
        Contract _contract = new Contract(Constants.ADDR_DEAD, chainId);
        while (true) {
            _contract.ScanAll(async (_txs, _blockNum, _isNew)=>{
                if (_isNew) {
                    Feed.text += "\nLatest block "+_blockNum+"\n\tTransactions: "+_txs.Count;
                    Scroller.verticalScrollbar.value = 0;
                    Canvas.ForceUpdateCanvases();
                }
            });
            yield return new WaitForSeconds(ScanRate);
        }
    }
}
