using System.Collections.ObjectModel;

namespace fsm_minimalistic.fsm_m
{
    internal enum States
    {
        DEFAULT,
        IN_STRING,
        TAKE_NEXT,
        FINISHED_STRING,
    }

    internal enum Trigger
    {
        QUOTE,
        ESCAPE_CHAR,
        CHAR,
    }

    internal static class Handlers
    {
        internal static States HandleDefault(Trigger t)
        {
            if (t == Trigger.QUOTE)
            {
                return States.IN_STRING;
            }

            return States.DEFAULT;
        }
        internal static States HandleInString(Trigger t)
        {
            if (t == Trigger.CHAR)
            {
                return States.IN_STRING;
            }

            if (t == Trigger.ESCAPE_CHAR)
            {
                return States.TAKE_NEXT;
            }

            if (t == Trigger.QUOTE)
            {
                return States.FINISHED_STRING;
            }

            throw new Exception("Fatal error");
        }
        internal static States HandleTakeNext(Trigger _)
        {
            return States.IN_STRING;
        }
        internal static States HandleFinished(Trigger _)
        {
            return States.DEFAULT;
        }
    }

    public class FSMResult
    {
        public char? Data { get; set; }
    }

    public class FSMM
    {
        private States _currentState = States.DEFAULT;
        private readonly ReadOnlyDictionary<States, Func<Trigger, States>> _statesToHandlers = new(
                new Dictionary<States, Func<Trigger, States>>
                {
                    [States.DEFAULT] = Handlers.HandleDefault,
                    [States.IN_STRING] = Handlers.HandleInString,
                    [States.TAKE_NEXT] = Handlers.HandleTakeNext,
                    [States.FINISHED_STRING] = Handlers.HandleFinished,
                }
        );

        private static Trigger GetTrigger(char data)
        {
            return data switch
            {
                '"' => Trigger.QUOTE,
                '\\' => Trigger.ESCAPE_CHAR,
                _ => Trigger.CHAR,
            };
        }

        private static FSMResult EmptyResult => new()
        {
            Data = null,
        };

        public FSMResult Process(char data)
        {
            var _prevState = _currentState;
            Trigger trigger = GetTrigger(data);

            if (_statesToHandlers.TryGetValue(_currentState, out var currentHandler))
            {
                _currentState = currentHandler(trigger);
            }
            else
            {
                throw new Exception("Internal state logic fatal error");
            }

            switch (_currentState)
            {
                case States.DEFAULT:
                case States.TAKE_NEXT:
                case States.FINISHED_STRING:
                    return EmptyResult;
                case States.IN_STRING:
                    // meh
                    if (_prevState == States.DEFAULT)
                    {
                        return EmptyResult;
                    }

                    return new FSMResult
                    {
                        Data = data
                    };
                default:
                    throw new Exception("Fatal state transition error");
            }
        }
    }
}
