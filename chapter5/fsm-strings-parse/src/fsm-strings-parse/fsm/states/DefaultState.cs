using fsm_strings_parse.fsm.events;

namespace fsm_strings_parse.fsm.states
{
    public class DefaultState : AbstractState
    {
        public override IEnumerable<StateType> GetAllowedTransitions()
        {
            return new[] { StateType.DEFAULT, StateType.IN_STRING };
        }
        public override StateTransition MakeTransition(BaseEvent e)
        {
            if (e.Type == EventType.QUOTE)
            {
                return new StateTransition
                {
                    NextState = StateType.IN_STRING,
                    Output = null
                };
            }

            return new StateTransition
            {
                NextState = StateType.DEFAULT,
                Output = null
            };
        }
    }
}
