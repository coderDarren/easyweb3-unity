
using System.Text;
using Nethereum.Web3;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using Org.BouncyCastle.Crypto.Digests;
using UnityEngine;

namespace EasyWeb3 {
    public enum ChainName {
        ETH,
        BSC
    }
    public enum ChainId {
        ETH_MAINNET,
        ETH_ROPSTEN
    }
    public class Contract : Web3ify {
        protected string m_Contract;
        public Contract(string _contract, ChainName _n, ChainId _i) : base(_n,_i) {
            m_Contract = _contract;
        }
        private byte[] CalculateHash(byte[] value)
        {
            var digest = new KeccakDigest(256);
            var output = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(value, 0, value.Length);
            digest.DoFinal(output, 0);
            return output;
        }

        protected string FunctionHash(string _func) {
            var _digest = new KeccakDigest(256);
            byte[] _bytes = Encoding.ASCII.GetBytes("totalSupply()");
            _bytes = CalculateHash(_bytes);
            byte[] _newBytes = new byte[4];
            for (int i = 0; i < 4; i++) {
                _newBytes[i] = _bytes[i];
            }
            return "0x"+HexByteConvertorExtensions.ToHex(_newBytes);
        }
    }
    public class Web3ify {
        protected Web3 m_Web3;
        protected ChainName m_ChainName;
        protected ChainId m_ChainId;
        public ChainName chainName {
            get { return m_ChainName; }
            set {
                m_ChainName = value;
                m_Web3 = new Web3(GetNodeURL(m_ChainName, m_ChainId));
            }
        }
        public ChainId chainId {
            get { return m_ChainId; }
            set {
                m_ChainId = value;
                m_Web3 = new Web3(GetNodeURL(m_ChainName, m_ChainId));
            }
        }
        public Web3ify(ChainName _n, ChainId _i) {
            m_ChainName = _n;
            m_ChainId = _i;
            m_Web3 = new Web3(GetNodeURL(m_ChainName, m_ChainId));
        }
        private string GetNodeURL(ChainName _chainName, ChainId _chainId) {
            switch (_chainName) {
                case ChainName.ETH:
                    switch (_chainId) {
                        case ChainId.ETH_ROPSTEN: // ropsten
                            return Constants.NODEDEFAULT_ETH_ROPSTEN;
                        case ChainId.ETH_MAINNET: // mainnet
                            return Constants.NODEDEFAULT_ETH_MAINNET;
                    }
                    break;
            }
            return Constants.NODEDEFAULT_ETH_ROPSTEN;
        }
        protected void Log(string _msg) {
            if (!Constants.debug) return;
            Debug.Log(_msg);
        }
        protected void LogWarning(string _msg) {
            if (!Constants.debug) return;
            Debug.LogWarning(_msg);
        }
        protected void LogError(string _msg) {
            if (!Constants.debug) return;
            Debug.LogError(_msg);
        }
    }
}