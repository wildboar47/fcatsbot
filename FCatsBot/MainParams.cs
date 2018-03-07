using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;

using System.IO;
using System.Threading;
using System.Web.Http;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using System.Globalization;

namespace FCatsBot
{
    internal static class MainParams
    {

        internal static void FillCommands()
        {
            
            BotCommand start = new BotCommand();
            start.CommandDescription = "start chat with bot";
            start.CommandDescription_Ru = "начать чат с ботом";
            start.CommandName = "/start";
            MainParams.commands.Add(start);

            BotCommand help = new BotCommand();
            help.CommandDescription = "view commands list";
            help.CommandDescription_Ru = "посмотреть список команд бота";
            help.CommandName = "/help";
            MainParams.commands.Add(help);

            BotCommand changelanguage_en = new BotCommand();
            changelanguage_en.CommandDescription = "change bot language to English";
            changelanguage_en.CommandDescription_Ru = "сменить язык бота на английский";
            changelanguage_en.CommandName = "/changelanguage_en";
            MainParams.commands.Add(changelanguage_en);

            BotCommand changelanguage_far = new BotCommand();
            changelanguage_far.CommandDescription = "زبان فارسی";
            changelanguage_far.CommandDescription_Ru = "сменить язык бота на фарси";
            changelanguage_far.CommandName = "/changelanguage_far";
            MainParams.commands.Add(changelanguage_far);

            BotCommand changelanguage_ru = new BotCommand();
            changelanguage_ru.CommandDescription = "change bot language to Russian";
            changelanguage_ru.CommandDescription_Ru = "сменить язык бота на русский";
            changelanguage_ru.CommandName = "/changelanguage_ru";
            MainParams.commands.Add(changelanguage_ru);

            BotCommand whoisyourdaddy = new BotCommand();
            whoisyourdaddy.CommandDescription = "info about bot author";
            whoisyourdaddy.CommandDescription_Ru = "об авторе бота";
            whoisyourdaddy.CommandName = "/whoisyourdaddy";
            MainParams.commands.Add(whoisyourdaddy);

            BotCommand donate = new BotCommand();
            donate.CommandDescription = "donate bot author";
            donate.CommandDescription_Ru = "автору бота на пиво";
            donate.CommandName = "/donate";
            MainParams.commands.Add(donate);

            //vote
            BotCommand vote = new BotCommand();
            vote.CommandDescription = "vote for cats!";
            vote.CommandDescription_Ru = "голосуй за котиков!";
            vote.CommandName = "/vote";
            MainParams.commands.Add(vote);
            BotCommand catapi = new BotCommand();
            catapi.CommandDescription = "get cat picture from thecatapi.com and send it to moderation";
            catapi.CommandDescription_Ru = "получить картинку с котиком с thecatapi.com и отправить её на модерацию";
            catapi.CommandName = "/catapi";
            MainParams.commands.Add(catapi);

            BotCommand instructions = new BotCommand();
            instructions.CommandDescription = "bot instructions";
            instructions.CommandDescription_Ru = "инструкции к боту";
            instructions.CommandName = "/instructions";
            MainParams.commands.Add(instructions);

            

            //test commands
            //MainParams.commands.Add("inline");

        }
        internal static string catApiKey = "MTE0Nzc0";
        internal static int catFromUrl = 0;
        /// <summary>
        /// time of previous Post Update. to check if day changed and need reset of picturesCurrentDay
        /// </summary>
        internal static DateTime dtPreviousUpdate;
        internal static WatsonModeration WatsonModeration = new WatsonModeration();
        internal static WGoogleModeration GoogleModeration = new WGoogleModeration();
        internal static Random rnd = new Random();
        internal static TelegramBotClient TGBot;
        internal static Logger nlog;
        internal static int moderator_id= 101320350;
        /// <summary>
        /// received message ids
        /// </summary>
        internal static List<string> receivedMesIds = new List<string>();
        /// <summary>
        /// received update ids
        /// </summary>
        internal static List<string> receivedUpdIds = new List<string>();
        /// <summary>
        /// received message ids Inline
        /// </summary>
        internal static List<string> receivedMesIdsInline = new List<string>();
        /// <summary>
        /// received update ids Inline
        /// </summary>
        internal static List<string> receivedUpdIdsInline = new List<string>();
        /// <summary>
        /// processed messages ids Inline
        /// </summary>
        internal static List<string> processedMesIdsInline = new List<string>();
        /// <summary>
        /// processed messages ids
        /// </summary>
        internal static List<string> processedMesIds = new List<string>();
        /// <summary>
        /// Botan for Telegram
        /// https://github.com/botanio/sdk#sdk-usage
        /// </summary>
        internal static string botanio_key = "JqtDCIHWWdrSMPPayx2Qwdr5WyC:o8Z7";
        /// <summary>
        /// bot token
        /// </summary>
        internal static string bot_token = "232067264:AAGD1hmlGAKFry3awke7ybyGPhHVOJvK1-g";
        /// <summary>
        /// offset for updates
        /// </summary>
        internal static int offset = 0;
        /// <summary>
        /// pictures limit per day
        /// </summary>
        internal static int picturesLimit = 249;
        /// <summary>
        /// pictures sent to watson at current day
        /// </summary>
        internal static int picturesCurrentDay = 0;
        public static int picturesToModeration = 0;
        /// <summary>
        /// counter for inline messages
        /// </summary>
        internal static int counterInline = 0;
        /// <summary>
        /// processed updates (equal to offset must be)
        /// </summary>
        internal static int totalProcessedUpdates = 0;

        internal static BotDb datebase = new BotDb();

        /// <summary>
        /// bot commands
        /// </summary>
        internal static List<BotCommand> commands = new List<BotCommand>();



        internal static CultureInfo start_culture = CultureInfo.InvariantCulture;
        /// <summary>
        /// cashed users
        /// </summary>
        internal static List<BotUser> cashedUsers = new List<BotUser>();

        

        internal static bool checkFileSize(Telegram.Bot.Types.File file, Update update, BotUser curUser)
        {
            try
            {
                //return true if file bigger 20 MB
                if (file.FileSize > (1024 * 1024 * 20))
                    return true;
            }
            catch
            {
                return true;
            }
            return false;
        }
    }

    internal class BotCommand
    {
        /// <summary>
        /// name with slash, example "/start"
        /// </summary>
        internal string CommandName;
        /// <summary>
        /// description
        /// </summary>
        internal string CommandDescription;
        /// <summary>
        /// description in Russian (yes, blame me)
        /// </summary>
        internal string CommandDescription_Ru;

        public string ToString_En()
        {
            return CommandName + " - " + CommandDescription;
        }
        public string ToString_Ru()
        {
            return CommandName + " - " + CommandDescription_Ru;
        }

    }
    internal enum Langs
    {
        Ru,
        En,
        Farsi
    }
    /// <summary>
    /// bot user
    /// логика такая - чтоб не дергать базу. 
    /// 1) при загрузке бота подтягиваем список зарегенных и их язык
    /// 2) если не зареган - добавляем сюда и в базу
    /// 3) проверка зареган ли юзер - через этот класс чтоб не дергать базу
    /// 4) проверка языка - тоже тут
    /// 5) при смене языка - меняем тут и в базе
    /// </summary>
    internal class BotUser
    {
        internal DateTimeOffset when_user_registered;
        /// <summary>
        /// user id
        /// </summary>
        internal int user_id;
        /// <summary>
        /// chat id
        /// </summary>
        internal long chat_id;
        /// <summary>
        /// optional!!!
        /// </summary>
        internal string userName = "";
        /// <summary>
        /// Required! so must be filled
        /// </summary>
        internal string first_name = "";
        /// <summary>
        /// Required! so must be filled
        /// </summary>
        internal string last_name = "";
        /// <summary>
        /// list of file ids of uploaded photos. 
        /// (to send using SendPhoto to other users)
        /// </summary>
        internal List<BotUserCatPhoto> PhotosIds = new List<BotUserCatPhoto>();


        internal Langs current_Lang;
        internal CultureInfo user_culture = CultureInfo.InvariantCulture;

        public override string ToString()
        {
            return "user_id="+ user_id+ ";userName="+ userName+ ";first_name="+ first_name+ ";last_name=" + last_name+"; chatid="+chat_id;
        }
    }
    /// <summary>
    /// класс для фотки кота на модерации
    /// </summary>
    public class BotModerationCatPhoto
    {
        /// <summary>
        /// procent that this is cat by Watson (less 80% for moderation)
        /// </summary>
        internal double watson_percentage;
        /// <summary>
        /// procent of trashhold (more 50%)
        /// </summary>
        internal double watson_trashhold;
        /// <summary>
        /// file id on Telegram servers
        /// </summary>
        internal string file_id;
        /// <summary>
        /// user - owner of this photo (who send it)
        /// </summary>
        internal int user_id;
        /// <summary>
        /// watson json answer
        /// </summary>
        internal string watson_jsonAnswer;
    }
    /// <summary>
    /// класс для фотки кота от юзера, которая прошла модерацию
    /// </summary>
    public class BotUserCatPhoto
    {
        /// <summary>
        /// watson json answer
        /// </summary>
        internal string watson_jsonAnswer;
        /// <summary>
        /// procent that this is cat by Watson (more 80%)
        /// </summary>
        internal double watson_percentage;
        /// <summary>
        /// if it was approved manually, not by Watson
        /// </summary>
        internal bool approved_manually = false;
        /// <summary>
        /// file id on Telegram servers
        /// </summary>
        internal string file_id;
        /// <summary>
        /// user - owner of this photo (who send it)
        /// </summary>
        internal int user_id;
        /// <summary>
        /// likes
        /// </summary>
        internal long likes_count;
        /// <summary>
        /// dislikes
        /// </summary>
        internal long dislikes_count;
        /// <summary>
        /// views (shows to users)
        /// </summary>
        internal long views_count;
        public BotUserCatPhoto(string fileid, int userid)
        {
            watson_percentage = 0;
            approved_manually = true;
            file_id = fileid;
            user_id = userid;
            likes_count = 0;
            dislikes_count = 0;
            views_count = 0;
        }
        public BotUserCatPhoto(string fileid, int userid, double watson_procent)
        {
            watson_percentage = watson_procent;
            approved_manually = false;
            file_id = fileid;
            user_id = userid;
            likes_count = 0;
            dislikes_count = 0;
            views_count = 0;
        }
    }
}
