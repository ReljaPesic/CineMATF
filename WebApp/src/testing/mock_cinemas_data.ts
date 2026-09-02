import {
  Cinema,
  CinemaRequest,
  City,
  CreateHallsRequest,
  HallResponse,
  SeatResponse,
  SeatType,
  UpdateSeatTypeRequest,
} from '../app/cinema/models/cinema.model';

export const MOCK_CINEMAS: Cinema[] = [
  { id: 'c1', name: 'CineMax', city: City.Beograd },
  { id: 'c2', name: 'Cineplexx', city: City.NoviSad },
  { id: 'c3', name: 'Arena', city: City.Nis },
];

export const MOCK_CINEMA: Cinema = MOCK_CINEMAS[0];


export const MOCK_CINEMAS_BY_CITY: Cinema[] = [
  { id: 'c1', name: 'CineMax', city: City.Beograd },
  { id: 'c4', name: 'Tuckwood', city: City.Beograd },
];

export const MOCK_CINEMA_REQUEST: CinemaRequest = {
  name: 'CineMax',
  city: City.Beograd,
};

export const MOCK_HALLS: HallResponse[] = [
  { id: 'c1-h1', name: 'Hall 1', totalRows: 5, seatsPerRow: 10, cinemaId: 'c1' },
  { id: 'c1-h2', name: 'Hall 2', totalRows: 8, seatsPerRow: 12, cinemaId: 'c1' },
  { id: 'c2-h1', name: 'Hall 1', totalRows: 6, seatsPerRow: 8, cinemaId: 'c2' },
  { id: 'c3-h1', name: 'Hall 1', totalRows: 5, seatsPerRow: 10, cinemaId: 'c3' },
];

export const MOCK_HALL: HallResponse = MOCK_HALLS[0];

export const MOCK_HALLS_REQUEST: CreateHallsRequest = {
  halls: [
    { name: 'Hall 3', totalRows: 7, seatsPerRow: 10 },
    { name: 'Hall 4', totalRows: 4, seatsPerRow: 8 },
  ],
};

function buildSeatResponses(hallId: string, totalRows: number, seatsPerRow: number): SeatResponse[] {
  const seats: SeatResponse[] = [];
  for (let row = 1; row <= totalRows; row++) {
    for (let number = 1; number <= seatsPerRow; number++) {
      seats.push({
        id: `${hallId}-r${row}-n${number}`,
        row,
        number,
        seatType: row === 1 ? SeatType.VIP : SeatType.Standard,
      });
    }
  }
  return seats;
}

export const MOCK_SEATS: SeatResponse[] = buildSeatResponses('c1-h1', 5, 10);

export const MOCK_SEAT: SeatResponse = MOCK_SEATS[0];

// Body of PATCH /cinema/{id}/halls/{hallId}/seats/{seatId}.
export const MOCK_UPDATE_SEAT_TYPE_REQUEST: UpdateSeatTypeRequest = {
  seatType: SeatType.VIP,
};

export { buildSeatResponses };
