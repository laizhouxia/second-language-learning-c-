using System;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Web;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Net.Sockets;
using System.Collections.ObjectModel;
using System.Dynamic;
//using Newtonsoft.Json;

namespace testWordAlignment
{
    class Program
    {
        static void Main(string[] args)
        {
            string s = "According to Amine Tazi, who runs two of the town's biggest studios, foreign directors come for the dramatic light as well as the wide variety of landscapes.";
            AdmAccessToken admToken;
            string headerValue;
            //Get Client Id and Client Secret from https://datamarket.azure.com/developer/applications/
            //Refer obtaining AccessToken (http://msdn.microsoft.com/en-us/library/hh454950.aspx) 
            AdmAuthentication admAuth = new AdmAuthentication("ErYuShenChromeExtension", "0EsrmwtRP4JV0j7hqoI4fz1BHS2FGYNH+gab6Rx+urE=");
            ArrayList splitedInt = new ArrayList();
            try
            {
                admToken = admAuth.GetAccessToken();
                DateTime tokenReceived = DateTime.Now;
                // Create a header with the access_token property of the returned token
                headerValue = "Bearer " + admToken.access_token;
                TranslateArray2Method(headerValue,s, splitedInt);
                for (int i = 0; i < splitedInt.Count; i++)
                    Console.WriteLine(splitedInt[i]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        private static void TranslateArray2Method(string authToken,string s, ArrayList splitedInt)
        {
            // Add TranslatorService as a service reference, Address:http://api.microsofttranslator.com/V2/Soap.svc
            TranslatorService.LanguageServiceClient client = new TranslatorService.LanguageServiceClient();
            //Set Authorization header before sending the request
            HttpRequestMessageProperty httpRequestProperty = new HttpRequestMessageProperty();
            httpRequestProperty.Method = "POST";
            httpRequestProperty.Headers.Add("Authorization", authToken);

            // Creates a block within which an OperationContext object is in scope.
            using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                string[] translateArraySourceTexts = {s,""};

                TranslatorService.TranslateOptions translateArrayOptions = new TranslatorService.TranslateOptions(); // Use the default options
                //Keep appId parameter blank as we are sending access token in authorization header.
                TranslatorService.TranslateArray2Response[] translatedTexts = client.TranslateArray2("", translateArraySourceTexts, "en", "zh-CHS", translateArrayOptions);

                Console.WriteLine("The translated texts with alignment info from en to fr are: ");

                Console.WriteLine("Source text:{0}{1}Translated Text:{2}{1}Alignment info:{3}{1}", translateArraySourceTexts[0], Environment.NewLine, translatedTexts[0].TranslatedText, translatedTexts[0].Alignment);

                StreamWriter outFile = new StreamWriter(@"../../out.txt");
                outFile.WriteLine("Source text:{0}{1}Translated Text:{2}{1}Alignment info:{3}{1}", translateArraySourceTexts[0], Environment.NewLine, translatedTexts[0].TranslatedText, translatedTexts[0].Alignment);
                outFile.Close();
                string[] splitedString = translatedTexts[0].Alignment.Split(new char[]{ ':','-',' '});

                for (int i = 0; i < splitedString.Length; i++)
                    splitedInt.Add(Int32.Parse(splitedString[i]));
            }
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
