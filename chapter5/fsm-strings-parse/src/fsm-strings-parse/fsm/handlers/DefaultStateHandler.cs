using fsm_strings_parse.fsm.events;

namespace fsm_strings_parse.fsm.states
{
    public class DefaultStateHandler : AbstractStateHandler
    {
        public override IEnumerable<StateType> GetAllowedTransitions()
        {
            return new[] { StateType.DEFAULT, StateType.IN_STRING };
        }
        public override StateHandleResult MakeTransition(Event e)
        {
            if (e.Type == EventType.QUOTE)
            {
                return new StateHandleResult
                {
                    NextState = StateType.IN_STRING,
                    Output = null
                };
            }

            return new StateHandleResult
            {
                NextState = StateType.DEFAULT,
                Output = null
            };
        }
    }
}
