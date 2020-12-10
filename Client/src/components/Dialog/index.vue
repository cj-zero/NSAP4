<template>
  <el-dialog
    class="my-dialog-wrapper my-dialog-mini"
    v-el-drag-dialog
    v-loading.fullscreen="loading"
    v-bind="attrs"
    v-on="$listeners"
    :visible.sync="dialogVisible"
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
import { defaultConfig } from './default'
export default {
  directives: {
    elDragDialog
  },
  props: {
    isShowBtn: {
      type: Boolean,
      default: true
    },
    btnList: {
      type: Array,
      default () {
        return []
      }
    },
    loading: {
      type: Boolean,
      default: false
    },
  },
  computed: {
    attrs () {
      return Object.assign({}, defaultConfig, this.$attrs)
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