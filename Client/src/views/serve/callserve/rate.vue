<template>
  <div class="rate-wrapper">
    <div class="mask"></div>
    <header class="title">售后评价</header>
    <div class="rate-content-wrapper">
      <!-- 产品评价 -->
      <el-row type="flex" align="middle" class="product-wrapper">
        <span class="product-title">产品评价</span>
        <el-row type="flex" align="middle" style="flex: 1;">
          <el-col :span="8"> 
            <el-row type="flex" align="middle">
              <span class="product-item">产品质量</span>
              <Rate :score="newData.productQuality" type="productQuality" @change="onChange" :disabled="isView" ref="rateProduct" />
            </el-row>
          </el-col>
          <el-col :span="8"> 
            <el-row type="flex" align="middle">
              <span class="product-item">产品价格</span>
              <Rate :score="newData.servicePrice" type="servicePrice" @change="onChange" :disabled="isView" ref="ratePrice" />
            </el-row>
          </el-col>
        </el-row>
      </el-row>
      <!-- 技术人员评价 -->
      <div class="rate-list">
        <el-row 
          type="flex" 
          align="middle"
          v-for="(item, index) in newData.technicianEvaluates"
          :key="item.name">
          <div class="avatar-wrapper">
            <!-- <div class="avatar" :style="`background-image: url(${item.src});`"></div> -->
            <span class="name">{{ item.name }}</span>
          </div>
          <el-row 
            class="rate-item"
            style="flex: 1" 
            type="flex" 
            align="middle">
            <el-col>
              <el-row type="flex" align="middle">
                <span class="title">响应速度</span>
                <Rate @change="onChange" type="responseSpeed" :index="index" :score="item.responseSpeed" :disabled="isView" ref="rate" />
              </el-row>
            </el-col>
            <el-col>
              <el-row type="flex" align="middle">
                <span class="title">方案有效性</span>
                <Rate @change="onChange" type="schemeEffectiveness" :index="index" :score="item.schemeEffectiveness" :disabled="isView" ref="rate" />
              </el-row>
            </el-col>
            <el-col>
              <el-row type="flex" align="middle">
                <span class="title">服务态度</span>
                <Rate @change="onChange" type="serviceAttitude" :index="index" :score="item.serviceAttitude" :disabled="isView" ref="rate" />
              </el-row>
            </el-col>
          </el-row>
        </el-row>
      </div>
    </div>
  </div>
</template>

<script>
import Rate from '@/components/Rate'
export default {
  props: {
    data: {
      type: Object,
      default () {
        return {}
      }
    },
    isView: {
      type: Boolean,
      default: false
    }
  },
  components: {
    Rate
  },
  data () {
    return {
    }
  },
  computed: {
    newData () {
      return JSON.parse(JSON.stringify(this.data))
    }
  },
  methods: {
    onChange (data) {
      let { index, val, type } = data
      if (index !== -1) {
        this.$set(this.newData.technicianEvaluates[index], type, val)
      } else {
        this.$set(this.newData, type, val)
      }
      this.$emit('changeComment', this.newData)
      console.log(val, 'after', this.newData, this.data)
    }
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.rate-wrapper {
  padding: 10px;
  & > .title {
    height: 40px;
    font-size: 20px;
    text-align: center;
    line-height: 40px;
    border: 1px solid silver;
    font-weight: bold;
  }
  .rate-content-wrapper {
    box-sizing: border-box;
    padding: 20px;
    border: 1px solid silver;
    border-top: none;
    .product-wrapper {
      .product-title {
        width: 90px;
        margin-right: 30px;
        text-align: center;
        font-size: 18px;
        font-weight: bold;
      }
      .product-item {
        margin-right: 15px;
      }
    }
    .rate-list {
      overflow-y: scroll;
      max-height: 400px;
      margin-top: 20px;
      &::-webkit-scrollbar {
        display: none;
      }
      .avatar-wrapper {
        display: flex;
        flex-direction: column;
        align-items: center;
        width: 90px;
        margin-right: 30px;
        .avatar {
          width: 40px;
          height: 40px;
          border-radius: 50%;
          background-size: 100% auto;
        }
        .name {
          margin-top: 10px;
        }
      }
      .rate-item {
        margin: 10px 0;
        padding: 20px;
        border: 1px solid silver;
      } 
    }
  }
}
</style>