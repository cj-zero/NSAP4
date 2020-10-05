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
    height="100%"
    :max-height="maxHeight"
    style="width: 100%;"
    @current-change="onCurrentChange"
    @row-click="onRowClick"
    @selection-change="onSelectChange"
    :row-class-name="tableRowClassName"
    >
    <el-table-column 
      type="selection" 
      v-if="selectionColumns && selectionColumns.originType === 'selection'"
      reserve-selection
      :selectable="checkSelectable"
      show-overflow-tooltip
    >
    </el-table-column>
    <el-table-column
      v-for="item in normalColumns"
      :key="item.prop"
      :width="item.width"
      :label="item.label"
      :align="item.align || 'left'"
      :sortable="item.isSort || false"
      show-overflow-tooltip
    >
      <template slot-scope="scope" >
        <div class="link-container" v-if="item.type === 'link'">
          <img :src="rightImg" @click="item.handleJump(scope.row)" class="pointer">
          <span>{{ scope.row[item.prop] }}</span>
        </div>
        <template v-else-if="item.type === 'radio'">
          <el-radio class="radio" v-model="radio" :label="scope.row[item.prop]">{{ &nbsp; }}</el-radio>
        </template>
        <template v-else-if="item.type === 'operation'">
          <el-button 
            v-for="btnItem in item.actions"
            :key="btnItem.btnText"
            @click="btnItem.btnClick(scope.row)" 
            type="text" 
            :icon="item.icon || ''"
            :size="item.size || 'mini'"
          >{{ btnItem.btnText }}</el-button>
        </template>
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
      type: [Number, String],
      default: 0
    },
    selectedList: { // 已经选中里的列表(多选中，用来判断是否可以点击)
      type: Array,
      default () {
        return []
      }
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
    onCurrentChange (val) {
      console.log(val, 'val')
      // this.radio = val
    },
    onRowClick (row) {
      // let { index } = row
      let radioKey = row.radioKey
      this.radio = row[radioKey]
      this.currentRow = row
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
      return this.selectedList.length 
        ? this.selectedList.every(item => item.id !== row.id)
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