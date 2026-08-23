namespace fsm_strings_parse.fsm.handlers
{
    public class InStringStateHandler : StateHandler
    {
        public override IEnumerable<States> GetAllowedTransitions()
        {
            return new[] { States.IN_STRING, States.ADD_NEXT, States.FINISHED_STRING };
        }
        public override StateHandleResult MakeTransition(Event e)
        {
            if (e.Type == Trigger.ESCAPE)
            {
                return new StateHandleResult
                {
                    NextState = States.ADD_NEXT,
                    Output = null
                };
            }

            if (e.Type == Trigger.QUOTE)
            {
                return new StateHandleResult
                {
                    NextState = States.FINISHED_STRING,
                    Output = null
                };
            }

            return new StateHandleResult
            {
                NextState = States.IN_STRING,
                Output = e.Payload
            };
        }
    }
}
