import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { DefaultLayoutComponent } from './core';
import { PagesModule } from './pages/pages.module';

// Import Containers


export const routes: Routes = [
    
    { path: "", redirectTo: "login", pathMatch: "full" },
    {
        path: "", 
        component: DefaultLayoutComponent,
        children: [
            { path: "", loadChildren: "src/app/management/management.module#ManagementModule" },
        ]
    },
    { path: "**", redirectTo: "404" }
];

@NgModule({
    imports: [
        PagesModule,
        RouterModule.forRoot(routes),

    ],
    exports: [RouterModule]
})
export class AppRoutingModule { }
