using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace ECore
{
    public interface IMarshallable
    {
        bool Read(CMessage msg);
        void Write(CMessage msg);
    }

    public class Marshaler
    {
        public static void Write(CMessage msg, byte p)
        {
            msg.Write(p);
        }
        public static void Read(CMessage msg, out byte p)
        {
            msg.Read(out p);
        }

        public static void Write(CMessage msg, sbyte p)
        {
            msg.Write(p);
        }
        public static void Read(CMessage msg, out sbyte p)
        {
            msg.Read(out p);
        }

        public static void Write(CMessage msg, bool p)
        {
            msg.Write(p);
        }
        public static void Read(CMessage msg, out bool p)
        {
            msg.Read(out p);
        }

        public static void Write(CMessage msg, short p)
        {
            msg.Write(p);
        }
        public static void Read(CMessage msg, out short p)
        {
            msg.Read(out p);
        }

        public static void Write(CMessage msg, UInt16 p)
        {
            msg.Write(p);
        }
        public static void Read(CMessage msg, out UInt16 p)
        {
            msg.Read(out p);
        }

        public static void Write(CMessage msg, Int32 p)
        {
            msg.Write(p);
        }
        public static void Read(CMessage msg, out Int32 p)
        {
            msg.Read(out p);
        }

        public static void Write(CMessage msg, UInt32 p)
        {
            msg.Write(p);
        }
        public static void Read(CMessage msg, out UInt32 p)
        {
            msg.Read(out p);
        }

        public static void Write(CMessage msg, Int64 p)
        {
            msg.Write(p);
        }
        public static void Read(CMessage msg, out Int64 p)
        {
            msg.Read(out p);
        }

        public static void Write(CMessage msg, UInt64 p)
        {
            msg.Write(p);
        }
        public static void Read(CMessage msg, out UInt64 p)
        {
            msg.Read(out p);
        }

        public static void Write(CMessage msg, float p)
        {
            msg.Write(p);
        }
        public static void Read(CMessage msg, out float p)
        {
            msg.Read(out p);
        }

        public static void Write(CMessage msg, double p)
        {
            msg.Write(p);
        }
        public static void Read(CMessage msg, out double p)
        {
            msg.Read(out p);
        }

        public static void Write(CMessage msg, PackInternal p)
        {
            msg.Write(p);
        }
        public static void Read(CMessage msg, out PackInternal p)
        {
            msg.Read(out p);
        }

        public static void Write(CMessage msg, RemoteID p)
        {
            msg.Write(p);
        }
        public static void Read(CMessage msg, out RemoteID p)
        {
            msg.Read(out p);
        }

        public static void Write(CMessage msg, string p)
        {
            msg.Write(p);
        }
        public static void Read(CMessage msg, out string p)
        {
            msg.Read(out p);
        }

        public static void Write(CMessage msg, Guid p)
        {
            msg.Write(p);
        }
        public static void Read(CMessage msg, out Guid p)
        {
            msg.Read(out p);
        }

        public static void Write(CMessage msg, CPackOption p)
        {
            msg.Write(p.m_Dummy);
            msg.Write(p.m_remote);
            msg.Write(p.m_from);
            msg.Write(p.m_protocol);
            msg.Write(p.m_pack_mode);
            msg.Write(p.m_dmy_ex1);
            msg.Write(p.m_dmy_ex2);
        }
        public static void Read(CMessage msg, out CPackOption p)
        {
            CPackOption data = new CPackOption();
            msg.Read(out data.m_Dummy);
            msg.Read(out data.m_remote);
            msg.Read(out data.m_from);
            msg.Read(out data.m_protocol);
            msg.Read(out data.m_pack_mode);
            msg.Read(out data.m_dmy_ex1);
            msg.Read(out data.m_dmy_ex2);
            p = data;
        }

        public static void Write(CMessage msg, NetAddress p)
        {
            msg.Write(p);
        }
        public static void Read(CMessage msg, out NetAddress p)
        {
            msg.Read(out p);
        }

        public static void Write(CMessage msg, PacketType p)
        {
            short data = (short)p;
            msg.Write(data);
        }
        public static void Read(CMessage msg, out PacketType p)
        {
            short data;
            msg.Read(out data);
            p = (PacketType)data;
        }

        //// 기본 Array Raw 타입
        //public static void Write(CMessage msg, int[] p)
        //{
        //    Int32 data = p.Count();
        //    msg.Write(data);

        //    for (Int32 i = 0; i < data; i++)
        //    {
        //        msg.Write(p[i]);
        //    }
        //}
        //public static void Read(CMessage msg, out int[] p)
        //{
        //    Int32 data;
        //    msg.Read(out data);

        //    p = new int[data];
        //    for (Int32 i = 0; i < data; i++)
        //    {
        //        msg.Read(out p[i]);
        //    }
        //}
        //public static void Write(CMessage msg, byte[] p)
        //{
        //    Int32 data = p.Count();
        //    msg.Write(data);

        //    for (Int32 i = 0; i < data; i++)
        //    {
        //        msg.Write(p[i]);
        //    }
        //}
        //public static void Read(CMessage msg, out byte[] p)
        //{
        //    Int32 data;
        //    msg.Read(out data);

        //    p = new byte[data];
        //    for (Int32 i = 0; i < data; i++)
        //    {
        //        msg.Read(out p[i]);
        //    }
        //}
        //public static void Write(CMessage msg, RemoteID[] p)
        //{
        //    Int32 data = p.Count();
        //    msg.Write(data);

        //    for (Int32 i = 0; i < data; i++)
        //    {
        //        msg.Write(p[i]);
        //    }
        //}
        //public static void Read(CMessage msg, out RemoteID[] p)
        //{
        //    Int32 data;
        //    msg.Read(out data);

        //    p = new RemoteID[data];
        //    for (Int32 i = 0; i < data; i++)
        //    {
        //        msg.Read(out p[i]);
        //    }
        //}


        protected static void ReadL<T>(CMessage msg, out List<T> value) where T : IMarshallable, new()
        {
            value = new List<T>();
            int count = 0;
            msg.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                T elem = new T();
                elem.Read(msg);
                value.Add(elem);
            }
        }
        protected static void WriteL<T>(CMessage msg, List<T> value) where T : IMarshallable
        {
            msg.Write(value.Count);
            List<T>.Enumerator valueEnum = value.GetEnumerator();
            while (valueEnum.MoveNext())
            {
                T elem = valueEnum.Current;
                elem.Write(msg);
            }
        }



        protected static void ReadD<T>(CMessage msg, out Dictionary<int, T> value) where T : IMarshallable, new()
        {
            value = new Dictionary<int, T>();
            int count = 0;
            msg.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                int key;
                T elem = new T();
                msg.Read(out key);
                elem.Read(msg);
                value.Add(key, elem);
            }
        }
        protected static void WriteD<T>(CMessage msg, Dictionary<int, T> value) where T : IMarshallable
        {
            int size = value.Count;
            msg.Write(size);
            foreach (var pair in value)
            {
                msg.Write(pair.Key);
                pair.Value.Write(msg);
            }
        }

        protected static void ReadD<T>(CMessage msg, out Dictionary<uint, T> value) where T : IMarshallable, new()
        {
            value = new Dictionary<uint, T>();
            int count = 0;
            msg.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                uint key;
                T elem = new T();
                msg.Read(out key);
                elem.Read(msg);
                value.Add(key, elem);
            }
        }
        protected static void WriteD<T>(CMessage msg, Dictionary<uint, T> value) where T : IMarshallable
        {
            int size = value.Count;
            msg.Write(size);
            foreach (var pair in value)
            {
                msg.Write(pair.Key);
                pair.Value.Write(msg);
            }
        }

        protected static void ReadD<T>(CMessage msg, out Dictionary<Int64, T> value) where T : IMarshallable, new()
        {
            value = new Dictionary<Int64, T>();
            int count = 0;
            msg.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                Int64 key;
                T elem = new T();
                msg.Read(out key);
                elem.Read(msg);
                value.Add(key, elem);
            }
        }
        protected static void WriteD<T>(CMessage msg, Dictionary<Int64, T> value) where T : IMarshallable
        {
            int size = value.Count;
            msg.Write(size);
            foreach (var pair in value)
            {
                msg.Write(pair.Key);
                pair.Value.Write(msg);
            }
        }

        protected static void ReadD<T>(CMessage msg, out Dictionary<UInt64, T> value) where T : IMarshallable, new()
        {
            value = new Dictionary<UInt64, T>();
            int count = 0;
            msg.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                UInt64 key;
                T elem = new T();
                msg.Read(out key);
                elem.Read(msg);
                value.Add(key, elem);
            }
        }
        protected static void WriteD<T>(CMessage msg, Dictionary<UInt64, T> value) where T : IMarshallable
        {
            int size = value.Count;
            msg.Write(size);
            foreach (var pair in value)
            {
                msg.Write(pair.Key);
                pair.Value.Write(msg);
            }
        }

        protected static void ReadD<T>(CMessage msg, out Dictionary<string, T> value) where T : IMarshallable, new()
        {
            value = new Dictionary<string, T>();
            int count = 0;
            msg.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                string key;
                T elem = new T();
                msg.Read(out key);
                elem.Read(msg);
                value.Add(key, elem);
            }
        }
        protected static void WriteD<T>(CMessage msg, Dictionary<string, T> value) where T : IMarshallable
        {
            int size = value.Count;
            msg.Write(size);
            foreach (var pair in value)
            {
                msg.Write(pair.Key);
                pair.Value.Write(msg);
            }
        }


        public static bool ReadL(CMessage msg, out List<int> value)
        {
            value = new List<int>();
            int count = 0;
            msg.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                int elem;
                Read(msg, out elem);
                value.Add(elem);
            }
            return true;
        }
        public static void WriteL(CMessage msg, List<int> value)
        {
            msg.Write(value.Count);
            foreach (var temp in value)
            {
                Write(msg, temp);
            }
        }

        public static bool ReadL(CMessage msg, out List<uint> value)
        {
            value = new List<uint>();
            int count = 0;
            msg.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                uint elem;
                Read(msg, out elem);
                value.Add(elem);
            }
            return true;
        }
        public static void WriteL(CMessage msg, List<uint> value)
        {
            msg.Write(value.Count);
            foreach (var temp in value)
            {
                Write(msg, temp);
            }
        }


        public static bool ReadL(CMessage msg, out List<Int64> value)
        {
            value = new List<Int64>();
            int count = 0;
            msg.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                Int64 elem;
                Read(msg, out elem);
                value.Add(elem);
            }
            return true;
        }
        public static void WriteL(CMessage msg, List<Int64> value)
        {
            msg.Write(value.Count);
            foreach (var temp in value)
            {
                Write(msg, temp);
            }
        }

        public static bool ReadL(CMessage msg, out List<UInt64> value)
        {
            value = new List<UInt64>();
            int count = 0;
            msg.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                UInt64 elem;
                Read(msg, out elem);
                value.Add(elem);
            }
            return true;
        }
        public static void WriteL(CMessage msg, List<UInt64> value)
        {
            msg.Write(value.Count);
            foreach (var temp in value)
            {
                Write(msg, temp);
            }
        }

        public static bool ReadL(CMessage msg, out List<bool> value)
        {
            value = new List<bool>();
            int count = 0;
            msg.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                bool elem;
                Read(msg, out elem);
                value.Add(elem);
            }
            return true;
        }
        public static void WriteL(CMessage msg, List<bool> value)
        {
            msg.Write(value.Count);
            foreach (var temp in value)
            {
                Write(msg, temp);
            }
        }

        public static bool ReadL(CMessage msg, out List<string> value)
        {
            value = new List<string>();
            int count = 0;
            msg.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                string elem;
                Read(msg, out elem);
                value.Add(elem);
            }
            return true;
        }
        public static void WriteL(CMessage msg, List<string> value)
        {
            msg.Write(value.Count);
            foreach (var temp in value)
            {
                Write(msg, temp);
            }
        }

        public static bool Read(CMessage msg, out DateTime value)
        {
            long val;
            Read(msg, out val);
            value = new DateTime(val);
            return true;
        }
        public static void Write(CMessage msg, DateTime value)
        {
            //Write(msg, value.ToBinary()); // 이렇게 하면 kind에 따라 복원에 문제 발생되는 경우가 있다
            Write(msg, value.Ticks);
        }
    }
}
