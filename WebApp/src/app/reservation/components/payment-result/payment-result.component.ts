import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { Reservation, ReservationStatus } from '../../models/reservation.model';
import { ReservationService } from '../../services/reservation.service';

type ResultState = 'cancelled' | 'polling' | 'confirmed' | 'timed-out' | 'lapsed' | 'error';

const POLL_INTERVAL_MS = 2000;
const MAX_POLL_ATTEMPTS = 30;

// Landing page for Stripe Checkout's success_url/cancel_url. Actual payment confirmation
// happens server-side via the Stripe webhook, not this redirect, so on the success path we
// have to poll the reservation until the webhook has caught up (or bail out after a while).
@Component({
  selector: 'app-payment-result',
  standalone: false,
  templateUrl: './payment-result.component.html',
  styleUrl: './payment-result.component.css',
})
export class PaymentResultComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly reservationService = inject(ReservationService);

  reservationId: string | null = null;
  state: ResultState = 'polling';
  error: string | null = null;

  private pollTimer: ReturnType<typeof setTimeout> | null = null;
  private attempts = 0;

  ngOnInit(): void {
    const params = this.route.snapshot.queryParamMap;
    this.reservationId = params.get('reservationId');

    if (!this.reservationId) {
      this.state = 'error';
      this.error = 'Missing reservation reference.';
      return;
    }

    if (params.get('status') === 'cancelled') {
      this.state = 'cancelled';
      return;
    }

    this.poll();
  }

  ngOnDestroy(): void {
    if (this.pollTimer) clearTimeout(this.pollTimer);
  }

  private poll(): void {
    this.attempts++;
    this.reservationService.getReservation(this.reservationId!).subscribe({
      next: (reservation) => this.handleReservation(reservation),
      error: (err: HttpErrorResponse) => {
        this.state = 'error';
        this.error = err.error?.message ?? 'Could not check payment status.';
      },
    });
  }

  private handleReservation(reservation: Reservation): void {
    if (reservation.status === ReservationStatus.Confirmed) {
      // Idempotent server-side - safe to call even if tickets already exist.
      this.reservationService.generateTickets(reservation.id).subscribe({
        next: () => (this.state = 'confirmed'),
        error: () => (this.state = 'confirmed'),
      });
      return;
    }

    if (reservation.status === ReservationStatus.Cancelled || reservation.status === ReservationStatus.Expired) {
      this.state = 'lapsed';
      return;
    }

    if (this.attempts >= MAX_POLL_ATTEMPTS) {
      this.state = 'timed-out';
      return;
    }

    this.pollTimer = setTimeout(() => this.poll(), POLL_INTERVAL_MS);
  }

  retryPoll(): void {
    this.state = 'polling';
    this.attempts = 0;
    this.poll();
  }

  goToReservation(): void {
    if (this.reservationId) this.router.navigate(['/reservations', this.reservationId]);
  }

  bookAgain(): void {
    this.router.navigate(['/screenings']);
  }
}
