﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using PCRE;

namespace RefactorGraph
{
    public class Partition
    {
        #region Fields
        [XmlIgnore]
        private string _data;
        [XmlIgnore]
        public Partition prev;
        [XmlIgnore]
        public Partition next;
        [XmlIgnore]
        public Partition inner;
        #endregion

        #region Properties
        [XmlIgnore]
        public string Data
        {
            get => !IsPartitioned ? _data : GetInnerData();
            set
            {
                if (IsPartitioned)
                {
                    throw new Exception("Can't modify data of partitioned chunk");
                }
                _data = value;
            }
        }
        [XmlIgnore]
        public Partition First => !IsPartitioned ? null : inner.next;
        [XmlIgnore]
        public Partition Last
        {
            get
            {
                if (!IsPartitioned)
                {
                    return null;
                }
                var cur = First;
                while (cur.next != null)
                {
                    cur = cur.next;
                }
                return cur;
            }
        }

        public bool IsPartitioned => inner != null;
        public bool IsRoot => prev == null;
        #endregion

        #region Methods
        private string GetInnerData()
        {
            var innerData = string.Empty;
            var c = inner;
            while (c != null)
            {
                innerData += c.Data;
                c = c.next;
            }
            return innerData;
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
            if (!IsPartitioned)
            {
                return;
            }
            _data = GetInnerData();
            inner = null;
        }
        
        public override string ToString()
        {
            return Data;
        }

        public void PartitionByIndex(int index)
        {
            if (IsPartitioned)
            {
                throw new Exception("Data is already partitioned");
            }
            if (index > _data.Length)
            {
                throw new Exception("Index exceeds data length");
            }
            inner = new Partition();
            var cur = inner;
            if (index > 0)
            {
                var part1 = new Partition
                {
                    prev = cur,
                    Data = _data.Substring(0, index)
                };
                cur.next = part1;
                cur = part1;
            }
            if (index < _data.Length)
            {
                var part2 = new Partition
                {
                    prev = cur,
                    Data = _data.Substring(index)
                };
                cur.next = part2;
            }
        }

        public Partition PartitionByIndexAndLength(int index, int length)
        {
            if (IsPartitioned)
            {
                throw new Exception("Data is already partitioned");
            }
            if (index + length > _data.Length)
            {
                throw new Exception("Index+length exceeds data length");
            }
            inner = new Partition();
            var cur = inner;
            if (index > 0)
            {
                var part1 = new Partition
                {
                    prev = cur,
                    Data = _data.Substring(0, index)
                };
                cur.next = part1;
                cur = part1;
            }
            var part2 = new Partition
            {
                prev = cur,
                Data = _data.Substring(index, length)
            };
            cur.next = part2;
            cur = part2;
            if (index + length < _data.Length)
            {
                var part3 = new Partition
                {
                    prev = cur,
                    Data = _data.Substring(index + length)
                };
                cur.next = part3;
            }
            return part2;
        }

        public List<Partition> PartitionByIndexAndLength(List<KeyValuePair<int, int>> indexLengths)
        {
            if (IsPartitioned)
            {
                throw new Exception("Data is already partitioned");
            }
            var partitions = new List<Partition>();
            if (indexLengths.Count == 0)
            {
                return partitions;
            }
            inner = new Partition();
            var cur = inner;
            var index = 0;
            indexLengths = indexLengths.OrderBy(x => x.Key).ToList();
            for (var i = 0; i < indexLengths.Count; i++)
            {
                var indexLength = indexLengths[i];
                var length = indexLength.Key - index;
                if (index + length > _data.Length)
                {
                    throw new Exception("Index+length exceeds data length");
                }
                if (length > 0)
                {
                    var part1 = new Partition
                    {
                        prev = cur,
                        Data = _data.Substring(index, length)
                    };
                    cur.next = part1;
                    cur = part1;
                }
                index = indexLength.Key;
                length = indexLength.Value;
                var part2 = new Partition
                {
                    prev = cur,
                    Data = _data.Substring(index, length)
                };
                cur.next = part2;
                partitions.Add(part2);
                cur = part2;
                index += length;
                if (i < indexLengths.Count - 1)
                {
                    length = indexLengths[i + 1].Key - index;
                }
                else
                {
                    length = _data.Length - index;
                }
                if (index < _data.Length)
                {
                    var part3 = new Partition
                    {
                        prev = cur,
                        Data = _data.Substring(index, length)
                    };
                    cur.next = part3;
                    cur = part3;
                }
                index += length;
            }

            return partitions;
        }

        public Partition PartitionByFirstRegexMatch(string pattern, PcreOptions regexOptions)
        {
            if (IsPartitioned)
            {
                throw new Exception("Data is already partitioned");
            }
            var match = PcreRegex.Match(_data, pattern, regexOptions);
            return match.Success ? PartitionByIndexAndLength(match.Index, match.Length) : null;
        }

        public List<Partition> PartitionByAllRegexMatches(string pattern, PcreOptions regexOptions)
        {
            if (IsPartitioned)
            {
                throw new Exception("Data is already partitioned");
            }
            var matches = PcreRegex.Matches(_data, pattern, regexOptions);
            var indexLengths = matches
                .Select(x => new KeyValuePair<int, int>(x.Index, x.Length))
                .ToList();
            return PartitionByIndexAndLength(indexLengths);
        }
        #endregion
    }
}