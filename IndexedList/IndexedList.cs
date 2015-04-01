using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IndexedList
{
    public class Index<TItem, TPropType> : IEnumerable<TItem>, IList<TItem>
    {
        //Collection<int>

        protected Dictionary<List<string>, List<TItem>> Items;

        public Index(params Expression<Func<TItem, TPropType>>[] props)
        {
            var propList = props.Select(FieldNameSearcher.GetFieldName).ToList();

            if (propList.Any(p => p == null))
                throw new Exception(string.Format("Null field names not allowed"));

            var duplicate = propList.FirstOrDefault(p => propList.Count(pr => pr == p) > 1);
            if (duplicate != null)
                throw new Exception(string.Format("Duplicate field index creation not allowed. Duplicate value: \"{0}\"", duplicate));

            Items = new Dictionary<List<string>, List<TItem>> { { propList, new List<TItem>() } };
        }

        public Index(IEnumerable<TItem> items)
        {
//            Items = items.ToList();
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return Items.SelectMany(i => i.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region
        public void Add(TItem item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(TItem item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TItem item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; private set; }
        public bool IsReadOnly { get; private set; }
        public int IndexOf(TItem item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, TItem item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public TItem this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        #endregion
    }

    public class Clazz<T>
    {
//        readonly List<Index<T, ?>> _indexes = new List<Index<T>>();
//
//        public void AddIndex<TPropType>(Expression<Func<T, TPropType>> exp)
//        {
//            _indexes.Add(new Index<T, TPropType>(exp));
//        }
    }
}
