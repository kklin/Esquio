﻿using FluentAssertions;
using System.Threading.Tasks;
using UnitTests.Seedwork;
using Xunit;

namespace UnitTests.Esquio.AspNetCore.Mvc
{
    [Collection(nameof(AspNetCoreServer))]
    public class FeatureSwitchConstrainShould
    {
        ServerFixture _fixture;

        public FeatureSwitchConstrainShould(ServerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task enable_action_when_flag_is_enabled()
        {
            var response = await _fixture.TestServer
                .CreateClient()
                .GetAsync("http://localhost/test/ActionWithFlagSwitch");

            response.IsSuccessStatusCode
                .Should().BeTrue();

            (await response.Content.ReadAsStringAsync())
                .Should().BeEquivalentTo("Enabled");
        }

        [Fact]
        public async Task use_default_action_when_flag_is_disabled()
        {
            var response = await _fixture.TestServer
                .CreateClient()
                .GetAsync("http://localhost/test/ActionWithFlagSwitchDisabled");

            response.IsSuccessStatusCode
                .Should().BeTrue();

            (await response.Content.ReadAsStringAsync())
                .Should().BeEquivalentTo("Disabled");
        }
    }
}
