<template>
  <div class="area-selector-wrap">
    <i class="el-icon-close close-btn" @click="closeSelector"></i>
    <!-- 选项 -->
    <ul class="tab-list">
      <li 
        v-for="(item, index) in tabList" 
        :key="item.name" 
        @click="selectTab(item, index)"
        :class="{ active: index === activeIndex }"
      >{{ item.name }}</li>
    </ul>
    <!-- 选择列表 -->
    <ul class="select-list">
      <li 
        v-for="item in selectList" 
        :key="item.name" 
        @click="selectItem(item)"
        :class="{ active: includesName(item.name) }">{{ item.name }}</li>
    </ul>
  </div>
</template>

<script>
import addressList from './address'
export default {
  components: {},
  data () {
    return {
      addressList: [],
      tabList: [], // 所有选项卡的列表
      selectList: [], // 当前选择的列表
      currentTab: '', // 当前选中的选项卡
      overseasList: [], // 海外国家列表
      activeIndex: 0, // 当前选中的选项卡
      currentIndex: 0,
      currentItem: {}, // 当前选择的数据
      province: '', // 对应level1
      city: '', // 对应level2
      district: '', // 街道 对应level3
      currentLevel: 1 // 默认level1
    }
  },
  methods: {
    _normalizeAddressList () {
      addressList.forEach(item => {
        let { name, childrens } = item
        if (name === '亚洲') {
          if (childrens.length) {
            childrens.forEach(item => {
              if (item.name === '中国') {
                if (item.childrens.length) {
                  this.selectList = this._normalizeChinaList(item)
                }
              } else {
                this.overseasList.push({
                  name: item.name,
                  childrens: [],
                  level: 2
                }) // 亚洲其他地区精确到国家即可
              }
            })
          }
        } else { // 海外只精确到国家即可
          if (childrens.length) {
            childrens.forEach(item => {
              this.overseasList.push({
                name: item.name,
                childrens: [],
                level: 2
              })
            })
          }
        }
      })
    },
    _normalizeChinaList (addressList, level = 1) {
      let { childrens } = addressList
      let result = []
      childrens.forEach(item => {
        let target = {
          name: item.name,
          level,
          childrens: []
        }
        if(item.childrens.length) {
          target.childrens = this._normalizeChinaList(item, level + 1)
        }
        result.push(target)
      })
      return result
    },
    selectTab (item, index) {
      let { childrens } = item
      console.log(item, 'item')
      this.selectList = childrens
      this.activeIndex = index
      this.currentIndex = index
      console.log(item)
    },
    selectItem (item) { 
      let { name } = item
      this.tabList[this.currentIndex].name = name
      this.handleSelect(item)
      console.log(item)
    },
    handleSelect (item) {
      let { level, name } = item
      this.currentItem = item
      console.log(this.currentItem, level, 'handleSelect')
      switch (level) {
        case 1:
          this.province = name
          break
        case 2:
          this.city = name
          break
        case 3:
          this.district = name
          break
        default:
          break
      }
      console.log(this.province)
    },
    includesName (name) {
      let { province, city, district } = this
      return [province, city, district].includes(name)
    },
    closeSelector () {}
  },
  watch: {
    province () {
      console.log('province')
      let { childrens } = this.currentItem
      if (this.tabList.length > 1) {
        this.tabList = this.tabList.slice(0, 1)
      }
      if (childrens.length) {
        this.tabList.push({
          name: '请选择',
          childrens
        })
        // this.tabList.splice(length - 1, 0, item) // 每次都往数组最后一项前面添加
        this.activeIndex++
        this.currentIndex++
        this.selectList = childrens
      }
    },
    city () {
      let { childrens } = this.currentItem
      if (this.tabList.length > 2) {
        this.tabList = this.tabList.slice(0, 2)
      }
      if (childrens.length) {
        this.tabList.push({
          name: '请选择',
          childrens
        })
        // this.tabList.splice(length - 1, 0, item) // 每次都往数组最后一项前面添加
        this.activeIndex++
        this.currentIndex++
        this.selectList = childrens
      }
    },
    district () {
      let { childrens } = this.currentItem
      if (this.tabList.length > 3) {
        this.tabList = this.tabList.slice(0, 3)
      }
      if (childrens.length) {
        this.tabList.push({
          name: '请选择',
          childrens
        })
        // this.tabList.splice(length - 1, 0, item) // 每次都往数组最后一项前面添加
        this.activeIndex++
        this.currentIndex++
        this.selectList = childrens
      }
    }
  },
  created () {
    this._normalizeAddressList()
    this.selectList.push({ // 添加海外
      name: '海外',
      childrens: this.overseasList,
      level: 1
    })
    this.tabList.push({
      name: '请选择',
      childrens: this.selectList,
      level: 1
    })
  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.area-selector-wrap {
  box-sizing: border-box;
  overflow: hidden;
  width: 400px;
  margin: 10px 0;
  padding: 10px;
  border: 1px solid #e4e7ed;
  border-radius: 4px;
  background-color: #fff;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 1);
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
        // border: 1px solid #dcdfe6;
        // border-left: 1px solid #dcdfe6;
        color: rgba(0, 90, 160, 1);
        border-bottom: 1px solid transparent;
      }
    }
  }
  .select-list {
    display: flex;
    flex-wrap: wrap;
    overflow-y: scroll;
    max-height: 300px;
    
    & > li {
      width: 25%;
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