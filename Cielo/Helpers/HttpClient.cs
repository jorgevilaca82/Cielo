using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Cielo.Helpers
{
    public class EasyHttpClient
    {
        private string _CharSet = "UTF-8";
        private string _ContentType = "application/x-www-form-urlencoded";
        private string _UserAgent = "EasyHttpClient";
        private HttpClient _httpClient;

        public const string ClientCertHash = "‎4EB6D578499B1CCF5F581EAD56BE3D9B6744A5E5";

        public EasyHttpClient()
        {
            WebRequestHandler handler = new WebRequestHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual; // this would pick from the Current user store
            //handler.ClientCertificates.Add(GetClientCertificate());
            var certPath = @"C:\xampp\htdocs\lojaexemplo-php\pages\ssl\VERISI~1.CRT";
            handler.ClientCertificates.Add(X509Certificate.CreateFromCertFile(certPath));

            //var handler = new HttpClientHandler();
            //handler.ClientCertificateOptions = ClientCertificateOption.Automatic;
            _httpClient = new HttpClient(handler);
            
            //_httpClient.DefaultRequestHeaders.Add("charset", _CharSet);
            //_httpClient.DefaultRequestHeaders.Add("user-agent", _UserAgent);
            //_httpClient.DefaultRequestHeaders.Add("content-type", _ContentType + "; charset=" + _CharSet);
        }

        public EasyHttpClient(string pCharSet, string pContentType, string pUserAgent) :
            this()
        {
            _CharSet = pCharSet;
            _ContentType = pContentType;
            _UserAgent = pUserAgent;
        }

        public async Task<string> PostAsync(string pUrl, string pData)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, pUrl);
            req.Headers.Add("charset", _CharSet);
            req.Headers.Add("User-Agent", _UserAgent);
            req.Content = new StringContent(pData, null, _ContentType);
            //req.Content.Headers.ContentType = new MediaTypeHeaderValue(_ContentType);
            var res = _httpClient.SendAsync(req);
            var sRes = await res.Result.Content.ReadAsStringAsync();
            return sRes;
        }

        /*
        public string Post(string pUrl, string pData)
        {
            return Request(pUrl, "POST", pData);
        }
        */

        public string Get()
        {
            return "Not implemented";
        }

        /*
        private string Request(string pUrl, string pMethod, string pData)
        {
            var Ret = "";
            
            var Request = (HttpWebRequest)WebRequest.Create(pUrl);
            Request.ContentLength = Encoding.GetEncoding(_CharSet).GetBytes(pData).Length;
            Request.ContentType = _ContentType + "; charset=" + _CharSet;
            Request.Method = pMethod;
            Request.UserAgent = _UserAgent;

            var Writer = new StreamWriter(Request.GetRequestStream());
            Writer.Write(pData);
            Writer.Close();
            Writer.Dispose();

            var Response = (HttpWebResponse)Request.GetResponse();
            StreamReader Reader = new StreamReader(Response.GetResponseStream());
            Ret = Reader.ReadToEnd();
            Reader.Close();
            Reader.Dispose();

            return Ret;
        }
        */

        // You SHOULD update here to if you client certificate locates in other x509 store
        private static X509Certificate GetClientCertificate()
        {
            //X509Store store = new X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine);
            X509Store store = new X509Store();
            store.Open(OpenFlags.ReadOnly);
            X509CertificateCollection col = store.Certificates.Find(X509FindType.FindByThumbprint, ClientCertHash, false);

            if (col.Count == 1)
            {
                return col[0];
            }
            else
            {
                throw new ApplicationException("Cannot find a certificate. Please follow the setup steps at the beginning of this file to import the Client certificate.");
            }
        }
    }
}
