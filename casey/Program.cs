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
            string filename = @"C:\test\test.yymps";
            string filenameCopy = Path.GetDirectoryName(filename) + @"\" + Path.GetFileNameWithoutExtension(filename) + "_camelCase" + Path.GetExtension(filename);
            File.Copy(filename, filenameCopy, true);

            using FileStream zipToOpen = new FileStream(filenameCopy, FileMode.Open);
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
                    matches = Regex.Matches(str, @"///\ @member\ \{[^\s]+\}\s([^\s]+)|///\ @function\s+([^\(]+)");
                }
                else
                {
                    matches = matches.Concat(Regex.Matches(str, @"///\ @member\ \{[^\s]+\}\s([^\s]+)|///\ @function\s+([^\(]+)"));
                }
            }

            for (int i = 0; i < gmls.Count(); i++)
            {
                StringBuilder builder = new StringBuilder(strs[i]);

                string value;

                foreach (Match match in matches)
                {
                    value = match.Groups[1].Value;              // member

                    if (value != "")
                    {
                        builder.Replace(value + " = ", ToCamelCase(value + " = "));                 // declaration
                        builder.Replace("." + value, ToCamelCase("." + value));                     // use
                    }

                    value = match.Groups[2].Value;

                    if (value != "")                            // function
                    {
                        builder.Replace(" " + value + "(", ToCamelCase(" " + value + "("));         // jsdoc
                        builder.Replace(" " + value + " = ", ToCamelCase(" " + value + " = "));     // declaration
                        builder.Replace("." + value + "(", ToCamelCase("." + value + "("));         // use
                    }
                }

                using StreamWriter writer = new StreamWriter(gmls.ElementAt(i).Open());
                writer.Write(builder);

                //Console.WriteLine(builder);
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
