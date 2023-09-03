import { PUBLIC_CHAT_ENDPOINT, PUBLIC_SERVER_URL } from '$env/static/public';
import { writable } from 'svelte/store';

export type AppStore = {
	user: {
		id: string;
		email: string;
	} | null;
	notes: {
		title: string;
		content: string;
		done: boolean;
	}[];
	chat: {
		messages: {
			id: string;
			author: string;
			content: string;
		}[];
	};
	env: {
		serverUrl: string;
		chatEndpoint: string;
		loadedFromServer: boolean;
	};
};

export const appStore = writable<AppStore>({
	user: null,
	notes: [],
	chat: {
		messages: []
	},
	env: {
		chatEndpoint: PUBLIC_CHAT_ENDPOINT,
		serverUrl: PUBLIC_SERVER_URL,
		loadedFromServer: false
	}
});
