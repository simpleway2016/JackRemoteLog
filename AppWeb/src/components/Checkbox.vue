<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { v4 as uuidv4 } from 'uuid';

const props = defineProps(['modelValue', "text"])
const emit = defineEmits(['update:modelValue'])

const chkEle = ref(<any>null);
const generatedGuid = ref(uuidv4());
onMounted(() => {
    //在vue更改元素后，让select重新自动变更样子
    if (chkEle.value.parentElement) {
        chkEle.value.onchange = () => {
            emit("update:modelValue", chkEle.value.checked);
        }

        if (chkEle.value.checked != props.modelValue) {
            chkEle.value.checked = props.modelValue;
        }
    }

});
</script>

<template>
    <div class="checkbox">
        <input ref="chkEle" :id="`chk${generatedGuid}`" type="checkbox">
        <label :for="`chk${generatedGuid}`">
            {{ text }}
        </label>
    </div>
</template>

