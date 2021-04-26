import { findIndex } from '@/utils/process'
export default {
  namespaced: true,
  state: {
    downloadProcessList: [] // 证书下载进度条
  },
  mutations: {
    ADD_PROCESS (state, process) {
      state.downloadProcessList.push(process)
      console.log(state.downloadProcessList, 'ADD_PROCESS', process)
    },
    ADD_PROCESS_COUNT (state, process) {
      const index = findIndex(state.downloadProcessList, item => item.id === process.id)
      const target = state.downloadProcessList[index]
      target.count++
    },
    DELETE_PROCESS (state, process) {
      const index = findIndex(state.downloadProcessList, item => item.id === process.id)
      index > -1 && state.downloadProcessList.splice(index, 1)
    }
  }
}