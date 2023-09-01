<script lang="ts">
	import { tw } from '$lib/utils';

	export let classes: {
		root?: string;
	} = {};
</script>

<button
	class={tw(
		'px-2 py-1 rounded-md bg-indigo-800 active:scale-95 hover:brightness-105 transition-transform',
		classes.root
	)}
	on:click
>
	<slot />
</button>
