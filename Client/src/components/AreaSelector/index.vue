<template>
  <div class="area-selector-wrap">
    <i class="el-icon-close close-btn" @click="closeSelector"></i>
    <!-- 选项 -->
    <ul class="tab-list">
      <li 
        v-for="(item, index) in tabList" 
        :key="item.areaName" 
        @click="selectTab(item, index)"
        :class="{ active: index === activeIndex }"
      >{{ item.areaName }}</li>
    </ul>
    <!-- 选择列表 -->
    <el-scrollbar>
      <ul class="select-list">
        <li 
          v-for="item in selectList" 
          :key="item.areaName" 
          @click="selectItem(item)"
          :class="{ active: includesName(item.areaName) }">{{ item.areaName }}</li>
      </ul>
    </el-scrollbar>
  </div>
</template>

<script>
// import addressList from './address'
import { setLocalStorage, hasLocalStorage, setObject, getObject } from  '@/utils/storage'
import { getAreaList } from '@/api/serve/area'
export default {
  props: {
    options: {
      type: Object,
      default () {
        return {}
      }
    }
  },
  data () {
    return {
      addressList: [],
      tabList: [{
        areaName: '请选择'
      }], // 所有选项卡的列表
      selectList: [], // 当前选择的列表
      currentTab: '', // 当前选中的选项卡
      overseasList: [], // 海外国家列表
      activeIndex: 0, // 当前选中的选项卡
      currentIndex: 0,
      currentItem: {}, // 当前选择的数据
      province: '', // 对应level2
      city: '', // 对应level3
      district: '', // 街道 对应level4
      currentLevel: 1, // 默认level1
      isFirst: true // 默认是第一次加载
    }
  },
  methods: {
    selectTab (item, index) {
      let { pid } = item
      console.log(item, 'item')
      this.activeIndex = index
      this.currentIndex = index
      this._normalizeAddressList(pid)
      console.log(item)
    },
    selectItem (item) { 
      let { areaName, areaLevel } = item
      this.currentItem = item
      if (this.currentIndex === 0) {
        this.tabList[0].pid = ''
      }
      this.tabList[this.currentIndex].areaName = areaName
      this.handleSelect(areaName)
      if (Number(areaLevel) === 3) {
        this.$emit('change', {
          province: this.province || '',
          city: this.city || '',
          district: this.district || '',
          ...this.options
        })
        return this.closeSelector()
      }
      console.log(item)
    },
    handleSelect (areaName) {
      // console.log(this.currentItem, level, 'handleSelect')
      switch (this.currentIndex) {
        case 0:
          this.province = areaName
          break
        case 1:
          this.city = areaName
          break
        case 2:
          this.district = areaName
          break
        default:
          break
      }
    },
    includesName (name) {
      let { province, city, district } = this
      return [province, city, district].includes(name)
    },
    closeSelector () {
      this.$emit("close", this.options)
    },
    _normalizeAddressList (id, isReset) { // id: 根据id查询省市区 isRest: 根据省市区是否发生变化
      if (getObject('addressInfo', id)) { // 如果数据已经缓存则直接取缓存的数据
        this.selectList = getObject('addressInfo', id)
        if (isReset && Number(this.currentItem.areaLevel) !== 3) {
          this.tabList.push({
            areaName: '请选择',
            pid: id
          })
          this.activeIndex++
          this.currentIndex++ 
        }
      } else {
        getAreaList({
          ReqId: id
        }).then(res => {
          this.selectList = res.data
          setObject('addressInfo', id, this.selectList)
          if (isReset && Number(this.currentItem.areaLevel) !== 3) {
            this.tabList.push({
              areaName: '请选择',
              pid: id
            })
            this.activeIndex++
            this.currentIndex++ 
          }
          this.isFirst = false
        })
      }
    }
  },
  watch: {
    province () {
      let { id, areaLevel } = this.currentItem
      if (Number(areaLevel) !== 3) {
        if (this.tabList.length >= 2) {
          this.tabList = this.tabList.slice(0, 1)
          this.city = ''
          this.district = ''
        }
        this._normalizeAddressList(id, true)
      }
    },
    city (val) {
      let { id, areaLevel } = this.currentItem
      if (Number(areaLevel) !== 3 && val) { 
        if (this.tabList.length >= 3) {
          console.log('ciry', this.tabList.slice(0, 2))
          this.tabList = this.tabList.slice(0, 2)
          this.district = ''
          console.log(this.tabList, 'tabList')
        }
        this._normalizeAddressList(id, true)
      }
    },
    district (val) {
      let { id, areaLevel } = this.currentItem
      if (Number(areaLevel) !== 3 && val) { 
        if (this.tabList.length >= 4) {
          this.tabList = this.tabList.slice(0, 4)
        }
        this._normalizeAddressList(id, true)
      }
    }
  },
  created () {
    if (!hasLocalStorage('addressInfo')) { // 缓存地址信息
      setLocalStorage('addressInfo', {})
    }
    this._normalizeAddressList('')
  },
  mounted () {
    
  },
}
</script>
<style lang='scss' scoped>
.area-selector-wrap {
  position: relative;
  box-sizing: border-box;
  overflow: hidden;
  width: 640px;
  margin: 10px 0;
  padding: 10px;
  border: 1px solid #e4e7ed;
  border-radius: 4px;
  background-color: #fff;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 1);
  font-size: 12px;
  .close-btn {
    position: absolute;
    right: 10px;
    cursor: pointer;
  }
  & > ul {
    list-style-type: none;
    padding: 0;
    margin: 0;
  }
  .tab-list {
    margin-top: 5px;
    // border-bottom: 2px solid #e4393c;
    & > li {
      display: inline-block;
      height: 30px;
      padding: 0 10px;
      line-height: 30px;
      text-align: center;
      font-size: 12px;
      border: 1px solid #dcdfe6;
      &:hover {
        cursor: pointer;
      }
      &.active {
        color: rgba(0, 90, 160, 1);
        border-bottom: 1px solid transparent;
      }
    }
  }
  ::v-deep .el-scrollbar {
    .el-scrollbar__wrap {
      max-height: 300px !important; // 最大高度
      overflow-x: hidden; // 隐藏横向滚动栏
      margin-bottom: 0 !important;
    }
  }
  .select-list {
    display: flex;
    flex-wrap: wrap;
    // overflow-y: auto;
    // max-height: 300px;
    & > li {
      width: 25%;
      white-space: nowrap;
      line-height: 25px;
      &:hover {
        color: rgba(205, 49, 40, 1);
        cursor: pointer;
      }
      &.active {
        color: rgba(205, 49, 40, 1);
      }
    }
  }
}
</style>