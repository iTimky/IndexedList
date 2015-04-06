using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IndexedList
{
    public interface IReadOnlyHashIndex<TItem> : IReadOnlyCollection<TItem>
    {
        string FieldName { get; }
        IReadOnlyList<TItem> this[int hash] { get; }

    }
    public class ReadOnlyHashIndex<TItem> : IReadOnlyHashIndex<TItem>
    {
        protected readonly HashIndex<TItem> HashIndex;

        public ReadOnlyHashIndex(HashIndex<TItem> hashIndex)
        {
            HashIndex = hashIndex;
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return HashIndex.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get { return HashIndex.Count; } }

        public string FieldName { get { return HashIndex.FieldName; } }
        public IReadOnlyList<TItem> this[int hash]
        {
            get { return HashIndex[hash]; }
        }
    }


    public class HashIndex<TItem> : ICollection<TItem>
    {
        private readonly string _fieldName;
        public string FieldName { get { return _fieldName; } }
        protected readonly Dictionary<int, List<TItem>> HashDict = new Dictionary<int, List<TItem>>();
        protected Func<TItem, object> MemberReflector;
        protected readonly IReadOnlyList<TItem> EmptyList = new List<TItem>().AsReadOnly();

        public HashIndex(string fieldName, List<TItem> items)
        {
            _fieldName = fieldName;
            if (items != null && items.Count != 0)
                AddRange(items);
        }

        public IReadOnlyList<TItem> this[int hash]
        {
            get
            {
                if (!HashDict.ContainsKey(hash))
                    return EmptyList;

                return HashDict[hash].AsReadOnly();
            }
        }

        public void AddRange(IEnumerable<TItem> items)
        {
            foreach (var item in items)
                Add(item);
        }

        public bool Remove(TItem item)
        {
            int hash = GetMemberHash(item);
            if (!HashDict.ContainsKey(hash))
                return false;

            List<TItem> valueItems = HashDict[hash];
            bool isSuccess = valueItems.Remove(item);
            if (valueItems.Count == 0)
                HashDict.Remove(hash);

            if (isSuccess)
                Count--;

            return isSuccess;
        }

        public void Remove(IEnumerable<TItem> items)
        {
            foreach (var item in items)
                Remove(item);
        }

        private int GetMemberHash(TItem item)
        {
            if (MemberReflector == null)
            {
                Type type = typeof (TItem);
                FieldInfo fieldInfo = type.GetField(FieldName);
                if (fieldInfo == null)
                {
                    var propertyInfo = type.GetProperty(FieldName);
                    MemberReflector = tItem => propertyInfo.GetValue(tItem);
                }
                else
                    MemberReflector = tItem => fieldInfo.GetValue(item);
            }

            var memberValue = MemberReflector(item);
            if (memberValue != null)
                return memberValue.GetHashCode();

            return 0;
        }


        public IReadOnlyHashIndex<TItem> AsReadOnly()
        {
            return new ReadOnlyHashIndex<TItem>(this);
        }

        #region ICollection
        public IEnumerator<TItem> GetEnumerator()
        {
            return HashDict.SelectMany(v => v.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TItem item)
        {
            int hash = GetMemberHash(item);

            if (!HashDict.ContainsKey(hash))
                HashDict[hash] = new List<TItem> { item };
            else
                HashDict[hash].Add(item);

            Count++;
        }

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            HashDict.Values.SelectMany(item => item).ToList().CopyTo(array, arrayIndex);
        }

        bool ICollection<TItem>.Remove(TItem item)
        {
            return Remove(item);
        }

        public int Count { get; private set; }
        public bool IsReadOnly { get; private set; }

        public void Clear()
        {
            HashDict.Clear();
            Count = 0;
        }

        public bool Contains(TItem item)
        {
            return HashDict.Values.SelectMany(a => a).Contains(item);
        }
        #endregion
    }
}