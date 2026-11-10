import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_URL } from './api';
import { CreateUser, User } from './models';

@Injectable({ providedIn: 'root' })
export class UserService {
  private http = inject(HttpClient);
  private url = `${API_URL}/users`;

  getAll() {
    return this.http.get<User[]>(this.url);
  }

  create(user: CreateUser) {
    return this.http.post<User>(this.url, user);
  }
}
