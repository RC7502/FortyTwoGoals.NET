using System;
using System.Net;
using FortyTwoGoals.Models;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;
using RestSharp.Contrib;

namespace FortyTwoGoals
{
    public class Authenticator
    {
        const string BaseUrl = "http://api.42goals.com/v1";

        private string ConsumerKey;
        private string ConsumerSecret;
        private string RequestTokenUrl;
        private string AccessTokenUrl;
        private string AuthorizeUrl;

        private readonly IRestClient client;

        public Authenticator(string ConsumerKey, string ConsumerSecret, string RequestTokenUrl, string AccessTokenUrl,
                             string AuthorizeUrl, IRestClient restClient = null)
        {
            this.ConsumerKey = ConsumerKey;
            this.ConsumerSecret = ConsumerSecret;
            this.RequestTokenUrl = RequestTokenUrl;
            this.AccessTokenUrl = AccessTokenUrl;
            this.AuthorizeUrl = AuthorizeUrl;
            client = restClient ?? new RestClient(BaseUrl);
        }

        public string GenerateAuthUrlFromRequestToken(RequestToken token)
        {
            RestRequest request = null;
            request = new RestRequest(token.Token + "/");
            var authclient = new RestClient(AuthorizeUrl);
            var url = authclient.BuildUri(request).ToString();
            return url;
        }

        /// <summary>
        /// First step in the OAuth process is to ask Fitbit for a temporary request token. 
        /// From this you should store the RequestToken returned for later processing the auth token.
        /// </summary>
        /// <returns></returns>
        public RequestToken GetRequestToken()
        {
            client.Authenticator = OAuth1Authenticator.ForRequestToken(this.ConsumerKey, this.ConsumerSecret); 
            
            var request = new RestRequest(RequestTokenUrl, Method.GET);
            
            var response = client.Execute(request);

            var qs = HttpUtility.ParseQueryString(response.Content);

            RequestToken token = new RequestToken();

            token.Token = qs["oauth_token"];
            token.Secret = qs["oauth_token_secret"];

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception("Request Token Step Failed");

            return token;
        }

        /// <summary>
        /// For Desktop authentication. Your code should direct the user to the FitBit website to get
        /// Their pin, they can then enter it here.
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        public AuthCredential GetAuthCredentialFromPin(string pin, RequestToken token)
        {
            var request = new RestRequest("oauth/access_token", Method.POST);
            client.Authenticator = OAuth1Authenticator.ForAccessToken(ConsumerKey, ConsumerSecret, token.Token, token.Secret, pin);

            var response = client.Execute(request);
            var qs = RestSharp.Contrib.HttpUtility.ParseQueryString(response.Content);

            return new AuthCredential()
            {
                AuthToken = qs["oauth_token"],
                AuthTokenSecret = qs["oauth_token_secret"],
                UserId = qs["encoded_user_id"]
            };
        }

        public AuthCredential ProcessApprovedAuthCallback(RequestToken token)
        {
            if (string.IsNullOrWhiteSpace(token.Token))
                throw new Exception("RequestToken.Token must not be null");
            //else if 

            client.Authenticator = OAuth1Authenticator.ForRequestToken(this.ConsumerKey, this.ConsumerSecret);

            var request = new RestRequest(AccessTokenUrl, Method.POST);


            client.Authenticator = OAuth1Authenticator.ForAccessToken(
                this.ConsumerKey, this.ConsumerSecret, token.Token, token.Secret, token.Verifier
            );

            var response = client.Execute(request);

            //Assert.NotNull(response);
            //Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Content);

            var qs = HttpUtility.ParseQueryString(response.Content); //not actually parsing querystring, but body is formatted like htat
            var oauth_token = qs["oauth_token"];
            var oauth_token_secret = qs["oauth_token_secret"];
            var encoded_user_id = qs["encoded_user_id"];
            //Assert.NotNull(oauth_token);
            //Assert.NotNull(oauth_token_secret);

            /*
            request = new RestRequest("account/verify_credentials.xml");
            client.Authenticator = OAuth1Authenticator.ForProtectedResource(
                this.ConsumerKey, this.ConsumerSecret, oauth_token, oauth_token_secret
            );

            response = client.Execute(request);

             */

            return new AuthCredential()
            {
                AuthToken = oauth_token,
                AuthTokenSecret = oauth_token_secret,
                UserId = encoded_user_id
            };

            //Assert.NotNull(response);
            //Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            //request = new RestRequest("statuses/update.json", Method.POST);
            //request.AddParameter("status", "Hello world! " + DateTime.Now.Ticks.ToString());
            //client.Authenticator = OAuth1Authenticator.ForProtectedResource(
            //    consumerKey, consumerSecret, oauth_token, oauth_token_secret
            //);

            //response = client.Execute(request);

            //Assert.NotNull(response);
            //Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


    }
}
