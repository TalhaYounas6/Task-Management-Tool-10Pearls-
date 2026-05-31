import { Routes, Route } from 'react-router-dom';
import Login from './pages/Login';
import ProtectedRoute from './components/ProtectedRoute';
import Register from './pages/Register';
import Navbar from './components/NavBar';
import Dashboard from './pages/Dashboard';
import TaskList from './pages/TaskList';
import TaskDetail from './pages/TaskDetail';
import TaskForm from './pages/TaskForm';
import Profile from './pages/Profile';

function App() {
  return (
    <div className="min-h-screen bg-gray-50 text-gray-900">
      <Navbar/>
      <main className="container mx-auto px-4 py-8">
        <Routes>
          
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register/>} />
          
          
          <Route element={<ProtectedRoute />}>
            <Route path="/" element={<Dashboard/>} />
            <Route path="/tasks" element={<TaskList/>} />
            <Route path="/tasks/:id" element={<TaskDetail/>}/>
            <Route path="/tasks/new" element={<TaskForm/>}/>
            <Route path= "/tasks/:id/edit" element={<TaskForm/>}/>
            <Route path="/profile" element={<Profile/>} />
          </Route>
        </Routes>
      </main>
    </div>
  );
}

export default App;