using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace TwitDestructor
{
    class Program
    {
        const string API_KEY = "";
        const string API_SECRET = "";

        static void Main(string[] args)
        {
            try
            {
                TwitterClient twc = new TwitterClient(API_KEY, API_SECRET);
                
                Console.WriteLine("Requesting Twitter Tokens..");
                twc.request_token();

                Console.Write("Complete Twitter auth, and enter the pin > ");
                System.Diagnostics.Process.Start(twc.authenticate_url());

                string pin = Console.ReadLine();
                Console.WriteLine("Verifying pin inputs..");
                twc.authenticate(pin);
                

                dynamic cred = JObject.Parse(twc.oauth_send("https://api.twitter.com/1.1/account/verify_credentials.json", "GET"));
                Console.WriteLine("Welcome, @" + cred.screen_name.ToString() + "!");
                Console.WriteLine("Type the path of 'tweet.js' or drag&drop it!");
                Console.Write("> ");
                string archive_path = Console.ReadLine();

                if (!File.Exists(archive_path))
                    throw new Exception("No such file!");

                string contents;
                using (StreamReader sr = new StreamReader(archive_path, System.Text.Encoding.UTF8))
                    contents = sr.ReadToEnd();

                contents = "{\"data\":" + contents.Substring(25).Replace(" ", "").Replace("\r", "").Replace("\n", "") + "}";
                dynamic tweets = JObject.Parse(contents);

                Console.WriteLine("{0} tweets loaded!", tweets.data.Count);
                List<String> d_err = new List<string>();

                foreach (dynamic t in tweets)
                {
                    try
                    {
                        twc.remove_tweet(t.id_str.ToString());
                    }
                    catch
                    {
                        Console.WriteLine("Error deleting tweet(id=" + t.id_str.ToString());
                        d_err.Add(t.id_str.ToString());
                    }
                }

                while(d_err.Count > 0)
                {
                    
                    Console.Write("There were {0} errors, retry? (y/n) > ");
                    string answer = Console.ReadLine();
                    if (answer.ToLower() == "n")
                        break;
                    else if (answer.ToLower() != "y")
                        continue;

                    List<String> n_err = new List<string>();
                    foreach(string tid in d_err)
                    {
                        try
                        {
                            twc.remove_tweet(tid);
                        }
                        catch
                        {
                            Console.WriteLine("Error deleting tweet(id=" + tid);
                           n_err.Add(tid);
                        }
                    }

                    d_err = n_err;
                }

                Console.WriteLine("Done!");
                Console.ReadLine();
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception while executing: " + e.Message);
            }
        }
    }
}
