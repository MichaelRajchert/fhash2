using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Xml.Linq;
using System.Xml;

namespace fhash2
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] arg_genMD5 = { "-md5", "/md5" };
            string[] arg_genSHA1 = { "-sha1", "/sha1" };
            string[] arg_rawOut = { "-raw", "/raw" };
            string[] arg_pause = { "-pause", "/pause" };
            string[] arg_help = { "-help", "/help", "-h", "/h" };

            Dictionary<string, FileHash> hashes = new Dictionary<string, FileHash>();

            bool programSuccess = true;
            
            if (args.Intersect(arg_help).Any())
            {
                ProgramReport.Notice("Help"); //Get rid of this once Help is written
            }
            if (args.Intersect(arg_genMD5).Any())
            {
                if (args.Intersect(arg_rawOut).Any())
                {
                    Console.WriteLine(HashGen.GenMD5(args[0]));
                }
                else
                {
                    ProgramReport.Notice("Generating MD5");
                    string result = HashGen.GenMD5(args[0]);
                    if(result == null)
                    {
                        programSuccess = false;
                    }
                    else
                    {
                        Console.WriteLine(result); //have this write to spreadsheet instead
                    }
                }
            }
            if (args.Intersect(arg_genSHA1).Any())
            {
                if (args.Intersect(arg_rawOut).Any())
                {
                    Console.WriteLine(HashGen.GenSHA1(args[0]));
                }
                else
                {
                    ProgramReport.Notice("Generating SHA1");
                    string result = HashGen.GenSHA1(args[0]);
                    if(result == null)
                    {
                        programSuccess = false;
                    }
                    else
                    {
                        Console.WriteLine(result); //have this write to a spreadsheet instaed.
                    }
                }
            }
            if (!programSuccess)
            {
                ProgramReport.Warning("", "Program exited with no success, review the critical errors and try again.");
            }
            //END
            if (args.Intersect(arg_pause).Any())
            {
                Console.ReadKey();
            }
        }
    }
    class HashGen
    {
        public static string GenMD5(string filepath)
        {
            using (var hashResult = MD5.Create())
            {
                try
                {
                    using(var fileStream = File.OpenRead(filepath))
                    {
                        return BitConverter.ToString(hashResult.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();
                    }
                }
                catch (Exception e)
                {
                    ProgramReport.Error("GenMD5", "Failed to read file: " + filepath, e);
                    return null;
                }
            }
        }
        public static string GenSHA1(string filepath)
        {
            using (var hashResult = SHA1.Create())
            {
                try
                {
                    using (var fileStream = File.OpenRead(filepath))
                    {
                        return BitConverter.ToString(hashResult.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();
                    }
                }
                catch (Exception e)
                {
                    ProgramReport.Error("GenSHA1", "Failed to read file: " + filepath, e);
                    return null;
                }
            }
        }
    }
    class FileHash
    {
        private static string fp;
        private static List<string> md5HashHistory = new List<string>();
        private static List<string> sha1HashHistory = new List<string>();
        public static void Init(string FilePath, string md5 = null, string sha1 = null)
        {
            fp = FilePath;
            if (md5 == null && sha1 == null)
            {
                ProgramReport.Error("FileHash.init", "Cannot initialise a new hash history with no given hashes.");
            }
            if (md5 != null) { md5HashHistory.Add(md5); }
            if (sha1 != null) { sha1HashHistory.Add(sha1); }
        }
        public static void AddMD5(string md5) { md5HashHistory.Add(md5); }
        public static void AddSHA1(string sha1) { sha1HashHistory.Add(sha1); }
        public static void SetFilePath(string filePath) { fp = filePath; }
        public static string GetFilePath() { return fp; }
        public static List<string> GetMD5HashHistory() { return md5HashHistory; }
        public static List<string> GetSHA1HashHistory() { return sha1HashHistory; }
    }
    class ProgramReport
    {
        public static void Error(string location, string message, Exception exception = null)
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("X");
            Console.ResetColor();
            if(exception == null)
            {
                Console.WriteLine("]: {0}, {1}", location, message);
            }
            else
            {
                Console.WriteLine("]: {0} @ {1}, {2}", exception.Message, location, message);
            }
        }
        public static void Warning(string location, string message)
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("!");
            Console.ResetColor();
            if(location == "")
            {
                Console.WriteLine("]: {0}", message);
            }
            else
            {
                Console.WriteLine("]: {0} @ {1}", message, location);
            }
        }
        public static void Notice(string message)
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("~");
            Console.ResetColor();
            Console.WriteLine("]: {0}", message);
        }
    }
}
