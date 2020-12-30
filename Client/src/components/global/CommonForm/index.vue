<template>
  <div>
    <el-form 
      ref="form" 
      v-bind="attrs"
      v-on="$listeners"
      :model="form" 
    >
      <template v-for="(formInput, index) in formInputs">
        <el-row type="flex" :key="index">
          <el-col v-for="item in formInput" :key="item.attrs.prop" :span="item.span || 24">
            <el-form v-bind="item.itemAttrs">
              <component
                :is="item.tag"
                v-model="form[item.attrs.prop]"
                v-bind="item.attrs"
              ></component>
            </el-form>
          </el-col>
        </el-row>
      </template>
    </el-form>
  </div>
</template>

<script>
import { isFunction } from '@/utils/validate'
import { formConfig, formItemConfig } from './default'
import componentMap from './componentMap'
import MySelect from '@/components/Select'
export default {
  name: 'CommonForm',
  components: {
    MySelect
  },
  props: {
    model: {
      type: Object,
      default () { 
        return {} 
      }
    },
    formItems: {
      type: Array,
      default () { 
        return [] 
      } 
    },
    columnNumber: { // 一行多少个列数
      type: Number,
      default: 1
    },
    // lineNumber: { // 函数
    //   type: Number,
    //   default: 1
    // }
  },
  watch: {
    model: {
      immediate: true,
      handler (val) { // 只调用一次
        console.log(this.model, 'model')
        this.form = JSON.parse(JSON.stringify(val))
      }
    },
    form: { // 每次改变model的值都向外传递事件，保证拿到最新的值
      deep: true,
      immediate: true,
      handler (val) {
        console.log(this.form, 'form')
        this.$emit('change', val)
      }
    }
  },
  computed: {
    attrs () {
      return Object.assign({}, formConfig, this.$attrs)
    },
    formInputs () {
      return this.normalizeFormItems()
    }
  },
  updated () {
    console.log(this.attrs, 'attrs')
  },
  data () {
    return {
      form: {}
    }
  },
  methods: {
    normalizeFormItems () { // 先根据行列计算除二维数组，对二维数组的每一项都进行格式化
      let result = []
      let index = -1
      for (let i = 0; i < this.formItems.length; i++) {
        let item = this.formItems[i]
        if (i % this.columnNumber === 0) {
          result[++index] = []
        }
        if (this.isRender(item.isRender)) {
          result[index].push(this.createComputedInput(item))
        }
      }
      return result
    },
    createComputedInput (formItem) {
      let item = { ...formItem }
      let tag = item.tag || 'text' // 默认是text
      let { component, attrs: defaultAttrs } = componentMap[tag]
      item.tag = component
      item.attrs = Object.assign({}, defaultAttrs || {}, item.attrs || {})
      item.itemAttrs = Object.assign({}, formItemConfig, item.itemAttrs || {})
      return item
    },
    isRender (isRender) { // 是否渲染当前formITEM
      return isRender 
        ? isFunction(isRender)
          ? isRender(this.form)
          : isRender
        : true
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