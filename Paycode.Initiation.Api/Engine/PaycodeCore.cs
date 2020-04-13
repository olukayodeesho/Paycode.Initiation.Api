using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using NLog;
using Paycode.Initiation.Api.dbml;
using Paycode.Initiation.Api.DTO;
using Paycode.Initiation.Api.Engine;

namespace Paycode.Initiation.Api
{
    public class PaycodeCore
    {
        #region Utils 
        public static Logger nLogger = LogManager.GetCurrentClassLogger();

        private static Boolean IsLive()
        {
            try
            {
                return Convert.ToBoolean(ConfigurationManager.AppSettings["IsLive"]);
            }
            catch { return true; }
        }

        public const int tokenLifeTimeInMinutes = 120;
        public static string Hash512(String s)
        {
            HashAlgorithm Hasher = new SHA512CryptoServiceProvider();// SHA256CryptoServiceProvider();
            byte[] strBytes = Encoding.UTF8.GetBytes(s);
            byte[] strHash = Hasher.ComputeHash(strBytes);
            return BitConverter.ToString(strHash).Replace("-", "").ToLowerInvariant().Trim();
        }


        
        private static string PayCodeMsg()
        {
            try
            {
                var val = ConfigurationManager.AppSettings["PayCodeMessage"].ToString();
                if (string.IsNullOrEmpty(val)) { return val; } else { return "USSD: Your ATM PayCode is #PayCode#. Withdraw from any Quickteller Cardless enabled ATM. (Expires in #ExpiryTime#)"; }

            }
            catch { return "USSD: Your ATM PayCode is #PayCode#. Withdraw from any Quickteller Cardless enabled ATM. (Expires in #ExpiryTime#)"; }
        }
        static string ReadFileContent(string fileName)
        {
            try
            {
                var content = string.Empty;

                using (var streamReader = new StreamReader(fileName))
                {
                    content = streamReader.ReadToEnd();
                }

                return content;
            }
            catch (System.Exception ex) { return null; }
        }
        private static string GetMailMailContent(string paycode, string expiryTime)
        {
            try
            {
                var filePath = ConfigurationManager.AppSettings["MailTemplate"];
                var fileContent = ReadFileContent(filePath);
                if (!string.IsNullOrEmpty(fileContent))
                {
                    var customtemp = fileContent.Replace("#PayCode#", paycode)
                        .Replace("#ExpiryTime#", expiryTime);


                    return customtemp;
                }
                return "";
            }
            catch { return ""; }
        }


        private static void LogMail(string mailContent, string recipient)
        {
            try
            { 

            }
            catch (Exception e) { nLogger.Info(e); }
        }

        #endregion

        public static string JsonSerializer(object dataToSerialize)
        {
            if (dataToSerialize == null) return null;
            var str = JsonConvert.SerializeObject(dataToSerialize);
            return str;

        }


        public CardlessWithdrawalTransaction GeneratePayCode(String SessionKey, String SourceChannel, String AccountNumber, String subscriberID, String oneTimePin, String CIF, decimal Amount)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            ServicePointManager.SecurityProtocol =
                (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls) | (SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12);

  

            int tokenLifeTimeInMinute = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["tokenLifeExpiryInMinutes"]);

            String accountType = "00";//00- All 10-Savings 20-Current
            CardlessWithdrawalTransaction transaction = new CardlessWithdrawalTransaction();

            try
            {

                String additionalParameters = "";
 
                MobileTokenRequest tokenrequest = new MobileTokenRequest();

                tokenrequest.subscriberId = subscriberID;//Mandatory - AccountNumber

                
                String transactionreference = DateTime.Now.ToString("yyMMddHHmmssfff");
                String otp = DateTime.Now.ToString("yyMMddHHmmssfff"); 
                tokenrequest.oneTimePin = oneTimePin;
                tokenrequest.providerToken = otp;// GeneratedToken;//

                if (IsLive())
                {
                    nLogger.Info("is live  credential");
                    tokenrequest.paymentMethodTypeCode = PaymentMethodTypes.MMO;//Mandatory this is for production, 
                    tokenrequest.frontEndPartnerId = FrontEndPartners.Octopus;//Mandatory this is for production, 
                    tokenrequest.paymentMethodCode = PaymentMethods.Octopus;//Mandatory
                }
                else
                {
                    nLogger.Info("is test  credential");
                    tokenrequest.frontEndPartnerId = FrontEndPartners.HbOnline;//Mandatory:  this code needs to be agreed with ISW
                    tokenrequest.paymentMethodTypeCode = PaymentMethodTypes.MMO;//Mandatory t 
                    tokenrequest.paymentMethodCode = PaymentMethods.Octopus;//Mandatory
                }

                //tokenrequest.paymentMethodTypeCode = PaymentMethodTypes.MMO ;//Mandatory this is for test


                tokenrequest.tokenLifeTimeInMinutes = tokenLifeTimeInMinute;
                tokenrequest.payWithMobileChannel = PayWithMobileChannels.ATM;//ATM, POS,MOBILE and WEB.
                //tokenrequest.autoEnroll = "true";
                //tokenrequest.accountNo = AccountNumber;
                //tokenrequest.accountType = accountType;
                tokenrequest.ttid = "1234";

                tokenrequest.transactionType = TransactionTypes.CashWithdrawal; //A code to indicate the transaction type e.g Payment, Cash Withdrawal, Deposit
                tokenrequest.codeGenerationChannel = CodeGenerationChannels.Mobile;//Conditional//A code to identify the channel where the code is generated e.g. USSD, Mobile et al
                tokenrequest.Amount = Amount;// 9000.00;//Conditional

                
                //if (SessionKey != Hash512("GENTOKEN" + AccountNumber + Amount + subscriberID + oneTimePin + SourceChannel + CIF))
                if (false)

                {
                    transaction.TransactionReference = "67|Security Validation Failed";
                }
                else
                if ((Amount % 1000) != 0)
                {
                    transaction.TransactionReference = "67|Amount must be in multiples of 1000 (NGN).";
                }
                else if (Amount > 20000 || Amount < 1000)
                {
                    transaction.TransactionReference = "67|Amount cannot be greater than 20000 and less than 1000.";
                }
                else
                {

                    String hashKey = Hash512("GENTOKEN" + tokenrequest.oneTimePin + tokenrequest.subscriberId);
                    nLogger.Info("token request payload  :::::: " + JsonSerializer(tokenrequest));
                    TokenGeneration generation = new TokenGeneration();
                    String response = generation.GetAuthorizationToken(hashKey, additionalParameters, tokenrequest);
                    nLogger.Info("Before Getting AUthorisation");
                    nLogger.Info("response :::::: " + response);

                    if ((response + "").StartsWith("67|"))
                    {
                        transaction.TransactionReference = response;
                    }
                    else
            if (response.Length > 3)
                    {
                        nLogger.Info("response :::2::: " + response);
                        TokenRequest retobject = JsonConvert.DeserializeObject<TokenRequest>(response);

                        //Send WICODE to registered Phone Number
                        //Place Lien On Account
                        //Check Available Balance
                        nLogger.Info("response :::3::: " + response);
                        transaction = new CardlessWithdrawalTransaction()
                        {
                            SourceChannel = SourceChannel,
                            AccountNumber = AccountNumber,
                            CodeGenerationChannel = tokenrequest.codeGenerationChannel,
                            CIF = CIF,
                            FrontEndPartnerId = tokenrequest.frontEndPartnerId,
                            OneTimePassword = Hash512(tokenrequest.oneTimePin),
                            PaymentMethodCode = tokenrequest.paymentMethodCode,
                            PaymentMethodTypeCode = tokenrequest.paymentMethodTypeCode,
                            PayWithMobileChannel = tokenrequest.payWithMobileChannel,
                            ProviderToken = tokenrequest.providerToken,
                            RequestDate = DateTime.Now,
                            TokenUsageCount = 0,
                            AmountAuthorized = 0,
                            TokenLifeTimeInMinutes = tokenrequest.tokenLifeTimeInMinutes,
                            TransactionAmount = tokenrequest.Amount,
                            TransactionType = tokenrequest.transactionType,
                            TransactionReference = transactionreference,
                            PayWithMobileToken = retobject.payWithMobileToken,//WICODE
                            IsCanceled = false,
                            IsExpired = false,
                            IsTokenUsed = false
                        };



                        String signatureMethod = "SHA1";
                        String CypherKey = String.Format("{0}|{1}|{2}", transaction.AccountNumber, transaction.ProviderToken, transaction.TransactionType);
                        MessageDigest messageDigest = MessageDigest
                            .GetInstance(signatureMethod);
                        byte[] signatureBytes = messageDigest
                          .Digest(Encoding.UTF8.GetBytes(CypherKey));  // encode signature as base 64              
                        String signature = Convert.ToBase64String(signatureBytes);
                        nLogger.Info("response :::5::: " + signature);
                        transaction.AuthorizationSessionKey = signature;


                        DateTime ExpiryDate = transaction.RequestDate.Value.AddMinutes(Convert.ToInt32(transaction.TokenLifeTimeInMinutes));


                        transaction.ExpiryDate = ExpiryDate;
                        nLogger.Info("response :::6::: " + ExpiryDate);
                        nLogger.Info("response :::7:: " + JsonConvert.SerializeObject(transaction));
                        var clearPayCode = transaction.PayWithMobileToken;

                        //send mail and sms 
                    }
                    else
                    {
                        nLogger.Info(response);
                        transaction.TransactionReference = "67|Cannot generate Paycode at the moment. Please try again later.";//Could not generate WICode
                    }
                }

            }
            catch (WebException ex)
            {
                nLogger.Info(ex.ToString());
                try
                {
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
                { nLogger.Info("Something went wrong"); }


                transaction.TransactionReference = "67|Cannot generate Paycode at the moment. Please try again later.";
                nLogger.Error(ex);


            }
            catch (Exception ex)
            {
                transaction.TransactionReference = "67|Cannot generate Paycode at the moment. Please try again later.";
                nLogger.Error(ex);
            }

            return transaction;


        }


        public string CancelPayCode(String SessionKey, String subsriberId, String frontEndPartner, String transactionRef)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            ServicePointManager.SecurityProtocol =
                (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls) | (SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12);

            var iswResponse = string.Empty;
            try
            {

                String additionalParameters = "";
                 

                CancelPayCodeRequest req = new CancelPayCodeRequest();
                req.frontEndPartner = frontEndPartner;
                req.transactionRef = transactionRef;

                //String hashKey = Hash512("GENTOKEN" + tokenrequest.accountNo + tokenrequest.oneTimePin + tokenrequest.subscriberId);

                TokenGeneration generation = new TokenGeneration();
                iswResponse = generation.CancelPaycode("", subsriberId, req);
                nLogger.Info("Before Getting AUthorisation");
                nLogger.Info("response :::::: " + iswResponse);



            }
            catch (WebException ex)
            {
                nLogger.Info(ex.ToString());
                try
                {
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
                { nLogger.Info("Something went wrong"); }

                nLogger.Error(ex);

            }
            catch (Exception ex)
            {
                nLogger.Error(ex);
            }

            return iswResponse;


        }

        public string QueryPayCode(String SessionKey, String payCode, String subscriberId)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            ServicePointManager.SecurityProtocol =
                (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls) | (SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12);

            var iswResponse = string.Empty;
            try
            {

                String additionalParameters = "";

            
                QueryPayCodeStatus req = new QueryPayCodeStatus();
                req.paycode = payCode;
                req.subscriberID = subscriberId;

                //String hashKey = Hash512("GENTOKEN" + tokenrequest.accountNo + tokenrequest.oneTimePin + tokenrequest.subscriberId);

                TokenGeneration generation = new TokenGeneration();
                iswResponse = generation.QueryPaycode("", subscriberId, req);
                nLogger.Info("Before Getting Authorisation");
                nLogger.Info("response :::::: " + iswResponse);



            }
            catch (WebException ex)
            {
                nLogger.Info(ex.ToString());
                try
                {
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
                { nLogger.Info("Something went wrong"); }

                nLogger.Error(ex);

            }
            catch (Exception ex)
            {
                nLogger.Error(ex);
            }

            return iswResponse;


        }

    }
}