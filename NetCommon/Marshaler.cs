using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ECore;

namespace NetCommon
{
    // 샘플 클래스
    public class CAccount : IMarshallable
    {
        public string idname;
        public string pw;

        public CAccount() { }
        public CAccount(string _id, string _pw)
        {
            idname = _id;
            pw = _pw;
        }

        public bool Read(CMessage msg)
        {
            msg.Read(out idname);
            msg.Read(out pw);
            return true;
        }
        public void Write(CMessage msg)
        {
            msg.Write(idname);
            msg.Write(pw);
        }
    }

    public partial class Marshaler : ECore.Marshaler
    {
        public static void Write(CMessage msg, CAccount p)
        {
            p.Write(msg);
        }
        public static void Read(CMessage msg, out CAccount p)
        {
            p = new CAccount();
            p.Read(msg);
        }

        public static void Write(CMessage msg, List<CAccount> p)
        {
            Write(msg, p);
        }
        public static void Read(CMessage msg, out List<CAccount> p)
        {
            Read(msg, out p);
        }

        public static void Write(CMessage msg, Dictionary<int, CAccount> p)
        {
            Write(msg, p);
        }
        public static void Read(CMessage msg, out Dictionary<int, CAccount> p)
        {
            Read(msg, out p);
        }
    }
}
