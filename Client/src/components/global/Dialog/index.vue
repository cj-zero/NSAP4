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
    <div slot="footer" class="dialog-footer" style="text-align: center;">
      <slot name="footer">
        <template v-if="isShowBtn && newBtnList.length">
          <el-button
            v-for="btnItem in newBtnList"
            class="btn-item customer-btn-class"
            :class="btnItem.className"
            :key="btnItem.btnText"
            :type="btnItem.type || 'primary'"
            @click="btnItem.handleClick(btnItem.options || {})"
            :size="btnItem.size || 'mini'"
            :loading="btnItem.loading === undefined ? false: btnItem.loading"
          >{{ btnItem.btnText }}</el-button>
        </template>
      </slot>
      
    </div>
  </el-dialog>  
</template>

<script>
import elDragDialog from "@/directive/el-dragDialog";
import { defaultConfig } from './default'
export default {
  name: 'MyDialog',
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
    },
    newBtnList () {
      return this.btnList.filter(item => !!(item.isShow === undefined ? true : item.isShow))
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