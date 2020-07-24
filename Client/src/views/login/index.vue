<template>
  <div class="login-container">
    <div class="content">
      <img class="leftImg" src="~@/assets/login/left.png" alt />
      <el-form
        class="login-form"
        autocomplete="on"
        :model="loginForm"
        :rules="loginRules"
        ref="loginForm"
        label-position="left"
      >
        <h3 class="title">nSAP-V4.0</h3>
        <p class="tips">nSAP-V4.0 管理系统</p>
        <el-form-item prop="username">
          <span class="svg-container svg-container_login">
            <svg-icon icon-class="user" />
          </span>
          <el-input
            name="username"
            type="text"
            v-model="loginForm.username"
            autocomplete="on"
            placeholder="请输入登录账号"
          />
        </el-form-item>
        <el-form-item prop="password">
          <span class="svg-container">
            <svg-icon icon-class="password"></svg-icon>
          </span>
          <el-input
            name="password"
            :type="pwdType"
            @keyup.enter.native="handleLogin"
            v-model="loginForm.password"
            autocomplete="on"
            placeholder="请输入密码"
          ></el-input>
          <span class="show-pwd" @click="showPwd">
            <svg-icon :icon-class="pwdType === 'password' ? 'eye' : 'eye-open'" />
          </span>
        </el-form-item>
        <div class="tips" v-if="isIdentityAuth">
          <router-link to="/oidcRedirect">
            <el-badge is-dot>接口服务器启用了Oauth认证，请点击这里登录</el-badge>
          </router-link>
        </div>
        <el-form-item v-else>
          <el-button
            v-waves
            type="primary"
            style="width:100%;background:#4452D5;font-size: 24px;height: 50px;"
            :loading="loading"
            @click.native.prevent="handleLogin"
          >登 录</el-button>
        </el-form-item>
        <p style="height:25px;line-height:25px;">或</p>
        <el-form-item>
          <el-button
            v-waves
            type="success"
            style="width:100%;font-size: 24px;height: 50px;"
            @click.native.prevent="openQCCode"
          >扫描二维码登录</el-button>
        </el-form-item>
      </el-form>
    </div>
    <!-- <el-dialog title="请用新威智能App扫描此二维码" :visible.sync="dialogQ" width="800px"> 
      <span slot="footer" class="dialog-footer">
        <el-button @click="dialogQ = false">取 消</el-button>
        <el-button type="primary" @click="dialogQ = false">确 定</el-button>
      </span>
    </el-dialog>-->
  </div>
</template>

<script>
import * as login from "@/api/login";
import { setToken } from "@/utils/auth";
import waves from "@/directive/waves"; // 水波纹指令
import { mapGetters } from "vuex";
export default {
  name: "login",
  directives: {
    waves
  },
  data() {
    const validateUsername = (rule, value, callback) => {
      if (value.length <= 0) {
        callback(new Error("用户名不能为空"));
      } else {
        callback();
      }
    };
    const validatePass = (rule, value, callback) => {
      if (value.length <= 0) {
        callback(new Error("密码不能为空"));
      } else {
        callback();
      }
    };
    return {
      loginForm: {
        // username: 'System',
        // password: '123456'
        username: "",
        password: ""
      },
      baseURL: process.env.VUE_APP_BASE_API,

      dialogQ: false,
      randomNum: "",
      loginRules: {
        username: [
          {
            required: true,
            trigger: "blur",
            validator: validateUsername
          }
        ],
        password: [
          {
            required: true,
            trigger: "blur",
            validator: validatePass
          }
        ]
      },
      loading: false,
      url: "",
      pwdType: "password"
    };
  },
  computed: {
    ...mapGetters(["isIdentityAuth"])
  },
  // watch:{
  //   token:{
  //     handler(val){
  //       if(val){}
  //     }
  //   }
  // },
  methods: {
    showPwd() {
      if (this.pwdType === "password") {
        this.pwdType = "";
      } else {
        this.pwdType = "password";
      }
    },
    checkStatus(op) {
      const timer = setInterval(() => {
        login
          .ValidateLogin({ rd: this.randomNum })
          .then(res => {
            if (res.code == 200) {
              let token = res.result;
              op.close();
              this.$store.commit("SET_TOKEN", token);
              setToken(token);
              this.$message({
                type: "success",
                message: "登陆成功"
              });
              this.$router.push({
                path: "/"
              });
            }
          })
          .catch(error => console.log(error));
      }, 3000);

      this.$once("hook:beforeDestroy", () => {
        clearInterval(timer);
      });
      timer();
    },
    goPage() {
      this.randomNum = Math.random() * 10 + "&";
      this.url = `${this.baseURL}/QrCode/Get?rd=${this.randomNum}&`;
      let op = window.open(
        this.url,
        "newwindow",
        "height=500, width=500, top=500,  left=500, toolbar=no, menubar=no, scrollbars=no, resizable=no,location=no, status=no"
      );
      this.checkStatus(op);
    },
    openQCCode() {
      this.goPage();
    },
    handleLogin() {
      this.$refs.loginForm.validate(valid => {
        if (valid) {
          this.loading = true;
          this.$store
            .dispatch("Login", this.loginForm)
            .then(() => {
              this.loading = false;
              this.$router.push({
                path: "/"
              });
            })
            .catch(() => {
              this.loading = false;
            });
        } else {
          console.log("error submit!!");
          return false;
        }
      });
    }
  }
};
</script>

<style rel="stylesheet/scss" lang="scss">
$bg: #2d3a4b;
$light_gray: #eee;
$color_balck: #333;

/* reset element-ui css */
.login-container {
  .el-input {
    display: inline-block;
    height: 47px;
    width: 85%;
    input {
      background: transparent;
      border: 0px;
      -webkit-appearance: none;
      border-radius: 0px;
      padding: 12px 5px 12px 15px;
      color: $color_balck;
      height: 47px;

      &:-webkit-autofill {
        // -webkit-box-shadow: 0 0 0px 1000px $bg inset !important;
        // -webkit-text-fill-color: #fff !important;
        transition: background-color 5000s ease-in-out 0s;
      }
    }
  }
  .el-form-item {
    // border: 1px solid rgba(255, 255, 255, 0.1);
    // background: rgba(0, 0, 0, 0.1);
    margin-bottom: 35px;
    border-radius: 5px;
    color: #454545;
    .el-form-item__content {
      background: #fff;
      border: 1px solid rgba(223, 223, 223, 1);
    }
    &:last-child {
      padding-top: 20px;
      .el-form-item__content {
        border: none;
      }
    }
  }
}
</style>

<style rel="stylesheet/scss" lang="scss" scoped>
@media screen and (max-width: 1150px) {
  .leftImg {
    width: 450px !important;
  }
}
@media screen and (max-width: 1010px) {
  .leftImg {
    width: 380px !important;
  }
}
@media screen and (max-width: 940px) {
  .leftImg {
    display: block;
    width: 260px !important;
    margin: 0 auto !important;
  }
}
// $bg:#2d3a4b;
$dark_gray: #d1dfe8;
// $light_gray:#eee;

.login-container {
  height: 100%;
  background: url("~@/assets/login/bg.png") no-repeat;
  background-color: #ebebea;
  background-position: 0 0;
  background-size: 62% 100%;
  text-align: center;
  &:before {
    content: "";
    display: inline-block;
    height: 100%;
    vertical-align: middle;
  }
  .content {
    display: inline-block;
    vertical-align: middle;
    > img {
      width: 568px;
      margin-right: 150px;
      vertical-align: middle;
    }
    .login-form {
      display: inline-block;
      width: 400px;
      vertical-align: middle;
    }
  }

  .svg-container {
    // padding: 6px 5px 6px 15px;
    color: $dark_gray;
    // color: #D1DFE8;
    vertical-align: middle;
    width: 33px;
    display: inline-block;
    font-size: 22px;
    &_login {
      font-size: 31px;
    }
  }

  .title {
    font-size: 26px;
    font-weight: 400;
    color: #4452d5;
    margin: 0;
    // font-weight: bold;
    text-align: left;
  }
  .tips {
    color: #959595;
    font-size: 14px;
    margin-top: 0;
    margin-bottom: 40px;
    text-align: left;
  }

  .show-pwd {
    position: absolute;
    right: 10px;
    top: 7px;
    font-size: 16px;
    color: $dark_gray;
    cursor: pointer;
    user-select: none;
    font-size: 24px;
  }
}
</style>
