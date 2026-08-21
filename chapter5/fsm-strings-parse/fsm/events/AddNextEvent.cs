using fsm_strings_parse.fsm.states;

namespace fsm_strings_parse.fsm.events
{
    public class AddNextEvent(char payload) : BaseEvent(StateType.ADD_NEXT, payload);
}
