<template>
  <div class="chat-wrapper">
    <div class="tab-wrapper">
      <tab-list
        ref="tabList"
        :initialName="initialName" 
        :texts="tabList"
        @tabChange="onTabChange"></tab-list>
    </div>
    <!-- <template v-show="currentName === 'message'"> -->
    <!-- 留言 -->
    <div class="content-wrapper" :class="{ reimburse: this.formName === '报销' }">
      <Message :key="timer" :serveId="serveId" v-show="currentName === 'message'" :type="type"></Message>
      <!-- </template> -->
      <!-- 服务进度 -->
      <!-- <Log v-show="currentName === 'progress'" :serveId="serveId"></Log> -->
      <!-- 核对设备 -->
      <!-- 日报 -->
      <Daily v-show="currentName === 'daily'" :serviceOrderId="serveId" v-bind="$attrs"></Daily>
      <template v-if="formName === '编辑'">
        <check :sapOrderId="sapOrderId" :customerId="customerId" v-show="currentName === 'check'"></check>
      </template>
    </div>
    
  </div>
</template>

<script>
import TabList from './tabList'
import Message from './message'
import Check from './check'
import Daily from './daily'
// import Log from './log'
export default {
  components: {
    TabList,
    Message,
    Check,
    Daily
    // Log
  },
  props: {
    timer: null, // 用于重新渲染组件
    serveId: {
      type: [Number, String],
      default: ''
    },
    sapOrderId: {
      type: [Number, String],
      default: ''
    },
    customerId: {
      type: [Number, String],
      default: ''
    },
    formName: {
      type: String,
      default: ''
    }
  },
  data () {
    return {
      currentName: 'message',
      initialName: 'message',
      type: ''
    }
  },
  watch: {
    formName: {
      immediate: true,
      handler (val) {
        if (val === '报销') { // 报销模块
          this.currentName = 'message'
          this.initialName = 'message'
          this.type = 'readonly'
        }
      }
    }
  },
  computed: {
    tabList () {
      let tabList = []
      if (this.formName) {
        if (this.formName === '报销') { // 报销模块只有服务进度
          tabList = [{ label: '服务进度', name: 'message' }]
        } else {
          tabList = [
            { label: '消息', name: 'message' },
            // { label: '服务进度', name: 'progress' }
          ]
        }
        if (this.formName === '编辑') {
          tabList.push({
            label: '核对',
            name: 'check'
          })
        }
        if (this.formName === '查看') {
          tabList.push({
            label: '日报',
            name: 'daily'
          })
        }
      }
      return tabList
    }
  },
  methods: {
    onTabChange (val) {
      this.currentName = val
    },
    clearTabStore () {
      this.$refs.tabList.clear()
    }
  },
  created () {
    console.log(this.$attrs.isFinish, 'isFinish')
  },
  mounted () {

  },
  updated () {}
}
</script>
<style lang='scss' scoped>
.chat-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  .content-wrapper {
    overflow: hidden;
    flex: 1;
    &.reimburse {
      margin-top: 10px;
    }
  }
}
</style>