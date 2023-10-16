using DeepEqual;
using DeepEqual.Syntax;
using System.Linq.Expressions;

namespace Arco.Duplication;

internal static class Comparisons
{
    internal static Expression<Func<T, object>> CreateMemberExpression<T>(string memberName)
    {
        ParameterExpression param = Expression.Parameter(typeof(T));
        Expression memberExpression = Expression.PropertyOrField(param, memberName);

        if (memberExpression.Type != typeof(object))
        {
            memberExpression = Expression.Convert(memberExpression, typeof(object));
        }

        return Expression.Lambda<Func<T, object>>(memberExpression, param);
    }
}