export default {
  text: {
    component: 'el-input',
    attrs: {
      width: '100%',
      size: 'mini',
      clearable: true,
      placeholder: '请输入'
    }
  },
  number: {
    component: 'el-input-number',
    attrs: {
      style: 'width: 100%',
      size: 'mini',
      clearable: true,
      placeholder: '请输入'
    }
  },
  select: {
    component: 'my-select',
    attrs: {
      style: 'width: 100%',
      size: 'mini',
      clearable: true,
      placeholder: '请输入'
    }
  },
  date: {
    component: 'el-date-picker',
    attrs: {
      width: '100%',
      size: 'mini',
      clearable: true,
      placeholder: '请输入'
    }
  }
}