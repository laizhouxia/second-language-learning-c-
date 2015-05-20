using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignCategoryDictionary
{
    public class Word
    {
        public string english;
        public string chinese;
        public string post;
        public Word(string english_ = "", string chinese_ = "", string post_ = "")
        {
            english = english_;
            chinese = chinese_;
            post = post_;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ArrayList words = new ArrayList();
            string[] lines = File.ReadAllLines(@"../../dictionary2.txt");
            foreach (string line in lines)
            {
                string[] str = line.Split(',');
                if (str.Length == 3)
                {
                    Word tempWord = new Word(str[0], str[1], str[2]);
                    words.Add(tempWord);
                }
            }
            ArrayList fileNames = new ArrayList();
            fileNames.Add("entertainment");
            fileNames.Add("military");
            fileNames.Add("finance");
            fileNames.Add("sports");
            fileNames.Add("international");
            fileNames.Add("social");
            fileNames.Add("technology");
            fileNames.Add("lady");
            fileNames.Add("auto");
            fileNames.Add("game");
            fileNames.Add("education");
            ArrayList categories = new ArrayList();
            foreach (string fileName in fileNames)
            {
                lines = File.ReadAllLines(@"../../"+fileName+".out");
                ArrayList articles = new ArrayList();
                foreach (string line in lines)
                {
                    string[] str = line.Split(',');
                    if(str.Length > 1)
                    articles.Add(new ArrayList(str));
                }
                categories.Add(articles);
            }

            //Console.WriteLine("dictionary size : " + words.Count);
            //Console.WriteLine("category size : " + categories.Count);
            //foreach (ArrayList articles in categories)
            //    Console.Write(" " + articles.Count);

            StreamWriter outFile = new StreamWriter(@"../../dicionaryFrequency.txt");

            foreach (Word w in words)
            {
                string chinese = w.chinese;
                ArrayList frequencyArticles = new ArrayList();
                foreach (ArrayList articles in categories)
                {
                    int countFrequency = 0;
                    foreach (ArrayList wordList in articles)
                        if (wordList.Contains(chinese))
                            countFrequency++;
                    frequencyArticles.Add(countFrequency);
                }

                string outLine = "";
                outLine += w.english + "," + w.chinese + "," + w.post;

                int totalCount = 0;
                int maximum = 0;
                int index = -1;

                for(int i=0;i<frequencyArticles.Count;i++)
                {
                    totalCount += (int)frequencyArticles[i];
                    maximum = Math.Max((int)frequencyArticles[i], maximum);
                    if ((int)frequencyArticles[i] == maximum)
                        index = i;
                }

                ArrayList categoryList = new ArrayList();

                if (index == 1 || index == 5)
                    index = 3;
                else if (index == 2 || index == 3 || index == 4)
                    index -= 1;
                else if (index == 6 || index == 7 || index == 8)
                    index -= 2;

                categoryList.Add("entertainment");
                categoryList.Add("finance");
                categoryList.Add("sports");
                categoryList.Add("world");
                categoryList.Add("tech");
                categoryList.Add("fashion");
                categoryList.Add("travel");


                if (totalCount >= 10 && maximum >= totalCount * 0.8 && index >=0 && index < 7)
                {
                    outLine += "," + index + "," + (string)categoryList[index];
                    Console.WriteLine(outLine);
                }
                else
                    outLine += ",-1," + "wow";
                outFile.WriteLine(outLine);

            }

            outFile.Close();

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
    }
}
