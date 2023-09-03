<script lang="ts">
	import { browser } from '$app/environment';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { GET } from '$lib/api';
	import { initChat } from '$lib/chat';
	import Button from '$lib/components/button.svelte';
	import Toast from '$lib/components/toast/toast.svelte';
	import { appStore } from '$lib/store';
	import { onMount, tick } from 'svelte';
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
		if (
			browser &&
			mounted &&
			$appStore.user?.id &&
			!wsConnActive &&
			$appStore.env.loadedFromServer
		) {
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

	$: isNotLoggingPage = $page.url.pathname !== '/login';
	$: isUserAuth = !!$appStore.user?.id;
	$: console.log($page.url.pathname);

	onMount(async () => {
		try {
			const { serverUrl, chatEndpoint } = await (await fetch('/.env.json')).json();
			$appStore.env = {
				chatEndpoint,
				serverUrl,
				loadedFromServer: true
			};
			await tick();
		} catch (e) {
			console.error(e);
		}

		mounted = true;
	});
</script>

<div class="min-h-screen bg-slate-900 text-slate-100 flex flex-col">
	{#if isNotLoggingPage && isUserAuth}
		<header class="p-6 flex justify-between">
			<a class="px-2 py-1 hover:bg-slate-800 rounded-md active:scale-90" href="/"
				>Sveltekit + dotnet core playground</a
			>
			{#if $appStore.user}
				<div class="flex gap-2 items-center">
					{$appStore.user.email}
					<Button on:click={logout}>Logout</Button>
				</div>
			{/if}
		</header>
	{/if}
	{#if (isNotLoggingPage && isUserAuth) || !isNotLoggingPage}
		<slot />
	{/if}
</div>
<Toast />

<style lang="postcss">
	header {
		box-shadow: 1px 1px 4px theme(colors.slate.950);
	}
</style>
