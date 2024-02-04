using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;

namespace QCommonLib
{
    public struct QNativeList<T> : IDisposable, INativeDisposable, IEnumerable<T> where T : unmanaged, IDisposable//, INativeDisposable
    {
        /// <summary>
        /// Whether or not the list has valid elements. Different from m_Buffer.IsCreated in that IsCreated only considers if the list has been created.
        /// Note: Assumes that if any element is valid, all elements are valid.
        /// </summary>
        internal bool m_Active;
        /// <summary>
        /// The actual native list.
        /// </summary>
        internal NativeList<T> m_List;
        /// <summary>
        /// The number of elements that the list can contain.
        /// Note: Does not mean these elements have been created.
        /// </summary>
        internal readonly int Length => m_List.Length;
        /// <summary>
        /// The allocator is saved so it can be read back during debugging.
        /// </summary>
        internal Allocator m_Allocator;

        /// <summary>
        /// Constructor, creates actual array.
        /// </summary>
        public QNativeList(int length, Allocator allocator = Allocator.TempJob)
        {
            m_Allocator = allocator;
            m_Active = true;
            m_List = new(length, allocator);
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
                    throw new Exception($"Attempting to get element {i} in non-active NativeList");
                }
                return m_List[i];
            }
            set
            {
                if (!m_List.IsCreated)
                {
                    throw new Exception($"Attempting to set element {i} in non-created NativeList");
                }
                m_Active = true;
                m_List[i] = value;
            }
        }

        /// <summary>
        /// Append an element to the end of the list
        /// </summary>
        /// <param name="element">The element to add</param>
        public void Add(T element)
        {
            m_List.Add(element);
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
                    m_List[i].Dispose();
                }
                m_List.Dispose();
                m_Active = false;
            }
        }

        /// <summary>
        /// Cleanup the array including its elements, if any have been set.
        /// Note: Assumes that if any element has been set, all elements have been set.
        /// </summary>
        public JobHandle Dispose(JobHandle handle)
        {
            if (m_Active)
            {
                // We don't know if the elements are INativeDisposable and casting will cause a d
                if (m_List.Length > 0 && m_List[0] is INativeDisposable)
                {
                    //MethodInfo method = m_List[0].GetType().GetMethod("Dispose", new Type[] { typeof(JobHandle) });
                    for (int i = 0; i < Length; i++)
                    {
                        //handle = (JobHandle)method.Invoke(m_List[i], new object[] { handle });
                        handle = ((INativeDisposable)m_List[i]).Dispose(handle);
                    }
                }
                handle = m_List.Dispose(handle);
                m_Active = false;
            }
            return handle;
        }

        #region Enumeration
        public IEnumerator<T> GetEnumerator() => new Enumeration(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumeration(this);
        private class Enumeration : IEnumerator<T>
        {
            private int _Position = -1;
            private QNativeList<T> _List;

            public Enumeration(QNativeList<T> a)
            {
                _List = a;
            }

            public T Current => _List[_Position];

            object IEnumerator.Current => Current;

            public void Dispose()
            { }

            public bool MoveNext()
            {
                _Position++;
                return (_Position < _List.Length);
            }

            public void Reset()
            {
                _Position = -1;
            }
        }
        #endregion
    }
}
