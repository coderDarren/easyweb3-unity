using System.Collections;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using EasyWeb3;

public class CustomContractCall : MonoBehaviour
{
    private string EasyWeb3UnitTestContract = "0x70396512216dbf32C42EB798C51a9616FdDb683a";
    
    private void Start() {
        Call();
    }

    private async void Call() {
        ERC20 _token = new ERC20(EasyWeb3UnitTestContract);

        var _out = await _token.CallFunction("getComplex(uint256,string,bool)", new string[]{"string[]","struct(uint,string,bool,address)","uint"}, new string[]{"12","letsgetstringy","1"});
        string[] _strarr = (string[])_out[0];
        BigInteger _structint = (BigInteger)_out[1];
        string _structstring = (string)_out[2];
        bool _structbool = (bool)_out[3];
        string _structaddr = (string)_out[4];
        BigInteger _int = (BigInteger)_out[5];
        Debug.Log("\t_strarr: ");
        for(int i = 0; i < _strarr.Length; i++) {
            Debug.Log("\t\t"+i+": "+_strarr[i]);
        }
        Debug.Log("\t_struct:");
        Debug.Log("\t\tint: "+_structint);
        Debug.Log("\t\tstring: "+_structstring);
        Debug.Log("\t\tbool: "+_structbool);
        Debug.Log("\t\taddress: "+_structaddr);
        Debug.Log("\t_int: "+_int);
    }
}
