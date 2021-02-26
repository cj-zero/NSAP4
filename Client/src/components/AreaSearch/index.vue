<template>
  <el-autocomplete
    size="mini"
    v-model="value"
    :fetch-suggestions="querySearchAsync"
    placeholder="请输入内容"
    @select="handleSelect"
    v-infotooltip:200.start-top
  ></el-autocomplete>
</template>

<script>
import { searchAreaList } from '@/api/serve/area'
export default {
  data() {
    return {
      value: '',
      areaList: [],
      cancelRequestFn: null
    };
  },
  watch: {
    value () {
      console.log(this.value, 'value')
    }
  },
  methods: {
    async querySearchAsync(queryString, cb) {
      if (!queryString) {
        return cb([])
      }
      try {
        if (this.cancelRequestFn) {
          this.cancelRequestFn()
        }
        const areaList = (await searchAreaList({ areaName: queryString }, this)).data
        console.log(areaList, 'areaList')
        if (!areaList || !areaList.length) {
          return cb([])
        }
        const result = this.normalizeAreaList(areaList)
        console.log(result)
        
        cb(result)
      } catch (err) {
        this.$message.error(err.message)
      }
    },
    normalizeAreaList (areaList) {
      return areaList.map(areaItem => {
        const newItem = { value : '' }
        areaItem.forEach(area => {
          newItem.value += area.areaName
          newItem.areaList = areaItem
        })
        return newItem
      })
    },
    handleSelect(item) {
      console.log(item, 'item')
      this.$emit('selected', item)
    }
  },
  mounted() {
    
  }
}
</script>