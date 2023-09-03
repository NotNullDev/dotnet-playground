<script lang="ts">
	import { goto } from '$app/navigation';
	import { POST } from '$lib/api';
	import Button from '$lib/components/button.svelte';
	import Input from '$lib/components/input.svelte';
	import { showToast } from '$lib/components/toast/toast-store';
	import { appStore } from '$lib/store';

	let email = '';
	let password = '';
	let repeatPassword = '';

	async function register() {
		const { data, error } = await POST('/register', {
			body: {
				email,
				password
			}
		});
		if (error) {
			console.log(error);
			showToast('Registration failed', 'Unknown error');
			return;
		}
		if (data) {
			$appStore.user = {
				email: data.id ?? '',
				id: data.id ?? ''
			};
		}
		goto('/');
	}

	async function login() {
		const { error, data } = await POST('/login', {
			body: {
				email,
				password
			}
		});
		if (error) {
			console.log(error);
			showToast('Login failed', 'Unknown error');
			return;
		}
		if (data) {
			$appStore.user = {
				email: data.id ?? '',
				id: data.id ?? ''
			};
		}
		goto('/');
	}

	let state: 'login' | 'register' = 'login';
</script>

<div class="flex flex-1 h-full justify-center mt-10">
	<div class="flex flex-col">
		<form
			on:submit|preventDefault
			class="px-6 pb-6 pt-2 rounded-md shadow shadow-slate-950 flex flex-col gap-2 bg"
		>
			<div class="flex">
				<Button
					classes={{ root: 'w-full rounded-r-none border-r border-r-slate-950' }}
					on:click={() => {
						state = 'login';
					}}>Login</Button
				>
				<Button
					classes={{ root: 'w-full rounded-l-none' }}
					on:click={() => {
						state = 'register';
					}}>Register</Button
				>
			</div>

			{#if state === 'login'}
				<h2 class="my-3 text-3xl">Login</h2>
				<Input
					name="email"
					bind:value={email}
					type="email"
					placeholder="Username"
					label="Username"
				/>
				<Input
					name="password"
					bind:value={password}
					type="password"
					placeholder="Password"
					label="Password"
				/>
				<Button on:click={login}>Login</Button>
			{/if}
			{#if state === 'register'}
				<h2 class="my-3 text-3xl">Register</h2>
				<Input
					name="email"
					type="email"
					bind:value={email}
					required
					placeholder="Username"
					label="Username"
				/>
				<Input
					name="password"
					type="password"
					required
					bind:value={password}
					placeholder="Password"
					label="Password"
				/>
				<Input
					name="repeat-password"
					required
					type="password"
					bind:value={repeatPassword}
					placeholder="Repeat password"
					label="Password"
				/>
				<Button on:click={register}>Create account</Button>
			{/if}
		</form>
	</div>
</div>
