import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { environment } from '../../environments/environment.development';

interface LoginResponse {
    accessToken: string;
    refreshToken: string;
    fullName: string;
    roles: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
    private http = inject(HttpClient);
    private api = environment.apiUrl;

    login(username: string, password: string) {
        return this.http
            .post<LoginResponse>(`${this.api}/auth/login`, { username, password })
            .pipe(tap(r => {
                localStorage.setItem('access_token', r.accessToken);
                localStorage.setItem('refresh_token', r.refreshToken);
                localStorage.setItem('full_name', r.fullName);
            }));
    }

    logout() {
        localStorage.clear();
    }

    get token(): string | null {
        return localStorage.getItem('access_token');
    }

    private get payload(): any | null {
        const t = this.token;
        if (!t) return null;
        try {
            return JSON.parse(atob(t.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
        } catch { return null; }
    }

    get roles(): string[] {
        const r = this.payload?.['role']
            ?? this.payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
        return r ? (Array.isArray(r) ? r : [r]) : [];
    }

    isLoggedIn(): boolean {
        const p = this.payload;
        return !!p && p.exp * 1000 > Date.now();
    }

    hasAnyRole(roles: string[]): boolean {
        return this.roles.some(r => roles.includes(r));
    }
}
console.log('API URL =', environment.apiUrl);