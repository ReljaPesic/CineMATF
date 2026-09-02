// Rewritten entities from Cinema service into TS

export enum City{
    Beograd = 'Beograd',
    NoviSad = 'NoviSad',
    Nis = 'Nis',
    Kragujevac = 'Kragujevac'
}

export enum SeatType{
    Standard =  'Standard',
    VIP = 'VIP',
    Couple = 'Couple',
    Accessible = 'Accessible'
}

/** Every city value, for filter dropdowns and the cinema form. */
export const ALL_CITIES: City[] = Object.values(City);

/** Every seat type, for the seat-grid type picker and legend. */
export const ALL_SEAT_TYPES: SeatType[] = Object.values(SeatType);

export interface Cinema {
    id: string,
    name: string,
    city: City,
    halls?: Hall[] 
}


export interface Seat{
    id: string,
    row: number,
    number: number,
    seatType: SeatType,
    hallId: string,
    hall?: Hall
}

export interface Hall{
    id: string,
    name: string,
    totalRows: number,
    seatsPerRow: number, 
    cinemaId: string,
    movieTheatre?: Cinema,
    seats: Seat[]
}

export interface CinemaRequest{
    name: string,
    city: City
}

export interface CinemaResponse{
    id: string,
    name: string,
    city: City
}

export interface HallRequest{
    name: string,
    totalRows: number,
    seatsPerRow: number
}

export interface HallResponse{
    id: string,
    name: string ,
    totalRows: number,
    seatsPerRow: number,
    cinemaId: string
}

export interface SeatResponse{
    id: string,
    row: number,
    number: number,
    seatType: SeatType
}

export interface CreateHallsRequest{
    halls: HallRequest[]
}
export interface FailedHall{
        name: string,
        error: string
}

export interface CreateHallsResponse{
    created: number,
    failed: FailedHall[]
}

export interface UpdateSeatTypeRequest {
  seatType: SeatType;
}