<template>
  <el-table 
    ref="commonTable"
    :data="data" 
    v-loading="loading" 
    size="mini"
    stripe
    border
    fit
    :row-key="rowKey"
    :height="height"
    :max-height="maxHeight"
    @current-change="onCurrentChange"
    @row-click="onRowClick"
    @selection-change="onSelectChange"
    :row-class-name="tableRowClassName"
    :row-style="rowStyle"
    :cell-style="cellStyle"
    highlight-current-row
    >
    <!-- 是否出现多选 -->
    <el-table-column 
      type="selection" 
      v-if="hasSelection"
      reserve-selection
      :selectable="checkSelectable"
      show-overflow-tooltip
    >
    </el-table-column>
    <!-- 除了多选外的 -->
    <el-table-column
      v-for="item in normalColumns"
      :key="item.prop"
      :width="item.width"
      :label="item.label"
      :align="item.align || 'left'"
      :sortable="item.isSort || false"
      show-overflow-tooltip
    >
      <template slot="header" slot-scope="scope">
        <span v-if="false">{{ scope.$index }}</span>
        <!-- 自定义表头 -->
        <template v-if="item.isCustomizeHeader">
          <slot :name="`${item.prop}_header`" :row="{ ...scope, label: item.label }"></slot>
        </template>
        <template v-else>
          {{ item.label }}
        </template>
      </template>
      <template slot-scope="scope" >
        <!--  有箭头的操作 -->
        <div class="link-container" v-if="item.type === 'link'"> 
          <img :src="rightImg" @click="isFunction(item.handleClick) ? item.handleClick({ ...scope.row, ...(item.options || {})}) : _noop" class="pointer">
          <span>{{ scope.row[item.prop] }}</span>
        </div>
        <!-- 单选 -->
        <template v-else-if="item.type === 'radio'">
          <el-radio class="radio" v-model="radio" :label="scope.row[radioKey]">{{ &nbsp; }}</el-radio>
        </template>
        <!-- 序号 -->
        <template v-else-if="item.type === 'order'">
          <span>{{ scope.$index + 1 }}</span>
        </template>
        <!-- 按钮操作 -->
        <template v-else-if="item.type === 'operation'">
          <el-button 
            v-for="btnItem in item.actions"
            :key="btnItem.btnText"
            @click="btnItem.handleClick({ ...scope.row, ...(item.options || {})})" 
            :type="item.btnType || text" 
            :icon="item.icon || ''"
            :size="item.size || 'mini'"
          >{{ btnItem.btnText }}</el-button>
        </template>
        <!-- slot 可以再外部使用具名插槽 展示不同列的值 -->
        <template v-else-if="item.type === 'slot'">
          <!-- {{ JSON.stringify(scope.row) }} -->
          <slot :name="item.slotName || 'default'" :row="{ ...scope.row, prop: item.prop, ...(item.options || {})}"></slot>
        </template>
        <!-- 文本显示 -->
        <template v-else-if="item.originType !== 'selectoin'">
          {{ scope.row[item.prop] }}
        </template>
      </template>    
    </el-table-column>
  </el-table>  
</template>

<script>
import rightImg from '@/assets/table/right.png'
import { noop } from '@/utils/declaration'
import { isFunction } from '@/utils/validate'
export default {
  props: {
    rowKey: { // 表格标识符
      type: String,
      default: 'id'
    },
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
    rowStyle: {
      type: Function,
      default: () => {}
    },
    cellStyle: {
      type: Function,
      default: () => {}
    },
    height: {
      type: [String, Number],
      default: '100%'
    },
    maxHeight: {
      type: [Number, String]
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
  data () {
    return {
      rightImg,
      radio: '',
      currentRow: null,
      selectionList: [] // 多选的数据
    }
  },
  computed: {
    normalColumns () {
      return this.columns.filter(item => item.originType !== 'selection')
    },
    selectionColumns () {
      return this.columns.filter(item => item.originType === 'selection')[0]
    },
    hasSelection () {
      return this.selectionColumns && this.selectionColumns.originType === 'selection'
    }
  },
  methods: {
    isFunction,
    _noop () {
      noop()
    },
    onCurrentChange (val) {
      console.log(val, 'val')
      // this.radio = val
    },
    onRowClick (row) {
      if (this.radioKey) { // 点击行单选
        this.radio = row[this.radioKey]
      }
      if (this.selectionColumns && row.selectable) { // 点击行进行多选 选择
        this.$refs.commonTable.toggleRowSelection(row)
      } 
      this.currentRow = row
      this.$emit('rowClick', this.currentRow)
      // console.log(index, column, 'row click', radioKey, this.radio, Object.keys(row))
    },
    getCurrentRow () {
      return this.currentRow
    },
    resetCurrentRow () { 
      this.currentRow = null
    },
    getSelectionList () {
      return this.selectionList
    },
    tableRowClassName ({ row, rowIndex }) {
      // 把每一行的index加到row中
      row.index = rowIndex
    },
    resetRadio () {
      this.radio = ''
    },
    onSelectChange (val) {
      this.selectionList = val
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