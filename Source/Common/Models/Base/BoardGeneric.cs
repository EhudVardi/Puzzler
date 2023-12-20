using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.Base
{
    public class BoardGeneric<G, VC, GHC> : BoardGenericBase<CellBase, G, VC, GHC>
        where G : class
        where VC : class
        where GHC : class
    {

    }
}
