const tagsView = {
  state: {
    visitedViews: [],
    cachedViews: []
  },
  mutations: {
    ADD_VISITED_VIEWS: (state, view) => {
      if (state.visitedViews.some(v => v.path === view.path)) return
      // state.visitedViews.push(Object.assign({}, view, {
      //   title: view.meta.title || 'no-name'
      // }))
      state.visitedViews.push(Object.assign({}, view, {
        title: view.name === 'iframePage' ? state.iframeViews[view.params.code].name : view.meta.title || 'no-name'
      }))
      if (!view.meta.noCache) {
        state.cachedViews.push(view.name)
      }
    },
    COPY_VISITED_VIEWS:(state, { view, index })=>{
      console.log(view, index)
      // if (state.visitedViews.some(v => v.path === view.path)) return
      state.visitedViews.splice(index + 1, 0, Object.assign({}, view, {
        title: view.meta.title || 'no-name'
      }))
      // state.visitedViews.push(Object.assign({}, view, {
      //   title: view.meta.title || 'no-name'
      // }))
      if (!view.meta.noCache) {
        state.cachedViews.push(view.name)
      }
    },
    REFRESH_VISITED_VIEWS: (state, { originRoute, newRoute, index }) => {
     // 将当前页进行删除
     console.log(originRoute, newRoute, 'refresh visited views')
      state.visitedViews.splice(index, 1) // 将当前页面删除
      state.visitedViews.splice(index, 0, Object.assign({}, newRoute, { // 再将新的路由插入到当前页的索引里
        title: newRoute.meta.title || 'no-name'
      }))
    },
    DEL_CACHED_VIEW: (state, view) => {
      const index = state.cachedViews.indexOf(view.name)
      index > -1 && state.cachedViews.splice(index, 1)
    },
    DEL_VISITED_VIEWS: (state, view) => {
      for (const [i, v] of state.visitedViews.entries()) {
        if (v.fullPath === view.fullPath) {
          state.visitedViews.splice(i, 1)
          break
        }
      }
      for (const i of state.cachedViews) {
        if (i === view.name) {
          const index = state.cachedViews.indexOf(i)
          state.cachedViews.splice(index, 1)
          break
        }
      }
    },
    DEL_OTHERS_VIEWS: (state, view) => {
      for (const [i, v] of state.visitedViews.entries()) {
        if (v.fullPath === view.fullPath) {
          state.visitedViews = state.visitedViews.slice(i, i + 1)
          break
        }
      }
      for (const i of state.cachedViews) {
        if (i === view.name) {
          const index = state.cachedViews.indexOf(i)
          state.cachedViews = state.cachedViews.slice(index, index + 1)
          break
        }
      }
    },
    DEL_ALL_VIEWS: (state) => {
      state.visitedViews = []
      state.cachedViews = []
    },
    SET_IFRAME_TAGVIEWS(state, data){
      state.iframeViews = { ...state.iframeViews, ...data }
    }
  },
  actions: {
    setIframeTagViews({ commit }, data) {
      commit('SET_IFRAME_TAGVIEWS', data)
    },
    addVisitedViews({ commit }, view) {
      commit('ADD_VISITED_VIEWS', view)
    },
    copyVisitedViews({ commit }, payLoad){
      commit('COPY_VISITED_VIEWS', payLoad)
    },
    refreshVisitedViews({ commit }, payLoad) {
      commit('REFRESH_VISITED_VIEWS', payLoad)
    },
    delVisitedViews({ commit, state }, view) {
      return new Promise((resolve) => {
        commit('DEL_VISITED_VIEWS', view)
        resolve([...state.visitedViews])
      })
    },
    delOthersViews({ commit, state }, view) {
      return new Promise((resolve) => {
        commit('DEL_OTHERS_VIEWS', view)
        resolve([...state.visitedViews])
      })
    },
    delCachedView({ commit, state }, view) {
      return new Promise(resolve => {
        commit('DEL_CACHED_VIEW', view)
        resolve([...state.cachedViews])
      })
    },
    delAllViews({ commit, state }) {
      return new Promise((resolve) => {
        commit('DEL_ALL_VIEWS')
        resolve([...state.visitedViews])
      })
    }
  },
  getters: {
    iframeViews: state => state.iframeViews
  }
}

export default tagsView
