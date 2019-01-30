using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitDestructor
{
    using Filter = Tuple<int, object>;

    public static class FilterType
    {
        public const int VAL_RETWEET    = 1 << 1;
        public const int VAL_LIKE       = 2 << 1;
        public const int VAL_BODY       = 3 << 1;    
    }

    enum FilterOp
    {
        OP_AND,
        OP_OR,
        OP_XOR
    }

    class TweetFilter
    {
        FilterApp ft;
        private bool loaded = false;

        public TweetFilter(string config_file)
        {
            if(!System.IO.File.Exists(config_file))
            {
                ft = new FilterApp();
                return;
            }

            XmlDocument cfg = new XmlDocument();
            cfg.Load(config_file);
            XmlNode ft_xml = cfg.GetElementsByTagName("Filter")[0];
            if (ft_xml == null)
                throw new Exception("Failed in config: not valid config");

            ft = parse_xml(ft_xml);
            loaded = true;
        }

        public List<dynamic> apply(dynamic tweets)
        {
            return ft.apply(tweets);
        }

        public bool Load
        {
            get { return loaded; }
        }


        private FilterApp parse_xml(XmlNode ft)
        {
            if(ft.Attributes?["operation"] != null)
            {
                FilterOp op;
                switch(ft.Attributes["operation"].Value)
                {
                    case "or":
                        op = FilterOp.OP_OR;
                        break;
                    case "and":
                        op = FilterOp.OP_AND;
                        break;
                    case "xor":
                        op = FilterOp.OP_XOR;
                        break;
                    default:
                        throw new Exception("Failed in config: no such operation");
                }

                if(ft.ChildNodes.Count < 2)
                    throw new Exception("Failed in config: wrong parity");
                else if(op == FilterOp.OP_XOR && ft.ChildNodes.Count > 2)
                    throw new Exception("Failed in config: xor operation is binary operation");
                else
                {
                    FilterApp f1 = parse_xml(ft.ChildNodes[0]);
                    FilterApp f2 = parse_xml(ft.ChildNodes[1]);
                    FilterApp f = new FilterApp(f1, f2, op);

                    for (int i=2; i<ft.ChildNodes.Count; i++)
                        f = new FilterApp(f, parse_xml(ft.ChildNodes[i]), op);

                    return f;
                }
            }
            else
            {
                if (ft["Type"] == null || ft["Value"] == null)
                    throw new Exception("Failed in config: terminal filter is not valid");

                FilterApp f = null;

                if(ft["Type"].InnerText == "body")
                    f = new FilterApp(new Filter(FilterType.VAL_BODY, ft["Value"].InnerText));
                else if(ft["Type"].InnerText == "retweet")
                {
                    if (!int.TryParse(ft["Value"].InnerText, out int x))
                        throw new Exception("Failed in config: retweet filter must have integer threshold");
                    f = new FilterApp(new Filter(FilterType.VAL_RETWEET, x));
                }
                else if (ft["Type"].InnerText == "like")
                {
                    if (!int.TryParse(ft["Value"].InnerText, out int x))
                        throw new Exception("Failed in config: like filter must have integer threshold");
                    f = new FilterApp(new Filter(FilterType.VAL_LIKE, x));
                }

                return f;
            }
        }


    }

    class FilterApp
    {
        private Filter s_filter;
        private Tuple<FilterApp, FilterApp, FilterOp> m_filter;
        private bool multi = false;
        private bool none = false;

        public bool Multi
        {
            get { return multi; } 
        }

        public FilterApp()
        {
            none = true;
        }

        public FilterApp(Filter sft)
        {
            s_filter = sft;
            multi = false;
        }

        public FilterApp(FilterApp lft, FilterApp rft, FilterOp fop)
        {
            m_filter = new Tuple<FilterApp, FilterApp, FilterOp>(lft, rft, fop);
            multi = true;
        }

        public List<dynamic> apply(dynamic tweets)
        {
            List<dynamic> result = new List<dynamic>();

            foreach(dynamic t in tweets)
            {
                if (!check(t))
                    result.Add(t);
            }

            return result;
        }

        public bool check(dynamic tweet)
        {
            if (none)
                return true;

            if(!multi)
            {
                switch(s_filter.Item1 & 0b11110)
                {
                    case FilterType.VAL_RETWEET:
                        if (int.Parse(tweet.retweet_count.Value) >= (int)s_filter.Item2)
                            return true;
                        break;

                    case FilterType.VAL_LIKE:
                        if (int.Parse(tweet.favorite_count.Value) >= (int)s_filter.Item2)
                            return true;
                        break;

                    case FilterType.VAL_BODY:
                        if (tweet.full_text.Value.IndexOf((string)s_filter.Item2) > -1)
                            return true;
                        break;
                }
                return false;
            }
            else
            {
                switch(m_filter.Item3)
                {
                    case FilterOp.OP_AND:
                        return m_filter.Item1.check(tweet) & m_filter.Item2.check(tweet);

                    case FilterOp.OP_OR:
                        return m_filter.Item1.check(tweet) | m_filter.Item2.check(tweet);

                    case FilterOp.OP_XOR:
                        return m_filter.Item1.check(tweet) ^ m_filter.Item2.check(tweet);

                    default: return false;
                }
            }
        }

    }
}
