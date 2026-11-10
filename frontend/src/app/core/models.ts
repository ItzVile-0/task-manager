export type TaskStatus = 'Pending' | 'InProgress' | 'Done';
export type Priority = 'Low' | 'Medium' | 'High';

export interface User {
  id: number;
  name: string;
  email: string;
  createdAt: string;
}

export interface CreateUser {
  name: string;
  email: string;
}

export interface TaskInfo {
  priority?: Priority | null;
  estimatedEndDate?: string | null;
  tags?: string[] | null;
}

export interface Task {
  id: number;
  title: string;
  description: string | null;
  status: TaskStatus;
  userId: number;
  userName: string;
  additionalInfo: TaskInfo | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateTask {
  title: string;
  description: string | null;
  userId: number;
  additionalInfo: TaskInfo | null;
}

export interface TaskFilters {
  userId?: number | null;
  status?: TaskStatus | null;
  priority?: Priority | null;
}
