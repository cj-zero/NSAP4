<template>
  <transition :name="transition" @after-leave="doDestroy">
    <div
      class="infotooltip-row-tip"
      :class="{
        'infotooltip-row-tip-top-start': placement == 'top-start',
        'infotooltip-row-tip-top': placement == 'top',
        'infotooltip-row-tip-left': placement == 'left',
        'infotooltip-row-tip-right': placement == 'right',
        'infotooltip-row-tip-bottom': placement == 'bottom'
      }"
      :style="tooltipStyle"
      v-show="showPopper"
      :id="tooltipId"
      ref="popper"
    >
      <!-- {{tooltipContent}} -->
      <p class="content" v-for="content in tooltipContent" :key="content">{{ content }}</p>
    </div>
  </transition>
</template>

<script>
import Popper from 'element-ui/lib/utils/vue-popper.js'
import { generateId } from 'element-ui/lib/utils/util.js'

export default {
  name: 'infotoolTip',
  props: {
    //动画名称，元素显示隐藏的时候的动画
    transition: {
      type: String,
      default: 'fade-in-linear'
    },
    tooltipStyle: {
      type: Object,
      default() {
        return {}
      }
    },
    visibleArrow: {
      type: Boolean,
      default: true
    },
    //定位方向 left|top|right|bottom
    placement: {
      type: String,
      default: 'left'
    },
    //poper的设置，具体可参考popper.js
    popperOptions: {
      default() {
        return {
          boundariesPadding: 10,
          gpuAcceleration: false
        }
      }
    }
  },
  mixins: [Popper],
  data() {
    return {
      referenceElm: null, //定位元素
      tooltipContent: '' //显示内容
    }
  },
  computed: {
    //生产动态的元素ID
    tooltipId() {
      return `el-popover-${generateId()}`
    }
  },
  beforeCreate() {},
  mounted() {
    this.$nextTick(() => {
      //赋值 popperElm,
      this.popperElm = this.$refs.popper
    })
  },
  methods: {
    //显示tooltip
    handleMouseEnter() {
      //这句要加上，执行了doDestroy后方能够更新referenceElm，不然referenceElm这个不更新
      if (this.popperJS) {
        this.doDestroy()
      }
      console.log(this.handleMouseEnter, 'enter')
      this.showPopper = true
    },
    //隐藏tooltip
    handleMouseLeave() {
      this.showPopper = false
    }
  },
  watch: {}
}
</script>

<style lang="scss">
.infotooltip-row-tip {
  position: fixed;
  z-index: 6000 !important;
  padding: 10px;
  font-size: 12px;
  line-height: 1.2;
  min-width: 10px;
  background: #303133;
  color: #fff;
  border-radius: 4px;
  .content {
    word-break: break-all;
    word-wrap: break-word;
  }
}

.infotooltip-row-tip::after {
  content: '';
  display: block;
  width: 0;
  height: 0;
  position: absolute;
  // transform
}
.infotooltip-row-tip-top-start {
  transform: translate3d(0, -10px, 0);
  &::after {
    border-left: 8px solid transparent;
    border-right: 8px solid transparent;
    border-top: 8px solid #303133;
    bottom: -7px;
  }
}

.infotooltip-row-tip-top {
  transform: translate3d(0, -10px, 0);
  &::after {
    left: 50%;
    border-left: 8px solid transparent;
    border-right: 8px solid transparent;
    border-top: 8px solid #303133;
    bottom: -7px;
    transform: translate3d(-50%, 0, 0);
  }
}

.infotooltip-row-tip-left::after {
  border-top: 8px solid transparent;
  border-bottom: 8px solid transparent;
  border-left: 8px solid #303133;
  right: -7px;
}

.infotooltip-row-tip-right::after {
  border-top: 8px solid transparent;
  border-bottom: 8px solid transparent;
  border-right: 8px solid #303133;
  left: -7px;
}

.infotooltip-row-tip-bottom::after {
  border-left: 6px solid transparent;
  border-right: 6px solid transparent;
  border-bottom: 6px solid #303133;
  top: -3px;
}
</style>
