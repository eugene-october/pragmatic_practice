const ARG_FILTER_NAME = "--match";
const ARG_FIX = "--fix";
const supportedCommands = [ARG_FILTER_NAME, ARG_FIX];

class InvalidStateError extends Error { }

/**
 * @param {string[]} args
 */
export function parseArgs(args) {
    const states = {
        UNKNOWN: "UNKNOWN",
        FILTER: "FILTER",
        FIX: "FIX",
        EMPTY: "EMPTY",
    };
    let currentState = states.EMPTY;
    let matcher = null;
    let shouldFix = false;

    for (const arg of args) {
        if (currentState != states.EMPTY) {
            if (supportedCommands.includes(arg)) {
                throwInvalidStateError(currentState, arg);
            }
        }

        if (currentState === states.EMPTY) {
            if (!supportedCommands.includes(arg)) {
                throwInvalidStateError(currentState, arg);
            }
        }

        if (arg === ARG_FILTER_NAME) {
            currentState = states.FILTER;

            continue;
        }

        if (arg === ARG_FIX) {
            currentState = states.FIX;

            continue;
        }

        if (currentState === states.FIX) {
            if (!["1", "true", "0", "false"].includes(arg)) {
                throw new InvalidStateError(`Wrong arg for state: [state=${currentState}, arg=${arg}]`);
            }

            shouldFix = arg === "1" || arg === "true";
            currentState = states.EMPTY;

            continue;
        }

        if (currentState === states.FILTER) {
            try {
                matcher = new RegExp(arg);

                currentState = states.EMPTY;

                continue;
            } catch (e) {
                throwInvalidStateError(currentState, arg, e);
            }
        }
    }

    if (currentState !== states.EMPTY) {
        throwInvalidStateError(currentState, "null");
    }

    return {
        matcher,
        shouldFix,
    };
}

function throwInvalidStateError(state, arg, ex) {
    throw new InvalidStateError(`Wrong arg for state: [state=${state}, arg=${arg}${ex ? `, exception=${ex}` : ""}]`);
}
