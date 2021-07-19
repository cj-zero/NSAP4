using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.Repository
{
    public class ContextType
    {
        public static Type DefaultContextType => typeof(OpenAuthDBContext);
        public static Type Nsap4NwcaliDbContextType => typeof(Nsap4NwcaliDbContext);
        public static Type Nsap4MaterialDbContextType => typeof(Nsap4MaterialDbContext);
        public static Type Nsap4ServeDbContextType => typeof(Nsap4ServeDbContext);
        public static Type Nsap4SettlementContextType => typeof(Nsap4SettlementContext);

        public static Type NsapBoneDbContextType => typeof(NsapBoneDbContext);
        public static Type SapDbContextType => typeof(SapDbContext);
        public static Type NsapBaseDbContext => typeof(NsapBaseDbContext);
    }
}
