using System;
using System.Data;
using System.Linq;
using IndexedList;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class IndexTest
    {
        class TestObject
        {
            public int Id;
            public string Name { get; set; }
        }


        [TestMethod]
        [ExpectedException(typeof(DuplicateNameException))]
        public void DuplicateIndexTest()
        {
            var indexedList = new IndexedList<TestObject>(i => i.Id, i => i.Id);
        }

        [TestMethod]
        public void AddIndexOnNonEmptyList()
        {
            var testObjects = new IndexedList<TestObject>(i => i.Id);
            testObjects.Add(new TestObject());
            testObjects.AddIndex(o => o.Name);
            var indexes = testObjects.GetIndexes();
            
            Assert.AreEqual(1, indexes.First(i => i.FieldName == "Id").Count);
        }

        [TestMethod]
        public void RemoveItem()
        {
            var testObjects = new IndexedList<TestObject>() {new TestObject(), new TestObject()};
            testObjects.AddIndex(o => o.Id);
            testObjects.RemoveAt(0);
            var indexes = testObjects.GetIndexes();
            indexes.ForEach(i => Assert.AreEqual(0, i.Count));
        }
    }
}
