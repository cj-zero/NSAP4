<template>
  <el-dialog
    v-el-drag-dialog
    :center="center"
    :width="width"
    :title="title"
    :visible.sync="dialogVisible"
    :modal-append-to-body="mAddToBody"
    :close-on-click-modal="closeByClickModal"
    :append-to-body="appendToBody"
    @closed="onClosed"
  >
    <slot></slot>
    <span slot="footer" class="dialog-footer" v-if="isShowBtn">
      <el-button
        v-for="btnItem in btnList"
        :key="btnItem.btnText"
        :type="btnItem.type || 'primary'"
        @click="btnItem.handleClick"
        :size="btnItem.size || 'mini'"
      >{{ btnItem.btnText }}</el-button>
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
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
</style>