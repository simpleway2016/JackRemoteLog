<script setup lang="ts">

import { GlobalInfo } from "@/GlobalInfo";
import Loading from "@/components/Loading.vue";

import { ref, onMounted, watch, onUpdated } from "vue"
var autoLoadTimer = 0;
var updateAction: any;
const showAppContext = ref(false);
const isReady = ref(false);
const dataPickerEle = ref(<HTMLElement><any>null);
const rowAreaEle = ref(<HTMLElement><any>null);
const isBusy = ref(false);
const hasMore = ref(false);
const autoLoadMore = ref(false);
const datas = ref(<any[]>[]);
const timeRange = ref("");
const HitWordFormatBegin = "<font color='#209aed'><b>";
const HitWordFormatEnd = "</b></font>";

function isScrolledToBottom(element: HTMLElement) {
    var val = Math.abs(element.scrollHeight - element.scrollTop - element.clientHeight);
    return Math.abs(element.scrollHeight - element.scrollTop - element.clientHeight) < 5;
}

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
        //console.log("原始：", findlist);
        findlist.sort((a, b) => a.index - b.index);

        findlist = merge(findlist);
        //console.log("整合：", findlist);

        var ret = "";
        var endContentIndex = 0;
        findlist.forEach(obj => {
            if (obj.index > endContentIndex) {
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

onUpdated(() => {
    if (updateAction) {
        updateAction();
        updateAction = undefined;
    }

});

watch(autoLoadMore, (newValue) => {
    if (newValue) {
        checkIfAutoLoadMore();
    }
    else {
        if (autoLoadTimer) {
            window.clearTimeout(autoLoadTimer);
            autoLoadTimer = 0;
        }
    }
});

onMounted(() => {

    timeRange.value = (<any>window).moment().add(-10, 'minute').format("YYYY/MM/DD HH:mm:ss") + " - " +
        (<any>window).moment().startOf('day').add(1, 'month').format("YYYY/MM/DD HH:mm:ss");

    init();

    (<any>window).$(dataPickerEle.value).daterangepicker({
        timePicker: true,
        timePickerIncrement: 1,
        timePickerSeconds: true,
        timePicker12Hour: false,
        startDate: (<any>window).moment().add(-10, 'minute'),
        endDate: (<any>window).moment().startOf('day').add(1, 'month'),
        format: 'YYYY/MM/DD HH:mm:ss',
        locale: {
            applyLabel: '确定',
            cancelLabel: '取消',
            ///*customRangeLabel : '自定义'
            daysOfWeek: ['日', '一', '二', '三', '四', '五', '六'],
            monthNames: ['一月', '二月', '三月', '四月', '五月', '六月',
                '七月', '八月', '九月', '十月', '十一月', '十二月'],
            firstDay: 1
        }
    }, (start: any, end: any, label: any) => {
        //console.log(start.format("YYYY/MM/DD HH:mm:ss"), end.format("YYYY/MM/DD HH:mm:ss"), label);
    });
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

const checkIfAutoLoadMore = () => {
    if (autoLoadTimer) {
        window.clearTimeout(autoLoadTimer);
        autoLoadTimer = 0;
    }

    if (autoLoadMore.value) {
        autoLoadTimer = window.setTimeout(() => {
            showMore(true);
        }, hasMore.value ? 0 : 2000);
    }
}

const searchClick = async () => {
    if (isBusy.value)
        return;

    if (GlobalInfo.PublicInfo.SelectedAppContexts.length == 0) {
        GlobalInfo.showError("请先添加AppContext");
        return;
    }
    autoLoadMore.value = false;
    isReady.value = true;

    GlobalInfo.PublicInfo.SelectedAppContexts.forEach(x => {
        x.startTimestamp = undefined;
    });

    if (autoLoadTimer) {
        window.clearTimeout(autoLoadTimer);
        autoLoadTimer = 0;
    }

    isBusy.value = true;
    try {
        datas.value.splice(0, datas.value.length);

        var startTimeStamp;
        var endTimeStamp = undefined;
        var findex = timeRange.value.indexOf(" - ");
        if (findex < 0) {
            throw "时间格式不正确";
        }
        startTimeStamp = Date.parse((<any>window).moment(timeRange.value.substring(0, findex)).format("YYYY-MM-DD HH:mm:ss"));
        endTimeStamp = (<any>window).moment(timeRange.value.substring(findex + 3));

        if (endTimeStamp > new Date()) {
            endTimeStamp = undefined;
        }
        else {
            endTimeStamp = Date.parse(endTimeStamp.add(1, "second").format("YYYY-MM-DD HH:mm:ss"));
        }

        if (endTimeStamp != undefined && endTimeStamp <= startTimeStamp) {
            isBusy.value = false;
            return;
        }

        var list = [];
        var hasmoreFlag = false;
        showAppContext.value = GlobalInfo.PublicInfo.SelectedAppContexts.length > 1;
        for (var i = 0; i < GlobalInfo.PublicInfo.SelectedAppContexts.length; i++) {
            var app = GlobalInfo.PublicInfo.SelectedAppContexts[i];
            var param = {
                AppContext: app.name,
                Sources: app.SelectedSourceContexts,
                Start: startTimeStamp,
                Levels: GlobalInfo.PublicInfo.SelectedLevels.map(x => x.value),
                End: endTimeStamp,
                KeyWords: GlobalInfo.PublicInfo.SearchKeys,
                TraceIds: GlobalInfo.PublicInfo.SelectedTraceIds
            };

            var ret = JSON.parse(await GlobalInfo.postJson("/Log/ReadLogs", param));
            ret.forEach((item: any) => {
                item.appContext = param.AppContext;
            });

            if (ret.length) {
                if (ret.length >= ret[0].pageSize) {
                    hasmoreFlag = true;
                }
                app.startTimeStamp = ret[ret.length - 1].timestamp;
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

        hasMore.value = hasmoreFlag;
        datas.value.push(...list);
        checkIfAutoLoadMore();
    } catch (error) {
        GlobalInfo.showError(error);
    }
    finally {
        isBusy.value = false;
    }
}



const showMore = async (isAuto: boolean) => {
    if (GlobalInfo.PublicInfo.SelectedAppContexts.length == 0) {
        return;
    }

    autoLoadTimer = 0;
    if (isAuto == false) {
        isBusy.value = true;
    }
    try {

        var startTimeStamp;
        var endTimeStamp = undefined;
        var findex = timeRange.value.indexOf(" - ");
        if (findex < 0) {
            throw "时间格式不正确";
        }
        startTimeStamp = Date.parse((<any>window).moment(timeRange.value.substring(0, findex)).format("YYYY-MM-DD HH:mm:ss"));
        endTimeStamp = (<any>window).moment(timeRange.value.substring(findex + 3));

        if (endTimeStamp > new Date()) {
            endTimeStamp = undefined;
        }
        else {
            endTimeStamp = Date.parse(endTimeStamp.add(1, "second").format("YYYY-MM-DD HH:mm:ss"));
        }

        if (endTimeStamp != undefined && endTimeStamp <= startTimeStamp) {
            isBusy.value = false;

            autoLoadTimer = window.setTimeout(() => {
                showMore(true);
            }, 2000);

            return;
        }

        var list = <any[]>[];
        var hasmoreFlag = false;
        showAppContext.value = GlobalInfo.PublicInfo.SelectedAppContexts.length > 1;

        for (var i = 0; i < GlobalInfo.PublicInfo.SelectedAppContexts.length; i++) {
            var app = GlobalInfo.PublicInfo.SelectedAppContexts[i];
            var param = {
                AppContext: app.name,
                Sources: app.SelectedSourceContexts,
                Start: app.startTimeStamp ? app.startTimeStamp : startTimeStamp,
                Levels: GlobalInfo.PublicInfo.SelectedLevels.map(x => x.value),
                End: endTimeStamp,
                KeyWords: GlobalInfo.PublicInfo.SearchKeys,
                TraceIds: GlobalInfo.PublicInfo.SelectedTraceIds
            };

            var ret = JSON.parse(await GlobalInfo.postJson("/Log/ReadLogs", param));
            ret.forEach((item: any) => {
                item.appContext = param.AppContext;
            });

            if (ret.length) {
                if (ret.length >= ret[0].pageSize) {
                    hasmoreFlag = true;
                }

                app.startTimeStamp = ret[ret.length - 1].timestamp;
                var words = ret[0].searchWords;
                if (words && words.length) {
                    ret.forEach((item: any) => {
                        item.content = formatContent(words, item.content);
                    });
                }
            }

            ret.forEach((item: any) => {
                for (var i = datas.value.length - 1; i >= 0; i--) {
                    var existItem = datas.value[i];
                    if (existItem.appContext == item.appContext) {
                        if (existItem.timestamp == item.timestamp && existItem.content == item.content)
                            return;
                        else if (existItem.timestamp < item.timestamp)
                            break;
                    }
                }
                list.push(item);
            });


        }
        list.sort((a, b) => {
            return a.timestamp - b.timestamp;
        });


        hasMore.value = hasmoreFlag;

        if (list.length) {
            if (isScrolledToBottom(rowAreaEle.value)) {
                updateAction = () => {
                    rowAreaEle.value.scrollTo(0, rowAreaEle.value.scrollHeight);
                };
            }

            datas.value.push(...list);
        }
        checkIfAutoLoadMore();
    } catch (error) {
        GlobalInfo.showError(error);

        autoLoadTimer = window.setTimeout(() => {
            showMore(true);
        }, 2000);
    }
    finally {
        if (isAuto == false) {
            isBusy.value = false;
        }
    }
}
</script>

<template>
    <div class="pageContent">
        <Loading v-if="isBusy" class="loadingV3" />
        <!-- Start Page Header -->
        <div class="page-header">

            <div class="hor">
                <div class="controls">
                    <div class="input-prepend input-group" style="width: 430px;">
                        <span class="add-on input-group-addon"><i class="fa fa-calendar"></i></span>
                        <input v-model="timeRange" type="text" ref="dataPickerEle" class="form-control span4" />
                    </div>
                </div>

                <a class="btn btn-light" @click="searchClick">
                    <i class="fa fa-search"></i> 搜索</a>
            </div>

        </div>
        <!-- End Page Header -->

        <!-- Start Row -->
        <div ref="rowAreaEle" class="row" v-if="isReady">

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
                                            <div class="item app" v-if="showAppContext">{{ item.appContext }}</div>
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
                            <tfoot>
                                <tr v-if="!autoLoadMore && !isBusy">
                                    <td class="noborder" style="text-align: center;">
                                        <a @click="showMore(false)">显示更多</a>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="noborder" style="text-align: center;">
                                        <div class="checkbox checkbox-info checkbox-circle">
                                            <input id="chkAutoLoad" type="checkbox" v-model="autoLoadMore">
                                            <label for="chkAutoLoad">
                                                自动加载更多
                                            </label>
                                        </div>
                                    </td>
                                </tr>
                            </tfoot>
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
a {
    cursor: pointer;
}

.pageContent {
    display: flex;
    flex-direction: column;

}

.row {
    flex: 1;
    overflow-y: auto;
    overflow-x: hidden;
    margin: 0px 0px 0 0;
    padding-top: 20px;
}

.page-header {
    display: flex;
    flex-direction: column;
    align-items: center;
    box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.1);
    z-index: 3;
}

.hor {
    display: flex;
    flex-direction: row;
}

.hor .btn {
    margin-left: 10px;
    padding: unset 10px;
    height: 34px;
    display: flex;
    flex-direction: row;
    align-items: center;
    justify-content: center;
}

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
