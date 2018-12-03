/* 
    TODO LIST
        Add Date&Time collected for file hash
        Add CSV functionality
        Look into sorting the final CSV file by filePath
        Make the hashes list more efficient

    PROCESS:
        0 - If we supply a CSV case file and it EXISTS
            Read all the contents and put it in our dictionary.
            If it doesn't exist, we'll create one later.
        1 - find out if the given filepath (arg[0]) is a file, or a directory
            If it's a dir, call HashHandler() for the files in the folder, if not, just hash the file.
        2 - Get the output of HashHandler() and send it to HashOut().
            It'll store the hashes we generate in the dictionary, and if we want it to, write it to the console too.
        3 - If we've supplied a location for a CSV file and it doesn't exist, write our data collected to it.
        4 - Show a report at the end of all the stuff we discovered. Hopefully.
*/

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

        static bool programSuccess = true;
        static bool verboseMode = false;
        static bool noHashOutput = false;
        static bool noHashSpecified;
        static string csvCaseFilePath;
        static int fileHashDiffCount = 0;
        static void Main(string[] args)
        {
            string[] arg_genMD5 = { "-md5", "/md5" };
            string[] arg_genSHA1 = { "-sha1", "/sha1" };
            string[] arg_rawOut = { "-raw", "/raw", "-r", "/r" };
            string[] arg_pause = { "-pause", "/pause", "-p", "/p"};
            string[] arg_quiet = { "-quiet", "/quiet", "-q", "/q" };
            string[] arg_verbose = { "-verbose", "/verbose", "-v", "/v" };
            string[] arg_csv = { "-csv", "/csv" };
            string[] arg_sortedOut = { "-sort", "/sort", "-s", "/s" }; //TODO
            string[] arg_help = { "-help", "/help", "-h", "/h" };

            if (args.Intersect(arg_help).Any() || args.Length == 0)
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
                foreach (var arg in arg_csv) Console.Write(arg.ToString() + " ");
                Console.Write("\n    ");
                foreach (var arg in arg_sortedOut) Console.Write(arg.ToString() + " ");
                Console.Write("\n    ");
                foreach (var arg in arg_help) Console.Write(arg.ToString() + " ");
                Console.Write("\n");
                return;
            } //WRITE HELP
            else
            {
                if (!args.Intersect(arg_genMD5).Any() && !args.Intersect(arg_genSHA1).Any()) noHashSpecified = true;
                if (args.Intersect(arg_quiet).Any()) ProgramReport.quietMode = true; //DON'T OUTPUT ANY REPORTS
                if (args.Intersect(arg_rawOut).Any()) ProgramReport.quietMode = true; //ONLY OUTPUT HASHES
                if (args.Intersect(arg_verbose).Any()) verboseMode = true; //SHOW EXTRA LOGS

                if (args.Intersect(arg_csv).Any())
                {
                    noHashOutput = true;
                    int csvArgIndex = IndexOfArrayUsingArray(args, arg_csv);
                    try
                    {
                        csvCaseFilePath = args[csvArgIndex + 1];
                    }
                    catch (Exception e)
                    {
                        ProgramReport.Error("Program.Main", "bad '-csv' arguments given", e);
                    }
                    ProgramReport.Notice("Given CSV case Location: " + csvCaseFilePath);

                    if (File.Exists(csvCaseFilePath))
                    {
                        ProgramReport.Notice("Case file already exists. Data will be read and new hashes will be added.");
                        CSVReader(csvCaseFilePath);
                    }
                    else
                    {
                        ProgramReport.Notice("Case file doesn't exist. Creating new case file.");
                    }
                }

                //START GENERATING HASHES
                if (!args.Intersect(arg_quiet).Any()) Console.WriteLine("");
                if (args.Intersect(arg_genMD5).Any() && !args.Intersect(arg_quiet).Any()) { ProgramReport.Notice("Generating MD5"); }
                if (args.Intersect(arg_genSHA1).Any() && !args.Intersect(arg_quiet).Any()) { ProgramReport.Notice("Generating SHA1"); }
                if (noHashSpecified) { ProgramReport.Notice("Generating MD5"); ProgramReport.Notice("Generating SHA1"); }
                try
                {
                    FileAttributes attr = File.GetAttributes(args[0]);
                    if (attr.HasFlag(FileAttributes.Directory))
                    {
                        ProgramReport.Notice("Given Directory: " + args[0]);
                        DirectoryInfo dir = new DirectoryInfo(args[0]);
                        foreach (FileInfo fileInfo in dir.GetFiles("*.*"))
                        {
                            if (args.Intersect(arg_genMD5).Any()) HashHandler(fileInfo.FullName, "MD5", args.Intersect(arg_rawOut).Any() ? true : false, verboseMode);
                            if (args.Intersect(arg_genSHA1).Any()) HashHandler(fileInfo.FullName, "SHA1", args.Intersect(arg_rawOut).Any() ? true : false, verboseMode);
                            if (noHashSpecified)
                            {
                                HashHandler(fileInfo.FullName, "MD5", args.Intersect(arg_rawOut).Any() ? true : false, verboseMode);
                                HashHandler(fileInfo.FullName, "SHA1", args.Intersect(arg_rawOut).Any() ? true : false, verboseMode);
                            }
                        }
                    }
                    else
                    {
                        ProgramReport.Notice("Given File: " + args[0]);
                        if (args.Intersect(arg_genMD5).Any()) HashHandler(args[0], "MD5", args.Intersect(arg_rawOut).Any() ? true : false, verboseMode);
                        if (args.Intersect(arg_genSHA1).Any()) HashHandler(args[0], "SHA1", args.Intersect(arg_rawOut).Any() ? true : false, verboseMode);
                        if (noHashSpecified)
                        {
                            HashHandler(args[0], "MD5", args.Intersect(arg_rawOut).Any() ? true : false, verboseMode);
                            HashHandler(args[0], "SHA1", args.Intersect(arg_rawOut).Any() ? true : false, verboseMode);
                        }
                    }
                    if (verboseMode) ProgramReport.Notice("Finished generating hashes.");
                }
                catch (Exception e)
                {
                    ProgramReport.Error("Program.Main", "Could not get a file in the given file path", e);
                    programSuccess = false;
                } //GENERATE HASHES
            }
            
            //FINISHED GENERATING, ADDING NEW HASHES TO FILE
            if (!args.Intersect(arg_quiet).Any()) Console.WriteLine("");

            if (args.Intersect(arg_csv).Any()) 
            {
                int csvArgIndex = IndexOfArrayUsingArray(args, arg_csv);
                CSVWriter(hashes, csvCaseFilePath);
            }//WRITE COLLECTED HASHES TO CASE FILE

            //COMPARE HASHES
            foreach(KeyValuePair<string, FileHash> hashPair in hashes)
            {
                if (hashPair.Value.HashChanged("MD5")) Console.WriteLine(hashPair.Value.GetFilePath() + " CHANGED");
            }

            if (!programSuccess)
            {
                ProgramReport.Warning("", "Program exited with errors, review the critical errors and try again.");
            } //PROGRAM COULD NOT FINISH
            //END

            if (!args.Intersect(arg_quiet).Any() && !args.Intersect(arg_help).Any())
            {
                ProgramReport.Notice(String.Format("Done. \n\n"+
                                                   "     Scanned               : {0}\n" +
                                                   "     CaseFile              : {1}\n" +
                                                   "     Hashes Recorded       : {2}\n" +
                                                   "     Hash Changes Detected : {3}\n",
                                                   args[0],
                                                   csvCaseFilePath,
                                                   hashes.Count(),
                                                   fileHashDiffCount));
            } //END REPORT
            if (args.Intersect(arg_pause).Any()) //PAUSE WHEN DONE
            {
                Console.ReadKey();
            }
        }


        //HashHandler
        //  This calls the hash generator class and tells it what to do.
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
        
        //HashOut
        //  Basically handles what to do with a hash after we generate it.
        //  If we have hashOutput enabled or verbose enable, it'll print extra stuff to the console.
        static void HashOut(string hashValue, string filePath, string hashType = "UNK")
        {
            if (hashType != null)
            {
                if (hashes.ContainsKey(filePath))
                {
                    hashes[filePath].AddHashValue(hashValue, hashType);
                }
                else
                {
                    if(verboseMode) ProgramReport.Notice("New file entry found at "+filePath);
                    hashes.Add(filePath, new FileHash(hashValue, hashType, filePath, DateTime.Now.ToString("ss:mm:h tt dd-MM-yyyy")));
                }
                if(!noHashOutput) Console.WriteLine("    {0}: {1} @ {2}", hashType == "MD5" ? hashType + " " : hashType, hashValue, filePath);
            }
        }
        
        //CSVReader
        //  Reads existing data from a given file path and add's it to the FileHash object.
        //  Pretty much the same this as generating hashes but we don't actually generate anything
        static void CSVReader(string filePath)
        {
            try
            {
                using (System.IO.StreamReader csvCaseFile = new StreamReader(filePath))
                {
                    string line;
                    int lineNum = 0;
                    while((line = csvCaseFile.ReadLine()) != null)
                    {
                        if(lineNum != 0) //skip first row
                        {
                            List<string> lineData = line.Split(',').ToList<string>();
                            string dateCollected = lineData[0];
                            string hashFilePath = lineData[1];
                            string md5Curr = lineData[2];
                            string md5Old = lineData[3];
                            string sha1Curr = lineData[4];
                            string sha1Old = lineData[5];
                            string unkHash = lineData[6];

                            if (hashes.ContainsKey(hashFilePath))
                            {
                                hashes[hashFilePath].AddHashValue(md5Curr, "MD5");
                                hashes[hashFilePath].AddHashValue(sha1Curr, "SHA1");
                                if (unkHash != "") hashes[hashFilePath].AddHashValue(unkHash);
                            }
                            else
                            {
                                hashes.Add(hashFilePath, new FileHash(md5Curr, "MD5", hashFilePath, DateTime.Now.ToString("ss:mm:h tt dd-MM-yyyy")));
                                hashes[hashFilePath].AddHashValue(sha1Curr, "SHA1");
                                if (unkHash != "") hashes[hashFilePath].AddHashValue(unkHash);
                            }

                        }
                        lineNum++;
                    }
                }
            }
            catch(Exception e)
            {
                ProgramReport.Error("Program.CSVReader", "CSV Case File Reader Error" + filePath, e);
            }
        }

        //CSVWriter
        //  Writes our data to a case file, as a .csv
        //  Contains various information relating to a digital forensic investigation.
        static void CSVWriter(Dictionary<string, FileHash> fileHashes, string csvFilePath)
        {
            try
            {
                using (System.IO.StreamWriter csvCasefile = new StreamWriter(csvFilePath))
                {
                    csvCasefile.WriteLine("Date Collected, File Path, MD5 Hash Current, MD5 Hash Old, SHA1 Hash Current, SHA1 Hash Old, Unknown Hashes");
                    foreach(KeyValuePair<string, FileHash> hashPair in fileHashes.ToList())
                    {
                        FileHash hashObj = hashPair.Value;
                        string dateCollected = hashObj.GetDateTime();
                        string filePath = hashPair.Key;
                        string sha1CurrVal = hashObj.GetCurrentValue("SHA1");
                        string sha1OldVal = hashObj.GetOldValue("SHA1");
                        string md5CurrVal = hashObj.GetCurrentValue("MD5");
                        string md5OldVal = hashObj.GetOldValue("MD5");
                        string unkVal = hashObj.GetOldValue("");
                        csvCasefile.WriteLine(
                            "{0},{1},{2},{3},{4},{5},{6}",
                            dateCollected,
                            filePath,
                            md5CurrVal,
                            md5OldVal,
                            sha1CurrVal,
                            sha1OldVal,
                            unkVal
                        );
                    }
                }
            }
            catch(Exception e)
            {
                ProgramReport.Error("Program.CSVWriter", "CSV Case File Writer Error", e);
            }
        }

        //IndexOfArrayUsingArray
        //  I needed this earlier, basically searches through an array, using an array of keywords.
        //  If it finds something, it'll return the index integer.
        static int IndexOfArrayUsingArray(string[] find, string[] dictionary)
        {
            foreach(string item in find) foreach(string word in dictionary) if (word == item) return Array.IndexOf(find, word);
            return 0;
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
        private string dateTime = "";
        private string HashType { get; set; }
        private string HashFilePath { get; set; }
        public FileHash(string hashValue, string hashType = "", string hashFilePath = "", string dateTime = "")
        {
            if (hashType.ToLower() == "sha1")
            {
                hashHistorySHA1.Add(hashValue);
            }
            else if (hashType.ToLower() == "md5")
            {
                hashHistoryMD5.Add(hashValue);
            }
            else
            {
                hashHistoryUNK.Add(hashValue);
            }
            HashType = hashType;
            HashFilePath = hashFilePath;
        }
        public string GetCurrentValue(string hashType = "") {
            if (hashType.ToLower() == "md5" && hashHistoryMD5.Count() != 0) return hashHistoryMD5.Last();
            else if (hashType.ToLower() == "sha1" && hashHistorySHA1.Count() != 0) return hashHistorySHA1.Last();
            else if (hashType.ToLower() == "" && hashHistoryUNK.Count() != 0) return hashHistoryUNK.Last();
            else return "";
        }
        public string GetOldValue(string hashType = "")
        {
            if (hashType.ToLower() == "md5" && hashHistoryMD5.Count() > 1) { return hashHistoryMD5[hashHistoryMD5.Count() - 2]; }
            else if (hashType.ToLower() == "sha1" && hashHistorySHA1.Count() > 1) { return hashHistorySHA1[hashHistorySHA1.Count() - 2]; }
            else if (hashType.ToLower() == "" && hashHistoryUNK.Count() > 1) { return hashHistoryUNK[hashHistoryUNK.Count - 2]; }
            else { return ""; }
        }
        public string GetHashType() { return HashType; }
        public string GetFilePath() { return HashFilePath; }
        public string GetDateTime() { return dateTime; }
        public bool HashChanged(string hashType = "")
        {
            if(GetCurrentValue(hashType) != GetOldValue(hashType)) { return true; }
            return false;
        }
        public void AddHashValue(string updatedHash, string hashType = "")
        {
            if (hashType.ToLower() == "sha1") hashHistorySHA1.Add(updatedHash);
            else if (hashType.ToLower() == "md5") hashHistoryMD5.Add(updatedHash);
            else hashHistoryUNK.Add(updatedHash);
        }
    }
    class ProgramReport
    {
        public static bool quietMode = false;
        public static void Error(string location, string message, Exception exception = null)
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
