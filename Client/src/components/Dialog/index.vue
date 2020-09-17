<template>
  <el-dialog
    v-el-drag-dialog
    :width="width"
    :title="title"
    :visible.sync="dialogVisible"
    :modal-append-to-body="mAddToBody"
    :close-on-click-modal="closeByClickModal"
    :append-to-body="appendToBody"
  >
    <slot></slot>
    <span slot="footer" class="dialog-footer" v-if="isShowBtn">
      <el-button @click="cancel">取 消</el-button>
      <el-button type="primary" @click="confirm">确 定</el-button>
    </span>
  </el-dialog>
</template>

<script>
import { noop } from '@/utils/declaration'
export default {
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
    cancel: {
      type: Function,
      default () {
        return noop
      }
    },
    confirm: {
      type: Function,
      default () {
        return noop
      }
    }
  },
  data () {
    return {
      visible: false
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