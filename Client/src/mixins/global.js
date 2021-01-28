export default {
  methods: {
    reset () { //  重置组件的初始数据
      Object.assign(this.$data, this.$options.data.call(this))
    }
  }
}