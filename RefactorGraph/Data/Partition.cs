using System.Collections.Generic;
using PCRE;

namespace RefactorGraph
{
    public class Partition
    {
        #region Fields
        public string data;
        public Partition prev;
        public Partition next;
        public Partition inner;

        public string RasterizedData
        {
            get
            {
                var rData = data;
                var cur = inner;
                while (cur != null)
                {
                    rData += cur.RasterizedData;
                    cur = cur.next;
                }
                return rData;
            }
        }
        #endregion

        #region Methods
        public Partition GetRoot()
        {
            var root = this;
            while (root.prev != null)
            {
                root = root.prev;
            }
            return root;
        }

        public void Remove()
        {
            prev.next = next;
            if (next != null)
            {
                next.prev = prev;
            }
            next = null;
            prev = null;
        }

        public void Rasterize()
        {
            data = RasterizedData;
            inner = null;
        }

        public override string ToString()
        {
            return RasterizedData;
        }

        public List<Partition> PartitionByRegexMatch(string pattern, PcreOptions regexOptions = PcreOptions.MultiLine)
        {
            var partitions = new List<Partition>();
            var matches = PcreRegex.Matches(data, pattern, regexOptions);
            Partition cur = null;
            var index = 0;
            foreach (var match in matches)
            {
                if (match.Length == 0)
                {
                    continue;
                }
                if (match.Index > index)
                {
                    var pNoMatch = new Partition
                    {
                        data = data.Substring(index, match.Index - index),
                        prev = cur
                    };
                    if (cur != null)
                    {
                        cur.next = pNoMatch;
                    }
                    cur = pNoMatch;
                }
                var pMatch = new Partition
                {
                    data = match.Value,
                    prev = cur
                };
                if (cur != null)
                {
                    cur.next = pMatch;
                }
                cur = pMatch;
                partitions.Add(pMatch);
                index = match.Index + match.Length;
            }
            if (cur != null)
            {
                if (index < data.Length)
                {
                    var pNoMatch = new Partition
                    {
                        data = data.Substring(index),
                        prev = cur
                    };
                    cur.next = pNoMatch;
                }

                cur = cur.GetRoot();
                inner = cur;
                data = string.Empty;
            }
            return partitions;
        }

        public List<Partition> PartitionByRegexMatch(IEnumerable<string> pattern, PcreOptions regexOptions = PcreOptions.MultiLine)
        {
            var partitions = new List<Partition>();
            Partition cur = null;
            var index = 0;
            foreach (var p in pattern)
            {
                var match = PcreRegex.Match(data.Substring(index), p, regexOptions);
                if (!match.Success || match.Length == 0)
                {
                    partitions.Add(null);
                    continue;
                }
                if (match.Index > 0)
                {
                    var pNoMatch = new Partition
                    {
                        data = data.Substring(index, match.Index),
                        prev = cur
                    };
                    if (cur != null)
                    {
                        cur.next = pNoMatch;
                    }
                    cur = pNoMatch;
                }
                var pMatch = new Partition
                {
                    data = match.Value,
                    prev = cur
                };
                if (cur != null)
                {
                    cur.next = pMatch;
                }
                cur = pMatch;
                partitions.Add(pMatch);
                index += match.Index + match.Length;
            }
            if (cur != null)
            {
                if (index < data.Length)
                {
                    var pNoMatch = new Partition
                    {
                        data = data.Substring(index),
                        prev = cur
                    };
                    cur.next = pNoMatch;
                }

                cur = cur.GetRoot();
                inner = cur;
                data = string.Empty;
            }
            return partitions;
        }

        public Partition PartitionByFirstRegexMatch(string pattern, PcreOptions regexOptions = PcreOptions.MultiLine)
        {
            var match = PcreRegex.Match(data, pattern, regexOptions);
            if (!match.Success || match.Length == 0)
            {
                return null;
            }
            Partition cur = null;
            if (match.Index > 0)
            {
                var pNoMatch = new Partition
                {
                    data = data.Substring(0, match.Index)
                };
                cur = pNoMatch;
            }
            var pMatch = new Partition
            {
                data = match.Value,
                prev = cur
            };
            if (cur != null)
            {
                cur.next = pMatch;
            }
            cur = pMatch;
            inner = cur.GetRoot();
            data = string.Empty;
            return pMatch;
        }
        
        public static bool IsMatch(Partition partition, string pattern, PcreOptions regexOptions = PcreOptions.MultiLine)
        {
            return
                string.IsNullOrEmpty(pattern) ||
                partition != null &&
                PcreRegex.IsMatch(partition.data, pattern, regexOptions);
        }
        #endregion
    }
}