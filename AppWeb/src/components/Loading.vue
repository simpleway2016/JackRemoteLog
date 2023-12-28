<script setup lang="ts">
import { onMounted,onUnmounted,ref } from 'vue';
const props = defineProps(["class"]);
const show = ref(false);
var timer =0;
onMounted(()=>{
    timer = window.setTimeout(()=>{
        timer = 0;
        show.value = true;
    } , 500);
});

onUnmounted(()=>{
    window.clearTimeout(timer);
    timer = 0;
});
</script>

<template>
    <div v-show="show" :class="class" style="pointer-events:none;"><img src="/img/loading.gif" alt="loading-img"></div>
</template>