﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdvUtils;

namespace ClusterUrl
{
    class UrlEntity : IComparable<UrlEntity>
    {
        public string strUrl;
        public List<QueryItem> featureSet;


        public int CompareTo(UrlEntity other)
        {
            return featureSet.Count - other.featureSet.Count;
        }
    }

    class QueryItem
    {
        public string strQuery;
        public int freq;
        public double clickweight;
    }

    class Program
    {
        public static string ToDBC(string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new string(c);
        }

        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("ClusterUrl [input:query_url_freq_clickweight file name] [output:query_clusterId_freq_clickweight file name] [input:min frequency] [input:min clickweight]");
                return;
            }

            StreamReader sr = new StreamReader(args[0]);
            StreamWriter sw = new StreamWriter(args[1]);
            UrlEntity entity = null;
            int maxLine = 0;
            int minFreq = int.Parse(args[2]);
            double minClickWeight = double.Parse(args[3]);
            string strLine = null;
            while ((strLine = sr.ReadLine()) != null)
            {
                //Query \t Url \t Freq \t Clickweight
                string[] items = strLine.Split('\t');

                string strQuery = items[0].ToLower().Trim();
                strQuery = ToDBC(strQuery).ToLower().Trim();

                items[1] = items[1].ToLower().Trim();

                string strUrl = items[1];
                int freq = int.Parse(items[2]);
                if (freq < minFreq)
                {
                    continue;
                }

                double clickweight = double.Parse(items[3]);
                if (clickweight < minClickWeight)
                {
                    continue;
                }

                if (maxLine % 100000 == 0)
                {
                    Console.WriteLine("Line No. {0}", maxLine);
                }
                maxLine++;

                QueryItem qi = new QueryItem();
                qi.strQuery = strQuery;
                qi.freq = freq;
                qi.clickweight = clickweight;

                if (entity == null)
                {
                    entity = new UrlEntity();
                    entity.strUrl = strUrl;
                    entity.featureSet = new List<QueryItem>();
                    entity.featureSet.Add(qi);
                }
                else if (entity.strUrl == strUrl)
                {
                    entity.featureSet.Add(qi);
                }
                else
                {
                    if (entity.featureSet.Count > 1 &&
                        entity.featureSet.Count < 100)
                    {
                        foreach (QueryItem item in entity.featureSet)
                        {
                            sw.WriteLine("{0}\t{1}\t{2}\t{3}", item.strQuery, entity.strUrl, item.freq, item.clickweight);
                        }
                    }

                    entity.strUrl = strUrl;
                    entity.featureSet.Clear();
                    entity.featureSet.Add(qi);
                }

            }

            if (entity.featureSet.Count > 1 && entity.featureSet.Count < 100)
            {
                foreach (QueryItem item in entity.featureSet)
                {
                    sw.WriteLine("{0}\t{1}\t{2}\t{3}", item.strQuery, entity.strUrl, item.freq, item.clickweight);
                }
            }

            sr.Close();
            sw.Close();
        }
    }
}
