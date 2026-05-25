import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface AppUser {
  id: number;
  userName: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly baseUrl = 'https://localhost:7149/api';
  private readonly userStorageKey = 'query_optimizer_user';

  constructor(private readonly http: HttpClient){}

  register(userName: string): Observable<AppUser> {
    const user: AppUser = {
      id: 0,
      userName: userName
    };
    return this.http.post<AppUser>(`${this.baseUrl}/User/Register`, user);
  }

  login(userName: string): Observable<AppUser> {
    const user: AppUser = {
      id: 0,
      userName: userName
    };
    return this.http.post<AppUser>(`${this.baseUrl}/User/Login`, user);
  }

  setUser(user: AppUser): void {
    localStorage.setItem(this.userStorageKey, JSON.stringify(user));
  }

  getUser(): AppUser | null {
    const raw = localStorage.getItem(this.userStorageKey);

    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as AppUser;
    } catch {
      return null;
    }
  }

  isLoggedIn(): boolean {
    return this.getUser() !== null;
  }

  logout(): void {
    localStorage.removeItem(this.userStorageKey);
  }
}