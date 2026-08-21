using fsm_strings_parse.fsm.states;

namespace fsm_strings_parse.fsm.events
{
    public class FinishedStringEvent(char payload) : BaseEvent(StateType.FINISHED_STRING, payload);
}
