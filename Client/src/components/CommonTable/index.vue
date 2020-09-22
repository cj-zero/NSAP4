<template>
  <el-table 
    :data="data" 
    v-loading="loading" 
    size="mini"
    stripe
    border
    fit
    height="100%"
    :max-height="maxHeight"
    style="width: 100%;"
    @current-change="onCurrentChange"
    @row-click="onRowClick"
    :row-class-name="tableRowClassName"
    >
    <el-table-column
      v-for="item in columns"
      :key="item.prop"
      :width="item.width"
      :label="item.label"
      :align="item.align || 'left'"
      :sortable="item.isSort || false"
      :type="item.originType || ''"
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
        <template v-else>
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
    }
  },
  data () {
    return {
      rightImg,
      radio: '',
      currentRow: {}
    }
  },
  methods: {
    onCurrentChange (val) {
      console.log(val, 'val')
      // this.radio = val
    },
    onRowClick (row, column) {
      let { index } = row
      let radioKey = row.radioKey
      this.radio = row[radioKey]
      this.currentRow = row
      console.log(index, column, 'row click', radioKey, this.radio, Object.keys(row))
    },
    getCurrentRow () {
      return this.currentRow
    },
    tableRowClassName ({ row, rowIndex }) {
      // 把每一行的index加到row中
      row.index = rowIndex
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