import { Component, inject, input, output, signal, viewChild } from '@angular/core';
import { FormGroupDirective, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { finalize } from 'rxjs';
import { Priority, Task, TaskInfo, User } from '../../core/models';
import { TaskService } from '../../core/task.service';
import { PRIORITIES, PRIORITY_LABELS } from '../../core/task-status';

@Component({
  selector: 'app-task-form',
  imports: [ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule],
  templateUrl: './task-form.html',
  styleUrl: './task-form.css',
})
export class TaskForm {
  private fb = inject(NonNullableFormBuilder);
  private taskService = inject(TaskService);
  private snackBar = inject(MatSnackBar);
  private formDir = viewChild.required(FormGroupDirective);

  users = input.required<User[]>();
  created = output<Task>();

  readonly priorities = PRIORITIES;
  readonly priorityLabels = PRIORITY_LABELS;
  saving = signal(false);

  form = this.fb.group({
    // el pattern evita titulos que son solo espacios
    title: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(200)]],
    description: ['', Validators.maxLength(1000)],
    userId: this.fb.control<number | null>(null, Validators.required),
    priority: this.fb.control<Priority | ''>(''),
    estimatedEndDate: [''],
    tags: [''],
  });

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const tags = value.tags.split(',').map(t => t.trim()).filter(Boolean);

    const info: TaskInfo = {};
    if (value.priority) info.priority = value.priority;
    if (value.estimatedEndDate) info.estimatedEndDate = value.estimatedEndDate;
    if (tags.length) info.tags = tags;

    this.saving.set(true);
    this.taskService
      .create({
        title: value.title.trim(),
        description: value.description.trim() || null,
        userId: value.userId!,
        additionalInfo: Object.keys(info).length ? info : null,
      })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: task => {
          this.snackBar.open('Tarea creada', 'OK', { duration: 3000 });
          this.formDir().resetForm();
          this.created.emit(task);
        },
        error: () => {}, // el interceptor ya mostro el mensaje
      });
  }
}
