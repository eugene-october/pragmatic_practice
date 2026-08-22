using fsm_strings_parse.fsm.events;

namespace fsm_strings_parse.fsm.states
{
    public class AddNextState : AbstractState
    {
        private readonly IList<char> _data;

        public AddNextState() => _data = [];
        public override IEnumerable<StateType> GetAllowedTransitions()
        {
            return new[] { StateType.IN_STRING };
        }
        public override StateTransition MakeTransition(BaseEvent e)
        {
            _data.Add(e.Payload);

            return new StateTransition
            {
                NextState = StateType.IN_STRING,
                Output = _data
            };
        }
    }
}
