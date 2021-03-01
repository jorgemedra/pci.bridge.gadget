using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OD_Finesse_Bridge.utils
{
    class BridgeExceptions
    {
    }
    [Serializable()]
    public class InProgressException : System.Exception
    {
        public InProgressException() : base() { }
        public InProgressException(string message) : base(message) { }
        public InProgressException(string message, System.Exception inner) : base(message, inner) { }

        protected InProgressException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
    public class NullFieldException : System.Exception
    {
        public NullFieldException() : base() { }
        public NullFieldException(string message) : base(message) { }
        public NullFieldException(string message, System.Exception inner) : base(message, inner) { }

        protected NullFieldException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
    public class InvalidDecimalException : System.Exception
    {
        public InvalidDecimalException() : base() { }
        public InvalidDecimalException(string message) : base(message) { }
        public InvalidDecimalException(string message, System.Exception inner) : base(message, inner) { }

        protected InvalidDecimalException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
}
