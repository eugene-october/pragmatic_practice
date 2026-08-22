using fsm_strings_parse.fsm.events;

namespace fsm_strings_parse.fsm.states
{
    public class FinishedStringState : AbstractState
    {
        private readonly IList<char> _data;

        public FinishedStringState() => _data = [];
        public override IEnumerable<StateType> GetAllowedTransitions()
        {
            return new[] { StateType.DEFAULT };
        }
        public override StateTransition MakeTransition(BaseEvent e)
        {
            return new StateTransition
            {
                NextState = StateType.DEFAULT,
                Output = _data
            };
        }
    }
}
