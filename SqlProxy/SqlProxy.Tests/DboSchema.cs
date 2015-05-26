using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlProxy.Tests {

    public class Bookmark {
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }

    [SchemaContext("TestDev", Name = "dbo")]
    public abstract class DboSchema {

        [SchemaStoredProc("Bookmark_Insert")]
        public abstract int SaveBookmark(string name, DateTime date);

        [SchemaFunction("Bookmark_Select")]
        public abstract List<Bookmark> GetBookmarks(string name);

        [SchemaStoredProc("Bookmark_Select_Proc")]
        public abstract List<Bookmark> GetBookmarksProc(string name);

        [SchemaFunction("Bookmark_Select")]
        public abstract Task<List<Bookmark>> GetBookmarksAsync(string name);

        [SchemaFunction("Bookmark_Select")]
        public abstract Bookmark GetSingleBookmarks(string name);
    }
}
