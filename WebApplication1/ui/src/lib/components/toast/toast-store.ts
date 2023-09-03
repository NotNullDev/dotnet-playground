import { tick } from 'svelte';
import { writable } from 'svelte/store';

type ToastStoreType = {
	title: string | null;
	content: string | null;
	visible: boolean;
};

function hideToast() {
	toastStore.update((state) => {
		state.visible = false;
		return state;
	});
}

let toastTimeout: any;

export async function showToast(title: string, description: string) {
	toastStore.update((store) => {
		store.visible = false;
		return store;
	});

	clearTimeout(toastTimeout);

	await tick();

	toastStore.update((store) => {
		store.title = title;
		store.content = description;
		store.visible = true;
		return store;
	});

	toastTimeout = setTimeout(hideToast, 3000);
}

export const toastStore = writable({
	title: 'Toast title',
	content: 'Toast content',
	visible: false
});
