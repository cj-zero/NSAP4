import { shallowMount } from '@vue/test-utils'
import FileViewer from '@/components/global/FileViewer'
let wrapper

describe('FileViewer.vue', () => {
  beforeEach(() => {
    wrapper = shallowMount(FileViewer)
  })
  it('test single arrow', async () => {
    expect(wrapper.vm.isSingle).toBe(true)
    expect(wrapper.find('.el-image-viewer__prev').exists()).toBeFalsy()
    expect(wrapper.find('.el-image-viewer__next').exists()).toBeFalsy()
    await wrapper.setProps({ fileList: [{ fileType: 'image', url: '123' }, { fileType: 'image', url: '1233' }] })
    expect(wrapper.find('.el-image-viewer__prev').exists()).toBeTruthy()
    expect(wrapper.find('.el-image-viewer__next').exists()).toBeTruthy()
    expect(wrapper.vm.isSingle).toBe(false)
    expect(wrapper.props().fileList.length).toBe(2)
  })

})