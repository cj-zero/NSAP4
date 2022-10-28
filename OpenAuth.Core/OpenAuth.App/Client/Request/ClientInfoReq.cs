using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Client.Request
{
    /// <summary>
    /// 新增addmodel
    /// </summary>
    public class AddClientInfoReq
    {
        public int funcId { get; set; }
        public string keyId { get; set; }
        public string submitType { get; set; }
        /// <summary>
        /// 终端关系
        /// </summary>
        public string Terminals { get; set; }
        /// <summary>
        /// 请求类型add或者edit
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 线索单号
        /// </summary>
        public int baseEntry { get; set; }
        public ClientInfo clientInfo { get; set; }
    }

    public class UpdateClientJobReq
    {
        /// <summary>
        /// 审批id
        /// </summary>
        public string JobId { get; set; }
        /// <summary>
        /// 提交或是保存草稿
        /// </summary>
        public string submitType { get; set; }

        /// <summary>
        /// 终端关系
        /// </summary>
        public string Terminals { get; set; }
        /// <summary>
        /// 详情
        /// </summary>
        public ClientInfo clientInfo { get; set; }
    }

    public class ClientInfo
    {

        /// <summary>
        /// 帐套ID
        /// </summary>

        public string SboId { get; set; }

        /// <summary>
        /// 业务伙伴代码
        /// </summary>

        public string CardCode { get; set; }

        /// <summary>
        /// 业务伙伴名称
        /// </summary>

        public string CardName { get; set; }

        /// <summary>
        /// 外文名称
        /// </summary>

        public string CardFName { get; set; }

        /// <summary>
        /// 业务伙伴类型
        /// </summary>

        public string CardType { get; set; } = "C";

        /// <summary>
        /// 组代码
        /// </summary>

        public string GroupCode { get; set; }

        /// <summary>
        /// 公司个人
        /// </summary>

        public string CmpPrivate { get; set; }

        /// <summary>
        /// 默认开票到地址类型
        /// </summary>

        public string AddrType { get; set; }

        /// <summary>
        /// 默认运达到地址类型
        /// </summary>

        public string MailAddrTy { get; set; }

        /// <summary>
        /// 默认开票到地址标识
        /// </summary>

        public string Address { get; set; }

        /// <summary>
        /// 默认运达到地址标识
        /// </summary>

        public string MailAddres { get; set; }

        /// <summary>
        /// 收票方邮政编码
        /// </summary>

        public string ZipCode { get; set; }

        /// <summary>
        /// 送达方邮政编码
        /// </summary>

        public string MailZipCod { get; set; }

        /// <summary>
        /// 电话1
        /// </summary>

        public string Phone1 { get; set; }

        /// <summary>
        /// 电话2
        /// </summary>

        public string Phone2 { get; set; }

        /// <summary>
        /// 移动电话
        /// </summary>

        public string Cellular { get; set; }

        /// <summary>
        /// 传真
        /// </summary>

        public string Fax { get; set; }

        /// <summary>
        /// 电子邮件
        /// </summary>

        public string E_Mail { get; set; }

        /// <summary>
        /// 网站
        /// </summary>

        public string IntrntSite { get; set; }

        /// <summary>
        /// 联系人
        /// </summary>

        public string CntctPrsn { get; set; }

        /// <summary>
        /// 备注
        /// </summary>

        public string Notes { get; set; }

        /// <summary>
        /// 科目金额
        /// </summary>

        public string Balance { get; set; }

        /// <summary>
        /// 未清交货单余额
        /// </summary>

        public string DNotesBal { get; set; }

        /// <summary>
        /// 未清订单余额
        /// </summary>

        public string OrdersBal { get; set; }

        /// <summary>
        /// 未清机会
        /// </summary>

        public string OprCount { get; set; }

        /// <summary>
        /// 付款条款代码
        /// </summary>

        public string GroupNum { get; set; }

        /// <summary>
        /// 国税编号
        /// </summary>

        public string LicTradNum { get; set; }

        /// <summary>
        /// 价格清单编号
        /// </summary>

        public string ListNum { get; set; }

        /// <summary>
        /// 未清系统货币DN余额
        /// </summary>

        public string DNoteBalSy { get; set; }

        /// <summary>
        /// 未清系统货币订单余额
        /// </summary>

        public string OrderBalSy { get; set; }

        /// <summary>
        /// 自由文本
        /// </summary>

        public string FreeText { get; set; }

        /// <summary>
        /// 销售代表代码
        /// </summary>

        public string SlpCode { get; set; }

        /// <summary>
        /// 业务伙伴货币
        /// </summary>

        public string Currency { get; set; }

        /// <summary>
        /// 收款方详细地址
        /// </summary>

        public string Building { get; set; }

        /// <summary>
        /// 收货方详细地址
        /// </summary>

        public string MailBuildi { get; set; }

        /// <summary>
        /// 收款方城市
        /// </summary>

        public string City { get; set; }

        /// <summary>
        /// 收货城市
        /// </summary>

        public string MailCity { get; set; }

        /// <summary>
        /// 收款方省
        /// </summary>

        public string State1 { get; set; }

        /// <summary>
        /// 收货方所在省
        /// </summary>

        public string State2 { get; set; }

        /// <summary>
        /// 收款方国家
        /// </summary>

        public string Country { get; set; }

        /// <summary>
        /// 收货国家
        /// </summary>

        public string MailCountr { get; set; }

        /// <summary>
        /// 科目
        /// </summary>

        public string DflAccount { get; set; }

        /// <summary>
        /// 缺省分属
        /// </summary>

        public string DflBranch { get; set; }

        /// <summary>
        /// 银行名称
        /// </summary>

        public string BankCode { get; set; }

        /// <summary>
        /// 附加标识编号
        /// </summary>

        public string AddID { get; set; }

        /// <summary>
        /// 父类汇总类型
        /// </summary>

        public string FatherType { get; set; }

        /// <summary>
        /// 折扣对象
        /// </summary>

        public string DscntObjct { get; set; }

        /// <summary>
        /// 折扣比率
        /// </summary>

        public string DscntRel { get; set; }

        /// <summary>
        /// 数据源
        /// </summary>

        public string DataSource { get; set; }

        /// <summary>
        /// 优先级别OBPP
        /// </summary>

        public string Priority { get; set; }

        /// <summary>
        /// 信用卡
        /// </summary>

        public string CreditCard { get; set; }

        /// <summary>
        /// 信用卡编号
        /// </summary>

        public string CrCardNum { get; set; }

        /// <summary>
        /// 信用卡有效性
        /// </summary>

        public string CardValid { get; set; }

        /// <summary>
        /// 用户签名
        /// </summary>

        public string UserSign { get; set; }

        /// <summary>
        /// 本币对帐
        /// </summary>

        public string LocMth { get; set; }

        /// <summary>
        /// 活跃期间
        /// </summary>

        public string ValidFor { get; set; }

        /// <summary>
        /// 活跃开始日期
        /// </summary>

        public string ValidFrom { get; set; }

        /// <summary>
        /// 活跃结束日期
        /// </summary>

        public string ValidTo { get; set; }

        /// <summary>
        /// 冻结期间
        /// </summary>

        public string FrozenFor { get; set; }

        /// <summary>
        /// 冻结从
        /// </summary>

        public string FrozenFrom { get; set; }

        /// <summary>
        /// 冻结至
        /// </summary>

        public string FrozenTo { get; set; }

        /// <summary>
        /// 可用备注
        /// </summary>

        public string ValidComm { get; set; }

        /// <summary>
        /// 冻结注释
        /// </summary>

        public string FrozenComm { get; set; }

        /// <summary>
        /// 计税组
        /// </summary>

        public string VatGroup { get; set; }

        /// <summary>
        /// 日志实例
        /// </summary>

        public string LogInstanc { get; set; }

        /// <summary>
        /// 对象类型
        /// </summary>

        public string ObjType { get; set; }

        /// <summary>
        /// 标识
        /// </summary>

        public string Indicator { get; set; }

        /// <summary>
        /// 装运类型
        /// </summary>

        public string ShipType { get; set; }

        /// <summary>
        /// 应收/应付帐款
        /// </summary>

        public string DebPayAcct { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>

        public string DocEntry { get; set; }

        /// <summary>
        /// 开户行
        /// </summary>

        public string HouseBank { get; set; }

        /// <summary>
        /// 开户行国家
        /// </summary>

        public string HousBnkCry { get; set; }

        /// <summary>
        /// 开户行科目
        /// </summary>

        public string HousBnkAct { get; set; }

        /// <summary>
        /// 开户行分行
        /// </summary>

        public string HousBnkBrn { get; set; }

        /// <summary>
        /// 项目代码
        /// </summary>

        public string ProjectCod { get; set; }

        /// <summary>
        /// 统一国税编号
        /// </summary>

        public string VatIdUnCmp { get; set; }

        /// <summary>
        /// 代理商代码
        /// </summary>

        public string AgentCode { get; set; }

        /// <summary>
        /// 容差天数
        /// </summary>

        public string TolrncDays { get; set; }

        /// <summary>
        /// 本票
        /// </summary>

        public string SelfInvoic { get; set; }

        /// <summary>
        /// 递延税
        /// </summary>

        public string DeferrTax { get; set; }

        /// <summary>
        /// 免税信函号
        /// </summary>

        public string LetterNum { get; set; }

        /// <summary>
        /// 最大免税金额
        /// </summary>

        public string MaxAmount { get; set; }

        /// <summary>
        /// 免税有效期从
        /// </summary>

        public string FromDate { get; set; }

        /// <summary>
        /// 免税有效期至
        /// </summary>

        public string ToDate { get; set; }

        /// <summary>
        /// 应征预扣税
        /// </summary>

        public string WTLiable { get; set; }

        /// <summary>
        /// 证书号
        /// </summary>

        public string CrtfcateNO { get; set; }

        /// <summary>
        /// 到期日
        /// </summary>

        public string ExpireDate { get; set; }

        /// <summary>
        /// 登记号
        /// </summary>

        public string NINum { get; set; }

        /// <summary>
        /// 行业 - 待用
        /// </summary>

        public string Industry { get; set; }

        /// <summary>
        /// 行业
        /// </summary>

        public string IndustryC { get; set; }

        /// <summary>
        /// 业务
        /// </summary>

        public string Business { get; set; }

        /// <summary>
        /// 别名
        /// </summary>

        public string AliasName { get; set; }

        /// <summary>
        /// 售后主管
        /// </summary>

        public string DfTcnician { get; set; }

        /// <summary>
        /// 地域OTER
        /// </summary>

        public string Territory { get; set; }

        /// <summary>
        /// 金税登记号
        /// </summary>

        public string GTSRegNum { get; set; }

        /// <summary>
        /// 金税开户行及账号
        /// </summary>

        public string GTSBankAct { get; set; }

        /// <summary>
        /// 金税开票地址
        /// </summary>

        public string GTSBilAddr { get; set; }

        /// <summary>
        /// 开户行BIC/SWIFT码
        /// </summary>

        public string HsBnkSwift { get; set; }

        /// <summary>
        /// 开户行BIC/SWIFT码
        /// </summary>

        public string HsBnkIBAN { get; set; }

        /// <summary>
        /// 缺省银行BIC/SWIFT码
        /// </summary>

        public string DflSwift { get; set; }

        /// <summary>
        /// 拼音缩写
        /// </summary>

        public string U_PYSX { get; set; }

        /// <summary>
        /// 简称
        /// </summary>

        public string U_Name { get; set; }

        /// <summary>
        /// 外文简称
        /// </summary>

        public string U_FName { get; set; }

        /// <summary>
        /// 发票类别
        /// </summary>

        public string U_FPLB { get; set; }

        /// <summary>
        /// 销售量
        /// </summary>

        public string SalesVolume { get; set; }

        /// <summary>
        /// 服务费
        /// </summary>

        public string ServiceFees { get; set; }

        /// <summary>
        /// 是否待分配客户
        /// 0-待自动分配
        /// 1-待手动分配
        /// 2-已经分配
        /// </summary>

        public string WaitAssign { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>

        public string CreateDate { get; set; } = DateTime.Now.ToString();

        /// <summary>
        /// 修改时间
        /// </summary>

        public string UpdateDate { get; set; }

        /// <summary>
        /// 默认开票地址
        /// </summary>

        public string BillToDef { get; set; }

        /// <summary>
        /// 默认收货地址
        /// </summary>

        public string ShipToDef { get; set; }

        ///// <summary>
        ///// 属性 1
        ///// </summary>

        //public string QryGroup1 { get; set; }

        ///// <summary>
        ///// 属性 2
        ///// </summary>

        //public string QryGroup2 { get; set; }

        ///// <summary>
        ///// 属性 3
        ///// </summary>

        //public string QryGroup3 { get; set; }

        ///// <summary>
        ///// 属性 4
        ///// </summary>

        //public string QryGroup4 { get; set; }

        ///// <summary>
        ///// 属性 5
        ///// </summary>

        //public string QryGroup5 { get; set; }

        ///// <summary>
        ///// 属性 6
        ///// </summary>

        //public string QryGroup6 { get; set; }
        /// <summary>
        /// 业务伙伴操作类型
        /// </summary>

        public string ClientOperateType { get; set; }

        /// <summary>
        /// 自定义字段
        /// </summary>

        public string CustomFields { get; set; }

        /// <summary>
        /// 业务伙伴状态
        /// </summary>

        public string IsActive { get; set; }

        /// <summary>
        /// 前缀
        /// </summary>

        public string CardNamePrefix { get; set; }

        /// <summary>
        /// 核心名称
        /// </summary>

        public string CardNameCore { get; set; }

        /// <summary>
        /// 后缀
        /// </summary>

        public string CardNameSuffix { get; set; }

        /// <summary>
        /// 页面ID
        /// </summary>

        public string FuncId { get; set; }

        /// <summary>
        /// 销售员名称
        /// </summary>

        public string SlpName { get; set; }

        /// <summary>
        /// 售后主管【技术员编号】
        /// </summary>

        public string DfTcnicianCode { get; set; }

        /// <summary>
        /// 售后 技术员
        /// </summary>

        public string DfTcnicianHead { get; set; }

        /// <summary>
        /// 默认开票到地址[国家·省·市·详细地址]
        /// </summary>

        public string DefAddrBill { get; set; }

        /// <summary>
        /// 默认运达到地址[国家·省·市·详细地址]
        /// </summary>

        public string DefAddrShip { get; set; }

        /// <summary>
        /// 申请变更
        /// </summary>

        public string IsApplicationChange { get; set; }

        /// <summary>
        /// 变更类型
        /// </summary>

        public string ChangeType { get; set; }

        /// <summary>
        /// 变更的业务伙伴编码
        /// </summary>

        public string ChangeCardCode { get; set; }

        /// <summary>
        /// 是否中间商
        /// </summary>

        public string is_reseller { get; set; }

        /// <summary>
        /// 终端用户名
        /// </summary>

        public string EndCustomerName { get; set; }
        /// <summary>
        /// 终端用户联系人
        /// </summary>

        public string EndCustomerContact { get; set; }

        /// <summary>
        /// 联系人列表
        /// </summary>

        public List<clientOCPRReq> ContactList { get; set; }

        /// <summary>
        /// 地址列表
        /// </summary>

        public List<clientCRD1Req> AddrList { get; set; }

        /// <summary>
        /// 附件列表
        /// </summary>

        public List<billAttchmentReq> FilesDetails { get; set; }


        public string EshopUserId { get; set; }


        public List<clientAcct1Req> AcctList { get; set; }
        public string BankName { get; set; }
        /// <summary>
        /// 上级客户
        /// </summary>

        public string U_SuperClient { get; set; }
        /// <summary>
        /// 人员规模
        /// </summary>

        public string U_StaffScale { get; set; }
        /// <summary>
        /// 贸易类型
        /// </summary>

        public string U_TradeType { get; set; }
        /// <summary>
        /// 客户来源
        /// </summary>

        public string U_ClientSource { get; set; }
        /// <summary>
        /// 所属行业
        /// </summary>

        public string U_CompSector { get; set; }
        /// <summary>
        /// 新版客户类型
        /// </summary>
        public string U_CardTypeStr { get; set; }
    }
    public class clientCRD1Req
    {
        /// <summary>
        /// 序号
        /// </summary>

        public string SeqId { get; set; }

        /// <summary>
        /// 帐套ID
        /// </summary>

        public string SboId { get; set; }

        /// <summary>
        /// 业务伙伴代码
        /// </summary>

        public string CardCode { get; set; }

        /// <summary>
        /// 地址标识
        /// </summary>

        public string Address { get; set; }

        /// <summary>
        /// 邮政编码
        /// </summary>

        public string ZipCode { get; set; }

        /// <summary>
        /// 城市
        /// </summary>

        public string City { get; set; }

        /// <summary>
        /// 地区
        /// </summary>

        public string County { get; set; }

        /// <summary>
        /// 国家
        /// </summary>

        public string Country { get; set; }

        /// <summary>
        /// 省
        /// </summary>

        public string State { get; set; }

        /// <summary>
        /// 国家 - 编号
        /// </summary>

        public string CountryId { get; set; }

        /// <summary>
        /// 省 - 编号
        /// </summary>

        public string StateId { get; set; }

        /// <summary>
        /// 日志实例
        /// </summary>

        public string LogInstanc { get; set; }

        /// <summary>
        /// 对象类型
        /// </summary>

        public string ObjType { get; set; }

        /// <summary>
        /// 国税编号
        /// </summary>

        public string LicTradNum { get; set; }

        /// <summary>
        /// 行编号
        /// </summary>

        public string LineNum { get; set; }

        /// <summary>
        /// 税码
        /// </summary>

        public string TaxCode { get; set; }

        /// <summary>
        /// 大楼/楼层/房间
        /// </summary>

        public string Building { get; set; }

        /// <summary>
        /// 地址类型
        /// </summary>

        public string AdresType { get; set; }

        /// <summary>
        /// 地址2
        /// </summary>

        public string Address2 { get; set; }

        /// <summary>
        /// 地址3
        /// </summary>

        public string Address3 { get; set; }

        /// <summary>
        /// 是/否 可用
        /// </summary>

        public string Active { get; set; }

        /// <summary>
        /// 是否默认
        /// </summary>

        public string IsDefault { get; set; }

        /// <summary>
        /// 是否是推广员维护的
        /// </summary>
        public bool? isLims { get; set; } = false;
        /// <summary>
        /// 业务员编码
        /// </summary>
        public int? slpCode { get; set; }

    }
    public class clientOCPRReq
    {
        /// <summary>
        /// 序号
        /// </summary>

        public string SeqId { get; set; }

        /// <summary>
        /// 帐套ID
        /// </summary>

        public string SboId { get; set; }

        /// <summary>
        /// 联系人代码
        /// </summary>

        public string CntctCode { get; set; }

        /// <summary>
        /// 业务伙伴代码
        /// </summary>

        public string CardCode { get; set; }

        /// <summary>
        /// 联系人名称
        /// </summary>

        public string Name { get; set; }

        /// <summary>
        /// 职位
        /// </summary>

        public string Position { get; set; }

        /// <summary>
        /// 地址
        /// </summary>

        public string Address { get; set; }

        /// <summary>
        /// 电话1
        /// </summary>

        public string Tel1 { get; set; }

        /// <summary>
        /// 电话2
        /// </summary>

        public string Tel2 { get; set; }

        /// <summary>
        /// 移动电话
        /// </summary>

        public string Cellolar { get; set; }

        /// <summary>
        /// 传真
        /// </summary>

        public string Fax { get; set; }

        /// <summary>
        /// 电子邮件
        /// </summary>

        public string E_MailL { get; set; }

        /// <summary>
        /// 传呼机
        /// </summary>

        public string Pager { get; set; }

        /// <summary>
        /// 备注1
        /// </summary>

        public string Notes1 { get; set; }

        /// <summary>
        /// 备注2
        /// </summary>

        public string Notes2 { get; set; }

        /// <summary>
        /// 数据源
        /// </summary>

        public string DataSource { get; set; }

        /// <summary>
        /// 用户签名
        /// </summary>

        public string UserSign { get; set; }

        /// <summary>
        /// 密码
        /// </summary>

        public string Password { get; set; }

        /// <summary>
        /// 日志实例
        /// </summary>

        public string LogInstanc { get; set; }

        /// <summary>
        /// 对象类型
        /// </summary>

        public string ObjType { get; set; }

        /// <summary>
        /// 出生地
        /// </summary>

        public string BirthPlace { get; set; }

        /// <summary>
        /// 生日
        /// </summary>

        public string BirthDate { get; set; }

        /// <summary>
        /// 性别
        /// </summary>

        public string Gender { get; set; }

        /// <summary>
        /// 职业
        /// </summary>

        public string Profession { get; set; }

        /// <summary>
        /// 更新日期
        /// </summary>

        public string UpdateDate { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>

        public string UpdateTime { get; set; }

        /// <summary>
        /// 标题
        /// </summary>

        public string Title { get; set; }

        /// <summary>
        /// 出生城市
        /// </summary>

        public string BirthCity { get; set; }

        /// <summary>
        /// 名
        /// </summary>

        public string FirstName { get; set; }

        /// <summary>
        /// 中间名
        /// </summary>

        public string MiddleName { get; set; }

        /// <summary>
        /// 姓
        /// </summary>

        public string LastName { get; set; }

        /// <summary>
        /// 账号
        /// </summary>

        public string U_ACCT { get; set; }

        /// <summary>
        /// 开户行
        /// </summary>

        public string U_BANK { get; set; }

        /// <summary>
        /// 是/否 可用
        /// </summary>

        public string Active { get; set; }

        /// <summary>
        /// 是否 默认
        /// </summary>

        public string IsDefault { get; set; }
        /// <summary>
        /// 是否是推广员维护的
        /// </summary>
        public bool? isLims { get; set; } = false;
        /// <summary>
        /// 业务员编码
        /// </summary>
        public int? slpCode { get; set; }

    }
    public class billAttchmentReq
    {
        /// <summary>
        /// 附件ID
        /// </summary>

        public string fileId { get; set; }
        /// <summary>
        /// 附件类型
        /// </summary>

        public string filetype { get; set; }
        /// <summary>
        /// 附件类型Id
        /// </summary>

        public string filetypeId { get; set; }
        /// <summary>
        /// 附件名称
        /// </summary>

        public string filename { get; set; }
        /// <summary>
        /// 附件名称
        /// </summary>

        public string realName { get; set; }
        /// <summary>
        /// 附件备注
        /// </summary>

        public string remarks { get; set; }
        /// <summary>
        /// 附件下载路径
        /// </summary>

        public string filepath { get; set; }
        /// <summary>
        /// 附件预览路径
        /// </summary>

        public string attachPath { get; set; }
        /// <summary>
        /// 上传时间
        /// </summary>

        public string filetime { get; set; }
        /// <summary>
        /// 操作者
        /// </summary>

        public string username { get; set; }
        /// <summary>
        /// 用户Id
        /// </summary>

        public string fileUserId { get; set; }
    }
    public class clientAcct1Req
    {
        // <summary>
        /// 帐套ID
        /// </summary>

        public string SboId { get; set; }

        /// <summary>
        /// 业务伙伴代码
        /// </summary>

        public string CardCode { get; set; }
        /// <summary>
        /// 是/否 可用
        /// </summary>

        public string Active { get; set; }
        /// <summary>
        /// 账户类型
        /// </summary>

        public string AcctType { get; set; }
        /// <summary>
        /// 收款账号
        /// </summary>

        public string BankAcct { get; set; }
        /// <summary>
        /// 开户行
        /// </summary>


    }

    public class SaveCrmAuditInfoReq
    {
        public string AuditType { get; set; }
        public string CardCode { get; set; }
        public string DfTcnician { get; set; }
        public string JobId { get; set; }
    }

    public class AuditResubmitNextReq
    {
        public int jobId { get; set; }
        public string recommend { get; set; }
        public string auditOpinionid { get; set; }

    }
    /// <summary>
    /// 提交给我的相似客户
    /// </summary>
    public class CheckCardSimilarReq
    {
        public string qtype { get; set; }

        public string query { get; set; }

        public string sortname { get; set; }

        public string sortorder { get; set; }

        public string JobId { get; set; }

        public string SearchAll { get; set; }

    }

    //添加LIMS推广员
    public class AddLIMSInfo
    {
        public List<string> userIdList { get; set; }
        public string Type { get; set; }

        public int Count { get; set; } = 0;
    }

    //为客户绑定LIMS推广员
    public class AddLIMSInfoMap
    {
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public List<string> LimsIdList { get; set; }
    }

    public class DeleteLIMSInfoMap
    {
        public string CardCode { get; set; }
        public List<int> LimsIdList { get; set; }
    }

    public class SelLIMSByCodeReq : PageReq
    {
        public string CardCode { get; set; }
        public string Type { get; set; }
    }

    public class QueryClientInfoReq : PageReq
    {
        public int id { get; set; }
    }

    public class QueryLIMSInfoReq : PageReq
    {
        public string Type { get; set; }
        /// <summary>
        /// 是否客户详情页过来的数据
        /// </summary>
        public bool IsClientDetail { get; set; }
        public string CardCode { get; set; }
    }
    public class Getbase_entry
    {
        public string jobId { get; set; }
       
        public string CardCode { get; set; }
       
        public string Technician { get; set; }
        public string SlpName { get; set; }
    }
}
