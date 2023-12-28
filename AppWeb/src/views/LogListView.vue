<script setup lang="ts">

import { GlobalInfo } from "@/GlobalInfo";
import Loading from "@/components/Loading.vue";

import { ref, onMounted } from "vue"
const isBusy = ref(false);
const datas = ref(<any[]>[]);
const HitWordFormatBegin = "<font color='#209aed'><b>";
const HitWordFormatEnd = "</b></font>";
function merge(objects: any[]) {
    // 合并范围重叠的对象
    const mergedObjects = [];
    let currentObject = objects[0];

    for (let i = 1; i < objects.length; i++) {
        const nextObject = objects[i];

        if (nextObject.index <= currentObject.index + currentObject.length) {
            // 范围重叠，更新当前对象的长度
            currentObject.length = Math.max(
                currentObject.index + currentObject.length,
                nextObject.index + nextObject.length
            ) - currentObject.index;
        } else {
            // 范围不重叠，将当前对象添加到合并后的数组中
            mergedObjects.push(currentObject);
            currentObject = nextObject;
        }
    }

    // 将最后一个对象添加到合并后的数组中
    mergedObjects.push(currentObject);
    return mergedObjects;
}

function formatContent(words: string[], content: string): string {
    var lowContent = content.toLowerCase();
    var findlist = <any[]>[];

    words.forEach(word => {
        word = word.toLowerCase();
        var startIndex = 0;
        while (true) {
            var index = lowContent.indexOf(word, startIndex);
            if (index >= 0) {
                findlist.push({
                    index: index,
                    length: word.length
                });
                startIndex = index + word.length;
            }
            else {
                break;
            }
        }
    });

    if (findlist.length > 0) {
        //整合交集
        console.log("原始：", findlist);
        findlist.sort((a, b) => a.index - b.index);

        findlist = merge(findlist);
        console.log("整合：", findlist);

        var ret = "";
        var endContentIndex = 0;
        findlist.forEach(obj => {
            if( obj.index > endContentIndex )
            {
                ret += content.substring(endContentIndex, obj.index);
            }

            ret += HitWordFormatBegin;
            ret += content.substring(obj.index, obj.index + obj.length);
            ret += HitWordFormatEnd;
            endContentIndex = obj.index + obj.length;
        });

        if (endContentIndex >= 0 && endContentIndex < content.length) {
            ret += content.substring(endContentIndex);
        }
        return ret;
    }

    return content;

}

onMounted(() => {
    init();
});

const init = async () => {
    if (GlobalInfo.UserInfo.Token) {
        refreshDatas();
    }
    else {
        window.setTimeout(() => init(), 100);
    }

}

const refreshDatas = async () => {
    if (isBusy.value)
        return;

    isBusy.value = true;
    try {


    } catch (error) {
        GlobalInfo.showError(error);
    }
    finally {
        isBusy.value = false;
    }
}


const showTime = (timestamp: any) => {
    var date = new Date(timestamp);
    return date.toLocaleString();
}

const showLevel = (level: any) => {
    switch (level) {
        case 0:
            return "Trace";
        case 1:
            return "Debug";
        case 2:
            return "Information";
        case 3:
            return "Warning";
        case 4:
            return "Error";
        case 5:
            return "Critical";
    }
    return "";
}

const traceIdClick = (traceId: any) => {
    if (GlobalInfo.PublicInfo.SelectedTraceIds.some(x => x == traceId) == false) {
        GlobalInfo.PublicInfo.SelectedTraceIds.push(traceId);
        searchClick();
    }
}

const searchClick = async () => {
    if (GlobalInfo.PublicInfo.SelectedAppContexts.length == 0) {
        GlobalInfo.showError("请先添加AppContext");
        return;
    }

    GlobalInfo.PublicInfo.SelectedAppContexts.forEach(x => {
        x.startTimestamp = undefined;
    });

    isBusy.value = true;
    try {
        datas.value.splice(0, datas.value.length);

        var startTimeStamp;
        var endTimeStamp = undefined;
        if (!GlobalInfo.PublicInfo.StartTime) {
            startTimeStamp = Date.now() - 10 * 60000;
        }
        else {
            startTimeStamp = Date.parse(GlobalInfo.PublicInfo.StartTime);
        }

        if (GlobalInfo.PublicInfo.EndTime) {
            endTimeStamp = Date.parse(GlobalInfo.PublicInfo.EndTime);
        }

        var list = [];
        for (var i = 0; i < GlobalInfo.PublicInfo.SelectedAppContexts.length; i++) {
            var param = {
                AppContext: GlobalInfo.PublicInfo.SelectedAppContexts[i].name,
                Sources: GlobalInfo.PublicInfo.SelectedAppContexts[i].SelectedSourceContexts,
                Start: startTimeStamp,
                Levels: GlobalInfo.PublicInfo.SelectedLevels.map(x => x.value),
                End: endTimeStamp,
                KeyWords: GlobalInfo.PublicInfo.SearchKeys,
                TraceIds: GlobalInfo.PublicInfo.SelectedTraceIds
            };

            var ret = JSON.parse(await GlobalInfo.postJson("/Log/ReadLogs", param));
            if (GlobalInfo.PublicInfo.SelectedAppContexts.length > 1) {
                ret.forEach((item: any) => {
                    item.appContext = param.AppContext;
                });
            }
            if (ret.length) {
                GlobalInfo.PublicInfo.SelectedAppContexts[i].startTimestamp = ret[ret.length - 1].timestamp;
                var words = ret[0].searchWords;
                if (words && words.length) {
                    ret.forEach((item: any) => {
                        item.content = formatContent(words, item.content);
                    });
                }
            }
            list.push(...ret);
        }
        list.sort((a, b) => {
            return a.timestamp - b.timestamp;
        });
        datas.value.push(...list);

    } catch (error) {
        GlobalInfo.showError(error);
    }
    finally {
        isBusy.value = false;
    }
}

</script>

<template>
    <div class="pageContent">
        <Loading v-if="isBusy" class="loadingV3" />
        <!-- Start Page Header -->
        <div class="page-header">
            <h1 class="title">&nbsp;</h1>

            <!-- Start Page Header Right Div -->
            <div class="right">
                <div class="btn-group" role="group" aria-label="...">

                    <a class="btn btn-light" @click="searchClick">
                        <i class="fa fa-search"></i></a>
                </div>
            </div>
            <!-- End Page Header Right Div -->

        </div>
        <!-- End Page Header -->

        <!-- Start Row -->
        <div class="row">

            <!-- Start Panel -->
            <div class="col-md-12">
                <div class="panel panel-default">

                    <!-- <div class="panel-title">
                        DAtaTables
                    </div> -->
                    <div class="panel-body dataTables_wrapper">

                        <table class="table display">
                            <tbody>
                                <tr v-for="item, index in datas">
                                    <td :class="{ noborder: index == 0, err: item.level == 4 }">
                                        <div class="info">
                                            <div class="item">{{ showTime(item.timestamp) }}</div>
                                            <div class="item app" v-if="item.appContext">{{ item.appContext }}</div>
                                            <div class="item">{{ item.sourceContext }}</div>
                                            <div class="item">[{{ showLevel(item.level) }}]</div>
                                        </div>
                                        <div class="traceid" @click="traceIdClick(item.traceId)" v-if="item.traceId">
                                            TraceId: {{ item.traceId }}
                                        </div>
                                        <div v-html="item.content"></div>
                                    </td>
                                </tr>

                            </tbody>
                        </table>

                    </div>

                </div>
            </div>
            <!-- End Panel -->
        </div>
        <!-- End Row -->

    </div>
</template>

<style scoped>
.noborder {
    border-top: none;
}

.info {
    display: flex;
    flex-direction: row;
}

table .err {
    color: #ef4836;
}

.info .item {
    /* background-color: #f5f3f3; */
    border-radius: 3px;
    /* padding: 0 6px; */
    font-size: 12px;
    color: #aaa;
    margin-right: 3px;
}

.info .app {
    color: rgb(21, 127, 92);
}

.traceid {
    color: rgb(21, 127, 92);
    text-decoration: underline;
    cursor: pointer;
}

.table td {
    padding: 9px 15px;
}
</style>
