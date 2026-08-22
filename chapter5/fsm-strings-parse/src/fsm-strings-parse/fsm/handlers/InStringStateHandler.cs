using fsm_strings_parse.fsm.events;

namespace fsm_strings_parse.fsm.states
{
    public class InStringStateHandler : AbstractStateHandler
    {
        public override IEnumerable<StateType> GetAllowedTransitions()
        {
            return new[] { StateType.IN_STRING, StateType.ADD_NEXT, StateType.FINISHED_STRING };
        }
        public override StateHandleResult MakeTransition(Event e)
        {
            if (e.Type == EventType.ESCAPE)
            {
                return new StateHandleResult
                {
                    NextState = StateType.ADD_NEXT,
                    Output = null
                };
            }

            if (e.Type == EventType.QUOTE)
            {
                return new StateHandleResult
                {
                    NextState = StateType.FINISHED_STRING,
                    Output = null
                };
            }

            return new StateHandleResult
            {
                NextState = StateType.IN_STRING,
                Output = e.Payload
            };
        }
    }
}
