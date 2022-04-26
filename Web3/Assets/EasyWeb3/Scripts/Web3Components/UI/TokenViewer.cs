using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EasyWeb3;

public class TokenViewer : MonoBehaviour
{
    public Text Feed;
    public InputField TokenField;
    public Web3Player Player;

    private void Start() {
        Submit_LoadTokenDetail();
    }
    
    public async void Submit_LoadTokenDetail() {
        ERC20 _token = new ERC20(TokenField.text, ChainId.ETH_MAINNET);
        await _token.Load();
        Feed.text = "Name: "+_token.Name+"\nSymbol: "+_token.Symbol+"\nSupply: "+_token.ValueFromDecimals(_token.TotalSupply)+"\nOwner: "+_token.Owner;
        BigInteger _bal = await _token.GetBalanceOf(Player.ethAddress);
        Feed.text += "\nPlayer Balance: "+_token.ValueFromDecimals(_bal);
    }
}
