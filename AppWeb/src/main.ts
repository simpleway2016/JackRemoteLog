import "./init"
import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import './assets/main.css'
import {GlobalInfo} from "./GlobalInfo"
import Toast from "vue-toastification";
// Import the CSS or use your own!
import "vue-toastification/dist/index.css";



GlobalInfo.init();

const app = createApp(App)

const options = {
    timeout: 2000 
    // You can set your default options here
};

app.use(Toast,options);
app.use(router)

app.mount('#app')

