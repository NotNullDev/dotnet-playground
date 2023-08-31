<script lang="ts">
    import {onMount} from "svelte"
    import {GET} from "$lib/api";
    import type {components} from "../schema";
    
    let products: components['schemas']['Note'][] = []
    
    onMount(async () => {
        products = await (await GET("/products/", {})).response.json()
        console.log(products)
    })
</script>

<div class="flex flex-col gap-4 p-24">
    {#each products as p}
        <div>
            {p.id} {p.content}
        </div>
    {/each}
</div>