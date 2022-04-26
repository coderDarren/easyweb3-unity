using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using EasyWeb3;

public class BalanceViewer : MonoBehaviour
{
    public Web3Player Player;
    public Text eth;
    public Text bnb;
    public Text matic;

    private void Start()
    {
        Load();
    }

    private async void Load() {
        double _ethBalance = await Player.GetNativeBalanceFromChain(ChainId.ETH_MAINNET);
        double _bnbBalance = await Player.GetNativeBalanceFromChain(ChainId.BSC_MAINNET);
        double _polyBalance = await Player.GetNativeBalanceFromChain(ChainId.MATIC_MAINNET);
        eth.text = "ETH: "+(string.Format("{0:0.00}", _ethBalance));
        bnb.text = "BNB: "+(string.Format("{0:0.00}", _bnbBalance));
        matic.text = "MATIC: "+(string.Format("{0:0.00}", _polyBalance));
    }
}
