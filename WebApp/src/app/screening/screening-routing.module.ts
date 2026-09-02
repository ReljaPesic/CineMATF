import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ScreeningListComponent } from './components/screening-list/screening-list.component';
import { ScreeningDetailComponent } from './components/screening-detail/screening-detail.component';
import { ScreeningFormComponent } from './components/screening-form/screening-form.component';
import { adminGuard } from '../shared/guards/admin.guard';

const routes: Routes = [
  { path: '', component: ScreeningListComponent },
  { path: 'new', component: ScreeningFormComponent, canActivate: [adminGuard] },
  { path: ':id/edit', component: ScreeningFormComponent, canActivate: [adminGuard] },
  { path: ':id', component: ScreeningDetailComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ScreeningRoutingModule {}
