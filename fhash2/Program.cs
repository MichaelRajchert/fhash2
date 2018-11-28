using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.IO;

namespace fhash2
{
    class Program
    {
        static bool programSuccess = true;
        static bool verboseMode = false;
        static void Main(string[] args)
        {
            string[] arg_genMD5 = { "-md5", "/md5" };
            string[] arg_genSHA1 = { "-sha1", "/sha1" };
            string[] arg_rawOut = { "-raw", "/raw", "-r", "/r" };
            string[] arg_pause = { "-pause", "/pause", "-p", "/p"};
            string[] arg_quiet = { "-quiet", "/quiet", "-q", "/q" };
            string[] arg_verbose = { "-verbose", "/verbose", "-v", "/v" };
            string[] arg_sortedOut = { "-sort", "/sort", "-s", "/s" }; //TODO
            string[] arg_help = { "-help", "/help", "-h", "/h" }; //TODO


            if (args.Intersect(arg_quiet).Any()) ProgramReport.quietMode = true;
            if (args.Intersect(arg_verbose).Any()) verboseMode = true;
            if (args.Intersect(arg_help).Any())
            {
                ProgramReport.Notice("Help"); //Get rid of this once Help is written
            }

            FileAttributes attr = File.GetAttributes(args[0]);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                ProgramReport.Notice("Given Directory: " + args[0]);
                DirectoryInfo dir = new DirectoryInfo(args[0]);
                foreach (var file in dir.GetFiles("*.*"))
                {
                    if (args.Intersect(arg_genMD5).Any()) HashHandler(file.FullName, "MD5", args.Intersect(arg_rawOut).Any() ? true : false, verboseMode);
                    if (args.Intersect(arg_genSHA1).Any()) HashHandler(file.FullName, "SHA1", args.Intersect(arg_rawOut).Any() ? true : false, verboseMode);
                }
            }
            else
            {
                ProgramReport.Notice("Given File: " + args[0]);
                if (args.Intersect(arg_genMD5).Any()) HashHandler(args[0], "MD5", args.Intersect(arg_rawOut).Any() ? true : false, verboseMode);
                if (args.Intersect(arg_genSHA1).Any()) HashHandler(args[0], "SHA1", args.Intersect(arg_rawOut).Any() ? true : false, verboseMode);
            }
            
            if (!programSuccess)
            {
                ProgramReport.Warning("", "Program exited with errors, review the critical errors and try again.");
            }
            //END
            if (args.Intersect(arg_pause).Any())
            {
                Console.ReadKey();
            }
        }
        static void HashHandler(string filePath, string hashType = "MD5", bool raw = false, bool verbose = false)
        {
            if(hashType == "MD5")
            {
                if (raw)
                {
                    Console.WriteLine(HashGen.GenMD5(filePath));
                }
                else
                {
                    if(verbose) ProgramReport.Notice("Generating MD5");
                    string result = HashGen.GenMD5(filePath);
                    if (result == null)
                    {
                        programSuccess = false;
                    }
                    else
                    {
                        HashOut(result, filePath, "MD5");
                    }
                }
            }
            else if (hashType == "SHA1")
            {
                if (raw)
                {
                    Console.WriteLine(HashGen.GenSHA1(filePath));
                }
                else
                {
                    if(verbose) ProgramReport.Notice("Generating SHA1");
                    string result = HashGen.GenSHA1(filePath);
                    if (result == null)
                    {
                        programSuccess = false;
                    }
                    else
                    {
                        HashOut(result, filePath, "SHA1");
                    }
                }
            }

        }
        static void HashOut(string hashValue, string filePath, string hashType = null)
        {
            if (hashType != null) Console.WriteLine("{0}: {1} @ {2}", hashType == "MD5" ? hashType+" " : hashType, hashValue, filePath);
            else Console.WriteLine("{1} @ {2}", hashValue, filePath);
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
        public static bool quietMode = false;
        public static void Error(string location, string message, Exception exception = null)
        {
            if (!quietMode)
            {
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("X");
                Console.ResetColor();
                if (exception == null)
                {
                    Console.WriteLine("]: {0}, {1}", location, message);
                }
                else
                {
                    Console.WriteLine("]: {0} @ {1}, {2}", exception.Message, location, message);
                }
            }
        }
        public static void Warning(string location, string message)
        {
            if (!quietMode)
            {
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("!");
                Console.ResetColor();
                if (location == "")
                {
                    Console.WriteLine("]: {0}", message);
                }
                else
                {
                    Console.WriteLine("]: {0} @ {1}", message, location);
                }
            }
        }
        public static void Notice(string message)
        {
            if (!quietMode)
            {
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("~");
                Console.ResetColor();
                Console.WriteLine("]: {0}", message);
            }
        }
    }
}
