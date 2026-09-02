// Rewritten entities from Reservation service into TS

export enum ReservationStatus {
  Confirmed = 'Confirmed',
  Cancelled = 'Cancelled',
  Expired = 'Expired',
  Locked = 'Locked',
}

export interface Ticket {
  id: string;
  seatId: string;
  seatRow: number;
  seatNumber: number;
  price: number;
  qrCode: string;
}

export interface Reservation {
  id: string;
  userId: string;
  screeningId: string;
  status: ReservationStatus;
  totalPrice: number;
  createdAt: string;
  expiresAt: string;
  tickets: Ticket[];
}

export interface SeatLock {
  seatId: string;
  lockedAt: string;
  expiresAt: string;
}

export interface AvailableSeats {
  screeningId: string;
  availableSeats: string[];
  lockedSeats: SeatLock[];
}

export interface CreateReservationRequest {
  screeningId: string;
  seatIds: string[];
  userId: string;
}

export const SEAT_PRICE: Record<string, number> = {
  Standard: 10,
  VIP: 15,
  Couple: 18,
  Accessible: 10,
};
