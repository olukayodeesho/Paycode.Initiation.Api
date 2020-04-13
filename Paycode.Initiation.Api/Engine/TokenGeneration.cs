using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Paycode.Initiation.Api.Engine
{
    public class TokenGeneration
    {
        public TokenGeneration()
        {

            //Uncomment the following line if using designed components 
            //InitializeComponent(); 
        }


        public static Logger nLogger = LogManager.GetCurrentClassLogger();

        private static readonly DateTime UnixEpoch =
        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long GetCurrentUnixTimestampMillis()
        {

            //TimeZoneInfo timeZoneInfo;
            //DateTime dateTime;
            ////Set the time zone information to US Mountain Standard Time 
            //timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time");
            ////Get date and time in US Mountain Standard Time 
            //dateTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);
            //Print out the date and time
            return (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
            // return (long)(dateTime - UnixEpoch).TotalMilliseconds;
        }
        public static string UpperCaseUrlEncode(string s)
        {
            //HttpUtility.UrlEncode(resourceUrl, Encoding.GetEncoding(ISO_8859_1)).ToUpper(); //EncodeAsISO(resourceUrl);
            char[] temp = HttpUtility.UrlEncode(s, Encoding.GetEncoding(ISO_8859_1)).ToCharArray();
            for (int i = 0; i < temp.Length - 2; i++)
            {
                if (temp[i] == '%')
                {
                    temp[i + 1] = char.ToUpper(temp[i + 1]);
                    temp[i + 2] = char.ToUpper(temp[i + 2]);
                }
            }
            return new string(temp);
        }

        private const String ACCESS_TOKEN = "ACCESS_TOKEN";
        private const String TIMESTAMP = "Timestamp";
        private const String NONCE = "Nonce";
        private const String SIGNATURE_METHOD = "SignatureMethod";
        private const String SIGNATURE = "Signature";
        private const String AUTHORIZATION = "Authorization";
        private const String AUTHORIZATION_REALM = "InterswitchAuth";
        private const String ISO_8859_1 = "ISO-8859-1";
        private const String paycode = "paycode";

        private void Logheaders(WebHeaderCollection hdrCol)
        {


            // If you want it formated in some other way.
            var headers = String.Empty;
            foreach (var key in hdrCol.AllKeys)
                headers += key + "=" + hdrCol[key] + Environment.NewLine;

            nLogger.Info("headers  " + headers);
        }
        public string GetAuthorizationToken(String SessionKey,
             String additionalParameters, Poco.MobileTokenRequest tokenrequest)
        {
            string responseString = "";
            try
            {

                if (SessionKey != Hash512("GENTOKEN" + tokenrequest.oneTimePin + tokenrequest.subscriberId))
                {
                    return "67|Security Validation Failed";
                }
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                ServicePointManager.SecurityProtocol =
                    (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls) | (SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12);

                String httpMethod = "POST";
                String resourceUrl = ConfigurationManager.AppSettings["AresourceUrl"];
                resourceUrl = resourceUrl.Replace("{0}", tokenrequest.subscriberId);//Set the account number in the end point
                String clientId = ConfigurationManager.AppSettings["AclientId"];
                String clientSecretKey = ConfigurationManager.AppSettings["AclientSecretKey"];

                String signatureMethod = "SHA1";//"SHA-1"

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(resourceUrl);
                httpWebRequest.Method = httpMethod;
                String clientIdBase64 = GetAsBase64(clientId);
                String authorization = AUTHORIZATION_REALM + " " + clientIdBase64;


                long timestamp = GetCurrentUnixTimestampMillis() / 1000;
                Guid uuid = Guid.NewGuid();
                String nonce = uuid.ToString().Replace("-", "").Replace("+", "");

                String encodedResourceUrl = UpperCaseUrlEncode(resourceUrl);
                String signatureCipher = httpMethod + "&" + encodedResourceUrl + "&"
                  + timestamp + "&" + nonce + "&" + clientId + "&"
                  + clientSecretKey;



                if (!String.IsNullOrWhiteSpace(additionalParameters))
                    signatureCipher = signatureCipher + "&" + additionalParameters;


                MessageDigest messageDigest = MessageDigest
                  .GetInstance(signatureMethod);
                byte[] signatureBytes = messageDigest
                  .Digest(Encoding.UTF8.GetBytes(signatureCipher));     //    // encode signature as base 64 
                String signature = Convert.ToBase64String(signatureBytes);//.Replace("+","%2B");

                httpWebRequest.Timeout = 60000;
                httpWebRequest.ReadWriteTimeout = 60000;
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.KeepAlive = false;
                //httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, authorization);


                // httpWebRequest.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                String token = "";
                token = GetPassport();

                httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "InterswitchAuth " + clientIdBase64);

                httpWebRequest.Headers.Add(TIMESTAMP, timestamp.ToString());
                httpWebRequest.Headers.Add(NONCE, nonce);
                httpWebRequest.Headers.Add(SIGNATURE_METHOD, signatureMethod);
                httpWebRequest.Headers.Add(SIGNATURE, signature);
                httpWebRequest.Headers.Add(ACCESS_TOKEN, token);

                httpWebRequest.Headers.Add("frontEndPartnerId", tokenrequest.frontEndPartnerId);
                //Authorization: Bearer (Access Token)
                //httpWebRequest.Headers.Add("Authorization", "eyJhbGciOiJSUzI1NiJ9.eyJzY29wZSI6WyJwcm9maWxlIl0sImV4cCI6MTQ3MTYwODQxNCwianRpIjoiNGM5YzEyODAtZTVjMC00OWQwLTkxOGQtZGJiYTI0NDczYjdlIiwiY2xpZW50X2lkIjoiSUtJQURGQjZGNTNGRUUzQ0U0QjY5MUIzNDNDNTVEMjk1NkJFMUNGNEE5QkMifQ.GY7U2UMVvUHSTqz_ybapCxEIA0jHUQdxXL_iuPOE_rcXecz7n0AzsFbM_Nt9bdOGRszQJ8amB3PUf638BR9lVCRWZU73OXN6G5G6o8t-ZjP2GThc4J-34sLl-yZWGZpP4Fu4uMKgz07276QSvMEPWPIBhvUYS2x1PAgXQXUV4ayGT3ps9ROv2uTkMhKgQsWyMUNsvnWSGDSQbVK7AUPFruYAMk-jxo8loY8T3edqRP-rk7ZO48SiLoNhq-YLTU_RLUK76g8c8RtKphOR2HGXOPW0IInxPKvpM5dItg6VxTdeHiJPnEPMlhVcMbvQjbe");

                Logheaders(httpWebRequest.Headers);

                String postData = JsonConvert.SerializeObject(tokenrequest);
                nLogger.Info(" body", postData);
                StreamWriter requestWriter = new StreamWriter(httpWebRequest.GetRequestStream());
                requestWriter.Write(postData);
                requestWriter.Close();


                nLogger.Info("About to Get Response ");
                var response = (HttpWebResponse)httpWebRequest.GetResponse();

                //nLogger.Info("RESPONSE: " + response.StatusCode);
                if (HttpStatusCode.OK == response.StatusCode || HttpStatusCode.Created == response.StatusCode)//Successful
                {
                    // Get the stream containing content returned by the server.
                    Stream dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    responseString = reader.ReadToEnd();

                }
                nLogger.Info("responseString " + responseString);
            }
            catch (WebException ex)
            {


                try
                {

                    nLogger.Error(ex);
                    nLogger.Info(ex.StackTrace);
                    nLogger.Info(ex.Message);

                    using (WebResponse response = ex.Response)
                    {

                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        nLogger.Info("Error code: {0}", httpResponse.StatusCode);
                        nLogger.Info("Status Description: {0}", httpResponse.StatusDescription);
                        string text = "";
                        using (Stream data = response.GetResponseStream())
                        {
                            text = new StreamReader(data).ReadToEnd();
                            nLogger.Info(text);
                        }

                    }

                }
                catch
                {
                }

            }
            catch (Exception ex)
            {
                //nLogger.Info(ex);
                nLogger.Info(ex.ToString());
            }

            return responseString;
        }

        public string QueryPaycode(String SessionKey,
          String subsciberId, Poco.QueryPayCodeStatus querypayCodeStatus)
        {
            string responseString = "";
            try
            {
                nLogger.Info("About to Get Response...  QueryPaycode ");

                //if (SessionKey != Hash512("GENTOKEN" + tokenrequest.accountNo + tokenrequest.oneTimePin + tokenrequest.subscriberId))
                //{
                //    return "67|Security Validation Failed";
                //}
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                ServicePointManager.SecurityProtocol =
                    (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls) | (SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12);

                String httpMethod = "GET";
                String resourceUrl = ConfigurationManager.AppSettings["AresourceUrlForQueryPayCode"];
                resourceUrl = resourceUrl.Replace("{subscriberId}", subsciberId);//Set the account number in the end point
                String clientId = ConfigurationManager.AppSettings["AclientId"];
                String clientSecretKey = ConfigurationManager.AppSettings["AclientSecretKey"];

                String signatureMethod = "SHA1";//"SHA-1"

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(resourceUrl);
                httpWebRequest.Method = httpMethod;
                String clientIdBase64 = GetAsBase64(clientId);
                String authorization = AUTHORIZATION_REALM + " " + clientIdBase64;


                long timestamp = GetCurrentUnixTimestampMillis() / 1000;
                Guid uuid = Guid.NewGuid();
                String nonce = uuid.ToString().Replace("-", "").Replace("+", "");

                String encodedResourceUrl = UpperCaseUrlEncode(resourceUrl);
                String signatureCipher = httpMethod + "&" + encodedResourceUrl + "&"
                  + timestamp + "&" + nonce + "&" + clientId + "&"
                  + clientSecretKey;



                //if (!String.IsNullOrWhiteSpace(additionalParameters))
                //    signatureCipher = signatureCipher + "&" + additionalParameters;


                MessageDigest messageDigest = MessageDigest
                  .GetInstance(signatureMethod);
                byte[] signatureBytes = messageDigest
                  .Digest(Encoding.UTF8.GetBytes(signatureCipher));     //    // encode signature as base 64 
                String signature = Convert.ToBase64String(signatureBytes);//.Replace("+","%2B");

                httpWebRequest.Timeout = 60000;
                httpWebRequest.ReadWriteTimeout = 60000;
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.KeepAlive = false;
                //httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, authorization);


                // httpWebRequest.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                String token = "";
                token = GetPassport();

                httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "InterswitchAuth " + clientIdBase64);

                httpWebRequest.Headers.Add(TIMESTAMP, timestamp.ToString());
                httpWebRequest.Headers.Add(NONCE, nonce);
                httpWebRequest.Headers.Add(SIGNATURE_METHOD, signatureMethod);
                httpWebRequest.Headers.Add(SIGNATURE, signature);
                httpWebRequest.Headers.Add(ACCESS_TOKEN, token);
                httpWebRequest.Headers.Add(paycode, querypayCodeStatus.paycode);
                //httpWebRequest.Headers.Add("frontEndPartnerId", FrontEndPartners.WEMA);
                //Authorization: Bearer (Access Token)
                //httpWebRequest.Headers.Add("Authorization", "eyJhbGciOiJSUzI1NiJ9.eyJzY29wZSI6WyJwcm9maWxlIl0sImV4cCI6MTQ3MTYwODQxNCwianRpIjoiNGM5YzEyODAtZTVjMC00OWQwLTkxOGQtZGJiYTI0NDczYjdlIiwiY2xpZW50X2lkIjoiSUtJQURGQjZGNTNGRUUzQ0U0QjY5MUIzNDNDNTVEMjk1NkJFMUNGNEE5QkMifQ.GY7U2UMVvUHSTqz_ybapCxEIA0jHUQdxXL_iuPOE_rcXecz7n0AzsFbM_Nt9bdOGRszQJ8amB3PUf638BR9lVCRWZU73OXN6G5G6o8t-ZjP2GThc4J-34sLl-yZWGZpP4Fu4uMKgz07276QSvMEPWPIBhvUYS2x1PAgXQXUV4ayGT3ps9ROv2uTkMhKgQsWyMUNsvnWSGDSQbVK7AUPFruYAMk-jxo8loY8T3edqRP-rk7ZO48SiLoNhq-YLTU_RLUK76g8c8RtKphOR2HGXOPW0IInxPKvpM5dItg6VxTdeHiJPnEPMlhVcMbvQjbe");

                Logheaders(httpWebRequest.Headers);

                String postData = JsonConvert.SerializeObject(querypayCodeStatus);
                nLogger.Info("post data query paycode  " + postData + "resource url" + resourceUrl);
                //StreamWriter requestWriter = new StreamWriter(httpWebRequest.GetRequestStream());
                ////requestWriter.Write(postData);
                //requestWriter.Close();


                nLogger.Info("About to Get Response...  ");
                var response = (HttpWebResponse)httpWebRequest.GetResponse();

                //nLogger.Info("RESPONSE: " + response.StatusCode);
                if (HttpStatusCode.OK == response.StatusCode || HttpStatusCode.Created == response.StatusCode)//Successful
                {
                    // Get the stream containing content returned by the server.
                    Stream dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    responseString = reader.ReadToEnd();

                }
                nLogger.Info("responseString " + responseString);
            }
            catch (WebException ex)
            {


                try
                {

                    nLogger.Error(ex);
                    nLogger.Info(ex.StackTrace);
                    nLogger.Info(ex.Message);

                    using (WebResponse response = ex.Response)
                    {

                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        nLogger.Info("Error code: {0}", httpResponse.StatusCode);
                        nLogger.Info("Status Description: {0}", httpResponse.StatusDescription);
                        string text = "";
                        using (Stream data = response.GetResponseStream())
                        {
                            text = new StreamReader(data).ReadToEnd();
                            nLogger.Info(text);
                        }

                    }

                }
                catch
                {
                }

            }
            catch (Exception ex)
            {
                //nLogger.Info(ex);
                nLogger.Info(ex.ToString());
            }

            return responseString;
        }


        public string CancelPaycode(String SessionKey,
    String subsciberId, Poco.CancelPayCodeRequest cancelPayCodeReq)
        {
            string responseString = "";
            try
            {
                nLogger.Info("About to Get Response...  CancelPaycode ");
                //if (SessionKey != Hash512("GENTOKEN" + tokenrequest.accountNo + tokenrequest.oneTimePin + tokenrequest.subscriberId))
                //{
                //    return "67|Security Validation Failed";
                //}
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                ServicePointManager.SecurityProtocol =
                    (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls) | (SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12);

                String httpMethod = "DELETE";
                String resourceUrl = ConfigurationManager.AppSettings["AresourceUrlForCancelPayCode"];
                //resourceUrl = resourceUrl.Replace("{0}", subsciberId);//Set the account number in the end point
                String clientId = ConfigurationManager.AppSettings["AclientId"];
                String clientSecretKey = ConfigurationManager.AppSettings["AclientSecretKey"];

                String signatureMethod = "SHA1";//"SHA-1"

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(resourceUrl);
                httpWebRequest.Method = httpMethod;
                String clientIdBase64 = GetAsBase64(clientId);
                String authorization = AUTHORIZATION_REALM + " " + clientIdBase64;


                long timestamp = GetCurrentUnixTimestampMillis() / 1000;
                Guid uuid = Guid.NewGuid();
                String nonce = uuid.ToString().Replace("-", "").Replace("+", "");

                String encodedResourceUrl = UpperCaseUrlEncode(resourceUrl);
                String signatureCipher = httpMethod + "&" + encodedResourceUrl + "&"
                  + timestamp + "&" + nonce + "&" + clientId + "&"
                  + clientSecretKey;



                //if (!String.IsNullOrWhiteSpace(additionalParameters))
                //    signatureCipher = signatureCipher + "&" + additionalParameters;


                MessageDigest messageDigest = MessageDigest
                  .GetInstance(signatureMethod);
                byte[] signatureBytes = messageDigest
                  .Digest(Encoding.UTF8.GetBytes(signatureCipher));     //    // encode signature as base 64 
                String signature = Convert.ToBase64String(signatureBytes);//.Replace("+","%2B");

                httpWebRequest.Timeout = 60000;
                httpWebRequest.ReadWriteTimeout = 60000;
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.KeepAlive = false;
                //httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, authorization);


                // httpWebRequest.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                String token = "";
                token = GetPassport();

                httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "InterswitchAuth " + clientIdBase64);

                httpWebRequest.Headers.Add(TIMESTAMP, timestamp.ToString());
                httpWebRequest.Headers.Add(NONCE, nonce);
                httpWebRequest.Headers.Add(SIGNATURE_METHOD, signatureMethod);
                httpWebRequest.Headers.Add(SIGNATURE, signature);
                httpWebRequest.Headers.Add(ACCESS_TOKEN, token);

                httpWebRequest.Headers.Add("frontEndPartnerId", FrontEndPartners.WEMA);
                //Authorization: Bearer (Access Token)
                //httpWebRequest.Headers.Add("Authorization", "eyJhbGciOiJSUzI1NiJ9.eyJzY29wZSI6WyJwcm9maWxlIl0sImV4cCI6MTQ3MTYwODQxNCwianRpIjoiNGM5YzEyODAtZTVjMC00OWQwLTkxOGQtZGJiYTI0NDczYjdlIiwiY2xpZW50X2lkIjoiSUtJQURGQjZGNTNGRUUzQ0U0QjY5MUIzNDNDNTVEMjk1NkJFMUNGNEE5QkMifQ.GY7U2UMVvUHSTqz_ybapCxEIA0jHUQdxXL_iuPOE_rcXecz7n0AzsFbM_Nt9bdOGRszQJ8amB3PUf638BR9lVCRWZU73OXN6G5G6o8t-ZjP2GThc4J-34sLl-yZWGZpP4Fu4uMKgz07276QSvMEPWPIBhvUYS2x1PAgXQXUV4ayGT3ps9ROv2uTkMhKgQsWyMUNsvnWSGDSQbVK7AUPFruYAMk-jxo8loY8T3edqRP-rk7ZO48SiLoNhq-YLTU_RLUK76g8c8RtKphOR2HGXOPW0IInxPKvpM5dItg6VxTdeHiJPnEPMlhVcMbvQjbe");

                Logheaders(httpWebRequest.Headers);
                String postData = JsonConvert.SerializeObject(cancelPayCodeReq);
                nLogger.Info("post data cancel paycode  " + postData + "resource url" + resourceUrl);
                StreamWriter requestWriter = new StreamWriter(httpWebRequest.GetRequestStream());
                requestWriter.Write(postData);
                requestWriter.Close();


                nLogger.Info("About to Get Response...  ");
                var response = (HttpWebResponse)httpWebRequest.GetResponse();

                //nLogger.Info("RESPONSE: " + response.StatusCode);
                if (HttpStatusCode.OK == response.StatusCode || HttpStatusCode.Created == response.StatusCode)//Successful
                {
                    // Get the stream containing content returned by the server.
                    Stream dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    responseString = reader.ReadToEnd();

                }
                nLogger.Info("responseString " + responseString);
            }
            catch (WebException ex)
            {


                try
                {

                    nLogger.Error(ex);
                    nLogger.Info(ex.StackTrace);
                    nLogger.Info(ex.Message);

                    using (WebResponse response = ex.Response)
                    {

                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        nLogger.Info("Error code: {0}", httpResponse.StatusCode);
                        nLogger.Info("Status Description: {0}", httpResponse.StatusDescription);
                        string text = "";
                        using (Stream data = response.GetResponseStream())
                        {
                            text = new StreamReader(data).ReadToEnd();
                            nLogger.Info(text);
                        }

                    }

                }
                catch
                {
                }

            }
            catch (Exception ex)
            {
                //nLogger.Info(ex);
                nLogger.Info(ex.ToString());
            }

            return responseString;
        }



        public string GetPassport()
        {
            string responseString = "";
            try
            {


                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                ServicePointManager.SecurityProtocol =
                    (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls) | (SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12);

                String httpMethod = "POST";
                // String resourceUrl = ConfigurationManager.AppSettings["resourceUrl"];
                String passportUrl = ConfigurationManager.AppSettings["ApassportUrl"];
                // resourceUrl = resourceUrl.Replace("{0}", tokenrequest.subscriberId);
                //Set the account number in the end point
                String clientId = ConfigurationManager.AppSettings["AclientId"];
                String clientSecretKey = ConfigurationManager.AppSettings["AclientSecretKey"];

                String signatureMethod = "SHA1";//"SHA-1"

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(passportUrl);
                httpWebRequest.Method = httpMethod;
                String clientIdBase64 = GetAsBase64(clientId + ":" + clientSecretKey);
                String authorization = "Basic" + " " + clientIdBase64;


                long timestamp = GetCurrentUnixTimestampMillis() / 1000;
                Guid uuid = Guid.NewGuid();
                String nonce = uuid.ToString().Replace("-", "").Replace("+", "");

                String encodedResourceUrl = UpperCaseUrlEncode(passportUrl);
                String signatureCipher = httpMethod + "&" + encodedResourceUrl + "&"
                  + timestamp + "&" + nonce + "&" + clientId + "&"
                  + clientSecretKey;



                MessageDigest messageDigest = MessageDigest
                  .GetInstance(signatureMethod);
                byte[] signatureBytes = messageDigest
                  .Digest(Encoding.UTF8.GetBytes(signatureCipher));     //    // encode signature as base 64 
                String signature = Convert.ToBase64String(signatureBytes);//.Replace("+","%2B");



                httpWebRequest.Timeout = 60000;
                httpWebRequest.ReadWriteTimeout = 60000;
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.KeepAlive = false;

                httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, authorization);
                httpWebRequest.Headers.Add(TIMESTAMP, timestamp.ToString());
                httpWebRequest.Headers.Add(NONCE, nonce);
                httpWebRequest.Headers.Add(SIGNATURE_METHOD, signatureMethod);
                httpWebRequest.Headers.Add(SIGNATURE, signature);

                httpWebRequest.Headers.Add("cache-control", "no-cache");

                String postData = "scope=profile&grant_type=client_credentials"; //JsonConvert.SerializeObject(tokenrequest);
                StreamWriter requestWriter = new StreamWriter(httpWebRequest.GetRequestStream());
                requestWriter.Write(postData);
                requestWriter.Close();


                nLogger.Info("About to Get Response ");
                var response = (HttpWebResponse)httpWebRequest.GetResponse();

                //nLogger.Info("RESPONSE: " + response.StatusCode);
                if (HttpStatusCode.OK == response.StatusCode || HttpStatusCode.Created == response.StatusCode)//Successful
                {
                    // Get the stream containing content returned by the server.
                    Stream dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    responseString = reader.ReadToEnd();
                    AccessToken retobject = JsonConvert.DeserializeObject<AccessToken>(responseString);
                    responseString = retobject.access_token;

                }
            }
            catch (WebException ex)
            {
                try
                {

                    nLogger.Error(ex);
                    nLogger.Info(ex.StackTrace);
                    nLogger.Info(ex.Message);

                    using (WebResponse response = ex.Response)
                    {

                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        nLogger.Info("Error code: {0}", httpResponse.StatusCode);
                        nLogger.Info("Status Description: {0}", httpResponse.StatusDescription);
                        string text = "";
                        using (Stream data = response.GetResponseStream())
                        {
                            text = new StreamReader(data).ReadToEnd();
                            nLogger.Info(text);
                        }

                    }

                }
                catch
                {
                }

            }
            catch (Exception ex)
            {
                nLogger.Info(ex);
            }

            return responseString;
        }

        public class AccessToken
        {
            public String access_token { get; set; }
        }
        private string GetAsBase64(string clientId)
        {
            var bytes = Encoding.UTF8.GetBytes(clientId);
            var base64 = Convert.ToBase64String(bytes);
            return base64;
        }

        private string GetAsBase64(byte[] bytes)
        {
            //var bytes = Encoding.UTF8.GetBytes(clientId);
            var base64 = Convert.ToBase64String(bytes);
            return base64;
        }


        public static string Hash512(String s)
        {
            HashAlgorithm Hasher = new SHA512CryptoServiceProvider();// SHA256CryptoServiceProvider();
            byte[] strBytes = Encoding.UTF8.GetBytes(s);
            byte[] strHash = Hasher.ComputeHash(strBytes);
            return BitConverter.ToString(strHash).Replace("-", "").ToLowerInvariant().Trim();
        }

        public static string ConvertStringToMD5(string ClearText)
        {

            byte[] ByteData = Encoding.ASCII.GetBytes(ClearText);
            //MD5 creating MD5 object.
            MD5 oMd5 = MD5.Create();
            //Hash değerini hesaplayalım.
            byte[] HashData = oMd5.ComputeHash(ByteData);

            //convert byte array to hex format
            StringBuilder oSb = new StringBuilder();

            for (int x = 0; x < HashData.Length; x++)
            {
                //hexadecimal string value
                oSb.Append(HashData[x].ToString("x2"));
            }

            return oSb.ToString();
        }
        private string EncodeAsISO(string resourceUrl)
        {
            Encoding iso = Encoding.GetEncoding(ISO_8859_1);
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes(resourceUrl);
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            string msg = iso.GetString(isoBytes);
            return msg;
        }

        public static string Base64Encode(string plainText)
        {

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string GetSha1Hash(string key, string message)
        {
            var encoding = new System.Text.ASCIIEncoding();

            byte[] keyBytes = encoding.GetBytes(key);
            byte[] messageBytes = encoding.GetBytes(message);

            string Sha1Result = string.Empty;

            using (HMACSHA1 SHA1 = new HMACSHA1(keyBytes))
            {
                var Hashed = SHA1.ComputeHash(messageBytes);
                Sha1Result = Convert.ToBase64String(Hashed);
            }

            return Sha1Result;
        }

    }
}