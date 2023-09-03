import createClient from 'openapi-fetch';
import { get } from 'svelte/store';
import type { paths } from '../schema';
import { appStore } from './store';

export let { GET, POST, DELETE } = createClient<paths>({
	baseUrl: get(appStore).env.serverUrl,
	credentials: 'include'
});

appStore.subscribe((store) => {
	if (store.env.loadedFromServer) {
		appStore.subscribe((store) => {
			if (store.env.loadedFromServer) {
				({ GET, POST, DELETE } = createClient<paths>({
					baseUrl: store.env.serverUrl,
					credentials: 'include'
				}));
			}
		});
	}
});
