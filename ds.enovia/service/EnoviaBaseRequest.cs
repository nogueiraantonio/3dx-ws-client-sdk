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

using ds.enovia.common.constants;
using RestSharp;
using System.Collections.Generic;

namespace ds.enovia.service
{
    public class EnoviaBaseRequest : RestRequest
    {
        public EnoviaBaseRequest(string _resource, string _tenant = null, string _securitycontext = null, string _csrftoken = null) : base(_resource)
        {
            SecurityContext = _securitycontext;
            CSRFToken       = _csrftoken;
            Tenant          = _tenant;

            if (SecurityContext != null)
                this.AddHeader(HttpRequestHeaders.SECURITY_CONTEXT, _securitycontext);

            if (CSRFToken != null)
                this.AddHeader(HttpRequestHeaders.ENO_CSRF_TOKEN, _csrftoken);
      
            if (Tenant != null)
                this.AddQueryParameter(HttpRequestParams.TENANT, _tenant);
        }

        public string SecurityContext { get; private set; }
        public string Tenant { get; private set; }
        public string CSRFToken { get; private set; }

        public void AddQueryParameters(IDictionary<string, string> _queryParameters)
        {
            if (_queryParameters == null) return;

            IEnumerator<KeyValuePair<string, string>> queryParamEnumerator = _queryParameters.GetEnumerator();

            while (queryParamEnumerator.MoveNext())
            {
                KeyValuePair<string, string> queryParamPair = queryParamEnumerator.Current;
                this.AddQueryParameter(queryParamPair.Key, queryParamPair.Value);
            }
        }

    }
}
       