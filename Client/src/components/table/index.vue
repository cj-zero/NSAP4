<template>
  <el-table :data="data" v-loading="loading" size="mini">
    <column-item 
      v-for="item in columns"
      :key="item.prop"
      :columns="item"
    ></column-item>
    <!-- <el-table-column
      v-for="item in columns"
      :key="item.prop"
      :width="item.width"
      :label="item.label"
      :align="item.align || 'left'"
      :sortable="item.isSort || false"
    >
      <template slot-scope="scope" >
        <div class="link-container" v-if="item.type === 'link'">
          <img :src="rightImg" @click="item.handleJump(scope.row)" class="pointer">
          <span>{{ scope.row[item.prop] }}</span>
        </div>
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
    </el-table-column> -->
    
  </el-table>
</template>

<script>
import rightImg from '@/assets/table/right.png'
import ColumnItem from './column-item'
export default {
  components: {
    ColumnItem
  },
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
    }
  },
  watch: {
    data: {
      immediate: true,
      handler (val) {
        console.log('table', val)
      }
    },
    columns: {
      immediate: true,
      handler (val) {
        console.log('columns', val)
      }
    }  
  },
  data () {
    return {
      rightImg
    }
  },
  methods: {

  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
</style>