using fsm_strings_parse.fsm.events;

namespace fsm_strings_parse.fsm.states
{
    public class StateTransition()
    {
        public required StateType NextState { get; set; }
        public required ICollection<char> Output { get; set; }
    }

    public abstract class AbstractState
    {
        public abstract IEnumerable<StateType> GetAllowedTransitions();
        public bool IsTransitionAllowed(StateType stateType)
        {
            return GetAllowedTransitions().Contains(stateType);
        }
        public abstract StateTransition MakeTransition(BaseEvent e);
    }
}
