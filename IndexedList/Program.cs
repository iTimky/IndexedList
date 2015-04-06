using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IndexedList
{
    class Program
    {
        private static List<ManyToMany> list;
        private static IndexedList<ManyToMany> indexedList;
        private static List<ManyToMany> manyToManys;
        private static List<ManyToMany> resultsFromlist = new List<ManyToMany>();
        private static List<ManyToMany> resultsFromindexed = new List<ManyToMany>();


        private static int Count = 1000000;

        static void Main(string[] args)
        {
            Init();

            for (int i = 1; i <= Count; i *= 10)
                Test(i);
            
            Console.WriteLine("Done");
            Console.ReadKey();
        }

        static void Test(int count)
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();
                list = new List<ManyToMany>();
                list.AddRange(manyToManys.Take(count));
            stopwatch.Stop();
            Console.WriteLine("List init {0}: {1} ms", count, stopwatch.ElapsedMilliseconds);


            stopwatch.Reset();

            stopwatch.Start();
                indexedList = new IndexedList<ManyToMany>();
                indexedList.AddRange(manyToManys.Take(count));
                indexedList.AddIndex(mtm => mtm.FirstId);
            stopwatch.Stop();
            Console.WriteLine("IndexedList init {0}: {1} ms", count, stopwatch.ElapsedMilliseconds);


//            stopwatch.Reset();
//
//            stopwatch.Start();
//                indexedList.AddRange(manyToManys.Take(1000000));
//            stopwatch.Stop();
//            Console.WriteLine("IndexedList  add 1000000: {0} ms", stopwatch.ElapsedMilliseconds);

            
            stopwatch.Reset();


            stopwatch.Start();
            //foreach (var manyToMany in list)
            //    resultsFromlist.AddRange(list.Where(l => l.FirstId == manyToMany.FirstId).ToList());
                list.Where(l => l.FirstId == 1000).ToList();
            stopwatch.Stop();
            Console.WriteLine("List {0}: {1} ms", count, stopwatch.ElapsedMilliseconds);

            stopwatch.Reset();

            stopwatch.Start();
//            foreach (var manyToMany in indexedList)
//                resultsFromlist.AddRange(indexedList.Where(l => l.FirstId == manyToMany.FirstId).ToList());
                indexedList.Where(l => l.FirstId == 1000).ToList();
            stopwatch.Stop();
            Console.WriteLine("IndexedList {0}: {1} ms", count, stopwatch.ElapsedMilliseconds);
            Console.WriteLine();

            list.Clear();
            indexedList.Clear();
        }


        static void Init()
        {
            list = new List<ManyToMany>(Count);
            indexedList = new IndexedList<ManyToMany>();
            //indexedList.AddIndex(mtm => mtm.FirstId);

            manyToManys = new List<ManyToMany>();
            for (int i = 0; i < Count; i++)
                manyToManys.Add(new ManyToMany(i, Count - i));
        }


        class ManyToMany
        {
            public ManyToMany(int firstId, int secondId)
            {
                FirstId = firstId;
                SecondId = secondId;
            }

            public int FirstId { get; set; }
            public int SecondId { get; set; }
        }

        class First
        {
            public First(int id)
            {
                Id = id;
            }

            public int Id { get; set; }
            //public List<Second> Seconds = new List<Second>();
        }
//
//        class Second
//        {
//            public Second(int id)
//            {
//                Id = id;
//            }
//
//            public int Id { get; set; } 
//            public List<First> Firsts = new List<First>(); 
//        }

//        class Object
//        {
//            public int Id { get; set; }
//            public int ParentId { get; set; }
//            public string Name { get; set; }
//            public Object obj { get; set; }
//
//            public override string ToString()
//            {
//                return string.Format("Id: {0}, Name: {1}", Id, Name);
//            }
//        }
    }
}
