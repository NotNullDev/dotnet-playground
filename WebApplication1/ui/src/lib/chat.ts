import { HubConnectionBuilder, type HubConnection } from '@microsoft/signalr';
import { get } from 'svelte/store';
import { appStore } from './store';

let connection: HubConnection | null = null;

export async function sendMessage(content: string) {
	const userId = get(appStore).user?.id;
	await connection?.send('newMessage', userId, content);
}

export async function initChat() {
	connection = new HubConnectionBuilder()
		.withUrl(`${get(appStore).env.serverUrl}/${get(appStore).env.chatEndpoint}`)
		.build();

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

	await connection.start();
}
