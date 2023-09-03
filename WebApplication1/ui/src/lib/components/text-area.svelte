<script lang="ts">
	import { tw } from '$lib/utils';

	export let label = '';
	export let placeholder = '';
	export let value = '';
	export let classes: {
		root?: string;
		span?: string;
		textarea?: string;
	} = {};
</script>

<label {...$$restProps} class={tw('flex flex-col gap-[1px]', classes.root)}>
	<span class={tw(classes.span)}>{label}</span>
	<textarea
		{placeholder}
		rows="1"
		class={tw('bg-slate-800 rounded-md px-2 py-1', classes.textarea)}
		bind:value
	/>
</label>
