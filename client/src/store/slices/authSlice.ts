import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import { jwtDecode } from 'jwt-decode';


interface AuthState {
  token: string | null;
  isAuthenticated: boolean;
  role: string | null;
  userId: string | null;
}


const tokenFromStorage = localStorage.getItem('token');
let initialRole = null;
let initialUserId = null;

if (tokenFromStorage) {
  try {
    const decoded: any = jwtDecode(tokenFromStorage);
    // .NET Identity stores roles and IDs under these specific long URLs by default
    initialRole = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
    initialUserId = decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || null;
  } catch (error) {
    
    localStorage.removeItem('token');
  }
}


const initialState: AuthState = {
  token: tokenFromStorage,
  isAuthenticated: !!tokenFromStorage, 
  role: initialRole,
  userId: initialUserId,
};


const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    
    loginSuccess: (state, action: PayloadAction<string>) => {
      const token = action.payload;
      localStorage.setItem('token', token);
      
      const decoded: any = jwtDecode(token);
      
      state.token = token;
      state.isAuthenticated = true;
      state.role = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      state.userId = decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
    },
    
    logout: (state) => {
      localStorage.removeItem('token');
      state.token = null;
      state.isAuthenticated = false;
      state.role = null;
      state.userId = null;
    },
  },
});

export const { loginSuccess, logout } = authSlice.actions;

export default authSlice.reducer;