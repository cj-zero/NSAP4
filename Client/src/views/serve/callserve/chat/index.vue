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
      <Message :serveId="serveId" v-show="currentName === 'message'"></Message>
    <!-- </template> -->
    <!-- <template v-show="currentName === 'progress'">

    </template> -->
    <template v-if="formName === '编辑'">
      <check :sapOrderId="sapOrderId" :customerId="customerId" v-show="currentName === 'check'"></check>
    </template>
  </div>
</template>

<script>
import TabList from './tabList'
import Message from './message'
import Check from './check'
export default {
  components: {
    TabList,
    Message,
    Check
  },
  props: {
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
      initialName: 'message'
    }
  },
  computed: {
    tabList () {
      if (this.formName) {
        console.log('success')
        let tabList = [
          { label: '消息', name: 'message' },
          { label: '服务进度', name: 'progress' }
        ]
        if (this.formName === '编辑') {
          tabList.push({
            label: '核对',
            name: 'check'
          })
        }
        return tabList
      }
      return []
    }
  },
  methods: {
    onTabChange (val) {
      console.log(val, 'val')
      this.currentName = val
    },
    clearTabStore () {
      this.$refs.tabList.clear()
    }
  },
  created () {

  },
  mounted () {

  },
  updated () {}
}
</script>
<style lang='scss' scoped>
</style>