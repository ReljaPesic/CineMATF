import {
  AvailableSeats,
  CreateReservationRequest,
  Reservation,
  ReservationStatus,
  SeatLock,
  Ticket,
} from '../app/reservation/models/reservation.model';

function offsetIso(minutes: number): string {
  return new Date(Date.now() + minutes * 60_000).toISOString();
}

export const MOCK_TICKETS: Ticket[] = [
  { id: 't1', seatId: 'c1-h1-r1-n1', seatRow: 1, seatNumber: 1, price: 15, qrCode: 'CINEMATF|r1|t1' },
  { id: 't2', seatId: 'c1-h1-r1-n2', seatRow: 1, seatNumber: 2, price: 15, qrCode: 'CINEMATF|r1|t2' },
];

export const MOCK_RESERVATIONS: Reservation[] = [
  {
    // Confirmed booking, tickets already generated (Alice).
    id: 'r1',
    userId: 'u1',
    screeningId: 's1',
    status: ReservationStatus.Confirmed,
    totalPrice: 30,
    createdAt: offsetIso(-2 * 24 * 60),
    expiresAt: offsetIso(-2 * 24 * 60 + 15),
    tickets: MOCK_TICKETS,
  },
  {
    // Seats held but not paid yet - no tickets (Bob).
    id: 'r2',
    userId: 'u2',
    screeningId: 's2',
    status: ReservationStatus.Locked,
    totalPrice: 10,
    createdAt: offsetIso(-5),
    expiresAt: offsetIso(10),
    tickets: [],
  },
  {
    // Lock that ran out of time (Alice).
    id: 'r3',
    userId: 'u1',
    screeningId: 's3',
    status: ReservationStatus.Expired,
    totalPrice: 20,
    createdAt: offsetIso(-60),
    expiresAt: offsetIso(-45),
    tickets: [],
  },
  {
    // Cancelled after confirmation (Bob).
    id: 'r4',
    userId: 'u2',
    screeningId: 's4',
    status: ReservationStatus.Cancelled,
    totalPrice: 33,
    createdAt: offsetIso(-3 * 24 * 60),
    expiresAt: offsetIso(-3 * 24 * 60 + 15),
    tickets: [],
  },
];

export const MOCK_RESERVATION: Reservation = MOCK_RESERVATIONS[0];

export const MOCK_SEAT_LOCKS: SeatLock[] = [
  { seatId: 'c1-h2-r1-n1', lockedAt: offsetIso(-5), expiresAt: offsetIso(10) },
];


export const MOCK_AVAILABLE_SEATS: AvailableSeats = {
  screeningId: 's1',
  availableSeats: [
    'c1-h1-r1-n3', 'c1-h1-r1-n4', 'c1-h1-r1-n5',
    'c1-h1-r2-n1', 'c1-h1-r2-n2', 'c1-h1-r2-n3', 'c1-h1-r2-n4', 'c1-h1-r2-n5',
    'c1-h1-r3-n1', 'c1-h1-r3-n2', 'c1-h1-r3-n3',
  ],
  lockedSeats: MOCK_SEAT_LOCKS,
};

export const MOCK_CREATE_RESERVATION_REQUEST: CreateReservationRequest = {
  screeningId: 's1',
  seatIds: ['c1-h1-r2-n5', 'c1-h1-r2-n6'],
  userId: 'u1',
};
