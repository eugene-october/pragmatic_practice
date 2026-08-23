namespace fsm_strings_parse.fsm.handlers
{
    public class AddNextStateHandler : StateHandler
    {
        public override IEnumerable<States> GetAllowedTransitions()
        {
            return new[] { States.IN_STRING };
        }
        public override StateHandleResult MakeTransition(Event e)
        {
            return new StateHandleResult
            {
                NextState = States.IN_STRING,
                Output = e.Payload
            };
        }
    }
}
