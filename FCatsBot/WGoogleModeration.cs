using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Vision.v1;
using Google.Apis.Vision.v1.Data;
using FCatsBot.Strings;
using Telegram.Bot.Types;

namespace FCatsBot
{
    internal class WGoogleModeration
    {
        private string _JsonPath = @"C:\BOTS\fcatsbot\json.json";
        internal WGoogleModeration()
        {
            service = CreateAuthorizedClient(_JsonPath);
        }
        public void newAuth()
        {
            service = CreateAuthorizedClient(_JsonPath);
        }
        private VisionService service;
        // [START authenticate]
        /// <summary>
        /// Creates an authorized Cloud Vision client service using Application 
        /// Default Credentials.
        /// </summary>
        /// <returns>an authorized Cloud Vision client.</returns>
        private VisionService CreateAuthorizedClient(string JsonPath)
        {
            GoogleCredential credential =
                GoogleCredential.FromStream(new FileStream(JsonPath, FileMode.Open));
            //GoogleCredential.GetApplicationDefaultAsync().Result;
            // Inject the Cloud Vision scopes
            if (credential.IsCreateScopedRequired)
            {
                credential = credential.CreateScoped(new[]
                {
                    VisionService.Scope.CloudPlatform
                });
            }
            var res = new VisionService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                GZipEnabled = false
            });

            return res;
        }
        // [END authenticate]
        // [START detect_labels]
        /// <summary>
        /// Detect labels for an image using the Cloud Vision API.
        /// </summary>
        /// <param name="vision">an authorized Cloud Vision client.</param>
        /// <param name="imageUrl">the URL where the image is stored.</param>
        /// <returns>a list of labels detected by the Vision API for the image.
        /// </returns>
        private async Task<IList<AnnotateImageResponse>> DetectLabels(
            VisionService vision, string imageUrl)
        {
            //Console.WriteLine("Detecting Labels...");
            MainParams.nlog.Trace("Detecting Labels in url=" + imageUrl);

            // Convert image to Base64 encoded for JSON ASCII text based request   
            MemoryStream ms = new MemoryStream();
            using (var client = new HttpClient())
            {
                Stream imageBytes = await client.GetStreamAsync(imageUrl);
                imageBytes.CopyTo(ms);
            }
            byte[] imageArray = ms.ToArray();//System.IO.File.ReadAllBytes(imagePath);
            string imageContent = Convert.ToBase64String(imageArray);
            // Post label detection request to the Vision API
            // [START construct_request]
            var responses = vision.Images.Annotate(
                new BatchAnnotateImagesRequest()
                {
                    Requests = new[] {
                    new AnnotateImageRequest() {
                        Features = new []
                        { new Feature()
                            { Type =
                          "LABEL_DETECTION"}
                            },

                        Image = new Image() { Content = imageContent }
                    }
               }
                }).Execute();
            ms.Dispose();
            return responses.Responses;
            // [END construct_request]
        }
        private async Task< IList<AnnotateImageResponse> > DetectLabelsPhoto(
            VisionService vision, PhotoSize imageUrl)
        {
            //Console.WriteLine("Detecting Labels...");
            MainParams.nlog.Trace("~~~~~~~~~******~~~Detecting Labels in PhotoSize");

            // Convert image to Base64 encoded for JSON ASCII text based request   
            MemoryStream ms = new MemoryStream();
            if (imageUrl == null) MainParams.nlog.Trace("imageUrl NULL");

            if (imageUrl.FileStream == null)
            {
                MainParams.nlog.Trace("imageUrl FileStream NULL");
                var file = await MainParams.TGBot.GetFileAsync(imageUrl.FileId);
                if (file.FileStream == null) MainParams.nlog.Trace("~~~~~~~~~~FileStream STILL NULL");
                file.FileStream.CopyTo(ms);

                MainParams.nlog.Trace("imageUrl MemoryStream len="+ms.Length);
            }
            else
            {
                imageUrl.FileStream.CopyTo(ms);
            }

            byte[] imageArray = ms.ToArray();//System.IO.File.ReadAllBytes(imagePath);

            if (imageArray == null) MainParams.nlog.Trace("imageArray NULL");
            MainParams.nlog.Trace("imageArray len=" + imageArray.Length);

            string imageContent = Convert.ToBase64String(imageArray);
            if (imageContent == null) MainParams.nlog.Trace("imageContent NULL");
            // Post label detection request to the Vision API
            // [START construct_request]
            var responses = vision.Images.Annotate(
                new BatchAnnotateImagesRequest()
                {
                    Requests = new[] {
                    new AnnotateImageRequest() {
                        Features = new []
                        { new Feature()
                            { Type =
                          "LABEL_DETECTION"}
                            },

                        Image = new Image() { Content = imageContent }
                    }
               }
                }).Execute();
            ms.Dispose();
            MainParams.nlog.Trace("DetectLabelsPhoto Responses count=" + responses.Responses.Count);
            //no json....
            return responses.Responses;
            // [END construct_request]
        }
        // [END detect_labels]


        internal async Task<ModerationResult> doLabels(string inUrl)
        {
            string url = "";
            ModerationResult res = new ModerationResult("");
            try
            {
                // Create a new Cloud Vision client authorized via Application 
                // Default Credentials
                VisionService vision = service;
                if (vision == null) MainParams.nlog.Trace("!!!!!!! GOOGLE VISION NULL");
                // Use the client to get label annotations for the given image
                // [START parse_response]
                IList<AnnotateImageResponse> result = await DetectLabels(vision, inUrl);
                
                // Check if label annotations were found
                if (result != null)
                {

                    MainParams.nlog.Trace("Labels for image: " + inUrl);
                    // Loop through and output label annotations for the image
                    foreach (var response in result)
                    {
                        foreach (var label in response.LabelAnnotations)
                        {
                            double _score = label.Score == null ? 0 : Convert.ToDouble(label.Score.Value);
                            res.AddClassScore(label.Description.Trim(), _score, label.Mid);
                            //MainParams.nlog.Trace(label.Description + " (score:" + _score + ")");
                        }
                    }
                    res.json = res.ToStringNoNewline();
                    if (res.json.Length > 499) res.json = res.json.Substring(0, 499);
                    MainParams.nlog.Trace(res.ToStringNoNewline());
                }
                else
                {
                    MainParams.nlog.Trace("No labels found.");
                }
            }
            catch (Exception e)
            {
                MainParams.nlog.Debug("***NewLogs; GOOGLE doLabels!!! ;EX=;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                MainParams.nlog.Debug(e);
            }
            return res;
            // [END parse_response]
        }

        internal async Task<ModerationResult> doLabelsPhoto(Telegram.Bot.Types.PhotoSize inPhoto)
        {
            string url = "";
            ModerationResult res = new ModerationResult("");
            try
            {
                // Create a new Cloud Vision client authorized via Application 
                // Default Credentials
                VisionService vision = service;
                if (vision == null) MainParams.nlog.Trace("!!!!!!! GOOGLE VISION NULL");
                // Use the client to get label annotations for the given image
                // [START parse_response]
                IList<AnnotateImageResponse> result = await DetectLabelsPhoto(vision, inPhoto);

                // Check if label annotations were found
                if (result != null)
                {

                    MainParams.nlog.Trace("doLabelsPhoto Labels for inPhoto: " + inPhoto.FilePath+"; result count="+result.Count);
                    // Loop through and output label annotations for the image
                    int cr = 0;
                    string json = "";
                    foreach (var response in result)
                    {
                        MainParams.nlog.Trace("response count=" + cr);
                        if (response==null) MainParams.nlog.Trace("doLabelsPhoto response NULL count="+cr);
                        int cl = 0;
                        if (response.LabelAnnotations == null)
                        {
                            MainParams.nlog.Trace("doLabelsPhoto response LabelAnnotations  NULL count=" + cr);
                        }
                        
                        else
                        {
                            foreach (var label in response.LabelAnnotations)
                            {
                                if (label == null) MainParams.nlog.Trace("doLabelsPhoto label NULL count=" + cl);
                                if (label.Description == null) MainParams.nlog.Trace("doLabelsPhoto label Description NULL count=" + cl);
                                if (label.Mid == null) MainParams.nlog.Trace("doLabelsPhoto label Mid NULL count=" + cl);

                                double _score = label.Score.GetValueOrDefault(0);
                                res.AddClassScore(label.Description.Trim(), _score, label.Mid);

                                cl++;

                            }
                        }
                        cr++;
                    }
                    res.json = res.ToStringNoNewline();
                    if (res.json.Length > 499) res.json = res.json.Substring(0, 499);
                    MainParams.nlog.Trace(res.ToStringNoNewline());
                }
                else
                {
                    MainParams.nlog.Trace("No labels found.");
                }
            }
            catch (Exception e)
            {
                MainParams.nlog.Debug("***NewLogs; GOOGLE doLabelsPhoto!!! ;EX=;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                MainParams.nlog.Debug(e);
            }
            return res;
        }
            // [END parse_response]

            /// <summary>
            /// Google Cloud Vision API‎ api key
            /// </summary>
        internal string google_apikey = "59f9cd8fb292ded3e3410f198a692ba4ba337e96";

        /// <summary>
        /// google api url
        /// </summary>
        private string google_URL = "https://gateway-a.watsonplatform.net/visual-recognition/api/v3/classify";

        string oauth_id = "503265558363-01lnvq3arfj8nlm0t1j3um4qios05a24.apps.googleusercontent.com";
        string oauth_secret = "Ti6rCIfN69P9EidF4L8nEWUh";
        string oauth_url = "https://www.googleapis.com/auth/cloud-platform";

        private string google_example_URL = "https://watson-api-explorer.mybluemix.net/visual-recognition/api/v3/classify";
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
                var curUser = MainParams.cashedUsers.Find(usr => usr.user_id == userid);
                if (curUser == null) curUser = MainParams.datebase.FindUserInDb(userid);

                var file_path = file.FilePath;
                var urlImage = "https://api.telegram.org/file/bot" + MainParams.bot_token + "/" + file_path;
                var res = await moderateWUrl(urlImage, userid, true, curUser, true);

                return res;


            }
            catch (Exception e)
            {
                MainParams.nlog.Debug(e);
                Console.WriteLine("Google error moderateW " + e.Message);
            }
            return new ModerationResult("");
        }
        /// <summary>
        /// moderate defined file id
        /// </summary>
        /// <param name="fileid">file id</param>
        /// <param name="userid">user id</param>
        /// <returns>ModerationResult</returns>
        internal async Task<ModerationResult> moderateW_2(string fileid, int userid)
        {
            try
            {
                var file = await MainParams.TGBot.GetFileAsync(fileid);
                //MemoryStream ms = new MemoryStream();
                var curUser = MainParams.cashedUsers.Find(usr => usr.user_id == userid);
                if (curUser == null) curUser = MainParams.datebase.FindUserInDb(userid);

                var file_path = file.FilePath;
                var urlImage = "https://api.telegram.org/file/bot" + MainParams.bot_token + "/" + file_path;
                //var res = await moderateWUrl(urlImage, userid, true, curUser, true);
                try
                {




                    //to create new file id - send message to moderator
                    var mesToModerator = await MainParams.TGBot.SendPhotoAsync(MainParams.moderator_id, fileid, "cat from url =" + file_path);
                    var photo = mesToModerator.Photo.Last();
                    MainParams.nlog.Trace("~~~~~~~~~PHOTOSIZE len=" + photo.FileSize + " Width=" + photo.Width + " Height=" + photo.Height);
                    fileid = photo.FileId;
                    MainParams.nlog.Trace("Mess to Moderator sent; url old=" + urlImage);

                    string json = "";
                    var newPath = photo.FilePath;
                    var url2New = "https://api.telegram.org/file/bot" + MainParams.bot_token + "/" + newPath;
                    MainParams.nlog.Trace("Moderation 22 url=" + url2New);
                    ModerationResult res = new ModerationResult(json);
                    res = await doLabelsPhoto(photo);//full stream needed
                    MainParams.picturesCurrentDay++;

                    MainParams.catFromUrl++;
                    if (res.HasCatOrKittenClass)
                    {
                        //add to cats table
                        MainParams.datebase.ok_from_moderation_url(fileid, res.ToStringNoNewline(), userid, (float)res.maxCatScore, urlImage);
                    }
                    //string tableid = MainParams.datebase.updateJson(fileid, res.ToStringNoNewline(), userid, res.maxCatScore);

                    //add to cats table and delete from moderation

                    //MainParams.datebase.ok_from_moderation_url(fileid, res.ToStringNoNewline(), userid, (float)res.maxCatScore, url);



                    return res;

                }
                catch (Exception e)
                {
                    MainParams.nlog.Debug(e);
                    Console.WriteLine("Google error " + e.Message);
                }
                return new ModerationResult("");
                //return res;


            }
            catch (Exception e)
            {
                MainParams.nlog.Debug(e);
                Console.WriteLine("Google error moderateW " + e.Message);
            }
            return new ModerationResult("");
        }
        /// <summary>
        /// moderate jpg/png url.
        /// if has cat - process it to Telegram file id.
        /// </summary>
        /// <param name="url">jpg/png url</param>
        /// <param name="userid">user id</param>
        /// <param name="sendToUserid">send to user(true) or to moderator</param>
        /// <param name="curuser">user to send</param>
        /// <param name="fromUrl">we got pic from url (true) or from cat api (false)</param>
        /// <returns>ModerationResult</returns>
        internal async Task<ModerationResult> moderateWUrl(string url, int userid, bool sendToUserid = false, BotUser curuser = null, bool fromUrl = false)
        {
            try
            {

                var urlImage = url;
                using (var client = new HttpClient())
                {
                    string json = "";
                    MainParams.nlog.Trace("Moderation url=" + url);
                    ModerationResult res = new ModerationResult(json);
                    res = await doLabels(urlImage);
                    MainParams.picturesCurrentDay++;
                    //parse url and process it to fileid
                    if (res.HasCatOrKittenClass)
                    {
                        Stream imageBytes = await client.GetStreamAsync(url);
                        Telegram.Bot.Types.FileToSend fs = new Telegram.Bot.Types.FileToSend();
                        fs.Content = imageBytes;
                        Uri uri = new Uri(url);
                        fs.Filename = Path.GetFileName(uri.AbsolutePath);

                        if (!fs.Filename.EndsWith("jpg")) fs.Filename = fs.Filename + ".jpg";

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
                Console.WriteLine("Google error " + e.Message);
            }
            return new ModerationResult("");
        }
    }
}
