using System;
using System.Net;
using System.Text;

namespace Coveo.Dal
{
    public class SvcBaseWebProxy : SvcBase
    {
        private string _signService;
        
        public class ArgsBaseWebProxy : ArgsBase
        {
            public string Verb;
            public string UrlSuffix;
            public string Body;
        }

        public SvcBaseWebProxy(SvcLogger logger, SvcConfig config, string signService) : base(logger, config)
        {
            _signService = signService;
        }

        public string Redirect(string urlPrefix, ArgsBaseWebProxy args)
        {
            return Redirect(urlPrefix + args.UrlSuffix, args.Verb, args.Body);
        }
        
        public string Redirect(string uri, string verb, string body)
        {
            var request = WebRequest.CreateHttp(uri);
            request.Method = verb;
            byte[] bodyBytes = null;
            if (!string.IsNullOrWhiteSpace(body)) {
                request.ContentType = "application/json";
                bodyBytes = Encoding.UTF8.GetBytes(body);
                request.GetRequestStream().Write(bodyBytes);
            }

            if (_signService != null)
                SignV4Util.SignRequest(verb, request.RequestUri, new MyHeaders(request.Headers), bodyBytes, AwsCredentials.This, Reg.This.Name, _signService);
			
            try {
                return CmfUtil.ReadAllStreamAsUtf8String(request.GetResponse());
            } catch (Exception ex) {
                if (ex is WebException wex && wex.Response != null)
                    return CmfUtil.ReadAllStreamAsUtf8String(wex.Response);
                throw;
            }
        }
    }
}