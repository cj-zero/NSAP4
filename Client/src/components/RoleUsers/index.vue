<template>
  <div>
    <!-- {{users}} -->
    <el-tag style="margin-right: 4px;" v-for="(item,index) in users" :key="index" size="mini">{{item}}</el-tag>
    <!-- <el-link v-if="buttonVisible" @click="loadRoleUsers">{{text}}
      <i class="el-icon-view el-icon--right"></i>
    </el-link> -->
    <el-button style="height: 20px;padding: 0 10px;" @click="loadRoleUsers" type="primary" :loading="isLoading" plain title="查看更多" v-if="buttonVisible" size="mini" icon="el-icon-more"></el-button>
  </div>
</template>

<script>
  import * as apiUsers from '@/api/users'

  export default {
    name: 'role-users',
    props: ['roleId', 'selectUsers', 'userNames', 'selectUserIds'],
    data() { // todo:兼容layui的样式、图标
      return {
        // users: '',
        users: [],
        page: 1,
        limit: 99999, // 每页条数
        // text: '查看',
        text: '>>更多',
        buttonVisible: true,
        isLoading: false
      }
    },
    mounted() {
      var _this = this
      _this.loadRoleUsers()
    },
    watch: {
      selectUsers(val) {
        this.users = val.length > 0 && val.map(item => item.name || item.account) || []
        if (this.users.length >= this.count || this.users.length === 0) {
          this.buttonVisible = false
        }
      }
    },
    methods: {
      changeNames(names) {
        console.log('names', names)
      },
      loadRoleUsers(id) {
        var _this = this
        this.isLoading = true
        apiUsers.loadByRole({ page: _this.page, limit: _this.limit, roleId: id || _this.roleId }).then(response => {
          _this.$emit('update:selectUsers', response.data || [])
          _this.isLoading = false
          if (response.data.length > 0) {
            // var data = response.data.map(function(item, index, input) {
            //   return item.name || item.account
            // })
            // var ids = response.data.map(function(item, index, input) {
            //   return item.id
            // })
  
            // _this.$emit('update:selectUserIds', ids)
            // _this.$emit('update:userNames', data.length > 0 && data.join(','))
            // _this.users = _this.users.concat(data)
            // _this.users += '  ' + data.join(',')
            _this.page++ // 页码加
            _this.count = response.count
            // _this.text = '>>更多'
          }
          if (_this.users.length >= _this.count) {
            _this.buttonVisible = false
          }
  
          // if (response.data.length < _this.limit) {
          // _this.users += '  没有更多用户！！'
          // _this.buttonVisible = false
          // }
        })
      }
    }
  }

</script>

<style scoped>
 .el-transfer{
   margin-top:10px;
 }
</style>
