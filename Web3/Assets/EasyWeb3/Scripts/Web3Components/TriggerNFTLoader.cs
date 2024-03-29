using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Threading.Tasks;
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
    private Hashtable m_NFTTextures;

    private void Start() {
        m_NftContract = new ERC721(NFTContract, Web3Chain);
        m_NFTs = new List<NFT>();
        StartCoroutine("CycleTextures");
        Load();
    }

    private void OnDisable() {
        m_NftContract.Dispose(); // !important, Stops any asynchronous loading
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
        string _url = _nft.Data.image.Contains("ipfs://") ? _nft.Data.image.Replace("ipfs://","https://ipfs.io/ipfs/") : _nft.Data.image;
        if (_url.Contains("mp4")) {
            Img.gameObject.SetActive(false);
            Video.gameObject.SetActive(true);
            Video.url = _nft.Data.image;
            Video.Play();
        } else if (_url.ToLower().Contains(".png") || _url.ToLower().Contains(".jpg") || _url.ToLower().Contains(".jpeg") || _url.ToLower().Contains("https://ipfs.io/ipfs/")) {
            Img.gameObject.SetActive(true);
            Video.gameObject.SetActive(false);
            Img.texture = (Texture2D)m_NFTTextures[_nft.Id];
        } else {
            Debug.LogWarning("Unsupported NFT url: "+_url+". Contact support if you need help supporting this.");
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
            m_NFTTextures = new Hashtable();
        }
    }

    private async Task<bool> LoadOwnerNFTS(string _nftContract, string _nftOwner) {
        try {
            m_IsLoading = true;
            m_DidLoadNFTs = true;
            Debug.Log("[TriggerNFTLoader] Getting NFTs from owner...");
            List<NFT> _nfts = await m_NftContract.GetOwnerNFTs(_nftOwner,
                (_nft,_progress) => { // called when an nft is found
                    Progress.text = "Loaded "+((int)(_progress*100))+"%";
                    LoadTexture(_nft);
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

    private async Task<bool> LoadTexture(NFT _nft) {
        Debug.Log("\t[TriggerNFTLoader] Loaded NFT: "+_nft.Data.image);
        string _url = _nft.Data.image.Contains("ipfs://") ? _nft.Data.image.Replace("ipfs://","https://ipfs.io/ipfs/") : _nft.Data.image;
        if (_url.Contains("mp4")) {
        } else {
            UnityWebRequest _req = UnityWebRequestTexture.GetTexture(_url);
            await _req.SendWebRequest();
            Texture2D _tex = ((DownloadHandlerTexture)_req.downloadHandler).texture;
            m_NFTTextures.Add(_nft.Id, _tex);
            m_NFTs.Add(_nft);
            if (m_NFTs.Count == 1) {
                DrawNFTData(_nft);
            }
        }
        return true;
    }

    private IEnumerator LoadAllNFTs(Web3Player _player) {
        yield return null;
        LoadOwnerNFTS(NFTContract, _player.GetAddressFromChain(Web3Chain));
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
}