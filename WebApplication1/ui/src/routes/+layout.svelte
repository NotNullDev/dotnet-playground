<script lang="ts">
	import { GET } from '$lib/api';
	import Button from '$lib/components/button.svelte';
	import { appStore } from '$lib/store';
	import '../app.css';

	async function logout() {
		const { data, error } = await GET('/logout', {});

		if (error) {
			console.error(error);
			return;
		}
		$appStore.user = null;
	}
</script>

<div class="min-h-screen bg-slate-900 text-slate-100 flex flex-col">
	<header class="p-6 flex justify-between">
		<a class="px-2 py-1 hover:bg-slate-800 rounded-md active:scale-90" href="/">Notes app</a>
		{#if $appStore.user}
			<Button on:click={logout}>Logout</Button>
		{/if}
	</header>
	<slot />
</div>

<style lang="postcss">
	header {
		box-shadow: 1px 1px 4px theme(colors.slate.950);
	}
</style>
