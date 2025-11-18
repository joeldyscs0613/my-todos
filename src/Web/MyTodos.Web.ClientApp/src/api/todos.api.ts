import apiClient from './client';
import type { Todo, CreateTodoRequest, UpdateTodoRequest } from '../types/todo.types';

export const todosApi = {
  getAll: async (): Promise<Todo[]> => {
    const response = await apiClient.get<Todo[]>('/api/todos/items');
    return response.data;
  },

  getById: async (id: string): Promise<Todo> => {
    const response = await apiClient.get<Todo>(`/api/todos/items/${id}`);
    return response.data;
  },

  create: async (data: CreateTodoRequest): Promise<Todo> => {
    const response = await apiClient.post<Todo>('/api/todos/items', data);
    return response.data;
  },

  update: async (id: string, data: UpdateTodoRequest): Promise<Todo> => {
    const response = await apiClient.put<Todo>(`/api/todos/items/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/todos/items/${id}`);
  },

  toggle: async (id: string): Promise<Todo> => {
    const response = await apiClient.patch<Todo>(`/api/todos/items/${id}/toggle`);
    return response.data;
  },
};
