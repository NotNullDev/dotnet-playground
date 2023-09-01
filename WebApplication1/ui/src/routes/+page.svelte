<script lang="ts">
	import { DELETE, GET, POST } from '$lib/api';
	import Button from '$lib/components/button.svelte';
	import Input from '$lib/components/input.svelte';
	import TextArea from '$lib/components/text-area.svelte';
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
	}

	async function createNote() {
		await POST('/notes/', {
			body: {
				title: newNoteData.title,
				content: newNoteData.content
			}
		});
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
		await DELETE('/notes/{id}', { params: { path: { id: noteId } } });
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
