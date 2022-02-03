using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WpfQuizManager
{

    public static class Utils
    {
        public static QuizManagerEntities context = new QuizManagerEntities();
        public static List<int> questionsSetForCurrentQuiz = new List<int>();

        private static Dictionary<string, List<int>> nrQuestPerRank = new Dictionary<string, List<int>>() { { "IRON",new List<int>{ 3, 1, 1 } },
                                                                                                            { "BRONZE",new List<int>{ 8, 2, 0 } },
                                                                                                            { "SILVER",new List<int>{ 6, 3, 1 } },
                                                                                                            { "GOLD",new List<int>{ 3, 5, 2 } },
                                                                                                            { "PLATINUM",new List<int>{ 2, 4, 4 } },
                                                                                                            { "DIAMOND",new List<int>{ 1, 4, 5 } },
                                                                                                            { "MASTER",new List<int>{ 0, 3, 7 } },
                                                                                                            { "GMASTER",new List<int>{ 0, 1, 9 } },
                                                                                                            { "CHALLENGER",new List<int>{ 0, 0, 10 } }
        };
        private static Dictionary<string, int> timerPerRank = new Dictionary<string, int>()     { { "IRON",90 },
                                                                                                { "BRONZE", 80},
                                                                                                { "SILVER",70},
                                                                                                { "GOLD", 60},
                                                                                                { "PLATINUM",50 },
                                                                                                { "DIAMOND", 40},
                                                                                                { "MASTER", 30},
                                                                                                { "GMASTER", 20},
                                                                                                { "CHALLENGER",10 }
        };

        private static Dictionary<string, List<int>> pointsPerDiff = new Dictionary<string, List<int>>()
        {
            {"Easy", new List<int>{ 5, 2} },
            {"Medium", new List<int>{ 10, 4} },
            {"Hard", new List<int>{ 20, 8} }
        };
        public static int GetTimerPerRank(string rank)
        {
            if (timerPerRank.ContainsKey(rank))
                return timerPerRank[rank];
            else
                return -1;
        }

        public static List<int> GetNrOfQuest(string reqRank)
        {
            if (nrQuestPerRank.ContainsKey(reqRank))
                return nrQuestPerRank[reqRank];
            else
                return null;
        }
        public static List<int> GetNrOfPoints(string reqDiff)
        {
            if (pointsPerDiff.ContainsKey(reqDiff))
                return pointsPerDiff[reqDiff];
            else
                return null;

        }
        public static List<int> GetListOfRandoms(List<int> originalList, int totalCount)
        {
            var indexResultList = new List<int>();
            var finalResultList = new List<int>();
            var set = new HashSet<int>(); 

            Random rand = new Random();

            while (indexResultList.Count < totalCount)
            {
                int num;
                do
                {
                    num = rand.Next(1, originalList.Count);
                } while (set.Contains(num));
                set.Add(num);
                indexResultList.Add(num - 1);
                finalResultList.Add(originalList[num - 1]);
            }
            return finalResultList;

        }
        public static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString());
                }
                return builder.ToString();
            }
        }
        public static void InitTimer(ref DispatcherTimer timer, EventHandler timerEvent)
        {
            
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += timerEvent;
            timer.Start();
        }

    }
}
