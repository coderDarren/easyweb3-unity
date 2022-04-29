using System;
using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EasyWeb3
{
    public delegate void NFTSuccessDelegate(NFT _nft, float _progress);
    public delegate void NFTFailDelegate(int _tokenId, string _err);
    public class NFTAttribute
    {
        public string trait_type;
        public string value;
    }
    public class NFTData
    {
        public string title;
        public string image;
        public string name;
        public string description;
        public NFTAttribute[] attributes;
    }
    public class NFT
    {
        public int Id { get; private set; }
        public string Uri { get; private set; }
        public NFTData Data { get; private set; }
        public NFT(int _id, string _uri, NFTData _data)
        {
            Id = _id;
            Uri = _uri;
            Data = _data;
        }
    }
    public class ERC721 : Contract
    {
        private bool m_ApplicationIsRunning=true;

        public ERC721(string _contract, ChainId _i) : base(_contract, _i) { }
        public ERC721(string _contract) : base(_contract, ChainId.ETH_ROPSTEN) { }

        public async Task<bool> Load()
        {
            await GetTotalSupply(); //
            await GetName(); //
            await GetSymbol(); //
            return true;
        }

        public async Task<BigInteger> GetTotalSupply()
        {
            var _out = await CallFunction("totalSupply()", new string[] { "uint" });
            TotalSupply = (BigInteger)_out[0];
            return TotalSupply;
        }

        public async Task<string> GetName()
        {
            var _out = await CallFunction("name()", new string[] { "string" });
            Name = (string)_out[0];
            return Name;
        }

        public async Task<string> GetSymbol()
        {
            var _out = await CallFunction("symbol()", new string[] { "string" });
            Symbol = (string)_out[0];
            return Symbol;
        }

        /// <summary>
        /// Returns a count of NFTs assigned to an address
        /// </summary>
        public async Task<BigInteger> GetBalanceOf(string _owner)
        {
            var _out = await CallFunction("balanceOf(address)", new string[] { "uint" }, new string[] { _owner });
            return (BigInteger)_out[0];
        }

        public async Task<BigInteger> GetTokenOfOwnerByIndex(string _owner, int _index)
        {
            var _out = await CallFunction("tokenOfOwnerByIndex(address,uint256)", new string[] { "uint" }, new string[] { _owner, _index.ToString() });
            return (BigInteger)_out[0];
        }

        /// <summary>
        /// Returns URI to access NFT metadata for _tokenId
        /// </summary>
        public async Task<string> GetToken(int _tokenId)
        {
            var _out = await CallFunction("tokenURI(uint256)", new string[] { "string" }, new string[] { _tokenId.ToString() });
            return (string)_out[0];
        }

        public async Task<string> GetOwnerOf(int _tokenId)
        {
            var _out = await CallFunction("ownerOf(uint256)", new string[] { "string" }, new string[] { _tokenId.ToString() });
            return (string)_out[0];
        }

        /// <summary>
        /// Return who can sell _tokenId
        /// </summary>
        public async Task<string> GetApproved(int _tokenId)
        {
            var _out = await CallFunction("getApproved(uint256)", new string[] { "string" }, new string[] { _tokenId.ToString() });
            return (string)_out[0];
        }

        /// <summary>
        /// Return true if _operator can sell all tokenIds of _owner
        /// </summary>
        public async Task<bool> IsApprovedForAll(string _owner, string _operator)
        {
            var _out = await CallFunction("isApprovedForAll(address,address)", new string[] { "bool" }, new string[] { _owner, _operator });
            return (bool)_out[0];
        }

        public async Task<List<NFT>> GetOwnerNFTs(string _owner, NFTSuccessDelegate _onProgress = null, NFTFailDelegate _onFail = null)
        {
            List<NFT> _nfts = new List<NFT>();
            BigInteger _bal = await GetBalanceOf(_owner);
            for (int i = 0; i < _bal; i++)
            {
                if (!m_ApplicationIsRunning) break;
                try
                {
                    BigInteger _token = await GetTokenOfOwnerByIndex(_owner, i);
                    string _uri = await GetToken((int)_token);
                    string _requrl = _uri.Contains("ipfs://") ? _uri.Replace("ipfs://", "https://ipfs.io/ipfs/") : _uri;
                    string _json = await RestService.GetService().Get(_requrl);
                    NFTData _data = JsonConvert.DeserializeObject<NFTData>(_json);
                    NFT _nft = new NFT((int)_token, _uri, _data);
                    _nfts.Add(_nft);
                    if (_onProgress != null)
                    {
                        _onProgress(_nft, (i + 1) / (float)_bal);
                    }
                }
                catch (System.Exception _e)
                {
                    if (_onFail != null)
                    {
                        _onFail(i, "Unable to load owner nft: " + _e);
                    }
                }
            }
            return _nfts;
        }

        public void Dispose()
        {
            m_ApplicationIsRunning = false;
        }
    }
}
