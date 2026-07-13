import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedResult } from '../../shared/models/api-response.model';
import { CreateUserRequest, UpdateUserRequest, UserViewModel } from './user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly baseUrl = `${environment.apiUrl}/User`;

  constructor(private http: HttpClient) {}

  getPaged(page: number, pageSize: number): Observable<PagedResult<UserViewModel>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http
      .get<ApiResponse<PagedResult<UserViewModel>>>(this.baseUrl, { params })
      .pipe(map(response => response.data!));
  }

  getById(id: number): Observable<UserViewModel> {
    return this.http
      .get<ApiResponse<UserViewModel>>(`${this.baseUrl}/${id}`)
      .pipe(map(response => response.data!));
  }

  create(request: CreateUserRequest): Observable<number> {
    return this.http
      .post<ApiResponse<number>>(this.baseUrl, request)
      .pipe(map(response => response.data!));
  }

  update(id: number, request: UpdateUserRequest): Observable<void> {
    return this.http
      .put<ApiResponse<object>>(`${this.baseUrl}/${id}`, request)
      .pipe(map(() => undefined));
  }

  inactivate(id: number): Observable<void> {
    return this.http
      .delete<ApiResponse<object>>(`${this.baseUrl}/${id}`)
      .pipe(map(() => undefined));
  }
}
