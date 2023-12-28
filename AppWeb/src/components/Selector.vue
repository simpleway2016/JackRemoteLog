<script setup lang="ts">
import { ref, onMounted } from 'vue';

const props = defineProps(['modelValue', 'datas', 'textMember', 'valueMember'])
const emit = defineEmits(['update:modelValue'])
const selectEle = ref(<HTMLElement><any>null);

onMounted(() => {
    console.log("selector onMounted");
    //在vue更改元素后，让select重新自动变更样子
    if (selectEle.value.parentElement) {
        var ret = ((<any>window).$)(selectEle.value).selectpicker();
        if (ret.length) {
            ret[0].onchange = () => {
                emit("update:modelValue", ret[0].value);
            }
            if (ret[0].value != props.modelValue) {
                //emit("update:modelValue" , ret[0].value);
            }
        }
    }

});
</script>

<template>
    <select ref="selectEle" class="selectpicker" :value="modelValue">
        <option></option>
        <option :value="valueMember ? op[valueMember] : op" v-for="op in datas">{{ textMember ? op[textMember] : op }}</option>
    </select>
</template>

