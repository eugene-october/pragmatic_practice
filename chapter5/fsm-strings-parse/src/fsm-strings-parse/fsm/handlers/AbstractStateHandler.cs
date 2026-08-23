namespace fsm_strings_parse.fsm.handlers
{
    public class StateHandleResult()
    {
        public required States NextState { get; set; }
        // Output is produced only if needed. Some tokens should be ignored
        public char? Output { get; set; }
    }

    public abstract class StateHandler
    {
        public abstract IEnumerable<States> GetAllowedTransitions();
        public bool IsTransitionAllowed(States stateType)
        {
            return GetAllowedTransitions().Contains(stateType);
        }
        public abstract StateHandleResult MakeTransition(Event e);
    }
}
