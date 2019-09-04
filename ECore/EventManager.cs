using System;
using System.Collections;
using System.Collections.Generic;

namespace ECore
{
    public enum NETWORK_EVENT : byte
    {
        connected,      // 접속 완료.

        connect_fail,   // 접속 실패(클라이언트만 해당)

        disconnected,   // 연결 끊김

        pkt,

        end
    }

    public class EventManager
    {
        // 동기화 객체.
        object cs_event;

        // 네트워크 엔진에서 발생된 이벤트들을 보관해놓는 큐.
        Queue<CRecvedMsg> network_events;

        public EventManager()
        {
            this.network_events = new Queue<CRecvedMsg>();
            this.cs_event = new object();
        }

        public void enqueue_network_event(NETWORK_EVENT event_type)
        {
            lock (this.cs_event)
            {
                this.network_events.Enqueue(new CRecvedMsg(event_type));
            }
        }

        public bool has_event()
        {
            lock (this.cs_event)
            {
                return this.network_events.Count > 0;
            }
        }

        public CRecvedMsg dequeue_network_event()
        {
            lock (this.cs_event)
            {
                return this.network_events.Dequeue();
            }
        }

        public void enqueue_network_message(CRecvedMsg buffer)
        {
            lock (this.cs_event)
            {
                this.network_events.Enqueue(buffer);
            }
        }
    }
}