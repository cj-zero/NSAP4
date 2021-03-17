<template>
  <el-dialog
    class="my-dialog-wrapper"
    v-el-drag-dialog
    v-loading.fullscreen="loading"
    v-bind="attrs"
    v-on="$listeners"
    :visible.sync="dialogVisible"
    @click.native.stop
  >
    <el-row type="flex" align="middle" class="title-wrapper" slot="title">
      <slot name="title">
        <template v-if="title">
          <div class="my-dialog-icon"></div>
          <span class="my-dialog-title">{{ title }}</span>
        </template>
      </slot>
    </el-row>
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
  <!-- </div> -->
   
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
    title: {
      type: String,
      default: ''
    },
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
    },
    // dialogVisible: {
    //   get () {
    //     return this.dialogVisible
    //   },
    //   set (val) {
    //     this.$emit('update:visible', val)
    //   }
    // }
  },
  watch: {
    dialogVisible () {
      console.log(this.dialogVisible, 'dialogVisible')
    }
  },
  data () {
    return {
      // dialogVisible: false,
      dialogVisible: false,
      key: 0
    }
  },
  methods: {
    open () {
      this.dialogVisible = true
      this.$emit('update:visible', true)
    },
    close () {
      this.dialogVisible = false
      this.$emit('update:visible', false)
    },
    resetInfo () {
      // this.key++
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
  .title-wrapper {
    position: relative;
    .my-dialog-icon {
      position: absolute;
      left: 0;
      top: 0;
      bottom: 0;
      width: 4px;
      border-radius: 4px;
      background-color: #F8B500
    }
    .my-dialog-title {
      margin-left: 15px;
      font-size: 18px;
      font-weight: bold;
    }
  }
  
  &::v-deep {
    .el-dialog__header {
      overflow: hidden;
      min-height: 37px;
      padding: 10px 0 !important;
      margin: 0 10px !important;
      border-bottom: 1px solid #f2f2f3;
      .el-dialog__headerbtn {
        top: 10px;
        right: 7px;
      }
    }
    .el-dialog__footer {
      padding: 10px 20px;
      box-shadow: 0 -2px 1px 0px #eee, 0 2px 1px 0px #eee;
    }
    .el-form-item__label {
      font-size: 12px !important;
      font-weight: normal !important;
    }
    .el-form-item.el-form-item--small {
      margin-bottom: 8px;
    }
    .el-form-item.is-error.is-required.el-form-item--small {
      margin-bottom: 18px;
    }
  }
  &::v-deep .el-dialog__body {
    padding: 10px !important;
  }
  .btn-item {
    border: 1px solid #DCDFE6;
    &.danger {
      background-color: #D9001B;
    }
    &.outline {
      background-color: #fff;
      color: #F8B500;
      border: 1px solid #F8B500;
    }
    &.close {
      background-color: #fff;
      color: black;
    }
  }
}
</style>