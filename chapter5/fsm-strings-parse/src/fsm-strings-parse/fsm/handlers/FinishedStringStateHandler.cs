namespace fsm_strings_parse.fsm.handlers
{
    public class FinishedStringStateHandler : StateHandler
    {
        public override IEnumerable<States> GetAllowedTransitions()
        {
            return new[] { States.DEFAULT };
        }
        public override StateHandleResult MakeTransition(Event e)
        {
            return new StateHandleResult
            {
                NextState = States.DEFAULT,
                Output = null
            };
        }
    }
}
