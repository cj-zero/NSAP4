<template>
  <div class="message-wrapper" v-show="isShow">
    <ul class="message-list">
      <li class="message-item" v-for="(item, index) in messageList" :key="item.U_SAP_ID" @click="onClick(item, index)">
        <div class="info-wrapper">
          <span class="name">姓名: {{ item.Replier }}</span>
          <span class="serivceOrderId">服务单ID: {{ item.U_SAP_ID }}</span>
        </div>
        <el-tooltip effect="dark" placement="left">
          <div slot="content" class="mw-200">{{ item.Content }}</div>
          <div class="content">{{ item.Content }}</div>
        </el-tooltip>
        <div class="time">时间: {{ item.CreateTime }}</div>
      </li>
   </ul>
  </div>
</template>

<script>

export default {
  props: {
    messageList: {
      type: Array,
      default: () => []
    },
    isShow: Boolean
  },
  data () {
    return {

    }
  },
  methods: {
    onClick (item, index) {
      this.$emit('hasRead', { ...item, index })
    }
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.message-wrapper {
  line-height: normal;
  .message-list {
    .message-item {
      width: 200px;
      padding-right: 7px;
      border-bottom: 1px solid rgba(0, 0, 0, .25);
      cursor: pointer;
      &:hover {
        background-color: #ecf5ff;
      }
      .info-wrapper {
        display: flex;
        justify-content: space-between;
        margin-top: 5px;
      }
      .content {
        overflow: hidden;
        text-overflow: ellipsis;
        margin: 5px auto;
        max-height: 32px;
      }
      .time {
        margin-bottom: 5px;
      }
    }
  }
}
</style>