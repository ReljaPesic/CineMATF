import { Component, Input, OnChanges } from '@angular/core';

@Component({
  selector: 'app-movie-poster',
  standalone: true,
  templateUrl: './movie-poster.component.html',
  styleUrl: './movie-poster.component.css',
})
export class MoviePosterComponent implements OnChanges {

  @Input() url: string | null | undefined;
  @Input({ required: true }) title = '';


  failed = false;

  ngOnChanges(): void {
    this.failed = false;
  }

}
