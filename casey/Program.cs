using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace casey
{
    class Program
    {
        static void Main(string[] args)
        {
            using FileStream zipToOpen = new FileStream(@"C:\test\games.keevee.ArrayList.yymps", FileMode.Open);
            using ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update);

            IEnumerable<ZipArchiveEntry> gmls = archive.Entries.Where(entry => entry.FullName.Contains(".gml"));
            IEnumerable<Match> matches = null;
            List<string> strs = new List<string>();

            foreach (ZipArchiveEntry gml in gmls)
            {
                using StreamReader reader = new StreamReader(gml.Open());
                string str = reader.ReadToEnd();
                strs.Add(str);

                if (matches is null)
                {
                    matches = Regex.Matches(str, @"///\ @function\s+([^\(]+)");
                }
                else
                {
                    matches = matches.Concat(Regex.Matches(str, @"///\ @function\s+([^\(]+)"));
                }
            }

            for (int i = 0; i < gmls.Count(); i++)
            {
                StringBuilder builder = new StringBuilder(strs[i]);

                foreach (Match match in matches)
                {
                    string value = match.Groups[1].Value;
                    builder.Replace(" " + value + "(", ToCamelCase(" " + value + "("));
                    builder.Replace(" " + value + " = ", ToCamelCase(" " + value + " = "));
                }

                using StreamWriter writer = new StreamWriter(gmls.ElementAt(i).Open());
                writer.Write(builder);

                Console.WriteLine(builder);
            }
        }
        private static string ToCamelCase(string s)
        {
            return Regex.Replace(s, "_[a-z]", delegate (Match m) {
                return m.ToString().TrimStart('_').ToUpper();
            });
        }
    }
}
