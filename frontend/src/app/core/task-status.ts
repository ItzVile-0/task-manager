import { Priority, TaskStatus } from './models';

export const STATUSES: TaskStatus[] = ['Pending', 'InProgress', 'Done'];
export const PRIORITIES: Priority[] = ['High', 'Medium', 'Low'];

export const STATUS_LABELS: Record<TaskStatus, string> = {
  Pending: 'Pendiente',
  InProgress: 'En progreso',
  Done: 'Hecha',
};

export const PRIORITY_LABELS: Record<Priority, string> = {
  High: 'Alta',
  Medium: 'Media',
  Low: 'Baja',
};

// misma regla que valida el backend, aqui solo sirve para no ofrecer la opcion
export function canChangeStatus(from: TaskStatus, to: TaskStatus): boolean {
  return !(from === 'Pending' && to === 'Done');
}
