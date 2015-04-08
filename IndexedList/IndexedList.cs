#region usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

#endregion


[assembly: InternalsVisibleTo("Test")]
namespace IndexedList
{
    public class IndexedList<TItem> : IList<TItem>
    {
        readonly Dictionary<string, Index<TItem>> _indexes = new Dictionary<string, Index<TItem>>();
        readonly List<TItem> _items = new List<TItem>();

        public IndexedList() {}


        public IndexedList(IEnumerable<TItem> items)
        {
            _items.AddRange(items);
        }


        public void AddIndex<TMember>(Expression<Func<TItem, TMember>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException();

            string name = ExpressionParser.GetMemberName(expression);
            AddIndex(name);
        }


        public void AddIndex(string name)
        {
            if (name == null)
                throw new Exception(string.Format("Null field names not allowed"));

            Type type = typeof (TItem);
            if (type.GetProperty(name) == null && type.GetField(name) == null)
                throw new ArgumentOutOfRangeException(
                    string.Format("Type <{0}> doesn't contain field or property \"{1}\"", type.Name,
                        name));

            if (_indexes.ContainsKey(name))
                throw new DuplicateNameException(string.Format(
                    "Duplicate field index creation not allowed. Duplicate member name: \"{0}\"", name));

            _indexes[name] = new Index<TItem>(name, _items);
        }


        public void AddRange(IEnumerable<TItem> items)
        {
            List<TItem> itemList = items.ToList();
            _items.AddRange(itemList);
            foreach (Index<TItem> index in _indexes.Values)
                index.AddRange(itemList);
        }


        public void RemoveAll(Predicate<TItem> match)
        {
            if (match == null)
                throw new ArgumentNullException();

            List<TItem> toDelete = _items.Where(i => match(i)).ToList();
            _items.RemoveAll(match);
            foreach (Index<TItem> index in _indexes.Values)
                index.Remove(toDelete);
        }


        public void Remove(IEnumerable<TItem> items)
        {
            foreach (TItem item in items)
                Remove(item);
        }


        public bool HasIndex<TMember>(Expression<Func<TItem, TMember>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException();

            return HasIndex(ExpressionParser.GetMemberName(expression));
        }


        public bool HasIndex(string memberName)
        {
            return _indexes.ContainsKey(memberName);
        }


        public List<IReadOnlyIndex<TItem>> GetIndexes()
        {
            return _indexes.Values.Select(i => i.AsReadOnly()).ToList();
        }


        #region Enumerable extension methods override
        const string NoMatch = "No Match";
        const string NoElements = "No Elements";


        public IEnumerable<TItem> Where(Expression<Func<TItem, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException();

            string fieldName = ExpressionParser.GetMemberName(predicate);
            if (fieldName == null || !_indexes.ContainsKey(fieldName))
                return _items.Where(predicate.Compile());

            Index<TItem> index = _indexes[fieldName];
            int? hash = ExpressionParser.GetMemberHash(predicate);
            if (hash == null)
                return _items.Where(predicate.Compile());
            IReadOnlyCollection<TItem> items = index[hash.Value];
            if (items.Count == 0)
                return Enumerable.Empty<TItem>();

            if (!index.IsCollisionPossible)
                return items;

            return items.Where(predicate.Compile());
        }


        public bool Any(Expression<Func<TItem, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException();

            string fieldName = ExpressionParser.GetMemberName(predicate);
            if (fieldName == null || !_indexes.ContainsKey(fieldName))
                return _items.Any(predicate.Compile());

            Index<TItem> index = _indexes[fieldName];
            int? hash = ExpressionParser.GetMemberHash(predicate);
            if (hash == null)
                return _items.Any(predicate.Compile());
            var items = index[hash.Value];
            if (items.Count == 0)
                return false;
            if (items.Count == 1 && !index.IsCollisionPossible)
                return true;

            return index[hash.Value].Any(predicate.Compile());
        }


        public TItem First(Expression<Func<TItem, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException();

            if (_items.Count == 0)
                throw new InvalidOperationException(NoElements);

            string fieldName = ExpressionParser.GetMemberName(predicate);
            if (fieldName == null || !_indexes.ContainsKey(fieldName))
                return _items.First(predicate.Compile());

            Index<TItem> index = _indexes[fieldName];
            int? hash = ExpressionParser.GetMemberHash(predicate);
            if (hash == null)
                return _items.First(predicate.Compile());
            var items = index[hash.Value];
            if (items.Count == 0)
                throw new InvalidOperationException(NoElements);
            if (items.Count == 1 && !index.IsCollisionPossible)
                return items.First();

            return items.First(predicate.Compile());
        }


        public TItem FirstOrDefault(Expression<Func<TItem, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException();

            string fieldName = ExpressionParser.GetMemberName(predicate);
            if (fieldName == null || !_indexes.ContainsKey(fieldName))
                return _items.FirstOrDefault(predicate.Compile());

            Index<TItem> index = _indexes[fieldName];
            int? hash = ExpressionParser.GetMemberHash(predicate);
            if (hash == null)
                return _items.FirstOrDefault(predicate.Compile());
            IReadOnlyCollection<TItem> items = index[hash.Value];
            if (items.Count == 0)
                return default(TItem);
            if (items.Count == 1 && !index.IsCollisionPossible)
                return items.First();

            return items.FirstOrDefault(predicate.Compile());
        }


        public TItem Single(Expression<Func<TItem, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException();

            if (_items.Count == 0)
                throw new InvalidOperationException(NoElements);

            string fieldName = ExpressionParser.GetMemberName(predicate);
            if (fieldName == null || !_indexes.ContainsKey(fieldName))
                return _items.Single(predicate.Compile());

            Index<TItem> index = _indexes[fieldName];
            int? hash = ExpressionParser.GetMemberHash(predicate);
            if (hash == null)
                return _items.Single(predicate.Compile());
            IReadOnlyCollection<TItem> items = index[hash.Value];
            if (items.Count == 0)
                throw new InvalidOperationException(NoElements);
            if (items.Count == 1 && !index.IsCollisionPossible)
                return items.Single();

            return items.Single(predicate.Compile());
        }


        public TItem SingleOrDefault(Expression<Func<TItem, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException();

            if (_items.Count == 0)
                return default(TItem);

            string fieldName = ExpressionParser.GetMemberName(predicate);
            if (fieldName == null || !_indexes.ContainsKey(fieldName))
                return _items.Single(predicate.Compile());

            Index<TItem> index = _indexes[fieldName];
            int? hash = ExpressionParser.GetMemberHash(predicate);
            if (hash == null)
                return _items.Single(predicate.Compile());
            IReadOnlyCollection<TItem> items = index[hash.Value];
            if (items.Count == 0)
                return default(TItem);
            if (items.Count == 1 && !index.IsCollisionPossible)
                return items.Single();

            return items.SingleOrDefault(predicate.Compile());
        }
        #endregion


        #region IList<TItem>
        public IEnumerator<TItem> GetEnumerator()
        {
            return _items.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void Add(TItem item)
        {
            _items.Add(item);
            foreach (Index<TItem> index in _indexes.Values)
                index.Add(item);
        }


        public void Clear()
        {
            _items.Clear();
            foreach (Index<TItem> index in _indexes.Values)
                index.Clear();
        }


        public bool Contains(TItem item)
        {
            return _items.Contains(item);
        }


        public void CopyTo(TItem[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }


        public bool Remove(TItem item)
        {
            foreach (KeyValuePair<string, Index<TItem>> index in _indexes)
                index.Value.Remove(item);

            return _items.Remove(item);
        }


        public int Count { get { return _items.Count; } }

        public bool IsReadOnly { get { return false; } }


        public int IndexOf(TItem item)
        {
            return _items.IndexOf(item);
        }


        public void Insert(int index, TItem item)
        {
            _items.Insert(index, item);
            foreach (Index<TItem> hashIndex in _indexes.Values)
                hashIndex.Add(item);
        }


        public void RemoveAt(int index)
        {
            TItem item = _items[index];
            _items.RemoveAt(index);
            foreach (Index<TItem> hashIndex in _indexes.Values)
                hashIndex.Remove(item);
        }


        public TItem this[int index]
        {
            get { return _items[index]; }
            set
            {
                TItem oldItem = _items[index];
                _items[index] = value;
                foreach (Index<TItem> hashIndex in _indexes.Values)
                {
                    hashIndex.Remove(oldItem);
                    hashIndex.Add(value);
                }
            }
        }
        #endregion
    }
}