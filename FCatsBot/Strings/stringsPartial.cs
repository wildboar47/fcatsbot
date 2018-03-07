using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCatsBot.Strings
{
    internal partial class strings
    {
        internal static string getBtnVote(BotUser user)
        {
            // ISSUE: reference to a compiler-generated method
            return strings.ResourceManager.GetString("btnVote", strings.getUserCulture(user));
        }
        internal static string getBtnDonate(BotUser user)
        {
            // ISSUE: reference to a compiler-generated method
            return strings.ResourceManager.GetString("btnDonate", strings.getUserCulture(user));
        }
        internal static string getBtnMore(BotUser user)
        {
            // ISSUE: reference to a compiler-generated method
            return strings.ResourceManager.GetString("btnMore", strings.getUserCulture(user));
        }

        internal static string getBtnHelp(BotUser user)
        {
            // ISSUE: reference to a compiler-generated method
            return strings.ResourceManager.GetString("btnHelp", strings.getUserCulture(user));
        }

        private static CultureInfo getUserCulture(BotUser user)
        {
            //if there is no user...or culture is not filled.
            if (user == null || user.user_culture == null)
            {
                return CultureInfo.InvariantCulture;
            }
            return user.user_culture;
        }
        internal static string getAuthor(BotUser user)
        {
            return ResourceManager.GetString("Author", getUserCulture(user));

        }
        internal static string getSavedOnMega(BotUser user)
        {
             return ResourceManager.GetString("SavedOnMega", getUserCulture(user) );
            
        }
        internal static string getSavedPhoto(BotUser user)
        {
            return ResourceManager.GetString("SavedPhoto", getUserCulture(user));

        }
        internal static string getLocation(BotUser user)
        {
            return ResourceManager.GetString("Location", getUserCulture(user));

        }
        internal static string getYourLatestUrl(BotUser user)
        {
            return ResourceManager.GetString("YourLatestUrl", getUserCulture(user));

        }
        internal static string getInlineText(BotUser user)
        {
            return ResourceManager.GetString("InlineText", getUserCulture(user));

        }
        internal static string getInstructions(BotUser user)
        {
            return ResourceManager.GetString("Instructions", getUserCulture(user));

        }
        internal static string getStart(BotUser user)
        {
            return ResourceManager.GetString("Start", getUserCulture(user));

        }
        internal static string getLangChanged(BotUser user)
        {
            return ResourceManager.GetString("LangChanged", getUserCulture(user));

        }
        internal static string getCommands(BotUser user)
        {
            return ResourceManager.GetString("Commands", getUserCulture(user));

        }
        internal static string getEcho(BotUser user)
        {
            return ResourceManager.GetString("Echo", getUserCulture(user));

        }
        internal static string getContactMessage(BotUser user)
        {
            return ResourceManager.GetString("ContactMessage", getUserCulture(user));

        }
        internal static string getTypeNotSupported(BotUser user)
        {
            return ResourceManager.GetString("TypeNotSupported", getUserCulture(user));

        }
        internal static string getBigFile(BotUser user)
        {
            return ResourceManager.GetString("BigFile", getUserCulture(user));

        }
        internal static string getErrorFile(BotUser user)
        {
            return ResourceManager.GetString("ErrorFile", getUserCulture(user));

        }
        internal static string getYouHaveUrls(BotUser user)
        {
            return ResourceManager.GetString("YouHaveUrls", getUserCulture(user));

        }
        internal static string getNoUrls(BotUser user)
        {
            return ResourceManager.GetString("NoUrls", getUserCulture(user));

        }
        internal static string getRateBot(BotUser user)
        {
            return ResourceManager.GetString("RateBot", getUserCulture(user));

        }
        internal static string getWhyMega(BotUser user)
        {
            return ResourceManager.GetString("WhyMega", getUserCulture(user));

        }
        internal static string getGetHelp(BotUser user)
        {
            return ResourceManager.GetString("GetHelp", getUserCulture(user));


        }
        internal static string getNoCatsFound(BotUser user)
        {
            return ResourceManager.GetString("NoCatsFound", getUserCulture(user));

        }
        internal static string getCatCaption(BotUser user)
        {
            return ResourceManager.GetString("CatCaption", getUserCulture(user));

        }
        internal static string getViews(BotUser user)
        {
            return ResourceManager.GetString("Views", getUserCulture(user));

        }
        internal static string getVoteFalse(BotUser user)
        {
            return ResourceManager.GetString("VoteFalse", getUserCulture(user));

        }
        internal static string getCannotProcessUrl(BotUser user)
        {
            return ResourceManager.GetString("CannotProcessUrl", getUserCulture(user));

        }
        internal static string getVoteMore(BotUser user)
        {
            return ResourceManager.GetString("VoteMore", getUserCulture(user));

        }
        internal static string getWatsonHasCat(BotUser user)
        {
            return ResourceManager.GetString("WatsonHasCat", getUserCulture(user));

        }
        internal static string getWatsonNoCat(BotUser user)
        {
            return ResourceManager.GetString("WatsonNoCat", getUserCulture(user));

        }
        internal static string getModManual(BotUser user)
        {
            return ResourceManager.GetString("ModManual", getUserCulture(user));

        }
        internal static string getWatsonEmpty(BotUser user)
        {
            return ResourceManager.GetString("WatsonEmpty", getUserCulture(user));

        }
        internal static string getUrlNoJpg(BotUser user)
        {
            return ResourceManager.GetString("UrlNoJpg", getUserCulture(user));

        }
        internal static string getSpecialThanks(BotUser user)
        {
            return ResourceManager.GetString("SpecialThanks", getUserCulture(user));

        }
        internal static string getNext(BotUser user)
        {
            return ResourceManager.GetString("Next", getUserCulture(user));

        }
        internal static string getCatFromUrl(BotUser user)
        {
            return ResourceManager.GetString("CatFromUrl", getUserCulture(user));

        }
        internal static string getCatFromApi(BotUser user)
        {
            return ResourceManager.GetString("CatFromApi", getUserCulture(user));

        }
        
        internal static string getWatsonSay(BotUser user, string watson_class, double class_score)
        {
            string str= ResourceManager.GetString("WatsonSay", getUserCulture(user));

            return string.Format(str, watson_class, class_score.ToString("G6") );
        }
    }
}
