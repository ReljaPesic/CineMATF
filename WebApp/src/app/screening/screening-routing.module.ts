import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ScreeningListComponent } from './components/screening-list/screening-list.component';


const routes: Routes = [
  { path: '', component: ScreeningListComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ScreeningRoutingModule {}
