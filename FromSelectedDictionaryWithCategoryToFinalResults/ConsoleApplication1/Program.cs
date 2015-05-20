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


namespace testWordAlignment
{
    public class Word
    {
        public string english;
        public string chinese;
        public string post;
        public int categoryID;
        public string category;
        public Word(string english_ = "", string chinese_ = "", string post_ = "", int categoryID_ = 0, string category_ = "")
        {
            categoryID = categoryID_;
            english = english_;
            chinese = chinese_;
            category = category_;
            post = post_;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            //read dictionary file
            //transfer to an arraylist
            ArrayList words = new ArrayList();
            string[] lines = File.ReadAllLines(@"../../dictionary4.txt");
            for (int i = 0; i < lines.Length; i++)
            {
                string[] str = lines[i].Split(',');
                if (str.Length == 5)
                {
                    Word tempWord = new Word(str[0], str[1], str[2], Convert.ToInt32(str[3]), str[4]);
                    words.Add(tempWord);
                }
            }
            Console.WriteLine("Length of the dictionary is : " + words.Count);




            //read input file
            //transfer to array of string
            ArrayList news = new ArrayList();
            lines = File.ReadAllLines(@"../../in.txt");
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Replace(",", "").Replace(".", "").Replace(";", "").Replace("-", "").Replace("[", "").Replace("]", "").Replace("?", "").Replace("'", "").Replace("\"", "").ToLower();
                news.Add(line);
                //Console.WriteLine(line);
            }
            Console.WriteLine("Lines of the news is : " + news.Count);

            StreamWriter outFile = new StreamWriter(@"../../out.txt");

            string category = "";
            ArrayList categoryList = new ArrayList();

            categoryList.Add("entertainment");
            categoryList.Add("finance");
            categoryList.Add("sports");
            categoryList.Add("world");
            categoryList.Add("tech");
            categoryList.Add("fashion");
            categoryList.Add("travel");

            //main function
            for (int i = 0; i < news.Count; i++)
            {
                string currentSentence = (string)news[i];

                if (currentSentence.Length >= 3 && currentSentence[0] == '$' && currentSentence[1] == '$')
                {
                    category = (string)categoryList[(int)Char.GetNumericValue(currentSentence[2])];
                    continue;
                }

                string[] wordsCurrentSentence = ((string)news[i]).Split(' ');
                ArrayList splitedInt = new ArrayList();
                string bingResult = BingTranslation(currentSentence,splitedInt);
                //string bingResult = "";
                string posTaggerResult = POSTagger(currentSentence);
                //string posTaggerResult = "";
                string segmentedSentence = ChineseSegmentation(bingResult);
                ArrayList segmentedWordList = new ArrayList(segmentedSentence.Split(','));

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
                    Word finalMatchedWordCategory = new Word();

                    string chineseFromWordAlignment = "";
                    for(int k=0;k<splitedInt.Count/4;k++)
                        if(k*4+3<splitedInt.Count)
                        {
                            int index = k * 4;
                            if((int)splitedInt[index + 1]<currentSentence.Length && (int)splitedInt[index + 3] < bingResult.Length)
                            if(currentEnglishWord == currentSentence.Substring((int)splitedInt[index],(int)splitedInt[index+1]-(int)splitedInt[index]+1))
                            {
                                chineseFromWordAlignment = bingResult.Substring((int)splitedInt[index+2], (int)splitedInt[index + 3] - (int)splitedInt[index+2] + 1);
                            }
                        }

                    //find the word in BingTranslate result
                    if (matchedWordsDictionary.Count > 0)
                    {
                        needTranslation = true;
                        //Console.WriteLine("Need to translate!!!!!!!");
                        ArrayList matchedWordsBing = new ArrayList();
                        for (int k = 0; k < matchedWordsDictionary.Count; k++)
                        {
                            Word tempWord = (Word)matchedWordsDictionary[k];
                            if (bingResult.IndexOf(tempWord.chinese) >= 0)
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
                        Console.WriteLine("postagger : " + currentEnglishWordPOSTagger + " "+ finalMatchedWordPOST.chinese);
                    }

                    if (matchedWordsDictionary.Count > 0)
                    {
                        needTranslation = true;
                        
                        foreach(Word w in matchedWordsDictionary)
                        {
                            if (w.category.IndexOf(category) >= 0)
                            {
                                finalMatchedWordCategory = w;
                            }
                        }
                    }

                    string lineOutput = "";
                    
                    string segmentedWord = "";

                    if (needTranslation && finalMatchedWordMT.chinese!="")
                    {
                        string chineseWordBing = finalMatchedWordMT.chinese;
                        foreach (string s in segmentedWordList)
                        {
                            
                            if (s.IndexOf(chineseWordBing) != -1)
                                segmentedWord = s;
                        }
                    }

                    if (needTranslation)
                    {
                        lineOutput += currentSentence + "|";
                        lineOutput += currentEnglishWord + "|";
                        lineOutput += category + "|";
                        lineOutput += segmentedSentence + "|";
                        lineOutput += finalMatchedWordMT.chinese + "|";
                        lineOutput += segmentedWord + "|";
                        lineOutput += finalMatchedWordCategory.chinese + "|";
                        lineOutput += chineseFromWordAlignment + "|";
                        int count = 0;
                        foreach(int tempInt in splitedInt)
                        {
                            lineOutput += tempInt;
                            if(count==1)
                            {
                                lineOutput += "->";
                            }
                            else if(count==0||count==2)
                            {
                                lineOutput += ":";
                            }
                            else
                            {
                                count = -1;
                                lineOutput += " ";
                            }
                            count++;
                        }
                        lineOutput += "|";
                        lineOutput += currentEnglishWordPOSTagger + "|";
                        lineOutput += finalMatchedWordPOST.chinese + "|";
                        lineOutput += ((Word)matchedWordsDictionary[0]).chinese + "|";
                        for (int k = 0; k < matchedWordsDictionary.Count; k++)
                            lineOutput += ((Word)matchedWordsDictionary[k]).chinese + "(" + ((Word)matchedWordsDictionary[k]).post + "," + ((Word)matchedWordsDictionary[k]).category + ")" + "+";
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

        private static string ChineseSegmentation(string s)
        {
            try
            {
                // Create a TcpClient. 
                // Note, for this client to work you need to have a TcpServer  
                // connected to the same address as specified by the server, port 
                // combination.
                Int32 port = 1238;
                TcpClient client = new TcpClient("localhost", port);

                // Translate the passed message into ASCII and store it as a Byte array.
                string message = s + "\n";
                Byte[] data = System.Text.Encoding.UTF8.GetBytes(message);

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
                responseData = System.Text.Encoding.UTF8.GetString(data, 0, bytes);
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

        private static string BingTranslation(string s, ArrayList splitedInt)
        {
            AdmAccessToken admToken;
            string headerValue;
            //Get Client Id and Client Secret from https://datamarket.azure.com/developer/applications/
            //Refer obtaining AccessToken (http://msdn.microsoft.com/en-us/library/hh454950.aspx) 
            AdmAuthentication admAuth = new AdmAuthentication("ErYuShenChromeExtension", "0EsrmwtRP4JV0j7hqoI4fz1BHS2FGYNH+gab6Rx+urE=");
            try
            {
                admToken = admAuth.GetAccessToken();
                DateTime tokenReceived = DateTime.Now;
                // Create a header with the access_token property of the returned token
                headerValue = "Bearer " + admToken.access_token;
                return TranslateArray2Method(headerValue, s, splitedInt);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return "";
        }

        private static string TranslateArray2Method(string authToken, string s, ArrayList splitedInt)
        {
            // Add TranslatorService as a service reference, Address:http://api.microsofttranslator.com/V2/Soap.svc
            ConsoleApplication1.TranslatorService.LanguageServiceClient client = new ConsoleApplication1.TranslatorService.LanguageServiceClient();
            //Set Authorization header before sending the request
            HttpRequestMessageProperty httpRequestProperty = new HttpRequestMessageProperty();
            httpRequestProperty.Method = "POST";
            httpRequestProperty.Headers.Add("Authorization", authToken);

            // Creates a block within which an OperationContext object is in scope.
            using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                string[] translateArraySourceTexts = { s, "" };

                ConsoleApplication1.TranslatorService.TranslateOptions translateArrayOptions = new ConsoleApplication1.TranslatorService.TranslateOptions(); // Use the default options
                //Keep appId parameter blank as we are sending access token in authorization header.
                ConsoleApplication1.TranslatorService.TranslateArray2Response[] translatedTexts = client.TranslateArray2("", translateArraySourceTexts, "en", "zh-CHS", translateArrayOptions);

                Console.WriteLine("The translated texts with alignment info from en to fr are: ");

                Console.WriteLine("Source text:{0}{1}Translated Text:{2}{1}Alignment info:{3}{1}", translateArraySourceTexts[0], Environment.NewLine, translatedTexts[0].TranslatedText, translatedTexts[0].Alignment);

                string[] splitedString = translatedTexts[0].Alignment.Split(new char[] { ':', '-', ' ' });

                for (int i = 0; i < splitedString.Length; i++)
                    splitedInt.Add(Int32.Parse(splitedString[i]));

                return translatedTexts[0].TranslatedText;
            }
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