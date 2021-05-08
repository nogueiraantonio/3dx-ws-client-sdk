//------------------------------------------------------------------------------------------------------------------------------------
// Copyright 2021 Dassault Systèmes - CPE EMED
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
using ds.enovia.common.search;
using ds.enovia.document.exception;
using ds.enovia.document.model;
using ds.enovia.service;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ds.enovia.document.service
{
    public class DocumentService : EnoviaBaseService
    {
        private const string BASE_RESOURCE = "/resources/v1/modeler/documents";
        private const string FILES_CHECKIN = "/files/CheckinTicket";
        private const string PARENT = "/parentId";

        private const string SEARCH = "/search";

        private const string ATTACHMENT = "Reference Document";
        private const string SPECIFICATION = "PLMDocConnection";
        private const string SOURCE = "from";


        public string GetBaseResource()
        {
            return BASE_RESOURCE;
        }

        public DocumentService(string _enoviaService, IPassportAuthentication passport) : base(_enoviaService, passport)
        {

        }

        public async Task<DocumentResponse<Document>> Search(SearchQuery _searchString)
        {
            return await _Search(_searchString.GetSearchString());
        }

        //Important: Queries must not exceed 4096 characters.
        private async Task<DocumentResponse<Document>> _Search(string _searchString)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            //queryParams.Add("$include", _include.GetString());
            //queryParams.Add("$fields", _fields.ToString());
            queryParams.Add("searchStr", _searchString);

            string searchResource = string.Format("{0}{1}", GetBaseResource(), SEARCH);

            IRestResponse requestResponse = await GetAsync(searchResource, queryParams);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new DocumentSearchException (requestResponse));
            }

            return JsonConvert.DeserializeObject<DocumentResponse<Document>>(requestResponse.Content);
        }
        #region Public Interface
        public async Task<DocumentResponse<DocumentCreated>> CreateDocumentAsSpecification(string _title, string _description, string _parentId, string _fileLocalPath, string _collabSpace = null)
        {
            //TODO: Check that the _fileLocalPath exists and the process has read permissions

            string filename = System.IO.Path.GetFileName(_fileLocalPath);

            string uploadFileReceipt = await UploadFile(_fileLocalPath);

            DocumentCreate doc = InitializeDocument(_title, _description, _collabSpace); new DocumentCreate();

            return await CreateDocumentAsSpecification(doc, filename, _parentId, uploadFileReceipt);
        }

        public async Task<DocumentResponse<DocumentCreated>> CreateDocumentAsAttachment(string _title, string _description, string _parentId, string _fileLocalPath, string _collabSpace = null)
        {
            //TODO: Check that the _fileLocalPath exists and the process has read permissions

            string filename = System.IO.Path.GetFileName(_fileLocalPath);

            string uploadFileReceipt = await UploadFile(_fileLocalPath);

            DocumentCreate doc = InitializeDocument(_title, _description, _collabSpace); new DocumentCreate();

            return await CreateDocumentAsAttachment(doc, filename, _parentId, uploadFileReceipt);
        }

        public async Task<DocumentResponse<Document>> GetAttachedDocuments(string _parentId)
        {
            return await GetConnectedDocuments(_parentId, SOURCE, ATTACHMENT);
        }

        public async Task<DocumentResponse<Document>> GetSpecificationDocuments(string _parentId)
        {
            return await GetConnectedDocuments(_parentId, SOURCE, SPECIFICATION);
        }

        #endregion

        private async Task<DocumentResponse<Document>> GetConnectedDocuments(string _parentId, string _parentDirection, string _parentRelName)
        {
            string getDocumentsFromParent = string.Format("{0}{1}/{2}", GetBaseResource(), PARENT, _parentId);

            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("parentRelName", _parentRelName);
            queryParams.Add("parentDirection", _parentDirection);

            IRestResponse requestResponse = await GetAsync(getDocumentsFromParent, queryParams);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new GetDocumentsFromParentException(requestResponse));
            }

            return JsonConvert.DeserializeObject<DocumentResponse<Document>>(requestResponse.Content);            
        }

        private async Task<DocumentResponse<DocumentCreated>> CreateDocumentAsAttachment(DocumentCreate _doc, string _fileTitle, string _parentId, string _fileUploadReceipt)
        {
            return await CreateConnectedDocument(_doc, _fileTitle, _parentId, SOURCE, ATTACHMENT, _fileUploadReceipt);
        }
        private async Task<DocumentResponse<DocumentCreated>> CreateDocumentAsSpecification(DocumentCreate _doc, string _fileTitle, string _parentId, string _fileUploadReceipt)
        {
            return await CreateConnectedDocument(_doc, _fileTitle, _parentId, SOURCE, SPECIFICATION, _fileUploadReceipt);
        }


        private DocumentCreate InitializeDocument(string _title, string _description, string _collabSpace = null)
        {
            DocumentCreate __doc = new DocumentCreate();

            __doc.data.Add(new DocumentDataCreate());
            __doc.data[0].dataelements.title = _title;
            __doc.data[0].dataelements.description = _description;

            if (_collabSpace != null)
                __doc.data[0].dataelements.collabspace = _collabSpace;

            return __doc;
        }

        private async Task<DocumentResponse<DocumentCreated>> CreateConnectedDocument(DocumentCreate _doc, string _fileTitle, string _parentId, string _parentDirection, string _parentConnectionName,  string _fileUploadReceipt)
        {
            DocumentDataCreate docData = _doc.data[0];

            if (docData.relateddata == null)
                docData.relateddata = new DocumentRelatedDataCreate();

            docData.relateddata.files.Add(new DocumentRelatedDataFilesCreate());
            docData.relateddata.files[0].dataelements.title = _fileTitle;
            docData.relateddata.files[0].dataelements.receipt = _fileUploadReceipt;

            //Add parent 
            docData.dataelements.parentId = _parentId;
            docData.dataelements.parentDirection = _parentDirection;
            docData.dataelements.parentRelName = _parentConnectionName;

            string bodyRequest = _doc.toJson();

            IRestResponse createDocumentResponse = await PostAsync(GetBaseResource(), null, null, bodyRequest);

            if (createDocumentResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new CreateDocumentException(createDocumentResponse));
            }

            return JsonConvert.DeserializeObject<DocumentResponse<DocumentCreated>>(createDocumentResponse.Content);
        }
        private async Task<string> UploadFile(string _fileLocalPath)
        {
            //Get the FCS Checkin Ticket
            string filesCheckInResource = string.Format("{0}{1}", GetBaseResource(), FILES_CHECKIN);

            IRestResponse requestResponse = await PutAsync(filesCheckInResource);

            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new CheckInException(requestResponse));
            }

            FileCheckInTicket fileCheckInTicket = JsonConvert.DeserializeObject<FileCheckInTicket>(requestResponse.Content);

            //Upload file
            FileCheckInTicketData data = fileCheckInTicket.data[0];

            string ticketParamName = data.dataelements.ticketparamname;
            string ticket = data.dataelements.ticket;
            string ticketUrl = data.dataelements.ticketURL;

            RestRequest request = new RestRequest(ticketUrl, Method.POST);
            request.AddParameter(ticketParamName, ticket);
            request.AddFile(System.IO.Path.GetFileName(_fileLocalPath), _fileLocalPath);

            IRestResponse response = await m_client.ExecutePostAsync(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //handle according to established exception policy
                throw (new UploadFileException(requestResponse));
            }

            string __receipt = response.Content;

            if (__receipt.EndsWith("\n"))
            {
                __receipt = __receipt.Substring(0, __receipt.Length - 1);
            }

            return __receipt;
        }
    }
}
