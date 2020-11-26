<template>
  <el-dialog
    class="my-dialog-wrapper my-dialog-mini"
    v-el-drag-dialog
    v-loading.fullscreen="loading"
    :element-loading-text="loadingText"
    element-loading-spinner="el-icon-loading"
    element-loading-background="rgba(0, 0, 0, 0.8)"
    :center="center"
    :width="width"
    :title="title"
    :modal="modal"
    :top="top"
    :visible.sync="dialogVisible"
    :modal-append-to-body="mAddToBody"
    :close-on-click-modal="closeByClickModal"
    :append-to-body="appendToBody"
    :destroy-on-close="destroyOnClose"
    @closed="onClosed"
    @close="onClosed"
    @open="onOpen"
    @click.native.stop
  >
    <slot></slot>
    <div slot="footer" class="dialog-footer" style="text-align: center;" v-if="isShowBtn && btnList.length">
      <template v-for="btnItem in btnList">
        <el-button
          class="btn-item customer-btn-class"
          :class="btnItem.className"
          v-if="btnItem.isShow === undefined ? true : btnItem.isShow"
          :key="btnItem.btnText"
          :type="btnItem.type || 'primary'"
          @click="btnItem.handleClick(btnItem.options || {})"
          :size="btnItem.size || 'mini'"
          :loading="btnItem.loading === undefined ? false: btnItem.loading"
        >{{ btnItem.btnText }}</el-button>
      </template>
    </div>
  </el-dialog>  
</template>

<script>
import elDragDialog from "@/directive/el-dragDialog";
export default {
  directives: {
    elDragDialog
  },
  props: {
    width: {
      type: String,
      default: '50%'
    },
    modal: {
      type: Boolean,
      default: false
    },
    top: {
      type: String,
      default: '76px'
    },
    loadingText: {
      type: String,
      defualt: '拼命加载中'
    },
    title: {
      type: String,
      default: ''
    },
    isShowBtn: {
      type: Boolean,
      default: true
    },
    mAddToBody: { // 遮罩层是否插入至 body 元素上，若为 false，则遮罩层会插入至 Dialog 的父元素上
      type: Boolean,
      default: false
    },
    closeByClickModal: {
      type: Boolean,
      default: false
    },
    appendToBody: { // Dialog 自身是否插入至 body 元素上。嵌套的 Dialog 必须指定该属性并赋值为 true
      type: Boolean,
      default: false 
    },
    center: {
      type: Boolean,
      default: false
    },
    btnList: {
      type: Array,
      default () {
        return []
      }
    },
    onClosed: { // 弹窗关闭
      type: Function,
      default () { () => {} }
    },
    onOpen: { // 弹窗打开
      type: Function,
      default () { () => {} }
    },
    loading: {
      type: Boolean,
      default: false
    },
    destroyOnClose: Boolean // 关闭弹窗时 是否销毁内部元素
  },
  data () {
    return {
      dialogVisible: false
    }
  },
  methods: {
    open () {
      this.dialogVisible = true
    },
    close () {
      this.dialogVisible = false
    }
  },
  created () {},
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.my-dialog-wrapper {
  // overflow-y: hidden;
  &::v-deep .el-dialog__body {
    padding: 10px !important;
  }
  .btn-item {
    border: 1px solid #DCDFE6;
    &.danger {
      background-color: rgba(245, 108, 108, 1);
    }
    &.close {
      background-color: #fff;
      color: black;
    }
  }
}
</style>