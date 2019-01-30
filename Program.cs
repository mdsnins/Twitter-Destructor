using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace TwitDestructor
{
    class Program
    {
        #region API Key/Secret Config
        const string API_KEY = "";
        const string API_SECRET = "";
        #endregion  

        private static int th_max = 4;
        private static Work[] works;
        private static Thread[] threads;
<<<<<<< HEAD
        private static TweetFilter ft;
=======
>>>>>>> b0f5ce61d871140bb63c43541f76a3d2ec7e0f0e


        static void Main(string[] args)
        {
<<<<<<< HEAD
            if (args.Length > 1 || (args.Length == 1 && !int.TryParse(args[0], out th_max)))
            {
                Console.Write("Wrong arguments... ");
=======
            if (args.Length > 1 || (args.Length == 1 && int.TryParse(args[0], out th_max)))
            {
                Console.Write("Wrong arguments...");
>>>>>>> b0f5ce61d871140bb63c43541f76a3d2ec7e0f0e
                th_max = 4;
            }
            else if(args.Length == 0)
                th_max = 4;
            else
                th_max = int.Parse(args[0]);

<<<<<<< HEAD
            Console.WriteLine("Program will be run in maximum {0} threads", th_max);

            ft = new TweetFilter("filter.xml");
            if (ft.Load)
                Console.WriteLine("Tweet filter is loaded!");
=======
            Console.WriteLine(" program will be run in maximum {0} threads", th_max);

>>>>>>> b0f5ce61d871140bb63c43541f76a3d2ec7e0f0e

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

                Console.WriteLine("Welcome, @" + twc.get_user_id() + "!");
                

<<<<<<< HEAD
=======
                Console.WriteLine("Welcome, @" + twc.get_user_id() + "!");
>>>>>>> b0f5ce61d871140bb63c43541f76a3d2ec7e0f0e
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
<<<<<<< HEAD

                //parse data into list<string>
                List<string> raw_tweets = new List<string>();

                //Filter Added
                foreach (dynamic t in ft.apply(tweets.data))
                    raw_tweets.Add(t.id_str.Value);

                Console.WriteLine("{0} tweets are safe! Only {1} tweets will be deleted", tweets.data.Count - raw_tweets.Count, raw_tweets.Count);
                Console.WriteLine("Start deleting...");
                List<String> d_err = new List<string>();

=======
                Console.WriteLine("Start deleting...");
                List<String> d_err = new List<string>();

                //parse data into list<string>
                List<string> raw_tweets = new List<string>();


                foreach (dynamic t in tweets.data)
                    raw_tweets.Add(t.id_str.Value);

>>>>>>> b0f5ce61d871140bb63c43541f76a3d2ec7e0f0e
                if (th_max > (raw_tweets.Count / 500))
                    th_max = raw_tweets.Count / 500;

                Console.WriteLine("Deletion will be run in {0} threads", th_max);

                works = new Work[th_max];
                threads = new Thread[th_max];
                for (int i=0; i < th_max; i++)
                {
                    works[i] = new Work(i + 1, twc, raw_tweets.Select((e, idx) => e).Where((e, idx) => idx % th_max == i).ToList());
                    threads[i] = new Thread(new ThreadStart(works[i].delete_tweets));
                }

                foreach (Thread th in threads)
                    th.Start();

                while(true)
                {
                    bool done = true;
                    foreach (Work w in works)
                        done &= w.Done;
                    if (!done)
                        continue;
                    break;
                }

                foreach (Thread th in threads)
                    th.Abort();

                while(true)
                {
                    int errs = 0;
                    for (int i = 0; i < th_max; i++)
                        errs += works[i].Que_count;

                    Console.Write("Deletion is done with totally {0} errors, retry?(y/n) > ", errs);
                    string x = Console.ReadLine();
                    if (x == "n")
                        break;
                    else if (x != "y")
                        Console.WriteLine("Answer with 'y' or 'n'!");
                    else
                    {
                        for (int i = 0; i < th_max; i++)
                        {
                            threads[i] = new Thread(new ThreadStart(works[i].retry_error));
                            threads[i].Start();
                        }

                        while (true)
                        {
                            bool done = true;
                            foreach (Work w in works)
                                done &= w.Done;
                            if (!done)
                                continue;
                            break;
                        }
                        foreach (Thread th in threads)
                            th.Abort();
                    }
<<<<<<< HEAD
                }
=======

                }
                    
                


>>>>>>> b0f5ce61d871140bb63c43541f76a3d2ec7e0f0e
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception while executing: " + e.Message);
            }
        }
    }
}
