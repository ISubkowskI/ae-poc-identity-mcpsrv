using Ae.Poc.Identity.Mcp.Data;
using Ae.Poc.Identity.Mcp.Dtos;
using Ae.Poc.Identity.Mcp.Profiles;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Ae.Poc.Identity.Mcp.Unittests.Profiles;

public class MappingTests
{
    [Fact]
    public void Mapping_ClaimsQueryIncomingDto_To_ClaimsQuery_ShouldWork()
    {
        // Arrange


        var config = new MapperConfiguration(cfg => cfg.AddProfile<DataProfile>());
        config.AssertConfigurationIsValid(); // Check for configuration errors
        var mapper = config.CreateMapper();
        var dto = new ClaimsQueryIncomingDto { Skipped = 10, NumberOf = 20 };

        // Act
        var result = mapper.Map<ClaimsQuery>(dto);

        // Assert
        result.Should().NotBeNull();
        result.Skipped.Should().Be(10);
        result.NumberOf.Should().Be(20);
    }
}
