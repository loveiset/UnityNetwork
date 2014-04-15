namespace UnityNetwork
{
    static public class MessageIdentifiers
    {
        public enum ID
        {
            NULL = 0,
            CONNECTION_REQUEST_ACCEPTED,
            CONNECTION_ATTEMPT_FAILED,
            CONNECTION_LOST,
            NEW_INCOMING_CONNECTION,
            ID_CHAT,
        };
    }
}
