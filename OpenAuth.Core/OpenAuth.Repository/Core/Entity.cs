using System;
using System.ComponentModel;

namespace OpenAuth.Repository.Core
{
    public abstract class Entity : Entity<string>
    {
        public Entity()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
    public abstract class Entity<T>
    {
        [Browsable(false)]
        public T Id { get; set; }

        public Entity()
        {
        }
    }
}
