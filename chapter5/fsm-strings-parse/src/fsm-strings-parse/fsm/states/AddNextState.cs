using fsm_strings_parse.fsm.events;

namespace fsm_strings_parse.fsm.states
{
    public class AddNextState : AbstractState
    {
        public override IEnumerable<StateType> GetAllowedTransitions()
        {
            return new[] { StateType.IN_STRING };
        }
        public override StateTransition MakeTransition(BaseEvent e)
        {
            return new StateTransition
            {
                NextState = StateType.IN_STRING,
                Output = e.Payload
            };
        }
    }
}
