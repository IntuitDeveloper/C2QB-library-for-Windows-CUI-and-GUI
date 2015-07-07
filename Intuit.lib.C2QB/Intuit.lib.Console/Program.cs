using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intuit.lib.C2QB;
using Intuit.lib.C2QB.Configuration;
using System.Configuration;
namespace Intuit.lib.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            QbConfig config = new QbConfig();
            config.ApplicationToken = ConfigurationSettings.AppSettings["applicationToken"];
            config.ConsumerKey = ConfigurationSettings.AppSettings["consumerKey"];
            config.ConsumerSecret = ConfigurationSettings.AppSettings["consumerSecret"];
            config.OauthRequestTokenEndpoint = ConfigurationSettings.AppSettings["oauthRequestTokenEndpoint"];
            config.OauthAccessTokenEndpoint = ConfigurationSettings.AppSettings["oauthAccessTokenEndpoint"];
            config.OauthBaseUrl = ConfigurationSettings.AppSettings["oauthBaseUrl"];
            config.OauthUserAuthUrl = ConfigurationSettings.AppSettings["oauthUserAuthUrl"];
            config.SelectedMode = QbConfig.AppMode.Console;
            QbConnect obj = new QbConnect();
            obj.Connect(config);
        }
    }
}
