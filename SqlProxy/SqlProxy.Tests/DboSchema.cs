using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlProxy.Tests {

    [SchemaContext("TestDev", Name = "dbo")]
    public abstract class DboSchema {

        [SchemaMethod("Bookmark_Insert")]
        public abstract bool SaveBookmark(string name, DateTime date);
    }
}
