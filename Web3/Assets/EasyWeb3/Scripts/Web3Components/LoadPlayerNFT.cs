using System.Numerics;
using UnityEngine;
using EasyWeb3;

public class LoadPlayerNFT : MonoBehaviour
{
    public ChainId ChainId;
    public string NFTAddress;
    public string HolderAddress;

    private ERC721 m_ERC721;

    private void Start() {
        GetNFTs();
    }

    private void GetNFTs() {
        m_ERC721 = new ERC721(NFTAddress, ChainId);
        m_ERC721.GetOwnerNFTs(HolderAddress, OnNFTLoaded, OnNFTFailedToLoad);
        Debug.Log("Getting NFTs for player "+HolderAddress);
    }

    private void OnNFTLoaded(NFT _nft, float _progress) {
        Debug.Log("("+(_progress*100)+"%) Loaded player NFT: "+_nft.Data.image);
    }

    private void OnNFTFailedToLoad(int _index, string _error) {
        Debug.LogWarning("Failed to load player NFT ["+_index+"]: "+_error);
    }
}
