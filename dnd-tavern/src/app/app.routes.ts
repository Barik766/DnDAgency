// app.routes.ts
import { Routes } from '@angular/router';
import { adminGuard, gmGuard, guestGuard } from './features/auth/guards/access.guard';

export const routes: Routes = [
  // Public routes
  {
    path: '',
    loadComponent: () => import('./layout/main-layout/main-layout').then(m => m.MainLayout),
    children: [
      { path: '', redirectTo: '/home', pathMatch: 'full' },
      {
        path: 'home',
        loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent)
      },
      {
        path: 'campaigns',
        children: [
          {
            path: '',
            loadComponent: () =>
              import('./features/campaigns/campaigns.component/campaigns.component')
                .then(m => m.CampaignsComponent)
          },
          {
            path: 'create',
            loadComponent: () =>
              import('./features/campaigns/create.campaign.component/create.campaign.component')
                .then(m => m.CreateCampaignComponent),
            canActivate: [gmGuard, adminGuard]
          },
          {
            path: 'update/:id',  
            loadComponent: () =>
              import('./features/campaigns/update.campaign.component/update.campaign.component')
                .then(m => m.UpdateCampaignComponent),
            canActivate: [gmGuard, adminGuard]
          },
          {
            path: ':id',
            loadComponent: () =>
              import('./features/campaigns/campaigns.component/game-card-details/game-card-details')
                .then(m => m.GameCardDetails)
          }
        ]
      },
      {
        path: 'masters',
        children: [
          {
            path: '',
            loadComponent: () => import('./features/masters/masters.component/masters.component')
              .then(m => m.MastersComponent)
          },
          {
            path: 'me',
            loadComponent: () =>
              import('./features/masters/master.profile.component/master.profile.component')
                .then(m => m.MasterProfileComponent),
            canActivate: [gmGuard]
          },
          {
            path: ':id',
            loadComponent: () =>
              import('./features/masters/master.profile.component/master.profile.component')
                .then(m => m.MasterProfileComponent)
          }
        ]
      },
      {
        path: 'about',
        loadComponent: () => import('./features/about/about.component/about.component')
          .then(m => m.AboutComponent)
      },
      {
        path: 'my-bookings',
        loadComponent: () => import('./features/bookings/my-bookings.component/my-bookings.component')
          .then(m => m.MyBookingsComponent)
      }
    ]
  },
  // Authentication (routes for unauthenticated users only)
  {
    path: 'login',
    loadComponent: () => import('./features/auth/auth.component/auth.component').then(m => m.AuthComponent),
    canActivate: [guestGuard]
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/auth.component/auth.component').then(m => m.AuthComponent),
    canActivate: [guestGuard]
  },
];