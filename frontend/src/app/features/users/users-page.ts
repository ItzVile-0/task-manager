import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal, viewChild } from '@angular/core';
import { FormGroupDirective, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { finalize } from 'rxjs';
import { User } from '../../core/models';
import { UserService } from '../../core/user.service';

@Component({
  selector: 'app-users-page',
  imports: [DatePipe, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatTableModule],
  templateUrl: './users-page.html',
  styleUrl: './users-page.css',
})
export class UsersPage implements OnInit {
  private fb = inject(NonNullableFormBuilder);
  private userService = inject(UserService);
  private snackBar = inject(MatSnackBar);
  private formDir = viewChild.required(FormGroupDirective);

  readonly columns = ['name', 'email', 'createdAt'];
  users = signal<User[]>([]);
  saving = signal(false);

  form = this.fb.group({
    name: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(100)]],
    // pide un punto en el dominio, igual que el CHECK de la tabla Users
    email: ['', [Validators.required, Validators.pattern(/^[^@\s]+@[^@\s]+\.[^@\s]+$/), Validators.maxLength(150)]],
  });

  ngOnInit() {
    this.load();
  }

  load() {
    this.userService.getAll().subscribe({ next: users => this.users.set(users), error: () => {} });
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { name, email } = this.form.getRawValue();

    this.saving.set(true);
    this.userService
      .create({ name: name.trim(), email: email.trim() })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: user => {
          this.snackBar.open(`Usuario ${user.name} creado`, 'OK', { duration: 3000 });
          this.formDir().resetForm();
          this.load();
        },
        error: () => {},
      });
  }
}
