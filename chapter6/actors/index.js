import { start, dispatch, stop, spawnStateless } from 'nact';
import { spawnCurrentUserLogin } from './currentUserLogin.js';

const system = start();

// const greeter = spawnStateless(
//     system, // parent
//     (msg, ctx) => console.log(`Hi ${msg.name}`), // function
//     'greeter' // name
// );

// dispatch(greeter, { name: 'Jack' });

// const delay = (time) => new Promise((res) => setTimeout(res, time));
//
// const ping = spawnStateless(system, async (msg, ctx) => {
//     console.log(msg.value);
//     // ping: Pong is a little slow. So I'm giving myself a little handicap :P
//     await delay(500);
//     dispatch(msg.sender, { value: ctx.name, sender: ctx.self });
// }, 'ping');
//
// const pong = spawnStateless(system, async (msg, ctx) => {
//     console.log(`---msg.sender---${msg.sender}`);
//     console.log(msg.value);
//     await delay(100);
//     dispatch(msg.sender, { value: ctx.name, sender: ctx.self });
// }, 'pong');
//
// dispatch(ping, { value: 'begin', sender: pong });

const MAIN_LOOP_COMMAND = 'MAIN_LOOP_COMMAND';

const orchestrator = spawnStateless(system, async (msg, ctx) => {
    if (msg.action === MAIN_LOOP_COMMAND) {
        dispatch(currentUserGetter, { sender: ctx.self });

        return;
    }
    if (msg.sender === currentUserGetter) {
        const userData = msg.payload;

        console.log(`---JSON.stringify(userData, null, 4)---${JSON.stringify(userData, null, 4)}`);
    }
});
const currentUserGetter = spawnCurrentUserLogin(orchestrator);

dispatch(orchestrator, { action: MAIN_LOOP_COMMAND });
