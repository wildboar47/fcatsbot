using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using WatsonServices.Models.VisualRecognition;
using System.Net;
using FCatsBot.Strings;

namespace FCatsBot
{

    internal class ModerationResult
    {
        internal double maxCatScore = 0;
        internal string json;
        /// <summary>
        /// class and score
        /// </summary>
        internal Dictionary<string, double> ClassesScores;
        /// <summary>
        /// type_hierarchy and score
        /// </summary>
        internal Dictionary<string, double> TypesScores;
        internal bool HasCatOrKittenClass;

        internal ModerationResult(string _json)
        {
            json = _json;
            ClassesScores = new Dictionary<string, double>();
            TypesScores = new Dictionary<string, double>();
        }

        internal void AddClassScore(string wclass, double wscore, string wtype)
        {
            if (wclass == null) wclass = "";
            if (wtype == null) wtype = "";
            //if (wscore == null) wscore = 0;

            ClassesScores.Add(wclass, wscore);
            TypesScores.Add(wtype, wscore);
            if ((wclass == "cat" || wclass == "kitten" || wclass.Contains("kitten") || wclass.Contains("cat") ) && (wscore > 0.64))
            {
                HasCatOrKittenClass = true;
                maxCatScore = Math.Max(maxCatScore, wscore);
            }
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in ClassesScores)
            {
                sb.Append(item.Key);
                sb.Append(" - ");
                sb.Append(item.Value.ToString("G6"));
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }
        public string ToStringNoNewline()
        {
            StringBuilder sb = new StringBuilder();
            int ind = 0;
            foreach (var item in ClassesScores)
            {
                sb.Append(item.Key);
                sb.Append("(type ");
                sb.Append(TypesScores.ElementAt(ind).Key);
                sb.Append(") - ");
                sb.Append(item.Value.ToString("G6"));
                sb.Append("; ");
                ind++;
            }

            return sb.ToString();
        }
    }
    internal class WatsonModeration
    {
        private List<string> file_ids;
        /// <summary>
        /// watson api key
        /// </summary>
        internal string watson_apikey = "59f9cd8fb292ded3e3410f198a692ba4ba337e96";
        private string watson_version = "2016-05-20";
        /// <summary>
        /// watson url
        /// </summary>
        private string watson_URL = "https://gateway-a.watsonplatform.net/visual-recognition/api/v3/classify";

        private string watson_exURL = "https://watson-api-explorer.mybluemix.net/visual-recognition/api/v3/classify";

        //      "url": "https://gateway-a.watsonplatform.net/visual-recognition/api",
        //"note": "It may take up to 5 minutes for this key to become active",
        //"api_key": "59f9cd8fb292ded3e3410f198a692ba4ba337e96"
        internal WatsonModeration()
        {
            file_ids = new List<string>();
        }
        internal void add(string fileid)
        {
            file_ids.Add(fileid.Trim());
        }
        /// <summary>
        /// moderate defined file id
        /// </summary>
        /// <param name="fileid">file id</param>
        /// <param name="userid">user id</param>
        /// <returns>ModerationResult</returns>
        internal async Task<ModerationResult> moderateW(string fileid, int userid)
        {
            try
            {
                var file = await MainParams.TGBot.GetFileAsync(fileid);
                //MemoryStream ms = new MemoryStream();

                var file_path = file.FilePath;
                var urlImage = "https://api.telegram.org/file/bot" + MainParams.bot_token + "/" + file_path;
                using (var client = new HttpClient())
                {
                    string json = "";
                    //System.Net.WebUtility.UrlEncode
                    string fileUrl = Uri.EscapeUriString(urlImage);
                    string wurl = watson_URL + string.Format("?api_key={0}&version={1}&url={2}", watson_apikey, watson_version, fileUrl);
                    MainParams.nlog.Trace("FILE url=" + urlImage);
                    // MainParams.nlog.Trace("WATSON url=" + wurl);
                    var response = await client.GetAsync(wurl);
                    json = await response.Content.ReadAsStringAsync();
                    ModerationResult res = new ModerationResult(json);

                    var modelClassifyResponse = await response.Content.ReadAsAsync<ClassifyResponse>();

                    foreach (var img in modelClassifyResponse.Images)
                    {
                        foreach (var score in img.Scores)
                        {
                            foreach (var cres in score.ClassResults)
                            {
                                res.AddClassScore(cres.ClassId, cres.Score, cres.TypeHierarchy);
                            }
                        }
                    }

                    MainParams.nlog.Trace("WATSON picturesCurrentDay;"+ MainParams.picturesCurrentDay + "; JSON;" + res.ToStringNoNewline());
                    Console.WriteLine("Watson OK fid=" + fileid);
                    string tableid = MainParams.datebase.updateJson(fileid, res.ToStringNoNewline(), userid, res.maxCatScore);
                    MainParams.picturesCurrentDay++;
                    //add to cats table and delete from moderation
                    if (res.HasCatOrKittenClass)
                    {
                        MainParams.datebase.ok_from_moderation(tableid, false,urlImage);
                    }

                    return res;
                }
            }
            catch (Exception e)
            {
                MainParams.nlog.Debug(e);
                Console.WriteLine("Watson error " + e.Message);
            }
            return new ModerationResult("");
        }
        /// <summary>
        /// get telegram file id from url (send to moderator)
        /// </summary>
        /// <param name="url"></param>
        internal async Task<string> getFileIdFromUrl(string url)
        {
            string fileid = "";
            try
            {
                using (var client = new HttpClient())
                {
                    Stream imageBytes = await client.GetStreamAsync(url);
                    Telegram.Bot.Types.FileToSend fs = new Telegram.Bot.Types.FileToSend();
                    fs.Content = imageBytes;
                    Uri uri = new Uri(url);
                    fs.Filename = Path.GetFileName(uri.AbsolutePath);
                    var mesToModerator = await MainParams.TGBot.SendPhotoAsync(MainParams.moderator_id, fs, "cat from url #" + MainParams.catFromUrl);
                    fileid = mesToModerator.Photo.Last().FileId;
                    MainParams.nlog.Trace("getFileIdFromUrl Mess to Moderator sent; url=" + url);

                }
            }
            catch (Exception e)
            {
                MainParams.nlog.Debug("***NewLogs; getFileIdFromUrl!!! ;EX=;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                MainParams.nlog.Debug(e);
            }
            return fileid;
        }
        /// <summary>
        /// moderate jpg/png url.
        /// if has cat - process it to Telegram file id.
        /// </summary>
        /// <param name="url">jpg/png url</param>
        /// <param name="userid">user id</param>
        /// <param name="sendToUserid">send to user(true) or to moderator</param>
        /// <param name="curuser">user to send</param>
        /// <param name="fromUrl">we got pic from url (true) or from cat api</param>
        /// <returns>ModerationResult</returns>
        internal async Task<ModerationResult> moderateWUrl(string url, int userid, bool sendToUserid=false, BotUser curuser=null, bool fromUrl=false)
        {
            try
            {

                var urlImage = url;
                using (var client = new HttpClient())
                {
                    string json = "";
                    //System.Net.WebUtility.UrlEncode
                    string fileUrl = Uri.EscapeUriString(urlImage);
                    string wurl = watson_URL + string.Format("?api_key={0}&version={1}&url={2}", watson_apikey, watson_version, fileUrl);
                    MainParams.nlog.Trace("INCOMING url=" + urlImage);
                    //MainParams.nlog.Trace("WATSON url=" + wurl);
                    var response = await client.GetAsync(wurl);
                    json = await response.Content.ReadAsStringAsync();
                    ModerationResult res = new ModerationResult(json);

                    var modelClassifyResponse = await response.Content.ReadAsAsync<ClassifyResponse>();

                    foreach (var img in modelClassifyResponse.Images)
                    {
                        foreach (var score in img.Scores)
                        {
                            foreach (var cres in score.ClassResults)
                            {
                                res.AddClassScore(cres.ClassId, cres.Score, cres.TypeHierarchy);
                            }
                        }
                    }

                    MainParams.nlog.Trace("WATSON picturesCurrentDay;" + MainParams.picturesCurrentDay + "; JSON;" + res.ToStringNoNewline());
                    Console.WriteLine("Watson OK url=" + url);
                    MainParams.picturesCurrentDay++;
                    //parse url and process it to fileid
                    if (res.HasCatOrKittenClass)
                    {
                        Stream imageBytes = await client.GetStreamAsync(url);
                        Telegram.Bot.Types.FileToSend fs = new Telegram.Bot.Types.FileToSend();
                        fs.Content = imageBytes;
                        Uri uri = new Uri(url);
                        fs.Filename = Path.GetFileName(uri.AbsolutePath);
                        string fileid = "";
                        if (!sendToUserid)
                        {
                            //to create file id - send message to moderator
                            var mesToModerator = await MainParams.TGBot.SendPhotoAsync(MainParams.moderator_id, fs, "cat from url #" + MainParams.catFromUrl);
                            fileid = mesToModerator.Photo.Last().FileId;
                            MainParams.nlog.Trace("Mess to Moderator sent; url=" + url);
                        }
                        else
                        {
                            string ans = "";
                            if (fromUrl)
                            {
                                ans = strings.getCatFromUrl(curuser);
                            }
                            else
                            {
                                ans = strings.getCatFromApi(curuser);
                            }

                            var mesToUser = await MainParams.TGBot.SendPhotoAsync(curuser.chat_id, fs, ans);
                            fileid = mesToUser.Photo.Last().FileId;

                        }


                        MainParams.catFromUrl++;

                        //string tableid = MainParams.datebase.updateJson(fileid, res.ToStringNoNewline(), userid, res.maxCatScore);
                        
                        //add to cats table and delete from moderation

                        MainParams.datebase.ok_from_moderation_url(fileid, res.ToStringNoNewline(), userid, (float)res.maxCatScore, url);
                        fs.Content.Close();
                        imageBytes.Close();
                    }
                    else
                    {
                        //if no cats..send to user anyway and to manual moderation
                        if (sendToUserid)
                        {
                            Stream imageBytes = await client.GetStreamAsync(url);
                            Telegram.Bot.Types.FileToSend fs = new Telegram.Bot.Types.FileToSend();
                            fs.Content = imageBytes;
                            Uri uri = new Uri(url);
                            fs.Filename = Path.GetFileName(uri.AbsolutePath);
                            string ans = "";
                            if (fromUrl)
                            {
                                ans = strings.getCatFromUrl(curuser);
                            }
                            else
                            {
                                ans = strings.getCatFromApi(curuser);
                            }
                            var mesToUser = await MainParams.TGBot.SendPhotoAsync(curuser.chat_id, fs, ans);
                            var fileid = mesToUser.Photo.Last().FileId;
                            //add to manual moderation
                            MainParams.datebase.add_to_moderation(curuser.user_id, fileid);

                        }
                    }

                    return res;
                }
            }
            catch (Exception e)
            {
                MainParams.nlog.Debug(e);
                Console.WriteLine("Watson error " + e.Message);
            }
            return new ModerationResult("");
        }
        /// <summary>
        /// moderate first file id in queue file_ids
        /// </summary>
        /// <returns>json ans</returns>
        internal async Task<string> moderate()
        {
            //Console.WriteLine("moderate started");
            try
            {
                string jsonans = "";
                if (file_ids.Count() > 0)
                {
                    string json = "";
                    var fid = file_ids.First();
                    MemoryStream ms = new MemoryStream();
                    var file = await MainParams.TGBot.GetFileAsync(fid, ms);
                    //file.
                    //file.
                    //abstract: do post query to watson servers with file content and wait for json answer
                    using (var client = new HttpClient())
                    {


                        //var content = new FormUrlEncodedContent(values);
                        var requestContent = new MultipartFormDataContent();
                        //    here you can specify boundary if you need---^
                        var imageContent = new ByteArrayContent(ms.ToArray());//here we read filestream
                        imageContent.Headers.ContentType =
                            MediaTypeHeaderValue.Parse("image/jpeg");
                        requestContent.Add(imageContent, "image", "image.jpg");
                        var str = new StringContent(watson_apikey);
                        var version = new StringContent(watson_version);

                        //requestContent.Add(str, "api_key");
                        //requestContent.Add(version, "version");
                        //requestContent.Headers.Add("Accept-Language", "en");

                        //api key  - not to content.
                        string wurl = watson_URL + string.Format("?api_key={0}&version={1}", watson_apikey, watson_version);
                        var response = await client.PostAsync(wurl, requestContent);
                        //response.Content - json
                        json = await response.Content.ReadAsStringAsync();
                        MainParams.nlog.Trace("WATSON JSON;" + json);
                        Console.WriteLine("Watson OK id=" + fid);
                    }
                    file_ids.Remove(fid);
                    ms.Close();
                    ms.Dispose();
                    return json;
                }
                //return jsonans;
            }
            catch (Exception e)
            {
                MainParams.nlog.Debug(e);
                Console.WriteLine("Watson error " + e.Message);
            }
            return "false";
        }
    }
}
