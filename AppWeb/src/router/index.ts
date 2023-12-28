import { createRouter, createWebHashHistory } from 'vue-router'
import LogListView from "../views/LogListView.vue"

const router = createRouter({
  history: createWebHashHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path:"/",
      name:"home",
      component:LogListView //import('@/views/TestFile.vue')
    }
  ]
})

export default router
