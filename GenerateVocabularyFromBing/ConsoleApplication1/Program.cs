using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Web;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Threading;
using System.Collections;
using System.Net.Sockets;
using System.Collections.ObjectModel;
using System.Dynamic;
using Newtonsoft.Json;

namespace MicrosoftTranslatorSdk.HttpSamples
{
    public class Word
    {
        public int id;
        public string english;
        public string chinese;
        public string pinyin;
        public string post;
        public Word(int id_ = 0, string english_ = "", string chinese_ = "", string pinyin_ = "", string post_ = "")
        {
            id = id_;
            english = english_;
            chinese = chinese_;
            pinyin = pinyin_;
            if (post_ == "vi" || post_ == "vt")
                post = "v";
            else
                post = post_;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {


            ////read dictionary file
            ////transfer to an arraylist
            //ArrayList words = new ArrayList();
            //string[] lines = File.ReadAllLines(@"../../Words.csv");
            //for (int i = 0; i < lines.Length; i++)
            //{
            //    string[] str = lines[i].Split(',');
            //    if (str.Length == 5)
            //    {
            //        Word tempWord = new Word(Convert.ToInt32(str[0]), str[1], str[2], str[3], str[4]);
            //        words.Add(tempWord);
            //    }
            //}
            //Console.WriteLine("Length of the dictionary is : " + words.Count);


            //read dictionary file
            //transfer to an arraylist

            

            ArrayList words = new ArrayList();
            string[] lines = File.ReadAllLines(@"../../wordlist.txt");

            foreach (string line in lines)
            {
                var json = JsonConvert.DeserializeObject<dynamic>(line);
                //Console.WriteLine("json result is: " + json);
                int count1 = 1;
                int countWord = 0;
                string englishWord = "";
                foreach (var tempObject in json)
                {
                    if (count1 == 1 && tempObject != null)
                    {
                        int count2 = 1;
                        foreach (var subTempObject in tempObject)
                            foreach (var subSubTempObject in subTempObject)
                                if (count2++ == 2)
                                {
                                    //Console.WriteLine("English word is : " + subSubTempObject);
                                    englishWord = (string)subSubTempObject;
                                }
                    }
                    if (count1 == 2 && tempObject != null)
                    {
                        foreach (var subTempObject in tempObject)
                        {
                            int count2 = 1;
                            string postWord = "";
                            foreach (var subSubTempObject in subTempObject)
                            {
                                if (count2 == 1)
                                {
                                    postWord = subSubTempObject;
                                    //Console.WriteLine("POSTagger is : " + subSubTempObject);
                                }
                                if (count2 == 2)
                                {
                                    //Console.WriteLine(subSubTempObject);
                                    //Console.WriteLine("---------------------------");
                                    foreach (var subSubSubTempObject in subSubTempObject)
                                    {
                                        Word tempWord = new Word(countWord++, englishWord, (string)subSubSubTempObject, "", postWord);
                                        //Console.WriteLine("detail of the word is : " + englishWord + " "+ (string)subSubSubTempObject + postWord);
                                        words.Add(tempWord);
                                    }
                                }
                                count2++;
                            }

                        }
                    }
                    count1++;
                }
            }

            Console.WriteLine("size of the dictionary is : " + words.Count);


            ArrayList segmented = new ArrayList();
            ArrayList segmented2 = new ArrayList();
            lines = File.ReadAllLines(@"../../bing.txt");

            foreach(string line in lines)
            {
                segmented.Add(new ArrayList(line.Split(',')));
                segmented2.Add(line);
            }
            Console.WriteLine("size of segmented is : " + segmented.Count);

            int totalMatchedWordCount = 0;

            //StreamWriter outFile2 = new StreamWriter(@"../../dictionary2.txt");
            //foreach (Word w in words)
            //{
            //    outFile2.WriteLine(w.english + "," + w.chinese + "," + w.post);
            //}
            //outFile2.Close();

            //read input file
            //transfer to array of string
            ArrayList news = new ArrayList();
            lines = File.ReadAllLines(@"../../in.txt");
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Replace(",", "").Replace(".", "").Replace(";", "").Replace("[", "").Replace("]", "").Replace("?", "").Replace("'", "").Replace("\"", "").ToLower();
                news.Add(line);
                //Console.WriteLine(line);
            }
            Console.WriteLine("Lines of the news is : " + news.Count);

            StreamWriter outFile = new StreamWriter(@"../../out.txt");

            //main function
            for (int i = 0; i < news.Count; i++)
            {
                string currentSentence = (string)news[i];
                string[] wordsCurrentSentence = ((string)news[i]).Split(' ');

                string bingResult = BingTranslation(currentSentence);
                //string bingResult = "";
                string posTaggerResult = POSTagger(currentSentence);
                //string posTaggerResult = "";

                string[] posTaggerWords = posTaggerResult.Split(' ');
                if (posTaggerWords.Length >= wordsCurrentSentence.Length)
                    for (int j = 0; j < posTaggerWords.Length; j++)
                    {
                        string[] tempStringArray = posTaggerWords[j].Split('_');
                        if (tempStringArray.Length > 1)
                        {
                            string tempPOSTagger = tempStringArray[1];
                            //if (tempPOSTagger.IndexOf("NN") >= 0)
                            //    posTaggerWords[j] = "n";
                            //else if (tempPOSTagger.IndexOf("VB") >= 0)
                            //    posTaggerWords[j] = "v";
                            //else if (tempPOSTagger.IndexOf("IN") >= 0)
                            //    posTaggerWords[j] = "prep";
                            //else if (tempPOSTagger.IndexOf("PRP") >= 0)
                            //    posTaggerWords[j] = "pron";
                            //else if (tempPOSTagger.IndexOf("WP") >= 0)
                            //    posTaggerWords[j] = "pron";
                            //else if (tempPOSTagger.IndexOf("JJ") >= 0)
                            //    posTaggerWords[j] = "adj";
                            //else if (tempPOSTagger.IndexOf("WRB") >= 0)
                            //    posTaggerWords[j] = "adv";
                            //else if (tempPOSTagger.IndexOf("RB") >= 0)
                            //    posTaggerWords[j] = "adv";
                            //else
                            //    posTaggerWords[j] = tempPOSTagger;
                            if (tempPOSTagger.IndexOf("NN") >= 0)
                                posTaggerWords[j] = "noun";
                            else if (tempPOSTagger.IndexOf("VB") >= 0)
                                posTaggerWords[j] = "verb";
                            else if (tempPOSTagger.IndexOf("IN") >= 0)
                                posTaggerWords[j] = "preposition";
                            else if (tempPOSTagger.IndexOf("PRP") >= 0)
                                posTaggerWords[j] = "pronoun";
                            else if (tempPOSTagger.IndexOf("WP") >= 0)
                                posTaggerWords[j] = "pronoun";
                            else if (tempPOSTagger.IndexOf("JJ") >= 0)
                                posTaggerWords[j] = "adjective";
                            else if (tempPOSTagger.IndexOf("WRB") >= 0)
                                posTaggerWords[j] = "adverb";
                            else if (tempPOSTagger.IndexOf("RB") >= 0)
                                posTaggerWords[j] = "adverb";
                            else
                                posTaggerWords[j] = tempPOSTagger;
                        }
                    }
                else
                {
                    Console.WriteLine("POST return error: " + currentSentence + "|||||" + posTaggerResult);
                    continue;
                }


                for (int j = 0; j < wordsCurrentSentence.Length; j++)
                {
                    bool needTranslation = false;
                    string currentEnglishWord = wordsCurrentSentence[j];
                    string currentEnglishWordPOSTagger = posTaggerWords[j];

                    //find all matched words in dictionary for tempwords[j]
                    ArrayList matchedWordsDictionary = new ArrayList();
                    for (int k = 0; k < words.Count; k++)
                        if (((Word)words[k]).english == currentEnglishWord)
                            matchedWordsDictionary.Add(words[k]);

                    Word finalMatchedWordMT = new Word();
                    Word finalMatchedWordPOST = new Word();
                    //find the word in BingTranslate result
                    if (matchedWordsDictionary.Count > 0)
                    {
                        needTranslation = true;
                        //Console.WriteLine("Need to translate!!!!!!!");
                        ArrayList matchedWordsBing = new ArrayList();
                        for (int k = 0; k < matchedWordsDictionary.Count; k++)
                        {
                            Word tempWord = (Word)matchedWordsDictionary[k];
                            if (bingResult.IndexOf(tempWord.chinese) > 0)
                            {
                                matchedWordsBing.Add(tempWord);
                            }
                        }
                        if (matchedWordsBing.Count > 0)
                        {
                            finalMatchedWordMT = (Word)matchedWordsBing[0];
                            for (int k = 0; k < matchedWordsBing.Count; k++)
                                if (((Word)matchedWordsBing[k]).chinese.Length > finalMatchedWordMT.chinese.Length)
                                    finalMatchedWordMT = (Word)matchedWordsBing[k];
                        }
                        //else
                        //finalMatchedWordMT = (Word)matchedWordsDictionary[0];
                    }

                    //find the word using POSTagger
                    if (matchedWordsDictionary.Count > 0)
                    {
                        needTranslation = true;
                        for (int k = 0; k < matchedWordsDictionary.Count; k++)
                            if (((Word)matchedWordsDictionary[k]).post == currentEnglishWordPOSTagger)
                            {
                                finalMatchedWordPOST = (Word)matchedWordsDictionary[k];
                                break;
                            }
                            else if ((((Word)matchedWordsDictionary[k]).post == "ad" || ((Word)matchedWordsDictionary[k]).post == "a") && (currentEnglishWordPOSTagger == "adv" || currentEnglishWordPOSTagger == "adj"))
                            {
                                finalMatchedWordPOST = (Word)matchedWordsDictionary[k];
                                break;
                            }
                    }

                    string lineOutput = "";
                    string segmentedSentence = (string)segmented2[totalMatchedWordCount];
                    string segmentedWord = "";

                    if (needTranslation && finalMatchedWordMT.chinese!="")
                    {
                        string chineseWordBing = finalMatchedWordMT.chinese;
                        foreach (string s in (ArrayList)segmented[totalMatchedWordCount])
                        {
                            
                            if (s.IndexOf(chineseWordBing) != -1)
                                segmentedWord = s;
                        }
                    }

                    if(needTranslation)
                        totalMatchedWordCount++;

                    if (needTranslation)
                    {
                        lineOutput += currentSentence + "|";
                        lineOutput += currentEnglishWord + "|";
                        //lineOutput += bingResult + "|";
                        lineOutput += segmentedSentence + "|";
                        lineOutput += finalMatchedWordMT.chinese + "|";
                        lineOutput += segmentedWord + "|";
                        lineOutput += currentEnglishWordPOSTagger + "|";
                        lineOutput += finalMatchedWordPOST.chinese + "|";
                        lineOutput += ((Word)matchedWordsDictionary[0]).chinese + "|";
                        for (int k = 0; k < matchedWordsDictionary.Count; k++)
                            lineOutput += ((Word)matchedWordsDictionary[k]).chinese + "(" + ((Word)matchedWordsDictionary[k]).post + ")" + "+";
                        lineOutput += "|";
                        outFile.WriteLine(lineOutput);
                    }
                    else
                        lineOutput += currentEnglishWord + "|" + "failed...e";


                }
            }

            outFile.Close();
            //MachineTranslation("this is a book");
            //string test = POSTagger("this is a book\n");

            //string test = BingTranslation("this is a book");

            //Console.WriteLine(test);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        private static string BingTranslation(string s)
        {
            AdmAccessToken admToken;
            string headerValue;
            //Get Client Id and Client Secret from https://datamarket.azure.com/developer/applications/
            //Refer obtaining AccessToken (http://msdn.microsoft.com/en-us/library/hh454950.aspx) 
            AdmAuthentication admAuth = new AdmAuthentication("ErYuShenChromeExtension", "0EsrmwtRP4JV0j7hqoI4fz1BHS2FGYNH+gab6Rx+urE=");
            try
            {
                admToken = admAuth.GetAccessToken();
                // Create a header with the access_token property of the returned token
                headerValue = "Bearer " + admToken.access_token;
                return DetectMethod(headerValue, s);
            }
            catch (WebException e)
            {
                ProcessWebException(e);
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
            return "";
        }

        private static string MachineTranslation(string s)
        {
            try
            {
                // Create a TcpClient. 
                // Note, for this client to work you need to have a TcpServer  
                // connected to the same address as specified by the server, port 
                // combination.
                Int32 port = 4001;
                TcpClient client = new TcpClient("han.d1.comp.nus.edu.sg", port);

                // Translate the passed message into ASCII and store it as a Byte array.
                string message = s;
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing. 
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response. 

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);
                // Close everything.
                stream.Close();
                client.Close();

                return Convert.ToString(responseData);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            return "";
        }

        private static string POSTagger(string s)
        {
            try
            {
                // Create a TcpClient. 
                // Note, for this client to work you need to have a TcpServer  
                // connected to the same address as specified by the server, port 
                // combination.
                Int32 port = 4003;
                TcpClient client = new TcpClient("han.d1.comp.nus.edu.sg", port);

                // Translate the passed message into ASCII and store it as a Byte array.
                string message = s + "\n";
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing. 
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                //Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response. 

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                //Console.WriteLine("Received: {0}", responseData);

                // Close everything.
                stream.Close();
                client.Close();

                return Convert.ToString(responseData);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            return "";
        }

        private static string DetectMethod(string authToken, string s)
        {
            Console.WriteLine("Enter Text to detect language:");
            //Keep appId parameter blank as we are sending access token in authorization header.
            string uri = "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + s + "&to=zh-CHS&from=en";
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Authorization", authToken);
            WebResponse response = null;
            try
            {
                response = httpWebRequest.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    System.Runtime.Serialization.DataContractSerializer dcs = new System.Runtime.Serialization.DataContractSerializer(Type.GetType("System.String"));
                    string result = (string)dcs.ReadObject(stream);
                    Console.WriteLine(string.Format("Language detected:{0}", result));
                    return result;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
        }
        private static void ProcessWebException(WebException e)
        {
            Console.WriteLine("{0}", e.ToString());
            // Obtain detailed error information
            string strResponse = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)e.Response)
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(responseStream, System.Text.Encoding.ASCII))
                    {
                        strResponse = sr.ReadToEnd();
                    }
                }
            }
            Console.WriteLine("Http status code={0}, error message={1}", e.Status, strResponse);
        }
    }
    [DataContract]
    public class AdmAccessToken
    {
        [DataMember]
        public string access_token { get; set; }
        [DataMember]
        public string token_type { get; set; }
        [DataMember]
        public string expires_in { get; set; }
        [DataMember]
        public string scope { get; set; }
    }
    public class AdmAuthentication
    {
        public static readonly string DatamarketAccessUri = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
        private string clientId;
        private string clientSecret;
        private string request;
        private AdmAccessToken token;
        private Timer accessTokenRenewer;
        //Access token expires every 10 minutes. Renew it every 9 minutes only.
        private const int RefreshTokenDuration = 9;
        public AdmAuthentication(string clientId, string clientSecret)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            //If clientid or client secret has special characters, encode before sending request
            this.request = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=http://api.microsofttranslator.com", HttpUtility.UrlEncode(clientId), HttpUtility.UrlEncode(clientSecret));
            this.token = HttpPost(DatamarketAccessUri, this.request);
            //renew the token every specfied minutes
            accessTokenRenewer = new Timer(new TimerCallback(OnTokenExpiredCallback), this, TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
        }
        public AdmAccessToken GetAccessToken()
        {
            return this.token;
        }
        private void RenewAccessToken()
        {
            AdmAccessToken newAccessToken = HttpPost(DatamarketAccessUri, this.request);
            //swap the new token with old one
            //Note: the swap is thread unsafe
            this.token = newAccessToken;
            Console.WriteLine(string.Format("Renewed token for user: {0} is: {1}", this.clientId, this.token.access_token));
        }
        private void OnTokenExpiredCallback(object stateInfo)
        {
            try
            {
                RenewAccessToken();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Failed renewing access token. Details: {0}", ex.Message));
            }
            finally
            {
                try
                {
                    accessTokenRenewer.Change(TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Failed to reschedule the timer to renew access token. Details: {0}", ex.Message));
                }
            }
        }
        private AdmAccessToken HttpPost(string DatamarketAccessUri, string requestDetails)
        {
            //Prepare OAuth request 
            WebRequest webRequest = WebRequest.Create(DatamarketAccessUri);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            byte[] bytes = Encoding.ASCII.GetBytes(requestDetails);
            webRequest.ContentLength = bytes.Length;
            using (Stream outputStream = webRequest.GetRequestStream())
            {
                outputStream.Write(bytes, 0, bytes.Length);
            }
            using (WebResponse webResponse = webRequest.GetResponse())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AdmAccessToken));
                //Get deserialized object from JSON stream
                AdmAccessToken token = (AdmAccessToken)serializer.ReadObject(webResponse.GetResponseStream());
                return token;
            }
        }
    }
}