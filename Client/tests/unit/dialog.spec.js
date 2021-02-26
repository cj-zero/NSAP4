import { mount, createLocalVue } from '@vue/test-utils'
import MyDialog from '@/components/global/Dialog'
import { Dialog, Row, Button, Loading } from 'element-ui'

const plugins = {
  install (_) {
    _.component(Dialog.name, Dialog)
    _.component(Row.name, Row)
    _.component(Button.name, Button)
    _.use(Loading.directive)
  }
}
const localVue = createLocalVue()
localVue.use(plugins)
let wrapper 


describe('Dialog.vue', () => {
  beforeEach(() => {
    wrapper = mount(MyDialog, {
      localVue
    })    
  })
  it('test toggle show', async () => {
    expect(wrapper.vm.dialogVisible).toBe(false)
    wrapper.vm.open()
    expect(wrapper.vm.dialogVisible).toBe(true)
    expect(wrapper.emitted()['update:visible'][0]).toEqual([true])
    wrapper.vm.close()
    expect(wrapper.vm.dialogVisible).toBe(false)
    expect(wrapper.emitted()['update:visible'][1]).toEqual([false])
  })
  
  it.skip('test props btnList', async () => {
    const btnList = [
      { btnText: 'a' },
      { btnText: 'b' },
      { btnText: 'c' }
    ]
    expect(wrapper.findAllComponents({ name: 'ElButton' }).length).toBe(0)
    await wrapper.setProps({ btnList })
    expect(wrapper.findAllComponents({ name: 'ElButton' }).length).toBe(3)
    expect(wrapper.props().btnList.length).toBe(3)
  })
})