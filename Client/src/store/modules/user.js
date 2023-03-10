import { login, logout, getInfo, getModules, getModulesTree, getOrgs } from '@/api/login'
import { getToken, setToken, removeToken } from '@/utils/auth'
import { getUserInfoAll } from '@/api/users'
// import { getRoles } from '@/api/roles'
const user = {
  state: {
    token: getToken(),
    name: '',
    avatar: '',
    modules: null,
    defaultorg: null, // 登录后默认的操作机构
    roles: [], // 用户角色
    userInfoAll: {} // 用户的所有信息
  },

  mutations: {
    SET_TOKEN: (state, token) => {
      state.token = token
    },
    SET_NAME: (state, name) => {
      state.name = name
    },
    SET_MODULES: (state, modules) => {
      state.modules = modules
    },
    SET_DEFAULTORG: (state, org) => {
      state.defaultorg = org
    },
    SET_ROLES: (state, roles) => {
      state.roles = roles
    },
    SET_USER_INFO: (state, userInfoAll) => {
      state.userInfoAll = userInfoAll
    }
  },

  actions: {
    // 登录
    Login({ commit }, userInfo) {
      const username = userInfo.username.trim()
      return new Promise((resolve, reject) => {
        login(username, userInfo.password).then(response => {
          const data = response
          setToken(data.token)
          commit('SET_TOKEN', data.token)
          resolve()
        }).catch(error => {
          reject(error)
        })
      })
    },
    // 获取当前用户的名字、部门、角色列表、劳务关系
    GetUserInfoAll ({ commit }) {
      return new Promise((resolve, reject) => {
        getUserInfoAll().then(response => {
          commit('SET_USER_INFO', response.data)
          resolve(response.data)
        }).catch(error => {
          reject(error)
        })
      })
    },
    // 获取用户的角色信息
    // GetRoles({ commit }) {
    //   return new Promise((resolve, reject) => {
    //     getRoles().then(response => {
    //       commit('SET_ROLES', response.result)
    //       resolve(response.result)
    //     }).catch(error => {
    //       reject(error)
    //     })
    //   })
    // },
    // 获取用户信息
    GetInfo({ commit, state }) {
      return new Promise((resolve, reject) => {
        getInfo(state.token).then(response => {
          commit('SET_NAME', response.result)
          resolve(response)
        }).catch(error => {
          reject(error)
        })
      })
    },
    // todo:默认登录后取第一个机构的id作为默认，可以在【个人中心】界面修改默认
    // 在大型业务系统中，应该让用户登录成功后弹出选择框选择操作的机构
    GetDefaultOrg({ commit, state }) {
      return new Promise((resolve, reject) => {
        getOrgs(state.token).then(response => {
          commit('SET_DEFAULTORG', response.result[0])
          resolve(response)
        }).catch(error => {
          reject(error)
        })
      })
    },
    // 获取用户模块
    GetModules({ commit, state }) {
      return new Promise((resolve, reject) => {
        getModules(state.token).then(response => {
          commit('SET_MODULES', response.result)
          resolve(response)
        }).catch(error => {
          reject(error)
        })
      })
    },
    // 获取用户模块
    GetModulesTree({ commit, state }) {
      return new Promise((resolve, reject) => {
        if (state.modules != null) {
          resolve(state.modules)
          return
        }
        getModulesTree(state.token).then(response => {
          commit('SET_MODULES', response.result)
          resolve(response.result)
        }).catch(error => {
          reject(error)
        })
      })
    },
    // 登出
    LogOut({ commit, state }) {
      return new Promise((resolve, reject) => {
        logout(state.token).then(() => {
          commit('SET_TOKEN', '')
          commit('SET_MODULES', [])
          removeToken()
          resolve()
        }).catch(error => {
          reject(error)
        })
      })
    },

    // 前端 登出
    FedLogOut({ commit }) {
      return new Promise(resolve => {
        commit('SET_TOKEN', '')
        removeToken()
        resolve()
      })
    }
  }
}

export default user
