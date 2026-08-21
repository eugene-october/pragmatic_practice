namespace fsm_strings_parse.fsm.states
{
    public enum StateType
    {
        DEFAULT, // skip current
        IN_STRING, // " met
        FINISHED_STRING, // " closed, concat whole string
        ADD_NEXT, // \ met, take next char
    }

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
        public abstract StateTransition MakeTransition(char e);
    }
}
