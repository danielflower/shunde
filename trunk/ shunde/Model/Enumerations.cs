using System;
using System.Collections.Generic;
using System.Text;

namespace Shunde
{

    /// <summary>Search types for full-text searches</summary>
    public enum SearchType
    {

        /// <summary>All words must be contained</summary>
        And = 1,

        /// <summary>Any words must be contained</summary>
        Or = 2

    }

}
