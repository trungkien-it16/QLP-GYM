import { Routes } from '@angular/router';
import { authGuard } from './core/guards';
import { Login } from './pages/login/login';
import { Dashboard } from './pages/dashboard/dashboard';
import { Forbidden } from './pages/forbidden/forbidden';

export const routes: Routes = [
    { path: 'login', component: Login },
    { path: 'dashboard', component: Dashboard, canActivate: [authGuard] },
    { path: 'forbidden', component: Forbidden },
    { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    { path: '**', redirectTo: 'dashboard' }
];