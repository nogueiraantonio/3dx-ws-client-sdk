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

using ds.authentication;
using ds.enovia.common.collection;
using ds.enovia.common.search;
using ds.enovia.dseng.exception;
using ds.enovia.dseng.mask;
using ds.enovia.dseng.model;
using ds.enovia.dseng.model.configured;
using ds.enovia.dseng.model.filterable;
using ds.enovia.dseng.search;
using ds.enovia.service;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ds.enovia.dseng.service
{
    public class EngineeringServices : EnoviaBaseService
    {
        //example: "/resources/v1/modeler/dseng/dseng:EngItem/search?tenant={{tenant}}&$searchStr={{search-string}}&$mask=dsmveng:EngItemMask.Details&$top=1000&$skip=0"
        private const string BASE_RESOURCE = "/resources/v1/modeler/dseng/dseng:EngItem";
        private const string SEARCH = "/search";
        private const string ENTERPRISE_REFERENCE = "/dseng:EnterpriseReference";
        private const string CONFIGURED = "/dscfg:Configured";
        private const string ENGINEERING_INSTANCES = "/dseng:EngInstance";
        private const string FILTERABLE = "/dscfg:Filterable";

        public string GetBaseResource()
        {
            return BASE_RESOURCE;
        }

        public EngineeringServices (string _enoviaService, IPassportAuthentication passport) : base(_enoviaService, passport)
        {

        }

        public async Task<EngineeringSearchPage> Search(SearchQuery _searchString, long _skip = 0, long _top = 100, EngineeringSearchMask _mask = EngineeringSearchMask.Default)
        {
            return await _Search(_searchString.GetSearchString(), _skip, _top, _mask);
        }

        //Important: Queries must not exceed 4096 characters.
        private async Task<EngineeringSearchPage> _Search(string _searchString, long _skip = 0, long _top = 100, EngineeringSearchMask _mask = EngineeringSearchMask.Default)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("$mask", _mask.GetString());
            queryParams.Add("$skip", _skip.ToString());
            queryParams.Add("$top", _top.ToString());
            queryParams.Add("$searchStr", _searchString);

            string searchResource = string.Format("{0}{1}", GetBaseResource(), SEARCH);

            IRestResponse requestResponse = await GetAsync(searchResource, queryParams);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                //throw (new DerivedOutputException(requestResponse));
            }

            return JsonConvert.DeserializeObject<EngineeringSearchPage>(requestResponse.Content);
        }

        public async Task<EngineeringItem> GetEngineeringItemDetails(string _engineeringItemId)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("$mask", "dsmveng:EngItemMask.Details");

            string searchResource = string.Format("{0}/{1}", GetBaseResource(), _engineeringItemId);

            IRestResponse requestResponse = await GetAsync(searchResource, queryParams);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new EngineeringResponseException(requestResponse));
            }

            NlsLabeledItemSet<EngineeringItem> engItemSet = JsonConvert.DeserializeObject<NlsLabeledItemSet<EngineeringItem>>(requestResponse.Content);
            if ((engItemSet != null) && (engItemSet.totalItems == 1))
            {
                return engItemSet.member[0];
            }

            return null;
        }

        public async Task<EnterpriseReference> SetEnterpriseReference(EngineeringItem _item, EnterpriseReferenceCreate _itemNumber)
        {
            string setEnterpriseRefEndpoint = string.Format("{0}/{1}{2}", GetBaseResource(), _item.id, ENTERPRISE_REFERENCE);

            string payload = _itemNumber.toJson();

            IRestResponse requestResponse = await PostAsync(setEnterpriseRefEndpoint, null, null, payload);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new SetEnterpriseReferenceException(requestResponse));
            }

            EnterpriseReferenceSet enterpriseRefSet = JsonConvert.DeserializeObject<EnterpriseReferenceSet>(requestResponse.Content);
            if ((enterpriseRefSet != null) && (enterpriseRefSet.totalItems == 1))
            {
                return enterpriseRefSet.member[0];
            }

            return null;

        }
        public async Task<EnterpriseReference> GetEnterpriseReference(EngineeringItem _item)
        {
            string getEnterpriseRefEndpoint = string.Format("{0}/{1}{2}", GetBaseResource(), _item.id, ENTERPRISE_REFERENCE);

            IRestResponse requestResponse = await GetAsync(getEnterpriseRefEndpoint);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new GetEnterpriseReferenceException(requestResponse));
            }

            EnterpriseReferenceSet enterpriseRefSet = JsonConvert.DeserializeObject<EnterpriseReferenceSet>(requestResponse.Content);
            if ((enterpriseRefSet != null) && (enterpriseRefSet.totalItems ==1))
            {
                return enterpriseRefSet.member[0];
            }
            
            return null;
        }

        //Modifies the Enterprise Reference of an Engineering item
        public async Task<EnterpriseReference> UpdateEnterpriseReference(EngineeringItem _item, EnterpriseReferenceCreate _newRef)
        {
            string setEnterpriseRefEndpoint = string.Format("{0}/{1}{2}", GetBaseResource(), _item.id, ENTERPRISE_REFERENCE);

            string messageBody = _newRef.toJson();

            IRestResponse requestResponse = await PatchAsync(setEnterpriseRefEndpoint, null, null, messageBody);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new UpdateEnterpriseReferenceException(requestResponse));
            }

            EnterpriseReferenceSet enterpriseRefSet = JsonConvert.DeserializeObject<EnterpriseReferenceSet>(requestResponse.Content);
            if ((enterpriseRefSet != null) && (enterpriseRefSet.totalItems == 1))
            {
                return enterpriseRefSet.member[0];
            }

            return null;
        }

        #region  dseng:EngInstance
        
        //Gets all the Engineering Item Instances
        public async Task<NlsLabeledItemSet<EngineeringInstanceRef>> GetEngineeringInstances(EngineeringItem _item)
        {
            string getEngineeringInstances = string.Format("{0}/{1}{2}", GetBaseResource(), _item.id, ENGINEERING_INSTANCES);

            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("$mask", "dsmveng:EngInstanceMask.Details");

            IRestResponse requestResponse = await GetAsync(getEngineeringInstances, queryParams);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new EngineeringResponseException(requestResponse));
            }

            return JsonConvert.DeserializeObject<NlsLabeledItemSet<EngineeringInstanceRef>>(requestResponse.Content);
        }

        public async Task< NlsLabeledItemSet<EngineeringInstanceEffectivity>> GetEngineeringInstancesEffectivity(EngineeringItem _item)
        {
            string getEngineeringInstances = string.Format("{0}/{1}{2}", GetBaseResource(), _item.id, ENGINEERING_INSTANCES);

            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("$mask", "dsmveng:EngInstanceMask.Filterable");

            IRestResponse requestResponse = await GetAsync(getEngineeringInstances, queryParams);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new EngineeringResponseException(requestResponse));
            }

            return JsonConvert.DeserializeObject<NlsLabeledItemSet<EngineeringInstanceEffectivity>>(requestResponse.Content);
        }
        #endregion

        #region  dscfg:Filterable
        public async Task<ItemSet<EngInstanceEffectivityContent>> GetEngineeringInstanceEffectivity(string _itemId, string _instanceId)
        {
            string getEngineeringInstances = string.Format("{0}/{1}{2}/{3}", GetBaseResource(), _itemId, ENGINEERING_INSTANCES, _instanceId, FILTERABLE);

            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("$mask", "dsmvcfg:FilterableDetails");
            queryParams.Add("$fields", "dsmvcfg:attribute.effectivityContent");

            IRestResponse requestResponse = await GetAsync(getEngineeringInstances, queryParams);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new EngineeringResponseException(requestResponse));
            }

            return JsonConvert.DeserializeObject<ItemSet<EngInstanceEffectivityContent>>(requestResponse.Content); ;
        }

        public async Task< ItemSet<EngInstanceEffectivityHasEffectivity>> GetEngineeringInstanceHasEffectivity(string _itemId, string _instanceId)
        {
            string getEngineeringInstances = string.Format("{0}/{1}{2}/{3}", GetBaseResource(), _itemId, ENGINEERING_INSTANCES, _instanceId, FILTERABLE);

            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("$mask", "dsmvcfg:FilterableDetails");
            queryParams.Add("$fields", "dsmvcfg:attribute.hasEffectivity");

            IRestResponse requestResponse = await GetAsync(getEngineeringInstances, queryParams);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new EngineeringResponseException(requestResponse));
            }

            return JsonConvert.DeserializeObject<ItemSet<EngInstanceEffectivityHasEffectivity>>(requestResponse.Content); ;
        }

        public async Task<ItemSet<EngInstanceEffectivityHasChange>> GetEngineeringInstanceHasChangeOrder(string _itemId, string _instanceId)
        {
            string getEngineeringInstances = string.Format("{0}/{1}{2}/{3}", GetBaseResource(), _itemId, ENGINEERING_INSTANCES, _instanceId, FILTERABLE);

            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("$mask", "dsmvcfg:FilterableDetails");
            queryParams.Add("$fields", "dsmvcfg:attribute.hasChange");

            IRestResponse requestResponse = await GetAsync(getEngineeringInstances, queryParams);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new EngineeringResponseException(requestResponse));
            }
            return JsonConvert.DeserializeObject<ItemSet<EngInstanceEffectivityHasChange>>(requestResponse.Content); ;            
        }
       
        #endregion

        #region  dscfg:Configured

        //This extension gets the Enabled Criteria and Configuration Contexts of Configured object
        public async Task<EngineeringItemConfigurationDetails> GetConfigurationDetails(string _itemId)
        {
            string getConfigurationDetails = string.Format("{0}/{1}{2}", GetBaseResource(), _itemId, CONFIGURED);

            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("$mask", "dsmvcfg:ConfiguredDetails");

            IRestResponse requestResponse = await GetAsync(getConfigurationDetails, queryParams);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new EngineeringResponseException(requestResponse));
            }

            NlsLabeledItemSet<EngineeringItemConfigurationDetails> configurationSet = JsonConvert.DeserializeObject<NlsLabeledItemSet<EngineeringItemConfigurationDetails>>(requestResponse.Content);
            if ((configurationSet != null) && (configurationSet.totalItems == 1))
            {
                return configurationSet.member[0];
            }

            return null;
        }

        public async Task<bool?> GetIsConfigured(string _itemId)
        {
            string getIsConfigured = string.Format("{0}/{1}{2}", GetBaseResource(), _itemId, CONFIGURED);

            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("$fields", "dsmvcfg:attribute.isConfigured");

            IRestResponse requestResponse = await GetAsync(getIsConfigured, queryParams);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new EngineeringResponseException(requestResponse));
            }

            ItemSet<EngineeringItemIsConfigured> isConfiguredSet = 
                JsonConvert.DeserializeObject<ItemSet<EngineeringItemIsConfigured>>(requestResponse.Content);

            if ((isConfiguredSet != null) && (isConfiguredSet.totalItems == 1))
            {
                return isConfiguredSet.member[0].isConfigured;
            }

            return null;
        }
        
        #endregion

    }
}
