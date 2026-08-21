using fsm_strings_parse.fsm.states;

namespace fsm_strings_parse.fsm.events
{
    public class InStringEvent(char payload) : BaseEvent(StateType.IN_STRING, payload);
}
