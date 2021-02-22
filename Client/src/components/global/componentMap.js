export default {
  text: {
    component: 'el-input',
    attrs: {
      width: '100%',
      size: 'mini',
      clearable: true,
    }
  },
  number: {
    component: 'el-input-number',
    attrs: {
      style: 'width: 100%',
      size: 'mini',
      clearable: true,
    }
  },
  select: {
    component: 'my-select',
    attrs: {
      style: 'width: 100%',
      size: 'mini',
      clearable: true,
      placeholder: ''
    }
  },
  date: {
    component: 'el-date-picker',
    attrs: {
      style: 'width: 100%',
      clearable: false,
      type: 'date'   
    }
  },
  // area: {
  //   component: 'my-area-selector',
  //   attrs: {
  //     style: 'width: 100%',
  //     size: 'mini',
  //     readonly: true,
  //     clearable: false
  //   }
  // }
  area: {
    component: 'my-new-area-selector',
    attrs: {
      style: 'width: 100%',
      size: 'mini',
      readonly: true,
      clearable: false
    }
  }
}