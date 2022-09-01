using System;
using System.Linq.Expressions;

namespace CopyDat.Core.Builders
{
    public class ExpressionBuilder
    {
        public static Expression CreateEquals<TLeft, TRight>(Expression<Func<TLeft, TRight>> propertySelector, TRight value)
        {
            var leftType = typeof(TLeft);
            var rightType = typeof(TRight);
            var propertySelectorBody = (MemberExpression)propertySelector.Body;
            string name;
            if (propertySelectorBody.Expression is UnaryExpression ue)
            {
                name = ((ParameterExpression)ue.Operand).Name;
                var entityParameter = Expression.Parameter(leftType, name);
                var s = Expression.Property(entityParameter, leftType, propertySelectorBody.Member.Name);

                var valueParameter = Expression.Constant(value, rightType);
                var binaryExpression = Expression.Equal(s, valueParameter);
                return Expression.Lambda(binaryExpression, new[] { entityParameter });
            }
            else
            {
                name = ((ParameterExpression)propertySelectorBody.Expression).Name;
                var entityParameter = Expression.Parameter(leftType, name);
                var valueParameter = Expression.Constant(value, rightType);
                var binaryExpression = Expression.Equal(propertySelectorBody, valueParameter);
                return Expression.Lambda(binaryExpression, new[] { entityParameter });
            }

        }
    }
}
