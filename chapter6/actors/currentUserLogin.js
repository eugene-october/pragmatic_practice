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
            accessToken: res.accessToken,
        };
        const sender = ctx.self;

        dispatch(msg.sender, { payload, sender });
    } catch (error) {
        dispatch(msg.sender, { error });
    }
}, 'currentUserLogin');

