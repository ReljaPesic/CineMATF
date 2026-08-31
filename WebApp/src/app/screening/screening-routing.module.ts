import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ScreeningListComponent } from './components/screening-list/screening-list.component';
import { ScreeningDetailComponent } from './components/screening-detail/screening-detail.component';

const routes: Routes = [
  { path: '', component: ScreeningListComponent },
  { path: ':id', component: ScreeningDetailComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ScreeningRoutingModule {}
