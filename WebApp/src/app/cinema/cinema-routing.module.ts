import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { CinemaListComponent } from './components/cinema-list/cinema-list.component';
import { CinemaFormComponent } from './components/cinema-form/cinema-form.component';
import { CinemaDetailComponent } from './components/cinema-detail/cinema-detail.component';
import { HallSeatsComponent } from './components/hall-seats/hall-seats.component';


const routes: Routes = [
  { path: '', component: CinemaListComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class CinemaRoutingModule {}
