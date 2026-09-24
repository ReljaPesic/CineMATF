import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { CinemaRoutingModule } from './cinema-routing.module';
import { CinemaListComponent } from './components/cinema-list/cinema-list.component';
import { CinemaFormComponent } from './components/cinema-form/cinema-form.component';
import { CinemaDetailComponent } from './components/cinema-detail/cinema-detail.component';
import { HallSeatsComponent } from './components/hall-seats/hall-seats.component';
import { RegisterCinemaAdminComponent } from './components/register-cinema-admin/register-cinema-admin.component';
import { CinemaAdminListComponent } from './components/cinema-admin-list/cinema-admin-list.component';
import { CinemaAdminEditComponent } from './components/cinema-admin-edit/cinema-admin-edit.component';

@NgModule({
  declarations: [
    CinemaListComponent,
    CinemaFormComponent,
    CinemaDetailComponent,
    HallSeatsComponent,
    RegisterCinemaAdminComponent,
    CinemaAdminListComponent,
    CinemaAdminEditComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    CinemaRoutingModule,
  ],
})
export class CinemaModule {}
