﻿using System.Collections;
using Smartstore.Threading;

namespace Smartstore.Collections
{
    public sealed class SyncedCollection<T> : Disposable, ICollection<T>
    {
        // INFO: Don't call it SynchronizedCollection because of framework dupe.
        private readonly ICollection<T> _col;
        private readonly ReaderWriterLockSlim _rwLock = new(LockRecursionPolicy.SupportsRecursion);

        public SyncedCollection(ICollection<T> wrappedCollection)
        {
            _col = Guard.NotNull(wrappedCollection);
        }

        public ReaderWriterLockSlim Lock 
        {
            get => _rwLock; 
        }

        public void AddRange(IEnumerable<T> collection)
        {
            using (_rwLock.GetWriteLock())
            {
                _col.AddRange(collection);
            }
        }

        public void Insert(int index, T item)
        {
            if (_col is List<T> list)
            {
                using (_rwLock.GetWriteLock())
                {
                    list.Insert(index, item);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void InsertRange(int index, IEnumerable<T> values)
        {
            if (_col is List<T> list)
            {
                using (_rwLock.GetWriteLock())
                {
                    list.InsertRange(index, values);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public int RemoveRange(IEnumerable<T> values)
        {
            int numRemoved = 0;

            using (_rwLock.GetWriteLock())
            {
                foreach (var value in values)
                {
                    if (_col.Remove(value))
                        numRemoved++;
                }
            }

            return numRemoved;
        }

        public void RemoveRange(int index, int count)
        {
            if (_col is List<T> list)
            {
                using (_rwLock.GetWriteLock())
                {
                    list.RemoveRange(index, count);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void RemoveAt(int index)
        {
            using (_rwLock.GetWriteLock())
            {
                if (_col is List<T> list)
                {
                    list.RemoveAt(index);
                }
                else
                {
                    var item = _col.ElementAtOrDefault(index);
                    if (item != null)
                    {
                        _col.Remove(item);
                    }
                }
            }
        }

        public T this[int index]
        {
            get
            {
                using (_rwLock.GetReadLock())
                {
                    return _col.ElementAt(index);
                }
            }
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                _rwLock.Dispose();
            }
        }

        #region ICollection<T>

        public int Count
        {
            get
            {
                using (_rwLock.GetReadLock())
                {
                    return _col.Count;
                }
            }
        }

        public bool IsReadOnly => _col.IsReadOnly;

        public void Add(T item)
        {
            using (_rwLock.GetWriteLock())
            {
                _col.Add(item);
            }
        }

        public void Clear()
        {
            using (_rwLock.GetWriteLock())
            {
                _col.Clear();
            }
        }

        public bool Contains(T item)
        {
            using (_rwLock.GetReadLock())
            {
                return _col.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Guard.NotNull(array);
            
            if (array.IsSynchronized)
            {
                // Have timeout in case of deadlock
                if (Monitor.TryEnter(array.SyncRoot, 1000))
                {
                    try
                    {
                        using (_rwLock.GetReadLock())
                        {
                            _col.CopyTo(array, arrayIndex);
                        }
                    }
                    finally
                    {
                        Monitor.Exit(array.SyncRoot);
                    }
                }
                else
                {
                    throw new TimeoutException("Failed to copy to array.");
                }
            }
            else
            {
                using (_rwLock.GetReadLock())
                {
                    _col.CopyTo(array, arrayIndex);
                }
            }
        }

        public bool Remove(T item)
        {
            using (_rwLock.GetWriteLock())
            {
                return _col.Remove(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            using (_rwLock.GetWriteLock())
            {
                // Create a collection snaphot
                var snaphot = _col.ToList();
                return new SafeEnumerator(snaphot.GetEnumerator(), _rwLock);
            }
        }

        #endregion

        #region SafeEnumerator

        sealed class SafeEnumerator : IEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;
            private readonly ReaderWriterLockSlim _rwLock;
            private bool _disposed;

            public SafeEnumerator(IEnumerator<T> inner, ReaderWriterLockSlim rwLock)
            {
                _inner = inner;
                _rwLock = rwLock;
            }

            public bool MoveNext()
            {
                if (!_rwLock.IsReadLockHeld)
                {
                    _rwLock.EnterReadLock();
                }
                
                return _inner.MoveNext();
            }

            public void Reset()
            {
                if (!_rwLock.IsReadLockHeld)
                {
                    _rwLock.EnterReadLock();
                }

                _inner.Reset();
            }

            public T Current
            {
                get 
                {
                    if (!_rwLock.IsReadLockHeld)
                    {
                        _rwLock.EnterReadLock();
                    }

                    return _inner.Current;
                }
            }

            object IEnumerator.Current
            {
                get => Current;
            }

            public void Dispose()
            {
                // Dispose inner enumerator when foreach loop finishes
                if (!_disposed)
                {
                    try
                    {
                        if (_rwLock.IsReadLockHeld)
                        {
                            _rwLock.ExitReadLock();
                        }
                        
                        _inner.Dispose();
                        _disposed = true;
                    }
                    catch
                    {
                    }
                }
            }

            ~SafeEnumerator()
            {
                Dispose();
            }
        }

        #endregion
    }
}
