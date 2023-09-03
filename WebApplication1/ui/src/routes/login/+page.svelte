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
	let validationErrors: string[] = [];

	async function register() {
		if (password !== repeatPassword) {
			validationErrors = [`Passwords don't match.`];
			return;
		}
		validationErrors = [];

		const { data, error } = await POST('/register', {
			body: {
				email,
				password
			}
		});
		if (error) {
			if (error.creationErrors) {
				validationErrors = error.creationErrors;
			}
			if (error.validationErrors) {
				validationErrors = error.validationErrors;
			}

			showToast('Registration failed', 'Unknown error');
			return;
		}
		if (data) {
			$appStore.user = {
				email: data.email ?? '',
				id: data.id ?? ''
			};
		}
		goto('/');
	}

	async function login() {
		validationErrors = [];
		const { error, data } = await POST('/login', {
			body: {
				email,
				password
			}
		});
		if (error) {
			if (error.error) {
				validationErrors = [error.error];
			}
			if (error.validationError) {
				validationErrors = error.validationError;
			}
			showToast('Login failed', 'Unknown error');
			return;
		}
		if (data) {
			$appStore.user = {
				email: data.email ?? '',
				id: data.id ?? ''
			};
		}
		goto('/');
	}

	let state: 'login' | 'register' = 'login';
</script>

<div class="flex flex-1 h-full justify-center items-center">
	<div class="flex flex-col">
		<form
			on:submit|preventDefault={() => {
				validationErrors = [];
				if (state === 'login') login();
				if (state === 'register') register();
			}}
			class="px-6 pb-6 pt-2 rounded-md shadow shadow-slate-950 flex flex-col gap-2 bg"
		>
			<div class="flex">
				<Button
					classes={{ root: 'w-full rounded-r-none border-r border-r-slate-950' }}
					type="button"
					on:click={() => {
						state = 'login';
						validationErrors = [];
					}}>Login</Button
				>
				<Button
					classes={{ root: 'w-full rounded-l-none' }}
					type="button"
					on:click={() => {
						state = 'register';
						validationErrors = [];
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
					min={1}
				/>
				<Input
					name="password"
					bind:value={password}
					type="password"
					min={1}
					placeholder="Password"
					label="Password"
				/>
				<pre class="text-red-500">{validationErrors.join('\n')}</pre>

				<Button type="submit">Login</Button>
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
				<pre class="text-red-500">{validationErrors.join('\n')}</pre>
				<Input
					name="repeat-password"
					required
					type="password"
					bind:value={repeatPassword}
					placeholder="Repeat password"
					label="Repeat password"
				/>
				<Button type="submit">Create account</Button>
			{/if}
		</form>
	</div>
</div>
