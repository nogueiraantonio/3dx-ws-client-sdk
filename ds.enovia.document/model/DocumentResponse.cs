using ds.enovia.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ds.enovia.document.model
{
    public class DocumentResponse<T> : SerializableJsonObject
    {
        public bool success { get; set; }
        public long statusCode { get; set; }

        public CsrfToken csrf { get; set; }

        public DocumentResponse()
        {
            data = new List<T>();
        }

        public List<T> data { get; set; }
        public List<object> definitions { get; set; }
    }
}
