<template>
  <div style="width:100%;" class="message-wrapper">
    <template v-if="wordList && wordList.length">
      <el-form label-position="top" label-width="80px" class="chatForm">
        <el-form-item :label="label">
          <el-scrollbar class="scroll-bar" v-loading="isLoading">
            <ul>
              <li 
                v-for="(tValue, index) in wordList" :key="tValue.id" 
                v-click-outside="hidePannel"
                @contextmenu.prevent="openMenu({ tValue, index }, $event)" ref="wordItem">
                <div class="otherWord">
                  <p class="content">
                    {{tValue.replier?tValue.replier:'未知发送者'}}
                    <span class="content">{{tValue.createTime}}</span>
                  </p>
                  <template v-for="(content, index) in tValue.content">
                    <p class="text" v-if="content" :key="index">{{ content }}</p>
                  </template>
                </div>
                <!-- <div v-else class="ownWord">
                  <p style="text-align:right;">
                    <span>{{tValue.createTime}}</span>
                    {{tValue.replier}}
                  </p>
                  <template v-for="(content, index) in tValue.content">
                    <p class="text" v-if="content" :key="index">{{ content }}</p>
                  </template>
                </div> -->
                <!-- 图片列表 -->
                <template v-if="tValue.serviceOrderMessagePictures && tValue.serviceOrderMessagePictures.length">
                  <img-list :imgList="tValue.serviceOrderMessagePictures"></img-list>
                </template>
              </li>
            </ul>
          </el-scrollbar>
          
        </el-form-item>
      </el-form>
    </template>
    <template v-else><div v-if="!type">暂无留言噢~~</div></template>
    <template v-if="!type">
      <el-form class="content-wrapper" :disabled="isDisabled">
        <p class="title">留言</p>
        <el-input type="textarea" v-model="content" size="mini" :rows="2"></el-input>
        <div class="btn-wrapper">
          <el-button type="success" size="mini" @click="dialogVisible=true">上传图片</el-button>
          <el-button type="primary" size="mini" @click="submitForm()" :loading="loadingBtn">确定</el-button>
        </div>
      </el-form>
      <div class="operation-wrapper" v-show="isVisible" :style="{ left: left + 'px', top: top + 'px' }">
        <div @click="withDraw">撤回</div>
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
    </template>
  </div>
</template>

<script>
import * as callserve from "@/api/callserve";
import upLoadImage from "@/components/upLoadFile";
import ImgList from '@/components/imgList'
import ClickOutside from 'element-ui/lib/utils/clickoutside'
export default {
  props: ["serveId", "type"],
  directives: {
    ClickOutside
  },
  components: { upLoadImage, ImgList },
  data() {
    return {
      isVisible: false,
      isLoading: false,
      left: 0,
      top: 0,
      isDisabled: false,
      wordList: [],
      content: "", // 输入内容
      baseURL: process.env.VUE_APP_BASE_API + "/files/Download",
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
        if (this.serveId) {
          this.getList()
        }
      },
    },
  },
  computed: {
    label () {
      return this.type ? '' : '留言'
    }
  },
  methods: {
    openMenu ({ tValue, index }, event) {
      if (this.type) return
      this.isVisible = true
      this.tValue = tValue
      this.index = index
      const offsetLeft = event.target.getBoundingClientRect().left; // container margin left
      const refOffset = this.$refs.wordItem[index].getBoundingClientRect().left
      console.log(refOffset === offsetLeft, tValue)
      this.left = event.clientX - 35; // 15: margin right
      this.top = event.clientY - 15;
    },
    hidePannel () {
      this.isVisible = false
    },
    async withDraw () {
      if (!this.tValue) {
        return this.$message.warning('没有消息ID')
      }
      this.isVisible = false
      try {
        this.isLoading = true
        this.isDisabled = true
        const data = await callserve.withDrawMessage({ messageId: this.tValue.id })
        console.log(data, 'data')
        this.wordList.splice(this.index, 1)
        this.$message.success(data.message)
        this.isLoading = false
        this.isDisabled = false
      } catch (err) {
        this.$message.error(err.message)
        this.isLoading = false
        this.isDisabled = false
      }
    },
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
        // item.serviceOrderMessagePictures = item.serviceOrderMessagePictures.map(item => {
        //   item.url = `${this.baseURL}/${item.id || item.fileId}?X-Token=${this.tokenValue}`
        // })
        return item
      })
    },
    getImgList(val) {
      //获取图片列表
      this.serviceOrderMessagePictures = val;
    },
    submitForm() {
      if (!this.content.trim() && !this.serviceOrderMessagePictures.length) {
        return this.$message.error("留言内容或图片不能为空");
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
            this.$message.error( `${res.message}`);
          }
        })
        .catch((res) => {
          this.loadingBtn = false
          this.$message.error( `${res.message}`);
        });
    },
  },
};
</script>

<style lang="scss" scoped>
.chatForm {
  padding-right: 13px !important;
  .scroll-bar {
    &.el-scrollbar {
       ::v-deep {
        .el-scrollbar__wrap {
          max-height: 400px; // 最大高度
          overflow-x: hidden; // 隐藏横向滚动栏
          margin-bottom: 0 !important;
        }
      }
    }
  }
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
.operation-wrapper {
  position: fixed;
  z-index: 10000;
  left: 0;
  top: 0;
  width: 60px;
  padding: 5px;
  background: #fff;
  border-radius: 4px;
  color: #333;
  box-shadow: 2px 2px 3px 0 rgba(0, 0, 0, 0.3);
  div {
    line-height: 30px;
    height: 30px;
    text-align: center;
    background-color: #eee;
    cursor: pointer;
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
      .content {
        font-size: 13px;
      }
      p:nth-child(2) {
        border-radius: 5px;
        padding: 2px;
        // color: #409eff;
      }
      .text {
        font-size: 12px;
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
