namespace Reservation.API.Tests.Unit;

public class ReservationServiceTests
{
    private readonly Mock<IReservationRepository> _repositoryMock;
    private readonly Mock<IReservationFactory> _factoryMock;
    private readonly Mock<ICinemaApiClient> _cinemaApiClientMock;
    private readonly Mock<IScreeningApiClient> _screeningApiClientMock;
    private readonly IMapper _mapper;
    private readonly ReservationService _service;

    public ReservationServiceTests()
    {
        _repositoryMock = new Mock<IReservationRepository>();
        _factoryMock = new Mock<IReservationFactory>();
        _cinemaApiClientMock = new Mock<ICinemaApiClient>();
        _screeningApiClientMock = new Mock<IScreeningApiClient>();
        _screeningApiClientMock.Setup(c => c.GetScreeningAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScreeningDetails(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1), "TwoD"));

        var config = new MapperConfiguration(cfg => cfg.AddProfile<Mapping.ReservationMappingProfile>());
        _mapper = config.CreateMapper();

        var options = Options.Create(new ReservationOptions { LockDurationMinutes = 10 });
        _service = new ReservationService(_repositoryMock.Object, _mapper, _factoryMock.Object, options, _cinemaApiClientMock.Object, _screeningApiClientMock.Object);
    }

    private void SetUpSeat(Guid seatId, string seatType = "Standard") =>
        _cinemaApiClientMock.Setup(c => c.GetSeatAsync(seatId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SeatDetails(seatId, Row: 1, Number: 1, SeatType: seatType));

    [Fact]
    public async Task CreateReservationAsync_WithEmptyUserId_ReturnsFailureWithoutHittingRepository()
    {
        var request = new CreateReservationRequest(Guid.NewGuid(), [Guid.NewGuid()], Guid.Empty);

        var (success, error, response) = await _service.CreateReservationAsync(request);

        success.Should().BeFalse();
        error.Should().Be("UserId must be provided");
        response.Should().BeNull();
        _repositoryMock.Verify(r => r.GetActiveLocksBySeatsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_WithDuplicateSeatIds_ReturnsFailureWithoutHittingRepository()
    {
        var seatId = Guid.NewGuid();
        var request = new CreateReservationRequest(Guid.NewGuid(), [seatId, seatId], Guid.NewGuid());

        var (success, error, response) = await _service.CreateReservationAsync(request);

        success.Should().BeFalse();
        error.Should().Contain(seatId.ToString());
        response.Should().BeNull();
        _repositoryMock.Verify(r => r.GetActiveLocksBySeatsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_WithNonExistentScreening_ReturnsFailure()
    {
        var screeningId = Guid.NewGuid();
        var request = new CreateReservationRequest(screeningId, [Guid.NewGuid()], Guid.NewGuid());
        _screeningApiClientMock.Setup(c => c.GetScreeningAsync(screeningId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ScreeningDetails?)null);

        var (success, error, response) = await _service.CreateReservationAsync(request);

        success.Should().BeFalse();
        error.Should().Be("Screening not found");
        response.Should().BeNull();
        _repositoryMock.Verify(r => r.GetActiveLocksBySeatsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_WithPastScreening_ReturnsFailure()
    {
        var screeningId = Guid.NewGuid();
        var request = new CreateReservationRequest(screeningId, [Guid.NewGuid()], Guid.NewGuid());
        _screeningApiClientMock.Setup(c => c.GetScreeningAsync(screeningId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScreeningDetails(screeningId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-1), "TwoD"));

        var (success, error, response) = await _service.CreateReservationAsync(request);

        success.Should().BeFalse();
        error.Should().Be("Screening has already started");
        response.Should().BeNull();
    }

    [Fact]
    public async Task CreateReservationAsync_WithAlreadyLockedSeat_ReturnsFailure()
    {
        var screeningId = Guid.NewGuid();
        var seatId = Guid.NewGuid();
        var request = new CreateReservationRequest(screeningId, [seatId], Guid.NewGuid());

        _repositoryMock.Setup(r => r.GetActiveLocksBySeatsAsync(screeningId, request.SeatIds))
            .ReturnsAsync([new Entities.SeatLock { Id = Guid.NewGuid(), ScreeningId = screeningId, SeatId = seatId }]);

        var (success, error, response) = await _service.CreateReservationAsync(request);

        success.Should().BeFalse();
        error.Should().Contain(seatId.ToString());
        response.Should().BeNull();
    }

    [Fact]
    public async Task CreateReservationAsync_WithNonExistentSeat_ReturnsFailureWithoutStartingTransaction()
    {
        var screeningId = Guid.NewGuid();
        var seatId = Guid.NewGuid();
        var request = new CreateReservationRequest(screeningId, [seatId], Guid.NewGuid());

        _repositoryMock.Setup(r => r.GetActiveLocksBySeatsAsync(screeningId, request.SeatIds)).ReturnsAsync([]);
        _cinemaApiClientMock.Setup(c => c.GetSeatAsync(seatId, It.IsAny<CancellationToken>())).ReturnsAsync((SeatDetails?)null);

        var (success, error, response) = await _service.CreateReservationAsync(request);

        success.Should().BeFalse();
        error.Should().Contain(seatId.ToString());
        response.Should().BeNull();
        _repositoryMock.Verify(r => r.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenCinemaApiUnreachable_ReturnsFailure()
    {
        var screeningId = Guid.NewGuid();
        var seatId = Guid.NewGuid();
        var request = new CreateReservationRequest(screeningId, [seatId], Guid.NewGuid());

        _repositoryMock.Setup(r => r.GetActiveLocksBySeatsAsync(screeningId, request.SeatIds)).ReturnsAsync([]);
        _cinemaApiClientMock.Setup(c => c.GetSeatAsync(seatId, It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("unreachable"));

        var (success, error, response) = await _service.CreateReservationAsync(request);

        success.Should().BeFalse();
        error.Should().Be("Unable to verify seats right now, please try again later");
        response.Should().BeNull();
        _repositoryMock.Verify(r => r.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_WithValidRequest_LocksSeatsCreatesTicketsAndCommits()
    {
        var screeningId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var seatId = Guid.NewGuid();
        var request = new CreateReservationRequest(screeningId, [seatId], userId);

        _repositoryMock.Setup(r => r.GetActiveLocksBySeatsAsync(screeningId, request.SeatIds)).ReturnsAsync([]);
        SetUpSeat(seatId);

        var reservationId = Guid.NewGuid();
        var reservation = new Entities.Reservation
        {
            Id = reservationId,
            UserId = userId,
            ScreeningId = screeningId,
            Status = ReservationStatus.Locked,
            TotalPrice = 10m
        };
        var tickets = new List<Entities.Ticket>
        {
            new() { Id = Guid.NewGuid(), ReservationId = reservationId, SeatId = seatId, Price = 10m, QrCode = "qr" }
        };
        _factoryMock.Setup(f => f.CreateReservation(It.IsAny<Guid>(), userId, screeningId, ReservationStatus.Locked, It.Is<IEnumerable<SeatDetails>>(seats => seats.Single().SeatId == seatId)))
            .Returns((reservation, tickets));

        var transactionMock = new Mock<IDbContextTransaction>();
        _repositoryMock.Setup(r => r.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);
        _repositoryMock.Setup(r => r.CreateReservationAsync(reservation)).ReturnsAsync(reservation);

        var (success, error, response) = await _service.CreateReservationAsync(request);

        success.Should().BeTrue();
        error.Should().BeNull();
        response.Should().NotBeNull();
        response!.Id.Should().Be(reservationId);

        _repositoryMock.Verify(r => r.LockSeatAsync(It.Is<Entities.SeatLock>(sl => sl.SeatId == seatId && sl.ReservationId == reservationId)), Times.Once);
        _repositoryMock.Verify(r => r.CreateTicketsAsync(tickets), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenSaveThrowsUniqueConstraintViolation_RollsBackAndReturnsFailure()
    {
        var screeningId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var seatId = Guid.NewGuid();
        var request = new CreateReservationRequest(screeningId, [seatId], userId);

        _repositoryMock.Setup(r => r.GetActiveLocksBySeatsAsync(screeningId, request.SeatIds)).ReturnsAsync([]);
        SetUpSeat(seatId);

        var reservationId = Guid.NewGuid();
        var reservation = new Entities.Reservation
        {
            Id = reservationId,
            UserId = userId,
            ScreeningId = screeningId,
            Status = ReservationStatus.Locked,
            TotalPrice = 10m
        };
        _factoryMock.Setup(f => f.CreateReservation(It.IsAny<Guid>(), userId, screeningId, ReservationStatus.Locked, It.IsAny<IEnumerable<SeatDetails>>()))
            .Returns((reservation, new List<Entities.Ticket>()));

        var transactionMock = new Mock<IDbContextTransaction>();
        _repositoryMock.Setup(r => r.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);
        _repositoryMock.Setup(r => r.CreateReservationAsync(reservation)).ReturnsAsync(reservation);
        _repositoryMock.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new DbUpdateException("failed", new Exception("duplicate key value violates unique constraint")));

        var (success, error, response) = await _service.CreateReservationAsync(request);

        success.Should().BeFalse();
        error.Should().Be("Some seats are no longer available");
        response.Should().BeNull();
        transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenSaveThrowsUnrelatedException_RollsBackAndRethrows()
    {
        var screeningId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var seatId = Guid.NewGuid();
        var request = new CreateReservationRequest(screeningId, [seatId], userId);

        _repositoryMock.Setup(r => r.GetActiveLocksBySeatsAsync(screeningId, request.SeatIds)).ReturnsAsync([]);
        SetUpSeat(seatId);

        var reservation = new Entities.Reservation { Id = Guid.NewGuid(), UserId = userId, ScreeningId = screeningId, Status = ReservationStatus.Locked };
        _factoryMock.Setup(f => f.CreateReservation(It.IsAny<Guid>(), userId, screeningId, ReservationStatus.Locked, It.IsAny<IEnumerable<SeatDetails>>()))
            .Returns((reservation, new List<Entities.Ticket>()));

        var transactionMock = new Mock<IDbContextTransaction>();
        _repositoryMock.Setup(r => r.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);
        _repositoryMock.Setup(r => r.CreateReservationAsync(reservation)).ReturnsAsync(reservation);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ThrowsAsync(new InvalidOperationException("boom"));

        var action = () => _service.CreateReservationAsync(request);

        await action.Should().ThrowAsync<InvalidOperationException>();
        transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAvailableSeatsAsync_ExcludesLockedSeatsFromAvailable()
    {
        var screeningId = Guid.NewGuid();
        var cinemaId = Guid.NewGuid();
        var hallId = Guid.NewGuid();
        var lockedSeatId = Guid.NewGuid();
        var freeSeatId = Guid.NewGuid();

        _screeningApiClientMock.Setup(c => c.GetScreeningAsync(screeningId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScreeningDetails(screeningId, Guid.NewGuid(), hallId, cinemaId, DateTime.UtcNow.AddDays(1), "TwoD"));
        _cinemaApiClientMock.Setup(c => c.GetSeatsByHallAsync(cinemaId, hallId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new SeatDetails(lockedSeatId, 1, 1, "Standard"), new SeatDetails(freeSeatId, 1, 2, "Standard")]);

        var seatLock = new Entities.SeatLock
        {
            Id = Guid.NewGuid(),
            ScreeningId = screeningId,
            SeatId = lockedSeatId,
            LockedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        _repositoryMock.Setup(r => r.GetActiveLocksByScreeningAsync(screeningId)).ReturnsAsync([seatLock]);

        var result = await _service.GetAvailableSeatsAsync(screeningId);

        result.Should().NotBeNull();
        result!.ScreeningId.Should().Be(screeningId);
        result.LockedSeats.Should().ContainSingle(s => s.SeatId == lockedSeatId);
        result.AvailableSeats.Should().ContainSingle(id => id == freeSeatId);
    }

    [Fact]
    public async Task GetAvailableSeatsAsync_WithNonExistentScreening_ReturnsNull()
    {
        var screeningId = Guid.NewGuid();
        _screeningApiClientMock.Setup(c => c.GetScreeningAsync(screeningId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ScreeningDetails?)null);

        var result = await _service.GetAvailableSeatsAsync(screeningId);

        result.Should().BeNull();
        _repositoryMock.Verify(r => r.GetActiveLocksByScreeningAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetReservationByIdAsync_WithExistingId_ReturnsResponse()
    {
        var id = Guid.NewGuid();
        var reservation = new Entities.Reservation { Id = id, UserId = Guid.NewGuid(), ScreeningId = Guid.NewGuid(), Status = ReservationStatus.Locked, TotalPrice = 10m };
        _repositoryMock.Setup(r => r.GetReservationByIdAsync(id)).ReturnsAsync(reservation);

        var result = await _service.GetReservationByIdAsync(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Status.Should().Be(nameof(ReservationStatus.Locked));
    }

    [Fact]
    public async Task GetReservationByIdAsync_WithNonExistingId_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetReservationByIdAsync(id)).ReturnsAsync((Entities.Reservation?)null);

        var result = await _service.GetReservationByIdAsync(id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task PayAsync_WithNonExistingReservation_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetReservationByIdAsync(id)).ReturnsAsync((Entities.Reservation?)null);

        var (success, error) = await _service.PayAsync(id);

        success.Should().BeFalse();
        error.Should().Be("Reservation not found");
    }

    [Fact]
    public async Task PayAsync_WithNonLockedReservation_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        var reservation = new Entities.Reservation { Id = id, Status = ReservationStatus.Pending };
        _repositoryMock.Setup(r => r.GetReservationByIdAsync(id)).ReturnsAsync(reservation);

        var (success, error) = await _service.PayAsync(id);

        success.Should().BeFalse();
        error.Should().Be("Only locked reservations can initiate payment");
    }

    [Fact]
    public async Task PayAsync_WithLockedReservation_MovesToPending()
    {
        var id = Guid.NewGuid();
        var reservation = new Entities.Reservation { Id = id, Status = ReservationStatus.Locked, ExpiresAt = DateTime.UtcNow.AddMinutes(10) };
        _repositoryMock.Setup(r => r.GetReservationByIdAsync(id)).ReturnsAsync(reservation);

        var (success, error) = await _service.PayAsync(id);

        success.Should().BeTrue();
        error.Should().BeNull();
        _repositoryMock.Verify(r => r.UpdateReservationStatusAsync(id, ReservationStatus.Pending), Times.Once);
    }

    [Fact]
    public async Task PayAsync_WithExpiredReservation_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        var reservation = new Entities.Reservation { Id = id, Status = ReservationStatus.Locked, ExpiresAt = DateTime.UtcNow.AddMinutes(-1) };
        _repositoryMock.Setup(r => r.GetReservationByIdAsync(id)).ReturnsAsync(reservation);

        var (success, error) = await _service.PayAsync(id);

        success.Should().BeFalse();
        error.Should().Be("Reservation has expired");
        _repositoryMock.Verify(r => r.UpdateReservationStatusAsync(It.IsAny<Guid>(), It.IsAny<ReservationStatus>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmReservationAsync_WithNonExistingReservation_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetReservationByIdAsync(id)).ReturnsAsync((Entities.Reservation?)null);

        var (success, error) = await _service.ConfirmReservationAsync(id, Guid.NewGuid());

        success.Should().BeFalse();
        error.Should().Be("Reservation not found");
    }

    [Fact]
    public async Task ConfirmReservationAsync_WithNonPendingReservation_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        var reservation = new Entities.Reservation { Id = id, Status = ReservationStatus.Locked };
        _repositoryMock.Setup(r => r.GetReservationByIdAsync(id)).ReturnsAsync(reservation);

        var (success, error) = await _service.ConfirmReservationAsync(id, Guid.NewGuid());

        success.Should().BeFalse();
        error.Should().Be("Only pending reservations can be confirmed");
    }

    [Fact]
    public async Task ConfirmReservationAsync_WithPendingReservation_MovesToConfirmed()
    {
        var id = Guid.NewGuid();
        var reservation = new Entities.Reservation { Id = id, Status = ReservationStatus.Pending, ExpiresAt = DateTime.UtcNow.AddMinutes(10) };
        _repositoryMock.Setup(r => r.GetReservationByIdAsync(id)).ReturnsAsync(reservation);

        var (success, error) = await _service.ConfirmReservationAsync(id, Guid.NewGuid());

        success.Should().BeTrue();
        error.Should().BeNull();
        _repositoryMock.Verify(r => r.UpdateReservationStatusAsync(id, ReservationStatus.Confirmed), Times.Once);
    }

    [Fact]
    public async Task ConfirmReservationAsync_WithExpiredReservation_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        var reservation = new Entities.Reservation { Id = id, Status = ReservationStatus.Pending, ExpiresAt = DateTime.UtcNow.AddMinutes(-1) };
        _repositoryMock.Setup(r => r.GetReservationByIdAsync(id)).ReturnsAsync(reservation);

        var (success, error) = await _service.ConfirmReservationAsync(id, Guid.NewGuid());

        success.Should().BeFalse();
        error.Should().Be("Reservation has expired");
        _repositoryMock.Verify(r => r.UpdateReservationStatusAsync(It.IsAny<Guid>(), It.IsAny<ReservationStatus>()), Times.Never);
    }

    [Fact]
    public async Task CancelReservationAsync_WithLockedReservation_UpdatesStatusAndDeletesLocks()
    {
        var id = Guid.NewGuid();
        var lockId = Guid.NewGuid();
        var reservation = new Entities.Reservation { Id = id, Status = ReservationStatus.Locked, SeatLocks = [new Entities.SeatLock { Id = lockId }] };
        _repositoryMock.Setup(r => r.GetReservationByIdAsync(id)).ReturnsAsync(reservation);
        _repositoryMock.Setup(r => r.BeginTransactionAsync()).ReturnsAsync(Mock.Of<IDbContextTransaction>());

        var (success, error) = await _service.CancelReservationAsync(id);

        success.Should().BeTrue();
        error.Should().BeNull();
        _repositoryMock.Verify(r => r.UpdateReservationStatusAsync(id, ReservationStatus.Cancelled), Times.Once);
        _repositoryMock.Verify(r => r.DeleteSeatLocksAsync(It.Is<IEnumerable<Guid>>(ids => ids.Contains(lockId))), Times.Once);
    }

    [Fact]
    public async Task CancelReservationAsync_WithPendingReservation_UpdatesStatusAndDeletesLocks()
    {
        var id = Guid.NewGuid();
        var lockId = Guid.NewGuid();
        var reservation = new Entities.Reservation { Id = id, Status = ReservationStatus.Pending, SeatLocks = [new Entities.SeatLock { Id = lockId }] };
        _repositoryMock.Setup(r => r.GetReservationByIdAsync(id)).ReturnsAsync(reservation);
        _repositoryMock.Setup(r => r.BeginTransactionAsync()).ReturnsAsync(Mock.Of<IDbContextTransaction>());

        var (success, error) = await _service.CancelReservationAsync(id);

        success.Should().BeTrue();
        error.Should().BeNull();
        _repositoryMock.Verify(r => r.UpdateReservationStatusAsync(id, ReservationStatus.Cancelled), Times.Once);
    }

    [Fact]
    public async Task CancelReservationAsync_WithConfirmedReservation_ReturnsFailureWithoutTouchingLocks()
    {
        var id = Guid.NewGuid();
        var lockId = Guid.NewGuid();
        var reservation = new Entities.Reservation { Id = id, Status = ReservationStatus.Confirmed, SeatLocks = [new Entities.SeatLock { Id = lockId }] };
        _repositoryMock.Setup(r => r.GetReservationByIdAsync(id)).ReturnsAsync(reservation);

        var (success, error) = await _service.CancelReservationAsync(id);

        success.Should().BeFalse();
        error.Should().Be("Only locked or pending reservations can be cancelled");
        _repositoryMock.Verify(r => r.BeginTransactionAsync(), Times.Never);
        _repositoryMock.Verify(r => r.UpdateReservationStatusAsync(It.IsAny<Guid>(), It.IsAny<ReservationStatus>()), Times.Never);
        _repositoryMock.Verify(r => r.DeleteSeatLocksAsync(It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }

    [Fact]
    public async Task CancelReservationAsync_WithNonExistingReservation_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetReservationByIdAsync(id)).ReturnsAsync((Entities.Reservation?)null);

        var (success, error) = await _service.CancelReservationAsync(id);

        success.Should().BeFalse();
        error.Should().Be("Reservation not found");
    }

    [Fact]
    public async Task ExpireReservationAsync_WithExistingReservation_UpdatesStatusAndDeletesLocks()
    {
        var id = Guid.NewGuid();
        var lockId = Guid.NewGuid();
        var reservation = new Entities.Reservation { Id = id, Status = ReservationStatus.Locked, SeatLocks = [new Entities.SeatLock { Id = lockId }] };
        _repositoryMock.Setup(r => r.GetReservationByIdAsync(id)).ReturnsAsync(reservation);
        _repositoryMock.Setup(r => r.BeginTransactionAsync()).ReturnsAsync(Mock.Of<IDbContextTransaction>());

        await _service.ExpireReservationAsync(id);

        _repositoryMock.Verify(r => r.UpdateReservationStatusAsync(id, ReservationStatus.Expired), Times.Once);
        _repositoryMock.Verify(r => r.DeleteSeatLocksAsync(It.Is<IEnumerable<Guid>>(ids => ids.Contains(lockId))), Times.Once);
    }

    [Fact]
    public async Task ExpireReservationAsync_WithNonExistingReservation_DoesNothing()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetReservationByIdAsync(id)).ReturnsAsync((Entities.Reservation?)null);

        await _service.ExpireReservationAsync(id);

        _repositoryMock.Verify(r => r.UpdateReservationStatusAsync(It.IsAny<Guid>(), It.IsAny<ReservationStatus>()), Times.Never);
    }
}
