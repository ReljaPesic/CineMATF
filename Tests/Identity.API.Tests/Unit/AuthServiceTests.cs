using Identity.API.Data;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Tests.Unit;

public class AuthServiceTests
{
    private readonly Mock<UserManager<User>> _userManager;
    private readonly Mock<ITokenService> _tokenService;
    private readonly IdentityContext _dbContext;
    private readonly AuthService _service;
    
    // this executes before each test 
    public AuthServiceTests()
    {
        _userManager = MockUserManager();
        _tokenService = new Mock<ITokenService>();
        
        //  InMemory database per test doesn't leak into other tests
        _dbContext = new IdentityContext(
            new DbContextOptionsBuilder<IdentityContext>()
                .UseInMemoryDatabase($"AuthServiceTests_{Guid.NewGuid():N}")
                .Options);

        _service = new AuthService(_userManager.Object, _tokenService.Object, _dbContext);
    }
    
    private static Mock<UserManager<User>> MockUserManager()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    // It is necessary to have all field notnull so EF doesn't trow an error (non-nullable)
    private static User NewUser(string userName = "alice") => new()
    {
        Id = Guid.NewGuid().ToString(),
        UserName = userName,
        Email = $"{userName}@cinematf.local",
        FirstName = "Test",
        LastName = "User",
        CardNumber = "4111111111111111",
    };

    // just creates a LoginRequest object
    private static LoginRequest Login(string userName = "alice", string password = "Passw0rd!") =>
        new(userName, password);

    //  ValidateUser 
    [Fact]
    public async Task ValidateUser_ReturnsNull_WhenUsernameIsUnknown()
    {
        // userManager returns null when name "ghost" is looked-up
        _userManager.Setup(m => m.FindByNameAsync("ghost")).ReturnsAsync((User?)null);

        var result = await _service.ValidateUser(Login("ghost"));
        result.Should().BeNull();
        _userManager.Verify(m => m.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ValidateUser_ReturnsNull_WhenPasswordDoesNotMatch()
    {
        var user = NewUser();
        _userManager.Setup(m => m.FindByNameAsync(user.UserName!)).ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, "wrongPassword")).ReturnsAsync(false);

        var result = await _service.ValidateUser(Login(user.UserName!, "wrongPassword"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateUser_ReturnsTheUser_WhenCredentialsAreCorrect()
    {
        var user = NewUser();
        _userManager.Setup(m => m.FindByNameAsync(user.UserName!)).ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, "Passw0rd!")).ReturnsAsync(true);

        var result = await _service.ValidateUser(Login(user.UserName!));
        
        result.Should().BeSameAs(user);
    }

    //  CreateAuthResponse

    [Fact]
    public async Task CreateAuthResponse_ReturnsBothTokensAndPersistsTheRefreshToken()
    {
        var user = NewUser();
        
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var refreshToken = new RefreshToken { Token = "refresh-token", ExpiryTime = DateTime.UtcNow.AddDays(7) };

        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["User"]);
        _tokenService.Setup(t => t.CreateAccessToken(user, It.Is<IEnumerable<string>>(r => r.Contains("User"))))
            .Returns("access-token");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns(refreshToken);

        var response = await _service.CreateAuthResponse(user);
        
        response.AccessToken.Should().Be("access-token");
        response.RefreshToken.Should().Be("refresh-token");
        
        user.RefreshTokens.Should().ContainSingle(t => t.Token == "refresh-token");
        // the refresh token should be in the database 
        (await _dbContext.Set<RefreshToken>().AsNoTracking().ToListAsync())
            .Should().ContainSingle(t => t.Token == "refresh-token");
    }

    // --- RemoveRefreshToken ----------------------------------------------

    [Fact]
    public async Task RemoveRefreshToken_DeletesAMatchingToken()
    {
        var user = NewUser();
        var token = new RefreshToken { Token = "to-remove", ExpiryTime = DateTime.UtcNow.AddDays(7) };
        user.RefreshTokens.Add(token);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        await _service.RemoveRefreshToken(user, "to-remove");

        // token is gone from the in-memory collection
        user.RefreshTokens.Should().BeEmpty();
        // and gone from the database
        (await _dbContext.Set<RefreshToken>().AsNoTracking().AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task RemoveRefreshToken_WhenTheTokenIsNotOnTheUser()
    {
        var user = NewUser();
        user.RefreshTokens.Add(new RefreshToken { Token = "some-other-token" });
        
        // no token shouldn't trow an error and shouldn't remove existing tokens
        var act = () => _service.RemoveRefreshToken(user, "does-not-exist");

        await act.Should().NotThrowAsync();
        user.RefreshTokens.Should().ContainSingle(t => t.Token == "some-other-token");
    }
}
