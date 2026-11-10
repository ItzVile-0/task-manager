import { Routes } from '@angular/router';
import { TasksPage } from './features/tasks/tasks-page';
import { UsersPage } from './features/users/users-page';

export const routes: Routes = [
  { path: '', redirectTo: 'tasks', pathMatch: 'full' },
  { path: 'tasks', component: TasksPage, title: 'Tareas' },
  { path: 'users', component: UsersPage, title: 'Usuarios' },
  { path: '**', redirectTo: 'tasks' },
];
