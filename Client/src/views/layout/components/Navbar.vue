<template>
  <el-menu class="navbar" mode="horizontal">
    <hamburger class="hamburger-container" :toggleClick="toggleSideBar" :isActive="sidebar.opened"></hamburger>
		<div class="logo">
			<img class="user-avatar" :src="logo">
		</div>
    <el-dropdown class="avatar-container" @command="handleCommand" trigger="click" @visible-change="onVisibleChange">
      <div class="avatar-wrapper">
				欢迎您，{{name}}
        <i class="el-icon-caret-bottom"></i>
      </div>
      <el-dropdown-menu class="user-dropdown" slot="dropdown">
        <el-dropdown-item command="handleGoProfile">
					<span>个人中心</span>
				</el-dropdown-item>
        <!-- <el-dropdown-item>
					<span>切换主题 <el-switch :active-value="1" :inactive-value="0" style="margin-left: 5px;" v-model="theme" /></span>
				</el-dropdown-item> -->
        <el-dropdown-item command="logout" divided>
          <span>退出</span>
        </el-dropdown-item>
      </el-dropdown-menu>
    </el-dropdown>
    <el-dropdown class="notify-message" trigger="click" @visible-change="onMsgVisibleChange" :hide-on-click="false" ref="dropDown">
      <div class="notify-message">
        <i class="unread-number" v-show="notHasNumber">{{ notHasNumber }}</i>
        <div class="notify-icon-wrapper">
          <el-icon class="el-icon-close-notification" ></el-icon>
        </div>
      </div>
      <el-dropdown-menu :class="{ 'hidden-dropdown': !newMessageList.length }" class="notify-message-dropdown" slot="dropdown">
        <el-dropdown-item>
          <div class="global-message-list-wrapper" v-if="newMessageList.length">
            <div class="button-wrapper">
              <template v-for="(item, index) in messageBtnList">
                <span 
                  :key="item.text"
                  :class="{ active: index === currentIndex }"
                  @click="changeList(index)"
                >{{ item.text }}</span>
              </template>
            </div>
            <div class="empty-wrapper" v-if="!transformList.length">暂无数据</div>
            <el-scrollbar class="drop-scrollbar" v-else>
              <message-list :messageList="transformList" @hasRead="onHasRead" :isShow="isShowMessage" v-loading="messageLoading"></message-list>
            </el-scrollbar> 
          </div>
				</el-dropdown-item>
      </el-dropdown-menu>
    </el-dropdown>
    <my-dialog
      ref="serviceDetail"
      width="1210px"
      title="服务单详情"
      :mAddToBody="true" 
      :appendToBody="true"
    >
      <el-row :gutter="20" class="position-view">
        <el-col :span="18" >
          <zxform
            formName="查看"
            labelposition="right"
            labelwidth="72px"
            max-width="800px"
            :isCreate="false"
            :refValue="dataForm"
          ></zxform>
        </el-col>
        <el-col :span="6" class="lastWord">   
          <zxchat :serveId='serveId' :timer="timer" formName="查看"></zxchat>
        </el-col>
      </el-row>
    </my-dialog>
  </el-menu>
</template>

<script>
import { mapGetters, mapActions } from 'vuex'
import Hamburger from '@/components/Hamburger'
import MyDialog from '@/components/Dialog'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import logo from '@/assets/logo_new.png?imageView2/1/w/80/h/80'
import MessageList from './MessageList'
import { readMessage } from '@/api/message'
import { GetDetails } from '@/api/serve/callservesure'
import { findIndex } from '@/utils/process'
export default {
  props: {
    messageList: {
      type: Array,
      defualt: () => []
    }
  },
  data: function() {
    return {
      logo: logo,
      theme: 1,
      isShowMessage: false, // 是否展示未读消息
      newMessageList: [],
      messageLoading: false,
      serveId: '',
      timer: null, // 用来更新弹窗中的子组件
      dataForm: {}, //传递的表单props
      temp: {
        id: "", // Id
        sltCode: "", // SltCode
        subject: "", // Subject
        cause: "", // Cause
        symptom: "", // Symptom
        descriptio: "", // Descriptio
        status: "", // Status
        extendInfo: "" // 其他信息,防止最后加逗号，可以删除
      },
      messageBtnList: [
        { text: '未读消息', hasRead: false },
        { text: '已读消息', hasRead: true }
      ],
      currentIndex: 0, // 当前默认展示未读消息
      hasReadList: [], // 已读列表
      notHasReadList: [] // 未读列表
    }
  },
  components: {
    Hamburger,
    MessageList,
    MyDialog,
    zxform,
    zxchat
  },
  computed: {
    ...mapGetters(['sidebar', 'isIdentityAuth', 'name', 'themeStatus']),
    transformList () {
      return this.newMessageList.filter(item => item.HasRead === !!this.currentIndex)
    },
    notHasNumber () { // 未读消息
      return this.newMessageList.filter(item => !item.HasRead).length
    }
  },
  watch: {
    theme() {
      this.toggleClass(document.body, 'custom-theme')
    },
    messageList: {
      deep: true,
      immediate: true,
      handler (val) {
        console.log(val,'messageList')
        this.newMessageList = val
      }
    }
  },
  mounted() {
    // this.theme = Number(this.themeStatus)
    // this.toggleClass(document.body, 'custom-theme')
    // document.addEventListener('click', e => {
    //   let ele = e.target
    //   if (e.target.className !== 'notify-content-wrapper') {
    //     this.isShowMessage = false
    //   }
    // })
  },
  methods: {
    ...mapActions([
      'signOutOidc',
      'saveTheme'
    ]),
    changeList (index) {
      this.currentIndex = index
    },
    onVisibleChange (visible) {
      console.log(visible, 'visible')
      if (visible) {
        this.isShowMessage = false
        this.currentIndex = 0
      }
    },
    onMsgVisibleChange (visible) {
      this.isShowMessage = visible
      if (!this.isShowMessage) {
        this.currentIndex = 0
      }
    },
    onHasRead (data) {
      // 消息已读从列表中删除
      let { FroUserId, ServiceOrderId, index } = data
      console.log(FroUserId, ServiceOrderId, index, 'delete', readMessage)
      this.openTree(data)
      console.log(this.newMessageList, 'newmessage')
    },
    openTree({ ServiceOrderId, FroUserId, HasRead, U_SAP_ID }) {
      if (!ServiceOrderId) {
        return this.$message.error('无服务单ID')
      }
      console.log(findIndex)
      this.messageLoading = true
      GetDetails(ServiceOrderId).then(res => {
        if (res.code == 200) {
          this.timer = new Date().getTime()
          this.dataForm = this._normalizeOrderDetail(res.result);
          this.serveId = ServiceOrderId
          this.$refs.serviceDetail.open()
          if (!HasRead) {
            readMessage({
              currentUserId: FroUserId,
              serviceOrderId: ServiceOrderId
            })
            let index = findIndex(this.newMessageList, item => { // 找到未读列表中删除的项 在原数组newMessageList中的索引值
              return item.U_SAP_ID === U_SAP_ID
            })
            console.log(index, 'index')
            this.newMessageList.splice(index, 1)
          }
          console.log(this.newMessageList, FroUserId, 'delete')
          this.messageLoading = false
        }
      }).catch((err) => {
        this.messageLoading = false
        console.error(err)
        this.$message.error('获取服务单详情失败')
      })
    },
    deleteSeconds (date) { // yyyy-MM-dd HH:mm:ss 删除秒
      return date ? date.slice(0, -3) : date
    },
    _normalizeOrderDetail (data) {
      let reg = /[\r|\r\n|\n\t\v]/g
      let { serviceWorkOrders } = data
      if (serviceWorkOrders && serviceWorkOrders.length) {
        serviceWorkOrders.forEach(serviceOrder => {
          let { warrantyEndDate, bookingDate, visitTime, liquidationDate, completeDate } = serviceOrder
          serviceOrder.warrantyEndDate = this.deleteSeconds(warrantyEndDate)
          serviceOrder.bookingDate = this.deleteSeconds(bookingDate)
          serviceOrder.visitTime = this.deleteSeconds(visitTime)
          serviceOrder.liquidationDate = this.deleteSeconds(liquidationDate)
          serviceOrder.completeDate = this.deleteSeconds(completeDate)
          serviceOrder.themeList = JSON.parse(serviceOrder.fromTheme.replace(reg, ''))
        })
      }
      return data
    },
    toggleClass(element, className) {
      if (!element || !className) {
        return
      }
      let classString = element.className
      const nameIndex = classString.indexOf(className)
      if (nameIndex === -1) {
        classString += '' + className
      } else {
        classString =
          classString.substr(0, nameIndex) +
          classString.substr(nameIndex + className.length)
      }
      element.className = this.theme === 1 ? classString : ''
      this.saveTheme(this.theme)
    },
    toggleSideBar() {
      this.$store.dispatch('ToggleSideBar')
    },
    logout() {
      if (this.isIdentityAuth) {
        this.signOutOidc()
      } else {
        this.$store.dispatch('LogOut').then(() => {
          location.reload() // 为了重新实例化vue-router对象 避免bug
        })
      }
    },
    handleGoProfile() {
      this.$router.push('/profile')
    },
    handleCommand(name) {
      if(!name) return
      this[name]()
    }
  }
}
</script>
<style rel="stylesheet/scss" lang="scss" scoped>
.navbar {
  .notify-message {
    position: relative;
    .click-mask {
      position: absolute;
      z-index: 11;
      left: 0;
      right: 0;
      top: 0;
      bottom: 0;
    }
    .unread-number {
      position: absolute;
      display: inline-block;
      right: -7px;
      top: 3px;
      color:#fff !important;
      height: 18px;
      line-height: 18px;
      font-size: 12px;
      font-style: normal;
      padding: 0 6px;
      text-align: center;
      white-space: nowrap;
      background-color: rgba(230, 162, 60, 1);
      border-radius: 10px;
    }
    .notify-content-wrapper {
      position: absolute;
      z-index: 10;
      top: 41px;
      left: -94px;
      color: #000;
      font-size: 12px;
      background-color: #fff;
      box-shadow: 0 0 4px 1px;
      border-radius: 5px;
    }
    .caret {
      position: absolute;
      top: 4px;
      left: 50%;
      color: #fff;
      font-size: 15px;
      transform: translate3d(-50%, -100%, 0);
    }
    .scrollbar {
      &.el-scrollbar {
        &.drop-enter-active, &.drop-leave-active {
          transition: all .5s linear;
        }
        &.drop-enter, &.drop-leave-to {
          height: 0;
          opacity: 0;
        }
        &.drop-enter-to, &.drop-leave {
          height: 200px;
          opacity: 1;
        }
        ::v-deep .el-scrollbar__wrap {
          max-height: 200px; // 最大高度
          overflow-x: hidden; // 隐藏横向滚动栏
          margin-bottom: 0 !important;
        }
      }
    }
  }
}


</style>
