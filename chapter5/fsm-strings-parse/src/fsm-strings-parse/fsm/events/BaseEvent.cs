namespace fsm_strings_parse.fsm.events
{

    public class BaseEvent
    {
        public EventType Type { get; set; }
        public char Payload { get; set; }
    }
}
