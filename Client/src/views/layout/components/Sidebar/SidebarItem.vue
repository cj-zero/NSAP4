<template>
  <div v-if="!item.hidden" class="menu-wrapper">
      <template v-if="!item.children || item.children.length <= 0 || item.alwaysShow">
        <el-menu-item :index="item.path" :class="{'submenu-title-noDropdown':!isNest}">
          <i :class="`iconfont icon-${item.meta.icon}`"></i>
          <span v-if="item.meta && item.meta.title" slot="title">{{item.meta.title}}</span>
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
                <span class="notice-wrapper" 
                  v-if="child.meta.title === '服务呼叫待确认' && vm.message.ServiceOrderCount"
                >
                  {{vm.message.ServiceOrderCount | process }}
                </span>
                <span 
                  class="notice-wrapper" 
                  v-if="child.meta.title === '服务呼叫未派单' && vm.message.ServiceWorkOrderCount"
                >
                  {{vm.message.ServiceWorkOrderCount | process }}
                </span>
              </div>
            </el-menu-item>
          </template>
        </template>
      </el-submenu>
  </div>
</template>

<script>
// import { message } from '@/utils/signalR'
export default {
  name: 'SidebarItem',
  inject: ['vm'],
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
    return {
      routes: []
    }
  },
  filters: {
    process (val) {
      return val >= 99 ? '99+' : val
    }
  },
  watch: {
    item() {
      this.groupRouters()
    }
  },
  mounted () {
    
  },
  created() {
    this.groupRouters()
  },
  methods: {
    groupRouters() {
      this.routes = this.item.children && this.item.children.length > 0 && this.item.children.sort((a, b) => a.meta.sortNo - b.meta.sortNo)
    }
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
    .notice-wrapper {
      position: relative;
      display: inline-block;
      right: 15px;
      top: -17px;
      // padding: 5px;
      color:#fff !important;
      height: 18px;
      line-height: 18px;
      font-size: 12px;
      padding: 0 6px;
      text-align: center;
      white-space: nowrap;
      background-color: rgba(230, 162, 60, 1);
      border-radius: 10px;
    }
  }
</style>


