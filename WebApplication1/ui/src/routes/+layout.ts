import { appStore } from '$lib/store';
import { redirect } from '@sveltejs/kit';
import { get } from 'svelte/store';
import type { LayoutLoad } from './$types';

export const load: LayoutLoad = async ({ route }) => {
	if (route.id === '/login') {
		console.log('login');
		return;
	}

	if (!get(appStore).user?.id) {
		console.log('user not found');
		throw redirect(303, '/login');
	}
};
