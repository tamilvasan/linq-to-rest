﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Messerli.LinqToRest.Test.Stub;
using Messerli.QueryProvider;
using NSubstitute;
using Xunit;

namespace Messerli.LinqToRest.Test
{
    public class ResourceRetrieverTest
    {
        [Fact]
        public void ReturnsRestObject()
        {
            var resourceRetriever = CreateResourceRetriever();

            var uri = new Uri(EntityWithQueryableMemberResult.Query, UriKind.Absolute);
            var actual = resourceRetriever.RetrieveResource<IEnumerable<EntityWithQueryableMember>>(uri);

            Assert.Equal(EntityWithQueryableMemberResult.Object, actual);
        }

        [Fact]
        public void ReturnsRestObjectWithSelect()
        {
            var resourceRetriever = CreateResourceRetriever();

            var uri = new Uri(NameResult.Query, UriKind.Absolute);
            var type = typeof(IEnumerable<>).MakeGenericType(new { Name = string.Empty }.GetType());
            var actual = resourceRetriever.RetrieveResource(type, uri);

            Assert.Equal(NameResult.Object, actual);
        }

        [Fact]
        public void ReturnsRestObjectWithSelectedUniqueIdentifier()
        {
            var resourceRetriever = CreateResourceRetriever();

            var uri = new Uri(UniqueIdentifierNameResult.Query, UriKind.Absolute);
            var type = typeof(IEnumerable<>).MakeGenericType(new { UniqueIdentifier = string.Empty, Name = string.Empty }.GetType());
            var actual = resourceRetriever.RetrieveResource(type, uri);

            Assert.Equal(UniqueIdentifierNameResult.Object, actual);
        }

        private static ResourceRetriever CreateResourceRetriever()
        {
            return new ResourceRetriever(
                MockHttpClient(),
                new QueryableFactory(
                    new QueryProvider(
                        Substitute.For<IResourceRetriever>(),
                        null,
                        () => new QueryBinder(new EntityValidator()),
                        RootUri)));
        }

        #region Mock

        private static HttpClient MockHttpClient()
        {
            return new HttpClientMock(RootUri)
                .RegisterJsonResponse(UniqueIdentifierNameRequestUri, UniqueIdentifierNameJson)
                .RegisterJsonResponse(EntityWithQueryableMemberRequestUri, EntityWithQueryableMemberJson)
                .ToHttpClient();
        }


        private static QueryBinderFactory MockQueryBinderFactory()
        {
            var queryBinderFactory = Substitute.For<QueryBinderFactory>();
            queryBinderFactory().Returns(new QueryBinder(new EntityValidator()));

            return queryBinderFactory;
        }

        #endregion

        #region Data

        private static Uri RootUri => new Uri("http://www.example.com/api/v1/", UriKind.Absolute);

        private static string EntityWithQueryableMemberRequestUri =>
            $"{RootUri}entitywithqueryablemembers";

        private static string EntityWithQueryableMemberJson => @"
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

        private static QueryResult<object> EntityWithQueryableMemberResult => new QueryResult<object>(
            new Uri(EntityWithQueryableMemberRequestUri),
            new object[]
            {
                new EntityWithQueryableMember("Test1", CreateQuery<EntityWithSimpleMembers>("entitywithqueryablemembers/Test1/")),
                new EntityWithQueryableMember("Test2", CreateQuery<EntityWithSimpleMembers>("entitywithqueryablemembers/Test2/"))
            });

        private static QueryProvider CreateQueryProvider(Uri root)
        {
            return new QueryProvider(CreateResourceRetriever(), null, () => new QueryBinder(new EntityValidator()), root);
        }

        private static IQueryable<T> CreateQuery<T>()
        {
            return new Query<T>(CreateQueryProvider(RootUri));
        }

        private static IQueryable<T> CreateQuery<T>(string subPath)
        {
            return new Query<T>(CreateQueryProvider(new Uri(RootUri, subPath)));
        }

        private static string UniqueIdentifierNameRequestUri =>
            $"{RootUri}entitywithqueryablemembers?fields=uniqueIdentifier,name";

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

        private static QueryResult<object> NameResult => new QueryResult<object>(
            new Uri(UniqueIdentifierNameRequestUri),
            new object[]
            {
                new
                {
                    Name = "Test1"
                },
                new
                {
                    Name = "Test2"
                }
            });

        private static QueryResult<object> UniqueIdentifierNameResult => new QueryResult<object>(
            new Uri(UniqueIdentifierNameRequestUri),
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
}
