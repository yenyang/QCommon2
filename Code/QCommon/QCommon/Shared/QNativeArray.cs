using System;
using Unity.Collections;

namespace QCommonLib
{
    public struct QNativeArray<T> : IDisposable where T : unmanaged, IDisposable
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
        /// Constructor, does not create the actual array.
        /// </summary>
        public QNativeArray()
        {
            m_Array = new(1, Allocator.Persistent);
            m_Allocator = Allocator.Persistent;
            m_Active = false;
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
        /// Create the actual array
        /// </summary>
        /// <param name="length">How many possible elements should the array contain.</param>
        /// <param name="allocator">The allocator to use for the native array.</param>
        public void Create(int length, Allocator allocator = Allocator.TempJob)
        {
            Dispose();
            m_Array = new(length, allocator);
            m_Allocator = allocator;
            m_Active = true;
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
    }
}
