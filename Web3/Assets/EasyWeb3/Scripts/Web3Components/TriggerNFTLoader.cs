using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using UniRx.Async;
using EasyWeb3;

public class TriggerNFTLoader : MonoBehaviour {
    public ChainId Web3Chain;
    public string NFTContract;
    public RawImage Img;
    public VideoPlayer Video;

    [Header("Project Details")]
    public Text Name;
    public Text Symbol;
    public Text Supply;

    [Header("NFT Details")]
    public Text Title;
    public Text Progress;
    public Text Description;
    public Text Attributes;

    private ERC721 m_NftContract;
    private bool m_DidLoadContract;
    private bool m_DidLoadNFTs;
    private bool m_IsLoading;
    private List<NFT> m_NFTs;
    private int m_NFTCycleIndex;

    private void Start() {
        m_NftContract = new ERC721(NFTContract, Web3Chain);
        m_NFTs = new List<NFT>();
        StartCoroutine("CycleTextures");
        Load();
    }

    private void OnApplicationQuit() {
        StopAllCoroutines();
    }

    private void OnDisable() {
        StopAllCoroutines();
    }

    private void OnTriggerEnter(Collider _col) {
        Web3Player _player = _col.gameObject.GetComponent<Web3Player>();
        if (_player != null) {
            if (m_DidLoadNFTs || m_IsLoading) return;
            StartCoroutine(LoadAllNFTs(_player));
        }
    }

    private void DrawNFTData(NFT _nft) {
        if (_nft.AssetUrl.Contains("mp4")) {
            Img.gameObject.SetActive(false);
            Video.gameObject.SetActive(true);
            Video.url = _nft.Data.image;
            Video.Play();
        } else if (_nft.AssetUrl.ToLower().Contains(".png") || _nft.AssetUrl.ToLower().Contains(".jpg") || _nft.AssetUrl.ToLower().Contains(".jpeg") || _nft.AssetUrl.ToLower().Contains("https://ipfs.io/ipfs/")) {
            Img.gameObject.SetActive(true);
            Video.gameObject.SetActive(false);
            Img.texture = _nft.texture;
        } else {
            Debug.LogWarning("Unsupported NFT url: "+_nft.AssetUrl+". Contact support if you need help supporting this.");
        }
        Title.text = _nft.Data.name != null ? _nft.Data.name : 
                    _nft.Data.title != null ? _nft.Data.title : "No Title Available";
        Description.text = _nft.Data.description != null ? _nft.Data.description : "No Description Available";
        string _attributes = "Attributes:\n";
        if (_nft.Data.attributes != null) {
            foreach(NFTAttribute _attr in _nft.Data.attributes) {
                _attributes += _attr.trait_type+": "+_attr.value+"\n";
            }
            Attributes.text = _attributes;
        } else {
            Attributes.text = "No Attribute Data Available";
        }
    }

    private async void Load() {
        if (!m_DidLoadContract) {
            Debug.Log("[TriggerNFTLoader] Loading NFT Contract...");
            await m_NftContract.Load();
            Name.text = m_NftContract.Name;
            Symbol.text = m_NftContract.Symbol;
            Supply.text = "Supply: "+m_NftContract.ValueFromDecimals(m_NftContract.TotalSupply).ToString();
            m_DidLoadContract = true;
        }
    }

    private async UniTask<bool> LoadOwnerNFTS(string _nftContract, string _nftOwner) {
        ERC721 _nft = new ERC721(_nftContract, ChainId.ETH_MAINNET);
        try {
            m_IsLoading = true;
            m_DidLoadNFTs = true;
            Debug.Log("[TriggerNFTLoader] Getting NFTs from owner...");
            List<NFT> _nfts = await _nft.GetOwnerNFTs(_nftOwner,
                (_nft,_progress) => { // called when an nft is found
                    Progress.text = "Loaded "+((int)(_progress*100))+"%";
                    StartCoroutine(LoadTexture(_nft,_progress));
                },
                (_nftId, _error) => { // called when an nft fails to load
                    Debug.LogWarning("\t[TriggerNFTLoader] Failed to load tokenId "+_nftId+": "+_error);
                });
        } catch (System.Exception) {
            m_IsLoading = false;
            m_DidLoadNFTs = false;
        }
        return true;
    }

    private IEnumerator LoadAllNFTs(Web3Player _player) {
        yield return null;
        LoadOwnerNFTS(NFTContract, _player.ethAddress);
    }

    private IEnumerator CycleTextures() {
        while (true) {
            if (m_NFTs.Count == 0) {}
            else {
                m_NFTCycleIndex++;
                m_NFTCycleIndex = m_NFTCycleIndex > m_NFTs.Count - 1 ? 0 : m_NFTCycleIndex;
                NFT _nft = m_NFTs[m_NFTCycleIndex];
                DrawNFTData(_nft);
            }
            yield return new WaitForSeconds(3);
        }
    }

    private IEnumerator LoadTexture(NFT _nft, float _progress) {
        Debug.Log("\t[TriggerNFTLoader] "+(_progress*100)+"% | Loaded NFT: "+_nft.Data.image);
        string _url = _nft.Data.image.Contains("ipfs://") ? _nft.Data.image.Replace("ipfs://","https://ipfs.io/ipfs/") : _nft.Data.image;
        _nft.AssetUrl = _url;
        if (_url.Contains("mp4")) {
        } else {
            UnityWebRequest _req = UnityWebRequestTexture.GetTexture(_url);
            yield return _req.SendWebRequest();
            Texture2D _tex = new Texture2D (1, 1);
            _tex = ((DownloadHandlerTexture)_req.downloadHandler).texture;
            _nft.texture = _tex;
            m_NFTs.Add(_nft);
            if (m_NFTs.Count == 1) {
                DrawNFTData(_nft);
            }
        }
    }
}
//0x7DD04448c6CD405345D03529Bff9749fd89F8F4F crypto pills
//0x7AB2352b1D2e185560494D5e577F9D3c238b78C5 adam bomb squad