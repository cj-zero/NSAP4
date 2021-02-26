import { createLocalVue, shallowMount } from '@vue/test-utils'
import Area from '@/components/global/NewArea'
import NewAreaDownPicker from '@/components/global/NewArea/area'
import { Input, Scrollbar } from 'element-ui'

const plugin = {
  install (_) {
    _.component(Input.name, Input)
    _.component(Scrollbar.name, Scrollbar)
    _.component(NewAreaDownPicker.name, NewAreaDownPicker)
  }
}

const localVue = createLocalVue()
localVue.use(plugin)
let wrapper
describe('Area.vue', () => {
  beforeEach(() => {
    wrapper = shallowMount(Area, { 
      localVue
    })    
  })
  it('test toggle show', async () => {
    const input = wrapper.findComponent({ ref: 'reference' })
    expect(wrapper.vm.isShow).toBe(false)
    await input.trigger('click')
    expect(wrapper.vm.isShow).toBe(true)
    await input.trigger('click')
    expect(wrapper.vm.isShow).toBe(false)
  })
  it('test processValueFn', async () => {
    const processValueFn = function ({ province, city, district }) {
      return province + city + district + district
    }
    wrapper.setProps({ processValueFn })
    wrapper.vm.$emit('input', processValueFn({
      province: '123',
      city: 'asd',
      district: '456'
    }))
    await wrapper.vm.$nextTick()
    expect(wrapper.emitted().input[0]).toEqual(['123asd456456'])
  })

})