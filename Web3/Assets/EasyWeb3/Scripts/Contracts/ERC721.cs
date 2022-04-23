using System;
using System.Numerics;
using System.Collections.Generic;
using UniRx.Async;
using Newtonsoft.Json;

namespace EasyWeb3 {
    public delegate void NFTSuccessDelegate(NFT _nft, float _progress);
    public delegate void NFTFailDelegate(int _tokenId, string _err);
    public class NFTAttribute {
        public string trait_type;
        public string value;
    }
    public class NFTData {
        public string title;
        public string image;
        public string name;
        public string description;
        public NFTAttribute[] attributes;
    }
    public class NFT {
        public string Uri {get; private set;}
        public NFTData Data {get; private set;}
        public UnityEngine.Texture2D texture;
        public NFT(string _uri, NFTData _data) {
            Uri = _uri;
            Data = _data;
        }
    }
    public class ERC721 : Contract {
        public ERC721(string _contract, ChainId _i) : base(_contract,_i) {}
        public ERC721(string _contract) : base(_contract,ChainId.ETH_ROPSTEN) {}

        public async UniTask<bool> Load() {
            await GetTotalSupply(); //
            await GetName(); //
            await GetSymbol(); //
            return true;
        }

        public async UniTask<BigInteger> GetTotalSupply() {
            var _out = await CallFunction("totalSupply()", new string[]{"uint"});
            TotalSupply = (BigInteger)_out[0];
            return TotalSupply;
        }

        public async UniTask<BigInteger> GetDecimals() {
            var _out = await CallFunction("decimals()", new string[]{"uint"});
            Decimals = (BigInteger)_out[0];
            return Decimals;
        }

        public async UniTask<string> GetName() {
            var _out = await CallFunction("name()", new string[]{"string"});
            Name = (string)_out[0];
            return Name;
        }

        public async UniTask<string> GetSymbol() {
            var _out = await CallFunction("symbol()", new string[]{"string"});
            Symbol = (string)_out[0];
            return Symbol;
        }

        /// <summary>
        /// Returns a count of NFTs assigned to an address
        /// </summary>
        public async UniTask<BigInteger> GetBalanceOf(string _owner) {
            var _out = await CallFunction("balanceOf(address)", new string[]{"uint"}, new string[]{_owner});
            return (BigInteger)_out[0];
        }

        public async UniTask<BigInteger> GetTokenOfOwnerByIndex(string _owner, int _index) {
            var _out = await CallFunction("tokenOfOwnerByIndex(address,uint256)", new string[]{"uint"}, new string[]{_owner,_index.ToString()});
            return (BigInteger)_out[0];
        }
        
        public async UniTask<string> GetToken(int _tokenId) {
            var _out = await CallFunction("tokenURI(uint256)", new string[]{"string"}, new string[]{_tokenId.ToString()});
            return (string)_out[0];
        }

        public async UniTask<List<NFT>> GetOwnerNFTs(string _owner, NFTSuccessDelegate _onProgress=null, NFTFailDelegate _onFail=null) {
            List<NFT> _nfts = new List<NFT>();
            BigInteger _bal = await GetBalanceOf(_owner);
            for (int i = 0; i < _bal; i++) {
                if (!UnityEngine.Application.isPlaying) break;
                try {
                    BigInteger _token = await GetTokenOfOwnerByIndex(_owner, i);
                    string _uri = await GetToken((int)_token);
                    string _requrl = _uri.Contains("ipfs://") ? _uri.Replace("ipfs://","https://ipfs.io/ipfs/") : _uri;
                    string _json = await RestService.GetService().Get(_requrl);
                    UnityEngine.Debug.Log(_requrl);
                    NFTData _data = JsonConvert.DeserializeObject<NFTData>(_json);
                    NFT _nft = new NFT(_uri, _data);
                    _nfts.Add(_nft);
                    if (_onProgress != null) {
                        _onProgress(_nft, (i+1)/(float)_bal);
                    }
                } catch (System.Exception _e) {
                    if (_onFail != null) {
                        _onFail(i, "Unable to load owner nft: "+_e);
                    }
                }
            }
            return _nfts;
        }

        // function tokenByIndex(uint256 _index) external view returns (uint256);
        // function tokenOfOwnerByIndex(address _owner, uint256 _index) external view returns (uint256);
        // function tokenURI(uint256 _tokenId) external view returns (string);
        // function ownerOf(uint256 _tokenId) external view returns (address);
        // function getApproved(uint256 _tokenId) external view returns (address); (return who can sell this tokenId)
        // function isApprovedForAll(address _owner, address _operator) external view returns (bool); (can _operator sell all tokenIds)

        /* NFT Metadata
        {
            "title": "Asset Metadata",
            "type": "object",
            "properties": {
                "name": {
                    "type": "string",
                    "description": "Identifies the asset to which this NFT represents"
                },
                "description": {
                    "type": "string",
                    "description": "Describes the asset to which this NFT represents"
                },
                "image": {
                    "type": "string",
                    "description": "A URI pointing to a resource with mime type image/* representing the asset to which this NFT represents. Consider making any images at a width between 320 and 1080 pixels and aspect ratio between 1.91:1 and 4:5 inclusive."
                }
            }
        }
        */
    }
}
/*
000000000000000000000000600235122c6299BA845E6C67f718E0854923040a
0000000000000000000000000000000000000000000000000000000000000001
*/