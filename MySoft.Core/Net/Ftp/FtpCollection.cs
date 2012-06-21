using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MySoft.Net.Ftp
{
    //I decide not to use UCOMIEnumVariant since I got no idea how 
    //to Marshal int back to array. As a result, I re-define the
    //IEnumVARIANT interface to simplified my work
    [
        Guid("00020404-0000-0000-C000-000000000046"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
    ]
    public interface IEnumVARIANT
    {
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int Next(UInt32 celt, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0), Out]object[] rgelt, IntPtr pceltFetched);
        void Skip(UInt32 celt);
        void Reset();
        void Clone(int ppenum);
    }

    [Guid("A4C46780-499F-101B-BB78-00AA00383CBB")]
    public interface IVBCollection
    {
        [DispId(0)]
        object Item([In]ref object Index);

        [DispId(1)]
        void Add([In]ref object Item, [In, Optional]ref object Key, [In, Optional]ref object Before, [In, Optional]ref object After);

        [DispId(2)]
        Int32 Count();

        [DispId(3)]
        void Remove([In]ref object Index);

        [DispId(-4)]
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object _NewEnum();
    }

    [
        Guid("A4C4671C-499F-101B-BB78-00AA00383CBB"),
        ClassInterface(ClassInterfaceType.None)
    ]
    public class VbEnumableCollection : IVBCollection, IEnumVARIANT, IEnumerable
    {
        ICollection m_collection;
        IEnumerator m_enumerator;

        internal VbEnumableCollection(ICollection c)
        {
            m_collection = c;
            m_enumerator = c.GetEnumerator();
        }

        #region Implementation of IEnumerable
        public IEnumerator GetEnumerator()
        {
            return m_enumerator;
        }
        #endregion

        #region Implementation of IVBCollection
        public object Item(ref object Index)
        {
            throw new NotSupportedException("Method Item() not supported for VbEnumableCollection.");
        }
        public void Add(ref object Item, ref object Key, ref object Before, ref object After)
        {
            throw new NotSupportedException("Method Add() not supported for VbEnumableCollection.");
        }
        public Int32 Count()
        {
            return m_collection.Count;
        }
        public void Remove(ref object Index)
        {
            throw new NotSupportedException("Method Remove() not supported for VbEnumableCollection.");
        }
        public object _NewEnum()
        {
            return new VbEnumableCollection(m_collection);
        }
        #endregion

        #region Implementation of IEnumVariant
        public int Next(UInt32 celt, object[] rgelt, IntPtr pceltFetched)
        {
            if (pceltFetched != IntPtr.Zero)
                Marshal.WriteInt32(pceltFetched, 0);
            if (celt > 1)
                throw new NotSupportedException("Each time can fetch one item in VbCallableCollection.");
            if (m_enumerator.MoveNext())
            {
                rgelt[0] = m_enumerator.Current;
                if (pceltFetched != IntPtr.Zero)
                    Marshal.WriteInt32(pceltFetched, 1);
                return 0; //S_OK
            }
            else
                return 1; //S_FALSE
        }
        public void Skip(UInt32 celt)
        {
            throw new NotSupportedException("Method Skip() not supported in VbEnumerableCollection.");
        }
        public void Reset()
        {
            m_enumerator.Reset();
        }
        public void Clone(int ppenum)
        {
            throw new NotSupportedException("Cannot clone interface.");
        }
        #endregion
    }
}
