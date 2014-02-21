using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FortyTwoGoals;
using FortyTwoGoals.Models;

namespace SampleMVCApp.Controllers
{
    public class FortyTwoGoalsController : Controller
    {
        public ActionResult Authorize()
        {

            //make sure you've set these up in Web.Config under <appSettings>:
            string ConsumerKey = ConfigurationManager.AppSettings["FTGConsumerKey"];
            string ConsumerSecret = ConfigurationManager.AppSettings["FTGConsumerSecret"];

            var authenticator = new Authenticator(ConsumerKey,ConsumerSecret,"oauth/request_token","oauth/access_token","http://42goals.com/settings/authorize/");
            RequestToken token = authenticator.GetRequestToken();
            Session.Add("FTGRequestTokenSecret", token.Secret); //store this somehow, like in Session as we'll need it after the Callback() action

            //note: at this point the RequestToken object only has the Token and Secret properties supplied. Verifier happens later.

            string authUrl = authenticator.GenerateAuthUrlFromRequestToken(token);


            return Redirect(authUrl);
        }

        //Final step. Take this authorization information and use it in the app
        public ActionResult Callback()
        {
            RequestToken token = new RequestToken();
            token.Token = Request.Params["oauth_token"];
            token.Secret = Session["FTGRequestTokenSecret"].ToString();
            token.Verifier = Request.Params["oauth_verifier"];

            string ConsumerKey = ConfigurationManager.AppSettings["FTGConsumerKey"];
            string ConsumerSecret = ConfigurationManager.AppSettings["FTGConsumerSecret"];

            //this is going to go back to Fitbit one last time (server to server) and get the user's permanent auth credentials

            //create the Authenticator object
            Authenticator authenticator = new Authenticator(ConsumerKey, ConsumerSecret,
                                                                                    "http://api.42goals.com/v1/oauth/request_token/",
                                                                                    "http://api.42goals.com/v1/oauth/access_token/",
                                                                                    "http://42goals.com/settings/authorize/");


            //execute the Authenticator request to Fitbit
            AuthCredential credential = authenticator.ProcessApprovedAuthCallback(token);

            //here, we now have everything we need for the future to go back to Fitbit's API (STORE THESE):
            //  credential.AuthToken;
            //  credential.AuthTokenSecret;
            //  credential.UserId;

            // For demo, put this in the session managed by ASP.NET
            Session["FTGAuthToken"] = credential.AuthToken;
            Session["FTGAuthTokenSecret"] = credential.AuthTokenSecret;
            Session["FTGUserId"] = credential.UserId;

            return RedirectToAction("Index", "Home");

        }

    }
}
