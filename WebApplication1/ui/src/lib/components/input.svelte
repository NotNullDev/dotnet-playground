<script lang="ts">
	import { tw } from '$lib/utils';
	export let value = '';
	export let label = '';
	export let placeholder = '';
	export let classes: {
		root?: string;
		span?: string;
		input?: string;
	} = {};
</script>

<label class={tw('flex gap-[1px] flex-col', classes.root)}>
	<span class={tw(classes.span)}>{label}</span>
	<input
		class={tw('rounded-md bg-slate-800 px-2 py-1', classes.input)}
		type="text"
		{placeholder}
		bind:value
		{...$$restProps}
	/>
</label>
