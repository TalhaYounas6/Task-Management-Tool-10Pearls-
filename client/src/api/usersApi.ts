import { api } from './axiosInstance';

export interface UserItem {
  id: string;
  fullName: string;
  email: string;
}

export const usersService = {
  // GET: /api/auth/users
  getAllUsers: async () => {
    const response = await api.get<UserItem[]>('/auth/users');
    return response.data;
  },

  // GET: /api/auth/me 
  getMyProfile: async () => {
  
    const response = await api.get<UserItem>('/auth/me'); 
    return response.data;
  }
};

