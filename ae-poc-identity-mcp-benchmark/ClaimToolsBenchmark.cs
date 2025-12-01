using Ae.Poc.Identity.Mcp.Data;
using Ae.Poc.Identity.Mcp.Dtos;
using Ae.Poc.Identity.Mcp.Services;
using Ae.Poc.Identity.Mcp.Tools;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.Logging;

namespace Ae.Poc.Identity.Mcp.Benchmarks;

[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
public class ClaimToolsBenchmark
{
    private ILoggerFactory _loggerFactory = null!;
    private IClaimClient _claimClient = null!;
    private IMapper _mapper = null!;
    private IDtoValidator _validator = null!;
    private List<AppClaim> _testClaims = null!;

    [GlobalSetup]
    public void Setup()
    {
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _testClaims = new List<AppClaim>
        {
            new AppClaim
            {
                Id = Guid.NewGuid(),
                Type = "test-type",
                Value = "test-value",
                ValueType = "string",
                DisplayText = "Test Claim",
                Description = "Test description"
            },
            new AppClaim
            {
                Id = Guid.NewGuid(),
                Type = "test-type-2",
                Value = "test-value-2",
                ValueType = "string",
                DisplayText = "Test Claim 2",
                Description = "Test description 2"
            }
        };

        _claimClient = new FakeClaimClient(_testClaims);
        var config = new MapperConfiguration(cfg => { cfg.CreateMap<AppClaim, ClaimOutgoingDto>(); }, _loggerFactory);
        _mapper = config.CreateMapper();
        //ToDo _validator = 
    }

    [Benchmark]
    public async Task GetClaimsAsync()
    {
        ClaimsQueryIncomingDto dto = new()
        {
            Skipped = 0,
            NumberOf = 10
        };
        var result = await ClaimTools.GetClaimsAsync(dto, _claimClient, _mapper, _validator, _loggerFactory, CancellationToken.None);
        if (result == null) throw new InvalidOperationException("Result is null");
    }

    private sealed class FakeClaimClient : IClaimClient
    {
        private readonly List<AppClaim> _claims;

        public FakeClaimClient(List<AppClaim> claims) => _claims = claims;

        public Task<ClaimsInfo?> GetClaimsInfoAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IEnumerable<AppClaim>> LoadClaimsAsync(ClaimsQuery claimsQuery, CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<AppClaim>>(_claims);
        public Task<AppClaim> LoadClaimDetailsAsync(string claimId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<AppClaim> DeleteClaimAsync(string claimId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<AppClaim> CreateClaimAsync(AppClaim appClaim, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<AppClaim> UpdateClaimAsync(string claimId, AppClaim appClaim, CancellationToken ct = default) => throw new NotImplementedException();
    }
}
