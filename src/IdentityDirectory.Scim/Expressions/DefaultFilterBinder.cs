namespace IdentityDirectory.Scim.Query
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Expressions;
    using Klaims.Framework.Utility;
    using Services;

    public class DefaultFilterBinder : IFilterBinder
    {
        public Expression<Func<TResource, bool>> Bind<TResource>(ScimExpression filter, string sortBy, bool ascending, IAttributeNameMapper mapper = null)
        {
            return this.BuildExpression<TResource>(filter, mapper, null);
        }

        private Expression<Func<TResource, bool>> BuildExpression<TResource>(ScimExpression filter, IAttributeNameMapper mapper, string prefix)
        {
            var callExpression = filter as ScimCallExpression;
            if (callExpression!=null && (callExpression.OperatorName == "And" || callExpression.OperatorName == "Or"))
            {
                return this.BindLogicalExpression<TResource>(callExpression, mapper, prefix);
            }
          
            if (callExpression != null && (callExpression.OperatorName != "And" || callExpression.OperatorName != "Or"))
            {
                return this.BindAttributeExpression<TResource>(callExpression, mapper);
            }

            throw new InvalidOperationException("Unknown node type");
        }

        protected virtual Expression<Func<TResource, bool>> BindAttributeExpression<TResource>(ScimCallExpression expression, IAttributeNameMapper mapper)
        {
            var attributePathExpression = expression.Operands[0];
            var parameter = Expression.Parameter(typeof(TResource));
            var property = mapper.MapToInternal(expression.Operands[0].ToString())
                .Split('.')
                .Aggregate<string, Expression>(parameter,Expression.PropertyOrField);
            
            Expression binaryExpression = null;
            if (expression.OperatorName.Equals("Equal"))
            {
                // Workaround for missing coersion between String and Guid types.
                var propertyValue = property.Type == typeof(Guid) ? (object)Guid.Parse(expression.Operands[1].ToString()) : expression.Operands[1].ToString();
                binaryExpression = Expression.Equal(property, Expression.Convert(Expression.Constant(propertyValue), property.Type));
            }
            else if (expression.OperatorName.Equals("StartsWith"))
            {
                var method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                binaryExpression = Expression.Call(property, method, Expression.Convert(Expression.Constant(expression.Operands[1].ToString()), property.Type));
            }
            else if (expression.OperatorName.Equals("EndsWith"))
            {
                var method = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
                binaryExpression = Expression.Call(property, method, Expression.Convert(Expression.Constant(expression.Operands[1].ToString()), property.Type));
            }
            else if (expression.OperatorName.Equals("Contains"))
            {
                var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                binaryExpression = Expression.Call(property, method, Expression.Constant(expression.Operands[1].ToString(), typeof(string)));
            }
            else if (expression.OperatorName.Equals("GreaterThan"))
            {
                binaryExpression = Expression.GreaterThan(property, Expression.Convert(Expression.Constant(expression.Operands[1].ToString()), property.Type));
            }
            else if (expression.OperatorName.Equals("GreaterThanOrEqual"))
            {
                binaryExpression = Expression.GreaterThanOrEqual(property, Expression.Convert(Expression.Constant(expression.Operands[1].ToString()), property.Type));
            }
            else if (expression.OperatorName.Equals("LessThan"))
            {
                binaryExpression = Expression.LessThan(property, Expression.Convert(Expression.Constant(expression.Operands[1].ToString()), property.Type));
            }
            else if (expression.OperatorName.Equals("LessThanOrEqual"))
            {
                binaryExpression = Expression.LessThanOrEqual(property, Expression.Convert(Expression.Constant(expression.Operands[1].ToString()), property.Type));
            }
            else if (expression.OperatorName.Equals("Present"))
            {
                // If value cannot be null, then it is always present.
                if (IsNullable(property.Type))
                {
                    binaryExpression = Expression.NotEqual(property, Expression.Constant(null, property.Type));
                }
                else
                {
                    binaryExpression = Expression.Constant(true);
                }
            }
            if (binaryExpression == null)
            {
                throw new InvalidOperationException("Unsupported node operator");
            }
            
            return Expression.Lambda<Func<TResource, bool>>(binaryExpression, parameter);
        }

        protected virtual Expression<Func<TResource, bool>> BindLogicalExpression<TResource>(ScimCallExpression expression, IAttributeNameMapper mapper, string prefix)
        {
            var leftNodeExpression = this.BuildExpression<TResource>(expression.Operands[0], mapper, prefix);
            var rightNodeExpression = this.BuildExpression<TResource>(expression.Operands[1], mapper, prefix);

            if (expression.OperatorName.Equals("And"))
            {
                return leftNodeExpression.And(rightNodeExpression);
            }
            if (expression.OperatorName.Equals("Or"))
            {
                return leftNodeExpression.Or(rightNodeExpression);
            }

            throw new InvalidOperationException("Unsupported branch operator");
        }

        // TODO: Move to extensions
        private static bool IsNullable(Type type)
        {
            if (!type.GetTypeInfo().IsValueType) return true; // ref-type
            return Nullable.GetUnderlyingType(type) != null;
        }
    }
}