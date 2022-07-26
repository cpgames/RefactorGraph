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
        public Partition parent;
        #endregion

        #region Properties
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

        public bool IsRoot => prev == null;
        #endregion

        #region Methods
        public Partition GetRoot()
        {
            var cur = this;
            while (!cur.IsRoot)
            {
                cur = cur.prev;
            }
            return cur;
        }

        public void Remove()
        {
            if (parent != null)
            {
                parent.inner = next;
                parent = null;
            }
            if (prev != null)
            {
                prev.next = next;
            }
            if (next != null)
            {
                next.prev = prev;
            }
            next = null;
            prev = null;
        }

        public void Rasterize()
        {
            if (inner != null)
            {
                data = RasterizedData;
                var cur = inner;
                while (cur != null)
                {
                    cur.parent = null;
                    cur = cur.next;
                }
                inner = null;
            }
        }

        public override string ToString()
        {
            return RasterizedData;
        }

        public static List<Partition> PartitionByRegexMatch(Partition partition, string pattern, PcreOptions regexOptions = PcreOptions.MultiLine)
        {
            if (partition == null)
            {
                return null;
            }
            var partitions = new List<Partition>();
            var matches = PcreRegex.Matches(partition.data, pattern, regexOptions);
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
                        data = partition.data.Substring(index, match.Index - index),
                        prev = cur,
                        parent = partition
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
                    prev = cur,
                    parent = partition
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
                if (index < partition.data.Length)
                {
                    var pNoMatch = new Partition
                    {
                        data = partition.data.Substring(index),
                        prev = cur,
                        parent = partition
                    };
                    cur.next = pNoMatch;
                }

                cur = cur.GetRoot();
                partition.inner = cur;
                partition.data = string.Empty;
            }
            return partitions;
        }

        public static List<Partition> PartitionByRegexMatch(Partition partition, IEnumerable<string> pattern, PcreOptions regexOptions = PcreOptions.MultiLine)
        {
            if (partition == null)
            {
                return  null;
            }
            var partitions = new List<Partition>();
            Partition cur = null;
            var index = 0;
            foreach (var p in pattern)
            {
                var match = PcreRegex.Match(partition.data.Substring(index), p, regexOptions);
                if (!match.Success || match.Length == 0)
                {
                    partitions.Add(null);
                    continue;
                }
                if (match.Index > 0)
                {
                    var pNoMatch = new Partition
                    {
                        data = partition.data.Substring(index, match.Index),
                        prev = cur,
                        parent = partition
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
                    prev = cur,
                    parent = partition
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
                if (index < partition.data.Length)
                {
                    var pNoMatch = new Partition
                    {
                        data = partition.data.Substring(index),
                        prev = cur,
                        parent = partition
                    };
                    cur.next = pNoMatch;
                }

                cur = cur.GetRoot();
                partition.inner = cur;
                partition.data = string.Empty;
            }
            return partitions;
        }

        public static Partition PartitionByFirstRegexMatch(Partition partition, string pattern, PcreOptions regexOptions = PcreOptions.MultiLine)
        {
            if (partition == null)
            {
                return null;
            }
            var match = PcreRegex.Match(partition.data, pattern, regexOptions);
            if (!match.Success || match.Length == 0)
            {
                return null;
            }
            Partition cur = null;
            if (match.Index > 0)
            {
                var pNoMatch = new Partition
                {
                    data = partition.data.Substring(0, match.Index),
                    parent = partition
                };
                cur = pNoMatch;
            }
            var pMatch = new Partition
            {
                data = match.Value,
                prev = cur,
                parent = partition
            };
            if (cur != null)
            {
                cur.next = pMatch;
            }
            cur = pMatch;
            if (match.Index + match.Length < partition.data.Length)
            {
                var pNoMatch = new Partition
                {
                    data = partition.data.Substring(match.Index + match.Length),
                    prev = cur,
                    parent = partition
                };
                cur.next = pNoMatch;
            }
            partition.inner = pMatch.GetRoot();
            partition.data = string.Empty;
            return pMatch;
        }

        public static bool IsMatch(Partition partition, string pattern, PcreOptions regexOptions = PcreOptions.MultiLine)
        {
            return
                string.IsNullOrEmpty(pattern) ||
                (partition != null &&
                    PcreRegex.IsMatch(partition.data, pattern, regexOptions));
        }

        public static void Swap(Partition a, Partition b)
        {
            if (a == null || b == null)
            {
                return;
            }

            var aParent = a.parent;
            var bParent = b.parent;

            if (aParent != null)
            {
                if (a.IsRoot)
                {
                    aParent.inner = b;
                }
            }
            if (bParent != null)
            {
                if (b.IsRoot)
                {
                    bParent.inner = a;
                }
            }
            a.parent = bParent;
            b.parent = aParent;

            var aPrev = a.prev;
            var aNext = a.next;
            var bPrev = b.prev;
            var bNext = b.next;
            if (aPrev != null)
            {
                aPrev.next = b;
            }
            if (aNext != null)
            {
                aNext.prev = b;
            }
            if (bPrev != null)
            {
                bPrev.next = a;
            }
            if (bNext != null)
            {
                bNext.prev = a;
            }
            a.prev = bPrev;
            a.next = bNext;
            b.prev = aPrev;
            b.next = aNext;
        }
        #endregion
    }
}