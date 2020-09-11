<template>
  <div class="search-wrapper">
    <template v-for="(item, index) in config">
      <template v-if="!item.type">
        <el-input
          class="filter-item"
          :style="{ width: item.width + 'px' }"
          v-model="listSearchQuery[item.prop]" 
          :key="index"
          @keyup.enter.native="onSearch"
          :placeholder="item.placeholder"
          :size="item.size || 'mini'"
          :disabled="item.disabled"
        ></el-input>
      </template>
      <template v-else-if="item.type === 'select'">
        <el-select 
          class="filter-item"
          clearable
          :style="{ width: item.width + 'px' }"
          :key="index"
          v-model="listSearchQuery[item.prop]" 
          :placeholder="item.placeholder"
          :size="item.size || 'mini'"
          :disabled="item.disabled">
          <el-option
            v-for="(item,index) in item.options"
            :key="index"
            :label="item.label"
            :value="item.value"
          ></el-option>
        </el-select>
      </template>
      <template v-else-if="item.type === 'tree'">
        <el-cascader
          :key="index"
          class="filter-item"
          :style="{ width: item.width + 'px' }"
          :props="{ value:'id',label:'name',children:'childTypes',expandTrigger: 'hover', emitPath: false }"  
          clearable
          :placeholder="item.placeholder"
          v-model="listSearchQuery[item.prop]"
          :options="item.options"
          :size="item.size || 'mini'"></el-cascader>
      </template>
      <template v-else-if="item.type === 'date'">
        <div class="filter-item" :key="index">
          <el-date-picker
            :value-format="item.valueFormat || 'yyyy-MM-dd'"
            :type="item.dateType || 'date'"
            :placeholder="item.placeholder"
            v-model="listSearchQuery[item.prop]"
            :size="item.size || 'mini'"
          ></el-date-picker>
          <!-- <span class="filter-item" v-if="item.showText">至</span> -->
        </div>
      </template>
      <template v-else-if="item.type === 'search'">
        <el-button
          :key="index"
          class="filter-item"
          @click="onSearch(item)"
          icon="el-icon-search"
          :size="item.size || 'mini'">
          {{ item.btnText ? item.btnText : '查询' }}
        </el-button>
      </template>
    </template>
  </div>
</template>

<script>
import { deepClone } from '@/utils'
export default {
  components: {},
  props: {
     listQuery: {
        type: Object,
        default () {
          return {}
        }
      },
      config: {
        type: Array,
        default () {
          return []
        }
      }
  },
  data () {
    return {
      listSearchQuery: {}
      // { placeholder, width, size, eventName: 'enter', type, prop }
    }
  },
  watch: {
    listQuery: {
      immediate: true,
      handler (val) {
        this.listSearchQuery = deepClone(val)
      },
      deep: true
    },
    listSearchQuery: {
      deep: true,
      handler (val) {
        this.$emit('changeForm', val)
      }
    }
  },
  methods: {
    onSearch (item) {
      console.log(this.listSearchQuery, 'search')
      let { btnText } = item
      btnText === '高级查询' 
        ? this.$emit('advanced')
        : this.$emit('search', this.listSearchQuery)
    }
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.search-wrapper {
  display: inline-block;
}
</style>