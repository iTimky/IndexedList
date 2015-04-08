using System.Collections;
using System.Collections.Generic;

namespace IndexedList
{
    internal struct IndexLeafEnumerator<TItem> : IEnumerator<TItem>
    {
        private readonly IIndexLeaf<TItem> _firstLeaf;
        private IIndexLeaf<TItem> _currentLeaf;
        public IndexLeafEnumerator(IIndexLeaf<TItem> leaf)
        {
            _firstLeaf = leaf;
            _currentLeaf = null;
        }

        public void Dispose() { }

        public bool MoveNext()
        {
            if (_firstLeaf == null)
                return false;

            if (_currentLeaf == null)
            {
                _currentLeaf = _firstLeaf;
                return true;
            }

            if (_currentLeaf.Previous == null)
                return false;

            _currentLeaf = _currentLeaf.Previous;
            return true;
        }

        public void Reset()
        {
            _currentLeaf = null;
        }

        public TItem Current
        {
            get { return _currentLeaf.Item; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}