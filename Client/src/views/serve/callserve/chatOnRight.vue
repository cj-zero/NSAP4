<template>
  <div style="width:100%;">
    <el-collapse accordion>
      <el-collapse-item
        :title="`技术员：${item.key}`"
        :key="`key_${index}`"
        v-for="(item, index) in wordList"
        :name="index"
      >
        <el-form label-position="top" label-width="80px" :model="item" class="chatForm">
          <el-form-item label="留言">
            <!-- <el-input v-model="item.says" disabled></el-input> -->
            <ul max-height="200px">
              <li v-for="tValue in item.data" :key="tValue.id">
                <div v-if="userType!==tValue.replier" class="otherWord">
                  <p>
                    {{tValue.replier?tValue.replier:'未知发送者'}}
                    <span>{{tValue.createTime}}</span>
                  </p>
                  <p>{{tValue.content}}</p>
                </div>
                <div v-else class="ownWord">
                  <p style="text-align:right;">
                    <span>{{tValue.createTime}}</span>
                    {{tValue.replier}}
                  </p>
                  <p>{{tValue.content}}</p>
                </div>
              </li>
            </ul>
          </el-form-item>
          <el-form-item label="回复">
            <el-input type="textarea" v-model="item.content" size="mini"></el-input>
          </el-form-item>
          <el-button type="success" size="mini" @click="dialogVisible=true">上传图片</el-button>
          <el-button type="primary" size="mini" @click="submitForm(item)">确定</el-button>
        </el-form>
      </el-collapse-item>
    </el-collapse>
    <el-dialog title="提示" :visible.sync="dialogVisible" :append-to-body="true" width="500px">
      <el-row :gutter="10" type="flex" style="margin:0 0 10px 0 ;" class="row-bg">
        <el-col :span="4" style="line-height:40px;">
          <div style="font-size:12px;color:#606266;width:100px;">上传图片</div>
        </el-col>
        <el-col :span="18">
          <upLoadImage setImage="100px" @get-ImgList="getImgList"></upLoadImage>
        </el-col>
      </el-row>
      <span slot="footer" class="dialog-footer">
        <el-button @click="dialogVisible = false">取 消</el-button>
        <el-button type="primary" @click="function(){dialogVisible = false,changeModel=true}">确 定</el-button>
      </span>
    </el-dialog>
  </div>
</template>

<script>
import * as callserve from "@/api/callserve";
import upLoadImage from "@/components/upLoadFile";
// import { mapState } from "vuex";

export default {
  props: ["serveId"],
  components: { upLoadImage },
  data() {
    return {
      wordList: [],
      baseURL: process.env.VUE_APP_BASE_API,
      tokenValue: this.$store.state.user.token,
      previewUrl: "", //预览图片的定义
      serviceOrderMessagePictures: [],
      changeModel: false,
      previewVisible: false, //图片预览的dia
      dialogVisible: false,
      userType: "",
      formValue: {
        froTechnicianName: "",
        froTechnicianId: "",
        serviceOrderId: 0,
        content: "",
        appUserId: 0,
        // serviceOrderMessagePictures: [
        //   {
        //     serviceOrderMessageId: "",
        //     pictureId: "",
        //     id: "",
        //   },
        // ],
      },
    };
  },
  mounted() {
    this.getList();
    this.getInfo();
  },
  // computed:mapState([
  //  "count"
  // ]),
  watch: {
    serveId: {
      handler(val) {
        callserve
          .GetServiceOrderMessages({ serviceOrderId: val })
          .then((res) => {
            this.wordList = res.result;
            console.log(res.result);
          });
      },
    },
  },
  methods: {
    getList() {
      callserve
        .GetServiceOrderMessages({ serviceOrderId: this.serveId })
        .then((res) => {
          this.wordList = res.result;
        });
    },
    getInfo() {
      // return new Promise((resolve,reject)=>{
      callserve
        .GetUserProfile(this.tokenValue)
        .then((res) => {
          this.userType = res.result.name;
        })
        .catch((error) => {
          console.log(error);
        });
      // })
    },
    getImgList(val) {
      //获取图片列表
      this.formValue.serviceOrderMessagePictures = val.map((item) => {
        item.id = item.pictureId;
        return item;
      });
    },
    submitForm(item) {
      this.formValue.froTechnicianName = item.data[0].froTechnicianName;
      this.formValue.froTechnicianId = item.data[0].froTechnicianId;
      this.formValue.serviceOrderId = item.data[0].serviceOrderId;
      this.formValue.content = item.content;
      this.formValue.appUserId = item.data[0].appUserId;
      if (
        !this.formValue.content &&
        !this.formValue.serviceOrderMessagePictures
      ) {
        this.$message({
          type: "warning",
          message: "你不能发送空消息",
        });
      } else {
        callserve
          .SendMessageToTechnician(this.formValue)
          .then((res) => {
            if(res.code==200){
     this.getList();
            this.$message({
              type: "success",
              message: "发送成功",
            });
            }else{
                  this.$message({
              type: "warning",
              message:  `${res}`,
            });
            }
          })
          .catch((res) => {
            this.$message({
              type: "error",
              message: `${res}`,
            });
          });
      }
    },
  },
};
</script>

<style lang="scss" scoped>
.chatForm {
  ::v-deep .el-form-item {
    margin-bottom: 6px;
    .el-form-item__label {
      line-height: 20px;
      font-size: 15px;
    }
    .el-form-item__content {
      line-height: 30px;
    }
  }
}

ul {
  font-size: 13px;
  margin-top: -5px;
  background-color: #ebeef5;
  list-style: none;
  border: 1px solid white;
  border-radius: 5px;
  padding: 5px;
  max-height: 400px;
  overflow-y: auto;
  li {
    width: 100%;
    p {
      margin: 0;
      line-height: 20px;
      font-size: x-small;
      span {
        font-size: 12px;
        margin: 0 5px;
        color: silver;
      }
    }
    .otherWord {
      margin: 5px 0;
      border: 2px solid white;
      border-radius: 5px;
      padding: 5px;
      p:nth-child(2) {
        border-radius: 5px;
        padding: 2px;
        color: #409eff;
      }
    }
    .ownWord {
      margin: 5px 0;
      border: 2px solid white;
      border-radius: 5px;
      padding: 5px;

      p:nth-child(2) {
        color: #67c23a;
      }
    }
  }
}
</style>
