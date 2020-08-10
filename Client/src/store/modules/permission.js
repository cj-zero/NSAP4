import { constantRouterMap } from '@/router'
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
  arr = arr.concat(constantRouterMap)
  arr.length > 0 && constantRouterMap.forEach(item => {
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
    }
  },
  actions: {
    GenerateRoutes({ commit, dispatch }, data) {
      return new Promise(resolve => {
        (async() => {
          const newPaths = []
          await data.modules.forEach((value) => {
            newPaths.push(groupRoutes(value, dispatch))
          })

          commit('SET_ROUTERS', { addRouters: newPaths, routers: groupRouterLists(newPaths, dispatch) })
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
