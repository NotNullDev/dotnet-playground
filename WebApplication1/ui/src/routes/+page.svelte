<script lang="ts">
	import { goto } from '$app/navigation';
	import { navigating } from '$app/stores';
	import { DELETE, GET, POST } from '$lib/api';
	import { sendMessage } from '$lib/chat';
	import Button from '$lib/components/button.svelte';
	import Input from '$lib/components/input.svelte';
	import TextArea from '$lib/components/text-area.svelte';
	import { showToast } from '$lib/components/toast/toast-store';
	import { appStore } from '$lib/store';
	import { onMount } from 'svelte';
	import type { components } from '../schema';

	let notes: components['schemas']['Note'][] = [];

	onMount(async () => {
		const { data, error, response } = await GET('/notes/', {});
		if (data) {
			notes = data;
		}

		if (response.status === 401 && $navigating?.complete) {
			await goto('/login');
			return;
		}

		refetchData();
	});

	let newNoteData = {
		title: '',
		content: ''
	};

	async function refetchData() {
		const { data, error } = await GET('/notes/', {});
		if (data) {
			notes = data;
		}
		if (error) {
			showToast('Failed to refetch notes.', 'Unknown error.');
		}
	}

	async function createNote() {
		const { error } = await POST('/notes/', {
			body: {
				title: newNoteData.title,
				content: newNoteData.content
			}
		});

		if (error) {
			showToast('Note creation failed', error.map((e) => e.errorMessage).join('\n'));
			return;
		}

		newNoteData = {
			title: '',
			content: ''
		};
		refetchData();
	}

	async function deleteNote(noteId?: number) {
		if (!noteId) {
			console.warn('noteId is null!');
			return;
		}
		const { error } = await DELETE('/notes/{id}', { params: { path: { id: noteId } } });

		if (error) {
			showToast('Note deletion failed.', 'Unknown error');
		}

		refetchData();
	}

	let chatMessage = {
		message: ''
	};

	async function send() {
		try {
			await sendMessage(chatMessage.message);
			chatMessage.message = '';
		} catch (e) {
			showToast('Failed to send chat message', 'Unknown error');
		}
	}
</script>

<div class="flex justify-around p-4 flex-col gap-12 flex-1">
	<div class="flex gap-12 items-center justify-center">
		<form
			on:submit|preventDefault={createNote}
			class="flex flex-col rounded-md p-6 gap-2 bg px-12 h-min items-center"
		>
			<h1 class="text-2xl mb-3">New note</h1>
			<Input
				label="Title"
				placeholder="Title"
				classes={{
					root: 'max-w-[200px]'
				}}
				bind:value={newNoteData.title}
			/>
			<TextArea
				label="Description"
				placeholder="Description"
				classes={{
					root: 'max-w-[200px]'
				}}
				bind:value={newNoteData.content}
			/>
			<Button
				classes={{
					root: 'max-w-[200px] mt-1'
				}}>Create</Button
			>
		</form>
	</div>

	<div class="flex gap-12 flex-1 h-full justify-around">
		<div class="flex flex-col bg px-12 py-3 overflow-hidden h-[50vh] w-[40vw]">
			<h2 class="text-2xl my-4">Notes</h2>
			<div class="flex flex-col-reverse gap-4 w-full overflow-y-auto py-2">
				{#each notes.reverse() as n}
					<div class="flex gap-2 items-center bg p-2 mx-4">
						<div class="flex-1 p-2 gap-2 flex flex-col w-full">
							<h3 class="w-full text-3xl">{n.title}</h3>
							<pre class="w-full">{n.content}</pre>
						</div>
						<Button on:click={() => deleteNote(n.id)}>Delete</Button>
					</div>
				{/each}
			</div>
		</div>

		<div class="bg p-4 flex flex-col h-[50vh] w-[40vw]">
			<h2 class="text-3xl">Chat</h2>
			<div class="flex flex-col-reverse flex-1 overflow-y-auto p-4">
				{#each $appStore.chat.messages.reverse() as msg}
					<div class="bg w-full p-4">
						<div class="text-2xl">{msg.author}</div>
						<pre>{msg.content}</pre>
					</div>
				{/each}
			</div>
			<form on:submit|preventDefault={send} class="flex justify-between items-end gap-3">
				<TextArea bind:value={chatMessage.message} classes={{ root: 'flex-1' }} />
				<Button classes={{ root: 'w-[60px] h-min' }} type="submit">Send</Button>
			</form>
		</div>
	</div>
</div>
