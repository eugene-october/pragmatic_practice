import { dispatch, spawnStateless } from 'nact';

export const spawnCurrentUserLogin = (parent) => spawnStateless(parent, async (msg, ctx) => {
    try {
        const response = await fetch('https://dummyjson.com/user/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                username: 'emilys',
                password: 'emilyspass',
            }),
        });
        const res = await response.json();

        const payload = {
            id: res.id,
            username: res.username,
            email: res.email,
            firstName: res.firstName,
            lastName: res.lastName,
            accessToken: res.accessToken,
        };
        const sender = ctx.self;

        dispatch(msg.sender, { payload, sender });
    } catch (e) {
        console.log(`--USER_LOGIN_ERROR-e---${e}`);
    }
}, 'ping');

