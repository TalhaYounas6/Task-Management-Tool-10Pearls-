import axios from 'axios';
import { store } from '../store/store';
import { logout } from '../store/slices/authSlice';

export const api = axios.create({
  baseURL: 'https://localhost:7169/api', 
  headers: {
    'Content-Type': 'application/json',
  },
});


api.interceptors.request.use(
  (config) => {
    
    const token = localStorage.getItem('token');
    
    
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

api.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    if (error.response && error.response.status === 401) {

      store.dispatch(logout());
      
      window.location.href = '/login';
    }
    
    return Promise.reject(error);
  }
);