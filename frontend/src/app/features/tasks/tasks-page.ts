import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { finalize } from 'rxjs';
import { Task, TaskFilters, TaskStatus, User } from '../../core/models';
import { TaskService } from '../../core/task.service';
import { canChangeStatus, PRIORITIES, PRIORITY_LABELS, STATUSES, STATUS_LABELS } from '../../core/task-status';
import { UserService } from '../../core/user.service';
import { TaskForm } from './task-form';

@Component({
  selector: 'app-tasks-page',
  imports: [DatePipe, MatFormFieldModule, MatSelectModule, MatTableModule, MatProgressBarModule, TaskForm],
  templateUrl: './tasks-page.html',
  styleUrl: './tasks-page.css',
})
export class TasksPage implements OnInit {
  private taskService = inject(TaskService);
  private userService = inject(UserService);
  private snackBar = inject(MatSnackBar);

  readonly statuses = STATUSES;
  readonly priorities = PRIORITIES;
  // las filas de mat-table llegan sin tipo (let task), por eso los mapas se indexan con string
  readonly statusLabels: Record<string, string> = STATUS_LABELS;
  readonly priorityLabels: Record<string, string> = PRIORITY_LABELS;
  readonly canChange = canChangeStatus;
  readonly columns = ['title', 'user', 'priority', 'dueDate', 'createdAt', 'status', 'actions'];

  tasks = signal<Task[]>([]);
  users = signal<User[]>([]);
  loading = signal(false);
  filters = signal<TaskFilters>({});

  ngOnInit() {
    this.userService.getAll().subscribe(users => this.users.set(users));
    this.loadTasks();
  }

  setFilter(change: TaskFilters) {
    this.filters.update(f => ({ ...f, ...change }));
    this.loadTasks();
  }

  loadTasks() {
    this.loading.set(true);
    this.taskService
      .getAll(this.filters())
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({ next: tasks => this.tasks.set(tasks), error: () => {} });
  }

  changeStatus(task: Task, status: TaskStatus) {
    this.taskService.changeStatus(task.id, status).subscribe({
      next: () => {
        this.snackBar.open(`"${task.title}" pasó a ${this.statusLabels[status]}`, 'OK', { duration: 3000 });
        this.loadTasks();
      },
      // si falla se recarga para que la fila vuelva a mostrar el estado real
      error: () => this.loadTasks(),
    });
  }
}
