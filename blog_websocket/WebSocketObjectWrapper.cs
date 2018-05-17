using System;
namespace blog_websocket
{
    public class WebSocketObjectWrapper<T>
    {
		public WebSocketObjectWrapper(T t, Actions action, BlogObjects blogObject)
		{
			ClassName = blogObject;
			Action = action;
			Obj = t;
		}

		private MessageType messageType = MessageType.BUSINESS_OBJECT;

		public MessageType MessageType
		{
			get {
				return messageType;
			}
			set {
				messageType = value;
			}
		}

        public BlogObjects ClassName
        {
            get;
            set;
        }

        public Actions Action
        {
            get;
            set;
        }

        public T Obj
        {
            get;
            set;
        }
    }
}
