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
using System.Threading.Tasks;
namespace Intuit.lib.C2QB
{
    public sealed class QbConnect : IConnect<QbConfig>
    {
        private string _oauthVerifier = string.Empty;
        private const string _tempProtocol = "http://";
        private const string _tempHost = "www.example.com";
        private IToken _requestToken;
        private QbResponse _qbResponse = null;
        private QbConfig _qbConfiguration = null;
        private SHDocVw.InternetExplorer _internetExplorer = null;
        private bool _isReadytoExit = false;
        public QbConnect()
        {

        }
        private async void ProcessAuthAsync()
        {
            if (_qbConfiguration!=null)
            {
                Task<QbResponse> task = StartOAuthHandshake(_qbConfiguration);
                _qbResponse = await task;
                if (_qbConfiguration.SelectedMode == QbConfig.AppMode.Console)
                {
                    Console.WriteLine("Access Token: " + _qbResponse.AccessToken);
                }
            }
        }
        /// <summary>
        /// TODO:Need to work on this
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public bool Validate(QbConfig objectType)
        {
            throw new NotImplementedException();
        }

        public void Connect(QbConfig qbConfiguration, out QbResponse qbResponse)
        {
            try
            {
                _qbConfiguration = qbConfiguration;
                Task task = new Task(ProcessAuthAsync);
                task.Start();
                task.Wait();
            }
            catch (Exception)
            {
                
                throw;
            }
            qbResponse = _qbResponse;
        }
        private async Task<QbResponse> StartOAuthHandshake(QbConfig objectType)
        {
            _requestToken = getOauthRequestTokenFromIpp(objectType);
            return await redirectToIppForUserAuthorization(_requestToken, objectType);
        }
        private IToken getOauthRequestTokenFromIpp(QbConfig objectType)
        {
            IOAuthSession oauthSession = createDevDefinedOAuthSession(objectType);
            return oauthSession.GetRequestToken();
        }
        private async Task<QbResponse> redirectToIppForUserAuthorization(IToken requestToken, QbConfig objectType)
        {
            string navigateUrl = string.Format("{0}?oauth_token={1}&oauth_callback={2}",
                objectType.OauthUserAuthUrl,
                requestToken.Token, 
                UriUtility.UrlEncode(_tempProtocol + _tempHost));
            return await WinSys(navigateUrl, objectType);
        }
        private async Task<QbResponse> WinSys(string url, QbConfig qbConfig)
        {
            return await WinApiAsync(url, qbConfig);
        }

        private async Task<QbResponse> WinApiAsync(string url, QbConfig qbConfig)
        {
            try
            {
                _qbResponse = new QbResponse();
                _qbConfiguration = qbConfig;
                var IE = new SHDocVw.InternetExplorer();
                object URL = url;
                IE.ToolBar = 0;
                IE.StatusBar = false;
                IE.MenuBar = false;
                IE.Width = 1022;
                IE.Height = 782;
                IE.Visible = true;
                IE.Navigate2(url, null, null, null, null);
                if (_qbConfiguration.SelectedMode == QbConfig.AppMode.Console)
                {
                    Console.WriteLine("Begin Loading...");
                }
                while (IE.ReadyState != SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
                {
                    if (_qbConfiguration.SelectedMode == QbConfig.AppMode.Console)
                    {
                        Console.WriteLine("Loading the web page...");
                    }
                    if (IE.ReadyState == SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
                    {
                        IE.NavigateComplete2 += IE_NavigateComplete2;
                        if (_qbConfiguration.SelectedMode == QbConfig.AppMode.Console)
                        {
                            Console.WriteLine("Loaded!");
                        }
                        _internetExplorer = IE;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            while (true)
            {
                if (_isReadytoExit)
                {
                    return _qbResponse;
                }
            };
        }
        void IE_NavigateComplete2(object pDisp, ref object URL)
        {
            string hostUrl = URL as string;
            if (hostUrl.Contains(_tempHost))
            {
                NameValueCollection query = System.Web.HttpUtility.ParseQueryString(hostUrl);
                _oauthVerifier = query["oauth_verifier"];
                _qbResponse.RealmId = query["realmId"];
                _qbResponse.DataSource = query["dataSource"];
                IToken accessToken = exchangeRequestTokenForAccessToken(_qbConfiguration, _requestToken);
                _qbResponse.AccessToken = accessToken.Token;
                _qbResponse.AccessSecret = accessToken.TokenSecret;
                _qbResponse.ExpirationDateTime = DateTime.Now.AddMonths(6);
                _internetExplorer.Quit();
                Debug.WriteLine(string.Format("Access Token : {0}", _qbResponse.AccessToken));
                Debug.WriteLine(string.Format("Access Secret : {0}", _qbResponse.AccessSecret));
                if (_qbConfiguration.SelectedMode == QbConfig.AppMode.Console)
                {
                    Console.WriteLine(string.Format("Access Token : {0}", _qbResponse.AccessToken));
                    Console.WriteLine(string.Format("Access Secret : {0}", _qbResponse.AccessSecret));
                }
                _isReadytoExit = true;
            }
            if (_qbConfiguration.SelectedMode == QbConfig.AppMode.Console)
            {
                Console.WriteLine(URL as string);
            }
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