using fsm_strings_parse.fsm.events;

namespace fsm_strings_parse.fsm.states
{
    public class StateHandleResult()
    {
        public required StateType NextState { get; set; }
        // Output is produced only if needed. Some tokens should be ignored
        public char? Output { get; set; }
    }

    public abstract class AbstractStateHandler
    {
        public abstract IEnumerable<StateType> GetAllowedTransitions();
        public bool IsTransitionAllowed(StateType stateType)
        {
            return GetAllowedTransitions().Contains(stateType);
        }
        public abstract StateHandleResult MakeTransition(Event e);
    }
}
