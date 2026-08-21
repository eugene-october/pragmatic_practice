using fsm_strings_parse.fsm.states;

namespace fsm_strings_parse.fsm.events
{

    public abstract class BaseEvent(StateType name, char payload)
    {
        public StateType Name = name;
        public char Payload = payload;
    }
}
