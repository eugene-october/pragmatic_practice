import { dispatch, spawnStateless } from 'nact';

export const spawnCurrentUserGetter = (parent) => spawnStateless(parent, async (msg, ctx) => {
    const sender = ctx.self;
    const { accessToken } = msg;

    if (!accessToken) {
        const error = new Error("accessToken can't be null");

        return dispatch(parent, { error });
    }

    try {
        const response = await fetch('https://dummyjson.com/user/me', {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${accessToken}`,
            },
            credentials: 'include'
        });
        const res = await response.json();

        const payload = {
            id: res.id,
            username: res.username,
            email: res.email,
            firstName: res.firstName,
            lastName: res.lastName,
        };

        dispatch(parent, { payload, sender });
    } catch (error) {
        dispatch(parent, { error })
    }
}, 'currentUserGetter');
