using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlProxy.Tests {
    [TestClass]
    public class StoredProcTest {

        [TestMethod]
        public void CanCreateNewBookmark() {
            var success = SqlProxy.Create<DboSchema>().SaveBookmark("TestBookMark", DateTime.Now);
            Assert.IsTrue(success, "Could not create bookmark");
        }
    }
}
