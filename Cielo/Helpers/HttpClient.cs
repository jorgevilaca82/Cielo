using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Cielo.Helpers
{
    public class EasyHttpClient
    {
        private string _CharSet = "UTF-8";
        private string _ContentType = "application/x-www-form-urlencoded";
        private string _UserAgent = "EasyHttpClient";
        private HttpClient _httpClient;

        public EasyHttpClient()
        {
            _httpClient = new HttpClient();
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

    }
}
