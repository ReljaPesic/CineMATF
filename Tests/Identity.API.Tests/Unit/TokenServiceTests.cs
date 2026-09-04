using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Identity.API.Tests.Unit;

public class TokenServiceTests
{
    // data from appsettings.json for JWT
    private static readonly JwtSettings Jwt = new()
    {
        Issuer = "CineMATF.Identity",
        Audience = "CineMATF.Services",
        SecretKey = "MyVerySecretMessageThatOnlyIKnow",
        ExpiryMinutes = 60,
    };

    private readonly TokenService _service = new(Options.Create(Jwt));
    private static User NewUser() => new()
    {
        Id = Guid.NewGuid().ToString(),
        UserName = "alice",
        Email = "alice@cinematf.local",
        CardNumber = "4111111111111111",
    };
    
    // helper function that returns ClaimsPricipal if the token is valid or trows an error
    private static ClaimsPrincipal ValidateAndRead(string token)
    {
        var parameters = new TokenValidationParameters
        {
            // we want to check if the following are valid: issuer, audience, lifetime and issuer signing key 
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Jwt.Issuer,
            ValidAudience = Jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Jwt.SecretKey)),
            ClockSkew = TimeSpan.Zero,
        };
        // we check validity of the token against the parameters 
        return new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
    }
    
    [Fact]
    public void CreateAccessToken_ProducesATokenAcceptedByTheStandardValidationParameters()
    {
        var token = _service.CreateAccessToken(NewUser(), ["User"]);

        var act = () => ValidateAndRead(token);

        act.Should().NotThrow();
    }

    // checks if all the claims are paced in the token
    [Fact]
    public void CreateAccessToken_PutsTheUserIdentityIntoClaims()
    {
        var user = NewUser();

        var principal = ValidateAndRead(_service.CreateAccessToken(user, ["User"]));

        // TokenService writes `sub` but mapping turns it into NameIdentifier
        principal.FindFirstValue(ClaimTypes.NameIdentifier).Should().Be(user.Id);
        principal.FindFirstValue(ClaimTypes.Name).Should().Be(user.UserName);
        principal.FindFirstValue(ClaimTypes.Email).Should().Be(user.Email);
        principal.FindFirstValue("cardNumber").Should().Be(user.CardNumber);
    }

    // One role claim per role string, and IsInRole (used by [Authorize(Roles=...)]) works.
    [Fact]
    public void CreateAccessToken_EmitsOneRoleClaimPerRole()
    {
        var principal = ValidateAndRead(_service.CreateAccessToken(NewUser(), ["User", "Admin"]));

        principal.FindAll(ClaimTypes.Role).Select(c => c.Value)
            .Should().BeEquivalentTo("User", "Admin");
        principal.IsInRole("Admin").Should().BeTrue();
        principal.IsInRole("User").Should().BeTrue();
        principal.IsInRole("SuperUser").Should().BeFalse();
    }

  
    [Fact]
    public void CreateAccessToken_WithNoRoles_EmitsNoRoleClaims()
    {
        var principal = ValidateAndRead(_service.CreateAccessToken(NewUser(), []));
        principal.FindAll(ClaimTypes.Role).Should().BeEmpty();
    }

    // TokenService should convert null values into String.Empty
    [Fact]
    public void CreateAccessToken_WithNullOptionalFields_WritesEmptyStringsRatherThanThrowing()
    {
        var user = new User { Id = Guid.NewGuid().ToString(), UserName = null, Email = null, CardNumber = null! };

        var principal = ValidateAndRead(_service.CreateAccessToken(user, ["User"]));

        principal.FindFirstValue("cardNumber").Should().BeEmpty();
    }

    [Fact]
    public void CreateAccessToken_SetsIssuerAudienceAndExpiry()
    {
        var before = DateTime.UtcNow;

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(_service.CreateAccessToken(NewUser(), ["User"]));

        jwt.Issuer.Should().Be(Jwt.Issuer);
        jwt.Audiences.Should().Contain(Jwt.Audience);
        jwt.ValidTo.Should().BeCloseTo(before.AddMinutes(Jwt.ExpiryMinutes), TimeSpan.FromMinutes(1));
    }


    [Fact]
    public void CreateAccessToken_IsRejectedWhenValidatedWithADifferentKey()
    {
        var token = _service.CreateAccessToken(NewUser(), ["User"]);

        var parameters = new TokenValidationParameters
        {
            // only check is the signing key is valid
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("NotMyVerySecretKeyThatOnlyIKnow")),
        };

        var act = () => new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);

        act.Should().Throw<SecurityTokenSignatureKeyNotFoundException>();
    }

    // The refresh token is an opaque random string (not a JWT) with an id and a
    // 7-day lifetime - the long-lived half of the pair, stored server-side.
    [Fact]
    public void GenerateRefreshToken_ReturnsARandomTokenValidForSevenDays()
    {
        var token = _service.GenerateRefreshToken();

        token.Id.Should().NotBe(Guid.Empty);
        token.Token.Should().NotBeNullOrWhiteSpace();
        token.ExpiryTime.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));
    }

    // token generation is cryptographically random, so two calls never collide
    [Fact]
    public void GenerateRefreshToken_ReturnsADifferentValueEachCall()
    {
        var first = _service.GenerateRefreshToken();
        var second = _service.GenerateRefreshToken();

        second.Token.Should().NotBe(first.Token);
    }
}
