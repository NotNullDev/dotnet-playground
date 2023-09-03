import { goto } from '$app/navigation';
import { GET } from '$lib/api';
import { appStore } from '$lib/store';
import { tick } from 'svelte';
import type { PageLoad } from './$types';

export const load: PageLoad = async () => {
	const { data } = await GET('/me', {});

	console.log('after get');

	if (data?.id) {
		appStore.update((store) => {
			store.user = {
				id: data.id ?? '',
				email: data.email ?? ''
			};
			return store;
		});
		await tick();
		console.log('user is already logged in');
		goto('/');
	}
};
