import createClient from 'openapi-fetch';
import { get } from 'svelte/store';
import type { paths } from '../schema';
import { appStore } from './store';

export let { GET, POST, DELETE } = createClient<paths>({
	baseUrl: get(appStore).env.serverUrl,
	credentials: 'include'
});

appStore.subscribe(async (store) => {
	if (store.env.loadedFromServer) {
		appStore.subscribe((store) => {
			if (store.env.loadedFromServer) {
				({ GET, POST, DELETE } = createClient<paths>({
					baseUrl: store.env.serverUrl,
					credentials: 'include'
				}));
				tryAuthSilent();
			}
		});
	}
});

export async function tryAuthSilent(): Promise<boolean> {
	let ok = false;
	try {
		const { data } = await GET('/me', {});

		if (data?.id) {
			appStore.update((store) => {
				store.user = {
					id: data.id ?? '',
					email: data.email ?? ''
				};
				return store;
			});
			ok = true;
		}
	} catch (e) {
		console.error(e);
	}
	return ok;
}
