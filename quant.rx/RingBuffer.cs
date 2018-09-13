using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace quant.rx
{
    /// <summary>
    /// RingBuffer Aka Circular Buffer of Fixed Size
    /// List keeps track of latest elements.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RingBuffer<T>
    {
        readonly T[] _buffer = null;
        uint _head = 0;
        uint _tail = 0;
        uint _size = 0;

        #region ctor
        public RingBuffer(uint length) {
            // allocate fixed length buffer
            _buffer = new T[length];
        }
        #endregion
        #region properties
        public uint Capacity => (uint)_buffer.Length;
        public bool IsFull => Size == Capacity;
        public bool IsEmpty => Size == 0;
        public uint Size => _size;
        #endregion

        public void Enqueue(T item)
        {
            if (IsFull) {
                // override old data
                _buffer[_tail] = item;
                _tail = (_tail + 1) % Capacity;
                _head = _tail;

            } else {
                _buffer[_tail] = item;
                _tail = (_tail + 1) % Capacity;
                ++_size;
            }
        }

        public T Dequeue()
        {
            if (IsEmpty) return default(T);
            ////Read data and advance the tail (we now have a free space)
            var val = _buffer[_head];
            _buffer[_head] = default(T);
            _head = (_head + 1) % Capacity;
            --_size;
            return val;
        }
        #region IEnumerable<T> Members
        public IEnumerator<T> GetEnumerator()
        {
            uint idx = _head;
            for (int itr = 0; itr < Size; ++itr)
                yield return _buffer[(idx + itr) % Capacity];
        }
        #endregion
        /// <summary>
        /// make a copy values in sequence of insertion
        /// </summary>
        /// <returns></returns>
        public T[] ToArray()
        {
            var ret = new T[Size];
            for (int itr = 0; itr < Size; ++itr)
            {
                ret[itr] = _buffer[(_head + itr) % Capacity];
            }
            return ret;
        }
        /// <summary>
        /// Gets the element at the specified index
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public T this[int idx] {
            get {
                return _buffer[(_head + idx) % Capacity];
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public T Min() {
            return IsFull ? _buffer.Min() : ToArray().Min();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public T Max() {
            return IsFull ? _buffer.Max() : ToArray().Max();
        }
        /// <summary>
        /// Extension function to update the values.
        /// </summary>
        /// <param name="func"></param>
        public void Adjust(Func<T, T> func)
        {
            for (int itr = 0; itr < Size; ++itr)
            {
                long idx = (_head + itr) % Capacity;
                _buffer[idx] = func(_buffer[idx]);
            }

        }
    }
}
