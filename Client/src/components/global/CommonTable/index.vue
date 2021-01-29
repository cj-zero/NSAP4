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
    <template v-for="column in newColumns">
      <!-- 多选 -->
      <el-table-column 
        :key="column.prop"
        type="selection"
        v-if="column.type === 'selection'"
        v-bind="column"
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
        v-bind="column"
      >
        <template slot="header" slot-scope="scope">
          <!-- 自定义表头 -->
          <template v-if="column.isCustomizeHeader">
            <slot :name="column.headerName || `${column.prop}_header`" :row="scope.row" :label="column.label"></slot>
          </template>
          <template v-else>
            {{ column.label }}
          </template>
        </template>
        <template slot-scope="scope">
          <!--  有箭头的操作 -->
          <div class="link-container text-overflow" v-if="column.type === 'link'" style="width: 100%;">
            <img :src="rightImg" @click="isFunction(column.handleClick) ? column.handleClick({ ...scope.row, ...(column.options || {}) }) : _noop" class="pointer" />
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
          <!-- 表单组件 -->
          <template v-else-if="column.component">
            <!-- 当表格中字段很多且多数为表单元素的时候，不推荐使用slot，使用component选项能大大减少template代码量，可以配合在common-table外层套上el-form使用 -->
            <el-form-item :prop="dataKey + '.' + scope.$index + '.' + column.prop" v-bind="column.component.itemAttrs">
              <component
                :is="column.component.componentName"
                v-model="scope.row[column.prop || column.component.attrs.prop]"
                v-bind="column.component.attrs"
                v-on="column.component.on"
                v-infotooltip:200.top-start
              ></component>
            </el-form-item>
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
import { mergeConfig, getComponentName, mergeComponentAttrs } from '../mergeConfig'
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
    dataKey: {
      type: String,
      default: 'list'
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
      return mergeConfig(defaultTableConfig, this.$attrs)
    },
    selectionColumns () {
      return this.columns.some(item => item.type === 'selection')
    }
  },
  watch: {
    columns: {
      immediate: true,
      handler () {
        this.newColumns = this.normalizeColumns(this.columns)
      }
    }
  },
  data () {
    return {
      rightImg,
      radio: '',
      newColumns: [],
      currentRow: null,
      selectionList: [] // 多选的数据
    }
  },
  methods: {
    isFunction,
    _noop () {
      noop()
    },
    normalizeColumns (columns) {
      return columns.map(column => {
        column = mergeConfig(defaultColumnConfig, column)
        if (column.component) {
          let component = column.component
          component.componentName = getComponentName(component)
          component.attrs = mergeComponentAttrs(component)
          component.itemAttrs = mergeComponentAttrs(component, 'itemAttrs')
          component.on = mergeComponentAttrs(column.component, 'on')
        }
        return column
      })
    },
    onRowClick (row) {
      if (this.radioKey) {
        // 点击行单选
        this.radio = row[this.radioKey]
      }
      if (this.selectionColumns && row.selectable) {
        // 点击行进行多选 选择
        this.$refs.commonTable.toggleRowSelection(row)
      }
      this.currentRow = row
      console.log(this.currentRow, 'currentRow')
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
    resetRadio () {
      this.radio = ''
    },
    onSelectChange (val) {
      this.selectionList = val
    },
    checkSelectable (row) {
      row.selectable = this.selectedList.length ? this.selectedList.every(item => item[this.selectedKey] !== row[this.selectedKey]) : true
      return row.selectable
    }
  },
  created() {},
  mounted() {
    setTimeout(() => {
      Object.keys(this.$refs.commonTable.$options.methods).forEach(methodName => {
        if (methodName in this) return
        this[methodName] = this.$refs.commonTable[methodName]
      })
    }, 20)
  }
}
</script>
<style lang="scss" scoped></style>
