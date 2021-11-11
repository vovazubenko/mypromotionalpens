using System.Collections.Generic;
using System.Linq;

namespace Nop.Web.Extensions
{
    public class ValidationRulesForNewCustomer
    {
        public static bool ValidationRulesForNewCustomerByEmail(string email)
        {
            bool result = false;

            if (GetRussianDomainFromEmail(email)) result = true;
            if (ExistDotsInUsername(email)) result = true;
            if (CheckAmountOfCharsBetweenDots(email)) result = true;

            return result;
        }



        /// <summary>
        /// Check if email contains russian domain.
        /// </summary>
        private static bool GetRussianDomainFromEmail(string email)
        {
            bool russianDomainExists = false;

            IEnumerable<string> blockedDomains = new string[] { "ru" };
            IEnumerable<string> domainArray = email.Split('.');
            string domain = domainArray.Skip(domainArray.Count() - 1).Take(1).FirstOrDefault();

            if (blockedDomains.Contains(domain)) russianDomainExists = true;

            return russianDomainExists;
        }

        /// <summary>
        /// Check amount of dots in username and return true if amount of dots are more than amountOfDotsPermission = 3.
        /// </summary>
        private static bool ExistDotsInUsername(string email)
        {
            bool dotsInUserameExists = false;
            int amountOfDotsPermission = 3;

            string username = email.Split('@')[0];
            int amountOfDostInUsername = username.Split('.').Count();

            if (amountOfDostInUsername >= amountOfDotsPermission) dotsInUserameExists = true;

            return dotsInUserameExists;
        }

        /// <summary>
        /// Check amount of chars between dots and return true if amount of dots are more than amountOfDotsMaximumuPermission = 2.
        /// </summary>
        private static bool CheckAmountOfCharsBetweenDots(string email)
        {
            bool amountOfCharsBetweenDotsExists = false;
            int amountOfDotsMaximumuPermission = 2;
            List<int> dataAmount = new List<int>();

            string username = email.Split('@')[0];
            IEnumerable<string> dataArray = username.Split('.');
            dataArray.ToList().ForEach(x => dataAmount.Add(x.Length));

            if (dataAmount.Where(x => x <= amountOfDotsMaximumuPermission).Count() > 0) amountOfCharsBetweenDotsExists = true;

            return amountOfCharsBetweenDotsExists;
        }
    }
}
