using fsm_strings_parse.fsm.events;

namespace fsm_strings_parse.fsm.states
{
    public class AddNextStateHandler : AbstractStateHandler
    {
        public override IEnumerable<StateType> GetAllowedTransitions()
        {
            return new[] { StateType.IN_STRING };
        }
        public override StateHandleResult MakeTransition(Event e)
        {
            return new StateHandleResult
            {
                NextState = StateType.IN_STRING,
                Output = e.Payload
            };
        }
    }
}
