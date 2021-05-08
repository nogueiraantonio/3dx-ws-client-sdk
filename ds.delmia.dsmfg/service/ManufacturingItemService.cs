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
using ds.delmia.dsmfg.converter;
using ds.delmia.dsmfg.exception;
using ds.delmia.dsmfg.fields;
using ds.delmia.dsmfg.mask;
using ds.delmia.dsmfg.model;
using ds.enovia.common.collection;
using ds.enovia.common.search;
using ds.enovia.service;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ds.delmia.dsmfg.service
{
    public class ManufacturingItemService : EnoviaBaseService
    {
        private const string BASE_RESOURCE = "/resources/v1/modeler/dsmfg";
        private const string SEARCH = "/search";
        private const string MANUFACTURING_ITEM = "/dsmfg:MfgItem";
        private const string MANUFACTURING_INSTANCE= "/dsmfg:MfgItemInstance";
        private const string RESULTING_ENG_ITEM = "/dsmfg:ResultingEngItem";
        private const string SCOPE_ENG_ITEM = "/dsmfg:ScopeEngItem";

        public string GetBaseResource()
        {
            return BASE_RESOURCE;
        }

        public ManufacturingItemService(string _enoviaService, IPassportAuthentication passport) : base(_enoviaService, passport)
        {
        }

        #region dsmfg:ManufacturingItem

        //Gets a indexed search result of Manufacturing Item
        public async Task<NlsLabeledItemSet<ManufacturingItem>> Search(SearchQuery _searchQuery, long _skip = 0, long _top = 100)
        {
            string searchManufacturingItems = string.Format("{0}{1}{2}", GetBaseResource(), MANUFACTURING_ITEM, SEARCH);

            ManufacturingItemMask mfgItemMask = ManufacturingItemMask.Default;

            // masks
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("$mask", mfgItemMask.GetString());
            queryParams.Add("$skip", _skip.ToString());
            queryParams.Add("$top", _top.ToString());
            queryParams.Add("$searchStr", _searchQuery.GetSearchString());

            IRestResponse requestResponse = await GetAsync(searchManufacturingItems, queryParams);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new ManufacturingResponseException(requestResponse));
            }

            return JsonConvert.DeserializeObject<NlsLabeledItemSet<ManufacturingItem>>(requestResponse.Content);
        }

        public async Task<NlsLabeledItemSet<ManufacturingItemDetails>> SearchWithDetails(SearchQuery _searchQuery, long _skip = 0, long _top = 100)
        {
            string searchManufacturingItems = string.Format("{0}{1}{2}", GetBaseResource(), MANUFACTURING_ITEM, SEARCH);

            ManufacturingItemMask mfgItemMask = ManufacturingItemMask.Details;

            // masks
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("$mask", mfgItemMask.GetString());
            queryParams.Add("$skip", _skip.ToString());
            queryParams.Add("$top", _top.ToString());
            queryParams.Add("$searchStr", _searchQuery.GetSearchString());

            IRestResponse requestResponse = await GetAsync(searchManufacturingItems, queryParams);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new ManufacturingResponseException(requestResponse));
            }

            return JsonConvert.DeserializeObject<NlsLabeledItemSet<ManufacturingItemDetails>>(requestResponse.Content, new ManufacturingItemDetailsConverter() );

        }

        public async Task<NlsLabeledItemSet<ManufacturingItem>> GetManufacturingItem(string _mfgItemId)
        {
            string searchManufacturingItems = string.Format("{0}{1}/{2}", GetBaseResource(), MANUFACTURING_ITEM, _mfgItemId);

            ManufacturingItemMask mfgItemMask = ManufacturingItemMask.Default;

            // masks
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("$mask", mfgItemMask.GetString());

            IRestResponse requestResponse = await GetAsync(searchManufacturingItems, queryParams);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new ManufacturingResponseException(requestResponse));
            }

            return JsonConvert.DeserializeObject<NlsLabeledItemSet<ManufacturingItem>>(requestResponse.Content, new ManufacturingItemDetailsConverter());

        }

        public async Task<NlsLabeledItemSet<ManufacturingItem>> GetdManufacturingItemFields(string _mfgItemId, ManufacturingItemFields _mfgItemFields)
        {
            string searchManufacturingItems = string.Format("{0}{1}/{2}", GetBaseResource(), MANUFACTURING_ITEM, _mfgItemId);

            ManufacturingItemMask mfgItemMask = ManufacturingItemMask.Default;

            // masks
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("$mask", mfgItemMask.GetString());
            queryParams.Add("$fields", _mfgItemFields.ToString());

            IRestResponse requestResponse = await GetAsync(searchManufacturingItems, queryParams);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new ManufacturingResponseException(requestResponse));
            }

            return JsonConvert.DeserializeObject<NlsLabeledItemSet<ManufacturingItem>>(requestResponse.Content, new ManufacturingItemDetailsConverter());

        }

        public async Task<NlsLabeledItemSet<ManufacturingItemDetails>> GetManufacturingItemFieldsDetails(string _mfgItemId, ManufacturingItemFields _fields)
        {
            string getManufacturingItem = string.Format("{0}{1}/{2}", GetBaseResource(), MANUFACTURING_ITEM, _mfgItemId);

            ManufacturingItemMask mfgItemMask = ManufacturingItemMask.Details;

            // masks
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("$mask", mfgItemMask.GetString());
            queryParams.Add("$fields", _fields.ToString());
            
            IRestResponse requestResponse = await GetAsync(getManufacturingItem, queryParams);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new ManufacturingResponseException(requestResponse));
            }

            return JsonConvert.DeserializeObject<NlsLabeledItemSet<ManufacturingItemDetails>>(requestResponse.Content, new ManufacturingItemDetailsConverter());

        }

        #endregion

        #region dsmfg:ManufacturingInstance
        
        // Gets all the Manufacturing Item Instances
        public async Task<NlsLabeledItemSet<ManufacturingInstance>> GetManufacturingItemInstances(string _mfgItemId)
        {
            string getManufacturingInstances = string.Format("{0}{1}/{2}{3}", GetBaseResource(), MANUFACTURING_ITEM, _mfgItemId, MANUFACTURING_INSTANCE );

            ManufacturingItemInstanceMask mfgItemInstanceMask = ManufacturingItemInstanceMask.Default;

            // masks
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("$mask", mfgItemInstanceMask.GetString());

            IRestResponse requestResponse = await GetAsync(getManufacturingInstances, queryParams);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new ManufacturingResponseException(requestResponse));
            }

            return JsonConvert.DeserializeObject<NlsLabeledItemSet<ManufacturingInstance>>(requestResponse.Content);

        }

        public async Task<NlsLabeledItemSet<ManufacturingInstanceReference>> GetManufacturingItemInstancesWithReference(string _mfgItemId)
        {
            string getManufacturingInstances = string.Format("{0}{1}/{2}{3}", GetBaseResource(), MANUFACTURING_ITEM, _mfgItemId, MANUFACTURING_INSTANCE);

            ManufacturingItemInstanceMask mfgItemInstanceMask = ManufacturingItemInstanceMask.Details;

            // masks
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("$mask", mfgItemInstanceMask.GetString());

            IRestResponse requestResponse = await GetAsync(getManufacturingInstances, queryParams);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new ManufacturingResponseException(requestResponse));
            }

            return JsonConvert.DeserializeObject<NlsLabeledItemSet<ManufacturingInstanceReference>>(requestResponse.Content);

        }
        #endregion
    }
}
