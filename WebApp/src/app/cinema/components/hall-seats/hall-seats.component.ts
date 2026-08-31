import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { ALL_SEAT_TYPES, SeatResponse, SeatType } from '../../models/cinema.model';
import { CinemaService } from '../../services/cinema.service';

interface SeatRow {
  row: number;
  seats: SeatResponse[];
}

@Component({
  selector: 'app-hall-seats',
  standalone: false,
  templateUrl: './hall-seats.component.html',
  styleUrl: './hall-seats.component.css',
})
export class HallSeatsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly cinemaService = inject(CinemaService);

  cinemaId!: string;
  hallId!: string;

  rows: SeatRow[] = [];
  readonly seatTypes = ALL_SEAT_TYPES;

  loading = true;
  error: string | null = null;
  generating = false;
  updatingSeatId: string | null = null;

  selectedSeat: SeatResponse | null = null;

  ngOnInit(): void {
    this.cinemaId = this.route.snapshot.paramMap.get('id')!;
    this.hallId = this.route.snapshot.paramMap.get('hallId')!;
    this.loadSeats();
  }

  get totalSeats(): number {
    return this.rows.reduce((sum, r) => sum + r.seats.length, 0);
  }

  private loadSeats(): void {
    this.loading = true;
    this.error = null;
    this.selectedSeat = null;
    this.cinemaService.getSeatsByCinemaAndHallIds(this.cinemaId, this.hallId).subscribe({
      next: (seats) => {
        this.rows = this.groupByRow(seats);
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        console.error('GET seats failed', err);
        this.error = 'Could not load seats.';
      },
    });
  }

  private groupByRow(seats: SeatResponse[]): SeatRow[] {
    const byRow = new Map<number, SeatResponse[]>();
    for (const seat of seats) {
      const list = byRow.get(seat.row) ?? [];
      list.push(seat);
      byRow.set(seat.row, list);
    }
    // creates a new array containing key-value elements
    return [...byRow.entries()]
      .sort(([a], [b]) => a - b)
      .map(([row, list]) => ({ row, seats: list.sort((a, b) => a.number - b.number) }));
  }

  select(seat: SeatResponse): void {
    this.selectedSeat = this.selectedSeat?.id === seat.id ? null : seat;
  }

  setType(type: SeatType): void {
    const seat = this.selectedSeat;
    if (!seat || this.updatingSeatId || seat.seatType === type) return;

    this.updatingSeatId = seat.id;
    this.error = null;
    this.cinemaService
      .updateSeatType(this.cinemaId, this.hallId, seat.id, { seatType: type })
      .subscribe({
        next: (updated) => {
          this.updatingSeatId = null;
          seat.seatType = updated.seatType;
          this.selectedSeat = null;
        },
        error: (err) => {
          this.updatingSeatId = null;
          console.error('PATCH seat type failed', err);
          this.error = 'Could not change the seat type.';
        },
      });
  }

  generateSeats(): void {
    if (this.generating) return;
    this.generating = true;
    this.error = null;
    this.cinemaService.createSeats(this.cinemaId, this.hallId).subscribe({
      next: () => {
        this.generating = false;
        this.loadSeats();
      },
      error: (err) => {
        this.generating = false;
        console.error('POST seats failed', err);
        this.error = 'Could not generate seats.';
      },
    });
  }
}
