using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.IO;

namespace fhash2
{
    class Program
    {
        static Dictionary<string, FileHash> hashes = new Dictionary<string, FileHash>();
        //static List<FileHash> hashes = new List<FileHash>();
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
            string[] arg_csvEnabled = { "-csv", "/csv" }; //TODO
            string[] arg_sortedOut = { "-sort", "/sort", "-s", "/s" }; //TODO
            string[] arg_help = { "-help", "/help", "-h", "/h" }; //TODO


            if (args.Intersect(arg_help).Any())
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("fhash2 for CNT4513 UHA1188\n\n");
                Console.ResetColor();

                Console.WriteLine("Generates MD5 and/or SHA1 hashes based on a given file/dir path,\n" +
                    "can save the results to a .csv file");

                Console.WriteLine("\nUSAGE:");
                Console.Write("    fhash FILE_PATH | DIR_PATH [[-md5] | [-sha1]] [-raw] [-pause] [-quiet]\n" +
                              "                               [-verbose] [[-csv FILE_PATH] | [-sort]] [-help]\n" +
                              "\nOPTIONS:\n");
                Console.WriteLine("    -md5              - Calculate MD5 hash");
                Console.WriteLine("    -sha1             - Calculate SHA1 hash");
                Console.WriteLine("    -raw              - Exclusively return Hash Value");
                Console.WriteLine("    -pause            - Wait for keystroke before exiting");
                Console.WriteLine("    -quiet            - Display no messages");
                Console.WriteLine("    -verbose          - Display all messages");
                Console.WriteLine("    -csv FILE_PATH    - Save hashes to CSV");
                Console.WriteLine("    -sort             - Return CSV hashes sorted by filepath a-Z");
                Console.WriteLine("    -help             - Display this message again.");

                Console.WriteLine("\nEXAMPLES:");
                Console.WriteLine("    > fhash test.txt");
                Console.WriteLine("    > fhash test.txt -md5");
                Console.WriteLine("    > fhash test.txt -sha1");
                Console.WriteLine("    > fhash C:/test -sha1 -md5");
                Console.WriteLine("    > fhash C:/test -sha1 -md5 -csv C:/output.csv");

                Console.WriteLine("\nARGUMENTS: ");
                Console.Write("    ");
                foreach (var arg in arg_genMD5) Console.Write(arg.ToString() + " ");
                Console.Write("\n    ");
                foreach (var arg in arg_genSHA1) Console.Write(arg.ToString() + " ");
                Console.Write("\n    ");
                foreach (var arg in arg_rawOut) Console.Write(arg.ToString() + " ");
                Console.Write("\n    ");
                foreach (var arg in arg_pause) Console.Write(arg.ToString() + " ");
                Console.Write("\n    ");
                foreach (var arg in arg_quiet) Console.Write(arg.ToString() + " ");
                Console.Write("\n    ");
                foreach (var arg in arg_verbose) Console.Write(arg.ToString() + " ");
                Console.Write("\n    ");
                foreach (var arg in arg_csvEnabled) Console.Write(arg.ToString() + " ");
                Console.Write("\n    ");
                foreach (var arg in arg_sortedOut) Console.Write(arg.ToString() + " ");
                Console.Write("\n    ");
                foreach (var arg in arg_help) Console.Write(arg.ToString() + " ");
                Console.Write("\n");
            }
            else
            {
                if (args.Intersect(arg_quiet).Any()) ProgramReport.quietMode = true;
                if (args.Intersect(arg_verbose).Any()) verboseMode = true;

                try
                {
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
                }
                catch (Exception e)
                {
                    ProgramReport.Error("Program.Main", "Could not get a file in the given file path", e);
                }
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
            if (hashType != null)
            {
                if (hashes.ContainsKey(filePath))
                {
                    
                }
                else
                {
                    hashes.Add(filePath, new FileHash(hashValue, hashType, filePath));
                }
                Console.WriteLine("{0}: {1} @ {2}", hashType == "MD5" ? hashType + " " : hashType, hashValue, filePath);
            }
            else //Generally this won't happen, but if it does we can handle it.
            {
                if (hashes.ContainsKey(filePath))
                {

                }
                else
                {
                    hashes.Add(filePath, new FileHash(hashValue, null, filePath));
                }
                Console.WriteLine("{1} @ {2}", hashValue, filePath);
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
        private List<string> hashHistorySHA1 = new List<string>();
        private List<string> hashHistoryMD5 = new List<string>();
        private List<string> hashHistoryUNK = new List<string>(); //unknown hash method
        private string hashType;
        private string hashFilePath;
        public FileHash(string hashValue, string hashType = "", string hashFilePath = "")
        {
            if(hashType.ToLower() == "sha1")
            {
                hashHistorySHA1.Add(hashValue);
            }
            else if(hashType.ToLower() == "md5")
            {
                hashHistoryMD5.Add(hashValue);
            }
            else
            {
                hashHistoryUNK.Add(hashValue);
            }
            this.hashType = hashType;
            this.hashFilePath = hashFilePath;
        }
        public string getCurrentValue(string hashType = "") {
            if (hashType.ToLower() == "md5") return hashHistoryMD5.Last();
            else if (hashType.ToLower() == "sha1") return hashHistorySHA1.Last();
            else return hashHistoryUNK.Last();
        }
        public string getType() { return hashType; }
        public string getFilePath() { return hashFilePath; }
        public void addHashValue(string updatedHash) { }
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
