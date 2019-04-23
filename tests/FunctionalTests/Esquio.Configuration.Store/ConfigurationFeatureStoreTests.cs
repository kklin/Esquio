﻿using Esquio.Abstractions;
using Esquio.Toggles;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.Esquio.Configuration.Store
{
    public class ConfigurationFeatureStoreShould : IClassFixture<Fixture>
    {
        private readonly Fixture _fixture;

        public ConfigurationFeatureStoreShould(Fixture fixture)
        {
            _fixture = fixture ?? throw new System.ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task return_false_when_try_to_add_new_feature()
        {
            (await _fixture.FeatureStore.AddFeatureAsync("featureName", "applicationName", enabled: true))
                .Should().BeFalse();
        }
        [Fact]
        public async Task return_null_when_find_a_non_existing_feature()
        {
            (await _fixture.FeatureStore.FindFeatureAsync("non-valid-feature-name", "non-valid-application-name"))
                .Should().BeNull();

            (await _fixture.FeatureStore.FindFeatureAsync("non-valid-feature-name", "MySampleApplication"))
                .Should().BeNull();
        }
        [Fact]
        public async Task return_feature_if_is_configured()
        {
            (await _fixture.FeatureStore.FindFeatureAsync("non-valid-feature-name", "non-valid-application-name"))
                .Should().BeNull();

            var feature = await _fixture.FeatureStore.FindFeatureAsync("FeatureA", "MySampleApplication");

            feature.Should().NotBeNull();

            feature.Name.Should().BeEquivalentTo("FeatureA");
            feature.Enabled.Should().BeTrue();
        }
        [Fact]
        public async Task return_false_when_try_to_add_new_toggle()
        {
            (await _fixture.FeatureStore.AddToggleAsync<FakeToggle>("featureName", "applicationName", new Dictionary<string, object>()))
               .Should().BeFalse();
        }
        [Fact]
        public async Task return_empty_collection_when_find_toggle_types_on_non_existing_feature()
        {
            (await _fixture.FeatureStore.FindTogglesTypesAsync("featureName", "applicationName"))
               .Any().Should().BeFalse();
        }
        [Fact]
        public async Task return_configured_toggle_types()
        {
            var configuredTypes = await _fixture.FeatureStore.FindTogglesTypesAsync("FeatureA", "MySampleApplication");

            configuredTypes.Count().Should().Be(2);

            configuredTypes.Contains("Esquio.Toggles.OnToggle").Should().BeTrue();
            configuredTypes.Contains("Esquio.Toggles.UserNameToggle").Should().BeTrue();
        }
        [Fact]
        public async Task return_null_when_find_parameter_value_on_non_existing_feature()
        {
            (await _fixture.FeatureStore.GetToggleParameterValueAsync<UserNameToggle>("FeatureB", "MySampleApplication", "Users"))
                .Should().BeNull();
        }
        [Fact]
        public async Task return_null_when_find_parameter_value_on_non_existing_toggle()
        {
            (await _fixture.FeatureStore.GetToggleParameterValueAsync<OffToggle>("FeatureA", "MySampleApplication", "Users"))
                .Should().BeNull();
        }
        [Fact]
        public async Task return_parameter_value_on_existing_feature()
        {
            var parameterValue = await _fixture.FeatureStore.GetToggleParameterValueAsync<UserNameToggle>("FeatureA", "MySampleApplication", "Users");

            parameterValue.Should().NotBeNull();
            parameterValue.Should().BeEquivalentTo("user1;user2;user3");
        }
    }

    class FakeToggle : IToggle
    {
        public Task<bool> IsActiveAsync(string applicationName, string featureName)
        {
            return Task.FromResult(false);
        }
    }
}
