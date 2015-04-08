using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace IndexedList
{
    interface IIndexLeaf<TItem> : IEnumerable<TItem>
    {
        TItem Item { get; }
        IIndexLeaf<TItem> Previous { get; set; }
        int Count { get; }
        void Set(LastIndexLeaf<TItem> leaf);
    }

//    internal struct IndexLeaf<TItem> : IIndexLeaf<TItem>
//    {
//        TItem Item { get; set; }
//        IIndexLeaf<TItem> Previous { get; set; }
//        public void Set(LastIndexLeaf<TItem> leaf)
//        {
//            Item = leaf.Item;
//            Previous = leaf.Previous;
//            Count = leaf.Count;
//        }
//    }


    internal struct LastIndexLeaf<TItem> : IIndexLeaf<TItem>
    {
        public TItem Item { get; set; }

        public IIndexLeaf<TItem> Previous { get; set; }
        private List<IIndexLeaf<TItem>> EmptyObjects;
        //public LastIndexLeaf() { }
        public LastIndexLeaf(TItem item, IIndexLeaf<TItem> previous = null) : this()

        {
            Item = item;
            if (previous != null)
            {
                Previous = previous;
            }
            Count = (previous != null ? previous.Count : 0) + 1;
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return new IndexLeafEnumerator<TItem>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public LastIndexLeaf<TItem> Add(TItem item)
        {
            if (Count == 0)
                return new LastIndexLeaf<TItem>(item);

            if (EmptyObjects == null)
                EmptyObjects = new List<IIndexLeaf<TItem>>(4)
                { new LastIndexLeaf<TItem>(), new LastIndexLeaf<TItem>(),
                new LastIndexLeaf<TItem>(), new LastIndexLeaf<TItem>() };

            if (Count == EmptyObjects.Capacity)
            {
                for (int i = 0; i < Count; i++)
                    EmptyObjects.Add(new LastIndexLeaf<TItem>());
            }
            return new LastIndexLeaf<TItem>(item, this);
        }

        public bool Contains(TItem item)
        {
            return Enumerable.Contains(this, item);
        }

        public LastIndexLeaf<TItem>? Remove(TItem item)
        {
            if (Count == 0)
                return null;

            var equatable = item as IEquatable<TItem>;
            if (equatable != null)
                return Remove(equatable);

            if (Equals(Item, item))
            {
                if (Previous == null)
                    return null;

                var previous = (LastIndexLeaf<TItem>)Previous;
                previous.Count = Count - 1;
                return previous;
            }
            
            IIndexLeaf<TItem> last = this;
            IIndexLeaf<TItem> next = last;
            var current = Previous;
            while (current != null)
            {
                if (Equals(current.Item, item))
                {
                    next.Previous = current.Previous;
                    var result = (LastIndexLeaf<TItem>)last;
                    result.Count--;
                    return result;
                }

                next = current;
                current = current.Previous;
            }

            return this;
        }

        private LastIndexLeaf<TItem>? Remove(IEquatable<TItem> item)
        {
            if (item.Equals(Item))
            {
                if (Previous == null)
                    return null;

                var previous = (LastIndexLeaf<TItem>)Previous;
                previous.Count = Count - 1;
                return previous;
            }

            IIndexLeaf<TItem> last = this;
            IIndexLeaf<TItem> next = last;
            var current = Previous;
            while (current != null)
            {
                if (item.Equals(Item))
                {
                    next.Previous = current.Previous;
                    var result = (LastIndexLeaf<TItem>)last;
                    result.Count--;
                    return result;
                }

                next = current;
                current = current.Previous;
            }

            return this;
        }

        public int Count { get; set; }
        public void Set(LastIndexLeaf<TItem> leaf)
        {
            Item = leaf.Item;
            Previous = leaf.Previous;
            Count = leaf.Count;
        }

        public bool IsReadOnly { get { return false; } }

        public ReadOnlyIndexLeaf<TItem> AsReadOnly()
        {
            return new ReadOnlyIndexLeaf<TItem>(this);
        }
    }

    class ReadOnlyIndexLeaf<TItem> : IEnumerable<TItem>
    {
        private LastIndexLeaf<TItem> _leaf;

        public ReadOnlyIndexLeaf(LastIndexLeaf<TItem> leaf)
        {
            _leaf = leaf;
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return _leaf.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get { return _leaf.Count; } }
    }
}