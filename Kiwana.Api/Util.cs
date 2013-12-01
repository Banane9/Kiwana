using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Kiwana.Api
{
    /// <summary>
    /// Some useful utilities
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Matches the User Host Mask nick!name@host.com
        /// </summary>
        public static Regex HostMaskRegex = new Regex(@"(?<=^\:|^)[^:].+(!|!~).+@[\w\.\-]+(.[\w\.\-])+");

        /// <summary>
        /// Matches the nick of the user from nick!name@host.com
        /// </summary>
        public static Regex NickRegex = new Regex(@"(?<=^\:|^)[^:].+(?=(!|!~)[^~].+(?=@))");

        /// <summary>
        /// Matches the name of the user from nick!name@host.com
        /// </summary>
        public static Regex NameRegex = new Regex(@"(?<=!|!~)[^~].+(?=@)");

        /// <summary>
        /// Matches the host of the user from nick!name@host.com
        /// </summary>
        public static Regex HostRegex = new Regex(@"(?<=@).+");

        /// <summary>
        /// Matches the server from irc.server.com
        /// </summary>
        public static Regex ServerRegex = new Regex(@"[\w\.\-]+(.[\w\.\-])+");

        /// <summary>
        /// Matches the Message and Motd format. :Message in ex[0] for motd and at ex[3] for message.
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

            for (int i = (int)start; i < _end; i++)
            {
                if (i < _end && i > start)
                { str += glue; }

                str += strings[i];
            }

            return str;
        }

        /// <summary>
        /// Converts a Unix Timestamp (Seconds since January 1st 00:00:00) into a DateTime
        /// </summary>
        /// <param name="unixTime">The Unix Timestamp</param>
        /// <returns>The DateTime representing the Unix Timestamp</returns>
        public static DateTime UnixToDateTime(long unixTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(unixTime);
        }

        /// <summary>
        /// Converts a DateTime into a Unix Timestamp (Seconds since January 1st 00:00:00)
        /// </summary>
        /// <param name="dateTime">The DateTime</param>
        /// <returns>The Unix Timestamp representing the DateTime</returns>
        public static long DateTimeToUnix(DateTime dateTime)
        {
            return (long) (new DateTime(1970, 1, 1, 0, 0, 0) - dateTime).TotalSeconds;
        }
    }
}
