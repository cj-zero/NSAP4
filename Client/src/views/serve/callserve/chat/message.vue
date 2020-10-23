<template>
  <div style="width:100%;">
    <template v-if="wordList && wordList.length">
      <el-form label-position="top" label-width="80px" class="chatForm">
        <el-form-item label="留言">
          <ul max-height="200px">
            <li v-for="tValue in wordList" :key="tValue.id">
              <div v-if="userType!==tValue.replier" class="otherWord">
                <p>
                  {{tValue.replier?tValue.replier:'未知发送者'}}
                  <span>{{tValue.createTime}}</span>
                </p>
                <template v-for="(content, index) in tValue.content">
                  <p class="text" v-if="content" :key="index">{{ content }}</p>
                </template>
              </div>
              <div v-else class="ownWord">
                <p style="text-align:right;">
                  <span>{{tValue.createTime}}</span>
                  {{tValue.replier}}
                </p>
                <template v-for="(content, index) in tValue.content">
                  <p class="text" v-if="content" :key="index">{{ content }}</p>
                </template>
              </div>
            </li>
          </ul>
        </el-form-item>
      </el-form>
    </template>
    <template v-else>暂无留言噢~~</template>
    <div class="content-wrapper">
      <p class="title">留言</p>
      <el-input type="textarea" v-model="content" size="mini" :rows="2"></el-input>
      <div class="btn-wrapper">
        <el-button type="success" size="mini" @click="dialogVisible=true">上传图片</el-button>
        <el-button type="primary" size="mini" @click="submitForm()" :loading="loadingBtn">确定</el-button>
      </div>
    </div>
    <el-dialog title="提示" :visible.sync="dialogVisible" :append-to-body="true" width="500px">
      <el-row :gutter="10" type="flex" style="margin:0 0 10px 0 ;" class="row-bg">
        <el-col :span="4" style="line-height:40px;">
          <div style="font-size:12px;color:#606266;width:100px;">上传图片</div>
        </el-col>
        <el-col :span="18">
          <upLoadImage setImage="50px" @get-ImgList="getImgList"></upLoadImage>
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

export default {
  props: ["serveId"],
  components: { upLoadImage },
  data() {
    return {
      wordList: [],
      content: "", // 输入内容
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
      loadingBtn: false
    };
  },
  mounted() {
    this.getList();
    this.getInfo();
  },
  watch: {
    serveId: {
      handler() {
        this.getList()
      },
    },
  },
  methods: {
    getList() {
      callserve
        .GetServiceOrderMessages({ serviceOrderId: this.serveId })
        .then((res) => {
          this.wordList = this._normalizeWordList(res.result);
        }).catch(() => {
          this.$message.error('加载留言列表失败')
        })
    },
    getInfo() {
      callserve
        .GetUserProfile(this.tokenValue)
        .then((res) => {
          this.userType = res.result.name;
        })
        .catch((error) => {
          console.log(error);
        });
    },
    _normalizeWordList (wordList) {
      return wordList.map(item => {
        item.content = item.content.replace(/\n/g, '<br>')
        item.content = item.content.split('<br>')
        return item
      })
    },
    getImgList(val) {
      //获取图片列表
      this.serviceOrderMessagePictures = val;
    },
    submitForm() {
      if (!this.content.trim()) {
        return this.$message.error("留言内容不能为空");
      }
      this.loadingBtn = true
      callserve
        .SendMessageToTechnician({
          content: this.content,
          serviceOrderMessagePictures: this.serviceOrderMessagePictures,
          serviceOrderId: this.serveId
        })
        .then((res) => {
          if (res.code == 200) {
            this.serviceOrderMessagePictures = []
            this.content = ''
            this.$message({
              type: "success",
              message: "发送成功",
            });
            this.loadingBtn = false
            this.getList();
          } else {
            this.loadingBtn = false
            this.$message({
              type: "warning",
              message: `${res}`,
            });
          }
        })
        .catch((res) => {
          this.loadingBtn = false
          this.$message({
            type: "error",
            message: `${res}`,
          });
        });
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
.content-wrapper {
  .title {
    margin-bottom: 10px;
  }
  .btn-wrapper {
    margin-top: 10px;
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
        // color: #409eff;
      }
      .text {
        color: #409eff;
      }
    }
    .ownWord {
      margin: 5px 0;
      border: 2px solid white;
      border-radius: 5px;
      padding: 5px;
      text-align: right;
      p:nth-child(2) {
        // color: #67c23a;
      }
      .text {
        // color: #67c23a;
      }
    }
  }
}
</style>
