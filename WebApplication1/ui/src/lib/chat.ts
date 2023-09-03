import { HubConnectionBuilder } from '@microsoft/signalr';
import { get } from 'svelte/store';
import { appStore } from './store';

const connection = new HubConnectionBuilder().withUrl('http://localhost:5193/chat').build();

connection.on('chatMessageReceived', (id: string, from: string, content: string) => {
	console.log(`Received message: ${id} ${from} ${content}`);
	appStore.update((store) => {
		store.chat.messages.push({
			id,
			author: from,
			content
		});
		return store;
	});
});

export async function sendMessage(content: string) {
	const userId = get(appStore).user?.id;
	await connection.send('newMessage', userId, content);
}

export async function initChat() {
	await connection.start();
}
