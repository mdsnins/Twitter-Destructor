using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography;
using System.IO;

namespace TwitDestructor
{
    class TwitterClient
    {
        private string api_key;
        private string api_secret;

        private string token = "";
        private string token_secret ="";


        private string ver = "1.0";
        private string sign_method = "HMAC-SHA1";

        public TwitterClient(string consumer_key, string consumer_secret)
        {
            api_key = consumer_key;
            api_secret = consumer_secret;
        }

        public void set_dev_token(string dtoken, string dsecret)
        {
            token = dtoken;
            token_secret = dsecret;
        }

        private string generate_nonce()
        {
            return Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
        }

        private string get_timestamp()
        {
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64(timeSpan.TotalSeconds).ToString();

        }

        public void remove_tweet(string twt_id)
        {
            try
            {
                oauth_send("https://api.twitter.com/1.1/statuses/destroy/" + twt_id + ".json");
            }
            catch
            {
                throw new Exception("Failed in deleting tweet!");
            }
        }
        
        public void request_token()
        {
            try
            {
                string response = oauth_send("https://api.twitter.com/oauth/request_token");
                string[] data = response.Split('&');

                token = data[0].Substring(12);
                token_secret = data[1].Substring(19);
            }
            catch
            {
                throw new Exception("Failed in requesting token!");
            }
        }

        public string authenticate_url()
        {
            if (token.Length > 0)
                return "https://api.twitter.com/oauth/authenticate?oauth_token=" + token;
            else
                throw new Exception("Token isn't generated");
        }

        public void authenticate(string pin)
        {
            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param["oauth_verifier"] = pin;
                string response = oauth_send("https://api.twitter.com/oauth/access_token", "POST", param);
                string[] data = response.Split('&');

                token = data[0].Substring(12);
                token_secret = data[1].Substring(19);
            }
            catch
            {
                throw new Exception("Failed in authenticate!");
            }
        }
           
        public string oauth_send(string target, string method="POST", Dictionary<string, string> user_data = null)
        {
            string send_string = "";

            // unique request details
            string nonce = generate_nonce();
            string timestamp = get_timestamp();

            // create oauth signature
            string base_format = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
                            "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}";


            string param_string = string.Format(base_format, api_key, nonce, sign_method, timestamp, token, ver);

            if (user_data != null)
                foreach (KeyValuePair<String, String> kv in user_data)
                {
                    param_string = param_string + "&" + kv.Key + "=" + Uri.EscapeDataString(kv.Value);
                    send_string = send_string + "&" + kv.Key + "=" + Uri.EscapeDataString(kv.Value);
                }
            if (send_string.Length > 0)
                send_string = send_string.Substring(1);
            param_string = method + "&" + Uri.EscapeDataString(target) + "&" + Uri.EscapeDataString(param_string);

            var compositeKey = Uri.EscapeDataString(api_secret) + "&" + Uri.EscapeDataString(token_secret);

            string oauth_signature;
            using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey)))
            {
                oauth_signature = Convert.ToBase64String(
                    hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(param_string)));
            }

            // create the request header
            string headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                               "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                               "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                               "oauth_version=\"{6}\"";

            string authHeader = string.Format(headerFormat,
                                    Uri.EscapeDataString(nonce),
                                    Uri.EscapeDataString(sign_method),
                                    Uri.EscapeDataString(timestamp),
                                    Uri.EscapeDataString(api_key),
                                    Uri.EscapeDataString(token),
                                    Uri.EscapeDataString(oauth_signature),
                                    Uri.EscapeDataString(ver)
                            );

            // make the request
            ServicePointManager.Expect100Continue = false;

            if (method == "GET")
                target = target + "?" + send_string;
            WebRequest request = WebRequest.Create(target);
            request.Headers.Add("Authorization", authHeader);
            request.Method = method;
            request.ContentType = "application/x-www-form-urlencoded";

            if (method == "POST")
            {
                byte[] param_data = Encoding.UTF8.GetBytes(send_string);
                request.ContentLength = param_data.Length;
                Stream data_stream = request.GetRequestStream();
                data_stream.Write(param_data, 0, param_data.Length);
                data_stream.Close();
            }


            WebResponse response = request.GetResponse();
            string responseData = new StreamReader(response.GetResponseStream()).ReadToEnd();
            return responseData;
        }
    }
}
