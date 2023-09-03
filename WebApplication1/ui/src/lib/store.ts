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
};

export const appStore = writable<AppStore>({
	user: null,
	notes: [],
	chat: {
		messages: []
	}
});
