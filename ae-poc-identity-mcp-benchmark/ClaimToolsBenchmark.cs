using Ae.Poc.Identity.Mcp.Data;
using Ae.Poc.Identity.Mcp.Dtos;
using Ae.Poc.Identity.Mcp.Services;
using Ae.Poc.Identity.Mcp.Tools;
using Ae.Poc.Identity.Mcp.Profiles;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

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
    private ClaimTools _claimTools = null!;

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
        var config = new MapperConfiguration(cfg => { cfg.AddProfile<DataProfile>(); });
        _mapper = config.CreateMapper();
        _validator = new FakeValidator();
        _claimTools = new ClaimTools(_claimClient, _mapper, _validator, _loggerFactory.CreateLogger<ClaimTools>());
    }

    [Benchmark]
    public async Task GetClaimsAsync()
    {
        ClaimsQueryIncomingDto dto = new()
        {
            Skipped = 0,
            NumberOf = 10
        };
        var result = await _claimTools.GetClaimsAsync(dto, CancellationToken.None);
        if (result == null) throw new InvalidOperationException("Result is null");
    }

    [Benchmark]
    public async Task GetClaimDetailsAsync()
    {
        var result = await _claimTools.GetClaimDetailsAsync(_testClaims[0].Id.ToString(), CancellationToken.None);
        if (result == null) throw new InvalidOperationException("Result is null");
    }

    [Benchmark]
    public async Task CreateClaimAsync()
    {
        var dto = new ClaimCreateDto
        {
            Type = "new-type",
            Value = "new-value",
            ValueType = "string",
            DisplayText = "New Claim"
        };
        var result = await _claimTools.CreateClaimAsync(dto, CancellationToken.None);
        if (result == null) throw new InvalidOperationException("Result is null");
    }

    [Benchmark]
    public async Task UpdateClaimAsync()
    {
        var claim = _testClaims[0];
        var dto = new ClaimUpdateDto
        {
            Id = claim.Id,
            Type = "updated-type",
            Value = "updated-value",
            ValueType = "string",
            DisplayText = "Updated Claim"
        };
        var result = await _claimTools.UpdateClaimAsync(claim.Id.ToString(), dto, CancellationToken.None);
        if (result == null) throw new InvalidOperationException("Result is null");
    }

    [Benchmark]
    public async Task DeleteClaimAsync()
    {
        var result = await _claimTools.DeleteClaimAsync(_testClaims[0].Id.ToString(), CancellationToken.None);
        if (result == null) throw new InvalidOperationException("Result is null");
    }

    private sealed class FakeClaimClient : IClaimClient, IClaimClientHealth
    {
        private readonly List<AppClaim> _claims;

        public FakeClaimClient(List<AppClaim> claims) => _claims = claims;

        public Task<ClaimsInfo?> GetClaimsInfoAsync(CancellationToken ct = default) => Task.FromResult<ClaimsInfo?>(new ClaimsInfo { TotalCount = _claims.Count });
        public Task<IEnumerable<AppClaim>> LoadClaimsAsync(ClaimsQuery claimsQuery, CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<AppClaim>>(_claims);
        public Task<AppClaim?> LoadClaimDetailsAsync(string claimId, CancellationToken ct = default) => Task.FromResult(_claims.FirstOrDefault(c => c.Id.ToString() == claimId));
        public Task<AppClaim> DeleteClaimAsync(string claimId, CancellationToken ct = default)
        {
            var claim = _claims.FirstOrDefault(c => c.Id.ToString() == claimId);
            return Task.FromResult(claim ?? new AppClaim { Id = Guid.Parse(claimId), Type = "deleted" });
        }
        public Task<AppClaim> CreateClaimAsync(AppClaim appClaim, CancellationToken ct = default)
        {
            appClaim.Id = Guid.NewGuid();
            return Task.FromResult(appClaim);
        }
        public Task<AppClaim> UpdateClaimAsync(string claimId, AppClaim appClaim, CancellationToken ct = default) => Task.FromResult(appClaim);

        public Task<DependencyHealthDto> GetHealthAsync(CancellationToken ct = default) => Task.FromResult(new DependencyHealthDto { IsReady = true, Version = "0.0.0", ClientId = "fake-client" });
    }

    private sealed class FakeValidator : IDtoValidator
    {
        public bool TryValidate(object dto, out ICollection<ValidationResult> results)
        {
            results = new List<ValidationResult>();
            return true;
        }
    }
}
