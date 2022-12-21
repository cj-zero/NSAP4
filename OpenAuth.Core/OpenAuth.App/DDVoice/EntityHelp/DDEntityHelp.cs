using System;
using System.Collections.Generic;
using System.Text;
using OpenAuth.App.Request;

namespace OpenAuth.App.DDVoice.EntityHelp
{
    /// <summary>
    /// 登录
    /// </summary>
    public class DDLogin
    {
        /// <summary>
        /// 返回码
        /// </summary>
        public int errcode { get; set; }

        /// <summary>
        /// api凭证
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// 返回码描述
        /// </summary>
        public string errmsg { get; set; }

        /// <summary>
        /// access_token的过期时间，单位秒
        /// </summary>
        public int expires_in { get; set; }
    }

    /// <summary>
    /// 推送消息实体参数
    /// </summary>
    public class DDMsgBodyParam
    {
        /// <summary>
        /// 微应用AgentId
        /// </summary>
        public string agent_id { get; set; }

        /// <summary>
        /// 接收者userid列表
        /// </summary>
        public string userid_list { get; set; }

        /// <summary>
        /// 接收者部门id列表
        /// </summary>
        public string dept_id_list { get; set; }

        /// <summary>
        /// 是否发送给所有用户
        /// </summary>
        public bool to_all_user { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public DDSendMsg msg { get; set; }
    }

    /// <summary>
    /// 推送消息
    /// </summary>
    public class DDSendMsg
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public string msgtype { get; set; }

        /// <summary>
        /// 文本消息
        /// </summary>
        public DDTextMsg text { get; set; }
    }

    /// <summary>
    /// 推送文本消息
    /// </summary>
    public class DDTextMsg
    {
        /// <summary>
        /// 文本内容
        /// </summary>
        public string content { get; set; }
    }

    /// <summary>
    /// 推送结果
    /// </summary>
    public class DDSendResult
    {
        /// <summary>
        /// 请求Id
        /// </summary>
        public string request_id { get; set; }

        /// <summary>
        /// 返回码描述
        /// </summary>
        public string errmsg { get; set; }

        /// <summary>
        /// 返回码
        /// </summary>
        public int errcode { get; set; }

        /// <summary>
        /// 创建的异步发送任务Id
        /// </summary>
        public long task_id { get; set; }
    }

    /// <summary>
    /// 获取部门参数
    /// </summary>
    public class DDDepartParam
    { 
        /// <summary>
        /// 父部门id
        /// </summary>
        public long dept_id { get; set; }

        /// <summary>
        /// 通讯录语言
        /// </summary>
        public string language { get; set; }
    }

    /// <summary>
    /// 获取部门返回结果
    /// </summary>
    public class DDDepartResult
    {
        /// <summary>
        /// 请求Id
        /// </summary>
        public string request_id { get; set; }

        /// <summary>
        /// 返回码描述
        /// </summary>
        public string errmsg { get; set; }

        /// <summary>
        /// 返回码
        /// </summary>
        public int errcode { get; set; }

        /// <summary>
        /// 部门列表信息
        /// </summary>
        public List<DDDepartResultMsg> result { get; set; }
    }

    /// <summary>
    /// 获取部门返回详细信息
    /// </summary>
    public class DDDepartResultMsg
    { 
        /// <summary>
        /// 部门Id
        /// </summary>
        public long dept_id { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 父部门id
        /// </summary>
        public long parent_id { get; set; }

        /// <summary>
        /// 是否同步创建一个关联此部门的企业群
        /// </summary>
        public bool auto_add_user { get; set; }

        /// <summary>
        /// 部门群已经创建后，有新人加入部门是否会自动加入该群
        /// </summary>
        public bool create_dept_group { get; set; }
    }

    /// <summary>
    /// 部门信息实体
    /// </summary>
    public class DDDepartMsgs
    {
        /// <summary>
        /// 部门id
        /// </summary>
        public long departId { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string departName { get; set; }
    }

    /// <summary>
    /// 获取部门用户基础信息参数
    /// </summary>
    public class DDDepartUserBaseParam
    { 
        /// <summary>
        /// 部门id
        /// </summary>
        public long dept_id { get; set; }

        /// <summary>
        /// 分页页码
        /// </summary>
        public int cursor { get; set; }

        /// <summary>
        /// 分页长度
        /// </summary>
        public int size { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string order_field { get; set; }

        /// <summary>
        /// 是否返回访问受限的员工
        /// </summary>
        public bool contain_access_limit { get; set; }

        /// <summary>
        /// 通讯录语言
        /// </summary>
        public string language { get; set; }
    }

    /// <summary>
    /// 部门获取用户基础信息返回参数
    /// </summary>
    public class DDBaseUserResult
    {
        /// <summary>
        /// 返回码描述
        /// </summary>
        public string errmsg { get; set; }

        /// <summary>
        /// 返回码
        /// </summary>
        public int errcode { get; set; }

        /// <summary>
        /// 返回结果
        /// </summary>
        public DDBaseUserPageResult result { get; set; }
    }

    /// <summary>
    /// 返回结果
    /// </summary>
    public class DDBaseUserPageResult
    {
        /// <summary>
        /// 是否还有更多的数据
        /// </summary>
        public bool has_more { get; set; }

        /// <summary>
        /// 下一次分页的游标
        /// </summary>
        public int next_cursor { get; set; }

        /// <summary>
        /// 用户信息列表
        /// </summary>
        public List<DDUserMsgs> list { get; set; }
    }

    /// <summary>
    /// 用户详细信息
    /// </summary>
    public class DDUserMsgs
    { 
        /// <summary>
        /// 用户id
        /// </summary>
        public string userid { get; set; }

        /// <summary>
        /// 用户在当前开发者企业帐号范围内的唯一标识
        /// </summary>
        public string unionid { get; set; }

        /// <summary>
        /// 用户姓名
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 头像地址
        /// </summary>
        public string avatar { get; set; }

        /// <summary>
        /// 国际电话区号
        /// </summary>
        public string state_code { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string mobile { get; set; }

        /// <summary>
        /// 是否号码隐藏
        /// </summary>
        public bool hide_mobile { get; set; }

        /// <summary>
        /// 分机号
        /// </summary>
        public string telephone { get; set; }

        /// <summary>
        /// 员工工号
        /// </summary>
        public string job_number { get; set; }

        /// <summary>
        /// 职位
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// 员工邮箱
        /// </summary>
        public string email { get; set; }

        /// <summary>
        /// 员工企业邮箱
        /// </summary>
        public string org_email { get; set; }

        /// <summary>
        /// 办公地点
        /// </summary>
        public string work_place { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 所属部门id列表
        /// </summary>
        public long[] dept_id_list { get; set; }

        /// <summary>
        /// 员工在部门中的排序
        /// </summary>
        public long dept_order { get; set; }

        /// <summary>
        /// 扩展属性
        /// </summary>
        public string extension { get; set; }

        /// <summary>
        /// 入职时间
        /// </summary>
        public long hired_date { get; set; }

        /// <summary>
        /// 是否激活钉钉
        /// </summary>
        public bool active { get; set; }

        /// <summary>
        /// 是否为管理员
        /// </summary>
        public bool admin { get; set; }

        /// <summary>
        /// 是否为企业老板
        /// </summary>
        public bool boss { get; set; }

        /// <summary>
        /// 是否为部门主管
        /// </summary>
        public bool leader { get; set; }

        /// <summary>
        /// 是否专属账号
        /// </summary>
        public bool exclusive_account { get; set; }
    }

    /// <summary>
    /// 需要绑定的用户信息
    /// </summary>
    public class DDNeedBindUserMsg
    { 
         /// <summary>
         /// 用户Id
         /// </summary>
         public string UserId { get; set; }

         /// <summary>
         /// 用户名称
         /// </summary>
         public string UserName { get; set; }
    }

    /// <summary>
    /// 手动绑定用户参数实体
    /// </summary>
    public class DDUpdateBindUserParam
    { 
        /// <summary>
        /// 用户Id
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 钉钉用户Id
        /// </summary>
        public string DDUserId { get; set; }
    }

    /// <summary>
    /// 钉钉用户查询实体
    /// </summary>
    public class QueryDDUserMsg : PageReq
    {
        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartName { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public string UserPhone { get; set; }
    }

    public class DDDepartMsgParam
    { 
        public long dept_id { get; set; }


        public string language { get; set; }
    }

    public class DDDepartMsgResult
    { 
        public string request_id { get; set; }

        public int errcode { get; set; }

        public string errmsg { get; set; }

        public DDDepartGetResponse result { get; set; }
    }

    public class DDDepartGetResponse
    { 
        public long dept_id { get; set; }

        public string name { get; set; }

        public long parent_id { get; set; }

        public string source_identifier { get; set; }

        public bool create_dept_group { get; set; }

        public bool auto_add_user { get; set; }

        public bool auto_approve_apply { get; set; }

        public bool from_union_org { get; set; }

        public string tags { get; set; }

        public long order { get; set; }

        public string dept_group_chat_id { get; set; }

        public bool group_contain_sub_dept { get; set; }

        public string org_dept_owner { get; set; }

        public List<string> dept_manager_userid_list { get; set; }

        public bool outer_dept { get; set; }

        public List<int> outer_permit_depts { get; set; }

        public List<string> outer_permit_users { get; set; }

        public bool hide_dept { get; set; }

        public List<string> user_permits { get; set; }

        public List<int> dept_permits { get; set; }
    }
}
