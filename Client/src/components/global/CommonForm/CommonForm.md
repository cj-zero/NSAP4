[参考链接](https://element.eleme.cn/#/zh-CN/component/form)

### Form Attribute

(在el-form基础上进行封装，保留原有属性)

|     参数      |                             说明                             |   类型   | 可选值 |            默认值             |
| :-----------: | :----------------------------------------------------------: | :------: | :----: | :---------------------------: |
|     model     |                           表单数据                           |  Object  |   --   |              {}               |
|   formItems   |        每一个表单项的配置，详情请看(**FormItem**配置)        |  Array   |   --   |              []               |
| columnNumber  | 表示一行里显示几个表单项，当且仅当isCustomerEnd为false时生效 |  Number  |   --   |               1               |
| isCustomerEnd |    是否自定义表单项换行，与formItems里的isEnd属性配合使用    | Boolean  |   --   |             false             |
|   rowStyle    |             表单每一行(可能包括多个表单项)的样式             | Function |        | function(model, rowIndex) {}  |
|   cellStyle   |                      每一个表单项的样式                      | Function |        | function(model, cellIndex) {} |

### Form Method 

(与el-form方法一致)

### **Form Item**

|   参数    |                             说明                             |            类型            | 可选值 | 默认值 |
| :-------: | :----------------------------------------------------------: | :------------------------: | :----: | :----: |
|    tag    | 用于映射对应的组件,具体关系可参考项目/src/components/global/componentMap.js |           String           |   --   |   --   |
|   attrs   | 表单项的配置，具体参考各个表单项组件的[配置](https://element.eleme.cn/#/zh-CN/component/installation) |           Object           |   --   |   --   |
| slotName  |             自定义表单项插槽名字（tag属性不生效)             |           String           |   --   |   --   |
|    on     |                     绑定表单组件项的事件                     |           Object           |   --   |   {}   |
| itemAttrs |                     el-form-item的配置项                     |           Object           |   --   |   {}   |
|   isEnd   | 配合isCustomerEnd进行使用，当且仅当isCustomerEnd为true时生效，用来自定义表单项的换行 |          Boolean           |   --   |   --   |
| isRender  |                  判断当前表单项是否进行渲染                  | Boolean/Function(model) {} |   --   |   --   |
|   span    |       el-col的span属性，用来控制每个表单项占一行的宽度       |           Number           |   --   |   24   |

#### From Slot Scope

| 参数                    | 说明                                                         |
| ----------------------- | ------------------------------------------------------------ |
| formItems的slotName的值 | 参数为{ model, rowIndex, colIndex, item }<br />model为表单数据，rowIndex为行索引，colIndex为列索引，item为表单项的所有配置，参考上方的**FormItem** |



#### 示例代码一

![wucolu.png](https://i.loli.net/2021/01/25/fLy94qQSBAPNZt8.png)

```vue
// 渲染表单项
<common-form :model="model" :formItems="items"></common-form>

data () {
	reutrn { 
		model: { name: 'lxb', age: '18', sex: '女', date: '123' }，
		items: [
			// el-input
			{ tag: 'text', attrs: { prop: 'name'}, itemAttrs: { label: '名字' } },
			// el-input-number
			{ tag: 'number', attrs: { prop: 'age' }, itemAttrs: { label: '年龄' } },
			// el-select
            { tag: 'select', attrs: { prop: 'sex', options: [{ label: '男' }, { label: '女' }] }, itemAttrs: { label: '性别' } },
			// el-date-picker
            { tag: 'date', attrs: { prop: 'date', 'value-format': 'yyyy-MM-dd' }, itemAttrs: { label: '日期' }, isRender: this.dateRender }
		]
	}
},
methods: {
	dateRender (model) {
		return this.model.age >= 20 // 当年龄小于20，就不渲染
	}
}
```

#### 示例代码二

![columnNumber.png](https://i.loli.net/2021/01/25/xuigfJnsvXZQWmw.png)**使用columnNumber换行**

![Snipaste_2021-01-25_17-54-38.png](https://i.loli.net/2021/01/25/FVweLxdaW7h6yzn.png)**自定义换行**

```vue
// 表单项换行
<common-form :model="model" :formItems="items" :columnNumber="2" label-width="50px"></common-form>
// 自定义换行
<common-form :model="model" :formItems="items" :columnNumber="2" :isCustomerEnd="true" label-width="50px"></common-form>
data () {
	reutrn { 
		model: { name: 'lxb', age: '18', sex: '女', date: '123' }，
		items: [
			{ tag: 'text', attrs: { prop: 'name'}, itemAttrs: { label: '名字' }, isEnd: true },
			{ tag: 'number', attrs: { prop: 'age' }, itemAttrs: { label: '年龄' } },
            { tag: 'select', attrs: { prop: 'sex', options: ...  }, itemAttrs: { label: '性别' } },
            { tag: 'date', attrs: { prop: 'name', 'value-format': 'yyyy-MM-dd' }, itemAttrs: { label: '日期' }, isRender: this.dateRender }
		]
	}
}
```

#### 示例代码三

![校验.png](https://i.loli.net/2021/01/25/tmZT4zgd6L2pcEX.png)

```vue
// 表单校验 用法与el-form一致
<common-form 
  ref="form" :model="model" :formItems="items" :rules="rules" label-width="50px">
</common-form>
<button @click="validate">校验</button>

data () {
	reutrn { 
		rules: {
			name: [{ required: true }]
		},
		model: { name: 'lxb', age: '18', sex: '女', date: '123' }，
		items: [
        { tag: 'text', attrs: { prop: 'name'}, itemAttrs: { label: '名字', prop: 'name' }, isEnd: true },
        { tag: 'number', attrs: { prop: 'age' }, itemAttrs: { label: '年龄', prop: 'age' } },
        { tag: 'select', attrs: { prop: 'sex', options: [{ label: '男' }, { label: '女' }] }, itemAttrs: { label: '性别', prop: 'sex', rules: [{ required: true, trigger: ['blur', 'change'] }] } },
        { tag: 'date', attrs: { prop: 'name', 'value-format': 'yyyy-MM-dd' }, itemAttrs: { label: '日期', prop: 'date' } }
      ],
	}
},
methods: {
	validate () {
		this.$refs.form.validate(isValid => {
			if (!isValid) return
			// ....
		})
	}
}
```

#### 示例代码四

![插槽.png](https://i.loli.net/2021/01/25/U34tMcTSGEyZXBA.png)

```vue
// 表单项插槽
<common-form 
  ref="form" :model="model" :formItems="items" :rules="rules" label-width="50px">
  <template v-slot:age="{ model, rowIndex, colIndex, item }">
        <el-form-item label="年龄" prop="age" :rules="[{ required: true }]">
          <el-input size="mini" v-model="model.age"></el-input>
          <p style="font-size: 15px;">当前行 {{ rowIndex + 1 }} 当前列 {{ colIndex + 1 }} </p>
          <p style="font-size: 15px;">
            <span>{{ JSON.stringify(item) }}</span>
          </p>
        </el-form-item>
        </template>
</common-form>
<button @click="validate">校验</button>

data () {
	reutrn { 
		rules: {
			age: [{ required: true }]
		},
		model: { name: 'lxb', age: '18', sex: '女', date: '123' }，
		items: [
        { tag: 'text', attrs: { prop: 'name'}, itemAttrs: { label: '名字', prop: 'name' }, isEnd: true },
		// 插槽
        { tag: 'number', attrs: { prop: 'age' }, itemAttrs: { label: '年龄', prop: 'age' } , slotName: 'age' },
        { tag: 'select', attrs: { prop: 'sex', options: [{ label: '男' }, { label: '女' }] }, itemAttrs: { label: '性别', prop: 'sex' }] } },
        { tag: 'date', attrs: { prop: 'name', 'value-format': 'yyyy-MM-dd' }, itemAttrs: { label: '日期', prop: 'date' } }
      ],
	}
},
methods: {
	validate () {
		this.$refs.form.validate(isValid => {
			if (!isValid) return
			// ....
		})
	}
}
```