import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

interface LoginRequest {
  email: string;
  senha: string;
}

interface ApiResponse<T> {
  success: boolean;
  message: string | null;
  data: T | null;
  errors: string[] | null;
}

interface TokenPayload {
  sub: string;
  email: string;
  role: string;
  exp: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'auth_token';

  constructor(private http: HttpClient) {}

  login(email: string, senha: string): Observable<ApiResponse<string>> {
    const body: LoginRequest = { email, senha };

    return this.http.post<ApiResponse<string>>(`${environment.apiUrl}/Auth/login`, body).pipe(
      tap(response => {
        if (response.success && response.data) {
          localStorage.setItem(this.tokenKey, response.data);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  isAuthenticated(): boolean {
    const payload = this.getPayload();
    return payload !== null && payload.exp * 1000 > Date.now();
  }

  getRole(): string | null {
    return this.getPayload()?.role ?? null;
  }

  private getPayload(): TokenPayload | null {
    const token = this.getToken();
    if (!token) {
      return null;
    }

    try {
      const base64Payload = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
      return JSON.parse(atob(base64Payload)) as TokenPayload;
    } catch {
      return null;
    }
  }
}
