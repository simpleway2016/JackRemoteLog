<script setup lang="ts">
import { onMounted, onUnmounted, ref,watch,toRef } from 'vue';
const props = defineProps(["value","underValue"]);

const toRefvalue = toRef(props, "value");

onMounted(()=>{
    var percent = parseFloat(props.value);
    var l = 2*Math.PI*20*percent/100.0;
    dasharray.value = `${l} 300000`;
    console.log("onMounted" , l);
});

const dasharray = ref("");
watch(toRefvalue , (newValue:any,oldValue)=>{
    var percent = parseFloat(newValue);
    var l = 2*Math.PI*20*percent/100.0;
    dasharray.value = `${l} 300000`;
    console.log("watch" , l);
});
</script>

<template>
    <div style="position: relative;">
        <svg>
            <circle cx="25" cy="25" r="20" fill="none" stroke="#eee" stroke-width="6"></circle>
        </svg>
        <svg class="s2" style="position: absolute;left: 0;top: 0;">
            <circle v-if="value>=100" cx="25" cy="25" r="20" fill="none" stroke="green" stroke-width="6"></circle>
            <circle v-else cx="25" cy="25" r="20" fill="none" stroke="green" stroke-width="6" :stroke-dasharray="dasharray"></circle>
        </svg>
        <div v-if="underValue" :title="underValue" class="value" style="text-decoration: underline;">
            {{ value < 1 ? value : parseInt(value) }}%
        </div>
        <div v-else class="value">
            {{ value < 1 ? value : parseInt(value) }}%
        </div>
    </div>
</template>
<style scoped>
div,svg {
    width: 50px;
    height: 50px;
}

.s2{
    transform: rotate(-90deg);
}
.value{
    font-size: 12px;
    position: absolute;
    left: 0;
    top: 0;
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-family: 'Montserrat', sans-serif;
}
</style>