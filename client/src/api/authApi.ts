import { api } from './axiosInstance';


export interface LoginRequest {
  email: string;
  password: string; 
}

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
}

export interface AuthResponse {
  token: string;
  message: string;
}

export interface RegisterResponse {
  message: string;
}



export const authService = {
  
  // POST: api/auth/login
  login: async (credentials: LoginRequest) => {
    const response = await api.post<AuthResponse>('/auth/login', credentials);
    return response.data;
  },

  // POST: api/auth/register
  register: async (userData: RegisterRequest) => {
    const response = await api.post<RegisterResponse>('/auth/register', userData);
    return response.data;
  }
  
};