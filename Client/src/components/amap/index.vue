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
import { remoteLoad } from '@/utils/remoteLoad.js'
// import { MapKey, MapCityName } from '@/config/config'
export default {
  props: ['lat', 'lng'],
  data () {
    return {
      searchKey: '',
      placeSearch: null,
      dragStatus: false,
      AMapUI: null,
      AMap: null,
      MapKey :'cfd8da5cf010c5f7441834ff5e764f5b',
      MapCityName : '中国'
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
      // 加载PositionPicker，loadUI的路径参数为模块名中 'ui/' 之后的部分
      let AMapUI = this.AMapUI = window.AMapUI
      let AMap = this.AMap = window.AMap
      // console.log(AMapUI, AMap, 'amp')
      AMapUI.loadUI(['misc/PositionPicker'], PositionPicker => {
        // console.log(mapConfig, 'before mapConfig')
        let mapConfig = {
          zoom: 16,
          cityName: this.MapCityName
        }
        // console.log(this.lat, this.lng, mapConfig, 'mapConfig')
        if (this.lat && this.lng) {
          mapConfig.center = [this.lng, this.lat]
        }
        // console.log(mapConfig, 'mapConfig')
        let map = new AMap.Map('js-container', mapConfig)
        // 加载地图搜索插件
        AMap.service('AMap.PlaceSearch', () => {
          this.placeSearch = new AMap.PlaceSearch({
            pageSize: 5,
            pageIndex: 1,
            citylimit: true,
            city: this.MapCityName,
            map: map,
            panel: 'js-result'
          })
        })
        // 启用工具条
        AMap.plugin(['AMap.ToolBar'], function () {
          map.addControl(new AMap.ToolBar({
            position: 'RB'
          }))
        })
        // 创建地图拖拽
        let positionPicker = new PositionPicker({
          mode: 'dragMap', // 设定为拖拽地图模式，可选'dragMap'、'dragMarker'，默认为'dragMap'
          map: map // 依赖地图对象
        })
        // 拖拽完成发送自定义 drag 事件
        positionPicker.on('success', positionResult => {
          // 过滤掉初始化地图后的第一次默认拖放
          if (!this.dragStatus) {
            this.dragStatus = true
          } else {
            this.allAddress = positionResult
            this.$emit('drag', positionResult)
          }
        })
        // 启动拖放
        positionPicker.start()
      })
    }
  },
  mounted () {
    // 已载入高德地图API，则直接初始化地图
    let that =this
    this.$nextTick(
      async function(){
        if (window.AMap && window.AMapUI) {
          //  await remoteLoad(`http://webapi.amap.com/maps?v=1.3&key=${this.MapKey}`)
          // await remoteLoad('http://webapi.amap.com/ui/1.0/main.js')
          this.initMap()
        // 未载入高德地图API，则先载入API再初始化
        } else {
          await remoteLoad(`http://webapi.amap.com/maps?v=1.3&key=${this.MapKey}`)
          await remoteLoad('http://webapi.amap.com/ui/1.0/main.js')
          setTimeout(function(){
              that.initMap()
          },300)
        }
      }
    )
  },
  created () {
//     // 已载入高德地图API，则直接初始化地图
//     let that =this
//    this.$nextTick(
//      function(){
//     if (window.AMap && window.AMapUI) {
//       //  await remoteLoad(`http://webapi.amap.com/maps?v=1.3&key=${this.MapKey}`)
//       // await remoteLoad('http://webapi.amap.com/ui/1.0/main.js')
//       this.initMap()
//     // 未载入高德地图API，则先载入API再初始化
//     } else {
//       remoteLoad(`http://webapi.amap.com/maps?v=1.3&key=${this.MapKey}`)
//       setTimeout(function(){
//           remoteLoad('http://webapi.amap.com/ui/1.0/main.js')
//       },200)
//         setTimeout(function(){
//            that.initMap()
//       },300)
    
//     }
//    }
//  )
  }
}
</script>

<style lang="css">
.m-map{ width: 0; height: 0; position: relative; opacity: 0; }
.m-map .map{ width: 100%; height: 100%; }
.m-map .search{ position: absolute; top: 10px; right: 30px; width: 285px; z-index: 1; }
.m-map .search input{ width: 180px; border: 1px solid #ccc; line-height: 20px; padding: 5px; outline: none; }
.m-map .search button{ line-height: 26px; background: #fff; border: 1px solid #ccc; width: 50px; text-align: center; }
.m-map .result{ max-height: 300px; overflow: auto; margin-top: 10px; }
</style>