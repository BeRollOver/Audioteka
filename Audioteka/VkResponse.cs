using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Audioteka
{
    class VkResponse
    {
        public static T getResp<T>(string request)
        {
            string data;
            var webrequest = WebRequest.Create(request);
            var response = webrequest.GetResponseAsync();
            using (Stream stream = response.Result.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    data = reader.ReadToEnd();
                }
            }
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}
