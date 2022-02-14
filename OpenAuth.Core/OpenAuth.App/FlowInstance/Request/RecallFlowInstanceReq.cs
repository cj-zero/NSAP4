namespace OpenAuth.App.Request
{
    /// <summary>
    /// 召回、撤销流程
    /// </summary>
    public class RecallFlowInstanceReq
    {
        /// <summary>
        /// 召回、撤销的流程实例ID
        /// </summary>
        public string FlowInstanceId { get; set; }
        /// <summary>
        /// 撤回备注
        /// </summary>
        public string Description { get; set; }


        /// <summary>
        /// 驳回的步骤，即驳回到的节点ID
        /// </summary>
        public string NodeRejectStep { get; set; }

        /// <summary>
        /// 驳回类型。null:使用节点配置的驳回类型/0:前一步/1:第一步/2：指定节点，使用NodeRejectStep
        /// </summary>
        public string NodeRejectType { get; set; }
    }
}
