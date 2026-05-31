import { configureStore } from '@reduxjs/toolkit';
import authReducer from './slices/authSlice';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    
  },
});


// tell  React components exactly what data exists in  store.
export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;