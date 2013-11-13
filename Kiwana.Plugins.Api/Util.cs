using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Kiwana.Plugins.Api
{
    /// <summary>
    /// Some useful utilities
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Matches the nick of the user from :nick!name@host.com in ex[0]
        /// </summary>
        public static Regex NickRegex = new Regex(@"(?<=\:).+(?=\!)");

        /// <summary>
        /// Matches the name of the user from :nick!name@host.com in ex[0]
        /// </summary>
        public static Regex NameRegex = new Regex(@"(?<=!|!~)[^~].+(?=@)");

        /// <summary>
        /// Matches the Message and Motd format. :Message in ex[0] for motd and in starting at ex[3] for message.
        /// </summary>
        public static Regex MessageRegex = new Regex(@"(?<=\:).+");


        /// <summary>
        /// Joins the strings in the list with the glue. Only uses the strings in the range specified.
        /// </summary>
        /// <param name="strings">The list of strings.</param>
        /// <param name="glue">The glue that the strings are joined with.</param>
        /// <param name="start">The index at which the joining starts.</param>
        /// <param name="end">The index at which the joining ends.</param>
        /// <returns></returns>
        public static string JoinStringList(List<string> strings, string glue = "", uint start = 0, uint end = 0)
        {
            string str = "";

            int _end = end == 0 ? strings.Count : (int)end;

            for (int i = (int)start; i < end; i++)
            {
                if (i < end && i > start)
                { str += glue; }

                str += strings[i];
            }

            return str;
        }
    }
}
