export const prerender = true;

import { browser } from '$app/environment';
import { tryAuthSilent } from '$lib/api';
import { appStore } from '$lib/store';
import { redirect } from '@sveltejs/kit';
import { get } from 'svelte/store';
import type { LayoutLoad } from './$types';

export const load: LayoutLoad = async ({ route }) => {
	if (!browser) {
		return;
	}

	let userLoggedIn = !!get(appStore).user?.id;

	if (!userLoggedIn) {
		userLoggedIn = await tryAuthSilent();
	}

	if (route.id === '/login') {
		if (userLoggedIn) {
			throw redirect(300, '/');
		}
	}

	if (route.id !== '/login') {
		if (!userLoggedIn) {
			throw redirect(300, '/login');
		}
	}
};
