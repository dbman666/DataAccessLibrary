using System;
using System.Net;
using System.Text;
using Coveo.Dal;

namespace slack
{
    public class SlackReporter
    {
        private string _username;
        private SvcLogger _logger;
        private string _slackUrl;
        private string _slackChannel;

        public SlackReporter(string p_Username, SvcLogger p_Logger, string p_SlackUrl, string p_SlackChannel)
        {
            _username = p_Username;
            _logger = p_Logger;
            _slackUrl = p_SlackUrl;
            _slackChannel = p_SlackChannel;
        }
        
        public void Send(string p_Message)
        {
            try {
                var req = WebRequest.CreateHttp(_slackUrl);
                req.Method = "POST";
                var body = $"{{ \"username\": \"{_username}\", \"channel\":\"{_slackChannel}\",  \"text\":\"{p_Message}\" }}";
                var bodyBytes = Encoding.UTF8.GetBytes(body);
                req.GetRequestStream().Write(bodyBytes, 0, bodyBytes.Length);
                var resp = (HttpWebResponse)req.GetResponse();
                if (resp.StatusCode != HttpStatusCode.OK)
                    _logger.Log($"Failed to report to Slack ({resp.StatusDescription}).");
            } catch (Exception exc) {
                _logger.Log($"Failed to report to Slack: {exc.Message}.");
            }
        }

    }
}
