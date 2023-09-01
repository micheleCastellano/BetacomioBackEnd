using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLibrary
{
    public static class MyConverter
    {
        public static string HexString2B64String(this string input)
        {
            return System.Convert.ToBase64String(input.HexStringToHex());
        }

        public static byte[] HexStringToHex(this string inputHex)
        {
            var resultantArray = new byte[inputHex.Length / 2];
            for (var i = 0; i < resultantArray.Length; i++)
            {
                resultantArray[i] = System.Convert.ToByte(inputHex.Substring(i * 2, 2), 16);
            }
            return resultantArray;
        }

        public static string GetImage(string path, byte[] bytes)
        {
            //Byte[] bytes = Convert.FromBase64String(b64Str);
            File.WriteAllBytes($"C:\\Users\\Betacom\\Desktop\\BetacomioBackEnd\\images\\{path}", bytes);

            return "";
        }
    }
}
