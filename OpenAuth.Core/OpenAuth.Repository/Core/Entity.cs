using System;
using System.ComponentModel;

namespace OpenAuth.Repository.Core
{
    public abstract class Entity : BaseEntity
    {
    }

    public abstract class BaseEntity : BaseEntity<string>
    {
        /// <summary>
        /// 判断主键是否为空，常用做判定操作是【添加】还是【编辑】
        /// </summary>
        /// <returns></returns>
        public override bool KeyIsNull()
        {
            return string.IsNullOrEmpty(Id);
        }

        /// <summary>
        /// 创建默认的主键值
        /// </summary>
        public override void GenerateDefaultKeyVal()
        {

            Id = Guid.NewGuid().ToString();
        }

        public BaseEntity()
        {
            if (KeyIsNull())
            {
                GenerateDefaultKeyVal();
            }
        }
    }
    public abstract class BaseEntity<T>
    {
        [Browsable(false)]
        public T Id { get; set; }


        /// <summary>
        /// 判断主键是否为空，常用做判定操作是【添加】还是【编辑】
        /// </summary>
        /// <returns></returns>
        public abstract bool KeyIsNull();

        /// <summary>
        /// 创建默认的主键值
        /// </summary>
        public abstract void GenerateDefaultKeyVal();

        public BaseEntity()
        {
            if (KeyIsNull())
            {
                GenerateDefaultKeyVal();
            }
        }
    }
}
