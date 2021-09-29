using System.Collections;

namespace Assets.Scripts.WT_FrameWork.MSGCenter
{
    public class CBaseEvent
    {
        protected Hashtable argments;
        protected string eventName;
        protected object sender;

        public string EventName
        {
            get { return eventName; }
            set { eventName = value; }
        }

        public Hashtable Argments
        {
            get { return argments; }
            set { argments = value; }
        }

        public object Sender
        {
            get { return sender; }
            //set { sender = value; }
        }

        public override string ToString()
        {
            return this.eventName + "[" + ((this.sender == null) ? "null" : this.sender.ToString()) + "]";
        }

        public CBaseEvent Clone()
        {
            return new CBaseEvent(this.eventName, this.argments.Clone() as Hashtable, this.sender);
        }

        public CBaseEvent(string name, Hashtable args, object sender)
        {
            this.eventName = name;
            this.argments = args;
            this.sender = sender;
            if (this.argments == null)
            {
                this.argments = new Hashtable();
            }
        }
    }
}