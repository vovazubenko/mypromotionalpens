using System;
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

        public static bool ValidationRulesForNewCustomerByCompanyName(string companyName)
        {
            bool result = false;

            if (DoesCompanyInBlackList(companyName)) result = true;

            return result;
        }

        public static bool ValidationRulesForNewCustomerByFullName(string companyName)
        {
            bool result = false;
            var distanceSimilarity = 0.5;

            var cleanedData = string.Join(" ", companyName.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList().Select(x => x.Trim()));
            List<string> cleanedDataAmount = cleanedData.Split(' ').ToList();

            if (cleanedDataAmount.Count() == 2)
            {
                string source = cleanedDataAmount[0].ToLower();
                string target = cleanedDataAmount[1].ToLower();

                int stepsToSame = ComputeLevenshteinDistance(source, target);
                var distancePercent = (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));

                if (distancePercent > distanceSimilarity) result = true;
            }

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

        /// <summary>
        /// Check if company name in blacklist.
        /// </summary>
        private static bool DoesCompanyInBlackList(string companyName)
        {
            bool result = false;
            IEnumerable<string> blockedCompanies = new string[] { "google" };

            if (blockedCompanies.Contains(companyName.ToLower())) result = true;

            return result;
        }

        /// <summary>
        /// Returns the number of steps required to transform the source string into the target string.
        /// </summary>
        public static int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;

            if (targetWordCount == 0)
                return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2
            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 4
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
        }
    }
}
