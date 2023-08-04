using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLibrary
{
    public static class MyFileManager
    {
        public static string[] ReadTxtFile(string CompletePath)
        {
            string[]? dataFiles = null;
            try
            {
                if (!Directory.Exists(CompletePath))
                    throw new Exception($"Directory e/o file NON trovati {CompletePath}");
                dataFiles = File.ReadAllLines(CompletePath);
            }
            catch (Exception ex)
            {
                Console.Write($"Errore metodo ReadTxtFile: {ex.Message}");
            }
            return dataFiles;
        }

        public static string GetFullPath(string sPath, string sFile)
        {
            if (string.IsNullOrEmpty(sFile) || string.IsNullOrEmpty(sPath)) return null;
            return Path.Combine(sPath, sFile);
        }
    }
}
