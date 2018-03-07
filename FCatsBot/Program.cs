using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;
using NLog;
using Telegram.Bot;
using Owin;
using Microsoft.Owin.Hosting;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using FCatsBot.Strings;
using System.Globalization;

namespace FCatsBot
{
    class Program
    {
        /// <summary>
        /// moskvaCarsharing bot test
        /// "212104571:AAFp94DuvmtP2kSZ28dVdb95v-9dp4Cz6oY" - moskvaCarsharing
        /// "200051211:AAEbvSPhsAGDXFwFpOQOZpUwgnsgACS8qoQ" - meganz
        /// "216226146:AAEF5sl1nQfEgersf92Wld6CVxpykO2FBPU" - fastdates
        /// "232067264:AAGD1hmlGAKFry3awke7ybyGPhHVOJvK1-g" - fcats
        /// </summary>
        private static readonly TelegramBotClient Bot = new TelegramBotClient("232067264:AAGD1hmlGAKFry3awke7ybyGPhHVOJvK1-g");
        
        static void Main(string[] args)
        {
            
            MainParams.bot_token = "232067264:AAGD1hmlGAKFry3awke7ybyGPhHVOJvK1-g";
            MainParams.nlog = LogManager.GetCurrentClassLogger();
            MainParams.FillCommands();
            MainParams.TGBot = Bot;              
            MainParams.offset = 0;
            MainParams.GoogleModeration = new WGoogleModeration();
            //get Watson token from file
            if (System.IO.File.Exists("watson.txt"))
            {
                StreamReader sr = System.IO.File.OpenText("watson.txt");
                string token = sr.ReadLine().Trim();
                sr.Close();
                sr.Dispose();
                if (token != MainParams.WatsonModeration.watson_apikey)
                {
                    MainParams.WatsonModeration.watson_apikey = token;
                    MainParams.nlog.Trace("watson key Changed");
                }
            }
            MainParams.dtPreviousUpdate = DateTime.Now;
            //strings.Culture = CultureInfo.GetCultureInfo("ru-RU");
            //OWIN
            //BotDb local = new BotDb();
            MainParams.datebase.fillUsersOnStart();
            Console.WriteLine("users cached!");
            using (WebApp.Start<Startup>("https://+:443/"))
            {
                // Register WebHook
                MainParams.TGBot.SetWebhook("https://fcatsbot.hramskuratovo.ru:443/WebHook").Wait();

                
                Console.WriteLine("Server FCatsBot Started at 443. PRESS ENTER TO EXIT!!!!");
                //WebHookController.
                
                //local.
                // Stop Server after <Enter>
                Console.ReadLine();

                // Unregister WebHook
                MainParams.TGBot.SetWebhook().Wait();
            }
            Console.WriteLine("Exiting");



            //Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            //Bot.OnMessage += BotOnMessageReceived;
            //Bot.OnMessageEdited += BotOnMessageReceived;
            //Bot.OnInlineQuery += BotOnInlineQueryReceived;
            //Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            //Bot.OnReceiveError += BotOnReceiveError;

            //var me = Bot.GetMeAsync().Result;

            //Console.Title = me.Username;

            //Bot.StartReceiving();
            //Console.WriteLine("Bot " + me.Username + " Started...");
            //Console.ReadLine();
            //Bot.StopReceiving();
        }
        public class Startup
        {
            public void Configuration(IAppBuilder app)
            {
                var configuration = new HttpConfiguration();

                configuration.Routes.MapHttpRoute("WebHook", "{controller}");

                app.UseWebApi(configuration);
            }
        }

//        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
//        {
//            Console.WriteLine(receiveErrorEventArgs.ApiRequestException.ToString());
//            Debugger.Break();
//        }

//        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
//        {
//            //seem not working...
//            Console.WriteLine($"Received choosen inline result id: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
//        }

//        private static async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
//        {
//            Console.WriteLine($"Received InlineQuery Query=: {inlineQueryEventArgs.InlineQuery.Query}");
//            InlineQueryResult[] results = {
//                new InlineQueryResultLocation
//                {
//                    Id = "1",
//                    Latitude = 40.7058316f, // displayed result
//                    Longitude = -74.2581888f,
//                    Title = "New York",
//                    InputMessageContent = new InputLocationMessageContent // message if result is selected
//                    {
//                        Latitude = 40.7058316f,
//                        Longitude = -74.2581888f,
//                    }
//                },

//                new InlineQueryResultLocation
//                {
//                    Id = "2",
//                    Longitude = 52.507629f, // displayed result
//                    Latitude = 13.1449577f,
//                    Title = "Berlin",
//                    InputMessageContent = new InputLocationMessageContent // message if result is selected
//                    {
//                        Longitude = 52.507629f,
//                        Latitude = 13.1449577f
//                    }
//                }
//            };

//            await Bot.AnswerInlineQueryAsync(inlineQueryEventArgs.InlineQuery.Id, results, isPersonal: true, cacheTime: 0);
//        }

//        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
//        {
//            var message = messageEventArgs.Message;

//            if (message == null)
//            {
//                return;
//            }
//            if (message.Type != MessageType.TextMessage)
//            {
//                Console.WriteLine("off Type=" + message.Type.ToString());
//                if (message.Type == MessageType.ContactMessage)
//                {
//                    Console.WriteLine("Contact PhoneNumber=" + message.Contact.PhoneNumber);
//                }
//                else if (message.Type == MessageType.LocationMessage)
//                {
//                    Console.WriteLine("LocationMessage Location lat/lon=" + message.Location.Latitude.ToString() + "  " + message.Location.Longitude.ToString());
//                }
//                return;
//            }
//            if (message.Text.StartsWith("/inline")) // send inline keyboard
//            {
//                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

//                var keyboard = new InlineKeyboardMarkup(new[]
//                {
//                    new[] // first row
//                    {
//                        new InlineKeyboardButton("1.1"),
//                        new InlineKeyboardButton("1.2"),
//                    },
//                    new[] // second row
//                    {
//                        new InlineKeyboardButton("2.1"),
//                        new InlineKeyboardButton("2.2"),
//                    }
//                });

//                await Task.Delay(500); // simulate longer running task

//                await Bot.SendTextMessageAsync(message.Chat.Id, "Choose",
//                    replyMarkup: keyboard);
//            }
//            else if (message.Text.StartsWith("/keyboard")) // send custom keyboard
//            {
//                var keyboard = new ReplyKeyboardMarkup(new[]
//                {
//                    new [] // first row
//                    {
//                        new KeyboardButton("1.1"),
//                        new KeyboardButton("1.2"),
//                    },
//                    new [] // last row
//                    {
//                        new KeyboardButton("2.1"),
//                        new KeyboardButton("2.2"),
//                    }
//                });

//                await Bot.SendTextMessageAsync(message.Chat.Id, "Choose",
//                    replyMarkup: keyboard);
//            }
//            else if (message.Text.StartsWith("/photo")) // send a photo
//            {
//                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

//                const string file = @"C:\koroteevIV\work\logoS.png";//TEST!

//                var fileName = file.Split('\\').Last();

//                using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
//                {
//                    var fts = new FileToSend(fileName, fileStream);

//                    await Bot.SendPhotoAsync(message.Chat.Id, fts, "Nice Picture");
//                }
//            }
//            else if (message.Text.StartsWith("/request")) // request location or contact
//            {
//                var keyboard = new ReplyKeyboardMarkup(new[]
//                {
//                    new KeyboardButton("Location")
//                    {
//                        RequestLocation = true
//                    },
//                    new KeyboardButton("Contact")
//                    {
//                        RequestContact = true
//                    },
//                });

//                await Bot.SendTextMessageAsync(message.Chat.Id, "Who or Where are you?", replyMarkup: keyboard);
//            }
//            else if (message.Text.StartsWith("/help"))
//            {

//                var usage = @"KOROTEEV RULES Usage:
///inline   - send inline keyboard
///keyboard - send custom keyboard
///photo    - send a photo
///create - insert db entry
///update - update db entry
///delete - delete db entry
///select - select db entry
///request  - request location or contact
//";

//                await Bot.SendTextMessageAsync(message.Chat.Id, usage,
//                    replyMarkup: new ReplyKeyboardHide());
//            }
//            else if (message.Text.StartsWith("/create"))
//            {
                
//                int usr_from_id = 0;

//                usr_from_id = message.From.Id;
//                using (FCatsBotEntities Context = new FCatsBotEntities())
//                {
//                    //check if user exists
//                    bool isUserExist = Context.users.Any(user => user.user_id == usr_from_id);

//                    if (isUserExist)
//                    {
//                        //already registered
//                        Console.WriteLine("User with id=" + usr_from_id + " EXIST!");
//                        await Bot.SendTextMessageAsync(message.Chat.Id, "User with id=" + usr_from_id + " EXIST!", replyMarkup: new ReplyKeyboardHide());
//                    }
//                    else
//                    {
//                        //ok, now id is unique
//                        users newRecord = new users();
//                        newRecord.datetime_user_registered = DateTimeOffset.Now;
//                        newRecord.user_id = usr_from_id;
//                        newRecord.user_name_fix = "@"+ message.From.Username;
//                        newRecord.user_lang_var = "ru_RU";
//                        //newRecord.
//                        Context.users.Add(newRecord);
//                        Context.SaveChanges();
//                        await Bot.SendTextMessageAsync(message.Chat.Id, "Create ok", replyMarkup: new ReplyKeyboardHide());
//                    }

//                }
//                // Console.WriteLine("Ok insert user_id=" + usr_from_id);


//            }
//            else if (message.Text.StartsWith("/select"))
//            {

//                StringBuilder sb = new StringBuilder();
//                sb.Append("All from users: ");
//                sb.Append(Environment.NewLine);
//                using (FCatsBotEntities Context = new FCatsBotEntities())
//                {
//                    foreach (var usr in Context.users)
//                    {
//                        sb.Append(usr.user_id);
//                        sb.Append(usr.user_name_fix);
//                        sb.Append(" registered: ");
//                        sb.Append(usr.datetime_user_registered.ToString());
//                        sb.Append(Environment.NewLine);
//                    }
//                }
                

//                await Bot.SendTextMessageAsync(message.Chat.Id, sb.ToString(), replyMarkup: new ReplyKeyboardHide());
//            }
//            else
//            {
//                Console.WriteLine("Type=" + message.Type.ToString());
//                await Bot.SendTextMessageAsync(message.Chat.Id, "can't process this");
//            }
//        }

//        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
//        {
//            //result for /inline command
//            Console.WriteLine($"Received CallbackQuery data {callbackQueryEventArgs.CallbackQuery.Data}");

//            await Bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id,
//                $"Received CallbackQuery data {callbackQueryEventArgs.CallbackQuery.Data}");
//        }
    }
}
