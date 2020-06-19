using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.Repository
{
    public class ContextType
    {
        public static Type DefaultContextType => typeof(OpenAuthDBContext);
        public static Type Nsap4NwcaliDbContextType => typeof(Nsap4NwcaliDbContext);
    }
}
