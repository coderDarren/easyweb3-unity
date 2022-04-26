using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using EasyWeb3;

public class ScanViewer : MonoBehaviour
{
    public Text Feed;
    public ScrollRect Scroller;

    private ChainId m_ChainId = ChainId.ETH_MAINNET;

    private void Start() {
        StartScan();
    }

    private void OnDisable() {
        StopAllCoroutines();
    }

    private void StartScan() {
        Contract _uniswapv2 = new Contract("0x7a250d5630B4cF539739dF2C5dAcb4c659F2488D", m_ChainId);
        StartCoroutine(Scan(_uniswapv2));
    }

    private IEnumerator Scan(Contract _contract) {
        while (true) {
            _contract.Scan(async (_txs, _blockNum, _isNew)=>{
                if (_isNew) {
                    Feed.text += "\nScanning block "+_blockNum+" ("+_txs.Count+")";
                }
                ProcessTransactions(_txs);
            });
            Scroller.verticalScrollbar.value = 0;
            yield return new WaitForSeconds(3);
        }
    }

    /// <summary>
    /// TX Object: https://github.com/Nethereum/Nethereum/blob/abd05e8a3b936419f9f278bbfe57f816ee1182b0/src/Nethereum.RPC/Eth/DTOs/Transaction.cs
    /// </summary>
    private async void ProcessTransactions(List<Nethereum.RPC.Eth.DTOs.Transaction> _txs) {
        foreach (var _tx in _txs) {
            bool _success = await ProcessTx(new Transaction(_tx));
        }
    }

    private async Task<bool> ProcessTx(Transaction _tx) {
        try {
            // These are the transactions we want to track
            string[] _trackedMethods = new string[]{
                "swapExactTokensForTokens(uint256,uint256,address[],address,uint256)",
                "swapExactTokensForETHSupportingFeeOnTransferTokens(uint256,uint256,address[],address,uint256)",
                "swapExactTokensForTokensSupportingFeeOnTransferTokens(uint256,uint256,address[],address,uint256)",
                "swapTokensForExactTokens(uint256,uint256,address[],address,uint256)",
                "swapExactTokensForETH(uint256,uint256,address[],address,uint256)",
                "swapTokensForExactETH(uint256,uint256,address[],address,uint256)",
                "swapExactETHForTokens(uint256,address[],address,uint256)",
                "swapExactETHForTokensSupportingFeeOnTransferTokens(uint256,address[],address,uint256)",
                "swapETHForExactTokens(uint256,address[],address,uint256)",
                "addLiquidityETH(address,uint256,uint256,uint256,address,uint256)",
                "removeLiquidityETHWithPermit(address,uint256,uint256,uint256,address,uint256,bool,uint8,bytes32,bytes32)"
            };

            for (int i = 0; i < _trackedMethods.Length; i++) {
                if (Web3Utils.FunctionHash(_trackedMethods[i]) == _tx.MethodId) {
                    List<object> _inputs = null;
                    string[] _path = null;
                    string _symbol = null;
                    string _token = null;
                    switch (i) {
                        case 0: //swapExactTokensForTokens
                        case 1: //swapExactTokensForETHSupportingFeeOnTransferTokens
                        case 2: //swapExactTokensForTokensSupportingFeeOnTransferTokens
                        case 3: //swapTokensForExactTokens
                        case 4: //swapExactTokensForETH
                        case 5: //swapTokensForExactETH
                            _inputs = _tx.GetInputs(new string[]{"uint","uint","address[]","address","uint"});
                            _path = (string[])_inputs[2]; // path (address[]) is the 3rd input
                            Feed.text += "\n\tNew swap between ";
                            foreach (string _addr in _path) {
                                _symbol = await (new ERC20(_addr, m_ChainId)).GetSymbol();
                                Feed.text += _symbol + ", ";
                            }
                            Feed.text = Feed.text.Substring(0,Feed.text.Length-2);
                            break;
                        case 6: //swapExactETHForTokens
                        case 7: //swapETHForExactTokens
                        case 8: //swapExactETHForTokensSupportingFeeOnTransferTokens
                            _inputs = _tx.GetInputs(new string[]{"uint","address[]","address","uint"});
                            _path = (string[])_inputs[1]; // path (address[]) is the 2nd input
                            Feed.text += "\n\tNew swap between ";
                            foreach (string _addr in _path) {
                                _symbol = await (new ERC20(_addr, m_ChainId)).GetSymbol();
                                Feed.text += _symbol + ", ";
                            }
                            Feed.text = Feed.text.Substring(0,Feed.text.Length-2);
                            break;
                        case 9: //addLiquidityETH
                            _inputs = _tx.GetInputs(new string[]{"address","uint","uint","uint","address","uint"});
                            _token = (string)_inputs[0]; // token (address) is the 1st input
                            _symbol = await (new ERC20(_token, m_ChainId)).GetName();
                            Feed.text += "\n\tLiquidity was just added to "+_symbol;
                            break;
                        case 10: //removeLiquidityETHWithPermit
                            _inputs = _tx.GetInputs(new string[]{"address","uint","uint","uint","address","uint","bool","uint","bytes","bytes"});
                            _token = (string)_inputs[0]; // token (address) is the 1st input
                            _symbol = await (new ERC20(_token, m_ChainId)).GetName();
                            Feed.text += "\n\tLiquidity was just removed from "+_symbol;
                            break;
                    }
                    Scroller.verticalScrollbar.value = 0;
                }
            }
            return true;
        } catch (System.Exception) {
            return false;
        }
    }
}
/*
0000000000000000000000002f4697762178f511eaf2517e6dc47ccf6b30faa9
0000000000000000000000000000000000000000000000f26f5a94ae2a3e374a
0000000000000000000000000000000000000000002681589ba33dc6b1c02c3e
0000000000000000000000000000000000000000000000000561aa6d50e19f3d
000000000000000000000000a9405498a68c1e16ce96834b3bbc7f1a2939a4c0
0000000000000000000000000000000000000000000000000000000062670480
0000000000000000000000000000000000000000000000000000000000000000
000000000000000000000000000000000000000000000000000000000000001b
3c689d9fc81d04f8d363783616bcc31c5a75e78ee653497dd3c44ffdd4f1337b
3ecd99b88a3ee69e4fd181cb071ccf7db12f457579a231134d616dd72cefc973
*/