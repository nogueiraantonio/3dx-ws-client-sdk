//------------------------------------------------------------------------------------------------------------------------------------
// Copyright 2020 Dassault Systèmes - CPE EMED
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify,
// merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//------------------------------------------------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using ds.authentication.model;


namespace ds.authentication
{
    public class UserPassport : CASCookieBasedPassport, IPassportAuthentication
    {
        private const string LOGIN_WS = "/login";
    

        public UserPassport(string passportUrl) : base(passportUrl)
        {
        }


        private async Task<TicketInfo> GetLoginTicketInfo()
        {
            //Step 1 - Request login ticket
            RestRequest getLoginTicketRequest = new RestRequest(GetEndpointURL(LOGIN_WS));
            getLoginTicketRequest.AddQueryParameter("action", "get_auth_params");

            IRestResponse getLoginTicketResponse = await Client.ExecuteGetAsync(getLoginTicketRequest);

            if (getLoginTicketResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                //TODO: throw
            }

            //Handle ticket login response
            TicketInfo loginTicketInfo = SimpleJson.DeserializeObject<TicketInfo>(getLoginTicketResponse.Content);

            if (loginTicketInfo.response != "login")
            {
                //handle according to established exception policy
                //TODO: throw
            };

            return loginTicketInfo;
        }

        
        /*Bearer*/
        public async Task<bool> CASLogin(string username, string password, bool rememberMe)
        {
            //Step 1 - Request login ticket
            TicketInfo loginTicketInfo = await GetLoginTicketInfo();

            // Step 2 - build the login request
            string loginUrl = GetEndpointURL(LOGIN_WS);
            
            RestRequest loginRequest = new RestRequest(loginUrl, Method.POST, DataFormat.Json);

            loginRequest.AddParameter("lt", loginTicketInfo.lt);
            loginRequest.AddParameter("username", username);
            loginRequest.AddParameter("password", password);
            loginRequest.AddParameter("rememberMe", rememberMe);

            IRestResponse loginRequestResponse = await Client.ExecutePostAsync(loginRequest);

            if (loginRequestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                //throw new PassportException("CASLogin");
            }
            return IsCookieAuthenticated;
        }

        public async Task<T> CASLoginWithRedirection<T>(string username, string password, bool rememberMe, IPassportServiceRedirection _redir)
        {
            //Step 1 - Request login ticket
            TicketInfo loginTicketInfo = await GetLoginTicketInfo();

            // Step 2 - build the login request
            string loginUrl = string.Format("{0}?service={1}", GetEndpointURL(LOGIN_WS), _redir.GetServiceURL());
           
            RestRequest loginRequest = new RestRequest(loginUrl, Method.POST, DataFormat.Json);

            loginRequest.AddParameter("lt", loginTicketInfo.lt);
            loginRequest.AddParameter("username", username);
            loginRequest.AddParameter("password", password);
            loginRequest.AddParameter("rememberMe", rememberMe);

            IRestResponse loginRequestResponse = await Client.ExecutePostAsync(loginRequest);

            if (loginRequestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                //throw new PassportException("CASLoginWithRedirection");
            }

            if (!IsCookieAuthenticated)
            {
                //handle according to established exception policy
                //throw new PassportException("CASLoginWithRedirection - no cookie authentication");
            }

            return SimpleJson.DeserializeObject<T>(loginRequestResponse.Content);
        }

        /*IPassportAuthentication interface*/
        public bool IsCookieAuthentication()
        {
            return true;
        }

        public bool AuthenticateRequest(IRestRequest request, bool refreshAutomatically = true)
        {
            throw new NotImplementedException();
        }


        public CookieContainer GetCookieContainer()
        {
            return CookieContainer;
        }

        public string GetIdentity()
        {
            throw new NotImplementedException();
        }

        public bool IsValid()
        {
            return IsCookieAuthenticated;
        }

        //
    }
}
