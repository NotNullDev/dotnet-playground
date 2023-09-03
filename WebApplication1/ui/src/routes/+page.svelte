<script lang="ts">
	import { DELETE, GET, POST } from '$lib/api';
	import Button from '$lib/components/button.svelte';
	import Input from '$lib/components/input.svelte';
	import TextArea from '$lib/components/text-area.svelte';
	import { showToast } from '$lib/components/toast/toast-store';
	import { onMount } from 'svelte';
	import type { components } from '../schema';

	let notes: components['schemas']['Note'][] = [];

	onMount(async () => {
		const { data, error } = await GET('/notes/', {});
		if (data) {
			notes = data;
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
</script>

<div class="flex justify-around">
	<div class="flex flex-col rounded-md p-4 gap-1">
		<h1>New note</h1>
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
				root: 'max-w-[200px]'
			}}
			on:click={createNote}>Create</Button
		>
	</div>

	<div class="flex flex-col-reverse gap-4 p-24">
		{#each notes as n}
			<div class="flex gap-2 items-center">
				<div>
					{n.id}
					{n.content}
					{n.title}
				</div>
				<Button on:click={() => deleteNote(n.id)}>Delete</Button>
			</div>
		{/each}
	</div>
</div>
