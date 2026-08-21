using fsm_strings_parse.fsm.states;

namespace fsm_strings_parse.fsm.events
{
    public class DefaultEvent(char payload) : BaseEvent(StateType.DEFAULT, payload);
}
