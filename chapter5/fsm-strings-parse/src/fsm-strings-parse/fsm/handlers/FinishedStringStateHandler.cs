using fsm_strings_parse.fsm.events;

namespace fsm_strings_parse.fsm.states
{
    public class FinishedStringStateHandler : AbstractStateHandler
    {
        public override IEnumerable<StateType> GetAllowedTransitions()
        {
            return new[] { StateType.DEFAULT };
        }
        public override StateHandleResult MakeTransition(Event e)
        {
            return new StateHandleResult
            {
                NextState = StateType.DEFAULT,
                Output = null
            };
        }
    }
}
