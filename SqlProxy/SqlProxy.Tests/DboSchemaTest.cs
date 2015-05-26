using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace SqlProxy.Tests {
    [TestClass]
    public class DboSchemaTest {

        [TestMethod]
        public void CanCreateNewBookmark() {

            var proxy = SqlProxy.Create<DboSchema>();

            var success = proxy.SaveBookmark("TestBookMark", DateTime.Now) == 5;

            var exists = proxy.GetBookmarks("TestBookMark").Any();

            Assert.IsTrue(success, "Could not create bookmark");
        }

        [TestMethod]
        public void CanCreateAndSelectBookMarkWithFunc() {
            var proxy = SqlProxy.Create<DboSchema>();

            var success = proxy.SaveBookmark("AnotherBookMark", DateTime.Now) == 5;

            var exists = proxy.GetBookmarks("AnotherBookMark").Any();

            Assert.IsTrue(exists, "Could not find any bookmarks");
        }

        [TestMethod]
        public void CanCreateAndSelectBookMarkWithProc() {
            var proxy = SqlProxy.Create<DboSchema>();

            var success = proxy.SaveBookmark("AnotherBookMarkProc", DateTime.Now) == 5;

            var exists = proxy.GetBookmarksProc("AnotherBookMarkProc").Any();

            Assert.IsTrue(exists, "Could not find any bookmarks");
        }

        [TestMethod]
        public void CanCreateAndSelectSingleBookMarWithFunc() {
            var proxy = SqlProxy.Create<DboSchema>();

            var success = proxy.SaveBookmark("SingleAnotherBookMark", DateTime.Now) == 5;

            var exists = proxy.GetSingleBookmarks("SingleAnotherBookMark") != null;

            Assert.IsTrue(exists, "Could not find any bookmarks");
        }
    }
}
