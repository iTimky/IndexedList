using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace IndexedList
{
    public class IndexedList<TItem> : IList<TItem>
    {
        protected readonly Dictionary<string, HashIndex<TItem>> Indexes = new Dictionary<string, HashIndex<TItem>>();
        protected readonly List<TItem> Items = new List<TItem>();

        public IndexedList() { }

        public IndexedList(IEnumerable<TItem> items)
        {
            Items.AddRange(items);
        }

        public IndexedList(params string[] names)
        {
            foreach (string name in names)
                AddIndex(name);
        }


        #region ctros via Expressions
        public IndexedList(params Expression<Func<TItem, byte>>[] expressions)
        {
            foreach (var expression in expressions)
                AddIndex(expression);
        }

        public IndexedList(params Expression<Func<TItem, short>>[] expressions)
        {
            foreach (var expression in expressions)
                AddIndex(expression);
        }

        public IndexedList(params Expression<Func<TItem, int>>[] expressions)
        {
            foreach (var expression in expressions)
                AddIndex(expression);
        }

        public IndexedList(params Expression<Func<TItem, long>>[] expressions)
        {
            foreach (var expression in expressions)
                AddIndex(expression);
        }

        public IndexedList(params Expression<Func<TItem, char>>[] expressions)
        {
            foreach (var expression in expressions)
                AddIndex(expression);
        }

        public IndexedList(params Expression<Func<TItem, string>>[] expressions)
        {
            foreach (var expression in expressions)
                AddIndex(expression);
        }

        public IndexedList(params Expression<Func<TItem, DateTime>>[] expressions)
        {
            foreach (var expression in expressions)
                AddIndex(expression);
        }

        #endregion


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

            if (Indexes.ContainsKey(name))
                throw new DuplicateNameException(string.Format(
                    "Duplicate field index creation not allowed. Duplicate member name: \"{0}\"", name));

            Indexes[name] = new HashIndex<TItem>(name, Items);
        }


        public void AddRange(IEnumerable<TItem> items)
        {
            List<TItem> itemList = items.ToList();
            Items.AddRange(itemList);
            foreach (var index in Indexes.Values)
                index.AddRange(itemList);
        }


        public void RemoveAll(Predicate<TItem> match)
        {
            if (match == null)
                throw new ArgumentNullException();

            List<TItem> toDelete = Items.Where(i => match(i)).ToList();
            Items.RemoveAll(match);
            foreach (var index in Indexes.Values)
                index.Remove(toDelete);
        }


        public void Remove(IEnumerable<TItem> items)
        {
            foreach (var item in items)
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
            return Indexes.ContainsKey(memberName);
        }


        public List<IReadOnlyHashIndex<TItem>> GetIndexes()
        {
            return Indexes.Values.Select(i => i.AsReadOnly()).ToList();
        }


        #region Enumerable extension methods override

        private const string NoMatch = "No Match";
        private const string NoElements = "No Elements";

        public IEnumerable<TItem> Where(Expression<Func<TItem, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException();

            string fieldName = ExpressionParser.GetMemberName(predicate);
            if (fieldName == null || !Indexes.ContainsKey(fieldName))
                return Items.Where(predicate.Compile());

            HashIndex<TItem> index = Indexes[fieldName];
            int? hash = ExpressionParser.GetFieldHash(predicate);
            if (hash == null)
                return Enumerable.Empty<TItem>();
            return index[hash.Value].Where(predicate.Compile());
        }


        public bool Any(Expression<Func<TItem, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException();

            string fieldName = ExpressionParser.GetMemberName(predicate);
            if (fieldName == null || !Indexes.ContainsKey(fieldName))
                return Items.Any(predicate.Compile());

            HashIndex<TItem> hashIndex = Indexes[fieldName];
            int? hash = ExpressionParser.GetFieldHash(predicate);
            if (hash == null)
                return false;
            return hashIndex[hash.Value].Any(predicate.Compile());
        }


        public TItem First(Expression<Func<TItem, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException();

            if (Items.Count == 0)
                throw new InvalidOperationException(NoElements);

            string fieldName = ExpressionParser.GetMemberName(predicate);
            if (fieldName == null || !Indexes.ContainsKey(fieldName))
                return Items.First(predicate.Compile());

            HashIndex<TItem> hashIndex = Indexes[fieldName];
            int? hash = ExpressionParser.GetFieldHash(predicate);
            if (hash == null)
                throw new InvalidOperationException(NoMatch);
            return hashIndex[hash.Value].First(predicate.Compile());
        }


        public TItem FirstOrDefault(Expression<Func<TItem, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException();

            string fieldName = ExpressionParser.GetMemberName(predicate);
            if (fieldName == null || !Indexes.ContainsKey(fieldName))
                return Items.FirstOrDefault(predicate.Compile());

            HashIndex<TItem> hashIndex = Indexes[fieldName];
            int? hash = ExpressionParser.GetFieldHash(predicate);
            if (hash == null)
                return default(TItem);
            return hashIndex[hash.Value].FirstOrDefault(predicate.Compile());
        }


        public TItem Single(Expression<Func<TItem, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException();

            if (Items.Count == 0)
                throw new InvalidOperationException(NoElements);

            string fieldName = ExpressionParser.GetMemberName(predicate);
            if (fieldName == null || !Indexes.ContainsKey(fieldName))
                return Items.Single(predicate.Compile());

            HashIndex<TItem> hashIndex = Indexes[fieldName];
            int? hash = ExpressionParser.GetFieldHash(predicate);
            if (hash == null)
                throw new InvalidOperationException(NoMatch);
            return hashIndex[hash.Value].Single(predicate.Compile());
        }

        public TItem SingleOrDefault(Expression<Func<TItem, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException();

            if (Items.Count == 0)
                return default(TItem);

            string fieldName = ExpressionParser.GetMemberName(predicate);
            if (fieldName == null || !Indexes.ContainsKey(fieldName))
                return Items.Single(predicate.Compile());

            HashIndex<TItem> hashIndex = Indexes[fieldName];
            int? hash = ExpressionParser.GetFieldHash(predicate);
            if (hash == null)
                throw new InvalidOperationException(NoMatch);
            return hashIndex[hash.Value].SingleOrDefault(predicate.Compile());
        }

        #endregion


        #region IList<TItem>

        public IEnumerator<TItem> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TItem item)
        {
            Items.Add(item);
            foreach (var index in Indexes.Values)
                index.Add(item);
        }

        public void Clear()
        {
            Items.Clear();
            foreach (var index in Indexes.Values)
                index.Clear();
        }

        public bool Contains(TItem item)
        {
            return Items.Contains(item);
        }

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        public bool Remove(TItem item)
        {
            foreach (var index in Indexes)
                index.Value.Remove(item);

            return Items.Remove(item);
        }

        public int Count
        {
            get { return Items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(TItem item)
        {
            return Items.IndexOf(item);
        }

        public void Insert(int index, TItem item)
        {
            Items.Insert(index, item);
            foreach (var hashIndex in Indexes.Values)
                hashIndex.Add(item);
        }

        public void RemoveAt(int index)
        {
            TItem item = Items[index];
            Items.RemoveAt(index);
            foreach (var hashIndex in Indexes.Values)
                hashIndex.Remove(item);
        }

        public TItem this[int index]
        {
            get { return Items[index]; }
            set
            {
                TItem oldItem = Items[index];
                Items[index] = value;
                foreach (var hashIndex in Indexes.Values)
                {
                    hashIndex.Remove(oldItem);
                    hashIndex.Add(value);
                }
            }
        }

        #endregion
    }
}