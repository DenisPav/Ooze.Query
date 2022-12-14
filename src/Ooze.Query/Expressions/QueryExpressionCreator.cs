using System.ComponentModel;
using System.Linq.Expressions;
using Ooze.Query.Exceptions;
using Ooze.Query.Filters;
using Ooze.Query.Tokenization;
using Superpower.Model;

namespace Ooze.Query.Expressions;

internal static class QueryExpressionCreator
{
    private const string ParameterName = "x";
    private const string Where = nameof(Where);

    public static ExpressionResult Create<TEntity>(
        IEnumerable<QueryFilterDefinition<TEntity>> filterDefinitions,
        Expression queryExpression,
        IEnumerable<Token<QueryToken>> queryDefinitionTokens)
    {
        var parameterExpression = Expression.Parameter(
            typeof(TEntity),
            ParameterName
        );

        var tokenStack = new Stack<Token<QueryToken>>(queryDefinitionTokens.Reverse());
        var bracketStack = new Stack<Token<QueryToken>>();

        try
        {
            var createdExpr = CreateExpression(filterDefinitions, parameterExpression, tokenStack, bracketStack);
            if (bracketStack.Any())
                throw new ExpressionCreatorException("no matching ending bracket");

            var finalExpression = Expression.Lambda<Func<TEntity, bool>>(createdExpr, parameterExpression);
            var quoteExpr = Expression.Quote(finalExpression);
            var callExpr = Expression.Call(typeof(Queryable), Where, new[] { typeof(TEntity) }, queryExpression,
                quoteExpr);

            return new ExpressionResult(callExpr, null);
        }
        catch (Exception e)
        {
            return new ExpressionResult(null, new ExpressionCreatorException(e.Message));
        }
    }

    private static Expression CreateExpression<TEntity>(
        IEnumerable<QueryFilterDefinition<TEntity>> filterDefinitions,
        ParameterExpression parameterExpression,
        Stack<Token<QueryToken>> tokenStack,
        Stack<Token<QueryToken>> bracketStack)
    {
        var propertyExpressions = new List<Expression>();
        var logicalOperationExpressions = new List<Func<Expression, Expression, Expression>>();

        while (tokenStack.Any())
        {
            var token = tokenStack.Peek();
            switch (token.Kind)
            {
                case QueryToken.Property:
                    CreateMemberExpression(filterDefinitions, parameterExpression, tokenStack,
                        propertyExpressions);
                    break;
                case QueryToken.LogicalOperation:
                    tokenStack.Pop();
                    logicalOperationExpressions.Add(Expression.AndAlso);
                    break;
                case QueryToken.BracketLeft:
                    tokenStack.Pop();
                    bracketStack.Push(token);
                    var subExpr = CreateExpression(filterDefinitions, parameterExpression, tokenStack, bracketStack);
                    propertyExpressions.Add(subExpr);
                    break;
                case QueryToken.BracketRight:
                    tokenStack.Pop();
                    if (bracketStack.Any() == false)
                        throw new ExpressionCreatorException("no matching start bracket");

                    bracketStack.Pop();
                    return CreateLogicalExpression(logicalOperationExpressions, propertyExpressions);
                case QueryToken.Operation:
                case QueryToken.Value:
                    throw new ExpressionCreatorException("wrong order of tokens found in query");
            }
        }

        return CreateLogicalExpression(logicalOperationExpressions, propertyExpressions);
    }

    private static Expression CreateLogicalExpression(
        IList<Func<Expression, Expression, Expression>> logicalOperationExpressions,
        IEnumerable<Expression> propertyExpressions)
    {
        if (logicalOperationExpressions.Any() == false)
            return propertyExpressions.FirstOrDefault();

        var skip = 0;
        var expression = logicalOperationExpressions.Aggregate((Expression)null, (agg, current) =>
        {
            if (agg == null)
            {
                var args = propertyExpressions.Skip(skip).Take(2).ToArray();
                skip += 2;
                return current(args[0], args[1]);
            }
            else
            {
                var args = propertyExpressions.Skip(skip).Take(1).ToArray();
                skip += 1;
                return current(agg, args[0]);
            }
        });
        return expression;
    }

    /// <summary>
    /// Take next 3 tokens and create member expression from them.
    /// </summary>
    /// <param name="parameterExpression">Root parameter used for building expression</param>
    /// <param name="tokens">Current stack of tokens</param>
    /// <param name="propertyExpressions">List of member current member expressions</param>
    /// <typeparam name="TEntity">Type of Queryable instance</typeparam>
    private static void CreateMemberExpression<TEntity>(
        IEnumerable<QueryFilterDefinition<TEntity>> filterDefinitions,
        ParameterExpression parameterExpression,
        Stack<Token<QueryToken>> tokens,
        ICollection<Expression> propertyExpressions)
    {
        var token = tokens.Pop();
        var queryPropertyName = token.ToStringValue();
        var property = filterDefinitions.Single(definition =>
                string.Compare(queryPropertyName, definition.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
            .TargetProperty;
        var currentMemberAccess = Expression.MakeMemberAccess(parameterExpression, property);
        token = tokens.Pop();
        var operationExpressionFactory = Operations.OperatorExpressionFactories[token.ToStringValue()];
        token = tokens.Pop();

        var value = token.ToStringValue();
        var clearValue = value.Replace(QueryTokenizer.ValueTick, string.Empty);
        // var convertedValue = Convert.ChangeType(clearValue, property.PropertyType);
        var convertedValue = TypeDescriptor.GetConverter(property.PropertyType).ConvertFrom(clearValue);
        var valueExpression = Expression.Constant(convertedValue);

        var finalOperationExpression = operationExpressionFactory(currentMemberAccess, valueExpression);
        propertyExpressions.Add(finalOperationExpression);
    }
}

internal record class ExpressionResult(Expression Expression, Exception Error);