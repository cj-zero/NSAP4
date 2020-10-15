<template>
  <el-dialog
    class="my-dialog-wrapper my-dialog-mini"
    v-el-drag-dialog
    v-loading="loading"
    :center="center"
    :width="width"
    :title="title"
    :visible.sync="dialogVisible"
    :modal-append-to-body="mAddToBody"
    :close-on-click-modal="closeByClickModal"
    :append-to-body="appendToBody"
    @closed="onClosed"
    @close="onClosed"
  >
    <slot></slot>
    <span slot="footer" class="dialog-footer" v-if="isShowBtn && btnList.length">
      <template v-for="btnItem in btnList">
        <el-button
          class="customer-btn-class"
          v-if="btnItem.isShow === undefined ? true : btnItem.isShow"
          :key="btnItem.btnText"
          :type="btnItem.type || 'primary'"
          @click="btnItem.handleClick(btnItem.options)"
          :size="btnItem.size || 'mini'"
          :loading="btnItem.loading === undefined ? false: btnItem.loading"
        >{{ btnItem.btnText }}</el-button>
      </template>
    </span>
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
    onClosed: {
      type: Function,
      default () { () => {} }
    },
    loading: {
      type: Boolean,
      default: false
    }
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
  &::v-deep .el-dialog__body {
    padding: 10px !important;
  }
}

</style>