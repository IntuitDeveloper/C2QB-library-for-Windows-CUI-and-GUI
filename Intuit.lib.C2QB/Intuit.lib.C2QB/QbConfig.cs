using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intuit.lib.C2QB.Configuration
{
    class QbConfig
    {
        public string ApplicationToken { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string OauthRequestTokenEndpoint { get; set; }
        public string OauthAccessTokenEndpoint { get; set; }
        public string OauthBaseUrl { get; set; }
        public string OauthUserAuthUrl { get; set; }
        [Flags]
        public enum AppMode { Desktop,Console,WindowsPhone};
        public AppMode SelectedMode { get; set; }
    }
}
