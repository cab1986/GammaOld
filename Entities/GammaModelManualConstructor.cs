using System.Data.Common;

namespace Gamma.Entities
{
    public partial class GammaEntities
    {
        public GammaEntities(string connection)
            : base(connection)
        {
#if DEBUG
            this.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
#endif
        }
        public GammaEntities(DbConnection connection)
    : base(connection, false)
        {
#if DEBUG
            this.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
#endif
        }
    }
}
