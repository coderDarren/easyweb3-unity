/*
Tests run against contract: https://ropsten.etherscan.io/address/0x0F6F756e309C451558FCf1F7A66aEf0D553A6e86#code
*/

using System.Numerics;
using System.Collections.Generic;
using UniRx.Async;
using EasyWeb3;
using UnityEngine;

public class EasyWeb3Tests : MonoBehaviour {
    private string EasyWeb3UnitTestContract = "0x0F6F756e309C451558FCf1F7A66aEf0D553A6e86";

    private void Start() {
        Load();
    }

    private async void Load() {
        // await TestERC20Calls();
        // await TestUintCalls();
        // await TestByteCalls();
        // await TestArrayCalls();
        // await TestComplexCalls();
        await TestERC721Calls();
    }

    // NFTs
    private async UniTask<bool> TestERC721Calls() {
        try {
            
            // Debug.Log("TEST NFT (BoredApeYachtClub)");
            // await LoadOwnerNFTS("0xBC4CA0EdA7647A8aB7C2061c2E118A18a936f13D", "0xf7801B8115f3Fe46AC55f8c0Fdb5243726bdb66A");
            // Debug.Log("TEST NFT (Quantum Art)");
            // await LoadOwnerNFTS("0x46Ac8540d698167FCBb9e846511Beb8CF8af9BD8", "0x67A74108A9990bbE21582193bB99cbEd6ecfEA30");
            // Debug.Log("TEST NFT (CyberKongz VX)");
            // await LoadOwnerNFTS("0x7EA3Cca10668B8346aeC0bf1844A49e995527c8B", "0xcE2a954E8cc24EEc66A56B2f9aDE93AF7568873B");
            // Debug.Log("TEST NFT (CyberKongz VX)");
            // await LoadOwnerNFTS("0xED5AF388653567Af2F388E6224dC7C4b3241C544", "0x600235122c6299BA845E6C67f718E0854923040a");
            // ERC721 _nft = new ERC721("0xED5AF388653567Af2F388E6224dC7C4b3241C544", ChainId.ETH_MAINNET);
            // BigInteger _token = await _nft.GetTokenOfOwnerByIndex("0x600235122c6299BA845E6C67f718E0854923040a", 1);

        } catch (System.Exception _e) {
            Debug.LogWarning("[TestERC721Calls] Tests Failed: "+_e);
            return false;
        }

        return true;
    }

    private async UniTask<bool> LoadOwnerNFTS(string _nftContract, string _nftOwner) {
        ERC721 _nft = new ERC721(_nftContract, ChainId.ETH_MAINNET);
        await _nft.Load();
        Debug.Log("\tname: "+_nft.Name);
        Debug.Log("\tsymbol: "+_nft.Symbol);
        Debug.Log("\ttotal supply: "+_nft.ValueFromDecimals(_nft.TotalSupply));
        Debug.Log("Getting NFTs from an owner...");
        List<NFT> _nfts = await _nft.GetOwnerNFTs(_nftOwner,
            (_nft,_progress) => { // called when an nft is found
                Debug.Log("\t"+(_progress*100)+"% | Loaded NFT: "+_nft.Data.image);
            },
            (_nftId, _error) => { // called when an nft fails to load
                Debug.LogWarning("\tFailed to load tokenId "+_nftId+": "+_error);
            });
        return true;
    }

    // Tokens
    private async UniTask<bool> TestERC20Calls() {
        try {
            // ERC20 _token = new ERC20("0x1A2933fbA0c6e959c9A2D2c933f3f8AD4aa9f06e", ChainId.ETH_MAINNET);
            ERC20 _token = new ERC20(EasyWeb3UnitTestContract);
            // method 1
            Debug.Log("\nMethod 1 ERC20 Data Extraction");
            await _token.Load();
            Debug.Log("\tdecimals: "+_token.Decimals);
            Debug.Log("\tname: "+_token.Name);
            Debug.Log("\tsymbol: "+_token.Symbol);
            Debug.Log("\towner: "+_token.Owner);
            Debug.Log("\ttotal supply: "+_token.ValueFromDecimals(_token.TotalSupply));

            // // method 2
            Debug.Log("\nMethod 2 ERC20 Data Extraction");
            BigInteger _decimals = await _token.GetDecimals();
            BigInteger _totalSupply = await _token.GetTotalSupply();
            string _name = await _token.GetName();
            string _symbol = await _token.GetSymbol();
            string _owner = await _token.GetOwner();
            Debug.Log("\tdecimals: "+_decimals);
            Debug.Log("\tname: "+_name);
            Debug.Log("\tsymbol: "+_symbol);
            Debug.Log("\towner: "+_owner);
            Debug.Log("\ttotal supply: "+_token.ValueFromDecimals(_totalSupply));

            // // standard calls
            Debug.Log("\nStandard ERC20 Data");
            BigInteger _balance = await _token.GetBalanceOf("0x34221445c2dd9fd3f41a8a8bfa7d49ec898e0ef4");
            BigInteger _allowance = await _token.GetAllowance("0xbd0dbb9fddc73b6ebffc7c09cfae1b19d6dece40", Constants.ADDR_UNISWAPV2);
            Debug.Log("\nOther ERC20 Calls");
            Debug.Log("\tbalance: "+_token.ValueFromDecimals(_balance));
            Debug.Log("\tallowance: "+_token.ValueFromDecimals(_allowance));
        } catch (System.Exception _e) {
            Debug.LogWarning("[TestERC20Calls] Tests Failed: "+_e);
            return false;
        }

        return true;
    }

    private async UniTask<bool> TestUintCalls() {
        try {
            ERC20 _token = new ERC20(EasyWeb3UnitTestContract);

            Debug.Log("TEST: getUint256()");
            var _out = await _token.CallFunction("getUint256()", new string[]{"uint"});
            Debug.Log("\treturned: "+_out[0]);

            Debug.Log("TEST: getUint128()");
            _out = await _token.CallFunction("getUint128()", new string[]{"uint"});
            Debug.Log("\treturned: "+_out[0]);

            Debug.Log("TEST: getUint64()");
            _out = await _token.CallFunction("getUint64()", new string[]{"uint"});
            Debug.Log("\treturned: "+_out[0]);

            Debug.Log("TEST: getUint32()");
            _out = await _token.CallFunction("getUint32()", new string[]{"uint"});
            Debug.Log("\treturned: "+_out[0]);

            Debug.Log("TEST: getUint16()");
            _out = await _token.CallFunction("getUint16()", new string[]{"uint"});
            Debug.Log("\treturned: "+_out[0]);

            Debug.Log("TEST: getUint8()");
            _out = await _token.CallFunction("getUint8()", new string[]{"uint"});
            Debug.Log("\treturned: "+_out[0]);

            Debug.Log("TEST: getUint()");
            _out = await _token.CallFunction("getUint()", new string[]{"uint"});
            Debug.Log("\treturned: "+_out[0]);
        } catch (System.Exception _e) {
            Debug.LogWarning("[TestUintCalls] Tests Failed: "+_e);
            return false;
        }

        return true;
    }

    private async UniTask<bool> TestByteCalls() {
        try {
            ERC20 _token = new ERC20(EasyWeb3UnitTestContract);

            Debug.Log("TEST: getBytes()");
            var _out = await _token.CallFunction("getBytes(bytes)", new string[]{"bytes"}, new string[]{"testbytes"});
            Debug.Log("\treturned: "+_out[0]);

            Debug.Log("TEST: getBytesArr()");
            _out = await _token.CallFunction("getBytesArr(bytes[])", new string[]{"bytes[]"}, new string[]{"bytes[](testbytes1,testbytes2)"});
            string[] _bytesarr = (string[])_out[0];
            foreach(string _s in _bytesarr) {
                Debug.Log("\t"+_s);
            }

        } catch (System.Exception _e) {
            Debug.LogWarning("[TestByteCalls] Tests Failed: "+_e);
            return false;
        }

        return true;
    }

    private async UniTask<bool> TestComplexCalls() {
        try {
            ERC20 _token = new ERC20(EasyWeb3UnitTestContract);

            Debug.Log("TEST: getDataStruct()");
            var _out = await _token.CallFunction("getDataStruct()", new string[]{"struct(uint,string,bool,address)"});
            foreach(object _o in _out) {
                Debug.Log("\t"+_o);
            }

            Debug.Log("TEST: getStructMultiReturnData()");
            _out = await _token.CallFunction("getStructMultiReturnData()", new string[]{"uint","string","bool","address","struct(uint,string,bool,address)"});
            foreach(object _o in _out) {
                Debug.Log("\t"+_o);
            }

            Debug.Log("TEST: getPrimitiveMultiReturnData()");
            _out = await _token.CallFunction("getPrimitiveMultiReturnData()", new string[]{"uint","string","bool","address"});
            Debug.Log("\treturned length: "+_out.Count);
            foreach(object _o in _out) {
                Debug.Log("\t\t"+_o);
            }

            Debug.Log("TEST: getComplex(uint256,string,bool)");
            _out = await _token.CallFunction("getComplex(uint256,string,bool)", new string[]{"string[]","struct(uint,string,bool,address)","uint"}, new string[]{"12","letsgetstringy","1"});
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

            Debug.Log("TEST: getComplex2(string[],uint256[],bool[])");
            _out = await _token.CallFunction("getComplex2(string[],uint256[],bool[])", 
                new string[]{"struct(uint,string,bool,address)","uint[]","string"}, 
                new string[]{"string[](str1,str2)","uint[](1,2,3)","bool[](1,0,1,0)"});
            _structint = (BigInteger)_out[0];
            _structstring = (string)_out[1];
            _structbool = (bool)_out[2];
            _structaddr = (string)_out[3];
            BigInteger[] _intarr = (BigInteger[])_out[4];
            string _str = (string)_out[5];
            Debug.Log("\t_struct:");
            Debug.Log("\t\tint: "+_structint);
            Debug.Log("\t\tstring: "+_structstring);
            Debug.Log("\t\tbool: "+_structbool);
            Debug.Log("\t\taddress: "+_structaddr);
            Debug.Log("\t_intarr: ");
            for(int i = 0; i < _intarr.Length; i++) {
                Debug.Log("\t\t"+i+": "+_intarr[i]);
            }
            Debug.Log("\tstring: "+_str);


        } catch (System.Exception _e) {
            Debug.LogWarning("[TestComplexCalls] Tests Failed: "+_e);
            return false;
        }

        return true;
    }

    private async UniTask<bool> TestArrayCalls() {
        try{
            ERC20 _token = new ERC20(EasyWeb3UnitTestContract);

            Debug.Log("TEST: getUintArr()");
            var _out = await _token.CallFunction("getUintArr(uint256[])", new string[]{"uint[]"}, new string[]{"uint[](12415,23431,3555523)"});
            BigInteger[] _intarr = (BigInteger[])_out[0];
            foreach(BigInteger _bigint in _intarr) {
                Debug.Log("\t"+_bigint);
            }

            Debug.Log("TEST: getStrArr()");
            _out = await _token.CallFunction("getStrArr(string[])", new string[]{"string[]"}, new string[]{"string[](asfsdfsdfgsfdgsdfgdsbcasfsdfsdfgdfgga,123,ldksjflsjf;safjllfkjdsa;flkjasdflkjsfsdfdfgdfgkljlakjfal;kwjkljglkjawe,f31)"});
            string[] _strarr = (string[])_out[0];
            foreach(string _s in _strarr) {
                Debug.Log("\t"+_s);
            }

            Debug.Log("TEST: getBoolArr()");
            _out = await _token.CallFunction("getBoolArr(bool[])", new string[]{"bool[]"}, new string[]{"bool[](0,1,1)"});
            bool[] _boolarr = (bool[])_out[0];
            foreach(bool _s in _boolarr) {
                Debug.Log("\t"+_s);
            }

            Debug.Log("TEST: getAddrArr()");
            _out = await _token.CallFunction("getAddrArr(address[])", new string[]{"address[]"}, new string[]{"address[](0x4C9B1DB55c8A89cb0312C8fD84B0cB650E648618,0x1A2933fbA0c6e959c9A2D2c933f3f8AD4aa9f06e)"});
            string[] _addrarr = (string[])_out[0];
            foreach(string _s in _addrarr) {
                Debug.Log("\t"+_s);
            }
        } catch (System.Exception _e) {
            Debug.LogWarning("[TestArrayCalls] Tests Failed: "+_e);
            return false;
        }

        return true;
    }
}
/*
0000000000000000000000000000000000000000000000000000000000000020
0000000000000000000000000000000000000000000000000000000000000001
0000000000000000000000000000000000000000000000000000000000000020
0000000000000000000000000000000000000000000000000000000000000032
6162637165737472776572776572776572776572776572776566646676646676
6664676766686173646661736466686766680000000000000000000000000000
*/