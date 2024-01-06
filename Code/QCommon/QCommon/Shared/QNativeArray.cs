using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

namespace QCommonLib
{
    public struct QNativeArray<T> : IDisposable, IEnumerable<T> where T : unmanaged, IDisposable
    {
        /// <summary>
        /// Whether or not the array has valid elements. Different from m_Buffer.IsCreated in that IsCreated only considers if the array has been created.
        /// Note: Assumes that if any element is valid, all elements are valid.
        /// </summary>
        internal bool m_Active;
        /// <summary>
        /// The actual native array.
        /// </summary>
        internal NativeArray<T> m_Array;
        /// <summary>
        /// The number of elements that the array can contain.
        /// Note: Does not mean these elements have been created.
        /// </summary>
        internal int Length => m_Array.Length;
        /// <summary>
        /// The allocator is saved so it can be read back during debugging.
        /// </summary>
        internal Allocator m_Allocator;

        /// <summary>
        /// Constructor, creates actual array.
        /// </summary>
        public QNativeArray(int length, Allocator allocator = Allocator.TempJob)
        {
            m_Allocator = allocator;
            m_Active = true;
            m_Array = new(length, allocator);
        }

        /// <summary>
        /// Get or Set an element of the native array.
        /// </summary>
        /// <param name="i">The element's index.</param>
        /// <returns>The element at the passed index.</returns>
        /// <exception cref="Exception">If the array isn't ready for the get or set call.</exception>
        public T this[int i]
        {
            get
            {
                if (!m_Active)
                {
                    throw new Exception($"Attempting to get element {i} in non-active NativeArray");
                }
                return m_Array[i];
            }
            set
            {
                if (!m_Array.IsCreated)
                {
                    throw new Exception($"Attempting to set element {i} in non-created NativeArray");
                }
                m_Active = true;
                m_Array[i] = value;
            }
        }

        /// <summary>
        /// Cleanup the array including its elements, if any have been set.
        /// Note: Assumes that if any element has been set, all elements have been set.
        /// </summary>
        public void Dispose()
        {
            if (m_Active)
            {
                for (int i = 0; i < Length; i++)
                {
                    m_Array[i].Dispose();
                }
                m_Array.Dispose();
                m_Active = false;
            }
        }

        #region Enumeration
        public IEnumerator<T> GetEnumerator() => new Enumeration(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumeration(this);
        private class Enumeration : IEnumerator<T>
        {
            private int _Position = -1;
            private QNativeArray<T> _Array;

            public Enumeration(QNativeArray<T> a)
            {
                _Array = a;
            }

            public T Current => _Array[_Position];

            object IEnumerator.Current => Current;

            public void Dispose()
            { }

            public bool MoveNext()
            {
                _Position++;
                return (_Position < _Array.Length);
            }

            public void Reset()
            {
                _Position = -1;
            }
        }
        #endregion
    }
}
