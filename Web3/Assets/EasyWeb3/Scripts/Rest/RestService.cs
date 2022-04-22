using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace EasyWeb3 {
    public class RestService 
    {
        private static RestService m_Service;
        private static HttpClient m_Http;

        public static RestService GetService() {
            if (m_Service == null) {
                m_Service = new RestService();
                m_Http = new HttpClient();
                ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
            }

            return m_Service;
        }

        public async Task<string> Post(string _url, string _body) {
            try {
                var _content = new StringContent(_body, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage _response = await m_Http.PostAsync(_url, _content);
                _response.EnsureSuccessStatusCode();
                string _responseBody = await _response.Content.ReadAsStringAsync();
                return _responseBody;
            } catch (HttpRequestException _err) {
                return "Error: "+_err;
            }
        }

        public async Task<string> Get(string _url) {
            try {
                HttpResponseMessage _response = await m_Http.GetAsync(_url);
                _response.EnsureSuccessStatusCode();
                string _responseBody = await _response.Content.ReadAsStringAsync();
                return _responseBody;
            } catch (HttpRequestException _err) {
                return "Error: "+_err;
            }
        }

        private static bool MyRemoteCertificateValidationCallback(System.Object sender,
            X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;
            // If there are errors in the certificate chain,
            // look at each error to determine the cause.
            if (sslPolicyErrors != SslPolicyErrors.None) {
                for (int i=0; i<chain.ChainStatus.Length; i++) {
                    if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown) {
                        continue;
                    }
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan (0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build ((X509Certificate2)certificate);
                    if (!chainIsValid) {
                        isOk = false;
                        break;
                    }
                }
            }
            return isOk;
        }
    }
}