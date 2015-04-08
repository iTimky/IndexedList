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
            var indexedList = new IndexedList<TestObject>();
            indexedList.AddIndex(i => i.Id);
            indexedList.AddIndex(i => i.Id);
        }

        [TestMethod]
        public void AddIndexOnNonEmptyList()
        {
            var indexedList = new IndexedList<TestObject>();
            indexedList.AddIndex(i => i.Id);
            indexedList.Add(new TestObject());
            indexedList.AddIndex(o => o.Name);
            var indexes = indexedList.GetIndexes();
            
            Assert.AreEqual(1, indexes.First(i => i.FieldName == "Id").Count);
        }

        [TestMethod]
        public void RemoveItem()
        {
            var testObject = new TestObject();
            var indexedList = new IndexedList<TestObject>() {testObject, testObject};
            indexedList.AddIndex(o => o.Id);
            indexedList.Remove(testObject);
            var indexes = indexedList.GetIndexes();

            Assert.IsTrue(indexedList.Contains(testObject));
            Assert.AreEqual(1, indexedList.Count);
            Assert.AreEqual(1, indexes[0].Count);
        }
    }
}
