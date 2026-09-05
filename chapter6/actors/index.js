import { start, dispatch, stop, spawnStateless } from 'nact';
import { spawnCurrentUserLogin } from './currentUserLogin.js';
import { spawnCurrentUserGetter } from './currentUserGetter.js';

const system = start();

const orchestrator = spawnStateless(system, async (msg, ctx) => {
    const payload = { sender: ctx.self };

    console.log("[RECEIVED MESSAGE]: ");
    console.log("\t[SENDER NAME]: ", msg.sender.name);
    msg.error && console.log("\t[ERROR]: ", msg.error);
    msg.payload && console.log("\t[PAYLOAD]: ", msg.payload);

    if (msg.error) {
        console.error("Fatal error: ", msg.error);
        return;
    }

    if (msg.sender === system) {
        dispatch(currentUserLogin, payload);

        return;
    }

    if (msg.sender === currentUserLogin) {
        const userData = msg.payload;
        const { accessToken } = userData;

        dispatch(currentUserGetter, { ...payload, accessToken })

        return;
    }

    if (msg.sender === currentUserGetter) {
        return;
    }
});
const currentUserLogin = spawnCurrentUserLogin(orchestrator);
const currentUserGetter = spawnCurrentUserGetter(orchestrator);

dispatch(orchestrator, { sender: system });

// const greeter = spawnStateless(
//     system, // parent
//     (msg, ctx) => console.log(`Hi ${msg.name}`), // function
//     'greeter' // name
// );

// dispatch(greeter, { name: 'Jack' });
