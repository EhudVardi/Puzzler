using System;
using System.Collections.Generic;

namespace Common.Models.Base
{
    public class BoardGeneric<TGroup, TValueCell, TGroupHolder> : BoardGenericBase<CellBase, TGroup, TValueCell, TGroupHolder>
        where TGroup       : class
        where TValueCell   : class
        where TGroupHolder : class
    {
    }
}
