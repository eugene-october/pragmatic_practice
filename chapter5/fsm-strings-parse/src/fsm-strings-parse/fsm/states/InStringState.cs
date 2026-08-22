using fsm_strings_parse.fsm.events;

namespace fsm_strings_parse.fsm.states
{
    public class InStringState : AbstractState
    {
        private readonly IList<char> _data;

        public InStringState() => _data = [];
        public override IEnumerable<StateType> GetAllowedTransitions()
        {
            return new[] { StateType.IN_STRING, StateType.ADD_NEXT, StateType.FINISHED_STRING };
        }
        public override StateTransition MakeTransition(BaseEvent e)
        {
            if (e.Type == EventType.ESCAPE)
            {
                return new StateTransition
                {
                    NextState = StateType.ADD_NEXT,
                    Output = _data
                };
            }

            if (e.Type == EventType.QUOTE)
            {
                return new StateTransition
                {
                    NextState = StateType.FINISHED_STRING,
                    Output = _data
                };
            }

            _data.Add(e.Payload);

            return new StateTransition
            {
                NextState = StateType.IN_STRING,
                Output = _data
            };
        }
    }
}
