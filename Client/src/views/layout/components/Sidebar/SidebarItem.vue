<template>
  <div v-if="!item.hidden" class="menu-wrapper">
      <template v-if="!item.children || item.children.length <= 0 || item.alwaysShow">
        <el-menu-item :index="item.path" :class="{'submenu-title-noDropdown':!isNest}">
          <i :class="`iconfont icon-${item.meta.icon}`"></i>
          <span v-if="item.meta && item.meta.title" slot="title">{{item.meta.title}}</span>
          <span class="notice" v-if="item.meta.title === '服务呼叫待确认'">{{ serviceOrderCount }}</span>
          <span class="notice" v-if="item.meta.title === '服务呼叫未派单'">{{ serviceWorkOrderCount }}</span>
        </el-menu-item>
      </template>

      <el-submenu v-else :index="item.path">
        <template slot="title">
          <i :class="`iconfont icon-${item.meta.icon}`"></i>
          <span v-if="item.meta&&item.meta.title" slot="title">{{item.meta.title}}</span>
        </template>

        <template v-for="child in routes">
          <template v-if="!child.hidden">
            <sidebar-item :is-nest="true" class="nest-menu" v-if="child.children && child.children.length>0" :item="child" :key="child.path"></sidebar-item>
            <el-menu-item  v-else :key="child.name" :index="child.path">
              <div class="menu-outer-wrapper">
                <i :class="`iconfont icon-${child.meta.icon}`"></i>
                <span v-if="child.meta&&child.meta.title" slot="title">{{child.meta.title}}</span>
                <span class="notice" v-if="child.meta.title === '服务呼叫待确认' && message">{{ message.ServiceOrderCount }}</span>
                <span class="notice" v-if="child.meta.title === '服务呼叫未派单' && message">{{ message.ServiceWorkOrderCount }}</span>
              </div>
            </el-menu-item>
          </template>
        </template>
      </el-submenu>

  </div>
</template>

<script>
import { mapGetters } from 'vuex'
// import { message } from '@/utils/signalR'
export default {
  name: 'SidebarItem',
  props: {
    // route配置json
    item: {
      type: Object,
      required: true
    },
    isNest: {
      type: Boolean,
      default: false
    },
    basePath: {
      type: String,
      default: ''
    }
  },
  data() {
    console.log(this.$pendingNumber, 'pendingNubmer')
    return {
      routes: [],
      message: this.$pendingNumber || {
        ServiceOrderCount: 0,
        ServiceWorkOrderCount: 0
      }
    }
  },
  watch: {
    item() {
      this.groupRouters()
    },
    message (val) {
      console.log(val, 'message pending')
    }
  },
  created() {
    this.groupRouters()
  },
  methods: {
    groupRouters() {
      this.routes = this.item.children && this.item.children.length > 0 && this.item.children.sort((a, b) => a.meta.sortNo - b.meta.sortNo)
    }
  },
  computed: {
    ...mapGetters([
      'serviceOrderCount',
      'serviceWorkOrderCount'
    ])
  }
}
</script>
<style lang="scss">
  .menu-wrapper .iconfont{
    margin-right: 5px;
    font-size: 16px;
    vertical-align: middle;
  }
  .menu-outer-wrapper {
    position: relative;
    .notice {
      position: absolute;
      right: -43px;
      top: -14px;
      color: red !important;
    }
  }
</style>


