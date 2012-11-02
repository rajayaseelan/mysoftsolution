using System.Collections.Generic;
using System.Threading;

namespace MySoft.IoC.Communication.Threading
{
    /// <summary>
    /// This class is used to store key-value based items in a thread safe manner.
    /// It uses System.Collections.Generic.SortedList internally.
    /// </summary>
    /// <typeparam name="TK">Key type</typeparam>
    /// <typeparam name="TV">Value type</typeparam>
    public class ThreadSafeSortedList<TK, TV>
    {
        /// <summary>
        /// Gets/adds/replaces an item by key.
        /// </summary>
        /// <param name="key">Key to get/set value</param>
        /// <returns>Item associated with this key</returns>
        public TV this[TK key]
        {
            get
            {
                lock (_items)
                {
                    return _items.ContainsKey(key) ? _items[key] : default(TV);
                }
            }

            set
            {
                lock (_items)
                {
                    _items[key] = value;
                }
            }
        }

        /// <summary>
        /// Gets count of items in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_items)
                {
                    return _items.Count;
                }
            }
        }

        /// <summary>
        /// Internal collection to store items.
        /// </summary>
        protected readonly IDictionary<TK, TV> _items;

        /// <summary>
        /// Creates a new ThreadSafeSortedList object.
        /// </summary>
        public ThreadSafeSortedList()
        {
            _items = new SortedList<TK, TV>();
        }

        /// <summary>
        /// Checks if collection contains spesified key.
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>True; if collection contains given key</returns>
        public bool ContainsKey(TK key)
        {
            lock (_items)
            {
                return _items.ContainsKey(key);
            }
        }

        /// <summary>
        /// Removes an item from collection.
        /// </summary>
        /// <param name="key">Key of item to remove</param>
        public bool Remove(TK key)
        {
            lock (_items)
            {
                if (!_items.ContainsKey(key))
                {
                    return false;
                }

                _items.Remove(key);
                return true;
            }
        }

        /// <summary>
        /// Gets all items in collection.
        /// </summary>
        /// <returns>Item list</returns>
        public List<TV> GetAllItems()
        {
            lock (_items)
            {
                return new List<TV>(_items.Values);
            }
        }

        /// <summary>
        /// Removes all items from list.
        /// </summary>
        public void ClearAll()
        {
            lock (_items)
            {
                _items.Clear();
            }
        }

        /// <summary>
        /// Gets then removes all items in collection.
        /// </summary>
        /// <returns>Item list</returns>
        public List<TV> GetAndClearAllItems()
        {
            lock (_items)
            {
                var list = new List<TV>(_items.Values);
                _items.Clear();
                return list;
            }
        }
    }
}
