import { constantRouterMap } from '@/router'
import { deepClone } from '@/utils/index'
import Layout from '@/views/layout/Layout'
const groupRoutes = (data, dispatch) => {
  const parentPath = data.item.url.indexOf('?') > -1 ? data.item.url.split('?')[0] : data.item.url
  var newPath = {
    path: parentPath || '/',
    component: data.item && data.item.parentId === null ? Layout : () => import('@/views' + parentPath.toLowerCase()),
    meta: {
      title: data.item.name,
      sortNo: data.item.sortNo,
      icon: data.item.iconName || 'streamlist',
      elements: data.item && data.item.elements || '',
      url: data.item.url,
      code: data.item.code,
      id: data.item.cascadeId
    },
    name: data.item.name,
    hidden: false,
    children: []
  }
  if(newPath.path.indexOf('http') > -1){
    let obj = {}
    obj[newPath.meta.code] = {
      name: newPath.name,
      url: newPath.meta.url
    }
    dispatch('setIframeTagViews', obj, { root: true })
  }
  newPath.path = newPath.path.indexOf('http') > -1 ? `/iframePage/${newPath.meta.code}` : newPath.path
  if (data.children && data.children.length > 0) {
    data.children.forEach(child => {
      newPath.children.push(groupRoutes(child, dispatch))
    })
  }
  return newPath
}
const groupRouterLists = (data) => {
  let arr = []
  let children = []
  data.length > 0 && data.forEach(item => {
    if(item.component && item.component.name && item.component.name === 'layout'){
      let obj = {
        component: () => import('@/views' + item.path.toLowerCase()),
        path: item.children && item.children.length > 0 ? `${item.path}/${item.meta.id}` : item.path,
        name: item.name,
        children: item.children,
        meta: item.meta
      }
      children = children.concat(obj)
      return
    }
    arr.push(item)
  })
  
  arr = arr.concat(deepClone(constantRouterMap))
  arr.length > 0 && arr.forEach(item => {
    if(item.name === 'layout'){
      item.children = item.children.concat(children)
    }
  })
  return arr
}
const groupData = (data) => {
  if(data.children && data.children.length > 0){
    data.children.forEach(child => {
      let code = groupData(child)
      code && (code = code.replace(code[0],code[0].toLowerCase()))
      code && code !== undefined && permission.state.keepAliveData.push(code)
    })
    return
  }
  return data.meta.code
}
const permission = {
  state: {
    routers: constantRouterMap,
    addRouters: [],
    keepAliveData: ['forms']
  },
  mutations: {
    SET_ROUTERS: (state, { addRouters, routers }) => {
      state.addRouters = addRouters
      state.routers = routers
    },
    SAVE_KEEP_ALIVE_DATA: (state, data) => {
      data && data.length > 0 && data.forEach(item => {
        let code = groupData(item)
        code && (code = code.replace(code[0],code[0].toLowerCase()))
        code && code !== undefined && state.keepAliveData.push(code)
      })
    },
    DELETE_KEEP_ALIVE_DATA: (state, { code: componentName, callback }) => {
      console.log(componentName, callback)
      let deleteIndex = state.keepAliveData.findIndex(name => name === componentName)
      
      if (deleteIndex > - 1) {
        state.keepAliveData.splice(deleteIndex, 1)
        callback()
      }
    },
    ADD_KEEP_ALIVE_DATA: (state, componentName) => {
      state.keepAliveData.push(componentName)
    },
  },
  actions: {
    GenerateRoutes({ commit, dispatch }, data) {
      return new Promise(resolve => {
        (async() => {
          const newPaths = []
          await data.modules.forEach((value) => {
            newPaths.push(groupRoutes(value, dispatch))
          })
          // console.log(newPaths, 'NEWPaths')
          const routers = groupRouterLists(newPaths, dispatch)
          const addrouters = routers.find(item => item.name === 'layout').children.filter(itemF=>!constantRouterMap.find(item => item.name === 'layout').children.some(ele=>ele.path===itemF.path))
          newPaths.length > 0 && newPaths.forEach(item => {
            if(item.path.indexOf('/iframePage') <= -1 && item.children.length <= 0){
              addrouters.filter(router => router.path === item.path)
              item.children = addrouters.filter(router => router.path === item.path)
            }
          })
          commit('SET_ROUTERS', { addRouters: newPaths, routers: routers })
          commit('SAVE_KEEP_ALIVE_DATA', newPaths)
          resolve(newPaths)
        })()
      })
    }
  },
  getters: {
    keepAliveData: state => state.keepAliveData
  }
}

export default permission
