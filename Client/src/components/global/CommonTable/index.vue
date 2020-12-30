<template>
  <el-table 
    ref="commonTable"
    :data="data" 
    v-loading="loading" 
    v-bind="attrs"
    v-on="$listeners"
    @row-click="onRowClick"
    @selection-change="onSelectChange"
  >
    <template v-for="column in columns">
      <!-- 多选 -->
      <el-table-column 
        :key="column.prop"
        type="selection" 
        v-if="column.type === 'selection'"
        v-bind="mergeColumnConfig(column)"
        :selectable="checkSelectable"
      >
      </el-table-column>
      <!-- 序号 -->
      <el-table-column
        :key="column.prop" 
        type="index" 
        v-bind="column"
        v-else-if="column.type === 'index'"
      >
      </el-table-column>
      <!-- 展开 -->
      <el-table-column
        :key="column.prop" 
        type="expand" 
        v-bind="column"
        v-else-if="column.type === 'expand'"
      >
        <template slot-scope="scope">
          <slot name="expand" :row="scope.row"></slot>
        </template>
      </el-table-column>
      <el-table-column
        v-else
        :key="column.prop"
        v-bind="mergeColumnConfig(column)"
      >
        <template slot="header" slot-scope="scope">
          <!-- 自定义表头 -->
          <template v-if="column.isCustomizeHeader">
            <slot :name="`${column.prop}_header`" :row="scope.row" :label="column.label"></slot>
          </template>
          <template v-else>
            {{ column.label }}
          </template>
        </template>
        <template slot-scope="scope" >
          <!--  有箭头的操作 -->
          <div class="link-container text-overflow" v-if="column.type === 'link'" style="width: 100%;"> 
            <img :src="rightImg" @click="isFunction(column.handleClick) ? column.handleClick({ ...scope.row, ...(column.options || {})}) : _noop" class="pointer">
            <span>{{ scope.row[column.prop] }}</span>
          </div>
          <!-- 单选 -->
          <template v-else-if="column.type === 'radio'">
            <el-radio class="radio" v-model="radio" :label="scope.row[radioKey]">{{ &nbsp; }}</el-radio>
          </template>
          <!-- slot 可以再外部使用具名插槽 展示不同列的值 -->
          <template v-else-if="column.slotName">
            <slot :name="column.slotName || 'default'" :index="scope.$index" :row="scope.row" :prop="column.prop"></slot>
          </template>
         <!-- 组件 -->
         <template v-else-if="column.component">
           <component
              :is="transformComponent(column.component)"
              v-model="scope.row[column.prop]"
              v-bind="mergeComponentAttrs(column.component)"
              v-on="(column.component.on || {})"
            ></component>
         </template>
          <!-- 文本显示 -->
          <template v-else>
            {{ scope.row[column.prop] }}
          </template>
        </template>    
      </el-table-column>
    </template>
  </el-table>  
</template>

<script>
import { defaultTableConfig, defaultColumnConfig } from './default'
import componentMap from '../componentMap'
import rightImg from '@/assets/table/right.png'
import { noop } from '@/utils/declaration'
import { isFunction } from '@/utils/validate'
export default {
  name: 'CommonTable',
  props: {
    data: {
      type: Array,
      default () {
        return []
      }
    },
    columns: { 
      // 表格数据示例 { label: '文本信息', prop: '数据字段', originType: 'selection表格自带的类型', 
      //  type: '用户定义的类型', handleClick: '执行的function', width: '单元格宽度' }
      type: Array,
      default () {
        return []
      }
    },
    loading: {
      type: Boolean,
      default: false
    },
    selectedList: { // 已经选中里的列表(多选中，用来判断是否可以点击)
      type: Array,
      default () {
        return []
      }
    },
    selectedKey: { // 用来判断多选是否可以点击的key(selectedList数组中对象的唯一key值)
      type: String,
      default: 'id'
    },
    radioKey: { // 单选标识字段
      type: String
    }
  },
  computed: {
    attrs () {
      return this.mergeConfig(defaultTableConfig, this.$attrs)
    },
    selectionColumns () {
      return this.columns.some(item => item.type === 'selection')
    }
  },
  data () {
    return {
      rightImg,
      radio: '',
      currentRow: null,
      selectionList: [] // 多选的数据
    }
  },
  methods: {
    isFunction,
    _noop () {
      noop()
    },
    mergeConfig (defaultConfig = {}, customerConfig = {}) { // 用户自定义配置与默认配置合并
      return Object.assign({}, defaultConfig, customerConfig)
    },
    transformComponent (component) {
      return componentMap[component.tag].component
    },
    mergeComponentAttrs (component) {
      let newComponent = componentMap[component.tag]
      console.log(newComponent.attrs)
      return this.mergeConfig(newComponent.attrs, component.attrs)
    },
    onRowClick (row) {
      if (this.radioKey) { // 点击行单选
        this.radio = row[this.radioKey]
      }
      if (this.selectionColumns && row.selectable) { // 点击行进行多选 选择
        this.$refs.commonTable.toggleRowSelection(row)
      } 
      this.currentRow = row
      console.log(this.currentRow, 'currentRow')
      this.$emit('rowClick', this.currentRow)
      // console.log(index, column, 'row click', radioKey, this.radio, Object.keys(row))
    },
    getCurrentRow () {
      return this.currentRow
    },
    toggleRowSelection (row) {
      this.$refs.commonTable.toggleRowSelection(row)
    },
    resetCurrentRow () { 
      this.currentRow = null
    },
    getSelectionList () {
      return this.selectionList
    },
    resetRadio () {
      this.radio = ''
    },
    onSelectChange (val) {
      this.selectionList = val
      this.$emit('selectChange', val)
      console.log(val, 'selection')
    },
    clearSelection () {
      this.$refs.commonTable.clearSelection()
    },
    checkSelectable (row) {
      row.selectable = this.selectedList.length 
        ? this.selectedList.every(item => item[this.selectedKey] !== row[this.selectedKey])
        : true
      return row.selectable
    },
    mergeColumnConfig (column) { // 合并自定义配置与默认配置
      return this.mergeConfig(defaultColumnConfig, column)
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