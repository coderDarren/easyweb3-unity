using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx.Async;
using EasyWeb3;

public class ScanViewer : MonoBehaviour
{
    public Text Feed;
    public ScrollRect Scroller;

    private void Start() {
        StartScan();
        Scroller.velocity=new Vector2(0f,1000f);
    }

    private void Update() {
        Scroller.verticalScrollbar.value = 0;
    }

    private void StartScan() {
        Contract _uniswapv2 = new Contract("0x7a250d5630B4cF539739dF2C5dAcb4c659F2488D", ChainId.ETH_MAINNET);
        StartCoroutine(Scan(_uniswapv2));
    }

    private IEnumerator Scan(Contract _contract) {
        while (true) {
            _contract.Scan(async (_txs, _blockNum, _isNew)=>{
                if (_isNew) {
                    Feed.text += "Scanning block "+_blockNum+"\n";
                }
                foreach (var _tx in _txs) {
                    ProcessTx(new Transaction(_tx));
                }
            });
            yield return new WaitForSeconds(3);
        }
    }

    private void ProcessTx(Transaction _tx) {
        string _targetMethod = Web3Utils.FunctionHash("swapExactTokensForTokens(uint256,uint256,address[],address,uint256)");
        Feed.text += "\tMethod Id: "+_tx.MethodId+"\n";
        Feed.text += "\tTarget Method "+_targetMethod+"\n";
        if (_tx.MethodId == _targetMethod) {
            List<object> _inputs = _tx.GetInputs(new string[]{"uint","uint","address[]","address","uint"});
            string[] _path = (string[])_inputs[2]; // path (address[]) is the 3rd input
            Feed.text += "\tNew swap between ";
            foreach (string _addr in _path) {
                Feed.text += new Wallet(_addr).ShortAddress + ", ";
            }
            Feed.text = Feed.text.Substring(0,Feed.text.Length-2) + "\n";
        }
    }
}
