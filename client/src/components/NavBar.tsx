import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import { type RootState } from '../store/store';
import { logout } from '../store/slices/authSlice';

export default function Navbar() {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const location = useLocation();
  
  const { isAuthenticated, role } = useSelector((state: RootState) => state.auth);

  // If not logged in don't show the navbar
  if (!isAuthenticated) return null;

  const handleLogout = () => {
    dispatch(logout());
    navigate('/login');
  };

  
  const isActive = (path: string) => {
    return location.pathname === path 
      ? "text-blue-600 font-semibold border-b-2 border-blue-600" 
      : "text-gray-500 hover:text-gray-900";
  };

  return (
    <nav className="bg-white border-b border-gray-200 sticky top-0 z-10">
      <div className="container mx-auto px-4">
        <div className="flex justify-between items-center h-16">
          
          {/* Left Side: Brand and Links */}
          <div className="flex items-center space-x-8">
            <Link to="/" className="text-xl font-bold text-gray-900 tracking-tight">
              TaskMaster
            </Link>
            
            <div className="hidden md:flex space-x-6">
              <Link to="/" className={`py-5 transition-colors ${isActive('/')}`}>
                Dashboard
              </Link>
              <Link to="/tasks" className={`py-5 transition-colors ${isActive('/tasks')}`}>
                Tasks
              </Link>
              
              {role === 'Admin' && (
                <span className="py-5 text-gray-400 cursor-not-allowed text-sm flex items-center">
                  (Admin Mode)
                </span>
              )}
            </div>
          </div>

          {/* Right Side: Actions and Profile */}
          {/* Right Side: Actions and Profile */}
       <div className="flex items-center space-x-4">
         {/* <Link 
           to="/tasks/new" 
           className="bg-blue-600 text-white px-4 py-2 text-sm font-medium hover:bg-blue-700 transition-colors shadow-sm"
         >
           + New Task
         </Link> */}

         <div className="h-6 w-px bg-gray-300 mx-2"></div> {/* Vertical Divider */}

         <Link 
           to="/profile"
           className="text-sm font-medium text-gray-600 hover:text-blue-600 transition-colors cursor-pointer"
         >
           My Profile
         </Link>
       </div>

        </div>
      </div>
    </nav>
  );
}