using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace evocontest.Submission
{
    [DebuggerDisplay("Count = {Count}")]
    public class ArrayList<T> : IList<T>, IReadOnlyList<T>
    {
        private T[] _items;
        private bool arrayWitdrawn;

        public ArrayList(int capacity)
        {
            Count = 0;
            arrayWitdrawn = false;
            _items = new T[capacity];
        }

        public ArrayList(T[] source)
        {
            arrayWitdrawn = false;
            _items = source;
            Count = source.Length;
        }
        public static ArrayList<T> FromArray(T[] source) => new ArrayList<T>(source);

        public static implicit operator ReadOnlySpan<T>(ArrayList<T> a) => ((ReadOnlySpan<T>)a._items)[..a.Count];

        public int Count { get; private set; }
        public int Capacity
        {
            get => _items.Length;
            private set
            {
                if (value != _items.Length)
                {
                    T[] newItems = new T[value];
                    Array.Copy(_items, 0, newItems, 0, Count);
                    _items = newItems;
                }
            }
        }

        bool ICollection<T>.IsReadOnly => arrayWitdrawn;

        public T this[int index]
        {
            get => _items[index];
            set
            {
                if (arrayWitdrawn) throw new InvalidOperationException("Array is already witdrawn");
                _items[index] = value;
            }
        }

        public void Add(T item)
        {
            if (arrayWitdrawn) throw new InvalidOperationException("Array is already witdrawn");
            if (Count == _items.Length) EnsureCapacity(Count + 1);
            _items[Count++] = item;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (arrayWitdrawn) throw new InvalidOperationException("Array is already witdrawn");
            InsertRange(Count, collection);
        }

        public void AddRange(ReadOnlySpan<T> span)
        {
            if (arrayWitdrawn) throw new InvalidOperationException("Array is already witdrawn");
            EnsureCapacity(Count + span.Length);
            var length = span.Length;
            for (int i = Count, j = 0; j < length; i++, j++)
            {
                _items[i] = span[j];
            }
            Count += length;
        }

        public void Clear()
        {
            if (arrayWitdrawn) throw new InvalidOperationException("Array is already witdrawn");
            Count = 0;
        }

        public bool Contains(T item)
        {
            for (int i = 0; i < Count; i++) if (item.Equals(_items[i])) return true;
            return false;
        }

        private void EnsureCapacity(int min)
        {
            if (arrayWitdrawn) throw new InvalidOperationException("Array is already witdrawn");
            if (_items.Length < min)
            {
                int newCapacity = _items.Length * 2;
                if ((uint)newCapacity > Int32.MaxValue) newCapacity = Int32.MaxValue;
                if (newCapacity < min) newCapacity = min;
                Capacity = newCapacity;
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => new Enumerator(this);

        public int IndexOf(T item) => Array.IndexOf(_items, item, 0, Count);

        public int IndexOf(T item, int index) => Array.IndexOf(_items, item, index, Count - index);

        public int IndexOf(T item, int index, int count) => Array.IndexOf(_items, item, index, count);

        public void Insert(int index, T item)
        {
            if (arrayWitdrawn) throw new InvalidOperationException("Array is already witdrawn");
            if (Count == _items.Length) EnsureCapacity(Count + 1);
            if (index < Count)
            {
                Array.Copy(_items, index, _items, index + 1, Count - index);
            }
            _items[index] = item;
            Count++;
        }        

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (arrayWitdrawn)
            {
                throw new InvalidOperationException("Array is already witdrawn");
            }
            if (collection is ICollection<T> c)
            {    // if collection is ICollection<T>
                int count = c.Count;
                if (count > 0)
                {
                    EnsureCapacity(Count + count);
                    if (index < Count)
                    {
                        Array.Copy(_items, index, _items, index + count, Count - index);
                    }

                    T[] itemsToInsert = new T[count];
                    c.CopyTo(itemsToInsert, 0);
                    itemsToInsert.CopyTo(_items, index);

                    Count += count;
                }
            }
            else
            {
                using IEnumerator<T> en = collection.GetEnumerator();
                while (en.MoveNext())
                {
                    Insert(index++, en.Current);
                }
            }
        }

        public int LastIndexOf(T item)
        {
            if (Count == 0) return -1;
            else return LastIndexOf(item, Count - 1, Count);
        }

        public int LastIndexOf(T item, int index) => LastIndexOf(item, index, index + 1);

        public int LastIndexOf(T item, int index, int count)
        {
            if (Count == 0) return -1;
            return Array.LastIndexOf(_items, item, index, count);
        }

        public bool Remove(T item)
        {
            if (arrayWitdrawn) throw new InvalidOperationException("Array is already witdrawn");
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            if (arrayWitdrawn) throw new InvalidOperationException("Array is already witdrawn");
            Count--;
            if (index < Count)
            {
                Array.Copy(_items, index + 1, _items, index, Count - index);
            }
            _items[Count] = default;
        }

        public T[] ToArray()
        {
            if (arrayWitdrawn) throw new InvalidOperationException("Array is already witdrawn");
            arrayWitdrawn = true;
            return _items;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (arrayWitdrawn) throw new InvalidOperationException("Array is already witdrawn");
            Array.Copy(_items, 0, array, arrayIndex, Count);
        }

        public override string ToString()
        {
#if DEBUG
            if (typeof(T) == typeof(OffsetRange))
            {
                return string.Join(", ", _items.Cast<OffsetRange>().Select(i => i.ToDebug()));
            } else
#endif
                return string.Join(", ", _items);
        }

        public struct Enumerator : IEnumerator<T>, System.Collections.IEnumerator
        {
            private readonly ArrayList<T> list;
            private int index;

            internal Enumerator(ArrayList<T> list)
            {
                this.list = list;
                index = 0;
                Current = default;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                var localList = list;
                if (((uint)index < (uint)localList.Count))
                {
                    Current = localList._items[index];
                    index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                index = list.Count + 1;
                Current = default;
                return false;
            }

            public T Current { get; private set; }

            object System.Collections.IEnumerator.Current => Current;

            void System.Collections.IEnumerator.Reset()
            {
                index = 0;
                Current = default;
            }
        }
    }
}
