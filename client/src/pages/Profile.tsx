import { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { type RootState } from '../store/store';
import { logout } from '../store/slices/authSlice';
import { usersService, type UserItem } from '../api/usersApi';

export default function Profile() {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  
  // Pull the role from Redux
  const { role } = useSelector((state: RootState) => state.auth);
  
  const [profile, setProfile] = useState<UserItem | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const data = await usersService.getMyProfile();
        setProfile(data);
      } catch (err) {
        setError('Failed to load profile data.');
      } finally {
        setIsLoading(false);
      }
    };

    fetchProfile();
  }, []);

  const handleLogout = () => {
    
    dispatch(logout());
    
    navigate('/login');
  };

  if (isLoading) return <div className="p-8 text-center text-gray-500">Loading profile...</div>;
  if (error || !profile) return <div className="p-4 bg-red-50 text-red-700 border border-red-200">{error}</div>;

  return (
    <div className="max-w-md mx-auto mt-8">
      
      {/* Profile Card */}
      <div className="bg-white border border-gray-200 shadow-sm">
        
        
        <div className="bg-gray-50 px-6 py-6 border-b border-gray-200 flex flex-col items-center">
          
          <div className="w-20 h-20 bg-blue-100 text-blue-600 rounded-full flex items-center justify-center text-2xl font-bold mb-4">
            {profile.fullName.charAt(0).toUpperCase()}
          </div>
          <h2 className="text-xl font-semibold text-gray-900">{profile.fullName}</h2>
          <p className="text-sm text-gray-500 mt-1">{profile.email}</p>
        </div>

        {/* Card Body*/}
        <div className="px-6 py-6 space-y-4">
          <div>
            <p className="text-xs font-medium text-gray-500 uppercase tracking-wider mb-1">Account Role</p>
            <div className="flex items-center">
              <span className={`px-2.5 py-0.5 text-xs font-medium uppercase tracking-wide ${
                role === 'Admin' ? 'bg-purple-100 text-purple-800' : 'bg-green-100 text-green-800'
              }`}>
                {role === 'Admin' ? 'Administrator' : 'Regular User'}
              </span>
            </div>
          </div>
          
          <div>
            <p className="text-xs font-medium text-gray-500 uppercase tracking-wider mb-1">Account ID</p>
            <p className="text-sm text-gray-900 font-mono bg-gray-50 p-2 border border-gray-100">
              {profile.id}
            </p>
          </div>
        </div>

        {/* Card Footer */}
        <div className="px-6 py-4 bg-gray-50 border-t border-gray-200">
          <button 
            onClick={handleLogout}
            className="w-full bg-white cursor-pointer border border-red-300 text-black font-medium py-2 px-4 hover:bg-red-500 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 transition-colors"
          >
            Log Out Securely
          </button>
        </div>
      </div>
      
    </div>
  );
}