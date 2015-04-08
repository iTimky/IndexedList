using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IndexedList
{
    public abstract class ExpressionParser
    {
        private static readonly Dictionary<ExpressionType, ExpressionParser> Searchers = new Dictionary
            <ExpressionType, ExpressionParser>
        {
            {ExpressionType.Equal, new CompareExpressionParser()}
        };

        static ExpressionParser()
        {
            var expCompareFieldSearcher = new CompareExpressionParser();
            Searchers[ExpressionType.Equal] = expCompareFieldSearcher;
            Searchers[ExpressionType.LessThanOrEqual] = expCompareFieldSearcher;
            Searchers[ExpressionType.LessThan] = expCompareFieldSearcher;
            Searchers[ExpressionType.GreaterThan] = expCompareFieldSearcher;
            Searchers[ExpressionType.GreaterThanOrEqual] = expCompareFieldSearcher;

            Searchers[ExpressionType.Call] = new CallExpressionParser();

            Searchers[ExpressionType.MemberAccess] = new AccessExpressionParser();
        }

        public static string GetMemberName<TItem, TResult>(Expression<Func<TItem, TResult>> expression)
        {
            if (expression == null || !Searchers.ContainsKey(expression.Body.NodeType))
                return null;

            return Searchers[expression.Body.NodeType].FindFieldName(expression);
        }

        public static int? GetMemberHash<TItem>(Expression<Func<TItem, bool>> expression)
        {
            if (expression == null || !Searchers.ContainsKey(expression.Body.NodeType))
                return null;

            return Searchers[expression.Body.NodeType].FindFieldHash(expression);
        }

        protected abstract string FindFieldName<TItem, TResult>(Expression<Func<TItem, TResult>> expression);
        protected abstract int? FindFieldHash<TItem>(Expression<Func<TItem, bool>> expression);
    }

    #region ExpressionParser Implementations

    public class CompareExpressionParser : ExpressionParser
    {
        protected override string FindFieldName<TItem, TResult>(Expression<Func<TItem, TResult>> expression)
        {
            ParameterExpression param = expression.Parameters[0];
            var binaryExpression = (BinaryExpression) expression.Body;
            var left = binaryExpression.Left as MemberExpression;
            if (left != null && left.Expression == param)
                return left.Member.Name;

            var right = binaryExpression.Right as MemberExpression;
            if (right != null && right.Expression == param)
                return right.Member.Name;

            return null;
        }

        protected override int? FindFieldHash<TItem>(Expression<Func<TItem, bool>> expression)
        {
            var binaryExpression = (BinaryExpression) expression.Body;
            var leftConst = binaryExpression.Left as ConstantExpression;
            if (leftConst != null)
                return leftConst.Value.GetHashCode();

            var rightConst = binaryExpression.Right as ConstantExpression;
            if (rightConst != null)
                return rightConst.Value.GetHashCode();

            ParameterExpression param = expression.Parameters[0];
            var left = binaryExpression.Left as MemberExpression;
            if (left != null && left.Expression != param)
            {
                object value = GetValue(left);
                if (value != null)
                    return value.GetHashCode();
            }

            var right = binaryExpression.Right as MemberExpression;
            if (right != null && right.Expression != param)
            {
                object value = GetValue(right);
                if (value != null)
                    return value.GetHashCode();
            }

            var leftNew = binaryExpression.Left as NewExpression;
            if (leftNew != null)
            {
                object value = CreateInstance(leftNew);
                if (value != null)
                    return value.GetHashCode();
            }

            var rightNew = binaryExpression.Right as NewExpression;
            if (rightNew != null)
            {
                object value = CreateInstance(rightNew);
                if (value != null)
                    return value.GetHashCode();
            }

            return null;
        }


        private object GetValue(MemberExpression exp)
        {
            var names = new List<string>();
            names.Add(exp.Member.Name);

            var expressionList = new List<Expression>();
            expressionList.Add(exp.Expression);
            Expression lastExpression = exp.Expression;
            var memberExpression = exp.Expression as MemberExpression;

            while (memberExpression != null)
            {
                names.Add(memberExpression.Member.Name);
                lastExpression = memberExpression.Expression;
                expressionList.Add(memberExpression.Expression);
                memberExpression = memberExpression.Expression as MemberExpression;
            }

            var constantExpression = lastExpression as ConstantExpression;
            if (constantExpression != null)            
                return ReflectValue(constantExpression.Value, names);


            var memberInitExpression = lastExpression as MemberInitExpression;
            if (memberInitExpression != null && memberInitExpression.Bindings.Count != 0)
            {
                var bind = memberInitExpression.Bindings.FirstOrDefault(b => b.Member.Name == names.Last()) as MemberAssignment;
                if (bind != null)
                {
                    names.RemoveAt(names.Count - 1);
                    memberExpression = bind.Expression as MemberExpression;
                    while (memberExpression != null)
                    {
                        names.Add(memberExpression.Member.Name);
                        lastExpression = memberExpression.Expression;
                        memberExpression = memberExpression.Expression as MemberExpression;
                    }

                    constantExpression = lastExpression as ConstantExpression;
                    if (constantExpression != null)
                        return ReflectValue(constantExpression.Value, names);
                }
            }

            var newExpression = lastExpression as NewExpression;
            if (newExpression != null)
            {
                var rootObj = CreateInstance(newExpression);
                return ReflectValue(rootObj, names);
            }

            return null;
        }

        private object CreateInstance(NewExpression newExpression)
        {
            var createObject = Expression.Lambda(newExpression).Compile();
            return createObject.DynamicInvoke();
        }

        private object ReflectValue(object rootObj, IEnumerable<string> names)
        {
            object obj = rootObj;
            var nameList = names.Reverse().ToList();
            for (int i = 0; i < nameList.Count; i++)
            {
                var name = nameList[i];
                Type type = obj.GetType();
                var propertyInfo = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (propertyInfo != null)
                {
                    if (Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null)
                        i++;

                    obj = propertyInfo.GetValue(obj);
                }
                else
                {
                    var fieldInfo = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (Nullable.GetUnderlyingType(fieldInfo.FieldType) != null)
                        i++;

                    obj = fieldInfo.GetValue(obj);
                }
                
            }

            return obj;
        }
    }

    public class CallExpressionParser : ExpressionParser
    {
        protected override string FindFieldName<TItem, TResult>(Expression<Func<TItem, TResult>> expression)
        {
            ParameterExpression param = expression.Parameters[0];
            var call = (MethodCallExpression) expression.Body;
            if (call.Object != null)
            {
                var obj = (MemberExpression) call.Object;
                if (obj.Expression == param)
                    return obj.Member.Name;

                var arg =
                    (MemberExpression) call.Arguments.FirstOrDefault(a => ((MemberExpression) a).Expression == param);
                if (arg != null)
                    return arg.Member.Name;
            }
            else if (call.Arguments.Any())
            {
                var memberExpression = call.Arguments[0] as MemberExpression;
                if (memberExpression != null)
                {
                    var arg =
                        (MemberExpression)
                            call.Arguments.FirstOrDefault(a => ((MemberExpression) a).Expression == param);
                    if (arg != null)
                        return arg.Member.Name;
                }

                var unaryExpression = call.Arguments[0] as UnaryExpression;
                if (unaryExpression != null)
                {
                    MemberExpression arg = call.Arguments.Select(e => (MemberExpression) ((UnaryExpression) e).Operand)
                        .FirstOrDefault(o => o.Expression == param);
                    if (arg != null)
                        return arg.Member.Name;
                }
            }

            return null;
        }

        protected override int? FindFieldHash<TItem>(Expression<Func<TItem, bool>> expression)
        {
            throw new NotImplementedException();
        }
    }

    public class AccessExpressionParser : ExpressionParser
    {
        protected override string FindFieldName<TItem, TResult>(Expression<Func<TItem, TResult>> expression)
        {
            if (expression.Body.NodeType != ExpressionType.MemberAccess)
                return null;

            return ((MemberExpression) expression.Body).Member.Name;
        }

        protected override int? FindFieldHash<TItem>(Expression<Func<TItem, bool>> expression)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}