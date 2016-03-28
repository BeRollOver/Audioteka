using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Audioteka
{
    public class VkResponse
    {
        public static T getResp<T>(string request)
        {
            string data;
            try
            {
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
            catch (Exception)
            {
                throw;
            }
        }

        public static string getAttachString(ObservableCollection<Attachment> attch)
        {
            string attachments = "";
            foreach (var item in attch)
            {
                switch (item.Type)
                {
                    case "photo":
                        attachments += item.Type + item.Photo.OwnerId + "_" + item.Photo.Id + ",";
                        continue;
                    case "video":
                        attachments += item.Type + item.Video.OwnerId + "_" + item.Video.Id + ",";
                        continue;
                    case "audio":
                        attachments += item.Type + item.Audio.OwnerId + "_" + item.Audio.Id + ",";
                        continue;
                    case "doc":
                        attachments += item.Type + item.Doc.OwnerId + "_" + item.Doc.Id + ",";
                        continue;
                    case "graffiti":
                        attachments += item.Type + item.Graffiti.OwnerId + "_" + item.Graffiti.Id + ",";
                        continue;
                    case "page":
                        attachments += item.Type + item.Page.OwnerId + "_" + item.Page.Id + ",";
                        continue;
                    case "note":
                        attachments += item.Type + item.Note.OwnerId + "_" + item.Note.Id + ",";
                        continue;
                    case "poll":
                        attachments += item.Type + item.Poll.OwnerId + "_" + item.Poll.Id + ",";
                        continue;
                    case "album":
                        attachments += item.Type + item.AlbumPhoto.OwnerId + "_" + item.AlbumPhoto.Id + ",";
                        continue;
                    case "link":
                        attachments += item.Link.Url + ",";
                        continue;
                    default:
                        continue;
                }
            }
            var i = attachments.Length - 1;
            attachments = attachments.Remove(i);
            return attachments;
        }
    }
}
