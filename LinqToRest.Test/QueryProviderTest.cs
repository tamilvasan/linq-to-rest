using Messerli.LinqToRest.Test.Stub;
using Messerli.QueryProvider;
using Messerli.ServerCommunication;
using NSubstitute;
using RichardSzalay.MockHttp;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using RestQueryProvider = Messerli.LinqToRest.QueryProvider;

namespace Messerli.LinqToRest.Test
{
    public class QueryProviderTest
    {
        [Fact]
        public void ReturnsRestQuery()
        {
            var actual = CreateQuery<EntityWithQueryableMember>()
                .ToString();

            var expected = EntityWithQueryableMemberResult;

            Assert.Equal(actual, expected.Query);
        }

        [Fact]
        public void ReturnsRestQueryWithSelect()
        {
            var actual = CreateQuery<EntityWithQueryableMember>()
                .Select(entity => new { entity.Name })
                .ToString();

            var expected = UniqueIdentifierNameResult;

            Assert.Equal(actual, expected.Query);
        }

        [Fact]
        public void ReturnsRestQueryWithSelectedUniqueIdentifier()
        {
            var actual = CreateQuery<EntityWithQueryableMember>()
                .Select(entity => new { entity.UniqueIdentifier, entity.Name })
                .ToString();

            var expected = UniqueIdentifierNameResult;

            Assert.Equal(actual, expected.Query);
        }

        [Fact]
        public void ReturnsRestObject()
        {
            var actual = new QueryResult<object>(
                CreateQuery<EntityWithQueryableMember>());

            var expected = EntityWithQueryableMemberResult;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReturnsRestObjectWithSelect()
        {
            var actual = new QueryResult<object>(
                CreateQuery<EntityWithQueryableMember>()
                    .Select(entity => new { entity.Name }));

            var expected = UniqueIdentifierNameResult;

            Assert.Equal(actual, expected);
        }

        [Fact]
        public void ReturnsRestObjectWithSelectedUniqueIdentifier()
        {
            var actual = new QueryResult<object>(
                CreateQuery<EntityWithQueryableMember>()
                    .Select(entity => new { entity.UniqueIdentifier, entity.Name }));

            var expected = UniqueIdentifierNameResult;

            Assert.Equal(actual, expected);
        }

        #region Helper

        private static Query<T> CreateQuery<T>()
        {
            var serviceUri = MockServiceUri();
            var resourceRetriever = MockResourceRetriever();
            var objectResolver = MockObjectResolver();
            var queryBinderFactory = MockQueryBinderFactory();

            var queryProvider =
                new RestQueryProvider(resourceRetriever, objectResolver, queryBinderFactory, serviceUri);

            return new Query<T>(queryProvider);
        }

        #endregion

        #region Mock

        private static Uri MockServiceUri()
        {
            return RootUri;
        }

        private static HttpClient MockHttpClient()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .RegisterJsonResponse(EntityWithQueryableMemberRequestUri.ToString(), EntityWithQueryableMemberJson)
                .RegisterJsonResponse(UniqueIdentifierNameRequestUri.ToString(), UniqueIdentifierNameJson);

            return mockHttp.ToHttpClient();
        }

        private static IResourceRetriever MockResourceRetriever()
        {
            return new HttpResourceRetriever(MockHttpClient(), new JsonDeserializer(new EnumerableObjectCreator()));
        }

        private static IResourceRetriever AddUriMock<T>(IResourceRetriever resourceRetriever, Uri uri, object value)
        {
            resourceRetriever.RetrieveResource<T>(uri).Returns(Task.FromResult((T)value));

            return resourceRetriever;
        }

        private static IResourceRetriever AddUriMock(Type type, IResourceRetriever resourceRetriever, Uri uri,
            object[] value)
        {
            resourceRetriever.RetrieveResource(type, uri).Returns(Task.FromResult(value));

            return resourceRetriever;
        }

        private static IObjectResolver MockObjectResolver()
        {
            var queryableFactory = new QueryableFactory(MockQueryProviderFactory());

            return new QueryableObjectResolver(queryableFactory);
        }

        private static QueryProviderFactory MockQueryProviderFactory()
        {
            return new QueryProviderFactory(
                MockResourceRetriever(),
                // Todo: resolve circular dependency!
                new DefaultObjectResolver(),
                MockQueryBinderFactory(),
                MockServiceUri());
        }

        private static QueryBinderFactory MockQueryBinderFactory()
        {
            var queryBinderFactory = Substitute.For<QueryBinderFactory>();
            queryBinderFactory().Returns(new QueryBinder(new EntityValidator()));

            return queryBinderFactory;
        }

        #endregion

        #region Data

        private static Uri RootUri => new Uri("http://www.exapmle.com/api/v1/", UriKind.Absolute);

        private static Uri EntityWithQueryableMemberRequestUri => new Uri(RootUri, "entitywithqueryablemembers");

        private static object[] EntityWithQueryableMemberDeserialized => new[]
        {
            new EntityWithQueryableMember("Test1", null),
            new EntityWithQueryableMember("Test2", null)
        };

        private static string EntityWithQueryableMemberJson => @"
[
    {
        ""name"": ""Test1""
    },
    {
        ""name"": ""Test2""
    }
]
";

        private static QueryResult<object> EntityWithQueryableMemberResult => new QueryResult<object>(
            EntityWithQueryableMemberRequestUri,
            new object[]
            {
                new EntityWithQueryableMember(
                    "Test1",
                    new Query<EntityWithSimpleMembers>(
                        MockQueryProviderFactory().Create(new Uri(RootUri, "entitywithqueryablemembers/Test1/")))),
                new EntityWithQueryableMember(
                    "Test2",
                    new Query<EntityWithSimpleMembers>(
                        MockQueryProviderFactory().Create(new Uri(RootUri, "entitywithqueryablemembers/Test2/"))))
            });

        private static Uri UniqueIdentifierNameRequestUri =>
            new Uri(RootUri, "entitywithqueryablemembers?fields=uniqueIdentifier,name");

        private static object[] UniqueIdentifierNameDeserialized => new[]
        {
            new {UniqueIdentifier = "Test1", Name = "Test1"},
            new {UniqueIdentifier = "Test2", Name = "Test2"},
        };

        private static string UniqueIdentifierNameJson => @"
[
    {
        ""uniqueIdentifier"": ""Test1"",
        ""name"": ""Test1""
    },
    {
        ""uniqueIdentifier"": ""Test2"",
        ""name"": ""Test2""
    }
]
";

        private static QueryResult<object> UniqueIdentifierNameResult => new QueryResult<object>(
            UniqueIdentifierNameRequestUri,
            new object[]
            {
                new
                {
                    UniqueIdentifier = "Test1",
                    Name = "Test1"
                },
                new
                {
                    UniqueIdentifier = "Test2",
                    Name = "Test2"
                }
            });

        #endregion
    }

    internal static class Extension
    {
        public static MockHttpMessageHandler RegisterJsonResponse(
            this MockHttpMessageHandler httpMessageHandler,
            string route,
            string jsonResponse)
        {
            httpMessageHandler.When(route).Respond("application/json", jsonResponse);
            return httpMessageHandler;
        }
    }

}
