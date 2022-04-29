using System.Collections.Generic;

namespace EasyWeb3
{
    public class Transaction
    {
        private Nethereum.RPC.Eth.DTOs.Transaction m_Tx;

        public string MethodId
        {
            get
            {
                if (m_Tx.Input.Length >= 10)
                    return m_Tx.Input.Substring(0, 10);
                else
                    return "0x";
            }
        }

        public Nethereum.RPC.Eth.DTOs.Transaction Data
        {
            get { return m_Tx; }
        }

        /// <summary>
        /// TX Object: https://github.com/Nethereum/Nethereum/blob/abd05e8a3b936419f9f278bbfe57f816ee1182b0/src/Nethereum.RPC/Eth/DTOs/Transaction.cs
        /// </summary>
        public Transaction(Nethereum.RPC.Eth.DTOs.Transaction _tx)
        {
            m_Tx = _tx;
        }

        public List<object> GetInputs(string[] _types)
        {
            int _l = 0;
            List<object> _ret = new Decoder().Decode(m_Tx.Input.Substring(10), _types, ref _l);
            return _ret;
        }
    }
}