using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IndexedList
{
    public abstract class FieldNameSearcher
    {
        private static readonly Dictionary<ExpressionType, FieldNameSearcher> Searchers = new Dictionary<ExpressionType, FieldNameSearcher>
        {
            {ExpressionType.Equal, new CompareFieldNameSearcher()}
        };

        static FieldNameSearcher()
        {
            var expCompareFieldSearcher = new CompareFieldNameSearcher();
            Searchers[ExpressionType.Equal] = expCompareFieldSearcher;
            Searchers[ExpressionType.LessThanOrEqual] = expCompareFieldSearcher;
            Searchers[ExpressionType.LessThan] = expCompareFieldSearcher;
            Searchers[ExpressionType.GreaterThan] = expCompareFieldSearcher;
            Searchers[ExpressionType.GreaterThanOrEqual] = expCompareFieldSearcher;

            Searchers[ExpressionType.Call] = new CallFieldNameSearcher();

            Searchers[ExpressionType.MemberAccess] = new AccessFiledNameSearcher();
        }

        public static string GetFieldName<TItem, TResult>(Expression<Func<TItem, TResult>> expression)
        {
            if (expression == null || !Searchers.ContainsKey(expression.Body.NodeType))
                return null;

            return Searchers[expression.Body.NodeType].FindFieldName(expression);
        }

        protected abstract string FindFieldName<TItem, TResult>(Expression<Func<TItem, TResult>> expression);
    }

    #region FieldNameSearcher Implementations
    public class CompareFieldNameSearcher : FieldNameSearcher
    {
        protected override string FindFieldName<TItem, TResult>(Expression<Func<TItem, TResult>> expression)
        {
            var param = expression.Parameters[0];
            var left = ((BinaryExpression)expression.Body).Left as MemberExpression;
            if (left != null && left.Expression == param)
                return left.Member.Name;

            var right = ((BinaryExpression)expression.Body).Right as MemberExpression;
            if (right != null && right.Expression == param)
                return right.Member.Name;

            return null;
        }
    }

    public class CallFieldNameSearcher : FieldNameSearcher
    {
        protected override string FindFieldName<TItem, TResult>(Expression<Func<TItem, TResult>> expression)
        {
            var param = expression.Parameters[0];
            var call = (MethodCallExpression)expression.Body;
            if (call.Object != null)
            {
                var obj = (MemberExpression)call.Object;
                if (obj.Expression == param)
                    return obj.Member.Name;

                var arg = (MemberExpression)call.Arguments.FirstOrDefault(a => ((MemberExpression)a).Expression == param);
                if (arg != null)
                    return arg.Member.Name;
            }
            else if (call.Arguments.Any())
            {
                var memberExpression = call.Arguments[0] as MemberExpression;
                if (memberExpression != null)
                {
                    var arg = (MemberExpression)call.Arguments.FirstOrDefault(a => ((MemberExpression)a).Expression == param);
                    if (arg != null)
                        return arg.Member.Name;
                }

                var unaryExpression = call.Arguments[0] as UnaryExpression;
                if (unaryExpression != null)
                {
                    var arg = call.Arguments.Select(e => (MemberExpression)((UnaryExpression)e).Operand)
                            .FirstOrDefault(o => o.Expression == param);
                    if (arg != null)
                        return arg.Member.Name;
                }
            }

            return null;
        }
    }

    public class AccessFiledNameSearcher : FieldNameSearcher
    {
        protected override string FindFieldName<TItem, TResult>(Expression<Func<TItem, TResult>> expression)
        {
            var member = ((MemberExpression)expression.Body).Member;

            return member.Name;
        }
    }
    #endregion 
}