import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { CinemaListComponent } from './components/cinema-list/cinema-list.component';
import { CinemaFormComponent } from './components/cinema-form/cinema-form.component';



const routes: Routes = [
  { path: '', component: CinemaListComponent },
  { path: 'new', component: CinemaFormComponent },
  { path: ':id/edit', component: CinemaFormComponent }
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class CinemaRoutingModule {}
