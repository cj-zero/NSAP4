<template>
  <div class="form-wrapper">
    <el-form 
      ref="form" 
      v-bind="attrs"
      v-on="$listeners"
      :model="model" 
    >
      <template v-for="(formInput, rowIndex) in formInputs">
        <el-row type="flex" :key="rowIndex" :style="rowStyle(formInput, rowIndex)">
          <el-col 
            v-for="(item, colIndex) in formInput" 
            :key="`${rowIndex}_${colIndex}`" 
            :span="item.span || 24" 
            :style="cellStyle(item, colIndex)">
            <!-- 插槽 -->
            <slot
              v-if="item.slotName"
              :name="item.slotName" 
              :model="model"
              :rowIndex="rowIndex"
              :colIndex="colIndex"
              :item="item">
            </slot>
            <!-- 表单组件 -->
            <el-form-item v-bind="item.itemAttrs" v-else>
              <component
                :is="item.componentName"
                v-model="model[item.attrs.prop]"
                v-bind="item.attrs"
                v-on="item.on"
                v-infotooltip:200
              ></component>
            </el-form-item>
          </el-col>
        </el-row>
      </template>
    </el-form>
  </div>
</template>

<script>
import { isFunction } from '@/utils/validate'
import { formConfig, formItemConfig } from './default'
import { mergeConfig, getComponentName, mergeComponentAttrs } from '../mergeConfig'
import MySelect from '@/components/Select'
import { normalizeFormConfig } from '@/utils/format'
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
    columnNumber: { // 一行多少个列数(设置了isCustomerEnd，则会无效)
      type: Number,
      default: 1
    },
    isCustomerEnd: { // 是否自定义换行
      type: Boolean
    },
    rowStyle: { // 用来设置行样式
      type: Function,
      default () { 
        () => { return {} }
      }
    },
    cellStyle: { // 用来设置列样式
      type: Function,
      default () { 
        () => { return {} }
      }
    }
  },
  watch: {
    model: {
      immediate: true,
      handler (val) { // 只调用一次
        console.log(this.model, 'model')
        // this.form = JSON.parse(JSON.stringify(val))
        this.form =val
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
  data () {
    return {
      form: {}
    }
  },
  methods: {
    normalizeFormItems () { // 先根据行列计算除二维数组，对二维数组的每一项都进行格式化
      let result = []
      let index = -1
      let items = this.formItems.filter(item => this.isRender(item.isRender))
      if (this.isCustomerEnd) {
        items = items.map(item => this.createComputedInput(item))
        result = normalizeFormConfig(items)
      } else {
        for (let i = 0; i < items.length; i++) {
          let item = items[i]
          if (i % this.columnNumber === 0) {
            result[++index] = []
          }
          result[index].push(this.createComputedInput(item))
        }
      }
      console.log(result, 'result')
      return result
    },
    createComputedInput (formItem) {
      let component = { ...formItem }
      if (component.tag) {
        component.componentName = getComponentName(component) // 组件名称
        component.attrs = mergeComponentAttrs(component) // 组件属性
        component.itemAttrs = mergeConfig(formItemConfig, component.itemAttrs)  // el-form-item属性
        component.on = mergeComponentAttrs(component, 'on') // 组件事件
      }
      return component
    },
    isRender (isRender) { // 是否渲染当前formITEM
      return typeof isRender !== 'undefined'
        ? isFunction(isRender)
          ? isRender(this.model)
          : isRender
        : true
    },
    // resetFields () {
    //   this.$refs.form.resetFields()
    //   console.log(this.model, 'model')
    // },
    clearValidate () {
      this.$refs.form.clearValidate()
    },
    // validate (callback) {
    //   return this.$refs.form.validate(callback)
    // }
  },
  created () {

  },
  mounted () {
    setTimeout(() => {
      // 将el-form上所有的方法都定义到当前的common-form组件上
      Object.keys(this.$refs.form.$options.methods).forEach(methodName => {
        if (methodName in this) return
        this[methodName] = this.$refs.form[methodName]
      })
    }, 20)
  },
}
</script>
<style lang='scss' scoped>
.form-wrapper {
  font-size: 12px !important;
  ::v-deep .el-form-item__label {
    padding-right: 4px;
  }
  ::v-deep .el-form-item--mini.el-form-item, .el-form-item--small.el-form-item {
    margin-bottom: 5px;
  }
}
</style>