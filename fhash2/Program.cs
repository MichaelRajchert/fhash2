using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace fhash2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(HashGen.GenMD5("B:\\Users\\Michael Rajchert\\Desktop\\Project 2 Outline.pdf"));
            Console.ReadKey();
        }
    }
    class HashGen
    {
        public static string GenMD5(string filepath)
        {
            var hashResult = MD5.Create();
            try
            {
                var fileStream = File.OpenRead(filepath);
                return BitConverter.ToString(hashResult.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();
            }
            catch (Exception e)
            {
                ProgramReport.Error(e, "genMD5", "Failed to read file: " + filepath);
                return null;
            }
        }
        public static string GenSHA1(string filepath)
        {
            var hashResult = SHA1.Create();
            try
            {
                var fileStream = File.OpenRead(filepath);
                return BitConverter.ToString(hashResult.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();
            }
            catch (Exception e)
            {
                ProgramReport.Error(e, "genMD5", "Failed to read file: " + filepath);
                return null;
            }
        }
    }
    class ProgramReport
    {
        public static void Error(Exception exception, string location, string message)
        {
            Console.WriteLine("ERROR: {0} @ {1}, {2}", exception.Message, location, message);
        }
        public static void Warning(string location, string message)
        {
            Console.WriteLine("WARNING: {0} @ {1}", message, location);
        }
    }
}
