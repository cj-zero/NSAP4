[参考链接](https://element.eleme.cn/#/zh-CN/component/table)

### table  

##### (在el-table基础上扩展的属性，保留table原有的属性)

|     参数     |                             说明                             |  类型   | 可选值 | 默认值 |
| :----------: | :----------------------------------------------------------: | :-----: | :----: | :----: |
|     data     |                           表格数据                           |  array  |   --   |   []   |
|   columns    | 表格每一列的数据(保留el-table-column参数，在基础上扩展了其他参数)，参考table-column-attributes |  array  |   --   |   []   |
|   dataKey    | 如果需要对表格里的表单数据进行验证，需要外面套了一层el-form进行表单验证，**需要指定key值，参考示例1**  [实现思路参考链接](https://www.cnblogs.com/kummy/p/9470393.html) | string  |   --   |  list  |
|   loading    |         表格loading，也可以直接在组件外添加v-loading         | boolean |   --   |   --   |
| selectedKey  | 多选情况下，与selectList配合使用，指定数据的唯一键值(column-attributes中对应的prop的值)，判断当前选项是否已经选择过**（参考示例2）** | string  |        |   id   |
| selectedList | 多选情况下，已经选择过的数据集合，相当于data的子集与selectedKey配合使用**（参考示例2）** |  array  |        |   []   |
|   radioKey   |      单选的唯一键值，column-attributes中对应的prop的值       |         |        |        |

### Table Methods

**(在el-table的基础上，保留原方法，进行需求扩展)**

|      方法名      |                             说明                             |  参数  |
| :--------------: | :----------------------------------------------------------: | :----: |
|  getCurrentRow   |                   获取表格当前点击的行数据                   | object |
| getSelectionList | 获取表格多选的表格数据(**过滤掉已经选择过的数据**，**且默认支持翻页保留**) | array  |
| resetCurrentRow  |                   清空表格当前点击的行数据                   |   --   |

### Table-column-attributes

##### (基于el-table-column-attributes上添加，保留原有属性)

新增或者扩展的属性列表

|       参数        |                             说明                             |  类型   | 可选值 |                          默认值                           |
| :---------------: | :----------------------------------------------------------: | :-----: | :----: | :-------------------------------------------------------: |
|       type        | 对应列的类型，在原基础上添加了radio类型，需要手动指定radioKey，否则会产生混乱 | string  |   --   |                            --                             |
|     slotName      |                自定义表格的插槽**(不能重复)**                | string  |   --   |                            --                             |
|     component     | 表格内表单组件的配置,包括el-input, el-select等，**component-attributes**配置** | object  |   --   |                            {}                             |
| isCustomizeHeader |                      是否自定义表格头部                      | boolean |   --   |                           false                           |
|    headerName     |  自定义表头的插槽名，当且仅当isCustomizeHeader为true时生效   | string  |   --   | 默认为prop属性跟字符串header的拼接`${column.prop}_header` |

##### Table-column Scoped Slot

#####  (使用了slotName的插槽，用法参考示例3)

|            name             |         参数         |
| :-------------------------: | :------------------: |
|        slotName的值         | { index, row, prop } |
| isCustomizeHeader的插槽名字 |    { row, label }    |

**Component-attributes**

##### （当表格字段里有很多表单组件的话，建议使用component属性而不是slotName属性)

| 参数      |                             说明                             |  类型  | 可选值 | 默认值 |
| --------- | :----------------------------------------------------------: | :----: | :----: | :----: |
| tag       | 用于映射对应的组件,具体关系可参考项目/src/components/global/componentMap.js | string |   --   |   --   |
| itemAttrs | el-form-item的配置项，[请参考官网](https://element.eleme.cn/#/zh-CN/component/form) | object |   --   |   --   |
| attrs     |                 tag属性映射后的组件的属性值                  | object |   --   |   --   |
| on        |                tag属性映射后的组件的监听事件                 | object |   --   |   --   |

**注意事项**

1. 必须指定columns的prop值，将来会直接将prop的值当作表单组件的v-model绑定。

#### 示例1

```vue
// dataKey的使用
<template>
	<el-form :model="model">
        <!-- dataKey就是表单数据的model下的 'list' -->
        <!-- 如果使用model2，则dataKey需指定为'data' -->
        <common-table :data="model.list" :columns="columns" dataKey="list">
        </common-table>
	</el-form>
</template>

<script>
export default {
    computed: {
        model () { return { list: this.data }}，
        model2 () { return { data: this.data }}
    }
    data () {
        data: [...],
        columns: [...]
    }
 }
</script>
```

#### 示例2

```vue
// 使用场景：需要对表格中选择过的数据进行过滤
// selectedList selectedKey使用方法
<template>
	<div>
        <common-table ref="table" :data="model.list" :columns="columns" selectedKey="id" :selectedList="selectedList">
    	</common-table>
        <el-button @click="select">选择</el-button>
    </div>
</template>

<script>
 export default {
     data () {
        data: [{ name: '下位机掉线' }，{ name: '辅助通道的向好和下位机重复' }],
     	columns: [{ type: 'selection' }, { label: '呼叫主题', prop: 'name' }],
        selectedList: []
    },
	methods： {
        select () {
            let selectList = this.$table.getSelectionList()
            this.selectedList = selectList
        }
    }
 }
</script>
```

选择之后的效果图如下:![Snipaste_2020-12-31_17-37-36.png](https://i.loli.net/2020/12/31/kFxyOz5G9qHP7Ue.png)

如果将this.selectedList对应数据清空，则对应的数据行会显示可选状态

#### 示例3

```vue
// slotName 自定义表格列内容
// isCustomizeHeader 自定义表头
<template>
	<div>
        <common-table ref="table" :data="data" :columns="columns">
            <!-- 自定义表头 -->
            <template v-slot:description_header="{ row, label }">
                <span>{{ label }}</span>
				<span>{{ row.description }}</span>
			</template>
			<!-- 自定义内容插槽 -->
            <template v-slot:formTheme="{row,index,prop}">
				<p>{{ row.name }} {{ index }} {{ prop }}</p>
                <!-- 下位机掉线  0  'name' -->
                <el-input v-model="row.name"></el-input>
                <!-- 也可以直接绑定表单元素 -->
			</template>
    	</common-table>
        <el-button @click="select">选择</el-button>
    </div>
</template>

<script>
 export default {
     data () {
        data: [{ name: '下位机掉线', description: '123' }],
     	columns: [
            { label: '呼叫主题', prop: 'name', slotName: 'formTheme' },
            { isCustomizeHeader: true, label: '描述', prop: 'description' }
        ]
    }
 }
</script>
```

#### 示例4 

```vue
// component-attributes
// 使用场景: 当表格中有多个字段，并且有大多数字段都需要用到表单组件，使用component属性替代slotName，可大大减少代码量

<template>
	<div>
        <el-form :model="model" ref="form">
            <common-table ref="table" :data="model.list" :columns="columns" dataKey="list">	
            </common-table>
    	</el-form>
    </div>
</template>

<script>
 export default {
     computed: {
         model () { return { list: this.data }}
     },
     data () {
          const rulesItem = [{ required: true, trigger: 'blur' }]
          const textComponent = {
              tag: 'text', // 映射成el-input 具体映射配置参考项目中/src/components/global/componentsMap.js
              attrs: {}, // 组件的属性值 $attrs
              on: {}, // 组件监听的事件 $listeners，不支持原生事件，只支持组件
              itemAttrs： { // el-form-item属性
              rules: rulesItem
          }
          }
        data: [{ name: '下位机掉线', description: '123' }],
     	columns: [
            { label: '呼叫主题', prop: 'name', component: textComponent }, // 必须指定prop的值
            { label: '描述', prop: 'description', component: textComponent }
        ]
    },
    methods: {
        validate () {
            // this.$refs.form.validate()
        }
    }
 }
</script>
```

注意事项:

1.  如果需要对表格中的表单组件进行校验，必须手动指定component属性中的itemAttrs属性中的rules(相当于el-form-item)，只是在el-form指定rules不会生效。
2. **itemAttrs中的prop属性不需要指定，内部已经封装了。**