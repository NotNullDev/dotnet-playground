<script>
	import { fade, fly } from 'svelte/transition';
	import { toastStore } from './toast-store';
</script>

{#if $toastStore.visible}
	<div
		class="absolute bottom-0 right-0 m-12 w-[300px] rounded-md text-white flex flex-col gap-2 p-3 bg-primary overflow-hidden"
		in:fly={{ y: -50 }}
		out:fade
	>
		<h2 class="text-xl">{$toastStore.title}</h2>
		<pre class="whitespace-break-spaces">{$toastStore.content}</pre>
	</div>
{/if}
