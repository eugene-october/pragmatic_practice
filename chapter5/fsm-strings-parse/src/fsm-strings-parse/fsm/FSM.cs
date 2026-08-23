using fsm_strings_parse.fsm.handlers;

namespace fsm_strings_parse.fsm
{
    public class FSMResult
    {
        public char? Data { get; set; }
    }

    public enum States
    {
        DEFAULT, // skip current
        IN_STRING, // " met
        FINISHED_STRING, // " closed, concat whole string
        ADD_NEXT, // \ met, take next char
    }

    public enum Trigger
    {
        TOKEN, // any letter
        QUOTE, // "
        ESCAPE, // \
    }

    public class FSM
    {
        private static readonly States _defaultStateType = States.DEFAULT;
        private StateHandler _currentStateHandler = CreateStateHandler(_defaultStateType);

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

        private static StateHandler CreateStateHandler(States type)
        {
            return type switch
            {
                States.DEFAULT => new DefaultStateHandler(),
                States.IN_STRING => new InStringStateHandler(),
                States.ADD_NEXT => new AddNextStateHandler(),
                States.FINISHED_STRING => new FinishedStringStateHandler(),
                _ => throw new NotImplementedException(),
            };
        }

        private static Event CreateEvent(char token)
        {
            return new Event
            {
                Type = CreateEventType(token),
                Payload = token
            };
        }

        private static Trigger CreateEventType(char token)
        {
            return token switch
            {
                '"' => Trigger.QUOTE,
                '\\' => Trigger.ESCAPE,
                _ => Trigger.TOKEN,
            };
        }
    }
}
