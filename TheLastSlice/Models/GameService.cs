using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace TheLastSlice.Models
{
    public class GameService
    {
        private const string CHALLENGE_ROOT_URL = "https://thelastslice.azurewebsites.net/";

        private const string CHALLENGE_ONE_URL = CHALLENGE_ROOT_URL + "api/ChallengeOne";

        private const string SCORE_BOARD_URL = CHALLENGE_ROOT_URL + "api/ScoreBoard";

        private const string SCORE_BOARD_TOP_TEN_URL = CHALLENGE_ROOT_URL + "api/ScoreBoard/top10";

        private HttpClient client;

        public bool IsSuccessStatusCode { get; private set; }

        public GameService()
        {
            string tenant = "thelastslice.onmicrosoft.com";

            string clientId = "17315b0a-1d61-41c1-a614-aaa908fc6c3c";

            string webApiId = "webapi";

            string webApiResourceId = string.Format("https://{0}/{1}", tenant, webApiId);

            string signUpSignInPolicy = "B2C_1_SignUp_SignIn";

            string webApiScope = "default_scope";

            string[] scopes = new string[] { string.Format("https://{0}/{1}/{2}", tenant, webApiId, webApiScope) };

            client = new HttpClient(tenant, clientId, webApiResourceId, scopes, signUpSignInPolicy);
        }

        #region Login Methods

        public void ClearUserCache()
        {
            client.ClearCache();
        }

        public bool HasUserLoggedIn()
        {
            bool cachedUserExists = client.CachedUserExists();

            return cachedUserExists;
        }

        public async Task<string> Login()
        {
            string token = await client.GetAccessTokenAsync();

            return token;
        }

        public void Logout()
        {
            client.Logout();
        }

        #endregion

        #region Service Methods

        public async Task<string> PostScoreAsync(string score, string initials)
        {
            JProperty initialsProperty = new JProperty("Initials", initials);

            JProperty scoreProperty = new JProperty("Score", score);

            JObject payLoad = new JObject(initialsProperty, scoreProperty);

            var response = await client.PostUrlAsync(CHALLENGE_ONE_URL, payLoad.ToString());

            var content = response.Content;

            string result = content.ReadAsStringAsync().GetResults();

            IsSuccessStatusCode = response.IsSuccessStatusCode;

            return result;
        }

        public async Task<JArray> GetLeaderboardAsync()
        {
            HttpResponseMessage httpResponse = await client.GetUrlAnonymousAsync(SCORE_BOARD_TOP_TEN_URL);

            var content = httpResponse.Content;

            string result = content.ReadAsStringAsync().GetResults();

            JArray scores = ParseScores(result);

            return scores;
        }

        public async Task<JArray> GetLeaderboardWithCurrentUserAsync()
        {
            HttpResponseMessage httpResponse = await client.GetUrlAsync(SCORE_BOARD_URL); 

            var content = httpResponse.Content;

            string result = content.ReadAsStringAsync().GetResults();

            JArray scores = ParseScores(result);

            return scores;
        }

        #endregion

        #region Utility Methods

        private JArray ParseScores(string result)
        {
            JObject response = JObject.Parse(result);

            JArray scores = (JArray)response["Scores"];

            return scores;
        }

        #endregion
    }
}
