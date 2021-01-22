using System;
using System.Linq;
using System.Net;

namespace LmpUpdater.storm
{
    public class UpdateChecker
    {

        public static Version GetLatestVersion()
        {
            string verstr = string.Empty;

            try
            {
                using (var wc = new WebClient())
                {
                    verstr = wc.DownloadString("https://storm37k.com/ksp_lmp/server.ver");
                }
            }
            catch (Exception)
            {
                //Try http if https isnt working for some reason...
                try
                {
                    using (var wc = new WebClient())
                    {
                        verstr = wc.DownloadString("http://storm37k.com/ksp_lmp/server.ver");
                    }
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error getting latest version " + e.Message);
                    Console.ResetColor();
                }
            }

            if (verstr == string.Empty) return new Version("0.0.0");

            var version = new string(verstr.Where(c => char.IsDigit(c) || char.IsPunctuation(c)).ToArray()).Split('.');

            return version.Length == 3 ?
                new Version(int.Parse(version[0]), int.Parse(version[1]), int.Parse(version[2])) :
                new Version("0.0.0");
        }
    }
}
