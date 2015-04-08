#region usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion



namespace IndexedList
{
    public interface IReadOnlyIndex<out TItem> : IReadOnlyCollection<TItem>
    {
        string FieldName { get; }
        IReadOnlyCollection<TItem> this[int hash] { get; }
    }



    public class ReadOnlyIndex<TItem> : IReadOnlyIndex<TItem>
    {
        protected readonly Index<TItem> Index;


        public ReadOnlyIndex(Index<TItem> index)
        {
            Index = index;
        }


        public IEnumerator<TItem> GetEnumerator()
        {
            return Index.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public int Count { get { return Index.Count; } }

        public string FieldName { get { return Index.FieldName; } }
        public IReadOnlyCollection<TItem> this[int hash] { get { return Index[hash]; } }
    }



    public class Index<TItem> : ICollection<TItem>
    {
        readonly string _fieldName;
        readonly Dictionary<int, List<TItem>> _hashDict = new Dictionary<int, List<TItem>>();
        Func<TItem, object> _memberReflector;
        static readonly IReadOnlyCollection<TItem> EmptyList = new List<TItem>().AsReadOnly();
        static readonly IReadOnlyCollection<Type> SaveTypes = new List<Type>() { typeof(byte), typeof(short), typeof(int) }.AsReadOnly(); 

        public string FieldName { get { return _fieldName; } }
        public bool IsCollisionPossible { get; private set; }

        public Index(string fieldName, List<TItem> items)
        {
            _fieldName = fieldName;
            if (items != null && items.Count != 0)
                AddRange(items);

            IsCollisionPossible = GetCollisionPossibility();
        }


        private bool GetCollisionPossibility()
        {
            Type type = typeof (TItem);
            FieldInfo fieldInfo = type.GetField(FieldName);
            Type memberType = fieldInfo == null ? type.GetProperty(FieldName).PropertyType : fieldInfo.FieldType;

            if (SaveTypes.Contains(memberType))
                return false;

            return true;
        }


        public IReadOnlyCollection<TItem> this[int hash]
        {
            get
            {
                if (!_hashDict.ContainsKey(hash))
                    return EmptyList;

                return _hashDict[hash];
            }
        }


        public void AddRange(IEnumerable<TItem> items)
        {
            foreach (TItem item in items)
                Add(item);
        }


        public bool Remove(TItem item)
        {
            int hash = GetMemberHash(item);
            if (!_hashDict.ContainsKey(hash))
                return false;

            List<TItem> valueItems = _hashDict[hash];
            bool success = valueItems.Remove(item);
            if (valueItems.Count == 0)
                _hashDict.Remove(hash);

            if (success)
                Count--;

            return success;
        }


        public void Remove(IEnumerable<TItem> items)
        {
            foreach (TItem item in items)
                Remove(item);
        }


        int GetMemberHash(TItem item)
        {
            if (_memberReflector == null)
            {
                Type type = typeof (TItem);
                FieldInfo fieldInfo = type.GetField(FieldName);
                if (fieldInfo == null)
                {
                    PropertyInfo propertyInfo = type.GetProperty(FieldName);
                    _memberReflector = tItem => propertyInfo.GetValue(tItem);
                }
                else
                    _memberReflector = tItem => fieldInfo.GetValue(item);
            }

            object memberValue = _memberReflector(item);
            if (memberValue != null)
                return memberValue.GetHashCode();

            return 0;
        }


        public IReadOnlyIndex<TItem> AsReadOnly()
        {
            return new ReadOnlyIndex<TItem>(this);
        }


        #region ICollection
        public IEnumerator<TItem> GetEnumerator()
        {
            return _hashDict.SelectMany(v => v.Value).GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void Add(TItem item)
        {
            int hash = GetMemberHash(item);

            if (!_hashDict.ContainsKey(hash))
                _hashDict[hash] = new List<TItem>(1) {item};
            else
                _hashDict[hash].Add(item);

            Count++;
        }


        public void CopyTo(TItem[] array, int arrayIndex)
        {
            _hashDict.Values.SelectMany(item => item).ToList().CopyTo(array, arrayIndex);
        }


        bool ICollection<TItem>.Remove(TItem item)
        {
            return Remove(item);
        }


        public int Count { get; private set; }
        public bool IsReadOnly { get { return false; } }


        public void Clear()
        {
            _hashDict.Clear();
            Count = 0;
        }


        public bool Contains(TItem item)
        {
            return _hashDict.Values.SelectMany(a => a).Contains(item);
        }
        #endregion
    }
}