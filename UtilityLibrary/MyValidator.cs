using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UtilityLibrary
{
    public static class MyValidator
    {
        public static bool IsEmailAddress(string email)
        {
            if(string.IsNullOrEmpty(email)) return false;
            try
            {
                MailAddress m = new MailAddress(email);
                return true;
            }catch(FormatException)
            {
                return false;
            }
        }
    }
}
