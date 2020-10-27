<template>
  <div class="m-map" >
    <div class="search" v-if="placeSearch">
      <input type="text" placeholder="请输入关键字" v-model="searchKey">
      <button type="button" @click="handleSearch">搜索</button>
      <div id="js-result" v-show="searchKey" class="result"></div>
    </div>
    <div id="js-container" class="map">正在加载数据 ...</div>

  </div>
</template>

<script>
import { loadBMap } from '@/utils/remoteLoad.js'
export default {
  data () {
    return {
      searchKey: '',
      placeSearch: null,
      MapKey :'uGyEag9q02RPI81dcfk7h7vT8tUovWfG',
    }
  },
  watch: {
    searchKey () {
      if (this.searchKey === '') {
        this.placeSearch.clear()
      }
    }
  },
  methods: {
    // 搜索
    handleSearch () {
      if (this.searchKey) {
        this.placeSearch.search(this.searchKey)
      }
    },
    // 实例化地图
    initMap () {
      let BMap = this.BMap = window.BMap
      this.map = new BMap.Map("js-container");
      console.log(window.BMap, 'map', this.map)
      let point = new BMap.Point(116.404, 39.915);
      this.map.centerAndZoom(point, 15); 
      this.map.enableScrollWheelZoom() // 滚轮缩放
      this.$emit('mapInitail', {
        map: this.map,
        BMap: this.BMap
      })
    }
  },
  async mounted () {
    try {
      // await remoteLoad(`https://api.map.baidu.com/api?v=1.0&type=webgl&ak=${this.MapKey}`)
      await loadBMap(this.MapKey) //加载引入BMap
      this.initMap()
    } catch (err) {
      console.log(err)
    } 
  },
  created () {
  }
}
</script>

<style lang="css">
.m-map{ min-width: 0; height: 0; position: relative; opacity: 0; }
.m-map .map{ width: 100%; height: 100%; }
.m-map .search{ position: absolute; top: 10px; right: 30px; width: 285px; z-index: 1; }
.m-map .search input{ width: 180px; border: 1px solid #ccc; line-height: 20px; padding: 5px; outline: none; }
.m-map .search button{ line-height: 26px; background: #fff; border: 1px solid #ccc; width: 50px; text-align: center; }
.m-map .result{ max-height: 300px; overflow: auto; margin-top: 10px; }
</style>