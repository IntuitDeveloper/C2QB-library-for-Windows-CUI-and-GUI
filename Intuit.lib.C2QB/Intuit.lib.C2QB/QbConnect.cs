using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intuit.lib.C2QB.Configuration;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using System.Diagnostics;
namespace Intuit.lib.C2QB
{
    sealed class QbConnect : IConnect<QbConfig>
    {
        private string _oauthVerifier = string.Empty;
        private const string _dummyProtocol = "http://";
        private const string _dummyHost = "www.example.com";
        private IToken _requestToken;
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
            //redirectToIppForUserAuthorization(_requestToken, objectType);
        }
        private IToken getOauthRequestTokenFromIpp(QbConfig objectType)
        {
            IOAuthSession oauthSession = createDevDefinedOAuthSession(objectType);
            return oauthSession.GetRequestToken();
        }
        //private void oauthBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        //{
        //    if (e.Url.Host == _dummyHost)
        //    {
        //        NameValueCollection query = HttpUtility.ParseQueryString(e.Url.Query);
        //        _oauthVerifier = query["oauth_verifier"];
        //        _ippRealmOAuthProfile.realmId = query["realmId"];
        //        _ippRealmOAuthProfile.dataSource = query["dataSource"];
        //        _caughtCallback = true;
        //        oauthBrowser.Navigate("about:blank");
        //    }
        //    else if (_caughtCallback)
        //    {
        //        IToken accessToken = exchangeRequestTokenForAccessToken(_consumerKey, _consumerSecret, _requestToken);
        //        _ippRealmOAuthProfile.accessToken = accessToken.Token;
        //        _ippRealmOAuthProfile.accessSecret = accessToken.TokenSecret;
        //        _ippRealmOAuthProfile.expirationDateTime = DateTime.Now.AddMonths(6);
        //        this.DialogResult = DialogResult.OK;
        //        this.Close();
        //    }
        //}
        //private void redirectToIppForUserAuthorization(IToken requestToken, QbConfig objectType)
        //{
        //    var oauthUserAuthorizeUrl = objectType.OauthUserAuthUrl;
        //    string navigateUrl = oauthUserAuthorizeUrl + "?oauth_token=" + requestToken.Token + "&oauth_callback=" + UriUtility.UrlEncode(_dummyProtocol + _dummyHost);
        //    System.Diagnostics 
        //}
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