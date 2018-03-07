using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using System;
using FCatsBot.Strings;
using Telegram.Bot.Types.InputMessageContents;
using System.Net.Http;
using System.Collections.Generic;

namespace FCatsBot
{

    public class WebHookController : ApiController
    {

        private void ProcessFilePhoto(Telegram.Bot.Types.File file, Update update)
        {
            try
            {

                string caption = update.Message.Caption;
                if (update.Message.Caption == null) caption = file.FilePath;
                //Console.WriteLine("Received Photo: {0}; caption={2}; id={1}", file.FilePath, file.FileId, caption);

                MainParams.nlog.Trace(String.Format("Received Photo: FilePath {0}; caption={2}; id={1}; chatId={2}; MessageId={3};", file.FilePath, file.FileId, caption, update.Message.Chat.Id, update.Message.MessageId));

                //file.FileId
                var ext = file.FilePath.Split('.').Last();

                var filename = caption + "_" + DateTime.Now.ToString(dtFormat) + "." + ext;
                //stream
                var userid = update.Message.From.Id;
                var curUser = MainParams.cashedUsers.Find(usr => usr.user_id == userid);
                if (curUser == null) curUser = datebase.FindUserInDb(userid);
                //add to moderation
                datebase.add_to_moderation(update.Message.From.Id, file.FileId);
                //datebase.addUrl(update.Message.From.Id, update.Message.From.Username, uri, caption);
                var ans = strings.getSavedOnMega(curUser);

                if (userid == MainParams.moderator_id) ans += "  you can /watson_moderate this cat";
                MainParams.WatsonModeration.add(file.FileId.Trim());

                //var t = MainParams.TGBot.SendTextMessageAsync(update.Message.Chat.Id, ans);

                //MainParams.last_uri = uri;
                //AddUrlToTable(uri, update.Message.From.Id);
                //dispose stream...

            }
            catch (Exception e)
            {
                MainParams.nlog.Debug("ProcessFilePhoto;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
            }

        }
        /// <summary>
        /// send photo to moderator
        /// </summary>
        /// <param name="ChatId">chat id</param>
        /// <param name="MessageId">message id (optional)</param>
        private void ProcessModerate(long ChatId, int MessageId)
        {
            try
            {
                var res = datebase.getModeration();
                string fileid = res.fileid.Trim();
                MainParams.nlog.Trace("getModeration OK! fileid=" + fileid);
                StringBuilder sb = new StringBuilder();
                sb.Append("We have ");
                sb.Append(res.count);
                sb.Append(" cats on Manual moderation");
                sb.Append("; Pics this day to Watson: ");
                sb.Append(MainParams.picturesCurrentDay);
                sb.Append(Environment.NewLine);
                sb.Append("; Pics this day on Moderation: ");
                sb.Append(MainParams.picturesToModeration);
                sb.Append(Environment.NewLine);

                sb.Append("Cat #");
                sb.Append(res.id);
                sb.Append(Environment.NewLine);
                sb.Append("/mod_ok_");
                sb.Append(res.id);
                sb.Append(Environment.NewLine);
                sb.Append("/mod_shit_");
                sb.Append(res.id);
                sb.Append(Environment.NewLine);
                string ans = sb.ToString();
                //MainParams.nlog.Trace("ProcessModerate ans=" + ans);
                var t = MainParams.TGBot.SendPhoto(ChatId, fileid, ans);
            }
            catch (Exception e)
            {
                MainParams.nlog.Debug("ProcessModerate;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
            }
        }
        private void ProcessLocation(Update update)
        {
            try
            {
                Location loc = update.Message.Location;
                //Console.WriteLine("Location Lat={0}; Long={1};", update.Message.Location.Latitude, update.Message.Location.Longitude);
                MainParams.nlog.Trace(String.Format("Location Lat=;{0}; Long=;{1};", update.Message.Location.Latitude, update.Message.Location.Longitude));
                var curUser = MainParams.cashedUsers.Find(usr => usr.user_id == update.Message.From.Id);
                if (curUser == null) curUser = datebase.FindUserInDb(update.Message.From.Id);
                var sloc = strings.getLocation(curUser);
                var t = MainParams.TGBot.SendTextMessageAsync(update.Message.Chat.Id,
                    string.Format(sloc, update.Message.Location.Latitude, update.Message.Location.Longitude));
            }
            catch (Exception e)
            {
                MainParams.nlog.Debug("ProcessLocation;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
            }

        }
        BotDb datebase = MainParams.datebase;
        private ReplyKeyboardMarkup CAT_KEYS(BotUser curUser)
        {
           
            //var row1 = new KeyboardButton[2];
            //row1[0] = new KeyboardButton("\xD83D\xDE3A " + strings.getBtnVote(curUser));
            //row1[1] = new KeyboardButton(strings.getBtnMore(curUser));
            //var row2 = new KeyboardButton[2];
            //row2[0] = new KeyboardButton("⚙ " + strings.getBtnHelp(curUser));
            //row2[1] = new KeyboardButton(strings.getBtnDonate(curUser));
            //KeyboardButton[][] keyboard = new KeyboardButton[2][];

            //keyboard[0] = row1;
            //keyboard[1] = row2;
            return new ReplyKeyboardMarkup(new KeyboardButton[4]
              {
                new KeyboardButton("\xD83D\xDE3A " + strings.getBtnVote(curUser)),
                new KeyboardButton(strings.getBtnMore(curUser)),
                new KeyboardButton("⚙ " + strings.getBtnHelp(curUser)),
                  new KeyboardButton(strings.getBtnDonate(curUser))
              }, true, false);
            //return new ReplyKeyboardMarkup(keyboard, true, false);
        }
        static string dtFormat = "yyyy_MM_dd_HH_mm_ss_ffff";
        private async Task<Message> ProcessPhotoModeration(Update update, ModerationResult resModeration, BotUser curUser)
        {
            try
            {
                string anss = "";
                if (resModeration.HasCatOrKittenClass)
                {
                    //auto-moderation
                    anss = strings.getWatsonHasCat(curUser);
                }
                else
                {
                    //manual moderation
                    anss = strings.getWatsonNoCat(curUser);
                }
                string resmod = resModeration.ToString();
                if (string.IsNullOrEmpty(resmod)) resmod = " " + strings.getWatsonEmpty(curUser);
                anss += Environment.NewLine + resmod;
                var resMess = await MainParams.TGBot.SendTextMessageAsync(update.Message.Chat.Id, anss,false, false, 0, this.CAT_KEYS(curUser));
                return resMess;
            }
            catch (Exception e)
            {
                MainParams.nlog.Debug("ProcessPhotoModeration;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
            }
            return null;
        }


        /// <summary>
        /// test get method. usage: https://bott.hramskuratovo.ru:443/WebHook?value=3432
        /// </summary>
        /// <param name="value">value from get url</param>
        /// <returns>Ok</returns>
        [HttpGet]
        public IHttpActionResult Get([FromUri] string value)
        {
            var message = value;

            Console.WriteLine("Received GET Message type {0} ToString={1} ", message.GetType().ToString(), message.ToString());

            return Ok();
        }

        [HttpPost]
        public async Task<IHttpActionResult> Post(Update update)
        {
            if (MainParams.totalProcessedUpdates == 0)
            {
                var me = await MainParams.TGBot.GetMe();
                Console.WriteLine("Hello my BOT name is {0}; dir={1};", me.Username, "PostgreSql");
                MainParams.nlog.Debug(String.Format("Hello my BOT name is {0}; dir for Files={1}; ", me.Username, "PostgreSql"));
            }

            var message = update.Message;
            var Bot = MainParams.TGBot;
            try
            {

                var diff = DateTime.Now;
                if (diff.Day != MainParams.dtPreviousUpdate.Day)
                {
                    //day changed! reset picturesCurrentDay
                    MainParams.picturesCurrentDay = 0;
                    MainParams.picturesToModeration = 0;
                    MainParams.GoogleModeration.newAuth();
                }
                MainParams.dtPreviousUpdate = DateTime.Now;

                if (update.InlineQuery != null)
                {

                    //var chatId = update.InlineQuery.From
                    //***INLINE
                    Console.WriteLine("InlineQuery received: " + update.InlineQuery.Query);
                    //var t = await MainParams.TGBot.SendTextMessageAsync(chatId, "Ok, we started. Send me file and I will upload it to mega.nz");
                    try
                    {
                        MainParams.nlog.Trace(String.Format("Received inline query #{0}  query={1} id={2} counter={3}", MainParams.totalProcessedUpdates,
                         update.InlineQuery.Query, update.InlineQuery.Id, MainParams.counterInline));
                        //process inline
                        InlineQuery query = update.InlineQuery;
                        var InlineId = query.Id;
                        var userQuery = query.Query;
                        //from inline answer
                        var from = update.InlineQuery.From.Id;
                        var curUser = MainParams.cashedUsers.Find(usr => usr.user_id == from);
                        if (curUser == null) curUser = datebase.FindUserInDb(from);
                        var caturl = datebase.getMostLikedCat(from);
                        if (!string.IsNullOrEmpty(caturl.caption))
                        {
                            InlineQueryResultCachedPhoto[] result =
                            {
                                new InlineQueryResultCachedPhoto
                                {
                                    Id="1",
                                    Description=strings.getYourLatestUrl(curUser),
                                    //ReplyMarkup=new InlineKeyboardMarkup(),
                                    Title = strings.getYourLatestUrl(curUser),
                                    FileId = caturl.fileid,
                                    Caption= caturl.caption
                                    //,                                
                                    //InputMessageContent = new InputTextMessageContent
                                    //{
                                    //    MessageText = strings.getInlineText(curUser)
                                    //}
                                }
                            };
                            //ans inline
                            await Bot.AnswerInlineQueryAsync(InlineId, result, isPersonal: true, cacheTime: 0);
                        }
                        else
                        {
                            var ans = string.Format(strings.getErrorFile(curUser), "InlineQueryResultCachedPhoto");
                            InlineQueryResult[] res =
                            {
                                new InlineQueryResultArticle {
                                    InputMessageContent = new InputTextMessageContent { MessageText = ans }
                                                             }
                            };
                            await Bot.AnswerInlineQueryAsync(InlineId, res, isPersonal: true, cacheTime: 0);
                        }
                        MainParams.counterInline++;
                    }
                    catch (Exception eInline)
                    {
                        Console.WriteLine("INLINE ex=" + eInline.Message + " ");
                        MainParams.nlog.Debug("eInline;" + eInline.Message + ";Source=" + eInline.Source + ";stack=" + eInline.StackTrace + ";e.inner=" + eInline.InnerException);

                        MainParams.nlog.Debug(eInline);
                        MainParams.nlog.Debug(eInline.StackTrace);
                        MainParams.nlog.Debug(eInline.Source);
                    }

                }
                else if (update.Message != null)
                {

                    //*****Messages
                    var userId = update.Message.From.Id;

                    var userName = update.Message.From.Username;
                    var messageType = update.Message.Type;
                    long chatId = update.Message.Chat.Id;
                    //update.Message.EntityValues;
                    Console.WriteLine("Received Message id=" + update.Message.MessageId + "; Type=" + update.Message.Type.ToString() + "; userID=" + userId);

                    MainParams.nlog.Trace("Received Message id=" + update.Message.MessageId + "; Type=" + update.Message.Type.ToString() + "; chatId=" + chatId + ";userId=" + userId);

                    //WARNING: бот может получать несколько раз один и тот же update. надо это исправить!
                    //register user
                    bool isRegistered = datebase.isUserRegistered(userId);
                    if (!isRegistered)
                    {
                        MainParams.nlog.Trace("USER WITH ID " + userId + " will be now Registered");
                        datebase.registerUser(userId, message);
                    }
                    BotUser curUser = MainParams.cashedUsers.Find(usr => usr.user_id == userId);
                    if (curUser == null) curUser = datebase.FindUserInDb(userId);
                    if (curUser == null)
                    {
                        MainParams.nlog.Debug("Received Message id=" + update.Message.MessageId + "; Type=" + update.Message.Type.ToString() + "; chatId=" + chatId + ";userId=" + userId);
                        MainParams.nlog.Debug("*******~~~~~~Cur user NULL~~~~~~~*********");
                        curUser = datebase.AddToCache(userId, update.Message);
                    }
                    if (messageType == MessageType.TextMessage)
                    {
                        var text = update.Message.Text;
                        MainParams.nlog.Trace("FROM;" + userId + ";NAME;" + userName + ";TEXT; " + text);


                        if (text.Contains("/start"))
                        {
                            try
                            {
                                //текст как у @TagvisorBot  (*жми на 📎 и выбери File/Photo)
                                //TODO: language choice thru inline keyboard
                                var instructs = string.Format(strings.getInstructions(curUser), "📎");
                                var start = strings.getGetHelp(curUser); //+ Environment.NewLine + instructs;
                                var t = await MainParams.TGBot.SendTextMessageAsync(chatId, strings.getStart(curUser), replyMarkup: this.CAT_KEYS(curUser));
                                var t2 = await MainParams.TGBot.SendTextMessageAsync(chatId, start, replyMarkup: this.CAT_KEYS(curUser));

                                //if (!isRegistered)
                                //{
                                    var ChangeLangkeyboard = new InlineKeyboardMarkup(new[]
                                       {
                                        new InlineKeyboardButton("English 🇬🇧","changelanguage_en"),
                                        new InlineKeyboardButton("زبان فارسی 🇮🇷","changelanguage_far"),
                                        new InlineKeyboardButton("Русский 🇷🇺","changelanguage_ru")

                                    }
                                    );
                                    string mess = "На каком языке поговорим?" + Environment.NewLine + "What language will we speak?" 
                                    + Environment.NewLine + "اين ربات مي تواند به زبان فارسي صحبت كند.";
                                    var tt = await MainParams.TGBot.SendTextMessageAsync(chatId, mess, replyMarkup: ChangeLangkeyboard );
                                //}
                            }
                            catch (Exception e)
                            {
                                MainParams.nlog.Debug("***NewLogs; Start command;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                            }

                        }
                        else if (text.Contains("/changelanguage"))
                        {
                            try
                            {
                                if (text.Contains("en") || text.Contains("_en"))
                                {

                                    datebase.change_User_Lang(Langs.En, userId);
                                }
                                else if (text.Contains("ru") || text.Contains("_ru"))
                                {

                                    datebase.change_User_Lang(Langs.Ru, userId);
                                }
                                else if (text.Contains("far") || text.Contains("_far"))
                                {

                                    datebase.change_User_Lang(Langs.Farsi, userId);
                                }
                                //change lang.
                                //send echo message to user
                                var t = await MainParams.TGBot.SendTextMessageAsync(chatId, strings.getLangChanged(curUser), replyMarkup: this.CAT_KEYS(curUser));
                            }
                            catch (Exception e)
                            {
                                MainParams.nlog.Debug("***NewLogs; ChangeLang command;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                            }
                        }
                        else if (text.Contains("/help") || text.Contains(strings.getBtnHelp(curUser)))
                        {
                            try
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.Append(strings.getAuthor(curUser));
                                sb.Append(Environment.NewLine);
                                sb.Append("  telegram: @Koroteev  ");
                                sb.Append(Environment.NewLine);
                                sb.Append(strings.getRateBot(curUser));
                                sb.Append(Environment.NewLine);
                                sb.Append(strings.getCommands(curUser));
                                sb.Append(Environment.NewLine);
                                //commands with description
                                foreach (var cmd in MainParams.commands)
                                {
                                    if (curUser.current_Lang == Langs.En || curUser.current_Lang == Langs.Farsi)
                                    {
                                        sb.Append(cmd.ToString_En());
                                    }
                                    else
                                    {
                                        sb.Append(cmd.ToString_Ru());
                                    }
                                    sb.Append(Environment.NewLine);
                                }
                                sb.Append(Environment.NewLine);
                                sb.Append(strings.getWhyMega(curUser));
                                sb.Append(Environment.NewLine);
                                var t = await MainParams.TGBot.SendTextMessageAsync(chatId, sb.ToString(), replyMarkup: new ReplyKeyboardHide());
                            }
                            catch (Exception e)
                            {
                                MainParams.nlog.Debug("***NewLogs; Help command;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                            }
                        }
                        else if (text.Contains("/vote") || text.Contains(strings.getBtnVote(curUser)))
                        {
                            try
                            {
                                //send cate and vote inline keyboard
                                //update table cats_viewed_by_users
                                var t = await ProcessVoteCat(userId, chatId, curUser);
                            }
                            catch (Exception e)
                            {
                                MainParams.nlog.Debug("***NewLogs; VOTE command;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                            }

                        }
                        else if (text.Contains("/donate") || text.Contains("اهدای کمک مالی") || text.Contains(strings.getBtnDonate(curUser)) )
                        {
                            try
                            {
                                string dmessage = "Donate to bot author at https://www.paypal.me/koroteew/5usd";
                                if (curUser.current_Lang == Langs.Farsi)
                                    dmessage = "اهدای کمک مالی https://www.paypal.me/koroteew/5usd";
                                if (curUser.current_Lang == Langs.Ru)
                                    dmessage = "Автору бота на пиво https://www.paypal.me/koroteew/200rur";

                                var t = await MainParams.TGBot.SendTextMessageAsync(chatId, dmessage, replyMarkup: this.CAT_KEYS(curUser));
                            }
                            catch (Exception e)
                            {
                                MainParams.nlog.Debug("***NewLogs; donate command;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                            }

                        }
                        else if (text.Contains("/instructions"))
                        {
                            try
                            {
                                //bot instructions
                                //REMARK: текст как у @TagvisorBot  (*жми на 📎 и выбери File/Photo)
                                var instructs = string.Format(strings.getInstructions(curUser), "📎");
                                instructs += Environment.NewLine;
                                instructs += strings.getRateBot(curUser);
                                instructs += Environment.NewLine;
                                instructs += strings.getWhyMega(curUser);
                                instructs += Environment.NewLine;
                                var t = await MainParams.TGBot.SendTextMessageAsync(chatId, instructs, replyMarkup: this.CAT_KEYS(curUser));
                            }
                            catch (Exception e)
                            {
                                MainParams.nlog.Debug("***NewLogs; instructions command;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                            }
                        }
                        else if (text.Contains("/whoisyourdaddy"))
                        {

                            try
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.Append(strings.getAuthor(curUser));//Invariant (user)... - from strings.resx
                                sb.Append(Environment.NewLine);
                                sb.Append("  telegram: @Koroteev  ");
                                sb.Append(Environment.NewLine);
                                sb.Append(strings.getRateBot(curUser));
                                sb.Append(Environment.NewLine);
                                //special thanks to section
                                sb.Append(strings.getSpecialThanks(curUser));
                                sb.Append(Environment.NewLine);
                                sb.Append("Robin Müller https://github.com/MrRoundRobin");
                                sb.Append(Environment.NewLine);
                                sb.Append("Aden Forshaw http://thecatapi.com/docs.html");
                                //icon
                                //sb.Append("Icon by http://www.flaticon.com/authors/darius-dan from http://www.flaticon.com/")
                                var t = await MainParams.TGBot.SendTextMessageAsync(chatId, sb.ToString(), replyMarkup: this.CAT_KEYS(curUser));
                            }
                            catch (Exception e)
                            {
                                MainParams.nlog.Debug("***NewLogs; whoisyourdaddy command;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                            }
                        }


                        //************************************************************************************************************

                        //****the cat api
                        else if (text.Contains("/catapi1") && userId == MainParams.moderator_id)
                        {
                            //single pic from cat api
                            string urlParse = await getCatApiPicture();
                            if (!string.IsNullOrEmpty(urlParse))
                            {
                                MainParams.nlog.Trace("URL to Process=" + urlParse);
                                var resMod = await MainParams.GoogleModeration.moderateWUrl(urlParse, userId, false, curUser);
                                var mess = await ProcessPhotoModeration(update, resMod, curUser);
                            }
                        }
                        else if (text.Contains("/catapi") || text.Contains(strings.getBtnMore(curUser)))
                        {
                            try
                            {
                                string urlParse = await getCatApiPicture();
                                if (!string.IsNullOrEmpty(urlParse))
                                {
                                    MainParams.nlog.Trace("URL catapi to Process=" + urlParse);
                                    var resMod = await MainParams.GoogleModeration.moderateWUrl(urlParse, userId, true, curUser);
                                    var mess = await ProcessPhotoModeration(update, resMod, curUser);
                                }
                            }
                            catch (Exception e)
                            {
                                MainParams.nlog.Debug("***NewLogs; CatApi command!!! ;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                            }
                        }
                        else if (text.Contains("/cat_api") && userId == MainParams.moderator_id)
                        {
                            //TODO: check what if we reach limit (ask 300 photos from cat api)
                            //for (int i = 0; i < 100; i++)
                            //{
                            //    string urlParse = await getCatApiPicture();
                            //    if (!string.IsNullOrEmpty(urlParse))
                            //    {
                            //        MainParams.nlog.Trace("catapi100 URL #" + i + " to Process=" + urlParse);
                            //        var resMod = await MainParams.WatsonModeration.moderateWUrl(urlParse, userId,false, curUser);
                            //        var mess = await ProcessPhotoModeration(update, resMod, curUser);
                            //    }
                            //}
                        }
                        //google mod
                        else if (text.Contains("/modg"))
                        {
                            if (userId == MainParams.moderator_id)
                            {
                                Dictionary<int, string> moderationdFids = new Dictionary<int, string>();
                                using (Entity.FCatsBotEntities conext = new Entity.FCatsBotEntities())
                                {
                                    foreach (var item in conext.cats_on_moderation)
                                    {
                                        moderationdFids.Add(item.id, item.file_id.Trim());
                                    }
                                }
                                int cc = 0;
                                foreach (var item in moderationdFids)
                                {
                                    var res = await MainParams.GoogleModeration.moderateW_2(item.Value, MainParams.moderator_id);
                                    if (res.HasCatOrKittenClass == false)
                                    {
                                        //if google found nothing, delete this shit
                                        datebase.delete_from_moderation(item.Key.ToString());
                                        await Bot.SendTextMessageAsync(MainParams.moderator_id, "cat fail, delete."+Environment.NewLine+"found:"+res.ToString(), replyMarkup: this.CAT_KEYS(curUser));
                                    }
                                    else
                                    {
                                        datebase.delete_from_moderation(item.Key.ToString());
                                        await Bot.SendTextMessageAsync(MainParams.moderator_id, "cat ok!" + Environment.NewLine + "found:" + res.ToString(), replyMarkup: this.CAT_KEYS(curUser));
                                    }
                                    cc++;
                                    if (cc == 10) break;
                                    //}
                                }
                                await Bot.SendTextMessageAsync(MainParams.moderator_id, "FINISHED 10", replyMarkup: this.CAT_KEYS(curUser));

                            }
                        }
                        //***MODERATION MANUAL
                        else if (text.Contains("/moderate"))
                        {
                            if (userId == MainParams.moderator_id)
                            {
                                //логика такая: присылаем first запись из таблицы cats_on_moderation
                                //и команды /mod_ok /mod_shit 
                                //если ок, добавляем в базу, если не ок - удаляем из таблицы модерации
                                ProcessModerate(message.Chat.Id, message.MessageId);
                            }
                            else
                            {
                                await Bot.SendTextMessageAsync(message.Chat.Id, "you are not moderator, cheater", replyMarkup: this.CAT_KEYS(curUser));
                            }

                        }
                        else if (text.Contains("/mod_shit_") && userId == MainParams.moderator_id)
                        {
                            var modid = message.Text.Replace("/mod_shit_", "");
                            datebase.delete_from_moderation(modid);
                            await Bot.SendTextMessageAsync(message.Chat.Id, "delete ok", replyMarkup: this.CAT_KEYS(curUser));

                        }
                        else if (text.Contains("/mod_ok_") && userId == MainParams.moderator_id)
                        {
                            var modid = message.Text.Replace("/mod_ok_", "");
                            datebase.ok_from_moderation(modid);
                            await Bot.SendTextMessageAsync(message.Chat.Id, "ok, added to cats database", replyMarkup: this.CAT_KEYS(curUser));
                        }
                        else if (text.Contains("/watson_moderate") && userId == MainParams.moderator_id)
                        {
                            var mod = await MainParams.WatsonModeration.moderate();
                            await Bot.SendTextMessageAsync(message.Chat.Id, "watson says: " + mod, replyMarkup: this.CAT_KEYS(curUser));
                        }
                        else if (text.Contains("http"))
                        {
                            try
                            {
                                //02/09/2016 хмм оказывается телеграм сам парсит фото из ссылок и на десктопе
                                //и на телефоне тоже...хоть это по-прежнему TextMessage
                                Uri uri = new Uri(text.Trim());
                                var Filename = Path.GetFileName(uri.AbsolutePath);
                                var ext = Path.GetExtension(Filename);
                                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                                {
                                    //try find photos...
                                    try
                                    {
                                        string photoCount = "";
                                        if (update.Message.Photo == null)
                                        {
                                            photoCount = "photo NULL";
                                        }
                                        else
                                        {
                                            photoCount = update.Message.Photo.Count().ToString();
                                        }
                                        string pinnedExist = (update.Message.PinnedMessage != null).ToString();
                                        if (update.Message.PinnedMessage != null) pinnedExist += " photos in pinned==null " + (update.Message.PinnedMessage.Photo == null).ToString();
                                        MainParams.nlog.Trace("photocount=" + photoCount + "; pinned=" + pinnedExist);
                                    }
                                    catch (Exception e)
                                    {



                                        MainParams.nlog.Debug(e);
                                    }
                                    //answer that mod is in process
                                    var ans = strings.getSavedOnMega(curUser);
                                    bool manualMod = false;
                                    if (MainParams.picturesCurrentDay > MainParams.picturesLimit)
                                    {
                                        ans = strings.getModManual(curUser);
                                        manualMod = true;
                                    }
                                    await MainParams.TGBot.SendTextMessageAsync(update.Message.Chat.Id, ans, replyMarkup: this.CAT_KEYS(curUser));

                                    //like http://cs8.pikabu.ru/post_img/2016/08/29/6/1472459084147535117.jpg
                                    //process url
                                    string urlParse = text.Trim();
                                    MainParams.nlog.Trace("URL to Process=" + urlParse);
                                    if (!manualMod)
                                    {
                                        var resMod = await MainParams.GoogleModeration.moderateWUrl(urlParse, userId, true, curUser, true);

                                        var mess = await ProcessPhotoModeration(update, resMod, curUser);
                                    }
                                    else
                                    {
                                        var fid = await MainParams.WatsonModeration.getFileIdFromUrl(uri.AbsolutePath);
                                        if (!string.IsNullOrEmpty(fid))
                                        {
                                            datebase.add_to_moderation(userId, fid, uri.AbsolutePath);
                                        }
                                    }

                                }
                                else
                                {
                                    //no jpg or png
                                    await Bot.SendTextMessageAsync(message.Chat.Id, strings.getUrlNoJpg(curUser), replyMarkup: this.CAT_KEYS(curUser));
                                }
                            }
                            catch (Exception e)
                            {
                                MainParams.nlog.Debug("***NewLogs; Http PIC moderation!!! ;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                                MainParams.nlog.Debug(e);
                                try
                                {
                                    await Bot.SendTextMessageAsync(message.Chat.Id, strings.getCannotProcessUrl(curUser), replyMarkup: this.CAT_KEYS(curUser));
                                }
                                catch { }
                            }
                        }
                        else
                        {
                            try
                            {
                                //if no commands were detected in text
                                await Bot.SendTextMessageAsync(message.Chat.Id, strings.getEcho(curUser) + " " + strings.getGetHelp(curUser), replyMarkup: this.CAT_KEYS(curUser));
                            }
                            catch (Exception e)
                            {

                                MainParams.nlog.Debug("***NewLogs; NO commands Detected!!! ;Text was=" + update.Message.Text + Environment.NewLine + ";EX=;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);

                            }
                        }


                    }//endif type==text
                    else if (messageType == MessageType.PhotoMessage)
                    {
                        try
                        {
                            //receive photo only one(

                            var Photo = update.Message.Photo.LastOrDefault();
                            var photoSize = Photo.FileSize;
                            var fid = Photo.FileId;

                            var sizeMb = photoSize / (1024 * 1024);
                            MainParams.nlog.Trace("FILE incoming PhotoMessage id =" + fid + " size=" + photoSize + " mb=" + sizeMb);
                            bool isFileBig = sizeMb > 20;//MainParams.checkFileSize(file, update, curUser);
                            if (!isFileBig)
                            {
                                var file = await Bot.GetFileAsync(fid);
                                ProcessFilePhoto(file, update);
                                var ans = strings.getSavedOnMega(curUser);
                                bool manualMod = false;
                                if (MainParams.picturesCurrentDay > MainParams.picturesLimit)
                                {
                                    ans = strings.getModManual(curUser);
                                    manualMod = true;
                                }
                                await MainParams.TGBot.SendTextMessageAsync(update.Message.Chat.Id, ans, replyMarkup: this.CAT_KEYS(curUser));

                                MainParams.nlog.Trace("FILE PhotoMessage PROCESSED id =" + file.FileId);
                                if (!manualMod)
                                {
                                    var resModeration = await MainParams.GoogleModeration.moderateW(fid, update.Message.From.Id);

                                    Console.WriteLine("Processed PhotoMessage id=" + update.Message.MessageId);

                                    var mess = await ProcessPhotoModeration(update, resModeration, curUser);
                                }
                                else
                                {
                                    datebase.add_to_moderation(update.Message.From.Id, fid, "manual PhotoMessage");
                                }

                            }
                            else
                            {
                                await MainParams.TGBot.SendTextMessageAsync(update.Message.Chat.Id, strings.getBigFile(curUser), replyMarkup: this.CAT_KEYS(curUser));
                            }
                        }
                        catch (Exception e)
                        {
                            MainParams.nlog.Debug("***NewLogs; PhotoMessage processing!!! ;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                            MainParams.nlog.Debug(e);
                            try
                            {
                                await MainParams.TGBot.SendTextMessageAsync(update.Message.Chat.Id, string.Format(strings.getErrorFile(curUser), "PhotoMessage"), replyMarkup: this.CAT_KEYS(curUser));
                            }
                            catch { }
                        }
                    }
                    else if (messageType == MessageType.DocumentMessage)
                    {
                        try
                        {
                            await MainParams.TGBot.SendTextMessageAsync(update.Message.Chat.Id, string.Format(strings.getTypeNotSupported(curUser), "<DocumentMessage>"), replyMarkup: this.CAT_KEYS(curUser));
                        }
                        catch (Exception e)
                        {

                            MainParams.nlog.Debug("***NewLogs; DocumentMessage!!! ;EX=;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);

                        }
                    }
                    else if (messageType == MessageType.VideoMessage)
                    {
                        try
                        {
                            await MainParams.TGBot.SendTextMessageAsync(update.Message.Chat.Id, string.Format(strings.getTypeNotSupported(curUser), "<VideoMessage>"), replyMarkup: this.CAT_KEYS(curUser));
                        }
                        
                        catch (Exception e)
                        {

                            MainParams.nlog.Debug("***NewLogs; VideoMessage!!! ;EX=;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);

                        }
                    }
                    else if (messageType == MessageType.AudioMessage)
                    {
                        try
                        {
                            await MainParams.TGBot.SendTextMessageAsync(update.Message.Chat.Id, string.Format(strings.getTypeNotSupported(curUser), "<AudioMessage>"), replyMarkup: this.CAT_KEYS(curUser));
                        }
                        catch (Exception e)
                        {

                            MainParams.nlog.Debug("***NewLogs; AudioMessage!!! ;EX=;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);

                        }
                    }
                    else if (messageType == MessageType.LocationMessage)
                    {
                        try
                        {
                            ProcessLocation(update);
                            Console.WriteLine("Processed DocumentMessage id=" + update.Message.MessageId);
                        }
                        catch (Exception e)
                        {
                            MainParams.nlog.Debug("***NewLogs; LocationMessage!!! ;EX=;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                            MainParams.nlog.Debug(e);
                            try
                            {
                                await MainParams.TGBot.SendTextMessageAsync(update.Message.Chat.Id, string.Format(strings.getTypeNotSupported(curUser), "Location"), replyMarkup: this.CAT_KEYS(curUser));
                            }
                            catch { }
                        }
                    }
                    else if (messageType == MessageType.ContactMessage)
                    {
                        try
                        {
                            MainParams.nlog.Trace("Contact;phone;" + message.Contact.PhoneNumber);
                            await Bot.SendChatActionAsync(chatId, ChatAction.Typing);
                            await Task.Delay(500);
                            var t = await Bot.SendTextMessageAsync(chatId, string.Format(strings.getTypeNotSupported(curUser), "ContactMessage"), replyMarkup: this.CAT_KEYS(curUser));
                            MainParams.nlog.Trace(String.Format("ContactMessage Type={0}; User={1}; mess={3}; phone={4};", update.Message.Type.ToString(), update.Message.From.Username, update.Message.ToString(), update.Message.Contact.PhoneNumber));
                        }
                        catch (Exception e)
                        {
                            MainParams.nlog.Debug(e);
                            MainParams.nlog.Debug("***NewLogs; ContactMessage!!! ;EX=;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                            try
                            {
                                await MainParams.TGBot.SendTextMessageAsync(update.Message.Chat.Id, string.Format(strings.getTypeNotSupported(curUser), "ContactMessage"), replyMarkup: this.CAT_KEYS(curUser));
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        //message!=null && other type && inline==null
                        try
                        {
                            Console.WriteLine("Strange MessageNotNull Type=" + update.Type);
                            if (update.Message.Type == MessageType.ServiceMessage)
                            {
                                MainParams.nlog.Trace("ServiceMessage;" + update.Message.ToString());
                            }
                            //callback1 - not here...
                            if (update.CallbackQuery != null)
                            {
                                Console.WriteLine("CallbackQuery data selected: " + update.CallbackQuery.Data);
                                await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Received CallbackQuery data " + update.CallbackQuery.Data);
                            }
                            else
                            {
                                await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, string.Format(strings.getTypeNotSupported(curUser), update.Type.ToString()));
                            }
                        }
                        catch (Exception e)
                        {
                            MainParams.nlog.Debug("***~~~***~~~***~~~***MessageNotNull;Received Message id=" + update.Message.MessageId + "; Type=" + update.Message.Type.ToString() + "; chatId=" + chatId + ";userId=" + userId);
                            MainParams.nlog.Debug("***NewLogs; MessageNotNull!!! ;EX=;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                        }
                    }
                }//endif message!=null
                else
                {
                    var updType = UpdateType.UnkownUpdate;
                    try
                    {
                        updType = update.Type;
                    }
                    catch (ArgumentOutOfRangeException EXX)
                    {
                        //do nothing
                        
                    }
                    
                    if (updType == UpdateType.CallbackQueryUpdate)
                    {
                        try
                        {
                            //callback2 - HERE!!!
                            if (update.CallbackQuery != null)
                            {
                                var userId = update.CallbackQuery.From.Id;
                                var curUser = MainParams.cashedUsers.Find(usr => usr.user_id == userId);
                                if (curUser == null) curUser = datebase.FindUserInDb(userId);
                                //liked_cat_{0}_user_{1}                            
                                var data = update.CallbackQuery.Data;
                                if (data.Contains("changelanguage"))
                                {
                                    if (data.Contains("_en"))
                                    {

                                        datebase.change_User_Lang(Langs.En, userId);
                                    }
                                    else if (data.Contains("_ru"))
                                    {

                                        datebase.change_User_Lang(Langs.Ru, userId);
                                    }
                                    else if (data.Contains("_far"))
                                    {

                                        datebase.change_User_Lang(Langs.Farsi, userId);
                                    }
                                    //change lang.
                                    //send echo message to user
                                    var t = await MainParams.TGBot.SendTextMessageAsync(userId, strings.getLangChanged(curUser),  replyMarkup: this.CAT_KEYS(curUser));
                                }
                                else
                                {
                                    if (data.Contains("next_cat"))
                                    {
                                        var t = await ProcessVoteCat(userId, curUser.chat_id, curUser);
                                    }
                                    else
                                    {
                                        bool wasliked = datebase.updateLikes(data);
                                        var origmesid = update.CallbackQuery.InlineMessageId;
                                        string ans = "ОК";
                                        if (!wasliked) ans = strings.getVoteFalse(curUser);
                                        //Console.WriteLine("CallbackQuery!!! data selected: " + data+" orig id="+ origmesid);

                                        await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, ans);
                                        //vote more
                                        await ProcessVoteCat(userId, curUser.chat_id, curUser);
                                        //await Bot.SendTextMessageAsync(userId, strings.getVoteMore(curUser) + " /vote");
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            MainParams.nlog.Debug("***~~~***~~~***~~~***CallbackQuery---!!!;Received Message id=" + update.Message.MessageId + "; Type=" + update.Message.Type.ToString() +";userId=" + update.CallbackQuery.From.Id);
                            MainParams.nlog.Debug("***NewLogs; CallbackQueryUpdate!!! ;EX=;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                        }
                    }
                    else
                    {
                        try
                        {
                            //if message==null && inline==null
                            Console.WriteLine("Strange!!! type=" + updType+"; message type="+message.Type);
                            await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Type not supported: " + updType.ToString() + "; message type=" + message.Type.ToString());
                        }
                        catch (Exception e)
                        {
                            MainParams.nlog.Debug("***NewLogs; strange type line 823;EX=;" + e.Message + ";Source=" + e.Source + ";stack=" + e.StackTrace + ";e.inner=" + e.InnerException);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("general post ex="+ex.Message);
                MainParams.nlog.Debug("***NewLogs; GENERAL POST EX=;" + ex.Message + ";Source=" + ex.Source + ";stack=" + ex.StackTrace + ";e.inner=" + ex.InnerException);

                MainParams.nlog.Debug(ex);
                MainParams.nlog.Debug(ex.StackTrace);
            }
            MainParams.totalProcessedUpdates++;
            return Ok();
        }//post end

        private async Task<Message> ProcessVoteCat(int userId, long chatId, BotUser curuser)
        {
            var votecat = datebase.getRandomCat_ThatWasNotViewed(userId);
            if (votecat != null && string.IsNullOrEmpty(votecat.caption) == false)
            {
                var votekeyboard = new InlineKeyboardMarkup(new[]
                    {
                                        new InlineKeyboardButton("😻",string.Format("liked_cat_{0}_user_{1}",votecat.id,userId)),
                                        new InlineKeyboardButton("😾",string.Format("disliked_cat_{0}_user_{1}",votecat.id,userId))
                                        //,new InlineKeyboardButton(strings.getNext(curuser)+ "➡️","next_cat")
                                        ,new InlineKeyboardButton("➡️","next_cat")
                                }
                );

                var t = await MainParams.TGBot.SendPhotoAsync(chatId, votecat.fileid, votecat.caption, replyMarkup: votekeyboard);
                return t;
            }
            else if (votecat == null)
            {
                MainParams.nlog.Debug("***NewLogs; VOTECAT NULL!!! ;userId=" + userId + ";chatId=" + chatId + ";curuser=" + curuser.ToString());
            }
            else if (!string.IsNullOrEmpty(votecat.infoNotFound))
            {
                var t = await MainParams.TGBot.SendTextMessageAsync(chatId, votecat.infoNotFound, replyMarkup: this.CAT_KEYS(curuser));
                return t;
            }
            return null;
        }

        private async Task<string> getCatApiPicture()
        {
            string url = "";
            using (var client = new HttpClient())
            {
                var urlcats = "http://thecatapi.com/api/images/get?format=src&results_per_page=1&type=jpg&api_key=" + MainParams.catApiKey;
                var result = await client.GetAsync(urlcats);
                url = result.RequestMessage.RequestUri.AbsoluteUri;
                MainParams.nlog.Trace("CATSAPI url=" + url);
            }
            return url;
        }
    }

}
