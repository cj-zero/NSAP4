<template>
  <el-row 
    ref="reference"
    type="flex" 
    justify="space-between" 
    class="input-wrapper"
    :class="{
      'single': isShowInput && !isRange,
      'range': isShowInput && isRange
    }"
    v-click-outside="hidePicker" 
    @click.native="showPicker" 
    v-if="isShowInput" 
  >
    <el-icon class="icon el-icon-time"></el-icon>
    <input class="input" :class="{ 'single': !isRange }" :value="prevValue" :readonly="true" placeholder="开始日期" />
    <template v-if="isRange">
      <span class="range-text">至</span>
      <input class="input" :value="nextValue" :readonly="true" placeholder="结束日期" />
    </template>
  </el-row>
</template>

<script>
import ClickOutside from 'element-ui/lib/utils/clickoutside'
import { formatDate } from '@/utils/date'
import Vue from 'vue'
import ControlPannel from './ControlPannel'
import Popper from 'element-ui/lib/utils/vue-popper';
import merge from 'element-ui/lib/utils/merge'
const NewPopper = {
  props: {
    appendToBody: Popper.props.appendToBody,
    placement: { type: String, default: 'bottom-end' },
    offset: Popper.props.offset,
    boundariesPadding: Popper.props.boundariesPadding,
    arrowOffset: Popper.props.arrowOffset
  },
  methods: Popper.methods,
  data() {
    return merge({ visibleArrow: true}, Popper.data)
  },
  beforeDestroy: Popper.beforeDestroy
};

export default {
  mixins: [NewPopper],
  props: {
    value: {
      type: [Date, Array, String],
      default: ''
    },
    markedDateList: {
      type: Array,
      default: () => []
    },
    isRange: {
      type: Boolean
    },
    isShowInput: {
      type: Boolean
    },
    isSingleClick: {
      type: Boolean
    }
  },
  directives: {
    ClickOutside
  },
  computed: {
    reference() {
      const reference = this.$refs.reference;
      return reference.$el || reference;
    },
    prevValue () {
      return this._formatDate(formatDate(Array.isArray(this.value) ? this.value[0] || '' : this.value || ''))
    },
    nextValue () {
      return this._formatDate(formatDate(Array.isArray(this.value) ? this.value[1] || '': ''))
    },
  },
  watch: { // 6 * 7
    isShowInput: {
      immediate: true,
      handler (val) {
        console.log(val, 'val')
        if (val) {
          this.pickerVisible = false
        }
      }
    },
    value: {
      immediate: true,
      handler () {
        console.log('watch', this.value, this.picker)
        this.$nextTick(() => {
          if (this.picker) {
            console.log(this.value, 'prev value')
            this.picker.value = this.value
          }
        })
      }
    }
  },
  data () {
    return {
      pickerVisible: true
    }
  },
  methods: {
    _formatDate (value) {
      console.log(value, 'copy value prevvalue')
      return value ? formatDate(value) : ''
    },
    hidePannel () {
      if (!this.isShowInput) return
      this.isShow = false
      this.picker.visible = false
    },
    togglePannel () {
      console.log('click')
      this.picker.visible = !this.picker.visible
      console.log(this.picker.visible, 'visible')
    },
    hidePicker() {
      if (!this.isShowInput) return
      if (this.picker) {
        this.pickerVisible = this.picker.visible = false;
        this.destroyPopper()
      }
    },
    showPicker() {
      if (!this.picker) {
        this.mountPicker();
      }
      this.$nextTick(() => {
        this.picker.visible = this.pickerVisible = true;
        this.updatePopper();
        this.picker.value = this.value;
      })
    },
    mountPicker () {
      this.picker = new Vue(ControlPannel).$mount()
      this.popperElm = this.picker.$el
      this.picker.value = this.value
      this.picker.markedDateList = this.markedDateList
      this.picker.isShowInput = this.isShowInput
      this.picker.isRange = this.isRange
      this.picker.visible = this.pickerVisible
      this.picker.isSingleClick = this.isSingleClick
      this.$el.appendChild(this.picker.$el);
      if (this.isShowInput) {
        this.picker.$on('dodestroy', this.doDestroy);
      } else { 
        this.$el.appendChild(this.picker.$el);
      }
      this.picker.$on('pick', (date = '', visible = false) => {
        if (this.isShowInput) {
          this.pickerVisible = this.picker.visible = visible;
        }
        this.$emit('input', date)
      });
      this.picker.$on('click', val => { // 向外传递click事件
        this.$emit('click', val)
      })
    },
    unmountPicker() {
      if (this.picker) {
        this.picker.$destroy();
        this.picker.$off();
        const parentNode = this.picker.$el.parentNode
        parentNode && parentNode.removeChild(this.picker.$el);
      }
    },
  },
  created () {
    this.popperOptions = {
      boundariesPadding: 0,
      gpuAcceleration: false
    }
  },
  mounted () {
    // this.mountPicker()
  },
  beforeDestroy () {
    this.unmountPicker()
  }
}
</script>
<style lang='scss' scoped>
.input-wrapper {
  // display: inline-block;
  position: relative;
  box-sizing: border-box;
  padding: 3px 10px;
  border: 1px solid #DCDFE6;
  background-color: #fff;
  transition: all .4s;
  border-radius: 4px;
  &.range {
    width: 300px;
  }
  &.single {
    width: 150px;
  }
  &:hover {
    border-color: #409eff;
  }
  .icon {
    position: absolute;
    left: 10px;
    top: 50%;
    color: rgba(0, 0, 0, .5);
    transform: translate3d(0, -50%, 0);
  }
  .input {
    width: 50%;
    height: 28px;
    line-height: 28px;
    text-align: center;
    border: none;
    outline: none;
    background-color: transparent;
    &.single {
      width: 100%;
    }
  }
  .range-text {
    height: 28px;
    padding: 0 4px;
    line-height: 28px;
    text-align: center;
  }
}
  
</style>