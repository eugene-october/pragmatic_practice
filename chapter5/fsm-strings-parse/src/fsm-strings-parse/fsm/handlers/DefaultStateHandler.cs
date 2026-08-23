namespace fsm_strings_parse.fsm.handlers
{
    public class DefaultStateHandler : StateHandler
    {
        public override IEnumerable<States> GetAllowedTransitions()
        {
            return new[] { States.DEFAULT, States.IN_STRING };
        }
        public override StateHandleResult MakeTransition(Event e)
        {
            if (e.Type == Trigger.QUOTE)
            {
                return new StateHandleResult
                {
                    NextState = States.IN_STRING,
                    Output = null
                };
            }

            return new StateHandleResult
            {
                NextState = States.DEFAULT,
                Output = null
            };
        }
    }
}
