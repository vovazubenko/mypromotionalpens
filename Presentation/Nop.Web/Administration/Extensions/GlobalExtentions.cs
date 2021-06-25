using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nop.Admin.Extensions
{
    public static class GlobalExtentions
    {
        const string passphrase = "huhXREIHU.1";
        const string slashreplacing = "$";
        const string plusreplacing = "_";
        public static string Append(this string currentData, string appending)
        {
            return currentData = currentData + appending;
        }

        public static bool IsValidEmail(this string strIn)
        {
            if (String.IsNullOrEmpty(strIn))
                return false;
            // Return true if strIn is in valid e-mail format. 
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
        public static string ToNullableLower(this string str)
        {
            if (str == null)
                return "";
            return str.ToLower();
        }
        public static string ToNullableString(this string str)
        {
            if (str == null)
                return "";
            return Convert.ToString(str);
        }
        public static int ToNullableINT(this string str)
        {
            int i = 0;
            int.TryParse(str, out i);
            return i;
        }
        public static string EncryptString(this string str)
        {
            /*
            byte[] passBytes = System.Text.Encoding.Unicode.GetBytes(str);
            string encryptPassword = Convert.ToBase64String(passBytes);
            return encryptPassword;
             */
            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(passphrase));
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;
            byte[] DataToEncrypt = UTF8.GetBytes(str);
            try
            {
                ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor();
                Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
            }
            finally
            {
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }
            return Convert.ToBase64String(Results).Replace("/", slashreplacing).Replace("+", plusreplacing);
        }
        public static string DecryptString(this string encryptedstr)
        {
            encryptedstr = encryptedstr.Replace(slashreplacing, "/").Replace(plusreplacing, "+");
            /*
            byte[] passByteData = Convert.FromBase64String(encryptedstr);
            string originalPassword = System.Text.Encoding.Unicode.GetString(passByteData);
            return originalPassword;
             */
            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(passphrase));
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;
            byte[] DataToDecrypt = Convert.FromBase64String(encryptedstr);
            try
            {
                ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
                Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
            }
            finally
            {
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }
            return UTF8.GetString(Results);
        }
        /*
        #region Email
        public static void SendEmail(string emailFrom, string emailTo, string subject, string mailBody, string emailCc)
        {
            var hcssEmail = ConfigurationManager.AppSettings["Text2ThemEmail"];
            var hcssPassword = ConfigurationManager.AppSettings["Text2ThemPassword"];
            var mailHost = ConfigurationManager.AppSettings["MailHost"];
            var mail = new MailMessage();
            var client = new SmtpClient(mailHost)
            {
                EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]),
                Credentials = new System.Net.NetworkCredential(hcssEmail, hcssPassword),
                Host = mailHost,
                Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"])
            };
            mail.IsBodyHtml = true;
            mail.Subject = subject;
            mail.From = emailFrom.Equals("") ? new MailAddress("info@ss.net") : new MailAddress(emailFrom);
            mail.To.Add(emailTo);
            if (emailCc.Equals("")) { }
            else
            {
                mail.CC.Add(emailCc);
            }
            mail.Body = mailBody;
            client.Send(mail);
        }
        #endregion 
        */

    }
    public static class EnumExtentions
    {
        public static string GetDescription(this object enumValue, string defaultDescription)
        {
            FieldInfo fi = enumValue.GetType().GetField(enumValue.ToString());

            if (null != fi)
            {
                object[] attrs = fi.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }

            return defaultDescription;
        }
        public static IList<EnumRecord> GetAsList(this object enumValue)
        {
            string s = "";
            List<EnumRecord> records = new List<EnumRecord>();
            foreach (object val in Enum.GetValues(enumValue.GetType()))
            {
                EnumRecord record = new EnumRecord();
                record.Name = val.ToString();
                record.Id = (int)val;
                record.Description = val.GetDescription("");
                records.Add(record);
            }
            return records;
        }
    }
    public class EnumRecord
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
