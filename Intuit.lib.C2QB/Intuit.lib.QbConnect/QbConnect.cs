using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intuit.lib.C2QB.Configuration;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;
namespace Intuit.lib.C2QB
{
    public sealed class QbConnect : IConnect<QbConfig>
    {
        private string _oauthVerifier = string.Empty;
        private const string _dummyProtocol = "http://";
        private const string _dummyHost = "www.example.com";
        private IToken _requestToken;
        private QbResponse _qbResponse = null;
        private QbConfig _objectType = null;
        private SHDocVw.InternetExplorer _internetExplorer = null;
        public QbConnect()
        {

        }
        public bool Validate(QbConfig objectType)
        {
            throw new NotImplementedException();
        }

        public object Connect(QbConfig objectType)
        {
            try
            {
                startOAuthHandshake(objectType);
            }
            catch (Exception)
            {
                
                throw;
            }
            return null;
        }
        private void startOAuthHandshake(QbConfig objectType)
        {
            _requestToken = getOauthRequestTokenFromIpp(objectType);
            redirectToIppForUserAuthorization(_requestToken, objectType);
        }
        private IToken getOauthRequestTokenFromIpp(QbConfig objectType)
        {
            IOAuthSession oauthSession = createDevDefinedOAuthSession(objectType);
            return oauthSession.GetRequestToken();
        }
        private void redirectToIppForUserAuthorization(IToken requestToken, QbConfig objectType)
        {
            var oauthUserAuthorizeUrl = objectType.OauthUserAuthUrl;
            string navigateUrl = oauthUserAuthorizeUrl + "?oauth_token=" + requestToken.Token + "&oauth_callback=" + UriUtility.UrlEncode(_dummyProtocol + _dummyHost);
            WinSys(navigateUrl, objectType);
        }

        private void WinSys(string url, QbConfig objectType)
        {
            try
            {
                _qbResponse = new QbResponse();
                _objectType = objectType;
                var IE = new SHDocVw.InternetExplorer();
                object URL = url;
                IE.ToolBar = 0;
                IE.StatusBar = false;
                IE.MenuBar = false;
                IE.Width = 1022;
                IE.Height = 782;
                IE.Visible = true;
                IE.Navigate2(url, null, null, null, null);
                Console.WriteLine("Begin");
                while (IE.ReadyState != SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
                {
                    Console.WriteLine("Loading the web page...");
                    if (IE.ReadyState == SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
                    {
                        IE.NavigateComplete2 += IE_NavigateComplete2;
                        Console.WriteLine("Loaded!");
                        _internetExplorer = IE;
                    }
                }
            }
            catch (Exception)
            {
                
                throw;
            }
            while (true) ;
        }

        void IE_NavigateComplete2(object pDisp, ref object URL)
        {
            string hostUrl = URL as string;
            if (hostUrl.Contains("www.example.com"))
            {
                NameValueCollection query = System.Web.HttpUtility.ParseQueryString(hostUrl);
                _oauthVerifier = query["oauth_verifier"];
                _qbResponse.RealmId = query["realmId"];
                _qbResponse.DataSource = query["dataSource"];
                IToken accessToken = exchangeRequestTokenForAccessToken(_objectType,_requestToken);
                _qbResponse.AccessToken = accessToken.Token;
                _qbResponse.AccessSecret = accessToken.TokenSecret;
                _qbResponse.ExpirationDateTime = DateTime.Now.AddMonths(6);
                _internetExplorer.Quit();
                Debug.WriteLine(string.Format("Access Token : {0}", _qbResponse.AccessToken));
                Debug.WriteLine(string.Format("Access Secret : {0}", _qbResponse.AccessSecret));
                Console.WriteLine(string.Format("Access Token : {0}",_qbResponse.AccessToken));
                Console.WriteLine(string.Format("Access Secret : {0}",_qbResponse.AccessSecret));
            }
            Console.WriteLine(URL as string);
        }
        private IOAuthSession createDevDefinedOAuthSession(QbConfig objectType)
        {

            var oauthRequestTokenUrl = objectType.OauthBaseUrl + objectType.OauthRequestTokenEndpoint;
            var oauthAccessTokenUrl = objectType.OauthBaseUrl + objectType.OauthAccessTokenEndpoint;
            var oauthUserAuthorizeUrl = objectType.OauthUserAuthUrl;
            OAuthConsumerContext consumerContext = new OAuthConsumerContext
            {
                ConsumerKey = objectType.ConsumerKey,
                ConsumerSecret = objectType.ConsumerSecret,
                SignatureMethod = SignatureMethod.HmacSha1
            };
            return new OAuthSession(consumerContext, oauthRequestTokenUrl, oauthUserAuthorizeUrl, oauthAccessTokenUrl);
        }
        public IToken exchangeRequestTokenForAccessToken(QbConfig objectType, IToken requestToken)
        {
            IOAuthSession oauthSession = createDevDefinedOAuthSession(objectType);
            return oauthSession.ExchangeRequestTokenForAccessToken(requestToken, _oauthVerifier);
        }
    }
}