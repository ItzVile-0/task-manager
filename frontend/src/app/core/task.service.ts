import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_URL } from './api';
import { CreateTask, Task, TaskFilters, TaskStatus } from './models';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private http = inject(HttpClient);
  private url = `${API_URL}/tasks`;

  getAll(filters: TaskFilters) {
    let params = new HttpParams();
    if (filters.userId) params = params.set('userId', filters.userId);
    if (filters.status) params = params.set('status', filters.status);
    if (filters.priority) params = params.set('priority', filters.priority);

    return this.http.get<Task[]>(this.url, { params });
  }

  create(task: CreateTask) {
    return this.http.post<Task>(this.url, task);
  }

  changeStatus(id: number, status: TaskStatus) {
    return this.http.put<Task>(`${this.url}/${id}/status`, { status });
  }
}
