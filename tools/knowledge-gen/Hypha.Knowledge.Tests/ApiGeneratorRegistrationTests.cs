// ------------------------------------------------------------------------------------------------
// <copyright file="ApiGeneratorRegistrationTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.IO;
    using System.Linq;

    using Hypha.Knowledge;
    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Api.CrossReferences;
    using Hypha.Knowledge.Api.OpenApi;
    using Hypha.Knowledge.Generation;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Tests for how the composition root registers the API generators of this library: as their own
    /// collection, in their own order, and never among the language-release generators.
    /// </summary>
    [TestFixture]
    public class ApiGeneratorRegistrationTests
    {
        private DirectoryInfo root = null!;
        private ServiceProvider provider = null!;

        [SetUp]
        public void SetUp()
        {
            this.root = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-api-registration-{Guid.NewGuid():N}"));

            this.provider = new ServiceCollection()
                .AddHyphaKnowledge(options => options.RepositoryRoot = this.root)
                .BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            this.provider.Dispose();

            this.root.Refresh();
            if (this.root.Exists)
            {
                this.root.Delete(recursive: true);
            }
        }

        [Test]
        public void Registers_the_three_api_generators_of_this_library_in_their_order()
        {
            var generators = this.provider.GetServices<IApiKnowledgeGenerator>()
                .OrderBy(generator => generator.Order)
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(
                    generators.Select(generator => generator.Artifact),
                    Is.EqualTo(new[] { "openapi", "spec", "cross-references" }));
                Assert.That(generators.Select(generator => generator.Order), Is.EqualTo(new[] { 20, 30, 40 }));
                Assert.That(generators[0], Is.InstanceOf<OpenApiGenerator>());
                Assert.That(generators[1], Is.InstanceOf<ApiSpecGenerator>());
                Assert.That(generators[2], Is.InstanceOf<ApiCrossReferenceGenerator>());
            });
        }

        [Test]
        public void No_api_generator_is_also_registered_as_a_language_release_generator()
        {
            // An API generator is given an API version, not a release tag: a run over the language
            // releases must never pick one up.
            var generators = this.provider.GetServices<IKnowledgeGenerator>().ToList();

            Assert.Multiple(() =>
            {
                Assert.That(generators, Is.Not.Empty);
                Assert.That(generators, Has.None.InstanceOf<IApiKnowledgeGenerator>());
            });
        }

        [Test]
        public void Resolves_the_services_the_api_generators_are_built_from()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.provider.GetService<IOpenApiReader>(), Is.InstanceOf<OpenApiReader>());
                Assert.That(this.provider.GetService<IOpenApiRenderer>(), Is.InstanceOf<OpenApiRenderer>());
                Assert.That(
                    this.provider.GetService<IApiCrossReferenceBuilder>(), Is.InstanceOf<ApiCrossReferenceBuilder>());
            });
        }
    }
}
