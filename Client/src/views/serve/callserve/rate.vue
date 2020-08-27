<template>
  <div class="rate-wrapper">
    <el-row 
      class="header-wrapper" 
      type="flex" 
      justify="center" 
      align="middle"
    >
      <h2>售后评价</h2>
    </el-row>
    <div class="content-wrapper">
      <el-row type="flex" class="product-wrapper">
        <span class="title">产品评价</span>
        <el-row
          type="flex"
          align="middle"
          class="product-rate-item"
          v-for="title in titleList"
          :key="title">
          <span>{{ ttile }}</span>
          <Rate />
        </el-row>
      </el-row>
      <el-scrollbar class="tech-wrapper" style="max-height: 400px;" >
        <ul class="tech-list">
          <li class="tech-item" v-for="item in techList" :key="item.name">
            <el-row type="flex" align="middle">
              <div class="tech-info">

              </div>
              <div class="tech-rate">

              </div>
            </el-row>
          </li>
        </ul>
        
      </el-scrollbar>
    </div>
  </div>
</template>

<script>
const titleList = ['响应速度', '服务价格']
const techRateList = ['响应速度', '方案有效性', '服务态度']
import Rate from '@/components/Rate'
export default {
  components: {
    Rate
  },
  props: {
    rateList: {
      type: Object,
      default () {
        return {}
      }
    }
  },
  data () {
    return {
      titleList,
      techRateList
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
.rate-wrapper {
  border: 1px solid;
  .header-wrapper {
    border-bottom: 1px solid;
  }
}
</style>