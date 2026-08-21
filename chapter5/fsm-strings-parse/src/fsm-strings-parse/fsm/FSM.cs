using fsm_strings_parse.fsm.states;

namespace fsm_strings_parse.fsm
{
    public class FSMResult
    {
        public IEnumerable<char>? Data { get; set; }
    }

    public class FSM
    {
        private static readonly StateType _defaultStateType = StateType.DEFAULT;
        private AbstractState _currentState = CreateState(_defaultStateType);
        private IEnumerable<char> _data = new List<char> { };

        public FSMResult Process(char e)
        {
            // TODO: e -> EventClass
            var newState = _currentState.MakeTransition(e);
            _currentState = CreateState(newState.NextState);

            if (newState.NextState == StateType.FINISHED_STRING)
            {
                return new FSMResult
                {
                    Data = _data
                };
            }

            if (newState.Output.Count > 0)
            {
                _data = _data.Concat(newState.Output);
            }


            return new FSMResult();
        }

        private static AbstractState CreateState(StateType type)
        {
            switch (type)
            {
                case StateType.DEFAULT:
                    return new DefaultState();
                case StateType.IN_STRING:
                    return new InStringState();
                case StateType.ADD_NEXT:
                    return new AddNextState();
                case StateType.FINISHED_STRING:
                    return new FinishedStringState();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
