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
              <Rate :score="newData.quality" type="quality" @change="onChange"/>
            </el-row>
          </el-col>
          <el-col :span="8"> 
            <el-row type="flex" align="middle">
              <span class="product-item">产品价格</span>
              <Rate :score="newData.price" type="price" @change="onChange" />
            </el-row>
          </el-col>
        </el-row>
      </el-row>
      <!-- 技术人员评价 -->
      <div class="rate-list">
        <el-row 
          type="flex" 
          align="middle"
          v-for="(item, index) in newData.rateList"
          :key="item.name">
          <div class="avatar-wrapper">
            <div class="avatar" :style="`background-image: url(${item.src});`"></div>
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
                <Rate @change="onChange" type="xy" :index="index" :score="item.xy" />
              </el-row>
            </el-col>
            <el-col>
              <el-row type="flex" align="middle">
                <span class="title">方案有效性</span>
                <Rate @change="onChange" type="fa" :score="item.fa" />
              </el-row>
            </el-col>
            <el-col>
              <el-row type="flex" align="middle">
                <span class="title">服务态度</span>
                <Rate @change="onChange" type="fw" :score="item.fw" />
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
const rateList = []
for (let i = 0; i < 10; i++) {
  rateList.push({
    src: 'http://192.168.1.207:52789/api/files/Download/084569cc-c152-4f04-91a3-8363a108c726?X-Token=3ff24de6', 
    name: 'lxb' + i,
    xy: Math.floor(Math.random() * 6),
    fa: Math.floor(Math.random() * 6),
    fw: Math.floor(Math.random() * 6)
  })
}
export default {
  components: {
    Rate
  },
  data () {
    return {
      data: {
        quality: 3,
        price: 1,
        rateList
      },
      isEdit: true,
      currentIndex: -1,
      type: ''
    }
  },
  computed: {
    newData () {
      return JSON.parse(JSON.stringify(this.data))
    }
  },
  methods: {
    onChange (val) {
      if (this.currentIndex !== -1) {
        this.$set(this.newData[this.currentIndex], this.type, val)
      } else {
        this.$set(this.newData, this.type, val)
      }
      console.log(val, 'after', this.newData)
    },
    onUpdate ({ val, type }) {
      console.log(val, 'update', type)
      if (this.currentIndex !== -1) {
        this.$set(this.newData[this.currentIndex], type, val)
      } else {
        this.$set(this.newData, type, val)
      }
      console.log(this.newData, 'newData')
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