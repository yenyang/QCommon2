using System;
using Unity.Collections;

namespace QCommonLib
{
    public struct QNativeBuffer<T> : IDisposable where T : unmanaged, IDisposable
    {
        internal bool m_Active;
        internal NativeArray<T> m_Buffer;
        internal int Length => m_Buffer.Length;

        public QNativeBuffer()
        {
            m_Active = false;
            m_Buffer = new();
        }

        public T this[int i]
        {
            get
            {
                if (!m_Active)
                {
                    throw new Exception($"Attempting to set element {i} in non-active NativeBuffer");
                }
                return m_Buffer[i];
            }
            set
            {
                m_Active = true;
                m_Buffer[i] = value;
            }
        }

        public void Create(int length, Allocator allocator = Allocator.TempJob)
        {
            Dispose();
            m_Buffer = new(length, allocator);
            m_Active = true;
        }

        public void Dispose()
        {
            if (m_Active)
            {
                for (int i = 0; i < Length; i++)
                {
                    m_Buffer[i].Dispose();
                }
            }
            m_Buffer.Dispose();
            m_Active = false;
        }
    }
}
