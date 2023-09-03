<script lang="ts">
	import { browser } from '$app/environment';
	import { goto } from '$app/navigation';
	import { GET } from '$lib/api';
	import { initChat } from '$lib/chat';
	import Button from '$lib/components/button.svelte';
	import Toast from '$lib/components/toast/toast.svelte';
	import { appStore } from '$lib/store';
	import { onMount } from 'svelte';
	import '../app.css';

	let wsConnActive = false;
	let mounted = false;

	async function logout() {
		const { data, error } = await GET('/logout', {});

		if (error) {
			console.error(error);
			return;
		}
		$appStore.user = null;
		goto('/login');
	}

	$: {
		if (browser && mounted && $appStore.user?.id && !wsConnActive) {
			(async () => {
				try {
					initChat();
					wsConnActive = true;
				} catch (e) {
					console.error(e);
				}
			})();
		}
	}

	onMount(() => {
		mounted = true;
	});
</script>

<div class="min-h-screen bg-slate-900 text-slate-100 flex flex-col overflow-hidden h-full">
	<header class="p-6 flex justify-between">
		<a class="px-2 py-1 hover:bg-slate-800 rounded-md active:scale-90" href="/">Notes app</a>
		{#if $appStore.user}
			<Button on:click={logout}>Logout</Button>
		{/if}
	</header>
	<slot />
	<Toast />
</div>

<style lang="postcss">
	header {
		box-shadow: 1px 1px 4px theme(colors.slate.950);
	}
</style>
