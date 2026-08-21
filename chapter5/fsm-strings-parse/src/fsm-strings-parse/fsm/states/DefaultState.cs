namespace fsm_strings_parse.fsm.states
{
    public class DefaultState : AbstractState
    {
        private readonly IList<char> _data;

        public DefaultState() => _data = [];
        public override IEnumerable<StateType> GetAllowedTransitions()
        {
            return new[] { StateType.DEFAULT, StateType.IN_STRING };
        }
        public override StateTransition MakeTransition(char e)
        {
            if (e == '"')
            {
                return new StateTransition
                {
                    NextState = StateType.IN_STRING,
                    Output = _data
                };
            }

            return new StateTransition
            {
                NextState = StateType.DEFAULT,
                Output = _data
            };
        }
    }
}
