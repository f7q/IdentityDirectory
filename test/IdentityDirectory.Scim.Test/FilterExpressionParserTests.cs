namespace Klaims.Scim.Tests
{
    using System;
    using IdentityDirectory.Scim.Query;
    using Xunit;

    public class ScimFilterParserTests
    {
        const string SimpleFilter = "title pr and userType eq \"Employee\"";
        const string PathFilter = "name.familyName";
        const string PrecedenceFilter = "title pr and (userType eq \"Employee\" or userType eq \"ParttimeEmployee\")";
        const string CollectionFilter = "addresses[type eq \"work\"].streetAddress";

        // [Fact]
        public void CanParseSimpleFilter()
        {
            var rootNode = ScimExpressionParser.ParseExpression(SimpleFilter);
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
        }

         [Fact]
        public void CanParseFilterWithPrecedence()
        {
            var rootNode = ScimExpressionParser.ParseExpression(PrecedenceFilter);
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
        }

        [Fact]
        public void CanParsePathPrecedence()
        {
             var rootNode = ScimExpressionParser.ParseExpression(PathFilter);
             Assert.NotNull(rootNode);
             Console.WriteLine(rootNode);
        }

        [Fact]
        public void CanParseCollectionFilter()
        {
            var rootNode = ScimExpressionParser.ParseExpression(CollectionFilter);
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);

        }
    }
}
