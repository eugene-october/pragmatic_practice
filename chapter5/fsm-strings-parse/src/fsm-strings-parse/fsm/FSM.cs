using fsm_strings_parse.fsm.events;
using fsm_strings_parse.fsm.states;

namespace fsm_strings_parse.fsm
{
    public class FSMResult
    {
        public char? Data { get; set; }
    }

    public enum StateType
    {
        DEFAULT, // skip current
        IN_STRING, // " met
        FINISHED_STRING, // " closed, concat whole string
        ADD_NEXT, // \ met, take next char
    }

    public enum EventType
    {
        TOKEN, // any letter
        QUOTE, // "
        ESCAPE, // \
    }

    public class FSM
    {
        private static readonly StateType _defaultStateType = StateType.DEFAULT;
        private AbstractState _currentState = CreateState(_defaultStateType);

        public FSMResult Process(char e)
        {
            var fsmEvent = CreateEvent(e);
            var newState = _currentState.MakeTransition(fsmEvent);
            _currentState = CreateState(newState.NextState);

            if (newState.Output is not null)
            {
                return new FSMResult
                {
                    Data = newState.Output
                };
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

        private static BaseEvent CreateEvent(char token)
        {
            return new BaseEvent
            {
                Type = CreateEventType(token),
                Payload = token
            };
        }

        private static EventType CreateEventType(char token)
        {
            switch (token)
            {
                case '"':
                    return EventType.QUOTE;
                case '\\':
                    return EventType.ESCAPE;
                default:
                    return EventType.TOKEN;
            }
        }
    }
}
