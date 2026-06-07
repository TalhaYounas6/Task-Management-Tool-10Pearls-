import { api } from './axiosInstance';

// check on later
// export type TaskCategory = 'Development' | 'Design' | 'Testing' | 'Marketing' | string;


export interface TaskItem {
  id: number;
  title: string;
  description?: string;
  dueDate?: string; 
  status: string; 
  priority: string; 
  category?: string;
  assignedUserId?: string;
  assignedUserName?: string;
  creatorUserId?: string;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  dueDate?: string;
  priority?: string; // Default is "Medium" on backend
  category?: string;
  assignedUserId?: string;
}


export interface UpdateTaskRequest {
  title?: string;
  description?: string;
  dueDate?: string;
  status?: string;
  priority?: string;
  category?: string;
  assignedUserId?: string;
}


export const tasksService = {
  // GET: /api/tasks
  getAllTasks: async () => {
    const response = await api.get<TaskItem[]>('/tasks');
    return response.data;
  },

  // GET: /api/tasks/{id}
  getTaskById: async (id: number) => {
    const response = await api.get<TaskItem>(`/tasks/${id}`);
    return response.data;
  },

  // POST: /api/tasks
  createTask: async (taskData: CreateTaskRequest) => {
    const response = await api.post<TaskItem>('/tasks', taskData);
    return response.data;
  },

  // PUT: /api/tasks/{id}
  updateTask: async (id: number, taskData: UpdateTaskRequest) => {
    const response = await api.put(`/tasks/${id}`, taskData);
    return response.data;
  },

  // DELETE: /api/tasks/{id}
  deleteTask: async (id: number) => {
    const response = await api.delete(`/tasks/${id}`);
    return response.data;
  }
};