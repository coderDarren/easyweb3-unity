using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;

namespace EasyWeb3 {
    public class EasyWeb3Controller : MonoBehaviour
    {
    #region Unity Standard Functions
        private void Start() {
            Run();
        }
    #endregion

    #region Public UI Accessor Functions

    #endregion

    #region Private Functions
        
        private async void Run() {
            ERC20 _token = new ERC20("0x1A2933fbA0c6e959c9A2D2c933f3f8AD4aa9f06e", ChainId.ETH_MAINNET);
            var _data = await _token.CallFunction("limitsInEffect()", new string[]{"bool"});
            Debug.Log("limitsInEffect: "+(bool)_data[0]);

            _data = await _token.CallFunction("totalFees()", new string[]{"uint"});
            Debug.Log("totalFees: "+(BigInteger)_data[0]);

            _data = await _token.CallFunction("tradingActive()", new string[]{"bool"});
            Debug.Log("tradingActive: "+(bool)_data[0]);

            _data = await _token.CallFunction("transferDelayEnabled()", new string[]{"bool"});
            Debug.Log("transferDelayEnabled: "+(bool)_data[0]);
        }

    #endregion
    }
}