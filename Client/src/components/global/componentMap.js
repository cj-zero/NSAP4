const commonComponentMap = {
  text: {
    component: 'el-input',
    attrs: {
      style: { width: '100%' },
      size: 'mini',
      clearable: true,
    }
  },
  button: {
    component: 'el-button',
    attrs: {
      style: { width: '100%' },
      size: 'mini'
    }
  },
  number: {
    component: 'el-input-number',
    attrs: {
      style: { width: '100%' },
      size: 'mini',
      clearable: true,
    }
  },
  select: {
    component: 'my-select',
    attrs: {
      style: { width: '100%' },
      size: 'mini',
      clearable: true,
      placeholder: ''
    }
  },
  date: {
    component: 'el-date-picker',
    attrs: {
      style: { width: '100%' },
      clearable: false,
      type: 'date',
      size: 'mini',
      'value-format': 'yyyy-MM-dd'
    }
  },
  tree: {
    component: 'el-cascader',
    attrs: {
      props: { value: 'id', label: 'name', children: 'childTypes', expandTrigger: 'hover', emitPath: false },
      clearable: true,
      size: 'mini'
    }
  },
  area: {
    component: 'my-new-area-selector',
    attrs: {
      style: { width: '100%' },
      size: 'mini',
      readonly: true,
      clearable: false
    }
  },
  's-input': {
    component: 'search-input',
    attrs: {
      style: { width: '100%' },
      size: 'mini',
      clearable: true,
    }
  },
  's-upload': {
    component: 'search-upload',
    attrs: {
      style: { display: 'inline-block' },
      size: 'mini'
    }
  },
  's-button': {
    component: 'search-button',
    attrs: {
      size: 'mini'
    }
  }
}

export default commonComponentMap
  