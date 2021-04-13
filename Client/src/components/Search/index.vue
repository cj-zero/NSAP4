<template>
  <div class="search-wrapper">
    <template v-for="(item, index) in config">
      <!-- 普通el-input -->
      <template v-if="!item.type">
        <el-input
          v-if="item.isShow === undefined ? true : item.isShow"
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
      <!-- 下拉选择 -->
      <template v-else-if="item.type === 'select'">
        <el-select 
          v-if="item.isShow === undefined ? true : item.isShow"
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
      <template v-else-if="item.type === 'upload'">
        <el-upload
          v-if="item.isShow === undefined ? true : item.isShow"
          v-bind="item.attrs"
          :key="index"
        >
          <slot>
            <el-button size="mini" type="primary">{{ item.attrs.btnText || '点击上传'}}</el-button>
          </slot>
        </el-upload>
      </template>
      <!-- 树状选择 -->
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
      <!-- 日期选择 -->
      <template v-else-if="item.type === 'date'">
        <div class="filter-item" :key="index">
          <el-date-picker
            v-if="item.isShow === undefined ? true : item.isShow"
            :style="{ width: item.width + 'px' }"
            :value-format="item.valueFormat || 'yyyy-MM-dd'"
            :type="item.dateType || 'date'"
            :placeholder="item.placeholder"
            :picker-options="pickerOptions"
            v-model="listSearchQuery[item.prop]"
            :size="item.size || 'mini'"
          ></el-date-picker>
        </div>
      </template>
      <!-- 查询按钮 -->
      <template v-else-if="item.type === 'search'">
        <el-button
          :key="index"
          class="filter-item"
          :class="{ 'customer-btn-class': item.isSpecial }"
          @click="onSearch(item)"
          icon="el-icon-search"
          :size="item.size || 'mini'">
          {{ item.btnText ? item.btnText : '查询' }}
        </el-button>
      </template>
      <template v-else-if="item.type === 'button'">
        <el-button
          v-if="item.isShow === undefined ? true : item.isShow"
          :key="index"
          class="filter-item"
          :style="item.style || {}"
          :class="{ 'customer-btn-class': item.isSpecial }"
          @click="item.handleClick({ listSearchQuery, ...(item.options || {}) })"
          :icon="item.icon || ''"
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
      listSearchQuery: {},
      // { placeholder, width, size, eventName: 'enter', type, prop }
      pickerOptions: {
        disabledDate: this.disabledDate
      }
    }
  },
  watch: {
    listQuery: {
      immediate: true,
      handler (val) {
        this.listSearchQuery = deepClone(val)
      },
      // deep: true
    },
    listSearchQuery: {
      deep: true,
      handler (val) {
        console.log(val, 'val')
        this.$emit('changeForm', val)
      }
    }
  },
  methods: {
    disabledDate (date) {
      return date.getTime() > Date.now()
    },
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
  display: inline;
}
</style>