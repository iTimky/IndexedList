using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IndexedList
{
    class Program
    {
        static void Main(string[] args)
        {
            var newO = new Object() { Id = 1, Name = "SomeName" };
            Expression<Func<Object, bool>> expr1 = o => newO.Id.Equals(o.Id);
            Expression<Func<Object, bool>> expr2 = o => o.Id.Equals(newO.Id);
            Expression<Func<Object, bool>> expr3 = o => ReferenceEquals(o.Id, newO.Id);
            Expression<Func<Object, bool>> expr4 = o => A(o.Id, newO.Id);

            Expression<Func<Object, bool>> expr5 = o => o.Id == newO.Id;
            Expression<Func<Object, bool>> expr6 = o => o.Id <= newO.Id;
            Expression<Func<Object, bool>> expr7 = o => o.Id < newO.Id;
            Expression<Func<Object, bool>> expr8 = o => o.Id > newO.Id;
            Expression<Func<Object, bool>> expr9 = o => o.Id >= newO.Id;

            var fieldName1 = FieldNameSearcher.GetFieldName(expr1);
            var fieldName2 = FieldNameSearcher.GetFieldName(expr2);
            var fieldName3 = FieldNameSearcher.GetFieldName(expr3);
            var fieldName4 = FieldNameSearcher.GetFieldName(expr4);

            var fieldName5 = FieldNameSearcher.GetFieldName(expr5);
            var fieldName6 = FieldNameSearcher.GetFieldName(expr6);
            var fieldName7 = FieldNameSearcher.GetFieldName(expr7);
            var fieldName8 = FieldNameSearcher.GetFieldName(expr8);
            var fieldName9 = FieldNameSearcher.GetFieldName(expr9);


            var fieldName10 = FieldNameSearcher.GetFieldName((Object o) => string.Empty == o.Name);


            Console.WriteLine(fieldName1);
            Console.WriteLine(fieldName2);
            Console.WriteLine(fieldName3);
            Console.WriteLine(fieldName4);

            Console.WriteLine(fieldName5);
            Console.WriteLine(fieldName6);
            Console.WriteLine(fieldName7);
            Console.WriteLine(fieldName8);
            Console.WriteLine(fieldName9);
            Console.WriteLine(fieldName10);

            Console.ReadKey();
        }

        static bool A(int a, int b)
        {
            return true;
        }

        class Object
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return string.Format("Id: {0}, Name: {1}", Id, Name);
            }
        }
    }
}
