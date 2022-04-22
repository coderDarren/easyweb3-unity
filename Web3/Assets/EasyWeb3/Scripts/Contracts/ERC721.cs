using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EasyWeb3 {
    public class NFTData {
        public string image;
    }
    public class NFT {
        public string Uri {get; private set;}
        public NFTData Data {get; private set;}
        public NFT(string _uri, NFTData _data) {
            Uri = _uri;
            Data = _data;
        }
    }
    public class ERC721 : Contract {
        public ERC721(string _contract, ChainId _i) : base(_contract,_i) {}
        public ERC721(string _contract) : base(_contract,ChainId.ETH_ROPSTEN) {}

        public async Task<bool> Load() {
            await GetTotalSupply(); //
            await GetName(); //
            await GetSymbol(); //
            return true;
        }

        public async Task<BigInteger> GetTotalSupply() {
            var _out = await CallFunction("totalSupply()", new string[]{"uint"});
            TotalSupply = (BigInteger)_out[0];
            return TotalSupply;
        }

        public async Task<BigInteger> GetDecimals() {
            var _out = await CallFunction("decimals()", new string[]{"uint"});
            Decimals = (BigInteger)_out[0];
            return Decimals;
        }

        public async Task<string> GetName() {
            var _out = await CallFunction("name()", new string[]{"string"});
            Name = (string)_out[0];
            return Name;
        }

        public async Task<string> GetSymbol() {
            var _out = await CallFunction("symbol()", new string[]{"string"});
            Symbol = (string)_out[0];
            return Symbol;
        }

        /// <summary>
        /// Returns a count of NFTs assigned to an address
        /// </summary>
        public async Task<BigInteger> GetBalanceOf(string _owner) {
            var _out = await CallFunction("balanceOf(address)", new string[]{"uint"}, new string[]{_owner});
            return (BigInteger)_out[0];
        }

        public async Task<BigInteger> GetTokenOfOwnerByIndex(string _owner, int _index) {
            var _out = await CallFunction("tokenOfOwnerByIndex(address,uint256)", new string[]{"uint"}, new string[]{_owner,_index.ToString()});
            return (BigInteger)_out[0];
        }
        
        public async Task<string> GetToken(int _tokenId) {
            var _out = await CallFunction("tokenURI(uint256)", new string[]{"string"}, new string[]{_tokenId.ToString()});
            return (string)_out[0];
        }

        public async Task<List<NFT>> GetOwnerNFTs(string _owner) {
            List<NFT> _nfts = new List<NFT>();
            BigInteger _bal = await GetBalanceOf(_owner);
            for (int i = 0; i < _bal; i++) {
                BigInteger _token = await GetTokenOfOwnerByIndex(_owner, i);
                string _uri = await GetToken((int)_token);
                UnityEngine.Debug.Log("NFT uri: "+_uri);
                string _requrl = _uri.Contains("ipfs://") ? _uri.Replace("ipfs://","https://ipfs.io/ipfs/") : _uri;
                UnityEngine.Debug.Log("NFT request url: "+_requrl);
                string _json = await RestService.GetService().Get(_requrl);
                if (_json.Contains("Error")) {
                    UnityEngine.Debug.Log(_json);
                    continue;
                }
                UnityEngine.Debug.Log("NFT json: "+_json);
                NFTData _data = JsonConvert.DeserializeObject<NFTData>(_json);
                UnityEngine.Debug.Log("NFT image: "+_data.image);
                _nfts.Add(new NFT(_uri, _data));
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