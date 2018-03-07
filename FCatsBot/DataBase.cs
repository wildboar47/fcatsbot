using FCatsBot.Strings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using FCatsBot.Entity;
namespace FCatsBot
{
    internal class moderate
    {
        internal string fileid;
        internal int id;
        internal int count;
        internal moderate()
        {
            fileid = "";
            count = 0;
            id = 0;
        }
    }
    internal class CatForVote
    {
        /// <summary>
        /// caption
        /// </summary>
        internal string caption;
        /// <summary>
        /// info if no cat was found (viewed everything)
        /// </summary>
        internal string infoNotFound;
        /// <summary>
        /// url, if have
        /// </summary>
        internal string from_url;
        internal string fileid;
        internal int id;
        internal long user_id;
        internal long views;
        internal long likes;
        internal long dislikes;
        internal CatForVote()
        {
            fileid = "";
            caption = "";
            infoNotFound = "";
            views = 0;
            id = 0;
            user_id = 0;
            likes = 0;
            dislikes = 0;
            from_url = "";
        }
    }
    internal class BotDb
    {
        Random rnd;
        internal BotDb()
        {
            rnd = new Random();
        }
        internal moderate getModeration()
        {
            moderate res = new moderate();
            using (FCatsBotEntities Context = new FCatsBotEntities())
            {

                res.count = Context.cats_on_moderation.Count();
                if (res.count > 0)
                {
                    //first or last are not working!!!
                    var catmaxid = Context.cats_on_moderation.Max(sel => sel.id);
                    var cat = Context.cats_on_moderation.Where(tcat => tcat.id == catmaxid).First();
                    res.fileid = cat.file_id;
                    res.id = cat.id;
                }

            }
            return res;
        }
        internal CatForVote getMostLikedCat(int userId)
        {

            CatForVote res = new CatForVote();
            BotUser cachedUsr = MainParams.cashedUsers.Find(user => user.user_id == userId);
             
            if (cachedUsr == null)
            {
                    cachedUsr = MainParams.datebase.FindUserInDb(userId);
                    MainParams.nlog.Debug("cachedUsr NULL in getMostLikedCat!!!userId=" + userId);
                    if (cachedUsr == null) cachedUsr = AddToCacheFail(userId);
            }

            try
            {
                using (FCatsBotEntities Context = new FCatsBotEntities())
                {
                    //02.09.2016 - search max liked not viewed cats!
                    var arr_cats_viewed_by_this_user = Context.cats_viewed_by_users.Where(catviewed => catviewed.user_id == userId).ToList();
                    List<int> arrviewed = new List<int>();
                    foreach (var item in arr_cats_viewed_by_this_user)
                    {
                        arrviewed.Add(item.cat_id);
                    }
                    var cats_not_viewed = Context.cats.Where(cat => arrviewed.Contains(cat.id) == false).ToList();
                    //int index = rnd.Next(0, cats_not_viewed.Count - 1);
                    var maxlikes = cats_not_viewed.Max(cts => cts.likes_count);
                    var found_cat = cats_not_viewed.Where(cts => cts.likes_count == maxlikes).First();
                    //add to cats_viewed
                    cats_viewed_by_users viewed = new cats_viewed_by_users();
                    viewed.cat_id = found_cat.id;
                    viewed.viewed = true;
                    viewed.user_id = userId;
                    viewed.liked = false;
                    viewed.disliked = false;
                    viewed.datetime_viewed = DateTime.Now;
                    viewed.file_id = found_cat.file_id;
                    Context.cats_viewed_by_users.Add(viewed);
                    var catmain = Context.cats.FirstOrDefault(catt => catt.id == found_cat.id);
                    catmain.views_count++;
                    Context.SaveChanges();
                    //fill result
                    res.fileid = found_cat.file_id.Trim();
                    res.likes = found_cat.likes_count;
                    res.dislikes = found_cat.dislikes_count;
                    res.views = found_cat.views_count;
                    res.id = found_cat.id;
                    res.user_id = userId;
                    res.from_url = found_cat.from_url;
                    //var user = Context.users
                    res.caption = string.Format(strings.getCatCaption(cachedUsr), found_cat.id) + " " + string.Format(strings.getViews(cachedUsr), found_cat.views_count);


                }
            }
            catch (Exception e)
            {
                MainParams.nlog.Debug("***NewLogs; DATABASE getMostLikedCat!!! ;EX=;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                MainParams.nlog.Debug(e);
            }
            return res;
        }

        internal CatForVote getRandomCat_ThatWasNotViewed(int userId)
        {
            CatForVote res = new CatForVote();
            var cachedUsr = MainParams.cashedUsers.Find(user => user.user_id == userId);
            
            if (cachedUsr == null)
            {
                cachedUsr = MainParams.datebase.FindUserInDb(userId);
                MainParams.nlog.Debug("cachedUsr NULL in getRandomCat_ThatWasNotViewed!!!userId=" + userId);
                if (cachedUsr == null) cachedUsr = AddToCacheFail(userId);
            }
            try
            {
                using (FCatsBotEntities Context = new FCatsBotEntities())
                {
                    var arr_cats_viewed_by_this_user = Context.cats_viewed_by_users.Where(catviewed => catviewed.user_id == userId).ToList();
                    List<int> arrviewed = new List<int>();
                    foreach (var item in arr_cats_viewed_by_this_user)
                    {
                        arrviewed.Add(item.cat_id);
                    }
                    var cats_not_viewed = Context.cats.Where(cat => arrviewed.Contains(cat.id) == false).ToList();
                    if (cats_not_viewed.Count() > 0)
                    {
                        int index = rnd.Next(0, cats_not_viewed.Count - 1);
                        var found_cat = cats_not_viewed[index];
                        //add to cats_viewed
                        cats_viewed_by_users viewed = new cats_viewed_by_users();
                        viewed.cat_id = found_cat.id;
                        viewed.viewed = true;
                        viewed.user_id = found_cat.user_id;
                        viewed.liked = false;
                        viewed.disliked = false;
                        viewed.datetime_viewed = DateTime.Now;
                        viewed.file_id = found_cat.file_id;
                        Context.cats_viewed_by_users.Add(viewed);
                        var catmain = Context.cats.FirstOrDefault(catt => catt.id == found_cat.id);
                        catmain.views_count++;
                        Context.SaveChanges();
                        //fill result
                        res.fileid = found_cat.file_id.Trim();
                        res.likes = found_cat.likes_count;
                        res.dislikes = found_cat.dislikes_count;
                        res.views = found_cat.views_count;
                        res.id = found_cat.id;
                        res.user_id = found_cat.user_id;
                        res.from_url = found_cat.from_url;
                        //var user = Context.users
                        //res.caption = string.Format(strings.getCatCaption(cachedUsr), found_cat.id) + " " + string.Format(strings.getViews(cachedUsr), found_cat.views_count);
                        res.caption = strings.getCatCaption(cachedUsr).Replace("{0}", found_cat.id.ToString() ) + " " + 
                            strings.getViews(cachedUsr).Replace("{0}", found_cat.views_count.ToString());

                    }
                    else
                    {
                        res.infoNotFound = strings.getNoCatsFound(cachedUsr);
                    }
                }
            }
            catch (Exception e)
            {
                MainParams.nlog.Debug("***NewLogs; DATABASE getRandomCat_ThatWasNotViewed!!! ;EX=;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                MainParams.nlog.Debug(e);
            }
            return res;
        }

        internal string updateJson(string fileid, string json, int userid, double wpercentage = 0)
        {

            string res = "";
            try
            {
                using (FCatsBotEntities Context = new FCatsBotEntities())
                {
                    var listOnModeration = Context.cats_on_moderation.Where(ct => ct.user_id == userid).ToList();
                    foreach (var item in listOnModeration)
                    {
                        if (item.file_id.Trim() == fileid)
                        {
                            //update
                            //var entry = item;
                            if (json.Length > 500) json = json.Substring(0, 499);
                            //json = json.Replace(" ", "");
                            //json = json.Replace(Environment.NewLine, " ");

                            item.watson_percentage = Convert.ToSingle(wpercentage);
                            item.watson_jsonanswer = json;
                            Context.SaveChanges();
                            res = item.id.ToString();
                            //entry.
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MainParams.nlog.Debug("***NewLogs; DATABASE updateJson!!! ;EX=;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
            }
            return res;
        }

        internal bool updateLikes(string data)
        {
            //data like "liked_cat_{0}_user_{1}" or "disliked_cat_{0}_user_{1}"
            try
            {
                string rr = "";
                bool liked = true;
                if (data.Contains("disliked"))
                {
                    rr = data.Replace("disliked_cat_", "");
                    liked = false;
                }
                else
                {
                    rr = data.Replace("liked_cat_", "");
                }
                rr = rr.Replace("_user_", "_");
                //rr = rr.Replace("___", "_");
                string scatid = rr.Substring(0, rr.IndexOf("_"));

                string suserid = rr.Substring(rr.IndexOf("_"));

                scatid = scatid.Replace("_", "");
                suserid = suserid.Replace("_", "");
                MainParams.nlog.Trace("updateLikes scatid=" + scatid + "; suserid=" + suserid);
                int catid = Convert.ToInt32(scatid);
                long userid = Convert.ToInt64(suserid);
                using (FCatsBotEntities Context = new FCatsBotEntities())
                {
                    //cats viewed
                    var cv = Context.cats_viewed_by_users.FirstOrDefault(cvb => cvb.cat_id == catid && cvb.user_id == userid);

                    var catmain = Context.cats.FirstOrDefault(catt => catt.id == catid);
                    try
                    {
                        if (catmain == null)
                        {
                            MainParams.nlog.Debug("updateLikes CatMain=null; catid=" + catid + ";userId=" + userid);
                        }
                        if (cv == null)
                        {
                            MainParams.nlog.Debug("updateLikes catViewed=null (NOT error); catid=" + catid + ";userId=" + userid);
                            //add 
                            //add to cats_viewed by user
                            cats_viewed_by_users viewed = new cats_viewed_by_users();
                            viewed.cat_id = catmain.id;
                            viewed.viewed = true;
                            viewed.user_id = catmain.user_id;
                            viewed.liked = liked;
                            viewed.disliked = !liked;
                            viewed.datetime_viewed = DateTime.Now;
                            viewed.file_id = catmain.file_id;
                            Context.cats_viewed_by_users.Add(viewed);
                        }
                        else
                        {
                            //if cat was viewed, update likes
                            //voted if like or dislike is true
                            if (cv.liked || cv.disliked)
                            {
                                return false;
                            }
                            cv.liked = liked;
                            cv.disliked = !liked;
                        }
                    }
                    catch (Exception e)
                    {
                        MainParams.nlog.Debug("***NewLogs; DATABASE updatelikes fff!!! ;EX=;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                        MainParams.nlog.Debug(e);
                    }
                    try
                    {
                        if (liked)
                            catmain.likes_count++;
                        else
                            catmain.dislikes_count++;
                    }
                    catch (Exception e)
                    {
                        MainParams.nlog.Debug("***NewLogs; DATABASE updatelikes CatMain!!! ;EX=;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                    }
                    Context.SaveChanges();
                    //catmain.views_count++
                }
            }
            catch (Exception e)
            {
                MainParams.nlog.Debug("***NewLogs; DATABASE updatelikes General!!! ;EX=;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                MainParams.nlog.Debug(e);
            }
            return true;
        }
        /// <summary>
        /// add to manual moderation
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="fileid"></param>
        internal void add_to_moderation(long userid, string fileid, string fromUrl = "")
        {
            try
            {
                using (FCatsBotEntities Context = new FCatsBotEntities())
                {
                    MainParams.picturesToModeration++;
                    cats_on_moderation newcat = new cats_on_moderation();
                    newcat.file_id = fileid;
                    newcat.from_url = fromUrl;
                    newcat.user_id = userid;
                    newcat.datetime_added = DateTime.Now;
                    newcat.watson_jsonanswer = "";
                    newcat.watson_percentage = 0;
                    newcat.watson_trashhold = 0;
                    Context.cats_on_moderation.Add(newcat);
                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MainParams.nlog.Debug("***NewLogs; DATABASE add_to_moderation!!! ;EX=;" + ex.Message + ";Source=" + ex.Source + ";stack=" + ex.StackTrace + ";e.inner=" + ex.InnerException);
                MainParams.nlog.Debug(ex);
            }
        }
        internal void delete_from_moderation(string tableid)
        {
            try
            {
                int tid = Convert.ToInt32(tableid);
                using (FCatsBotEntities Context = new FCatsBotEntities())
                {
                    var dcat = Context.cats_on_moderation.FirstOrDefault(cat => cat.id == tid);
                    Context.cats_on_moderation.Remove(dcat);
                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MainParams.nlog.Debug("***NewLogs; DATABASE delete_from_moderation!!! ;EX=;" + ex.Message + ";Source=" + ex.Source + ";stack=" + ex.StackTrace + ";e.inner=" + ex.InnerException);
                MainParams.nlog.Debug(ex);
            }
        }
        internal void ok_from_moderation(string tableid, bool approvedManually = true, string url = "")
        {
            try
            {
                int tid = Convert.ToInt32(tableid);
                using (FCatsBotEntities Context = new FCatsBotEntities())
                {
                    var CatOnMod = Context.cats_on_moderation.FirstOrDefault(mcat => mcat.id == tid);

                    cats cat = new cats();
                    cat.file_id = CatOnMod.file_id.Trim();
                    cat.user_id = CatOnMod.user_id;
                    cat.watson_jsonanswer = CatOnMod.watson_jsonanswer;
                    cat.watson_percentage = CatOnMod.watson_percentage;
                    cat.watson_trashhold = CatOnMod.watson_trashhold;
                    cat.views_count = 0;
                    cat.likes_count = 0;
                    cat.dislikes_count = 0;
                    cat.approved_manually = approvedManually;
                    cat.datetime_added = DateTime.Now;
                    cat.from_url = url;

                    Context.cats_on_moderation.Remove(CatOnMod);
                    Context.cats.Add(cat);
                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MainParams.nlog.Debug("***NewLogs; DATABASE ok_from_moderation!!! ;EX=;" + ex.Message + ";Source=" + ex.Source + ";stack=" + ex.StackTrace + ";e.inner=" + ex.InnerException);
                MainParams.nlog.Debug(ex);
            }
        }
        internal void ok_from_moderation_url(string fileid, string watson_jsonanswer, int userid, float watson_percentage, string url = "")
        {
            try
            {
                //int tid = Convert.ToInt32(tableid);
                using (FCatsBotEntities Context = new FCatsBotEntities())
                {

                    cats cat = new cats();
                    cat.file_id = fileid.Trim();
                    cat.user_id = userid;
                    cat.watson_jsonanswer = watson_jsonanswer;
                    cat.watson_percentage = watson_percentage;
                    cat.watson_trashhold = 0;
                    cat.views_count = 0;
                    cat.likes_count = 0;
                    cat.dislikes_count = 0;
                    cat.approved_manually = false;
                    cat.datetime_added = DateTime.Now;

                    cat.from_url = url;

                    Context.cats.Add(cat);
                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MainParams.nlog.Debug("***NewLogs; DATABASE ok_from_moderation_url!!! ;EX=;" + ex.Message + ";Source=" + ex.Source + ";stack=" + ex.StackTrace + ";e.inner=" + ex.InnerException);
                MainParams.nlog.Debug(ex);
            }
        }
        internal void change_User_Lang(Langs newlang, int userid)
        {
            try
            {
                //change cached
                var cachedUsr = MainParams.cashedUsers.Find(user => user.user_id == userid);
                if (cachedUsr==null) cachedUsr = MainParams.datebase.FindUserInDb(userid);

                cachedUsr.current_Lang = newlang;
                cachedUsr.user_culture = (newlang == Langs.En ? CultureInfo.InvariantCulture : 
                    (newlang == Langs.Farsi ? CultureInfo.GetCultureInfo("fa-IR") : CultureInfo.GetCultureInfo("ru-RU") ) );
                //change db
                using (FCatsBotEntities Context = new FCatsBotEntities())
                {
                    var dbUser = Context.users.FirstOrDefault(uss => uss.user_id == userid);

                    dbUser.user_lang_var = (newlang == Langs.En ? "en" : (newlang == Langs.Farsi ? "farsi": "ru") );
                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MainParams.nlog.Debug("***NewLogs; DATABASE change_User_Lang!!! ;EX=;" + ex.Message + ";Source=" + ex.Source + ";stack=" + ex.StackTrace + ";e.inner=" + ex.InnerException);
                MainParams.nlog.Debug(ex);
            }
        }
        /// <summary>
        /// fill users cache on startup
        /// </summary>
        internal void fillUsersOnStart()
        {
            try
            {
                using (FCatsBotEntities Context = new FCatsBotEntities())
                {
                    MainParams.nlog.Trace("USERS COUNT=" + Context.users.Count());
                    if (Context.users.Count() > 0)
                    {
                        //fill cache cycle
                        foreach (var user in Context.users)
                        {
                            BotUser usr = new BotUser();

                            usr.user_id = Convert.ToInt32(user.user_id);
                            usr.when_user_registered = user.datetime_user_registered;
                            //usr.user_id = user.GetValueOrDefault();
                            usr.userName = user.user_name_fix;
                            usr.chat_id = user.chat_id;
                            usr.first_name = user.user_first_name;

                            usr.current_Lang = (user.user_lang_var == "en" ? Langs.En : (user.user_lang_var == "farsi" ? Langs.Farsi :Langs.Ru));
                            usr.user_culture = (user.user_lang_var == "en" ? CultureInfo.InvariantCulture : 
                                (user.user_lang_var == "farsi" ? CultureInfo.GetCultureInfo("fa-IR") : CultureInfo.GetCultureInfo("ru-RU"))  );
                            MainParams.cashedUsers.Add(usr);
                        }
                        MainParams.nlog.Trace("Cache USERS OK");
                        try
                        {

                            var usList = Context.users.Where(usst => usst.chat_id < 0).ToList();
                            foreach (var user in usList)
                            {
                                if (string.IsNullOrEmpty(user.user_first_name)) user.user_first_name = "-";
                                if (user.user_first_name.Length>99) user.user_first_name = user.user_first_name.Substring(0, 99);

                                if (string.IsNullOrEmpty(user.user_last_name)) user.user_last_name = "-";
                                if (user.user_last_name.Length > 99) user.user_last_name = user.user_last_name.Substring(0, 99);


                                //replace chat id cycle
                                //MainParams.nlog.Trace("ERROR in fillUsersOnStart chat_id <=0; chat_id=" + user.chat_id + "; userId =" + user.user_id);
                                user.chat_id = user.user_id;
                            }
                            Context.SaveChanges();
                            MainParams.nlog.Trace("change chat id USERS OK");
                        }
                        //DbEntityValidationException: Validation failed for one or more entities. See 'EntityValidationErrors'
                        catch (System.Data.Entity.Validation.DbEntityValidationException e2)
                        {
                            MainParams.nlog.Debug("***NewLogs; DATABASE DbEntityValidationException");
                            foreach (var dberr in e2.EntityValidationErrors)
                            {
                                MainParams.nlog.Debug("entry id=" + ((users)dberr.Entry.Entity).id );
                                foreach (var entryError in dberr.ValidationErrors)
                                {
                                    MainParams.nlog.Debug("entryError=" + entryError.ErrorMessage+" prop="+entryError.PropertyName);
                                }                                
                            }
                            
                        }
                        catch (Exception ex)
                        {
                            MainParams.nlog.Debug("***NewLogs; DATABASE fillUsersOnStart REPLACE cycle ;EX=;" + ex.Message + ";Source=" + ex.Source + ";stack=" + ex.StackTrace + ";e.inner=" + ex.InnerException);
                            MainParams.nlog.Debug(ex);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MainParams.nlog.Debug("***NewLogs; DATABASE fillUsersOnStart!!! ;EX=;" + ex.Message + ";Source=" + ex.Source + ";stack=" + ex.StackTrace + ";e.inner=" + ex.InnerException);
                MainParams.nlog.Debug(ex);
            }
        }
        /// <summary>
        /// add user to cached
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="msg"></param>
        /// <param name="lang"></param>
        internal BotUser AddToCache(int userId, Message msg, string lang = "ru")
        {
            int usr_from_id = userId;
            var uname = msg.From.Username;
            //add to cache
            BotUser usr = new BotUser();
            usr.user_id = usr_from_id;
            usr.when_user_registered = DateTimeOffset.Now;
            //usr.user_id = user.GetValueOrDefault();
            usr.userName = uname;
            usr.chat_id = msg.Chat.Id;
            usr.first_name = msg.From.FirstName;
            usr.last_name = msg.From.LastName;
            usr.current_Lang = (lang == "en" ? Langs.En : (lang == "farsi" ? Langs.Farsi : Langs.Ru));
            usr.user_culture = (lang == "en" ? CultureInfo.InvariantCulture :
                (lang == "farsi" ? CultureInfo.GetCultureInfo("fa-IR") : CultureInfo.GetCultureInfo("ru-RU")));
            //usr.current_Lang = (lang == "en" ? Langs.En : Langs.Ru);
            //usr.user_culture = (lang == "en" ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo("ru-RU"));
            MainParams.cashedUsers.Add(usr);
            return usr;
        }
        internal BotUser AddToCacheFail(int userId, string lang = "ru")
        {
            int usr_from_id = userId;
            var uname = "@";
            //add to cache
            BotUser usr = new BotUser();
            usr.user_id = usr_from_id;
            usr.when_user_registered = DateTimeOffset.Now;
            //usr.user_id = user.GetValueOrDefault();
            usr.userName = uname;
            usr.chat_id = userId;
            usr.first_name = "";
            usr.last_name = "";
            usr.current_Lang = (lang == "en" ? Langs.En : (lang == "farsi" ? Langs.Farsi : Langs.Ru));
            usr.user_culture = (lang == "en" ? CultureInfo.InvariantCulture :
                (lang == "farsi" ? CultureInfo.GetCultureInfo("fa-IR") : CultureInfo.GetCultureInfo("ru-RU")));
            //usr.current_Lang = (lang == "en" ? Langs.En : Langs.Ru);
            //usr.user_culture = (lang == "en" ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo("ru-RU"));
            MainParams.cashedUsers.Add(usr);
            return usr;
        }
        /// <summary>
        /// register new user and add it to cache
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="username"></param>
        /// <param name="lang">en or ru</param>
        /// <returns></returns>
        internal bool registerUser(int userid, Message msg, string lang = "ru")
        {
            bool registerOk = false;
            try
            {
                int usr_from_id = userid;

                using (FCatsBotEntities Context = new FCatsBotEntities())
                {
                    //check if user exists
                    bool isUserExist = isUserRegistered(usr_from_id);// Context.users.Any(user => user.user_id == usr_from_id);

                    if (isUserExist)
                    {
                        //updateChatId_OldUsers(userid, msg);


                        //Console.WriteLine("User with id=" + usr_from_id + " EXIST!");
                        //MainParams.nlog.Trace("User with id=" + usr_from_id + " EXIST!");
                        //await Bot.SendTextMessageAsync(message.Chat.Id, "User with id=" + usr_from_id + " EXIST!", replyMarkup: new ReplyKeyboardHide());
                        //return false;
                    }
                    else
                    {
                        var uname = msg.From.Username;
                        //add to cache
                        BotUser usr = new BotUser();
                        usr.user_id = usr_from_id;
                        usr.when_user_registered = DateTimeOffset.Now;
                        //usr.user_id = user.GetValueOrDefault();
                        usr.userName = uname;
                        usr.chat_id = msg.Chat.Id;
                        usr.first_name = msg.From.FirstName;
                        usr.last_name = msg.From.LastName;
                        usr.current_Lang = (lang == "en" ? Langs.En : (lang == "farsi" ? Langs.Farsi : Langs.Ru));
                        usr.user_culture = (lang == "en" ? CultureInfo.InvariantCulture :
                            (lang == "farsi" ? CultureInfo.GetCultureInfo("fa-IR") : CultureInfo.GetCultureInfo("ru-RU")));
                        // usr.current_Lang = (lang == "en" ? Langs.En : Langs.Ru);
                        // usr.user_culture = (lang == "en" ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo("ru-RU"));
                        MainParams.cashedUsers.Add(usr);
                        //add to db
                        users newRecord = new users();
                        //newRecord.user_lang_var
                        newRecord.datetime_user_registered = DateTime.Now;
                        newRecord.user_id = usr_from_id;
                        newRecord.user_name_fix = "@" + uname;
                        newRecord.user_lang_var = lang;
                        newRecord.chat_id = msg.Chat.Id == 0 ? usr_from_id : msg.Chat.Id;
                        newRecord.user_first_name = msg.From.FirstName == null ? "-" : msg.From.FirstName;
                        newRecord.user_last_name = msg.From.LastName == null ? "-" : msg.From.LastName;

                        if (newRecord.user_last_name.Length > 99) newRecord.user_last_name=newRecord.user_last_name.Substring(0, 99);
                        if (newRecord.user_first_name.Length > 99) newRecord.user_first_name = newRecord.user_first_name.Substring(0, 99);

                        //newRecord.
                        //newRecord.
                        Context.users.Add(newRecord);
                        MainParams.nlog.Trace("User Username @" + uname + " Registered! userid=" + userid);
                        Context.SaveChanges();


                        registerOk = true;
                    }

                }
            }
            //catch (System.Data.Entity.Validation.DbEntityValidationException dbex)
            //{
            //    try
            //    {
            //        MainParams.nlog.Trace("***!!!!!!!!***********DbEntityValidationException ***********!!!!!!!!!!");
            //        foreach (var error in dbex.EntityValidationErrors)
            //        {
            //            foreach (var ver in error.ValidationErrors)
            //            {
            //                MainParams.nlog.Trace(ver.ErrorMessage + "; PropertyName=" + ver.PropertyName);
            //            }
            //            MainParams.nlog.Trace(error.Entry);
            //        }
            //    }
            //    catch (Exception exqw)
            //    { }
            //    MainParams.nlog.Debug(dbex);
            //}
            catch (Exception ex)
            {
                MainParams.nlog.Debug("***NewLogs; DATABASE registerUser!!! ;EX=;" + ex.Message + ";Source=" + ex.Source + ";stack=" + ex.StackTrace + ";e.inner=" + ex.InnerException);
                MainParams.nlog.Debug(ex);
            }
            return registerOk;
        }
        /// <summary>
        /// using cache!
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal bool isUserRegistered(int userId)
        {
            return MainParams.cashedUsers.Any(usr => usr.user_id == userId);
        }
        /// <summary>
        /// find user in db by user_id (if no in cache!)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal BotUser FindUserInDb(int userId)
        {
            BotUser usr = new BotUser();
            usr.current_Lang = Langs.En;
            usr.user_culture = CultureInfo.InvariantCulture;
            try
            {
                using (FCatsBotEntities Context = new FCatsBotEntities())
                {
                    var user = Context.users.Where(ussr => ussr.user_id == userId).FirstOrDefault();
                    if (user == null) return usr;


                    usr.user_id = Convert.ToInt32(user.user_id);
                    usr.when_user_registered = user.datetime_user_registered;
                    //usr.user_id = user.GetValueOrDefault();
                    usr.userName = user.user_name_fix;
                    usr.chat_id = user.chat_id;
                    usr.first_name = user.user_first_name;
                    var lang = user.user_lang_var;
                    usr.current_Lang = (lang == "en" ? Langs.En : (lang == "farsi" ? Langs.Farsi : Langs.Ru));
                    usr.user_culture = (lang == "en" ? CultureInfo.InvariantCulture :
                        (lang == "farsi" ? CultureInfo.GetCultureInfo("fa-IR") : CultureInfo.GetCultureInfo("ru-RU")));
                    //usr.current_Lang = (user.user_lang_var == "en" ? Langs.En : Langs.Ru);
                    //usr.user_culture = (user.user_lang_var == "en" ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo("ru-RU"));
                    MainParams.cashedUsers.Add(usr);
                }
            }
            catch (Exception ex)
            {
                MainParams.nlog.Debug("***NewLogs; FindUserInDb!!! ;EX=;" + ex.Message + ";Source=" + ex.Source + ";stack=" + ex.StackTrace + ";e.inner=" + ex.InnerException);
                MainParams.nlog.Debug(ex);
            }
            return usr;
        }


    }//botdb
}
