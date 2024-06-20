//using RestSharp;
//using RestSharp.Authenticators;
//using RestSharp.Authenticators.OAuth2;

//namespace RedditTopPostsAndUsers
//{
//    public class RedditApiAuthenticator : AuthenticatorBase
//    {
//        private const string _baseUrl = "https://www.reddit.com";

//        public override 

//        public RedditApiAuthenticator(string clientId, string clientSecret)
//        {
//            var b = new  OAuth2AuthorizationRequestHeaderAuthenticator()
//            {
                
//            }

//            var t = new RestClient(new RestClient(new RestClientOptions(null) { Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator()}))
//        }

//        protected override ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
//        {
//            return new ValueTask<Parameter>(
//        }
//    }
//}
