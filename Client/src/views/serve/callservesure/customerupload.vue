<template>
  <div>
    <el-form 
      label-width="100px"  
      disabled :model="form" 
      class="demo-form-inline rowStyle"
      size="mini">
      <div
        style="font-size:22px;text-align:center;padding-bottom:10px ; margin-bottom:10px ;border-bottom:1px solid silver;"
      >服务申请信息</div>
      <el-row type="flex" class="row-bg" justify="space-around">
        <el-col :span="8">
          <el-form-item label="客户代码">
            <el-input v-model="form.customerId"></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="8">
          <el-form-item label="服务单号">
            <el-input v-model="form.u_SAP_ID"></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="8">
          <el-form-item label="创建时间">
            <el-input v-model="form.createTime"></el-input>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" class="row-bg" justify="space-around">
        <el-col :span="16">
          <el-form-item label="客户名称">
            <el-input v-model="form.customerName"></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="8">
          <el-form-item label="联系人">
            <el-input v-model="form.newestContacter"></el-input>
          </el-form-item>
        </el-col>
        <!-- <el-col :span="8">
          <el-form-item label="电话号码">
            <el-input v-model="form.newestContactTel"></el-input>
          </el-form-item>
        </el-col> -->
      </el-row>
      <el-row type="flex" class="row-bg" justify="space-around">
        <el-form-item label="问题类型">
          <el-input v-model="form.newestContacter"></el-input>
        </el-form-item>
      </el-row>
      <el-row type="flex" class="row-bg" justify="space-around">
        <el-col :span="8">
          <!-- <el-form-item label="地址">
            <el-input v-model="form.city"></el-input>
          </el-form-item> -->
                <el-form-item label="现地址">
                    <!-- <el-input size="mini" v-model="form.city"></el-input> -->
                    <p
                      style="border: 1px solid silver;color:silver; border-radius:5px;line-height:30px;overflow:hidden;height:30px;background-color:#F5F7FA;margin:0;padding-left:10px;font-size:12px;"
                    >{{allArea}}</p>
                  </el-form-item>
        </el-col>
        <el-col :span="16">
          <el-form-item label>
            <el-input v-model="form.addr"></el-input>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" class="row-bg" justify="space-around">
        <el-col :span="24">
          <el-form-item label="服务内容">
            <el-input type="textarea" v-model="form.services"></el-input>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row
        type="flex"
        class="row-bg"
        justify="space-around"
        v-for=" (item,index) in form.serviceOrderSNs"
        :key="`inx${index}`"
      >
        <el-col>
          <el-row type="flex" class="row-bg" justify="space-around">
            <el-col>
              <el-form-item label="制造商序列号">
                <el-input v-model="item.manufSN" style="width: 130px;"></el-input>
                <!-- <div style="border:1px solid #E4E7ED;background-color:#F5F7FA;border-radius:5px;padding-left:5px;">{{item.manufSN?item.manufSN:'暂无数据'}}</div> -->
              </el-form-item>
            </el-col>
            <el-col>
              <el-form-item label="物料编码">
                <el-input v-model="item.itemCode" style="width: 150px;"></el-input>
              </el-form-item>
            </el-col>
      
          </el-row>
        </el-col>
        <el-col :span="8">
          <el-form-item label>
            <span></span>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row
        v-if="serviceOrderPictures && serviceOrderPictures.length"
        :gutter="10"
        type="flex"
        style="margin:0 0 10px 0 ;"
        class="row-bg"
      >
        <el-col :span="1" style="line-height:40px;"></el-col>
        <el-col :span="2" style="line-height:40px;">
          <div style="font-size:12px;color:#606266;width:120px;">已上传图片</div>
        </el-col>
        <el-col :span="20" v-if="serviceOrderPictures&&serviceOrderPictures.length">
          <div class="demo-image__lazy">
            <div class="img-list"
              v-for="url in serviceOrderPictures"
              :key="url.id">
              <el-image
                style="width:60px;height:50px;display:inline-block;"  
                :src="`${baseURL}/${url.pictureId?url.pictureId:url.id}?X-Token=${tokenValue}`"
                lazy
                >
              </el-image>
              <div class="operation-wrapper">
                <i class="el-icon-zoom-in" @click="handlePreviewFile(`${baseURL}/${url.pictureId?url.pictureId:url.id}?X-Token=${tokenValue}`)"></i>
                <i class="el-icon-download" @click="downloadFile(`${baseURL}/${url.pictureId?url.pictureId:url.id}?X-Token=${tokenValue}`)"></i>
              </div>
            </div>
          </div>
        </el-col>
      </el-row>
    </el-form>
    <Model
      :visible="previewVisible"
      @on-close="previewVisible = false"
      ref="formPreview"
      width="600px"
      form
    >
      <img :src="previewUrl" alt style="display: block;width: 80%;margin: 0 auto;" />
      <template slot="action">
        <el-button size="mini" @click="previewVisible = false">关闭</el-button>
      </template>
    </Model>
  </div>
</template>

<script>
import { download } from '@/utils/file'
import Model from "@/components/Formcreated/components/Model";
export default {
  components: {
    Model
  },
  // props: { form: { type: Object, default: () => ({}) } },
  props:{
    form: {
      type: Object,
      default () {
        return {}
      }
    },
    serviceOrderPictures: {
      type: Array,
      default () {
        return []
      }
    }
  },
  data() {
    return {
      formV: {
        name: "",
        cardCode: ""
      },
      baseURL: process.env.VUE_APP_BASE_API + '/files/Download',
      tokenValue: this.$store.state.user.token,
      previewUrl: "", //预览图片的定义
      previewVisible: false
    };
  },
  watch:{
    handler(val){
      console.log(val)
    }
  },
    computed: {
    allArea() {

      return this.form.province + this.form.city + this.form.area;
    },
  },
  methods: {
    downloadFile (url) {
      download(url)
    },
    handlePreviewFile(item) {
      //预览图片
      this.previewVisible = true;
      this.previewUrl = item;
    },
  }
};
</script>

<style lang="scss" scoped>
.rowStyle{
  ::v-deep .el-form-item{
    margin-bottom:5px;
  }
}
/* 图片样式 */
.demo-image__lazy {
  .img-list {
    position: relative;
    display: inline-block;
    width: 60px;
    height: 50px;
    margin: 0 10px;
    .operation-wrapper {
      position: absolute;
      display: flex;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      opacity: 0;
      justify-content: space-around;
      align-items: center;
      transition: opacity 0.5s;
      background-color: rgba(0, 0, 0, 5);
      .el-icon-download,
      .el-icon-zoom-in {
        color: white;
      }
    }
    &:hover {
      .operation-wrapper {
        opacity: 1;
      }
    }
  }
}
</style>