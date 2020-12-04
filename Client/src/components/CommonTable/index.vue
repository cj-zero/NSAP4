<template>
  <el-table 
    ref="commonTable"
    :data="data" 
    v-loading="loading" 
    size="mini"
    stripe
    border
    fit
    row-key="id"
    :height="height"
    :max-height="maxHeight"
    @current-change="onCurrentChange"
    @row-click="onRowClick"
    @selection-change="onSelectChange"
    :row-class-name="tableRowClassName"
    highlight-current-row
    >
    <!-- 是否出现多选 -->
    <el-table-column 
      type="selection" 
      v-if="selectionColumns && selectionColumns.originType === 'selection'"
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
      :show-overflow-tooltip="!item.isMultipleLines"
    >
      <template slot-scope="scope" >
        <!--  有箭头的操作 -->
        <div class="link-container" v-if="item.type === 'link'"> 
          <img :src="rightImg" @click="item.handleClick({ ...scope.row, ...(item.options || {})})" class="pointer">
          <span>{{ scope.row[item.prop] }}</span>
        </div>
        <!-- 单选 -->
        <template v-else-if="item.type === 'radio'">
          <el-radio class="radio" v-model="radio" :label="scope.row[item.prop]">{{ &nbsp; }}</el-radio>
        </template>
        <!-- 按钮操作 -->
        <template v-else-if="item.type === 'operation'">
          <el-button 
            v-for="btnItem in item.actions"
            :key="btnItem.btnText"
            @click="btnItem.item.handleClick({ ...scope, ...(item.options || {})})" 
            type="text" 
            :icon="item.icon || ''"
            :size="item.size || 'mini'"
          >{{ btnItem.btnText }}</el-button>
        </template>
        <!-- 插槽 -->
        <template v-else-if="item.type === 'slot'">
          <slot :name="item.slotName || 'default'" :row="{ ...scope.row, ...(item.options || {}), prop: item.prop }"></slot>
        </template>
        <!-- 冒泡提示语分行显示 -->
        <template v-else-if="item.isMultipleLines">
          <el-tooltip placement="top-start">
            <div slot="content">
              <p v-for="(content, index) in _formatArray(scope.row[item.prop], item.contentField)" :key="index">{{ content }}</p>
            </div>
            <span style="white-space: nowrap;">{{ _formatText(scope.row[item.prop], item.contentField) }}</span>
          </el-tooltip>
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
export default {
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
    maxHeight: {
      type: [Number, String]
    },
    height: {
      type: [Number, String],
      default: '100%'
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
    }
  },
  data () {
    return {
      rightImg,
      radio: '',
      currentRow: {},
      selectionList: [] // 多选的数据
    }
  },
  computed: {
    normalColumns () {
      return this.columns.filter(item => item.originType !== 'selection')
    },
    selectionColumns () {
      return this.columns.filter(item => item.originType === 'selection')[0]
    }
  },
  methods: {
    _formatArray (data, contentField) {
      let reg = /[\r|\r\n|\n\t\v]/g
      let result = Array.isArray(data) ? data : JSON.parse(data.replace(reg, ''))
      return result.map(item => item[contentField])
    },
    _formatText (data, contentField) {
      let reg = /[\r|\r\n|\n\t\v]/g
      let result = Array.isArray(data) ? data : JSON.parse(data.replace(reg, ''))  
      return result.map(item => item[contentField]).join(' ')
    },
    onCurrentChange (val) {
      console.log(val, 'val')
      // this.radio = val
    },
    onRowClick (row) {
      // let { index } = row
      let radioKey = row.radioKey
      this.radio = row[radioKey]
      this.currentRow = row
      if (this.selectionColumns && row.selectabled) {
        this.$refs.commonTable.toggleRowSelection(row)
      }
      // console.log(index, column, 'row click', radioKey, this.radio, Object.keys(row))
    },
    getCurrentRow () {
      return this.currentRow
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
      row.selectabled = this.selectedList.length 
        ? this.selectedList.every(item => item[this.selectedKey] !== row[this.selectedKey])
        : true
      return row.selectabled
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