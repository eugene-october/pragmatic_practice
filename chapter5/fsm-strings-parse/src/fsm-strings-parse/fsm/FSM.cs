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
        private AbstractStateHandler _currentStateHandler = CreateStateHandler(_defaultStateType);

        public FSMResult Process(char e)
        {
            Event fsmEvent = CreateEvent(e);
            StateHandleResult stateHandleResult = _currentStateHandler.MakeTransition(fsmEvent);

            if (!_currentStateHandler.IsTransitionAllowed(stateHandleResult.NextState))
            {
                throw new Exception("Invalid transition");
            }

            _currentStateHandler = CreateStateHandler(stateHandleResult.NextState);

            return new FSMResult
            {
                Data = stateHandleResult.Output
            };
        }

        private static AbstractStateHandler CreateStateHandler(StateType type)
        {
            switch (type)
            {
                case StateType.DEFAULT:
                    return new DefaultStateHandler();
                case StateType.IN_STRING:
                    return new InStringStateHandler();
                case StateType.ADD_NEXT:
                    return new AddNextStateHandler();
                case StateType.FINISHED_STRING:
                    return new FinishedStringStateHandler();
                default:
                    throw new NotImplementedException();
            }
        }

        private static Event CreateEvent(char token)
        {
            return new Event
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
