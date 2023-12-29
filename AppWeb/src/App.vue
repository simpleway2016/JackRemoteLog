<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { GlobalInfo } from './GlobalInfo';
import LoginView from './views/LoginView.vue';
import { useRouter } from 'vue-router';
const userInfo = GlobalInfo.UserInfo;
const router = useRouter();

const publicInfo = GlobalInfo.PublicInfo;
const leftMenuEle = ref(<HTMLElement><any>null);
const showHideLeftMenu = () => {
    (<any>window).$(leftMenuEle.value).toggle(100);
}

onMounted(() => {
    loadAppContexts();
});

const loadAppContexts = async () => {
    if (!GlobalInfo.UserInfo.Token) {
        window.setTimeout(loadAppContexts, 1000);
    }
    else {
        try {
            var apps = JSON.parse(await GlobalInfo.get("/Log/GetApplications", null));
            apps.forEach((context: any) => {
                publicInfo.AppContexts.push({
                    name: context,
                    startTimeStamp: undefined,
                    SourceContexts: [],
                    SelectedSourceContexts: [],
                });
            });
        } catch (error: any) {
            if (error && error.statusCode == 401) {
                window.setTimeout(loadAppContexts, 1000);
                return;
            }
            GlobalInfo.showError(error);
        }
    }
}

const searchKeyUp = (e: KeyboardEvent) => {
    if (e.key == "Enter") {
        //router.push({ name: '/', params: { search: ProjectListProperties.searchKey } });
    }
}

const addContext = (context: any) => {
    if (publicInfo.SelectedAppContexts.some(x => x == context) == false) {

        var arr = publicInfo.SelectedAppContexts.filter(x=>x.startTimeStamp).sort( (a,b)=>a.startTimeStamp - b.startTimeStamp );
        if(arr.length){
            context.startTimeStamp = arr[arr.length - 1].startTimeStamp;
            console.log("时间戳设置为：" , context.startTimeStamp);
        }
        publicInfo.SelectedAppContexts.push(context);
        loadAppSourceContext(context);
    }
}

const addSource = (context: any, source: any) => {
    if (context.SelectedSourceContexts.some((x: any) => x == source) == false) {
        context.SelectedSourceContexts.push(source);
    }
}
const addLevel = (levelObj: any) => {
    if (publicInfo.SelectedLevels.some(x => x == levelObj) == false) {
        publicInfo.SelectedLevels.push(levelObj);
    }
}

const addTraceId = () => {
    var traceId = window.prompt("请输入TraceId", "");
    if (traceId) {
        if (publicInfo.SelectedTraceIds.some(x => x == traceId) == false) {
            publicInfo.SelectedTraceIds.push(traceId);
        }
    }
}

const addSearchKey = () => {
    var key = window.prompt("请输入关键字", "");
    if (key) {
        if (publicInfo.SearchKeys.some(x => x == key) == false) {
            publicInfo.SearchKeys.push(key);
        }
    }
}

const loadAppSourceContext = async (context: any) => {
    try {
        var sources = JSON.parse(await GlobalInfo.get("/Log/GetSourceContexts", {
            applicationContext: context.name
        }));
        context.SourceContexts = sources;
    } catch (error) {
        GlobalInfo.showError(error);
    }
}

const cancelContext = (context: any) => {
    var index = publicInfo.SelectedAppContexts.indexOf(context);
    if (index >= 0) {
        publicInfo.SelectedAppContexts.splice(index, 1);
        context.startTimeStamp = undefined;
        context.SourceContexts = [];
        context.SelectedSourceContexts = [];
    }
}

const cancelSource = (context: any, source: any) => {
    var index = context.SelectedSourceContexts.indexOf(source);
    if (index >= 0) {
        context.SelectedSourceContexts.splice(index, 1);
    }
}

const cancelLevel = (levelObj: any) => {
    var index = publicInfo.SelectedLevels.indexOf(levelObj);
    if (index >= 0) {
        publicInfo.SelectedLevels.splice(index, 1);
    }
}

const cancelTraceId = (traceId:any) => {
    var index = publicInfo.SelectedTraceIds.indexOf(traceId);
    if (index >= 0) {
        publicInfo.SelectedTraceIds.splice(index, 1);
    }
}
const cancelSearchKey = (key:any) => {
    var index = publicInfo.SearchKeys.indexOf(key);
    if (index >= 0) {
        publicInfo.SearchKeys.splice(index, 1);
    }
}
const logout = () => {
    userInfo.Token = <any>null;
    localStorage.removeItem("Token");
}
</script>

<template>
    <div class="main">
        <!-- bottom -->
        <div id="bottom">
            <!-- //////////////////////////////////////////////////////////////////////////// -->
            <!-- START SIDEBAR -->
            <div ref="leftMenuEle" id="leftMenu" class="sidebar">

                <ul class="sidebar-panel nav">
                    <li class="sidetitle">AppContexts
                        <div class="dropdown link addbtn">
                            <i data-toggle="dropdown" class="fa fa-plus"></i>
                            <ul class="dropdownBg dropdown-menu dropdown-menu-list">
                                <li v-for="context in publicInfo.AppContexts">
                                    <a @click="addContext(context)">{{ context.name }}</a>
                                </li>
                            </ul>
                        </div>
                    </li>


                    <div class="contextParent">
                        <template v-for="context in publicInfo.SelectedAppContexts">
                            <i :title="'取消 ' + context.name" @click="cancelContext(context)" class="contextitem">{{
                                context.name }}</i>
                        </template>

                    </div>

                </ul>

                <ul class="sidebar-panel nav">
                    <li class="sidetitle">SourceContexts
                        <div class="dropdown link addbtn">
                            <i data-toggle="dropdown" class="fa fa-plus"></i>
                            <ul class="dropdownBg dropdown-menu dropdown-menu-list">
                                <template v-for="context in publicInfo.SelectedAppContexts">
                                    <li role="presentation" class="dropdown-header">{{ context.name }}</li>
                                    <template v-for="source in context.SourceContexts">
                                        <li><a @click="addSource(context, source)">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{{
                                            source }}</a></li>
                                    </template>
                                </template>
                            </ul>
                        </div>
                    </li>
                    <div class="contextParent">
                        <template v-for="context in publicInfo.SelectedAppContexts">
                            <template v-for="source in context.SelectedSourceContexts">
                                <i :title="'取消 ' + source" @click="cancelSource(context, source)" class="contextitem">{{
                                    source
                                }}</i>
                            </template>

                        </template>

                    </div>
                </ul>

                <ul class="sidebar-panel nav">
                    <li class="sidetitle">TraceIds
                        <div class="dropdown link addbtn">
                            <i @click="addTraceId" class="fa fa-plus"></i>
                        </div>
                    </li>
                    <div class="contextParent">
                        <template v-for="traceid in publicInfo.SelectedTraceIds">
                            <i :title="'取消 ' + traceid" @click="cancelTraceId(traceid)" class="contextitem">{{
                                traceid
                            }}</i>
                        </template>

                    </div>
                </ul>

                <ul class="sidebar-panel nav">
                    <li class="sidetitle">Levels
                        <div class="dropdown link addbtn">
                            <i data-toggle="dropdown" class="fa fa-plus"></i>
                            <ul class="dropdownBg dropdown-menu dropdown-menu-list">
                                <li v-for="levelObj in publicInfo.LevelObjs">
                                    <a @click="addLevel(levelObj)">{{ levelObj.name }}</a>
                                </li>
                            </ul>
                        </div>
                    </li>
                    <div class="contextParent">
                        <template v-for="levelObj in publicInfo.SelectedLevels">
                            <i :title="'取消 ' + levelObj.name" @click="cancelLevel(levelObj)" class="contextitem">{{
                                levelObj.name
                            }}</i>
                        </template>

                    </div>
                </ul>

                <ul class="sidebar-panel nav">
                    <li class="sidetitle">关键字
                        <div class="dropdown link addbtn">
                            <i @click="addSearchKey" class="fa fa-plus"></i>
                        </div>
                    </li>
                    <div class="contextParent">
                        <template v-for="key in publicInfo.SearchKeys">
                            <i :title="'取消 ' + key" @click="cancelSearchKey(key)" class="contextitem">{{
                                key
                            }}</i>
                        </template>

                    </div>
                </ul>


            </div>
            <!-- END SIDEBAR -->
            <!-- //////////////////////////////////////////////////////////////////////////// -->

            <!-- //////////////////////////////////////////////////////////////////////////// -->
            <!-- START CONTENT -->
            <div id="mainContent">
                <RouterView v-slot="{ Component, route }">
                    <component :is="Component" />
                </RouterView>
            </div>
            <!-- End Content -->
            <!-- //////////////////////////////////////////////////////////////////////////// -->
        </div>


    </div>
    <div class="loginarea" v-show="!userInfo.Token">
        <LoginView />
    </div>
</template>

<style scoped>
.main {
    width: 100%;
    height: 100%;
}

#top {
    height: 60px;
    background: #399bff;
    color: #fff;
    width: 100%;
}

#bottom {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    display: flex;
    flex-direction: row;
}

#leftMenu {
    width: 250px;
    flex-shrink: 0;
}

#mainContent {
    background-color: #f5f5f5;
    flex: 1;
    flex-shrink: 0;
    width: 1px;
}

.loginarea {
    position: absolute;
    left: 0;
    top: 0;
    width: 100%;
    height: 100%;
    background-color: #F5F5F5;
    z-index: 10;
}

.sidetitle {
    position: relative;
}

.contextParent {
    display: flex;
    flex-direction: column;
    align-items: start;
}

.contextitem {
    cursor: pointer;
    font-size: 12px;
    border-radius: 1000px;
    background-color: #51b7a3;
    color: #eee;
    padding: 0px 10px;
    margin-right: 5px;
    margin-bottom: 5px;

    max-width: 100%;
    display: block;

    overflow: hidden;
    text-overflow: ellipsis;
    direction: rtl;
    white-space: nowrap;
}

.addbtn {
    position: absolute;
    cursor: pointer;
    font-size: 12px;
    right: 0;
    top: 0;
    border-radius: 1000px;
    background-color: #d2527f;
    color: #d8d7d7;
    width: 20px;
    height: 20px;
    display: flex;
    align-items: center;
    justify-content: center;
}

.dropdown-header {
    text-transform: none !important;
}

.dropdownBg {
    background-color: #fff;

}

.dropdownBg li a {
    color: #3D464D;
    padding: 9px;
}</style>