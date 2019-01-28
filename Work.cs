using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitDestructor
{
    class Work
    {
        private TwitterClient twc;
        private List<string> twt_queue;
        private int th_num;
        private bool done;

        public int Que_count
        {
            get { return twt_queue.Count; }
        }

        public bool Done
        {
            get { return done; }
        }

        public Work(int th, TwitterClient orig, List<string> tweets)
        {
            th_num = th;
            twc = new TwitterClient(orig);
            twt_queue = tweets;
        }

        public void delete_tweets()
        { 
            int count = 0;
            List<string> d_err = new List<string>();
            done = false;

            foreach (string tid in twt_queue)
            {
                try
                {
                    twc.remove_tweet(tid);

                    count++;
                    if (count % 50 == 0)
                        Console.WriteLine("Thread {0}: {1} tweets deleted", th_num, count);
                }
                catch
                {
                    Console.WriteLine("Thread {0}: Error deleting tweet(id={1})", th_num, tid);
                    d_err.Add(tid);
                }
            }
            done = true;
            twt_queue = d_err;
        }

        public void retry_error()
        {
            if (Que_count > 0)
                delete_tweets();
            done = true;
        }
    }
}
