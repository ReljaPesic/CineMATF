import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { ScreeningRoutingModule } from './screening-routing.module';
import { ScreeningListComponent } from './components/screening-list/screening-list.component';

@NgModule({
  declarations: [
    ScreeningListComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    ScreeningRoutingModule,
  ],
})
export class ScreeningModule {}
