using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace TheLastSlice
{
    public class HttpClient
    {
        //values set in the constructor
        private string _tenant = string.Empty;

        //Application Id from the native app registration in the b2c Directory.  Guid
        private string _clientId = string.Empty;

        //App ID URI from Web API Registration in b2c directory settings
        private string _webApiResourceId = string.Empty;

        private string _signup_signin_policy = string.Empty;

        private IEnumerable<string> _scopes = null;

        //constant used to create the _authority
        const string BASE_AUTHORITY = "https://login.microsoftonline.com/tfp/{tenant}/{policy}/oauth2/v2.0/authorize";

        //used for connecting to AAD.  Populated in the constructor
        private string _authority = string.Empty;

        private Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();
        private PublicClientApplication _pca = null;

        private string _bearerToken = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenant">The Tenant in Azure Active Directory.  {TenantName}.onmicrosoft.com</param>
        /// <param name="clientId">pplication Id from the native app registration in the b2c Directory.  Guid format</param>
        /// <param name="webApiResourceId">App ID URI from Web API Registration in b2c directory settings</param>
        /// <param name="scopes">an array of scopes that the application would like access to</param>
        /// <param name="signup_signin_policy">the name of the policy in AAD B2C for sign up / sign in</param>
        public HttpClient(string tenant, string clientId, string webApiResourceId, IEnumerable<string> scopes, string signup_signin_policy)
        {
            //copy parameters to private variables
            _tenant = tenant;
            _clientId = clientId;
            _scopes = scopes;
            _signup_signin_policy = signup_signin_policy;
            _webApiResourceId = webApiResourceId;
            _authority = BASE_AUTHORITY.Replace("{tenant}", _tenant).Replace("{policy}", signup_signin_policy);

            //create the object used to communicate with Azure AAD B2C
            _pca = new PublicClientApplication(_clientId);
        }

        public void Logout()
        {
            _bearerToken = string.Empty;
            ClearCache();
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_bearerToken))
            {
                return _bearerToken;
            }

            AuthenticationResult authResult = null;
            try
            {
                IUser user = GetUserByPolicy(_pca.Users, _signup_signin_policy);
                if (user == null)
                {
                    authResult = await _pca.AcquireTokenAsync(_scopes, user, UIBehavior.SelectAccount, string.Empty, null, _authority);
                }
                else
                {
                    authResult = await _pca.AcquireTokenSilentAsync(_scopes, user, _authority, true);
                }
            }
            catch (Exception ex)
            {
                new TelemetryClient().TrackException(new ExceptionTelemetry(ex));
                ClearCache();
                Stopwatch stopwatch = Stopwatch.StartNew();
                while (true)
                {
                    //some other processing to do possible
                    if (stopwatch.ElapsedMilliseconds >= 2000)
                    {
                        break;
                    }
                }
                return await GetAccessTokenAsync();
            }

            _bearerToken = authResult.AccessToken;
            return _bearerToken;
        }

        private IUser GetUserByPolicy(IEnumerable<IUser> users, string policy)
        {
            foreach (var user in users)
            {
                string userIdentifier = Base64UrlDecode(user.Identifier.Split('.')[0]);
                if (userIdentifier.EndsWith(policy.ToLower()))
                    return user;
            }

            return null;
        }

        private string Base64UrlDecode(string s)
        {
            s = s.Replace('-', '+').Replace('_', '/');
            s = s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
            var byteArray = Convert.FromBase64String(s);
            var decoded = Encoding.UTF8.GetString(byteArray, 0, byteArray.Count());
            return decoded;
        }

        public async Task<HttpResponseMessage> GetUrlAsync(string url)
        {
            string token = await GetAccessTokenAsync();

            // Add the access token to the Authorization Header of the call to the Graph API, and call the Graph API.
            httpClient.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", token);
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(url));

            return response;
        }

        public async Task<HttpResponseMessage> GetUrlAnonymousAsync(string url)
        {
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(url));
            return response;
        }


        public async Task<HttpResponseMessage> PostUrlAsync(string url, string body)
        {
            string token = await GetAccessTokenAsync();

            // Add the access token to the Authorization Header of the call to the Graph API, and call the Graph API.
            httpClient.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", token);
            HttpStringContent stringBody = new HttpStringContent(body, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(new Uri(url), stringBody);

            return response;
        }

        //UWP uses persistent storage, and MSAL uses it to save the users in cache.  if you try to change users, you will get an error when the 
        //user found in the cache, and sent in the request does not match the user returned by the login.  
        //ClearCache() removes all users from the cache.
        public void ClearCache()
        {
            foreach (var user in _pca.Users)
            {
                _pca.Remove(user);
            }
        }

        public IEnumerable<string> GetCachedUsers()
        {
            return _pca.Users.Select(c => c.Identifier).ToArray();
        }

        public bool CachedUserExists()
        {
            return _pca.Users.Count() > 0;
        }
    }
}
